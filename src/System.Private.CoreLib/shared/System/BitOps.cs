// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using Internal.Runtime.CompilerServices;

namespace System
{
    // This class is not meant to be merged in its current form.
    // The duplicate methods are intentional - they show the ubiquity of similar known code across the stack
    // If this PR progresses, the duplicates will be consolidated and optimized, and the call sites updated.
    internal static class BitOps
    {
        // This comment is for tracking purposes and will be deleted
        // PR already merged: https://github.com/dotnet/coreclr/pull/22118

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(int matches)
        {
            if (Bmi1.IsSupported)
            {
                return (int)Bmi1.TrailingZeroCount((uint)matches);
            }
            else // Software fallback
            {
                // https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
                // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
                return Unsafe.AddByteOffset(
                    ref MemoryMarshal.GetReference(TrailingCountMultiplyDeBruijn),
                    ((uint)((matches & -matches) * 0x077CB531U)) >> 27);
            }
        }

        private static ReadOnlySpan<byte> TrailingCountMultiplyDeBruijn => new byte[32]
        {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        #region LZCNT

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
        // It is shown here in its entirety, to show ubiquity of such code.
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

        #region L1CNT

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

        #region POPCNT

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
        
        #region ROTR

        // https://github.com/dotnet/corert/blob/87e58839d6629b5f90777f886a2f52d7a99c076f/src/System.Private.CoreLib/src/System/Marvin.cs#L120-L124
        private static uint RotateRight(uint v, int n)
        {
            Debug.Assert(n >= 0 && n < 32);

            if (n == 0)
            {
                return v;
            }

            return v >> n | (v << (32 - n));
        }

        #endregion

        #region ROTL

        // https://github.com/dotnet/corert/blob/1bb85b989d9b01563df9eb83dce1c6a3415ce182/src/ILCompiler.ReadyToRun/src/Compiler/ReadyToRunHashCode.cs#L214
        private static int RotateLeft(int value, int bitCount)
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

        #region WriteBit

        // https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        private static uint InsertBit_Hash(int position, uint bits)
        {
            Debug.Assert(0 == (bits & (1u << position)));
            return bits | (1u << position);
        }

        // https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        internal static void SetBit_AD(ref int value, uint bitmask)
        {
            value = (int)(((uint)value) | ((uint)bitmask));
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

        #region ClearBit

        // https://raw.githubusercontent.com/dotnet/roslyn/367e08d8f9af968584d5bab84756eceda1587bd9/src/Workspaces/Core/Portable/Utilities/CompilerUtilities/ImmutableHashMap.cs
        private static uint RemoveBit_Hash(int position, uint bits)
        {
            Debug.Assert(0 != (bits & (1u << position)));
            return bits & ~(1u << position);
        }

        // https://raw.githubusercontent.com/dotnet/corefx/bd414c68872c4e4c6e8b1a585675a8383b3a9555/src/System.DirectoryServices.AccountManagement/src/System/DirectoryServices/AccountManagement/Utils.cs
        internal static void ClearBit_AD(ref int value, uint bitmask)
        {
            value = (int)(((uint)value) & ((uint)(~bitmask)));
        }

        // https://raw.githubusercontent.com/dotnet/iot/93f2bd3f2a4d64528ca97a8da09fe0bfe42d648f/src/devices/Mcp23xxx/Mcp23xxx.cs
        internal static void ClearBit_Mcp(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data &= (byte)~(1 << bitNumber);
        }

        #endregion

        #region ReadBit

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
    }
}
