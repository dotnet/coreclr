// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
    public unsafe sealed class Utf8String
    {
        /*
         * STATIC FIELDS
         */

        public static readonly Utf8String Empty = FastAllocate(0);

        /*
         * INSTANCE FIELDS
         * WARNING - Do not reorder these fields. Must match layout of Utf8StringObject in object.h.
         */

        private readonly int _length;
        private readonly byte _firstByte;

        /*
         * CONSTRUCTORS
         * These are special. The implementation methods for these have a different signature from the
         * declared constructors. Keep these ctors declared in the same order as listed in ecall.cpp.
         */

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<byte> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
            {
                return Empty;
            }

            Utf8String newString = FastAllocate(value.Length);
            Buffer.Memmove(
                destination: ref newString.DangerousGetMutableReference(),
                source: ref MemoryMarshal.GetNonNullPinnableReference(value),
                elementCount: (nuint)value.Length);

            // From jkotas: all stores of reference type instances into the GC heap
            // are treated as volatile writes, so no need for an explicit memory
            // barrier here.

            // TODO: validation of incoming data.

            return newString;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<char> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return Empty;
            }

            // TODO: Use a list of buffers and perform the transcoding piecemeal,
            // which reduces the constant factor on the O(n) operation.

            Encoding e = Encoding.UTF8;
            int length = e.GetByteCount(value);
            Utf8String newString = FastAllocate(length);

            unsafe
            {
                fixed (byte* pFirstByte = &newString._firstByte)
                fixed (char* pFirstChar = &MemoryMarshal.GetNonNullPinnableReference(value))
                {
                    e.GetBytes(pFirstChar, length, pFirstByte, length);
                }
            }

            // No validation needed, as transcoding always fixes up invalid characters.

            return newString;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(byte[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(byte[] value, int startIndex, int length)
        {
            // TODO: Real parameter validation with friendlier exception messages
            return Ctor(value.AsSpan(startIndex, length));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public extern Utf8String(byte* value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(byte* value)
        {
            if (value == null)
            {
                return Empty;
            }

            return Ctor(new ReadOnlySpan<byte>(value, checked((int)strlen(value))));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(char[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(char[] value, int startIndex, int length)
        {
            // TODO: Real parameter validation with friendlier exception messages
            return Ctor(value.AsSpan(startIndex, length));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public extern Utf8String(char* value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(char* value)
        {
            if (value == null)
            {
                return Empty;
            }

            return Ctor(new ReadOnlySpan<char>(value, string.wcslen(value)));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(string value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(string value)
        {
            // TODO: Null check parameter
            // TODO: Check for interning

            return Ctor(value.AsSpan());
        }

        /*
         * STANDARD PROPERTIES AND METHODS
         */

        public int Length => _length;

        /// <summary>
        /// Converts this instance to a <see cref="ReadOnlySpan{byte}"/>.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref DangerousGetMutableReference(), Length);

        /// <summary>
        /// Returns a mutable ref byte pointing to the internal null-terminated UTF-8 data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte DangerousGetMutableReference() => ref Unsafe.AsRef(in _firstByte);

        // Creates a new zero-initialized instance of the specified length. Actual storage allocated is "length + 1" bytes (the extra
        // +1 is for the NUL terminator.)
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Utf8String FastAllocate(int length);

        public ref readonly byte GetPinnableReference() => ref _firstByte;

        public static bool IsNullOrEmpty(Utf8String value)
        {
            // See comments in String.IsNullOrEmpty for why the code is written this way.
            return (value == null || 0u >= (uint)value.Length) ? true : false;
        }

        private static nuint strlen(byte* value)
        {
            Debug.Assert(value != null);

            // TODO: Optimize this method.

            nuint idx = 0;
            while (value[idx] != 0)
            {
                idx++;
            }
            return idx;
        }

        public override string ToString()
        {
            if (Length == 0)
            {
                return string.Empty;
            }

            // UTF8 -> UTF16 transcoding will never shrink the total number of code units,
            // so we should never end up in a situation where the destination buffer is too
            // small.

            if ((uint)Length <= 64)
            {
                Span<char> chars = stackalloc char[Length];
                int charCount = Encoding.UTF8.GetChars(AsSpan(), chars);
                Debug.Assert(charCount > 0);

                return new string(chars.Slice(0, charCount));
            }
            else
            {
                ArrayPool<char> pool = ArrayPool<char>.Shared;
                var chars = pool.Rent(Length);
                int charcount = Encoding.UTF8.GetChars(AsSpan(), chars);
                Debug.Assert(charcount > 0);

                var retVal = new string(chars, 0, charcount);
                pool.Return(chars);
                return retVal;
            }
        }
    }
}
