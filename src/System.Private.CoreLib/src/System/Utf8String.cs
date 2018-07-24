// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Internal.Runtime.CompilerServices;
using System.Threading;


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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Characteristics GetCharacteristics() => default;

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

        public Utf8String Substring(int startIndex)
        {
            if ((uint)startIndex >= (uint)Length)
            {
                if (startIndex == Length)
                {
                    // We allow the start index to point just past the end of the string; this creates an empty substring
                    return Empty;
                }
                else
                {
                    // TODO: Throw the correct exception type with the correct resource
                    throw new ArgumentOutOfRangeException(paramName: nameof(startIndex));
                }
            }

            if (startIndex == 0)
            {
                return this;
            }

            // TODO: Validate that the bounds check below is elided
            var slice = AsSpan().Slice(startIndex);
            var unbaked = new UnbakedUtf8String(slice);

            // Any flags (well-formed, ASCII, etc.) will transfer to the new substring as long as the new substring wasn't
            // split in the middle of a multi-byte sequence. We can check this cheaply by seeing if the first byte of the
            // substring is a continuation byte; if so then we know we've performed an invalid split. In that case we'll
            // return the new substring as-is without applying any characteristics.

            if (!UnicodeHelpers.IsUtf8ContinuationByte(in MemoryMarshal.GetReference(slice)))
            {
                unbaked.ApplyCharacteristics(this.GetCharacteristics());
            }

            return unbaked.BakeWithoutValidation();
        }

        public Utf8String Substring(int startIndex, int length)
        {
            if (((uint)startIndex > (uint)Length) || ((uint)length > (uint)(Length - startIndex)))
            {
                // TODO: Throw the correct exception type with the correct resource
                throw new ArgumentOutOfRangeException();
            }

            if (length == 0)
            {
                return Empty;
            }

            if (length == Length)
            {
                return this;
            }

            // TODO: Validate that the bounds check below is elided
            var slice = AsSpan().Slice(startIndex, length);
            var unbaked = new UnbakedUtf8String(slice);

            // See comments in Substring(int) for explanation of below logic. Difference here is that we check two bytes:
            // the first byte of the substring; and the byte just past the end of the substring (which could be the null
            // terminator, which is not a continuation byte).

            ref byte firstByte = ref MemoryMarshal.GetReference(slice);
            if (!UnicodeHelpers.IsUtf8ContinuationByte(in firstByte) && !UnicodeHelpers.IsUtf8ContinuationByte(in Unsafe.Add(ref firstByte, length)))
            {
                unbaked.ApplyCharacteristics(this.GetCharacteristics());
            }

            return unbaked.BakeWithoutValidation();
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
        
        // TODO! Decide if string interning should be a public feature
        internal static Utf8String Intern(Utf8String str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return Thread.GetDomain().GetOrInternUtf8String(str);
        }
    }
}
