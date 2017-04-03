// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_CONFIGURATION_H__
#define __EVENTPIPE_CONFIGURATION_H__

#include "common.h"

// Provider ID constants (these match src/vm/ClrEtwAll.man).
DEFINE_GUID(RuntimeProviderID,0xe13c0d23,0xccbc,0x4e12,0x93,0x1b,0xd9,0xcc,0x2e,0xee,0x27,0xe4);
DEFINE_GUID(PrivateProviderID,0x763fd754,0x7086,0x4dfe,0x95,0xeb,0xc0,0x1a,0x46,0xfa,0xf4,0xca);
DEFINE_GUID(RundownProviderID,0xa669021c,0xc450,0x4609,0xa0,0x35,0x5a,0xf5,0x9a,0xf4,0xdf,0x18);
DEFINE_GUID(StressProviderID,0xcc2bcbba,0x16b6,0x4cf3,0x89,0x90,0xd7,0x4c,0x2e,0x8a,0xf5,0x00);


class EventPipeConfiguration
{
private:

    // Contains the configuration for a single event provider.
    class ProviderConfiguration
    {
    private:

        // Every provider has a unique GUID identifier.
        GUID m_providerID;

        // Bitmask representing the enabled keywords for the provider.
        // Every provider can have at most 64 keywords.
        INT64 m_enabledKeywords;

    public:

        ProviderConfiguration(const GUID &providerID, INT64 enabledKeywords)
        {
            LIMITED_METHOD_CONTRACT;

            m_providerID = providerID;
            m_enabledKeywords = enabledKeywords;
        }

        INT64 GetEnabledKeywords()
        {
            LIMITED_METHOD_CONTRACT;

            return m_enabledKeywords;
        }

        bool SetKeywords(INT64 enabledKeywords)
        {
            LIMITED_METHOD_CONTRACT;

            m_enabledKeywords = enabledKeywords;
            return true;
        }
    };

public:

    EventPipeConfiguration();

    // Set the configuration for a provider.
    bool SetConfiguration(const GUID &providerID, INT64 keywords);

    // Clear disable all providers.
    bool DisableAllProviders();

    // Returns true if the specified provider is enabled.
    bool ProviderEnabled(const GUID &providerID);

    // Returns true if the specific provider and keyword combination is enabled.
    bool KeywordEnabled(const GUID &providerID, INT64 keyword);

private:

    // Get the configuration for the specified provider ID.
    static ProviderConfiguration* GetProviderConfig(const GUID &providerID);

    // Configuration objects for each known provider.
    static ProviderConfiguration s_runtimeProviderConfig;
    static ProviderConfiguration s_privateProviderConfig;
    static ProviderConfiguration s_rundownProviderConfig;
    static ProviderConfiguration s_stressProviderConfig;
};

#endif // __EVENTPIPE_CONFIGURATION_H__
