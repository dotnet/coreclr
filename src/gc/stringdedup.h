// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !defined(_STRINGDEDUP_H_)
#define _STRINGDEDUP_H_
#include "common.h"
#include "stringdedupqueue.h"
#include "stringdedupthread.h"
#include "stringdeduptable.h"
#include "stringobjhashtable.h"

class StringDedup
{
public:
    static bool Init(size_t number_of_heaps);
    static void EnqPromoted(uint8_t* str, int thread);
    static void Rewind();
    static void GcStarted();
    static void GcFinished();
    
    static bool AdvanceDups(int thread);
    static uint8_t* DequeueDup(int thread);
    static void ResetDupsKey(uint8_t* new_original, int thread);
    static uint32_t CurrentDupsStringLength();
    static WCHAR* CurrentDupsStringBuf();
private:
    static StringDedupThread* thread;
    static StringDedupTable* table;
#ifdef MULTIPLE_HEAPS
    static size_t nqueues;
    static StringDedupQueue* queues;
    static size_t* mem_write_sizes;
    static GCHashTableIteration* iters;
#else
    static StringDedupQueue* queue;
    static size_t mem_write_size;
    static GCHashTableIteration iter;
#endif //MULTIPLE_HEAPS
};

#endif // !defined(_STRINGDEDUP_H_)
