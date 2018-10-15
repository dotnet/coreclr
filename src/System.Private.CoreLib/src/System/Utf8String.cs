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

            return a.AsSpanFast().SequenceEqual(b.AsSpanFast());
        }

        public static bool operator !=(Utf8String a, Utf8String b) => !(a == b);

        public static implicit operator ReadOnlySpan<Utf8Char>(Utf8String value) => value.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<byte> AsMutableSpan() => MemoryMarshal.CreateSpan(ref GetRawStringData(), _length);

        // Similar to the AsSpan() extension method, but doesn't map a null 'this' to the empty ROS.
        // Instead, null 'this' is observed as a null reference exception.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan<byte> AsSpanFast() => MemoryMarshal.CreateReadOnlySpan(ref GetRawStringData(), Length);

        public static Utf8String Concat(Utf8String str0, Utf8String str1)
        {
            if (IsNullOrEmpty(str0))
            {
                return str1 ?? Empty;
            }

            if (IsNullOrEmpty(str1))
            {
                return str0;
            }

            // Integer addition below may overflow, resulting in allocating a new Utf8String instance
            // of an unexpected size. That's fine; we'll check for this in the copy routines below and
            // will throw if there's a problem.

            UnbakedUtf8String retVal = new UnbakedUtf8String(str0.Length + str1.Length);
            Span<byte> writeableSpan = retVal.GetSpan();

            str0.AsSpanFast().CopyTo(writeableSpan);
            str1.AsSpanFast().CopyTo(writeableSpan.Slice(str0.Length));

            // TODO: Merge characteristics from input Utf8String instances, copy them into new Utf8String instance.

            return retVal.BakeWithoutValidation();
        }

        public static Utf8String Concat(Utf8String str0, Utf8String str1, Utf8String str2)
        {
            if (IsNullOrEmpty(str0))
            {
                return Concat(str1, str2);
            }

            // TODO: The concatenations below can be optimized because we know that
            // some strings (like str0) have actual data.

            if (IsNullOrEmpty(str1))
            {
                return Concat(str0, str2);
            }

            if (IsNullOrEmpty(str2))
            {
                return Concat(str0, str1);
            }

            // Integer addition below may overflow, resulting in allocating a new Utf8String instance
            // of an unexpected size. That's fine; we'll check for this in the copy routines below and
            // will throw if there's a problem.

            UnbakedUtf8String retVal = new UnbakedUtf8String(str0.Length + str1.Length + str2.Length);
            Span<byte> writeableSpan = retVal.GetSpan();

            str0.AsSpanFast().CopyTo(writeableSpan);
            writeableSpan = writeableSpan.Slice(str0.Length);
            str1.AsSpanFast().CopyTo(writeableSpan);
            writeableSpan = writeableSpan.Slice(str1.Length);
            str2.AsSpanFast().CopyTo(writeableSpan);

            // TODO: Merge characteristics from input Utf8String instances, copy them into new Utf8String instance.

            return retVal.BakeWithoutValidation();
        }

        public static Utf8String Concat(Utf8String str0, Utf8String str1, Utf8String str2, Utf8String str3)
        {
            if (IsNullOrEmpty(str0))
            {
                return Concat(str1, str2, str3);
            }

            // TODO: The concatenations below can be optimized because we know that
            // some strings (like str0) have actual data.

            if (IsNullOrEmpty(str1))
            {
                return Concat(str0, str2, str3);
            }

            if (IsNullOrEmpty(str2))
            {
                return Concat(str0, str1, str3);
            }

            if (IsNullOrEmpty(str3))
            {
                return Concat(str0, str1, str2);
            }

            // Integer addition below may overflow, resulting in allocating a new Utf8String instance
            // of an unexpected size. That's fine; we'll check for this in the copy routines below and
            // will throw if there's a problem.

            UnbakedUtf8String retVal = new UnbakedUtf8String(str0.Length + str1.Length + str2.Length + str3.Length);
            Span<byte> writeableSpan = retVal.GetSpan();

            str0.AsSpanFast().CopyTo(writeableSpan);
            writeableSpan = writeableSpan.Slice(str0.Length);
            str1.AsSpanFast().CopyTo(writeableSpan);
            writeableSpan = writeableSpan.Slice(str1.Length);
            str2.AsSpanFast().CopyTo(writeableSpan);
            writeableSpan = writeableSpan.Slice(str2.Length);
            str3.AsSpanFast().CopyTo(writeableSpan);

            // TODO: Merge characteristics from input Utf8String instances, copy them into new Utf8String instance.

            return retVal.BakeWithoutValidation();
        }

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
        internal ref byte GetRawStringData() => ref Unsafe.AsRef(in _firstByte);

        public bool EndsWith(UnicodeScalar value)
        {
            if (Length == 0)
            {
                // Empty string doesn't end with anything
                return false;
            }

            // Common case is looking for an ASCII value (one UTF-8 code unit), so try that now
            // optimistically.

            if (Unsafe.Add(ref GetRawStringData(), Length) == value.Value)
            {
                return true; // match!
            }

            if (value.IsAscii || Length < 2)
            {
                return false; // no need to search future bytes
            }

            // Slow path: searching for a multi-byte sequence
            // TODO: This can be optimized if the input UnicodeScalar is a compile-time constant.

            Span<byte> valueAsUtf8 = stackalloc byte[4]; // longest sequence is 4 bytes
            int actualSequenceLength = value.ToUtf8(valueAsUtf8);
            return AsSpanFast().EndsWith(valueAsUtf8.Slice(0, actualSequenceLength));
        }

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
            // but we won't worry too much about this. The call to AsSpanFast() below will
            // throw in that case.

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

            return Equals(a.AsSpanFast(), b.AsBytes(), comparisonType);
        }

        internal static bool Equals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal)
            {
                return a.SequenceEqual(b);
            }
            else if (comparisonType == StringComparison.OrdinalIgnoreCase)
            {
                return EqualsOrdinalIgnoreCase(a, b);
            }
            else
            {
                // We rely on the fact that UTF-8 to UTF-16 transcoding never increases the overall
                // code unit count, even in the face of invalid input.

                char[] borrowedArrA = null;
                Span<char> scratchUtf16BufferA = a.Length <= 255 ?
                    stackalloc char[255] :
                    (borrowedArrA = ArrayPool<char>.Shared.Rent(a.Length));

                char[] borrowedArrB = null;
                Span<char> scratchUtf16BufferB = b.Length <= 255 ?
                    stackalloc char[255] :
                    (borrowedArrB = ArrayPool<char>.Shared.Rent(b.Length));

                int utf16CodeUnitCountA = ConvertToUtf16PreservingCorruption(a, scratchUtf16BufferA);
                int utf16CodeUnitCountB = ConvertToUtf16PreservingCorruption(b, scratchUtf16BufferB);

                // Now that we're in the UTF-16 world, we can call into the localization routines.

                bool retVal = MemoryExtensions.Equals(
                    scratchUtf16BufferA.Slice(0, utf16CodeUnitCountA),
                    scratchUtf16BufferB.Slice(0, utf16CodeUnitCountB),
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

        // TODO: Replace exception message below
        public UnicodeScalar GetScalarAt(int index) => (TryGetScalarAt(index, out UnicodeScalar scalar)) ? scalar : throw new Exception("Invalid data.");

        public int IndexOf(char value)
        {
            return IndexOf_Char_NoBoundsChecks(value, 0, Length);
        }

        public int IndexOf(char value, int startIndex)
        {
            if ((uint)startIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            return IndexOf_Char_NoBoundsChecks(value, startIndex, Length - startIndex);
        }

        public int IndexOf(char value, int startIndex, int count)
        {
            if ((uint)startIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            if ((uint)count > (uint)(Length - startIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            return IndexOf_Char_NoBoundsChecks(value, startIndex, count);
        }

        public int IndexOf(UnicodeScalar value)
        {
            return IndexOf_Scalar_NoBoundsChecks(value, 0, Length);
        }

        public int IndexOf(UnicodeScalar value, int startIndex)
        {
            if ((uint)startIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            return IndexOf_Scalar_NoBoundsChecks(value, startIndex, Length - startIndex);
        }

        public int IndexOf(UnicodeScalar value, int startIndex, int count)
        {
            if ((uint)startIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            if ((uint)count > (uint)(Length - startIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            return IndexOf_Scalar_NoBoundsChecks(value, startIndex, count);
        }

        private int IndexOf_Ascii_NoBoundsChecks(byte value, int startIndex, int count)
        {
            return SpanHelpers.IndexOf(ref Unsafe.Add(ref GetRawStringData(), startIndex), value, count);
        }

        private int IndexOf_Char_NoBoundsChecks(char value, int startIndex, int count)
        {
            if (value <= 0x7FU)
            {
                // ASCII
                return IndexOf_Ascii_NoBoundsChecks((byte)value, startIndex, count);
            }
            else if (!IsKnownAscii() && UnicodeScalar.TryCreate(value, out var scalar))
            {
                // Search character is not ASCII (but is still a valid scalar),
                // and the search space may contain non-ASCII data.
                return IndexOf_Scalar_NoBoundsChecks(scalar, startIndex, count);
            }

            // If we reached this point, we're being asked to look for a non-ASCII
            // character in a string we know to be all-ASCII, or we're being asked
            // to look for a character that can't be represented in UTF-8 (such as
            // a UTF-16 surrogate code unit). In both cases, return "not found".

            return -1;
        }

        private int IndexOf_Scalar_NoBoundsChecks(UnicodeScalar value, int startIndex, int count)
        {
            Span<byte> valueAsUtf8 = stackalloc byte[4]; // worst possible output case
            int utf8CodeUnitCount = value.ToUtf8(valueAsUtf8);
            return SpanHelpers.IndexOf(ref Unsafe.Add(ref GetRawStringData(), startIndex), count, ref MemoryMarshal.GetReference(valueAsUtf8), utf8CodeUnitCount);
        }

        public static bool IsEmptyOrWhiteSpace(ReadOnlySpan<byte> value)
        {
            return value.IsEmpty || IsWhiteSpaceCore(value);
        }

        /// <summary>
        /// Returns true iff this instance is known to contain all-ASCII data. Returns
        /// false if the string contains non-ASCII data or if the instance was never
        /// scanned to confirm that all the data is ASCII.
        /// </summary>
        internal bool IsKnownAscii() => GetCharacteristics().HasFlag(Characteristics.IsAscii);

        public static bool IsNullOrEmpty(Utf8String value)
        {
            // See comments in String.IsNullOrEmpty for why the code is written this way.
            return (value == null || 0u >= (uint)value.Length) ? true : false;
        }

        public static bool IsNullOrWhiteSpace(Utf8String value)
        {
            return IsNullOrEmpty(value) || IsWhiteSpaceCore(value.AsSpanFast());
        }

        private static bool IsWhiteSpaceCore(ReadOnlySpan<byte> value)
        {
            return Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(value) == value.Length;
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

        public bool StartsWith(UnicodeScalar value)
        {
            if (Length == 0)
            {
                // Empty string doesn't start with anything
                return false;
            }

            // Common case is looking for an ASCII value (one UTF-8 code unit), so try that now
            // optimistically.

            if (_firstByte == value.Value)
            {
                return true; // match!
            }

            if (value.IsAscii || Length < 2)
            {
                return false; // no need to search future bytes
            }

            // Slow path: searching for a multi-byte sequence
            // TODO: This can be optimized if the input UnicodeScalar is a compile-time constant.

            Span<byte> valueAsUtf8 = stackalloc byte[4]; // longest sequence is 4 bytes
            int actualSequenceLength = value.ToUtf8(valueAsUtf8);
            return AsSpanFast().StartsWith(valueAsUtf8.Slice(0, actualSequenceLength));
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
            if ((uint)startIndex < (uint)Length)
            {
                // Common case: arguments are in bounds and caller isn't trying to substring away the entire contents

                if (startIndex != 0)
                {
                    // TODO: Validate that the bounds check below is elided
                    var slice = AsSpanFast().Slice(startIndex);
                    var unbaked = new UnbakedUtf8String(slice);

                    // Any flags (well-formed, ASCII, etc.) will transfer to the new substring as long as the new substring wasn't
                    // split in the middle of a multi-byte sequence. We can check this cheaply by seeing if the first byte of the
                    // substring is a continuation byte; if so then we know we've performed an invalid split. In that case we'll
                    // return the new substring as-is without applying any characteristics.

                    if (!UnicodeHelpers.IsUtf8ContinuationByte(in MemoryMarshal.GetReference(slice)))
                    {
                        unbaked.CopyCharacteristicsFrom(this);
                    }

                    return unbaked.BakeWithoutValidation();
                }
                else
                {
                    return this;
                }
            }
            else if (startIndex == Length)
            {
                // Less common case: caller is trying to substring away the entire contents
                return Empty;
            }
            else
            {
                // Rarest case: arguments are out of bounds
                Debug.Assert(startIndex < 0 || startIndex > Length);

                // Determine the actual failure cause so that we can throw the proper exception.

                if (startIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_StartIndex);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_StartIndexLargerThanLength);
                }
            }
        }

        public Utf8String Substring(int startIndex, int length)
        {
            if ((uint)startIndex > (uint)Length || (uint)length > (uint)(Length - startIndex))
            {
                goto Error;
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
                unbaked.CopyCharacteristicsFrom(this);
            }

            return unbaked.BakeWithoutValidation();

        Error:

            // Determine the actual failure cause so that we can throw the proper exception.

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_StartIndex);
            }

            if (startIndex > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_StartIndexLargerThanLength);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NegativeLength);
            }

            Debug.Assert(startIndex > Length - length);
            throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);
        }

        public override string ToString()
        {
            if (Length != 0)
            {
                return ToString(AsSpanFast());
            }
            else
            {
                return string.Empty;
            }
        }

        internal static string ToString(ReadOnlySpan<byte> span)
        {
            // UTF8 -> UTF16 transcoding will never shrink the total number of code units,
            // so we should never end up in a situation where the destination buffer is too
            // small.

            if ((uint)span.Length <= 64)
            {
                Span<char> chars = stackalloc char[span.Length];
                int charCount = Encoding.UTF8.GetChars(span, chars);
                Debug.Assert(charCount > 0);

                return new string(chars.Slice(0, charCount));
            }
            else
            {
                ArrayPool<char> pool = ArrayPool<char>.Shared;
                var chars = pool.Rent(span.Length);
                int charcount = Encoding.UTF8.GetChars(span, chars);
                Debug.Assert(charcount > 0);

                var retVal = new string(chars, 0, charcount);
                pool.Return(chars);
                return retVal;
            }
        }

        /// <summary>
        /// Returns a new <see cref="Utf8String"/> that represents the current instance with all leading
        /// and trailing whitespace characters removed.
        /// </summary>
        public Utf8String Trim() => TrimHelper(TrimType.Both);

        /// <summary>
        /// Returns a new <see cref="Utf8String"/> that represents the current instance with all trailing
        /// whitespace characters removed.
        /// </summary>
        public Utf8String TrimEnd() => TrimHelper(TrimType.Tail);

        private Utf8String TrimHelper(TrimType trimType)
        {
            ReadOnlySpan<byte> span = AsSpanFast();

            if (trimType.HasFlag(TrimType.Head))
            {
                span = span.DangerousSliceWithoutBoundsCheck(Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(span));
            }

            if (trimType.HasFlag(TrimType.Tail))
            {
                span = new ReadOnlySpan<byte>(ref MemoryMarshal.GetReference(span), Utf8Utility.GetIndexOfTrailingWhiteSpaceSequence(span));
            }

            if (span.Length > 0)
            {
                if (span.Length < Length)
                {
                    // Create a substring
                    UnbakedUtf8String unbakedString = new UnbakedUtf8String(span);
                    unbakedString.CopyCharacteristicsFrom(this); // any validity properties from this string propagate to the new one
                    return unbakedString.BakeWithoutValidation();
                }
                else
                {
                    // There's no whitespace to trim - return this
                    return this;
                }
            }
            else
            {
                // String is only whitespace - return Empty singleton
                return Empty;
            }
        }

        /// <summary>
        /// Returns a new <see cref="Utf8String"/> that represents the current instance with all leading
        /// whitespace characters removed.
        /// </summary>
        public Utf8String TrimStart() => TrimHelper(TrimType.Head);

        public bool TryGetScalarAt(int index, out UnicodeScalar scalar)
        {
            if ((uint)index >= Length)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(index), SR.ArgumentOutOfRange_Index);
            }

            var result = UnicodeReader.PeekFirstScalarUtf8(AsSpanFast().DangerousSliceWithoutBoundsCheck(index));
            scalar = result.scalar; // will be U+FFFD on failure
            return (result.status == SequenceValidity.Valid);
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

        internal ChunkToUtf16Enumerator ChunkToUtf16(Span<char> chunkBuffer)
        {
            return new ChunkToUtf16Enumerator(this.AsSpanFast(), chunkBuffer);
        }

        internal ref struct ChunkToUtf16Enumerator
        {
            ReadOnlySpan<byte> _source;
            Span<char> _chunkBuffer;
            int _numCharsConvertedInChunkBuffer;

            public ChunkToUtf16Enumerator(ReadOnlySpan<byte> source, Span<char> chunkBuffer)
            {
                _source = source;
                _chunkBuffer = chunkBuffer;
                _numCharsConvertedInChunkBuffer = 0;
            }

            public int Current => _numCharsConvertedInChunkBuffer;

            public ChunkToUtf16Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                if (!_source.IsEmpty)
                {
                    Unicode.TranscodeUtf8ToUtf16(_source, _chunkBuffer, isFinalChunk: true, fixupInvalidSequences: true, out int bytesConsumed, out int _numCharsConvertedInChunkBuffer);
                    Debug.Assert(bytesConsumed != 0, "Should've consumed a non-zero amount of data.");
                    _source = _source.Slice(bytesConsumed);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
