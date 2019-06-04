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

        //TODO: VS handle OOM. maybe lower size to min?
        m_Table = (BASEARRAYREF)AllocatePrimitiveArray(CorElementType::ELEMENT_TYPE_I8, (size + 1) * sizeof(CastCacheEntry) / sizeof(INT64));

        //TODO: VS this is actually a Log2. Is there a helper?
        this->TableBits() = CountBits(size - 1);
    }

    CastCache(OBJECTREF arr)
    {
        m_Table = (BASEARRAYREF)arr;
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

    FORCEINLINE static void TryAddToCacheAny(MethodTable* pSourceMT, TypeHandle target, BOOL result)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_ANY;
        }
        CONTRACTL_END;

        GCX_COOP();

        TryAddToCache(pSourceMT, target, result);
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

    FORCEINLINE DWORD TableSize()
    {
        return ((DWORD)this->m_Table->GetNumComponents() * sizeof(INT64) / sizeof(CastCacheEntry)) - 1;
    }

    FORCEINLINE CastCacheEntry* Elements()
    {

        // element 0 is used for embedded aux data, skip it
        // TODO: VS perhaps just offset by ARRAYBASE_SIZE ?
        return (CastCacheEntry*)this->m_Table->GetDataPtr() + 1;
    }

    FORCEINLINE DWORD TableMask()
    {
        return this->TableSize() - 1;
    }

    FORCEINLINE BYTE& TableBits()
    {
        return *((BYTE*)&AuxData() + 0);
    }

    FORCEINLINE BYTE& VictimCounter()
    {
        return *((BYTE*)&AuxData() + 1);
    }

private:

#if DEBUG
    static const DWORD INITIAL_CACHE_SIZE = 8;    // MUST BE A POWER OF TWO
#else
    static const DWORD INITIAL_CACHE_SIZE = 128;  // MUST BE A POWER OF TWO
#endif

    static const DWORD MAXIMUM_CACHE_SIZE = 128 * 1024; //TODO: VS too big?
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

    FORCEINLINE DWORD& AuxData()
    {
        // element 0 is used for embedded aux data
        return (DWORD&)*this->m_Table->GetDataPtr();
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

        //TODO: VS need to be smarter with resize or this is enough? 
        //TODO: any problems with concurent expansion and waste, perhaps a spinlock?
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
        // upper bits are less interesting, so we do "rotl(source, <half-size>) ^ target" for mixing;
        // hash the mixed value to desired size
        // we use fibonacci hashing

#if BIT64
        TADDR hash = (((ULONGLONG)source << 32) | ((ULONGLONG)source >> 32)) ^ target;
        return (DWORD)((hash * 11400714819323198485llu) >> (64 - this->TableBits()));
#else
        TADDR hash = _rotl(source, 16) ^ target;
        return (DWORD)((hash * 2654435769ul) >> (32 - this->TableBits()));
#endif
    }

    static CastCache MaybeReplaceCacheWithLarger(CastCache currentCache);

    TypeHandle::CastResult TryGet(TADDR source, TADDR target);
    static void TrySet(TADDR source, TADDR target, BOOL result, BOOL noGC);

#else
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

#endif // !DACCESS_COMPILE && !CROSSGEN_COMPILE
};

#endif
