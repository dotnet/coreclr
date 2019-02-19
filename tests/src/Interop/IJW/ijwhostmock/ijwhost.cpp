// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <windows.h>
#include "xplatform.h"
#include <set>

std::set<HINSTANCE> g_modulesQueried = {};

// Entry-point that coreclr looks for.
extern "C" DLL_EXPORT INT32 STDMETHODCALLTYPE GetTokenForVTableEntry(HINSTANCE hInst, BYTE **ppVTEntry)
{
    g_modulesQueried.emplace(hInst);
    return (INT32)(UINT_PTR)*ppVTEntry;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE WasModuleVTableQueried(HINSTANCE hInst)
{
    return g_modulesQueried.find(hInst) != g_modulesQueried.end() ? TRUE : FALSE;
}
