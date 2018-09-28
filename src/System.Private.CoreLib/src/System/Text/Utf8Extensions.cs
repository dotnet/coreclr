// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

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

        public static ScalarCollection GetScalars(this ReadOnlySpan<Utf8Char> span)
        {
            return new ScalarCollection(span.AsBytes());
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

        public static Utf8String ToUtf8String(this ReadOnlySpan<Utf8Char> span) => new Utf8String(span.AsBytes());

        public static ReadOnlyMemory<Utf8Char> Trim(this ReadOnlyMemory<Utf8Char> memory) => TrimHelper(memory, TrimType.Both);

        public static ReadOnlySpan<Utf8Char> Trim(this ReadOnlySpan<Utf8Char> span) => TrimEnd(TrimStart(span));

        private static ReadOnlyMemory<Utf8Char> TrimHelper(this ReadOnlyMemory<Utf8Char> memory, TrimType trimType)
        {
            ReadOnlySpan<byte> span = memory.Span.AsBytes();
            object memoryObject = memory.GetObjectStartLength(out int memoryStart, out int memoryLength);

            if (trimType.HasFlag(TrimType.Head))
            {
                int indexOfFirstNonWhiteSpaceChar = Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(span);
                memoryStart += indexOfFirstNonWhiteSpaceChar;
                memoryLength -= indexOfFirstNonWhiteSpaceChar;
                span = span.DangerousSliceWithoutBoundsCheck(indexOfFirstNonWhiteSpaceChar);
            }

            if (trimType.HasFlag(TrimType.Tail))
            {
                memoryLength = Utf8Utility.GetIndexOfTrailingWhiteSpaceSequence(span);
            }

            return new ReadOnlyMemory<Utf8Char>(memoryObject, memoryStart, memoryLength);
        }

        public static ReadOnlyMemory<Utf8Char> TrimEnd(this ReadOnlyMemory<Utf8Char> memory) => TrimHelper(memory, TrimType.Tail);

        public static ReadOnlySpan<Utf8Char> TrimEnd(this ReadOnlySpan<Utf8Char> span)
        {
            return span.Slice(0, Utf8Utility.GetIndexOfTrailingWhiteSpaceSequence(span.AsBytes()));
        }

        public static ReadOnlyMemory<Utf8Char> TrimStart(this ReadOnlyMemory<Utf8Char> memory) => TrimHelper(memory, TrimType.Head);

        public static ReadOnlySpan<Utf8Char> TrimStart(this ReadOnlySpan<Utf8Char> span)
        {
            return span.Slice(Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(span.AsBytes()));
        }

        public readonly ref struct ScalarCollection
        {
            private readonly ReadOnlySpan<byte> _utf8Data;

            internal ScalarCollection(ReadOnlySpan<byte> utf8Data)
            {
                _utf8Data = utf8Data;
            }

            public Enumerator GetEnumerator() => new Enumerator(_utf8Data);

            public ref struct Enumerator
            {
                private ReadOnlySpan<byte> _remainingData;

                internal Enumerator(ReadOnlySpan<byte> utf8Data)
                {
                    _remainingData = utf8Data;
                    Current = default;
                }

                public UnicodeScalar Current { get; private set; }

                public bool MoveNext()
                {
                    var result = UnicodeReader.PeekFirstScalarUtf8(_remainingData);
                    Current = result.scalar;
                    return (result.charsConsumed != 0); // return true iff we read any data at all
                }
            }
        }
    }
}
