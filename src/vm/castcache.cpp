// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: castcache.h
//

#include "common.h"
#include "castcache.h"

CastCache* CastCache::s_cache = NULL;

// this is only called on GC or during shutdown (see: SyncClean::CleanUp).
CastCache::~CastCache()
{
    LIMITED_METHOD_CONTRACT;
    delete[] m_Table;
}

CastCache* CastCache::MaybeReplaceCacheWithLarger(CastCache* currentCache)
{
    WRAPPER_NO_CONTRACT;

    if (currentCache == NULL)
    {
        CastCache* newCache = new (nothrow) CastCache(INITIAL_CACHE_SIZE);
        if (newCache != NULL)
        {
            s_cache = newCache;
        }

        return newCache;
    }

    CastCache* newCache = new (nothrow) CastCache(currentCache->Size() * 2);
    if (newCache == NULL)
        return FALSE; // oh well, continue using the old cache.

    CastCache* prevCache = FastInterlockCompareExchangePointer(&s_cache, newCache, currentCache);

    if (prevCache == currentCache)
    {
        SyncClean::AddObsoleteCastCache(prevCache);
        return newCache;
    }
    else
    {
        SyncClean::AddObsoleteCastCache(newCache);
        return prevCache;
    }
}

void CastCache::FlushCurrentCache()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    CastCache* oldCache = s_cache;
    int newSize = oldCache == NULL ? INITIAL_CACHE_SIZE : oldCache->Size();

    CastCache* newCache = new (nothrow) CastCache(newSize);
    if (newCache == NULL)
        newCache = new (nothrow) CastCache(INITIAL_CACHE_SIZE);

    CastCache* obsoleteCache = FastInterlockExchangePointer(&s_cache, newCache);

    if (obsoleteCache != NULL)
    {
        SyncClean::AddObsoleteCastCache(obsoleteCache);
    }
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

    DWORD index = KeyToBucket(source, target);
    CastCacheEntry* pEntry = &m_Table[index];

    for (DWORD i = 0; i < BUCKET_SIZE; i++)
    {
        // must read in this order: version1 -> entry parts -> version2
        // because writers change them in opposite order
        DWORD version1 = VolatileLoad(&pEntry->version1);

#if defined(_ARM64_) && defined(__GNUC__)
        // VolatileLoad is a half barrier, use that
        TADDR entrySource = VolatileLoad(&pEntry->source);
        TADDR entryTargetAndResult = VolatileLoad(&pEntry->targetAndResult);
#else
        // VolatileLoad is a full barrier (or no barrier on x64), just use one at the end.
        TADDR entrySource = VolatileLoadWithoutBarrier(&pEntry->source);
        TADDR entryTargetAndResult = VolatileLoadWithoutBarrier(&pEntry->targetAndResult);
        VOLATILE_MEMORY_BARRIER();
#endif

        if (entrySource == source &&
            (entryTargetAndResult & ~(TADDR)1) == target)
        {
            DWORD version2 = pEntry->version2;
            if (version2 != version1)
            {
                // oh, so close, someone stomped over the entry while we were reading.
                // treat it as a miss.
                break;
            }

            return TypeHandle::CastResult(entryTargetAndResult & 1);
        }

        if (version1 == 0)
        {
            // the rest of the bucket is unclaimed, no point to search further
            break;
        }

        // quadratic reprobe
        index += i;
        pEntry = &m_Table[index & m_TableMask];
    }

    return TypeHandle::MaybeCast;
}

void CastCache::TrySet(TADDR source, TADDR target, BOOL result, BOOL noGC)
{
    WRAPPER_NO_CONTRACT;

    if (s_cache == NULL && !noGC)
        TryGrow();

    DWORD bucket;
    CastCache* currentCache;

    do
    {
        currentCache = VolatileLoadWithoutBarrier(&s_cache);
        if (currentCache == NULL)
        {
            return;
        }

        bucket = currentCache->KeyToBucket(source, target);
        DWORD index = bucket;
        CastCacheEntry* pEntry = &currentCache->m_Table[index];

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
            pEntry = &currentCache->m_Table[index & currentCache->m_TableMask];
        }

        // bucket is full.
    } while (!noGC && TryGrow());

    // pick a victim somewhat randomly within a bucket 
    // NB: ++ is not interlocked. We are ok if we lose counts here. It is just a number that changes.
    DWORD victim = currentCache->m_victimCount++ & (BUCKET_SIZE - 1);
    // position the victim in a quadratic reprobe bucket
    victim = (victim * victim + victim) / 2;

    {
        CastCacheEntry* pEntry = &currentCache->m_Table[(bucket + victim) & currentCache->m_TableMask];

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
