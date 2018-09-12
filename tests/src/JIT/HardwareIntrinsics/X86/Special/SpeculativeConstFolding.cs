// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IntelHardwareIntrinsicTest
{
    [StructLayout(LayoutKind.Sequential, Size = 12)]
    internal struct TwelveBytes
    {
        public double DoubleData;
        public int IntData;
    }

    [StructLayout(LayoutKind.Sequential, Size = 11)]
    internal struct ElevenBytes
    {
        public double DoubleData;
        public ushort UShortData;
        public byte ByteData;
    }

    internal static unsafe class Program
    {
        private static byte* Data;
        private static int* IntData;

        private const int IntConst = 7;
        private const uint UIntConst = 3;
        private const float SingleConst = 5.0F;
        private const double DoubleConst = 11.0;
        private const string StringValue = "9";

        private const int Pass = 100;
        private const int Fail = 0;
        private static int[] bArray = new int[3];
        private static int[] array = new int[] { 1, 2, 3 };
        private static int[] longArray = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)sizeof(TChar));
            return (byte)sizeof(TChar) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TestI<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)sizeof(TChar));
            return (byte)sizeof(TChar) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test1<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + 4 - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + 4 - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test1I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + 4 - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + 4 - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test2<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);

            byte C = default;
            if (sizeof(TChar) == 1)
            {
                C = 3;
            }
            else if (sizeof(TChar) == 2)
            {
                C = 7;
            }
            else if (sizeof(TChar) == 4)
            {
                C = 2;
            }
            else if (sizeof(TChar) == 8)
            {
                C = 11;
            }
            else
            {
                C = 15;
            }

            v = Sse2.ShiftRightLogical128BitLane(v, C);
            return C == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test2I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);

            byte C = default;
            if (sizeof(TChar) == 1)
            {
                C = 3;
            }
            else if (sizeof(TChar) == 2)
            {
                C = 7;
            }
            else if (sizeof(TChar) == 4)
            {
                C = 2;
            }
            else if (sizeof(TChar) == 8)
            {
                C = 11;
            }
            else
            {
                C = 15;
            }

            v = Sse2.ShiftRightLogical128BitLane(v, C);
            return C == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test3<TChar>(int loc = 3) where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + loc - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + loc - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test3I<TChar>(int loc = 3) where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + loc - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + loc - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test4<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + 3 - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + 3 - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test4I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + 3 - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + 3 - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test5<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test5I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)));
            return (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test6<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, sizeof(TChar) > 2 ?
                (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) :
                (byte)((3 * sizeof(TChar) / 2) + DoubleConst - sizeof(TChar)));

            if (sizeof(TChar) > 2)
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar));
            }

            else
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + DoubleConst - sizeof(TChar));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test6I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, sizeof(TChar) > 2 ?
                (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) :
                (byte)((3 * sizeof(TChar) / 2) + DoubleConst - sizeof(TChar)));

            if (sizeof(TChar) > 2)
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar));
            }

            else
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + DoubleConst - sizeof(TChar));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test7<TChar>(int loc = -5) where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, sizeof(TChar) > 2 ?
                (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) :
                (byte)((3 * sizeof(TChar) / 2) + DoubleConst + loc - sizeof(TChar)));

            if (sizeof(TChar) > 2)
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar));
            }

            else
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + DoubleConst + loc - sizeof(TChar));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test7I<TChar>(int loc = -5) where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, sizeof(TChar) > 2 ?
                (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar)) :
                (byte)((3 * sizeof(TChar) / 2) + DoubleConst + loc - sizeof(TChar)));

            if (sizeof(TChar) > 2)
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + SingleConst - sizeof(TChar));
            }

            else
            {
                return Sse41.Extract(v, 0) == (byte)((3 * sizeof(TChar) / 2) + DoubleConst + loc - sizeof(TChar));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test8<TChar>() where TChar : unmanaged
        {
            try
            {
                Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
                v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 0) + 3 - sizeof(TChar)));
                return false;
            }
            catch (DivideByZeroException)
            {
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test8I<TChar>() where TChar : unmanaged
        {
            try
            {
                Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
                v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 0) + 3 - sizeof(TChar)));
                return false;
            }
            catch (DivideByZeroException)
            {
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test9<TChar>() where TChar : unmanaged
        {
            try
            {
                Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
                v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)));
                return (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)) >= 16 ?
                    0 == Sse41.Extract(v, 0) : (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)) == Sse41.Extract(v, 0);
            }
            catch(OverflowException)
            {
                return sizeof(TChar) == 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test9I<TChar>() where TChar : unmanaged
        {
            try
            {
                Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
                v = Sse2.ShiftRightLogical128BitLane(v, (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)));
                return (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)) >= 16 ?
                    0 == Sse41.Extract(v, 0) : (byte)((3 * sizeof(TChar) / 2) + (int.MinValue / (sizeof(TChar) - 3)) - sizeof(TChar)) == Sse41.Extract(v, 0);
            }
            catch (OverflowException)
            {
                return sizeof(TChar) == 2;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test10<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)(StringValue[0] - 0x37 + sizeof(TChar)));
            return (byte)(StringValue[0] - 0x37 + sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test10I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)(StringValue[0] - 0x37 + sizeof(TChar)));
            return (byte)(StringValue[0] - 0x37 + sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test11<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)Math.Sin(StringValue[0] - 0x37 + sizeof(TChar)));
            return (byte)Math.Sin(StringValue[0] - 0x37 + sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test11I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            v = Sse2.ShiftRightLogical128BitLane(v, (byte)Math.Sin(StringValue[0] - 0x37 + sizeof(TChar)));
            return (byte)Math.Sin(StringValue[0] - 0x37 + sizeof(TChar)) == Sse41.Extract(v, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test12<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            return Sse41.Extract(v, (byte)(array.Length)) == array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test12I<TChar>() where TChar : unmanaged
        {
            Vector128<byte> v = Sse2.LoadAlignedVector128(Data);
            return Sse41.Extract(v, (byte)(array.Length)) == array.Length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Test13<TChar>() where TChar : unmanaged
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(array.Length)) == array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Test13I<TChar>() where TChar : unmanaged
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(array.Length)) == array.Length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Test14(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(array.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Test14I(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(array.Length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Test15(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(longArray.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Test15I(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(longArray.Length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Test16(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(bArray.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Test16I(int a)
        {
            Vector128<int> v = Sse2.LoadAlignedVector128(IntData);
            return Sse41.Extract(v, (byte)(bArray.Length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Test17(int a)
        {
            Vector128<int> v = Sse2.SetZeroVector128<int>();
            return Sse41.Extract(v, (byte)(bArray.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Test17I(int a)
        {
            Vector128<int> v = Sse2.SetZeroVector128<int>();
            return Sse41.Extract(v, (byte)(bArray.Length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Test18(int a)
        {
            Vector128<int> v = Sse2.SetZeroVector128<int>();
            return Sse41.Extract(v, (byte)(bArray[a]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Test18I(int a)
        {
            Vector128<int> v = Sse2.SetZeroVector128<int>();
            return Sse41.Extract(v, (byte)(bArray[a]));
        }

        public static T* AlignAs<T>(T* pointer, uint alignment) where T : unmanaged
        {
            return (T*)(((ulong)pointer) + (alignment - ((ulong)pointer % alignment)));
        }

        public static int Main(string[] args)
        {
            byte* buffer = stackalloc byte[128];
            byte* srcBuffer = stackalloc byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            int* intBuffer = stackalloc int[32];
            int* srcIntBuffer = stackalloc int[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            buffer = AlignAs<byte>(buffer, 32);
            Unsafe.CopyBlock(buffer, srcBuffer, 16);
            Data = buffer;
            intBuffer = AlignAs<int>(intBuffer, 32);
            Unsafe.CopyBlock(intBuffer, srcIntBuffer, 16 * sizeof(int));
            IntData = intBuffer;

            int testResult = Pass;

            if (Sse41.IsSupported)
            {
                if (!Test<byte>()) testResult = Fail;
                if (!Test<ushort>()) testResult = Fail;
                if (!Test<int>()) testResult = Fail;
                if (!Test<long>()) testResult = Fail;
                if (!Test<TwelveBytes>()) testResult = Fail;
                if (!Test<ElevenBytes>()) testResult = Fail;

                if (!TestI<byte>()) testResult = Fail;
                if (!TestI<ushort>()) testResult = Fail;
                if (!TestI<int>()) testResult = Fail;
                if (!TestI<long>()) testResult = Fail;
                if (!TestI<TwelveBytes>()) testResult = Fail;
                if (!TestI<ElevenBytes>()) testResult = Fail;

                if (!Test1<byte>()) testResult = Fail;
                if (!Test1<ushort>()) testResult = Fail;
                if (!Test1<int>()) testResult = Fail;
                if (!Test1<long>()) testResult = Fail;
                if (!Test1<TwelveBytes>()) testResult = Fail;
                if (!Test1<ElevenBytes>()) testResult = Fail;

                if (!Test1I<byte>()) testResult = Fail;
                if (!Test1I<ushort>()) testResult = Fail;
                if (!Test1I<int>()) testResult = Fail;
                if (!Test1I<long>()) testResult = Fail;
                if (!Test1I<TwelveBytes>()) testResult = Fail;
                if (!Test1I<ElevenBytes>()) testResult = Fail;

                if (!Test2<byte>()) testResult = Fail;
                if (!Test2<ushort>()) testResult = Fail;
                if (!Test2<int>()) testResult = Fail;
                if (!Test2<long>()) testResult = Fail;
                if (!Test2<TwelveBytes>()) testResult = Fail;
                if (!Test2<ElevenBytes>()) testResult = Fail;

                if (!Test2I<byte>()) testResult = Fail;
                if (!Test2I<ushort>()) testResult = Fail;
                if (!Test2I<int>()) testResult = Fail;
                if (!Test2I<long>()) testResult = Fail;
                if (!Test2I<TwelveBytes>()) testResult = Fail;
                if (!Test2I<ElevenBytes>()) testResult = Fail;

                if (!Test3<byte>()) testResult = Fail;
                if (!Test3<ushort>()) testResult = Fail;
                if (!Test3<int>()) testResult = Fail;
                if (!Test3<long>()) testResult = Fail;
                if (!Test3<TwelveBytes>()) testResult = Fail;
                if (!Test3<ElevenBytes>()) testResult = Fail;

                if (!Test3I<byte>()) testResult = Fail;
                if (!Test3I<ushort>()) testResult = Fail;
                if (!Test3I<int>()) testResult = Fail;
                if (!Test3I<long>()) testResult = Fail;
                if (!Test3I<TwelveBytes>()) testResult = Fail;
                if (!Test3I<ElevenBytes>()) testResult = Fail;

                if (!Test4<byte>()) testResult = Fail;
                if (!Test4<ushort>()) testResult = Fail;
                if (!Test4<int>()) testResult = Fail;
                if (!Test4<long>()) testResult = Fail;
                if (!Test4<TwelveBytes>()) testResult = Fail;
                if (!Test4<ElevenBytes>()) testResult = Fail;

                if (!Test4I<byte>()) testResult = Fail;
                if (!Test4I<ushort>()) testResult = Fail;
                if (!Test4I<int>()) testResult = Fail;
                if (!Test4I<long>()) testResult = Fail;
                if (!Test4I<TwelveBytes>()) testResult = Fail;
                if (!Test4I<ElevenBytes>()) testResult = Fail;

                if (!Test5<byte>()) testResult = Fail;
                if (!Test5<ushort>()) testResult = Fail;
                if (!Test5<int>()) testResult = Fail;
                if (!Test5<long>()) testResult = Fail;
                if (!Test5<TwelveBytes>()) testResult = Fail;
                if (!Test5<ElevenBytes>()) testResult = Fail;

                if (!Test5I<byte>()) testResult = Fail;
                if (!Test5I<ushort>()) testResult = Fail;
                if (!Test5I<int>()) testResult = Fail;
                if (!Test5I<long>()) testResult = Fail;
                if (!Test5I<TwelveBytes>()) testResult = Fail;
                if (!Test5I<ElevenBytes>()) testResult = Fail;

                if (!Test6<byte>()) testResult = Fail;
                if (!Test6<ushort>()) testResult = Fail;
                if (!Test6<int>()) testResult = Fail;
                if (!Test6<long>()) testResult = Fail;
                if (!Test6<TwelveBytes>()) testResult = Fail;
                if (!Test6<ElevenBytes>()) testResult = Fail;

                if (!Test6I<byte>()) testResult = Fail;
                if (!Test6I<ushort>()) testResult = Fail;
                if (!Test6I<int>()) testResult = Fail;
                if (!Test6I<long>()) testResult = Fail;
                if (!Test6I<TwelveBytes>()) testResult = Fail;
                if (!Test6I<ElevenBytes>()) testResult = Fail;

                if (!Test7<byte>()) testResult = Fail;
                if (!Test7<ushort>()) testResult = Fail;
                if (!Test7<int>()) testResult = Fail;
                if (!Test7<long>()) testResult = Fail;
                if (!Test7<TwelveBytes>()) testResult = Fail;
                if (!Test7<ElevenBytes>()) testResult = Fail;

                if (!Test7I<byte>()) testResult = Fail;
                if (!Test7I<ushort>()) testResult = Fail;
                if (!Test7I<int>()) testResult = Fail;
                if (!Test7I<long>()) testResult = Fail;
                if (!Test7I<TwelveBytes>()) testResult = Fail;
                if (!Test7I<ElevenBytes>()) testResult = Fail;

                if (!Test8<byte>()) testResult = Fail;
                if (!Test8<ushort>()) testResult = Fail;
                if (!Test8<int>()) testResult = Fail;
                if (!Test8<long>()) testResult = Fail;
                if (!Test8<TwelveBytes>()) testResult = Fail;
                if (!Test8<ElevenBytes>()) testResult = Fail;

                if (!Test8I<byte>()) testResult = Fail;
                if (!Test8I<ushort>()) testResult = Fail;
                if (!Test8I<int>()) testResult = Fail;
                if (!Test8I<long>()) testResult = Fail;
                if (!Test8I<TwelveBytes>()) testResult = Fail;
                if (!Test8I<ElevenBytes>()) testResult = Fail;

                if (!Test9<byte>()) testResult = Fail;
                if (!Test9<ushort>()) testResult = Fail;
                if (!Test9<int>()) testResult = Fail;
                if (!Test9<long>()) testResult = Fail;
                if (!Test9<TwelveBytes>()) testResult = Fail;
                if (!Test9<ElevenBytes>()) testResult = Fail;

                if (!Test9I<byte>()) testResult = Fail;
                if (!Test9I<ushort>()) testResult = Fail;
                if (!Test9I<int>()) testResult = Fail;
                if (!Test9I<long>()) testResult = Fail;
                if (!Test9I<TwelveBytes>()) testResult = Fail;
                if (!Test9I<ElevenBytes>()) testResult = Fail;

                if (!Test10<byte>()) testResult = Fail;
                if (!Test10<ushort>()) testResult = Fail;
                if (!Test10<int>()) testResult = Fail;
                if (!Test10<long>()) testResult = Fail;
                if (!Test10<TwelveBytes>()) testResult = Fail;
                if (!Test10<ElevenBytes>()) testResult = Fail;

                if (!Test10I<byte>()) testResult = Fail;
                if (!Test10I<ushort>()) testResult = Fail;
                if (!Test10I<int>()) testResult = Fail;
                if (!Test10I<long>()) testResult = Fail;
                if (!Test10I<TwelveBytes>()) testResult = Fail;
                if (!Test10I<ElevenBytes>()) testResult = Fail;

                if (!Test11<byte>()) testResult = Fail;
                if (!Test11<ushort>()) testResult = Fail;
                if (!Test11<int>()) testResult = Fail;
                if (!Test11<long>()) testResult = Fail;
                if (!Test11<TwelveBytes>()) testResult = Fail;
                if (!Test11<ElevenBytes>()) testResult = Fail;

                if (!Test11I<byte>()) testResult = Fail;
                if (!Test11I<ushort>()) testResult = Fail;
                if (!Test11I<int>()) testResult = Fail;
                if (!Test11I<long>()) testResult = Fail;
                if (!Test11I<TwelveBytes>()) testResult = Fail;
                if (!Test11I<ElevenBytes>()) testResult = Fail;

                if (!Test12<byte>()) testResult = Fail;
                if (!Test12<ushort>()) testResult = Fail;
                if (!Test12<int>()) testResult = Fail;
                if (!Test12<long>()) testResult = Fail;
                if (!Test12<TwelveBytes>()) testResult = Fail;
                if (!Test12<ElevenBytes>()) testResult = Fail;

                if (!Test12I<byte>()) testResult = Fail;
                if (!Test12I<ushort>()) testResult = Fail;
                if (!Test12I<int>()) testResult = Fail;
                if (!Test12I<long>()) testResult = Fail;
                if (!Test12I<TwelveBytes>()) testResult = Fail;
                if (!Test12I<ElevenBytes>()) testResult = Fail;

                if (!Test13<byte>()) testResult = Fail;
                if (!Test13<ushort>()) testResult = Fail;
                if (!Test13<int>()) testResult = Fail;
                if (!Test13<long>()) testResult = Fail;
                if (!Test13<TwelveBytes>()) testResult = Fail;
                if (!Test13<ElevenBytes>()) testResult = Fail;

                if (!Test13I<byte>()) testResult = Fail;
                if (!Test13I<ushort>()) testResult = Fail;
                if (!Test13I<int>()) testResult = Fail;
                if (!Test13I<long>()) testResult = Fail;
                if (!Test13I<TwelveBytes>()) testResult = Fail;
                if (!Test13I<ElevenBytes>()) testResult = Fail;

                if (Test14(1) != 3) testResult = Fail;
                if (Test14I(1) != 3) testResult = Fail;

                if (Test15(1) != 0) testResult = Fail;
                if (Test15I(1) != 0) testResult = Fail;

                if (Test16(1) != 3) testResult = Fail;
                if (Test16I(1) != 3) testResult = Fail;

                if (Test17(1) != 0) testResult = Fail;
                if (Test17I(1) != 0) testResult = Fail;

                if (Test18(1) != 0) testResult = Fail;
                if (Test18I(1) != 0) testResult = Fail;
            }

            if (args.Length > 0)
            {
                Console.WriteLine($"Test passed: {testResult == 100}");
            }

            return testResult;
        }
    }
}
