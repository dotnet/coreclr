// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _NATIVELIBRARY_H
#define _NATIVELIBRARY_H

class NativeLibrary
{

public:
	static
		INT_PTR QCALLTYPE LoadLibrary(QCall::AssemblyHandle assembly, LPCWSTR libraryName, DWORD dllImportSearchPathFlag);
};

#endif
