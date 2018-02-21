// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System
{
    /// <summary>
    /// Extension methods and non-generic helpers for Span, ReadOnlySpan, Memory, and ReadOnlyMemory.
    /// </summary>
    public static class Span
    {

        public static bool Contains(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            return (IndexOf(span, value, comparisonType, out _) >= 0);
        }

        public static bool Equals(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            StringSpanHelpers.CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(span, value, CompareOptions.None) == 0);

                case StringComparison.CurrentCultureIgnoreCase:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(span, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.InvariantCulture:
                    return (CompareInfo.Invariant.Compare(span, value, CompareOptions.None) == 0);

                case StringComparison.InvariantCultureIgnoreCase:
                    return (CompareInfo.Invariant.Compare(span, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.Ordinal:
                    if (span.Length != value.Length)
                        return false;
                    if (value.Length == 0)
                        return true;
                    return OrdinalHelper(span, value, value.Length);

                case StringComparison.OrdinalIgnoreCase:
                    if (span.Length != value.Length)
                        return false;
                    if (value.Length == 0)
                        return true;
                    return (CompareInfo.CompareOrdinalIgnoreCase(span, value) == 0);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        public static int CompareTo(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            StringSpanHelpers.CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(span, value, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(span, value, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CompareInfo.Invariant.Compare(span, value, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.Compare(span, value, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    if (span.Length == 0 || value.Length == 0)
                        return span.Length - value.Length;
                    return CompareOrdinalHelper(span, value);

                case StringComparison.OrdinalIgnoreCase:
                    return CompareInfo.CompareOrdinalIgnoreCase(span, value);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        public static unsafe int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType, out int matchedLength)
        {
            if (value.Length == 0)
            {
                StringSpanHelpers.CheckStringComparison(comparisonType);
                matchedLength = 0;
                return 0;
            }

            if (span.Length == 0)
            {
                StringSpanHelpers.CheckStringComparison(comparisonType);
                matchedLength = 0;
                return -1;
            }

            int defaultMatchLength = value.Length;

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    int index = IndexOfCultureHelper(span, value, &defaultMatchLength);
                    matchedLength = defaultMatchLength;
                    return index;

                case StringComparison.CurrentCultureIgnoreCase:
                    index = IndexOfCultureIgnoreCaseHelper(span, value, &defaultMatchLength);
                    matchedLength = defaultMatchLength;
                    return index;

                case StringComparison.InvariantCulture:
                    index = IndexOfCultureHelper(span, value, &defaultMatchLength, invariantCulture: true);
                    matchedLength = defaultMatchLength;
                    return index;

                case StringComparison.InvariantCultureIgnoreCase:
                    index = IndexOfCultureIgnoreCaseHelper(span, value, &defaultMatchLength, invariantCulture: true);
                    matchedLength = defaultMatchLength;
                    return index;

                case StringComparison.Ordinal:
                    matchedLength = defaultMatchLength;
                    return IndexOfOrdinalHelper(span, value, false);

                case StringComparison.OrdinalIgnoreCase:
                    matchedLength = defaultMatchLength;
                    return IndexOfOrdinalHelper(span, value, true);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        internal static unsafe int IndexOfCultureHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, int* matchLengthPtr, bool invariantCulture = false)
        {
            Debug.Assert(span.Length != 0);
            Debug.Assert(value.Length != 0);
            Debug.Assert(matchLengthPtr != null);

            if (GlobalizationMode.Invariant)
            {
                *matchLengthPtr = value.Length;
                return CompareInfo.InvariantIndexOf(span, value, false);
            }

            return invariantCulture ?
                CompareInfo.Invariant.IndexOf(span, value, CompareOptions.None, matchLengthPtr) :
                CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, CompareOptions.None, matchLengthPtr);
        }

        internal static unsafe int IndexOfCultureIgnoreCaseHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, int* matchLengthPtr, bool invariantCulture = false)
        {
            Debug.Assert(span.Length != 0);
            Debug.Assert(value.Length != 0);
            Debug.Assert(matchLengthPtr != null);

            if (GlobalizationMode.Invariant)
            {
                *matchLengthPtr = value.Length;
                return CompareInfo.InvariantIndexOf(span, value, true);
            }

            return invariantCulture ?
                CompareInfo.Invariant.IndexOf(span, value, CompareOptions.IgnoreCase, matchLengthPtr) :
                CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, CompareOptions.IgnoreCase, matchLengthPtr);
        }

        internal static int IndexOfOrdinalHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, bool ignoreCase)
        {
            Debug.Assert(span.Length != 0);
            Debug.Assert(value.Length != 0);

            if (GlobalizationMode.Invariant)
            {
                return CompareInfo.InvariantIndexOf(span, value, ignoreCase);
            }

            return CompareInfo.Invariant.IndexOfOrdinal(span, value, ignoreCase);
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase.
        /// </summary>
        public static int ToLower(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
        {
            if (culture == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.culture);

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

            if (GlobalizationMode.Invariant)
                culture.TextInfo.ToLowerAsciiInvariant(source, destination);
            else
                culture.TextInfo.ChangeCase(source, destination, toUpper: false);
            return source.Length;
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase
        /// using the casing rules of the invariant culture.
        /// </summary>
        public static int ToLowerInvariant(this ReadOnlySpan<char> source, Span<char> destination)
        {
            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

            if (GlobalizationMode.Invariant)
                CultureInfo.InvariantCulture.TextInfo.ToLowerAsciiInvariant(source, destination);
            else
                CultureInfo.InvariantCulture.TextInfo.ChangeCase(source, destination, toUpper: false);
            return source.Length;
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase.
        /// </summary>
        public static int ToUpper(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
        {
            if (culture == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.culture);

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

            if (GlobalizationMode.Invariant)
                culture.TextInfo.ToUpperAsciiInvariant(source, destination);
            else
                culture.TextInfo.ChangeCase(source, destination, toUpper: true);
            return source.Length;
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase
        /// using the casing rules of the invariant culture.
        /// </summary>
        public static int ToUpperInvariant(this ReadOnlySpan<char> source, Span<char> destination)
        {
            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

            if (GlobalizationMode.Invariant)
                CultureInfo.InvariantCulture.TextInfo.ToUpperAsciiInvariant(source, destination);
            else
                CultureInfo.InvariantCulture.TextInfo.ChangeCase(source, destination, toUpper: true);
            return source.Length;
        }

        /// <summary>
        /// Determines whether the beginning of the span matches the specified value when compared using the specified comparison option.
        /// </summary>
        public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            if (value.Length == 0)
            {
                StringSpanHelpers.CheckStringComparison(comparisonType);
                return true;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return StartsWithCultureHelper(span, value);

                case StringComparison.CurrentCultureIgnoreCase:
                    return StartsWithCultureIgnoreCaseHelper(span, value);

                case StringComparison.InvariantCulture:
                    return StartsWithCultureHelper(span, value, invariantCulture: true);

                case StringComparison.InvariantCultureIgnoreCase:
                    return StartsWithCultureIgnoreCaseHelper(span, value, invariantCulture: true);

                case StringComparison.Ordinal:
                    return StartsWithOrdinalHelper(span, value);

                case StringComparison.OrdinalIgnoreCase:
                    return StartsWithOrdinalIgnoreCaseHelper(span, value);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        internal static bool StartsWithCultureHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, bool invariantCulture = false)
        {
            Debug.Assert(value.Length != 0);

            if (GlobalizationMode.Invariant)
            {
                return StartsWithOrdinalHelper(span, value);
            }
            if (span.Length == 0)
            {
                return false;
            }
            return invariantCulture ?
                CompareInfo.Invariant.IsPrefix(span, value, CompareOptions.None) :
                CultureInfo.CurrentCulture.CompareInfo.IsPrefix(span, value, CompareOptions.None);
        }

        internal static bool StartsWithCultureIgnoreCaseHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, bool invariantCulture = false)
        {
            Debug.Assert(value.Length != 0);

            if (GlobalizationMode.Invariant)
            {
                return StartsWithOrdinalIgnoreCaseHelper(span, value);
            }
            if (span.Length == 0)
            {
                return false;
            }
            return invariantCulture ?
                CompareInfo.Invariant.IsPrefix(span, value, CompareOptions.IgnoreCase) :
                CultureInfo.CurrentCulture.CompareInfo.IsPrefix(span, value, CompareOptions.IgnoreCase);
        }

        internal static bool StartsWithOrdinalIgnoreCaseHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            Debug.Assert(value.Length != 0);

            if (span.Length < value.Length)
            {
                return false;
            }
            return CompareInfo.CompareOrdinalIgnoreCase(span.Slice(0, value.Length), value) == 0;
        }

        internal static unsafe bool StartsWithOrdinalHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            Debug.Assert(value.Length != 0);

            if (span.Length < value.Length)
            {
                return false;
            }
            return OrdinalHelper(span, value, value.Length);
        }

        internal static unsafe bool OrdinalHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, int length)
        {
            Debug.Assert(length != 0);
            Debug.Assert(span.Length >= length);

            fixed (char* ap = &MemoryMarshal.GetReference(span))
            fixed (char* bp = &MemoryMarshal.GetReference(value))
            {
                char* a = ap;
                char* b = bp;

#if BIT64
                // Single int read aligns pointers for the following long reads
                if (length >= 2)
                {
                    if (*(int*)a != *(int*)b)
                        return false;
                    length -= 2;
                    a += 2;
                    b += 2;
                }

                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b)
                        return false;
                    if (*(long*)(a + 4) != *(long*)(b + 4))
                        return false;
                    if (*(long*)(a + 8) != *(long*)(b + 8))
                        return false;
                    length -= 12;
                    a += 12;
                    b += 12;
                }
#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) return false;
                    if (*(int*)(a+2) != *(int*)(b+2)) return false;
                    if (*(int*)(a+4) != *(int*)(b+4)) return false;
                    if (*(int*)(a+6) != *(int*)(b+6)) return false;
                    if (*(int*)(a+8) != *(int*)(b+8)) return false;
                    length -= 10; a += 10; b += 10;
                }
#endif

                while (length >= 2)
                {
                    if (*(int*)a != *(int*)b)
                        return false;
                    length -= 2;
                    a += 2;
                    b += 2;
                }

                // PERF: This depends on the fact that the String objects are always zero terminated 
                // and that the terminating zero is not included in the length. For even string sizes
                // this compare can include the zero terminator. Bitwise OR avoids a branch.
                return length == 0 | *a == *b;
            }
        }

        private static unsafe int CompareOrdinalHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            Debug.Assert(value.Length != 0);
            Debug.Assert(span.Length != 0);
            int length = Math.Min(span.Length, value.Length);

            fixed (char* ap = &MemoryMarshal.GetReference(span))
            fixed (char* bp = &MemoryMarshal.GetReference(value))
            {
                char* a = ap;
                char* b = bp;

                if (*a != *b)
                    goto DiffOffsetStart;

                if (length >= 2)
                {
                    if (*(a + 1) != *(b + 1))
                        goto DiffOffset1;

                    // Since we know that the first two chars are the same,
                    // we can increment by 2 here and skip 4 bytes.
                    // This leaves us 8-byte aligned, which results
                    // on better perf for 64-bit platforms.
                    length -= 2;
                    a += 2;
                    b += 2;
                }

                // unroll the loop
#if BIT64
                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b)
                        goto DiffOffset0;
                    if (*(long*)(a + 4) != *(long*)(b + 4))
                        goto DiffOffset4;
                    if (*(long*)(a + 8) != *(long*)(b + 8))
                        goto DiffOffset8;
                    length -= 12;
                    a += 12;
                    b += 12;
                }
#else // BIT64
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto DiffOffset0;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) goto DiffOffset2;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) goto DiffOffset4;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) goto DiffOffset6;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) goto DiffOffset8;
                    length -= 10; a += 10; b += 10; 
                }
#endif // BIT64

                // Fallback loop:
                // go back to slower code path and do comparison on 4 bytes at a time.
                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0)
                {
                    if (*(int*)a != *(int*)b)
                        goto DiffNextInt;
                    length -= 2;
                    a += 2;
                    b += 2;
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return span.Length - value.Length;

#if BIT64
DiffOffset8:
                a += 4;
                b += 4;
DiffOffset4:
                a += 4;
                b += 4;
#else // BIT64
                // Use jumps instead of falling through, since
                // otherwise going to DiffOffset8 will involve
                // 8 add instructions before getting to DiffNextInt
                DiffOffset8: a += 8; b += 8; goto DiffOffset0;
                DiffOffset6: a += 6; b += 6; goto DiffOffset0;
                DiffOffset4: a += 2; b += 2;
                DiffOffset2: a += 2; b += 2;
#endif // BIT64

DiffOffset0:

// If we reached here, we already see a difference in the unrolled loop above
#if BIT64
                if (*(int*)a == *(int*)b)
                {
                    a += 2;
                    b += 2;
                }
#endif // BIT64

DiffNextInt:
                if (*a != *b)
                    return *a - *b;

                if (length == 1)
                {
                    if (span.Length == value.Length)
                        return *a - *b;
                    return span.Length - value.Length;
                }
DiffOffset1:
                Debug.Assert(*(a + 1) != *(b + 1), "This char must be different if we reach here!");
                return *(a + 1) - *(b + 1);

DiffOffsetStart:
                Debug.Assert(*a != *b, "This char must be different if we reach here!");
                return *a - *b;
            }
        }

        /// <summary>
        /// Determines whether the end of the span matches the specified value when compared using the specified comparison option.
        /// </summary>
        public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            if (value.Length == 0)
            {
                StringSpanHelpers.CheckStringComparison(comparisonType);
                return true;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return EndsWithCultureHelper(span, value);

                case StringComparison.CurrentCultureIgnoreCase:
                    return EndsWithCultureIgnoreCaseHelper(span, value);

                case StringComparison.InvariantCulture:
                    return EndsWithCultureHelper(span, value, invariantCulture: true);

                case StringComparison.InvariantCultureIgnoreCase:
                    return EndsWithCultureIgnoreCaseHelper(span, value, invariantCulture: true);

                case StringComparison.Ordinal:
                    return EndsWithOrdinalHelper(span, value);

                case StringComparison.OrdinalIgnoreCase:
                    return EndsWithOrdinalIgnoreCaseHelper(span, value);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        internal static bool EndsWithCultureHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, bool invariantCulture = false)
        {
            Debug.Assert(value.Length != 0);

            if (GlobalizationMode.Invariant)
            {
                return EndsWithOrdinalHelper(span, value);
            }
            if (span.Length == 0)
            {
                return false;
            }
            return invariantCulture ?
                CompareInfo.Invariant.IsSuffix(span, value, CompareOptions.None) :
                CultureInfo.CurrentCulture.CompareInfo.IsSuffix(span, value, CompareOptions.None);
        }

        internal static bool EndsWithCultureIgnoreCaseHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value, bool invariantCulture = false)
        {
            Debug.Assert(value.Length != 0);

            if (GlobalizationMode.Invariant)
            {
                return EndsWithOrdinalIgnoreCaseHelper(span, value);
            }
            if (span.Length == 0)
            {
                return false;
            }
            return invariantCulture ?
                CompareInfo.Invariant.IsSuffix(span, value, CompareOptions.IgnoreCase) :
                CultureInfo.CurrentCulture.CompareInfo.IsSuffix(span, value, CompareOptions.IgnoreCase);
        }

        internal static bool EndsWithOrdinalIgnoreCaseHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            Debug.Assert(value.Length != 0);

            if (span.Length < value.Length)
            {
                return false;
            }
            return (CompareInfo.CompareOrdinalIgnoreCase(span.Slice(span.Length - value.Length), value) == 0);
        }

        internal static unsafe bool EndsWithOrdinalHelper(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            Debug.Assert(value.Length != 0);

            if (span.Length < value.Length)
            {
                return false;
            }
            return OrdinalHelper(span.Slice(span.Length - value.Length), value, value.Length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{char}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        public static ReadOnlyMemory<char> AsReadOnlyMemory(this string text)
        {
            if (text == null)
                return default;

            return new ReadOnlyMemory<char>(text, 0, text.Length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{char}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;text.Length).
        /// </exception>
        public static ReadOnlyMemory<char> AsReadOnlyMemory(this string text, int start)
        {
            if (text == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

            if ((uint)start > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlyMemory<char>(text, start, text.Length - start);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{char}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index or <paramref name="length"/> is not in range.
        /// </exception>
        public static ReadOnlyMemory<char> AsReadOnlyMemory(this string text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlyMemory<char>(text, start, length);
        }

        /// <summary>Attempts to get the underlying <see cref="string"/> from a <see cref="ReadOnlyMemory{T}"/>.</summary>
        /// <param name="readOnlyMemory">The memory that may be wrapping a <see cref="string"/> object.</param>
        /// <param name="text">The string.</param>
        /// <param name="start">The starting location in <paramref name="text"/>.</param>
        /// <param name="length">The number of items in <paramref name="text"/>.</param>
        /// <returns></returns>
        public static bool TryGetString(this ReadOnlyMemory<char> readOnlyMemory, out string text, out int start, out int length)
        {
            if (readOnlyMemory.GetObjectStartLength(out int offset, out int count) is string s)
            {
                text = s;
                start = offset;
                length = count;
                return true;
            }
            else
            {
                text = null;
                start = 0;
                length = 0;
                return false;
            }
        }

        /// <summary>
        /// Casts a Span of one primitive type <typeparamref name="T"/> to Span of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="source">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsBytes<T>(this Span<T> source)
            where T : struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            return new Span<byte>(
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(source)),
                checked(source.Length * Unsafe.SizeOf<T>()));
        }

        /// <summary>
        /// Casts a ReadOnlySpan of one primitive type <typeparamref name="T"/> to ReadOnlySpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="source">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> source)
            where T : struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            return new ReadOnlySpan<byte>(
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(source)),
                checked(source.Length * Unsafe.SizeOf<T>()));
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// reference (Nothing in Visual Basic).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsReadOnlySpan(this string text)
        {
            if (text == null)
                return default;

            return new ReadOnlySpan<char>(ref text.GetRawStringData(), text.Length);
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;text.Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsReadOnlySpan(this string text, int start)
        {
            if (text == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

            if ((uint)start > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlySpan<char>(ref Unsafe.Add(ref text.GetRawStringData(), start), text.Length - start);
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index or <paramref name="length"/> is not in range.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsReadOnlySpan(this string text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlySpan<char>(ref Unsafe.Add(ref text.GetRawStringData(), start), length);
        }

        internal static unsafe void ClearWithoutReferences(ref byte b, nuint byteLength)
        {
            if (byteLength == 0)
                return;

#if CORECLR && (AMD64 || ARM64)
            if (byteLength > 4096)
                goto PInvoke;
            Unsafe.InitBlockUnaligned(ref b, 0, (uint)byteLength);
            return;
#else
            // TODO: Optimize other platforms to be on par with AMD64 CoreCLR
            // Note: It's important that this switch handles lengths at least up to 22.
            // See notes below near the main loop for why.

            // The switch will be very fast since it can be implemented using a jump
            // table in assembly. See http://stackoverflow.com/a/449297/4077294 for more info.

            switch (byteLength)
            {
                case 1:
                    b = 0;
                    return;
                case 2:
                    Unsafe.As<byte, short>(ref b) = 0;
                    return;
                case 3:
                    Unsafe.As<byte, short>(ref b) = 0;
                    Unsafe.Add<byte>(ref b, 2) = 0;
                    return;
                case 4:
                    Unsafe.As<byte, int>(ref b) = 0;
                    return;
                case 5:
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.Add<byte>(ref b, 4) = 0;
                    return;
                case 6:
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    return;
                case 7:
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.Add<byte>(ref b, 6) = 0;
                    return;
                case 8:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    return;
                case 9:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.Add<byte>(ref b, 8) = 0;
                    return;
                case 10:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    return;
                case 11:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.Add<byte>(ref b, 10) = 0;
                    return;
                case 12:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    return;
                case 13:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.Add<byte>(ref b, 12) = 0;
                    return;
                case 14:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
                    return;
                case 15:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
                    Unsafe.Add<byte>(ref b, 14) = 0;
                    return;
                case 16:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    return;
                case 17:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.Add<byte>(ref b, 16) = 0;
                    return;
                case 18:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 16)) = 0;
                    return;
                case 19:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 16)) = 0;
                    Unsafe.Add<byte>(ref b, 18) = 0;
                    return;
                case 20:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 16)) = 0;
                    return;
                case 21:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 16)) = 0;
                    Unsafe.Add<byte>(ref b, 20) = 0;
                    return;
                case 22:
#if BIT64
                    Unsafe.As<byte, long>(ref b) = 0;
                    Unsafe.As<byte, long>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
#else
                    Unsafe.As<byte, int>(ref b) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 4)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 8)) = 0;
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 12)) = 0;
#endif
                    Unsafe.As<byte, int>(ref Unsafe.Add<byte>(ref b, 16)) = 0;
                    Unsafe.As<byte, short>(ref Unsafe.Add<byte>(ref b, 20)) = 0;
                    return;
            }

            // P/Invoke into the native version for large lengths
            if (byteLength >= 512) goto PInvoke;

            nuint i = 0; // byte offset at which we're copying

            if ((Unsafe.As<byte, int>(ref b) & 3) != 0)
            {
                if ((Unsafe.As<byte, int>(ref b) & 1) != 0)
                {
                    Unsafe.AddByteOffset<byte>(ref b, i) = 0;
                    i += 1;
                    if ((Unsafe.As<byte, int>(ref b) & 2) != 0)
                        goto IntAligned;
                }
                Unsafe.As<byte, short>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                i += 2;
            }

            IntAligned:

            // On 64-bit IntPtr.Size == 8, so we want to advance to the next 8-aligned address. If
            // (int)b % 8 is 0, 5, 6, or 7, we will already have advanced by 0, 3, 2, or 1
            // bytes to the next aligned address (respectively), so do nothing. On the other hand,
            // if it is 1, 2, 3, or 4 we will want to copy-and-advance another 4 bytes until
            // we're aligned.
            // The thing 1, 2, 3, and 4 have in common that the others don't is that if you
            // subtract one from them, their 3rd lsb will not be set. Hence, the below check.

            if (((Unsafe.As<byte, int>(ref b) - 1) & 4) == 0)
            {
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                i += 4;
            }

            nuint end = byteLength - 16;
            byteLength -= i; // lower 4 bits of byteLength represent how many bytes are left *after* the unrolled loop

            // We know due to the above switch-case that this loop will always run 1 iteration; max
            // bytes we clear before checking is 23 (7 to align the pointers, 16 for 1 iteration) so
            // the switch handles lengths 0-22.
            Debug.Assert(end >= 7 && i <= end);

            // This is separated out into a different variable, so the i + 16 addition can be
            // performed at the start of the pipeline and the loop condition does not have
            // a dependency on the writes.
            nuint counter;

            do
            {
                counter = i + 16;

                // This loop looks very costly since there appear to be a bunch of temporary values
                // being created with the adds, but the jit (for x86 anyways) will convert each of
                // these to use memory addressing operands.

                // So the only cost is a bit of code size, which is made up for by the fact that
                // we save on writes to b.

#if BIT64
                Unsafe.As<byte, long>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                Unsafe.As<byte, long>(ref Unsafe.AddByteOffset<byte>(ref b, i + 8)) = 0;
#else
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i + 4)) = 0;
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i + 8)) = 0;
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i + 12)) = 0;
#endif

                i = counter;

                // See notes above for why this wasn't used instead
                // i += 16;
            }
            while (counter <= end);

            if ((byteLength & 8) != 0)
            {
#if BIT64
                Unsafe.As<byte, long>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
#else
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i + 4)) = 0;
#endif
                i += 8;
            }
            if ((byteLength & 4) != 0)
            {
                Unsafe.As<byte, int>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                i += 4;
            }
            if ((byteLength & 2) != 0)
            {
                Unsafe.As<byte, short>(ref Unsafe.AddByteOffset<byte>(ref b, i)) = 0;
                i += 2;
            }
            if ((byteLength & 1) != 0)
            {
                Unsafe.AddByteOffset<byte>(ref b, i) = 0;
                // We're not using i after this, so not needed
                // i += 1;
            }

            return;
#endif

        PInvoke:
            RuntimeImports.RhZeroMemory(ref b, byteLength);
        }

        internal static unsafe void ClearWithReferences(ref IntPtr ip, nuint pointerSizeLength)
        {
            if (pointerSizeLength == 0)
                return;

            // TODO: Perhaps do switch casing to improve small size perf

            nuint i = 0;
            nuint n = 0;
            while ((n = i + 8) <= (pointerSizeLength))
            {
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 0) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 1) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 2) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 3) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 4) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 5) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 6) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 7) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                i = n;
            }
            if ((n = i + 4) <= (pointerSizeLength))
            {
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 0) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 1) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 2) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 3) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                i = n;
            }
            if ((n = i + 2) <= (pointerSizeLength))
            {
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 0) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 1) * (nuint)sizeof(IntPtr)) = default(IntPtr);
                i = n;
            }
            if ((i + 1) <= (pointerSizeLength))
            {
                Unsafe.AddByteOffset<IntPtr>(ref ip, (i + 0) * (nuint)sizeof(IntPtr)) = default(IntPtr);
            }
        }
    }
}
