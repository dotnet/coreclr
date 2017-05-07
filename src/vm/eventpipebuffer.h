// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_BUFFER_H__
#define __EVENTPIPE_BUFFER_H__

#ifdef FEATURE_PERFTRACING

#include "eventpipeevent.h"
#include "eventpipeeventinstance.h"

class EventPipeBuffer
{

    friend class EventPipeBufferList;
    friend class EventPipeBufferManager;

private:

    // A pointer to the actual buffer.
    BYTE *m_pBuffer;

    // The current write pointer.
    BYTE *m_pCurrent;

    // The max write pointer (end of the buffer).
    BYTE *m_pLimit;

    // The timestamp of the most recent event in the buffer.
    LARGE_INTEGER m_mostRecentTimeStamp;

    // Each buffer will become part of a per-thread linked list of buffers.
    // The linked list is invasive, thus we declare the pointers here.
    EventPipeBuffer *m_pPrevBuffer;
    EventPipeBuffer *m_pNextBuffer;

    EventPipeBuffer* GetPrevious() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pPrevBuffer;
    }

    EventPipeBuffer* GetNext() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_pNextBuffer;
    }

    void SetPrevious(EventPipeBuffer *pBuffer)
    {
        LIMITED_METHOD_CONTRACT;
        m_pPrevBuffer = pBuffer;
    }

    void SetNext(EventPipeBuffer *pBuffer)
    {
        LIMITED_METHOD_CONTRACT;
        m_pNextBuffer = pBuffer;
    }

public:

    EventPipeBuffer(unsigned int bufferSize);
    ~EventPipeBuffer();

    // Write an event to the buffer.
    // Returns:
    //  - true: The write succeeded.
    //  - false: The write failed.  In this case, the buffer should be considered full.
    bool WriteEvent(Thread *pThread, EventPipeEvent &event, BYTE *pData, unsigned int dataLength);

    // Get the timestamp of the most recent event in the buffer.
    LARGE_INTEGER GetMostRecentTimeStamp() const;

    // Clear the buffer.
    void Clear();

    // Get the next event from the buffer.  Input of NULL gets the first event.
    EventPipeEventInstance* GetNext(EventPipeEventInstance *pEvent);
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_BUFFER_H__
