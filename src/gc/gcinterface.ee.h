// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _GCINTERFACE_EE_H_
#define _GCINTERFACE_EE_H_

struct ScanContext;
struct gc_alloc_context;
class CrawlFrame;

// Callback passed to GcScanRoots.
typedef void promote_func(PTR_PTR_Object, ScanContext*, uint32_t);

// Callback passed to GcEnumAllocContexts.
typedef void enum_alloc_context_func(gc_alloc_context*, void*);

// Callback passed to CreateBackgroundThread.
typedef uint32_t (__stdcall *GCBackgroundThreadFunction)(void* param);

// Struct often used as a parameter to callbacks.
typedef struct
{
    promote_func*  f;
    ScanContext*   sc;
    CrawlFrame *   cf;
} GCCONTEXT;

// IGCToCLR provides the interface that the GC will use to speak to the rest
// of the execution engine. Everything that the GC does that requires the EE
// to be informed or that requires EE action must go through this interface.
class IGCToCLR {
public:
    // SUSPEND_REASON is the reason why the GC wishes to suspend the EE,
    // used as an argument to IGCToCLR::SuspendEE.
    typedef enum
    {
        SUSPEND_FOR_GC = 1,
        SUSPEND_FOR_GC_PREP = 6
    } SUSPEND_REASON;

    // Suspends the EE for the given reason.
    virtual void SuspendEE(SUSPEND_REASON reason) = 0;

    // Resumes all paused threads, with a boolean indicating
    // if the EE is being restarted because a GC is complete.
    virtual void RestartEE(bool bFinishedGC) = 0;

    // Performs a stack walk of all managed threads and invokes the given promote_func
    // on all GC roots encountered on the stack. Depending on the condemned generation,
    // this function may also enumerate all static GC refs if necessary.
    virtual void GcScanRoots(promote_func* fn, int condemned, int max_gen, ScanContext* sc) = 0;

    // Callback from the GC informing the EE that it is preparing to start working.
    virtual void GcStartWork(int condemned, int max_gen) = 0;

    // Callback from the GC informing the EE that it has completed the managed stack
    // scan. User threads are still suspended at this point.
    virtual void AfterGcScanRoots(int condemned, int max_gen, ScanContext* sc) = 0;

    // Callback from the GC informing the EE that the background sweep phase of a BGC is
    // about to begin.
    virtual void GcBeforeBGCSweepWork() = 0;

    // Callback from the GC informing the EE that a GC has completed.
    virtual void GcDone(int condemned) = 0;

    // Predicate for the GC to query whether or not a given refcounted handle should
    // be promoted.
    virtual bool RefCountedHandleCallbacks(Object * pObject) = 0;

    // Performs a weak pointer scan of the sync block cache.
    virtual void SyncBlockCacheWeakPtrScan(HANDLESCANPROC scanProc, uintptr_t lp1, uintptr_t lp2) = 0;

    // Indicates to the EE that the GC intends to demote objects in the sync block cache.
    virtual void SyncBlockCacheDemote(int max_gen) = 0;

    // Indicates to the EE that the GC has granted promotion to objects in the sync block cache.
    virtual void SyncBlockCachePromotionsGranted(int max_gen) = 0;

    // Queries whether or not the given thread has preemptive GC disabled.
    virtual bool IsPreemptiveGCDisabled(Thread * pThread) = 0;

    // Enables preemptive GC on the given thread.
    virtual void EnablePreemptiveGC(Thread * pThread) = 0;

    // Disables preemptive GC on the given thread.
    virtual void DisablePreemptiveGC(Thread * pThread) = 0;

    // Retrieves the alloc context associated with a given thread.
    virtual gc_alloc_context * GetAllocContext(Thread * pThread) = 0;

    // Returns true if this thread is waiting to reach a safe point.
    virtual bool CatchAtSafePoint(Thread * pThread) = 0;

    // Calls the given enum_alloc_context_func with every active alloc context.
    virtual void GcEnumAllocContexts(enum_alloc_context_func* fn, void* param) = 0;

    // Creates and returns a new background thread.
    virtual Thread* CreateBackgroundThread(GCBackgroundThreadFunction threadStart, void* arg) = 0;
};

#endif // _GCINTERFACE_EE_H_
