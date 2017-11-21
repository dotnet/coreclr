// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "stringdedupqueue.h"


StringDedupQueue::StringDedupQueue()
{
}

StringDedupQueue::~StringDedupQueue()
{
	delete[] buf;
}

size_t StringDedupQueue::Size() const 
{ 
    return (write_pos - read_pos); 
} 

bool StringDedupQueue::Write(uint8_t*& item)
{
	if (!EnsureCapacity())
		return false;

    buf[write_pos & (capacity - 1)] = item;
    write_pos++;
    return true;
}

bool StringDedupQueue::Read(uint8_t*& item)
{
	if (Size() == 0)
		return false;

    item = buf[read_pos & (capacity - 1)];
    read_pos++;
    return true;
}

bool StringDedupQueue::EnsureCapacity()
{

    if (Size() < capacity)
        return true;

    size_t new_size = capacity == 0 ? 4 : (capacity * 2);
    uint8_t** new_buf = new (nothrow) uint8_t*[new_size];
    if (new_buf == NULL)
        return false;

    // static_assert(std::is_trivially_copyable<T>::value, "memcpy here");
    //memcpy(new_buf, buf, capacity * sizeof(T));
    uint8_t** from = buf + (write_pos & (capacity - 1));
    uint8_t** to = new_buf;
    size_t elemsCount = capacity - (write_pos & (capacity - 1));
    memcpy(to, from, elemsCount * sizeof(uint8_t*));
    from = buf;
    to = to + elemsCount;
    elemsCount = write_pos & (capacity - 1);
    memcpy(to, from, elemsCount * sizeof(uint8_t*));

    delete[] buf;
    buf = new_buf;
    capacity = new_size;
    write_pos = Size();
    read_pos = 0;
    return true;
}
