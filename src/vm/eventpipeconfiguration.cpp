// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "eventpipeprovider.h"

EventPipeConfiguration::EventPipeConfiguration()
{
    STANDARD_VM_CONTRACT;

    m_pProviderList = new SList<SListElem<EventPipeProvider*>>();
}

EventPipeConfiguration::~EventPipeConfiguration()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    if(m_pProviderList != NULL)
    {
        delete(m_pProviderList);
        m_pProviderList = NULL;
    }
}

bool EventPipeConfiguration::RegisterProvider(EventPipeProvider &provider)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the lock before manipulating the provider list.
    CrstHolder _crst(EventPipe::GetLock());

    // See if we've already registered this provider.
    EventPipeProvider *pExistingProvider = GetProviderNoLock(provider.GetProviderID());
    if(pExistingProvider != NULL)
    {
        return false;
    }

    // The provider has not been registered, so register it.
    m_pProviderList->InsertTail(new SListElem<EventPipeProvider*>(&provider));

    // TODO: Set the provider configuration and enable it if we know
    // anything about the provider before it is registered.

    return true;
}

EventPipeProvider* EventPipeConfiguration::GetProvider(const GUID &providerID)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Take the lock before touching the provider list to ensure no one tries to
    // modify the list.
    CrstHolder _crst(EventPipe::GetLock());

    return GetProviderNoLock(providerID);
}

EventPipeProvider* EventPipeConfiguration::GetProviderNoLock(const GUID &providerID)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    SListElem<EventPipeProvider*> *pElem = m_pProviderList->GetHead();
    while(pElem != NULL)
    {
        EventPipeProvider *pProvider = pElem->GetValue();
        if(pProvider->GetProviderID() == providerID)
        {
            return pProvider;
        }

        pElem = m_pProviderList->GetNext(pElem);
    }

    return NULL;
}

void EventPipeConfiguration::Enable()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        // Lock must be held by EventPipe::Enable.
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    SListElem<EventPipeProvider*> *pElem = m_pProviderList->GetHead();
    while(pElem != NULL)
    {
        // TODO: Only enable the providers that have been explicitly enabled.
        EventPipeProvider *pProvider = pElem->GetValue();
        pProvider->SetConfiguration(true /* providerEnabled */, 0 /* keywords */, EventPipeEventLevel::Critical /* level */);

        pElem = m_pProviderList->GetNext(pElem);
    }

}

void EventPipeConfiguration::Disable()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        // Lock must be held by EventPipe::Disable.
        PRECONDITION(EventPipe::GetLock()->OwnedByCurrentThread());
    }
    CONTRACTL_END;

    SListElem<EventPipeProvider*> *pElem = m_pProviderList->GetHead();
    while(pElem != NULL)
    {
        EventPipeProvider *pProvider = pElem->GetValue();
        pProvider->SetConfiguration(false /* providerEnabled */, 0 /* keywords */, EventPipeEventLevel::Critical /* level */);

        pElem = m_pProviderList->GetNext(pElem);
    }
}
