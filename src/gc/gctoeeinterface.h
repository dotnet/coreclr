// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __GCTOEEINTERFACE_H__
#define __GCTOEEINTERFACE_H__

#ifdef FEATURE_STANDALONE_GC

// The singular interface instance. All calls in GCToEEInterface
// will be fowarded to this interface instance.
extern IGCToCLR* g_theGCToCLR;

// When we are building the GC in a standalone environment, we
// will be dispatching virtually against g_theGCToCLR to call
// into the EE. This class provides an identical API to the existing
// GCToEEInterface, but only forwards the call onto the global
// g_theGCToCLR instance.
class GCToEEInterface {
public:
    static inline void SuspendEE(SUSPEND_REASON reason) 
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->SuspendEE(reason);
    }

    static inline void RestartEE(bool bFinishedGC)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->RestartEE(bFinishedGC);
    }

    static inline void GcScanRoots(promote_func* fn, int condemned, int max_gen, ScanContext* sc)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->GcScanRoots(fn, condemned, max_gen, sc);
    }

    static inline void GcStartWork(int condemned, int max_gen)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->GcStartWork(condemned, max_gen);
    }

    static inline void AfterGcScanRoots(int condemned, int max_gen, ScanContext* sc)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->AfterGcScanRoots(condemned, max_gen, sc);
    }

    static inline void GcBeforeBGCSweepWork()
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->GcBeforeBGCSweepWork();
    }

    static inline void GcDone(int condemned)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->GcDone(condemned);
    }

    static inline bool RefCountedHandleCallbacks(Object * pObject)
    {
        assert(g_theGCToCLR != nullptr);
        return g_theGCToCLR->RefCountedHandleCallbacks(pObject);
    }

    static inline void SyncBlockCacheWeakPtrScan(HANDLESCANPROC scanProc, uintptr_t lp1, uintptr_t lp2)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->SyncBlockCacheWeakPtrScan(scanProc, lp1, lp2);
    }

    static inline void SyncBlockCacheDemote(int max_gen)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->SyncBlockCacheDemote(max_gen);
    }

    static inline void SyncBlockCachePromotionsGranted(int max_gen)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->SyncBlockCachePromotionsGranted(max_gen);
    }

    static inline bool IsPreemptiveGCDisabled(Thread * pThread)
    {
        assert(g_theGCToCLR != nullptr);
        return g_theGCToCLR->IsPreemptiveGCDisabled(pThread);
    }


    static inline void EnablePreemptiveGC(Thread * pThread)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->EnablePreemptiveGC(pThread);
    }

    static inline void DisablePreemptiveGC(Thread * pThread)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->DisablePreemptiveGC(pThread);
    }

    static inline gc_alloc_context * GetAllocContext(Thread * pThread)
    {
        assert(g_theGCToCLR != nullptr);
        return g_theGCToCLR->GetAllocContext(pThread);
    }


    static inline bool CatchAtSafePoint(Thread * pThread)
    {
        assert(g_theGCToCLR != nullptr);
        return g_theGCToCLR->CatchAtSafePoint(pThread);
    }

    static inline void GcEnumAllocContexts(enum_alloc_context_func* fn, void* param)
    {
        assert(g_theGCToCLR != nullptr);
        g_theGCToCLR->GcEnumAllocContexts(fn, param);
    }

    static inline Thread* CreateBackgroundThread(GCBackgroundThreadFunction threadStart, void* arg)
    {
        assert(g_theGCToCLR != nullptr);
        return g_theGCToCLR->CreateBackgroundThread(threadStart, arg);
    }
};
#endif // FEATURE_STANDALONE_GC

#endif // __GCTOEEINTERFACE_H__
