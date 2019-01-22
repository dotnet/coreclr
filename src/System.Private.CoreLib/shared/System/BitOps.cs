// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace System
{
    internal static class BitOps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(int matches)
        {
            if (Bmi1.IsSupported)
            {
                return (int)Bmi1.TrailingZeroCount((uint)matches);
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    return TrailingZeroCountFallback(matches);
                }
                else
                {
                    // Not sure the above failback will work on BigEndian
                    Debug.Fail("TrailingZeroCount not implemented for BigEndian");
                    return 0;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int TrailingZeroCountFallback(int matches)
        {
            // https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            return TrailingCountMultiplyDeBruijn[(int)(((uint)((matches & -matches) * 0x077CB531U)) >> 27)];
        }

        private static ReadOnlySpan<byte> TrailingCountMultiplyDeBruijn => new byte[32]
        {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };
    }
}
