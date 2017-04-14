// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "eventpipeevent.h"
#include "eventpipeprovider.h"
#include "eventpipejsonfile.h"
#include "sampleprofiler.h"

#ifdef FEATURE_PAL
#include "pal.h"
#endif // FEATURE_PAL

CrstStatic EventPipe::s_configCrst;
bool EventPipe::s_tracingInitialized = false;
bool EventPipe::s_tracingEnabled = false;
EventPipeConfiguration* EventPipe::s_pConfig = NULL;
EventPipeJsonFile* EventPipe::s_pJsonFile = NULL;

void EventPipe::Initialize()
{
    STANDARD_VM_CONTRACT;

    s_tracingInitialized = s_configCrst.InitNoThrow(
        CrstEventPipe,
        (CrstFlags)(CRST_TAKEN_DURING_SHUTDOWN));

    s_pConfig = new EventPipeConfiguration();
}

void EventPipe::EnableOnStartup()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Test COMPLUS variable to enable tracing at start-up.
    if(CLRConfig::GetConfigValue(CLRConfig::INTERNAL_PerformanceTracing) != 0)
    {
        Enable();
    }
}

void EventPipe::Shutdown()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    Disable();
}

void EventPipe::Enable()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    if(!s_tracingInitialized)
    {
        return;
    }

    // Take the lock before enabling tracing.
    CrstHolder _crst(GetLock());

    // Set the bit that actually enables tracing.
    s_tracingEnabled = true;
    if(CLRConfig::GetConfigValue(CLRConfig::INTERNAL_PerformanceTracing) == 2)
    {
        // File placed in current working directory.
        SString outputFilePath;
        outputFilePath.Printf("Process-%d.PerfView.json", GetCurrentProcessId());
        s_pJsonFile = new EventPipeJsonFile(outputFilePath);
    }

    // Enable the sample profiler
    SampleProfiler::Enable();

    // TODO: Iterate through the set of providers, enable them as appropriate.
    // This in-turn will iterate through all of the events and set their isEnabled bits.
}

void EventPipe::Disable()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the lock before disabling tracing.
    CrstHolder _crst(GetLock());

    // Actually disable tracing.
    s_tracingEnabled = false;
    SampleProfiler::Disable();
    s_pConfig->Disable();

    // TODO: Fix race conditions.  It's possible that these resources get deleted
    // while other threads are attempting to use them.
    if(s_pJsonFile != NULL)
    {
        delete(s_pJsonFile);
        s_pJsonFile = NULL;
    }

    if(s_pConfig != NULL)
    {
        delete(s_pConfig);
        s_pConfig = NULL;
    }
}

void EventPipe::WriteEvent(EventPipeEvent &event, BYTE *pData, size_t length)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Exit early if the event is not enabled.
    if(!event.IsEnabled())
    {
        return;
    }

    // Walk the stack if requested.
    StackContents stackContents;
    bool stackWalkSucceeded = false;

    if(event.NeedStack())
    {
        stackWalkSucceeded = WalkManagedStackForCurrentThread(stackContents);
    }

    EX_TRY
    {
        if(s_pJsonFile != NULL)
        {
            Thread *pThread = GetThread();

            CommonEventFields eventFields;
            PopulateCommonEventFields(eventFields, pThread);

            const unsigned int guidSize = 39;
            WCHAR wszProviderID[guidSize];
            if(!StringFromGUID2(event.GetProvider()->GetProviderID(), wszProviderID, guidSize))
            {
                wszProviderID[0] = '\0';
            }
            SString message;
            message.Printf("Provider=%S/EventID=%d/Version=%d", wszProviderID, event.GetEventID(), event.GetEventVersion());
            s_pJsonFile->WriteEvent(eventFields, message, stackContents);
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}

void EventPipe::WriteSampleProfileEvent(Thread *pThread, StackContents &stackContents)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

    EX_TRY
    {
        if(s_pJsonFile != NULL)
        {
            CommonEventFields eventFields;
            QueryPerformanceCounter(&eventFields.TimeStamp);
            eventFields.ThreadID = pThread->GetOSThreadId();

            static SString message(W("THREAD_TIME"));
            s_pJsonFile->WriteEvent(eventFields, message, stackContents);
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
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
    _ASSERTE(pThread != NULL);
    return WalkManagedStackForThread(pThread, stackContents);
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

    stackContents.Reset();

    StackWalkAction swaRet = pThread->StackWalkFrames(
        (PSTACKWALKFRAMESCALLBACK) &StackWalkCallback,
        &stackContents,
        ALLOW_ASYNC_STACK_WALK | FUNCTIONSONLY | HANDLESKIPPEDFRAMES);

    return ((swaRet == SWA_DONE) || (swaRet == SWA_CONTINUE));
}

StackWalkAction EventPipe::StackWalkCallback(CrawlFrame *pCf, StackContents *pData)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_PREEMPTIVE;
        PRECONDITION(pCf != NULL);
        PRECONDITION(pData != NULL);
    }
    CONTRACTL_END;

    // Get the IP.
    UINT_PTR controlPC = (UINT_PTR)pCf->GetRegisterSet()->ControlPC;
    if(controlPC == 0)
    {
        if(pData->GetLength() == 0)
        {
            // This happens for pinvoke stubs on the top of the stack.
            return SWA_CONTINUE;
        }
    }

    _ASSERTE(controlPC != 0);

    // Add the IP to the captured stack.
    pData->Append(
        controlPC,
        pCf->GetFunction()
        );

    // Continue the stack walk.
    return SWA_CONTINUE;
}

EventPipeConfiguration* EventPipe::GetConfiguration()
{
    LIMITED_METHOD_CONTRACT;

    return s_pConfig;
}

void EventPipe::PopulateCommonEventFields(CommonEventFields &commonEventFields, Thread * pThread)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

    QueryPerformanceCounter(&commonEventFields.TimeStamp);
    commonEventFields.ThreadID = pThread->GetOSThreadId();
}

CrstStatic* EventPipe::GetLock()
{
    LIMITED_METHOD_CONTRACT;

    return &s_configCrst;
}
