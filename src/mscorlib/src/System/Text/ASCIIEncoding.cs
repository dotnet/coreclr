// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;

    // ASCIIEncoding
    //
    // Note that ASCIIEncoding is optomized with no best fit and ? for fallback.
    // It doesn't come in other flavors.
    //
    // Note: ASCIIEncoding is the only encoding that doesn't do best fit (windows has best fit).
    //
    // Note: IsAlwaysNormalized remains false because 1/2 the code points are unassigned, so they'd
    //       use fallbacks, and we cannot guarantee that fallbacks are normalized.
    //

    [Serializable]
    [ComVisible(true)]
    public class ASCIIEncoding : Encoding
    {
        // Allow for devirtualization (see https://github.com/dotnet/coreclr/issues/1166#issuecomment-276251559)
        internal sealed class ASCIIEncodingSealed : ASCIIEncoding { }
        // Used by Encoding.ASCII for lazy initialization
        // The initialization code will not be run until a static member of the class is referenced
        internal static readonly ASCIIEncodingSealed s_default = new ASCIIEncodingSealed();

        public ASCIIEncoding() : base(Encoding.CodePageASCII)
        {
        }

        internal override void SetDefaultFallbacks()
        {
            // For ASCIIEncoding we just use default replacement fallback
            this.encoderFallback = EncoderFallback.ReplacementFallback;
            this.decoderFallback = DecoderFallback.ReplacementFallback;
        }

        // WARNING: GetByteCount(string chars), GetBytes(string chars,...), and GetString(byte[] byteIndex...)
        // WARNING: have different variable names than EncodingNLS.cs, so this can't just be cut & pasted,
        // WARNING: or it'll break VB's way of calling these.

        // NOTE: Many methods in this class forward to EncodingForwarder for
        // validating arguments/wrapping the unsafe methods in this class 
        // which do the actual work. That class contains
        // shared logic for doing this which is used by
        // ASCIIEncoding, EncodingNLS, UnicodeEncoding, UTF32Encoding,
        // UTF7Encoding, and UTF8Encoding.
        // The reason the code is separated out into a static class, rather
        // than a base class which overrides all of these methods for us
        // (which is what EncodingNLS is for internal Encodings) is because
        // that's really more of an implementation detail so it's internal.
        // At the same time, C# doesn't allow a public class subclassing an
        // internal/private one, so we end up having to re-override these
        // methods in all of the public Encodings + EncodingNLS.
        
        // Returns the number of bytes required to encode a range of characters in
        // a character array.

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return EncodingForwarder.GetByteCount(this, chars, index, count);
        }

        public override int GetByteCount(String chars)
        {
            return EncodingForwarder.GetByteCount(this, chars);
        }

        [CLSCompliant(false)]
        [ComVisible(false)]
        public override unsafe int GetByteCount(char* chars, int count)
        {
            return EncodingForwarder.GetByteCount(this, chars, count);
        }

        public override unsafe int GetBytes(String chars, int charIndex, int charCount,
                                              byte[] bytes, int byteIndex)
        {
            // Validate input parameters
            if (chars == null || bytes == null || charIndex < 0 || charCount < 0 ||
                (chars.Length - charIndex < charCount) ||
                (byteIndex < 0 || byteIndex > bytes.Length))
            {
                EncodingForwarder.ThrowValidationFailedException(this, chars, charIndex, charCount, bytes);
            }
            Contract.EndContractBlock();

            // Note that byteCount is the # of bytes to decode, not the size of the array
            int byteCount = bytes.Length - byteIndex;
            int bytesWritten;
            if (charCount > 0) 
            {
                if (byteCount == 0)
                {
                    // Definitely not enough space, early bail
                    EncodingForwarder.ThrowBytesOverflow(this);
                }
                fixed (char* pInput = chars)
                fixed (byte* pOutput = &bytes[0]) 
                {
                    char* input = pInput + charIndex;
                    byte* output = pOutput + byteIndex;
                    int charactersConsumed;
                    if (!TryEncode(input, charCount, output, byteCount, out charactersConsumed, out bytesWritten)) 
                    {
                        // Not all ASCII, GetBytesFallback for remaining conversion
                        bytesWritten += GetBytesFallback(input + charactersConsumed, charCount - charactersConsumed, output + bytesWritten, byteCount - bytesWritten, null);
                    }
                }
            } 
            else 
            {
                // Nothing to encode
                bytesWritten = 0;
            }

            return bytesWritten;
        }

        public override unsafe byte[] GetBytes(String s)
        {
            if (s == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s, ExceptionResource.ArgumentNull_String);
            Contract.EndContractBlock();

            int charCount = s.Length;

            byte[] bytes;
            if (charCount > 0)
            {
                fixed (char* input = s)
                    bytes = GetBytesValidated(input, charCount);
            }
            else
            {
                bytes = Array.Empty<byte>();
            }

            return bytes;
        }

        public override byte[] GetBytes(char[] chars)
        {
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            Contract.EndContractBlock();
            
            return GetBytesValidated(chars, 0, chars.Length);
        }

        public override byte[] GetBytes(char[] chars, int index, int count)
        {
            // Validate input parameters
            if (chars == null || index < 0 || count < 0 ||
                (chars.Length - index < count))
            {
                EncodingForwarder.ThrowValidationFailedException(chars, index, count);
            }
            Contract.EndContractBlock();
            
            return GetBytesValidated(chars, index, count);
        }

        private unsafe byte[] GetBytesValidated(char[] chars, int index, int count)
        {
            byte[] bytes;
            if (count > 0)
            {
                fixed (char* input = chars)
                {
                    bytes = GetBytesValidated(input + index, count);
                }
            }
            else
            {
                bytes = Array.Empty<byte>();
            }

            return bytes;
        }

        private unsafe byte[] GetBytesValidated(char* input, int charCount)
        {
            int remaining = 0;
            // Assume string is all ASCII and size array for that
            byte[] bytes = new byte[charCount];

            int bytesWritten;
            fixed (byte* output = &bytes[0]) 
            {
                int charactersConsumed;
                if (!TryEncode(input, charCount, output, charCount, out charactersConsumed, out bytesWritten)) 
                {
                    // Not all ASCII, get the byte count for the remaining encoded conversion
                    remaining = GetByteCount(input + charactersConsumed, charCount - charactersConsumed, null);
                }
            }

            if (remaining > 0) 
            {
                // Not all ASCII, fallback to slower path for remaining encoding
                var encoded = ResizeGetRemainingBytes(input, charCount, ref bytes, bytesWritten, remaining);
                Debug.Assert(encoded == remaining);
            }

            return bytes;
        }

        private unsafe int ResizeGetRemainingBytes(char* chars, int charCount, ref byte[] bytes, int alreadyEncoded, int remaining)
        {
            if (bytes.Length - remaining != alreadyEncoded) {
                // Resize the array to the correct size
                byte[] oldArray = bytes;
                bytes = new byte[alreadyEncoded + remaining];
                // Copy already encoded bytes
                Array.Copy(oldArray, 0, bytes, 0, alreadyEncoded);
            }
            int encoded;
            fixed (byte* output = &bytes[0]) 
            {
                // Use GetBytesFallback for remaining conversion
                encoded = GetBytesFallback(chars + alreadyEncoded, charCount - alreadyEncoded, output + alreadyEncoded, remaining, null);
            }

            return encoded;
        }
        
        // Encodes a range of characters in a character array into a range of bytes
        // in a byte array. An exception occurs if the byte array is not large
        // enough to hold the complete encoding of the characters. The
        // GetByteCount method can be used to determine the exact number of
        // bytes that will be produced for a given range of characters.
        // Alternatively, the GetMaxByteCount method can be used to
        // determine the maximum number of bytes that will be produced for a given
        // number of characters, regardless of the actual character values.
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, 
                                               byte[] bytes, int byteIndex)
        {
            // Validate input parameters
            if (chars == null || bytes == null || charIndex < 0 || charCount < 0 ||
                (chars.Length - charIndex < charCount) ||
                (byteIndex < 0 || byteIndex > bytes.Length))
            {
                EncodingForwarder.ThrowValidationFailedException(chars, charIndex, charCount, bytes);
            }
            Contract.EndContractBlock();

            // Note that byteCount is the # of bytes to decode, not the size of the array
            int byteCount = bytes.Length - byteIndex;
            int bytesWritten;
            if (charCount > 0) 
            {
                if (byteCount == 0)
                {
                    // Definitely not enough space, early bail
                    EncodingForwarder.ThrowBytesOverflow(this);
                }

                fixed (char* pInput = &chars[0])
                fixed (byte* pOutput = &bytes[0]) 
                {
                    char* input = pInput + charIndex;
                    byte* output = pOutput + byteIndex;
                    int charactersConsumed;
                    if (!TryEncode(input, charCount, output, byteCount, out charactersConsumed, out bytesWritten)) 
                    {
                        // Not all ASCII, GetBytesFallback for remaining conversion
                        bytesWritten += GetBytesFallback(input + charactersConsumed, charCount - charactersConsumed, output + bytesWritten, byteCount - bytesWritten, null);
                    }
                }
            } 
            else 
            {
                // Nothing to encode
                bytesWritten = 0;
            }

            return bytesWritten;
        }

        [CLSCompliant(false)]
        [ComVisible(false)]
        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            if ((bytes == null || chars == null) ||
                (charCount < 0 || byteCount < 0))
            {
                EncodingForwarder.ThrowValidationFailedException(chars, charCount, bytes);
            }
            Contract.EndContractBlock();

            int bytesWritten;
            if (charCount > 0)
            {
                if (byteCount == 0)
                {
                    // Definitely not enough space, early bail
                    EncodingForwarder.ThrowBytesOverflow(this);
                }
                int charactersConsumed;
                if (!TryEncode(chars, charCount, bytes, byteCount, out charactersConsumed, out bytesWritten))
                {
                    // Not all ASCII, GetBytesFallback for remaining conversion
                    bytesWritten += GetBytesFallback(chars + charactersConsumed, charCount - charactersConsumed, bytes + bytesWritten, byteCount - bytesWritten, null);
                }
            }
            else
            {
                // Nothing to encode
                bytesWritten = 0;
            }

            return bytesWritten;
        }

        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            // Just need to Assert, this is called by internal EncoderNLS and parameters should already be checked
            Debug.Assert(this != null);
            Debug.Assert(bytes != null);
            Debug.Assert(chars != null);
            Debug.Assert(charCount >= 0);
            Debug.Assert(byteCount >= 0);

            int bytesWritten;
            int charactersConsumed = 0;
            if (((encoder?.InternalHasFallbackBuffer ?? false) && 
                 (encoder.FallbackBuffer.Remaining > 0)) ||
                (charCount > byteCount))
            {
                // Data already in Fallback buffer, so straight to GetBytesFallback
                bytesWritten = GetBytesFallback(chars, charCount, bytes, byteCount, encoder);
            } 
            else if (charCount > 0)
            {
                if (byteCount == 0)
                {
                    // Definitely not enough space, early bail
                    EncodingForwarder.ThrowBytesOverflow(this);
                }
                if (!TryEncode(chars, charCount, bytes, byteCount, out charactersConsumed, out bytesWritten))
                {
                    // Not all ASCII, use GetBytesFallback for remaining conversion
                    bytesWritten += GetBytesFallback(chars + charactersConsumed, charCount - charactersConsumed, bytes + bytesWritten, byteCount - bytesWritten, encoder);
                }
            }
            else
            {
                // Nothing to encode
                bytesWritten = 0;
            }

            if (encoder != null)
            {
                encoder.m_charsUsed += charactersConsumed;
            }
            
            return bytesWritten;
        }

        private unsafe static bool TryEncode(char* chars, int charCount, byte* bytes, int byteCount, out int charactersConsumed, out int bytesWritten)
        {
            const int Shift16Shift24 = (1 << 16) | (1 << 24);
            const int Shift8Identity = (1 << 8) | (1);

            int charsToEncode = Math.Min(charCount, byteCount);

            // Encode as bytes upto the first non-ASCII byte and return count encoded
            int i = 0;
#if BIT64 && !BIGENDIAN
            if (charsToEncode < 4) goto trailing;

            int unaligned = (int)(((ulong)chars) & 0x7) >> 1;
            // Unaligned chars
            for (; i < unaligned; i++)
            {
                char ch = *(chars + i);
                if (ch > 0x7F)
                {
                    goto exit; // Found non-ASCII, bail
                }
                else
                {
                    *(bytes + i) = (byte)ch; // Cast convert
                }
            }

            // Aligned
            int ulongDoubleCount = (charsToEncode - i) & ~0x7;
            for (; i < ulongDoubleCount; i += 8)
            {
                ulong inputUlong0 = *(ulong*)(chars + i);
                ulong inputUlong1 = *(ulong*)(chars + i + 4);
                if (((inputUlong0 | inputUlong1) & 0xFF80FF80FF80FF80) != 0)
                {
                    goto exit; // Found non-ASCII, bail
                }
                // Pack 16 ASCII chars into 16 bytes
                *(uint*)(bytes + i) =
                    ((uint)((inputUlong0 * Shift16Shift24) >> 24) & 0xffff) |
                    ((uint)((inputUlong0 * Shift8Identity) >> 24) & 0xffff0000);
                *(uint*)(bytes + i + 4) =
                    ((uint)((inputUlong1 * Shift16Shift24) >> 24) & 0xffff) |
                    ((uint)((inputUlong1 * Shift8Identity) >> 24) & 0xffff0000);
            }
            if (charsToEncode - 4 > i)
            {
                ulong inputUlong = *(ulong*)(chars + i);
                if ((inputUlong & 0xFF80FF80FF80FF80) != 0)
                {
                    goto exit; // Found non-ASCII, bail
                }
                // Pack 8 ASCII chars into 8 bytes
                *(uint*)(bytes + i) =
                    ((uint)((inputUlong * Shift16Shift24) >> 24) & 0xffff) |
                    ((uint)((inputUlong * Shift8Identity) >> 24) & 0xffff0000);
                i += 4;
            }

        trailing:
            for (; i < charsToEncode; i++)
            {
                char ch = *(chars + i);
                if (ch > 0x7F)
                {
                    goto exit; // Found non-ASCII, bail
                }
                else
                {
                    *(bytes + i) = (byte)ch; // Cast convert
                }
            }
#else
            // Unaligned chars
            if ((unchecked((int)chars) & 0x2) != 0) 
            {
                char ch = *chars;
                if (ch > 0x7F) 
                {
                    goto exit; // Found non-ASCII, bail
                } 
                else 
                {
                    i = 1;
                    *(bytes) = (byte)ch; // Cast convert
                }
            }

            // Aligned
            int uintCount = (charsToEncode - i) & ~0x3;
            for (; i < uintCount; i += 4) 
            {
                uint inputUint0 = *(uint*)(chars + i);
                uint inputUint1 = *(uint*)(chars + i + 2);
                if (((inputUint0 | inputUint1) & 0xFF80FF80) != 0) 
                {
                    goto exit; // Found non-ASCII, bail
                }
                // Pack 4 ASCII chars into 4 bytes
#if BIGENDIAN
                *(bytes + i) = (byte)(inputUint0 >> 16);
                *(bytes + i + 1) = (byte)inputUint0;
                *(bytes + i + 2) = (byte)(inputUint1 >> 16);
                *(bytes + i + 3) = (byte)inputUint1;
#else // BIGENDIAN
                *(ushort*)(bytes + i) = (ushort)(inputUint0 | (inputUint0 >> 8));
                *(ushort*)(bytes + i + 2) = (ushort)(inputUint1 | (inputUint1 >> 8));
#endif // BIGENDIAN
            }
            if (charsToEncode - 1 > i) 
            {
                uint inputUint = *(uint*)(chars + i);
                if ((inputUint & 0xFF80FF80) != 0) 
                {
                    goto exit; // Found non-ASCII, bail
                }
#if BIGENDIAN
                *(bytes + i) = (byte)(inputUint0 >> 16);
                *(bytes + i + 1) = (byte)inputUint0;
#else // BIGENDIAN
                // Pack 2 ASCII chars into 2 bytes
                *(ushort*)(bytes + i) = (ushort)(inputUint | (inputUint >> 8));
#endif // BIGENDIAN
                i += 2;
            }

            if (i < charsToEncode) 
            {
                char ch = *(chars + i);
                if (ch > 0x7F) 
                {
                    goto exit; // Found non-ASCII, bail
                }
                else 
                {
#if BIGENDIAN
                    *(bytes + i) = (byte)(ch >> 16);
#else // BIGENDIAN
                    *(bytes + i) = (byte)ch; // Cast convert
#endif // BIGENDIAN
                    i = charsToEncode;
                }
            }
#endif // BIT64
        exit:
            bytesWritten = i;
            charactersConsumed = i;
            return charCount == charactersConsumed;
        }

        // Returns the number of characters produced by decoding a range of bytes
        // in a byte array.

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return EncodingForwarder.GetCharCount(this, bytes, index, count);
        }

        [CLSCompliant(false)]
        [ComVisible(false)]
        public override unsafe int GetCharCount(byte* bytes, int count)
        {
            return EncodingForwarder.GetCharCount(this, bytes, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
                                              char[] chars, int charIndex)
        {
            return EncodingForwarder.GetChars(this, bytes, byteIndex, byteCount, chars, charIndex);
        }

        [CLSCompliant(false)]
        [ComVisible(false)]
        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            return EncodingForwarder.GetChars(this, bytes, byteCount, chars, charCount);
        }

        // Returns a string containing the decoded representation of a range of
        // bytes in a byte array.

        public override String GetString(byte[] bytes, int byteIndex, int byteCount)
        {
            return EncodingForwarder.GetString(this, bytes, byteIndex, byteCount);
        }
        
        // End of overridden methods which use EncodingForwarder

        // GetByteCount
        // Note: We start by assuming that the output will be the same as count.  Having
        // an encoder or fallback may change that assumption
        internal override unsafe int GetByteCount(char* chars, int charCount, EncoderNLS encoder)
        {
            // Just need to ASSERT, this is called by something else internal that checked parameters already
            Debug.Assert(charCount >= 0, "[ASCIIEncoding.GetByteCount]count is negative");
            Debug.Assert(chars != null, "[ASCIIEncoding.GetByteCount]chars is null");

            // Assert because we shouldn't be able to have a null encoder.
            Debug.Assert(encoderFallback != null, "[ASCIIEncoding.GetByteCount]Attempting to use null fallback encoder");

            char charLeftOver = (char)0;
            EncoderReplacementFallback fallback = null;

            // Start by assuming default count, then +/- for fallback characters
            char* charEnd = chars + charCount;

            // For fallback we may need a fallback buffer, we know we aren't default fallback.
            EncoderFallbackBuffer fallbackBuffer = null;

            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                Debug.Assert(charLeftOver == 0 || Char.IsHighSurrogate(charLeftOver),
                    "[ASCIIEncoding.GetByteCount]leftover character should be high surrogate");

                fallback = encoder.Fallback as EncoderReplacementFallback;

                // We mustn't have left over fallback data when counting
                if (encoder.InternalHasFallbackBuffer)
                {
                    // We always need the fallback buffer in get bytes so we can flush any remaining ones if necessary
                    fallbackBuffer = encoder.FallbackBuffer;
                    if (fallbackBuffer.Remaining > 0 && encoder.m_throwOnOverflow)
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty",
                        this.EncodingName, encoder.Fallback.GetType()));

                    // Set our internal fallback interesting things.
                    fallbackBuffer.InternalInitialize(chars, charEnd, encoder, false);
                }

                // Verify that we have no fallbackbuffer, for ASCII its always empty, so just assert
                Debug.Assert(!encoder.m_throwOnOverflow || !encoder.InternalHasFallbackBuffer ||
                    encoder.FallbackBuffer.Remaining == 0,
                    "[ASCIICodePageEncoding.GetByteCount]Expected empty fallback buffer");
//                if (encoder.InternalHasFallbackBuffer && encoder.FallbackBuffer.Remaining > 0)
//                    throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty",
//                    this.EncodingName, encoder.Fallback.GetType()));
            }
            else
            {
                fallback = this.EncoderFallback as EncoderReplacementFallback;
            }

            // If we have an encoder AND we aren't using default fallback,
            // then we may have a complicated count.
            if (fallback != null && fallback.MaxCharCount == 1)
            {
                // Replacement fallback encodes surrogate pairs as two ?? (or two whatever), so return size is always
                // same as input size.
                // Note that no existing SBCS code pages map code points to supplimentary characters, so this is easy.

                // We could however have 1 extra byte if the last call had an encoder and a funky fallback and
                // if we don't use the funky fallback this time.

                // Do we have an extra char left over from last time?
                if (charLeftOver > 0)
                    charCount++;

                return (charCount);
            }

            // Count is more complicated if you have a funky fallback
            // For fallback we may need a fallback buffer, we know we're not default fallback
            int byteCount = 0;

            // We may have a left over character from last time, try and process it.
            if (charLeftOver > 0)
            {
                Debug.Assert(Char.IsHighSurrogate(charLeftOver), "[ASCIIEncoding.GetByteCount]leftover character should be high surrogate");
                Debug.Assert(encoder != null, "[ASCIIEncoding.GetByteCount]Expected encoder");

                // Since left over char was a surrogate, it'll have to be fallen back.
                // Get Fallback
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, false);

                // This will fallback a pair if *chars is a low surrogate
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
            }

            // Now we may have fallback char[] already from the encoder

            // Go ahead and do it, including the fallback.
            char ch;
            while ((ch = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != 0 ||
                    chars < charEnd)
            {

                // First unwind any fallback
                if (ch == 0)
                {
                    // No fallback, just get next char
                    ch = *chars;
                    chars++;
                }

                // Check for fallback, this'll catch surrogate pairs too.
                // no chars >= 0x80 are allowed.
                if (ch > 0x7f)
                {
                    if (fallbackBuffer == null)
                    {
                        // Initialize the buffer
                        if (encoder == null)
                            fallbackBuffer = this.encoderFallback.CreateFallbackBuffer();
                        else
                            fallbackBuffer = encoder.FallbackBuffer;
                        fallbackBuffer.InternalInitialize(charEnd - charCount, charEnd, encoder, false);
                    }

                    // Get Fallback
                    fallbackBuffer.InternalFallback(ch, ref chars);
                    continue;
                }

                // We'll use this one
                byteCount++;
            }

            Debug.Assert(fallbackBuffer == null || fallbackBuffer.Remaining == 0,
                "[ASCIIEncoding.GetByteCount]Expected Empty fallback buffer");

            return byteCount;
        }

        private unsafe int GetBytesFallback(char* chars, int charCount,
                                                byte* bytes, int byteCount, EncoderNLS encoder)
        {
            // Just need to ASSERT, this is called by something else internal that checked parameters already
            Debug.Assert(bytes != null, "[ASCIIEncoding.GetBytes]bytes is null");
            Debug.Assert(byteCount >= 0, "[ASCIIEncoding.GetBytes]byteCount is negative");
            Debug.Assert(chars != null, "[ASCIIEncoding.GetBytes]chars is null");
            Debug.Assert(charCount >= 0, "[ASCIIEncoding.GetBytes]charCount is negative");

            // Assert because we shouldn't be able to have a null encoder.
            Debug.Assert(encoderFallback != null, "[ASCIIEncoding.GetBytes]Attempting to use null encoder fallback");

            // Get any left over characters
            char charLeftOver = (char)0;
            EncoderReplacementFallback fallback = null;

            // For fallback we may need a fallback buffer, we know we aren't default fallback.
            EncoderFallbackBuffer fallbackBuffer = null;

            // prepare our end
            char* charEnd = chars + charCount;
            byte* byteStart = bytes;
            char* charStart = chars;

            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                fallback = encoder.Fallback as EncoderReplacementFallback;

                // We mustn't have left over fallback data when counting
                if (encoder.InternalHasFallbackBuffer)
                {
                    // We always need the fallback buffer in get bytes so we can flush any remaining ones if necessary
                    fallbackBuffer = encoder.FallbackBuffer;
                    if (fallbackBuffer.Remaining > 0 && encoder.m_throwOnOverflow)
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty",
                        this.EncodingName, encoder.Fallback.GetType()));

                    // Set our internal fallback interesting things.
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                }

                Debug.Assert(charLeftOver == 0 || Char.IsHighSurrogate(charLeftOver),
                    "[ASCIIEncoding.GetBytes]leftover character should be high surrogate");

                // Verify that we have no fallbackbuffer, for ASCII its always empty, so just assert
                Debug.Assert(!encoder.m_throwOnOverflow || !encoder.InternalHasFallbackBuffer ||
                    encoder.FallbackBuffer.Remaining == 0,
                    "[ASCIICodePageEncoding.GetBytes]Expected empty fallback buffer");
//                if (encoder.m_throwOnOverflow && encoder.InternalHasFallbackBuffer &&
//                  encoder.FallbackBuffer.Remaining > 0)
//                  throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty",
//                        this.EncodingName, encoder.Fallback.GetType()));
            }
            else
            {
                fallback = this.EncoderFallback as EncoderReplacementFallback;
            }


            // See if we do the fast default or slightly slower fallback
            if (fallback != null && fallback.MaxCharCount == 1)
            {
                // Fast version
                char cReplacement = fallback.DefaultString[0];

                // Check for replacements in range, otherwise fall back to slow version.
                if (cReplacement <= (char)0x7f)
                {
                    // We should have exactly as many output bytes as input bytes, unless there's a left
                    // over character, in which case we may need one more.
                    // If we had a left over character will have to add a ?  (This happens if they had a funky
                    // fallback last time, but not this time.) (We can't spit any out though
                    // because with fallback encoder each surrogate is treated as a seperate code point)
                    if (charLeftOver > 0)
                    {
                        // Have to have room
                        // Throw even if doing no throw version because this is just 1 char,
                        // so buffer will never be big enough
                        if (byteCount == 0)
                            ThrowBytesOverflow(encoder, true);

                        // This'll make sure we still have more room and also make sure our return value is correct.
                        *(bytes++) = (byte)cReplacement;
                        byteCount--;                // We used one of the ones we were counting.
                    }

                    // This keeps us from overrunning our output buffer
                    if (byteCount < charCount)
                    {
                        // Throw or make buffer smaller?
                        ThrowBytesOverflow(encoder, byteCount < 1);

                        // Just use what we can
                        charEnd = chars + byteCount;
                    }

                    // We just do a quick copy
                    while (chars < charEnd)
                    {
                        char ch2 = *(chars++);
                        if (ch2 >= 0x0080) *(bytes++) = (byte)cReplacement;
                        else *(bytes++) = unchecked((byte)(ch2));
                    }

                    // Clear encoder
                    if (encoder != null)
                    {
                        encoder.charLeftOver = (char)0;
                        encoder.m_charsUsed = (int)(chars-charStart);
                    }

                    return (int)(bytes - byteStart);
                }
            }

            // Slower version, have to do real fallback.

            // prepare our end
            byte* byteEnd = bytes + byteCount;

            // We may have a left over character from last time, try and process it.
            if (charLeftOver > 0)
            {
                // Initialize the buffer
                Debug.Assert(encoder != null,
                    "[ASCIIEncoding.GetBytes]Expected non null encoder if we have surrogate left over");
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, true);

                // Since left over char was a surrogate, it'll have to be fallen back.
                // Get Fallback
                // This will fallback a pair if *chars is a low surrogate
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
            }

            // Now we may have fallback char[] already from the encoder

            // Go ahead and do it, including the fallback.
            char ch;
            while ((ch = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != 0 ||
                    chars < charEnd)
            {
                // First unwind any fallback
                if (ch == 0)
                {
                    // No fallback, just get next char
                    ch = *chars;
                    chars++;
                }

                // Check for fallback, this'll catch surrogate pairs too.
                // All characters >= 0x80 must fall back.
                if (ch > 0x7f)
                {
                    // Initialize the buffer
                    if (fallbackBuffer == null)
                    {
                        if (encoder == null)
                            fallbackBuffer = this.encoderFallback.CreateFallbackBuffer();
                        else
                            fallbackBuffer = encoder.FallbackBuffer;
                        fallbackBuffer.InternalInitialize(charEnd - charCount, charEnd, encoder, true);
                    }

                    // Get Fallback
                    fallbackBuffer.InternalFallback(ch, ref chars);

                    // Go ahead & continue (& do the fallback)
                    continue;
                }

                // We'll use this one
                // Bounds check
                if (bytes >= byteEnd)
                {
                    // didn't use this char, we'll throw or use buffer
                    if (fallbackBuffer == null || fallbackBuffer.bFallingBack == false)
                    {
                        Debug.Assert(chars > charStart || bytes == byteStart,
                            "[ASCIIEncoding.GetBytes]Expected chars to have advanced already.");
                        chars--;                                        // don't use last char
                    }
                    else
                        fallbackBuffer.MovePrevious();

                    // Are we throwing or using buffer?
                    ThrowBytesOverflow(encoder, bytes == byteStart);    // throw?
                    break;                                              // don't throw, stop
                }

                // Go ahead and add it
                *bytes = unchecked((byte)ch);
                bytes++;
            }

            // Need to do encoder stuff
            if (encoder != null)
            {
                // Fallback stuck it in encoder if necessary, but we have to clear MustFlush cases
                if (fallbackBuffer != null && !fallbackBuffer.bUsedEncoder)
                    // Clear it in case of MustFlush
                    encoder.charLeftOver = (char)0;

                // Set our chars used count
                encoder.m_charsUsed = (int)(chars - charStart);
            }

            Debug.Assert(fallbackBuffer == null || fallbackBuffer.Remaining == 0 ||
                (encoder != null && !encoder.m_throwOnOverflow ),
                "[ASCIIEncoding.GetBytes]Expected Empty fallback buffer at end");

            return (int)(bytes - byteStart);
        }

        // This is internal and called by something else,
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
        {
            // Just assert, we're called internally so these should be safe, checked already
            Debug.Assert(bytes != null, "[ASCIIEncoding.GetCharCount]bytes is null");
            Debug.Assert(count >= 0, "[ASCIIEncoding.GetCharCount]byteCount is negative");

            // ASCII doesn't do best fit, so don't have to check for it, find out which decoder fallback we're using
            DecoderReplacementFallback fallback = null;

            if (decoder == null)
                fallback = this.DecoderFallback as DecoderReplacementFallback;
            else
            {
                fallback = decoder.Fallback as DecoderReplacementFallback;
                Debug.Assert(!decoder.m_throwOnOverflow || !decoder.InternalHasFallbackBuffer ||
                    decoder.FallbackBuffer.Remaining == 0,
                    "[ASCIICodePageEncoding.GetCharCount]Expected empty fallback buffer");
            }

            if (fallback != null && fallback.MaxCharCount == 1)
            {
                // Just return length, SBCS stay the same length because they don't map to surrogate
                // pairs and we don't have a decoder fallback.

                return count;
            }

            // Only need decoder fallback buffer if not using default replacement fallback, no best fit for ASCII
            DecoderFallbackBuffer fallbackBuffer = null;

            // Have to do it the hard way.
            // Assume charCount will be == count
            int charCount = count;
            byte[] byteBuffer = new byte[1];

            // Do it our fast way
            byte* byteEnd = bytes + count;

            // Quick loop
            while (bytes < byteEnd)
            {
                // Faster if don't use *bytes++;
                byte b = *bytes;
                bytes++;

                // If unknown we have to do fallback count
                if (b >= 0x80)
                {
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                            fallbackBuffer = this.DecoderFallback.CreateFallbackBuffer();
                        else
                            fallbackBuffer = decoder.FallbackBuffer;
                        fallbackBuffer.InternalInitialize(byteEnd - count, null);
                    }

                    // Use fallback buffer
                    byteBuffer[0] = b;
                    charCount--;            // Have to unreserve the one we already allocated for b
                    charCount += fallbackBuffer.InternalFallback(byteBuffer, bytes);
                }
            }

            // Fallback buffer must be empty
            Debug.Assert(fallbackBuffer == null || fallbackBuffer.Remaining == 0,
                "[ASCIIEncoding.GetCharCount]Expected Empty fallback buffer");

            // Converted sequence is same length as input
            return charCount;
        }

        internal override unsafe int GetChars(byte* bytes, int byteCount,
                                                char* chars, int charCount, DecoderNLS decoder)
        {
            // Just need to ASSERT, this is called by something else internal that checked parameters already
            Debug.Assert(bytes != null, "[ASCIIEncoding.GetChars]bytes is null");
            Debug.Assert(byteCount >= 0, "[ASCIIEncoding.GetChars]byteCount is negative");
            Debug.Assert(chars != null, "[ASCIIEncoding.GetChars]chars is null");
            Debug.Assert(charCount >= 0, "[ASCIIEncoding.GetChars]charCount is negative");

            // Do it fast way if using ? replacement fallback
            byte* byteEnd = bytes + byteCount;
            byte* byteStart = bytes;
            char* charStart = chars;

            // Note: ASCII doesn't do best fit, but we have to fallback if they use something > 0x7f
            // Only need decoder fallback buffer if not using ? fallback.
            // ASCII doesn't do best fit, so don't have to check for it, find out which decoder fallback we're using
            DecoderReplacementFallback fallback = null;

            if (decoder == null)
                fallback = this.DecoderFallback as DecoderReplacementFallback;
            else
            {
                fallback = decoder.Fallback as DecoderReplacementFallback;
                Debug.Assert(!decoder.m_throwOnOverflow || !decoder.InternalHasFallbackBuffer ||
                    decoder.FallbackBuffer.Remaining == 0,
                    "[ASCIICodePageEncoding.GetChars]Expected empty fallback buffer");
            }

            if (fallback != null && fallback.MaxCharCount == 1)
            {
                // Try it the fast way
                char replacementChar = fallback.DefaultString[0];

                // Need byteCount chars, otherwise too small buffer
                if (charCount < byteCount)
                {
                    // Need at least 1 output byte, throw if must throw
                    ThrowCharsOverflow(decoder, charCount < 1);

                    // Not throwing, use what we can
                    byteEnd = bytes + charCount;
                }

                // Quick loop, just do '?' replacement because we don't have fallbacks for decodings.
                while (bytes < byteEnd)
                {
                    byte b = *(bytes++);
                    if (b >= 0x80)
                        // This is an invalid byte in the ASCII encoding.
                        *(chars++) = replacementChar;
                    else
                        *(chars++) = unchecked((char)b);
                }

                // bytes & chars used are the same
                if (decoder != null)
                    decoder.m_bytesUsed = (int)(bytes - byteStart);
                return (int)(chars - charStart);
            }

            // Slower way's going to need a fallback buffer
            DecoderFallbackBuffer fallbackBuffer = null;
            byte[] byteBuffer = new byte[1];
            char*   charEnd = chars + charCount;

            // Not quite so fast loop
            while (bytes < byteEnd)
            {
                // Faster if don't use *bytes++;
                byte b = *(bytes);
                bytes++;

                if (b >= 0x80)
                {
                    // This is an invalid byte in the ASCII encoding.
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                            fallbackBuffer = this.DecoderFallback.CreateFallbackBuffer();
                        else
                            fallbackBuffer = decoder.FallbackBuffer;
                        fallbackBuffer.InternalInitialize(byteEnd - byteCount, charEnd);
                    }

                    // Use fallback buffer
                    byteBuffer[0] = b;

                    // Note that chars won't get updated unless this succeeds
                    if (!fallbackBuffer.InternalFallback(byteBuffer, bytes, ref chars))
                    {
                        // May or may not throw, but we didn't get this byte
                        Debug.Assert(bytes > byteStart || chars == charStart,
                            "[ASCIIEncoding.GetChars]Expected bytes to have advanced already (fallback case)");
                        bytes--;                                            // unused byte
                        fallbackBuffer.InternalReset();                     // Didn't fall this back
                        ThrowCharsOverflow(decoder, chars == charStart);    // throw?
                        break;                                              // don't throw, but stop loop
                    }
                }
                else
                {
                    // Make sure we have buffer space
                    if (chars >= charEnd)
                    {
                        Debug.Assert(bytes > byteStart || chars == charStart,
                            "[ASCIIEncoding.GetChars]Expected bytes to have advanced already (normal case)");
                        bytes--;                                            // unused byte
                        ThrowCharsOverflow(decoder, chars == charStart);    // throw?
                        break;                                              // don't throw, but stop loop
                    }

                    *(chars) = unchecked((char)b);
                    chars++;
                }
            }

            // Might have had decoder fallback stuff.
            if (decoder != null)
                decoder.m_bytesUsed = (int)(bytes - byteStart);

            // Expect Empty fallback buffer for GetChars
            Debug.Assert(fallbackBuffer == null || fallbackBuffer.Remaining == 0,
                "[ASCIIEncoding.GetChars]Expected Empty fallback buffer");

            return (int)(chars - charStart);
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Contract.EndContractBlock();

            // Characters would be # of characters + 1 in case high surrogate is ? * max fallback
            long byteCount = (long)charCount + 1;

            if (EncoderFallback.MaxCharCount > 1)
                byteCount *= EncoderFallback.MaxCharCount;

            // 1 to 1 for most characters.  Only surrogates with fallbacks have less.

            if (byteCount > 0x7fffffff)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_GetByteCountOverflow);

            return (int)byteCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Contract.EndContractBlock();

            // Just return length, SBCS stay the same length because they don't map to surrogate
            long charCount = (long)byteCount;

            // 1 to 1 for most characters.  Only surrogates with fallbacks have less, unknown fallbacks could be longer.
            if (DecoderFallback.MaxCharCount > 1)
                charCount *= DecoderFallback.MaxCharCount;

            if (charCount > 0x7fffffff)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_GetCharCountOverflow);

            return (int)charCount;
        }

        // True if and only if the encoding only uses single byte code points.  (Ie, ASCII, 1252, etc)

        [ComVisible(false)]
        public override bool IsSingleByte => true;

        [ComVisible(false)]
        public override Decoder GetDecoder()
        {
            return new DecoderNLS(this);
        }


        [ComVisible(false)]
        public override Encoder GetEncoder()
        {
            return new EncoderNLS(this);
        }
    }
}
