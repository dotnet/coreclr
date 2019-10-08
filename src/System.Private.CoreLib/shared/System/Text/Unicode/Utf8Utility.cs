// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Internal.Runtime.CompilerServices;

#pragma warning disable SA1121 // explicitly using type aliases instead of built-in types
#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else
using nint = System.Int32;
using nuint = System.UInt32;
#endif

namespace System.Text.Unicode
{
    internal static partial class Utf8Utility
    {
        /// <summary>
        /// The maximum number of bytes that can result from UTF-8 transcoding
        /// any Unicode scalar value.
        /// </summary>
        internal const int MaxBytesPerScalar = 4;

        /// <summary>
        /// The UTF-8 representation of <see cref="UnicodeUtility.ReplacementChar"/>.
        /// </summary>
        private static ReadOnlySpan<byte> ReplacementCharSequence => new byte[] { 0xEF, 0xBF, 0xBD };

        /// <summary>
        /// A <see cref="Vector128{SByte}"/> where each element has a value corresponding to its index.
        /// </summary>
        private static readonly Vector128<sbyte> VectorOfElementIndices = Vector128.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        /// <summary>
        /// Returns the byte index in <paramref name="utf8Data"/> where the first invalid UTF-8 sequence begins,
        /// or -1 if the buffer contains no invalid sequences. Also outs the <paramref name="isAscii"/> parameter
        /// stating whether all data observed (up to the first invalid sequence or the end of the buffer, whichever
        /// comes first) is ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int GetIndexOfFirstInvalidUtf8Sequence(ReadOnlySpan<byte> utf8Data, out bool isAscii)
        {
            fixed (byte* pUtf8Data = &MemoryMarshal.GetReference(utf8Data))
            {
                byte* pFirstInvalidByte = GetPointerToFirstInvalidByte(pUtf8Data, utf8Data.Length, out int utf16CodeUnitCountAdjustment, out _);
                int index = (int)(void*)Unsafe.ByteOffset(ref *pUtf8Data, ref *pFirstInvalidByte);

                isAscii = (utf16CodeUnitCountAdjustment == 0); // If UTF-16 char count == UTF-8 byte count, it's ASCII.
                return (index < utf8Data.Length) ? index : -1;
            }
        }

#if FEATURE_UTF8STRING
        /// <summary>
        /// Given a buffer <paramref name="utf8Data"/> which represents guaranteed well-formed UTF-8 data,
        /// returns the number of <see langword="char"/>s which would result from transcoding the buffer
        /// to UTF-16. The behavior of this method is undefined if <paramref name="utf8Data"/> contains
        /// any ill-formed UTF-8 subsequences.
        /// </summary>
        public static unsafe int GetUtf16CharCountFromKnownWellFormedUtf8(ReadOnlySpan<byte> utf8Data)
        {
            // Remember: the number of resulting UTF-16 chars will never be greater than the number
            // of UTF-8 bytes given well-formed input, so we can get away with casting the final
            // result to an 'int'.

            fixed (byte* pPinnedUtf8Data = &MemoryMarshal.GetReference(utf8Data))
            {
                if (Sse2.IsSupported && Popcnt.IsSupported)
                {
                    // Optimizations via SSE2 & POPCNT are available - use them.

                    Debug.Assert(BitConverter.IsLittleEndian, "SSE2 only supported on little-endian platforms.");
                    Debug.Assert(sizeof(nint) == IntPtr.Size, "nint defined incorrectly.");
                    Debug.Assert(sizeof(nuint) == IntPtr.Size, "nuint defined incorrectly.");

                    byte* pBuffer = pPinnedUtf8Data;
                    nuint bufferLength = (uint)utf8Data.Length;

                    // Optimization: Can we stay in the all-ASCII code paths?

                    nuint utf16CharCount = ASCIIUtility.GetIndexOfFirstNonAsciiByte(pBuffer, bufferLength);

                    if (utf16CharCount != bufferLength)
                    {
                        // Found at least one non-ASCII byte, so fall down the slower (but still vectorized) code paths.
                        // Given well-formed UTF-8 input, we can compute the number of resulting UTF-16 code units
                        // using the following formula:
                        //
                        // utf16CharCount = utf8ByteCount - numUtf8ContinuationBytes + numUtf8FourByteHeaders

                        utf16CharCount = bufferLength;

                        Vector128<sbyte> vecAllC0 = Vector128.Create(unchecked((sbyte)0xC0));
                        Vector128<sbyte> vecAll80 = Vector128.Create(unchecked((sbyte)0x80));
                        Vector128<sbyte> vecAll6F = Vector128.Create(unchecked((sbyte)0x6F));

                        {
                            // Perform an aligned read of the first part of the buffer.
                            // We'll mask out any data at the start of the buffer we don't care about.
                            //
                            // For example, if (pBuffer MOD 16) = 2:
                            // [ AA BB CC DD ... ] <-- original vector
                            // [ 00 00 CC DD ... ] <-- after PANDN operation

                            nint offset = -((nint)pBuffer & (sizeof(Vector128<sbyte>) - 1));
                            Vector128<sbyte> shouldBeMaskedOut = Sse2.CompareGreaterThan(Vector128.Create((byte)((int)offset + sizeof(Vector128<sbyte>) - 1)).AsSByte(), VectorOfElementIndices);
                            Vector128<sbyte> thisVector = Sse2.AndNot(shouldBeMaskedOut, Unsafe.Read<Vector128<sbyte>>(pBuffer + offset));

                            // If there's any data at the end of the buffer we don't care about, mask it out now.
                            // If this happens the 'bufferLength' value will be a lie, but it'll cause all of the
                            // branches later in the method to be skipped, so it's not a huge problem.

                            if (bufferLength < (nuint)offset + (uint)sizeof(Vector128<sbyte>))
                            {
                                Vector128<sbyte> shouldBeAllowed = Sse2.CompareLessThan(VectorOfElementIndices, Vector128.Create((byte)((int)bufferLength - (int)offset)).AsSByte());
                                thisVector = Sse2.And(shouldBeAllowed, thisVector);
                                bufferLength = (nuint)offset + (uint)sizeof(Vector128<sbyte>);
                            }

                            uint maskOfContinuationBytes = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(vecAllC0, thisVector));
                            uint countOfContinuationBytes = Popcnt.PopCount(maskOfContinuationBytes);
                            utf16CharCount -= countOfContinuationBytes;

                            uint maskOfFourByteHeaders = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.Xor(thisVector, vecAll80), vecAll6F));
                            uint countOfFourByteHeaders = Popcnt.PopCount(maskOfFourByteHeaders);
                            utf16CharCount += countOfFourByteHeaders;

                            bufferLength -= (nuint)offset;
                            bufferLength -= (uint)sizeof(Vector128<sbyte>);

                            pBuffer += offset;
                            pBuffer += (uint)sizeof(Vector128<sbyte>);
                        }

                        // At this point, pBuffer is guaranteed aligned.

                        Debug.Assert((nuint)pBuffer % (uint)sizeof(Vector128<sbyte>) == 0, "pBuffer should have been aligned.");

                        while (bufferLength >= (uint)sizeof(Vector128<sbyte>))
                        {
                            Vector128<sbyte> thisVector = Sse2.LoadAlignedVector128((sbyte*)pBuffer);

                            uint maskOfContinuationBytes = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(vecAllC0, thisVector));
                            uint countOfContinuationBytes = Popcnt.PopCount(maskOfContinuationBytes);
                            utf16CharCount -= countOfContinuationBytes;

                            uint maskOfFourByteHeaders = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.Xor(thisVector, vecAll80), vecAll6F));
                            uint countOfFourByteHeaders = Popcnt.PopCount(maskOfFourByteHeaders);
                            utf16CharCount += countOfFourByteHeaders;

                            pBuffer += sizeof(Vector128<sbyte>);
                            bufferLength -= (uint)sizeof(Vector128<sbyte>);
                        }

                        if ((uint)bufferLength > 0)
                        {
                            // There's still more data to be read.
                            // We need to mask out elements of the vector we don't care about.
                            // These elements will occur at the end of the vector.
                            //
                            // For example, if 14 bytes remain in the input stream:
                            // [ ... CC DD EE FF ] <-- original vector
                            // [ ... CC DD 00 00 ] <-- after PANDN operation

                            Vector128<sbyte> shouldBeMaskedOut = Sse2.CompareGreaterThan(VectorOfElementIndices, Vector128.Create((byte)((int)bufferLength - 1)).AsSByte());
                            Vector128<sbyte> thisVector = Sse2.AndNot(shouldBeMaskedOut, *(Vector128<sbyte>*)pBuffer);

                            uint maskOfContinuationBytes = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(vecAllC0, thisVector));
                            uint countOfContinuationBytes = Popcnt.PopCount(maskOfContinuationBytes);
                            utf16CharCount -= countOfContinuationBytes;

                            uint maskOfFourByteHeaders = (uint)Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.Xor(thisVector, vecAll80), vecAll6F));
                            uint countOfFourByteHeaders = Popcnt.PopCount(maskOfFourByteHeaders);
                            utf16CharCount += countOfFourByteHeaders;
                        }
                    }

                    return (int)utf16CharCount;
                }
                else
                {
                    // Cannot use SSE2 & POPCNT. Fall back to slower code paths.

                    byte* pbFirstInvalid = GetPointerToFirstInvalidByte(pPinnedUtf8Data, utf8Data.Length, out int utf16CodeUnitCountAdjustment, out _);
                    Debug.Assert(pbFirstInvalid == pPinnedUtf8Data + utf8Data.Length, "The input was not well-formed UTF-8!");
                    return utf8Data.Length + utf16CodeUnitCountAdjustment;
                }
            }
        }

        /// <summary>
        /// Returns a value stating whether <paramref name="utf8Data"/> contains only well-formed UTF-8 data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsWellFormedUtf8(ReadOnlySpan<byte> utf8Data)
        {
            fixed (byte* pUtf8Data = &MemoryMarshal.GetReference(utf8Data))
            {
                // The return value here will point to the end of the span if the data is well-formed.
                byte* pFirstInvalidByte = GetPointerToFirstInvalidByte(pUtf8Data, utf8Data.Length, out int _, out _);
                return (pFirstInvalidByte == (pUtf8Data + (uint)utf8Data.Length));
            }
        }

        /// <summary>
        /// Returns <paramref name="value"/> if it is null or contains only well-formed UTF-8 data;
        /// otherwises allocates a new <see cref="Utf8String"/> instance containing the same data as
        /// <paramref name="value"/> but where all invalid UTF-8 sequences have been replaced
        /// with U+FFFD.
        /// </summary>
        public static Utf8String ValidateAndFixupUtf8String(Utf8String value)
        {
            if (value.Length == 0)
            {
                return value;
            }

            ReadOnlySpan<byte> valueAsBytes = value.AsBytes();

            int idxOfFirstInvalidData = GetIndexOfFirstInvalidUtf8Sequence(valueAsBytes, out _);
            if (idxOfFirstInvalidData < 0)
            {
                return value;
            }

            // TODO_UTF8STRING: Replace this with the faster implementation once it's available.
            // (The faster implementation is in the dev/utf8string_bak branch currently.)

            MemoryStream memStream = new MemoryStream();
            memStream.Write(valueAsBytes.Slice(0, idxOfFirstInvalidData));

            valueAsBytes = valueAsBytes.Slice(idxOfFirstInvalidData);
            do
            {
                if (Rune.DecodeFromUtf8(valueAsBytes, out _, out int bytesConsumed) == OperationStatus.Done)
                {
                    // Valid scalar value - copy data as-is to MemoryStream
                    memStream.Write(valueAsBytes.Slice(0, bytesConsumed));
                }
                else
                {
                    // Invalid scalar value - copy U+FFFD to MemoryStream
                    memStream.Write(ReplacementCharSequence);
                }

                valueAsBytes = valueAsBytes.Slice(bytesConsumed);
            } while (!valueAsBytes.IsEmpty);

            bool success = memStream.TryGetBuffer(out ArraySegment<byte> memStreamBuffer);
            Debug.Assert(success, "Couldn't get underlying MemoryStream buffer.");

            return Utf8String.DangerousCreateWithoutValidation(memStreamBuffer, assumeWellFormed: true);
        }
#endif // FEATURE_UTF8STRING
    }
}
