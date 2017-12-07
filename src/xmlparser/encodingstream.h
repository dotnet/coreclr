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
/////////////////////////////////////////////////////////////////////////////////
//
// fusion\xmlparser\EncodingStream.hxx
//
/////////////////////////////////////////////////////////////////////////////////
#ifndef _FUSION_XMLPARSER__ENCODINGSTREAM_H_INCLUDE_
#define _FUSION_XMLPARSER__ENCODINGSTREAM_H_INCLUDE_

#include "codepage.h"
#include "charencoder.h"
#include "core.h"				//UNUSED() is used
#include <ole2.h>
#include <xmlparser.h>
#include <objbase.h>
typedef _reference<IStream> RStream;

class EncodingStream : public _unknown<IStream, &IID_IStream>
{
protected:

    EncodingStream(IStream * stream);
    ~EncodingStream();

public:
	// create an EncodingStream for input
    static IStream * newEncodingStream(IStream * stream);

    HRESULT STDMETHODCALLTYPE Read(void * pv, ULONG cb, ULONG * pcbRead);

    HRESULT STDMETHODCALLTYPE Write(void const* pv, ULONG cb, ULONG * pcbWritten)
    {
		UNUSED(pv);
		UNUSED(cb);
		UNUSED(pcbWritten);
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Seek(LARGE_INTEGER dlibMove, DWORD dwOrigin, ULARGE_INTEGER * plibNewPosition)
    {
		UNUSED(dlibMove);
		UNUSED(dwOrigin);
		UNUSED(plibNewPosition);
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE SetSize(ULARGE_INTEGER libNewSize)
    {
		UNUSED(libNewSize);
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE CopyTo(IStream * pstm, ULARGE_INTEGER cb, ULARGE_INTEGER * pcbRead, ULARGE_INTEGER * pcbWritten)
    {
		UNUSED(pstm);
		UNUSED(cb);
		UNUSED(pcbRead);
		UNUSED(pcbWritten);

        return E_NOTIMPL;
    } 

    virtual HRESULT STDMETHODCALLTYPE Commit(DWORD grfCommitFlags)
    {
        return stream->Commit(grfCommitFlags);
    }
    
    virtual HRESULT STDMETHODCALLTYPE Revert(void)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE LockRegion( ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType)
    {
		UNUSED(libOffset);
		UNUSED(cb);
		UNUSED(dwLockType);

        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE UnlockRegion(ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType)
    {
        UNUSED(libOffset);
		UNUSED(cb);
		UNUSED(dwLockType);

		return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Stat(STATSTG * pstatstg, DWORD grfStatFlag)
    {
		UNUSED(pstatstg);
		UNUSED(grfStatFlag);
		
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Clone(IStream ** ppstm)
    {
		UNUSED(ppstm);

        return E_NOTIMPL;
    }

    ///////////////////////////////////////////////////////////
    // public methods
    //

    /**
     * Defines the character encoding of the input stream.  
     * The new character encoding must agree with the encoding determined by the constructer.  
     * setEncoding is used to clarify between encodings that are not fully determinable 
     * through the first four bytes in a stream and not to change the encoding.
     * This method must be called within BUFFERSIZE reads() after construction.
     */
    HRESULT switchEncodingAt(Encoding * newEncoding, int newPosition);
    HRESULT checkNewEncoding(const WCHAR **charset, ULONG * len);



	// For Read EncodingStreams, this method can be used to push raw data
    // which is an alternate approach to providing another IStream.
    HRESULT AppendData(const BYTE* buffer, ULONG length, BOOL lastBuffer);

    HRESULT BufferData();
  
    void setReadStream(bool flag) { _fReadStream = flag; }

  
private:
	/**
	* Buffer Size
	*/
    static const int BUFFERSIZE;  
	
	HRESULT autoDetect();

    HRESULT prepareForInput(ULONG minlen);

    /**
     * Character encoding variables:                         only encoding is used for reading, other three used for writeXML
     */ 
    CODEPAGE codepage;   // code page number
    Encoding * encoding; // encoding
    //bool  _fTextXML;     // MIME type, true: "text/xml", false: "application/xml"
    //bool  _fSetCharset;  // Whether the charset has been set from outside. e.g, when mime type text/xml or application/xml
                         // has charset parameter
    
    /** 
	* Multibyte buffer 
	*/
    BYTE	*buf;           // storage for multibyte bytes
    ULONG	bufsize;
    UINT	bnext;       // point to next available byte in the rawbuffer
    ULONG	btotal;     // total number of bytes in the rawbuffer
    int		startAt;        // where the buffer starts at in the input stream 
	
	/**
     * Function pointer to convert from multibyte to unicode
     */
    WideCharFromMultiByteFunc * pfnWideCharFromMultiByte;

	UINT maxCharSize;		// maximum number of bytes of a wide char
							//                         used for writeXML, 
    RStream stream;
    bool	isInput;
    bool	lastBuffer;
    bool	_fEOF;
	bool	_fUTF8BOM;
    bool	_fReadStream;	// lets Read() method call Read() on wrapped stream object.
	

	DWORD _dwMode;			// MLANG context.

};

typedef _reference<EncodingStream> REncodingStream;

#endif // _FUSION_XMLPARSER__ENCODINGSTREAM_H_INCLUDE_
