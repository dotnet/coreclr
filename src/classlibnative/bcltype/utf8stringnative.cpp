// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: StringNative.cpp
//

//
// Purpose: The implementation of the Utf8String class.
//

//

#include "common.h"

#include "object.h"
#include "utilcode.h"
#include "excep.h"
#include "frames.h"
#include "field.h"
#include "vars.hpp"
#include "utf8stringnative.h"
#include "comutilnative.h"
#include "metasig.h"
#include "excep.h"

// Compile the string functionality with these pragma flags (equivalent of the command line /Ox flag)
// Compiling this functionality differently gives us significant throughout gain in some cases.
#if defined(_MSC_VER) && defined(_TARGET_X86_)
#pragma optimize("tgy", on)
#endif

FCIMPL1(Utf8StringObject *, Utf8StringFastAllocate, DWORD length)
{
    FCALL_CONTRACT;

    UTF8STRINGREF rv = NULL; // not protected

    HELPER_METHOD_FRAME_BEGIN_RET_1(rv);
    rv = SlowAllocateUtf8String(length);
    HELPER_METHOD_FRAME_END();

    return UTF8STRINGREFToObject(rv);
}
FCIMPLEND


// Revert to command line compilation flags
#if defined(_MSC_VER) && defined(_TARGET_X86_)
#pragma optimize ("", on)
#endif
