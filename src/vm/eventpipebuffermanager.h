// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_BUFFERMANAGER_H__
#define __EVENTPIPE_BUFFERMANAGER_H__

#ifdef FEATURE_PERFTRACING

#include "eventpipefile.h"
#include "eventpipebuffer.h"
#include "spinlock.h"

class EventPipeBufferList;

class EventPipeBufferManager
{
private:

    // A list of linked-lists of buffer objects.
    // Each entry in this list represents a set of buffers owned by a single thread.
    // The actual Thread object has a pointer to the object contained in this list.  This ensures that
    // each thread can access its own list, while at the same time, ensuring that when
    // a thread is destroyed, we keep the buffers around without having to perform any
    // migration or book-keeping.
    SList<SListElem<EventPipeBufferList*>> *m_pPerThreadBufferList;

    // The total allocation size of buffers under management.
    size_t m_sizeOfAllBuffers;

    // Lock to protect access to the per-thread buffer list and total allocation size.
    SpinLock m_lock;

    // Add a buffer to the thread buffer list.
    void AddBufferToThreadBufferList(EventPipeBufferList *pThreadBuffers, EventPipeBuffer *pBuffer);

    // Find the thread that owns the oldest buffer that is eligible to be stolen.
    EventPipeBufferList* FindThreadToStealFrom();

public:

    EventPipeBufferManager();

    // Allocate a new buffer for the specified thread.
    // Returns:
    //  - true: The buffer allocation succeeded and the buffer can be retrieved from the thread.
    //  - false: Unable to allocate a new buffer.
    bool AllocateBufferForThread(Thread *pThread);

    // Write the contents of the managed buffers to the specified file.
    void WriteAllBuffersToFile(EventPipeFile *pFile);
};

// Represents a list of buffers associated with a specific thread.
class EventPipeBufferList
{
private:

    // Buffers are stored in an intrusive linked-list from oldest to newest.
    // Head is the oldest buffer.  Tail is the newest (and currently used) buffer.
    EventPipeBuffer *m_pHeadBuffer;
    EventPipeBuffer *m_pTailBuffer;

#ifdef _DEBUG
    // For diagnostics, keep the thread pointer.
    Thread *m_pCreatingThread;
#endif // _DEBUG

public:

    EventPipeBufferList();
    ~EventPipeBufferList();

    // Get the head node of the list.
    EventPipeBuffer* GetHead();

    // Get the tail node of the list.
    EventPipeBuffer* GetTail();

    // Insert a new buffer at the tail of the list.
    void InsertTail(EventPipeBuffer *pBuffer);

    // Remove the head node of the list.
    EventPipeBuffer* GetAndRemoveHead();

#ifdef _DEBUG
    // Get the thread associated with this list.
    Thread* GetThread();

    // Validate the consistency of the list.
    // This function will assert if the list is in an inconsistent state.
    bool EnsureConsistency();
#endif // _DEBUG
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_BUFFERMANAGER_H__
