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
    LIMITED_METHOD_CONTRACT;

    m_providerID = providerID;
    m_enabled = false;
    m_keywords = 0;
    m_providerLevel = Critical;
    m_pEventList = new SList<SListElem<EventPipeEvent*>>();
    m_lock.Init(LOCK_TYPE_DEFAULT);

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

    return (Enabled() && ((m_keywords & keywords) != 0));
}

bool EventPipeProvider::EventEnabled(INT64 keywords, EventPipeEventLevel eventLevel) const
{
    LIMITED_METHOD_CONTRACT;

    return (EventEnabled(keywords) &&
        ((eventLevel == LogAlways) || (m_providerLevel >= eventLevel)));
}

void EventPipeProvider::SetConfiguration(INT64 keywords, EventPipeEventLevel providerLevel)
{
    LIMITED_METHOD_CONTRACT;

    m_keywords = keywords;
    m_providerLevel = providerLevel;

    RefreshAllEvents();
}

void EventPipeProvider::AddEvent(EventPipeEvent &event)
{
    LIMITED_METHOD_CONTRACT;

    SpinLockHolder _slh(&m_lock);
    m_pEventList->InsertTail(new SListElem<EventPipeEvent*>(&event));
}

void EventPipeProvider::RefreshAllEvents()
{
    LIMITED_METHOD_CONTRACT;

    SpinLockHolder _slh(&m_lock);

    SListElem<EventPipeEvent*> *pElem = m_pEventList->GetHead();
    while(pElem != NULL)
    {
        EventPipeEvent *pEvent = pElem->GetValue();
        pEvent->RefreshState();

        pElem = m_pEventList->GetNext(pElem);
    }
}
