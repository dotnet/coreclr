// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Win32
{
    // This class now only exists to service ilmarshalers.h, which has methods that emit
    // calls to it (once those are cleaned up, this class can be deleted).  The DEFINE_CLASS
    // mechanism they rely on doesn't currently support Interop.Kernel32 instead of
    // Microsoft.Win32.Win32Native, because Interop is a class rather than a namespace.
    internal static class Win32Native
    {
        internal static IntPtr CoTaskMemAlloc(UIntPtr cb) => Interop.Kernel32.CoTaskMemAlloc(cb);
        internal static void CoTaskMemFree(IntPtr ptr) => Interop.Kernel32.CoTaskMemFree(ptr);
    }
}
