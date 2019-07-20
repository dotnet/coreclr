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
// 1) try getting a bigger table if not already at max size. Otherwise
// 2) pick a random victim entry within the bucket and replace it with a new entry. 
// That is basically our expiration policy. We want to keep things simple.
// 
// The table permits fully concurrent writes and stores. We use double-versioned entries to detect tearing, 
// which happens temporarily during updating. Entries in a torn state are ignored by readers and writers.
// As a result TryGet is Wait-Free - no locking or spinning.
//             TryAdd is mostly Wait-Free (may try allocating a new table), but is more complex than TryGet.
// 
// The assumption that same source and target TypeHandle keep the same relationship could be 
// broken if the types involved are unloaded and their handles are reused.
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

    CastCache(DWORD size)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        m_Table = (BASEARRAYREF)AllocatePrimitiveArray(CorElementType::ELEMENT_TYPE_I8, (size + 1) * sizeof(CastCacheEntry) / sizeof(INT64));

        this->TableMask() = size - 1;

        // Fibonacci hash reduces the value into desired range by shifting right by the number of leading zeroes in 'size-1' 
        DWORD bitCnt;
#if BIT64
        this->HashShift() = (BYTE)(63 - BitScanReverse64(&bitCnt, size - 1));
#else
        BitScanReverse(&bitCnt, size - 1);
        this->HashShift() = (BYTE)(31 - bitCnt);
#endif
    }

    CastCache(OBJECTREF arr)
        : m_Table((BASEARRAYREF)arr)
    {
    }

    FORCEINLINE static void TryAddToCache(TypeHandle source, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        TryAddToCache(source.AsTAddr(), target.AsTAddr(), result, false);
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

        TryAddToCache((TADDR)pSourceMT, target.AsTAddr(), result, false);
    }

    FORCEINLINE static void TryAddToCacheNoGC(TypeHandle source, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        TryAddToCache(source.AsTAddr(), target.AsTAddr(), result, true);
    }

    FORCEINLINE static void TryAddToCacheNoGC(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        TryAddToCache((TADDR)pSourceMT, target.AsTAddr(), result, true);
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

    FORCEINLINE CastCacheEntry* Elements()
    {

        // element 0 is used for embedded aux data, skip it
        return (CastCacheEntry*)AuxData() + 1;
    }

    // TableMask is "size - 1" 
    // we need that more often that we need size
    FORCEINLINE DWORD& TableMask()
    {
        return *(DWORD*)AuxData();
    }

    FORCEINLINE BYTE& HashShift()
    {
        return *((BYTE*)AuxData() + sizeof(DWORD));
    }

    FORCEINLINE BYTE& VictimCounter()
    {
        return *((BYTE*)AuxData() + sizeof(DWORD) + 1);
    }

    FORCEINLINE DWORD TableSize()
    {
        return this->TableMask() + 1;
    }

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
    BASEARRAYREF m_Table;


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

        OBJECTHANDLE c = s_cache;
        if (c == NULL)
        {
            return TypeHandle::MaybeCast;
        }

        CastCache cache = CastCache(ObjectFromHandle(c));
        return cache.TryGet(source, target);
    }

    FORCEINLINE static void TryAddToCache(TADDR source, TADDR target, BOOL result, BOOL noGC)
    {
        CONTRACTL
        {
            if (noGC) { NOTHROW; } else { THROWS; }
            if (noGC) { GC_NOTRIGGER; } else { GC_TRIGGERS; }
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        if (source == target)
            return;
        
        TrySet(source, target, result, noGC);
    }

    FORCEINLINE byte* AuxData()
    {
        // element 0 is used for embedded aux data
        return (byte*)OBJECTREFToObject(this->m_Table) + ARRAYBASE_SIZE;
    }

    FORCEINLINE static bool TryGrow(CastCache currentCache)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
        }
        CONTRACTL_END;

        if (currentCache.m_Table == NULL || currentCache.Size() < MAXIMUM_CACHE_SIZE)
        {
            return MaybeReplaceCacheWithLarger(currentCache).m_Table != NULL;
        }

        return false;
    }

    FORCEINLINE DWORD Size()
    {
        return this->TableSize();
    }

    FORCEINLINE DWORD KeyToBucket(TADDR source, TADDR target)
    {
        // upper bits of addresses do not vary much, so to reduce loss due to cancelling out, 
        // we do `rotl(source, <half-size>) ^ target` for mixing inputs.
        // then we use fibonacci hashing to reduce the value to desired size.

#if BIT64
        TADDR hash = (((ULONGLONG)source << 32) | ((ULONGLONG)source >> 32)) ^ target;
        return (DWORD)((hash * 11400714819323198485llu) >> this->HashShift());
#else
        TADDR hash = _rotl(source, 16) ^ target;
        return (DWORD)((hash * 2654435769ul) >> this->HashShift());
#endif
    }

    static CastCache MaybeReplaceCacheWithLarger(CastCache currentCache);

    TypeHandle::CastResult TryGet(TADDR source, TADDR target);
    static void TrySet(TADDR source, TADDR target, BOOL result, BOOL noGC);

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

    FORCEINLINE static void TryAddToCacheNoGC(TypeHandle source, TypeHandle target, BOOL result)
    {
    }

    FORCEINLINE static void TryAddToCacheNoGC(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
    }

    FORCEINLINE static TypeHandle::CastResult TryGetFromCache(TypeHandle source, TypeHandle target)
    {
        return TypeHandle::MaybeCast;
    }

#endif // !DACCESS_COMPILE && !CROSSGEN_COMPILE
};

#endif
