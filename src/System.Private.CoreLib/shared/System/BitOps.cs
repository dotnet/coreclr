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
        private const uint DeBruijn32 = 0x07C4_ACDDu;

        // Magic C# optimization that directly wraps the data section of the dll (a bit like string constants)
        // https://github.com/dotnet/coreclr/pull/22118#discussion_r249957516
        // https://github.com/dotnet/roslyn/pull/24621
        // https://github.com/benaadams/coreclr/blob/9ba65b563918c778c256f18e234be69174173f12/src/System.Private.CoreLib/shared/System/BitOps.cs

        private static ReadOnlySpan<byte> TrailingCountMultiplyDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03, 30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07, 26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> LeadingZeroCountDeBruijn32 => new byte[32]
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
        /// <param name="value">The mask.</param>
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

            uint val = value;

            val -= (val >> 1) & c0;
            val = (val & c1) + ((val >> 2) & c1);
            val = (val + (val >> 4)) & c2;
            val *= c3;
            val >>= 24;

            return val;
        }

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(int value)
            => PopCount(unchecked((uint)value));

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
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
                uint hv = (uint)(value >> 32); // High-32
                uint bv = (uint)value; // Low-32

                uint h = Popcnt.PopCount(hv);
                uint b = Popcnt.PopCount(bv);

                return h + b;
            }

            const ulong c0 = 0x_5555_5555_5555_5555;
            const ulong c1 = 0x_3333_3333_3333_3333;
            const ulong c2 = 0x_0F0F_0F0F_0F0F_0F0F;
            const ulong c3 = 0x_0101_0101_0101_0101;

            ulong val = value;

            val -= (value >> 1) & c0;
            val = (val & c1) + ((val >> 2) & c1);
            val = (val + (val >> 4)) & c2;
            val *= c3;
            val >>= 56;

            return (uint)val;
        }

        /* Legacy implementations
        https://raw.githubusercontent.com/dotnet/corefx/master/src/System.Reflection.Metadata/src/System/Reflection/Internal/Utilities/BitArithmetic.cs
        https://raw.githubusercontent.com/dotnet/corert/cf5dc501e870b1efe8cba3d6990752538d174773/src/System.Private.CoreLib/shared/System/Buffers/Text/FormattingHelpers.CountDigits.cs
        https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        https://raw.githubusercontent.com/dotnet/coreclr/030a3ea9b8dbeae89c90d34441d4d9a1cf4a7de6/tests/src/JIT/Performance/CodeQuality/V8/Crypto/Crypto.cs
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
        {
            uint val = (value >> offset) | (value << (32 - offset));
            return val;
        }

        /* Legacy implementations
        https://github.com/dotnet/corert/blob/87e58839d6629b5f90777f886a2f52d7a99c076f/src/System.Private.CoreLib/src/System/Marvin.cs#L120-L124
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
        {
            uint val = (value << offset) | (value >> (32 - offset));
            return val;
        }

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
        https://github.com/dotnet/corert/blob/1bb85b989d9b01563df9eb83dce1c6a3415ce182/src/ILCompiler.ReadyToRun/src/Compiler/ReadyToRunHashCode.cs#L214
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/NativeFormat/TypeHashingAlgorithms.cs#L20
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/Text/Utf8String.cs#L47
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/Internal/Runtime/CompilerServices/OpenMethodResolver.cs#L178
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/System/RuntimeMethodHandle.cs#L67
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/ILCompiler.Compiler/src/Compiler/DependencyAnalysis/NodeFactory.NativeLayout.cs#L345
        https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.TypeLoader/src/Internal/Runtime/TypeLoader/TypeLoaderEnvironment.NamedTypeLookup.cs#L121
        */

        #endregion

        #region LeadingZeroCount

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(uint value)
        {
            if (Lzcnt.IsSupported)
            {
                return Lzcnt.LeadingZeroCount(value);
            }

            uint val = value;
            FoldTrailing(ref val);

            uint ix = (val * DeBruijn32) >> 27;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            ref byte lz = ref MemoryMarshal.GetReference(LeadingZeroCountDeBruijn32);
            int zeros = 31 - Unsafe.AddByteOffset(ref lz, (IntPtr)ix);

            // Log(0) is undefined: Return 32.
            zeros += IsZero(value);

            return (uint)zeros;
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(ulong value)
        {
            if (Lzcnt.X64.IsSupported)
            {
                return (uint)Lzcnt.X64.LeadingZeroCount(value);
            }

            // Instead of writing a 64-bit function,
            // we use the 32-bit function twice.

            uint h, b;
            if (Lzcnt.IsSupported)
            {
                // TODO: Check the math of this path
                h = Lzcnt.LeadingZeroCount((uint)(value >> 32)); // High-32
                b = Lzcnt.LeadingZeroCount((uint)value); // Low-32
            }
            else
            {
                ulong val = value;
                FoldTrailing(ref val);

                uint hv = (uint)(val >> 32); // High-32
                uint bv = (uint)val; // Low-32

                uint hi = (hv * DeBruijn32) >> 27;
                uint bi = (bv * DeBruijn32) >> 27;

                // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
                ref byte lz = ref MemoryMarshal.GetReference(LeadingZeroCountDeBruijn32);
                h = (uint)(31 - Unsafe.AddByteOffset(ref lz, (IntPtr)(int)hi));
                b = (uint)(31 - Unsafe.AddByteOffset(ref lz, (IntPtr)(int)bi)); // Use warm cache
            }

            // Log(0) is undefined: Return 32 + 32.
            h += IsZero((uint)(value >> 32)); // value == 0 ? 1 : 0
            b += IsZero((uint)value);

            // Keep b iff h==32
            uint mask = h & ~32u; // Zero 5th bit (32)
            mask = IsZero(mask);  // mask == 0 ? 1 : 0
            b = mask * b;

            return h + b;
        }

        /* Legacy implementations
        https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs
        https://raw.githubusercontent.com/dotnet/corefx/151a6065fa8184feb1ac4a55c89752342ab7c3bb/src/Common/src/CoreLib/System/Decimal.DecCalc.cs
        */

        #endregion

        #region TrailingZeroCount

        // PR already merged: https://github.com/dotnet/coreclr/pull/22118

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
            if (Bmi1.IsSupported)
            {
                return Bmi1.TrailingZeroCount(value);
            }

            // Software fallback
            // https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            ref byte tz = ref MemoryMarshal.GetReference(TrailingCountMultiplyDeBruijn);
            long val = (value & -value) * 0x077CB531U;
            uint offset = ((uint)val) >> 27;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(ref tz, offset);
        }

        #endregion

        #region LeadingOneCount

        /// <summary>
        /// Count the number of leading one bits in a mask.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingOneCount(uint value)
            => LeadingZeroCount(~value);

        /// <summary>
        /// Count the number of leading one bits in a mask.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingOneCount(int value)
            => LeadingOneCount(unchecked((uint)value));

        /// <summary>
        /// Count the number of leading one bits in a mask.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingOneCount(ulong value)
            => LeadingZeroCount(~value);

        /* Legacy impementations
        https://raw.githubusercontent.com/dotnet/coreclr/030a3ea9b8dbeae89c90d34441d4d9a1cf4a7de6/tests/src/JIT/Performance/CodeQuality/V8/Crypto/Crypto.cs
        https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs
        */
        
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(int value, int bitOffset)
            => ExtractBit(unchecked((uint)value), bitOffset);

        /* Legacy implementations
        https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        */

        #endregion

        #region ClearBit

        /// <summary>
        /// Clears the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTR.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClearBit(ref byte value, int bitOffset)
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int value, int bitOffset)
            => unchecked((int)ClearBit((uint)value, bitOffset));

        /* Legacy implementations
        https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        */

        #endregion

        #region InsertBit

        /// <summary>
        /// Sets the specified bit in a mask and returns whether it was originally set.
        /// Similar in behavior to the x86 instruction BTS.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InsertBit(ref byte value, int bitOffset)
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
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
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int InsertBit(int value, int bitOffset)
            => unchecked((int)InsertBit((uint)value, bitOffset));

        /* Legacy implementations
        https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        */

        #endregion

        #region Helpers

        // Some of these helpers may be unnecessary depending on how JIT optimizes certain bool operations.

        // Normalize bool's underlying value to 0|1
        // https://github.com/dotnet/roslyn/issues/24652

        // byte b;                 // Non-negative
        // int val = b;            // Widen byte to int so that negation is reliable
        // val = -val;             // Negation will set sign-bit iff non-zero
        // val = (uint)val >> 31;  // Send sign-bit to lsb (all other bits will be thus zero'd)

        // Would be great to use intrinsics here instead:
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

        // XOR is theoretically slightly cheaper than subtraction,
        // due to no carry logic. But both 1 clock cycle regardless.

        /// <summary>
        /// Returns 1 if <paramref name="value"/> is zero, else returns 0.
        /// Does not incur branching.
        /// Similar in behavior to the x86 instruction CMOVZ.
        /// </summary>
        /// <param name="value">The value to inspect.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte IsZero(uint value)
            => (byte)(1u ^ NonZero(value));

        /// <summary>
        /// Fills the trailing zeros in a mask with ones.
        /// </summary>
        /// <param name="value">The value to mutate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FoldTrailing(ref uint value)
        {
            // byte#                         4          3   2  1
            //                       1000 0000  0000 0000  00 00
            value |= value >> 01; // 1100 0000  0000 0000  00 00
            value |= value >> 02; // 1111 0000  0000 0000  00 00
            value |= value >> 04; // 1111 1111  0000 0000  00 00
            value |= value >> 08; // 1111 1111  1111 1111  00 00
            value |= value >> 16; // 1111 1111  1111 1111  FF FF
        }

        /// <summary>
        /// Fills the trailing zeros in a mask with ones.
        /// </summary>
        /// <param name="value">The value to mutate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FoldTrailing(ref ulong value)
        {
            // byte#                         8          7   6  5   4  3   2  1
            //                       1000 0000  0000 0000  00 00  00 00  00 00
            value |= value >> 01; // 1100 0000  0000 0000  00 00  00 00  00 00
            value |= value >> 02; // 1111 0000  0000 0000  00 00  00 00  00 00
            value |= value >> 04; // 1111 1111  0000 0000  00 00  00 00  00 00
            value |= value >> 08; // 1111 1111  1111 1111  00 00  00 00  00 00
            value |= value >> 16; // 1111 1111  1111 1111  FF FF  00 00  00 00

            value |= value >> 32; // 1111 1111  1111 1111  FF FF  FF FF  FF FF
        }

        #endregion
    }
}
