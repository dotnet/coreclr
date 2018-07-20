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
    public sealed partial class Utf8String : IEquatable<Utf8String>
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
         * STANDARD PROPERTIES AND METHODS
         */

        public int Length => _length;

        public static bool operator ==(Utf8String a, Utf8String b)
        {
            // See main comments in Utf8String.Equals(Utf8String) method.
            // Primary difference with this method is that we need to allow for the
            // case where 'a' is null without incurring a null ref.

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            return a.AsSpan().SequenceEqual(b.AsSpan());
        }

        public static bool operator !=(Utf8String a, Utf8String b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<byte> AsMutableSpan() => MemoryMarshal.CreateSpan(ref DangerousGetMutableReference(), _length);

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

        public override bool Equals(object obj) => (obj is Utf8String other) && this.Equals(other);

        public bool Equals(Utf8String value)
        {
            // Fast check - same instance?
            if (ReferenceEquals(this, value))
            {
                return true;
            }

            // Being compared against null?
            if (ReferenceEquals(value, null))
            {
                return false;
            }

            // It's possible 'this' could be null if somebody was futzing about with the IL,
            // but we won't worry too much about this. The Length property getter below will
            // throw a null ref in that case.

            if (this.Length != value.Length)
            {
                return false;
            }

            // Same length, now check byte-for-byte equality.

            // TODO: There's potential for optimization here, such as including the _length field
            // or the null terminator in the "to-be-compared" span if it would better allow the
            // equality comparison routine to consume more bytes at a time rather than drain off
            // single bytes. We can make this optimization if perf runs show it's useful.

            return this.AsSpan().SequenceEqual(value.AsSpan());
        }
        
        public override int GetHashCode()
        {
            return Marvin.ComputeHash32(AsSpan(), Marvin.DefaultSeed);
        }

        public ref readonly byte GetPinnableReference() => ref _firstByte;

        public static bool IsNullOrEmpty(Utf8String value)
        {
            // See comments in String.IsNullOrEmpty for why the code is written this way.
            return (value == null || 0u >= (uint)value.Length) ? true : false;
        }

        private static unsafe nuint strlen(byte* value)
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

        /// <summary>
        /// Characteristics of a <see cref="Utf8String"/> instance that can be determined by examining
        /// the two storage bits of the object header.
        /// </summary>
        [Flags]
        private enum Characteristics
        {
            /// <summary>
            /// No characteristics have been determined.
            /// </summary>
            None = 0,

            /// <summary>
            /// This instance contains only ASCII data.
            /// </summary>
            IsAscii,

            /// <summary>
            /// This instance has been validated and is known to contain only well-formed UTF-8 sequences.
            /// </summary>
            IsWellFormed
        }
    }
}
