// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

using Internal.Runtime.CompilerServices;

#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else
using nint = System.Int32;
using nuint = System.UInt32;
#endif
using Block16 = System.Runtime.Intrinsics.Vector128<byte>;
using Block32 = System.Runtime.Intrinsics.Vector256<byte>;

namespace System
{
    public static partial class Buffer
    {
        public static int ByteLength(Array array)
        {
            // Is the array present?
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Is it of primitive types?
            if (!IsPrimitiveTypeArray(array))
                throw new ArgumentException(SR.Arg_MustBePrimArray, nameof(array));

            return _ByteLength(array);
        }

        public static byte GetByte(Array array, int index)
        {
            // Is the array present?
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Is it of primitive types?
            if (!IsPrimitiveTypeArray(array))
                throw new ArgumentException(SR.Arg_MustBePrimArray, nameof(array));

            // Is the index in valid range of the array?
            if ((uint)index >= (uint)_ByteLength(array))
                throw new ArgumentOutOfRangeException(nameof(index));

            return Unsafe.Add<byte>(ref array.GetRawArrayData(), index);
        }

        public static void SetByte(Array array, int index, byte value)
        {
            // Is the array present?
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Is it of primitive types?
            if (!IsPrimitiveTypeArray(array))
                throw new ArgumentException(SR.Arg_MustBePrimArray, nameof(array));

            // Is the index in valid range of the array?
            if ((uint)index >= (uint)_ByteLength(array))
                throw new ArgumentOutOfRangeException(nameof(index));

            Unsafe.Add<byte>(ref array.GetRawArrayData(), index) = value;
        }

        // This is currently used by System.IO.UnmanagedMemoryStream
        internal static unsafe void ZeroMemory(byte* dest, long len)
        {
            Debug.Assert((ulong)(len) == (nuint)(len));
            ZeroMemory(dest, (nuint)(len));
        }

        // This method has different signature for x64 and other platforms and is done for performance reasons.
        internal static unsafe void ZeroMemory(byte* dest, nuint len)
        {
            SpanHelpers.ClearWithoutReferences(ref *dest, len);
        }

        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static unsafe void MemoryCopy(void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
            }
            Memmove((byte*)destination, (byte*)source, checked((nuint)sourceBytesToCopy));
        }

        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static unsafe void MemoryCopy(void* source, void* destination, ulong destinationSizeInBytes, ulong sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
            }
            Memmove((byte*)destination, (byte*)source, checked((nuint)sourceBytesToCopy));
        }

        internal static unsafe void Memcpy(byte[] dest, int destIndex, byte* src, int srcIndex, int len)
        {
            Debug.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(dest.Length - destIndex >= len, "not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len == 0)
                return;
            fixed (byte* pDest = dest)
            {
                Memcpy(pDest + destIndex, src + srcIndex, len);
            }
        }

        internal static unsafe void Memcpy(byte* pDest, int destIndex, byte[] src, int srcIndex, int len)
        {
            Debug.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len == 0)
                return;
            fixed (byte* pSrc = src)
            {
                Memcpy(pDest + destIndex, pSrc + srcIndex, len);
            }
        }

        // This method has different signature for x64 and other platforms and is done for performance reasons.
        internal static unsafe void Memmove(byte* dest, byte* src, nuint len)
            => Memmove(ref Unsafe.AsRef<byte>(dest), ref Unsafe.AsRef<byte>(src), len);

        // This method has different signature for x64 and other platforms and is done for performance reasons.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Memmove<T>(ref T destination, ref T source, nuint elementCount)
        {
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                // Blittable memmove

                Memmove(
                    ref Unsafe.As<T, byte>(ref destination),
                    ref Unsafe.As<T, byte>(ref source),
                    elementCount * (nuint)Unsafe.SizeOf<T>());
            }
            else
            {
                // Non-blittable memmove

                // Try to avoid calling RhBulkMoveWithWriteBarrier if we can get away
                // with a no-op.
                if (!Unsafe.AreSame(ref destination, ref source) && elementCount != 0)
                {
                    RuntimeImports.RhBulkMoveWithWriteBarrier(
                        ref Unsafe.As<T, byte>(ref destination),
                        ref Unsafe.As<T, byte>(ref source),
                        elementCount * (nuint)Unsafe.SizeOf<T>());
                }
            }
        }

        // This method has different signature for x64 and other platforms and is done for performance reasons.
        private static void Memmove(ref byte dest, ref byte src, nuint len)
        {
            // P/Invoke into the native version when the buffers are overlapping.
            if (((nuint)Unsafe.ByteOffset(ref src, ref dest) < len) || ((nuint)Unsafe.ByteOffset(ref dest, ref src) < len))
            {
                goto BuffersOverlap;
            }

            // Use "(IntPtr)(nint)len" to avoid overflow checking on the explicit cast to IntPtr

            ref byte srcEnd = ref Unsafe.Add(ref src, (IntPtr)(nint)len);
            ref byte destEnd = ref Unsafe.Add(ref dest, (IntPtr)(nint)len);

            if (len <= 16)
                goto MCPY02;
            if (len > 64)
                goto MCPY05;

        MCPY00:
            // Copy bytes which are multiples of 16 and leave the remainder for MCPY01 to handle.
            Debug.Assert(len > 16 && len <= 64);

            Unsafe.As<byte, Block16>(ref dest) = Unsafe.As<byte, Block16>(ref src); // [0,16]

            if (len <= 32)
                goto MCPY01;

            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref src, 16)); // [0,32]

            if (len <= 48)
                goto MCPY01;

            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref src, 32)); // [0,48]

        MCPY01:
            // Unconditionally copy the last 16 bytes using destEnd and srcEnd and return.
            Debug.Assert(len > 16 && len <= 64);

            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref srcEnd, -16));

            return;

        MCPY02:
            // Copy the first 8 bytes and then unconditionally copy the last 8 bytes and return.
            if ((len & 24) == 0)
                goto MCPY03;
            Debug.Assert(len >= 8 && len <= 16);
#if BIT64
            Unsafe.As<byte, long>(ref dest) = Unsafe.As<byte, long>(ref src);
            Unsafe.As<byte, long>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref srcEnd, -8));
#else
            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -4));
#endif
            return;

        MCPY03:
            // Copy the first 4 bytes and then unconditionally copy the last 4 bytes and return.
            if ((len & 4) == 0)
                goto MCPY04;
            Debug.Assert(len >= 4 && len < 8);
            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -4));
            return;

        MCPY04:
            // Copy the first byte. For pending bytes, do an unconditionally copy of the last 2 bytes and return.
            Debug.Assert(len < 4);
            if (len == 0)
                return;
            dest = src;
            if ((len & 2) == 0)
                return;
            Unsafe.As<byte, short>(ref Unsafe.Add(ref destEnd, -2)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref srcEnd, -2));
            return;

        MCPY05:
            // PInvoke to the native version when the copy length exceeds the threshold.
            if (len > MemmoveNativeThreshold)
            {
                goto PInvoke;
            }

            // Copy 64-bytes at a time until the remainder is less than 64.
            // If remainder is greater than 16 bytes, then jump to MCPY00. Otherwise, unconditionally copy the last 16 bytes and return.
            Debug.Assert(len > 64 && len <= MemmoveNativeThreshold);
            nuint n = len >> 6;

        MCPY06:
            Unsafe.As<byte, Block32>(ref dest) = Unsafe.As<byte, Block32>(ref src);
            Unsafe.As<byte, Block32>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, Block32>(ref Unsafe.Add(ref src, 32));

            dest = ref Unsafe.Add(ref dest, 64);
            src = ref Unsafe.Add(ref src, 64);
            n--;
            if (n != 0)
                goto MCPY06;

            len %= 64;
            if (len > 16)
                goto MCPY00;

            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref srcEnd, -16));

            return;

        BuffersOverlap:
            // If the buffers overlap perfectly, there's no point to copying the data.
            if (Unsafe.AreSame(ref dest, ref src))
            {
                return;
            }

        PInvoke:
            _Memmove(ref dest, ref src, len);
        }

        // Non-inlinable wrapper around the QCall that avoids polluting the fast path
        // with P/Invoke prolog/epilog.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void _Memmove(byte* dest, byte* src, nuint len)
        {
            __Memmove(dest, src, len);
        }

        // Non-inlinable wrapper around the QCall that avoids polluting the fast path
        // with P/Invoke prolog/epilog.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void _Memmove(ref byte dest, ref byte src, nuint len)
        {
            fixed (byte* pDest = &dest)
            fixed (byte* pSrc = &src)
                __Memmove(pDest, pSrc, len);
        }
    }
}
