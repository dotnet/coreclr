// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeevent.h"
#include "eventpipeprovider.h"

#ifdef FEATURE_PERFTRACING

EventPipeEvent::EventPipeEvent(
    EventPipeProvider &provider,
    INT64 keywords,
    unsigned int eventID,
    unsigned int eventVersion,
    EventPipeEventLevel level,
    bool needStack,
    BYTE *pMetadata,
    unsigned int metadataLength) : m_pProvider(&provider),
                                   m_keywords(keywords),
                                   m_eventID(eventID),
                                   m_eventVersion(eventVersion),
                                   m_level(level),
                                   m_needStack(needStack),
                                   m_enabled(false),
                                   m_pMetadata(nullptr),
                                   m_sessions(0)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(&provider != nullptr);
    }
    CONTRACTL_END;

    if (pMetadata != nullptr)
    {
        m_pMetadata = new BYTE[metadataLength];
        memcpy(m_pMetadata, pMetadata, metadataLength);
        m_metadataLength = metadataLength;
    }
    else
    {
        // if metadata is not provided, we have to build the minimum version. It's required by the serialization contract
        m_pMetadata = BuildMinimumMetadata();
        m_metadataLength = MinimumMetadataLength;
    }
}

EventPipeEvent::~EventPipeEvent()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    delete[] m_pMetadata;
}

BYTE *EventPipeEvent::BuildMinimumMetadata()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    BYTE *minmumMetadata = new BYTE[MinimumMetadataLength];
    BYTE *currentPtr = minmumMetadata;

    // the order of fields is defined in coreclr\src\mscorlib\shared\System\Diagnostics\Tracing\EventSource.cs DefineEventPipeEvents method
    memcpy(currentPtr, &m_eventID, sizeof(m_eventID));
    currentPtr += sizeof(m_eventID);

    SString eventName = SString::Empty();
    unsigned int eventNameSize = (eventName.GetCount() + 1) * sizeof(WCHAR);
    memcpy(currentPtr, (BYTE *)eventName.GetUnicode(), eventNameSize);
    currentPtr += eventNameSize;

    memcpy(currentPtr, &m_keywords, sizeof(m_keywords));
    currentPtr += sizeof(m_keywords);

    memcpy(currentPtr, &m_eventVersion, sizeof(m_eventVersion));
    currentPtr += sizeof(m_eventVersion);

    memcpy(currentPtr, &m_level, sizeof(m_level));
    currentPtr += sizeof(m_level);

    unsigned int noParameters = 0;
    memcpy(currentPtr, &noParameters, sizeof(noParameters));
    currentPtr += sizeof(noParameters);

    return minmumMetadata;
}

void EventPipeEvent::RefreshState()
{
    LIMITED_METHOD_CONTRACT;
    m_enabled = m_pProvider->EventEnabled(m_keywords, m_level);
}

void EventPipeEvent::RefreshState(
    uint64_t sessionId,
    INT64 sessionKeywords,
    EventPipeEventLevel sessionLevel)
{
    LIMITED_METHOD_CONTRACT;
    RefreshState();

    // If event is enabled, then we need to check if applicable to this session.
    const bool areKeywordsSet = (m_keywords == 0) || (sessionKeywords == 0) || ((m_keywords & sessionKeywords) != 0);
    const bool isAppropriateLevel = (sessionLevel == EventPipeEventLevel::LogAlways) || (sessionLevel >= m_level);
    m_sessions = (m_enabled && areKeywordsSet && isAppropriateLevel) ?
        (m_sessions | sessionId) : (m_sessions & ~sessionId);
}

// TODO: Refresh Provider/Events {SessionIds, Keywords, Level} after removing a session.
void EventPipeEvent::LazyRemoveSession(uint64_t sessionId)
{
    LIMITED_METHOD_CONTRACT;
    if (m_sessions & sessionId)
        m_sessions &= ~sessionId;
}

#endif // FEATURE_PERFTRACING
