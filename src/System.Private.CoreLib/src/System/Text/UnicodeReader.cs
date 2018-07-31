// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    /// <summary>
    /// Provides low-level methods for reading data directly from Unicode strings.
    /// </summary>
    internal static class UnicodeReader
    {
        public static (SequenceValidity status, UnicodeScalar scalar, int charsConsumed) PeekFirstScalarUtf16(ReadOnlySpan<char> buffer)
        {
            if (buffer.IsEmpty)
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 0);
            }

            // First, check for a single UTF-16 code unit (non-surrogate).

            uint firstChar = buffer[0];
            if (!UnicodeHelpers.IsSurrogateCodePoint(firstChar))
            {
                return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(firstChar), charsConsumed: 1);
            }

            // The first code unit is a surrogate (hasn't yet been determined if high or low).
            // Need to read the second code unit to see if this is a valid surrogate pair.

            if (buffer.Length == 1)
            {
                goto BufferContainsOnlySingleChar;
            }

            uint secondChar = buffer[1];
            if (!UnicodeHelpers.IsHighSurrogateCodePoint(firstChar) || !UnicodeHelpers.IsLowSurrogateCodePoint(secondChar))
            {
                goto InvalidSurrogateSequence;
            }

            // Valid surrogate pair!

            return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(UnicodeHelpers.GetScalarFromUtf16SurrogatePair(firstChar, secondChar)), charsConsumed: 2);

        BufferContainsOnlySingleChar:

            // A high surrogate at the end of the buffer is incomplete. Signal to the caller that we're waiting for more data.

            if (UnicodeHelpers.IsHighSurrogateCodePoint(firstChar))
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 1);
            }

        InvalidSurrogateSequence:

            // At this point, we have either a high surrogate followed by something other than a low surrogate, or we have
            // a low surrogate not preceded by a high surrogate. In either case this is an invalid sequence of length 1.

            return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 1);
        }

        public static (SequenceValidity status, UnicodeScalar scalar, int charsConsumed) PeekFirstScalarUtf8(ReadOnlySpan<byte> buffer)
        {
            // This method is implemented to match the behavior of System.Text.Encoding.UTF8 in terms of
            // how many bytes it consumes when reporting invalid sequences. The behavior is as follows:
            //
            // - Some bytes are *always* invalid (ranges [ C0..C1 ] and [ F5..FF ]), and when these
            //   are encountered it's an invalid sequence of length 1.
            //
            // - Standalone continuation bytes (when the beginning of a sequence is expected) are treated
            //   as an invalid sequence of length 1.
            //
            // - Multi-byte sequences which are overlong are reported as an invalid sequence of length 2,
            //   since per the Unicode Standard Table 3-7 it's always possible to tell these by the second byte.
            //   Exception: Sequences which begin with [ C0..C1 ] are covered by the above case, thus length 1.
            //
            // - Multi-byte sequences which are improperly terminated (no continuation byte when one is
            //   expected) are reported as invalid sequences up to and including the last seen continuation byte.

            if (buffer.Length == 0)
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 0);
            }

            // First, check for ASCII.

            uint currentScalar = buffer[0];

            if (UnicodeHelpers.IsAsciiCodePoint(currentScalar))
            {
                return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(currentScalar), charsConsumed: 1);
            }

            // Not ASCII, go down multi-byte sequence path.
            // This is optimized for the success case; failure cases are handled at the bottom of the method.

            // Check for 2-byte sequence.

            if (buffer.Length < 2)
            {
                goto Error; // out of data
            }

            uint nextByte = buffer[1]; // optimistically assume for now it's a valid continuation byte
            currentScalar = (currentScalar << 6) + nextByte - 0x80U /* remove continuation byte marker */ - 0x3000U /* remove first byte header */ ;

            if (UnicodeHelpers.IsInRangeInclusive(currentScalar, 0x80U, 0x7FFU) && UnicodeHelpers.IsUtf8ContinuationByte(nextByte))
            {
                // Valid 2-byte sequence.
                return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(currentScalar), charsConsumed: 2);
            }

            // Check for 3-byte sequence.

            if (buffer.Length < 3)
            {
                goto Error; // out of data
            }

            uint continuationByteAccumulator = nextByte - 0x80U; // bits 6 and 7 should never be set
            nextByte = (uint)buffer[2] - 0x80U; // optimistically assume for now it's a valid continuation byte
            continuationByteAccumulator |= nextByte;
            currentScalar = (currentScalar << 6) + nextByte + 0xC0000U - 0xE0000U /* fix first byte header */;

            if (UnicodeHelpers.IsInRangeInclusive(currentScalar, 0x800U, 0xFFFFU) && !UnicodeHelpers.IsSurrogateCodePoint(currentScalar) && ((continuationByteAccumulator & 0xC0U) == 0))
            {
                // Valid 3-byte sequence.
                return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(currentScalar), charsConsumed: 3);
            }

            // Check for 4-byte sequence.

            if (buffer.Length < 4)
            {
                goto Error; // out of data
            }

            nextByte = (uint)buffer[3] - 0x80U; // optimistically assume for now it's a valid continuation byte
            continuationByteAccumulator |= nextByte;
            currentScalar = (currentScalar << 6) + nextByte + 0x3800000U - 0x3C00000U /* fix first byte header */;

            if (UnicodeHelpers.IsInRangeInclusive(currentScalar, 0x10000U, 0x10FFFFU) && ((continuationByteAccumulator & 0xC0U) == 0))
            {
                // Valid 4-byte sequence.
                return (SequenceValidity.Valid, UnicodeScalar.DangerousCreateWithoutValidation(currentScalar), charsConsumed: 4);
            }

        Error:

            /*
             * ERROR HANDLING
             */

            // At this point, we know the buffer doesn't represent a well-formed sequence.
            // It's ok for this logic to be somewhat unoptimized since ill-formed buffers should be rare.

            // First, see if the first byte isn't a valid sequence start byte.
            // If so, we have an invalid sequence of length 1.

            if (!UnicodeHelpers.IsInRangeInclusive(buffer[0], 0xC2U, 0xF4U))
            {
                return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 1);
            }

            // First byte is fine, are we simply lacking further data?
            // If so, we have an incomplete sequence of length 1.

            if (buffer.Length < 2)
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 1);
            }

            // Is the second byte a continuation byte?
            // If not, we have an invalid sequence of length 1.
            // (For this purpose ignore overlong / out-of-range / surrogate sequences.)

            if ((buffer[1] & 0xC0U) != 0x80U)
            {
                return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 1);
            }

            // Did the second byte result in an overlong, surrogate, or out-of-range sequence?
            // If so, we have an invalid sequence of length 2.

            uint firstTwoBytes = ((uint)buffer[0]) << 8 | buffer[1];

            if (!UnicodeHelpers.IsInRangeInclusive(firstTwoBytes, 0xE0A0U, 0xED9FU)
                && !UnicodeHelpers.IsInRangeInclusive(firstTwoBytes, 0xEE80U, 0xEFBFU)
                && !UnicodeHelpers.IsInRangeInclusive(firstTwoBytes, 0xF090U, 0xF48FU))
            {
                return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 2);
            }

            // First two bytes are fine, are we simply lacking further data?
            // If so, we have an incomplete sequence of length 2.

            if (buffer.Length < 3)
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 2);
            }

            // Is the third byte a continuation byte?
            // If not, we have an invalid sequence of length 2.

            if ((buffer[2] & 0xC0U) != 0x80U)
            {
                return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 2);
            }

            // First three bytes are fine, are we simply lacking further data?
            // If so, we have an incomplete sequence of length 3.

            if (buffer.Length < 4)
            {
                return (SequenceValidity.Incomplete, UnicodeScalar.ReplacementChar, charsConsumed: 3);
            }

            // Only possible remaining option is that the last byte isn't a continuation byte as expected.
            // And so we have an invalid sequence of length 3.

            return (SequenceValidity.Invalid, UnicodeScalar.ReplacementChar, charsConsumed: 3);
        }
    }
}
