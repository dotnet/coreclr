// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    public static partial class MemoryExtensions
    {
        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Memory<T> Trim<T>(this Memory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            Span<T> span = memory.Span;
            int start = ClampStart(span, trimElement);
            int length = ClampEnd(span, start, trimElement);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Memory<T> TrimStart<T>(this Memory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(memory.Span, trimElement);
            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Memory<T> TrimEnd<T>(this Memory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            int length = ClampEnd(memory.Span, 0, trimElement);
            return memory.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyMemory<T> Trim<T>(this ReadOnlyMemory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            ReadOnlySpan<T> span = memory.Span;
            int start = ClampStart(span, trimElement);
            int length = ClampEnd(span, start, trimElement);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyMemory<T> TrimStart<T>(this ReadOnlyMemory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(memory.Span, trimElement);
            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyMemory<T> TrimEnd<T>(this ReadOnlyMemory<T> memory, T trimElement)
            where T : IEquatable<T>
        {
            int length = ClampEnd(memory.Span, 0, trimElement);
            return memory.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Span<T> Trim<T>(this Span<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(span, trimElement);
            int length = ClampEnd(span, start, trimElement);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Span<T> TrimStart<T>(this Span<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(span, trimElement);
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static Span<T> TrimEnd<T>(this Span<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int length = ClampEnd(span, 0, trimElement);
            return span.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(span, trimElement);
            int length = ClampEnd(span, start, trimElement);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlySpan<T> TrimStart<T>(this ReadOnlySpan<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int start = ClampStart(span, trimElement);
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlySpan<T> TrimEnd<T>(this ReadOnlySpan<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int length = ClampEnd(span, 0, trimElement);
            return span.Slice(0, length);
        }

        /// <summary>
        /// Delimits all leading occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        private static int ClampStart<T>(ReadOnlySpan<T> span, T trimElement)
            where T : IEquatable<T>
        {
            int start = 0;

            if (trimElement != null)
            {
                for (; start < span.Length; start++)
                {
                    if (!trimElement.Equals(span[start]))
                    {
                        break;
                    }
                }
            }
            else
            {
                for (; start < span.Length; start++)
                {
                    if (span[start] != null)
                    {
                        break;
                    }
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        private static int ClampEnd<T>(ReadOnlySpan<T> span, int start, T trimElement)
            where T : IEquatable<T>
        {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert((uint)start <= span.Length);

            int end = span.Length - 1;

            if (trimElement != null)
            {
                for (; end >= start; end--)
                {
                    if (!trimElement.Equals(span[end]))
                    {
                        break;
                    }
                }
            }
            else
            {
                for (; end >= start; end--)
                {
                    if (span[end] != null)
                    {
                        break;
                    }
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static Memory<T> Trim<T>(this Memory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : Trim(memory, trimElements[0]);
            }

            Span<T> span = memory.Span;
            int start = ClampStart(span, trimElements);
            int length = ClampEnd(span, start, trimElements);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static Memory<T> TrimStart<T>(this Memory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : TrimStart(memory, trimElements[0]);
            }

            int start = ClampStart(memory.Span, trimElements);
            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static Memory<T> TrimEnd<T>(this Memory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : TrimEnd(memory, trimElements[0]);
            }

            int length = ClampEnd(memory.Span, 0, trimElements);
            return memory.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyMemory<T> Trim<T>(this ReadOnlyMemory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : Trim(memory, trimElements[0]);
            }

            ReadOnlySpan<T> span = memory.Span;
            int start = ClampStart(span, trimElements);
            int length = ClampEnd(span, start, trimElements);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyMemory<T> TrimStart<T>(this ReadOnlyMemory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : TrimStart(memory, trimElements[0]);
            }

            int start = ClampStart(memory.Span, trimElements);
            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyMemory<T> TrimEnd<T>(this ReadOnlyMemory<T> memory, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? memory : TrimEnd(memory, trimElements[0]);
            }

            int length = ClampEnd(memory.Span, 0, trimElements);
            return memory.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static Span<T> Trim<T>(this Span<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : Trim(span, trimElements[0]);
            }

            int start = ClampStart(span, trimElements);
            int length = ClampEnd(span, start, trimElements);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static Span<T> TrimStart<T>(this Span<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : TrimStart(span, trimElements[0]);
            }

            int start = ClampStart(span, trimElements);
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static Span<T> TrimEnd<T>(this Span<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : TrimEnd(span, trimElements[0]);
            }

            int length = ClampEnd(span, 0, trimElements);
            return span.Slice(0, length);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : Trim(span, trimElements[0]);
            }

            int start = ClampStart(span, trimElements);
            int length = ClampEnd(span, start, trimElements);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlySpan<T> TrimStart<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : TrimStart(span, trimElements[0]);
            }

            int start = ClampStart(span, trimElements);
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlySpan<T> TrimEnd<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Use nested checks to avoid unnecessary branching for the unlikely case of N <= 1
            if (trimElements.Length <= 1)
            {
                return trimElements.Length == 0 ? span : TrimEnd(span, trimElements[0]);
            }

            int length = ClampEnd(span, 0, trimElements);
            return span.Slice(0, length);
        }

        /// <summary>
        /// Delimits all leading occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        private static int ClampStart<T>(ReadOnlySpan<T> span, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!trimElements.Contains(span[start]))
                {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        private static int ClampEnd<T>(ReadOnlySpan<T> span, int start, ReadOnlySpan<T> trimElements)
            where T : IEquatable<T>
        {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert((uint)start <= span.Length);

            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!trimElements.Contains(span[end]))
                {
                    break;
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        public static Memory<char> Trim(this Memory<char> memory)
        {
            Span<char> span = memory.Span;

            int start = ClampStart(span);
            int length = ClampEnd(span, start);

            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        public static Memory<char> TrimStart(this Memory<char> memory)
            => memory.Slice(ClampStart(memory.Span));

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        public static Memory<char> TrimEnd(this Memory<char> memory)
            => memory.Slice(0, ClampEnd(memory.Span, 0));

        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> memory)
        {
            ReadOnlySpan<char> span = memory.Span;

            int start = ClampStart(span);
            int length = ClampEnd(span, start);

            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> memory)
            => memory.Slice(ClampStart(memory.Span));

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> memory)
            => memory.Slice(0, ClampEnd(memory.Span, 0));

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span)
        {
            int start = ClampStart(span);
            int length = ClampEnd(span, start);

            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span)
            => span.Slice(ClampStart(span));

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span)
            => span.Slice(0, ClampEnd(span, 0));

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, char trimChar)
        {
            int start = ClampStart(span, trimChar);
            int length = ClampEnd(span, start, trimChar);

            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, char trimChar)
            => span.Slice(ClampStart(span, trimChar));

        /// <summary>
        /// Removes all trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, char trimChar)
            => span.Slice(0, ClampEnd(span, 0, trimChar));

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return span.Trim();
            }

            int start = ClampStart(span, trimChars);
            int length = ClampEnd(span, start, trimChars);

            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return span.TrimStart();
            }

            return span.Slice(ClampStart(span, trimChars));
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return span.TrimEnd();
            }

            return span.Slice(0, ClampEnd(span, 0, trimChars));
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        public static Span<char> Trim(this Span<char> span)
        {
            int start = ClampStart(span);
            int length = ClampEnd(span, start);

            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        public static Span<char> TrimStart(this Span<char> span)
            => span.Slice(ClampStart(span));

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        public static Span<char> TrimEnd(this Span<char> span)
            => span.Slice(0, ClampEnd(span, 0));

        /// <summary>
        /// Delimits all leading occurrences of whitespace.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampStart(ReadOnlySpan<char> span)
        {
            int start = 0;

            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of whitespace.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampEnd(ReadOnlySpan<char> span, int start)
        {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert((uint)start <= span.Length);

            int end = span.Length - 1;

            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                {
                    break;
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Delimits all leading occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimChar">The specified element to look for and remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampStart(ReadOnlySpan<char> span, char trimChar)
        {
            int start = 0;

            for (; start < span.Length; start++)
            {
                if (span[start] != trimChar)
                {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimChar">The specified element to look for and remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampEnd(ReadOnlySpan<char> span, int start, char trimChar)
        {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert((uint)start <= span.Length);

            int end = span.Length - 1;

            for (; end >= start; end--)
            {
                if (span[end] != trimChar)
                {
                    break;
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Delimits all leading occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimChars">The span which contains the set of elements to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampStart(ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!trimChars.Contains(span[start]))
                {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a specified element.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimChars">The span which contains the set of elements to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampEnd(ReadOnlySpan<char> span, int start, ReadOnlySpan<char> trimChars)
        {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert((uint)start <= span.Length);

            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!trimChars.Contains(span[end]))
                {
                    break;
                }
            }

            return end - start + 1;
        }
    }
}
