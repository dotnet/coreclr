// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "stringdedup.h"
#include "stringdedupqueue.h"
#include "stringdedupthread.h"
#include "stringdeduptable.h"
#include "gcenv.h"

    StringDedupThread* StringDedup::thread;   
    StringDedupTable* StringDedup::table;   
#ifdef MULTIPLE_HEAPS
    size_t StringDedup::nqueues;
    StringDedupQueue* StringDedup::queues;    
    size_t* StringDedup::mem_write_sizes;
    GCHashTableIteration* StringDedup::iters;
#else
    StringDedupQueue* StringDedup::queue; 
    size_t StringDedup::mem_write_size;
    GCHashTableIteration StringDedup::iter;
#endif //MULTIPLE_HEAPS

bool StringDedup::Init(
    size_t number_of_heaps
)
{
    table = new (nothrow) StringDedupTable;
    bool table_online = table->Init();
    if (!table_online)
    {
        return false;
    }
#ifdef MULTIPLE_HEAPS
    nqueues = number_of_heaps;
    queues = new (nothrow) StringDedupQueue [number_of_heaps];
    thread = new (nothrow) StringDedupThread(table, queues, number_of_heaps);
    iters = new (nothrow) GCHashTableIteration [number_of_heaps];
    if (!queues || !thread || !iters)
    {
        return false;
    }
#else
    queue = new (nothrow) StringDedupQueue;
    thread = new (nothrow) StringDedupThread(table, queue);
    if (!queue || !thread)
    {
        return false;
    }
#endif // MULTIPLE_HEAPS
    return true;
}

void StringDedup::Rewind()
{
    // when we going mark_phase -> sweep_phase
    // cycle is regular such as
    // GcFinished -> GcStarted -> GcFinished
    // however when we going mark_phase -> relocate_phase
    // the cycle is as follows:
    // GcFinished -> GcStarted -> Rewind -> GcFinished
    // and we are enqueueing each pointer twice
    // and to prevent growing queue twice as well
    // we're remembering original (logical) write positions
    // as of instructing `mem_write_size = queue->Size();`
    // from `GcStarted()` in mark_phase
    // and then restoring it calling `Rewind()` in relocate_phase
    // by instructing `queue->write_pos = (queue->read_pos + mem_write_size) & (queue->capacity - 1);`
#ifdef MULTIPLE_HEAPS
    for (size_t i = 0; i < nqueues; i++)
    {
        queues[i]->write_pos = (queues[i]->read_pos + mem_write_sizes[i]) & (queues[i]->capacity - 1);
    }
#else
    queue->write_pos = (queue->read_pos + mem_write_size) & (queue->capacity - 1);
#endif // MULTIPLE_HEAPS
}

bool StringDedup::AdvanceDups(int thread)
{
#ifdef MULTIPLE_HEAPS
#else
    iter.DupIndex = -1;
    return table->ht->Dequeue(&iter);
#endif // MULTIPLE_HEAPS
}

uint8_t* StringDedup::DequeueDup(int thread)
{
#ifdef MULTIPLE_HEAPS
#else
    if (++iter.DupIndex == 0)
    {
        return iter.Entry->Key.GetAddress();
    }
    if (iter.DupIndex - 1 < iter.Entry->Data->Size())
    {
        return iter.Entry->Data->Buf()[iter.DupIndex - 1];
    }
    return NULL;
#endif // MULTIPLE_HEAPS
}

uint32_t StringDedup::CurrentDupsStringLength()
{
#ifdef MULTIPLE_HEAPS
#else    
    return iter.Entry->Key.GetCharCount();
#endif // MULTIPLE_HEAPS
}

TCHAR* StringDedup::CurrentDupsStringBuf()
{
#ifdef MULTIPLE_HEAPS
#else    
    return iter.Entry->Key.GetStringBuffer();
#endif // MULTIPLE_HEAPS
}

void StringDedup::ResetDupsKey(uint8_t* new_original, int thread)
{
    StringObject* stringref = (StringObject*)new_original;
#ifdef MULTIPLE_HEAPS
#else    
    iter.Entry->Key.SetAddress (new_original);
    iter.Entry->Key.SetCharCount (stringref->GetStringLength());
    iter.Entry->Key.SetStringBuffer (stringref->GetBuffer());
#endif // MULTIPLE_HEAPS
}

void StringDedup::GcStarted()
{
#ifdef MULTIPLE_HEAPS
    size_t table_fraction = table->ht->numBuckets / nqueues;
    for (size_t i = 0; i < nqueues; i++)
    {
        mem_write_sizes[i] = queues[i]->Size();
        iters[i].Bucket = i * table_fraction - 1;
        iters[i].DupIndex = -1;
        iters[i].Entry = NULL;
        iters[i].Limit = (i+1) * table_fraction;
    }
#else
    mem_write_size = queue->Size();
    iter.Bucket = -1;
    iter.DupIndex = -1;
    iter.Entry = NULL;
    iter.Limit = table->ht->numBuckets;

#endif // MULTIPLE_HEAPS
}

void StringDedup::GcFinished()
{
    thread->Run();
}

void StringDedup::EnqPromoted(uint8_t* str
    , int thread
)
{
#ifdef MULTIPLE_HEAPS
    (queues[thread])->Write(str);
#else
    queue->Write(str);
#endif //MULTIPLE_HEAPS
}
