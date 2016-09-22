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

// SUSPEND_REASON is the reason why the GC wishes to suspend the EE,
// used as an argument to IGCToCLR::SuspendEE.
typedef enum
{
    SUSPEND_FOR_GC = 1,
    SUSPEND_FOR_GC_PREP = 6
} SUSPEND_REASON;

// The rationale behind these defines it that, if we are intending
// to link the GC statically to the runtime, there's no need to dispatch
// this interface virtually and we can call directly into the interface
// implementation. If our GC will be dynamically linked, though, this
// interface must be virtual.
#ifdef FEATURE_STANDALONE_GC
# define EE_CALL_DECL_BEGIN virtual
# define EE_CALL_DECL_END = 0
# define EE_INTERFACE_NAME IGCToCLR
#else
# define EE_CALL_DECL_BEGIN static
# define EE_CALL_DECL_END
# define EE_INTERFACE_NAME GCToEEInterface

// Even if FEATURE_STANDALONE_GC is not defined,
// this type still exists as an opaque class.
class IGCToCLR;

#endif // FEATURE_STANDALONE_GC

// This interface provides the interface that the GC will use to speak to the rest
// of the execution engine. Everything that the GC does that requires the EE
// to be informed or that requires EE action must go through this interface.
//
// When FEATURE_STANDALONE_GC is defined, this class is named IGCToCLR and is
// an abstract class. The EE will provide a class that fulfills this interface,
// and the GC will dispatch virtually on it to call into the EE. When FEATURE_STANDALONE_GC
// is not defined, this class is named GCToEEInterface and the GC will dispatch statically on it.
class EE_INTERFACE_NAME {
public:
    // Suspends the EE for the given reason.
    EE_CALL_DECL_BEGIN
    void SuspendEE(SUSPEND_REASON reason) EE_CALL_DECL_END;

    // Resumes all paused threads, with a boolean indicating
    // if the EE is being restarted because a GC is complete.
    EE_CALL_DECL_BEGIN
    void RestartEE(bool bFinishedGC) EE_CALL_DECL_END;

    // Performs a stack walk of all managed threads and invokes the given promote_func
    // on all GC roots encountered on the stack. Depending on the condemned generation,
    // this function may also enumerate all static GC refs if necessary.
    EE_CALL_DECL_BEGIN
    void GcScanRoots(promote_func* fn, int condemned, int max_gen, ScanContext* sc) EE_CALL_DECL_END;

    // Callback from the GC informing the EE that it is preparing to start working.
    EE_CALL_DECL_BEGIN
    void GcStartWork(int condemned, int max_gen) EE_CALL_DECL_END;

    // Callback from the GC informing the EE that it has completed the managed stack
    // scan. User threads are still suspended at this point.
    EE_CALL_DECL_BEGIN
    void AfterGcScanRoots(int condemned, int max_gen, ScanContext* sc) EE_CALL_DECL_END;

    // Callback from the GC informing the EE that the background sweep phase of a BGC is
    // about to begin.
    EE_CALL_DECL_BEGIN
    void GcBeforeBGCSweepWork() EE_CALL_DECL_END;

    // Callback from the GC informing the EE that a GC has completed.
    EE_CALL_DECL_BEGIN
    void GcDone(int condemned) EE_CALL_DECL_END;

    // Predicate for the GC to query whether or not a given refcounted handle should
    // be promoted.
    EE_CALL_DECL_BEGIN
    bool RefCountedHandleCallbacks(Object * pObject) EE_CALL_DECL_END;

    // Performs a weak pointer scan of the sync block cache.
    EE_CALL_DECL_BEGIN
    void SyncBlockCacheWeakPtrScan(HANDLESCANPROC scanProc, uintptr_t lp1, uintptr_t lp2) EE_CALL_DECL_END;

    // Indicates to the EE that the GC intends to demote objects in the sync block cache.
    EE_CALL_DECL_BEGIN
    void SyncBlockCacheDemote(int max_gen) EE_CALL_DECL_END;

    // Indicates to the EE that the GC has granted promotion to objects in the sync block cache.
    EE_CALL_DECL_BEGIN
    void SyncBlockCachePromotionsGranted(int max_gen) EE_CALL_DECL_END;

    // Queries whether or not the given thread has preemptive GC disabled.
    EE_CALL_DECL_BEGIN
    bool IsPreemptiveGCDisabled(Thread * pThread) EE_CALL_DECL_END;

    // Enables preemptive GC on the given thread.
    EE_CALL_DECL_BEGIN
    void EnablePreemptiveGC(Thread * pThread) EE_CALL_DECL_END;

    // Disables preemptive GC on the given thread.
    EE_CALL_DECL_BEGIN
    void DisablePreemptiveGC(Thread * pThread) EE_CALL_DECL_END;

    // Retrieves the alloc context associated with a given thread.
    EE_CALL_DECL_BEGIN
    gc_alloc_context * GetAllocContext(Thread * pThread) EE_CALL_DECL_END;

    // Returns true if this thread is waiting to reach a safe point.
    EE_CALL_DECL_BEGIN
    bool CatchAtSafePoint(Thread * pThread) EE_CALL_DECL_END;

    // Calls the given enum_alloc_context_func with every active alloc context.
    EE_CALL_DECL_BEGIN
    void GcEnumAllocContexts(enum_alloc_context_func* fn, void* param) EE_CALL_DECL_END;

    // Creates and returns a new background thread.
    EE_CALL_DECL_BEGIN
    Thread* CreateBackgroundThread(GCBackgroundThreadFunction threadStart, void* arg) EE_CALL_DECL_END;
};

#undef EE_CALL_DECL_BEGIN
#undef EE_CALL_DECL_END
#undef EE_INTERFACE_NAME

#endif // _GCINTERFACE_EE_H_
