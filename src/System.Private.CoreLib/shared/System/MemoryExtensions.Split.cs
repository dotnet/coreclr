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
        {
            return new SpanSplitEnumerator<T>(span, separator, options);
        }

        public ref struct SpanSplitEnumerator<T>
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
            where T : IEquatable<T>
#nullable restore
        {
            private readonly ReadOnlySpan<T> _span;
            private readonly T _separator;
            private readonly bool _removeEmpty;
            private bool _started;
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
                int start = _started ? _current.End.Value + 1 : 0;
                _started = true;

                int end = start;
                for (; end < _span.Length; end++)
                {
                    if (_span[end].Equals(_separator))
                    {
                        if (_removeEmpty
                            && end - start == 0)
                        {
                            start = end + 1;
                            continue;
                        }

                        goto found;
                    }

                    if (end == _span.Length - 1)
                    {
                        goto found;
                    }
                }

                return false;

            found:
                _current = new Range(new Index(start), new Index(end));
                return true;
            }

            /// <summary>
            /// Implements the IEnumerator pattern.
            /// </summary>
            public Range Current
            {
                get
                {
                    if (!_started)
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
                _current = default;
            }
        }
    }
}
