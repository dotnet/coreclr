// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System.Text
{
    /// <summary>
    /// Contains extension methods for working with UTF-8 textual data.
    /// </summary>
    public static class Utf8Extensions
    {
        public static bool Equals(this ReadOnlySpan<Utf8Char> span, ReadOnlySpan<Utf8Char> other, StringComparison comparisonType)
        {
            return Utf8String.Equals(span.AsBytes(), other.AsBytes(), comparisonType);
        }

        public static bool IsWhiteSpace(this ReadOnlySpan<Utf8Char> span)
        {
            return Utf8String.IsEmptyOrWhiteSpace(span.AsBytes());
        }

        public static int ToLowerInvariant(this ReadOnlySpan<Utf8Char> source, Span<Utf8Char> destination)
        {
            return Utf8.ToLowerInvariant(source.AsBytes(), MemoryMarshal.Cast<Utf8Char, byte>(destination));
        }

        public static int ToUpperInvariant(this ReadOnlySpan<Utf8Char> source, Span<Utf8Char> destination)
        {
            return Utf8.ToUpperInvariant(source.AsBytes(), MemoryMarshal.Cast<Utf8Char, byte>(destination));
        }

        public static Utf8String ToUtf8String(this ReadOnlySpan<Utf8Char> value) => new Utf8String(value.AsBytes());

        public static ReadOnlySpan<Utf8Char> Trim(this ReadOnlySpan<Utf8Char> span) => TrimEnd(TrimStart(span));

        public static ReadOnlySpan<Utf8Char> TrimEnd(this ReadOnlySpan<Utf8Char> span)
        {
            return span.Slice(0, Utf8Utility.GetIndexOfTrailingWhiteSpaceSequence(span.AsBytes()));
        }

        public static ReadOnlySpan<Utf8Char> TrimStart(this ReadOnlySpan<Utf8Char> span)
        {
            return span.Slice(Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(span.AsBytes()));
        }
    }
}
