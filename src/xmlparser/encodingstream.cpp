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

#include "core.h"
#include "xmlhelper.h"
#include "encodingstream.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

const int EncodingStream::BUFFERSIZE = 4096*sizeof(WCHAR);
//////////////////////////////////////////////////////////////////////////////////
EncodingStream::EncodingStream(IStream * pStream): encoding(NULL), buf(NULL), stream(pStream)
{

    pfnWideCharFromMultiByte = NULL;
    btotal = bnext = startAt = 0;
    lastBuffer = false;
    bufsize = 0;
    _fEOF = false;
    _fReadStream = true;
    _fUTF8BOM = false;
    //_fTextXML = false;
    //_fSetCharset = false;
    _dwMode = 0;
    codepage = CP_UNDEFINED;
}
//////////////////////////////////////////////////////////////////////////////////
/**
 * Builds the EncodingStream for input.
 * Reads the first two bytes of the InputStream * in order to make a guess
 * as to the character encoding of the file.
 */
IStream * EncodingStream::newEncodingStream(IStream * pStream)
{
    EncodingStream * es = NEW (EncodingStream(pStream));
    if (es == NULL)
        return NULL;

    es->AddRef(); // xwu@@ : check this addRef()!

    es->isInput = true;
    es->buf = NULL;

    return es;
}
//////////////////////////////////////////////////////////////////////////////////
EncodingStream::~EncodingStream()
{
    if (buf)
        delete [] buf;
    if (encoding != NULL)
        delete encoding;

    stream = NULL; // smart pointer
}
//////////////////////////////////////////////////////////////////////////////////
/**
 * Reads characters from stream and encode it to Unicode
 */
HRESULT STDMETHODCALLTYPE EncodingStream::Read(void * pv, ULONG cb, ULONG * pcbRead)
{
    HRESULT hr;
    
    ULONG num = 0;

    if (pcbRead != NULL)
        *pcbRead = 0;

    if (btotal == 0 && _fEOF)          // we already hit EOF - so return right away.
        return S_OK;

    // Calculate how many UNICODE chars we are allowed to return, 
    //                         which is the same as the number of BYTES read from the file
    cb /= sizeof(WCHAR);    
    checkhr2(prepareForInput(cb));

    if (stream && _fReadStream)
    {
        // btotal = number of bytes already in start of buffer.
        if (cb > btotal)
        {
            hr = stream->Read(buf + btotal, cb - btotal, &num);
            if (hr == E_PENDING && num > 0)
            {
                // in which case we ignore the error, and continue on !!.
                hr = 0;
            }
            if (FAILED(hr))
            {
                return hr;
            }
            if (btotal == 0 && num == 0)
            {
                _fEOF = true;
                return hr;
            }
        }
        else
        {
            hr = S_OK;
        }
    }
    else if (btotal == 0)
    {
    	return (lastBuffer) ? S_FALSE : E_PENDING;
    }

    btotal += num;
    UINT b = btotal, utotal = cb;

    if (b > cb)
    {
        // If we have more bytes in our buffer than the caller has
        // room for, then only return the number of bytes the caller
        // asked for -- otherwise pfnWideCharFromMultiByte will write
        // off the end of the caller's buffer.
        b = cb;
    }
    if (pfnWideCharFromMultiByte == NULL) // first read() call
    {
        checkhr2(autoDetect());
        if (pfnWideCharFromMultiByte == NULL) // failed to fully determine encoding
            return (lastBuffer) ? S_FALSE : E_PENDING;
        b -= bnext;
        startAt -= bnext;
    }
    hr = (this->pfnWideCharFromMultiByte)(&_dwMode, codepage, buf + bnext, &b, (WCHAR *)pv, &utotal);
    if (hr != S_OK)
        return hr;	
    if (b == 0 && num == 0 && (stream || lastBuffer))
    {
        // stream says we're at the end, but pfnWideCharFromMultiByte
        // disagrees !!

        return XML_E_INCOMPLETE_ENCODING;
    }
    bnext += b;
    if (pcbRead != NULL)
        *pcbRead = utotal*sizeof(WCHAR);
    return (utotal == 0) ? E_PENDING : S_OK;
} 
//////////////////////////////////////////////////////////////////////////////////
/**
 * Checks the first two/four bytes of the input Stream in order to 
 * detect UTF-16/UCS-4 or UTF-8 encoding;
 * otherwise assume it is UTF-8

 *                                                                                              
 */
HRESULT EncodingStream::autoDetect()
{
    // wait until we have enough to be sure.
    if (btotal < 2)
        return S_OK;

    unsigned int guess = (((unsigned char)buf[0]) << 8) + ((unsigned char)buf[1]);
    HRESULT hr;

    if (guess != 0xFEFF && guess != 0xFFFE)
    {
        // No BOM found. Let's see if we can infer one.

        if (buf[0] == 0 && buf[1] != 0)
        {
            // This looks to be UTF-16 Big endian
            guess = 0xFEFF;
        }
        else if (buf[0] != 0 && buf[1] == 0)
        {
            // Looks to be UTF-16 little endian
            guess = 0xFFFE;
        }

        // Otherwise, we can't identify this encoding. We'll assume it's UTF8
    }

    if (guess == 0xFEFF || guess == 0xFFFE)  // BOM found
    {
        // wait until we have enough to be sure.
        if (btotal < 4)
            return S_OK;
		
        unsigned int guess1 = (((unsigned char)buf[2]) << 8) + ((unsigned char)buf[3]);
        if (guess == guess1)
        {			
            /*
			if (!encoding)
            {
                static const WCHAR* wchUCS4 = TEXT("UCS-4");
                encoding = Encoding::newEncoding(wchUCS4, 5, (0xFFFE == guess), true);
            }
            bnext = 4;	
			*/
			// FUSION_XML_PARSER does not support UCS4
			return XML_E_INVALIDENCODING;
        }
        else
        {
            if (!encoding)
            {   
                // Note, -1 below to remove the count for the NULL

                if (guess == 0xFFFE)
                    encoding = Encoding::newEncoding(L"UTF-16LE", NumItems(L"UTF-16LE")-1, true, true);
                else
                    encoding = Encoding::newEncoding(L"UTF-16BE", NumItems(L"UTF-16BE")-1, false, true);
            }
            bnext = 2;
        }

        if (NULL == encoding)
            return E_OUTOFMEMORY; 
        
        encoding->littleendian =  (0xFFFE == guess);
    }
    else
    {
        if (!encoding)
        {
            encoding = Encoding::newEncoding(); // default encoding : UTF-8 
            if (NULL == encoding)
                return E_OUTOFMEMORY;
        }

        // In some system, such as win2k, there is BOM 0xEF BB BF for UTF8
        if (guess == 0xEFBB)
        {
            if (btotal < 3)
                return S_OK;
			
            if (buf[2] == 0xBF)
                _fUTF8BOM = true; 
			
            bnext = 3; 
        }
        else
        {
            encoding->byteOrderMark = false;
        }
    }

    checkhr2(CharEncoder::getWideCharFromMultiByteInfo(encoding, &codepage, &pfnWideCharFromMultiByte, &maxCharSize));
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////////////////
/**
 * Switchs the character encoding of the input stream
 * Returns:
 *         S_OK: succeeded, and do not need re-read
 *         S_FALSE: succeeded, needs to re-read from <code> newPosition </code>
 *         Otherwise: error code
 * Notice: 
 *         This method only works for input stream, newPosition starts with 1
 */
HRESULT EncodingStream::switchEncodingAt(Encoding * newEncoding, int newPosition)
{

    int l = newPosition - startAt;
    if (l < 0 || l > (int)bnext) 
    {
        // out of range
        delete newEncoding;
        return E_INVALIDARG;
    }

    UINT newcodepage;
    UINT newCharSize;
    //
    // get and check charset information
    //
    WideCharFromMultiByteFunc * pfn;
    HRESULT hr = CharEncoder::getWideCharFromMultiByteInfo(newEncoding, &newcodepage, &pfn, &newCharSize);
    if (hr != S_OK)
    {
        delete newEncoding;
        return E_INVALIDARG;
    }
    if (codepage == newcodepage)
    {
        delete newEncoding;
        return S_OK;
    }

    // Ok, then, let's make the switch.
    delete encoding;
    encoding = newEncoding;
    maxCharSize = newCharSize;
    codepage = newcodepage;
    pfnWideCharFromMultiByte = pfn;

    // Because the XML declaration is encoded in UTF-8, 
    // Mapping input characters to wide characters is one-to-one mapping
    if ((int)bnext != l)
    {
        bnext = l;
        return S_FALSE;
    }
    return S_OK;
}

HRESULT EncodingStream::checkNewEncoding(const WCHAR **charset, ULONG * len)
{
    // If the character set that we want to switch to is UCS-2, and we're already in a 
    // UTF-16 character set, then we don't need to make the change.
    //
    // If we're not in a UTF-16 set, then we'll default to using UTF-16LE for the UCS-2
    // character set.

    if (!_wcsnicmp(*charset, L"ucs-2", *len))
    {
        // If the current encoding is UTF-16, then we don't need to make a change

        if (codepage == 1200 || codepage == 1201)
            return S_FALSE;


        // Otherwise, let's make ucs-2 be UTF-16LE
        *charset = L"utf-16le";

        // -1 so len doesn't include the null terminator
        *len = NumItems(L"utf-16le") -1;
    }

    return S_OK;
}// checkNewEncoding


//////////////////////////////////////////////////////////////////////////////////
// minlen is the number of UNICODE, which is the same number of byte we read from the file 
HRESULT EncodingStream::prepareForInput(ULONG minlen)
{
//    Assert(btotal >= bnext);
    btotal -= bnext;

    if (bufsize < minlen)
    {
        BYTE* newbuf = NEW (BYTE[minlen]);
        if (newbuf == NULL) { 
            return E_OUTOFMEMORY;
        }

        if (buf){
            ::memcpy(newbuf, buf+bnext, btotal);
            delete[] buf;
        }

        buf = newbuf;
        bufsize = minlen;
    }
    else if (bnext > 0 && btotal > 0)
    {
        // Shift remaining bytes down to beginning of buffer.
        ::memmove(buf, buf + bnext, btotal);          
    }

    startAt += bnext;
    bnext = 0; 
    return S_OK;
}
//////////////////////////////////////////////////////////////////////////////////
HRESULT EncodingStream::AppendData( const BYTE* buffer, ULONG length, BOOL fLastBuffer)
{
    Assert(btotal >= bnext);
    lastBuffer = (fLastBuffer != FALSE);
    HRESULT hr;
    ULONG minlen = length + (btotal - bnext); // make sure we don't loose any data
    if (minlen < (ULONG) BUFFERSIZE)
        minlen = BUFFERSIZE;
    checkhr2( prepareForInput(minlen)); // guarantee enough space in the array
    
    if (length > 0 && buffer != NULL){
        // Copy raw data into new buffer.
        ::memcpy(buf + btotal, buffer, length);
        btotal += length;
    }
	if (pfnWideCharFromMultiByte == NULL) // first AppendData call
    {
        checkhr2(autoDetect());
    }
    

    return hr;
}
//////////////////////////////////////////////////////////////////////////////////
HRESULT EncodingStream::BufferData()
{
    HRESULT hr = S_OK;
    checkhr2(prepareForInput(0)); // 0 is used just for shift down (so bnext=0).

    if (_fEOF)          // already hit the end of the stream.
        return S_FALSE;

    const DWORD BUFSIZE = 4096;

    DWORD dwRead = 1;

    while (S_OK == hr && dwRead > 0)
    {
        // if we cannot fit another buffer full, then re-allocate.
        DWORD minsize = (btotal+BUFSIZE > bufsize) ? bufsize + BUFSIZE : bufsize;
        checkhr2( prepareForInput(minsize)); // make space available.

        dwRead = 0;
        hr = stream->Read(buf + btotal, BUFSIZE, &dwRead);
        btotal += dwRead;
    }

    if (SUCCEEDED(hr) && dwRead == 0)
    {
        _fEOF = true;
        hr = S_FALSE; // return S_FALSE when at eof.
    }
    return hr;
}


