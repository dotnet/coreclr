// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeprovider.h"

EventPipeProvider::EventPipeProvider(const GUID &providerID)
{
    LIMITED_METHOD_CONTRACT;

    m_providerID = providerID;
    m_keywords = 0;

    // TODO: What is the right default?
    // Should this be an enum?
    m_level = 0;
}

const GUID& EventPipeProvider::GetProviderID() const
{
    LIMITED_METHOD_CONTRACT;

    return m_providerID;
}

bool EventPipeProvider::Enabled() const
{
    LIMITED_METHOD_CONTRACT;

    return m_keywords != 0;
}

bool EventPipeProvider::EventEnabled(INT64 keywords) const
{
    LIMITED_METHOD_CONTRACT;

    return ((m_keywords & keywords) != 0);
}

bool EventPipeProvider::EventEnabled(INT64 keywords, int level) const
{
    LIMITED_METHOD_CONTRACT;

    return (EventEnabled(keywords) && (level == m_level));
}

void EventPipeProvider::SetConfiguration(INT64 keywords, int level)
{
    LIMITED_METHOD_CONTRACT;

    // TODO: Do we need to handle synchronization here?
    m_keywords = keywords;
    m_level = level;
}
