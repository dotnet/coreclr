// ==++==
// 
//   
//    Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//   
//    The use and distribution terms for this software are contained in the file
//    named license.txt, which can be found in the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by the
//    terms of this license.
//   
//    You must not remove this notice, or any other, from this software.
//   
// 
// ==--==
/*
 *                                                
 * 
 */
#ifndef _FUSION_XMLPARSER__CHARENCODER_HXX
#define _FUSION_XMLPARSER__CHARENCODER_HXX
//#include "codepage.h"



typedef HRESULT WideCharFromMultiByteFunc(DWORD* pdwMode, CODEPAGE codepage, BYTE * bytebuffer, 
                         UINT * cb, WCHAR * buffer, UINT * cch);

struct EncodingEntry
{
    UINT codepage;
    WCHAR * charset;
    UINT  maxCharSize;
    WideCharFromMultiByteFunc * pfnWideCharFromMultiByte;
};

class Encoding
{
protected: 
    Encoding() {};

public:

    // default encoding is UTF-8.
    static Encoding* newEncoding(const WCHAR * s = TEXT("UTF-8"), ULONG len = 5, bool endian = false, bool mark = false);
    virtual ~Encoding();
    WCHAR * charset;        // charset 
    bool    littleendian;   // endian flag for UCS-2/UTF-16 encoding, true: little endian, false: big endian
    bool    byteOrderMark;  // byte order mark (BOM) flag, BOM appears when true
};

/**
 * 
 * An Encoder specifically for dealing with different encoding formats 
 *                                     
 */

class CharEncoder
{
    //
    // class CharEncoder is a utility class, makes sure no instance can be defined
    //
    private: virtual int charEncoder() = 0;

public:

    static HRESULT getWideCharFromMultiByteInfo(Encoding * encoding, CODEPAGE * pcodepage, WideCharFromMultiByteFunc ** pfnWideCharFromMultiByte, UINT * mCharSize);

    /**
     * Encoding functions: get Unicode from other encodings
     */
    static WideCharFromMultiByteFunc wideCharFromMultiByteWin32;

    // actually, we only use these three functions for UCS-2 and UTF-8
	static WideCharFromMultiByteFunc wideCharFromUtf8;
    static WideCharFromMultiByteFunc wideCharFromUcs2Bigendian;
    static WideCharFromMultiByteFunc wideCharFromUcs2Littleendian;

    /**
     * Encoding functions: from Unicode to other encodings
     */

    static int getCharsetInfo(const WCHAR * charset, CODEPAGE * pcodepage, UINT * mCharSize);

private: 
private:


    static const EncodingEntry charsetInfo [];
};

#endif // _FUSION_XMLPARSER__CHARENCODER_HXX
