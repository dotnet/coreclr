// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public static partial class MemoryExtensions
    {
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span)
            => new SpanSplitEnumerator<char>(new CharSpanSplitTrimEnumerator(span));

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(new CharSpanSplitByCharEnumerator(span, separator));

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, string separator)
            => new SpanSplitEnumerator<char>(new CharSpanSplitByStringEnumerator(span, separator));

        public ref struct SpanSplitEnumerator<T>
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
        {
            private readonly CharSpanTrimKind _kind;
            private readonly CharSpanSplitTrimEnumerator _enumeratorTrim;
            private readonly CharSpanSplitByCharEnumerator _enumeratorChar;
            private readonly CharSpanSplitByStringEnumerator _enumeratorString;

            public SpanSplitEnumerator<T> GetEnumerator() => this;

            internal SpanSplitEnumerator(CharSpanSplitTrimEnumerator enumerator)
            {
                _kind = CharSpanTrimKind.Trim;
                _enumeratorTrim = enumerator;
                _enumeratorChar = default;
                _enumeratorString = default;
            }

            internal SpanSplitEnumerator(CharSpanSplitByCharEnumerator enumerator)
            {
                _kind = CharSpanTrimKind.Char;
                _enumeratorTrim = default;
                _enumeratorChar = enumerator;
                _enumeratorString = default;
            }

            internal SpanSplitEnumerator(CharSpanSplitByStringEnumerator enumerator)
            {
                _kind = CharSpanTrimKind.String;
                _enumeratorTrim = default;
                _enumeratorChar = default;
                _enumeratorString = enumerator;
            }

            public bool MoveNext()
            {
                switch (_kind)
                {
                    default: // TODO: Throw?
                    case CharSpanTrimKind.Trim: return _enumeratorTrim.MoveNext();
                    case CharSpanTrimKind.Char: return _enumeratorChar.MoveNext();
                    case CharSpanTrimKind.String: return _enumeratorString.MoveNext();
                }
            }

            public Range Current
            {
                get
                {
                    switch (_kind)
                    {
                        default: // TODO: Throw?
                        case CharSpanTrimKind.Trim: return _enumeratorTrim.Current;
                        case CharSpanTrimKind.Char: return _enumeratorChar.Current;
                        case CharSpanTrimKind.String: return _enumeratorString.Current;
                    }
                }
            }

            private enum CharSpanTrimKind
            {
                Trim = 0,
                Char,
                String
            }
        }

        internal ref struct CharSpanSplitTrimEnumerator
        {
            private readonly ReadOnlySpan<char> _span;
            private bool _started;
            private bool _ended;
            private int _start;
            private Range _current;

            internal CharSpanSplitTrimEnumerator(ReadOnlySpan<char> span)
            {
                _span = span;
                _started = false;
                _ended = false;
                _start = 0;
                _current = default;
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
                    int index = slice.IndexOfAny(' ', '\n'); // TODO: Fix

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
        }

        internal ref struct CharSpanSplitByCharEnumerator
        {
            private readonly ReadOnlySpan<char> _span;
            private readonly char _separator;
            private bool _started;
            private bool _ended;
            private int _start;
            private Range _current;

            internal CharSpanSplitByCharEnumerator(ReadOnlySpan<char> span, char separator)
            {
                _span = span;
                _separator = separator;
                _started = false;
                _ended = false;
                _start = 0;
                _current = default;
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
                    int index = slice.IndexOf(_separator);

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
        }

        internal ref struct CharSpanSplitByStringEnumerator
        {
            private readonly ReadOnlySpan<char> _span;
            private readonly string _separator;
            private bool _started;
            private bool _ended;
            private int _start;
            private Range _current;

            internal CharSpanSplitByStringEnumerator(ReadOnlySpan<char> span, string separator)
            {
                _span = span;
                _separator = separator;
                _started = false;
                _ended = false;
                _start = 0;
                _current = default;
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
                    int index = slice.IndexOf(_separator);

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
        }
    }
}
