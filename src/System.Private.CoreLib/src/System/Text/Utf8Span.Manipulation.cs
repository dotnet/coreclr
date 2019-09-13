// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace System.Text
{
    public readonly ref partial struct Utf8Span
    {
        [StackTraceHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSplitOptions(Utf8StringSplitOptions options)
        {
            if ((uint)options > (uint)(Utf8StringSplitOptions.RemoveEmptyEntries | Utf8StringSplitOptions.TrimEntries))
            {
                CheckSplitOptions_Throw(options);
            }
        }

        [StackTraceHidden]
        private static void CheckSplitOptions_Throw(Utf8StringSplitOptions options)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(options),
                message: SR.Format(SR.Arg_EnumIllegalVal, (int)options));
        }

        public SplitResult Split(char separator, Utf8StringSplitOptions options = Utf8StringSplitOptions.None)
        {
            if (!Rune.TryCreate(separator, out Rune rune))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.separator, ExceptionResource.ArgumentOutOfRange_Utf16SurrogatesDisallowed);
            }

            CheckSplitOptions(options);

            return new SplitResult(this, rune, options);
        }

        public SplitResult Split(Rune separator, Utf8StringSplitOptions options = Utf8StringSplitOptions.None)
        {
            CheckSplitOptions(options);

            return new SplitResult(this, separator, options);
        }

        public SplitResult Split(Utf8Span separator, Utf8StringSplitOptions options = Utf8StringSplitOptions.None)
        {
            if (separator.IsEmpty)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_CannotBeEmptySpan, ExceptionArgument.separator);
            }

            CheckSplitOptions(options);

            return new SplitResult(this, separator, options);
        }

        /// <summary>
        /// Locates <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOn(char separator)
        {
            return TryFind(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOn(char separator, StringComparison comparisonType)
        {
            return TryFind(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOn(Rune separator)
        {
            return TryFind(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOn(Rune separator, StringComparison comparisonType)
        {
            return TryFind(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOn(Utf8Span separator)
        {
            return TryFind(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Utf8Span)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOn(Utf8Span separator, StringComparison comparisonType)
        {
            return TryFind(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOnLast(char separator)
        {
            return TryFindLast(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOnLast(char separator, StringComparison comparisonType)
        {
            return TryFindLast(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOnLast(Rune separator)
        {
            return TryFindLast(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOnLast(Rune separator, StringComparison comparisonType)
        {
            return TryFindLast(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// An ordinal search is performed.
        /// </remarks>
        public SplitOnResult SplitOnLast(Utf8Span separator)
        {
            return TryFindLast(separator, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Locates the last occurrence of <paramref name="separator"/> within this <see cref="Utf8Span"/> instance, creating <see cref="Utf8Span"/>
        /// instances which represent the data on either side of the separator. If <paramref name="separator"/> is not found
        /// within this <see cref="Utf8Span"/> instance, returns the tuple "(this, Empty)".
        /// </summary>
        /// <remarks>
        /// The search is performed using the specified <paramref name="comparisonType"/>.
        /// </remarks>
        public SplitOnResult SplitOnLast(Utf8Span separator, StringComparison comparisonType)
        {
            return TryFindLast(separator, comparisonType, out Range range) ? new SplitOnResult(this, range) : new SplitOnResult(this);
        }

        /// <summary>
        /// Trims whitespace from the beginning and the end of this <see cref="Utf8Span"/>,
        /// returning a new <see cref="Utf8Span"/> containing the resulting slice.
        /// </summary>
        public Utf8Span Trim() => TrimHelper(TrimType.Both);

        /// <summary>
        /// Trims whitespace from only the end of this <see cref="Utf8Span"/>,
        /// returning a new <see cref="Utf8Span"/> containing the resulting slice.
        /// </summary>
        public Utf8Span TrimEnd() => TrimHelper(TrimType.Tail);

        internal Utf8Span TrimHelper(TrimType trimType)
        {
            ReadOnlySpan<byte> retSpan = Bytes;

            if ((trimType & TrimType.Head) != 0)
            {
                int indexOfFirstNonWhiteSpaceChar = Utf8Utility.GetIndexOfFirstNonWhiteSpaceChar(retSpan);
                Debug.Assert((uint)indexOfFirstNonWhiteSpaceChar <= (uint)retSpan.Length);

                // TODO_UTF8STRING: Can use an unsafe slicing routine below if we need a perf boost.

                retSpan = retSpan.Slice(indexOfFirstNonWhiteSpaceChar);
            }

            if ((trimType & TrimType.Tail) != 0)
            {
                int indexOfTrailingWhiteSpaceSequence = Utf8Utility.GetIndexOfTrailingWhiteSpaceSequence(retSpan);
                Debug.Assert((uint)indexOfTrailingWhiteSpaceSequence <= (uint)retSpan.Length);

                // TODO_UTF8STRING: Can use an unsafe slicing routine below if we need a perf boost.

                retSpan = retSpan.Slice(0, indexOfTrailingWhiteSpaceSequence);
            }

            return UnsafeCreateWithoutValidation(retSpan);
        }

        /// <summary>
        /// Trims whitespace from only the beginning of this <see cref="Utf8Span"/>,
        /// returning a new <see cref="Utf8Span"/> containing the resulting slice.
        /// </summary>
        public Utf8Span TrimStart() => TrimHelper(TrimType.Head);

        [StructLayout(LayoutKind.Auto)]
        public readonly ref struct SplitResult
        {
            private readonly Utf8Span _originalSource;
            private readonly int _searchRune; // -1 if not specified, takes less space than "Rune?"
            private readonly Utf8Span _searchTerm;
            private readonly Utf8StringSplitOptions _splitOptions;

            internal SplitResult(Utf8Span source, Rune searchRune, Utf8StringSplitOptions splitOptions)
            {
                _originalSource = source;
                _searchRune = searchRune.Value;
                _searchTerm = default;
                _splitOptions = splitOptions;
            }

            internal SplitResult(Utf8Span source, Utf8Span searchTerm, Utf8StringSplitOptions splitOptions)
            {
                _originalSource = source;
                _searchRune = -1;
                _searchTerm = searchTerm;
                _splitOptions = splitOptions;
            }

            private void ApplySplitOptions(ref Utf8Span input)
            {
                if ((_splitOptions & Utf8StringSplitOptions.TrimEntries) != 0)
                {
                    input = input.Trim();
                }

                if ((_splitOptions & Utf8StringSplitOptions.RemoveEmptyEntries) != 0)
                {
                    if (input.IsEmpty)
                    {
                        input = default;
                    }
                }
            }

            private void DeconstructHelper(in Utf8Span actualSource, out Utf8Span firstItem, out Utf8Span remainder)
            {
                // n.b. Our callers might pass the same reference for 'actualSource' and 'remainder'.
                // We need to take care not to read 'actualSource' after writing 'remainder'.

                if (actualSource.IsNull)
                {
                    firstItem = default;
                    remainder = default;
                    return;
                }

                if (_searchRune >= 0)
                {
                    Debug.Assert(Rune.IsValid(_searchRune));
                    actualSource.SplitOn(Rune.UnsafeCreate((uint)_searchRune)).Deconstruct(out firstItem, out remainder);
                }
                else
                {
                    actualSource.SplitOn(_searchTerm).Deconstruct(out firstItem, out remainder);
                }

                ApplySplitOptions(ref firstItem);

                // It's possible that 'firstItem' could be null, such as if it's an empty string
                // and we were asked to trim empty entries. We'll keep iterating either until we
                // run out of data or until we have a non-null output for firstItem.

                while (firstItem.IsNull && !remainder.IsNull)
                {
                    if (_searchRune >= 0)
                    {
                        Debug.Assert(Rune.IsValid(_searchRune));
                        remainder.SplitOn(Rune.UnsafeCreate((uint)_searchRune)).Deconstruct(out firstItem, out remainder);
                    }
                    else
                    {
                        remainder.SplitOn(_searchTerm).Deconstruct(out firstItem, out remainder);
                    }

                    ApplySplitOptions(ref firstItem);
                }

                ApplySplitOptions(ref remainder);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2)
            {
                DeconstructHelper(in _originalSource, out item1, out item2);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out item3);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3, out Utf8Span item4)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out remainder);
                DeconstructHelper(in remainder, out item3, out item4);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3, out Utf8Span item4, out Utf8Span item5)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out remainder);
                DeconstructHelper(in remainder, out item3, out remainder);
                DeconstructHelper(in remainder, out item4, out item5);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3, out Utf8Span item4, out Utf8Span item5, out Utf8Span item6)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out remainder);
                DeconstructHelper(in remainder, out item3, out remainder);
                DeconstructHelper(in remainder, out item4, out remainder);
                DeconstructHelper(in remainder, out item5, out item6);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3, out Utf8Span item4, out Utf8Span item5, out Utf8Span item6, out Utf8Span item7)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out remainder);
                DeconstructHelper(in remainder, out item3, out remainder);
                DeconstructHelper(in remainder, out item4, out remainder);
                DeconstructHelper(in remainder, out item5, out remainder);
                DeconstructHelper(in remainder, out item6, out item7);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span item1, out Utf8Span item2, out Utf8Span item3, out Utf8Span item4, out Utf8Span item5, out Utf8Span item6, out Utf8Span item7, out Utf8Span item8)
            {
                DeconstructHelper(in _originalSource, out item1, out Utf8Span remainder);
                DeconstructHelper(in remainder, out item2, out remainder);
                DeconstructHelper(in remainder, out item3, out remainder);
                DeconstructHelper(in remainder, out item4, out remainder);
                DeconstructHelper(in remainder, out item5, out remainder);
                DeconstructHelper(in remainder, out item6, out remainder);
                DeconstructHelper(in remainder, out item7, out item8);
            }

            public Enumerator GetEnumerator() => new Enumerator(this);

            [StructLayout(LayoutKind.Auto)]
            public ref struct Enumerator
            {
                private Utf8Span _current;
                private Utf8Span _remainder;
                private readonly SplitResult _result;

                internal Enumerator(SplitResult result)
                {
                    _result = result;
                    _remainder = result._originalSource;
                    _current = default;
                }

                public Utf8Span Current => _current;

                public bool MoveNext()
                {
                    _result.DeconstructHelper(in _remainder, out _current, out _remainder);
                    return !_current.IsNull;
                }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly ref struct SplitOnResult
        {
            // Used when there is no match.
            internal SplitOnResult(Utf8Span originalSearchSpace)
            {
                Before = originalSearchSpace;
                After = Empty;
            }

            // Used when a match is found.
            internal SplitOnResult(Utf8Span originalSearchSpace, Range searchTermMatchRange)
            {
                (int startIndex, int length) = searchTermMatchRange.GetOffsetAndLength(originalSearchSpace.Length);

                // TODO_UTF8STRING: The below indexer performs correctness checks. We can skip these checks (and even the
                // bounds checks more generally) since we know the inputs are all valid and the containing struct is not
                // subject to tearing.

                Before = originalSearchSpace[..startIndex];
                After = originalSearchSpace[(startIndex + length)..];
            }

            public Utf8Span After { get; }
            public Utf8Span Before { get; }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out Utf8Span before, out Utf8Span after)
            {
                before = Before;
                after = After;
            }
        }
    }
}
