// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;

namespace System.Text
{
    public static class Unicode
    {
        public static OperationStatus TranscodeUtf8ToUtf16(ReadOnlySpan<byte> source, Span<char> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int charsWritten)
        {
            // We disallow 'leave unchanged' because it's meaningless
            if ((uint)behavior > (uint)InvalidSequenceBehavior.ReplaceInvalidSequence)
            {
                // TODO: Throw a better exception.
                throw new ArgumentOutOfRangeException(paramName: nameof(behavior));
            }

            return TranscodeUtf8ToUtf16(source, destination, isFinalChunk, fixupInvalidSequences: (behavior != InvalidSequenceBehavior.Fail), out bytesConsumed, out charsWritten);
        }

        internal static OperationStatus TranscodeUtf8ToUtf16(ReadOnlySpan<byte> source, Span<char> destination, bool isFinalChunk, bool fixupInvalidSequences, out int bytesConsumed, out int charsWritten)
        {
            // TODO: Implement me in a much more optimized fashion.

            bytesConsumed = 0;
            charsWritten = 0;

            while (!source.IsEmpty)
            {
                var result = UnicodeReader.PeekFirstScalarUtf8(source);
                if (result.status == SequenceValidity.Valid)
                {
                    // source begins with a valid UTF-8 sequence
                    int utf16SequenceLength = result.scalar.Utf16SequenceLength;
                    if (destination.Length >= utf16SequenceLength)
                    {
                        int numCharsWrittenToDest = result.scalar.ToUtf16(destination);
                        Debug.Assert(utf16SequenceLength == numCharsWrittenToDest);

                        bytesConsumed += result.charsConsumed;
                        source = source.Slice(result.charsConsumed);

                        charsWritten += utf16SequenceLength;
                        destination = destination.Slice(utf16SequenceLength);
                    }
                    else
                    {
                        return OperationStatus.DestinationTooSmall;
                    }
                }
                else if (result.status == SequenceValidity.Invalid)
                {
                    // source begins with an invalid UTF-8 sequence
                    if (fixupInvalidSequences)
                    {
                        if (!destination.IsEmpty)
                        {
                            // write out U+FFFD
                            destination[0] = '\uFFFD';

                            bytesConsumed += result.charsConsumed;
                            source = source.Slice(result.charsConsumed);

                            charsWritten++;
                            destination = destination.Slice(1);
                        }
                        else
                        {
                            return OperationStatus.DestinationTooSmall;
                        }
                    }
                    else
                    {
                        return OperationStatus.InvalidData;
                    }
                }
                else
                {
                    // source represents an incomplete sequence
                    if (isFinalChunk)
                    {
                        if (fixupInvalidSequences)
                        {
                            if (destination.Length >= 1)
                            {
                                // write out U+FFFD
                                destination[0] = '\uFFFD';

                                bytesConsumed += source.Length;
                                charsWritten += 1;
                                break; // no more data
                            }
                            else
                            {
                                return OperationStatus.DestinationTooSmall;
                            }
                        }
                        else
                        {
                            return OperationStatus.InvalidData;
                        }
                    }
                    else
                    {
                        return OperationStatus.NeedMoreData;
                    }
                }
            }

            return OperationStatus.Done;
        }

        public static OperationStatus TranscodeUtf16ToUtf8(ReadOnlySpan<char> source, Span<byte> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int charsConsumed, out int bytesWritten)
        {
            // We disallow 'leave unchanged' because it's meaningless
            if ((uint)behavior > (uint)InvalidSequenceBehavior.ReplaceInvalidSequence)
            {
                // TODO: Throw a better exception.
                throw new ArgumentOutOfRangeException(paramName: nameof(behavior));
            }

            return TranscodeUtf16ToUtf8(source, destination, isFinalChunk, fixupInvalidSequences: (behavior != InvalidSequenceBehavior.Fail), out charsConsumed, out bytesWritten);
        }

        internal static OperationStatus TranscodeUtf16ToUtf8(ReadOnlySpan<char> source, Span<byte> destination, bool isFinalChunk, bool fixupInvalidSequences, out int charsConsumed, out int bytesWritten)
        {
            // TODO: Optimize me, including vectorization, BMI, and unaligned reads / writes.

            int originalDestinationLength = destination.Length;

            int sourceIdx;
            for (sourceIdx = 0; sourceIdx < source.Length; sourceIdx++)
            {
                uint ch = source[sourceIdx];
                if (ch <= 0x7FU)
                {
                    // ASCII -> 1 UTF-8 code unit
                    if (destination.Length >= 1)
                    {
                        destination[0] = (byte)ch;
                        destination = destination.Slice(1);
                    }
                    else
                    {
                        goto DestinationTooSmall;
                    }
                }
                else if (ch <= 0x7FFU)
                {
                    // 2 UTF-8 code units
                    if (destination.Length >= 2)
                    {
                        destination[0] = (byte)((ch >> 6) | 0b110_00000);
                        destination[1] = (byte)((ch & 0x3FU) | 0b10_000000);
                        destination = destination.Slice(2);
                    }
                    else
                    {
                        goto DestinationTooSmall;
                    }
                }
                else if (!UnicodeHelpers.IsSurrogateCodePoint(ch))
                {
                    // 3 UTF-8 code units
                    if (destination.Length >= 3)
                    {
                        destination[0] = (byte)((ch >> 12) | 0b1110_0000);
                        destination[1] = (byte)(((ch >> 6) & 0x3FU) | 0b10_000000);
                        destination[2] = (byte)((ch & 0x3FU) | 0b10_000000);
                        destination = destination.Slice(3);
                    }
                    else
                    {
                        goto DestinationTooSmall;
                    }
                }
                else if (ch <= 0xDBFFU)
                {
                    // found a high surrogate character
                    int nextIdx = sourceIdx + 1;
                    if ((uint)nextIdx < (uint)source.Length)
                    {
                        uint ch2 = source[nextIdx];
                        if (UnicodeHelpers.IsLowSurrogateCodePoint(ch2))
                        {
                            // proper surrogate pair
                            if (destination.Length >= 4)
                            {
                                ch += (1 << 6); // uuuuu = wwww + 1
                                destination[0] = (byte)((ch >> 10) - 0b110110_00 + 0b11110_000); // remove high surrogate marker; add UTF-8 4-byte sequence start marker
                                destination[1] = (byte)(((ch >> 2) & 0x3FU) | 0b10_000000);
                                destination[2] = (byte)(((ch & 0x3U) << 4) + (ch2 >> 6) - 0b110111_00 + 0b10_000000); // remove low surrogate marker; add UTF-8 continuation byte marker
                                destination[3] = (byte)((ch2 & 0x3FU) | 0b10_000000);
                                destination = destination.Slice(4);
                                sourceIdx++;
                            }
                            else
                            {
                                goto DestinationTooSmall;
                            }
                        }
                        else
                        {
                            // found a high surrogate character not followed by a low surrogate character - INVALID!
                            goto HandleInvalidSequence;
                        }
                    }
                    else
                    {
                        if (isFinalChunk)
                        {
                            // found a high surrogate character at the very end of the input buffer, but we're told
                            // this is the final chunk - INVALID!
                            goto HandleInvalidSequence;
                        }
                        else
                        {
                            goto NeedMoreData;
                        }
                    }
                }
                else
                {
                    // found a low surrogate character not preceded by a high surrogate character - INVALID!
                    goto HandleInvalidSequence;
                }

                continue;

            HandleInvalidSequence:
                if (fixupInvalidSequences)
                {
                    if (destination.Length >= 3)
                    {
                        // write out U+FFFD
                        destination[0] = 0xEF;
                        destination[1] = 0xBF;
                        destination[2] = 0xFD;
                        destination = destination.Slice(3);
                    }
                    else
                    {
                        goto DestinationTooSmall;
                    }
                }
                else
                {
                    goto InvalidData;
                }
            }

            // If we reached this point, all source data was converted.

            charsConsumed = sourceIdx;
            bytesWritten = originalDestinationLength - destination.Length;
            return OperationStatus.Done;

        DestinationTooSmall:
            charsConsumed = sourceIdx;
            bytesWritten = originalDestinationLength - destination.Length;
            return OperationStatus.DestinationTooSmall;

        NeedMoreData:
            charsConsumed = sourceIdx;
            bytesWritten = originalDestinationLength - destination.Length;
            return OperationStatus.NeedMoreData;

        InvalidData:
            charsConsumed = sourceIdx;
            bytesWritten = originalDestinationLength - destination.Length;
            return OperationStatus.InvalidData;
        }
    }
}
