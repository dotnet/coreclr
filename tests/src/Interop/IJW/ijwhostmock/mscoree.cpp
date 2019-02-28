// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <windows.h>
#include <xplatform.h>
#include <set>

std::set<HINSTANCE> g_modulesQueried = {};

// Entry-point that coreclr looks for.
extern "C" INT32 STDMETHODCALLTYPE GetTokenForVTableEntry(HINSTANCE hInst, BYTE **ppVTEntry)
{
    g_modulesQueried.emplace(hInst);
    return (INT32)(UINT_PTR)*ppVTEntry;
}

extern "C" DLL_EXPORT BOOL __cdecl WasModuleVTableQueried(HINSTANCE hInst)
{
    return g_modulesQueried.find(hInst) != g_modulesQueried.end() ? TRUE : FALSE;
}

// Entrypoint jumped to by IJW dlls when their dllmain is called
// This can be combined into ijwhost.cpp when we get an updated MSVC compiler that can
// link the IJW entrypoints to ijwhost.dll instead of to mscoree.dll.
extern "C" BOOL WINAPI _CorDllMain(HINSTANCE hInst, DWORD dwReason, LPVOID lpReserved)
{
    return TRUE;
}
