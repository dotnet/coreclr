// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _GCINTERFACE_EE_H_
#define _GCINTERFACE_EE_H_

// This interface provides the interface that the GC will use to speak to the rest
// of the execution engine. Everything that the GC does that requires the EE
// to be informed or that requires EE action must go through this interface.
//
// When FEATURE_STANDALONE_GC is defined, this class is named IGCToCLR and is
// an abstract class. The EE will provide a class that fulfills this interface,
// and the GC will dispatch virtually on it to call into the EE. When FEATURE_STANDALONE_GC
// is not defined, this class is named GCToEEInterface and the GC will dispatch statically on it.
class IGCToCLR {
public:
    // Suspends the EE for the given reason.
    virtual
    void SuspendEE(SUSPEND_REASON reason) = 0;
    
    // Resumes all paused threads, with a boolean indicating
    // if the EE is being restarted because a GC is complete.
    virtual
    void RestartEE(bool bFinishedGC) = 0;

    // Performs a stack walk of all managed threads and invokes the given promote_func
    // on all GC roots encountered on the stack. Depending on the condemned generation,
    // this function may also enumerate all static GC refs if necessary.
    virtual
    void GcScanRoots(promote_func* fn, int condemned, int max_gen, ScanContext* sc) = 0;

    // Callback from the GC informing the EE that it is preparing to start working.
    virtual
    void GcStartWork(int condemned, int max_gen) = 0;

    // Callback from the GC informing the EE that it has completed the managed stack
    // scan. User threads are still suspended at this point.
    virtual
    void AfterGcScanRoots(int condemned, int max_gen, ScanContext* sc) = 0;

    // Callback from the GC informing the EE that the background sweep phase of a BGC is
    // about to begin.
    virtual
    void GcBeforeBGCSweepWork() = 0;

    // Callback from the GC informing the EE that a GC has completed.
    virtual
    void GcDone(int condemned) = 0;

    // Predicate for the GC to query whether or not a given refcounted handle should
    // be promoted.
    virtual
    bool RefCountedHandleCallbacks(Object * pObject) = 0;

    // Performs a weak pointer scan of the sync block cache.
    virtual
    void SyncBlockCacheWeakPtrScan(HANDLESCANPROC scanProc, uintptr_t lp1, uintptr_t lp2) = 0;

    // Indicates to the EE that the GC intends to demote objects in the sync block cache.
    virtual
    void SyncBlockCacheDemote(int max_gen) = 0;

    // Indicates to the EE that the GC has granted promotion to objects in the sync block cache.
    virtual
    void SyncBlockCachePromotionsGranted(int max_gen) = 0;

    // Queries whether or not the given thread has preemptive GC disabled.
    virtual
    bool IsPreemptiveGCDisabled(Thread * pThread) = 0;

    // Enables preemptive GC on the given thread.
    virtual
    void EnablePreemptiveGC(Thread * pThread) = 0;

    // Disables preemptive GC on the given thread.
    virtual
    void DisablePreemptiveGC(Thread * pThread) = 0;

    // Retrieves the alloc context associated with a given thread.
    virtual
    gc_alloc_context * GetAllocContext(Thread * pThread) = 0;

    // Returns true if this thread is waiting to reach a safe point.
    virtual
    bool CatchAtSafePoint(Thread * pThread) = 0;

    // Calls the given enum_alloc_context_func with every active alloc context.
    virtual
    void GcEnumAllocContexts(enum_alloc_context_func* fn, void* param) = 0;

    // Creates and returns a new background thread.
    virtual
    Thread* CreateBackgroundThread(GCBackgroundThreadFunction threadStart, void* arg) = 0;

    // When a GC starts, gives the diagnostics code a chance to run.
    virtual
    void DiagGCStart(int gen, bool isInduced) = 0;

    // When GC heap segments change, gives the diagnostics code a chance to run.
    virtual
    void DiagUpdateGenerationBounds() = 0;

    // When a GC ends, gives the diagnostics code a chance to run.
    virtual
    void DiagGCEnd(size_t index, int gen, int reason, bool fConcurrent) = 0;

    // During a GC after we discover what objects' finalizers should run, gives the diagnostics code a chance to run.
    virtual
    void DiagWalkFReachableObjects(void* gcContext) = 0;

    // During a GC after we discover the survivors and the relocation info, 
    // gives the diagnostics code a chance to run. This includes LOH if we are 
    // compacting LOH.
    virtual
    void DiagWalkSurvivors(void* gcContext) = 0;

    // During a full GC after we discover what objects to survive on LOH,
    // gives the diagnostics code a chance to run.
    virtual
    void DiagWalkLOHSurvivors(void* gcContext) = 0;

    // At the end of a background GC, gives the diagnostics code a chance to run.
    virtual
    void DiagWalkBGCSurvivors(void* gcContext) = 0;

    // Informs the EE of changes to the location of the card table, potentially updating the write
    // barrier if it needs to be updated.
    virtual
    void StompWriteBarrier(WriteBarrierParameters* args) = 0;

    // Signals to the finalizer thread that there are objects ready to
    // be finalized.
    virtual
    void EnableFinalization(bool foundFinalizers) = 0;

    // Signals to the EE that the GC encountered a fatal error and can't recover.
    virtual
    void HandleFatalError(unsigned int exitCode) = 0;

    // Asks the EE if it wants a particular object to be finalized when unloading
    // an app domain.
    virtual
    bool ShouldFinalizeObjectForUnload(AppDomain* pDomain, Object* obj) = 0;

    // Offers the EE the option to finalize the given object eagerly, i.e.
    // not on the finalizer thread but on the current thread. The
    // EE returns true if it finalized the object eagerly and the GC does not
    // need to do so, and false if it chose not to eagerly finalize the object
    // and it's up to the GC to finalize it later.
    virtual
    bool EagerFinalized(Object* obj) = 0;

    // Asks the EE if it wishes for the current GC to be a blocking GC. The GC will
    // only invoke this callback when it intends to do a full GC, so at this point
    // the EE can opt to elevate that collection to be a blocking GC and not a background one.
    virtual
    bool ForceFullGCToBeBlocking() = 0;

    // Creates a new auto event that is host-aware, with the given initial signalled state.
    // Returns null on failure - either on allocation failure or on a failure to create
    // the event.
    virtual
    CLREventStatic* CreateAutoEvent(bool initialState) = 0;

    // Creates a new manual event that is host-aware, with the given initial signalled state.
    // Returns null on failure - either on allocation failure or on a failure to create
    // the event.
    virtual
    CLREventStatic* CreateManualEvent(bool initialState) = 0;

    // Creates a new auto event that is not host-aware, with the given initial signalled state.
    // Returns null on failure - either on allocation failure or on a failure to create
    // the event.
    virtual
    CLREventStatic* CreateOSAutoEvent(bool initialState) = 0;

    // Creates a new manual event that is not host-aware, with the given initial signalled state.
    // Returns null on failure - either on allocation failure on on a failure to create
    // the event.
    virtual
    CLREventStatic* CreateOSManualEvent(bool initialState) = 0;

    // Closes an event created by one of the four above methods. It is a logic error to use the event
    // after closing it using this method.
    virtual
    void CloseEvent(CLREventStatic* event) = 0;

    // Sets the state of an event to signalled.
    virtual
    bool SetEvent(CLREventStatic* event) = 0;

    // Resets the state of an event. Should only be used with manual events - auto-resetting events
    // automatically reset when a thread that is waiting on it is released, so resetting it here
    // will have no effect.
    virtual
    bool ResetEvent(CLREventStatic* event) = 0;

    // Waits on an event for the given timeout amount of time (or infinitely, if INFINITE). The
    // wait will be alertable if "alertable" is true. Upon return, returns WAIT_TIMEOUT on timeout
    // or WAIT_OBJECT_0 when woken by another thread signalling the event. Any other return value
    // is an error.
    virtual
    uint32_t WaitOnEvent(CLREventStatic* event, uint32_t milliseconds, bool alertable) = 0;
};

#endif // _GCINTERFACE_EE_H_
