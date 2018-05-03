// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
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

