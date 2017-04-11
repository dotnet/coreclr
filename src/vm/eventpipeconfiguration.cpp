// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeconfiguration.h"
#include "eventpipeprovider.h"

EventPipeConfiguration::EventPipeConfiguration()
{
    LIMITED_METHOD_CONTRACT;

    m_pProviderList = new SList<SListElem<EventPipeProvider*>>();
    m_lock.Init(LOCK_TYPE_DEFAULT);
}

EventPipeConfiguration::~EventPipeConfiguration()
{
    LIMITED_METHOD_CONTRACT;

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
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    SpinLockHolder _slh(&m_lock);

    // See if we've already registered this provider.
    EventPipeProvider *pExistingProvider = GetProvider(provider.GetProviderID());
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
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    SpinLockHolder _slh(&m_lock);

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
