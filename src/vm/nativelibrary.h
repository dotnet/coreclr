// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 
// File: NativeLibrary.h
//

//
// Unmanaged implementation of the NativeLibrary class
//


#ifndef __NATIVELIBRARY_H__
#define __NATIVELIBRARY_H__

#include "qcall.h"

class NativeLibrary
{
public:
    static LPVOID QCALLTYPE GetProcAddress(HMODULE hModule, LPCSTR lpProcName);
    static HINSTANCE QCALLTYPE LoadLibrary(LPCUTF8 moduleName, QCall::AssemblyHandle callingAssembly, BOOL searchAssemblyDirectory, DWORD searchPaths);
};

#endif
