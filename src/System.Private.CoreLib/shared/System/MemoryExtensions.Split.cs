// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public static partial class MemoryExtensions
    {
        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> span, T separator, StringSplitOptions options = StringSplitOptions.None)
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
            => new SpanSplitEnumerator<T>(span, separator, options);

        public ref struct SpanSplitEnumerator<T>
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
        {
            private readonly ReadOnlySpan<T> _span;
            private readonly T _separator;
            private readonly bool _removeEmpty;
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

                while (true)
                {
                    if (_start > _span.Length)
                    {
                        _ended = true;
                        return false;
                    }

                    ReadOnlySpan<T> slice = _start == 0 ? _span : _span.Slice(_start);
                    int index = slice.IndexOf(_separator);

                    switch (index)
                    {
                        case -1:
                            index = slice.Length;
                            break;

                        case 0:
                            if (_removeEmpty)
                            {
                                _start++;
                                continue;
                            }
                            break;
                    }

                    int end = _start + index;
                    _current = new Range(_start, end);
                    _start = end + 1;

                    return true;
                }
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

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator, StringSplitOptions options)
            {
                _span = span;
                _separator = separator;
                _removeEmpty = (options & StringSplitOptions.RemoveEmptyEntries) != 0;
                _started = false;
                _ended = false;
                _start = 0;
                _current = default;
            }
        }
    }
}
