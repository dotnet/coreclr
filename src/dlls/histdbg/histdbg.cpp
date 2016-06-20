// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifdef FEATURE_PAL

#include "classfactory.h"
HINSTANCE g_hThisInst;  // This library.

const CLSID CLSID_HistDbgProfiler = {0xF96885DD,0x704C,0x0346,{0x70,0x25,0xEC,0x49,0x5E,0x9E,0x72,0xE0}};

extern "C" {
#ifdef __llvm__
__attribute__((used))
#endif // __llvm__
	HRESULT STDMETHODCALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void **ppv)
	{
		if (ppv == NULL || rclsid != CLSID_HistDbgProfiler)
			return E_FAIL;

		*ppv = new ClassFactory();

		return S_OK;
	}
}

#endif // FEATURE_PAL
