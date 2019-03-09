// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// #include <stdlib.h>
#include "common.h"
#include "clrtypes.h"
#include "diagnosticsipc.h"
#include "processdescriptor.h"
#include "safemath.h"
#include "eventpipe.h"
#include "eventpipebuffermanager.h"
#include "eventpipeconfiguration.h"
#include "eventpipesessionprovider.h"
#include "eventpipeevent.h"
#include "eventpipeeventsource.h"
#include "eventpipefile.h"
#include "eventpipeprovider.h"
#include "eventpipesession.h"
#include "eventpipejsonfile.h"
#include "eventtracebase.h"
#include "sampleprofiler.h"
#include "win32threadpool.h"

#ifdef FEATURE_PAL
#include "pal.h"
#endif // FEATURE_PAL

#ifdef FEATURE_PERFTRACING

CrstStatic EventPipe::s_configCrst;
bool EventPipe::s_tracingInitialized = false;
EventPipeConfiguration *EventPipe::s_pConfig = NULL;
EventPipeSession *EventPipe::s_pSession = NULL;
EventPipeBufferManager *EventPipe::s_pBufferManager = NULL;
LPCWSTR EventPipe::s_pOutputPath = NULL;
EventPipeFile *EventPipe::s_pFile = NULL;
EventPipeEventSource *EventPipe::s_pEventSource = NULL;
LPCWSTR EventPipe::s_pCommandLine = NULL;
unsigned long EventPipe::s_nextFileIndex;
HANDLE EventPipe::s_fileSwitchTimerHandle = NULL;
ULONGLONG EventPipe::s_lastFileSwitchTime = 0;

#ifdef FEATURE_PAL
// This function is auto-generated from /src/scripts/genEventPipe.py
extern "C" void InitProvidersAndEvents();
#else
void InitProvidersAndEvents();
#endif

EventPipeEventPayload::EventPipeEventPayload(BYTE *pData, unsigned int length)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pData = pData;
    m_pEventData = NULL;
    m_eventDataCount = 0;
    m_allocatedData = false;

    m_size = length;
}

EventPipeEventPayload::EventPipeEventPayload(EventData *pEventData, unsigned int eventDataCount)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pData = NULL;
    m_pEventData = pEventData;
    m_eventDataCount = eventDataCount;
    m_allocatedData = false;

    S_UINT32 tmp_size = S_UINT32(0);
    for (unsigned int i = 0; i < m_eventDataCount; i++)
    {
        tmp_size += S_UINT32(m_pEventData[i].Size);
    }

    if (tmp_size.IsOverflow())
    {
        // If there is an overflow, drop the data and create an empty payload
        m_pEventData = NULL;
        m_eventDataCount = 0;
        m_size = 0;
    }
    else
    {
        m_size = tmp_size.Value();
    }
}

EventPipeEventPayload::~EventPipeEventPayload()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if (m_allocatedData && m_pData != NULL)
    {
        delete[] m_pData;
        m_pData = NULL;
    }
}

void EventPipeEventPayload::Flatten()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if (m_size > 0)
    {
        if (!IsFlattened())
        {
            BYTE *tmp_pData = new (nothrow) BYTE[m_size];
            if (tmp_pData != NULL)
            {
                m_allocatedData = true;
                CopyData(tmp_pData);
                m_pData = tmp_pData;
            }
        }
    }
}

void EventPipeEventPayload::CopyData(BYTE *pDst)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if (m_size > 0)
    {
        if (IsFlattened())
        {
            memcpy(pDst, m_pData, m_size);
        }

        else if (m_pEventData != NULL)
        {
            unsigned int offset = 0;
            for (unsigned int i = 0; i < m_eventDataCount; i++)
            {
                memcpy(pDst + offset, (BYTE *)m_pEventData[i].Ptr, m_pEventData[i].Size);
                offset += m_pEventData[i].Size;
            }
        }
    }
}

BYTE *EventPipeEventPayload::GetFlatData()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if (!IsFlattened())
    {
        Flatten();
    }
    return m_pData;
}

void EventPipe::Initialize()
{
    STANDARD_VM_CONTRACT;

    s_tracingInitialized = s_configCrst.InitNoThrow(
        CrstEventPipe,
        (CrstFlags)(CRST_REENTRANCY | CRST_TAKEN_DURING_SHUTDOWN | CRST_HOST_BREAKABLE));

    s_pConfig = new EventPipeConfiguration();
    s_pConfig->Initialize();

    s_pBufferManager = new EventPipeBufferManager();

    s_pEventSource = new EventPipeEventSource();

    // This calls into auto-generated code to initialize the runtime providers
    // and events so that the EventPipe configuration lock isn't taken at runtime
    InitProvidersAndEvents();
}

void EventPipe::Shutdown()
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Mark tracing as no longer initialized.
    s_tracingInitialized = false;

    // We are shutting down, so if disabling EventPipe throws, we need to move along anyway.
    EX_TRY
    {
        Disable((EventPipeSessionID)s_pSession);
    }
    EX_CATCH {}
    EX_END_CATCH(SwallowAllExceptions);

    // Save pointers to the configuration and buffer manager.
    EventPipeConfiguration *pConfig = s_pConfig;
    EventPipeBufferManager *pBufferManager = s_pBufferManager;

    // Set the static pointers to NULL so that the rest of the EventPipe knows that they are no longer available.
    // Flush process write buffers to make sure other threads can see the change.
    s_pConfig = NULL;
    s_pBufferManager = NULL;
    FlushProcessWriteBuffers();

    // Free resources.
    delete pConfig;
    delete pBufferManager;
    delete s_pEventSource;
    s_pEventSource = NULL;
    delete[] s_pOutputPath;
    s_pOutputPath = NULL;

    // On Windows, this is just a pointer to the return value from
    // GetCommandLineW(), so don't attempt to free it.
#ifdef FEATURE_PAL
    delete[] s_pCommandLine;
    s_pCommandLine = NULL;
#endif
}

EventPipeSessionID EventPipe::Enable(
    LPCWSTR strOutputPath,
    uint32_t circularBufferSizeInMB,
    uint64_t profilerSamplingRateInNanoseconds,
    const EventPipeSessionProviderList &providers,
    uint64_t multiFileTraceLengthInSeconds)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Create a new session.
    SampleProfiler::SetSamplingRate((unsigned long)profilerSamplingRateInNanoseconds);
    EventPipeSession *pSession = s_pConfig->CreateSession(
        (strOutputPath != NULL) ? EventPipeSessionType::File : EventPipeSessionType::Streaming,
        circularBufferSizeInMB,
        providers,
        multiFileTraceLengthInSeconds);

    // Enable the session.
    return Enable(strOutputPath, pSession);
}

EventPipeSessionID EventPipe::Enable(LPCWSTR strOutputPath, EventPipeSession *pSession)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pSession != NULL);
    }
    CONTRACTL_END;

    // If tracing is not initialized or is already enabled, bail here.
    if (!s_tracingInitialized || s_pConfig == NULL || s_pConfig->Enabled())
    {
        return 0;
    }

    // If the state or arguments are invalid, bail here.
    if (pSession == NULL || !pSession->IsValid())
    {
        return 0;
    }

    // Enable the EventPipe EventSource.
    s_pEventSource->Enable(pSession); // TODO: Should this be under the lock?

    // Take the lock before enabling tracing.
    CrstHolder _crst(GetLock());

    // Initialize the next file index.
    s_nextFileIndex = 1;

    // Initialize the last file switch time.
    s_lastFileSwitchTime = CLRGetTickCount64();

    // Create the event pipe file.
    // A NULL output path means that we should not write the results to a file.
    // This is used in the EventListener streaming case.
    if (strOutputPath != NULL)
    {
        // Save the output file path.
        SString outputPath(strOutputPath);
        SIZE_T outputPathLen = outputPath.GetCount();
        WCHAR *pOutputPath = new WCHAR[outputPathLen + 1];
        wcsncpy(pOutputPath, outputPath.GetUnicode(), outputPathLen);
        pOutputPath[outputPathLen] = '\0';
        s_pOutputPath = pOutputPath;

        SString nextTraceFilePath;
        GetNextFilePath(pSession, nextTraceFilePath);

        s_pFile = new EventPipeFile(nextTraceFilePath);
    }

    // Save the session.
    s_pSession = pSession;

    // Enable tracing.
    s_pConfig->Enable(s_pSession);

    // Enable the sample profiler
    SampleProfiler::Enable();

    // Enable the file switch timer if needed.
    if (s_pSession->GetMultiFileTraceLengthInSeconds() > 0)
    {
        CreateFileSwitchTimer();
    }

    // Return the session ID.
    return (EventPipeSessionID)s_pSession;
}

void EventPipe::Disable(EventPipeSessionID id)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Only perform the disable operation if the session ID
    // matches the current active session.
    if (id != (EventPipeSessionID)s_pSession)
    {
        return;
    }

    // Don't block GC during clean-up.
    GCX_PREEMP();

    // Take the lock before disabling tracing.
    CrstHolder _crst(GetLock());

    if (s_pConfig != NULL && s_pConfig->Enabled())
    {
        // Disable the profiler.
        SampleProfiler::Disable();

        // Log the process information event.
        s_pEventSource->SendProcessInfo(s_pCommandLine);

        // Log the runtime information event.
        ETW::InfoLog::RuntimeInformation(ETW::InfoLog::InfoStructs::Normal);

        // Disable tracing.
        s_pConfig->Disable(s_pSession);

        // Delete the session.
        s_pConfig->DeleteSession(s_pSession);
        s_pSession = NULL;

        // Delete the file switch timer.
        DeleteFileSwitchTimer();

        // Flush all write buffers to make sure that all threads see the change.
        FlushProcessWriteBuffers();

        // Write to the file.
        if (s_pFile != NULL)
        {
            LARGE_INTEGER disableTimeStamp;
            QueryPerformanceCounter(&disableTimeStamp);
            s_pBufferManager->WriteAllBuffersToFile(s_pFile, disableTimeStamp);

            if (CLRConfig::GetConfigValue(CLRConfig::INTERNAL_EventPipeRundown) > 0)
            {
                // Before closing the file, do rundown.
                const unsigned int numRundownProviders = 2;
                EventPipeProviderConfiguration rundownProviders[] = {
                    {W("Microsoft-Windows-DotNETRuntime"), 0x80020138, static_cast<unsigned int>(EventPipeEventLevel::Verbose), NULL},       // Public provider.
                    {W("Microsoft-Windows-DotNETRuntimeRundown"), 0x80020138, static_cast<unsigned int>(EventPipeEventLevel::Verbose), NULL} // Rundown provider.
                };

                // Create a new session.
                // TODO: We should probably create this on initialization and cache it.
                EventPipeSessionProviderList providers;
                for (uint32_t i = 0; i < numRundownProviders; ++i)
                {
                    const EventPipeProviderConfiguration &config = rundownProviders[i];
                    providers.AddSessionProvider(new EventPipeSessionProvider(
                        config.GetProviderName(),
                        config.GetKeywords(),
                        (EventPipeEventLevel)config.GetLevel(),
                        config.GetFilterData()));
                }

                // The circular buffer size doesn't matter because all events are written synchronously during rundown.
                s_pSession = s_pConfig->CreateSession(
                    EventPipeSessionType::File,
                    1 /* circularBufferSizeInMB */,
                    providers);
                s_pConfig->EnableRundown(s_pSession);

                // Ask the runtime to emit rundown events.
                if (g_fEEStarted && !g_fEEShutDown)
                {
                    ETW::EnumerationLog::EndRundown();
                }

                // Disable the event pipe now that rundown is complete.
                s_pConfig->Disable(s_pSession);

                // Delete the rundown session.
                s_pConfig->DeleteSession(s_pSession);
                s_pSession = NULL;
            }

            delete s_pFile;
            s_pFile = NULL;
        }

        // De-allocate buffers.
        s_pBufferManager->DeAllocateBuffers();

        // Delete deferred providers.
        // Providers can't be deleted during tracing because they may be needed when serializing the file.
        s_pConfig->DeleteDeferredProviders();
    }
}

void EventPipe::CreateFileSwitchTimer()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END

    NewHolder<ThreadpoolMgr::TimerInfoContext> timerContextHolder = new (nothrow) ThreadpoolMgr::TimerInfoContext();
    if (timerContextHolder == NULL)
    {
        return;
    }
    timerContextHolder->TimerId = 0;

    bool success = false;
    _ASSERTE(s_fileSwitchTimerHandle == NULL);
    EX_TRY
    {
        if (ThreadpoolMgr::CreateTimerQueueTimer(
                &s_fileSwitchTimerHandle,
                SwitchToNextFileTimerCallback,
                timerContextHolder,
                FileSwitchTimerPeriodMS,
                FileSwitchTimerPeriodMS,
                0 /* flags */))
        {
            _ASSERTE(s_fileSwitchTimerHandle != NULL);
            success = true;
        }
    }
    EX_CATCH
    {
    }
    EX_END_CATCH(RethrowTerminalExceptions);
    if (!success)
    {
        _ASSERTE(s_fileSwitchTimerHandle == NULL);
        return;
    }

    timerContextHolder.SuppressRelease(); // the timer context is automatically deleted by the timer infrastructure
}

void EventPipe::DeleteFileSwitchTimer()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END

    if ((s_fileSwitchTimerHandle != NULL) && (ThreadpoolMgr::DeleteTimerQueueTimer(s_fileSwitchTimerHandle, NULL)))
    {
        s_fileSwitchTimerHandle = NULL;
    }
}

void WINAPI EventPipe::SwitchToNextFileTimerCallback(PVOID parameter, BOOLEAN timerFired)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(timerFired);
    }
    CONTRACTL_END;

    // Take the lock control lock to make sure that tracing isn't disabled during this operation.
    CrstHolder _crst(GetLock());

    // Make sure that we should actually switch files.
    UINT64 multiFileTraceLengthInSeconds = s_pSession->GetMultiFileTraceLengthInSeconds();
    if (!Enabled() || s_pSession->GetSessionType() != EventPipeSessionType::File || multiFileTraceLengthInSeconds == 0)
    {
        return;
    }

    GCX_PREEMP();

    if (CLRGetTickCount64() > (s_lastFileSwitchTime + (multiFileTraceLengthInSeconds * 1000)))
    {
        SwitchToNextFile();
        s_lastFileSwitchTime = CLRGetTickCount64();
    }
}

void EventPipe::SwitchToNextFile()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(s_pSession != NULL);
        PRECONDITION(GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END

    // Get the current time stamp.
    // WriteAllBuffersToFile will use this to ensure that no events after the current timestamp are written into the file.
    LARGE_INTEGER stopTimeStamp;
    QueryPerformanceCounter(&stopTimeStamp);
    s_pBufferManager->WriteAllBuffersToFile(s_pFile, stopTimeStamp);

    // Open the new file.
    SString nextTraceFilePath;
    GetNextFilePath(s_pSession, nextTraceFilePath);
    EventPipeFile *pFile = new (nothrow) EventPipeFile(nextTraceFilePath);
    if (pFile == NULL)
    {
        // TODO: Add error handling.
        return;
    }

    // Close the previous file.
    delete s_pFile;

    // Swap in the new file.
    s_pFile = pFile;
}

void EventPipe::GetNextFilePath(EventPipeSession *pSession, SString &nextTraceFilePath)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pSession != NULL);
        PRECONDITION(GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    // Set the full path to the requested trace file as the next file path.
    nextTraceFilePath.Set(s_pOutputPath);

    // If multiple files have been requested, then add a sequence number to the trace file name.
    UINT64 multiFileTraceLengthInSeconds = pSession->GetMultiFileTraceLengthInSeconds();
    if (multiFileTraceLengthInSeconds > 0)
    {
        // Remove the ".netperf" file extension if it exists.
        SString::Iterator netPerfExtension = nextTraceFilePath.End();
        if (nextTraceFilePath.FindBack(netPerfExtension, W(".netperf")))
        {
            nextTraceFilePath.Truncate(netPerfExtension);
        }

        // Add the sequence number and the ".netperf" file extension.
        WCHAR strNextIndex[21];
        swprintf_s(strNextIndex, 21, W(".%u.netperf"), s_nextFileIndex++);
        nextTraceFilePath.Append(strNextIndex);
    }
}

EventPipeSession *EventPipe::GetSession(EventPipeSessionID id)
{
    LIMITED_METHOD_CONTRACT;

    EventPipeSession *pSession = NULL;
    if ((EventPipeSessionID)s_pSession == id)
    {
        pSession = s_pSession;
    }
    return pSession;
}

bool EventPipe::Enabled()
{
    LIMITED_METHOD_CONTRACT;

    bool enabled = false;
    if (s_pConfig != NULL)
    {
        enabled = s_pConfig->Enabled();
    }

    return enabled;
}

EventPipeProvider *EventPipe::CreateProvider(const SString &providerName, EventPipeCallback pCallbackFunction, void *pCallbackData)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeProvider *pProvider = NULL;
    if (s_pConfig != NULL)
    {
        pProvider = s_pConfig->CreateProvider(providerName, pCallbackFunction, pCallbackData);
    }

    return pProvider;
}

EventPipeProvider *EventPipe::GetProvider(const SString &providerName)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeProvider *pProvider = NULL;
    if (s_pConfig != NULL)
    {
        pProvider = s_pConfig->GetProvider(providerName);
    }

    return pProvider;
}

void EventPipe::DeleteProvider(EventPipeProvider *pProvider)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the lock to make sure that we don't have a race
    // between disabling tracing and deleting a provider
    // where we hold a provider after tracing has been disabled.
    CrstHolder _crst(GetLock());

    if (pProvider != NULL)
    {
        if (Enabled())
        {
            // Save the provider until the end of the tracing session.
            pProvider->SetDeleteDeferred();
        }
        else
        {
            // Delete the provider now.
            if (s_pConfig != NULL)
            {
                s_pConfig->DeleteProvider(pProvider);
            }
        }
    }
}

void EventPipe::WriteEvent(EventPipeEvent &event, BYTE *pData, unsigned int length, LPCGUID pActivityId, LPCGUID pRelatedActivityId)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeEventPayload payload(pData, length);
    EventPipe::WriteEventInternal(event, payload, pActivityId, pRelatedActivityId);
}

void EventPipe::WriteEvent(EventPipeEvent &event, EventData *pEventData, unsigned int eventDataCount, LPCGUID pActivityId, LPCGUID pRelatedActivityId)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeEventPayload payload(pEventData, eventDataCount);
    EventPipe::WriteEventInternal(event, payload, pActivityId, pRelatedActivityId);
}

void EventPipe::WriteEventInternal(EventPipeEvent &event, EventPipeEventPayload &payload, LPCGUID pActivityId, LPCGUID pRelatedActivityId)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Exit early if the event is not enabled.
    if (!event.IsEnabled())
    {
        return;
    }

    // Get the current thread;
    Thread *pThread = GetThread();

    if (s_pConfig == NULL)
    {
        // We can't procede without a configuration
        return;
    }
    _ASSERTE(s_pSession != NULL);

    // If the activity id isn't specified AND we are in a managed thread, pull it from the current thread.
    // If pThread is NULL (we aren't in writing from a managed thread) then pActivityId can be NULL
    if (pActivityId == NULL && pThread != NULL)
    {
        pActivityId = pThread->GetActivityId();
    }

    if (!s_pConfig->RundownEnabled() && s_pBufferManager != NULL)
    {
        s_pBufferManager->WriteEvent(pThread, *s_pSession, event, payload, pActivityId, pRelatedActivityId);
    }
    else if (s_pConfig->RundownEnabled())
    {
        // It is possible that some events that are enabled on rundown can be emitted from other threads.
        // We're not interested in these events and they can cause corrupted trace files because rundown
        // events are written synchronously and not under lock.
        // If we encounter an event that did not originate on the thread that is doing rundown, ignore it.
        if (pThread == NULL || !s_pConfig->IsRundownThread(pThread))
        {
            return;
        }

        BYTE *pData = payload.GetFlatData();
        if (pData != NULL)
        {
            // Write synchronously to the file.
            // We're under lock and blocking the disabling thread.
            // This copy occurs here (rather than at file write) because
            // A) The FastSerializer API would need to change if we waited
            // B) It is unclear there is a benefit to multiple file write calls
            //    as opposed a a buffer copy here
            EventPipeEventInstance instance(
                *s_pSession,
                event,
                pThread->GetOSThreadId(),
                pData,
                payload.GetSize(),
                pActivityId,
                pRelatedActivityId);

            if (s_pFile != NULL)
            {
                // EventPipeFile::WriteEvent needs to allocate a metadata event
                // and can therefore throw. In this context we will silently
                // fail rather than disrupt the caller
                EX_TRY
                {
                    s_pFile->WriteEvent(instance);
                }
                EX_CATCH {}
                EX_END_CATCH(SwallowAllExceptions);
            }
        }
    }
}

void EventPipe::WriteSampleProfileEvent(Thread *pSamplingThread, EventPipeEvent *pEvent, Thread *pTargetThread, StackContents &stackContents, BYTE *pData, unsigned int length)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
    }
    CONTRACTL_END;

    EventPipeEventPayload payload(pData, length);

    // Write the event to the thread's buffer.
    if (s_pBufferManager != NULL)
    {
        // Specify the sampling thread as the "current thread", so that we select the right buffer.
        // Specify the target thread so that the event gets properly attributed.
        s_pBufferManager->WriteEvent(pSamplingThread, *s_pSession, *pEvent, payload, NULL /* pActivityId */, NULL /* pRelatedActivityId */, pTargetThread, &stackContents);
    }
}

bool EventPipe::WalkManagedStackForCurrentThread(StackContents &stackContents)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    Thread *pThread = GetThread();
    if (pThread != NULL)
    {
        return WalkManagedStackForThread(pThread, stackContents);
    }

    return false;
}

bool EventPipe::WalkManagedStackForThread(Thread *pThread, StackContents &stackContents)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

    // Calling into StackWalkFrames in preemptive mode violates the host contract,
    // but this contract is not used on CoreCLR.
    CONTRACT_VIOLATION(HostViolation);

    stackContents.Reset();

    StackWalkAction swaRet = pThread->StackWalkFrames(
        (PSTACKWALKFRAMESCALLBACK)&StackWalkCallback,
        &stackContents,
        ALLOW_ASYNC_STACK_WALK | FUNCTIONSONLY | HANDLESKIPPEDFRAMES | ALLOW_INVALID_OBJECTS);

    return ((swaRet == SWA_DONE) || (swaRet == SWA_CONTINUE));
}

StackWalkAction EventPipe::StackWalkCallback(CrawlFrame *pCf, StackContents *pData)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pCf != NULL);
        PRECONDITION(pData != NULL);
    }
    CONTRACTL_END;

    // Get the IP.
    UINT_PTR controlPC = (UINT_PTR)pCf->GetRegisterSet()->ControlPC;
    if (controlPC == 0)
    {
        if (pData->GetLength() == 0)
        {
            // This happens for pinvoke stubs on the top of the stack.
            return SWA_CONTINUE;
        }
    }

    _ASSERTE(controlPC != 0);

    // Add the IP to the captured stack.
    pData->Append(
        controlPC,
        pCf->GetFunction());

    // Continue the stack walk.
    return SWA_CONTINUE;
}

EventPipeConfiguration *EventPipe::GetConfiguration()
{
    LIMITED_METHOD_CONTRACT;

    return s_pConfig;
}

CrstStatic *EventPipe::GetLock()
{
    LIMITED_METHOD_CONTRACT;

    return &s_configCrst;
}

void EventPipe::SaveCommandLine(LPCWSTR pwzAssemblyPath, int argc, LPCWSTR *argv)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(pwzAssemblyPath != NULL);
        PRECONDITION(argc <= 0 || argv != NULL);
    }
    CONTRACTL_END;

    // Get the command line.
    LPCWSTR osCommandLine = GetCommandLineW();

#ifndef FEATURE_PAL
    // On Windows, osCommandLine contains the executable and all arguments.
    s_pCommandLine = osCommandLine;
#else
    // On UNIX, the PAL doesn't have the command line arguments, so we must build the command line.
    // osCommandLine contains the full path to the executable.
    SString commandLine(osCommandLine);
    commandLine.Append((WCHAR)' ');
    commandLine.Append(pwzAssemblyPath);

    for (int i = 0; i < argc; i++)
    {
        commandLine.Append((WCHAR)' ');
        commandLine.Append(argv[i]);
    }

    // Allocate a new string for the command line.
    SIZE_T commandLineLen = commandLine.GetCount();
    WCHAR *pCommandLine = new WCHAR[commandLineLen + 1];
    wcsncpy(pCommandLine, commandLine.GetUnicode(), commandLineLen);
    pCommandLine[commandLineLen] = '\0';

    s_pCommandLine = pCommandLine;
#endif
}

EventPipeEventInstance *EventPipe::GetNextEvent()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
    }
    CONTRACTL_END;

    EventPipeEventInstance *pInstance = NULL;

    // Only fetch the next event if a tracing session exists.
    // The buffer manager is not disposed until the process is shutdown.
    if (s_pSession != NULL)
    {
        pInstance = s_pBufferManager->GetNextEvent();
    }

    return pInstance;
}

const uint32_t DefaultCircularBufferMB = 1024; // 1 GB // TODO: Do we need this much as default?
const uint64_t DefaultMultiFileTraceLengthInSeconds = 0;
const uint32_t DefaultProfilerSamplingRateInNanoseconds = 1000000; // TODO: Read from user input.

//! x is clamped to the range [Minimum , Maximum]
//! Returns Minimum if x is less than Minimum.
//! Returns Maximum if x is greater than Maximum.
//! Returns x otherwise.
template <typename T>
const typename std::enable_if<std::is_integral<T>::value, T>::type Clamp(const T x, const T Minimum, const T Maximum)
{
    return (x < Minimum) ? Minimum : (Maximum < x) ? Maximum : x;
}

template <class InputIt, class T>
InputIt Find(InputIt first, InputIt last, const T value)
{
    for (; first != last; ++first)
        if (*first == value)
            return first;
    return last;
}

const char *ReadEventPipeConfig(const char *first, const char *last, SString &key, SString &value)
{
    if (first == last)
        return first; // Empty string.

    // Key
    const char *begin = first;
    const char *end = Find(first, last, '=');

    auto count = static_cast<COUNT_T>(end - begin);
    key.SetANSI(begin, count);

    if (end == last)
        return last; // Reached EOL

    first = end;
    ++first;
    if (first == last)
        return last; // Nothing else to parse after =

    // Value
    begin = first;
    end = Find(first, last, '\n');

    count = static_cast<COUNT_T>(end - begin);
    value.SetANSI(begin, count);
    if (value.GetCount() > 0 && value[value.GetCount() - 1] == '\r')
        value.Truncate(value.End() - 1); // Trim carriage return from end-of-string

    if (end == last)
        return last; // Reached EOL

    first = end;
    ++first;
    return first;
}

inline void AnsiToUnicodeSString(const char *first, const char *last, SString &outputString)
{
    const int64_t count = last - first;

    _ASSERTE(count >= 0);
    if (count <= 0)
        return;

    // TODO: Error handling checking. Overflow?

    SString tmpProviderName;
    tmpProviderName.SetANSI(first, static_cast<COUNT_T>(count));
    tmpProviderName.ConvertToUnicode(outputString);
}

inline void AnsiToInt32(const char *first, const char *last, uint32_t &integer)
{
    const int64_t count = last - first;

    _ASSERTE(count >= 0);
    if (count <= 0)
        return;

    StackScratchBuffer scratchBuffer;
    SString value;
    value.SetANSI(first, static_cast<COUNT_T>(count));
    integer = static_cast<uint32_t>(strtoul(value.GetANSI(scratchBuffer), nullptr, 0));
}

inline void AnsiToInt64(const char *first, const char *last, uint64_t &integer)
{
    const int64_t count = last - first;

    _ASSERTE(count >= 0);
    if (count <= 0)
        return;

    // StackScratchBuffer scratchBuffer;
    SString value;
    value.SetANSI(first, static_cast<COUNT_T>(count));
    integer = static_cast<uint64_t>(/*strtoull*/_wcstoui64(value.GetUnicode(), nullptr, 0));
}

//! Parses a string (Provider) in the following format:
//!     Provider:       "(GUID|KnownProviderName)[:Flags[:Level][:KeyValueArgs]]"
//!     KeyValueArgs:   "[key1=value1][;key2=value2]"
void ProviderParser(const char *first, const char *last, EventPipeSessionProviderList &providers)
{
    const uint64_t DefaultKeywords = UINT64_MAX;
    const uint32_t DefaultLevel = static_cast<uint32_t>(EventPipeEventLevel::Verbose);

    // Split on:
    //  Provider:Flags:Level
    //      - or -
    //  Provider:Flags:Level:KeyValueArgs
    SString providerName;
    uint64_t keywords = DefaultKeywords;
    uint32_t loggingLevel = DefaultLevel;
    SString filterData;
    const char *iter;

    iter = Find(first, last, ':'); // Provider name.
    if (first == iter)
        return; // Ignore undefined provider.
    AnsiToUnicodeSString(first, iter, providerName);

    // Keyword.
    if (iter != last)
    {
        first = ++iter;
        iter = Find(first, last, ':');
        AnsiToInt64(first, iter, keywords);

        // LoggingLevel.
        if (iter != last)
        {
            first = ++iter;
            iter = Find(first, last, ':');
            AnsiToInt32(first, iter, loggingLevel);
            loggingLevel = Clamp(loggingLevel,
                                 static_cast<uint32_t>(EventPipeEventLevel::LogAlways),
                                 static_cast<uint32_t>(EventPipeEventLevel::Verbose));

            // FilterData
            if (iter != last)
            {
                first = ++iter;
                AnsiToUnicodeSString(first, last, filterData);
            }
        }
    }

    // TODO: Move to a different function?
    if (wcslen(providerName.GetUnicode()) > 0)
    {
        NewHolder<EventPipeSessionProvider> hEventPipeSessionProvider = new (nothrow) EventPipeSessionProvider(
            providerName.GetUnicode(), // TODO: Make sure we do not end up with a dangling reference.
            keywords,
            static_cast<EventPipeEventLevel>(loggingLevel),
            filterData.GetCount() > 0 ? filterData.GetUnicode() : nullptr);
        if (hEventPipeSessionProvider.IsNull())
            return;
        providers.AddSessionProvider(hEventPipeSessionProvider.Extract());
    }
}

//! Parses a string (list of providers) in the following format: "Provider[,Provider]"
void ProvidersParser(const char *first, const char *last, EventPipeSessionProviderList &providers)
{
    for (; first != last; ++first)
    {
        const char *begin = first;
        const char *end = Find(first, last, ',');
        ProviderParser(begin, end, providers);
        if (end == last)
            break;
        first = end;
    }
}

void ConfigurationParser(
    const char *first,
    const char *last,
    SString &strOutputPath,
    uint32_t &circularBufferSizeInMB,
    EventPipeSessionProviderList &providers,
    uint64_t &multiFileTraceLengthInSeconds)
{
    // TODO: Are the defaults below correct?
    circularBufferSizeInMB = DefaultCircularBufferMB;

    while (first != last)
    {
        SString key;
        SString value;

        first = ReadEventPipeConfig(first, last, key, value);

        if (key.GetCount() > 0 && value.GetCount() > 0)
        {
            // TODO: Maybe trim white spaces, for an user friendlier experience?

            StackScratchBuffer scratchBuffer; // Is there a better way of calling SString::GetANSI?
            if (key.CompareCaseInsensitive(SString(W("Providers"))) == 0)
            {
                // TODO: Parse into an array of providers?
                ProvidersParser(
                    reinterpret_cast<const char *>(value.GetANSI(scratchBuffer)),
                    reinterpret_cast<const char *>(value.GetANSI(scratchBuffer)) + value.GetCount(),
                    providers);
            }
            else if (key.CompareCaseInsensitive(SString(W("CircularMB"))) == 0)
            {
                circularBufferSizeInMB = static_cast<uint32_t>(
                    strtoul(value.GetANSI(scratchBuffer), nullptr, 0));
            }
            else if (key.CompareCaseInsensitive(SString(W("OutputPath"))) == 0)
            {
                // TODO: Generate output file name (This is just the "Directory").
                //  Expected file name should be: "<AppName>.<Pid>.netperf"
                // TODO: Currently user needs to pass full file name.
                strOutputPath = value;
            }
            else if (key.CompareCaseInsensitive(SString(W("ProcessID"))) == 0)
            {
                // TODO: Add error handling (overflow?).
                const uint32_t processId = static_cast<uint32_t>(
                    strtoul(value.GetANSI(scratchBuffer), nullptr, 0));
                const DWORD pid = ProcessDescriptor::FromCurrentProcess().m_Pid;

                // TODO: If set, bail out early if the specified process does not match the current process.
                //  Do we need this anymore?
            }
            else if (key.CompareCaseInsensitive(SString(W("MultiFileSec"))) == 0)
            {
                // TODO: Add error handling (overflow?).
                multiFileTraceLengthInSeconds = static_cast<uint64_t>(
                    /*strtoull*/_wcstoui64(value.GetUnicode(), nullptr, 0));
            }
        }
    }

    // TODO: Clamp values where applicable?
}

void EventPipe::EnableFileTracingEventHandler(IpcStream *pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    // TODO: Read within a loop.
    const uint32_t BufferSize = 8192;
    char buffer[BufferSize]{};
    uint32_t nNumberOfBytesRead = 0;
    bool fSuccess = pStream->Read(buffer, sizeof(buffer), nNumberOfBytesRead);
    if (!fSuccess)
    {
        // TODO: Add error handling.
        delete pStream;
        return;
    }

    SString strOutputPath;
    uint32_t circularBufferSizeInMB = DefaultCircularBufferMB;
    uint64_t multiFileTraceLengthInSeconds = DefaultMultiFileTraceLengthInSeconds;
    EventPipeSessionProviderList providers(nullptr, 0);

    ConfigurationParser(
        buffer,
        buffer + nNumberOfBytesRead,
        strOutputPath,
        circularBufferSizeInMB,
        providers,
        multiFileTraceLengthInSeconds);

    EventPipeSessionID sessionId = (EventPipeSessionID) nullptr;
    if (!providers.IsEmpty())
    {
        LPCWSTR pStrOutputPath = wcslen(strOutputPath.GetUnicode()) > 0 ?
            strOutputPath.GetUnicode() : nullptr;
        sessionId = EventPipe::Enable(
            pStrOutputPath,                           // outputFile
            circularBufferSizeInMB,                   // circularBufferSizeInMB
            DefaultProfilerSamplingRateInNanoseconds, // ProfilerSamplingRateInNanoseconds
            providers,                                // pProviders
            multiFileTraceLengthInSeconds);           // multiFileTraceLengthInSeconds
    }

    uint32_t nBytesWritten = 0;
    fSuccess = pStream->Write(&sessionId, sizeof(sessionId), nBytesWritten);
    if (!fSuccess)
    {
        // TODO: Add error handling.
        delete pStream;
        return;
    }

    fSuccess = pStream->Flush();
    if (!fSuccess)
    {
        // TODO: Add error handling.
    }
    delete pStream;
}

void EventPipe::DisableTracingEventHandler(IpcStream *pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    uint32_t nNumberOfBytesRead = 0;
    EventPipeSessionID sessionId = (EventPipeSessionID) nullptr;
    const bool fSuccess = pStream->Read(&sessionId, sizeof(sessionId), nNumberOfBytesRead);
    if (!fSuccess || nNumberOfBytesRead != sizeof(sessionId))
    {
        // TODO: Add error handling.
        delete pStream;
        return;
    }

    EventPipe::Disable(sessionId);
    // TODO: Should we acknowledge back?
    delete pStream;
}

#endif // FEATURE_PERFTRACING
