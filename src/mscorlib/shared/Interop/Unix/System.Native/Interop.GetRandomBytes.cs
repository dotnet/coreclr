// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal unsafe partial class Sys
    {
        [DllImport(Interop.Libraries.SystemNative, EntryPoint = "SystemNative_GetNonCryptographicallySecureRandomBytes")]
        internal static extern void GetNonCryptographicallySecureRandomBytes(ref byte buffer, int length);
    }

    internal static void GetRandomBytes(Span<byte> buffer)
    {
        Sys.GetNonCryptographicallySecureRandomBytes(ref MemoryMarshal.GetReference(buffer), buffer.Length);
    }
}
