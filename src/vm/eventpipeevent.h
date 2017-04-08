// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_EVENT_H__
#define __EVENTPIPE_EVENT_H__

#include "common.h"

class EventPipeProvider;

class EventPipeEvent
{
private:

    // The provider that contains the event.
    EventPipeProvider *m_pProvider;

    // Bit vector containing the keywords that enable the event.
    INT64 m_keywords;

    // The unique ID (with the provider) of the event.
    int m_eventID;

    // The verbosity of the provider.
    int m_level;

    // True if a call stack should be captured when writing the event.
    bool m_needStack;

    // True if the event is current enabled.
    bool m_enabled;

public:

    EventPipeEvent(EventPipeProvider &provider, INT64 keywords, int eventID, int level, bool needStack);

    // Get the provider associated with this event.
    EventPipeProvider* GetProvider() const;

    // Get the keywords that enable the event.
    INT64 GetKeywords() const;

    // Get the unique ID (within the provider) of the event.
    int GetEventID() const;

    // Get the verbosity of the event.
    int GetLevel() const;

    // True if a call stack should be captured when writing the event.
    bool NeedStack() const;

    // True if the event is currently enabled.
    bool IsEnabled() const;

    // TODO:Make this private and add EventPipeProvider as a friend.
    // Refreshes the runtime state for this event.  Called by EventPipeProvider.
    void RefreshState();
};

#endif // __EVENTPIPE_EVENT_H__
