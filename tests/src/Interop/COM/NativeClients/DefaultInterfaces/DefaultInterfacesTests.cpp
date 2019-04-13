// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <cassert>
#include <Server.Contracts.h>

// COM headers
#include <objbase.h>
#include <combaseapi.h>

#define COM_CLIENT
#include <Servers.h>

#define THROW_IF_FAILED(exp) { hr = exp; if (FAILED(hr)) { ::printf("FAILURE: 0x%08x = %s\n", hr, #exp); throw hr; } }
#define THROW_FAIL_IF_FALSE(exp) { if (!(exp)) { ::printf("FALSE: %s\n", #exp); throw E_FAIL; } }

template<COINIT TM>
struct ComInit
{
    const HRESULT Result;

    ComInit()
        : Result{ ::CoInitializeEx(nullptr, TM) }
    { }

    ~ComInit()
    {
        if (SUCCEEDED(Result))
            ::CoUninitialize();
    }
};

using ComMTA = ComInit<COINIT_MULTITHREADED>;

void CallDefaultInterface();

int __cdecl main()
{
    ComMTA init;
    if (FAILED(init.Result))
        return -1;

    try
    {
        CoreShimComActivation csact{ W("NetServer.DefaultInterfaces"), W("DefaultInterfaceTesting") };

        CallDefaultInterface();
    }
    catch (HRESULT hr)
    {
        ::printf("Test Failure: 0x%08x\n", hr);
        return 101;
    }

    return 100;
}

void CallDefaultInterface()
{
    ::printf("Call functions on Default Interface...\n");

    HRESULT hr;

    ComSmartPtr<IDefaultInterfaceTesting> defInterface;
    hr = ::CoCreateInstance(CLSID_DefaultInterfaceTesting, nullptr, CLSCTX_INPROC, IID_IDefaultInterfaceTesting, (void**)&defInterface)

    const int COR_E_INVALIDOPERATION = 0x80131509;
    THROW_FAIL_IF_FALSE(hr == COR_E_INVALIDOPERATION);
}
