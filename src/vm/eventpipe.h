// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_H__
#define __EVENTPIPE_H__

#ifdef FEATURE_PERFTRACING

class CrstStatic;
class EventPipeConfiguration;
class EventPipeEvent;
class EventPipeFile;
class EventPipeJsonFile;
class EventPipeBuffer;
class EventPipeBufferManager;
class EventPipeProvider;
class MethodDesc;
class SampleProfilerEventInstance;
struct EventPipeProviderConfiguration;

// Define the event pipe callback to match the ETW callback signature.
typedef void (*EventPipeCallback)(
    LPCGUID SourceID,
    ULONG IsEnabled,
    UCHAR Level,
    ULONGLONG MatchAnyKeywords,
    ULONGLONG MatchAllKeywords,
    void *FilterData,
    void *CallbackContext);

struct EventData
{
public:
    unsigned long ptr;
    unsigned int size;
    unsigned int reserved;
};

class EventPipeEventPayload
{
private:
    BYTE *m_pData;
    EventData **m_pBlobs;
    unsigned int m_blobCount;
    unsigned int m_size;
    bool m_performedAllocation;

public:
    // Build this payload with a flat buffer inside
    EventPipeEventPayload(byte *pData, unsigned int length);

    // Build this payload to contain an array of EventData blobs
    EventPipeEventPayload(EventData **pBlobs, unsigned int blobCount);

    // If a buffer was allocated internally, delete it
    ~EventPipeEventPayload();
    
    // If the data is stored only as an array of blobs, create a flat buffer and copy into it
    void Flatten();

    // Copy the data (whether flat or array of blobs) into a flat buffer at pDst
    void CopyData(BYTE *pDst);

    // Return true is the data is stored in a flat buffer
    bool IsFlattened() const
    {
        LIMITED_METHOD_CONTRACT;

        return m_pData != NULL;
    }

    // The the size of buffer needed to contain the stored data
    unsigned int GetSize() const
    {
        LIMITED_METHOD_CONTRACT;

        return m_size;
    }

    BYTE* GetFlatData() const
    {
        LIMITED_METHOD_CONTRACT;

        return m_pData;
    }

    EventData** GetBlobData() const
    {
        LIMITED_METHOD_CONTRACT;

        return m_pBlobs;
    }
};

class StackContents
{
private:

    const static unsigned int MAX_STACK_DEPTH = 100;

    // Array of IP values from a stack crawl.
    // Top of stack is at index 0.
    UINT_PTR m_stackFrames[MAX_STACK_DEPTH];

#ifdef _DEBUG
    // Parallel array of MethodDesc pointers.
    // Used for debug-only stack printing.
    MethodDesc* m_methods[MAX_STACK_DEPTH];
#endif // _DEBUG

    // The next available slot in StackFrames.
    unsigned int m_nextAvailableFrame;

public:

    StackContents()
    {
        LIMITED_METHOD_CONTRACT;

        Reset();
    }

    void CopyTo(StackContents *pDest)
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(pDest != NULL);

        memcpy_s(pDest->m_stackFrames, MAX_STACK_DEPTH * sizeof(UINT_PTR), m_stackFrames, sizeof(UINT_PTR) * m_nextAvailableFrame);
#ifdef _DEBUG
        memcpy_s(pDest->m_methods, MAX_STACK_DEPTH * sizeof(MethodDesc*), m_methods, sizeof(MethodDesc*) * m_nextAvailableFrame);
#endif
        pDest->m_nextAvailableFrame = m_nextAvailableFrame;
    }

    void Reset()
    {
        LIMITED_METHOD_CONTRACT;

        m_nextAvailableFrame = 0;
    }

    bool IsEmpty()
    {
        LIMITED_METHOD_CONTRACT;

        return (m_nextAvailableFrame == 0);
    }

    unsigned int GetLength()
    {
        LIMITED_METHOD_CONTRACT;

        return m_nextAvailableFrame;
    }

    UINT_PTR GetIP(unsigned int frameIndex)
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(frameIndex < MAX_STACK_DEPTH);

        if (frameIndex >= MAX_STACK_DEPTH)
        {
            return 0;
        }

        return m_stackFrames[frameIndex];
    }

#ifdef _DEBUG
    MethodDesc* GetMethod(unsigned int frameIndex)
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(frameIndex < MAX_STACK_DEPTH);

        if (frameIndex >= MAX_STACK_DEPTH)
        {
            return NULL;
        }

        return m_methods[frameIndex];
    }
#endif // _DEBUG

    void Append(UINT_PTR controlPC, MethodDesc *pMethod)
    {
        LIMITED_METHOD_CONTRACT;

        if(m_nextAvailableFrame < MAX_STACK_DEPTH)
        {
            m_stackFrames[m_nextAvailableFrame] = controlPC;
#ifdef _DEBUG
            m_methods[m_nextAvailableFrame] = pMethod;
#endif
            m_nextAvailableFrame++;
        }
    }

    BYTE* GetPointer() const
    {
        LIMITED_METHOD_CONTRACT;

        return (BYTE*)m_stackFrames;
    }

    unsigned int GetSize() const
    {
        LIMITED_METHOD_CONTRACT;

        return (m_nextAvailableFrame * sizeof(UINT_PTR));
    }
};

class EventPipe
{
    // Declare friends.
    friend class EventPipeConfiguration;
    friend class EventPipeFile;
    friend class EventPipeProvider;
    friend class EventPipeBufferManager;
    friend class SampleProfiler;

    public:

        // Initialize the event pipe.
        static void Initialize();

        // Shutdown the event pipe.
        static void Shutdown();

        // Enable tracing from the start-up path based on COMPLUS variable.
        static void EnableOnStartup();

        // Enable tracing via the event pipe.
        static void Enable(
            LPCWSTR strOutputPath,
            unsigned int circularBufferSizeInMB,
            EventPipeProviderConfiguration *pProviders,
            int numProviders);

        // Disable tracing via the event pipe.
        static void Disable();

        // Specifies whether or not the event pipe is enabled.
        static bool Enabled();

        // Create a provider.
        static EventPipeProvider* CreateProvider(const GUID &providerID, EventPipeCallback pCallbackFunction = NULL, void *pCallbackData = NULL);

        // Delete a provider.
        static void DeleteProvider(EventPipeProvider *pProvider);

        // Write out an event.
        // Data is written as a serialized blob matching the ETW serialization conventions.
        static void WriteEvent(EventPipeEvent &event, BYTE *pData, unsigned int length, LPCGUID pActivityId = NULL, LPCGUID pRelatedActivityId = NULL);

        // Write out an event.
        // Data is written as a serialized blob matching the ETW serialization conventions.
        static void WriteEventBlob(EventPipeEvent &event, EventData **pBlobs, unsigned int blobCount, LPCGUID pActivityId = NULL, LPCGUID pRelatedActivityId = NULL);

        // Write out a sample profile event.
        static void WriteSampleProfileEvent(Thread *pSamplingThread, EventPipeEvent *pEvent, Thread *pTargetThread, StackContents &stackContents, BYTE *pData = NULL, unsigned int length = 0);
        
        // Get the managed call stack for the current thread.
        static bool WalkManagedStackForCurrentThread(StackContents &stackContents);

        // Get the managed call stack for the specified thread.
        static bool WalkManagedStackForThread(Thread *pThread, StackContents &stackContents);

    protected:

        // The counterpart to WriteEvent which after the payload is constructed
        static void WriteEventInternal(EventPipeEvent &event, EventPipeEventPayload &payload, LPCGUID pActivityId = NULL, LPCGUID pRelatedActivityId = NULL);

    private:

        // Callback function for the stack walker.  For each frame walked, this callback is invoked.
        static StackWalkAction StackWalkCallback(CrawlFrame *pCf, StackContents *pData);

        // Get the configuration object.
        // This is called directly by the EventPipeProvider constructor to register the new provider.
        static EventPipeConfiguration* GetConfiguration();

        // Get the event pipe configuration lock.
        static CrstStatic* GetLock();

        static CrstStatic s_configCrst;
        static bool s_tracingInitialized;
        static EventPipeConfiguration *s_pConfig;
        static EventPipeBufferManager *s_pBufferManager;
        static EventPipeFile *s_pFile;
#ifdef _DEBUG
        static EventPipeFile *s_pSyncFile;
        static EventPipeJsonFile *s_pJsonFile;
#endif // _DEBUG
};

struct EventPipeProviderConfiguration
{

private:

    LPCWSTR m_pProviderName;
    UINT64 m_keywords;
    unsigned int m_loggingLevel;

public:

    EventPipeProviderConfiguration()
    {
        LIMITED_METHOD_CONTRACT;
        m_pProviderName = NULL;
        m_keywords = NULL;
        m_loggingLevel = 0;
    }

    EventPipeProviderConfiguration(
        LPCWSTR pProviderName,
        UINT64 keywords,
        unsigned int loggingLevel)
    {
        LIMITED_METHOD_CONTRACT;
        m_pProviderName = pProviderName;
        m_keywords = keywords;
        m_loggingLevel = loggingLevel;
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

    unsigned int GetLevel() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_loggingLevel;
    }
};

class EventPipeInternal
{

public:

    static void QCALLTYPE Enable(
        __in_z LPCWSTR outputFile,
        unsigned int circularBufferSizeInMB,
        long profilerSamplingRateInNanoseconds,
        EventPipeProviderConfiguration *pProviders,
        int numProviders);

    static void QCALLTYPE Disable();

    static INT_PTR QCALLTYPE CreateProvider(
        GUID providerID,
        EventPipeCallback pCallbackFunc);

    static INT_PTR QCALLTYPE DefineEvent(
        INT_PTR provHandle,
        unsigned int eventID,
        __int64 keywords,
        unsigned int eventVersion,
        unsigned int level,
        void *pMetadata,
        unsigned int metadataLength);

    static void QCALLTYPE DeleteProvider(
        INT_PTR provHandle);

    static void QCALLTYPE WriteEvent(
        INT_PTR eventHandle,
        unsigned int eventID,
        void *pData,
        unsigned int length,
        LPCGUID pActivityId, LPCGUID pRelatedActivityId);

    static void QCALLTYPE WriteEventBlob(
        INT_PTR eventHandle,
        unsigned int eventID,
        EventData **pBlobs,
        unsigned int blobCount,
        LPCGUID pActivityId, LPCGUID pRelatedActivityId);
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_H__
