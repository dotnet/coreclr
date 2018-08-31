// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "CoreShim.h"

#include <vector>

namespace
{
    HRESULT InitializeCoreClr(_In_ coreclr* inst)
    {
        assert(inst != nullptr);

        HRESULT hr;

        std::string tpaList;
        RETURN_IF_FAILED(coreclr::CreateTpaList(tpaList));

        const char *keys[] =
        {
            "APP_PATHS",
            "TRUSTED_PLATFORM_ASSEMBLIES",
        };

        // [TODO] Support UNICODE app path
        char wd[MAX_PATH];
        (void)::GetCurrentDirectoryA(ARRAYSIZE(wd), wd);

        const char *values[] =
        {
            wd,
            tpaList.c_str(),
        };

        static_assert(ARRAYSIZE(keys) == ARRAYSIZE(values), "key/values pairs should match in length");

        return inst->Initialize(ARRAYSIZE(keys), keys, values, "COMAct");
    }

    HRESULT GetActivationAssemblies(_Inout_ std::vector<std::wstring> &assemblies, _Inout_ std::vector<const WCHAR *> &assembliesRaw)
    {
        assert(assemblies.empty());

        SIZE_T dataSize;

        // This data block is re-used multiple times.
        // Using the maximum possible size that could ever be needed in this function.
        BYTE data[sizeof(ACTIVATION_CONTEXT_ASSEMBLY_DETAILED_INFORMATION) + (MAX_PATH * 4)];

        // Get current context details
        BOOL suc = ::QueryActCtxW(
            QUERY_ACTCTX_FLAG_USE_ACTIVE_ACTCTX,
            nullptr,
            nullptr,
            ActivationContextDetailedInformation,
            data,
            ARRAYSIZE(data),
            &dataSize);
        if (suc == FALSE)
            return HRESULT_FROM_WIN32(::GetLastError());

        auto cdi = reinterpret_cast<PACTIVATION_CONTEXT_DETAILED_INFORMATION>(data);
        const DWORD assemblyCount = cdi->ulAssemblyCount;

        try
        {
            // Assembly index '0' is reserved and count doesn't include the reserved entry.
            for (DWORD adidx = 1; adidx <= assemblyCount; ++adidx)
            {
                // Collect all assemblies involved in the current context
                suc = ::QueryActCtxW(
                    QUERY_ACTCTX_FLAG_USE_ACTIVE_ACTCTX,
                    nullptr,
                    &adidx,
                    AssemblyDetailedInformationInActivationContext,
                    data,
                    ARRAYSIZE(data),
                    &dataSize);
                if (suc == FALSE)
                    return HRESULT_FROM_WIN32(::GetLastError());

                auto adi = reinterpret_cast<PACTIVATION_CONTEXT_ASSEMBLY_DETAILED_INFORMATION>(data);
                if (adi->ulManifestPathLength > 0)
                {
                    assemblies.push_back({ adi->lpAssemblyManifestPath });
                    assembliesRaw.push_back(assemblies.back().c_str());
                }
            }
        }
        catch (const std::bad_alloc&)
        {
            return E_OUTOFMEMORY;
        }

        return S_OK;
    }
}

STDAPI DllGetClassObject(
    _In_ REFCLSID rclsid,
    _In_ REFIID riid,
    _Outptr_ LPVOID FAR* ppv)
{
    HRESULT hr;

    coreclr *inst;
    RETURN_IF_FAILED(coreclr::GetCoreClrInstance(&inst));

    if (hr == S_OK)
        RETURN_IF_FAILED(InitializeCoreClr(inst));

    using GetClassFactoryForTypeInternal_ptr = HRESULT(*)(void *);
    GetClassFactoryForTypeInternal_ptr GetClassFactoryForTypeInternal;
    RETURN_IF_FAILED(inst->CreateDelegate(
        "System.Private.CoreLib",
        "System.Runtime.InteropServices.ComActivator",
        "GetClassFactoryForTypeInternal", (void**)&GetClassFactoryForTypeInternal));

    // Get all activation assemblies
    std::vector<std::wstring> assemblies;
    std::vector<const WCHAR *> assembliesRaw;
    RETURN_IF_FAILED(GetActivationAssemblies(assemblies, assembliesRaw));

    IUnknown *ccw = nullptr;

    struct ComActivationContext
    {
        GUID ClassId;
        GUID InterfaceId;
        DWORD AssemblyCount;
        void *AssemblyList;
        void **ClassFactoryDest;
    } comCxt{ rclsid, riid, static_cast<DWORD>(assembliesRaw.size()), assembliesRaw.data(), (void**)&ccw };

    RETURN_IF_FAILED(GetClassFactoryForTypeInternal(&comCxt));
    assert(ccw != nullptr);

    hr = ccw->QueryInterface(riid, ppv);
    ccw->Release();
    return hr;
}

STDAPI DllCanUnloadNow(void)
{
    return S_FALSE;
}
