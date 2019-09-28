// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text
{
    internal ref partial struct ValueStringBuilder
    {
        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        public int Length
        {
            get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
            }
        }

        public int Capacity => _chars.Length;

        public void EnsureCapacity(int capacity)
        {
            if (capacity > _chars.Length)
                Grow(capacity - _pos);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
        /// the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
        }

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars => _chars;

        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
        public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
        }

        public void Insert(int index, string s)
        {
            int count = s.Length;

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s.AsSpan().CopyTo(_chars.Slice(index));
            _pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string s)
        {
            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }

            s.AsSpan().CopyTo(_chars.Slice(pos));
            _pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            Span<char> dst = _chars.Slice(_pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            _pos += count;
        }

        public unsafe void Append(char* value, int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }

            Span<char> dst = _chars.Slice(_pos, length);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = *value++;
            }
            _pos += length;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = _pos;
            if (origPos > _chars.Length - length)
            {
                Grow(length);
            }

            _pos = origPos + length;
            return _chars.Slice(origPos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        /// <summary>
        /// Resize the internal buffer either by doubling current buffer size or
        /// by adding <paramref name="additionalCapacityBeyondPos"/> to
        /// <see cref="_pos"/> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        /// Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            char[] poolArray = ArrayPool<char>.Shared.Rent(Math.Max(_pos + additionalCapacityBeyondPos, _chars.Length * 2));

            _chars.CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }


        public void AppendFormat(string format, object? arg0) => AppendFormatHelper(null, format, new ParamsArray(arg0));

        public void AppendFormat(string format, object? arg0, object? arg1) => AppendFormatHelper(null, format, new ParamsArray(arg0, arg1));

        public void AppendFormat(string format, object? arg0, object? arg1, object? arg2) => AppendFormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));

        public void AppendFormat(string format, params object?[] args)
        {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                string paramName = (format == null) ? nameof(format) : nameof(args);
                throw new ArgumentNullException(paramName);
            }

            AppendFormatHelper(null, format, new ParamsArray(args));
        }

        public void AppendFormat(IFormatProvider? provider, string format, object? arg0) => AppendFormatHelper(provider, format, new ParamsArray(arg0));

        public void AppendFormat(IFormatProvider? provider, string format, object? arg0, object? arg1) => AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1));

        public void AppendFormat(IFormatProvider? provider, string format, object? arg0, object? arg1, object? arg2) => AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));

        public void AppendFormat(IFormatProvider? provider, string format, params object?[] args)
        {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                string paramName = (format == null) ? nameof(format) : nameof(args);
                throw new ArgumentNullException(paramName);
            }

            AppendFormatHelper(provider, format, new ParamsArray(args));
        }

        // Copied from StringBuilder, can't be done via generic extension
        // as ValueStringBuilder is a ref struct and cannot be used in a generic.
        internal void AppendFormatHelper(IFormatProvider? provider, string format, ParamsArray args)
        {
            // Undocumented exclusive limits on the range for Argument Hole Index and Argument Hole Alignment.
            const int IndexLimit = 1000000; // Note:            0 <= ArgIndex < IndexLimit
            const int WidthLimit = 1000000; // Note:  -WidthLimit <  ArgAlign < WidthLimit

            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            int pos = 0;
            int len = format.Length;
            char ch = '\x0';

            ICustomFormatter? cf = null;
            if (provider != null)
            {
                cf = (ICustomFormatter?)provider.GetFormat(typeof(ICustomFormatter));
            }

            while (true)
            {
                while (pos < len)
                {
                    ch = format[pos];

                    pos++;
                    // Is it a closing brace?
                    if (ch == '}')
                    {
                        // Check next character (if there is one) to see if it is escaped. eg }}
                        if (pos < len && format[pos] == '}')
                        {
                            pos++;
                        }
                        else
                        {
                            // Otherwise treat it as an error (Mismatched closing brace)
                            FormatError();
                        }
                    }
                    // Is it a opening brace?
                    if (ch == '{')
                    {
                        // Check next character (if there is one) to see if it is escaped. eg {{
                        if (pos < len && format[pos] == '{')
                        {
                            pos++;
                        }
                        else
                        {
                            // Otherwise treat it as the opening brace of an Argument Hole.
                            pos--;
                            break;
                        }
                    }
                    // If it neither then treat the character as just text.
                    Append(ch);
                }

                //
                // Start of parsing of Argument Hole.
                // Argument Hole ::= { Index (, WS* Alignment WS*)? (: Formatting)? }
                //
                if (pos == len)
                {
                    break;
                }

                //
                //  Start of parsing required Index parameter.
                //  Index ::= ('0'-'9')+ WS*
                //
                pos++;
                // If reached end of text then error (Unexpected end of text)
                // or character is not a digit then error (Unexpected Character)
                if (pos == len || (ch = format[pos]) < '0' || ch > '9') FormatError();
                int index = 0;
                do
                {
                    index = index * 10 + ch - '0';
                    pos++;
                    // If reached end of text then error (Unexpected end of text)
                    if (pos == len)
                    {
                        FormatError();
                    }
                    ch = format[pos];
                    // so long as character is digit and value of the index is less than 1000000 ( index limit )
                }
                while (ch >= '0' && ch <= '9' && index < IndexLimit);

                // If value of index is not within the range of the arguments passed in then error (Index out of range)
                if (index >= args.Length)
                {
                    throw new FormatException(SR.Format_IndexOutOfRange);
                }

                // Consume optional whitespace.
                while (pos < len && (ch = format[pos]) == ' ') pos++;
                // End of parsing index parameter.

                //
                //  Start of parsing of optional Alignment
                //  Alignment ::= comma WS* minus? ('0'-'9')+ WS*
                //
                bool leftJustify = false;
                int width = 0;
                // Is the character a comma, which indicates the start of alignment parameter.
                if (ch == ',')
                {
                    pos++;

                    // Consume Optional whitespace
                    while (pos < len && format[pos] == ' ') pos++;

                    // If reached the end of the text then error (Unexpected end of text)
                    if (pos == len)
                    {
                        FormatError();
                    }

                    // Is there a minus sign?
                    ch = format[pos];
                    if (ch == '-')
                    {
                        // Yes, then alignment is left justified.
                        leftJustify = true;
                        pos++;
                        // If reached end of text then error (Unexpected end of text)
                        if (pos == len)
                        {
                            FormatError();
                        }
                        ch = format[pos];
                    }

                    // If current character is not a digit then error (Unexpected character)
                    if (ch < '0' || ch > '9')
                    {
                        FormatError();
                    }
                    // Parse alignment digits.
                    do
                    {
                        width = width * 10 + ch - '0';
                        pos++;
                        // If reached end of text then error. (Unexpected end of text)
                        if (pos == len)
                        {
                            FormatError();
                        }
                        ch = format[pos];
                        // So long a current character is a digit and the value of width is less than 100000 ( width limit )
                    }
                    while (ch >= '0' && ch <= '9' && width < WidthLimit);
                    // end of parsing Argument Alignment
                }

                // Consume optional whitespace
                while (pos < len && (ch = format[pos]) == ' ') pos++;

                //
                // Start of parsing of optional formatting parameter.
                //
                object? arg = args[index];

                ReadOnlySpan<char> itemFormatSpan = default; // used if itemFormat is null
                // Is current character a colon? which indicates start of formatting parameter.
                if (ch == ':')
                {
                    pos++;
                    int startPos = pos;

                    while (true)
                    {
                        // If reached end of text then error. (Unexpected end of text)
                        if (pos == len)
                        {
                            FormatError();
                        }
                        ch = format[pos];

                        if (ch == '}')
                        {
                            // Argument hole closed
                            break;
                        }
                        else if (ch == '{')
                        {
                            // Braces inside the argument hole are not supported
                            FormatError();
                        }

                        pos++;
                    }

                    if (pos > startPos)
                    {
                        itemFormatSpan = format.AsSpan(startPos, pos - startPos);
                    }
                }
                else if (ch != '}')
                {
                    // Unexpected character
                    FormatError();
                }

                // Construct the output for this arg hole.
                pos++;
                string? s = null;
                string? itemFormat = null;

                if (cf != null)
                {
                    if (itemFormatSpan.Length != 0)
                    {
                        itemFormat = new string(itemFormatSpan);
                    }
                    s = cf.Format(itemFormat, arg, provider);
                }

                if (s == null)
                {
                    // If arg is ISpanFormattable and the beginning doesn't need padding,
                    // try formatting it into the remaining current chunk.
                    if (arg is ISpanFormattable spanFormattableArg &&
                        (leftJustify || width == 0) &&
                        spanFormattableArg.TryFormat(_chars.Slice(_pos), out int charsWritten, itemFormatSpan, provider))
                    {
                        _pos += charsWritten;

                        // Pad the end, if needed.
                        int padding = width - charsWritten;
                        if (leftJustify && padding > 0)
                        {
                            Append(' ', padding);
                        }

                        // Continue to parse other characters.
                        continue;
                    }

                    // Otherwise, fallback to trying IFormattable or calling ToString.
                    if (arg is IFormattable formattableArg)
                    {
                        if (itemFormatSpan.Length != 0)
                        {
                            itemFormat ??= new string(itemFormatSpan);
                        }
                        s = formattableArg.ToString(itemFormat, provider);
                    }
                    else if (arg != null)
                    {
                        s = arg.ToString();
                    }
                }
                // Append it to the final output of the Format String.
                if (s == null)
                {
                    s = string.Empty;
                }
                int pad = width - s.Length;
                if (!leftJustify && pad > 0)
                {
                    Append(' ', pad);
                }

                Append(s);
                if (leftJustify && pad > 0)
                {
                    Append(' ', pad);
                }
                // Continue to parse other characters.
            }
        }

        private static void FormatError()
        {
            throw new FormatException(SR.Format_InvalidString);
        }
    }
}
