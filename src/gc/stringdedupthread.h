// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _STRINGDEDUPTHREAD_H_
#define _STRINGDEDUPTHREAD_H_
#include "common.h"
#include "stringdedupqueue.h"
#include "stringdeduptable.h"

class StringDedupThread
{
friend class StringDedup;
public:
	StringDedupThread(
        StringDedupTable* table,
        StringDedupQueue* queues
#ifdef MULTIPLE_HEAPS
    , size_t nqueues
#endif 
    );
	~StringDedupThread();
	void Run() const;
private:
    StringDedupTable* table;
#ifdef MULTIPLE_HEAPS
    size_t nqueues;
    StringDedupQueue* queues;
#else
    StringDedupQueue* queue;
#endif //MULTIPLE_HEAPS
};

#endif _STRINGDEDUPTHREAD_H_
