// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _STRINGHASHTABLE_H_
#define _STRINGHASHTABLE_H_
#include "common.h"


class StringDupsList
{
private:
	uint8_t** buf = NULL;
	size_t capacity = 0;
	size_t write_pos = 0;

    bool EnsureCapacity()
    {
        if (write_pos < capacity)
            return true;

        size_t new_size = capacity == 0 ? 4 : (capacity * 2);
        uint8_t** new_buf = new (nothrow) uint8_t*[new_size];
        if (new_buf == NULL)
            return false;

        // static_assert(std::is_trivially_copyable<T>::value, "memcpy here");
        //memcpy(new_buf, buf, capacity * sizeof(T));
        uint8_t** from = buf;
        uint8_t** to = new_buf;
        size_t elemsCount = capacity;
        memcpy(to, from, elemsCount * sizeof(uint8_t*));

        delete[] buf;
        buf = new_buf;
        capacity = new_size;
        return true;
    }
public:
    ~StringDupsList()
    {
	    delete[] buf;
    }

    size_t Size() const 
    { 
        return write_pos; 
    } 

    bool StringDupsList::Write(uint8_t*& item)
    {
	    if (!EnsureCapacity())
		    return false;

        buf[write_pos] = item;
        write_pos++;
        return true;
    }

    uint8_t** Buf() const
    {
        return buf;
    }
};

typedef void* AllocationHeap;

class GCStringData
{
private:
    uint8_t* address;
    wchar_t* string;
    uint32_t count;

public:
    GCStringData() : address(NULL), count(0), string(NULL) {};
    GCStringData(uint8_t* address, uint32_t count, wchar_t* string) : count(0)
    {
        SetAddress(address);
        SetStringBuffer(string);
        SetCharCount(count);
    };
    inline uint8_t* GetAddress() const
    {
        return address;
    }
    inline void SetAddress(uint8_t* _address)
    {
        address = _address;
    }
    inline uint32_t GetCharCount() const
    {
        return count;
    }
    inline void SetCharCount(uint32_t _count)
    {
        count = _count;
    }
    inline wchar_t* GetStringBuffer() const
    {
        return string; 
    }
    inline void SetStringBuffer(wchar_t* _string)
    {
        string = _string;
    }
};

struct GCHashEntry
{
    GCHashEntry* Next;
    uint32_t HashValue;
    StringDupsList* Data;
    GCStringData Key;
};

struct GCHashTableIteration
{
    uint32_t Bucket;
    uint32_t DupIndex;
    uint32_t Limit;
    GCHashEntry* Entry;
};

class GCHashTableBase
{
friend class StringDedup;
public:
    bool Init(uint32_t numBuckets, AllocationHeap heap);
    
    StringDupsList** InsertOrGetValue(GCStringData* key, uint32_t hash);

    uint32_t GetHash(GCStringData* key);
    uint32_t GetCount();
    void Destroy();
    bool Dequeue(GCHashTableIteration* iter);

protected:
    bool GrowHashTable();
    static GCHashEntry* AllocateEntry(GCStringData* key);
    static void DeleteEntry(GCHashEntry* entry);
    static void DeleteEntryData(GCHashEntry* entry);
    static bool CompareKeys(GCHashEntry* entry, GCStringData* key);

    GCHashEntry** buckets;    // Pointer to first entry for each bucket  
    uint32_t numBuckets;    
    uint32_t numEntries;
    AllocationHeap heap;
};

class GCUnicodeStringHashTable : public GCHashTableBase
{
public:
    GCUnicodeStringHashTable()
    {
        this->buckets = NULL;
        this->numBuckets = 0;
        this->numEntries = 0;
    }

    ~GCUnicodeStringHashTable()
    {
        this->Destroy();
    }
};

#endif _STRINGHASHTABLE_H_
