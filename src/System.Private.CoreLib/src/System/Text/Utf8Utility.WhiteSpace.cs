// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

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
        /// Returns the index in <paramref name="utf8Data"/> where the first non-whitespace character
        /// appears, or the input length if the data contains only whitespace characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexOfFirstNonWhiteSpaceChar(ReadOnlySpan<byte> utf8Data)
        {
            return (int)GetIndexOfFirstNonWhiteSpaceChar(ref MemoryMarshal.GetReference(utf8Data), (uint)utf8Data.Length);
        }

        private static nuint GetIndexOfFirstNonWhiteSpaceChar(ref byte utf8Data, nuint length)
        {
            // This method is optimized for the case where the input data contains mostly ASCII
            // characters, and if it does begin with any whitespace characters it's probably a
            // very small number of characters (probably a handful of newlines or a tab).
            //
            // The full list of whitespace characters can be found at:
            // http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5Cp%7Bwhitespace%7D
            //
            // A naive implementation of this method would just call CharUnicodeInfo in a loop,
            // getting the category of each character seen. But we anticipate non-ASCII characters
            // being sufficiently common (and within that set, non-ASCII whitespace characters
            // being sufficiently rare) that we don't want to take the hit of parsing individual
            // scalars from the incoming data just to see if something is whitespace. Fortunately,
            // the set of whitespace characters is quite limited, so we can just hardcode the
            // UTF-8 sequences of all known whitespace characters.
            //
            // We treat invalid UTF-8 sequences as non-whitespace data.
            //
            // This method will need to be updated when the Unicode tables backing CharUnicodeInfo
            // change. A unit test should be sufficient to cover this.
            //
            // TODO: Add a unit test verifying this.

            for (nuint i = 0; i < length; /* increment will be done within branches below */)
            {
                byte firstByte = Unsafe.AddByteOffset(ref utf8Data, i);

                if ((sbyte)firstByte <= (sbyte)0x20U)
                {
                    // In the range [ 00 .. 20 ] (C0 control and space) or [ 80 .. FF ] (non-ASCII).

                    if (firstByte == 0x20U || UnicodeHelpers.IsInRangeInclusive(firstByte, 0x09U, 0x0DU))
                    {
                        // Space or C0 control
                        i++;
                        continue;
                    }
                    else if (firstByte == 0xC2U)
                    {
                        if (length - i >= 2)
                        {
                            byte secondByte = Unsafe.Add(ref Unsafe.AddByteOffset(ref utf8Data, i), 1);
                            if (secondByte == 0x85U || secondByte == 0xA0U)
                            {
                                // [ C2 85 ] => U+0085 NEXT LINE (NEL)
                                // [ C2 A0 ] => U+00A0 NO-BREAK SPACE
                                i += 2;
                                continue;
                            }
                        }
                    }
                    else if (UnicodeHelpers.IsInRangeInclusive(firstByte, 0xE1, 0xE3U))
                    {
                        if (length - i >= 3)
                        {
                            uint secondAndThirdByte = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref Unsafe.AddByteOffset(ref utf8Data, i), 1));
                            if (firstByte == 0xE1U)
                            {
                                if (secondAndThirdByte == ntoh16(0x9A80))
                                {
                                    // [ E1 9A 80 ] => U+1680 OGHAM SPACE MARK
                                    i += 3;
                                    continue;
                                }
                            }
                            else if (firstByte == 0xE2U)
                            {
                                // For this branch, we want to treat the integer as big-endian so that
                                // we can perform fast range checks.

                                secondAndThirdByte = ntoh16((ushort)secondAndThirdByte);

                                if (UnicodeHelpers.IsInRangeInclusive(secondAndThirdByte, 0x8080U, 0x808AU)
                                    || UnicodeHelpers.IsInRangeInclusive(secondAndThirdByte, 0x80A8U, 0x80A9U)
                                    || (secondAndThirdByte == 0x80AFU)
                                    || (secondAndThirdByte == 0x819FU))
                                {
                                    // [ E2 80 80 ] => U+2000 EN QUAD
                                    //   ...
                                    // [ E2 80 8A ] => U+200A HAIR SPACE
                                    // [ E2 80 A8 ] => U+2028 LINE SEPARATOR
                                    // [ E2 80 A9 ] => U+2029 PARAGRAPH SEPARATOR
                                    // [ E2 80 AF ] => U+202F NARROW NO-BREAK SPACE
                                    // [ E2 81 9F ] => U+205F MEDIUM MATHEMATICAL SPACE
                                    i += 3;
                                    continue;
                                }
                            }
                            else
                            {
                                Debug.Assert(firstByte == 0xE3U);
                                if (secondAndThirdByte == ntoh16(0x8080))
                                {
                                    // [ E3 80 80 ] => U+3000 IDEOGRAPHIC SPACE
                                    i += 3;
                                    continue;
                                }
                            }
                        }
                    }
                }

                return i; // found a non-whitespace char
            }

            return length; // only whitespace chars found
        }

        /// <summary>
        /// Returns the index in <paramref name="utf8Data"/> where the trailing whitespace sequence
        /// begins, or 0 if the data contains only whitespace characters, or the span length if the
        /// data does not end with any whitespace characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexOfTrailingWhiteSpaceSequence(ReadOnlySpan<byte> utf8Data)
        {
            return (int)GetIndexOfTrailingWhiteSpaceSequence(ref MemoryMarshal.GetReference(utf8Data), (uint)utf8Data.Length);
        }

        private static nuint GetIndexOfTrailingWhiteSpaceSequence(ref byte utf8Data, nuint length)
        {
            // See comments in GetIndexOfFirstNonWhiteSpaceChar for an overview of how this method
            // works. The key difference in this method is that we're reading the data backward,
            // so we'll have a combination of going backward and reading forward.

            for (nuint i = length; i > 0; /* decremeent will be done within branches below */)
            {
                byte finalByte = Unsafe.Add(ref Unsafe.AddByteOffset(ref utf8Data, i), -1);

                if ((sbyte)finalByte <= (sbyte)0x20U)
                {
                    // In the range [ 00 .. 20 ] (C0 control and space) or [ 80 .. FF ] (non-ASCII).

                    if (finalByte == 0x20U || UnicodeHelpers.IsInRangeInclusive(finalByte, 0x09U, 0x0DU))
                    {
                        // Space or C0 control
                        i--;
                        continue;
                    }
                    else if (i >= 2)
                    {
                        byte penultimateByte = Unsafe.Add(ref Unsafe.AddByteOffset(ref utf8Data, i), -2);
                        if (penultimateByte == 0xC2U)
                        {
                            if (finalByte == 0x85U || finalByte == 0xA0U)
                            {
                                // [ C2 85 ] => U+0085 NEXT LINE (NEL)
                                // [ C2 A0 ] => U+00A0 NO-BREAK SPACE
                                i -= 2;
                                continue;
                            }
                        }
                        else if (i >= 3)
                        {
                            byte thirdToLastByte = Unsafe.Add(ref Unsafe.AddByteOffset(ref utf8Data, i), -3);
                            if (UnicodeHelpers.IsInRangeInclusive(thirdToLastByte, 0xE1U, 0xE3U))
                            {
                                uint finalWordBigEndian = ((uint)penultimateByte << 8) | finalByte;
                                if (thirdToLastByte == 0xE1U)
                                {
                                    if (finalWordBigEndian == 0x9A80U)
                                    {
                                        // [ E1 9A 80 ] => U+1680 OGHAM SPACE MARK
                                        i -= 3;
                                        continue;
                                    }
                                }
                                else if (thirdToLastByte == 0xE2U)
                                {
                                    if (UnicodeHelpers.IsInRangeInclusive(finalWordBigEndian, 0x8080U, 0x808AU)
                                        || UnicodeHelpers.IsInRangeInclusive(finalWordBigEndian, 0x80A8U, 0x80A9U)
                                        || (finalWordBigEndian == 0x80AFU)
                                        || (finalWordBigEndian == 0x819FU))
                                    {
                                        // [ E2 80 80 ] => U+2000 EN QUAD
                                        //   ...
                                        // [ E2 80 8A ] => U+200A HAIR SPACE
                                        // [ E2 80 A8 ] => U+2028 LINE SEPARATOR
                                        // [ E2 80 A9 ] => U+2029 PARAGRAPH SEPARATOR
                                        // [ E2 80 AF ] => U+202F NARROW NO-BREAK SPACE
                                        // [ E2 81 9F ] => U+205F MEDIUM MATHEMATICAL SPACE
                                        i -= 3;
                                        continue;
                                    }
                                }
                                else
                                {
                                    Debug.Assert(thirdToLastByte == 0xE3U);
                                    if (finalWordBigEndian == 0x8080U)
                                    {
                                        // [ E3 80 80 ] => U+3000 IDEOGRAPHIC SPACE
                                        i -= 3;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                return i; // found a non-whitespace char
            }

            return 0; // only whitespace chars found
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ntoh16(ushort value)
        {
            // crossgen and JIT should constant fold this method if a constant literal parameter is provided.

            if (BitConverter.IsLittleEndian)
            {
                return BinaryPrimitives.ReverseEndianness(value);
            }
            else
            {
                return value;
            }
        }
    }
}
