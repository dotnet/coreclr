// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_H__
#define __EVENTPIPE_H__

#ifdef FEATURE_PERFTRACING
#include "common.h"
#include "stackcontents.h"

class CrstStatic;
class CrawlFrame;
class EventPipeConfiguration;
class EventPipeEvent;
class EventPipeEventInstance;
class EventPipeFile;
class EventPipeBufferManager;
class EventPipeEventSource;
class EventPipeProvider;
struct EventPipeProviderConfiguration;
class EventPipeSession;
class IpcStream;
enum class EventPipeSessionType;
enum class EventPipeSerializationFormat;
class EventPipeEventPayload;
struct EventData;

enum class EventPipeEventLevel
{
    LogAlways,
    Critical,
    Error,
    Warning,
    Informational,
    Verbose
};

// EVENT_FILTER_DESCRIPTOR (This type does not exist on non-Windows platforms.)
//  https://docs.microsoft.com/en-us/windows/desktop/api/evntprov/ns-evntprov-_event_filter_descriptor
//  The structure supplements the event provider, level, and keyword data that
//  determines which events are reported and traced. The structure gives the
//  event provider greater control over the selection of events for reporting
//  and tracing.
// TODO: EventFilterDescriptor and EventData (defined below) are the same.
struct EventFilterDescriptor
{
    // A pointer to the filter data.
    ULONGLONG Ptr;

    // The size of the filter data, in bytes. The maximum size is 1024 bytes.
    ULONG Size;

    // The type of filter data. The type is application-defined. An event
    // controller that knows about the provider and knows details about the
    // provider's events can use the Type field to send the provider an
    // arbitrary set of data for use as enhancements to the filtering of events.
    ULONG Type;
};

// Define the event pipe callback to match the ETW callback signature.
typedef void (*EventPipeCallback)(
    LPCGUID SourceID,
    ULONG IsEnabled,
    UCHAR Level,
    ULONGLONG MatchAnyKeywords,
    ULONGLONG MatchAllKeywords,
    EventFilterDescriptor *FilterData,
    void *CallbackContext);

typedef uint64_t EventPipeSessionID;

struct EventPipeProviderCallbackData
{
    LPCWSTR pFilterData;
    EventPipeCallback pCallbackFunction;
    bool enabled;
    INT64 keywords;
    EventPipeEventLevel providerLevel;
    void* pCallbackData;
};

class EventPipeProviderCallbackDataQueue
{
public:
    void Enqueue(EventPipeProviderCallbackData* pEventPipeProviderCallbackData);
    bool TryDequeue(EventPipeProviderCallbackData* pEventPipeProviderCallbackData);

private:
    SList<SListElem<EventPipeProviderCallbackData>> list;
};

class EventPipe
{
    // Declare friends.
    friend class EventPipeConfiguration;
    friend class EventPipeFile;
    friend class EventPipeProvider;

public:
    static const uint32_t MaxNumberOfSessions = 64;

    // Initialize the event pipe.
    static void Initialize();

    // Shutdown the event pipe.
    static void Shutdown();

    // Enable tracing via the event pipe.
    static EventPipeSessionID Enable(
        LPCWSTR strOutputPath,
        uint32_t circularBufferSizeInMB,
        const EventPipeProviderConfiguration *pProviders,
        uint32_t numProviders,
        EventPipeSessionType sessionType,
        EventPipeSerializationFormat format,
        IpcStream *const pStream);

    // Disable tracing via the event pipe.
    static void Disable(EventPipeSessionID id);

    // Get the session for the specified session ID.
    static EventPipeSession *GetSession(EventPipeSessionID id);

    // Specifies whether or not the event pipe is enabled.
    static bool Enabled();

    // Create a provider.
    static EventPipeProvider *CreateProvider(
        const SString &providerName,
        EventPipeCallback pCallbackFunction = nullptr,
        void *pCallbackData = nullptr);

    static EventPipeProvider *CreateProvider(const SString &providerName, EventPipeCallback pCallbackFunction, void *pCallbackData, EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Get a provider.
    static EventPipeProvider *GetProvider(const SString &providerName);

    // Delete a provider.
    static void DeleteProvider(EventPipeProvider *pProvider);

    // Write out an event from a flat buffer.
    // Data is written as a serialized blob matching the ETW serialization conventions.
    static void WriteEvent(EventPipeEvent &event, BYTE *pData, unsigned int length, LPCGUID pActivityId = NULL, LPCGUID pRelatedActivityId = NULL);

    // Write out an event from an EventData array.
    // Data is written as a serialized blob matching the ETW serialization conventions.
    static void WriteEvent(EventPipeEvent &event, EventData *pEventData, unsigned int eventDataCount, LPCGUID pActivityId = NULL, LPCGUID pRelatedActivityId = NULL);

    // Write out a sample profile event.
    static void WriteSampleProfileEvent(Thread *pSamplingThread, EventPipeEvent *pEvent, Thread *pTargetThread, StackContents &stackContents, BYTE *pData = NULL, unsigned int length = 0);

    // Get the managed call stack for the current thread.
    static bool WalkManagedStackForCurrentThread(StackContents &stackContents);

    // Get the managed call stack for the specified thread.
    static bool WalkManagedStackForThread(Thread *pThread, StackContents &stackContents);

    // Get next event.
    static EventPipeEventInstance *GetNextEvent(EventPipeSessionID sessionID);

#ifdef DEBUG
    static bool IsLockOwnedByCurrentThread();
#endif

    template <class T>
    static void RunWithCallbackPostponed(T f)
    {
        EventPipeProviderCallbackDataQueue eventPipeProviderCallbackDataQueue;
        EventPipeProviderCallbackData eventPipeProviderCallbackData;
        {
            CrstHolder _crst(GetLock());
            f(&eventPipeProviderCallbackDataQueue);
        }

        while (eventPipeProviderCallbackDataQueue.TryDequeue(&eventPipeProviderCallbackData))
            InvokeCallback(eventPipeProviderCallbackData);
    }

private:
    static void InvokeCallback(EventPipeProviderCallbackData eventPipeProviderCallbackData);

    // Get the event used to write metadata to the event stream.
    static EventPipeEventInstance *BuildEventMetadataEvent(EventPipeEventInstance &instance, unsigned int metadataId);

    // The counterpart to WriteEvent which after the payload is constructed
    static void WriteEventInternal(
        EventPipeEvent &event,
        EventPipeEventPayload &payload,
        LPCGUID pActivityId = nullptr,
        LPCGUID pRelatedActivityId = nullptr);

    static void WriteEventInternal(
        Thread *pThread,
        EventPipeEvent &event,
        EventPipeEventPayload &payload,
        LPCGUID pActivityId,
        LPCGUID pRelatedActivityId,
        Thread *pEventThread = nullptr,
        StackContents *pStack = nullptr);

    static void DisableInternal(EventPipeSessionID id, EventPipeProviderCallbackDataQueue* pEventPipeProviderCallbackDataQueue);

    // Enable the specified EventPipe session.
    static EventPipeSessionID EnableInternal(
        EventPipeSession *const pSession,
        EventPipeProviderCallbackDataQueue *pEventPipeProviderCallbackDataQueue);

    // Callback function for the stack walker.  For each frame walked, this callback is invoked.
    static StackWalkAction StackWalkCallback(CrawlFrame *pCf, StackContents *pData);

    template <typename EventPipeSessionHandlerCallback>
    static void ForEachSession(EventPipeSessionHandlerCallback callback)
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(IsLockOwnedByCurrentThread());

        for (VolatilePtr<EventPipeSession> &session : s_pSessions)
        {
            // Entering EventPipe lock gave us a barrier, we don't need
            // more of them
            EventPipeSession *const pSession = session.LoadWithoutBarrier();
            if (pSession)
                callback(*pSession);
        }
    }

    // Get the event pipe configuration lock.
    static CrstStatic *GetLock()
    {
        LIMITED_METHOD_CONTRACT;
        return &s_configCrst;
    }

    static CrstStatic s_configCrst;
    static Volatile<bool> s_tracingInitialized;
    static EventPipeConfiguration s_config;
    static VolatilePtr<EventPipeSession> s_pSessions[MaxNumberOfSessions];
    static EventPipeEventSource *s_pEventSource;
};

static_assert(EventPipe::MaxNumberOfSessions == 64, "Maximum number of EventPipe sessions is not 64.");

struct EventPipeProviderConfiguration
{
private:
    LPCWSTR m_pProviderName = nullptr;
    UINT64 m_keywords = 0;
    UINT32 m_loggingLevel = 0;
    LPCWSTR m_pFilterData = nullptr;

public:
    EventPipeProviderConfiguration() = default;

    EventPipeProviderConfiguration(LPCWSTR pProviderName, UINT64 keywords, UINT32 loggingLevel, LPCWSTR pFilterData) :
        m_pProviderName(pProviderName),
        m_keywords(keywords),
        m_loggingLevel(loggingLevel),
        m_pFilterData(pFilterData)
    {
    }

    LPCWSTR GetProviderName() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pProviderName;
    }

    UINT64 GetKeywords() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_keywords;
    }

    UINT32 GetLevel() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_loggingLevel;
    }

    LPCWSTR GetFilterData() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pFilterData;
    }
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_H__
