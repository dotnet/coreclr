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
// a boolean value indicating that the type of the source can cast to the target type or 
// definitely cannot.
//
// In the terminology of ECMA335 the relationship is called "compatible-with". 
// This is the relation used by castclass and isinst (III.4.3).
// 
// We generically allow either MethodTable* or TypeHandle as source/target. Either value
// uniquely maps to a single type with no possibility of confusion. Besides for most types 
// TypeHandle a MethodTable* are the same value anyways.
//
// One thing to consider is that cast analysis is relatively fast, which demands that the cache is fast too.
// On the other hand, we do not need to be 100% accurate about a presence of an entry in a cache, 
// since everything can be re-computed relatively quickly. 
// We still hope to have a good hit rate, but can tolerate items pushed/flushed from the cache. 
//
// The overal design of the cache is an open-addressing hash table with quadratic probing 
// strategy and a limited bucket size. 
// In a case of inserting into a full bucket, we -
// 1) try getting a bigger table if not already at max size. Otherwise
// 2) pick a random victim entry within the bucket and replace it with a new entry. 
// That is basically our expiration policy. We want to keep things simple.
// 
// The cache permits fully concurrent writes and stores. We use double-versioned entries to detect tearing, 
// which happens temporarily during updating. Entries in a torn state are ignored by readers and writers.
// As a result TryGet is Wait-Free - no locking or spinning.
//             TryAdd is mostly Wait-Free (may try allocating a new table), but is more complex than TryGet.
// 
// The assumption that same source and target keep the same relationship could be 
// broken if the types involved are unloaded and their handles are reused. (ABA problem).
// To counter that possibility we simply flush the whole cache on assembly unloads.
//
// Whenever we need to replace or resize the table, we simply allocate a new one and atomically 
// update the static handle. The old table may be still in use, but will eventually be collected by GC.
// 
class CastCache
{
#if !defined(DACCESS_COMPILE) && !defined(CROSSGEN_COMPILE)

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

    FORCEINLINE static void TryAddToCache(TypeHandle source, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        TryAddToCache(source.AsTAddr(), target.AsTAddr(), result);
    }

    FORCEINLINE static void TryAddToCache(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        TryAddToCache((TADDR)pSourceMT, target.AsTAddr(), result);
    }

    FORCEINLINE static TypeHandle::CastResult TryGetFromCache(TypeHandle source, TypeHandle target)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        return TryGetFromCache(source.AsTAddr(), target.AsTAddr());
    }

    FORCEINLINE static TypeHandle::CastResult TryGetFromCacheAny(TypeHandle source, TypeHandle target)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_ANY;
        }
        CONTRACTL_END;

        GCX_COOP();

        return TryGetFromCache(source, target);
    }

    FORCEINLINE static TypeHandle::CastResult TryGetFromCache(MethodTable* pSourceMT, TypeHandle target)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        return TryGetFromCache((TADDR)pSourceMT, target.AsTAddr());
    }

    static void FlushCurrentCache();

private:

// The cache size is driven by demand and generally is fairly small. (casts are repetitive)
// Even conversion-churning tests such as Linq.Expressions will not need > 4096
// When we reach the limit, the new entries start replacing the old ones somewhat randomly.
// Considering that typically the cache size is small and that hit rates are high with good locality, 
// just keeping the cache around seems a simple and viable strategy.
// 
// Additional behaviors that could be considered, if there are scenarios that could be improved:
//     - flush the cache based on some heuristics
//     - shrink the cache based on some heuristics
// 
#if DEBUG
    static const DWORD INITIAL_CACHE_SIZE = 8;    // MUST BE A POWER OF TWO
    static const DWORD MAXIMUM_CACHE_SIZE = 512;  // make this lower than release to make it easier to reach this in tests.
#else
    static const DWORD INITIAL_CACHE_SIZE = 128;  // MUST BE A POWER OF TWO
    static const DWORD MAXIMUM_CACHE_SIZE = 4096; // 4096 * sizeof(CastCacheEntry) is 98304 bytes on 64bit. We will rarely need this much though.
#endif

// Lower bucket size will cause the table to resize earlier
// Higher bucket size will increase upper bound cost of Get
//
// In a cold scenario and 64byte cache line:
//    1 cache miss for 1 probe, 
//    2 sequential misses for 3 probes, 
//    then a miss can be assumed for every additional probe.
// We pick 8 as the probe limit (hoping for 4 probes on average), but the number can be refined further.
    static const DWORD BUCKET_SIZE = 8;

    static OBJECTHANDLE   s_cache;

    FORCEINLINE static TypeHandle::CastResult TryGetFromCache(TADDR source, TADDR target)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        if (source == target)
        {
            return TypeHandle::CanCast;
        }

        return TryGet(source, target);
    }

    FORCEINLINE static void TryAddToCache(TADDR source, TADDR target, BOOL result)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        if (source == target)
            return;
        
        TrySet(source, target, result);
    }

    FORCEINLINE static bool TryGrow(BASEARRAYREF table)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        DWORD newSize = CacheElementCount(table) * 2;
        if (newSize <= MAXIMUM_CACHE_SIZE)
        {
            return MaybeReplaceCacheWithLarger(newSize);
        }

        return false;
    }

    FORCEINLINE static DWORD KeyToBucket(BASEARRAYREF table, TADDR source, TADDR target)
    {
        // upper bits of addresses do not vary much, so to reduce loss due to cancelling out, 
        // we do `rotl(source, <half-size>) ^ target` for mixing inputs.
        // then we use fibonacci hashing to reduce the value to desired size.

#if BIT64
        TADDR hash = (((ULONGLONG)source << 32) | ((ULONGLONG)source >> 32)) ^ target;
        return (DWORD)((hash * 11400714819323198485llu) >> HashShift(table));
#else
        TADDR hash = _rotl(source, 16) ^ target;
        return (DWORD)((hash * 2654435769ul) >> HashShift(table));
#endif
    }

    FORCEINLINE static byte* AuxData(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;

        // element 0 is used for embedded aux data
        return (byte*)OBJECTREFToObject(table) + ARRAYBASE_SIZE;
    }

    FORCEINLINE static CastCacheEntry* Elements(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;
        // element 0 is used for embedded aux data, skip it
        return (CastCacheEntry*)AuxData(table) + 1;
    }

    // TableMask is "size - 1" 
    // we need that more often that we need size
    FORCEINLINE static DWORD& TableMask(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;
        return *(DWORD*)AuxData(table);
    }

    FORCEINLINE static BYTE& HashShift(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;
        return *((BYTE*)AuxData(table) + sizeof(DWORD));
    }

    FORCEINLINE static BYTE& VictimCounter(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;
        return *((BYTE*)AuxData(table) + sizeof(DWORD) + 1);
    }

    FORCEINLINE static DWORD CacheElementCount(BASEARRAYREF table)
    {
        LIMITED_METHOD_CONTRACT;
        return TableMask(table) + 1;
    }

    static BASEARRAYREF CreateCastCache(DWORD size);
    static BOOL MaybeReplaceCacheWithLarger(DWORD size);
    static TypeHandle::CastResult TryGet(TADDR source, TADDR target);
    static void TrySet(TADDR source, TADDR target, BOOL result);

#else // !DACCESS_COMPILE && !CROSSGEN_COMPILE
public:
    FORCEINLINE static void TryAddToCache(TypeHandle source, TypeHandle target, BOOL result)
    {
    }

    FORCEINLINE static void TryAddToCacheAny(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
    }

    FORCEINLINE static void TryAddToCache(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
    }

    FORCEINLINE static TypeHandle::CastResult TryGetFromCache(TypeHandle source, TypeHandle target)
    {
        return TypeHandle::MaybeCast;
    }

#endif // !DACCESS_COMPILE && !CROSSGEN_COMPILE
};

#endif
