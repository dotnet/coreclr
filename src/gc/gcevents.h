// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __GCEVENTS_H__
#define __GCEVENTS_H__

#include "common.h"
#include "gcenv.h"
#include "gc.h"
#include "gceventstatus.h"

namespace gc_events
{

class GCEventBase
{
private:
    const char *m_name;
    GCEventProvider m_provider;
    GCEventLevel m_level;
    GCEventKeyword m_keyword;

public:
    GCEventBase(const char *name, GCEventProvider provider, GCEventLevel level, GCEventKeyword keyword)
        : m_name(name), m_provider(provider), m_level(level), m_keyword(keyword)
    {}

    GCEventProvider Provider() const { return m_provider; }

    GCEventLevel Level() const { return m_level; }

    GCEventKeyword Keyword() const { return m_keyword; }

    __forceinline bool IsEnabled() const
    {
        return GCEventStatus::IsEnabled(m_provider, m_keyword, m_level);
    }
};

class GCKnownEvent : public GCEventBase
{
public:
    GCKnownEvent(const char *name, GCEventProvider provider, GCEventLevel level, GCEventKeyword keyword)
        : GCEventBase(name, provider, level, keyword)
    {}
};

class GCDynamicEvent : public GCEventBase
{
    /* TODO(segilles) - Not Yet Implemented */
};

} // namespace gc_events

#define KNOWN_EVENT(name, _provider, _level, _keyword)   \
  extern gc_events::GCKnownEvent name##EventDescriptor;
#include "gcevents.def"

#define EVENT_ENABLED(name) \
  name##EventDescriptor.IsEnabled()

#endif // __GCEVENTS_H__
