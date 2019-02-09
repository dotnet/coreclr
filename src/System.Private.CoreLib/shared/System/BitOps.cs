﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using Internal.Runtime.CompilerServices;

// Some routines inspired by the Stanford Bit Twiddling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html

namespace System
{
    internal static class BitOps
    {
        // C# no-alloc optimization that directly wraps the data section of the dll (similar to string constants)
        // https://github.com/dotnet/roslyn/pull/24621

        private static ReadOnlySpan<byte> s_TrailingZeroCountDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> s_Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        #region TrailingZeroCount

        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(int value)
            => TrailingZeroCount((uint)value);

        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(uint value)
        {
            if (Bmi1.IsSupported)
            {
                // Note that TZCNT contract specifies 0->32
                return (int)Bmi1.TrailingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->32
            if (value == 0u)
                return 32;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(s_TrailingZeroCountDeBruijn),
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                ((uint)((value & -value) * 0x077CB531u)) >> 27);
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(long value)
            => TrailingZeroCount((ulong)value);

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(ulong value)
        {
            if (Bmi1.X64.IsSupported)
            {
                // Note that TZCNT contract specifies 0->64
                return (int)Bmi1.X64.TrailingZeroCount(value);
            }

            uint lo = (uint)value;

            if (lo == 0)
                return 32 + TrailingZeroCount((uint)(value >> 32));

            return TrailingZeroCount(lo);
        }

        #endregion

        #region LeadingZeroCount

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(uint value)
        {
            if (Lzcnt.IsSupported)
            {
                // Note that LZCNT contract specifies 0->32
                return (int)Lzcnt.LeadingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->32
            if (value == 0u)
                return 32;

            return 31 - Log2(value);
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(ulong value)
        {
            if (Lzcnt.X64.IsSupported)
            {
                // Note that LZCNT contract specifies 0->64
                return (int)Lzcnt.X64.LeadingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->64
            if (value == 0u)
                return 64;

            return 63 - Log2(value);
        }

        #endregion

        #region Log2

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// Does not incur branching.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
            value = FoldTrailingOnes(value);

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(s_Log2DeBruijn),
                (IntPtr)((value * 0x07C4ACDDu) >> 27));
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(ulong value)
        {
            uint hi = (uint)(value >> 32);

            if (hi != 0)
                return 32 + Log2(hi);

            return Log2((uint)value);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Fills the trailing zeros in a mask with ones.
        /// For example, 00010010 becomes 00011111.
        /// Does not incur branching.
        /// </summary>
        /// <param name="value">The value to mutate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FoldTrailingOnes(uint value)
        {
            // byte#                         4          3   2  1
            //                       1000 0000  0000 0000  00 00
            value |= value >> 01; // 1100 0000  0000 0000  00 00
            value |= value >> 02; // 1111 0000  0000 0000  00 00
            value |= value >> 04; // 1111 1111  0000 0000  00 00
            value |= value >> 08; // 1111 1111  1111 1111  00 00
            value |= value >> 16; // 1111 1111  1111 1111  FF FF

            return value;
        }

        #endregion
    }
}
