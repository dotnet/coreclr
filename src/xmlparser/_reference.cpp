// ==++==
// 
//   
//    Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//   
//    The use and distribution terms for this software are contained in the file
//    named license.txt, which can be found in the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by the
//    terms of this license.
//   
//    You must not remove this notice, or any other, from this software.
//   
// 
// ==--==
/*
 *                                             
 * 
 */

#include "core.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

void _assign(IUnknown ** ppref, IUnknown * pref)
{
    IUnknown *punkRef = *ppref;

    if (pref) pref->AddRef();
    (*ppref) = pref; 


    if (punkRef) punkRef->Release();
}    

void _release(IUnknown ** ppref)
{
    if (*ppref) 
    {
        (*ppref)->Release();
        *ppref = NULL;
    }
}
