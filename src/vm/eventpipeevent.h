// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_EVENT_H__
#define __EVENTPIPE_EVENT_H__

#ifdef FEATURE_PERFTRACING

#include "eventpipeprovider.h"

class EventPipeEvent
{
    // Declare friends.
    friend class EventPipeProvider;

private:

    // The provider that contains the event.
    EventPipeProvider *m_pProvider;

    // Bit vector containing the keywords that enable the event.
    INT64 m_keywords;

    // The ID (within the provider) of the event.
    unsigned int m_eventID;

    // The version of the event.
    unsigned int m_eventVersion;

    // The verbosity of the event.
    EventPipeEventLevel m_level;

    // True if a call stack should be captured when writing the event.
    bool m_needStack;

    // True if the event is current enabled.
    Volatile<bool> m_enabled;

    // Metadata
    BYTE *m_pMetadata;

    // Metadata length;
    unsigned int m_metadataLength;

    // Refreshes the runtime state for this event.
    // Called by EventPipeProvider when the provider configuration changes.
    void RefreshState();

    // Only EventPipeProvider can create events.
    // The provider is responsible for allocating and freeing events.
    EventPipeEvent(EventPipeProvider &provider, INT64 keywords, unsigned int eventID, unsigned int eventVersion, EventPipeEventLevel level, bool needStack, BYTE *pMetadata = NULL, unsigned int metadataLength = 0);

public:
    ~EventPipeEvent();

    // Get the provider associated with this event.
    EventPipeProvider *GetProvider() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pProvider;
    }

    // Get the keywords that enable the event.
    INT64 GetKeywords() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_keywords;
    }

    // Get the ID (within the provider) of the event.
    unsigned int GetEventID() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_eventID;
    }

    // Get the version of the event.
    unsigned int GetEventVersion() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_eventVersion;
    }

    // Get the verbosity of the event.
    EventPipeEventLevel GetLevel() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_level;
    }

    // True if a call stack should be captured when writing the event.
    bool NeedStack() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_needStack;
    }

    // True if the event is currently enabled.
    bool IsEnabled() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_enabled;
    }

    BYTE *GetMetadata() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pMetadata;
    }

    unsigned int GetMetadataLength() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_metadataLength;
    }

private:
    // used when Metadata is not provided
    BYTE *BuildMinimumMetadata();

    static constexpr uint32_t GetMinimumMetadataLength()
    {
        const WCHAR EmptyString[] = W("");
        return sizeof(m_eventID) +
               sizeof(EmptyString) + // size of empty unicode string
               sizeof(m_keywords) +
               sizeof(m_eventVersion) +
               sizeof(m_level) +
               sizeof(uint32_t); // parameter count
    }
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_EVENT_H__
