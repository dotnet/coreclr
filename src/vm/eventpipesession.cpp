// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipe.h"
#include "eventpipebuffermanager.h"
#include "eventpipeprovider.h"
#include "eventpipesession.h"
#include "eventpipesessionprovider.h"

#ifdef FEATURE_PERFTRACING

EventPipeSession::EventPipeSession(
    EventPipeSessionType sessionType,
    unsigned int circularBufferSizeInMB,
    const EventPipeProviderConfiguration *pProviders,
    uint32_t numProviders,
    bool rundownEnabled) : m_pProviderList(new EventPipeSessionProviderList(pProviders, numProviders)),
                           m_CircularBufferSizeInBytes(circularBufferSizeInMB * 1024 * 1024),
                           m_pBufferManager(new EventPipeBufferManager()),
                           m_rundownEnabled(rundownEnabled),
                           m_SessionType(sessionType)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(circularBufferSizeInMB > 0);
        PRECONDITION(numProviders > 0 && pProviders != nullptr);
    }
    CONTRACTL_END;

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

    delete m_pProviderList;

    // FIXME: Verify DeAllocateBuffers is called when destructing the buffer manager.
    m_pBufferManager->DeAllocateBuffers();
    delete m_pBufferManager;
}

bool EventPipeSession::IsValid() const
{
    LIMITED_METHOD_CONTRACT;
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

    return m_pProviderList->GetSessionProvider(pProvider);
}

void EventPipeSession::WriteAllBuffersToFile(
    EventPipeFile &fastSerializableObject,
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

    // Force all in-progress writes to either finish or cancel
    // This is required to ensure we can safely flush and delete the buffers
    m_pBufferManager->SuspendWriteEvent();
    {
        m_pBufferManager->WriteAllBuffersToFile(&fastSerializableObject, configuration, stopTimeStamp);
    }
    // Allow WriteEvent to begin accepting work again so that sometime in the future
    // we can re-enable events and they will be recorded
    // FIXME: Functions above might throw... Should we have a try catch here?
    m_pBufferManager->ResumeWriteEvent();
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
    return m_pBufferManager->WriteEvent(pThread, *this, event, payload, pActivityId, pRelatedActivityId);
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

    return m_pBufferManager->GetNextEvent();
}

#ifdef DEBUG
    bool EventPipeSession::IsLockOwnedByCurrentThread()
    {
        return m_pBufferManager->IsLockOwnedByCurrentThread();
    }
#endif // DEBUG

// void EventPipeSession::Enable(EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue)
// {
//     CONTRACTL
//     {
//         THROWS;
//         GC_TRIGGERS;
//         MODE_ANY;
//         // Lock must be held by EventPipe::Enable.
//         PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
//     }
//     CONTRACTL_END;

//     m_enabled = true;

//     // The provider list should be non-NULL, but can be NULL on shutdown.
//     if (m_pProviderList != NULL)
//     {
//         SListElem<EventPipeProvider *> *pElem = m_pProviderList->GetHead();
//         while (pElem != NULL)
//         {
//             EventPipeProvider *pProvider = pElem->GetValue();

//             // Enable the provider if it has been configured.
//             EventPipeSessionProvider *pSessionProvider = GetSessionProvider(pProvider);
//             if (pSessionProvider != NULL)
//             {
//                 EventPipeProviderCallbackData eventPipeProviderCallbackData =
//                     pProvider->SetConfiguration(
//                         true /* providerEnabled */,
//                         pSessionProvider->GetKeywords(),
//                         pSessionProvider->GetLevel(),
//                         pSessionProvider->GetFilterData());
//                 pEventPipeProviderCallbackDataQueue->Enqueue(&eventPipeProviderCallbackData);
//             }

//             pElem = m_pProviderList->GetNext(pElem);
//         }
//     }
// }

// void EventPipeSession::Disable(EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue)
// {
//     CONTRACTL
//     {
//         THROWS;
//         GC_TRIGGERS;
//         MODE_ANY;
//         // TODO: Multiple session support will require that the session be specified.
//         PRECONDITION(pSession != NULL);
//         // Lock must be held by EventPipe::Disable.
//         PRECONDITION(EventPipe::IsLockOwnedByCurrentThread());
//     }
//     CONTRACTL_END;

//     // The provider list should be non-NULL, but can be NULL on shutdown.
//     if (m_pProviderList != NULL)
//     {
//         SListElem<EventPipeProvider *> *pElem = m_pProviderList->GetHead();
//         while (pElem != NULL)
//         {
//             EventPipeProvider *pProvider = pElem->GetValue();
//             EventPipeProviderCallbackData eventPipeProviderCallbackData = pProvider->SetConfiguration(
//                 false /* providerEnabled */,
//                 0 /* keywords */,
//                 EventPipeEventLevel::Critical /* level */,
//                 NULL /* filterData */);
//             pEventPipeProviderCallbackDataQueue->Enqueue(&eventPipeProviderCallbackData);

//             pElem = m_pProviderList->GetNext(pElem);
//         }
//     }

//     m_enabled = false;
//     m_rundownEnabled = false;
//     m_pSession = NULL;
// }

#endif // FEATURE_PERFTRACING
