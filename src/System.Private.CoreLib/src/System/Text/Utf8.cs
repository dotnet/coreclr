// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Globalization;

namespace System.Text
{
    /// <summary>
    /// Contains static methods for working with UTF-8 textual data.
    /// </summary>
    public static class Utf8
    {
        private static OperationStatus ChangeCaseCommon(ReadOnlySpan<byte> source, Span<byte> destination, bool toUpper, CultureInfo culture, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            if ((uint)behavior > (uint)InvalidSequenceBehavior.LeaveUnchanged)
            {
                throw new ArgumentOutOfRangeException(nameof(behavior));
            }

            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII or performing bulk transcoding.

            int tempBytesConsumed = 0;
            int tempBytesWritten = 0;
            Span<byte> changeCaseBuffer = stackalloc byte[4]; // worst-case scenario for UTF-8 representation of any Unicode scalar

            while (true)
            {
                var result = UnicodeReader.PeekFirstScalarUtf8(source);
                if (result.status == SequenceValidity.Valid)
                {
                    // Found a good UTF-8 sequence - perform the conversion and write it to the output buffer
                    UnicodeScalar changeCaseScalar;
                    if (toUpper)
                    {
                        changeCaseScalar = UnicodeScalar.ToUpper(result.scalar, culture);
                    }
                    else
                    {
                        changeCaseScalar = UnicodeScalar.ToLower(result.scalar, culture);
                    }
                    int changeCaseScalarSequenceLength = changeCaseScalar.ToUtf8(changeCaseBuffer);
                    if (!changeCaseBuffer.Slice(0, changeCaseScalarSequenceLength).TryCopyTo(destination))
                    {
                        goto ReturnDestinationTooSmall;
                    }

                    source = source.Slice(result.charsConsumed); // number of source code units consumed
                    tempBytesConsumed += result.charsConsumed;

                    destination = destination.Slice(changeCaseScalarSequenceLength); // number of destination code units written
                    tempBytesWritten += changeCaseScalarSequenceLength;

                    continue;
                }
                else if (result.status == SequenceValidity.Incomplete)
                {
                    // The input buffer is empty or contains a partial UTF-8 sequence.

                    if (result.charsConsumed == 0)
                    {
                        // We've fully consumed the source buffer.
                        goto ReturnDone;
                    }

                    // There's a partial sequence at the end of the source span.
                    // How should we treat this?

                    if (!isFinalChunk)
                    {
                        // Caller told us to expect more data.
                        goto ReturnNeedMoreData;
                    }

                    // The caller told us that there was no more data after this, so treat
                    // the partial sequence as equivalent to an invalid sequence.
                }

                // At this point, we know we're working with an invalid sequence.

                switch (behavior)
                {
                    case InvalidSequenceBehavior.ReplaceInvalidSequence:
                        // write U+FFFD
                        if (destination.Length < 3)
                        {
                            goto ReturnDestinationTooSmall;
                        }

                        destination[0] = 0xEF;
                        destination[1] = 0xBF;
                        destination[2] = 0xBD;

                        source = source.Slice(result.charsConsumed);
                        tempBytesConsumed += result.charsConsumed;

                        destination = destination.Slice(3);
                        tempBytesWritten += 3;
                        continue;

                    case InvalidSequenceBehavior.LeaveUnchanged:
                        // copy over invalid bytes unmodified
                        if (!source.Slice(0, result.charsConsumed).TryCopyTo(destination))
                        {
                            goto ReturnDestinationTooSmall;
                        }

                        source = source.Slice(result.charsConsumed);
                        tempBytesConsumed += result.charsConsumed;

                        destination = destination.Slice(result.charsConsumed);
                        tempBytesWritten += result.charsConsumed;
                        continue;

                    default:
                        // fail
                        goto ReturnInvalidData;
                }
            }

        ReturnDone:
            OperationStatus retVal = OperationStatus.Done;

        ReturnCommon:
            bytesConsumed = tempBytesConsumed;
            bytesWritten = tempBytesWritten;
            return retVal;

        ReturnDestinationTooSmall:
            retVal = OperationStatus.DestinationTooSmall;
            goto ReturnCommon;

        ReturnInvalidData:
            retVal = OperationStatus.InvalidData;
            goto ReturnCommon;

        ReturnNeedMoreData:
            retVal = OperationStatus.NeedMoreData;
            goto ReturnCommon;
        }

        // Ordinal
        public static bool Contains(ReadOnlySpan<byte> source, UnicodeScalar scalar)
        {
            return IndexOf(source, scalar) >= 0;
        }

        public static bool Contains(ReadOnlySpan<byte> source, UnicodeScalar scalar, StringComparison comparison)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        // Ordinal; returns -1 if not found
        public static int IndexOf(ReadOnlySpan<byte> source, UnicodeScalar scalar)
        {
            Span<byte> scalarAsUtf8 = stackalloc byte[4];
            int scalarUtf8CodeUnitCount = scalar.ToUtf8(scalarAsUtf8);
            return source.IndexOf(scalarAsUtf8.Slice(0, scalarUtf8CodeUnitCount));
        }

        // Returns -1 if not found
        public static int IndexOf(ReadOnlySpan<byte> source, UnicodeScalar scalar, StringComparison comparison, out int matchLength)
        {
            // TODO: Implement me
            throw new NotImplementedException();
        }

        public static int ToLower(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.

            // For compatibility with String.ToUpperInvariant (UTF-16 processing), our default invalid sequence
            // behavior is to leave invalid code units as-is.

            var operationStatus = ToLower(source, destination, culture, isFinalChunk: true, InvalidSequenceBehavior.LeaveUnchanged, out int _, out int bytesWritten);
            return (operationStatus == OperationStatus.Done) ? bytesWritten : -1;
        }

        public static OperationStatus ToLower(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            return ChangeCaseCommon(source, destination, toUpper: false, culture, isFinalChunk, behavior, out bytesConsumed, out bytesWritten);
        }

        public static int ToLowerInvariant(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.
            return ToLower(source, destination, CultureInfo.InvariantCulture);
        }

        public static OperationStatus ToLowerInvariant(ReadOnlySpan<byte> source, Span<byte> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.
            return ToLower(source, destination, CultureInfo.InvariantCulture, isFinalChunk, behavior, out bytesConsumed, out bytesWritten);
        }

        public static int ToUpper(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.

            // For compatibility with String.ToUpperInvariant (UTF-16 processing), our default invalid sequence
            // behavior is to leave invalid code units as-is.

            var operationStatus = ToUpper(source, destination, culture, isFinalChunk: true, InvalidSequenceBehavior.LeaveUnchanged, out int _, out int bytesWritten);
            return (operationStatus == OperationStatus.Done) ? bytesWritten : -1;
        }

        public static OperationStatus ToUpper(ReadOnlySpan<byte> source, Span<byte> destination, CultureInfo culture, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            return ChangeCaseCommon(source, destination, toUpper: true, culture, isFinalChunk, behavior, out bytesConsumed, out bytesWritten);
        }

        public static int ToUpperInvariant(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.
            return ToUpper(source, destination, CultureInfo.InvariantCulture);
        }

        public static OperationStatus ToUpperInvariant(ReadOnlySpan<byte> source, Span<byte> destination, bool isFinalChunk, InvalidSequenceBehavior behavior, out int bytesConsumed, out int bytesWritten)
        {
            // TODO: Optimize me. This can be done much, much faster by special-casing ASCII.
            return ToUpper(source, destination, CultureInfo.InvariantCulture, isFinalChunk, behavior, out bytesConsumed, out bytesWritten);
        }
    }
}
