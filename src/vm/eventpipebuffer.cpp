// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifdef FEATURE_PERFTRACING

#include "common.h"
#include "eventpipeeventinstance.h"
#include "eventpipebuffer.h"

EventPipeBuffer::EventPipeBuffer(unsigned int bufferSize)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pBuffer = new BYTE[bufferSize];
    memset(m_pBuffer, 0, bufferSize);
    m_pCurrent = m_pBuffer;
    m_pLimit = m_pBuffer + bufferSize;

    m_mostRecentTimeStamp.QuadPart = 0;
    m_pPrevBuffer = NULL;
    m_pNextBuffer = NULL;
}

EventPipeBuffer::~EventPipeBuffer()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if(m_pBuffer != NULL)
    {
        delete[] m_pBuffer;
    }
}

bool EventPipeBuffer::WriteEvent(Thread *pThread, EventPipeEvent &event, BYTE *pData, unsigned int dataLength)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pThread != NULL);
    }
    CONTRACTL_END;

    // Calculate the size of the event.
    unsigned int eventSize = sizeof(EventPipeEventInstance) + dataLength;

    // Make sure we have enough space to write the event.
    if(m_pCurrent + eventSize >= m_pLimit)
    {
        return false;
    }

    // Calculate the location of the data payload.
    BYTE *pDataDest = m_pCurrent + sizeof(EventPipeEventInstance);

    bool success = true;
    EX_TRY
    {
        // Write the event payload data to the buffer.
        memcpy(pDataDest, pData, dataLength);

        // Placement-new the EventPipeEventInstance.
        EventPipeEventInstance *pInstance = new (m_pCurrent) EventPipeEventInstance(
            event,
            pThread->GetOSThreadId(),
            pData,
            dataLength);

        m_mostRecentTimeStamp = pInstance->GetTimeStamp();
    }
    EX_CATCH
    {
        // If a failure occurs, bail out and don't advance the pointer.
        success = false;
    }
    EX_END_CATCH(SwallowAllExceptions);

    if(success)
    {
        // Advance the current pointer past the event.
        m_pCurrent += eventSize;
    }

    return success;
}

LARGE_INTEGER EventPipeBuffer::GetMostRecentTimeStamp() const
{
    LIMITED_METHOD_CONTRACT;

    return m_mostRecentTimeStamp;
}

void EventPipeBuffer::Clear()
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    memset(m_pBuffer, 0, (size_t)(m_pLimit - m_pBuffer));
    m_pCurrent = m_pBuffer;
    m_mostRecentTimeStamp.QuadPart = 0;
}

// TODO: We need to know the timestamp that we stop tracing to ensure that we don't read an event that might be
// partially written after tracing stops.
EventPipeEventInstance* EventPipeBuffer::GetNext(EventPipeEventInstance *pEvent)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // If input is NULL, return the first event if there is one.
    if(pEvent == NULL)
    {
        // If this buffer contains an event, return it.
        // Otherwise, return NULL.
        if(m_pCurrent > m_pBuffer)
        {
            return (EventPipeEventInstance*)m_pBuffer;
        }
        return NULL;
    }

    // Confirm that pEvent is within the range of the buffer.
    if(((BYTE*)pEvent < m_pBuffer) || ((BYTE*)pEvent >= m_pLimit))
    {
        return NULL;
    }

    // We have a pointer within the bounds of the buffer.
    // Find the next event by skipping the current event with it's data payload immediately after the instance.
    EventPipeEventInstance *pNextInstance = (EventPipeEventInstance *)(pEvent->GetData() + pEvent->GetLength());

    return pNextInstance;
}

#endif // FEATURE_PERFTRACING
