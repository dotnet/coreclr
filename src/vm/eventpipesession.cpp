// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipe.h"
#include "eventpipebuffermanager.h"
#include "eventpipefile.h"
#include "eventpipeprovider.h"
#include "eventpipesession.h"
#include "eventpipesessionprovider.h"

#ifdef FEATURE_PERFTRACING

//! The keywords below seems to correspond to:
//!  LoaderKeyword                      (0x00000008)
//!  JitKeyword                         (0x00000010)
//!  NgenKeyword                        (0x00000020)
//!  unused_keyword                     (0x00000100)
//!  JittedMethodILToNativeMapKeyword   (0x00020000)
//!  ThreadTransferKeyword              (0x80000000)
const EventPipeProviderConfiguration RundownProviders[] = {
    {W("Microsoft-Windows-DotNETRuntime"), 0x80020138, static_cast<unsigned int>(EventPipeEventLevel::Verbose), NULL},       // Public provider.
    {W("Microsoft-Windows-DotNETRuntimeRundown"), 0x80020138, static_cast<unsigned int>(EventPipeEventLevel::Verbose), NULL} // Rundown provider.
};
const uint32_t RundownProvidersSize = sizeof(RundownProviders) / sizeof(EventPipeProviderConfiguration);

EventPipeSession::EventPipeSession(
    LPCWSTR strOutputPath,
    IpcStream *const pStream,
    EventPipeSessionType sessionType,
    unsigned int circularBufferSizeInMB,
    const EventPipeProviderConfiguration *pProviders,
    uint32_t numProviders,
    bool rundownEnabled) : // m_lock(CrstEventPipe, (CrstFlags)(CRST_REENTRANCY | CRST_TAKEN_DURING_SHUTDOWN | CRST_HOST_BREAKABLE)),
                           m_pProviderList(new EventPipeSessionProviderList(pProviders, numProviders)),
                           m_CircularBufferSizeInBytes(circularBufferSizeInMB * 1024 * 1024),
                           m_pBufferManager(new EventPipeBufferManager()),
                           m_rundownEnabled(rundownEnabled),
                           m_SessionType(sessionType)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(circularBufferSizeInMB > 0);
        PRECONDITION(numProviders > 0 && pProviders != nullptr);
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    // Create the event pipe file.
    // A NULL output path means that we should not write the results to a file.
    // This is used in the EventListener case.
    m_pFile = nullptr;
    switch (sessionType)
    {
        case EventPipeSessionType::File:
            if (strOutputPath != nullptr)
                m_pFile = new EventPipeFile(new FileStreamWriter(SString(strOutputPath)));
            break;

        case EventPipeSessionType::IpcStream:
            m_pFile = new EventPipeFile(new IpcStreamWriter(reinterpret_cast<EventPipeSessionID>(this), pStream));
            break;

        default:
            m_pFile = nullptr;
            break;
    }

    GetSystemTimeAsFileTime(&m_sessionStartTime);
    QueryPerformanceCounter(&m_sessionStartTimeStamp);
}

EventPipeSession::~EventPipeSession()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // TODO: Stop streaming thread? Review synchronization.
    // CrstHolder _crst(&m_lock);

    delete m_pProviderList;

    // FIXME: Verify DeAllocateBuffers is called when destructing the buffer manager.
    m_pBufferManager->DeAllocateBuffers();
    delete m_pBufferManager;
    delete m_pFile;
}

bool EventPipeSession::HasIpcStreamingStarted()
{
    // CrstHolder _crst(&m_lock);
    return m_pIpcStreamingThread != nullptr ? m_pIpcStreamingThread->HasStarted() : false;
}

void EventPipeSession::DestroyIpcStreamingThread()
{
    // CrstHolder _crst(&m_lock);

    if (!m_ipcStreamingEnabled)
        return;

    if (m_pIpcStreamingThread != nullptr)
        ::DestroyThread(m_pIpcStreamingThread);

    m_pIpcStreamingThread = nullptr;

    // Signal Disable() that the thread has been destroyed.
    m_threadShutdownEvent.Set();
}

static DWORD WINAPI ThreadProc(void *args)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(args != nullptr);
    }
    CONTRACTL_END;

    // TODO: Review Andrew's update/PR on SamplerProfiler

    if (args == nullptr)
        return 1;

    EventPipeSession *const pEventPipeSession = reinterpret_cast<EventPipeSession *>(args);
    if (pEventPipeSession->GetSessionType() != EventPipeSessionType::IpcStream)
        return 1;

    EX_TRY
    {
        if (pEventPipeSession->HasIpcStreamingStarted())
        {
            bool fSuccess = true;
            while (pEventPipeSession->IsIpcStreamingEnabled() && fSuccess)
            {
                {
                    // Switch to pre-emptive mode so that this thread doesn't starve the GC
                    GCX_PREEMP();
                    EventPipe::RunWithCallbackPostponed([&](EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue) {
                        // Make sure that we should actually flush.
                        EventPipeConfiguration *const pConfiguration = EventPipe::GetConfiguration();
                        if (!EventPipe::Enabled() || pConfiguration == nullptr)
                            fSuccess = false;

                        // Get the current time stamp.
                        // WriteAllBuffersToFile will use this to ensure that no events after
                        // the current timestamp are written into the file.
                        LARGE_INTEGER stopTimeStamp;
                        QueryPerformanceCounter(&stopTimeStamp);
                        if (!pEventPipeSession->WriteAllBuffersToFile(*pConfiguration, stopTimeStamp))
                        {
                            fSuccess = false;
                            pEventPipeSession->Disable(
                                *pConfiguration,
                                stopTimeStamp,
                                pEventPipeProviderCallbackDataQueue);
                        }
                    });
                }

                // Wait until it's time to sample again.
                const uint32_t PeriodInNanoSeconds = 100000000; // 100 msec.
                const uint32_t NUM_NANOSECONDS_IN_1_MS = 1000000;

                // FIXME: Poors man sleep
#ifdef FEATURE_PAL
                PAL_nanosleep(PeriodInNanoSeconds);
#else  //FEATURE_PAL
                ClrSleepEx(PeriodInNanoSeconds / NUM_NANOSECONDS_IN_1_MS, FALSE);
#endif //FEATURE_PAL
            }
        }
    }
    EX_CATCH
    {
        // TODO: STRESS_LOG ?
    }
    EX_END_CATCH(SwallowAllExceptions);

    pEventPipeSession->DestroyIpcStreamingThread();
    return 0;
}

void EventPipeSession::CreateIpcStreamingThread()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    m_ipcStreamingEnabled = true;
    m_pIpcStreamingThread = SetupUnstartedThread();
    if (m_pIpcStreamingThread->CreateNewThread(0, ThreadProc, this))
    {
        m_pIpcStreamingThread->SetBackground(TRUE);
        m_pIpcStreamingThread->StartThread();
    }
    else
    {
        _ASSERT(!"Unable to create IPC stream flushing thread.");
    }
    m_threadShutdownEvent.CreateManualEvent(FALSE);
}

bool EventPipeSession::IsValid()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    return (m_pProviderList != nullptr) && (!m_pProviderList->IsEmpty());
}

void EventPipeSession::AddSessionProvider(EventPipeSessionProvider *pProvider)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // CrstHolder _crst(&m_lock);
    m_pProviderList->AddSessionProvider(pProvider);
}

EventPipeSessionProvider* EventPipeSession::GetSessionProvider(EventPipeProvider *pProvider)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // CrstHolder _crst(&m_lock);
    return m_pProviderList->GetSessionProvider(pProvider);
}

bool EventPipeSession::WriteAllBuffersToFile(
    EventPipeConfiguration &configuration,
    LARGE_INTEGER stopTimeStamp)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    if (m_pFile == nullptr)
        return true;

    // CrstHolder _crst(&m_lock);
    m_pBufferManager->WriteAllBuffersToFile(m_pFile, configuration, stopTimeStamp);
    return !m_pFile->HasErrors();
}

bool EventPipeSession::WriteEvent(
        Thread *pThread,
        EventPipeEvent &event,
        EventPipeEventPayload &payload,
        LPCGUID pActivityId,
        LPCGUID pRelatedActivityId,
        Thread *pEventThread,
        StackContents *pStack)
{
    // CrstHolder _crst(&m_lock);
    return m_pBufferManager->WriteEvent(pThread, *this, event, payload, pActivityId, pRelatedActivityId);
}

void EventPipeSession::WriteEvent(
    EventPipeEventInstance &instance,
    EventPipeConfiguration &configuration)
{
    if (m_pFile == nullptr)
        return;

    // CrstHolder _crst(&m_lock);
    m_pFile->WriteEvent(instance, configuration);
}

EventPipeEventInstance *EventPipeSession::GetNextEvent()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(!EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    // CrstHolder _crst(&m_lock);
    return m_pBufferManager->GetNextEvent();
}

void EventPipeSession::Enable(EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        // Lock must be held by EventPipe::Enable.
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    if (m_SessionType == EventPipeSessionType::IpcStream)
        CreateIpcStreamingThread();
}

void EventPipeSession::Disable(
    EventPipeConfiguration &configuration,
    LARGE_INTEGER stopTimeStamp,
    EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        // Lock must be held by EventPipe::Disable.
        PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
    }
    CONTRACTL_END;

    if (m_pFile == nullptr)
        return;

    // CrstHolder _crst(&m_lock);

    // Disable streaming thread
    if ((m_SessionType == EventPipeSessionType::IpcStream) && m_ipcStreamingEnabled)
    {
        // Reset the event before shutdown.
        m_threadShutdownEvent.Reset();

        // The sampling thread will watch this value and exit
        // when profiling is disabled.
        m_ipcStreamingEnabled = false;

        // Wait for the sampling thread to clean itself up.
        m_threadShutdownEvent.Wait(INFINITE, FALSE /* bAlertable */);
        m_threadShutdownEvent.CloseEvent();
    }

    // Force all in-progress writes to either finish or cancel
    // This is required to ensure we can safely flush and delete the buffers
    m_pBufferManager->SuspendWriteEvent();
    {
        m_pBufferManager->WriteAllBuffersToFile(m_pFile, configuration, stopTimeStamp);

        if (CLRConfig::GetConfigValue(CLRConfig::INTERNAL_EventPipeRundown) > 0)
        {
            // Before closing the file, do rundown. We have to re-enable event writing for this.
            m_pBufferManager->ResumeWriteEvent();

            // Update provider list with rundown configuration.
            m_pProviderList->Clear();
            for (uint32_t i = 0; i < RundownProvidersSize; ++i)
            {
                const EventPipeProviderConfiguration &Config = RundownProviders[i];
                EventPipeSessionProvider *pProvider = new EventPipeSessionProvider(
                    Config.GetProviderName(),
                    Config.GetKeywords(),
                    (EventPipeEventLevel)Config.GetLevel(),
                    Config.GetFilterData());
                m_pProviderList->AddSessionProvider(pProvider);
            }

            // Enable rundown.
            m_rundownEnabled = true;

            // Ask the runtime to emit rundown events.
            if (g_fEEStarted && !g_fEEShutDown)
                ETW::EnumerationLog::EndRundown();

            // Suspend again after rundown session
            m_pBufferManager->SuspendWriteEvent();
        }
    }
    // Allow WriteEvent to begin accepting work again so that sometime in the future
    // we can re-enable events and they will be recorded
    // FIXME: Functions above might throw... Should we have a try catch here?
    m_pBufferManager->ResumeWriteEvent();
}

#endif // FEATURE_PERFTRACING
