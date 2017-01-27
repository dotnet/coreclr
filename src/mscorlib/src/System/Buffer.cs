// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System {
    
    //Only contains static methods.  Does not require serialization
    
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Security;
    using System.Runtime;

#if BIT64
    using nuint = System.UInt64;
#else // BIT64
    using nuint = System.UInt32;
#endif // BIT64

[System.Runtime.InteropServices.ComVisible(true)]
    public static class Buffer
    {
        // Copies from one primitive array to another primitive array without
        // respecting types.  This calls memmove internally.  The count and 
        // offset parameters here are in bytes.  If you want to use traditional
        // array element indices and counts, use Array.Copy.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void BlockCopy(Array src, int srcOffset,
            Array dst, int dstOffset, int count);

        // A very simple and efficient memmove that assumes all of the
        // parameter validation has already been done.  The count and offset
        // parameters here are in bytes.  If you want to use traditional
        // array element indices and counts, use Array.Copy.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void InternalBlockCopy(Array src, int srcOffsetBytes,
            Array dst, int dstOffsetBytes, int byteCount);

        // This is ported from the optimized CRT assembly in memchr.asm. The JIT generates 
        // pretty good code here and this ends up being within a couple % of the CRT asm.
        // It is however cross platform as the CRT hasn't ported their fast version to 64-bit
        // platforms.
        //
        internal unsafe static int IndexOfByte(byte* src, byte value, int index, int count)
        {
            Debug.Assert(src != null);

            byte* pByte = src + index;

            // Align up the pointer to sizeof(int).
            while (((int)pByte & 3) != 0)
            {
                if (count == 0)
                    return -1;
                else if (*pByte == value)
                    return (int) (pByte - src);

                count--;
                pByte++;
            }

            // Fill comparer with value byte for comparisons
            //
            // comparer = 0/0/value/value
            uint comparer = (((uint)value << 8) + (uint)value);
            // comparer = value/value/value/value
            comparer = (comparer << 16) + comparer;

            // Run through buffer until we hit a 4-byte section which contains
            // the byte we're looking for or until we exhaust the buffer.
            while (count > 3)
            {
                // Test the buffer for presence of value. comparer contains the byte
                // replicated 4 times.
                uint t1 = *(uint*)pByte;
                t1 = t1 ^ comparer;
                uint t2 = 0x7efefeff + t1;
                t1 = t1 ^ 0xffffffff;
                t1 = t1 ^ t2;
                t1 = t1 & 0x81010100;

                // if t1 is zero then these 4-bytes don't contain a match
                if (t1 != 0)
                {
                    // We've found a match for value, figure out which position it's in.
                    int foundIndex = (int) (pByte - src);
                    if (pByte[0] == value)
                        return foundIndex;
                    else if (pByte[1] == value)
                        return foundIndex + 1;
                    else if (pByte[2] == value)
                        return foundIndex + 2;
                    else if (pByte[3] == value)
                        return foundIndex + 3;
                }

                count -= 4;
                pByte += 4;

            }

            // Catch any bytes that might be left at the tail of the buffer
            while (count > 0)
            {
                if (*pByte == value)
                    return (int) (pByte - src);

                count--;
                pByte++;
            }

            // If we don't have a match return -1;
            return -1;
        }
        
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
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), nameof(array));

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
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), nameof(array));

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
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), nameof(array));

            return _ByteLength(array);
        }

        internal unsafe static void ZeroMemory(byte* src, long len)
        {
            while(len-- > 0)
                *(src + len) = 0;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void Memcpy(byte[] dest, int destIndex, byte* src, int srcIndex, int len) {
            Debug.Assert( (srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(dest.Length - destIndex >= len, "not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len==0)
                return;
            fixed(byte* pDest = dest) {
                Memcpy(pDest + destIndex, src + srcIndex, len);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void Memcpy(byte* pDest, int destIndex, byte[] src, int srcIndex, int len)
        {
            Debug.Assert( (srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");        
            Debug.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len==0)
                return;
            fixed(byte* pSrc = src) {
                Memcpy(pDest + destIndex, pSrc + srcIndex, len);
            }
        }

        // This is tricky to get right AND fast, so lets make it useful for the whole Fx.
        // E.g. System.Runtime.WindowsRuntime!WindowsRuntimeBufferExtensions.MemCopy uses it.

        // This method has a slightly different behavior on arm and other platforms.
        // On arm this method behaves like memcpy and does not handle overlapping buffers.
        // While on other platforms it behaves like memmove and handles overlapping buffers.
        // This behavioral difference is unfortunate but intentional because
        // 1. This method is given access to other internal dlls and this close to release we do not want to change it.
        // 2. It is difficult to get this right for arm and again due to release dates we would like to visit it later.
        [FriendAccessAllowed]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#if ARM
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe static extern void Memcpy(byte* dest, byte* src, int len);
#else // ARM
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal unsafe static void Memcpy(byte* dest, byte* src, int len) {
            Debug.Assert(len >= 0, "Negative length in memcopy!");
            MemoryCopyCore(dest, src, (uint)len);
        }
#endif // ARM

        // This method has different signature for x64 and other platforms for performance reasons.
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void MemoryCopyCore(byte* destination, byte* source, nuint length)
        {
            const nuint PInvokeThreshold = 512;
#if AMD64
            const nuint CopyAlignment = 16; // SIMD is enabled for AMD64, so align on a 16-byte boundary
            const nuint BytesPerIteration = 64;
#else
            const nuint CopyAlignment = 4; // Align on a 4-byte boundary
            const nuint BytesPerIteration = 16;
#endif

            // P/Invoke into the native version when the buffers are overlapping and the copy needs to be performed backwards.
            // This check can produce false positives for very large lengths if the destination is behind the source.
            // It is fine because we would take the P/Invoke path later for such large lengths anyway.

            if ((nuint)destination - (nuint)source < length)
            {
                goto PInvoke;
            }
            
            // Currently, the following code seems to be faster than `Unsafe.CopyBlock` in benchmarks. If that is no longer
            // the case after changes to the JIT, the below code can simply be replaced with a call to that method.

            // This switch will be fast since it is compiled into a jump table in assembly.
            // See http://stackoverflow.com/a/449297/4077294 for more info.
            switch (length)
            {
            case 0:
                return;
            case 1:
                *destination = *source;
                return;
            case 2:
                *(short*)destination = *(short*)source;
                return;
            case 3:
                *(short*)destination = *(short*)source;
                *(destination + 2) = *(source + 2);
                return;
            case 4:
                *(int*)destination = *(int*)source;
                return;
            case 5:
                *(int*)destination = *(int*)source;
                *(destination + 4) = *(source + 4);
                return;
            case 6:
                *(int*)destination = *(int*)source;
                *(short*)(destination + 4) = *(short*)(source + 4);
                return;
            case 7:
                *(int*)destination = *(int*)source;
                *(short*)(destination + 4) = *(short*)(source + 4);
                *(destination + 6) = *(source + 6);
                return;
            case 8:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                return;
            case 9:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(destination + 8) = *(source + 8);
                return;
            case 10:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(short*)(destination + 8) = *(short*)(source + 8);
                return;
            case 11:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(short*)(destination + 8) = *(short*)(source + 8);
                *(destination + 10) = *(source + 10);
                return;
            case 12:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(int*)(destination + 8) = *(int*)(source + 8);
                return;
            case 13:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(int*)(destination + 8) = *(int*)(source + 8);
                *(destination + 12) = *(source + 12);
                return;
            case 14:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(int*)(destination + 8) = *(int*)(source + 8);
                *(short*)(destination + 12) = *(short*)(source + 12);
                return;
            case 15:
#if BIT64
                *(long*)destination = *(long*)source;
#else
                *(int*)destination = *(int*)source;
                *(int*)(destination + 4) = *(int*)(source + 4);
#endif
                *(int*)(destination + 8) = *(int*)(source + 8);
                *(short*)(destination + 12) = *(short*)(source + 12);
                *(destination + 14) = *(source + 14);
                return;
            }

            // P/Invoke into the native version for large lengths
            if (length > PInvokeThreshold)
            {
                goto PInvoke;
            }
            
            // We've already handled lengths 0-15, so we can write at least 16 bytes.
            // This calculates the offset of the next aligned address we know it's okay to write up to.
            Debug.Assert(length >= 16);
            nuint offset = 16 - ((nuint)destination % CopyAlignment);

            Debug.Assert(offset > 0 && offset <= 16);
            Debug.Assert((nuint)(destination + offset) % CopyAlignment == 0);

#if AMD64
            // SIMD is enabled for AMD64. Take advantage of that and use movdqu
            *(Block16*)destination = *(Block16*)source;
#else
            // Make one unaligned 4-byte write, then 3 aligned 4-byte writes.
            *(int*)destination = source;
            *(int*)(destination + offset - 12) = *(int*)(source + offset - 12);
            *(int*)(destination + offset - 8) = *(int*)(source + offset - 8);
            *(int*)(destination + offset - 4) = *(int*)(source + offset - 4);
#endif

            // Catch unsigned overflow before we do the subtraction.
            if (length < BytesPerIteration)
            {
                goto AfterUnrolledCopy;
            }

            nuint endOffset = length - BytesPerIteration;
            
            while (offset <= endOffset)
            {
#if AMD64
                // Write 64 bytes at a time, taking advantage of xmm register on AMD64
                // This will be translated to 4 movdqus (maybe movdqas in the future, see dotnet/coreclr#2725)
                *(Block64*)destination = *(Block64*)source;
#else
                // Write 16 bytes at a time, via 4 4-byte writes.
                *(int*)(destination + offset) = *(int*)(source + offset);
                *(int*)(destination + offset + 4) = *(int*)(source + offset + 4);
                *(int*)(destination + offset + 8) = *(int*)(source + offset + 8);
                *(int*)(destination + offset + 12) = *(int*)(source + offset + 12);
#endif
                offset += BytesPerIteration;
            }

            AfterUnrolledCopy:

            Debug.Assert((nuint)(destination + offset) % CopyAlignment == 0);

            nuint remainingLength = length - offset;
            Debug.Assert(remainingLength < BytesPerIteration);

            // Finish up the copy by dividing it into blocks of smaller powers of 2.
            // The bits of `remainingLength` tells us how it can be expressed as a sum of powers of 2.

#if AMD64
            if ((remainingLength & 32) != 0)
            {
                *(Block32*)(destination + offset) = *(Block32*)(source + offset);
                offset += 32;
            }

            if ((remainingLength & 16) != 0)
            {
                *(Block16*)(destination + offset) = *(Block16*)(source + offset);
                offset += 16;
            }

            // Make one potentially unaligned write and quit.
            *(Block16*)(destination + length - 16) = *(Block16*)(source + length - 16);
#else
            // Make 3 aligned 4-byte writes, then one unaligned 4-byte write.
            *(int*)(destination + offset) = *(int*)(source + offset);
            *(int*)(destination + offset + 4) = *(int*)(source + offset + 4);
            *(int*)(destination + offset + 8) = *(int*)(source + offset + 8);
            *(int*)(destination + length - 4) = *(int*)(source + length - 4);
#endif
            return;

            PInvoke:
            _Memmove(destination, source, length);

        }

        // Non-inlinable wrapper around the QCall that avoids poluting the fast path
        // with P/Invoke prolog/epilog.
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private unsafe static void _Memmove(byte* dest, byte* src, nuint len)
        {
            __Memmove(dest, src, len);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        extern private unsafe static void __Memmove(byte* dest, byte* src, nuint len);

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
            MemoryCopyCore((byte*)destination, (byte*)source, checked((nuint)sourceBytesToCopy));
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
            MemoryCopyCore((byte*)destination, (byte*)source, sourceBytesToCopy);
#else // BIT64
            MemoryCopyCore((byte*)destination, (byte*)source, checked((uint)sourceBytesToCopy));
#endif // BIT64
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Block16 { }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Block32 { }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Block64 { }
    }
}
