// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public static partial class MemoryExtensions
    {
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span)
            => new SpanSplitEnumerator<char>(span, ' '); // TODO: split on char.IsWhiteSpace

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(span, separator);

        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, string separator)
            => new SpanSplitEnumerator<char>(span, separator[0]); // TODO: Fix impl

        public ref struct SpanSplitEnumerator<T>
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
        {
            private readonly ReadOnlySpan<T> _span;
            private readonly T _separator;
            private bool _started;
            private bool _ended;
            private int _start;
            private Range _current;

            /// <summary>
            /// Implements the IEnumerator pattern.
            /// </summary>
            public SpanSplitEnumerator<T> GetEnumerator() => this;

            /// <summary>
            /// Implements the IEnumerator pattern.
            /// </summary>
            public bool MoveNext()
            {
                _started = true;

                if (_start > _span.Length)
                {
                    _ended = true;
                    return false;
                }

                ReadOnlySpan<T> slice = _start == 0
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

            /// <summary>
            /// Implements the IEnumerator pattern.
            /// </summary>
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

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator)
            {
                _span = span;
                _separator = separator;
                _started = false;
                _ended = false;
                _start = 0;
                _current = default;
            }
        }
    }
}
