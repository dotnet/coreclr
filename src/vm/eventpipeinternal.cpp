// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// #include <algorithm>
// #include <string>
#include <type_traits>
#include "common.h"
#include "diagnosticsipc.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "eventpipeeventinstance.h"
#include "eventpipeinternal.h"
#include "eventpipeprovider.h"
#include "eventpipesession.h"
#include "processdescriptor.h"
#include "sampleprofiler.h"

#ifdef FEATURE_PAL
#include "pal.h"
#endif // FEATURE_PAL

#ifdef FEATURE_PERFTRACING

//! x is clamped to the range [Minimum , Maximum]
//! Returns Minimum if x is less than Minimum.
//! Returns Maximum if x is greater than Maximum.
//! Returns x otherwise.
template <typename T>
constexpr const typename std::enable_if<std::is_integral<T>::value, T>::type Clamp(const T x, const T Minimum, const T Maximum)
{
    _ASSERTE(Minimum < Maximum);
    return (x < Minimum) ? Minimum : (Maximum < x) ? Maximum : x;
}

template <class InputIt, class T>
constexpr InputIt &Find(InputIt &first, InputIt &last, const T &value)
{
    for (; first != last; ++first)
        if (*first == value)
            return first;
    return last;
}

typedef SList<SListElem<EventPipeProviderConfiguration *>> Providers;

void ProviderParser(const char *first, const char *last, Providers &providers)
{
    // Provider:        "(GUID|KnownProviderName)[:Flags[:Level][:KeyValueArgs]]"
    // KeyValueArgs:    "[key1=value1][;key2=value2]"

    // TODO: Should these be read from a config file?
    const uint64_t DefaultKeywords = UINT64_MAX;
    const uint32_t DefaultLevel = static_cast<uint32_t>(EventPipeEventLevel::Verbose);

    SString providerName;
    uint64_t keywords = DefaultKeywords;
    uint32_t loggingLevel = DefaultLevel;
    SString filterData;

    for (; first != last; ++first)
    {
        StackScratchBuffer scratchBuffer;

        // Split on : <- (Provider:Flags:Level - or - Provider:Flags:Level:KeyValueArgs)
        const char *begin = first;
        const char *end = Find(first, last, ':'); // Provider name.

        if (begin == end)
            break; // Ignore undefined provider.

        COUNT_T count = static_cast<COUNT_T>(end - begin);
        if (count > 0)
        {
            SString tmpProviderName;
            tmpProviderName.SetANSI(begin, count);
            tmpProviderName.ConvertToUnicode(providerName);
        }

        if (first == last)
            break; // There is nothing after the separator ':'
        ++first;   // Move on to the next string.

        begin = first;
        end = Find(first, last, ':'); // Keyword.

        if (begin != end)
        {
            // There is something to parse.
            count = static_cast<COUNT_T>(end - begin);
            if (count > 0)
            {
                SString value;
                value.SetANSI(begin, count);
                keywords = static_cast<uint64_t>(strtoull(value.GetANSI(scratchBuffer), nullptr, 0));
            }
        }

        if (first == last)
            break; // There is nothing after the separator ':'
        ++first;   // Move on to the next string.

        begin = first;
        end = Find(first, last, ':'); // Logging level.

        if (begin != end)
        {
            count = static_cast<COUNT_T>(end - begin);
            if (count > 0)
            {
                SString value;
                value.SetANSI(begin, count);
                loggingLevel = Clamp(
                    static_cast<uint32_t>(strtoul(value.GetANSI(scratchBuffer), nullptr, 0)),
                    static_cast<uint32_t>(EventPipeEventLevel::LogAlways),
                    static_cast<uint32_t>(EventPipeEventLevel::Verbose));
            }
        }

        if (first == last)
            break; // There is nothing after the separator ':'
        ++first;   // Move on to the next string.

        count = static_cast<COUNT_T>(last - first);
        if (count > 0)
        {
            SString tmpProviderName;
            tmpProviderName.SetANSI(first, count);
            tmpProviderName.ConvertToUnicode(filterData);
        }

        break;
    }

    // TODO: Move to a different function?
    if (providerName.GetCount() > 0)
    {
        NewHolder<EventPipeProviderConfiguration> hEventPipeProviderConfiguration = new (nothrow) EventPipeProviderConfiguration(
            providerName.GetUnicode(), // TODO: Make sure we do not end up with a dangling reference.
            keywords,
            loggingLevel,
            filterData.GetCount() > 0 ? filterData.GetUnicode() : nullptr);
        if (hEventPipeProviderConfiguration.IsNull())
            return;

        NewHolder<SListElem<EventPipeProviderConfiguration *>> hSListElem = new (nothrow) SListElem<EventPipeProviderConfiguration *>(
            hEventPipeProviderConfiguration.Extract());
        if (hSListElem.IsNull())
            return;
        providers.InsertTail(hSListElem.Extract());
    }
}

void ProvidersParser(const char *first, const char *last, Providers &providers)
{
    // Providers:       "Provider[,Provider]"

    for (; first != last; ++first)
    {
        const char *begin = first;
        const char *end = Find(first, last, ',');

        // TODO: Parse provider.
        ProviderParser(begin, end, providers);
        if (end == last)
            break;
    }
}

void ConfigurationParser(
    const char *first,
    const char *last,
    SString &strOutputPath,
    unsigned int &circularBufferSizeInMB,
    Providers &providers,
    UINT64 &multiFileTraceLengthInSeconds)
{
    // Matching Config file keys in EventPipeController.cs
    SString Providers;
    Providers.SetANSI("Providers");
    SString CircularMB;
    CircularMB.SetANSI("CircularMB");
    SString OutputPath;
    OutputPath.SetANSI("OutputPath");
    SString ProcessID;
    ProcessID.SetANSI("ProcessID");
    SString MultiFileSec;
    MultiFileSec.SetANSI("MultiFileSec");

    // TODO: Set defaults?
    const unsigned int DefaultCircularBufferMB = 1024; // 1 GB // TODO: Do we need this much as default?
    circularBufferSizeInMB = DefaultCircularBufferMB;

    const char *begin;
    const char *end;
    SString key;
    SString value;
    for (; first != last; ++first)
    {
        begin = first;
        end = Find(first, last, '=');
        key.SetANSI(reinterpret_cast<const ANSI *>(begin), static_cast<COUNT_T>(end - begin));
        if (end == last)
            break; // Only found a key (no value).

        if (first == last)
            break; // There is nothing after the separator '='
        ++first;   // Move on to the next string.

        begin = first;
        end = Find(first, last, '\n');
        value.SetANSI(reinterpret_cast<const ANSI *>(begin), static_cast<COUNT_T>(end - begin));
        if (value.GetCount() > 0 && value[value.GetCount() - 1] == '\r')
            value.Truncate(value.End() - 1);

        if (key.GetCount() > 0 && value.GetCount() > 0)
        {
            // TODO: Maybe trim white spaces, for an user friendlier experience?
            // TODO: Case insensitive comparison.

            StackScratchBuffer scratchBuffer;
            if (key.Compare(Providers) == 0)
            {
                // TODO: Parse into an array of providers?
                ProvidersParser(
                    reinterpret_cast<const char *>(value.GetANSI(scratchBuffer)),
                    reinterpret_cast<const char *>(value.GetANSI(scratchBuffer)) + value.GetCount(),
                    providers);
            }
            else if (key.Compare(CircularMB) == 0)
            {
                circularBufferSizeInMB = static_cast<uint32_t>(strtoul(value.GetANSI(scratchBuffer), nullptr, 0));
            }
            else if (key.Compare(OutputPath) == 0)
            {
                // TODO: Generate output file name (This is just the "Directory").
                //  Expected file name should be: "<AppName>.<Pid>.netperf"
                strOutputPath = value; // TODO: Expected type is LPCWSTR.
            }
            else if (key.Compare(ProcessID) == 0)
            {
                // TODO: Add error handling (overflow?).
                const uint32_t processId = static_cast<uint32_t>(strtoul(value.GetANSI(scratchBuffer), nullptr, 0));
                const DWORD pid = ProcessDescriptor::FromCurrentProcess().m_Pid;

                // TODO: If set, bail out early if the specified process does not match the current process.
                //  Do we need this anymore?
            }
            else if (key.Compare(MultiFileSec) == 0)
            {
                // TODO: Add error handling (overflow?).
                multiFileTraceLengthInSeconds = static_cast<UINT64>(strtoull(value.GetANSI(scratchBuffer), nullptr, 0));
            }
        }

        if (end == last)
            break; // Already reached EOL.
    }

    // TODO: Clamp values where applicable?
}

//! TODO: Temp func.
static uint64_t EventPipeEnableDispatcher(const char *string, uint32_t nBytes)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(string != nullptr);
    }
    CONTRACTL_END;

    // TODO: Parse and get the following data:
    SString strOutputPath;
    unsigned int circularBufferSizeInMB = 0;  // What should be the default minimum?
    UINT64 multiFileTraceLengthInSeconds = 0; // What should be the default minimum?
    Providers providers;

    ConfigurationParser(
        string,
        string + nBytes,
        strOutputPath,
        circularBufferSizeInMB,
        providers,
        multiFileTraceLengthInSeconds);

    uint32_t nElements = 0;
    for (auto pElem = providers.GetHead(); pElem != nullptr; pElem = providers.GetNext(pElem))
        ++nElements;

    if (nElements == 0)
        return (EventPipeSessionID) nullptr;

    NewArrayHolder<EventPipeProviderConfiguration> hRundownProviders = new (nothrow) EventPipeProviderConfiguration[nElements];
    if (hRundownProviders.IsNull())
        return (EventPipeSessionID) nullptr;

    uint32_t i = 0;
    for (auto pElem = providers.GetHead(); pElem != nullptr; pElem = providers.GetNext(pElem))
        hRundownProviders.GetValue()[i++] = *pElem->GetValue();

    const uint32_t profilerSamplingRateInNanoseconds = 1000000; // TODO: Read from user input.
    SampleProfiler::SetSamplingRate(profilerSamplingRateInNanoseconds);
    auto sessionId = EventPipe::Enable(
        strOutputPath.GetUnicode(),     // outputFile
        circularBufferSizeInMB,         // circularBufferSizeInMB
        hRundownProviders.Extract(),    // pProviders
        nElements,                      // numProviders
        multiFileTraceLengthInSeconds); // multiFileTraceLengthInSeconds

    // Clear the providers
    auto pIterElement = providers.GetHead();
    while (pIterElement != nullptr)
    {
        auto pCurrentElement = pIterElement;
        pIterElement = providers.GetNext(pIterElement);

        auto pEventPipeProviderConfiguration = pCurrentElement->GetValue();
        delete pEventPipeProviderConfiguration;
        delete pCurrentElement;
    }

    return sessionId;
}

static uint64_t EventPipeDisableDispatcher(const char *buffer, uint32_t nBytes)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(buffer != nullptr);
    }
    CONTRACTL_END;

    if (nBytes != sizeof(uint64_t))
        return 0;

    // TODO:
    const EventPipeSessionID sessionId = *(reinterpret_cast<const uint64_t *>(buffer));
    EventPipe::Disable(sessionId);
    return nBytes;
}

static DWORD WINAPI EventPipeMainThread(LPVOID lpThreadParameter)
{
    const ProcessDescriptor pd = ProcessDescriptor::FromCurrentProcess();
    DiagnosticsIpc ipc("dotnetcore-eventpipe", pd.m_Pid);

    while (true)
    {
        bool fSuccess = ipc.Open();
        if (!fSuccess)
        {
            // TODO: Add error handling.
            return 1;
        }

        fSuccess = ipc.Accept();
        if (!fSuccess)
        {
            // TODO: Add error handling.
            ipc.Close();
            continue;
        }

        uint32_t nNumberOfBytesRead = 0;
        IpcHeader header;
        fSuccess = ipc.Read(&header, sizeof(header), nNumberOfBytesRead);

        if (!fSuccess || nNumberOfBytesRead != sizeof(header))
        {
            // TODO: Add error handling.
            ipc.Close();
            continue;
        }
        else
        {
            // TODO: Read within a loop.
            const uint32_t BufferSize = 8192;
            char buffer[BufferSize]{};
            fSuccess = ipc.Read(buffer, sizeof(buffer), nNumberOfBytesRead);
            if (!fSuccess)
            {
                // TODO: Add error handing.
                ipc.Close();
                continue;
            }

            switch (header.requestType)
            {
            case EventPipeMessageType::Enable:
            {
                auto response = EventPipeEnableDispatcher(buffer, nNumberOfBytesRead);
                uint32_t nNumberOfBytesWritten = 0;
                fSuccess = ipc.Write(&response, sizeof(response), nNumberOfBytesWritten);
                if (nNumberOfBytesWritten != sizeof(response))
                {
                    // TODO: Add error handling.
                    ipc.Close();
                    continue;
                }

                if (!ipc.Flush())
                {
                    // TODO: Add error handling.
                    ipc.Close();
                    continue;
                }
            }
            break;

            case EventPipeMessageType::Disable:
                EventPipeDisableDispatcher(buffer, nNumberOfBytesRead);
                break;

            // case EventPipeMessageType::GetSessionInfo:
            // case EventPipeMessageType::CreateProvider:
            // case EventPipeMessageType::DefineEvent:
            // case EventPipeMessageType::GetProvider:
            // case EventPipeMessageType::DeleteProvider:
            // case EventPipeMessageType::EventActivityIdControl:
            // case EventPipeMessageType::WriteEvent:
            // case EventPipeMessageType::WriteEventData:
            // case EventPipeMessageType::GetNextEvent:
            //     break;

            default:
                // TODO: Add error handling?
                break;
            }
        }

        fSuccess = ipc.Close();
        if (!fSuccess)
        {
            // TODO: Add error handing.
            return 1;
        }
    }

    return 0;
}

void EventPipeInternal::Initialize()
{
    DWORD dwThreadId = 0;
    HANDLE hThread = CreateThread(
        nullptr,             // no security attribute
        0,                   // default stack size
        EventPipeMainThread, // thread proc
        nullptr,             // thread parameter
        0,                   // not suspended
        &dwThreadId);        // returns thread ID
    if (hThread == nullptr)
    {
        // TODO: Failed to create IPC thread. What should we do here?
    }
    else
    {
        CloseHandle(hThread); // Maybe hold on to the thread to abort/cleanup atexit?
    }
}

void EventPipeInternal::Shutdown()
{
    // TODO: Close IPC server thread?
}

UINT64 QCALLTYPE EventPipeInternal::Enable(
    __in_z LPCWSTR outputFile,
    UINT32 circularBufferSizeInMB,
    INT64 profilerSamplingRateInNanoseconds,
    EventPipeProviderConfiguration *pProviders,
    INT32 numProviders,
    UINT64 multiFileTraceLengthInSeconds)
{
    QCALL_CONTRACT;

    UINT64 sessionID = 0;

    BEGIN_QCALL;
    SampleProfiler::SetSamplingRate((unsigned long)profilerSamplingRateInNanoseconds);
    sessionID = EventPipe::Enable(outputFile, circularBufferSizeInMB, pProviders, numProviders, multiFileTraceLengthInSeconds);
    END_QCALL;

    return sessionID;
}

void QCALLTYPE EventPipeInternal::Disable(UINT64 sessionID)
{
    QCALL_CONTRACT;

    BEGIN_QCALL;
    EventPipe::Disable(sessionID);
    END_QCALL;
}

bool QCALLTYPE EventPipeInternal::GetSessionInfo(UINT64 sessionID, EventPipeSessionInfo *pSessionInfo)
{
    QCALL_CONTRACT;

    bool retVal = false;
    BEGIN_QCALL;

    if (pSessionInfo != NULL)
    {
        EventPipeSession *pSession = EventPipe::GetSession(sessionID);
        if (pSession != NULL)
        {
            pSessionInfo->StartTimeAsUTCFileTime = pSession->GetStartTime();
            pSessionInfo->StartTimeStamp.QuadPart = pSession->GetStartTimeStamp().QuadPart;
            QueryPerformanceFrequency(&pSessionInfo->TimeStampFrequency);
            retVal = true;
        }
    }

    END_QCALL;
    return retVal;
}

INT_PTR QCALLTYPE EventPipeInternal::CreateProvider(
    __in_z LPCWSTR providerName,
    EventPipeCallback pCallbackFunc)
{
    QCALL_CONTRACT;

    EventPipeProvider *pProvider = NULL;

    BEGIN_QCALL;

    pProvider = EventPipe::CreateProvider(providerName, pCallbackFunc, NULL);

    END_QCALL;

    return reinterpret_cast<INT_PTR>(pProvider);
}

INT_PTR QCALLTYPE EventPipeInternal::DefineEvent(
    INT_PTR provHandle,
    UINT32 eventID,
    __int64 keywords,
    UINT32 eventVersion,
    UINT32 level,
    void *pMetadata,
    UINT32 metadataLength)
{
    QCALL_CONTRACT;

    EventPipeEvent *pEvent = NULL;

    BEGIN_QCALL;

    _ASSERTE(provHandle != NULL);
    EventPipeProvider *pProvider = reinterpret_cast<EventPipeProvider *>(provHandle);
    pEvent = pProvider->AddEvent(eventID, keywords, eventVersion, (EventPipeEventLevel)level, (BYTE *)pMetadata, metadataLength);
    _ASSERTE(pEvent != NULL);

    END_QCALL;

    return reinterpret_cast<INT_PTR>(pEvent);
}

INT_PTR QCALLTYPE EventPipeInternal::GetProvider(
    __in_z LPCWSTR providerName)
{
    QCALL_CONTRACT;

    EventPipeProvider *pProvider = NULL;

    BEGIN_QCALL;

    pProvider = EventPipe::GetProvider(providerName);

    END_QCALL;

    return reinterpret_cast<INT_PTR>(pProvider);
}

void QCALLTYPE EventPipeInternal::DeleteProvider(INT_PTR provHandle)
{
    QCALL_CONTRACT;
    BEGIN_QCALL;

    if (provHandle != NULL)
    {
        EventPipeProvider *pProvider = reinterpret_cast<EventPipeProvider *>(provHandle);
        EventPipe::DeleteProvider(pProvider);
    }

    END_QCALL;
}

int QCALLTYPE EventPipeInternal::EventActivityIdControl(
    uint32_t controlCode,
    GUID *pActivityId)
{

    QCALL_CONTRACT;

    int retVal = 0;

    BEGIN_QCALL;

    Thread *pThread = GetThread();
    if (pThread == NULL || pActivityId == NULL)
    {
        retVal = 1;
    }
    else
    {
        ActivityControlCode activityControlCode = (ActivityControlCode)controlCode;
        GUID currentActivityId;
        switch (activityControlCode)
        {
        case ActivityControlCode::EVENT_ACTIVITY_CONTROL_GET_ID:

            *pActivityId = *pThread->GetActivityId();
            break;

        case ActivityControlCode::EVENT_ACTIVITY_CONTROL_SET_ID:

            pThread->SetActivityId(pActivityId);
            break;

        case ActivityControlCode::EVENT_ACTIVITY_CONTROL_CREATE_ID:

            CoCreateGuid(pActivityId);
            break;

        case ActivityControlCode::EVENT_ACTIVITY_CONTROL_GET_SET_ID:

            currentActivityId = *pThread->GetActivityId();
            pThread->SetActivityId(pActivityId);
            *pActivityId = currentActivityId;
            break;

        case ActivityControlCode::EVENT_ACTIVITY_CONTROL_CREATE_SET_ID:

            *pActivityId = *pThread->GetActivityId();
            CoCreateGuid(&currentActivityId);
            pThread->SetActivityId(&currentActivityId);
            break;

        default:
            retVal = 1;
        }
    }

    END_QCALL;
    return retVal;
}

void QCALLTYPE EventPipeInternal::WriteEvent(
    INT_PTR eventHandle,
    UINT32 eventID,
    void *pData,
    UINT32 length,
    LPCGUID pActivityId,
    LPCGUID pRelatedActivityId)
{
    QCALL_CONTRACT;
    BEGIN_QCALL;

    _ASSERTE(eventHandle != NULL);
    EventPipeEvent *pEvent = reinterpret_cast<EventPipeEvent *>(eventHandle);
    EventPipe::WriteEvent(*pEvent, (BYTE *)pData, length, pActivityId, pRelatedActivityId);

    END_QCALL;
}

void QCALLTYPE EventPipeInternal::WriteEventData(
    INT_PTR eventHandle,
    UINT32 eventID,
    EventData *pEventData,
    UINT32 eventDataCount,
    LPCGUID pActivityId,
    LPCGUID pRelatedActivityId)
{
    QCALL_CONTRACT;
    BEGIN_QCALL;

    _ASSERTE(eventHandle != NULL);
    EventPipeEvent *pEvent = reinterpret_cast<EventPipeEvent *>(eventHandle);
    EventPipe::WriteEvent(*pEvent, pEventData, eventDataCount, pActivityId, pRelatedActivityId);

    END_QCALL;
}

bool QCALLTYPE EventPipeInternal::GetNextEvent(
    EventPipeEventInstanceData *pInstance)
{
    QCALL_CONTRACT;

    EventPipeEventInstance *pNextInstance = NULL;
    BEGIN_QCALL;

    _ASSERTE(pInstance != NULL);

    pNextInstance = EventPipe::GetNextEvent();
    if (pNextInstance)
    {
        pInstance->ProviderID = pNextInstance->GetEvent()->GetProvider();
        pInstance->EventID = pNextInstance->GetEvent()->GetEventID();
        pInstance->ThreadID = pNextInstance->GetThreadId();
        pInstance->TimeStamp.QuadPart = pNextInstance->GetTimeStamp()->QuadPart;
        pInstance->ActivityId = *pNextInstance->GetActivityId();
        pInstance->RelatedActivityId = *pNextInstance->GetRelatedActivityId();
        pInstance->Payload = pNextInstance->GetData();
        pInstance->PayloadLength = pNextInstance->GetDataLength();
    }

    END_QCALL;
    return pNextInstance != NULL;
}

#endif // FEATURE_PERFTRACING
