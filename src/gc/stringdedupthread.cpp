// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "stringdedupthread.h"

StringDedupThread::StringDedupThread(
    StringDedupTable* table,
    StringDedupQueue* queues
#ifdef MULTIPLE_HEAPS
    , size_t nqueues
#endif 
)
{
    this->table = table;
#ifdef MULTIPLE_HEAPS
    this->nqueues = nqueues;
    this->queues = queues;
#else
    this->queue = queues;
#endif //MULTIPLE_HEAPS
}

StringDedupThread::~StringDedupThread()
{
}

void StringDedupThread::Run() const 
{
#ifdef MULTIPLE_HEAPS
#else

    uint8_t* item = NULL;
    while (queue->Read(item))
    {
        // item might be nulled in sweeping gc cycle
        // if string it references to appeared not marked
        // mutator can allocate in sweeped space
        // concurently with dedup thread
        // and we will read garbage instead of string len and chars
        if (item)
        {
            table->Insert(item);
        }
    }

#endif //MULTIPLE_HEAPS
} 
