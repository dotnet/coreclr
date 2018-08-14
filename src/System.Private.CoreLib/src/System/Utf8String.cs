// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

            return a.AsSpanFast().SequenceEqual(b.AsSpanFast());
        }

        public static bool operator !=(Utf8String a, Utf8String b) => !(a == b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<byte> AsMutableSpan() => MemoryMarshal.CreateSpan(ref DangerousGetMutableReference(), _length);
        
        // Similar to the AsSpan() extension method, but doesn't map a null 'this' to the empty ROS.
        // Instead, null 'this' is observed as a null reference exception.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan<byte> AsSpanFast() => MemoryMarshal.CreateReadOnlySpan(ref DangerousGetMutableReference(), Length);

        internal string ConvertToUtf16PreservingCorruption()
        {
            // We rely on the fact that UTF-8 to UTF-16 transcoding never increases the overall
            // code unit count, even in the face of invalid input.

            char[] borrowedArr = null;
            Span<char> span = Length <= 255 ?
                stackalloc char[255] :
                (borrowedArr = ArrayPool<char>.Shared.Rent(Length));
            
            int utf16CodeUnitCount = ConvertToUtf16PreservingCorruption(AsSpanFast(), span);
            string retVal = new string(span.Slice(0, utf16CodeUnitCount));

            // Return the borrowed arrays if necessary.
            if (borrowedArr != null)
            {
                ArrayPool<char>.Shared.Return(borrowedArr);
            }

            return retVal;
        }

        // Converts UTF-8 to UTF-16, preserving corrupted (ill-formed) sequences.
        // That is, an ill-formed UTF-8 input will result in an ill-formed UTF-16 output.
        // Returns the number of chars written.
        private static int ConvertToUtf16PreservingCorruption(ReadOnlySpan<byte> utf8, Span<char> utf16)
        {
            // TODO: Optimize me through vectorization and other tricks.

            int originalUtf16Length = utf16.Length;

            while (!utf8.IsEmpty)
            {
                var result = UnicodeReader.PeekFirstScalarUtf8(utf8);
                if (result.status == SequenceValidity.Valid)
                {
                    // Valid UTF-8 encoded scalar.
                    // Convert to UTF-16, copy to output, bump both buffers, and loop.

                    int utf16CharsWritten = result.scalar.ToUtf16(utf16);
                    utf16 = utf16.Slice(utf16CharsWritten);
                    utf8 = utf8.Slice(result.charsConsumed);
                }
                else
                {
                    // Not a valid UTF-8 encoded scalar.
                    // Convert to [corrupted] UTF-16, copy to output, bump both buffers, and loop.
                    //
                    // To corrupt the output, we widen each invalid UTF-8 byte to U+DFxx, where xx
                    // is the original input byte. This means that we're writing out a bunch of low
                    // surrogates, which is invalid UTF-16, but where the particular code units of
                    // the invalid UTF-16 are dependent on the particular code units of the input.
                    // We're never going to have preceded this with a standalone high surrogate (since
                    // UnicodeScalar.ToUtf16 will never emit unpaired surrogates), so we don't need
                    // to worry about inadvertently making a valid surrogate pair.

                    Debug.Assert(result.charsConsumed > 0);

                    for (int i = 0; i < result.charsConsumed; i++)
                    {
                        utf16[i] = (char)(0xDF00U | utf8[i]);
                    }

                    utf16 = utf16.Slice(result.charsConsumed);
                    utf8 = utf8.Slice(result.charsConsumed);
                }
            }

            // Returns the number of UTF-16 characters written
            return originalUtf16Length - originalUtf16Length;
        }

        /// <summary>
        /// Returns a mutable ref byte pointing to the internal null-terminated UTF-8 data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref byte DangerousGetMutableReference() => ref Unsafe.AsRef(in _firstByte);

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

            return this.AsSpanFast().SequenceEqual(value.AsSpanFast());
        }

        // ordinal equality
        public static bool Equals(Utf8String a, Utf8String b)
        {
            // Fast check - same instance?
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Null compared to non-null is always false
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            // Ordinal equality implies lengths must be equal
            if (a.Length != b.Length)
            {
                return false;
            }

            return a.AsSpanFast().SequenceEqual(b.AsSpanFast());
        }

        public static bool Equals(Utf8String a, Utf8String b, StringComparison comparisonType)
        {
            // This is based on the logic in String.Equals(String, String, StringComparison)

            string.CheckStringComparison(comparisonType);

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            if (comparisonType == StringComparison.Ordinal)
            {
                return a.AsSpanFast().SequenceEqual(b.AsSpanFast());
            }
            else if (comparisonType == StringComparison.OrdinalIgnoreCase)
            {
                return EqualsOrdinalIgnoreCase(a.AsSpanFast(), b.AsSpanFast());
            }
            else
            {
                // We rely on the fact that UTF-8 to UTF-16 transcoding never increases the overall
                // code unit count, even in the face of invalid input.

                char[] borrowedArrA = null;
                Span<char> spanA = a.Length <= 255 ?
                    stackalloc char[255] :
                    (borrowedArrA = ArrayPool<char>.Shared.Rent(a.Length));

                char[] borrowedArrB = null;
                Span<char> spanB = b.Length <= 255 ?
                    stackalloc char[255] :
                    (borrowedArrB = ArrayPool<char>.Shared.Rent(b.Length));

                int utf16CodeUnitCountA = ConvertToUtf16PreservingCorruption(a.AsSpanFast(), spanA);
                int utf16CodeUnitCountB = ConvertToUtf16PreservingCorruption(b.AsSpanFast(), spanB);

                // Now that we're in the UTF-16 world, we can call into the localization routines.

                bool retVal = MemoryExtensions.Equals(
                    spanA.Slice(0, utf16CodeUnitCountA),
                    spanB.Slice(0, utf16CodeUnitCountB),
                    comparisonType);

                // Return the borrowed arrays if necessary.
                if (borrowedArrA != null)
                {
                    ArrayPool<char>.Shared.Return(borrowedArrA);
                }
                if (borrowedArrB != null)
                {
                    ArrayPool<char>.Shared.Return(borrowedArrB);
                }

                return retVal;
            }
        }

        internal static bool EqualsOrdinalIgnoreCase(Utf8String a, Utf8String b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return EqualsOrdinalIgnoreCase(a.AsSpanFast(), b.AsSpanFast());
        }

        private static bool EqualsOrdinalIgnoreCase(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            // TODO: This has room for optimization, including special-casing ASCII input (which
            // also allows us to check code unit count), attempting ordinal comparison before
            // falling back to transcoding, and other tricks.

            // For UTF-8, changing case can also change the code unit count, so we can't compare the
            // lengths as a quick out unless we know that at least one of the inputs is ASCII or
            // we're opertating in invariant mode (no localization tables available).

            if (GlobalizationMode.Invariant && (a.Length != b.Length))
            {
                return false;
            }

            // We rely on the fact that UTF-8 to UTF-16 transcoding never increases the overall
            // code unit count, even in the face of invalid input.

            char[] borrowedArrA = null;
            Span<char> spanA = a.Length <= 255 ?
                stackalloc char[255] :
                (borrowedArrA = ArrayPool<char>.Shared.Rent(a.Length));

            char[] borrowedArrB = null;
            Span<char> spanB = b.Length <= 255 ?
                stackalloc char[255] :
                (borrowedArrB = ArrayPool<char>.Shared.Rent(b.Length));

            int utf16CodeUnitCountA = ConvertToUtf16PreservingCorruption(a, spanA);
            int utf16CodeUnitCountB = ConvertToUtf16PreservingCorruption(b, spanB);

            // Now that we're in the UTF-16 world, we can call into the localization routines.
            //
            // OrdinalIgnoreCase equality for UTF-16 strings only holds when
            // the strings have the same code unit count.

            bool retVal = (utf16CodeUnitCountA == utf16CodeUnitCountB)
                && CompareInfo.EqualsOrdinalIgnoreCase(
                    ref MemoryMarshal.GetReference(spanA),
                    ref MemoryMarshal.GetReference(spanB),
                    utf16CodeUnitCountA);

            // Return the borrowed arrays if necessary.
            if (borrowedArrA != null)
            {
                ArrayPool<char>.Shared.Return(borrowedArrA);
            }
            if (borrowedArrB != null)
            {
                ArrayPool<char>.Shared.Return(borrowedArrB);
            }

            return retVal;
        }

        private static bool EqualsOrdinalIgnoreCaseAscii(ref byte a, ref byte b, nuint length)
        {
            // TODO: Vectorize the below code.
            // (Not vectorizing for now because assuming inputs to equality check are short strings.)

            // n.b. May actually contain non-ASCII data; e.g., if we're operating with invariant globalization.
            // This isn't an error; we just treat this as opaque data.

            for (nuint i = 0; i < length; i++)
            {
                uint valA = Unsafe.Add(ref a, (IntPtr)i);
                uint valB = Unsafe.Add(ref b, (IntPtr)i);

                if (valA != valB)
                {
                    valA |= 0x20; // convert to lowercase
                    valB |= 0x20;

                    if (valA != valB)
                    {
                        return false; // valA and valB aren't equal, even after converting to lowercase
                    }

                    if (!UnicodeHelpers.IsInRangeInclusive(valA, 'a', 'z' - 'a'))
                    {
                        return false; // valA isn't alpha, so the lowercase conversion was invalid
                    }
                }
            }

            return true; // all checks passed
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Characteristics GetCharacteristics() => default;

        public override int GetHashCode() => GetHashCode(AsSpanFast());

        public int GetHashCode(StringComparison comparisonType) => GetHashCode(AsSpanFast(), comparisonType);

        // A span-based equivalent of Utf8String.GetHashCode(). Computes an ordinal hash code.
        public static int GetHashCode(ReadOnlySpan<byte> value)
        {
            // TODO: Should this use a different seed?
            return Marvin.ComputeHash32(value, Marvin.DefaultSeed);
        }

        // A span-based equivalent of Utf8String.GetHashCode(StringComparison). Uses the specified comparison type.
        public static int GetHashCode(ReadOnlySpan<byte> value, StringComparison comparisonType)
        {
            // Get the simple one out of the way first.

            if (comparisonType == StringComparison.Ordinal)
            {
                return GetHashCode(value);
            }

            // The bounds of comparisonType will be checked by string.GetHashCode.
            // 
            // Any other StringComparison requires a transcoding from UTF-8 to UTF-16.
            // We'll do that now, preserving any corruption in the input so that two distinct
            // invalid inputs result in different hash codes rather than both being normalized
            // to contain U+FFFD scalars.
            // 
            // We rely on the fact that UTF-8 to UTF-16 transcoding never increases the overall
            // code unit count, even in the face of invalid input.
            //
            // TODO: Make this better, perhaps by special-casing whether the input string contains
            // all-ASCII data.

            char[] borrowedArr = null;
            Span<char> span = value.Length <= 255 ?
                stackalloc char[255] :
                (borrowedArr = ArrayPool<char>.Shared.Rent(value.Length));

            int utf16CodeUnitCount = ConvertToUtf16PreservingCorruption(value, span);
            int hash = string.GetHashCode(span.Slice(0, utf16CodeUnitCount), comparisonType);

            // Return the borrowed array if necessary.
            if (borrowedArr != null)
            {
                ArrayPool<char>.Shared.Return(borrowedArr);
            }

            // TODO: The following xor is just to ensure we don't inadvertently take a dependency
            // on Utf8String.GetHashCode and String.GetHashCode returning the same value. We should
            // remove it when we update the hash code calculation routines above to avoid the transcoding
            // step or to use a different Marvin seed.

            return hash ^ 0x1234;
        }

        public ref readonly byte GetPinnableReference() => ref _firstByte;

        internal bool IsKnownAscii() => GetCharacteristics().HasFlag(Characteristics.IsAscii);

        public static bool IsNullOrEmpty(Utf8String value)
        {
            // See comments in String.IsNullOrEmpty for why the code is written this way.
            return (value == null || 0u >= (uint)value.Length) ? true : false;
        }

        /// <summary>
        /// Temporary method to help create UTF-8 string literals from UTF-16 string literals
        /// until full language support comes online.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Utf8String Literal(string value)
        {
            return RuntimeHelpers.GetUtf8StringLiteral(value);
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
            var slice = AsSpanFast().Slice(startIndex);
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
            var slice = AsSpanFast().Slice(startIndex, length);
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
                int charCount = Encoding.UTF8.GetChars(AsSpanFast(), chars);
                Debug.Assert(charCount > 0);

                return new string(chars.Slice(0, charCount));
            }
            else
            {
                ArrayPool<char> pool = ArrayPool<char>.Shared;
                var chars = pool.Rent(Length);
                int charcount = Encoding.UTF8.GetChars(AsSpanFast(), chars);
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
