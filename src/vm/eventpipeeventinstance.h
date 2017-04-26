// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_EVENTINSTANCE_H__
#define __EVENTPIPE_EVENTINSTANCE_H__

#include "eventpipe.h"
#include "eventpipeevent.h"
#include "fastserializer.h"

class EventPipeEventInstance
{

public:

    EventPipeEventInstance(EventPipeEvent &event, DWORD threadID, BYTE *pData, size_t length);

    // Get the stack contents object to either read or write to it.
    StackContents* GetStack();

    // Static serialize function that can be called by FastSerializer.
    static void Serialize(FastSerializer *pSerializer, EventPipeEventInstance *pInstance);

    // Called from the static Serialize method to do the actual work.
    void Serialize(FastSerializer *pSerializer);

    // Serialize this event to the JSON file.
    void SerializeToJsonFile(EventPipeJsonFile *pFile);

protected:

    EventPipeEvent *m_pEvent;
    DWORD m_threadID;
    LARGE_INTEGER m_timeStamp;

    BYTE *m_pData;
    size_t m_dataLength;
    StackContents m_stackContents;
};

// A specific type of event instance for use by the SampleProfiler.
// This is needed because the SampleProfiler knows how to walk stacks belonging
// to threads other than the current thread.
class SampleProfilerEventInstance : public EventPipeEventInstance
{

public:

    SampleProfilerEventInstance(Thread *pThread);
};

#endif // __EVENTPIPE_EVENTINSTANCE_H__
