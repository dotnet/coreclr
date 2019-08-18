// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.Win32
{
    internal static class UnsafeNativeMethods
    {
#if FEATURE_COMINTEROP
        [DllImport("api-ms-win-core-winrt-l1-1-0.dll", PreserveSig = true)]
        internal static extern int RoGetActivationFactory(
            [MarshalAs(UnmanagedType.HString)] string activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IInspectable)] out object factory);
#endif
    }
}
