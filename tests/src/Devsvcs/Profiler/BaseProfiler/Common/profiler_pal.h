// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma once

#ifndef WIN32
#include <cstdlib>
#include "pal_mstypes.h"
#include "pal.h"
#include "ntimage.h"
#include "corhdr.h"

#define CoTaskMemAlloc(cb) malloc(cb)
#define CoTaskMemFree(cb) free(cb)

#define UINT_PTR_FORMAT "lx"

#else
#define UINT_PTR_FORMAT "llx"
#endif