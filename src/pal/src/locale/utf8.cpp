// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    unicode/utf8.c

Abstract:
    Functions to encode and decode UTF-8 strings

Revision History:



--*/

#include "pal/utf8.h"
#include "pal/dbgmsg.h"
#include "pal/unicode_data.h"

//
//  Constant Declarations.
//

#define ASCII                 0x007f

#define UTF8_2_MAX            0x07ff  // max UTF8 2-byte sequence (32 * 64 = 2048)
#define UTF8_1ST_OF_2         0xc0    // 110x xxxx
#define UTF8_1ST_OF_3         0xe0    // 1110 xxxx
#define UTF8_1ST_OF_4         0xf0    // 1111 xxxx
#define UTF8_TRAIL            0x80    // 10xx xxxx

#define HIGHER_6_BIT(u)       ((u) >> 12)
#define MIDDLE_6_BIT(u)       (((u) & 0x0fc0) >> 6)
#define LOWER_6_BIT(u)        ((u) & 0x003f)

#define BIT7(a)               ((a) & 0x80)
#define BIT6(a)               ((a) & 0x40)

#define HIGH_SURROGATE_START  0xd800
#define HIGH_SURROGATE_END    0xdbff
#define LOW_SURROGATE_START   0xdc00
#define LOW_SURROGATE_END     0xdfff

// Template class used by the UTF-8 to unicode and unicode to UTF-8 decoders
// for real writing of decoded characters to the destination buffer.
// The CharType template parameter represents the type of the destination
// buffer characters.
template<typename CharType>
class RealWriter
{
public:
    // Write character to the destination buffer at position cch
    static void Write(CharType* lpDestStr, int cch, CharType c)
    {
        lpDestStr[cch] = c;
    }

    // Test if there is enough space for count characters in the destination buffer
    // starting at position cch.
    static bool CanWrite(int cch, int cchDest, int count)
    {
        return (cch + count) <= cchDest;
    }
};

// Template class used by the UTF-8 to unicode and unicode to UTF-8 decoders
// for counting the number of decoded characters without writing them anywhere.
// The CharType template parameter represents the type of the destination
// buffer characters.
template <typename CharType>
class NullWriter
{
public:
    // Dummy version of writing a character to the destination buffer at 
    // position cch. Does nothing since there is no buffer.
    static void Write(CharType* lpDestStr, int cch, CharType c)
    {
    }

    // Dummy version of the test if there is enough space for count characters 
    // in the destination buffer starting at position cch. 
    // Returns always true since there is no buffer.
    static bool CanWrite(int cch, int cchDest, int count)
    {
        return true;
    }
};

// Check if the byte is a lead byte
bool IsLeadByte(BYTE b)
{
    return (b & 0xc0) == 0xc0;
}

// Check if the byte is a trail byte
bool IsTrailByte(BYTE b)
{
    return (b & 0xc0) == 0x80;
}

// Check if the byte is a single byte character encoding
bool IsSingleByte(BYTE b)
{
    return (b & 0x80) == 0;
}

// Test if the character in the buffer at pUTF8 is encoded
// using the shortest possible form. 
// The cchSrc is the number of bytes available at pUTF8,
// the nTB is the number of bytes used for the encoding.
bool IsShortestForm(CONST BYTE* pUTF8, int cchSrc, int nTB)
{
    bool isShortest = false;

    if (nTB <= cchSrc)
    {
        WORD word;

        switch (nTB)
        {
            case 1:
                // Make sure that bit 8 ~ bit 11 is not all zero.
                // 110XXXXx 10xxxxxx
                isShortest = ((*pUTF8 & 0x1e) != 0);
                break;
            case 2:
                // Look ahead to check for non-shortest form.
                // 1110XXXX 10Xxxxxx 10xxxxxx
                isShortest = !(((*pUTF8 & 0x0f) == 0) && (*(pUTF8 + 1) & 0x20) == 0);
                break;
            case 3:
                // This is a surrogate unicode pair
                word = (((WORD)*pUTF8) << 8) | *(pUTF8 + 1);
                // Look ahead to check for non-shortest form.
                // 11110XXX 10XXxxxx 10xxxxxx 10xxxxxx
                // Check if the 5 X bits are all zero.
                // 0x0730 == 00000111 00110000
                isShortest = !( (word & 0x0730) == 0 ||
                      // If the 21st bit is 1, we have extra work
                      ( (word & 0x0400) == 0x0400 &&
                         // The 21st bit is 1.
                         // Make sure that the resulting Unicode is within the valid surrogate range.
                         // The 4 byte code sequence can hold up to 21 bits, and the maximum valid code point range
                         // that Unicode (with surrogate) could represent are from U+000000 ~ U+10FFFF.
                         // Therefore, if the 21 bit (the most significant bit) is 1, we should verify that the 17 ~ 20
                         // bit are all zero.
                         // I.e., in 11110XXX 10XXxxxx 10xxxxxx 10xxxxxx,
                         // XXXXX can only be 10000.
                         // 0x0330 = 0000 0011 0011 0000
                        (word & 0x0330) != 0 ) );
                break;
        }
    }

    return isShortest;
}

// Templated version of the UTF8 to unicode conversion. The template
// parameter Writer provides static methods to write a decoded character
// and test for space available in the destination.
// It is used to distinguish between code that measures the size of the
// needed buffer and code that actually writes out the decoded characters.
template<typename Writer>
int UTF8ToUnicodeInternal(
    LPCSTR lpSrcStr,
    int cchSrc,
    LPWSTR lpDestStr,
    int cchDest,
    DWORD dwFlags
    )
{
    int nTB = 0;                   // # trail bytes to follow
    int cchWC = 0;                 // # of Unicode code points generated
    CONST BYTE* pUTF8 = (CONST BYTE*)lpSrcStr;
    DWORD dwUnicodeChar = 0;       // Our character with room for full surrogate char
    BOOL bSurrogatePair = FALSE;   // Indicate we're collecting a surrogate pair
    BOOL bCheckInvalidBytes = (dwFlags & MB_ERR_INVALID_CHARS);
    BYTE UTF8;
    DWORD dwError = ERROR_SUCCESS;

    do
    {
        while ((cchSrc != 0) && IsSingleByte(*pUTF8) && Writer::CanWrite(cchWC, cchDest, 1))
        {
            Writer::Write(lpDestStr, cchWC++, (WCHAR)*pUTF8++);
            cchSrc--;
        }

        if (cchSrc == 0)
        {
            // The whole input was processed
            break;
        }

        if (IsSingleByte(*pUTF8))
        {
            // Error: Buffer too small, we didn't process all characters character
            dwError = ERROR_INSUFFICIENT_BUFFER;
            break;
        }

        // Check for lead bytes
        if (IsLeadByte(*pUTF8))
        {
            //  Calculate the number of bytes to follow.
            //  Look for the first 0 from left to right.
            UTF8 = *pUTF8;
            nTB = 0;
            while (BIT7(UTF8) != 0)
            {
                UTF8 <<= 1;
                nTB++;
            }

            // Recover first bits of the character data from the byte
            UTF8 >>= nTB;

            //  Store the value from the first byte and decrement
            //  the number of bytes to follow.
            dwUnicodeChar = UTF8;
            nTB--;

            bool bIsShortestForm = IsShortestForm(pUTF8, cchSrc, nTB);

            if (bIsShortestForm)
            {
                bSurrogatePair = (nTB == 3);
            }
            else if (bCheckInvalidBytes)
            {
                dwError = ERROR_NO_UNICODE_TRANSLATION;
                break;
            }

            pUTF8++;
            cchSrc--;

            // Complete building the unicode character code from the trailing bytes
            while ((cchSrc != 0) && (nTB != 0) && IsTrailByte(*pUTF8))
            {
                // Add room for trail byte and add the trail byte value
                dwUnicodeChar <<= 6;
                dwUnicodeChar |= LOWER_6_BIT(*pUTF8);

                pUTF8++;
                cchSrc--;
                nTB--;
            }

            if (nTB != 0)
            {
                // Some trail bytes were missing either due to the end of the string or
                // due to an invalid interruption of the trail bytes sequence
                if (bCheckInvalidBytes) 
                {
                    dwError = ERROR_NO_UNICODE_TRANSLATION;
                    break;
                }

                // Process the byte that has interrupted the trail bytes or terminate the loop if
                // there is no more input
                continue;
            }

            // Ignore characters that were not encoded using the shortest form
            if (bIsShortestForm)
            {
                if (bSurrogatePair)
                {
                    if (Writer::CanWrite(cchWC, cchDest, 2))
                    {
                        Writer::Write(lpDestStr, cchWC++, (WCHAR)(((dwUnicodeChar - 0x10000) >> 10) + HIGH_SURROGATE_START));
                        Writer::Write(lpDestStr, cchWC++, (WCHAR)(((dwUnicodeChar - 0x10000) & 0x3ff) + LOW_SURROGATE_START));
                    }
                    else
                    {
                        // Error: Buffer too small, we didn't process this character
                        dwError = ERROR_INSUFFICIENT_BUFFER;
                        break;
                    }
                }
                else
                {
                    if (Writer::CanWrite(cchWC, cchDest, 1))
                    {
                        Writer::Write(lpDestStr, cchWC++, (WCHAR)dwUnicodeChar);
                    }
                    else
                    {
                        // Error: Buffer too small, we didn't process this character
                        dwError = ERROR_INSUFFICIENT_BUFFER;
                        break;
                    }
                }
            }
        }
        else
        {
            // Trail byte without a lead byte    
            if (bCheckInvalidBytes) 
            {
                dwError = ERROR_NO_UNICODE_TRANSLATION;
                break;
            }

            pUTF8++;
            cchSrc--;
        }
    }
    while (cchSrc != 0);

    if (dwError != ERROR_SUCCESS)
    {
        SetLastError(dwError);
        cchWC = 0;
    }

    //  Return the number of Unicode characters written.
    return cchWC;
}

////////////////////////////////////////////////////////////////////////////
//
//  UTF8ToUnicode
//
//  Maps a UTF-8 character string to its wide character string counterpart.
//
////////////////////////////////////////////////////////////////////////////

int UTF8ToUnicode(
    LPCSTR lpSrcStr,
    int cchSrc,
    LPWSTR lpDestStr,
    int cchDest,
    DWORD dwFlags
    )
{
    if (cchDest == 0)
    {
        return UTF8ToUnicodeInternal<NullWriter<WCHAR>>(lpSrcStr, cchSrc, lpDestStr, cchDest, dwFlags);
    }
    else
    {
        return UTF8ToUnicodeInternal<RealWriter<WCHAR>>(lpSrcStr, cchSrc, lpDestStr, cchDest, dwFlags);
    }
}

// Test if the wide character is a high surrogate
bool IsHighSurrogate(WCHAR c)
{
    return (c >= HIGH_SURROGATE_START) && (c <= HIGH_SURROGATE_END);
}

// Test if the wide character is a low surrogate
bool IsLowSurrogate(WCHAR c)
{
    return (c >= LOW_SURROGATE_START) && (c <= LOW_SURROGATE_END);
}

// Test if the wide character has one byte encoding in UTF-8
bool HasOneByteEncoding(WCHAR c)
{
    return c <= ASCII;
}

// Test if the wide character has two byte encoding in UTF-8
bool HasTwoByteEncoding(WCHAR c)
{
    return (c > ASCII) && (c <= UTF8_2_MAX);
}

// Test if the wide character has three byte encoding in UTF-8
bool HasThreeByteEncoding(WCHAR c)
{
    return c > UTF8_2_MAX;
}

// Templated version of the unicode to UTF-8 conversion. The template
// parameter Writer provides static methods to write a decoded character
// and test for space available in the destination.
// It is used to distinguish between code that measures the size of the
// needed buffer and code that actually writes out the decoded characters.
template<typename Writer>
int UnicodeToUTF8Internal(
    LPCWSTR lpSrcStr,
    int cchSrc,
    LPSTR lpDestStr,
    int cchDest)
{
    LPCWSTR lpWC = lpSrcStr;
    int     cchU8 = 0; // # of UTF8 chars generated

    do
    {
        while ((cchSrc != 0) && HasOneByteEncoding(*lpWC) && Writer::CanWrite(cchU8, cchDest, 1))
        {
            Writer::Write(lpDestStr, cchU8++, (CHAR)*lpWC);
            lpWC++;
            cchSrc--;
        }

        while ((cchSrc != 0) && HasTwoByteEncoding(*lpWC) && Writer::CanWrite(cchU8, cchDest, 2))
        {
            Writer::Write(lpDestStr, cchU8++, UTF8_1ST_OF_2 | (*lpWC >> 6));
            Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | LOWER_6_BIT(*lpWC));
            lpWC++;
            cchSrc--;
        }

        if (cchSrc != 0)
        {
            // If the character after the 2-byte encoded characters is again ASCII, take the fast path
            if (HasOneByteEncoding(*lpWC))
            {
                continue;
            }

            if (HasTwoByteEncoding(*lpWC))
            {
                // The destination buffer is full - the last two byte character didn't fit into the buffer
                break;
            }

            // Check if high surrogate is available
            if (IsHighSurrogate(*lpWC))
            {
                WCHAR wchHighSurrogate = *lpWC;

                // In correct Unicode string, the high surrogate needs to be immediatelly followed
                // by a low surrogate.
                // If the high surrogate was the last character in the input string or there is no
                // low surrogate after that, the high surrogate will be later emitted as a regular
                // 3 byte encoded character.
                if ((cchSrc != 1) && IsLowSurrogate(*(lpWC + 1)))
                {
                    // Move to the next character after the high surrogate
                    lpWC++;
                    cchSrc--;

                    // A surrogate pair was found
                    if (!Writer::CanWrite(cchU8, cchDest, 4))
                    {
                        // The destination buffer is full
                        break;
                    }

                    // 3 bits from 1st byte, 6 bits from 2nd byte, 6 bits from 3rd byte, 6 bits from 4th byte
                    DWORD dwSurrogateChar = (((wchHighSurrogate - HIGH_SURROGATE_START) << 10) + 
                                            (*lpWC - LOW_SURROGATE_START) + 0x10000);
                    Writer::Write(lpDestStr, cchU8++, UTF8_1ST_OF_4 | (unsigned char)(dwSurrogateChar >> 18)); 
                    Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | (unsigned char)((dwSurrogateChar >> 12) & 0x3f));
                    Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | (unsigned char)((dwSurrogateChar >> 6) & 0x3f));
                    Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | (unsigned char)(0x3f & dwSurrogateChar));

                    lpWC++;
                    cchSrc--;

                    // Process next input character
                    continue;
                }
            }

            // Here we have either a regular character that can be encoded using 3 bytes or an orphan
            // low or high surrogate

            if (Writer::CanWrite(cchU8, cchDest, 3))
            {
                // Write 3 byte encoded character
                Writer::Write(lpDestStr, cchU8++, UTF8_1ST_OF_3 | HIGHER_6_BIT(*lpWC));
                Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | MIDDLE_6_BIT(*lpWC));
                Writer::Write(lpDestStr, cchU8++, UTF8_TRAIL    | LOWER_6_BIT(*lpWC));
            }

            lpWC++;
            cchSrc--;
        }
    }
    while (cchSrc != 0);

    //  Make sure the destination buffer was large enough.
    if (cchDest && (cchSrc > 0))
    {
        SetLastError(ERROR_INSUFFICIENT_BUFFER);
        cchU8 = 0;
    }

    //  Return the number of UTF-8 characters written.
    return cchU8;
}

////////////////////////////////////////////////////////////////////////////
//
//  UnicodeToUTF8
//
//  Maps a Unicode character string to its UTF-8 string counterpart.
//
////////////////////////////////////////////////////////////////////////////

int UnicodeToUTF8(
    LPCWSTR lpSrcStr,
    int cchSrc,
    LPSTR lpDestStr,
    int cchDest)
{
    if (cchDest == 0)
    {
        return UnicodeToUTF8Internal<NullWriter<CHAR>>(lpSrcStr, cchSrc, lpDestStr, cchDest);
    }
    else
    {
        return UnicodeToUTF8Internal<RealWriter<CHAR>>(lpSrcStr, cchSrc, lpDestStr, cchDest);
    }
}
