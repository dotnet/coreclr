// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeeventinstance.h"
#include "eventpipejsonfile.h"
#include "fastserializer.h"
#include "sampleprofiler.h"

EventPipeEventInstance::EventPipeEventInstance(
    EventPipeEvent &event,
    DWORD threadID,
    BYTE *pData,
    size_t length)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pEvent = &event;
    m_threadID = threadID;
    m_pData = pData;
    m_dataLength = length;
    QueryPerformanceCounter(&m_timeStamp);

    if(event.NeedStack())
    {
        EventPipe::WalkManagedStackForCurrentThread(m_stackContents);
    }
}

StackContents* EventPipeEventInstance::GetStack()
{
    LIMITED_METHOD_CONTRACT;

    return &m_stackContents;
}

void EventPipeEventInstance::Serialize(FastSerializer *pSerializer, EventPipeEventInstance *pInstance)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pSerializer != NULL);
        PRECONDITION(pInstance != NULL);
    }
    CONTRACTL_END;

    pInstance->Serialize(pSerializer);
}

void EventPipeEventInstance::Serialize(FastSerializer *pSerializer)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

    // TODO: Serialize the event using the serializer.
}

void EventPipeEventInstance::SerializeToJsonFile(EventPipeJsonFile *pFile)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if(pFile == NULL)
    {
        return;
    }

    EX_TRY
    {
        const unsigned int guidSize = 39;
        WCHAR wszProviderID[guidSize];
        if(!StringFromGUID2(m_pEvent->GetProvider()->GetProviderID(), wszProviderID, guidSize))
        {
            wszProviderID[0] = '\0';
        }
        memmove(wszProviderID, &wszProviderID[1], guidSize-3);
        wszProviderID[guidSize-3] = '\0';
        SString message;
        message.Printf("Provider=%S/EventID=%d/Version=%d", wszProviderID, m_pEvent->GetEventID(), m_pEvent->GetEventVersion());
        pFile->WriteEvent(m_timeStamp, m_threadID, message, m_stackContents);
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}

SampleProfilerEventInstance::SampleProfilerEventInstance(Thread *pThread)
    :EventPipeEventInstance(*SampleProfiler::s_pThreadTimeEvent, pThread->GetOSThreadId(), NULL, 0)
{
    LIMITED_METHOD_CONTRACT;
}
