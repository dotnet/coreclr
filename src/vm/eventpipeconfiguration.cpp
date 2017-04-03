// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeconfiguration.h"

EventPipeConfiguration::ProviderConfiguration EventPipeConfiguration::s_runtimeProviderConfig = {RuntimeProviderID,0};
EventPipeConfiguration::ProviderConfiguration EventPipeConfiguration::s_privateProviderConfig = {PrivateProviderID,0};
EventPipeConfiguration::ProviderConfiguration EventPipeConfiguration::s_rundownProviderConfig = {RundownProviderID,0};
EventPipeConfiguration::ProviderConfiguration EventPipeConfiguration::s_stressProviderConfig = {StressProviderID,0};

EventPipeConfiguration::EventPipeConfiguration()
{
    LIMITED_METHOD_CONTRACT;
}

bool EventPipeConfiguration::SetConfiguration(const GUID &providerID, INT64 keywords)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeConfiguration::ProviderConfiguration *pConfig = GetProviderConfig(providerID);
    if(pConfig == NULL)
    {
        return false;
    }

    return pConfig->SetKeywords(keywords);
}

bool EventPipeConfiguration::DisableAllProviders()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    return (SetConfiguration(RuntimeProviderID, 0) &&
        SetConfiguration(PrivateProviderID, 0) &&
        SetConfiguration(RundownProviderID, 0) &&
        SetConfiguration(StressProviderID, 0));
}

bool EventPipeConfiguration::ProviderEnabled(const GUID &providerID)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeConfiguration::ProviderConfiguration *pConfig = NULL;
    pConfig = GetProviderConfig(providerID);
    if(pConfig != NULL)
    {
        return (pConfig->GetEnabledKeywords() != 0);
    }

    return false;
}

bool EventPipeConfiguration::KeywordEnabled(const GUID &providerID, INT64 keyword)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeConfiguration::ProviderConfiguration *pConfig = NULL;
    pConfig = GetProviderConfig(providerID);
    if(pConfig != NULL)
    {
        return (pConfig->GetEnabledKeywords() & keyword);
    }

    return false;
}

EventPipeConfiguration::ProviderConfiguration* EventPipeConfiguration::GetProviderConfig(const GUID &providerID)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    EventPipeConfiguration::ProviderConfiguration *pConfig = NULL;

    if(IsEqualGUID(providerID, RuntimeProviderID))
    {
        pConfig = &s_runtimeProviderConfig;
    }
    else if(IsEqualGUID(providerID, PrivateProviderID))
    {
        pConfig = &s_privateProviderConfig;
    }
    else if(IsEqualGUID(providerID, RundownProviderID))
    {
        pConfig = &s_rundownProviderConfig;
    }
    else if(IsEqualGUID(providerID, StressProviderID))
    {
        pConfig = &s_stressProviderConfig;
    }
    else
    {
        _ASSERT(!"Invalid provider ID specified.");
    }

    return pConfig;
}
