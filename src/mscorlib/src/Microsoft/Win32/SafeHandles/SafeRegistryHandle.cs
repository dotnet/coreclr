// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//
// Implements Microsoft.Win32.SafeHandles.SafeRegistryHandle
//
// ======================================================================================

using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;

namespace Microsoft.Win32.SafeHandles
{
    internal sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeRegistryHandle() : base(true) { }

        public SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        override protected bool ReleaseHandle()
        {
            return (RegCloseKey(handle) == Interop.Errors.ERROR_SUCCESS);
        }

        [DllImport(Win32Native.ADVAPI32)]
        internal static extern int RegCloseKey(IntPtr hKey);
    }
}

