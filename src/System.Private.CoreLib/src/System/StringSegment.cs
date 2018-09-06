// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    // The StringSegment type guarantees internal consistency *only in single-threaded environments*.
    // In multithreaded environments, multiple threads may be updating a single StringSegment struct
    // simultaneously, which could lead to a torn (internally inconsistent) struct. In this case, methods
    // which operate on StringSegment instances may have undefined behavior, such as having exceptions
    // thrown unexpectedly or reading the wrong data, but the golden rule is that *we shouldn't AV*. This
    // implies that we need to avoid unsafe code whenever possible in this type.

    // n.b. There is no GetPinnableReference method since we can't guarantee null termination.
    // Instead, the caller must pull out the span, then pin the span.

    public readonly struct StringSegment : IEquatable<StringSegment>
    {
        private readonly string _value;
        private readonly int _offset;
        private readonly int _count;

        /*
         * CTORS
         */

        public StringSegment(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _value = value;
                _offset = 0;
                _count = value.Length;
            }
            else
            {
                _value = default;
                _offset = default;
                _count = default;
            }
        }

        public StringSegment(string value, int offset, int count)
        {
            if (value != null)
            {
                if (((uint)offset > (uint)value.Length) || ((uint)count > (uint)(value.Length - offset)))
                {
                    // TODO: better exception
                    throw new Exception("Arguments out of range.");
                }

                if (count == 0)
                {
                    value = null; // don't capture value if there's no need to
                    offset = 0;
                }

                _value = value;
                _offset = offset;
                _count = count;
            }
            else if (offset != 0 || count != 0)
            {
                // TODO: better exception
                throw new Exception("Arguments out of range.");
            }
            else
            {
                _value = default;
                _offset = default;
                _count = default;
            }
        }

        // non-validating ctor
        private StringSegment(string value, int offset, int count, bool unused)
        {
            // Caller should've checked already that empty segments are normalized to (null, 0, 0)

            if (count == 0)
            {
                Debug.Assert(value != null);
                Debug.Assert(offset == 0);
            }

            _value = value;
            _offset = offset;
            _count = count;
        }

        /*
         * OPERATORS
         */

        public static implicit operator ReadOnlySpan<char>(StringSegment value) => value.AsSpan();

        public static implicit operator StringSegment(string value) => new StringSegment(value);

        // ordinal case-sensitive comparison
        public static bool operator ==(StringSegment a, StringSegment b) => IsSameSegment(a, b) || a.AsSpan().SequenceEqual(b.AsSpan());

        // TODO: How do we make "<StringSegment> != null" result in a compilation error rather
        // than coercing 'null' to string?
        //
        // ordinal case-sensitive comparison
        public static bool operator !=(StringSegment a, StringSegment b) => !(a == b);

        /*
         * PROPERTIES
         */

        public bool IsEmpty => (Length == 0);

        public int Length => _count;

        /*
         * METHODS
         */

        public override bool Equals(object obj)
        {
            return (obj is StringSegment value) ? Equals(value) : (obj == null && IsEmpty);
        }

        // ordinal case-sensitive comparison
        public bool Equals(StringSegment value) => (this == value);

        public static bool Equals(StringSegment a, StringSegment b, StringComparison comparisonType)
        {
            // This is based on the logic in String.Equals(String, String, StringComparison)

            string.CheckStringComparison(comparisonType);

            // A substring will always compare equal with itself, so special-case that before
            // trying to call into any deep equality check routine.

            return IsSameSegment(a, b) || a.AsSpan().Equals(b.AsSpan(), comparisonType);
        }

        public string GetBuffer(out int offset, out int length)
        {
            offset = _offset;
            length = _count;
            return _value;
        }

        // ordinal case-sensitive hash code
        public override int GetHashCode() => string.GetHashCode(this.AsSpan());

        public int GetHashCode(StringComparison comparisonType) => string.GetHashCode(this.AsSpan(), comparisonType);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This type cannot be pinned because it may result in a char* without a null terminator.", error: true)]
        public ref readonly char GetPinnableReference() => throw new NotSupportedException();

        public bool IsEmptyOrWhiteSpace() => this.AsSpan().IsWhiteSpace(); // also performs "is empty?" check

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSameSegment(StringSegment a, StringSegment b)
        {
            // TODO: Can this be optimized by reading offset + count as a single int64?

            return a._value == b._value
                && a._offset == b._offset
                && a._count == b._count;
        }

        public StringSegment Substring(int startIndex)
        {
            if ((uint)startIndex > (uint)_count)
            {
                // TODO: better exception
                throw new Exception("Argument out of range.");
            }
            else if (startIndex != _count)
            {
                return new StringSegment(_value, _offset + startIndex, _count - startIndex, unused: false);
            }
            else
            {
                // don't hold on to 'value' if we don't need to
                return default;
            }
        }

        public StringSegment Substring(int startIndex, int length)
        {
            if (((uint)startIndex > (uint)_count) || ((uint)length > (uint)(_count - startIndex)))
            {
                // TODO: better exception
                throw new Exception("Argument out of range.");
            }
            else if (length != 0)
            {
                return new StringSegment(_value, _offset + startIndex, length, unused: false);
            }
            else
            {
                // don't hold on to 'value' if we don't need to
                return default;
            }
        }

        public override string ToString()
        {
            if (_count != 0)
            {
                if (_count == _value.Length)
                {
                    return _value;
                }
                else
                {
                    return _value.Substring(_offset, _count);
                }
            }
            else
            {
                return string.Empty;
            }
        }
        
        public StringSegment Trim() => TrimHelper(TrimType.Both);

        public StringSegment TrimEnd() => TrimHelper(TrimType.Tail);

        private StringSegment TrimHelper(TrimType trimType)
        {
            StringSegment retVal = this;

            if (trimType.HasFlag(TrimType.Head))
            {
                var span = retVal.AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    if (!char.IsWhiteSpace(span[i]))
                    {
                        retVal = retVal.Substring(i);
                        break;
                    }
                }
            }

            if (trimType.HasFlag(TrimType.Tail))
            {
                var span = retVal.AsSpan();
                for (int i = span.Length - 1; i >= 0; i++)
                {
                    if (!char.IsWhiteSpace(span[i]))
                    {
                        retVal = retVal.Substring(0, i + 1);
                        break;
                    }
                }
            }

            // The Substring method will clear out the 'value' field if this is an empty segment.

            return retVal;
        }

        public StringSegment TrimStart() => TrimHelper(TrimType.Head);
    }
}
