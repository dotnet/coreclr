// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    partial class MemoryExtensions // .Trim
    {
        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        public static Memory<char> Trim(this Memory<char> memory)
        {
            Span<char> span = memory.Span;

            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return memory.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        public static Memory<char> TrimStart(this Memory<char> memory)
        {
            Span<char> span = memory.Span;

            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        public static Memory<char> TrimEnd(this Memory<char> memory)
        {
            Span<char> span = memory.Span;

            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return memory.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> memory)
        {
            ReadOnlySpan<char> span = memory.Span;

            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return memory.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> memory)
        {
            ReadOnlySpan<char> span = memory.Span;

            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            return memory.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> memory)
        {
            ReadOnlySpan<char> span = memory.Span;

            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return memory.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }
            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }
            return span.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, char trimChar)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (span[start] != trimChar)
                    break;
            }
            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (span[end] != trimChar)
                    break;
            }
            return span.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, char trimChar)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (span[start] != trimChar)
                    break;
            }
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, char trimChar)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (span[end] != trimChar)
                    break;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            return span.TrimStart(trimChars).TrimEnd(trimChars);
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

            int start = 0;
            for (; start < span.Length; start++)
            {
                for (int i = 0; i < trimChars.Length; i++)
                {
                    if (span[start] == trimChars[i])
                        goto Next;
                }
                break;
            Next:
                ;
            }
            return span.Slice(start);
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

            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                for (int i = 0; i < trimChars.Length; i++)
                {
                    if (span[end] == trimChars[i])
                        goto Next;
                }
                break;
            Next:
                ;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        public static Span<char> Trim(this Span<char> span)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            int end = span.Length - 1;
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return span.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        public static Span<char> TrimStart(this Span<char> span)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }

            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        public static Span<char> TrimEnd(this Span<char> span)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }

            return span.Slice(0, end + 1);
        }
    }
}
