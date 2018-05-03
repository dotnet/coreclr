// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: StringNative.h
//

//
// Purpose: Contains types and method signatures for the Utf8String class
//

//

#include "fcall.h"
#include "qcall.h"
#include "excep.h"

#ifndef _UTF8STRINGNATIVE_H_
#define _UTF8STRINGNATIVE_H_

FCDECL1(Utf8StringObject *, Utf8StringFastAllocate, DWORD length);

#endif // _UTF8STRINGNATIVE_H_

