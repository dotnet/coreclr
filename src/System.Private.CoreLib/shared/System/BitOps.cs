// Licensed to the .NET Foundation under one or more agreements.
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
        // Magic C# optimization that directly wraps the data section of the dll (a bit like string constants)
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

        #region PopCount

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(uint value)
        {
            if (Popcnt.IsSupported)
            {
                return Popcnt.PopCount(value);
            }

            const uint c0 = 0x_5555_5555;
            const uint c1 = 0x_3333_3333;
            const uint c2 = 0x_0F0F_0F0F;
            const uint c3 = 0x_0101_0101;

            uint count = value;

            count -= (count >> 1) & c0;
            count = (count & c1) + ((count >> 2) & c1);
            count = (count + (count >> 4)) & c2;
            count *= c3;
            count >>= 24;

            return count;
        }

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(int value)
            => PopCount((uint)value);

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(ulong value)
        {
            if (Popcnt.IsSupported)
            {
                if (Popcnt.X64.IsSupported)
                {
                    return (uint)Popcnt.X64.PopCount(value);
                }

                // Use the 32-bit function twice
                uint hi = Popcnt.PopCount((uint)(value >> 32));
                uint lo = Popcnt.PopCount((uint)value);

                return hi + lo;
            }

            const ulong c0 = 0x_5555_5555_5555_5555;
            const ulong c1 = 0x_3333_3333_3333_3333;
            const ulong c2 = 0x_0F0F_0F0F_0F0F_0F0F;
            const ulong c3 = 0x_0101_0101_0101_0101;

            ulong count = value;

            count -= (value >> 1) & c0;
            count = (count & c1) + ((count >> 2) & c1);
            count = (count + (count >> 4)) & c2;
            count *= c3;
            count >>= 56;

            return (uint)count;
        }

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(long value)
            => PopCount((ulong)value);

        /* Legacy implementations
        DONE https://raw.githubusercontent.com/dotnet/corefx/master/src/System.Reflection.Metadata/src/System/Reflection/Internal/Utilities/BitArithmetic.cs
        DONE https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        TBD https://raw.githubusercontent.com/dotnet/coreclr/030a3ea9b8dbeae89c90d34441d4d9a1cf4a7de6/tests/src/JIT/Performance/CodeQuality/V8/Crypto/Crypto.cs
        */

        #endregion

        #region RotateRight

        // Will compile to instrinsics if pattern complies (uint/ulong):
        // https://github.com/dotnet/coreclr/pull/1830

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateRight(uint value, int offset)
            => (value >> offset) | (value << (32 - offset));

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateRight(int value, int offset)
            => unchecked((int)RotateRight((uint)value, offset));

        /* Legacy implementations
        DONE https://github.com/dotnet/corert/blob/87e58839d6629b5f90777f886a2f52d7a99c076f/src/System.Private.CoreLib/src/System/Marvin.cs#L120-L124
        */

        #endregion

        #region RotateLeft

        // Will compile to instrinsics if pattern complies (uint/ulong):
        // https://github.com/dotnet/coreclr/pull/1830

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset)
            => (value << offset) | (value >> (32 - offset));

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateLeft(int value, int offset)
            => unchecked((int)RotateLeft((uint)value, offset));

        /* Legacy implementations
        DONE https://github.com/dotnet/corert/blob/1bb85b989d9b01563df9eb83dce1c6a3415ce182/src/ILCompiler.ReadyToRun/src/Compiler/ReadyToRunHashCode.cs#L214
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/NativeFormat/TypeHashingAlgorithms.cs#L20
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/Text/Utf8String.cs#L47
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/Internal/Runtime/CompilerServices/OpenMethodResolver.cs#L178
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/System/RuntimeMethodHandle.cs#L67
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/ILCompiler.Compiler/src/Compiler/DependencyAnalysis/NodeFactory.NativeLayout.cs#L345
        DONE https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.TypeLoader/src/Internal/Runtime/TypeLoader/TypeLoaderEnvironment.NamedTypeLookup.cs#L121
        */

        #endregion

        #region Log2

        // TODO: May belong in System.Math, in which case may need to name it Log2Int or Log2Floor
        // to distinguish it from overloads accepting float/double

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2, without branching.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Log2(uint value)
        {
            FoldTrailingOnes(ref value);

            // Using deBruijn sequence, k=2, n=5 (2^5=32)
            const uint deBruijn = 0b_0000_0111_1100_0100_1010_1100_1101_1101;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(s_Log2DeBruijn), 
                (IntPtr)((value * deBruijn) >> 27));
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2, without branching.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Log2(ulong value)
        {
            // We only have to count the low-32 or the high-32, depending on limits

            // Assume we need only examine low-32
            var val = (uint)value;
            byte inc = 0;

            // If high-32 is non-zero
            if (value > uint.MaxValue)
            {
                // Then we need only examine high-32 (and add 32 to the result)
                val = (uint)(value >> 32); // Use high-32 instead
                inc = 32;
            }

            // Use low-32
            return inc + Log2(val);
        }

        /* Legacy implementations
        DONE https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs
        DONE https://github.com/dotnet/roslyn/blob/33a3a61d36ec3657dc4af5e630ca6593397e6bbf/src/Workspaces/Core/Portable/Shared/Utilities/IntegerUtilities.cs#L45
        */

        #endregion

        #region LeadingZeroCount

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(uint value)
        {
            if (Lzcnt.IsSupported)
            {
                // Note that LZCNT contract specifies 0->32
                return Lzcnt.LeadingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->32
            if (value == 0u)
                return 32u;

            return 31u - Log2(value);
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(ulong value)
        {
            if (Lzcnt.X64.IsSupported)
            {
                // Note that LZCNT contract specifies 0->64
                return (uint)Lzcnt.X64.LeadingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->64
            if (value == 0u)
                return 64u;

            // Use the 32-bit function twice.
            uint lz = (uint)(value >> 32); // hi
            if (Lzcnt.IsSupported)
            {
                lz = Lzcnt.LeadingZeroCount(lz); // hi

                // Use lo iff hi is 32 zeros
                if (lz == 32u)
                    lz += Lzcnt.LeadingZeroCount((uint)value); // lo
            }
            else
            {
                lz = LeadingZeroCount(lz); // hi

                // Use lo iff hi is 32 zeros
                if (lz == 32u)
                    lz += LeadingZeroCount((uint)value); // lo
            }

            return lz;
        }

        /* Legacy implementations
        DONE https://raw.githubusercontent.com/dotnet/corefx/151a6065fa8184feb1ac4a55c89752342ab7c3bb/src/Common/src/CoreLib/System/Decimal.DecCalc.cs
        DONE https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs
        */

        #endregion

        #region TrailingZeroCount

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(int value)
            => TrailingZeroCount((uint)value);

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(uint value)
        {
            if (Bmi1.IsSupported)
            {
                // Note that TZCNT contract specifies 0->32
                return Bmi1.TrailingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->32
            if (value == 0u)
                return 32u;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(s_TrailingZeroCountDeBruijn),
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                (IntPtr)(((uint)((value & -value) * 0x077CB531u)) >> 27));
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(long value)
            => TrailingZeroCount((ulong)value);

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(ulong value)
        {
            if (Bmi1.X64.IsSupported)
            {
                // Note that TZCNT contract specifies 0->64
                return (uint)Bmi1.X64.TrailingZeroCount(value);
            }

            // Main code has behavior 0->0, so special-case to match intrinsic path 0->64
            if (value == 0u)
                return 64u;

            // Use the 32-bit function twice.
            uint tz = (uint)value; // lo
            if (Bmi1.IsSupported)
            {
                tz = Bmi1.TrailingZeroCount(tz); // lo

                // Use hi iff lo is 32 zeros
                if (tz == 32u)
                    tz += Bmi1.TrailingZeroCount((uint)(value >> 32)); // hi
            }
            else
            {
                tz = TrailingZeroCount(tz); // lo

                // Use hi iff lo is 32 zeros
                if (tz == 32u)
                    tz += TrailingZeroCount((uint)(value >> 32)); // hi
            }

            return tz;
        }

        #endregion

        #region ExtractBit

        // For bitlength N, it is conventional to treat N as congruent modulo-N
        // under the shift operation.
        // So for uint, 1 << 33 == 1 << 1, and likewise 1 << -46 == 1 << +18.
        // Note -46 % 32 == -14. But -46 & 31 (0011_1111) == +18. So we use & not %.
        // Software & hardware intrinsics already do this for uint/ulong, but
        // we need to emulate for byte/ushort.

        /// <summary>
        /// Reads whether the specified bit in a mask is set.
        /// Similar in behavior to the x86 instruction BT.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(byte value, int bitOffset)
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            return (value & mask) != 0;
        }

        /// <summary>
        /// Reads whether the specified bit in a mask is set.
        /// Similar in behavior to the x86 instruction BT.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(uint value, int bitOffset)
        {
            // TODO: Not sure if there is a suitable (exposed) intrinsic for this
            uint mask = 1U << bitOffset;

            return (value & mask) != 0;
        }

        /// <summary>
        /// Reads whether the specified bit in a mask is set.
        /// Similar in behavior to the x86 instruction BT.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(int value, int bitOffset)
            => ExtractBit((uint)value, bitOffset);

        /* Legacy implementations
        TBD https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        TBD https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        */

        #endregion

        #region ClearBit

        /// <summary>
        /// Clears the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTR.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClearBit(ref byte value, int bitOffset) // TODO: offset should maybe be uint
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            uint btr = value & mask;
            value = (byte)(value & ~mask);

            return btr != 0;
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTR.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClearBit(ref int value, int bitOffset)
        {
            int mask = 1 << bitOffset;

            int btr = value & mask;
            value = value & ~mask;

            return btr != 0;
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ClearBit(uint value, int bitOffset)
        {
            // TODO: Not sure if there is a suitable (exposed) intrinsic for this
            uint mask = 1U << bitOffset;

            return value & ~mask;
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int value, int bitOffset)
            => unchecked((int)ClearBit((uint)value, bitOffset));

        /* Legacy implementations
        DONE https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        DONE https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        TBD https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        */

        #endregion

        #region InsertBit

        /// <summary>
        /// Sets the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTS.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InsertBit(ref byte value, int bitOffset) // TODO: offset should maybe be uint
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            uint bts = value & mask;
            value = (byte)(value | mask);

            return bts != 0;
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTS.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InsertBit(ref int value, int bitOffset)
        {
            int mask = 1 << bitOffset;

            int bts = value & mask;
            value = value | mask;

            return bts != 0;
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint InsertBit(uint value, int bitOffset)
        {
            // TODO: Not sure if there is a suitable (exposed) intrinsic for this
            uint mask = 1U << bitOffset;

            return value | mask;
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int InsertBit(int value, int bitOffset)
            => unchecked((int)InsertBit((uint)value, bitOffset));

        /* Legacy implementations
        DONE https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        DONE https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        TBD https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        TBD https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        */

        #endregion

        #region Helpers

        // Some of these helpers may be unnecessary depending on how JIT optimizes certain bool operations.

        // TODO: Consider exposing as public - this code is duplicated surprisingly often

        /// <summary>
        /// Fills the trailing zeros in a mask with ones.
        /// For example, 00010010 becomes 00011111.
        /// Does not incur branching.
        /// </summary>
        /// <param name="value">The value to mutate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FoldTrailingOnes(ref uint value)
        {
            // byte#                         4          3   2  1
            //                       1000 0000  0000 0000  00 00
            value |= value >> 01; // 1100 0000  0000 0000  00 00
            value |= value >> 02; // 1111 0000  0000 0000  00 00
            value |= value >> 04; // 1111 1111  0000 0000  00 00
            value |= value >> 08; // 1111 1111  1111 1111  00 00
            value |= value >> 16; // 1111 1111  1111 1111  FF FF
        }

        #endregion
    }
}
