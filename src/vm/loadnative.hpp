// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _LOADNATIVE_H
#define _LOADNATIVE_H

class LoadNative
{

public:
	static
		INT_PTR QCALLTYPE LoadLibrary(Assembly* pAssembly, LPCWSTR libraryName, BOOL searchAssemblyDirectory, DWORD dllImportSearchPathFlag);
};

#endif
