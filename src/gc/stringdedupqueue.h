// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _STRINGDEDUPQUEUE_H_
#define _STRINGDEDUPQUEUE_H_
#include "common.h"

class StringDedupQueue
{
friend class StringDedup;
public:
	StringDedupQueue();
	~StringDedupQueue();
	bool Write(uint8_t*& item);
	bool Read(uint8_t*& item);
    size_t Size() const;
private:
	uint8_t** buf = NULL;
	size_t capacity = 0;
	size_t read_pos = 0;
	size_t write_pos = 0;
	bool EnsureCapacity();
};

#endif // _STRINGDEDUPQUEUE_H_
