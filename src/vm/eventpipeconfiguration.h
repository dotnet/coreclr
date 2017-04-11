// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_CONFIGURATION_H__
#define __EVENTPIPE_CONFIGURATION_H__

#include "common.h"

class EventPipeEvent;
class EventPipeProvider;

class EventPipeConfiguration
{
public:

    EventPipeConfiguration();
    ~EventPipeConfiguration();

    // Register a provider.
    bool RegisterProvider(EventPipeProvider &provider);

    // Get the provider with the specified provider ID if it exists.
    EventPipeProvider* GetProvider(const GUID &providerID);

private:

    // The list of event pipe providers.
    SList<SListElem<EventPipeProvider*>> *m_pProviderList;

    // Lock to protect access to the provider list.
    SpinLock m_lock;
};

#endif // __EVENTPIPE_CONFIGURATION_H__
