// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

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
        private static T ReadUnaligned<T>(ReadOnlySpan<byte> value) where T : struct
        {
            if (value.Length < Unsafe.SizeOf<T>())
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            return Unsafe.ReadUnaligned<T>(ref value.DangerousGetPinnableReference());
        }

        private static bool WriteUnaligned<T>(T value, Span<byte> bytes) where T : struct
        {
            if (bytes.Length < Unsafe.SizeOf<T>())
                return false;
            Unsafe.As<byte, T>(ref bytes.DangerousGetPinnableReference());
            return true;
        }

        // Converts a Boolean into an array of bytes with length one.
        public static byte[] GetBytes(bool value)
        {
            byte[] r = new byte[1];
            r[0] = (value ? (byte)1 : (byte)0);
            return r;
        }

        // Converts a Boolean into a Span of bytes with length one.
        public static bool TryWriteBytes(Span<byte> destination, bool value) =>
            WriteUnaligned(value ? (byte)1 : (byte)0, destination);

        // Converts a char into an array of bytes with length two.
        public static byte[] GetBytes(char value)
        {
            byte[] bytes = new byte[sizeof(char)];
            Unsafe.As<byte, char>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, char value) =>
            WriteUnaligned(value, destination);

        // Converts a short into an array of bytes with length
        // two.
        public static byte[] GetBytes(short value)
        {
            byte[] bytes = new byte[sizeof(short)];
            Unsafe.As<byte, short>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, short value) =>
            WriteUnaligned(value, destination);

        // Converts an int into an array of bytes with length 
        // four.
        public static byte[] GetBytes(int value)
        {
            byte[] bytes = new byte[sizeof(int)];
            Unsafe.As<byte, int>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, int value) =>
            WriteUnaligned(value, destination);

        // Converts a long into an array of bytes with length 
        // eight.
        public static byte[] GetBytes(long value)
        {
            byte[] bytes = new byte[sizeof(long)];
            Unsafe.As<byte, long>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, long value) =>
            WriteUnaligned(value, destination);

        // Converts an ushort into an array of bytes with
        // length two.
        [CLSCompliant(false)]
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = new byte[sizeof(ushort)];
            Unsafe.As<byte, ushort>(ref bytes[0]) = value;
            return bytes;
        }

        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, ushort value) =>
            WriteUnaligned(value, destination);

        // Converts an uint into an array of bytes with
        // length four.
        [CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = new byte[sizeof(uint)];
            Unsafe.As<byte, uint>(ref bytes[0]) = value;
            return bytes;
        }

        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, uint value) =>
            WriteUnaligned(value, destination);

        // Converts an unsigned long into an array of bytes with
        // length eight.
        [CLSCompliant(false)]
        public static byte[] GetBytes(ulong value)
        {
            byte[] bytes = new byte[sizeof(ulong)];
            Unsafe.As<byte, ulong>(ref bytes[0]) = value;
            return bytes;
        }

        [CLSCompliant(false)]
        public static bool TryWriteBytes(Span<byte> destination, ulong value) =>
            WriteUnaligned(value, destination);

        // Converts a float into an array of bytes with length 
        // four.
        public static byte[] GetBytes(float value)
        {
            byte[] bytes = new byte[sizeof(float)];
            Unsafe.As<byte, float>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, float value) =>
            WriteUnaligned(value, destination);

        // Converts a double into an array of bytes with length 
        // eight.
        public static byte[] GetBytes(double value)
        {
            byte[] bytes = new byte[sizeof(double)];
            Unsafe.As<byte, double>(ref bytes[0]) = value;
            return bytes;
        }

        public static bool TryWriteBytes(Span<byte> destination, double value) =>
            WriteUnaligned(value, destination);

        // Converts an array of bytes into a char.  
        public static char ToChar(byte[] value, int startIndex) => unchecked((char)ReadInt16(value, startIndex));
        public static char ToChar(ReadOnlySpan<byte> value) => ReadUnaligned<char>(value);

        private static short ReadInt16(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(short))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<short>(ref value[startIndex]);
        }

        private static int ReadInt32(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(int))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<int>(ref value[startIndex]);
        }

        private static long ReadInt64(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            if (unchecked((uint)startIndex) >= unchecked((uint)value.Length))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            if (startIndex > value.Length - sizeof(long))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall, ExceptionArgument.value);

            return Unsafe.ReadUnaligned<long>(ref value[startIndex]);
        }

        // Converts an array of bytes into a short.  
        public static short ToInt16(byte[] value, int startIndex) => ReadInt16(value, startIndex);
        public static short ToInt16(ReadOnlySpan<byte> value) => ReadUnaligned<short>(value);

        // Converts an array of bytes into an int.  
        public static int ToInt32(byte[] value, int startIndex) => ReadInt32(value, startIndex);
        public static int ToInt32(ReadOnlySpan<byte> value) => ReadUnaligned<int>(value);

        // Converts an array of bytes into a long.  
        public static long ToInt64(byte[] value, int startIndex) => ReadInt64(value, startIndex);
        public static long ToInt64(ReadOnlySpan<byte> value) => ReadUnaligned<long>(value);

        // Converts an array of bytes into an ushort.
        // 
        [CLSCompliant(false)]
        public static ushort ToUInt16(byte[] value, int startIndex) => unchecked((ushort)ReadInt16(value, startIndex));
        [CLSCompliant(false)]
        public static ushort ToUInt16(ReadOnlySpan<byte> value) => ReadUnaligned<ushort>(value);

        // Converts an array of bytes into an uint.
        // 
        [CLSCompliant(false)]
        public static uint ToUInt32(byte[] value, int startIndex) => unchecked((uint)ReadInt32(value, startIndex));
        [CLSCompliant(false)]
        public static uint ToUInt32(ReadOnlySpan<byte> value) => ReadUnaligned<uint>(value);

        // Converts an array of bytes into an unsigned long.
        // 
        [CLSCompliant(false)]
        public static ulong ToUInt64(byte[] value, int startIndex) => unchecked((ulong)ReadInt64(value, startIndex));
        [CLSCompliant(false)]
        public static ulong ToUInt64(ReadOnlySpan<byte> value) => ReadUnaligned<ulong>(value);

        // Converts an array of bytes into a float.  
        public static unsafe float ToSingle(byte[] value, int startIndex)
        {
            int val = ReadInt32(value, startIndex);
            return *(float*)&val;
        }

        public static float ToSingle(ReadOnlySpan<byte> value) => ReadUnaligned<float>(value);

        // Converts an array of bytes into a double.  
        public static unsafe double ToDouble(byte[] value, int startIndex)
        {
            long val = ReadInt64(value, startIndex);
            return *(double*)&val;
        }

        public static double ToDouble(ReadOnlySpan<byte> value) => ReadUnaligned<double>(value);

        private static char GetHexValue(int i)
        {
            Debug.Assert(i >= 0 && i < 16, "i is out of range.");
            if (i < 10)
            {
                return (char)(i + '0');
            }

            return (char)(i - 10 + 'A');
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
                // (Int32.MaxValue / 3) == 715,827,882 Bytes == 699 MB
                throw new ArgumentOutOfRangeException(nameof(length), SR.Format(SR.ArgumentOutOfRange_LengthTooLarge, (int.MaxValue / 3)));
            }

            int chArrayLength = length * 3;
            const int StackLimit = 512; // arbitrary limit to switch from stack to heap allocation
            unsafe
            {
                if (chArrayLength < StackLimit)
                {
                    char* chArrayPtr = stackalloc char[chArrayLength];
                    return ToString(value, startIndex, length, chArrayPtr, chArrayLength);
                }
                else
                {
                    char[] chArray = new char[chArrayLength];
                    fixed (char* chArrayPtr = &chArray[0])
                        return ToString(value, startIndex, length, chArrayPtr, chArrayLength);
                }
            }
        }

        private static unsafe string ToString(byte[] value, int startIndex, int length, char* chArray, int chArrayLength)
        {
            Debug.Assert(length > 0);
            Debug.Assert(chArrayLength == length * 3);

            char* p = chArray;
            int endIndex = startIndex + length;
            for (int i = startIndex; i < endIndex; i++)
            {
                byte b = value[i];
                *p++ = GetHexValue(b >> 4);
                *p++ = GetHexValue(b & 0xF);
                *p++ = '-';
            }

            // We don't need the last '-' character
            return new string(chArray, 0, chArrayLength - 1);
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

        public static bool ToBoolean(ReadOnlySpan<byte> value) => ReadUnaligned<byte>(value) != 0;

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
