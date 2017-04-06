// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeconfiguration.h"
#include "eventpipeprovider.h"

EventPipeConfiguration::EventPipeConfiguration()
{
    LIMITED_METHOD_CONTRACT;

    m_nextProviderIndex = 0;
}

void EventPipeConfiguration::DisableAllProviders()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // TODO
}

EventPipeProvider* EventPipeConfiguration::GetOrCreateProvider(const GUID &providerID)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeProvider * pProvider = NULL;

    // Attempt to find the provider.
    for(int i=0; i<m_nextProviderIndex; i++)
    {
        _ASSERTE(m_providers[i] != NULL);
        if(providerID == m_providers[i]->GetProviderID())
        {
            pProvider = m_providers[i];
            break;
        }
    }

    // If we did not find the provider create it.
    if(pProvider == NULL)
    {
        pProvider = new EventPipeProvider(providerID);
        m_providers[m_nextProviderIndex++] = pProvider;
    }

    return pProvider;
}
