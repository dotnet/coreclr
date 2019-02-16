// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

namespace System.Buffers.Text
{
    internal static partial class FormattingHelpers
    {
        private static ReadOnlySpan<byte> UInt64MaxLog10GivenLzCount => new byte[]
        {
            20, 19, 19, 19, 19, 18, 18, 18, 17, 17, 17, 16, 16, 16, 16, 15, 15, 15, 14, 14, 14, 13, 13, 13, 13, 12, 12,
            12, 11, 11, 11, 10, 10, 10, 10, 9, 9, 9, 8, 8, 8, 7, 7, 7, 7, 6, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 2, 2, 
            2, 1, 1, 1, 2
        };

        private static readonly ulong[] s_uInt64PowersOf10 =
        {
            0, 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000, 10000000000, 100000000000, 
            1000000000000, 10000000000000, 100000000000000, 1000000000000000, 10000000000000000, 100000000000000000, 
            1000000000000000000, 10000000000000000000
        };

        private static ReadOnlySpan<byte> UInt32MaxLog10GivenLzCount => new byte[]
        {
            11, 10, 10, 9, 9, 9, 8, 8, 8, 7, 7, 7, 7, 6, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1, 2
        };

        private static readonly uint[] s_uInt32PowersOf10 =
        {
            0, 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000, 0
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(ulong value)
        {
            int y = Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(UInt64MaxLog10GivenLzCount),
                (IntPtr)BitOps.LeadingZeroCount(value));
            int d = unchecked((int)((value - s_uInt64PowersOf10[y]) >> 63));
            return y - d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(uint value)
        {
            int y = Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(UInt32MaxLog10GivenLzCount),
                (IntPtr)BitOps.LeadingZeroCount(value));
            int d = unchecked((int)((value - s_uInt32PowersOf10[y]) >> 31));
            return y - d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountHexDigits(ulong value)
        {
            return (64 - BitOps.LeadingZeroCount(value | 1) + 3) >> 2;
        }

        // Counts the number of trailing '0' digits in a decimal number.
        // e.g., value =      0 => retVal = 0, valueWithoutTrailingZeros = 0
        //       value =   1234 => retVal = 0, valueWithoutTrailingZeros = 1234
        //       value = 320900 => retVal = 2, valueWithoutTrailingZeros = 3209
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDecimalTrailingZeros(uint value, out uint valueWithoutTrailingZeros)
        {
            int zeroCount = 0;

            if (value != 0)
            {
                while (true)
                {
                    uint temp = value / 10;
                    if (value != (temp * 10))
                    {
                        break;
                    }

                    value = temp;
                    zeroCount++;
                }
            }

            valueWithoutTrailingZeros = value;
            return zeroCount;
        }
    }
}
