// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifdef FEATURE_PERFTRACING

#include "common.h"
#include "eventpipeconfiguration.h"
#include "eventpipebuffer.h"
#include "eventpipebuffermanager.h"

EventPipeBufferManager::EventPipeBufferManager()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pPerThreadBufferList = new SList<SListElem<EventPipeBufferList*>>();
    m_sizeOfAllBuffers = 0;
    m_lock.Init(LOCK_TYPE_DEFAULT);

#ifdef _DEBUG
    m_numBuffersAllocated = 0;
    m_numBuffersStolen = 0;
#endif // _DEBUG
}

EventPipeBuffer* EventPipeBufferManager::AllocateBufferForThread(Thread *pThread, unsigned int requestSize)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pThread != NULL);
        PRECONDITION(requestSize > 0);
    }
    CONTRACTL_END;

    // Allocating a buffer requires us to take the lock.
    SpinLockHolder _slh(&m_lock);

    // Determine if the requesting thread has at least one buffer.
    // If not, we guarantee that each thread gets at least one (to prevent thrashing when the circular buffer size is too small).
    bool allocateNewBuffer = false;
    EventPipeBufferList *pThreadBufferList = pThread->GetEventPipeBufferList();
    if(pThreadBufferList == NULL)
    {
        pThreadBufferList = new EventPipeBufferList();
        m_pPerThreadBufferList->InsertTail(new SListElem<EventPipeBufferList*>(pThreadBufferList));
        pThread->SetEventPipeBufferList(pThreadBufferList);
        allocateNewBuffer = true;
    }

    // Determine if policy allows us to allocate another buffer, or if we need to steal one
    // from another thread.
    if(!allocateNewBuffer)
    {
        EventPipeConfiguration *pConfig = EventPipe::GetConfiguration();
        if(pConfig == NULL)
        {
            return NULL;
        }

        size_t circularBufferSizeInBytes = pConfig->GetCircularBufferSize();
        if(m_sizeOfAllBuffers < circularBufferSizeInBytes)
        {
            // We don't worry about the fact that a new buffer could put us over the circular buffer size.
            // This is OK, and we won't do it again if we actually go over.
            allocateNewBuffer = true;
        }
    }

    EventPipeBuffer *pNewBuffer = NULL;
    if(!allocateNewBuffer)
    {
        // We can't allocate a new buffer.
        // Find the oldest buffer, zero it, and re-purpose it for this thread.

        // Find the thread that contains the oldest stealable buffer, and get its list of buffers.
        EventPipeBufferList *pListToStealFrom = FindThreadToStealFrom();
        if(pListToStealFrom != NULL)
        {
            // Assert that the buffer we're stealing is not the only buffer in the list.
            // This invariant is enforced by FindThreadToStealFrom.
            _ASSERTE((pListToStealFrom->GetHead() != NULL) && (pListToStealFrom->GetHead()->GetNext() != NULL));

            // Remove the oldest buffer from the list.
            pNewBuffer = pListToStealFrom->GetAndRemoveHead();

            // Clear the buffer.
            pNewBuffer->Clear();

#ifdef _DEBUG
            m_numBuffersStolen++;
#endif // _DEBUG

        }
        else
        {
            // This only happens when # of threads == # of buffers.
            // We'll allocate one more buffer, and then this won't happen again.
            allocateNewBuffer = true;
        }
    }

    if(allocateNewBuffer)
    {
        // Pick a buffer size by multiplying the base buffer size by the number of buffers already allocated for this thread.
        unsigned int sizeMultiplier = pThreadBufferList->GetCount() + 1;
        unsigned int baseBufferSize = 100 * 1024; // 100K
        unsigned int bufferSize = baseBufferSize * sizeMultiplier;

        // Make sure that buffer size >= request size so that the buffer size does not
        // determine the max event size.
        if(bufferSize < requestSize)
        {
            bufferSize = requestSize;
        }

        pNewBuffer = new EventPipeBuffer(bufferSize);
        m_sizeOfAllBuffers += bufferSize;
#ifdef _DEBUG
        m_numBuffersAllocated++;
#endif // _DEBUG
    }

    // Set the buffer on the thread.
    if(pNewBuffer != NULL)
    {
        pThreadBufferList->InsertTail(pNewBuffer);
        return pNewBuffer;
    }

    // TODO: If we steal a buffer from another thread, do we need to alert the file so that it can alert the reader?
    // TODO: Make sure that when we steal a buffer that we re-size as appropriate and don't just hand a big buffer to a thread that doesn't need it.

    return NULL;
}

EventPipeBufferList* EventPipeBufferManager::FindThreadToStealFrom()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    // Find the thread buffer list containing the buffer whose most recent event is the oldest as long as the buffer is not
    // the current buffer for the thread (e.g. it's next pointer is non-NULL).
    // This means that the thread must also have multiple buffers, so that we don't steal its only buffer.
    EventPipeBufferList *pOldestContainingList = NULL;

    SListElem<EventPipeBufferList*> *pElem = m_pPerThreadBufferList->GetHead();
    while(pElem != NULL)
    {
        EventPipeBufferList *pCandidate = pElem->GetValue();

        // The current candidate has more than one buffer (otherwise it is disqualified).
        if(pCandidate->GetHead()->GetNext() != NULL)
        {
            // If we haven't seen any candidates, this one automatically becomes the oldest candidate.
            if(pOldestContainingList == NULL)
            {
                pOldestContainingList = pCandidate;
            }
            // Otherwise, to replace the existing candidate, this candidate must have an older timestamp in its oldest buffer.
            else if((pOldestContainingList->GetHead()->GetMostRecentTimeStamp().QuadPart) > 
                      (pCandidate->GetHead()->GetMostRecentTimeStamp().QuadPart))
            {
                pOldestContainingList = pCandidate;
            }
        }

        pElem = m_pPerThreadBufferList->GetNext(pElem);
    }

    return pOldestContainingList;
}

bool EventPipeBufferManager::WriteEvent(Thread *pThread, EventPipeEvent &event, BYTE *pData, unsigned int length, Thread *pEventThread, StackContents *pStack)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        // The input thread must match the current thread because no lock is taken on the buffer.
        PRECONDITION(pThread == GetThread());
    }
    CONTRACTL_END;

    _ASSERTE(pThread == GetThread());

    // Check to see an event thread was specified.  If not, then use the current thread.
    if(pEventThread == NULL)
    {
        pEventThread = pThread;
    }

    // See if the thread already has a buffer to try.
    bool allocNewBuffer = false;
    EventPipeBuffer *pBuffer = NULL;
    EventPipeBufferList *pThreadBufferList = pThread->GetEventPipeBufferList();
    if(pThreadBufferList == NULL)
    {
        allocNewBuffer = true;
    }
    else
    {
        // The thread already has a buffer list.  Select the newest buffer and attempt to write into it.
        // TODO: Should we remove this indirection and just give the thread a direct pointer?
        pBuffer = pThreadBufferList->GetTail();
        if(pBuffer == NULL)
        {
            // This should never happen.  If the buffer list exists, it must contain at least one entry.
            _ASSERT(!"Thread buffer list with zero entries encountered.");
            return false;
        }
        else
        {
            // Attempt to write the event to the buffer.  If this fails, we should allocate a new buffer.
            allocNewBuffer = !pBuffer->WriteEvent(pEventThread, event, pData, length, pStack);
        }
    }

    // Check to see if we need to allocate a new buffer, and if so, do it here.
    if(allocNewBuffer)
    {
        unsigned int requestSize = sizeof(EventPipeEventInstance) + length;
        pBuffer = AllocateBufferForThread(pThread, requestSize);
    }

    // Try to write the event after we allocated (or stole) a buffer.
    // This is the first time if the thread had no buffers before the call to this function.
    // This is the second time if this thread did have one or more buffers, but they were full.
    if(allocNewBuffer && pBuffer != NULL)
    {
        allocNewBuffer = !pBuffer->WriteEvent(pEventThread, event, pData, length, pStack);
    }

    return !allocNewBuffer;
}

void EventPipeBufferManager::WriteAllBuffersToFile(EventPipeFile *pFile, LARGE_INTEGER stopTimeStamp)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pFile != NULL);
    }
    CONTRACTL_END;

#if 0
    // TODO: Will need to implement merge sort for proper ordering.
    SListElem<EventPipeBufferList*> *pElem = m_pPerThreadBufferList->GetHead();
    while(pElem != NULL)
    {
        EventPipeBufferList *pBufferList = pElem->GetValue();

        // Iterate through each of the buffers in the list.
        EventPipeBuffer *pBuffer = pBufferList->GetHead();
        while(pBuffer != NULL)
        {
            // Iterate though each of the events in the buffer.
            EventPipeEventInstance *pInstance = pBuffer->GetNext(NULL, stopTimeStamp);
            while(pInstance != NULL)
            {
                _ASSERTE(pInstance->EnsureConsistency());
                _ASSERTE(pInstance->GetEvent() != NULL);

                // Write the event.
                pFile->WriteEvent(*pInstance);

                // Get the next event.
                pInstance = pBuffer->GetNext(pInstance, stopTimeStamp);
            }
            // Get the next buffer.
            pBuffer = pBuffer->GetNext();
        }

        pElem = m_pPerThreadBufferList->GetNext(pElem);
    }
#endif

    // TODO: Better version of merge sort.
    // 1. Iterate through all of the threads, adding each buffer to a temporary list.
    // 2. While iterating, get the lowest most recent timestamp.  This is the timestamp that we want to process up to.
    // 3. Process up to the lowest most recent timestamp for the set of buffers.
    // 4. When we get NULLs from each of the buffers on PopNext(), we're done.
    // 5. While iterating if PopNext() == NULL && Empty() == NULL, remove the buffer from the list.  It's empty.
    // 6. While iterating, grab the next lowest most recent timestamp.
    // 7. Walk through the list again and look for any buffers that have a lower most recent timestamp than the next most recent timestamp.
    // 8. If we find one, add it to the list and select its most recent timestamp as the lowest.
    // 9. Process again (go to 3).
    // 10. Continue until there are no more buffers to process.

    // Niavely walk the circular buffer, writing the event stream in timestamp order.
    while(true)
    {
        EventPipeEventInstance *pOldestInstance = NULL;
        EventPipeBuffer *pOldestContainingBuffer = NULL;
        SListElem<EventPipeBufferList*> *pElem = m_pPerThreadBufferList->GetHead();
        while(pElem != NULL)
        {
            EventPipeBufferList *pBufferList = pElem->GetValue();

            // Peek the next event out of the list.
            EventPipeBuffer *pContainingBuffer = NULL;
            EventPipeEventInstance *pNext = pBufferList->PeekNextEvent(stopTimeStamp, &pContainingBuffer);
            if(pNext != NULL)
            {
                // If it's the oldest event we've seen, then save it.
                if((pOldestInstance == NULL) ||
                   (pOldestInstance->GetTimeStamp().QuadPart > pNext->GetTimeStamp().QuadPart)) 
                {
                    pOldestInstance = pNext;
                    pOldestContainingBuffer = pContainingBuffer;
                }
            }

            pElem = m_pPerThreadBufferList->GetNext(pElem);
        }

        if(pOldestInstance == NULL)
        {
            // We're done.  There are no more events.
            break;
        }

        // Write the oldest event.
        pFile->WriteEvent(*pOldestInstance);

        // Pop the event from the buffer.
        pOldestContainingBuffer->PopNext(stopTimeStamp);
    }
}

EventPipeBufferList::EventPipeBufferList()
{
    LIMITED_METHOD_CONTRACT;

    m_pHeadBuffer = NULL;
    m_pTailBuffer = NULL;
    m_bufferCount = 0;
    m_pReadBuffer = NULL;

#ifdef _DEBUG
    m_pCreatingThread = GetThread();
#endif // _DEBUG

   _ASSERTE(EnsureConsistency());
}

EventPipeBufferList::~EventPipeBufferList()
{
    LIMITED_METHOD_CONTRACT;

    // TODO: Should we free all of the buffers here?
}

EventPipeBuffer* EventPipeBufferList::GetHead()
{
    LIMITED_METHOD_CONTRACT;

    return m_pHeadBuffer;
}

EventPipeBuffer* EventPipeBufferList::GetTail()
{
    LIMITED_METHOD_CONTRACT;

    return m_pTailBuffer;
}

void EventPipeBufferList::InsertTail(EventPipeBuffer *pBuffer)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pBuffer != NULL);
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    // Ensure that the input buffer didn't come from another list that was improperly cleaned up.
    _ASSERTE((pBuffer->GetNext() == NULL) && (pBuffer->GetPrevious() == NULL));

    _ASSERTE(EnsureConsistency());

    // First node in the list.
    if(m_pTailBuffer == NULL)
    {
        m_pHeadBuffer = m_pTailBuffer = pBuffer;
    }
    else
    {
        // Set links between the old and new tail nodes.
        m_pTailBuffer->SetNext(pBuffer);
        pBuffer->SetPrevious(m_pTailBuffer);

        // Set the new tail node.
        m_pTailBuffer = pBuffer;
    }

    m_bufferCount++;

    _ASSERTE(EnsureConsistency());
}

EventPipeBuffer* EventPipeBufferList::GetAndRemoveHead()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    _ASSERTE(EnsureConsistency());

    EventPipeBuffer *pRetBuffer = NULL;
    if(m_pHeadBuffer != NULL)
    {
        // Save the head node.
        pRetBuffer = m_pHeadBuffer;

        // Set the new head node.
        m_pHeadBuffer = m_pHeadBuffer->GetNext();

        // Update the head node's previous pointer.
        if(m_pHeadBuffer != NULL)
        {
            m_pHeadBuffer->SetPrevious(NULL);
        }
        else
        {
            // We just removed the last buffer from the list.
            // Make sure both head and tail pointers are NULL.
            m_pTailBuffer = NULL;
        }

        // Clear the next pointer of the old head node.
        pRetBuffer->SetNext(NULL);

        // Ensure that the old head node has no dangling references.
        _ASSERTE((pRetBuffer->GetNext() == NULL) && (pRetBuffer->GetPrevious() == NULL));
    }

    m_bufferCount--;

    _ASSERTE(EnsureConsistency());

    return pRetBuffer;
}

unsigned int EventPipeBufferList::GetCount() const
{
    LIMITED_METHOD_CONTRACT;

    return m_bufferCount;
}

EventPipeEventInstance* EventPipeBufferList::PeekNextEvent(LARGE_INTEGER beforeTimeStamp, EventPipeBuffer **pContainingBuffer)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    // Get the current read buffer.
    // If it's not set, start with the head buffer.
    if(m_pReadBuffer == NULL)
    {
        m_pReadBuffer = m_pHeadBuffer;
    }

    // If the read buffer is still NULL, then this list contains no buffers.
    if(m_pReadBuffer == NULL)
    {
        return NULL;
    }

    // Get the next event in the buffer.
    EventPipeEventInstance *pNext = m_pReadBuffer->PeekNext(beforeTimeStamp);

    // If the next event is NULL, then go to the next buffer.
    if(pNext == NULL)
    {
        m_pReadBuffer = m_pReadBuffer->GetNext();
        if(m_pReadBuffer != NULL)
        {
            pNext = m_pReadBuffer->PeekNext(beforeTimeStamp);
        }
    }

    // Set the containing buffer.
    if(pNext != NULL && pContainingBuffer != NULL)
    {
        *pContainingBuffer = m_pReadBuffer;
    }

    // Make sure pContainingBuffer is properly set.
    _ASSERTE((pNext == NULL) || (pNext != NULL && pContainingBuffer == NULL) || (pNext != NULL && *pContainingBuffer == m_pReadBuffer));
    return pNext;
}

EventPipeEventInstance* EventPipeBufferList::PopNextEvent(LARGE_INTEGER beforeTimeStamp)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    // Get the next event.
    EventPipeBuffer *pContainingBuffer = NULL;
    EventPipeEventInstance *pNext = PeekNextEvent(beforeTimeStamp, &pContainingBuffer);

    // If the event is non-NULL, pop it.
    if(pNext != NULL && pContainingBuffer != NULL)
    {
        pContainingBuffer->PopNext(beforeTimeStamp);
    }

    return pNext;

}

#ifdef _DEBUG
Thread* EventPipeBufferList::GetThread()
{
    LIMITED_METHOD_CONTRACT;

    return m_pCreatingThread;
}

bool EventPipeBufferList::EnsureConsistency()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        // TODO: Assert that we are holding the lock.
    }
    CONTRACTL_END;

    // Either the head and tail nodes are both NULL or both are non-NULL.
    _ASSERTE((m_pHeadBuffer == NULL && m_pTailBuffer == NULL) || (m_pHeadBuffer != NULL && m_pTailBuffer != NULL));

    // If the list is NULL, check the count and return.
    if(m_pHeadBuffer == NULL)
    {
        _ASSERTE(m_bufferCount == 0);
        return true;
    }

    // If the list is non-NULL, walk the list forward until we get to the end.
    unsigned int nodeCount = (m_pHeadBuffer != NULL) ? 1 : 0;
    EventPipeBuffer *pIter = m_pHeadBuffer;
    while(pIter->GetNext() != NULL)
    {
        pIter = pIter->GetNext();
        nodeCount++;

        // Check for cycles.
        _ASSERTE(nodeCount <= m_bufferCount);
    }

    // When we're done with the walk, pIter must point to the tail node.
    _ASSERTE(pIter == m_pTailBuffer);

    // Node count must equal the buffer count.
    _ASSERTE(nodeCount == m_bufferCount);

    // Now, walk the list in reverse.
    pIter = m_pTailBuffer;
    nodeCount = (m_pTailBuffer != NULL) ? 1 : 0;
    while(pIter->GetPrevious() != NULL)
    {
        pIter = pIter->GetPrevious();
        nodeCount++;

        // Check for cycles.
        _ASSERTE(nodeCount <= m_bufferCount);
    }

    // When we're done with the reverse walk, pIter must point to the head node.
    _ASSERTE(pIter == m_pHeadBuffer);

    // Node count must equal the buffer count.
    _ASSERTE(nodeCount == m_bufferCount);

    // We're done.
    return true;
}
#endif // _DEBUG

#endif // FEATURE_PERFTRACING
