// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#include "platformdefines.h"

#pragma managed
int ManagedCallee()
{
    return 100;
}

#pragma unmanaged
int NativeFunction()
{
    return ManagedCallee();
}

extern "C" DLL_EXPORT int STDMETHODCALLTYPE NativeEntryPoint()
{
    return NativeFunction();
}

#pragma managed
public ref class TestClass
{
public:
    int ManagedEntryPoint()
    {
        return NativeFunction();
    }
};
