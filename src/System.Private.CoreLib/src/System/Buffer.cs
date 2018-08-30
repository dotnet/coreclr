// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if AMD64 || ARM64 || (BIT32 && !ARM)
#define HAS_CUSTOM_BLOCKS
#endif

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Security;
using System.Runtime;
using Internal.Runtime.CompilerServices;

#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else // BIT64
using nint = System.Int32;
using nuint = System.UInt32;
#endif // BIT64

namespace System
{
    public static class Buffer
    {
        // Copies from one primitive array to another primitive array without
        // respecting types.  This calls memmove internally.  The count and 
        // offset parameters here are in bytes.  If you want to use traditional
        // array element indices and counts, use Array.Copy.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void BlockCopy(Array src, int srcOffset,
            Array dst, int dstOffset, int count);

        // Returns a bool to indicate if the array is of primitive data types
        // or not.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool IsPrimitiveTypeArray(Array array);

        // Gets a particular byte out of the array.  The array must be an
        // array of primitives.  
        //
        // This essentially does the following: 
        // return ((byte*)array) + index.
        //
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern byte _GetByte(Array array, int index);

        public static byte GetByte(Array array, int index)
        {
            // Is the array present?
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Is it of primitive types?
            if (!IsPrimitiveTypeArray(array))
                throw new ArgumentException(SR.Arg_MustBePrimArray, nameof(array));

            // Is the index in valid range of the array?
            if (index < 0 || index >= _ByteLength(array))
                throw new ArgumentOutOfRangeException(nameof(index));

            return _GetByte(array, index);
        }

        // Sets a particular byte in an the array.  The array must be an
        // array of primitives.  
        //
        // This essentially does the following: 
        // *(((byte*)array) + index) = value.
        //
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void _SetByte(Array array, int index, byte value);

        public static void SetByte(Array array, int index, byte value)
        {
            // Is the array present?
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Is it of primitive types?
            if (!IsPrimitiveTypeArray(array))
                throw new ArgumentException(SR.Arg_MustBePrimArray, nameof(array));

            // Is the index in valid range of the array?
            if (index < 0 || index >= _ByteLength(array))
                throw new ArgumentOutOfRangeException(nameof(index));

            // Make the FCall to do the work
            _SetByte(array, index, value);
        }


        // Gets a particular byte out of the array.  The array must be an
        // array of primitives.  
        //
        // This essentially does the following: 
        // return array.length * sizeof(array.UnderlyingElementType).
        //
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int _ByteLength(Array array);

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

        internal static unsafe void ZeroMemory(byte* src, long len)
        {
            while (len-- > 0)
                *(src + len) = 0;
        }
        
        internal static unsafe void Memcpy(byte* pDest, int destIndex, byte[] src, int srcIndex, int len)
        {
            Debug.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len == 0)
                return;

            Memmove(ref *(pDest + destIndex), ref Unsafe.Add(ref src.GetRawSzArrayData(), srcIndex), (uint)len);
        }

        // This is tricky to get right AND fast, so lets make it useful for the whole Fx.
        // E.g. System.Runtime.WindowsRuntime!WindowsRuntimeBufferExtensions.MemCopy uses it.

        // This method has a slightly different behavior on arm and other platforms.
        // On arm this method behaves like memcpy and does not handle overlapping buffers.
        // While on other platforms it behaves like memmove and handles overlapping buffers.
        // This behavioral difference is unfortunate but intentional because
        // 1. This method is given access to other internal dlls and this close to release we do not want to change it.
        // 2. It is difficult to get this right for arm and again due to release dates we would like to visit it later.
#if ARM
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe void Memcpy(byte* dest, byte* src, int len);
#else // ARM
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Memcpy(byte* dest, byte* src, int len)
        {
            Debug.Assert(len >= 0, "Negative length in memcopy!");
            Memmove(ref *dest, ref *src, (uint)len);
        }
#endif // ARM
        
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
#if AMD64 || (BIT32 && !ARM)
            const nuint CopyThreshold = 2048;
#elif ARM64
#if PLATFORM_WINDOWS
            // Determined optimal value for Windows.
            // https://github.com/dotnet/coreclr/issues/13843
            const nuint CopyThreshold = ulong.MaxValue;
#else // PLATFORM_WINDOWS
            // Managed code is currently faster than glibc unoptimized memmove
            // TODO-ARM64-UNIX-OPT revisit when glibc optimized memmove is in Linux distros
            // https://github.com/dotnet/coreclr/issues/13844
            const nuint CopyThreshold = ulong.MaxValue;
#endif // PLATFORM_WINDOWS
#else
            const nuint CopyThreshold = 512;
#endif // AMD64 || (BIT32 && !ARM)

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
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block16>(ref dest) = Unsafe.As<byte, Block16>(ref src); // [0,16]
#elif BIT64
            Unsafe.As<byte, long>(ref dest) = Unsafe.As<byte, long>(ref src);
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 8)); // [0,16]
#else
            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12)); // [0,16]
#endif
            if (len <= 32)
                goto MCPY01;
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref src, 16)); // [0,32]
#elif BIT64
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 24)); // [0,32]
#else
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 28)); // [0,32]
#endif
            if (len <= 48)
                goto MCPY01;
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref src, 32)); // [0,48]
#elif BIT64
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 32));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 40)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 40)); // [0,48]
#else
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 32));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 36)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 36));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 40)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 40));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 44)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 44)); // [0,48]
#endif

MCPY01:
// Unconditionally copy the last 16 bytes using destEnd and srcEnd and return.
            Debug.Assert(len > 16 && len <= 64);
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref srcEnd, -16));
#elif BIT64
            Unsafe.As<byte, long>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref srcEnd, -16));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref srcEnd, -8));
#else
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -4));
#endif
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
            if (len > CopyThreshold)
            {
                goto PInvoke;
            }
            // Copy 64-bytes at a time until the remainder is less than 64.
            // If remainder is greater than 16 bytes, then jump to MCPY00. Otherwise, unconditionally copy the last 16 bytes and return.
            Debug.Assert(len > 64 && len <= CopyThreshold);
            nuint n = len >> 6;

MCPY06:
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block64>(ref dest) = Unsafe.As<byte, Block64>(ref src);
#elif BIT64
            Unsafe.As<byte, long>(ref dest) = Unsafe.As<byte, long>(ref src);
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 32));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 40)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 40));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 48)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 48));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref dest, 56)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref src, 56));
#else
            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 28));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 32)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 32));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 36)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 36));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 40)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 40));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 44)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 44));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 48)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 48));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 52)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 52));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 56)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 56));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 60)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 60));
#endif
            dest = ref Unsafe.Add(ref dest, 64);
            src = ref Unsafe.Add(ref src, 64);
            n--;
            if (n != 0)
                goto MCPY06;

            len %= 64;
            if (len > 16)
                goto MCPY00;
#if HAS_CUSTOM_BLOCKS
            Unsafe.As<byte, Block16>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, Block16>(ref Unsafe.Add(ref srcEnd, -16));
#elif BIT64
            Unsafe.As<byte, long>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref srcEnd, -16));
            Unsafe.As<byte, long>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, long>(ref Unsafe.Add(ref srcEnd, -8));
#else
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref destEnd, -4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref srcEnd, -4));
#endif
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
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static unsafe void _Memmove(ref byte dest, ref byte src, nuint len)
        {
            fixed (byte* pDest = &dest)
            fixed (byte* pSrc = &src)
                __Memmove(pDest, pSrc, len);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern unsafe void __Memmove(byte* dest, byte* src, nuint len);

        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static unsafe void MemoryCopy(void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
            }
            Memmove(ref *(byte*)destination, ref *(byte*)source, checked((nuint)sourceBytesToCopy));
        }


        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static unsafe void MemoryCopy(void* source, void* destination, ulong destinationSizeInBytes, ulong sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
            }

#if BIT64
            Memmove(ref *(byte*)destination, ref *(byte*)source, sourceBytesToCopy);
#else // BIT64
            Memmove(ref *(byte*)destination, ref *(byte*)source, checked((uint)sourceBytesToCopy));
#endif // BIT64
        }
        
#if HAS_CUSTOM_BLOCKS        
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Block16 { }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Block64 { } 
#endif // HAS_CUSTOM_BLOCKS         
    }
}
