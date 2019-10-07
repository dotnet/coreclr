// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: castcache.h
//

#include "common.h"
#include "castcache.h"

#if !defined(DACCESS_COMPILE) && !defined(CROSSGEN_COMPILE)

OBJECTHANDLE CastCache::s_cache = NULL;

BASEARRAYREF CastCache::CreateCastCache(DWORD size)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    // size must be positive
    _ASSERTE(size > 0);
    // size must be a power of two
    _ASSERTE((size & (size - 1)) == 0);

    BASEARRAYREF table = NULL;

    try
    {
        table = (BASEARRAYREF)AllocatePrimitiveArray(CorElementType::ELEMENT_TYPE_I8, (size + 1) * sizeof(CastCacheEntry) / sizeof(INT64));
    }
    catch (OutOfMemoryException)
    {
        // try a small cache
        size = INITIAL_CACHE_SIZE;
        try
        {
            table = (BASEARRAYREF)AllocatePrimitiveArray(CorElementType::ELEMENT_TYPE_I8, (size + 1) * sizeof(CastCacheEntry) / sizeof(INT64));
        }
        catch (OutOfMemoryException)
        {
            // OK, no cache then
            return NULL;
        }
    }

    TableMask(table) = size - 1;

    // Fibonacci hash reduces the value into desired range by shifting right by the number of leading zeroes in 'size-1' 
    DWORD bitCnt;
#if BIT64
    BitScanReverse64(&bitCnt, size - 1);
    HashShift(table) = (BYTE)(63 - bitCnt);
#else
    BitScanReverse(&bitCnt, size - 1);
    HashShift(table) = (BYTE)(31 - bitCnt);
#endif

    return table;
}

BOOL CastCache::MaybeReplaceCacheWithLarger(DWORD size)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    BASEARRAYREF newTable = CreateCastCache(size);
    if (!newTable)
    {
        return FALSE;
    }

    OBJECTREF currentTableRef = ObjectFromHandle(s_cache);
    OBJECTREF prevTableRef = (OBJECTREF)(Object*)InterlockedCompareExchangeObjectInHandle(s_cache, newTable, currentTableRef);

    return prevTableRef == currentTableRef;
}

void CastCache::FlushCurrentCache()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    BASEARRAYREF currentTableRef = (BASEARRAYREF)ObjectFromHandle(s_cache);
    int size = !currentTableRef ? INITIAL_CACHE_SIZE : CacheElementCount(currentTableRef);

    BASEARRAYREF newTable = CreateCastCache(size);
    StoreObjectInHandle(s_cache, newTable);
}

void CastCache::Initialize()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    s_cache = CreateGlobalHandle(NULL);
}

TypeHandle::CastResult CastCache::TryGet(TADDR source, TADDR target)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    BASEARRAYREF table = (BASEARRAYREF)ObjectFromHandle(s_cache);

    // we use NULL as a sentinel for a rare case when a table could not be allocated
    // because we avoid OOMs in conversions
    // we could use 0-element table instead, but then we would have to check the size here.
    if (!table)
    {
        return TypeHandle::MaybeCast;
    }

    DWORD index = KeyToBucket(table, source, target);
    CastCacheEntry* pEntry = &Elements(table)[index];

    for (DWORD i = 0; i < BUCKET_SIZE; i++)
    {
        // must read in this order: version1 -> entry parts -> version2
        // because writers change them in the opposite order
        DWORD version1 = VolatileLoad(&pEntry->version1);
        TADDR entrySource = pEntry->source;
        TADDR entryTargetAndResult = VolatileLoad(&pEntry->targetAndResult);

        if (entrySource == source)
        {
            // target never has its lower bit set.
            // a matching entryTargetAndResult would have same bits, except for the lowest one, which is the result.
            entryTargetAndResult ^= target;
            if (entryTargetAndResult <= 1)
            {
                DWORD version2 = pEntry->version2;
                if (version2 != version1)
                {
                    // oh, so close, someone stomped over the entry while we were reading.
                    // treat it as a miss.
                    break;
                }

                return TypeHandle::CastResult(entryTargetAndResult);
            }
        }

        if (version1 == 0)
        {
            // the rest of the bucket is unclaimed, no point to search further
            break;
        }

        // quadratic reprobe
        index += i;
        pEntry = &Elements(table)[index & TableMask(table)];
    }

    return TypeHandle::MaybeCast;
}

void CastCache::TrySet(TADDR source, TADDR target, BOOL result)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    DWORD bucket;
    BASEARRAYREF table;

    do
    {
        table = (BASEARRAYREF)ObjectFromHandle(s_cache);
        if (!table)
        {
            // we did not allocate a table, it is very rare, try flushing, but do not continue looping.
            FlushCurrentCache();
            return;
        }

        bucket = KeyToBucket(table, source, target);
        DWORD index = bucket;
        CastCacheEntry* pEntry = &Elements(table)[index];

        for (DWORD i = 0; i < BUCKET_SIZE; i++)
        {
            // claim the entry if unused
            DWORD version1 = pEntry->version1;
            if (version1 == 0)
            {
                DWORD version2 = InterlockedCompareExchangeT(&pEntry->version2, version1 + 1, version1);
                if (version2 == version1)
                {
                    pEntry->SetEntry(source, target, result);

                    // entry is in inconsistent state and cannot be read or written to until we 
                    // update the version, which is the last thing we do here
                    VolatileStore(&pEntry->version1, version1 + 1);
                    return;
                }
                // someone snatched the entry. try the next one in the bucket.
            }

            if (pEntry->Source() == source && pEntry->Target() == target)
            {
                // looks like we already have an entry for this. 
                // duplicate entries are harmless, but a bit of a waste.
                return;
            }

            // quadratic reprobe
            index += i;
            pEntry = &Elements(table)[index & TableMask(table)];
        }

        // bucket is full.
    } while (TryGrow(table));

    // pick a victim somewhat randomly within a bucket 
    // NB: ++ is not interlocked. We are ok if we lose counts here. It is just a number that changes.
    DWORD victim = VictimCounter(table)++ & (BUCKET_SIZE - 1);
    // position the victim in a quadratic reprobe bucket
    victim = (victim * victim + victim) / 2;

    {
        CastCacheEntry* pEntry = &Elements(table)[(bucket + victim) & TableMask(table)];

        DWORD version2 = pEntry->version1;
        if (version2 == MAXDWORD)
        {
            // It is unlikely for a reader to sit between versions while exactly 2^32 updates happens.
            // Anyways, to not bother about the possibility, lets get a new cache. It will not happen often, if ever.
            FlushCurrentCache();
            return;
        }

        DWORD version1 = InterlockedCompareExchangeT(&pEntry->version2, version2 + 1, version2);

        if (version1 == version2)
        {
            pEntry->SetEntry(source, target, result);
            VolatileStore(&pEntry->version1, version2 + 1);
        }
    }
}

#endif // !DACCESS_COMPILE && !CROSSGEN_COMPILE
