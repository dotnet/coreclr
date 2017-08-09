// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using Microsoft.Win32;

internal partial class Interop
{
	internal partial class Kernel32
	{
        internal const int LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern SafeLibraryHandle LoadLibraryEx(string libFilename, IntPtr reserved, int flags);
	}
}