// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


/*++

Module Name:

    gc.h

--*/

#ifndef __GC_H
#define __GC_H

#include "gcinterface.h"

/*
 * Promotion Function Prototypes
 */
typedef void enum_func (Object*);

// callback functions for heap walkers
typedef void object_callback_func(void * pvContext, void * pvDataLoc);

// stub type to abstract a heap segment
struct gc_heap_segment_stub;
typedef gc_heap_segment_stub *segment_handle;

struct segment_info
{
    void * pvMem; // base of the allocation, not the first object (must add ibFirstObject)
    size_t ibFirstObject;   // offset to the base of the first object in the segment
    size_t ibAllocated; // limit of allocated memory in the segment (>= firstobject)
    size_t ibCommit; // limit of committed memory in the segment (>= alllocated)
    size_t ibReserved; // limit of reserved memory in the segment (>= commit)
};

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
/* If you modify failure_get_memory and         */
/* oom_reason be sure to make the corresponding */
/* changes in toolbox\sos\strike\strike.cpp.    */
/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
enum failure_get_memory
{
    fgm_no_failure = 0,
    fgm_reserve_segment = 1,
    fgm_commit_segment_beg = 2,
    fgm_commit_eph_segment = 3,
    fgm_grow_table = 4,
    fgm_commit_table = 5
};

struct fgm_history
{
    failure_get_memory fgm;
    size_t size;
    size_t available_pagefile_mb;
    BOOL loh_p;

    void set_fgm (failure_get_memory f, size_t s, BOOL l)
    {
        fgm = f;
        size = s;
        loh_p = l;
    }
};

enum oom_reason
{
    oom_no_failure = 0,
    oom_budget = 1,
    oom_cant_commit = 2,
    oom_cant_reserve = 3,
    oom_loh = 4,
    oom_low_mem = 5,
    oom_unproductive_full_gc = 6
};

struct oom_history
{
    oom_reason reason;
    size_t alloc_size;
    uint8_t* reserved;
    uint8_t* allocated;
    size_t gc_index;
    failure_get_memory fgm;
    size_t size;
    size_t available_pagefile_mb;
    BOOL loh_p;
};

/* forward declerations */
class CObjectHeader;
class Object;

class GCHeap;

/* misc defines */

#ifdef GC_CONFIG_DRIVEN
#define MAX_GLOBAL_GC_MECHANISMS_COUNT 6
GARY_DECL(size_t, gc_global_mechanisms, MAX_GLOBAL_GC_MECHANISMS_COUNT);
#endif //GC_CONFIG_DRIVEN


#ifdef DACCESS_COMPILE
class DacHeapWalker;
#endif

#ifdef _DEBUG
#define  _LOGALLOC
#endif

#define MP_LOCKS

namespace WKS {
    ::GCHeap* CreateGCHeap();
    class GCHeap;
    class gc_heap;
    }

#if defined(FEATURE_SVR_GC)
namespace SVR {
    ::GCHeap* CreateGCHeap();
    class GCHeap;
    class gc_heap;
}
#endif // defined(FEATURE_SVR_GC)

/*
 * Ephemeral Garbage Collected Heap Interface
 */

// This is the complete definition of alloc_context that is used
// within the GC, making use of the two reserved fields.
struct alloc_context : gc_alloc_context
{
#ifdef FEATURE_SVR_GC
    inline SVR::GCHeap*& alloc_heap() { return reinterpret_cast<SVR::GCHeap*&>(gc_reserved_1); }
    inline SVR::GCHeap*& home_heap() { return reinterpret_cast<SVR::GCHeap*&>(gc_reserved_2); }
#endif
};

// The above structure *CAN NOT* differ in size than the interface alloc context
// from which it derives, due to alloc_context being used by-value in the VM.
#ifdef static_assert_no_msg // the Sample build does not define this macro
static_assert_no_msg(sizeof(gc_alloc_context) == sizeof(alloc_context));
#endif // static_assert_no_msg

#ifdef STRESS_HEAP
#define IN_STRESS_HEAP(x) x
#define STRESS_HEAP_ARG(x) ,x
#else // STRESS_HEAP
#define IN_STRESS_HEAP(x)
#define STRESS_HEAP_ARG(x)
#endif // STRESS_HEAP

//dynamic data interface
struct gc_counters
{
    size_t current_size;
    size_t promoted_size;
    size_t collection_count;
};

enum bgc_state
{
    bgc_not_in_process = 0,
    bgc_initialized,
    bgc_reset_ww,
    bgc_mark_handles,
    bgc_mark_stack,
    bgc_revisit_soh,
    bgc_revisit_loh,
    bgc_overflow_soh,
    bgc_overflow_loh,
    bgc_final_marking,
    bgc_sweep_soh,
    bgc_sweep_loh,
    bgc_plan_phase
};

enum changed_seg_state
{
    seg_deleted,
    seg_added
};

void record_changed_seg (uint8_t* start, uint8_t* end,
                         size_t current_gc_index,
                         bgc_state current_bgc_state,
                         changed_seg_state changed_state);

#ifdef GC_CONFIG_DRIVEN
void record_global_mechanism (int mech_index);
#endif //GC_CONFIG_DRIVEN

class GCHeap : public IGCHeap {
    friend struct ::_DacGlobals;
#ifdef DACCESS_COMPILE
    friend class ClrDataAccess;
#endif
    
public:

    virtual ~GCHeap() {}

    static GCHeap *GetGCHeap()
    {
        LIMITED_METHOD_CONTRACT;

        _ASSERTE(g_pGCHeap != NULL);
        IGCHeap* heap = g_pGCHeap;
        return (GCHeap*)heap;
    }

#ifndef DACCESS_COMPILE
    static BOOL IsGCInProgress(BOOL bConsiderGCStart = FALSE) 
    {
        WRAPPER_NO_CONTRACT;

        return (IsGCHeapInitialized() ? GetGCHeap()->IsGCInProgressHelper(bConsiderGCStart) : false);
    }   
#endif
    
    static BOOL IsGCHeapInitialized()
    {
        LIMITED_METHOD_CONTRACT;

        return (g_pGCHeap != NULL);
    }

    static void WaitForGCCompletion(BOOL bConsiderGCStart = FALSE)
    {
        WRAPPER_NO_CONTRACT;

        if (IsGCHeapInitialized())
            GetGCHeap()->WaitUntilGCComplete(bConsiderGCStart);
    }   
    
    BOOL IsValidSegmentSize(size_t cbSize) 
    {
        //Must be aligned on a Mb and greater than 4Mb
        return (((cbSize & (1024*1024-1)) ==0) && (cbSize >> 22));
    }

    BOOL IsValidGen0MaxSize(size_t cbSize) 
    {
        return (cbSize >= 64*1024);
    }

    inline static bool UseAllocationContexts()
    {
        WRAPPER_NO_CONTRACT;
#ifdef FEATURE_REDHAWK
        // SIMPLIFY:  only use allocation contexts
        return true;
#else
#if defined(_TARGET_ARM_) || defined(FEATURE_PAL)
        return true;
#else
        return ((IsServerHeap() ? true : (g_SystemInfo.dwNumberOfProcessors >= 2)));
#endif
#endif 
    }

#ifndef DACCESS_COMPILE
    static GCHeap * CreateGCHeap()
    {
        WRAPPER_NO_CONTRACT;

        GCHeap * pGCHeap;
#if defined(FEATURE_SVR_GC)
        pGCHeap = (IsServerHeap() ? SVR::CreateGCHeap() : WKS::CreateGCHeap());
#else
        pGCHeap = WKS::CreateGCHeap();
#endif // defined(FEATURE_SVR_GC)

        g_pGCHeap = pGCHeap;
        return pGCHeap;
    }
#endif // DACCESS_COMPILE

private:
    typedef enum
    {
        GC_HEAP_INVALID = 0,
        GC_HEAP_WKS     = 1,
        GC_HEAP_SVR     = 2
    } GC_HEAP_TYPE;

#ifdef FEATURE_SVR_GC
    SVAL_DECL(uint32_t,gcHeapType);
#endif // FEATURE_SVR_GC

public:
        // TODO Synchronization, should be moved out
    virtual BOOL    IsGCInProgressHelper (BOOL bConsiderGCStart = FALSE) = 0;
    virtual uint32_t    WaitUntilGCComplete (BOOL bConsiderGCStart = FALSE) = 0;
    virtual void SetGCInProgress(BOOL fInProgress) = 0;
    virtual CLREventStatic * GetWaitForGCEvent() = 0;

    virtual void    SetFinalizationRun (Object* obj) = 0;
    virtual Object* GetNextFinalizable() = 0;
    virtual size_t GetNumberOfFinalizable() = 0;

    virtual void SetFinalizeQueueForShutdown(BOOL fHasLock) = 0;
    virtual BOOL FinalizeAppDomain(AppDomain *pDomain, BOOL fRunFinalizers) = 0;
    virtual BOOL ShouldRestartFinalizerWatchDog() = 0;

    //wait for concurrent GC to finish
    virtual void WaitUntilConcurrentGCComplete () = 0;                                  // Use in managed threads
#ifndef DACCESS_COMPILE    
    virtual HRESULT WaitUntilConcurrentGCCompleteAsync(int millisecondsTimeout) = 0;    // Use in native threads. TRUE if succeed. FALSE if failed or timeout
#endif    
    virtual BOOL IsConcurrentGCInProgress() = 0;

    // Enable/disable concurrent GC    
    virtual void TemporaryEnableConcurrentGC() = 0;
    virtual void TemporaryDisableConcurrentGC() = 0;
    virtual BOOL IsConcurrentGCEnabled() = 0;

    virtual void FixAllocContext (gc_alloc_context* acontext, BOOL lockp, void* arg, void *heap) = 0;
    virtual Object* Alloc (gc_alloc_context* acontext, size_t size, uint32_t flags) = 0;

    // This is safe to call only when EE is suspended.
    virtual Object* GetContainingObject(void *pInteriorPtr) = 0;

        // TODO Should be folded into constructor
    virtual HRESULT Initialize () = 0;

    virtual HRESULT GarbageCollect (int generation = -1, BOOL low_memory_p=FALSE, int mode = collection_blocking) = 0;
    virtual Object*  Alloc (size_t size, uint32_t flags) = 0;
#ifdef FEATURE_64BIT_ALIGNMENT
    virtual Object*  AllocAlign8 (size_t size, uint32_t flags) = 0;
    virtual Object*  AllocAlign8 (gc_alloc_context* acontext, size_t size, uint32_t flags) = 0;
private:
    virtual Object*  AllocAlign8Common (void* hp, gc_alloc_context* acontext, size_t size, uint32_t flags) = 0;
public:
#endif // FEATURE_64BIT_ALIGNMENT
    virtual Object*  AllocLHeap (size_t size, uint32_t flags) = 0;
    virtual void     SetReservedVMLimit (size_t vmlimit) = 0;
    virtual void SetCardsAfterBulkCopy( Object**, size_t ) = 0;
#if defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)
    virtual void WalkObject (Object* obj, walk_fn fn, void* context) = 0;
#endif //defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)

    virtual bool IsThreadUsingAllocationContextHeap(gc_alloc_context* acontext, int thread_number) = 0;
    virtual int GetNumberOfHeaps () = 0; 
    virtual int GetHomeHeapNumber () = 0;
    
    virtual int CollectionCount (int generation, int get_bgc_fgc_count = 0) = 0;

        // Finalizer queue stuff (should stay)
    virtual bool    RegisterForFinalization (int gen, Object* obj) = 0;

        // General queries to the GC
    virtual BOOL    IsPromoted (Object *object) = 0;
    virtual unsigned WhichGeneration (Object* object) = 0;
    virtual BOOL    IsEphemeral (Object* object) = 0;
    virtual BOOL    IsHeapPointer (void* object, BOOL small_heap_only = FALSE) = 0;

    virtual unsigned GetCondemnedGeneration() = 0;
    virtual int GetGcLatencyMode() = 0;
    virtual int SetGcLatencyMode(int newLatencyMode) = 0;

    virtual int GetLOHCompactionMode() = 0;
    virtual void SetLOHCompactionMode(int newLOHCompactionyMode) = 0;

    virtual BOOL RegisterForFullGCNotification(uint32_t gen2Percentage,
                                               uint32_t lohPercentage) = 0;
    virtual BOOL CancelFullGCNotification() = 0;
    virtual int WaitForFullGCApproach(int millisecondsTimeout) = 0;
    virtual int WaitForFullGCComplete(int millisecondsTimeout) = 0;

    virtual int StartNoGCRegion(uint64_t totalSize, BOOL lohSizeKnown, uint64_t lohSize, BOOL disallowFullBlockingGC) = 0;
    virtual int EndNoGCRegion() = 0;

    virtual BOOL IsObjectInFixedHeap(Object *pObj) = 0;
    virtual size_t  GetTotalBytesInUse () = 0;
    virtual size_t  GetCurrentObjSize() = 0;
    virtual size_t  GetLastGCStartTime(int generation) = 0;
    virtual size_t  GetLastGCDuration(int generation) = 0;
    virtual size_t  GetNow() = 0;
    virtual unsigned GetGcCount() = 0;
    virtual void TraceGCSegments() = 0;

    virtual void PublishObject(uint8_t* obj) = 0;

    // static if since restricting for all heaps is fine
    virtual size_t GetValidSegmentSize(BOOL large_seg = FALSE) = 0;

    static BOOL IsLargeObject(MethodTable *mt) {
        WRAPPER_NO_CONTRACT;

        return mt->GetBaseSize() >= LARGE_OBJECT_SIZE;
    }

    virtual unsigned GetMaxGeneration() = 0;

    virtual size_t GetPromotedBytes(int heap_index) = 0;

protected:
    enum {
        max_generation  = 2,
    };
    
public:

#ifdef FEATURE_BASICFREEZE
    // frozen segment management functions
    virtual segment_handle RegisterFrozenSegment(segment_info *pseginfo) = 0;
    virtual void UnregisterFrozenSegment(segment_handle seg) = 0;
#endif //FEATURE_BASICFREEZE

        // debug support 
#ifndef FEATURE_REDHAWK // Redhawk forces relocation a different way
#ifdef STRESS_HEAP
    //return TRUE if GC actually happens, otherwise FALSE
    virtual BOOL    StressHeap(gc_alloc_context * acontext = 0) = 0;
#endif
#endif // FEATURE_REDHAWK
#ifdef VERIFY_HEAP
    virtual void    ValidateObjectMember (Object *obj) = 0;
#endif

#if defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)
    virtual void DescrGenerationsToProfiler (gen_walk_fn fn, void *context) = 0;
#endif // defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)

protected: 
#ifdef VERIFY_HEAP
public:
    // Return NULL if can't find next object. When EE is not suspended,
    // the result is not accurate: if the input arg is in gen0, the function could 
    // return zeroed out memory as next object
    virtual Object * NextObj (Object * object) = 0;
#ifdef FEATURE_BASICFREEZE
    // Return TRUE if object lives in frozen segment
    virtual BOOL IsInFrozenSegment (Object * object) = 0;
#endif //FEATURE_BASICFREEZE
#endif //VERIFY_HEAP    
};

// Go through and touch (read) each page straddled by a memory block.
void TouchPages(void * pStart, size_t cb);

#ifdef WRITE_BARRIER_CHECK
void updateGCShadow(Object** ptr, Object* val);
#endif

// the method table for the WeakReference class
extern MethodTable  *pWeakReferenceMT;
// The canonical method table for WeakReference<T>
extern MethodTable  *pWeakReferenceOfTCanonMT;
extern void FinalizeWeakReference(Object * obj);

#endif // __GC_H
