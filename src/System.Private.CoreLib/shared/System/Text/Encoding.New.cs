// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System.Text
{
    public partial class Encoding
    {
        /*
         * This file contains infrastructure code that supports a simplified way of writing
         * internally-implemented Encoding types. In this system, the individual Encoding types
         * are no longer responsible for handling anything related to the EncoderNLS / DecoderNLS
         * infrastructure, nor are they responsible for implementing anything related to fallback
         * buffers logic.
         * 
         * Instead, subclassed types are responsible only for transcoding of individual scalar values
         * to and from the encoding's byte representation (see the two methods immediately below).
         * They can optionally implement fast-path logic to perform bulk transcoding up until the
         * first segment of data that cannot be transcoded. They can special-case certain fallback
         * mechanisms if desired.
         * 
         * Most of the fast-path code is written using raw pointers as the exchange types, just as
         * in the standard Encoding infrastructure. Since the fallback logic is more complex, most
         * of it is written using type-safe constructs like Span<T>, with some amount of glue to
         * allow it to work correctly with pointer-based fast-path code.
         * 
         * A typical call graph for GetBytes is represented below, using ASCIIEncoding as an example.
         * 
         * ASCIIEncoding.GetBytes(...) [non-EncoderNLS path, public virtual override]
         * `- <parameter validation>
         *  - ASCIIEncoding.GetBytesCommon [private helper method per derived type, inlined]
         *    `- ASCIIEncoding.GetBytesFast [overridden fast-path implementation, inlined]
         *     - <if all data transcoded, return immediately>
         *     - <if all data not transcoded...>
         *       `- Encoding.GetBytesWithFallback [non-virtual stub method to call main GetBytesWithFallback worker]
         *          `- Encoding.GetBytesWithFallback [virtual method whose base implementation contains slow fallback logic]
         *             `- <may be overridden to provide optimized fallback logic>
         *              - <create EncodeFallbackBuffer instance>
         *              - <perform the following in a loop:>
         *                `- <invoke fast-path logic via virtual method dispatch on derived type>
         *                 - <read next "bad" scalar value from source>
         *                 - <run this bad value through the fallback buffer>
         *                 - <drain the fallback buffer to the destination>
         *                 - <loop until source is fully consumed or destination is full>
         *              - <signal full or partial success to EncoderNLS instance / throw if necessary>
         * 
         * The call graph for GetBytes(..., EncoderNLS) is similar:
         * 
         * Encoding.GetBytes(..., EncoderNLS) [base implementation]
         * `- <if no leftover data from previous invocation, invoke fast-path>
         *  - <if fast-path invocation above completed, return immediately>
         *  - <if not all data transcoded, or if there was leftover data from previous invocation...>
         *    `- Encoding.GetBytesWithFallback [non-virtual stub method]
         *       `- <drain any leftover data from previous invocation>
         *        - <invoke fast-path again>
         *        - <if all data transcoded, return immediately>
         *        - <if all data not transcoded...>
         *          `- Encoding.GetBytesWithFallback [virtual method as described above]
         *  
         * There are different considerations in each call graph for things like error handling,
         * since the error conditions will be different depending on whether or not an EncoderNLS
         * instance is available and what values its properties have.
         */

        /*
         * THESE TWO METHODS MUST BE OVERRIDDEN BY A SUBCLASSED TYPE
         */

        internal virtual OperationStatus GetBytes(Rune value, Span<byte> bytes, out int bytesWritten)
        {
            Debug.Fail("This should be overridden by a subclassed type.");
            throw NotImplemented.ByDesign;
        }

        internal virtual OperationStatus DecodeFirst(ReadOnlySpan<byte> bytes, out Rune value, out int bytesConsumed)
        {
            Debug.Fail("This should be overridden by a subclassed type.");
            throw NotImplemented.ByDesign;
        }

        /*
         * ALL OTHER LOGIC CAN BE IMPLEMENTED IN TERMS OF THE TWO METHODS ABOVE.
         * FOR IMPROVED PERFORMANCE, SUBCLASSED TYPES MAY WANT TO OVERRIDE ONE OR MORE VIRTUAL METHODS BELOW.
         */

        /*
         * GETBYTECOUNT FAMILY OF FUNCTIONS
         */

        internal virtual bool TryGetByteCount(Rune value, out int byteCount)
        {
            // Ideally this method should be overridden by a subclassed type,
            // but we can provide a basic implementation assuming the subclassed
            // implementation doesn't try writing more than 4 bytes.

            Span<byte> bytes = stackalloc byte[4];

            OperationStatus opStatus = GetBytes(value, bytes, out byteCount);
            Debug.Assert(opStatus == OperationStatus.Done || opStatus == OperationStatus.InvalidData, "Unexpected return value.");

            return (opStatus == OperationStatus.Done);
        }

        // Entry point from EncoderNLS implementation.
        internal virtual unsafe int GetByteCount(char* pChars, int charCount, EncoderNLS encoder)
        {
            Debug.Assert(encoder != null, "This code path should only be called from EncoderNLS.");
            Debug.Assert(charCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pChars != null || charCount == 0, "Cannot provide a null pointer and a non-zero count.");

            // First, draining any data that already exists on the encoder instance.

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(pChars, charCount);

            int totalByteCount = encoder.DrainLeftoverDataForGetByteCount(chars, out int charsConsumed);

            Debug.Assert(charsConsumed >= 0, "EncoderNLS shouldn't have reported negative chars consumed.");
            Debug.Assert(totalByteCount >= 0, "EncoderNLS shouldn't have reported negative byte count.");

            // Now try invoking the "fast path" (no fallback buffer) implementation.
            // If we consumed the entire buffer, report this to the caller and return success.
            // We can use Unsafe.AsPointer here since these spans are created from pinned data (raw pointers).
            // As we're tallying we'll need to check for integer overflow.

            chars = chars.Slice(charsConsumed);

            int byteCountThisIteration = GetByteCountNoFallbackBuffer(
                pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                charCount: chars.Length,
                fallback: encoder.Fallback,
                charsConsumed: out charsConsumed);

            Debug.Assert(0 <= charsConsumed && charsConsumed <= chars.Length, "Method returned invalid value.");
            Debug.Assert(byteCountThisIteration >= 0, "Method shouldn't have returned negative value.");

            totalByteCount += byteCountThisIteration;
            if (totalByteCount < 0)
            {
                ThrowConversionOverflow();
            }

            chars = chars.Slice(charsConsumed);

            // If there's still data remaining in the source buffer, go down the fallback path.
            // Otherwise we're finished.

            if (!chars.IsEmpty)
            {
                EncoderFallbackBuffer fallbackBuffer = encoder.FallbackBuffer; // will allocate if necessary
                fallbackBuffer.InternalInitialize(this, encoder, charCount);

                byteCountThisIteration = GetByteCountWithFallback(chars, fallbackBuffer);
                Debug.Assert(byteCountThisIteration >= 0, "Method shouldn't have returned negative value.");

                totalByteCount += byteCountThisIteration;
                if (totalByteCount < 0)
                {
                    ThrowConversionOverflow();
                }
            }

            return totalByteCount;
        }

        /// <summary>
        /// Gets the total byte count that would result from transcoding <paramref name="pChars"/> to bytes.
        /// If <paramref name="fallback"/> is specified, the method implementation may inspect properties of
        /// the instance in order to short-circuit the operation, but the method should not attempt
        /// to call <see cref="EncoderFallback.CreateFallbackBuffer"/>.
        /// </summary>
        /// <returns>
        /// Via <paramref name="charsConsumed"/>, the amount of <paramref name="pChars"/> consumed before terminating;
        /// and returns the total byte count for the number of chars consumed.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting byte count would exceed <see cref="int.MaxValue"/>.
        /// </exception>
        private protected virtual unsafe int GetByteCountNoFallbackBuffer(char* pChars, int charCount, EncoderFallback fallback, out int charsConsumed)
        {
            // Ideally this method should be overridden by a subclassed type,
            // but we can provide a slow correct implementation.

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(pChars, charCount);
            int totalByteCount = 0;

            while (!chars.IsEmpty)
            {
                int scalarValue = Rune.ReadFirstRuneFromUtf16Buffer(chars);
                if (scalarValue < 0)
                {
                    break; // invalid UTF-16 data
                }

                if (!TryGetByteCount(new Rune(scalarValue), out int byteCountThisIter))
                {
                    break; // couldn't convert this well-formed UTF-16 scalar
                }

                totalByteCount += byteCountThisIter;
                if (totalByteCount < 0)
                {
                    ThrowConversionOverflow();
                }

                chars = chars.Slice(byteCountThisIter);
            }

            charsConsumed = charCount - chars.Length;
            return totalByteCount;
        }

        /// <summary>
        /// Counts the number of bytes that would result from transcoding the provided chars,
        /// using the provided <see cref="EncoderFallbackBuffer"/> if necessary.
        /// </summary>
        /// <returns>
        /// The byte count resulting from transcoding the input data.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting byte count is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        private protected virtual unsafe int GetByteCountWithFallback(ReadOnlySpan<char> chars, EncoderFallbackBuffer fallbackBuffer)
        {
            Debug.Assert(!chars.IsEmpty, "Caller shouldn't invoke this method with an empty input buffer.");

            // Since we're using Unsafe.AsPointer in our central loop, we want to ensure everything is pinned.

            fixed (char* _pChars_Unused = &MemoryMarshal.GetReference(chars))
            {
                int totalByteCount = 0;

                do
                {
                    // First, run through the fast-path as far as we can.
                    // While building up the tally we need to continually check for integer overflow
                    // since fallbacks can change the total byte count in unexpected ways.

                    int byteCountThisIteration = GetByteCountNoFallbackBuffer(
                        pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                        charCount: chars.Length,
                        fallback: null, // wasn't able to be short-circuited by our caller; don't bother trying again
                        charsConsumed: out int charsConsumedThisIteration);

                    Debug.Assert(byteCountThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");
                    Debug.Assert(charsConsumedThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");

                    totalByteCount += byteCountThisIteration;
                    if (totalByteCount < 0)
                    {
                        ThrowConversionOverflow();
                    }

                    chars = chars.Slice(charsConsumedThisIteration);

                    if (!chars.IsEmpty)
                    {
                        // There's still data remaining in the source buffer.
                        // We need to figure out why we weren't able to make progress.
                        // There are two scenarios: (a) the source buffer contained bad UTF-16 data, or (b) the encoding can't translate this scalar value.

                        switch (Rune.DecodeUtf16(chars, out Rune firstScalarValue, out charsConsumedThisIteration))
                        {
                            case OperationStatus.NeedMoreData:
                                Debug.Assert(charsConsumedThisIteration == chars.Length, "If returning NeedMoreData, should out the entire buffer length as chars consumed.");
                                if (fallbackBuffer.encoder is null || fallbackBuffer.encoder.MustFlush)
                                {
                                    // If there's no EncoderNLS in use or if the EncoderNLS tells us that it's unable to store
                                    // leftover data (because a flush is required), we need to treat this standalone high
                                    // surrogate character as an individual unknown char for the purposes of fallback.

                                    goto case OperationStatus.InvalidData;
                                }
                                else
                                {
                                    // If we're not flushing, just pretend we consumed the entire input buffer.
                                    // It'll eventually be stored in EncoderNLS._charLeftOver by the next call to GetBytes.

                                    chars = ReadOnlySpan<char>.Empty;
                                    goto Finish;
                                }

                            case OperationStatus.InvalidData:
                                break;

                            default:
                                if (TryGetByteCount(firstScalarValue, out _))
                                {
                                    goto Finish; // Encoding instance was able to translate this value; must've been out of space in the destination buffer
                                }
                                break; // source buffer contained valid UTF-16 but encoder doesn't support this scalar value
                        }

                        // Now we know the reason for failure was that the original input was invalid
                        // for the encoding in use. Run it through the fallback mechanism.

                        byteCountThisIteration = fallbackBuffer.InternalFallbackGetByteCount(chars, out charsConsumedThisIteration);

                        Debug.Assert(byteCountThisIteration >= 0, "Fallback buffer shouldn't have returned a negative value.");
                        Debug.Assert(charsConsumedThisIteration >= 0, "Fallback buffer shouldn't have returned a negative value.");

                        totalByteCount += byteCountThisIteration;
                        if (totalByteCount < 0)
                        {
                            ThrowConversionOverflow();
                        }

                        chars = chars.Slice(charsConsumedThisIteration);
                    }
                } while (!chars.IsEmpty);

            Finish:

                Debug.Assert(fallbackBuffer.Remaining == 0, "There should be no data in the fallback buffer after GetByteCount.");

                return totalByteCount;
            }
        }

        /// <summary>
        /// Counts the number of bytes that would result from transcoding the provided chars,
        /// with no associated <see cref="EncoderNLS"/>. The first two arguments are based on the
        /// original input before invoking this method; and <paramref name="charsConsumedSoFar"/>
        /// signals where in the provided buffer the fallback loop should begin operating.
        /// </summary>
        /// <returns>
        /// The byte count resulting from transcoding the input data.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting byte count is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)] // don't stack spill spans into our caller
        private protected unsafe int GetByteCountWithFallback(char* pCharsOriginal, int originalCharCount, int charsConsumedSoFar)
        {
            Debug.Assert(0 <= charsConsumedSoFar && charsConsumedSoFar < originalCharCount, "Invalid arguments provided to method.");

            EncoderFallbackBuffer fallbackBuffer = EncoderFallback.CreateFallbackBuffer(); // allocate a fresh new instance
            fallbackBuffer.InternalInitialize(this, null, originalCharCount);

            int byteCount = GetByteCountWithFallback(
                chars: new ReadOnlySpan<char>(pCharsOriginal + charsConsumedSoFar, originalCharCount - charsConsumedSoFar),
                fallbackBuffer: fallbackBuffer);

            Debug.Assert(byteCount >= 0, "Workhorse shouldn't have returned a negative value.");

            return byteCount;
        }

        /*
         * GETBYTES FAMILY OF FUNCTIONS
         */

        // Entry point from EncoderNLS.
        internal virtual unsafe int GetBytes(char* pChars, int charCount, byte* pBytes, int byteCount, EncoderNLS encoder)
        {
            Debug.Assert(encoder != null, "This code path should only be called from EncoderNLS.");
            Debug.Assert(charCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pChars != null || charCount == 0, "Cannot provide a null pointer and a non-zero count.");
            Debug.Assert(byteCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pBytes != null || byteCount == 0, "Cannot provide a null pointer and a non-zero count.");

            // We're going to try to stay on the fast-path as much as we can. That means that we have
            // no leftover data to drain and the entire source buffer can be transcoded in a single
            // fast-path invocation. If either of these doesn't hold, we'll go down the slow path of
            // creating spans, draining the EncoderNLS instance, and falling back.

            int bytesWritten = 0;
            int charsConsumed = 0;

            if (!encoder.HasLeftoverData)
            {
                bytesWritten = GetBytesFast(pChars, charCount, pBytes, byteCount, out charsConsumed);
                if (charsConsumed == charCount)
                {
                    encoder._charsUsed = charCount;
                    return bytesWritten;
                }
            }

            // We had leftover data, or we couldn't consume the entire input buffer.
            // Let's go down the draining + fallback mechanisms.

            return GetBytesWithFallback(pChars, charCount, pBytes, byteCount, charsConsumed, bytesWritten, encoder);
        }

        /// <summary>
        /// Transcodes <see langword="char"/>s to <see langword="byte"/>s, exiting when the source or destination
        /// buffer is consumed or when the first unreadable data is encountered.
        /// </summary>
        /// <returns>
        /// Via <paramref name="charsConsumed"/>, the number of elements from <paramref name="pChars"/> which
        /// were consumed; and returns the number of elements written to <paramref name="pBytes"/>.
        /// </returns>
        /// <remarks>
        /// The implementation should not attempt to perform any sort of fallback behavior.
        /// If custom fallback behavior is necessary, override <see cref="GetBytesWithFallback"/>.
        /// </remarks>
        private protected virtual unsafe int GetBytesFast(char* pChars, int charsLength, byte* pBytes, int bytesLength, out int charsConsumed)
        {
            // Ideally this method should be overridden by a subclassed type,
            // but we can provide a slow correct implementation.

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(pChars, charsLength);
            Span<byte> bytes = new Span<byte>(pBytes, bytesLength);

            while (!chars.IsEmpty)
            {
                if (Rune.DecodeUtf16(chars, out Rune scalarValue, out int charsConsumedJustNow) != OperationStatus.Done
                    || GetBytes(scalarValue, bytes, out int bytesWrittenJustNow) != OperationStatus.Done)
                {
                    // Invalid UTF-16 data, or not convertible to target encoding, or destination buffer too small to contain encoded value

                    break;
                }

                chars = chars.Slice(charsConsumedJustNow);
                bytes = bytes.Slice(bytesWrittenJustNow);
            }

            charsConsumed = charsLength - chars.Length; // number of chars consumed across all loop iterations above
            return bytesLength - bytes.Length; // number of bytes written across all loop iterations above
        }

        /// <summary>
        /// Transcodes chars to bytes, with no associated <see cref="EncoderNLS"/>. The first four arguments are
        /// based on the original input before invoking this method; and <paramref name="charsConsumedSoFar"/>
        /// and <paramref name="bytesWrittenSoFar"/> signal where in the provided buffers the fallback loop
        /// should begin operating. The behavior of this method is to call the <see cref="GetBytesWithFallback"/>
        /// virtual method as overridden by the specific type, and failing that go down the shared fallback path.
        /// </summary>
        /// <returns>
        /// The total number of bytes written to <paramref name="pOriginalBytes"/>, including <paramref name="bytesWrittenSoFar"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the destination buffer is not large enough to hold the entirety of the transcoded data.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private protected unsafe int GetBytesWithFallback(char* pOriginalChars, int originalCharCount, byte* pOriginalBytes, int originalByteCount, int charsConsumedSoFar, int bytesWrittenSoFar)
        {
            // This is a stub method that's marked "no-inlining" so that it we don't stack-spill spans
            // into our immediate caller. Doing so increases the method prolog in what's supposed to
            // be a very fast path.

            Debug.Assert(0 <= charsConsumedSoFar && charsConsumedSoFar < originalCharCount, "Invalid arguments provided to method.");
            Debug.Assert(0 <= bytesWrittenSoFar && bytesWrittenSoFar <= originalByteCount, "Invalid arguments provided to method.");

            return GetBytesWithFallback(
                chars: new ReadOnlySpan<char>(pOriginalChars, originalCharCount).Slice(charsConsumedSoFar),
                originalCharsLength: originalCharCount,
                bytes: new Span<byte>(pOriginalBytes, originalByteCount).Slice(bytesWrittenSoFar),
                originalBytesLength: originalByteCount,
                encoder: null);
        }

        private unsafe int GetBytesWithFallback(char* pOriginalChars, int originalCharCount, byte* pOriginalBytes, int originalByteCount, int charsConsumedSoFar, int bytesWrittenSoFar, EncoderNLS encoder)
        {
            Debug.Assert(encoder != null, "This code path should only be called from EncoderNLS.");
            Debug.Assert(0 <= charsConsumedSoFar && charsConsumedSoFar < originalCharCount, "Caller should've checked this condition.");
            Debug.Assert(0 <= bytesWrittenSoFar && bytesWrittenSoFar <= originalByteCount, "Caller should've checked this condition.");

            // First, try draining any data that already exists on the encoder instance. If we can't complete
            // that operation, there's no point to continuing down to the main workhorse methods.

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(pOriginalChars, originalCharCount).Slice(charsConsumedSoFar);
            Span<byte> bytes = new Span<byte>(pOriginalBytes, originalByteCount).Slice(bytesWrittenSoFar);

            bool drainFinishedSuccessfully = encoder.TryDrainLeftoverDataForGetBytes(chars, bytes, out int charsConsumedJustNow, out int bytesWrittenJustNow);

            chars = chars.Slice(charsConsumedJustNow); // whether or not the drain finished, we may have made some progress
            bytes = bytes.Slice(bytesWrittenJustNow);

            if (!drainFinishedSuccessfully)
            {
                ThrowBytesOverflow(encoder, nothingEncoded: bytes.Length == originalByteCount); // might not throw if we wrote at least one byte
            }
            else
            {
                // Now try invoking the "fast path" (no fallback) implementation.
                // We can use Unsafe.AsPointer here since these spans are created from pinned data (raw pointers).

                bytesWrittenJustNow = GetBytesFast(
                    pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                    charsLength: chars.Length,
                    pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                    bytesLength: bytes.Length,
                    charsConsumed: out charsConsumedJustNow);

                chars = chars.Slice(charsConsumedJustNow);
                bytes = bytes.Slice(bytesWrittenJustNow);

                // If there's still data remaining in the source buffer, go down the fallback path.
                // Otherwise we're finished.

                if (!chars.IsEmpty)
                {
                    // We'll optimistically tell the encoder that we're using everything; the
                    // GetBytesWithFallback method will overwrite this field if necessary.

                    encoder._charsUsed = originalCharCount;
                    return GetBytesWithFallback(chars, originalCharCount, bytes, originalByteCount, encoder);
                }
            }

            encoder._charsUsed = originalCharCount - chars.Length; // total number of characters consumed up until now
            return originalByteCount - bytes.Length; // total number of bytes written up until now
        }

        /// <summary>
        /// Transcodes chars to bytes, using <see cref="Encoding.EncoderFallback"/> or <see cref="Encoder.Fallback"/> if needed.
        /// </summary>
        /// <returns>
        /// The total number of bytes written to <paramref name="bytes"/> (based on <paramref name="originalBytesLength"/>).
        /// </returns>
        /// <remarks>
        /// The derived class should override this method if it might be able to provide a more optimized fallback
        /// implementation, deferring to the base implementation if needed. This method calls <see cref="ThrowBytesOverflow"/>
        /// if necessary.
        /// </remarks>
        private protected virtual unsafe int GetBytesWithFallback(ReadOnlySpan<char> chars, int originalCharsLength, Span<byte> bytes, int originalBytesLength, EncoderNLS encoder)
        {
            Debug.Assert(!chars.IsEmpty, "Caller shouldn't invoke this method with an empty input buffer.");
            Debug.Assert(originalCharsLength >= 0, "Caller provided invalid parameter.");
            Debug.Assert(originalBytesLength >= 0, "Caller provided invalid parameter.");

            // Since we're using Unsafe.AsPointer in our central loop, we want to ensure everything is pinned.

            fixed (char* _pChars_Unused = &MemoryMarshal.GetReference(chars))
            fixed (byte* _pBytes_Unused = &MemoryMarshal.GetReference(bytes))
            {
                EncoderFallbackBuffer fallbackBuffer = EncoderFallbackBuffer.CreateAndInitialize(this, encoder, originalCharsLength);

                do
                {
                    // First, transcode as much well-formed data as we can via the fast path.

                    int bytesWrittenThisIteration = GetBytesFast(
                        pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                        charsLength: chars.Length,
                        pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                        bytesLength: bytes.Length,
                        charsConsumed: out int charsConsumedThisIteration);

                    Debug.Assert(bytesWrittenThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");
                    Debug.Assert(charsConsumedThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");

                    chars = chars.Slice(charsConsumedThisIteration);
                    bytes = bytes.Slice(bytesWrittenThisIteration);

                    if (!chars.IsEmpty)
                    {
                        // There's still data remaining in the source buffer.
                        // We need to figure out why we weren't able to make progress.
                        // There are two scenarios: (a) the source buffer contained bad UTF-16 data, or (b) the encoding can't translate this scalar value.

                        switch (Rune.DecodeUtf16(chars, out Rune firstScalarValue, out charsConsumedThisIteration))
                        {
                            case OperationStatus.NeedMoreData:
                                Debug.Assert(charsConsumedThisIteration == chars.Length, "If returning NeedMoreData, should out the entire buffer length as chars consumed.");
                                if (encoder is null || encoder.MustFlush)
                                {
                                    goto case OperationStatus.InvalidData; // see comment in GetByteCountWithFallback
                                }
                                else
                                {
                                    encoder._charLeftOver = chars[0]; // squirrel away remaining high surrogate char and finish
                                    chars = ReadOnlySpan<char>.Empty;
                                    goto Finish;
                                }

                            case OperationStatus.InvalidData:
                                break;

                            default:
                                if (GetBytes(firstScalarValue, bytes, out _) == OperationStatus.DestinationTooSmall)
                                {
                                    goto Finish; // source buffer contained valid UTF-16 but encoder ran out of space in destination buffer
                                }
                                break; // source buffer contained valid UTF-16 but encoder doesn't support this scalar value
                        }

                        // Now we know the reason for failure was that the original input was invalid
                        // for the encoding in use. Run it through the fallback mechanism.

                        bool fallbackFinished = fallbackBuffer.TryInternalFallbackGetBytes(chars, bytes, out charsConsumedThisIteration, out bytesWrittenThisIteration);

                        // Regardless of whether the fallback finished, it did consume some number of
                        // chars, and it may have written some number of bytes.

                        chars = chars.Slice(charsConsumedThisIteration);
                        bytes = bytes.Slice(bytesWrittenThisIteration);

                        if (!fallbackFinished)
                        {
                            goto Finish; // fallback has pending state - it'll get written out on the next GetBytes call
                        }
                    }
                } while (!chars.IsEmpty);

            Finish:

                // We reach this point when we deplete the source or destination buffer. There are a few
                // cases to consider now. If the source buffer has been fully consumed and there's no
                // leftover data in the EncoderNLS or the fallback buffer, we've completed transcoding.
                // If the source buffer isn't empty or there's leftover data in the fallback buffer,
                // it means we ran out of space in the destintion buffer. This is an unrecoverable error
                // if no EncoderNLS is in use (because only EncoderNLS can handle partial success), and
                // even if an EncoderNLS is in use this is only recoverable if the EncoderNLS instance
                // allows partial completion. Let's check all of these conditions now.

                if (!chars.IsEmpty || fallbackBuffer.Remaining > 0)
                {
                    // The line below will also throw if the encoder couldn't make any progress at all
                    // because the output buffer wasn't large enough to contain the result of even
                    // a single scalar conversion or fallback.

                    ThrowBytesOverflow(encoder, nothingEncoded: bytes.Length == originalBytesLength);
                }

                // If an EncoderNLS instance is active, update its "total consumed character count" value.

                if (encoder != null)
                {
                    Debug.Assert(originalCharsLength >= chars.Length, "About to report a negative number of chars used?");
                    encoder._charsUsed = originalCharsLength - chars.Length; // number of chars consumed
                }

                Debug.Assert(fallbackBuffer.Remaining == 0 || encoder != null, "Shouldn't have any leftover data in fallback buffer unless an EncoderNLS is in use.");

                return originalBytesLength - bytes.Length;
            }
        }

        /*
         * GETCHARCOUNT FAMILY OF FUNCTIONS
         */

        internal virtual unsafe int GetCharCount(byte* pBytes, int byteCount, DecoderNLS decoder)
        {
            Debug.Assert(decoder != null, "This code path should only be called from DecoderNLS.");
            Debug.Assert(byteCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pBytes != null || byteCount == 0, "Cannot provide a null pointer and a non-zero count.");

            // First, draining any leftover bytes that might already exist on the decoder instance.

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(pBytes, byteCount);
            int totalCharCount = 0;
            int bytesConsumed;

            if (decoder.HasLeftoverData)
            {
                totalCharCount = decoder.DrainLeftoverDataForGetCharCount(bytes, out bytesConsumed);
                Debug.Assert(bytesConsumed >= 0, "Workhorse routine shouldn't have returned a negative value.");
                bytes = bytes.Slice(bytesConsumed);
            }

            // Now try invoking the "fast path" (no fallback buffer) implementation.
            // If we consumed the entire buffer, report this to the caller and return success.
            // We can use Unsafe.AsPointer here since these spans are created from pinned data (raw pointers).
            // As we're tallying we'll need to check for integer overflow.

            int charCountThisIteration = GetCharCountNoFallbackBuffer(
                pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)), // OK since span was constructed from pinned pointer
                byteCount: bytes.Length,
                fallback: decoder.Fallback,
                bytesConsumed: out bytesConsumed);

            Debug.Assert(0 <= bytesConsumed && bytesConsumed <= bytes.Length, "Method returned invalid value.");
            Debug.Assert(charCountThisIteration >= 0, "Method shouldn't have returned negative value.");

            totalCharCount += charCountThisIteration;
            if (totalCharCount < 0)
            {
                ThrowConversionOverflow();
            }

            bytes = bytes.Slice(bytesConsumed);

            // If there's still data remaining in the source buffer, go down the fallback path.
            // Otherwise we're finished.

            if (!bytes.IsEmpty)
            {
                DecoderFallbackBuffer fallbackBuffer = decoder.FallbackBuffer; // will allocate if necessary
                fallbackBuffer.InternalInitialize(this, decoder, byteCount);

                charCountThisIteration = GetCharCountWithFallback(bytes, fallbackBuffer);
                Debug.Assert(charCountThisIteration >= 0, "Method shouldn't have returned negative value.");

                totalCharCount += charCountThisIteration;
                if (totalCharCount < 0)
                {
                    ThrowConversionOverflow();
                }
            }

            return totalCharCount;
        }

        internal virtual unsafe int GetChars(byte* pBytes, int byteCount, char* pChars, int charCount, DecoderNLS decoder)
        {
            Debug.Assert(decoder != null, "This code path should only be called from DecoderNLS.");
            Debug.Assert(byteCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pBytes != null || byteCount == 0, "Cannot provide a null pointer and a non-zero count.");
            Debug.Assert(charCount >= 0, "Caller should've checked this condition.");
            Debug.Assert(pChars != null || charCount == 0, "Cannot provide a null pointer and a non-zero count.");

            // First, try draining any data that already exists on the encoder instance. If we can't complete
            // that operation, there's no point to continuing down to the main workhorse methods.

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(pBytes, byteCount);
            Span<char> chars = new Span<char>(pChars, charCount);

            int bytesConsumed, charsWritten;

            if (decoder.HasLeftoverData)
            {
                // Like GetBytes, there may be leftover data in the DecoderNLS instance. But unlike GetBytes,
                // the bytes -> chars conversion doesn't allow leftover data in the fallback buffer. This means
                // that the drain operation below will either succeed fully or fail; there's no partial success
                // condition as with the chars -> bytes conversion. The drain method will throw if there's not
                // enough space in the destination buffer.

                charsWritten = decoder.DrainLeftoverDataForGetChars(bytes, chars, out bytesConsumed);
                bytes = bytes.Slice(bytesConsumed);
                chars = chars.Slice(charsWritten);
            }

            Debug.Assert(!decoder.InternalHasFallbackBuffer || decoder.FallbackBuffer.Remaining == 0, "Should be no remaining fallback data at this point.");

            // Now try invoking the "fast path" (no fallback buffer) implementation.
            // If we consumed the entire buffer, report this to the caller and return success.
            // We can use Unsafe.AsPointer here since these spans are created from pinned data (raw pointers).

            charsWritten = GetCharsNoFallbackBuffer(
                pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                byteCount: bytes.Length,
                pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                charCount: chars.Length,
                fallback: decoder.Fallback,
                bytesConsumed: out bytesConsumed);

            Debug.Assert(0 <= bytesConsumed && bytesConsumed <= bytes.Length, "Method returned invalid value.");
            Debug.Assert(0 <= charsWritten && charsWritten <= chars.Length, "Method returned invalid value.");

            bytes = bytes.Slice(bytesConsumed);
            chars = chars.Slice(charsWritten);

            // Optimistically set _bytesUsed assuming we've consumed the entire input buffer.
            // If this turns out to be incorrect, it'll be overwritten by GetCharsWithFallback.

            decoder._bytesUsed = byteCount;

            // If there's still data remaining in the source buffer, go down the fallback path.
            // Otherwise we're finished.

            if (!bytes.IsEmpty)
            {
                DecoderFallbackBuffer fallbackBuffer = decoder.FallbackBuffer; // will allocate if necessary
                fallbackBuffer.InternalInitialize(this, decoder, byteCount);

                charsWritten = GetCharsWithFallback(bytes, chars, fallbackBuffer);

                Debug.Assert(0 <= decoder._bytesUsed && decoder._bytesUsed <= byteCount, "Method wrote invalid value to decoder._bytesUsed.");
                Debug.Assert(0 <= charsWritten && charsWritten <= chars.Length, "Method returned invalid value.");

                chars = chars.Slice(charsWritten);

                // If we _still_ haven't consumed the entire source buffer due to the destination buffer being too
                // small to contain the full output, signal this condition now. The ThrowCharsOverflow call below
                // might not throw; e.g., if DecoderNLS.Convert is being called and partial conversions are allowed.

                if (decoder._bytesUsed != byteCount)
                {
                    ThrowCharsOverflow(decoder, nothingDecoded: charCount == chars.Length);
                }
            }

            return charCount - chars.Length; // total chars written
        }

        /// <summary>
        /// Gets the total char count that would result from transcoding <paramref name="pBytes"/> to chars.
        /// If <paramref name="fallback"/> is specified, the method implementation may inspect properties of
        /// the instance in order to short-circuit the operation, but the method should not attempt
        /// to call <see cref="DecoderFallback.CreateFallbackBuffer"/>.
        /// </summary>
        /// <returns>
        /// Via <paramref name="bytesConsumed"/>, the amount of <paramref name="pBytes"/> consumed before terminating;
        /// and returns the total char count for the number of bytes consumed.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting char count would exceed <see cref="int.MaxValue"/>.
        /// </exception>
        private protected virtual unsafe int GetCharCountNoFallbackBuffer(byte* pBytes, int byteCount, DecoderFallback fallback, out int bytesConsumed)
        {
            // Ideally this method should be overridden by a subclassed type,
            // but we can provide a slow correct implementation.

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(pBytes, byteCount);
            int totalCharCount = 0;

            while (!bytes.IsEmpty)
            {
                // We don't care about statuses other than Done. The fallback mechanism will handle those.

                if (DecodeFirst(bytes, out Rune value, out int bytesConsumedJustNow) != OperationStatus.Done)
                {
                    break;
                }

                totalCharCount += value.Utf16SequenceLength;
                if (totalCharCount < 0)
                {
                    ThrowConversionOverflow();
                }

                bytes = bytes.Slice(bytesConsumedJustNow);
            }

            bytesConsumed = byteCount - bytes.Length;
            return totalCharCount;
        }

        /// <summary>
        /// Counts the number of chars that would result from transcoding the provided bytes,
        /// with no associated <see cref="DecoderNLS"/>. The first two arguments are based on the
        /// original input before invoking this method; and <paramref name="bytesConsumedSoFar"/>
        /// signals where in the provided buffer the fallback loop should begin operating.
        /// </summary>
        /// <returns>
        /// The char count resulting from transcoding the input data.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting char count is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)] // don't stack spill spans into our caller
        private protected unsafe int GetCharCountWithFallback(byte* pOriginalBytes, int originalByteCount, int bytesConsumedSoFar)
        {
            Debug.Assert(0 <= bytesConsumedSoFar && bytesConsumedSoFar < originalByteCount, "Invalid arguments provided to method.");

            DecoderFallbackBuffer fallbackBuffer = DecoderFallback.CreateFallbackBuffer(); // allocate a fresh new instance
            fallbackBuffer.InternalInitialize(this, null, bytesConsumedSoFar);

            int charCount = GetCharCountWithFallback(
                bytes: new ReadOnlySpan<byte>(pOriginalBytes + bytesConsumedSoFar, originalByteCount - bytesConsumedSoFar),
                fallbackBuffer: fallbackBuffer);

            Debug.Assert(charCount >= 0, "Workhorse shouldn't have returned a negative value.");

            return charCount;
        }

        /// <summary>
        /// Counts the number of chars that would result from transcoding the provided bytes,
        /// using the provided <see cref="DecoderFallbackBuffer"/> if necessary.
        /// </summary>
        /// <returns>
        /// The char count resulting from transcoding the input data.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the resulting char count is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        private unsafe int GetCharCountWithFallback(ReadOnlySpan<byte> bytes, DecoderFallbackBuffer fallbackBuffer)
        {
            Debug.Assert(!bytes.IsEmpty, "Caller shouldn't invoke this method with an empty input buffer.");

            // Since we're using Unsafe.AsPointer in our central loop, we want to ensure everything is pinned.

            fixed (byte* _pBytes_Unused = &MemoryMarshal.GetReference(bytes))
            {
                int totalCharCount = 0;

                do
                {
                    // First, run through the fast-path as far as we can.
                    // While building up the tally we need to continually check for integer overflow
                    // since fallbacks can change the total char count in unexpected ways.

                    int charCountThisIteration = GetCharCountNoFallbackBuffer(
                        pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                        byteCount: bytes.Length,
                        fallback: null, // wasn't able to be short-circuited by our caller; don't bother trying again
                        bytesConsumed: out int bytesConsumedThisIteration);

                    Debug.Assert(charCountThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");
                    Debug.Assert(bytesConsumedThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");

                    totalCharCount += charCountThisIteration;
                    if (totalCharCount < 0)
                    {
                        ThrowConversionOverflow();
                    }

                    bytes = bytes.Slice(bytesConsumedThisIteration);

                    if (!bytes.IsEmpty)
                    {
                        // There's still data remaining in the source buffer.
                        // We need to figure out why we weren't able to make progress.

                        OperationStatus opStatus = DecodeFirst(bytes, out _, out bytesConsumedThisIteration);

                        if (opStatus == OperationStatus.NeedMoreData)
                        {
                            Debug.Assert(bytesConsumedThisIteration == bytes.Length, "If returning NeedMoreData, should out the entire buffer length as bytes consumed.");
                        }

                        if (opStatus != OperationStatus.NeedMoreData || fallbackBuffer._decoder is null || fallbackBuffer._decoder.MustFlush)
                        {
                            // If the return code is anything other than NeedMoreData, assume the data in the buffer
                            // is invalid and run it through the fallback mechanism. Additionally, even if the status
                            // is NeedMoreData, if we must flush because there's no decoder active or the decode is
                            // requesting an explicit flush then we need to run the partial data through the fallback
                            // mechanism anyway.

                            charCountThisIteration = fallbackBuffer.InternalFallbackGetCharCount(bytes, bytesConsumedThisIteration);
                            Debug.Assert(bytesConsumedThisIteration >= 0, "Fallback mechanism shouldn't have returned a negative value.");

                            totalCharCount += charCountThisIteration;
                            if (totalCharCount < 0)
                            {
                                ThrowConversionOverflow();
                            }

                            bytes = bytes.Slice(bytesConsumedThisIteration);
                        }
                        else
                        {
                            // Mark remainder of input buffer as consumed. The next call to GetBytes will actually
                            // consume these by squirreling them away and producing 0 chars output.

                            bytes = ReadOnlySpan<byte>.Empty;
                        }
                    }
                } while (!bytes.IsEmpty);

                // Quick consistency check: we didn't pollute the fallback buffer on method exit.

                Debug.Assert(fallbackBuffer.Remaining == 0);

                return totalCharCount;
            }
        }

        /// <summary>
        /// Transcodes <paramref name="pBytes"/> to <paramref name="pChars"/>. If <paramref name="fallback"/> is
        /// specified, the method implementation may inspect properties of the instance in order to short-circuit
        /// the operation, but the method should not attempt to call <see cref="DecoderFallback.CreateFallbackBuffer"/>.
        /// </summary>
        /// <returns>
        /// Via <paramref name="bytesConsumed"/>, the amount of <paramref name="pBytes"/> consumed before terminating;
        /// and returns the total char count written to <paramref name="pChars"/>.
        /// </returns>
        private protected virtual unsafe int GetCharsNoFallbackBuffer(byte* pBytes, int byteCount, char* pChars, int charCount, DecoderFallback fallback, out int bytesConsumed)
        {
            // Ideally this method should be overridden by a subclassed type,
            // but we can provide a slow correct implementation.

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(pBytes, byteCount);
            Span<char> chars = new Span<char>(pChars, charCount);

            while (!bytes.IsEmpty)
            {
                // We don't care about statuses other than Done. The fallback mechanism will handle those.

                if ((DecodeFirst(bytes, out Rune value, out int bytesConsumedJustNow) != OperationStatus.Done)
                    || !value.TryEncode(chars, out int charsWrittenJustNow))
                {
                    break;
                }

                bytes = bytes.Slice(bytesConsumedJustNow);
                chars = chars.Slice(charsWrittenJustNow);
            }

            bytesConsumed = byteCount - bytes.Length;
            return charCount - chars.Length;
        }

        /// <summary>
        /// Transcodes bytes to chars, with no associated <see cref="DecoderNLS"/>. The first four arguments are
        /// based on the original input before invoking this method; and <paramref name="bytesConsumedSoFar"/>
        /// and <paramref name="charsWrittenSoFar"/> signal where in the provided buffers the fallback loop
        /// should begin operating. The behavior of this method is to call the <see cref="GetCharsNoFallbackBuffer"/>
        /// method as overridden by the specific type, and failing that go down the shared fallback path.
        /// </summary>
        /// <returns>
        /// The total number of chars written to <paramref name="pOriginalChars"/>, including <paramref name="charsWrittenSoFar"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the destination buffer is not large enough to hold the entirety of the transcoded data.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)] // don't stack spill spans into our caller
        private protected unsafe int GetCharsWithFallback(byte* pOriginalBytes, int originalByteCount, char* pOriginalChars, int originalCharCount, int bytesConsumedSoFar, int charsWrittenSoFar)
        {
            Debug.Assert(0 <= bytesConsumedSoFar && bytesConsumedSoFar < originalByteCount, "Invalid arguments provided to method.");
            Debug.Assert(0 <= charsWrittenSoFar && charsWrittenSoFar <= originalCharCount, "Invalid arguments provided to method.");

            // Using spans below helps keep our slicing logic simple.
            // We can use Unsafe.AsPointer later in this method since we know the spans are
            // constructed from existing fixed pointers.

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(pOriginalBytes, originalByteCount).Slice(bytesConsumedSoFar);
            Span<char> chars = new Span<char>(pOriginalChars, originalCharCount).Slice(charsWrittenSoFar);

            // First, see if we can transcode without instantiating the fallback buffer.
            // The subclassed Encoding instance may have optimized this code path.

            int charsWrittenJustNow = GetCharsNoFallbackBuffer(
                pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                byteCount: bytes.Length,
                pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                charCount: chars.Length,
                fallback: this.DecoderFallback,
                bytesConsumed: out int bytesConsumedJustNow);

            bytes = bytes.Slice(bytesConsumedJustNow);
            chars = chars.Slice(charsWrittenJustNow);

            // If we _still_ haven't consumed the entire source input, instantiate the fallback buffer
            // and go down the unoptimized shared code path.

            if (!bytes.IsEmpty)
            {
                DecoderFallbackBuffer fallbackBuffer = DecoderFallback.CreateFallbackBuffer(); // allocate a fresh new instance
                fallbackBuffer.InternalInitialize(this, null, originalByteCount);

                charsWrittenJustNow = GetCharsWithFallback(bytes, chars, fallbackBuffer);
                Debug.Assert(0 <= charsWrittenJustNow && charsWrittenJustNow <= chars.Length, "Invalid value returned by workhorse.");
                chars = chars.Slice(charsWrittenJustNow);
            }


            return originalCharCount - chars.Length; // total number of chars written
        }

        /// <summary>
        /// Transcodes bytes to chars, using the provided <see cref="DecoderFallbackBuffer"/> if fallback is necessary.
        /// </summary>
        /// <returns>
        /// The number of chars written to <paramref name="chars"/>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="fallbackBuffer"/> is backed by an <see cref="DecoderNLS"/> instance and if that instance allows
        /// incomplete transcoding of source data, the number of bytes converted by this operation will
        /// be subtracted from the <see cref="DecoderNLS._bytesUsed"/> field. (This assumes the caller optimistically
        /// set the field to a value indicating "conversion completed successfully.")
        /// </remarks>
        private unsafe int GetCharsWithFallback(ReadOnlySpan<byte> bytes, Span<char> chars, DecoderFallbackBuffer fallbackBuffer)
        {
            Debug.Assert(!bytes.IsEmpty, "Caller shouldn't invoke this method with an empty input buffer.");
            Debug.Assert(fallbackBuffer != null, "Fallback buffer should've been provided by the caller.");

            // Since we're using Unsafe.AsPointer in our central loop, we want to ensure everything is pinned.

            fixed (byte* _pBytes_Unused = &MemoryMarshal.GetReference(bytes))
            fixed (char* _pChars_Unused = &MemoryMarshal.GetReference(chars))
            {
                int originalCharsLength = chars.Length;

                do
                {
                    // First, transcode as much well-formed data as we can via the fast path.

                    int charsWrittenThisIteration = GetCharsNoFallbackBuffer(
                        pBytes: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)),
                        byteCount: bytes.Length,
                        pChars: (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(chars)),
                        charCount: chars.Length,
                        fallback: null, // wasn't able to be short-circuited by our caller; don't bother trying again
                        bytesConsumed: out int bytesConsumedThisIteration);

                    Debug.Assert(charsWrittenThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");
                    Debug.Assert(bytesConsumedThisIteration >= 0, "Workhorse shouldn't have returned a negative value.");

                    bytes = bytes.Slice(bytesConsumedThisIteration);
                    chars = chars.Slice(charsWrittenThisIteration);

                    if (!bytes.IsEmpty)
                    {
                        // There's still data remaining in the source buffer.
                        // We need to figure out why we weren't able to make progress.

                        switch (DecodeFirst(bytes, out _, out bytesConsumedThisIteration))
                        {
                            case OperationStatus.NeedMoreData:
                                Debug.Assert(bytesConsumedThisIteration == bytes.Length, "If returning NeedMoreData, should out the entire buffer length as bytes consumed.");
                                if (fallbackBuffer._decoder is null || fallbackBuffer._decoder.MustFlush)
                                {
                                    goto case OperationStatus.InvalidData; // see comment in GetCharCountWithFallback
                                }
                                else
                                {
                                    fallbackBuffer._decoder.SetLeftoverData(bytes); // squirrel away remaining data and finish
                                    bytes = ReadOnlySpan<byte>.Empty;
                                    goto Finish;
                                }

                            case OperationStatus.InvalidData:
                                if (fallbackBuffer.TryInternalFallbackGetChars(bytes, bytesConsumedThisIteration, chars, out charsWrittenThisIteration))
                                {
                                    // We successfully consumed some bytes, sent it through the fallback, and wrote some chars.

                                    Debug.Assert(charsWrittenThisIteration >= 0, "Fallback shouldn't have returned a negative value.");
                                    bytes = bytes.Slice(bytesConsumedThisIteration);
                                    chars = chars.Slice(charsWrittenThisIteration);
                                    break;
                                }
                                else
                                {
                                    // We generated fallback data, but the destination buffer wasn't large enough to hold it.
                                    // Don't mark any of the bytes we ran through the fallback as consumed, and terminate
                                    // the loop now and let our caller handle this condition.

                                    goto Finish;
                                }

                            default:
                                goto Finish; // no error on input, so destination must have been too small
                        }
                    }
                } while (!bytes.IsEmpty);

            Finish:

                // Quick consistency check: Unlike GetBytes, GetChars cannot leave data in the
                // fallback buffer between calls.

                Debug.Assert(fallbackBuffer.Remaining == 0);

                // The checks below are very similar to those at the end of GetBytesWithFallback (see comments there),
                // with the primary difference being that we don't need to inspect the fallback buffer for leftover
                // data because GetChars doesn't leave data in the fallback buffer between invocations.

                if (fallbackBuffer._decoder != null)
                {
                    fallbackBuffer._decoder._bytesUsed -= bytes.Length;
                    Debug.Assert(fallbackBuffer._decoder._bytesUsed >= 0, "Integer overflow detected; shouldn't have consumed more bytes than in source buffer.");
                }

                if (!bytes.IsEmpty)
                {
                    ThrowCharsOverflow(fallbackBuffer._decoder, nothingDecoded: false);
                }

                return originalCharsLength - chars.Length;
            }
        }
    }
}
