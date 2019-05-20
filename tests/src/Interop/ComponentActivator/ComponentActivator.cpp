// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <platformdefines.h>

#ifdef WINDOWS
#include "mscoree.h"

#include <stdexcept>
#include <iostream>
#include <stdint.h>
#include <stdio.h>

// Value copied from mscoree.h
const IID IID_ICLRRuntimeHost4 = {0x64F6D366, 0xD7C2, 0x4F1F, {0xB4, 0xB2, 0xE8, 0x16, 0x0C, 0xAC, 0x43, 0xAF}};

typedef HRESULT  (STDAPICALLTYPE *FnGetCLRRuntimeHost)(REFIID riid, IUnknown **pUnk);
typedef HRESULT (STDAPICALLTYPE *FnCoreClrCreateDelegate)(void* host,
                                                        int domainId,
                                                        const char* assemblyName,
                                                        const char* typeNameNative,
                                                        const char* methodNameNative,
                                                        void** functionHandle);
typedef HRESULT (STDAPICALLTYPE *FnNativeDelegate)(void*, uint32_t);
typedef HRESULT (STDAPICALLTYPE *FnCreateNativeDelegate)(const char* assemblyPathNative,
                                                        const char* typeNameNative,
                                                        const char* methodNameNative,
                                                        const char* delegateTypeNative,
                                                        FnNativeDelegate* functionHandle);

class CoreClrClient
{
    FnGetCLRRuntimeHost pfnGetCLRRuntimeHost;
    FnCoreClrCreateDelegate pfnCoreClrCreateDelegate;
    void* host;
    FnCreateNativeDelegate pfnCreateNativeDelegate;

public:
    CoreClrClient()
    {
        printf("GetModuleHandle\n");
        HMODULE coreCLRModule = ::GetModuleHandle(L"coreclr.dll");
        if (!coreCLRModule)
        {
            ReportError("coreCLRModule not found");
        }

        printf("GetCLRRuntimeHost\n");
        pfnGetCLRRuntimeHost = (FnGetCLRRuntimeHost)::GetProcAddress(coreCLRModule, "GetCLRRuntimeHost");
        if (!pfnGetCLRRuntimeHost)
        {
            ReportError("GetCLRRuntimeHost not found");
        }

        printf("coreclr_create_delegate\n");
        pfnCoreClrCreateDelegate = (FnCoreClrCreateDelegate)::GetProcAddress(coreCLRModule, "coreclr_create_delegate");
        if (!pfnCoreClrCreateDelegate)
        {
            ReportError("coreclr_create_delegate not found");
        }

        printf("IID_ICLRRuntimeHost4\n");
        HRESULT hr = pfnGetCLRRuntimeHost(IID_ICLRRuntimeHost4, (IUnknown**)&host);
        if (FAILED(hr))
        {
            ReportError("GetCLRRuntimeHost call failed");
        }

        if (!host)
        {
            ReportError("host is null");
        }

        printf("pfnCoreClrCreateDelegate\n");
        const int DefaultADID = 1;
        hr = pfnCoreClrCreateDelegate(host,
                                      DefaultADID,
                                      "System.Private.Corelib",
                                      "Internal.Runtime.InteropServices.ComponentActivator",
                                      "CreateNativeDelegate",
                                      (void**)&pfnCreateNativeDelegate);

        if (FAILED(hr))
        {
            ReportError("coreclr_create_delegate call failed");
        }

        if (!pfnCreateNativeDelegate)
        {
            ReportError("pfnCreateNativeDelegate is null");
        }
    }

    int CallNativeDelegate(const char* path,
                           const char* typeName,
                           const char* methodName,
                           void* args,
                           int32_t argSizeBytes)
    {
        FnNativeDelegate nativeDelegate;

        printf("pfnCreateNativeDelegate\n");
        HRESULT hr = pfnCreateNativeDelegate(path, typeName, methodName, nullptr, &nativeDelegate);

        if (FAILED(hr))
        {
            ReportWarning("CreateNativeDelegate call failed");
            return hr;
        }

        if (!nativeDelegate)
        {
            ReportError("nativeDelegate is null");
        }

        printf("nativeDelegate\n");
        return nativeDelegate(args, argSizeBytes);
    }

    void ReportWarning(const char*message)
    {
        std::cerr << message;
    }

    void ReportError(const char*message)
    {
        std::cerr << message;

        throw new std::runtime_error(message);
    }
};

extern "C" DLL_EXPORT int STDMETHODCALLTYPE
CallNativeDelegate(const char* path,
                   const char* typeName,
                   const char* methodName,
                   void* args,
                   int32_t argSizeBytes)
{
    printf("CallNativeDelegate('%s', '%s', '%s', %p, %d)\n", path, typeName, methodName, args, argSizeBytes);
    static CoreClrClient coreClrClient;

    return coreClrClient.CallNativeDelegate(path, typeName, methodName, args, argSizeBytes);
}

#else // WINDOWS

#include <cstdint>

typedef uint32_t DWORD;

extern "C" DLL_EXPORT int STDMETHODCALLTYPE
CallNativeDelegate(const char* path,
                   const char* typeName,
                   const char* methodName,
                   void* args,
                   int32_t argSizeBytes)
{
    const int E_FAIL = 0x80004005;

    return E_FAIL;
}

#endif // WINDOWS
