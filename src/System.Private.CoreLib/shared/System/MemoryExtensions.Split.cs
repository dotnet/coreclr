// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public static partial class MemoryExtensions
    {
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span)
            => new SpanSplitEnumerator<char>(span);

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(span, separator);

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, string separator)
            => new SpanSplitEnumerator<char>(span, separator);

        // TODO: Is this required (missed on the spec)?
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, string separator, StringComparison comparisonType)
            => new SpanSplitEnumerator<char>(span, separator, comparisonType);

        public ref struct SpanSplitEnumerator<T>
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
        {
            private readonly CharSpanSplitKind _kind;
            private readonly ReadOnlySpan<char> _span;
            private readonly char _separatorChar;
            private readonly string _separatorString;
            private readonly StringComparison _comparisonType;
            private int _start;
            private bool _started;
            private bool _ended;
            private Range _current;

            public SpanSplitEnumerator<T> GetEnumerator() => this;

            public Range Current
            {
                get
                {
                    if (!_started || _ended)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }

                    return _current;
                }
            }

            internal SpanSplitEnumerator(ReadOnlySpan<char> span) : this()
            {
                _kind = CharSpanSplitKind.Trim;
                _span = span;
                _separatorString = string.Empty;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<char> span, char separator) : this()
            {
                _kind = CharSpanSplitKind.Char;
                _span = span;
                _separatorChar = separator;
                _separatorString = string.Empty;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<char> span, string separator) : this()
            {
                _kind = CharSpanSplitKind.String;
                _span = span;
                _separatorString = separator;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<char> span, string separator, StringComparison comparisonType) : this()
            {
                _kind = CharSpanSplitKind.StringCompare;
                _span = span;
                _separatorString = separator;
                _comparisonType = comparisonType;
            }

            public bool MoveNext()
            {
                _started = true;

                if (_start > _span.Length)
                {
                    _ended = true;
                    return false;
                }

                ReadOnlySpan<char> slice = _start == 0
                    ? _span
                    : _span.Slice(_start);

                int end = _start;
                if (slice.Length > 0)
                {
                    int index;
                    switch (_kind)
                    {
                        default: // TODO: Throw?

                        case CharSpanSplitKind.Trim:
                            index = MoveNext(slice);
                            break;

                        case CharSpanSplitKind.Char:
                            index = MoveNext(slice, _separatorChar);
                            break;

                        case CharSpanSplitKind.String:
                            index = MoveNext(slice, _separatorString);
                            break;

                        case CharSpanSplitKind.StringCompare:
                            index = MoveNext(slice, _separatorString, _comparisonType);
                            break;
                    }

                    if (index == -1)
                    {
                        index = slice.Length;
                    }

                    end += index;
                }

                _current = new Range(_start, end);
                _start = end + 1;

                return true;
            }

            private static int MoveNext(ReadOnlySpan<char> slice)
            {
                // TODO: Optimize
                for (int i = 0; i < slice.Length; i++)
                {
                    if (char.IsWhiteSpace(slice[i]))
                    {
                        return i;
                    }
                }

                return -1;
            }

            private static int MoveNext(ReadOnlySpan<char> slice, char separator)
                => slice.IndexOf(separator);

            private static int MoveNext(ReadOnlySpan<char> slice, string separator)
                => slice.IndexOf(separator);

            private static int MoveNext(ReadOnlySpan<char> slice, string separator, StringComparison comparisonType)
                => slice.IndexOf(separator, comparisonType);

            private enum CharSpanSplitKind
            {
                Trim = 0,
                Char,
                String,
                StringCompare
            }
        }
    }
}
