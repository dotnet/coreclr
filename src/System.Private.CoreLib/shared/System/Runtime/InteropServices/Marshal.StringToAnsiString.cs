// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Runtime.InteropServices
{
    public static partial class Marshal
    {
        internal static unsafe int StringToAnsiString(string s, byte* buffer, int wideCharLen, bool bestFit = false, bool throwOnUnmappableChar = false)
        {
            Debug.Assert(wideCharLen >= (s.Length + 1) * SystemMaxDBCSCharSize, "Insufficient buffer length passed to StringToAnsiString");

            int nb;

            uint flags = bestFit ? 0 : Interop.Kernel32.WC_NO_BEST_FIT_CHARS;
            uint defaultCharUsed = 0;

            fixed (char* pwzChar = s)
            {
                nb = Interop.Kernel32.WideCharToMultiByte(
                    Interop.Kernel32.CP_ACP,
                    flags,
                    pwzChar,
                    s.Length,
                    buffer,
                    wideCharLen,
                    IntPtr.Zero,
                    throwOnUnmappableChar ? new IntPtr(&defaultCharUsed) : IntPtr.Zero);
            }

            if (defaultCharUsed != 0)
            {
                throw new ArgumentException(SR.Interop_Marshal_Unmappable_Char);
            }

            buffer[nb] = 0;
            return nb;
        }
    }
}