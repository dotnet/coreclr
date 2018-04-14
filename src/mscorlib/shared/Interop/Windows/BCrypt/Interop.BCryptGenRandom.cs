// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class BCrypt
    {
        internal static int BCryptGenRandom(ref byte pbBuffer, int count)
        {            
            Debug.Assert(count >= 0);

            return BCryptGenRandom(IntPtr.Zero,ref pbBuffer, count, BCRYPT_USE_SYSTEM_PREFERRED_RNG);
        }

        private const int BCRYPT_USE_SYSTEM_PREFERRED_RNG = 0x00000002;
        internal const int STATUS_SUCCESS = 0x0;
        internal const int STATUS_NO_MEMORY = unchecked((int)0xC0000017);

        [DllImport(Libraries.BCrypt, CharSet = CharSet.Unicode)]
        private static extern int BCryptGenRandom(IntPtr hAlgorithm, ref byte pbBuffer, int cbBuffer, int dwFlags);
    }

    internal static void GetRandomBytes(Span<byte> buffer)
    {
        int status = BCrypt.BCryptGenRandom(ref MemoryMarshal.GetReference(buffer), buffer.Length);
        if (status != BCrypt.STATUS_SUCCESS)
        {
            if (status == BCrypt.STATUS_NO_MEMORY)
            {
                throw new OutOfMemoryException();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
