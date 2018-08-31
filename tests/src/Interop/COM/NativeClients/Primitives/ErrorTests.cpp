// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "ClientTests.h"

namespace
{
    void VerifyExpectedException(_In_ IErrorMarshalTesting *et)
    {
        HRESULT hrs[] =
        {
            E_NOTIMPL,
            E_POINTER,
            E_ACCESSDENIED,
            E_OUTOFMEMORY,
            E_INVALIDARG,
            E_UNEXPECTED,
            HRESULT{-1}
        };

        for (int i = 0; i < ARRAYSIZE(hrs); ++i)
        {
            HRESULT hr = hrs[i];
            HRESULT hrMaybe = et->raw_Throw_HResult(hr);
            THROW_FAIL_IF_FALSE(hr == hrMaybe);
        }
    }

    void VerifyReturnHResult(_In_ IErrorMarshalTesting *et)
    {
        HRESULT hrs[] =
        {
            E_NOTIMPL,
            E_POINTER,
            E_ACCESSDENIED,
            E_INVALIDARG,
            E_UNEXPECTED,
            HRESULT{-1},
            S_FALSE,
            HRESULT{2}
        };

        for (int i = 0; i < ARRAYSIZE(hrs); ++i)
        {
            HRESULT hr = hrs[i];
            HRESULT hrMaybe = et->Return_As_HResult(hr);
            THROW_FAIL_IF_FALSE(hr == hrMaybe);
        }
    }
}

void Run_ErrorTests()
{
    HRESULT hr;

    ComSmartPtr<IErrorMarshalTesting> errorMarshal;
    THROW_IF_FAILED(::CoCreateInstance(CLSID_ErrorMarshalTesting, nullptr, CLSCTX_INPROC, IID_IErrorMarshalTesting, (void**)&errorMarshal));

    VerifyExpectedException(errorMarshal);
    VerifyReturnHResult(errorMarshal);
}
