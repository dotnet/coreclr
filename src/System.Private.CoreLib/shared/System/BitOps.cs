// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using Internal.Runtime.CompilerServices;

// Some routines inspired by the Stanford Bit Widdling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html

namespace System
{
    // This class is not meant to be merged in its current form.
    // The duplicate methods are intentional - they show the pervasiveness of similar (known) code across the stack
    // If this PR progresses, the duplicates will be consolidated and optimized, and the call sites (corert, coreclr, corefx) updated.
    internal static class BitOps
    {
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
            // TODO: Benchmark & consolidate with similar methods proposed
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

        private static ReadOnlySpan<byte> TrailingCountMultiplyDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03, 30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07, 26, 12, 18, 06, 11, 05, 10, 09
        };

        // Legacy code copied from callsites.

        #region LZCNT (Legacy)

        // This method moved & renamed from the following location
        // https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount_Number(uint value)
        {
            return 32 - CountSignificantBits(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount_Number(ulong value)
        {
            return 64 - CountSignificantBits(value);
        }

        private static readonly uint[] s_MultiplyDeBruijnBitPosition = new uint[]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        // This is a similar method from a different location.
        // It is shown here in its entirety, to show pervasivness of such code.
        // It has been renamed so that the class compiles.
        // Duplicates will be consolidated and optimized later.
        // https://raw.githubusercontent.com/dotnet/corefx/151a6065fa8184feb1ac4a55c89752342ab7c3bb/src/Common/src/CoreLib/System/Decimal.DecCalc.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LeadingZeroCount_DecCalc(uint value)
        {
            Debug.Assert(value > 0);
            int c = 1;
            if ((value & 0xFFFF0000) == 0)
            {
                value <<= 16;
                c += 16;
            }
            if ((value & 0xFF000000) == 0)
            {
                value <<= 8;
                c += 8;
            }
            if ((value & 0xF0000000) == 0)
            {
                value <<= 4;
                c += 4;
            }
            if ((value & 0xC0000000) == 0)
            {
                value <<= 2;
                c += 2;
            }
            return c + ((int)value >> 31);
        }

        #endregion

        #region L1CNT (Legacy)

        // https://raw.githubusercontent.com/dotnet/coreclr/030a3ea9b8dbeae89c90d34441d4d9a1cf4a7de6/tests/src/JIT/Performance/CodeQuality/V8/Crypto/Crypto.cs

        // return index of lowest 1-bit in x, x < 2^31 (-1 for no set bits)
        public static int lbit(int x)
        {
            if (x == 0)
                return -1;
            int r = 0;
            if ((x & 0xffff) == 0)
            { x >>= 16; r += 16; }
            if ((x & 0xff) == 0)
            { x >>= 8; r += 8; }
            if ((x & 0xf) == 0)
            { x >>= 4; r += 4; }
            if ((x & 3) == 0)
            { x >>= 2; r += 2; }
            if ((x & 1) == 0)
                ++r;
            return r;
        }

        // This method moved & renamed from this location
        // https://raw.githubusercontent.com/dotnet/corefx/62c70143cfbb08bbf03b5b8aad60c2add84a0d9e/src/Common/src/CoreLib/System/Number.BigInteger.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountSignificantBits(uint value)
        {
            return (value != 0) ? (1 + LogBase2(value)) : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountSignificantBits(ulong value)
        {
            uint upper = (uint)(value >> 32);

            if (upper != 0)
            {
                return 32 + CountSignificantBits(upper);
            }

            return CountSignificantBits((uint)(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LogBase2(uint value)
        {
            Debug.Assert(value != 0);

            // This comes from the Stanford Bit Widdling Hacks by Sean Eron Anderson:
            // http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogDeBruijn

            value |= (value >> 1); // first round down to one less than a power of 2 
            value |= (value >> 2);
            value |= (value >> 4);
            value |= (value >> 8);
            value |= (value >> 16);

            uint index = (value * 0x07C4ACDD) >> 27;
            return s_MultiplyDeBruijnBitPosition[(int)(index)];
        }

        #endregion

        #region POPCNT (Legacy)

        // https://raw.githubusercontent.com/dotnet/corefx/master/src/System.Reflection.Metadata/src/System/Reflection/Internal/Utilities/BitArithmetic.cs

        internal static int CountBits_Reflection(int v)
        {
            return CountBits_Reflection(unchecked((uint)v));
        }

        internal static int CountBits_Reflection(uint v)
        {
            unchecked
            {
                v = v - ((v >> 1) & 0x55555555u);
                v = (v & 0x33333333u) + ((v >> 2) & 0x33333333u);
                return (int)((v + (v >> 4) & 0xF0F0F0Fu) * 0x1010101u) >> 24;
            }
        }

        internal static int CountBits_Reflection(ulong v)
        {
            const ulong Mask01010101 = 0x5555555555555555UL;
            const ulong Mask00110011 = 0x3333333333333333UL;
            const ulong Mask00001111 = 0x0F0F0F0F0F0F0F0FUL;
            const ulong Mask00000001 = 0x0101010101010101UL;
            v = v - ((v >> 1) & Mask01010101);
            v = (v & Mask00110011) + ((v >> 2) & Mask00110011);
            return (int)(unchecked(((v + (v >> 4)) & Mask00001111) * Mask00000001) >> 56);
        }

        // https://raw.githubusercontent.com/dotnet/corert/cf5dc501e870b1efe8cba3d6990752538d174773/src/System.Private.CoreLib/shared/System/Buffers/Text/FormattingHelpers.CountDigits.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits_Format(ulong value)
        {
            int digits = 1;
            uint part;
            if (value >= 10000000)
            {
                if (value >= 100000000000000)
                {
                    part = (uint)(value / 100000000000000);
                    digits += 14;
                }
                else
                {
                    part = (uint)(value / 10000000);
                    digits += 7;
                }
            }
            else
            {
                part = (uint)value;
            }

            if (part < 10)
            {
                // no-op
            }
            else if (part < 100)
            {
                digits += 1;
            }
            else if (part < 1000)
            {
                digits += 2;
            }
            else if (part < 10000)
            {
                digits += 3;
            }
            else if (part < 100000)
            {
                digits += 4;
            }
            else if (part < 1000000)
            {
                digits += 5;
            }
            else
            {
                Debug.Assert(part < 10000000);
                digits += 6;
            }

            return digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits_Format(uint value)
        {
            int digits = 1;
            if (value >= 100000)
            {
                value = value / 100000;
                digits += 5;
            }

            if (value < 10)
            {
                // no-op
            }
            else if (value < 100)
            {
                digits += 1;
            }
            else if (value < 1000)
            {
                digits += 2;
            }
            else if (value < 10000)
            {
                digits += 3;
            }
            else
            {
                Debug.Assert(value < 100000);
                digits += 4;
            }

            return digits;
        }

        // https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs

        private static int CountBits_Hash(uint v)
        {
            unchecked
            {
                v = v - ((v >> 1) & 0x55555555u);
                v = (v & 0x33333333u) + ((v >> 2) & 0x33333333u);
                return (int)((v + (v >> 4) & 0xF0F0F0Fu) * 0x1010101u) >> 24;
            }
        }

        // https://raw.githubusercontent.com/dotnet/coreclr/030a3ea9b8dbeae89c90d34441d4d9a1cf4a7de6/tests/src/JIT/Performance/CodeQuality/V8/Crypto/Crypto.cs

        // return number of 1 bits in x
        private static int cbit_BigInt(int x)
        {
            int r = 0;
            while (x != 0)
            { x &= x - 1; ++r; }
            return r;
        }

        #endregion

        #region ROTR (Legacy)

        // https://github.com/dotnet/corert/blob/87e58839d6629b5f90777f886a2f52d7a99c076f/src/System.Private.CoreLib/src/System/Marvin.cs#L120-L124
        private static uint RotateRight_Marvin(uint v, int n)
        {
            Debug.Assert(n >= 0 && n < 32);

            if (n == 0)
            {
                return v;
            }

            return v >> n | (v << (32 - n));
        }

        #endregion

        #region ROTL (Legacy)

        // https://github.com/dotnet/corert/blob/1bb85b989d9b01563df9eb83dce1c6a3415ce182/src/ILCompiler.ReadyToRun/src/Compiler/ReadyToRunHashCode.cs#L214
        private static int RotateLeft_r2r(int value, int bitCount)
        {
            return unchecked((int)(((uint)value << bitCount) | ((uint)value >> (32 - bitCount))));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/NativeFormat/TypeHashingAlgorithms.cs#L20
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint _rotl_hash(uint value, int shift)
        {
            // This is expected to be optimized into a single rol (or ror with negated shift value) instruction
            return (value << shift) | (value >> (32 - shift));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/Common/src/Internal/Text/Utf8String.cs#L47
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl_utf8(int value, int shift)
        {
            // This is expected to be optimized into a single rotl instruction
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/Internal/Runtime/CompilerServices/OpenMethodResolver.cs#L178
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl_omr(int value, int shift)
        {
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.CoreLib/src/System/RuntimeMethodHandle.cs#L67
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl_rtmh(int value, int shift)
        {
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/ILCompiler.Compiler/src/Compiler/DependencyAnalysis/NodeFactory.NativeLayout.cs#L345
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl(int value, int shift)
        {
            // This is expected to be optimized into a single rotl instruction
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        // https://github.com/dotnet/corert/blob/635cf21aca11265ded9d78d216424bd609c052f5/src/System.Private.TypeLoader/src/Internal/Runtime/TypeLoader/TypeLoaderEnvironment.NamedTypeLookup.cs#L121
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl_tle(int value, int shift)
        {
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        #endregion

        #region WriteBit (Legacy)

        // https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        private static uint InsertBit_Hash(int position, uint bits)
        {
            Debug.Assert(0 == (bits & (1u << position)));
            return bits | (1u << position);
        }

        // https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        internal static void SetBit_AD(ref int value, uint bitmask)
        {
            value = (int)(((uint)value) | bitmask);
        }

        // https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        internal static void SetBit_Mcp(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data |= (byte)(1 << bitNumber);
        }

        // https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        public static void SetFlag(ref int bitMask, int bitToSet, bool value)
        {
            // This method cannot be used to clear a flag that has already been set
            Debug.Assert(value || (bitMask & bitToSet) == 0);

            int validMask = GetValidMask(bitToSet);
            int bitsToSet = validMask + (value ? bitToSet : 0);
            SetBit(ref bitMask, bitsToSet);
        }

        public static void SetBit(ref int flags, int mask)
        {
            int oldValue;
            int newValue;
            bool updated;
            do
            {
                oldValue = flags;
                newValue = oldValue | mask;
                updated = oldValue == Interlocked.CompareExchange(ref flags, newValue, oldValue);
            }
            while (!updated);
        }

        #endregion

        #region ClearBit (Legacy)

        // https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        private static uint RemoveBit_Hash(int position, uint bits)
        {
            Debug.Assert(0 != (bits & (1u << position)));
            return bits & ~(1u << position);
        }

        // https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        internal static void ClearBit_AD(ref int value, uint bitmask)
        {
            value = (int)(((uint)value) & ~bitmask);
        }

        // https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        internal static void ClearBit_Mcp(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data &= (byte)~(1 << bitNumber);
        }

        #endregion

        #region ReadBit (Legacy)

        // https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        internal static bool GetBit_Mcp(byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            return ((data >> bitNumber) & 1) == 1;
        }

        internal static void ValidateBitNumber(int bitNumber)
        {
            if (bitNumber < 0 || bitNumber > 7)
            {
                throw new IndexOutOfRangeException("Invalid bit index.");
            }
        }

        // https://raw.githubusercontent.com/dotnet/wpf/2cbb1ad9759c32dc575c7537057a29ee7da2e1b2/src/Microsoft.DotNet.Wpf/src/System.Xaml/System/Xaml/Schema/Reflector.cs
        public static bool? GetFlag(int bitMask, int bitToCheck)
        {
            int validBit = GetValidMask(bitToCheck);
            if (0 != (bitMask & validBit))
            {
                return 0 != (bitMask & bitToCheck);
            }
            return null;
        }

        private static int GetValidMask(int flagMask)
        {
            // Make sure we're only using the low 16 bits for flags)
            Debug.Assert((flagMask & 0xFFFF) == flagMask, "flagMask should only use lower 16 bits of int");
            return flagMask << 16;
        }

        #endregion

        // Proposed new code.
        // Does not all use hardware intrinsics yet.

        #region PopCount

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(byte value)
            => PopCount((uint)value);

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(sbyte value)
            => PopCount(unchecked((byte)value));

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(ushort value)
            => PopCount((uint)value);

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(short value)
            => PopCount(unchecked((ushort)value));

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

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(long value)
            => PopCount(unchecked((ulong)value));

        #endregion

        #region RotateRight

        // Will compile to instrinsics if pattern complies (uint/ulong):
        // https://github.com/dotnet/coreclr/pull/1830
        // There is NO intrinsics support for byte/ushort rotation.

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RotateRight(byte value, int offset)
        {
            int shft = offset & 7;
            uint val = value;

            // Will NOT compile to instrinsics
            val = (val >> shft) | (val << (8 - shft));
            return (byte)val;
        }

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte RotateRight(sbyte value, int offset)
            => unchecked((sbyte)RotateRight((byte)value, offset));

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..15] is treated as congruent mod 16.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RotateRight(ushort value, int offset)
        {
            int shft = offset & 15;
            uint val = value;

            // Will NOT compile to instrinsics
            val = (val >> shft) | (val << (16 - shft));
            return (ushort)val;
        }

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short RotateRight(short value, int offset)
            => unchecked((short)RotateRight((ushort)value, offset));

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

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateRight(int value, int offset)
            => unchecked((int)RotateRight((uint)value, offset));

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateRight(ulong value, int offset)
        {
            ulong val = (value >> offset) | (value << (64 - offset));
            return val;
        }

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RotateRight(long value, int offset)
            => unchecked((long)RotateRight((ulong)value, offset));

        #endregion

        #region RotateLeft

        // Will compile to instrinsics if pattern complies (uint/ulong):
        // https://github.com/dotnet/coreclr/pull/1830
        // There is NO intrinsics support for byte/ushort rotation.

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RotateLeft(byte value, int offset)
        {
            int shft = offset & 7;
            uint val = value;

            // Will NOT compile to instrinsics
            val = (val << shft) | (val >> (8 - shft));
            return (byte)val;
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
        public static sbyte RotateLeft(sbyte value, int offset)
            => unchecked((sbyte)RotateLeft((byte)value, offset));

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..15] is treated as congruent mod 16.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RotateLeft(ushort value, int offset)
        {
            int shft = offset & 15;
            uint val = value;

            // Will NOT compile to instrinsics
            val = (val << shft) | (val >> (16 - shft));
            return (ushort)val;
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
        public static short RotateLeft(short value, int offset)
            => unchecked((short)RotateLeft((ushort)value, offset));

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

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(ulong value, int offset)
        {
            ulong val = (value << offset) | (value >> (64 - offset));
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
        public static long RotateLeft(long value, int offset)
            => unchecked((long)RotateLeft((ulong)value, offset));

        #endregion

        #region LeadingZeroCount

        // Magic C# optimization that directly wraps the data section of the dll (a bit like string constants)
        // https://github.com/dotnet/coreclr/pull/22118#discussion_r249957516
        // https://github.com/dotnet/roslyn/pull/24621
        // https://github.com/benaadams/coreclr/blob/9ba65b563918c778c256f18e234be69174173f12/src/System.Private.CoreLib/shared/System/BitOps.cs
        private static ReadOnlySpan<byte> LeadingZeroCountDeBruijn32 => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        private const uint DeBruijn32 = 0x07C4_ACDDu;

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(byte value)
            => LeadingZeroCount((uint)value) - 24;

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(sbyte value)
            => LeadingZeroCount(unchecked((byte)value));

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(ushort value)
            => LeadingZeroCount((uint)value) - 16;

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(short value)
            => LeadingZeroCount(unchecked((ushort)value));

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
        public static uint LeadingZeroCount(int value)
            => LeadingZeroCount(unchecked((uint)value));

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

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeadingZeroCount(long value)
            => LeadingZeroCount(unchecked((ulong)value));

        #endregion

        #region TrailingZeroCount

        // Magic C# optimization that directly wraps the data section of the dll (a bit like string constants)
        // https://github.com/dotnet/coreclr/pull/22118#discussion_r249957516
        // https://github.com/dotnet/roslyn/pull/24621
        // https://github.com/benaadams/coreclr/blob/9ba65b563918c778c256f18e234be69174173f12/src/System.Private.CoreLib/shared/System/BitOps.cs
        private static ReadOnlySpan<byte> TrailingZeroCountUInt32 => new byte[37]
        {
            32, 00, 01, 26, 02, 23, 27, 32,
            03, 16, 24, 30, 28, 11, 33, 13,
            04, 07, 17, 35, 25, 22, 31, 15,
            29, 10, 12, 06, 34, 21, 14, 09,
            05, 20, 08, 19, 18
        };

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(byte value)
            => Math.Min(8, TrailingZeroCount((uint)value));

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(sbyte value)
            => TrailingZeroCount(unchecked((byte)value));

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(ushort value)
            => Math.Min(16, TrailingZeroCount((uint)value));

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(short value)
            => TrailingZeroCount(unchecked((ushort)value));

        // TODO: Previously implemented in the original class (see top of file)
        // TODO: Benchmark and consolidate
        /*
        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(uint value)
        {
            // The expression (n & -n) returns lsb(n).
            // Only possible values are therefore [0,1,2,4,...]
            long lsb = value & -value; // eg 44==0010 1100 -> (44 & -44) -> 4. 4==0100, which is the lsb of 44.
            lsb %= 37; // mod 37

            // Benchmark: Lookup is 2x faster than Switch
            // Method |     Mean |     Error | StdDev    | Scaled |
            //------- | ---------| ----------| ----------| -------|
            // Lookup | 2.920 ns | 0.0893 ns | 0.2632 ns |   1.00 |
            // Switch | 6.548 ns | 0.1301 ns | 0.2855 ns |   2.26 |

            // long.MaxValue % 37 is always in range [0 - 36] so we use Unsafe.AddByteOffset to avoid bounds check
            ref byte tz = ref MemoryMarshal.GetReference(TrailingZeroCountUInt32);
            byte cnt = Unsafe.AddByteOffset(ref tz, (IntPtr)(int)lsb); // eg 44 -> 2 (44==0010 1100 has 2 trailing zeros)

            // NoOp: Hashing scheme has unused outputs (inputs 4,294,967,296 and higher do not fit a uint)
            Debug.Assert(lsb != 7 && lsb != 14 && lsb != 19 && lsb != 28, $"{value} resulted in unexpected {typeof(uint)} hash {lsb}, with count {cnt}");

            return cnt;
        }
        */

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TrailingZeroCount(ulong value)
        {
            if (Bmi1.X64.IsSupported)
            {
                return (uint)Bmi1.X64.TrailingZeroCount(value);
            }

            // Instead of writing a 64-bit function,
            // we use the 32-bit function twice.

            uint hv = (uint)(value >> 32); // High-32
            uint bv = (uint)value; // Low-32

            uint h, b;
            if (Bmi1.IsSupported)
            {
                h = Bmi1.TrailingZeroCount(hv);
                b = Bmi1.TrailingZeroCount(bv);
            }
            else
            {
                long hi = hv & -hv;
                long bi = bv & -bv;

                hi %= 37; // mod 37
                bi %= 37;

                // long.MaxValue % 37 is always in range [0 - 36] so we use Unsafe.AddByteOffset to avoid bounds check
                ref byte tz = ref MemoryMarshal.GetReference(TrailingZeroCountUInt32);
                h = Unsafe.AddByteOffset(ref tz, (IntPtr)(int)hi);
                b = Unsafe.AddByteOffset(ref tz, (IntPtr)(int)bi); // Use warm cache
            }

            // Keep h iff b==32
            uint mask = b & ~32u; // Zero 5th bit (32)
            mask = IsZero(mask);  // mask == 0 ? 1 : 0
            h = mask * h;

            return b + h;
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The mask.</param>
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
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(sbyte value, int bitOffset)
            => ExtractBit(unchecked((byte)value), bitOffset);

        /// <summary>
        /// Reads whether the specified bit in a mask is set.
        /// Similar in behavior to the x86 instruction BT.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..15] is treated as congruent mod 16.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(ushort value, int bitOffset)
        {
            int shft = bitOffset & 15;
            uint mask = 1U << shft;

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
        public static bool ExtractBit(short value, int bitOffset)
            => ExtractBit(unchecked((ushort)value), bitOffset);

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

        /// <summary>
        /// Reads whether the specified bit in a mask is set.
        /// Similar in behavior to the x86 instruction BT.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to read.
        /// Any value outside the range [0..63] is treated as congruent mod 63.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ExtractBit(ulong value, int bitOffset)
        {
            ulong mask = 1UL << bitOffset;

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
        public static bool ExtractBit(long value, int bitOffset)
            => ExtractBit(unchecked((ulong)value), bitOffset);

        #endregion

        #region ClearBit

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ClearBit(byte value, int bitOffset)
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            return (byte)(value & ~mask);
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ClearBit(sbyte value, int bitOffset)
            => unchecked((sbyte)ClearBit((byte)value, bitOffset));

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..15] is treated as congruent mod 16.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ClearBit(ushort value, int bitOffset)
        {
            int shft = bitOffset & 15;
            uint mask = 1U << shft;

            return (ushort)(value & ~mask);
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ClearBit(short value, int bitOffset)
            => unchecked((short)ClearBit((ushort)value, bitOffset));

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ClearBit(uint value, int bitOffset)
        {
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

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ClearBit(ulong value, int bitOffset)
        {
            ulong mask = 1UL << bitOffset;

            return value & ~mask;
        }

        /// <summary>
        /// Clears the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to clear.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ClearBit(long value, int bitOffset)
            => unchecked((long)ClearBit((ulong)value, bitOffset));

        #endregion

        #region InsertBit

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte InsertBit(byte value, int bitOffset)
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            return (byte)(value | mask);
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte InsertBit(sbyte value, int bitOffset)
            => unchecked((sbyte)InsertBit((byte)value, bitOffset));

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..15] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort InsertBit(ushort value, int bitOffset)
        {
            int shft = bitOffset & 15;
            uint mask = 1U << shft;

            return (ushort)(value | mask);
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short InsertBit(short value, int bitOffset)
            => unchecked((short)InsertBit((ushort)value, bitOffset));

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint InsertBit(uint value, int bitOffset)
        {
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

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong InsertBit(ulong value, int bitOffset)
        {
            ulong mask = 1UL << bitOffset;

            return value | mask;
        }

        /// <summary>
        /// Sets the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to write.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long InsertBit(long value, int bitOffset)
            => unchecked((long)InsertBit((ulong)value, bitOffset));

        #endregion

        #region ComplementBit

        // Truth table (1)
        // v   m  | ~m  ^v  ~
        // 00  01 | 10  10  01
        // 01  01 | 10  11  00
        // 10  01 | 10  00  11
        // 11  01 | 10  01  10
        //
        // 00  10 | 01  01  10
        // 01  10 | 01  00  11
        // 10  10 | 01  11  00
        // 11  10 | 01  10  01

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ComplementBit(byte value, int bitOffset)
        {
            int shft = bitOffset & 7;
            uint mask = 1U << shft;

            mask = ~(~mask ^ value);
            return (byte)mask;
        }

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ComplementBit(sbyte value, int bitOffset)
            => unchecked((sbyte)ComplementBit((byte)value, bitOffset));

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..15] is treated as congruent mod 16.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ComplementBit(ushort value, int bitOffset)
        {
            int shft = bitOffset & 15;
            uint mask = 1U << shft;

            mask = ~(~mask ^ value);
            return (ushort)mask;
        }

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ComplementBit(short value, int bitOffset)
            => unchecked((short)ComplementBit((ushort)value, bitOffset));

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ComplementBit(uint value, int bitOffset)
        {
            uint mask = 1U << bitOffset;

            mask = ~(~mask ^ value);
            return mask;
        }

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComplementBit(int value, int bitOffset)
            => unchecked((int)ComplementBit((uint)value, bitOffset));

        /// <summary>
        /// Complements the specified bit in a mask and returns whether it was originally set.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ComplementBit(ulong value, int bitOffset)
        {
            ulong mask = 1UL << bitOffset;

            mask = ~(~mask ^ value);
            return mask;
        }

        /// <summary>
        /// Complements the specified bit in a mask and returns the new value.
        /// </summary>
        /// <param name="value">The mask.</param>
        /// <param name="bitOffset">The ordinal position of the bit to complement.
        /// Any value outside the range [0..7] is treated as congruent mod 8.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ComplementBit(long value, int bitOffset)
            => unchecked((long)ComplementBit((ulong)value, bitOffset));

        #endregion

        #region Helpers

        // Some of these helpers may be unnecessary depending on how JIT optimizes certain bool operations.

        /// <summary>
        /// Casts the underlying <see cref="byte"/> value from a <see cref="bool"/> without normalization.
        /// Does not incur branching.
        /// </summary>
        /// <param name="condition">The value to cast.</param>
        /// <returns>Returns 0 if <paramref name="condition"/> is False, else returns a non-zero number per the remarks.</returns>
        /// <remarks>The ECMA 335 CLI specification permits a "true" boolean value to be represented by any nonzero value.
        /// See https://github.com/dotnet/roslyn/blob/master/docs/compilers/Boolean%20Representation.md
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte AsByte(ref bool condition)
            => Unsafe.As<bool, byte>(ref condition);

        /// <summary>
        /// Normalizes the underlying <see cref="byte"/> value from a <see cref="bool"/> without branching.
        /// Returns 1 if <paramref name="condition"/> is True, else returns 0.
        /// </summary>
        /// <param name="condition">The value to normalize.</param>
        /// <remarks>The ECMA 335 CLI specification permits a "true" boolean value to be represented by any nonzero value.
        /// See https://github.com/dotnet/roslyn/blob/master/docs/compilers/Boolean%20Representation.md
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Iff(ref bool condition)
            => NonZero(AsByte(ref condition));

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
        private static byte NonZero(ushort value)
            // Negation will set sign-bit iff non-zero
            => unchecked((byte)(((uint)-value) >> 31));

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
        /// Returns 1 if <paramref name="value"/> is non-zero, else returns 0.
        /// Does not incur branching.
        /// Similar in behavior to the x86 instruction CMOVNZ.
        /// </summary>
        /// <param name="value">The value to inspect.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte NonZero(ulong value)
            // Fold into uint
            => NonZero((uint)(value | value >> 32));

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
