// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: FloatSingle.cpp
//

#include <common.h>

#include "floatsingle.h"

#define IS_FLT_INFINITY(x)         (((*((INT32*)((void*)&x))) & 0x7FFFFFFF) == 0x7F800000)

// The default compilation mode is /fp:precise, which disables floating-point intrinsics. This
// default compilation mode has previously caused performance regressions in floating-point code.
// We enable /fp:fast semantics for the majority of the math functions, as it will speed up performance
// and is really unlikely to cause any other code regressions.

////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
///
///                         beginning of /fp:fast scope
///
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

#ifdef _MSC_VER
#pragma float_control(precise, off)
#endif

/*=====================================Abs=====================================
**
==============================================================================*/
FCIMPL1(float, COMSingle::Abs, float x)
    FCALL_CONTRACT;

    return (float)fabsf(x);
FCIMPLEND

#ifdef _MSC_VER
#pragma float_control(precise, on )
#endif

////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
///
///                         End of /fp:fast scope
///
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
