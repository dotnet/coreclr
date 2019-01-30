// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Internal.Runtime.CompilerServices;

// Some routines inspired by the Stanford Bit Widdling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html

namespace System
{
    internal static class BitOps
    {
        // Magic C# optimization that directly wraps the data section of the dll (a bit like string constants)
        // https://github.com/dotnet/coreclr/pull/22118#discussion_r249957516
        // https://github.com/dotnet/roslyn/pull/24621
        // https://github.com/benaadams/coreclr/blob/9ba65b563918c778c256f18e234be69174173f12/src/System.Private.CoreLib/shared/System/BitOps.cs

        private static ReadOnlySpan<byte> TrailingCountMultiplyDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> DeBruijnLog2 => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        private static ReadOnlySpan<byte> TrailingZeroCountUInt32 => new byte[37]
        {
            32, 00, 01, 26, 02, 23, 27, 32,
            03, 16, 24, 30, 28, 11, 33, 13,
            04, 07, 17, 35, 25, 22, 31, 15,
            29, 10, 12, 06, 34, 21, 14, 09,
            05, 20, 08, 19, 18
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
            => PopCount(unchecked((uint)value));

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
            => PopCount(unchecked((ulong)value));

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
        /// Note that by convention, input value 0 returns 32 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Log2(uint value)
        {
            // Log(0) is undefined. Return 32 for input 0, without branching:
            //                              0   1   2   n
            uint log = Log2Impl(value); //  0   0   1   log
            byte not0 = NonZero(value); //  0   1   1   1
            uint is0 = 1u ^ not0; //        1   0   0   0
            uint c32 = is0 * 32u; //        32  0   0   0
            log *= not0; //                 0   0   1   log

            return c32 + log; //            32  0   1   log
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2, without branching.
        /// Note that by convention, input value 0 returns 32 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(int value)
        {
            // TODO: Remove looping/branching
            int log = 0;
            while ((value >>= 1) != 0)
            {
                log++;
            }
            return log;
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2, without branching.
        /// Note that by convention, input value 0 returns 64 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Log2(ulong value)
        {
            // We only have to count the low-32 or the high-32, depending on limits

            // Assume we need only examine low-32
            var val = (uint)value;
            byte inc = 0;

            // TODO: Remove branching

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

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2, without branching.
        /// Note that by convention, input value 0 returns 64 since Log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(long value)
        {
            // TODO: Remove looping/branching
            int log = 0;
            while ((value >>= 1) != 0)
            {
                log++;
            }
            return log;
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
                return Lzcnt.LeadingZeroCount(value);
            }

            //                                  00  01  02  2B
            int count = (int)Log2Impl(value); //32  00  01  31
            count = 31 - count; //              -1  31  30  00
            byte not0 = NonZero(value); //      0   1   1   1
            int is0 = 1 ^ not0; //              1   0   0   0
            count += is0; //                    +1  +0  +0  +0

            return (uint)count; //              32  31  30  00
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
                return (uint)Lzcnt.X64.LeadingZeroCount(value);
            }

            // Use the 32-bit function twice.

            uint hi = (uint)(value >> 32);
            uint lo = (uint)value;

            if (Lzcnt.IsSupported)
            {
                hi = Lzcnt.LeadingZeroCount(hi);
                lo = Lzcnt.LeadingZeroCount(lo);
            }
            else
            {
                hi = LeadingZeroCount(hi);
                lo = LeadingZeroCount(lo);
            }

            // Keep lo iff hi==32
            uint m = hi & ~32u; // Zero 5th bit of hi
            byte not0 = NonZero(m); // not0 = m == 0 ? 0 : 1
            uint is0 = 1u ^ not0; // is0 = m == 0 ? 1 : 0
            lo *= is0; // lo *= (m == 0 ? 1 : 0)

            return hi + lo;
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
            => TrailingZeroCount(unchecked((uint)value));

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(uint value)
        {
            // PR already merged: https://github.com/dotnet/coreclr/pull/22118

            if (Bmi1.IsSupported)
            {
                return Bmi1.TrailingZeroCount(value);
            }

            // TODO: Remove branching

            if (value == 0)
                return 32;

            ref byte tz = ref MemoryMarshal.GetReference(TrailingCountMultiplyDeBruijn);
            long val = (value & -value) * 0x077C_B531u;
            uint offset = ((uint)val) >> 27;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            uint count = Unsafe.AddByteOffset(ref tz, (IntPtr)offset);
            return count;
        }

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
                return (uint)Bmi1.X64.TrailingZeroCount(value);
            }

            // Use the 32-bit function twice.

            uint hi = (uint)(value >> 32);
            uint lo = (uint)value;

            if (Bmi1.IsSupported)
            {
                hi = Bmi1.TrailingZeroCount(hi);
                lo = Bmi1.TrailingZeroCount(lo);
            }
            else
            {
                hi = TrailingZeroCount(hi);
                lo = TrailingZeroCount(lo);
            }

            // Keep hi iff lo==32
            uint m = lo & ~32u; // Zero 5th bit of lo
            byte not0 = NonZero(m); // not0 = m == 0 ? 0 : 1
            uint is0 = 1u ^ not0; // is0 = m == 0 ? 1 : 0
            hi *= is0; // hi *= (m == 0 ? 1 : 0)

            return lo + hi;
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(long value)
            => TrailingZeroCount(unchecked((ulong)value));

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
            => ExtractBit(unchecked((uint)value), bitOffset);

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

        // Would be great to use x86 intrinsics here instead:
        //     OR al, al
        //     CMOVNZ al, 1
        // CMOV isn't a branch and won't stall the pipeline.

        /// <summary>
        /// Returns 1 if <paramref name="value"/> is non-zero, else returns 0.
        /// Does not incur branching.
        /// Similar in behavior to the x86 instruction CMOVNZ.
        /// </summary>
        /// <param name="value">The value to inspect.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte NonZero(uint value)
            // Negation will set sign-bit iff non-zero
            => unchecked((byte)(((ulong)-value) >> 63));

        /// <summary>
        /// Returns the log of the specified value, base 2.
        /// Returns 0 for input value 0.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Log2Impl(uint value)
        {
            FoldTrailingOnes(ref value);
            uint ix = (value * 0x07C4_ACDDu) >> 27;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            ref byte lz = ref MemoryMarshal.GetReference(DeBruijnLog2);
            uint log = Unsafe.AddByteOffset(ref lz, (IntPtr)ix);

            return log;
        }

        // TODO: Consider exposing as public - this code is duplicated surprisingly often

        /// <summary>
        /// Fills the trailing zeros in a mask with ones.
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
