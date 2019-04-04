// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: castcache.h
//

#ifndef _CAST_CACHE_H
#define _CAST_CACHE_H

#include "util.hpp"
#include "syncclean.hpp"

//
// A very lightweight cache that maps {source, target} -> result, where result is 
// a boolean value indicating that the types are definitely convertible or definitely not.
// We generically allow either MethodTable* or TypeHandle as source/target. Either value
// uniquely maps to a single type with no possibility of confusion. Besides for most types 
// TypeHandle a MethodTable* are the same value anyways.
//
// The primary purpose is caching results of conversion analysis assuming that covertibility relationship, 
// once computed, cannot change. (except for unloadable assemblies case, which is special).
//
// One thing to consider is that conversions are relatively fast, which demands that the cache is fast too.
// On the other hand, we do not need to be 100% accurate about a presence of an entry in a cache, 
// since everything can be re-computed relatively quickly. (we still hope to have a good hit rate)
//
// The overal design of the cache is an open-addressing hash table with quadratic probing 
// strategy and a limited bucket size. 
// In a case of inserting into a full bucket, we -
// 1) try getting a bigger table if not already at max size. (additional grow heuristics TBD), otherwise
// 2) pick a random victim entry within the bucket and replace it with a new entry. 
// That is basically our expiration policy. We want to keep things simple.
// 
// The table permits fully concurrent writes and stores. We use double-versioned entries to detect tearing, 
// which happens temporarily during updating. Entries in a torn state are ignored by readers and writers.
// As a result TryGet is Wait-Free - no locking or spinning.
//             TryAdd is mostly Wait-Free (may try allocating a new table), but is more complex than TryGet.
// 
// The assumption that same source and target TypeHandle keep the same relationship could be, in theory, 
// broken if the types involved are unloaded and their handles are reused.
// To counter that possibility we simply flush the whole cache on assembly unloads.
//
// Whenever we need to replace or resize the table, the obsolete table is passed to SyncClean 
// for a safe disposal when EE is suspended. That guarantees that the obsolete table is no longer in use 
// without any syncronization with readers/writers, which would be very unfortunate.
// 
class CastCache
{
    friend class SyncClean;

    struct CastCacheEntry
    {
        DWORD               version1;
        DWORD               version2;

        TADDR               source;
        // pointers have unused lower bits due to alignment, we use one for the result
        TADDR               targetAndResult;

        FORCEINLINE TADDR Source()
        {
            return source;
        }

        FORCEINLINE TADDR Target()
        {
            return targetAndResult & ~(TADDR)1;
        }

        FORCEINLINE BOOL Result()
        {
            return targetAndResult & 1;
        };

        FORCEINLINE void SetEntry(TADDR source, TADDR target, BOOL result)
        {
            this->source = source;
            this->targetAndResult = target | (result & 1);
        }
    };

public:
    CastCache(DWORD size)
    {
        CONTRACTL
        {
            THROWS;
            MODE_ANY;
            GC_TRIGGERS;     
        }
        CONTRACTL_END;

        m_TableMask = size - 1;
        //TODO: VS this is actually a Log2. Is there a helper?
        m_TableBits = CountBits(size - 1);
        m_Table = new CastCacheEntry[size];
        memset(m_Table, 0, size * sizeof(CastCacheEntry));
    }

    enum CastCacheResult
    {
        CannotCast,
        CanCast,
        NotCached
    };

    FORCEINLINE static void TryAddToCache(TypeHandle source, TypeHandle target, BOOL result)
    {
        WRAPPER_NO_CONTRACT;
        TryAddToCache(source.AsTAddr(), target.AsTAddr(), result, false);
    }

    FORCEINLINE static void TryAddToCache(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
        WRAPPER_NO_CONTRACT;
        TryAddToCache((TADDR)pSourceMT, target.AsTAddr(), result, false);
    }

    FORCEINLINE static void TryAddToCacheNoGC(TypeHandle source, TypeHandle target, BOOL result)
    {
        WRAPPER_NO_CONTRACT;
        TryAddToCache(source.AsTAddr(), target.AsTAddr(), result, true);
    }

    FORCEINLINE static void TryAddToCacheNoGC(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
        WRAPPER_NO_CONTRACT;
        TryAddToCache((TADDR)pSourceMT, target.AsTAddr(), result, true);
    }

    FORCEINLINE static CastCacheResult TryGetFromCache(TypeHandle source, TypeHandle target)
    {
        WRAPPER_NO_CONTRACT;
        return TryGetFromCache(source.AsTAddr(), target.AsTAddr());
    }

    FORCEINLINE static CastCacheResult TryGetFromCache(MethodTable* pSourceMT, TypeHandle target)
    {
        WRAPPER_NO_CONTRACT;
        return TryGetFromCache((TADDR)pSourceMT, target.AsTAddr());
    }

    FORCEINLINE static BOOL IsConvertible(MethodTable* pSourceMT, MethodTable* pTargetMT)
    {
        WRAPPER_NO_CONTRACT;
        return TryGetFromCache((TADDR)pSourceMT, (TADDR)pTargetMT) == CastCacheResult::CanCast;
    }

    FORCEINLINE static BOOL IsConvertible(MethodTable* pSourceMT, TypeHandle target)
    {
        WRAPPER_NO_CONTRACT;
        return TryGetFromCache((TADDR)pSourceMT, target.AsTAddr()) == CastCacheResult::CanCast;
    }

    static void FlushCurrentCache();

private:

#if DEBUG
    static const DWORD INITIAL_CACHE_SIZE = 8;    // MUST BE A POWER OF TWO
#else
    static const DWORD INITIAL_CACHE_SIZE = 128;  // MUST BE A POWER OF TWO
#endif

    static const DWORD MAXIMUM_CACHE_SIZE = 128 * 1024; //TODO: VS too big?
    static const DWORD BUCKET_SIZE = 8;

    static CastCache*   s_cache;

    CastCacheEntry*     m_Table;
    DWORD               m_TableMask;
    DWORD               m_TableBits;
    DWORD               m_victimCount;
    CastCache*          m_NextObsolete;

    FORCEINLINE static CastCacheResult TryGetFromCache(TADDR source, TADDR target)
    {
        WRAPPER_NO_CONTRACT;

        if (source == target)
        {
            return CastCacheResult::CanCast;
        }

        return s_cache->TryGet(source, target);
    }

    FORCEINLINE static void TryAddToCache(TADDR source, TADDR target, BOOL result, BOOL noGC)
    {
        WRAPPER_NO_CONTRACT;

        if (source == target)
            return;

        s_cache->TrySet(source, target, result, noGC);
    }

    FORCEINLINE static BOOL TryGrow()
    {
        WRAPPER_NO_CONTRACT;

        CastCache* currentCache = s_cache;

        //TODO: VS need to be smarter with resize or this is enough? 
        //TODO: any problems with concurent expansion and waste, perhaps a spinlock?
        if (currentCache->Size() < MAXIMUM_CACHE_SIZE)
        {
            MaybeReplaceCacheWithLarger(currentCache);
            return TRUE;
        }

        return FALSE;
    }

    FORCEINLINE DWORD Size()
    {
        WRAPPER_NO_CONTRACT;

        return m_TableMask + 1;
    }

    FORCEINLINE DWORD KeyToBucket(TADDR source, TADDR target)
    {
        WRAPPER_NO_CONTRACT;

        // upper bits are less interesting, so we do "rotl(source, <half-size>) ^ target" for mixing;
        // hash the mixed value to desired size
        // we use fibonacci hashing

#if BIT64
        TADDR hash = (((ULONGLONG)source << 32) | ((ULONGLONG)source >> 32)) ^ target;
        return (DWORD)((hash * 11400714819323198485llu) >> (64 - m_TableBits));
#else
        TADDR hash = _rotl(source, 16) ^ target;
        return (DWORD)((hash * 2654435769ul) >> (32 - m_TableBits));
#endif
    }

    static void MaybeReplaceCacheWithLarger(CastCache* currentCache);
    CastCacheResult TryGet(TADDR source, TADDR target);
    static void TrySet(TADDR source, TADDR target, BOOL result, BOOL noGC);

    // only SyncClean is supposed to call this in GC
    ~CastCache();
};

#endif
