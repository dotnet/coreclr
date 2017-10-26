// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

//
// ===========================================================================
// File: comem.cpp
// 
// ===========================================================================

#include "common.h" 

PUB STDAPI_(LPVOID) CoTaskMemAlloc(SIZE_T cb)
{
    return LocalAlloc(LMEM_FIXED, cb);
}

PUB STDAPI_(LPVOID) CoTaskMemRealloc(LPVOID pv, SIZE_T cb)
{
    return LocalReAlloc(pv, cb, LMEM_MOVEABLE);
}

PUB STDAPI_(void) CoTaskMemFree(LPVOID pv)
{
    LocalFree(pv);
}
