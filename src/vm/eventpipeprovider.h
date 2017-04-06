// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_PROVIDER_H__
#define __EVENTPIPE_PROVIDER_H__

#include "common.h"

class EventPipeProvider
{
private:
    // The GUID of the provider.
    GUID m_providerID;

    // Bit vector containing the currently enabled keywords.
    INT64 m_keywords;

    // The current verbosity of the provider.
    int m_level;

public:

    EventPipeProvider(const GUID &providerID);

    // Get the provider ID.
    const GUID& GetProviderID() const;

    // Determine if the provider is enabled.
    bool Enabled() const;

    // Determine if the specified keywords are enabled.
    bool EventEnabled(INT64 keywords) const;

    // Determine if the specified keywords and level match the configuration.
    bool EventEnabled(INT64 keywords, int level) const;

    // Set the provider configuration (enable and disable sets of events).
    void SetConfiguration(INT64 keywords, int level);
};

#endif // __EVENTPIPE_PROVIDER_H__
