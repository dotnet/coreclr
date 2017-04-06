// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_CONFIGURATION_H__
#define __EVENTPIPE_CONFIGURATION_H__

#include "common.h"

class EventPipeProvider;

// Provider ID constants (these match src/vm/ClrEtwAll.man).
DEFINE_GUID(RuntimeProviderID,0xe13c0d23,0xccbc,0x4e12,0x93,0x1b,0xd9,0xcc,0x2e,0xee,0x27,0xe4);
DEFINE_GUID(PrivateProviderID,0x763fd754,0x7086,0x4dfe,0x95,0xeb,0xc0,0x1a,0x46,0xfa,0xf4,0xca);
DEFINE_GUID(RundownProviderID,0xa669021c,0xc450,0x4609,0xa0,0x35,0x5a,0xf5,0x9a,0xf4,0xdf,0x18);
DEFINE_GUID(StressProviderID,0xcc2bcbba,0x16b6,0x4cf3,0x89,0x90,0xd7,0x4c,0x2e,0x8a,0xf5,0x00);


class EventPipeConfiguration
{
public:

    EventPipeConfiguration();

    // Get or create a provider object.
    EventPipeProvider* GetOrCreateProvider(const GUID &providerID);

    // Clear disable all providers.
    void DisableAllProviders();

private:

    const static unsigned int MAX_PROVIDERS = 100;

    // The list of event pipe providers.
    EventPipeProvider* m_providers[MAX_PROVIDERS];

    // The index of the next free slot in the list of providers.
    unsigned int m_nextProviderIndex;
};

#endif // __EVENTPIPE_CONFIGURATION_H__
