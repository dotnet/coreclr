// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class FileApiInterop
    {
#pragma warning disable BCL0015 // Not part of our whitelist
        [DllImport(Libraries.FileApiInterop, EntryPoint = "CreateFile2FromApp", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern unsafe SafeFileHandle CreateFile2FromAppPrivate(
            string lpFileName,
            int dwDesiredAccess,
            FileShare dwShareMode,
            FileMode dwCreationDisposition,
            Kernel32.CREATEFILE2_EXTENDED_PARAMETERS* pCreateExParams);
#pragma warning restore BCL0015

        internal static unsafe SafeFileHandle CreateFile2FromApp(
            string lpFileName,
            int dwDesiredAccess,
            FileShare dwShareMode,
            FileMode dwCreationDisposition,
            Kernel32.CREATEFILE2_EXTENDED_PARAMETERS* pCreateExParams)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixOverMaxPath(lpFileName);
            return CreateFile2FromAppPrivate(lpFileName, dwDesiredAccess, dwShareMode, dwCreationDisposition, pCreateExParams);
        }
    }
}