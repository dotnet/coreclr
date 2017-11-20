// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 
// ProfAttach.cpp
// 

// 
// Definitions of functions that help with attaching and detaching profilers
// This is the main source of profattach.dll, which is used for profiler attach purposes 
// 

// ======================================================================================

#include <winwrap.h>
#include <utilcode.h>
#include <log.h>
#include <tlhelp32.h>
#include <cor.h>
#include <sstring.h>
#include "profattach.h"
#include <log.h>
#include <ex.h>
#include <getproductversionnumber.h>

#include "../../src/vm/profattach.h"
#include "../../src/vm/profattachclient.h"
#include "../../src/pal/prebuilt/inc/metahost.h"

#ifndef FEATURE_PAL
#define PSAPI_VERSION 2
#include <psapi.h>
#endif


#ifdef FEATURE_PAL
#define INITIALIZE_SHIM { if (PAL_InitializeDLL() != 0) return E_FAIL; }
#else
#define INITIALIZE_SHIM
#endif

// Contract for public APIs. These must be NOTHROW.
#define PUBLIC_CONTRACT \
    INITIALIZE_SHIM \
    CONTRACTL \
    { \
        NOTHROW; \
    } \
    CONTRACTL_END;

// Function exported by coreclr to get CLRProfiling
typedef HRESULT(STDAPICALLTYPE * fpICLRProfilingGetClassObject)(
    REFCLSID rclsid,
    REFIID riid,
    LPVOID * ppv);

HRESULT
CreateCLRProfiling(
    __in LPCWSTR pCoreCLRFullPath,
    __out LPVOID * ppCLRProfilingInstance)
{
    PUBLIC_CONTRACT;

    HRESULT hrIgnore = S_OK; // ignored HResult
    HRESULT hr = S_OK;
    HMODULE hMod = NULL;
    IUnknown * pCordb = NULL;

    LOG((LF_CORDB, LL_EVERYTHING, "Calling CreateCLRProfiling"));

    EX_TRY
    {
        SString szFullCoreClrPath;
        szFullCoreClrPath.Set(pCoreCLRFullPath, (COUNT_T)wcslen(pCoreCLRFullPath));

        // Issue:951525: coreclr mscordbi load fails on downlevel OS since LoadLibraryEx can't find 
        // dependent forwarder DLLs. Force LoadLibrary to look for dependencies in szFullDbiPath plus the default
        // search paths.
#ifndef FEATURE_PAL
        hMod = WszLoadLibraryEx(szFullCoreClrPath, NULL, LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
#else
        hMod = LoadLibraryExW(szFullCoreClrPath, NULL, 0);
#endif
    }
    EX_CATCH_HRESULT(hrIgnore); // failure leaves hMod null

    // Could not load or find coreclr
    if (hMod == NULL)
    {

    }
    
    // Now create instance
    fpICLRProfilingGetClassObject fpICLRProfiling = NULL;
    fpICLRProfiling = (fpICLRProfilingGetClassObject)GetProcAddress(hMod, "ICLRProfilingGetClassObject");

    IClassFactory * pFactory = NULL;
    hr = fpICLRProfiling(CLSID_CLRProfiling, IID_IClassFactory, (void**)&pFactory);
    if (SUCCEEDED(hr))
    {
        hr = pFactory->CreateInstance(NULL, CLSID_CLRProfiling, ppCLRProfilingInstance);
        pFactory->Release();
    }
    
    return hr;
}
