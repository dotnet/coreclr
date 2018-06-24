// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================
**
** Source:  LoadLibraryA.cpp
**
** Purpose: Test loading of two libraries when one use extern
**          symbols without linking
**
**============================================================*/
#include <palsuite.h>

/* SHLEXT is defined only for Unix variants */

#if defined(SHLEXT)
#define ModuleName    "librotor_pal"SHLEXT
#else
#define ModuleName    "rotor_pal.dll"
#endif

typedef int (__cdecl *AddonFunction)();

int __cdecl main(int argc, char *argv[])
{
    int err;
    const char *libs[] = {
        "./libmainlibrary.so",
        "./libaddon.so"
    };
    int libsNr = sizeof(libs) / sizeof(libs[0]);
    HMODULE *ModuleHandles = new HMODULE[libsNr];

    /* Initialize the PAL environment */
    err = PAL_Initialize(argc, argv);
    if(0 != err)
    {
        ExitProcess(FAIL);
    }

    for (int i = 0; i < libsNr; i++)
    {
        /* load a module */
        ModuleHandles[i] = LoadLibrary(libs[i]);
        if(!ModuleHandles[i])
        {
            Fail("Failed to call LoadLibrary API on %s!\n", libs[i]);
        }
    }

    AddonFunction testFunction =
        (AddonFunction)GetProcAddress(ModuleHandles[1], "GetIntAddon");
    testFunction();

    for (int i = 0; i < libsNr; i++)
    {
        /* decrement the reference count of the loaded dll */
        err = FreeLibrary(ModuleHandles[i]);
        if(0 == err)
        {
            Fail("\nFailed to all FreeLibrary API!\n");
        }
    }

    PAL_Terminate();

    delete[] ModuleHandles;

    return PASS;
}
