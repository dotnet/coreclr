// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Internal.Runtime.CompilerServices;

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
        public static bool operator ==(StringSegment a, StringSegment b) => a.AsSpan().SequenceEqual(b.AsSpan());

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

        public ref readonly char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Length)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                // We use AsSpanFast() because we expect the string to be non-null at this point,
                // as the check above should throw for any zero-length segment. If this is a torn
                // struct, the accessor below will null ref, which is fine. The span indexer will
                // also make sure we're not going out of bounds when reading the string instance.

                return ref _value.AsSpanFast()[_offset + index];
            }
        }

        /*
         * METHODS
         */

        // This method is guaranteed not to return a span outside the bounds of the
        // underlying string instance, even in the face of a torn struct.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe ReadOnlySpan<char> GetSpanInternal()
        {
            ref char spanBuffer = ref Unsafe.AsRef<char>(null);
            int spanLength = 0;

            // The stored length can be negative in the face of a torn struct.
            // The if check below prevent us from using it if this is the case.

            int tempLength = Length;

            if (tempLength > 0)
            {
                string tempString = _value;

                // The stored offset can be negative in the face of a torn struct.
                // Clear the high bit to force the number to be non-negative. We'll
                // perform a bounds check against this normalized value later.

                int tempOffset = _offset & 0x7FFFFFFF;

                // Since both the offset and the length are non-negative signed integers,
                // their sum fits into the range of an unsigned integer without overflow.

                if ((uint)(tempLength + tempOffset) > (uint)tempString.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                spanBuffer = ref Unsafe.Add(ref tempString.GetRawStringData(), tempOffset);
                spanLength = tempLength;
            }

            return new ReadOnlySpan<char>(ref spanBuffer, spanLength);
        }

        public bool Contains(char value) => this.AsSpan().Contains(value);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public int IndexOf(char value) => this.AsSpan().IndexOf(value);

        public bool IsEmptyOrWhiteSpace() => this.AsSpan().IsWhiteSpace(); // also performs "is empty?" check

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSameSegment(StringSegment a, StringSegment b)
        {
            // TODO: Can this be optimized by reading offset + count as a single int64?

            return a._value == b._value
                && a._offset == b._offset
                && a._count == b._count;
        }

        public int LastIndexOf(char value) => this.AsSpan().LastIndexOf(value);

        public StringSegment Substring(int startIndex)
        {
            if ((uint)startIndex < (uint)_count)
            {
                // Most common case: substring doesn't eliminate the entire value
                return new StringSegment(_value, _offset + startIndex, _count - startIndex, unused: false);
            }
            else if (startIndex == _count)
            {
                // Less common case: substring away the entire string contents
                return default;
            }
            else
            {
                // TODO: better exception
                throw new Exception("Argument out of range.");
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
            if (_value != null)
            {
                return _value.Substring(_offset, _count);
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
