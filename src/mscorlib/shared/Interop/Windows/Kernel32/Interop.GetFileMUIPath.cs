// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Text;

internal partial class Interop
{
    internal partial class Kernel32
    {
		// 
		// BOOL GetFileMUIPath(
		//   DWORD  dwFlags,
		//   PCWSTR  pcwszFilePath,
		//   PWSTR  pwszLanguage,
		//   PULONG  pcchLanguage,
		//   PWSTR  pwszFileMUIPath,
		//   PULONG  pcchFileMUIPath,
		//   PULONGLONG  pululEnumerator
		// );
		// 
        [DllImport(Libraries.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetFileMUIPath(
									 int flags,
									 [MarshalAs(UnmanagedType.LPWStr)]
									 string filePath,
									 [MarshalAs(UnmanagedType.LPWStr)]
									 StringBuilder language,
									 ref int languageLength,
									 [Out, MarshalAs(UnmanagedType.LPWStr)]
									 StringBuilder fileMuiPath,
									 ref int fileMuiPathLength,
									 ref long enumerator);

	}
}
