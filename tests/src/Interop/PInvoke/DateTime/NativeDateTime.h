// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>

#define NATIVEDATETIME_API DLL_EXPORT

// This class is exported from the NativeDateTime.dll
class NATIVEDATETIME_API CNativeDateTime {
public:
	CNativeDateTime(void);
};

extern NATIVEDATETIME_API int nNativeDateTime;

NATIVEDATETIME_API int fnNativeDateTime(void);
