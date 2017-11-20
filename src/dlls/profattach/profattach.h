// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// ProfAttach.h
// 

#include <windows.h>

#ifdef FEATURE_PROFAPI_ATTACH_DETACH
#include "../../vm/profattach.h"
#endif // FEATURE_PROFAPI_ATTACH_DETACH

/* Create a ICLRProfiling Instance that can be used to send attach requests */
EXTERN_C HRESULT
CreateCLRProfiling(
    __in LPCWSTR CLRFullPath,
    __out LPVOID pCLRProfilingInstance
);

