// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

namespace System
{
    // The BitConverter class contains methods for
    // converting an array of bytes to one of the base data 
    // types, as well as for converting a base data type to an
    // array of bytes.
    public static class BitConverter
    {
        // This field indicates the "endianess" of the architecture.
        // The value is set to true if the architecture is
        // little endian; false if it is big endian.
#if BIGENDIAN
        public static readonly bool IsLittleEndian /* = false */;
#else
        public static readonly bool IsLittleEndian = true;
#endif

        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ByteSwap(ushort value)
        {
            // Don't need to AND with 0xFF00 or 0x00FF since the final
            // cast back to ushort will clear out all bits above [ 15 .. 00 ].
            // This is normally implemented via "movzx eax, ax" on the return.
            // Alternatively, the compiler could elide the movzx instruction
            // entirely if it knows the caller is only going to access "ax"
            // instead of "eax" / "rax" when the function returns.

            return (ushort)((value >> 8) + (value << 8));
        }

        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ByteSwap(uint value)
        {
            // This takes advantage of the fact that the JIT can detect
            // ROL32 / ROR32 patterns and output the correct intrinsic.
            //
            // Input: value = [ ww xx yy zz ]
            //
            // First line generates : [ ww xx yy zz ]
            //                      & [ 00 FF 00 FF ]
            //                      = [ 00 xx 00 zz ]
            //             ROR32(8) = [ zz 00 xx 00 ]
            //
            // Second line generates: [ ww xx yy zz ]
            //                      & [ FF 00 FF 00 ]
            //                      = [ ww 00 yy 00 ]
            //             ROL32(8) = [ 00 yy 00 ww ]
            //
            //                (sum) = [ zz yy xx ww ]
            //
            // Testing shows that throughput increases if the AND
            // is performed before the ROL / ROR.

            uint mask_xx_zz = (value & 0x00FF00FFU);
            uint mask_ww_yy = (value & 0xFF00FF00U);
            return ((mask_xx_zz >> 8) | (mask_xx_zz << 24))
                + ((mask_ww_yy << 8) | (mask_ww_yy >> 24));
        }

        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ByteSwap(ulong value)
        {
            // Operations on 32-bit values have higher throughput than
            // operations on 64-bit values, so decompose.

            return ((ulong)ByteSwap((uint)value) << 32)
                + ByteSwap((uint)(value >> 32));
        }

        // Converts a Boolean into an array of bytes with length one.
        public static byte[] GetBytes(bool value)
        {
            byte[] r = new byte[1];
            r[0] = (value ? (byte)1 : (byte)0);
            return r;
        }

        // Converts a Boolean into a Span of bytes with length one.
        public static bool TryWriteBytes(Span<byte> destination, bool value)
        {
            if (destination.Length < sizeof(byte))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value ? (byte)1 : (byte)0);
            return true;
        }

        // Converts a char into an array of bytes with length two.
        public static byte[] GetBytes(char value)
        {
            byte[] bytes = new byte[sizeof(char)];
            Unsafe.As<byte, char>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a char into a Span
        public static bool TryWriteBytes(Span<byte> destination, char value)
        {
            if (destination.Length < sizeof(char))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts a short into an array of bytes with length
        // two.
        public static byte[] GetBytes(short value)
        {
            byte[] bytes = new byte[sizeof(short)];
            Unsafe.As<byte, short>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a short into a Span
        public static bool TryWriteBytes(Span<byte> destination, short value)
        {
            if (destination.Length < sizeof(short))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts an int into an array of bytes with length 
        // four.
        public static byte[] GetBytes(int value)
        {
            byte[] bytes = new byte[sizeof(int)];
            Unsafe.As<byte, int>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts an int into a Span
        public static bool TryWriteBytes(Span<byte> destination, int value)
        {
            if (destination.Length < sizeof(int))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts a long into an array of bytes with length 
        // eight.
        public static byte[] GetBytes(long value)
        {
            byte[] bytes = new byte[sizeof(long)];
            Unsafe.As<byte, long>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a long into a Span
        public static bool TryWriteBytes(Span<byte> destination, long value)
        {
            if (destination.Length < sizeof(long))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts an ushort into an array of bytes with
        // length two.
        [CLSCompliant(false)]
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = new byte[sizeof(ushort)];
            Unsafe.As<byte, ushort>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a ushort into a Span
        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, ushort value)
        {
            if (destination.Length < sizeof(ushort))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts an uint into an array of bytes with
        // length four.
        [CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = new byte[sizeof(uint)];
            Unsafe.As<byte, uint>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a uint into a Span
        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, uint value)
        {
            if (destination.Length < sizeof(uint))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts an unsigned long into an array of bytes with
        // length eight.
        [CLSCompliant(false)]
        public static byte[] GetBytes(ulong value)
        {
            byte[] bytes = new byte[sizeof(ulong)];
            Unsafe.As<byte, ulong>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a ulong into a Span
        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, ulong value)
        {
            if (destination.Length < sizeof(ulong))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts a float into an array of bytes with length 
        // four.
        public static byte[] GetBytes(float value)
        {
            byte[] bytes = new byte[sizeof(float)];
            Unsafe.As<byte, float>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a float into a Span
        public static bool TryWriteBytes(Span<byte> destination, float value)
        {
            if (destination.Length < sizeof(float))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts a double into an array of bytes with length 
        // eight.
        public static byte[] GetBytes(double value)
        {
            byte[] bytes = new byte[sizeof(double)];
            Unsafe.As<byte, double>(ref bytes[0]) = value;
            return bytes;
        }

        // Converts a double into a Span
        public static bool TryWriteBytes(Span<byte> destination, double value)
        {
            if (destination.Length < sizeof(double))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        // Converts an array of bytes into a char.  
        public static char ToChar(byte[] value, int startIndex) => unchecked((char)ToInt16(value, startIndex));

        // Converts a Span into a char
        public static char ToChar(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(char))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<char>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into a short.  
        public static short ToInt16(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(short))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<short>(ref value[startIndex]);
        }

        // Converts a Span into a short
        public static short ToInt16(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(short))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<short>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into an int.  
        public static int ToInt32(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(int))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<int>(ref value[startIndex]);
        }

        // Converts a Span into an int
        public static int ToInt32(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(int))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into a long.  
        public static long ToInt64(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(long))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<long>(ref value[startIndex]);
        }

        // Converts a Span into a long
        public static long ToInt64(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(long))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into an ushort.
        // 
        [CLSCompliant(false)]
        public static ushort ToUInt16(byte[] value, int startIndex) => unchecked((ushort)ToInt16(value, startIndex));

        // Converts a Span into a ushort
        [CLSCompliant(false)]
        public static ushort ToUInt16(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(ushort))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into an uint.
        // 
        [CLSCompliant(false)]
        public static uint ToUInt32(byte[] value, int startIndex) => unchecked((uint)ToInt32(value, startIndex));

        // Convert a Span into a uint
        [CLSCompliant(false)]
        public static uint ToUInt32(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(uint))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into an unsigned long.
        // 
        [CLSCompliant(false)]
        public static ulong ToUInt64(byte[] value, int startIndex) => unchecked((ulong)ToInt64(value, startIndex));

        // Converts a Span into an unsigned long
        [CLSCompliant(false)]
        public static ulong ToUInt64(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(ulong))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into a float.  
        public static float ToSingle(byte[] value, int startIndex) => Int32BitsToSingle(ToInt32(value, startIndex));

        // Converts a Span into a float
        public static float ToSingle(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(float))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<float>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into a double.  
        public static double ToDouble(byte[] value, int startIndex) => Int64BitsToDouble(ToInt64(value, startIndex));

        // Converts a Span into a double
        public static double ToDouble(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(double))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<double>(ref MemoryMarshal.GetReference(value));
        }

        // Converts an array of bytes into a String.  
        public static string ToString(byte[] value, int startIndex, int length)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (startIndex < 0 || startIndex >= value.Length && startIndex > 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_GenericPositive);
            if (startIndex > value.Length - length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            if (length == 0)
            {
                return string.Empty;
            }

            if (length > (int.MaxValue / 3))
            {
                // (int.MaxValue / 3) == 715,827,882 Bytes == 699 MB
                throw new ArgumentOutOfRangeException(nameof(length), SR.Format(SR.ArgumentOutOfRange_LengthTooLarge, (int.MaxValue / 3)));
            }

            return string.Create(length * 3 - 1, (value, startIndex, length), (dst, state) =>
            {
                const string HexValues = "0123456789ABCDEF";

                var src = new ReadOnlySpan<byte>(state.value, state.startIndex, state.length);

                int i = 0;
                int j = 0;

                byte b = src[i++];
                dst[j++] = HexValues[b >> 4];
                dst[j++] = HexValues[b & 0xF];

                while (i < src.Length)
                {
                    b = src[i++];
                    dst[j++] = '-';
                    dst[j++] = HexValues[b >> 4];
                    dst[j++] = HexValues[b & 0xF];
                }
            });
        }

        // Converts an array of bytes into a String.  
        public static string ToString(byte[] value)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            return ToString(value, 0, value.Length);
        }

        // Converts an array of bytes into a String.  
        public static string ToString(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            return ToString(value, startIndex, value.Length - startIndex);
        }

        /*==================================ToBoolean===================================
        **Action:  Convert an array of bytes to a boolean value.  We treat this array 
        **         as if the first 4 bytes were an Int4 an operate on this value.
        **Returns: True if the Int4 value of the first 4 bytes is non-zero.
        **Arguments: value -- The byte array
        **           startIndex -- The position within the array.
        **Exceptions: See ToInt4.
        ==============================================================================*/
        // Converts an array of bytes into a boolean.  
        public static bool ToBoolean(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (startIndex < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - 1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index); // differs from other overloads, which throw base ArgumentException

            return value[startIndex] != 0;
        }

        public static bool ToBoolean(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(byte))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<byte>(ref MemoryMarshal.GetReference(value)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe long DoubleToInt64Bits(double value)
        {
            return *((long*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double Int64BitsToDouble(long value)
        {
            return *((double*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int SingleToInt32Bits(float value)
        {
            return *((int*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float Int32BitsToSingle(int value)
        {
            return *((float*)&value);
        }
    }
}
