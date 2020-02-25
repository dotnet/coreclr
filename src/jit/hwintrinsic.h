// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _HW_INTRINSIC_H_
#define _HW_INTRINSIC_H_

#if defined(TARGET_XARCH)
#include "hwintrinsicxarch.h"
#elif defined(TARGET_ARM64)
#include "hwintrinsicArm64.h"
#endif

#endif // _HW_INTRINSIC_H_
