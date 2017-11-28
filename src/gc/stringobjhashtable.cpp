// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "stringobjhashtable.h"
#include "gcenv.h"
#include "gcenv.inl"


bool GCHashTableBase::Init(uint32_t numBuckets, AllocationHeap heap)
{
    buckets = new (nothrow) GCHashEntry*[numBuckets]();

    if (!buckets)
    {
        return false;
    }

    this->numBuckets = numBuckets;
    this->heap = heap;
    
    return true;
}

StringDupsList** GCHashTableBase::InsertOrGetValue(GCStringData* key, uint32_t hash)
{
    assert(numBuckets != 0);

    uint32_t bucket = hash % numBuckets;
    GCHashEntry* search;

    for (search = buckets[bucket]; search; search = search->Next)
    {
        if (search->HashValue == hash && CompareKeys(search, key))
        {
            // why one would insert same address twice?
            assert(search->Key.GetAddress() != key->GetAddress());
            // key already holds "original" pointer
            // we are saving space here and allocating dups list only when first duplicate appears
            // StringDedupTable::Insert need to be aware of such behavior
            if (!search->Data)
            {
                StringDupsList* list = new (nothrow) StringDupsList;
                if (!list)
                {
                    return NULL;
                }
                search->Data = list;
            }  
            return &(search->Data);
        }
    }

    if  (numEntries > numBuckets*2)
    {
        if (!GrowHashTable())
        {
            return NULL;
        }
        bucket = hash % numBuckets;
    }

    GCHashEntry* newEntry = AllocateEntry(key);
    if (!newEntry)
    {
        return NULL;
    }

    newEntry->Next = buckets[bucket];
    newEntry->Data = NULL;
    newEntry->HashValue = hash;

    buckets[bucket] = newEntry;

    numEntries++;

    return &(newEntry->Data);
}

uint32_t GCHashTableBase::GetHash(GCStringData* key)
{
    uint32_t hash = 5381;
    uint8_t const *data = (const uint8_t *)key->GetStringBuffer();
    size_t iSize = key->GetCharCount()*sizeof(TCHAR);
    uint8_t const *dataEnd = data + iSize;

    for (/**/ ; data < dataEnd; data++)
    {
        hash = ((hash << 5) + hash) ^ *data;
    }
    return hash;
}

uint32_t GCHashTableBase::GetCount()
{
    return numEntries;
}

void GCHashTableBase::Destroy()
{
    if (buckets != NULL)
    {
        uint32_t i;

        for (i = 0; i < numBuckets; i++)
        {
            GCHashEntry* entry,* Next;

            for (entry = buckets[i]; entry != NULL; entry = Next)
            {
                Next = entry->Next;
                DeleteEntry(entry);
            }
        }

        delete[] buckets;

		buckets = NULL;
    }
}

bool GCHashTableBase::Dequeue(GCHashTableIteration* iter)
{
    assert(iter->Limit <= numBuckets);
    // If we haven't started iterating yet, or if we are at the end of a particular
    // chain, advance to the next chain.
probe:
    GCHashEntry* memEntry = iter->Entry;
    while (iter->Entry == NULL || iter->Entry->Next == NULL)
    {
        if (++iter->Bucket == iter->Limit)
        {
            return false;
        }
        iter->Entry = buckets[iter->Bucket];

        // If this bucket has no chain, keep advancing.  Otherwise we are done
        if (iter->Entry)
        {
            // Cleaning last entry in previous chain
            // All entries other than last we've cleaned in out-of-loop path
            while (iter->Entry && !iter->Entry->Data)
            {
                // if no duplicates for iter->Entry->Key were found
                // we just silently skip such entry
                // todo: introduce eviction policy
                iter->Entry = iter->Entry->Next;
            }
            if (iter->Entry && iter->Entry->Data)
            {
                if (memEntry)
                {
                    DeleteEntryData(memEntry);
                }
                return true;
            }
        }
    }

    // We are within a chain. Advance to the next entry. Clean previous entry. 
    // Last entry in the chain will be cleared in loop above while seeking next chain
    // todo: introduce eviction policy
    DeleteEntryData(memEntry);
    iter->Entry = iter->Entry->Next;
    if (!iter->Entry->Data)
    {
        goto probe;
    }

    assert(iter->Entry);
    return true;
}

bool GCHashTableBase::GrowHashTable()
{
    uint32_t newNumBuckets = numBuckets * 4;

    GCHashEntry** newBuckets = new (nothrow) GCHashEntry*[newNumBuckets]();

    if (!newBuckets)
    {
        return false;
    }

    for (uint32_t i = 0; i < numBuckets; i++)
    {
        GCHashEntry* entry = buckets[i];
        while (entry != NULL)
        {
            uint32_t newBucket = entry->HashValue % newNumBuckets;
            GCHashEntry* nextEntry = entry->Next;
            entry->Next = newBuckets[newBucket];
            newBuckets[newBucket] = entry;
            entry = nextEntry;
        }
    }

    // Add old table to the to free list. Note that the SyncClean thing will only 
    // delete the buckets at a safe point
    //
    //todo SyncClean::AddEEHashTable(buckets);
    delete[] buckets;

	buckets = newBuckets;
	numBuckets = newNumBuckets;

    return true;
}

GCHashEntry* GCHashTableBase::AllocateEntry(GCStringData* key)
{
    GCHashEntry* entry = new (nothrow) GCHashEntry;
    if (entry) 
    {
        GCStringData* entryKey = &entry->Key;
        entryKey->SetAddress (key->GetAddress());
        entryKey->SetCharCount (key->GetCharCount());
        entryKey->SetStringBuffer (key->GetStringBuffer());
    }

    return entry;
}

void GCHashTableBase::DeleteEntry(GCHashEntry* entry)
{
    if (entry)
    {
        delete entry->Data;
        delete entry;
    }
}

void GCHashTableBase::DeleteEntryData(GCHashEntry* entry)
{
    if (entry)
    {
        delete entry->Data;
        entry->Data = NULL;
    }
}

bool GCHashTableBase::CompareKeys(GCHashEntry* entry, GCStringData* key)
{
    GCStringData* entryKey = &entry->Key;

    if (entryKey->GetCharCount() != key->GetCharCount())
        return false;

    return !memcmp(entryKey->GetStringBuffer(), key->GetStringBuffer(), entryKey->GetCharCount() * sizeof(TCHAR));
}
