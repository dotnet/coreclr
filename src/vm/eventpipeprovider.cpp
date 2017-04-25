// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "eventpipeevent.h"
#include "eventpipeprovider.h"

EventPipeProvider::EventPipeProvider(const GUID &providerID)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_providerID = providerID;
    m_enabled = false;
    m_keywords = 0;
    m_providerLevel = EventPipeEventLevel::Critical;
    m_pEventList = new SList<SListElem<EventPipeEvent*>>();
    m_pCallbackFunction = NULL;
    m_pCallbackData = NULL;

    // Register the provider.
    EventPipeConfiguration* pConfig = EventPipe::GetConfiguration();
    _ASSERTE(pConfig != NULL);
    pConfig->RegisterProvider(*this);
}

const GUID& EventPipeProvider::GetProviderID() const
{
    LIMITED_METHOD_CONTRACT;

    return m_providerID;
}

bool EventPipeProvider::Enabled() const
{
    LIMITED_METHOD_CONTRACT;

    return m_enabled;
}

bool EventPipeProvider::EventEnabled(INT64 keywords) const
{
    LIMITED_METHOD_CONTRACT;

    // The event is enabled if:
    //  - The provider is enabled.
    //  - The event keywords are unspecified in the manifest (== 0) or when masked with the enabled config are != 0.
    return (Enabled() && ((keywords == 0) || ((m_keywords & keywords) != 0)));
}

bool EventPipeProvider::EventEnabled(INT64 keywords, EventPipeEventLevel eventLevel) const
{
    LIMITED_METHOD_CONTRACT;

    // The event is enabled if:
    //  - The provider is enabled.
    //  - The event keywords are unspecified in the manifest (== 0) or when masked with the enabled config are != 0.
    //  - The event level is LogAlways or the provider's verbosity level is set to greater than the event's verbosity level in the manifest.
    return (EventEnabled(keywords) &&
        ((eventLevel == EventPipeEventLevel::LogAlways) || (m_providerLevel >= eventLevel)));
}

void EventPipeProvider::SetConfiguration(bool providerEnabled, INT64 keywords, EventPipeEventLevel providerLevel)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    m_enabled = providerEnabled;
    m_keywords = keywords;
    m_providerLevel = providerLevel;

    RefreshAllEvents();
    InvokeCallback();
}

void EventPipeProvider::AddEvent(EventPipeEvent &event)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the config lock before inserting a new event.
    CrstHolder _crst(EventPipe::GetLock());

    m_pEventList->InsertTail(new SListElem<EventPipeEvent*>(&event));
}

void EventPipeProvider::RegisterCallback(EventPipeCallback pCallbackFunction, void *pData)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the config lock before setting the callback.
    CrstHolder _crst(EventPipe::GetLock());

    if(m_pCallbackFunction == NULL)
    {
        m_pCallbackFunction = pCallbackFunction;
        m_pCallbackData = pData;
    }
}

void EventPipeProvider::UnregisterCallback(EventPipeCallback pCallbackFunction)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the config lock before setting the callback.
    CrstHolder _crst(EventPipe::GetLock());

    if(m_pCallbackFunction == pCallbackFunction)
    {
        m_pCallbackFunction = NULL;
        m_pCallbackData = NULL;
    }
}

void EventPipeProvider::InvokeCallback()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    if(m_pCallbackFunction != NULL)
    {
        (*m_pCallbackFunction)(
            &m_providerID,
            m_enabled,
            (UCHAR) m_providerLevel,
            m_keywords,
            0 /* matchAllKeywords */,
            NULL /* FilterData */,
            m_pCallbackData /* CallbackContext */);
    }
}

void EventPipeProvider::RefreshAllEvents()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    SListElem<EventPipeEvent*> *pElem = m_pEventList->GetHead();
    while(pElem != NULL)
    {
        EventPipeEvent *pEvent = pElem->GetValue();
        pEvent->RefreshState();

        pElem = m_pEventList->GetNext(pElem);
    }
}
