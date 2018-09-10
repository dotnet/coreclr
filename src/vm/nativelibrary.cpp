// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "nativelibrary.hpp"
#include "dllimport.h"

#include <stdlib.h>
#include "dllimport.h"
#include "field.h"
#include "assemblyname.hpp"
#include "eeconfig.h"
#include "strongname.h"
#include "interoputil.h"
#include "frames.h"
#include "typeparse.h"
#include "stackprobe.h"


//// static
INT_PTR QCALLTYPE NativeLibrary::LoadLibrary(QCall::AssemblyHandle callingAssembly, LPCWSTR libraryName, DWORD dllImportSearchPathFlag)
{
    QCALL_CONTRACT;

    HMODULE moduleHandle = nullptr;

    BEGIN_QCALL;

    Assembly* pAssembly = nullptr;
    if (callingAssembly != nullptr)
    {
        pAssembly = callingAssembly->GetAssembly();
    }

    BOOL searchAssemblyDirectory = TRUE;

    if (pAssembly->GetManifestModule()->HasDefaultDllImportSearchPathsAttribute())
    {
        searchAssemblyDirectory = pAssembly->GetManifestModule()->DllImportSearchAssemblyDirectory();
    }

    moduleHandle = NDirect::LoadLibraryModuleHierarchy(pAssembly, libraryName, searchAssemblyDirectory, dllImportSearchPathFlag, true);

    END_QCALL;

    return reinterpret_cast<INT_PTR>(moduleHandle);
}
