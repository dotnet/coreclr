
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _GC_INTERFACE_H_
#define _GC_INTERFACE_H_

// The allocation context must be known to the VM for use in the allocation
// fast path and known to the GC for performing the allocation. Every Thread
// has its own allocation context that it hands to the GC when allocating.
struct gc_alloc_context
{
    uint8_t*       alloc_ptr;
    uint8_t*       alloc_limit;
    int64_t        alloc_bytes; //Number of bytes allocated on SOH by this context
    int64_t        alloc_bytes_loh; //Number of bytes allocated on LOH by this context
    // These two fields are deliberately not exposed past the EE-GC interface.
    void*          gc_reserved_1;
    void*          gc_reserved_2;
    int            alloc_count;
public:

    void init()
    {
        LIMITED_METHOD_CONTRACT;

        alloc_ptr = 0;
        alloc_limit = 0;
        alloc_bytes = 0;
        alloc_bytes_loh = 0;
        gc_reserved_1 = 0;
        gc_reserved_1 = 0;
        alloc_count = 0;
    }
};

#ifdef PROFILING_SUPPORTED
#define GC_PROFILING       //Turn on profiling
#endif // PROFILING_SUPPORTED

#define LARGE_OBJECT_SIZE ((size_t)(85000))

class Object;
class IGCHeap;
class ICLRToGC;

GPTR_DECL(IGCHeap, g_pGCHeap);

// Initializes the garbage collector. Should only be called
// once, during EE startup.
IGCHeap* InitializeGarbageCollector(ICLRToGC* clrToGC);

#ifndef DACCESS_COMPILE
extern "C" {
#endif // !DACCESS_COMPILE
GPTR_DECL(uint8_t,g_lowest_address);
GPTR_DECL(uint8_t,g_highest_address);
GPTR_DECL(uint32_t,g_card_table);
#ifndef DACCESS_COMPILE
} 
#endif // !DACCESS_COMPILE

extern "C" uint8_t* g_ephemeral_low;
extern "C" uint8_t* g_ephemeral_high;

#ifdef WRITE_BARRIER_CHECK
//always defined, but should be 0 in Server GC
extern uint8_t* g_GCShadow;
extern uint8_t* g_GCShadowEnd;
// saves the g_lowest_address in between GCs to verify the consistency of the shadow segment
extern uint8_t* g_shadow_lowest_address;
#endif

// For low memory notification from host
extern int32_t g_bLowMemoryFromHost;

extern VOLATILE(int32_t) m_GCLock;

// !!!!!!!!!!!!!!!!!!!!!!!
// make sure you change the def in bcl\system\gc.cs 
// if you change this!
enum collection_mode
{
    collection_non_blocking = 0x00000001,
    collection_blocking = 0x00000002,
    collection_optimized = 0x00000004,
    collection_compacting = 0x00000008
#ifdef STRESS_HEAP
    , collection_gcstress = 0x80000000
#endif // STRESS_HEAP
};

// !!!!!!!!!!!!!!!!!!!!!!!
// make sure you change the def in bcl\system\gc.cs 
// if you change this!
enum wait_full_gc_status
{
    wait_full_gc_success = 0,
    wait_full_gc_failed = 1,
    wait_full_gc_cancelled = 2,
    wait_full_gc_timeout = 3,
    wait_full_gc_na = 4
};

// !!!!!!!!!!!!!!!!!!!!!!!
// make sure you change the def in bcl\system\gc.cs 
// if you change this!
enum start_no_gc_region_status
{
    start_no_gc_success = 0,
    start_no_gc_no_memory = 1,
    start_no_gc_too_large = 2,
    start_no_gc_in_progress = 3
};

enum end_no_gc_region_status
{
    end_no_gc_success = 0,
    end_no_gc_not_in_progress = 1,
    end_no_gc_induced = 2,
    end_no_gc_alloc_exceeded = 3
};

typedef BOOL (* walk_fn)(Object*, void*);
typedef void (* gen_walk_fn)(void *context, int generation, uint8_t *range_start, uint8_t * range_end, uint8_t *range_reserved);

// IGCHeap is the interface that the VM will use when interacting with the GC.
class IGCHeap {
public:
    virtual HRESULT Initialize() = 0;
    virtual BOOL IsPromoted(Object *object) = 0;
    virtual BOOL IsHeapPointer(void* object, BOOL small_heap_only = FALSE) = 0;
    virtual Object* GetNextFinalizable() = 0;
    virtual unsigned WhichGeneration(Object* obj) = 0;
    virtual size_t GetTotalBytesInUse() = 0;
    virtual unsigned GetCondemnedGeneration() = 0;
    virtual int GetGcLatencyMode() = 0;
    virtual int SetGcLatencyMode(int newLatencyMode) = 0;
    virtual int GetLOHCompactionMode() = 0;
    virtual void SetLOHCompactionMode(int newLOHCompactionMode) = 0;
    virtual BOOL RegisterForFullGCNotification(uint32_t gen2Percentage, uint32_t lohPercentage) = 0;
    virtual BOOL CancelFullGCNotification() = 0;
    virtual int WaitForFullGCApproach(int millisecondsTimeout) = 0;
    virtual int WaitForFullGCComplete(int millisecondsTimeout) = 0;
    virtual int CollectionCount(int generation, int get_bgc_fgc_coutn = 0) = 0;
    virtual size_t GetLastGCStartTime(int generation) = 0;
    virtual size_t GetLastGCDuration(int generation) = 0;
    virtual size_t GetNow() = 0;

    // TODO(segilles) these are all referenced by the VM. Are all of them
    // essential?
    virtual BOOL    IsGCInProgressHelper (BOOL bConsiderGCStart = FALSE) = 0;
    virtual unsigned GetMaxGeneration() = 0;
    virtual void SetCardsAfterBulkCopy(Object** obj, size_t length) = 0;
    virtual BOOL IsValidSegmentSize(size_t size) = 0;
    virtual BOOL IsValidGen0MaxSize(size_t size) = 0;
    virtual HRESULT GarbageCollect(int generation = -1, BOOL low_memory_p = FALSE, int mode = collection_blocking) = 0;
    virtual void WaitUntilConcurrentGCComplete() = 0;
    virtual BOOL IsConcurrentGCInProgress() = 0;
    virtual size_t GetValidSegmentSize(BOOL large_seg = FALSE) = 0;
    virtual BOOL FinalizeAppDomain(AppDomain *pDomain, BOOL fRunFinalizers) = 0;
    virtual void SetFinalizeQueueForShutdown(BOOL fHasLock) = 0;
    virtual size_t GetNumberOfFinalizable() = 0;
    virtual BOOL ShouldRestartFinalizerWatchDog() = 0;
    virtual unsigned GetGcCount() = 0;
    virtual bool IsThreadUsingAllocationContextHeap(gc_alloc_context* acontext, int thread_number) = 0;
    virtual BOOL IsObjectInFixedHeap(Object *pObj) = 0;
    virtual BOOL IsEphemeral (Object* object) = 0;
    virtual uint32_t WaitUntilGCComplete (BOOL bConsiderGCStart = FALSE) = 0;
    virtual void FixAllocContext (gc_alloc_context* acontext, BOOL lockp, void* arg, void *heap) = 0;
    virtual int StartNoGCRegion(uint64_t totalSize, BOOL lohSizeKnown, uint64_t lohSize, BOOL disallowFullBlockingGC) = 0;
    virtual int EndNoGCRegion() = 0;
    virtual void SetFinalizationRun (Object* obj) = 0;
    virtual bool RegisterForFinalization (int gen, Object* obj) = 0;
    virtual size_t GetCurrentObjSize() = 0;
    virtual void TemporaryEnableConcurrentGC() = 0;
    virtual void TemporaryDisableConcurrentGC() = 0;
    virtual BOOL IsConcurrentGCEnabled() = 0;
    virtual Object* Alloc (gc_alloc_context* acontext, size_t size, uint32_t flags) = 0;
    virtual Object* Alloc (size_t size, uint32_t flags) = 0;
    virtual Object* AllocLHeap (size_t size, uint32_t flags) = 0;
    virtual void PublishObject(uint8_t* obj) = 0;
    virtual Object* GetContainingObject(void *pInteriorPtr) = 0;
    virtual void SetGCInProgress(BOOL fInProgress) = 0;
    virtual CLREventStatic * GetWaitForGCEvent() = 0;
    virtual void TraceGCSegments() = 0;

#ifdef VERIFY_HEAP
    virtual void    ValidateObjectMember (Object *obj) = 0;
    virtual Object * NextObj (Object * object) = 0;
#endif // VERIFY_HEAP

#ifndef DACCESS_COMPILE    
    virtual HRESULT WaitUntilConcurrentGCCompleteAsync(int millisecondsTimeout) = 0;    // Use in native threads. TRUE if succeed. FALSE if failed or timeout
#endif

#if defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)
    virtual void WalkObject (Object* obj, walk_fn fn, void* context) = 0;
    virtual void DescrGenerationsToProfiler (gen_walk_fn fn, void *context) = 0;
#endif //defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)    

    IGCHeap() {}
    virtual ~IGCHeap() {}

#ifdef STRESS_HEAP
    //return TRUE if GC actually happens, otherwise FALSE
    virtual BOOL    StressHeap(gc_alloc_context * acontext = 0) = 0;
#endif // STRESS_HEAP

    inline static IGCHeap* GetGCHeap()
    {
        assert(g_pGCHeap != nullptr);
        return g_pGCHeap;
    }

#ifndef DACCESS_COMPILE
    inline static IGCHeap* CreateGCHeap(ICLRToGC* clrToGC)
    {
        IGCHeap* heap = InitializeGarbageCollector(clrToGC);
        assert(heap != nullptr);
        g_pGCHeap = heap;
        return g_pGCHeap;
    }
#endif

    inline static bool IsGCHeapInitialized()
    {
        return g_pGCHeap != nullptr;
    }

    inline static BOOL IsGCInProgress(BOOL bConsiderGCStart = FALSE) 
    {
        WRAPPER_NO_CONTRACT;

        return (IsGCHeapInitialized() ? GetGCHeap()->IsGCInProgressHelper(bConsiderGCStart) : false);
    }

    inline static BOOL MarkShouldCompeteForStatics()
    {
        WRAPPER_NO_CONTRACT;

        return IsServerHeap() && g_SystemInfo.dwNumberOfProcessors >= 2;
    }

    inline static void WaitForGCCompletion(BOOL bConsiderGCStart = FALSE)
    {
        WRAPPER_NO_CONTRACT;

        if (IsGCHeapInitialized())
            GetGCHeap()->WaitUntilGCComplete(bConsiderGCStart);
    }

    // The runtime needs to know whether we're using workstation or server GC 
    // long before the GCHeap is created.  So IsServerHeap cannot be a virtual 
    // method on GCHeap.  Instead we make it a static method and initialize 
    // gcHeapType before any of the calls to IsServerHeap.  Note that this also 
    // has the advantage of getting the answer without an indirection
    // (virtual call), which is important for perf critical codepaths.
#ifndef DACCESS_COMPILE
    inline static void InitializeHeapType(bool bServerHeap)
    {
        LIMITED_METHOD_CONTRACT;
#ifdef FEATURE_SVR_GC
        gcHeapType = bServerHeap ? GC_HEAP_SVR : GC_HEAP_WKS;
#ifdef WRITE_BARRIER_CHECK
        if (gcHeapType == GC_HEAP_SVR)
        {
            g_GCShadow = 0;
            g_GCShadowEnd = 0;
        }
#endif // WRITE_BARRIER_CHECK
#else // FEATURE_SVR_GC
        UNREFERENCED_PARAMETER(bServerHeap);
        CONSISTENCY_CHECK(bServerHeap == false);
#endif // FEATURE_SVR_GC
    }
#endif // DACCESS_COMPILE

    inline static bool IsServerHeap() 
    {
        LIMITED_METHOD_CONTRACT;
#ifdef FEATURE_SVR_GC
        _ASSERTE(gcHeapType != GC_HEAP_INVALID);
        return (gcHeapType == GC_HEAP_SVR);
#else // FEATURE_SVR_GC
        return false;
#endif // FEATURE_SVR_GC
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

    typedef enum
    {
        GC_HEAP_INVALID = 0,
        GC_HEAP_WKS     = 1,
        GC_HEAP_SVR     = 2
    } GC_HEAP_TYPE;

#ifdef FEATURE_SVR_GC
    SVAL_DECL(uint32_t, gcHeapType); 
#endif
};

#ifdef WRITE_BARRIER_CHECK
void updateGCShadow(Object** ptr, Object* val);
#endif

//constants for the flags parameter to the gc call back

#define GC_CALL_INTERIOR            0x1
#define GC_CALL_PINNED              0x2
#define GC_CALL_CHECK_APP_DOMAIN    0x4

//flags for IGCHeapAlloc(...)
#define GC_ALLOC_FINALIZE 0x1
#define GC_ALLOC_CONTAINS_REF 0x2
#define GC_ALLOC_ALIGN8_BIAS 0x4
#define GC_ALLOC_ALIGN8 0x8

// TODO(segilles) Does this belong here?
struct ScanContext
{
    Thread* thread_under_crawl;
    int thread_number;
    uintptr_t stack_limit; // Lowest point on the thread stack that the scanning logic is permitted to read
    BOOL promotion; //TRUE: Promotion, FALSE: Relocation.
    BOOL concurrent; //TRUE: concurrent scanning 
#if CHECK_APP_DOMAIN_LEAKS || defined (FEATURE_APPDOMAIN_RESOURCE_MONITORING) || defined (DACCESS_COMPILE)
    AppDomain *pCurrentDomain;
#endif //CHECK_APP_DOMAIN_LEAKS || FEATURE_APPDOMAIN_RESOURCE_MONITORING || DACCESS_COMPILE

#ifndef FEATURE_REDHAWK
#if defined(GC_PROFILING) || defined (DACCESS_COMPILE)
    MethodDesc *pMD;
#endif //GC_PROFILING || DACCESS_COMPILE
#endif // FEATURE_REDHAWK
#if defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)
    EtwGCRootKind dwEtwRootKind;
#endif // GC_PROFILING || FEATURE_EVENT_TRACE
    
    ScanContext()
    {
        LIMITED_METHOD_CONTRACT;

        thread_under_crawl = 0;
        thread_number = -1;
        stack_limit = 0;
        promotion = FALSE;
        concurrent = FALSE;
#ifdef GC_PROFILING
        pMD = NULL;
#endif //GC_PROFILING
#ifdef FEATURE_EVENT_TRACE
        dwEtwRootKind = kEtwGCRootKindOther;
#endif // FEATURE_EVENT_TRACE
    }
};

#if defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)
struct ProfilingScanContext : ScanContext
{
    BOOL fProfilerPinned;
    void * pvEtwContext;
    void *pHeapId;
    
    ProfilingScanContext(BOOL fProfilerPinnedParam) : ScanContext()
    {
        LIMITED_METHOD_CONTRACT;

        pHeapId = NULL;
        fProfilerPinned = fProfilerPinnedParam;
        pvEtwContext = NULL;
#ifdef FEATURE_CONSERVATIVE_GC
        // To not confuse GCScan::GcScanRoots
        promotion = g_pConfig->GetGCConservative();
#endif
    }
};
#endif // defined(GC_PROFILING) || defined(FEATURE_EVENT_TRACE)

#endif // _GC_INTERFACE_H_