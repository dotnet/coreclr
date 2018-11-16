// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#define RETURN_FALSE_IF_FAILED(x) do { if(FAILED(x)) { return FALSE; } } while(0)

#define RETURN_HR_IF_FAILED(x) do { HRESULT __CachedHR; if(FAILED(__CachedHR = (x))) { return __CachedHR; } } while(0)
