// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_CONFIGURATION_H__
#define __EVENTPIPE_CONFIGURATION_H__

#ifdef FEATURE_PERFTRACING

#include "eventpipe.h"
#include "slist.h"

class EventPipeSessionProvider;
class EventPipeEvent;
class EventPipeEventInstance;
class EventPipeProvider;
class EventPipeSession;

class EventPipeConfiguration
{
public:
    EventPipeConfiguration(EventPipeSessions *pSessions);
    ~EventPipeConfiguration();

    // Perform initialization that cannot be performed in the constructor.
    void Initialize();

    // Create a new provider.
    EventPipeProvider *CreateProvider(const SString &providerName, EventPipeCallback pCallbackFunction, void *pCallbackData, EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Delete a provider.
    void DeleteProvider(EventPipeProvider *pProvider);

    // Register a provider.
    bool RegisterProvider(EventPipeProvider &provider, EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Unregister a provider.
    bool UnregisterProvider(EventPipeProvider &provider);

    // Get the provider with the specified provider ID if it exists.
    EventPipeProvider *GetProvider(const SString &providerID);

    // Enable a session in the event pipe.
    void Enable(EventPipeSession *pSession, EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Disable a session in the event pipe.
    void Disable(EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Get the event used to write metadata to the event stream.
    EventPipeEventInstance *BuildEventMetadataEvent(EventPipeEventInstance &sourceInstance, unsigned int metdataId);

    // Delete deferred providers.
    void DeleteDeferredProviders();

private:
    // Get the provider without taking the lock.
    EventPipeProvider *GetProviderNoLock(const SString &providerID);

    // Get the enabled provider.
    EventPipeSessionProvider *GetSessionProvider(EventPipeSession *pSession, EventPipeProvider *pProvider);

    // The list of EventPipe sessions.
    EventPipeSessions *const m_pSessions;

    // The list of event pipe providers.
    SList<SListElem<EventPipeProvider *>> *m_pProviderList;

    // The provider used to write configuration events to the event stream.
    EventPipeProvider *m_pConfigProvider;

    // The event used to write event information to the event stream.
    EventPipeEvent *m_pMetadataEvent;

    // The provider name for the configuration event pipe provider.
    // This provider is used to emit configuration events.
    const static WCHAR *s_configurationProviderName;
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_CONFIGURATION_H__
