// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace System.Text
{
    /// <summary>
    /// Contains static methods for working with UTF-8 textual data.
    /// </summary>
    public static class Utf8
    {
        // Ordinal
        public static bool Contains(ReadOnlySpan<byte> source, UnicodeScalar scalar)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static bool Contains(ReadOnlySpan<byte> source, UnicodeScalar scalar, StringComparison comparison)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        // Ordinal; returns -1 if not found
        public static int IndexOf(ReadOnlySpan<byte> source, UnicodeScalar scalar)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        // Returns -1 if not found
        public static int IndexOf(ReadOnlySpan<byte> source, UnicodeScalar scalar, StringComparison comparison, out int matchLength)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static int ToLower(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static OperationStatus ToLower(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static int ToLowerInvariant(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static OperationStatus ToLowerInvariant(ReadOnlySpan<byte> source, Span<byte> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static int ToUpper(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static OperationStatus ToUpper(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static int ToUpperInvariant(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static OperationStatus ToUpperInvariant(ReadOnlySpan<byte> source, Span<byte> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }
    }
}
