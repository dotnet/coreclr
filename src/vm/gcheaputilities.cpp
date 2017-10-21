// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gcheaputilities.h"
#include "gcenv.ee.h"
#include "appdomain.hpp"


// These globals are variables used within the GC and maintained
// by the EE for use in write barriers. It is the responsibility
// of the GC to communicate updates to these globals to the EE through
// GCToEEInterface::StompWriteBarrierResize and GCToEEInterface::StompWriteBarrierEphemeral.
GPTR_IMPL_INIT(uint32_t, g_card_table,      nullptr);
GPTR_IMPL_INIT(uint8_t,  g_lowest_address,  nullptr);
GPTR_IMPL_INIT(uint8_t,  g_highest_address, nullptr);
GVAL_IMPL_INIT(GCHeapType, g_heap_type,     GC_HEAP_INVALID);
uint8_t* g_ephemeral_low  = (uint8_t*)1;
uint8_t* g_ephemeral_high = (uint8_t*)~0;

#ifdef FEATURE_MANUALLY_MANAGED_CARD_BUNDLES
uint32_t* g_card_bundle_table = nullptr;
#endif

// This is the global GC heap, maintained by the VM.
GPTR_IMPL(IGCHeap, g_pGCHeap);

GcDacVars g_gc_dac_vars;
GPTR_IMPL(GcDacVars, g_gcDacGlobals);

#ifdef FEATURE_USE_SOFTWARE_WRITE_WATCH_FOR_GC_HEAP

uint8_t* g_sw_ww_table = nullptr;
bool g_sw_ww_enabled_for_gc_heap = false;

#endif // FEATURE_USE_SOFTWARE_WRITE_WATCH_FOR_GC_HEAP

gc_alloc_context g_global_alloc_context = {};

enum GC_LOAD_STATUS {
    GC_LOAD_STATUS_START,
    GC_LOAD_STATUS_DONE_LOAD,
    GC_LOAD_STATUS_GET_VERSIONINFO,
    GC_LOAD_STATUS_CALL_VERSIONINFO,
    GC_LOAD_STATUS_DONE_VERSION_CHECK,
    GC_LOAD_STATUS_GET_INITIALIZE,
    GC_LOAD_STATUS_LOAD_COMPLETE
};

GC_LOAD_STATUS g_gc_load_status = GC_LOAD_STATUS_START;
VersionInfo g_gc_version_info;

extern "C" void GC_VersionInfo(/* Out */ VersionInfo* info);
extern "C" HRESULT GC_Initialize(
    /* In  */ IGCToCLR* clrToGC,
    /* Out */ IGCHeap** gcHeap,
    /* Out */ IGCHandleManager** gcHandleManager,
    /* Out */ GcDacVars* gcDacVars
);

#ifndef DACCESS_COMPILE

HRESULT GCHeapUtilities::InitializeAndLoad()
{
    LIMITED_METHOD_CONTRACT;

    // we should only call this once on startup. Attempting to load a GFC
    // twice is an error.
    assert(g_pGCHeap == nullptr);

    // we should not have attempted to load a GC already. Attempting a
    // load after the first load already failed is an error.
    assert(g_gc_load_status == GC_LOAD_STATUS_START);

    TCHAR* standaloneGcLocation = nullptr;
    CLRConfig::GetConfigValue(CLRConfig::EXTERNAL_GCName, &standaloneGcLocation);

    HMODULE hMod;
    IGCToCLR* gcToClr;
    if (!standaloneGcLocation)
    {
        LOG((LF_GC, LL_INFO100, "Standalone GC location not provided, using provided GC\n"));
        hMod = GetModuleInst();
        assert(hMod != nullptr);

        // a non-standalone GC links directly against the EE and invokes methods on GCToEEInterface
        // directly
        gcToClr = nullptr;
    }
    else
    {
#ifndef FEATURE_STANDALONE_GC
        LOG((LF_GC, LL_FATALERROR, "EE not built with the ability to load standalone GCs"));
        return E_FAIL;
#else
        LOG((LF_GC, LL_INFO100, "Loading standalone GC from path %S\n", standaloneGcLocation));
        hMod = CLRLoadLibrary(standaloneGcLocation);
        if (!hMod)
        {
            HRESULT err = GetLastError();
            LOG((LF_GC, LL_FATALERROR, "Load of %S failed\n", standaloneGcLocation));
            return err;
        }

        // a standalone GC dispatches virtually on GCToEEInterface.
        gcToClr = new (nothrow) standalone::GCToEEInterface();
        if (!gcToClr)
        {
            return E_OUTOFMEMORY;
        }
#endif // FEATURE_STANDALONE_GC
    }

    g_gc_load_status = GC_LOAD_STATUS_DONE_LOAD;
    GC_VersionInfoFunction versionInfo = (GC_VersionInfoFunction)GetProcAddress(hMod, "GC_VersionInfo");
    if (!versionInfo)
    {
        HRESULT err = GetLastError();
        LOG((LF_GC, LL_FATALERROR, "Load of `GC_VersionInfo` from standalone GC failed\n"));
        return err;
    }

    g_gc_load_status = GC_LOAD_STATUS_GET_VERSIONINFO;
    versionInfo(&g_gc_version_info);
    g_gc_load_status = GC_LOAD_STATUS_CALL_VERSIONINFO;

    if (g_gc_version_info.MajorVersion != GC_INTERFACE_MAJOR_VERSION)
    {
        LOG((LF_GC, LL_FATALERROR, "Loaded GC has incompatible major version number (expected %d, got %d)\n",
            GC_INTERFACE_MAJOR_VERSION, g_gc_version_info.MajorVersion));
        return E_FAIL;
    }

    if (g_gc_version_info.MinorVersion < GC_INTERFACE_MINOR_VERSION)
    {
        LOG((LF_GC, LL_INFO100, "Loaded GC has lower minor version number (%d) than EE was compiled against (%d)\n",
            g_gc_version_info.MinorVersion, GC_INTERFACE_MINOR_VERSION));
    }

    LOG((LF_GC, LL_INFO100, "Loaded GC identifying itself with name `%s`\n", g_gc_version_info.Name));
    g_gc_load_status = GC_LOAD_STATUS_DONE_VERSION_CHECK;
    GC_InitializeFunction initFunc = (GC_InitializeFunction)GetProcAddress(hMod, "GC_Initialize");
    if (!initFunc)
    {
        HRESULT err = GetLastError();
        LOG((LF_GC, LL_FATALERROR, "Load of `GC_Initialize` from standalone GC failed\n"));
        return err;
    }

    g_gc_load_status = GC_LOAD_STATUS_GET_INITIALIZE;
    IGCHeap* heap;
    IGCHandleManager* manager;
    HRESULT initResult = initFunc(gcToClr, &heap, &manager, &g_gc_dac_vars);
    if (initResult == S_OK)
    {
        g_pGCHeap = heap;
        g_pGCHandleManager = manager;
        g_gcDacGlobals = &g_gc_dac_vars;
        g_gc_load_status = GC_LOAD_STATUS_DONE_LOAD;
        LOG((LF_GC, LL_INFO100, "GC load successful\n"));
    }

    LOG((LF_GC, LL_INFO100, "GC initialization failed with HR = 0x%X\n", initResult));
    return initResult;
}

#endif // DACCESS_COMPILE
