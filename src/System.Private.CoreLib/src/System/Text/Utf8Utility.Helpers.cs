// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else // BIT64
using nint = System.Int32;
using nuint = System.UInt32;
#endif // BIT64

namespace System.Text
{
    internal static partial class Utf8Utility
    {
        /// <summary>
        /// Given a 24-bit integer which represents a three-byte buffer read in machine endianness,
        /// counts the number of consecutive ASCII bytes starting from the beginning of the buffer.
        /// Returns a value 0 - 3, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CountNumberOfLeadingAsciiBytesFrom24BitInteger(uint value)
        {
            // TODO: BMI & TZCNT support as optimization

            // The 'allBytesUpToNowAreAscii' DWORD uses bit twiddling to hold a 1 or a 0 depending
            // on whether all processed bytes were ASCII. Then we accumulate all of the
            // results to calculate how many consecutive ASCII bytes are present.

            value = ~value;

            if (BitConverter.IsLittleEndian)
            {
                // Read first byte
                uint allBytesUpToNowAreAscii = (value >>= 7) & 1;
                uint numAsciiBytes = allBytesUpToNowAreAscii;

                // Read second byte
                allBytesUpToNowAreAscii &= (value >>= 8);
                numAsciiBytes += allBytesUpToNowAreAscii;

                // Read third byte
                allBytesUpToNowAreAscii &= (value >>= 8);
                numAsciiBytes += allBytesUpToNowAreAscii;

                return numAsciiBytes;
            }
            else
            {
                // Read first byte
                uint allBytesUpToNowAreAscii = (value = ROL32(value, 1)) & 1;
                uint numAsciiBytes = allBytesUpToNowAreAscii;

                // Read second byte
                allBytesUpToNowAreAscii &= (value = ROL32(value, 8));
                numAsciiBytes += allBytesUpToNowAreAscii;

                // Read third byte
                allBytesUpToNowAreAscii &= (value = ROL32(value, 8));
                numAsciiBytes += allBytesUpToNowAreAscii;

                return numAsciiBytes;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> iff all bytes in <paramref name="value"/> are ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordAllBytesAreAscii(uint value)
        {
            return ((value & 0x80808080U) == 0U);
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the buffer contains two UTF-8 sequences
        /// that match the mask [ 110yyyyy 10xxxxxx 110yyyyy 10xxxxxx ]. This method *does not*
        /// validate that the sequences are well-formed; the caller must still perform
        /// overlong form checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsAndEndsWithUtf8TwoByteMask(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     const uint mask = 0xC0E0C0E0U;
            //     const uint comparand = 0x80C080C0U;
            //     return ((value & mask) == comparand);
            // }
            // else
            // {
            //     const uint mask = 0xE0C0E0C0U;
            //     const uint comparand = 0xC080C080U;
            //     return ((value & mask) == comparand);
            // }

            return (BitConverter.IsLittleEndian && (((value - 0x80C080C0U) & 0xC0E0C0E0U) == 0))
                || (!BitConverter.IsLittleEndian && (((value - 0xC080C080U) & 0xE0C0E0C0U) == 0));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the first two bytes of the buffer are
        /// an overlong representation of a sequence that should be represented as one byte.
        /// This method *does not* validate that the sequence matches the appropriate
        /// 2-byte sequence mask (see <see cref="DWordBeginsWithUtf8TwoByteMask"/>).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsWithOverlongUtf8TwoByteSequence(uint value)
        {
            // ASSUMPTION: Caller has already checked the '110yyyyy 10xxxxxx' mask of the input.
            Debug.Assert(DWordBeginsWithUtf8TwoByteMask(value));

            // Per Table 3-7, first byte of two-byte sequence must be within range C2 .. DF.
            // Since we already validated it's 80 <= ?? <= DF (per mask check earlier), now only need
            // to check that it's < C2.

            return (BitConverter.IsLittleEndian && ((byte)value < (byte)0xC2))
                || (!BitConverter.IsLittleEndian && (value < 0xC2000000U));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the first four bytes of the buffer match
        /// the UTF-8 4-byte sequence mask [ 11110www 10zzzzzz 10yyyyyy 10xxxxxx ]. This
        /// method *does not* validate that the sequence is well-formed; the caller must
        /// still perform overlong form or out-of-range checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsWithUtf8FourByteMask(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     const uint mask = 0xC0C0C0F8U;
            //     const uint comparand = 0x808080F0U;
            //     return ((value & mask) == comparand);
            // }
            // else
            // {
            //     const uint mask = 0xF8C0C0C0U;
            //     const uint comparand = 0xF0808000U;
            //     return ((value & mask) == comparand);
            // }

            return (BitConverter.IsLittleEndian && ((value & 0xC0C0C0F8U) == 0x808080F0U))
                   || (!BitConverter.IsLittleEndian && ((value & 0xF8C0C0C0U) == 0xF0808000U));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the first three bytes of the buffer match
        /// the UTF-8 3-byte sequence mask [ 1110zzzz 10yyyyyy 10xxxxxx ]. This method *does not*
        /// validate that the sequence is well-formed; the caller must still perform
        /// overlong form or surrogate checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsWithUtf8ThreeByteMask(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     const uint mask = 0x00C0C0F0U;
            //     const uint comparand = 0x008080E0U;
            //     return ((value & mask) == comparand);
            // }
            // else
            // {
            //     const uint mask = 0xF0C0C000U;
            //     const uint comparand = 0xE0808000U;
            //     return ((value & mask) == comparand);
            // }

            return (BitConverter.IsLittleEndian && (((value - 0x008080E0U) & 0x00C0C0F0U) == 0))
                || (!BitConverter.IsLittleEndian && (((value - 0xE0808000U) & 0xF0C0C000U) == 0));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the first two bytes of the buffer match
        /// the UTF-8 2-byte sequence mask [ 110yyyyy 10xxxxxx ]. This method *does not*
        /// validate that the sequence is well-formed; the caller must still perform
        /// overlong form checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsWithUtf8TwoByteMask(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     const uint mask = 0x0000C0E0U;
            //     const uint comparand = 0x000080C0U;
            //     return ((value & mask) == comparand);
            // }
            // else
            // {
            //     const uint mask = 0xE0C00000U;
            //     const uint comparand = 0xC0800000U;
            //     return ((value & mask) == comparand);
            // }

            return (BitConverter.IsLittleEndian && (((value - 0x000080C0U) & 0x000C0E0U) == 0))
                || (!BitConverter.IsLittleEndian && (((value - 0xC0800000U) & 0xE0C00000U) == 0));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the first two bytes of the buffer are
        /// an overlong representation of a sequence that should be represented as one byte.
        /// This method *does not* validate that the sequence matches the appropriate
        /// 2-byte sequence mask (see <see cref="DWordBeginsWithUtf8TwoByteMask"/>).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordEndsWithOverlongUtf8TwoByteSequence(uint value)
        {
            // ASSUMPTION: Caller has already checked the '110yyyyy 10xxxxxx' mask of the input.
            Debug.Assert(DWordEndsWithUtf8TwoByteMask(value));

            // Per Table 3-7, first byte of two-byte sequence must be within range C2 .. DF.
            // We already validated that it's 80 .. DF (per mask check earlier).
            // C2 = 1100 0010
            // DF = 1101 1111
            // This means that we can AND the leading byte with the mask 0001 1110 (1E),
            // and if the result is zero the sequence is overlong.

            return (BitConverter.IsLittleEndian && ((value & 0x001E0000U) == 0U))
                || (!BitConverter.IsLittleEndian && ((value & 0x1E00U) == 0U));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the last two bytes of the buffer match
        /// the UTF-8 2-byte sequence mask [ 110yyyyy 10xxxxxx ]. This method *does not*
        /// validate that the sequence is well-formed; the caller must still perform
        /// overlong form checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordEndsWithUtf8TwoByteMask(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     const uint mask = 0xC0E00000U;
            //     const uint comparand = 0x80C00000U;
            //     return ((value & mask) == comparand);
            // }
            // else
            // {
            //     const uint mask = 0x0000E0C0U;
            //     const uint comparand = 0x0000C080U;
            //     return ((value & mask) == comparand);
            // }

            return (BitConverter.IsLittleEndian && (((value - 0x80C00000U) & 0xC0E00000U) == 0))
                || (!BitConverter.IsLittleEndian && (((value - 0x0000C080U) & 0x0000E0C0U) == 0));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD on a little-endian machine,
        /// returns <see langword="true"/> iff the first two bytes of the buffer are a well-formed
        /// UTF-8 two-byte sequence. This wraps the mask check and the overlong check into a
        /// single operation. Returns <see langword="false"/> if running on a big-endian machine.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordBeginsWithValidUtf8TwoByteSequenceLittleEndian(uint value)
        {
            // Per Table 3-7, valid 2-byte sequences are [ C2..DF ] [ 80..BF ].
            // In little-endian, that would be represented as:
            // [ ######## ######## 10xxxxxx 110yyyyy ].
            // Due to the little-endian representation we can perform a trick by ANDing the low
            // WORD with the bitmask [ 11000000 11111111 ] and checking that the value is within
            // the range [ 11000000_11000010, 11000000_11011111 ]. This performs both the
            // 2-byte-sequence bitmask check and overlong form validation with one comparison.

            Debug.Assert(BitConverter.IsLittleEndian);

            return (BitConverter.IsLittleEndian && UnicodeHelpers.IsInRangeInclusive(value & 0xC0FFU, 0x80C2U, 0x80DFU))
                || (!BitConverter.IsLittleEndian && false); // this line - while weird - helps JITter produce optimal code
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD on a little-endian machine,
        /// returns <see langword="true"/> iff the last two bytes of the buffer are a well-formed
        /// UTF-8 two-byte sequence. This wraps the mask check and the overlong check into a
        /// single operation. Returns <see langword="false"/> if running on a big-endian machine.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordEndsWithValidUtf8TwoByteSequenceLittleEndian(uint value)
        {
            // See comments in DWordBeginsWithValidUtf8TwoByteSequenceLittleEndian.

            Debug.Assert(BitConverter.IsLittleEndian);

            return (BitConverter.IsLittleEndian && UnicodeHelpers.IsInRangeInclusive(value & 0xC0FF0000U, 0x80C20000U, 0x80DF0000U))
                || (!BitConverter.IsLittleEndian && false); // this line - while weird - helps JITter produce optimal code
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the fourth byte of the buffer is ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordFourthByteIsAscii(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     return ((int)value >= 0);
            // }
            // else
            // {
            //     return ((value & 0x80U) == 0U);
            // }

            return (BitConverter.IsLittleEndian && ((int)value >= 0))
                || (!BitConverter.IsLittleEndian && ((value & 0x80U) == 0U));
        }

        /// <summary>
        /// Given a UTF-8 buffer which has been read into a DWORD in machine endianness,
        /// returns <see langword="true"/> iff the third byte of the buffer is ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DWordThirdByteIsAscii(uint value)
        {
            // The code in this method is equivalent to the code
            // below, but the JITter is able to inline + optimize it
            // better in release builds.
            //
            // if (BitConverter.IsLittleEndian)
            // {
            //     return ((value & 0x800000U) == 0U);
            // }
            // else
            // {
            //     return ((value & 0x8000U) == 0U);
            // }

            return (BitConverter.IsLittleEndian && ((value & 0x800000U) == 0U))
                || (!BitConverter.IsLittleEndian && ((value & 0x8000U) == 0U));
        }

        /// <summary>
        /// Given a memory reference, returns the number of bytes that must be added to the reference
        /// before the reference is DWORD-aligned. Returns a number in the range 0 - 3, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nuint GetNumberOfBytesToNextDWordAlignment(ref byte @ref)
        {
            // return (-&ref) & 3;
            return (nuint)(-(nint)Unsafe.AsPointer(ref @ref)) & 3;
        }

        /// <summary>
        /// Returns the OR of the next two DWORDs in the buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadAndFoldTwoDWordsUnaligned(ref byte buffer)
        {
            return Unsafe.ReadUnaligned<uint>(ref buffer)
                | Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref buffer, sizeof(uint)));
        }

        /// <summary>
        /// Returns the OR of the next two QWORDs in the buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadAndFoldTwoQWordsUnaligned(ref byte buffer)
        {
            return Unsafe.ReadUnaligned<ulong>(ref buffer)
                | Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref buffer, sizeof(ulong)));
        }

        /// <summary>
        /// Rotates a DWORD left. The JITter is smart enough to turn this into a ROL / ROR instruction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ROL32(uint value, int shift) => (value << shift) | (value >> (32 - shift));

        /// <summary>
        /// Returns <see langword="true"/> iff all bytes in <paramref name="value"/> are ASCII.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool QWordAllBytesAreAscii(ulong value)
        {
            return ((value & 0x8080808080808080UL) == 0UL);
        }
    }
}
