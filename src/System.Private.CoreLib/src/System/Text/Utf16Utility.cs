// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;

#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else // BIT64
using nint = System.Int32;
using nuint = System.UInt32;
#endif // BIT64

namespace System.Text
{
    internal static partial class Utf16Utility
    {

        /// <summary>
        /// Returns true iff the DWORD represents two ASCII UTF-16 characters in machine endianness.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool DWordAllCharsAreAscii(uint value)
        {
            return (value & ~0x007F007FU) == 0;
        }

#if BIT64
        /// <summary>
        /// Returns true iff the QWORD represents four ASCII UTF-16 characters in machine endianness.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool QWordAllCharsAreAscii(ulong value)
        {
            return (value & ~0x007F007F007F007FUL) == 0;
        }
#endif // BIT64

        /// <summary>
        /// Given a DWORD that represents two ASCII UTF-16 characters, returns the invariant
        /// lowercase representation of those characters. Requires the input value to contain
        /// two ASCII UTF-16 characters in machine endianness.
        /// </summary>
        /// <remarks>
        /// This is a branchless implementation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint ToLowerInvariantAsciiDWord(uint value)
        {
            // ASSUMPTION: Caller has validated that input value is ASCII.
            Debug.Assert(DWordAllCharsAreAscii(value));

            // the 0x80 bit of each word of 'lowerIndicator' will be set iff the word has value >= 'A'
            uint lowerIndicator = value + 0x00800080U - 0x00410041U;

            // the 0x80 bit of each word of 'upperIndicator' will be set iff the word has value <= 'Z'
            uint upperIndicator = value + 0x00800080U - 0x005B005BU;

            // the 0x80 bit of each word of 'combinedIndicator' will be set iff the word has value >= 'A' and <= 'Z'
            uint combinedIndicator = (lowerIndicator ^ upperIndicator);

            // the 0x20 bit of each word of 'mask' will be set iff the word has value >= 'A' and <= 'Z'
            uint mask = (combinedIndicator & 0x00800080U) >> 2;

            return value ^ mask; // bit flip uppercase letters [A-Z] => [a-z]
        }

#if BIT64
        /// <summary>
        /// Given a QWORD that represents four ASCII UTF-16 characters, returns the invariant
        /// lowercase representation of those characters. Requires the input value to contain
        /// four ASCII UTF-16 characters in machine endianness.
        /// </summary>
        /// /// <remarks>
        /// This is a branchless implementation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong ToLowerInvariantAsciiQWord(ulong value)
        {
            // ASSUMPTION: Caller has validated that input value is ASCII.
            Debug.Assert(QWordAllCharsAreAscii(value));

            // the 0x80 bit of each word of 'lowerIndicator' will be set iff the word has value >= 'A'
            ulong lowerIndicator = value + 0x0080008000800080UL - 0x0041004100410041UL;

            // the 0x80 bit of each word of 'upperIndicator' will be set iff the word has value <= 'Z'
            ulong upperIndicator = value + 0x0080008008000800UL - 0x005B005B005B005BUL;

            // the 0x80 bit of each word of 'combinedIndicator' will be set iff the word has value >= 'A' and <= 'Z'
            ulong combinedIndicator = (lowerIndicator ^ upperIndicator);

            // the 0x20 bit of each word of 'mask' will be set iff the word has value >= 'A' and <= 'Z'
            ulong mask = (combinedIndicator & 0x0080008000800080UL) >> 2;

            return value ^ mask; // bit flip uppercase letters [A-Z] => [a-z]
        }
#endif // BIT64

        /// <summary>
        /// Given a DWORD that represents two ASCII UTF-16 characters, returns the invariant
        /// uppercase representation of those characters. Requires the input value to contain
        /// two ASCII UTF-16 characters in machine endianness.
        /// </summary>
        /// <remarks>
        /// This is a branchless implementation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint ToUpperInvariantAsciiDWord(uint value)
        {
            // ASSUMPTION: Caller has validated that input value is ASCII.
            Debug.Assert(DWordAllCharsAreAscii(value));

            // the 0x80 bit of each word of 'lowerIndicator' will be set iff the word has value >= 'a'
            uint lowerIndicator = value + 0x00800080U - 0x00610061U;

            // the 0x80 bit of each word of 'upperIndicator' will be set iff the word has value <= 'z'
            uint upperIndicator = value + 0x00800080U - 0x007B007BU;

            // the 0x80 bit of each word of 'combinedIndicator' will be set iff the word has value >= 'a' and <= 'z'
            uint combinedIndicator = (lowerIndicator ^ upperIndicator);

            // the 0x20 bit of each word of 'mask' will be set iff the word has value >= 'a' and <= 'z'
            uint mask = (combinedIndicator & 0x00800080U) >> 2;

            return value ^ mask; // bit flip lowercase letters [a-z] => [A-Z]
        }

#if BIT64
        /// <summary>
        /// Given a QWORD that represents four ASCII UTF-16 characters, returns the invariant
        /// uppercase representation of those characters. Requires the input value to contain
        /// four ASCII UTF-16 characters in machine endianness.
        /// </summary>
        /// /// <remarks>
        /// This is a branchless implementation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong ToUpperInvariantAsciiQWord(ulong value)
        {
            // ASSUMPTION: Caller has validated that input value is ASCII.
            Debug.Assert(QWordAllCharsAreAscii(value));

            // the 0x80 bit of each word of 'lowerIndicator' will be set iff the word has value >= 'a'
            ulong lowerIndicator = value + 0x0080008000800080UL - 0x0061006100610061UL;

            // the 0x80 bit of each word of 'upperIndicator' will be set iff the word has value <= 'z'
            ulong upperIndicator = value + 0x0080008008000800UL - 0x007B007B007B007BUL;

            // the 0x80 bit of each word of 'combinedIndicator' will be set iff the word has value >= 'a' and <= 'z'
            ulong combinedIndicator = (lowerIndicator ^ upperIndicator);

            // the 0x20 bit of each word of 'mask' will be set iff the word has value >= 'a' and <= 'z'
            ulong mask = (combinedIndicator & 0x0080008000800080UL) >> 2;

            return value ^ mask; // bit flip lowercase letters [a-z] => [A-Z]
        }
#endif // BIT64
    }
}
