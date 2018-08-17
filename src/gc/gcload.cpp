// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * gcload.cpp
 * 
 * Code for loading and initializing the GC. The code in this file
 * is used in the startup path of both a standalone and non-standalone GC.
 */

#include "common.h"
#include "gcenv.h"
#include "gc.h"

#ifdef _MSC_VER
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT __attribute__ ((visibility ("default")))
#endif // _MSC_VER

#define GC_EXPORT extern "C" DLLEXPORT

// These symbols are defined in gc.cpp and populate the GcDacVars
// structure with the addresses of DAC variables within the GC.
namespace WKS 
{
    extern void PopulateDacVars(GcDacVars* dacVars);
}

namespace SVR
{
    extern void PopulateDacVars(GcDacVars* dacVars);
}

// This symbol populates GcDacVars with handle table dacvars.
extern void PopulateHandleTableDacVars(GcDacVars* dacVars);

bool g_fIsNumaAwareEnabledByConfig;
bool g_fIsCPUGroupEnabledByConfig;

GC_EXPORT
void
GC_VersionInfo(/* Out */ VersionInfo* info)
{
    info->MajorVersion = GC_INTERFACE_MAJOR_VERSION;
    info->MinorVersion = GC_INTERFACE_MINOR_VERSION;
    info->BuildVersion = 0;
    info->Name = "CoreCLR GC";
}

GC_EXPORT
HRESULT
GC_Initialize(
    /* In  */ IGCToCLR* clrToGC,
    /* Out */ IGCHeap** gcHeap,
    /* Out */ IGCHandleManager** gcHandleManager,
    /* Out */ GcDacVars* gcDacVars
)
{
    IGCHeapInternal* heap;
    
    assert(gcDacVars != nullptr);
    assert(gcHeap != nullptr);
    assert(gcHandleManager != nullptr);

#ifdef BUILD_AS_STANDALONE
    assert(clrToGC != nullptr);
    g_theGCToCLR = clrToGC;
#else
    UNREFERENCED_PARAMETER(clrToGC);
    assert(clrToGC == nullptr);
#endif
    
    // Initialize GCConfig before anything else - initialization of our
    // various components may want to query the current configuration.
    GCConfig::Initialize();

    // Config items for GCToOsInterface
    g_fIsNumaAwareEnabledByConfig = GCConfig::GetGCNumaAware();
    g_fIsCPUGroupEnabledByConfig = GCConfig::GetGCCpuGroup();

    if (!GCToOSInterface::Initialize())
    {
        return E_FAIL;
    }

    IGCHandleManager* handleManager = CreateGCHandleManager();
    if (handleManager == nullptr)
    {
        return E_OUTOFMEMORY;
    }

#ifdef FEATURE_SVR_GC
    if (GCConfig::GetServerGC())
    {
#ifdef WRITE_BARRIER_CHECK
        g_GCShadow = 0;
        g_GCShadowEnd = 0;
#endif // WRITE_BARRIER_CHECK

        g_gc_heap_type = GC_HEAP_SVR;
        heap = SVR::CreateGCHeap();
        SVR::PopulateDacVars(gcDacVars);
    }
    else
    {
        g_gc_heap_type = GC_HEAP_WKS;
        heap = WKS::CreateGCHeap();
        WKS::PopulateDacVars(gcDacVars);
    }
#else
    g_gc_heap_type = GC_HEAP_WKS;
    heap = WKS::CreateGCHeap();
    WKS::PopulateDacVars(gcDacVars);
#endif

    PopulateHandleTableDacVars(gcDacVars);
    if (heap == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    g_theGCHeap = heap;
    *gcHandleManager = handleManager;
    *gcHeap = heap;
    return S_OK;
}
