// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue(VARIANT value)
{
    IUnknown* obj = *value.ppdispVal;

    if (obj == NULL)
    {
        printf("Marshal_ByValue (Native side) recieved an invalid IUnknown pointer\n");
        return FALSE;
    }

    obj->AddRef();

    obj->Release();

    return TRUE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Null(VARIANT value)
{
    return value.ppunkVal == NULL ? TRUE : FALSE;
}
