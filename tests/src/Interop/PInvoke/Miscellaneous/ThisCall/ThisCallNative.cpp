// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdio.h>
#include <xplatform.h>
#include <platformdefines.h>

struct SizeF
{
    float width;
    float height;
};

class C
{
    int dummy = 0xcccccccc;
    float width;
    float height;

public:
    C(float width, float height)
        :width(width),
        height(height)
    {}

    SizeF GetSize()
    {
        return {width, height};
    }
};


extern "C" DLL_EXPORT C* STDMETHODCALLTYPE CreateInstanceOfC(float width, float height)
{
    return new C(width, height);
}

using GetSizeFn_t = SizeF(C::*)();

extern "C" DLL_EXPORT GetSizeFn_t STDMETHODCALLTYPE GetSizeMemberFunction()
{
    return &C::GetSize;
}

extern "C" DLL_EXPORT void STDMETHODCALLTYPE FreeInstanceOfC(C* instance)
{
    delete instance;
}
