// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"

#include "utilcode.h"
#include "corjit.h"
#include "jithost.h"

void* JitHost::allocateMemory(size_t size, bool usePageAllocator)
{
    WRAPPER_NO_CONTRACT;

    if (usePageAllocator)
    {
        return GetEEMemoryManager()->ClrVirtualAlloc(nullptr, size, MEM_COMMIT, PAGE_READWRITE);
    }
    else
    {
        return ClrAllocInProcessHeap(0, S_SIZE_T(size));
    }
}

void JitHost::freeMemory(void* block, bool usePageAllocator)
{
    WRAPPER_NO_CONTRACT;

    if (usePageAllocator)
    {
        GetEEMemoryManager()->ClrVirtualFree(block, 0, MEM_RELEASE);
    }
    else
    {
        ClrFreeInProcessHeap(0, block);
    }
}

int JitHost::getIntConfigValue(const wchar_t* name, int defaultValue)
{
    WRAPPER_NO_CONTRACT;

    // Translate JIT call into runtime configuration query
    CLRConfig::ConfigDWORDInfo info{ name, defaultValue, CLRConfig::EEConfig_default };

    // Perform a CLRConfig look up on behalf of the JIT.
    return CLRConfig::GetConfigValue(info);
}

const wchar_t* JitHost::getStringConfigValue(const wchar_t* name)
{
    WRAPPER_NO_CONTRACT;

    // Translate JIT call into runtime configuration query
    CLRConfig::ConfigStringInfo info{ name, CLRConfig::EEConfig_default };

    // Perform a CLRConfig look up on behalf of the JIT.
    return CLRConfig::GetConfigValue(info);
}

void JitHost::freeStringConfigValue(const wchar_t* value)
{
    WRAPPER_NO_CONTRACT;

    CLRConfig::FreeConfigString(const_cast<wchar_t*>(value));
}

struct Slab
{
    Slab * pNext;
    size_t size;
    Thread* affinity;
};

static CrstStatic s_JitSlabAllocatorCrst;
static Slab* s_pCurrentCachedList;
static Slab* s_pPreviousCachedList;
static size_t s_TotalCached;

void* JitHost::allocateSlab(size_t size, size_t* pActualSize)
{
    size = max(size, sizeof(Slab));

    Thread* pCurrentThread = GetThread();
    if (s_pCurrentCachedList != NULL || s_pPreviousCachedList != NULL)
    {
        CrstHolder lock(&s_JitSlabAllocatorCrst);
        Slab** ppCandidate = NULL;

        for (Slab ** ppList = &s_pCurrentCachedList; *ppList != NULL; ppList = &(*ppList)->pNext)
        {
            Slab* p = *ppList;
            if (p->size >= size && p->size <= 4 * size) // Avoid wasting more than 4x memory
            {
                ppCandidate = ppList;
                if (p->affinity == pCurrentThread)
                    break;
            }
        }

        if (ppCandidate == NULL)
        {
            for (Slab ** ppList = &s_pPreviousCachedList; *ppList != NULL; ppList = &(*ppList)->pNext)
            {
                Slab* p = *ppList;
                if (p->size == size) // Allocation from previous list requires exact match
                {
                    ppCandidate = ppList;
                    if (p->affinity == pCurrentThread)
                        break;
                }
            }
        }

        if (ppCandidate != NULL)
        {
            Slab* p = *ppCandidate;
            *ppCandidate = p->pNext;

            s_TotalCached -= p->size;
            *pActualSize = p->size;

            return p;
        }
    }

    *pActualSize = size;
    return new (nothrow) BYTE[size];
}

void JitHost::freeSlab(void* slab, size_t actualSize)
{
    _ASSERTE(actualSize >= sizeof(Slab));

    if (actualSize < 0x100000) // Do not cache blocks that are more than 1MB
    {
        CrstHolder lock(&s_JitSlabAllocatorCrst);

        if (s_TotalCached < 0x1000000) // Do not cache more than 16MB
        {
            s_TotalCached += actualSize;

            Slab* pSlab = (Slab*)slab;
            pSlab->size = actualSize;
            pSlab->affinity = GetThread();
            pSlab->pNext = s_pCurrentCachedList;
            s_pCurrentCachedList = pSlab;
            return;
        }
    }

    delete [] (BYTE*)slab;
}

void JitHost::Init()
{
    s_JitSlabAllocatorCrst.Init(CrstLeafLock);
}

DWORD s_lastFlush = 0;

void JitHost::Reclaim()
{
    if (s_pCurrentCachedList != NULL || s_pPreviousCachedList != NULL)
    {
        DWORD ticks = ::GetTickCount();
        if (ticks - s_lastFlush < 2000) // Flush the free lists every 2 seconds
            return;
        s_lastFlush = ticks;

        // Flush all slabs in s_pPreviousCachedList
        for (;;)
        {
            Slab* slabToDelete = NULL;

            {
                CrstHolder lock(&s_JitSlabAllocatorCrst);
                slabToDelete = s_pPreviousCachedList;
                if (slabToDelete == NULL)
                {
                    s_pPreviousCachedList = s_pCurrentCachedList;
                    s_pCurrentCachedList = NULL;
                    break;
                }
                s_TotalCached -= slabToDelete->size;
                s_pPreviousCachedList = slabToDelete->pNext;
            }

            delete[](BYTE*)slabToDelete;
        }
    }
}

JitHost JitHost::theJitHost;
ICorJitHost* JitHost::getJitHost()
{
    STATIC_CONTRACT_SO_TOLERANT;
    STATIC_CONTRACT_GC_NOTRIGGER;
    STATIC_CONTRACT_NOTHROW;
    STATIC_CONTRACT_CANNOT_TAKE_LOCK;

    return &theJitHost;
}
