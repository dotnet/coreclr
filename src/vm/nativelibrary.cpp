// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 
// File: NativeLibrary.cpp
//

//
// Unmanaged implementation of the NativeLibrary class
//


#include "common.h"
#include "nativelibrary.h"
#include "dllimport.h"

//
// GetProcAddress
// Returns the address of the specified symbol from the target module, or nullptr if not found
LPVOID QCALLTYPE NativeLibrary::GetProcAddress(HMODULE hModule, LPCSTR lpProcName)
{
    QCALL_CONTRACT;

    // Arguments are check on managed side
    PRECONDITION(hModule != nullptr);

    LPVOID procAddress = nullptr;

    BEGIN_QCALL;

    procAddress = ::GetProcAddress(hModule, lpProcName);

    END_QCALL;

    return procAddress;
}

//
// LoadLibrary
// Loads the specified library, returning its handles, or nullptr if library cannot be found
HINSTANCE QCALLTYPE NativeLibrary::LoadLibrary(LPCUTF8 moduleName, QCall::AssemblyHandle callingAssembly, BOOL searchAssemblyDirectory, DWORD searchPaths)
{
    // Quick sanity check to make sure we've got the sizes correct on the managed side.
    static_assert(sizeof(HINSTANCE) == sizeof(uintptr_t), "Unexpected size for HINSTANCE.");

    QCALL_CONTRACT;

    // Arguments are check on managed side
    PRECONDITION(moduleName != nullptr);

    HINSTANCE hmod = nullptr;

    BEGIN_QCALL;

    Assembly* pAssembly = nullptr;

    if (static_cast<DomainAssembly*>(callingAssembly) != nullptr)
    {
        pAssembly = callingAssembly->GetAssembly();
    }

    hmod = NDirect::LoadLibraryModuleForNativeLibrary(moduleName, pAssembly, searchAssemblyDirectory, searchPaths);

    END_QCALL;

    return hmod;
}
