// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System {
    
    // Only contains static methods. Does not require serialization
    
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
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
        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void BlockCopy(Array src, int srcOffset,
            Array dst, int dstOffset, int count);

        // A very simple and efficient memmove that assumes all of the
        // parameter validation has already been done.  The count and offset
        // parameters here are in bytes.  If you want to use traditional
        // array element indices and counts, use Array.Copy.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void InternalBlockCopy(Array src, int srcOffsetBytes,
            Array dst, int dstOffsetBytes, int byteCount);

        // This is ported from the optimized CRT assembly in memchr.asm. The JIT generates 
        // pretty good code here and this ends up being within a couple % of the CRT asm.
        // It is however cross platform as the CRT hasn't ported their fast version to 64-bit
        // platforms.
        //
        [System.Security.SecurityCritical]  // auto-generated
        internal unsafe static int IndexOfByte(byte* src, byte value, int index, int count)
        {
            Contract.Assert(src != null, "src should not be null");

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
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool IsPrimitiveTypeArray(Array array);

        // Gets a particular byte out of the array.  The array must be an
        // array of primitives.  
        //
        // This essentially does the following: 
        // return ((byte*)array) + index.
        //
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern byte _GetByte(Array array, int index);

        [System.Security.SecuritySafeCritical]  // auto-generated
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
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void _SetByte(Array array, int index, byte value);

        [System.Security.SecuritySafeCritical]  // auto-generated
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
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int _ByteLength(Array array);

        [System.Security.SecuritySafeCritical]  // auto-generated
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

        [System.Security.SecurityCritical]  // auto-generated
        internal unsafe static void ZeroMemory(byte* src, long len)
        {
            while(len-- > 0)
                *(src + len) = 0;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void Memcpy(byte[] dest, int destIndex, byte* src, int srcIndex, int len) {
            Contract.Assert( (srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Contract.Assert(dest.Length - destIndex >= len, "not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if (len==0)
                return;
            fixed(byte* pDest = dest) {
                Memcpy(pDest + destIndex, src + srcIndex, len);
            }
        }

        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void Memcpy(byte* pDest, int destIndex, byte[] src, int srcIndex, int len)
        {
            Contract.Assert( (srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");        
            Contract.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
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
        [System.Security.SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#if ARM
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe static extern void Memcpy(byte* dest, byte* src, int len);
#else // ARM
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal unsafe static void Memcpy(byte* dest, byte* src, int len) {
            Contract.Assert(len >= 0, "Negative length in memcopy!");
            Memmove(dest, src, (uint)len);
        }
#endif // ARM

        // This method has different signature for x64 and other platforms and is done for performance reasons.
        [System.Security.SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe static void Memmove(byte* dest, byte* src, nuint len)
        {
            // P/Invoke into the native version when the buffers are overlapping and the copy needs to be performed backwards
            // This check can produce false positives for lengths greater than int.MaxValue. It is fine because we want to use the P/Invoke path for the large lengths anyway.

            if ((nuint)dest - (nuint)src < len) goto PInvoke;

            // This is portable version of memcpy. It mirrors what the hand optimized assembly versions of memcpy typically do.
            //
            // Ideally, we would just use the cpblk IL instruction here. Unfortunately, cpblk IL instruction is not as efficient as
            // possible yet and so we have this implementation here for now.

            // Note: It's important that this switch handles lengths at least up to 15 for AMD64.
            // We assume below len is at least 16 and make one 128-bit write without checking.

            // The switch will be very fast since it can be implemented using a jump
            // table in assembly. See http://stackoverflow.com/a/449297/4077294 for more info.

            switch (len)
            {
            case 0:
                return;
            case 1:
                *dest = *src;
                return;
            case 2:
                *(short*)dest = *(short*)src;
                return;
            case 3:
                *(short*)dest = *(short*)src;
                *(dest + 2) = *(src + 2);
                return;
            case 4:
                *(int*)dest = *(int*)src;
                return;
            case 5:
                *(int*)dest = *(int*)src;
                *(dest + 4) = *(src + 4);
                return;
            case 6:
                *(int*)dest = *(int*)src;
                *(short*)(dest + 4) = *(short*)(src + 4);
                return;
            case 7:
                *(int*)dest = *(int*)src;
                *(short*)(dest + 4) = *(short*)(src + 4);
                *(dest + 6) = *(src + 6);
                return;
            case 8:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                return;
            case 9:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(dest + 8) = *(src + 8);
                return;
            case 10:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(short*)(dest + 8) = *(short*)(src + 8);
                return;
            case 11:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(short*)(dest + 8) = *(short*)(src + 8);
                *(dest + 10) = *(src + 10);
                return;
            case 12:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(int*)(dest + 8) = *(int*)(src + 8);
                return;
            case 13:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(dest + 12) = *(src + 12);
                return;
            case 14:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(short*)(dest + 12) = *(short*)(src + 12);
                return;
            case 15:
#if BIT64
                *(long*)dest = *(long*)src;
#else // BIT64
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(short*)(dest + 12) = *(short*)(src + 12);
                *(dest + 14) = *(src + 14);
                return;
            }

            // P/Invoke into the native version for large lengths.
            // Currently the threshold at which the native version is faster seems to be around 1024
            // on amd64 Windows, but this is subject to change if this implementation can be made faster,
            // or new benchmarks are posted, or the case is different for other platforms.
#if AMD64
            const nuint NativeThreshold = 1024;
#else // AMD64
            const nuint NativeThreshold = 512;
#endif // AMD64
            if (len > NativeThreshold) goto PInvoke;

            // So far SIMD is only enabled for AMD64, so on that plaform we want
            // to 16-byte align while on others (including arm64) we'll want to word-align
#if AMD64
            nuint alignment = 16u;
#else // AMD64
            nuint alignment = (nuint)sizeof(nuint);
#endif // AMD64

            // (nuint)dest % alignment calculates how far we are from the previous aligned address
            // Note that it's *very* important alignment is unsigned.
            // (int)dest % (int)alignment for example will give different results if the lhs is negative.
            
            // If dest is aligned this will be 0.
            nuint i = (nuint)dest % alignment;

            // We know from the above switch-case that len is at least 16, so here
            // we subtract i from 16. This represents the furthest aligned address
            // we know it's okay to write upto.
            // To make it clearer, (dest + i) after this is equivalent to
            // [previous aligned address] + 16.
            i = 16u - i;

#if AMD64
            // SIMD is enabled for AMD64, so take advantage of that and use movdqu
            *(Buffer16*)dest = *(Buffer16*)src;
#elif ARM64
            // ARM64 has 64-bit words but no SIMD yet, so make 2 word writes
            // First one isn't aligned, second one is (remember from earlier notes dest + i is 8-aligned)
            *(long*)dest = *(long*)src;
            *(long*)(dest + i - 8) = *(long*)(src + i - 8);
#else // AMD64, ARM64
            // i386 and ARM: 32-bit words, no SIMD (yet)
            // make 1 unaligned word write, then 3 4-byte aligned ones
            *(int*)dest = *(int*)src;
            *(int*)(dest + i - 12) = *(int*)(src + i - 12);
            *(int*)(dest + i - 8) = *(int*)(src + i - 8);
            *(int*)(dest + i - 4) = *(int*)(src + i - 4);
#endif // AMD64, ARM64

            // i now represents the number of bytes we've copied so far.
            Contract.Assert(i <= len && i > 0 && i <= 16);
            Contract.Assert((nuint)(dest + i) % alignment == 0);

            // chunk: bytes processed per iteration in unrolled loop
#if AMD64
            nuint chunk = 64;
#elif ARM64
            nuint chunk = 32;
#else // AMD64, ARM64
            nuint chunk = 24; // 6 4-byte words per iteration
#endif // AMD64, ARM64

            // Protect ourselves from unsigned overflow
            if (len < chunk)
                goto LoopCleanup;

            // end: point after which we stop the unrolled loop
            // This is the end of the buffer, minus the space
            // required for 1 iteration of the loop.
            nuint end = len - chunk;

            // This can return false in the first iteration if the process of
            // aligning the pointer for writes has not left enough space
            // for this loop to run, so unfortunately this can't be a do-while loop.
            while (i <= end)
            {
                // Some versions of this loop looks very costly since there appear
                // to be a bunch of temporary values being created with the adds,
                // but the jit (for x86 anyways) will convert each of these to
                // use memory addressing operands.

                // So the only cost is a bit of code size, which is made up for by the fact that
                // we save on writes to dest/src.

#if AMD64
                // Write 64 bytes at a time, taking advantage of xmm register on AMD64
                // This will be translated to 4 movdqus (maybe movdqas in the future, dotnet/coreclr#2725)
                *(Buffer64*)(dest + i) = *(Buffer64*)(src + i);
#elif ARM64
                // ARM64: Unroll by 32 bytes, this time using longs since we don't
                // take advantage of SIMD for that plaform yet.
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
                *(long*)(dest + i + 16) = *(long*)(src + i + 16);
                *(long*)(dest + i + 24) = *(long*)(src + i + 24);
#else // AMD64, ARM64
                // i386/ARM32:
                // Write 24 bytes at a time, via 6 32-bit word writes
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
                *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                *(int*)(dest + i + 12) = *(int*)(src + i + 12);
                *(int*)(dest + i + 16) = *(int*)(src + i + 16);
                *(int*)(dest + i + 20) = *(int*)(src + i + 20);
#endif // AMD64, ARM64

                i += chunk;
            }

            LoopCleanup:
            // If we've reached this point, there are at most chunk - 1 bytes left

            len -= i; // len now represents the number of bytes left

#if AMD64
            // if the len & 32 bit is set that means this number
            // will be >= 32. (same principle applies for other
            // powers of 2 below)
            if ((len & 32) != 0)
            {
                *(Buffer32*)(dest + i) = *(Buffer32*)(src + i);
                i += 32;
            }
#endif // AMD64

            // Now there can be at most 31 bytes left (23 for 32-bit)

            if ((len & 16) != 0)
            {
#if AMD64
                *(Buffer16*)(dest + i) = *(Buffer16*)(src + i);
#elif ARM64
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
#else // AMD64, ARM64
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
                *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                *(int*)(dest + i + 12) = *(int*)(src + i + 12);
#endif // AMD64, ARM64

                i += 16;
            }

            // Now there can be at most 15 bytes left
            // For AMD64 we just want to make 1 (potentially) unaligned xmm write and quit.
            // For other platforms we have another switch-case for 0..15.
            // Again, this is implemented with a jump table so it's very fast.

#if AMD64
            i = len - 16;
            *(Buffer16*)(dest + i) = *(Buffer16*)(src + i);
#else // AMD64

            switch (len & 15)
            {
                case 0:
                    // No-op: We already finished copying all the bytes.
                    return;
                case 1:
                    *(dest + i) = *(src + i);
                    return;
                case 2:
                    *(short*)(dest + i) = *(short*)(src + i);
                    return;
                case 3:
                    *(short*)(dest + i) = *(short*)(src + i);
                    *(dest + i + 2) = *(src + i + 2);
                    return;
                case 4:
                    *(int*)(dest + i) = *(int*)(src + i);
                    return;
                case 5:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(dest + i + 4) = *(src + i + 4);
                    return;
                case 6:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(short*)(dest + i + 4) = *(short*)(src + i + 4);
                    return;
                case 7:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(short*)(dest + i + 4) = *(short*)(src + i + 4);
                    *(dest + i + 6) = *(src + i + 6);
                    return;
                case 8:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    return;
                case 9:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(dest + i + 8) = *(src + i + 8);
                    return;
                case 10:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(short*)(dest + i + 8) = *(short*)(src + i + 8);
                    return;
                case 11:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(short*)(dest + i + 8) = *(short*)(src + i + 8);
                    *(dest + i + 10) = *(src + i + 10);
                    return;
                case 12:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    return;
                case 13:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(dest + i + 12) = *(src + i + 12);
                    return;
                case 14:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(short*)(dest + i + 12) = *(short*)(src + i + 12);
                    return;
                case 15:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(short*)(dest + i + 12) = *(short*)(src + i + 12);
                    *(dest + i + 14) = *(src + i + 14);
                    return;
            }

#endif // AMD64

            return;

            PInvoke:
            _Memmove(dest, src, len);
        }

        // Non-inlinable wrapper around the QCall that avoids polluting the fast path
        // with P/Invoke prolog/epilog.
        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private unsafe static void _Memmove(byte* dest, byte* src, nuint len)
        {
            __Memmove(dest, src, len);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        extern private unsafe static void __Memmove(byte* dest, byte* src, nuint len);

        // The attributes on this method are chosen for best JIT performance. 
        // Please do not edit unless intentional.
        [System.Security.SecurityCritical]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
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
        [System.Security.SecurityCritical]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static unsafe void MemoryCopy(void* source, void* destination, ulong destinationSizeInBytes, ulong sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
            }
#if BIT64
            Memmove((byte*)destination, (byte*)source, sourceBytesToCopy);
#else // BIT64
            Memmove((byte*)destination, (byte*)source, checked((uint)sourceBytesToCopy));
#endif // BIT64
        }

        // These structs are used for fast copying of large buffers in
        // MemoryCopy. If you use these, you would do best not to make
        // unnecessary copies of them. e.g. *dest = *src where both dest
        // and src have type Buffer64* will generate 8 movdqu instructions
        // on AMD64, and potentially even more code on platforms where
        // RyuJIT does not take advantage of SIMD.

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Buffer64
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Buffer32
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Buffer16
        {
        }
    }
}
