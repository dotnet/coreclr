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
// fusion\xmlparser\XMLParser.hxx
//
/////////////////////////////////////////////////////////////////////////////////
#ifndef _FUSION_XMLPARSER__XMLPARSER_H_INCLUDE_
#define _FUSION_XMLPARSER__XMLPARSER_H_INCLUDE_

#include <winbase.h>
#include <ole2.h>
#include <xmlparser.h>
#include "xmlhelper.h"
class XMLStream;

typedef _reference<IXMLParser> RXMLParser;
typedef _reference<IXMLNodeFactory> RNodeFactory;
typedef _reference<IUnknown> RUnknown;

#include "encodingstream.h"

#include "_rawstack.h"

#include "clrhost.h"

//#define XMLFLAG_RUNBUFFERONLY   0x1000

//------------------------------------------------------------------------
// An internal Parser IID so that DTDNodeFactory can call internal methods.
const IID IID_Parser = {0xa79b04fe,0x8b3c,0x11d2,{0x9c, 0xd3,0x00,0x60,0xb0,0xec,0x3d,0x30}};

class XMLParser : public _unknown<IXMLParser, &IID_IXMLParser>
{
public:

	XMLParser();
        ~XMLParser();

		// ======= IUnknown override ============================
        virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void ** ppvObject);

        virtual ULONG STDMETHODCALLTYPE AddRef(void);
   
        virtual ULONG STDMETHODCALLTYPE Release(void);

		// ====== IXMLNodeSource methods ========================
        virtual HRESULT STDMETHODCALLTYPE SetFactory( 
            /* [in] */ IXMLNodeFactory __RPC_FAR *pNodeFactory);
        
        virtual HRESULT STDMETHODCALLTYPE GetFactory(
            /* [out] */ IXMLNodeFactory** ppNodeFactory);

        virtual HRESULT STDMETHODCALLTYPE Abort( 
            /* [in] */ BSTR bstrErrorInfo);

        virtual ULONG STDMETHODCALLTYPE GetLineNumber(void);
        
        virtual ULONG STDMETHODCALLTYPE GetLinePosition(void);
        
        virtual ULONG STDMETHODCALLTYPE GetAbsolutePosition(void);

        virtual HRESULT STDMETHODCALLTYPE GetLineBuffer( 
            /* [out] */ const WCHAR  **ppwcBuf,
            /* [out] */ ULONG  *pulLen,
            /* [out] */ ULONG  *pulStartPos);
        
        virtual HRESULT STDMETHODCALLTYPE GetLastError(void);
        
        virtual HRESULT STDMETHODCALLTYPE GetErrorInfo(/* [out] */ BSTR  *pbstrErrorInfo)
		{
			ASSERT(FALSE);
			UNUSED(pbstrErrorInfo);
			return E_NOTIMPL;
		}

		virtual ULONG STDMETHODCALLTYPE GetFlags() { 	
			return 0; 
		}

        virtual HRESULT STDMETHODCALLTYPE GetURL( 
            /* [out] */ const WCHAR  **ppwcBuf) {
			UNUSED(ppwcBuf);
			return E_NOTIMPL; 
		}

		// ====== IXMLParser methods ==============================

        virtual HRESULT STDMETHODCALLTYPE SetURL( 
            /* [in] */ const WCHAR* pszBaseUrl,
            /* [in] */ const WCHAR* pszRelativeUrl,
            /* [in] */ BOOL async) {
			UNUSED(pszBaseUrl);
			UNUSED(pszRelativeUrl);
			UNUSED(async);
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE Load( 
            /* [in] */ BOOL fFullyAvailable,
            /* [in] */ IMoniker *pimkName,
            /* [in] */ LPBC pibc,
            /* [in] */ DWORD grfMode) {

			UNUSED(fFullyAvailable);
			UNUSED(pimkName);
			UNUSED(pibc);
			UNUSED(grfMode);
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE SetInput( 
            /* [in] */ IUnknown*pStm);
        
        virtual HRESULT STDMETHODCALLTYPE PushData( 
            /* [in] */ const char *pData,
            /* [in] */ ULONG ulChars,
            /* [in] */ BOOL bLastBuffer);
        
        virtual HRESULT STDMETHODCALLTYPE LoadEntity(
            /* [in] */ const WCHAR* pszBaseUrl,
            /* [in] */ const WCHAR* pszRelativeUrl,
            /* [in] */ BOOL fpe) { 
			UNUSED(pszBaseUrl); 
			UNUSED(pszRelativeUrl); 
			UNUSED(fpe); 
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE ParseEntity(
            /* [in] */ const WCHAR* pwcText, 
            /* [in] */ ULONG ulLen,
            /* [in] */ BOOL fpe){ 
			UNUSED(pwcText);
			UNUSED(ulLen);
			UNUSED(fpe);
			return E_NOTIMPL;
		} 

	    virtual HRESULT STDMETHODCALLTYPE ExpandEntity(
            /* [in] */ const WCHAR* pwcText, 
            /* [in] */ ULONG ulLen) { 
			UNUSED(pwcText);
			UNUSED(ulLen);
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE SetRoot( 
            /* [in] */ PVOID pRoot) { 
			UNUSED(pRoot);
			return E_NOTIMPL;
		}
        
        virtual HRESULT STDMETHODCALLTYPE GetRoot( 
            /* [in] */ PVOID __RPC_FAR *ppRoot){ 
			UNUSED(ppRoot);
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE Run( 
            /* [in] */ long lChars);
        
        virtual HRESULT STDMETHODCALLTYPE GetParserState(void) ; 
                
        virtual HRESULT STDMETHODCALLTYPE Suspend(void) ; 
        
        virtual HRESULT STDMETHODCALLTYPE Reset(void) ;
        
        virtual HRESULT STDMETHODCALLTYPE SetFlags( 
            /* [in] */ ULONG iFlags) { 
			UNUSED(iFlags);
			return E_NOTIMPL;
		}
        
        virtual HRESULT STDMETHODCALLTYPE LoadDTD(
            /* [in] */ const WCHAR * pszBaseUrl,
            /* [in] */ const WCHAR * pszRelativeUrl){ 
			UNUSED(pszBaseUrl);
			UNUSED(pszRelativeUrl);
			return E_NOTIMPL;
		}
    
        virtual HRESULT STDMETHODCALLTYPE SetSecureBaseURL( 
            /* [in] */ const WCHAR* pszBaseUrl){ 
			UNUSED(pszBaseUrl);
			return E_NOTIMPL;
		}

        virtual HRESULT STDMETHODCALLTYPE GetSecureBaseURL( 
            /* [out] */ const WCHAR ** ppwcBuf){ 
			UNUSED(ppwcBuf);
			return E_NOTIMPL;
		}

        // ======= internal only methods for Parser 

		HRESULT ErrorCallback(HRESULT hr);

        bool ctorInit(CrstLevel crstLevel);
private:

		HRESULT PushTokenizer();
	
        HRESULT PushDownload(XMLStream* tokenizer);
		
        HRESULT PopDownload();

        void StackCleanupImpl();
        friend void XMLStackCleanup(XMLParser* self);

        XMLStream*  _pTokenizer;
        PVOID       _pRoot;
        HRESULT     _fLastError;
        BSTR        _bstrError;
        bool        _fWaiting;
        bool        _fSuspended;
        bool        _fStopped;
        bool        _fStarted;
        bool        _fInXmlDecl;
        bool        _fFoundEncoding;
        USHORT      _usFlags;
        bool        _fCaseInsensitive;
        //bool      _fTokenizerChanged;		// used in DTD, tokenizer may change in DTD file
        bool         _fGotVersion;			    // used in XML_VERSION
        long        _fRunEntryCount;		// used in Run(), counting how many is running the Parsing-While
        
        //bool        _fInLoading;			// used in PushURL(), Load(), HandleData(), 
        bool        _fInsideRun;			// used in Run()
        bool        _fFoundRoot;
        
        //bool        _fSeenDocType;		// used in DTD 
        bool        _fRootLevel;			// whether we are at the root level in document.
        bool        _fFoundNonWS;
        bool        _fPendingBeginChildren;
        bool        _fPendingEndChildren;
        
        //BSTR        _fAttemptedURL;        // used in PushURL(), Load(), GetErrorInfo();

        struct Download
        {
            XMLStream*      _pTokenizer;
            //RURLStream      _pURLStream;
            REncodingStream _pEncodingStream;
            bool            _fAsync;
            bool            _fDTD;
            bool            _fEntity;
            bool            _fPEReference;
            bool            _fFoundNonWS;
            bool            _fFoundRoot;    // saved values in case we're downloading a schema
            bool            _fSeenDocType;
            bool            _fRootLevel; // whether we are at the root level in document.
            int             _fDepth;    // current depth of stack.
        };
        _rawstack<Download> _pDownloads;
        Download*       _pdc;   // current download.


        // the Context struct contains members that map to the XML_NODE_INFO struct
        // defined in xmlparser.idl so that we can pass the contents of the Context
        // as a XML_NODE_INFO* pointer in BeginChildren, EndChildren and Error.

        typedef struct _MY_XML_NODE_INFO : public XML_NODE_INFO
        {
//            DWORD           dwSize;             // size of this struct
//            DWORD           dwType;             // node type (XML_NODE_TYPE)
//            DWORD           dwSubType;          // node sub type (XML_NODE_SUBTYPE)
//            BOOL            fTerminal;          // whether this node can have any children
//            WCHAR*          pwcText;            // element names, or text node contents.
//            ULONG           ulLen;              // length of pwcText
//            ULONG           ulNsPrefixLen;      // if element name, this is namespace prefix length.
//            PVOID           pNode;              // optionally created by & returned from node factory
//            PVOID           pReserved;          // reserved for factories to use between themselves.
            WCHAR*          _pwcTagName;        // saved tag name
            ULONG           _ulBufLen; 
        } MY_XML_NODE_INFO;
        typedef MY_XML_NODE_INFO* PMY_XML_NODE_INFO;

        _rawstack<MY_XML_NODE_INFO> _pStack;

        long            _lCurrentElement;
        PMY_XML_NODE_INFO _pCurrent; 
        USHORT          _cAttributes; // count of attributes on stack.

        // And we need a contiguous array of pointers to the XML_NODE_INFO 
        // structs for CreateNode.
        PMY_XML_NODE_INFO* _paNodeInfo;
        USHORT             _cNodeInfoAllocated;
        USHORT             _cNodeInfoCurrent;
        
        PVOID   _pNode; // current node (== pCurrent->pNode OR _pRoot).

        // Push saves this factory in the context and pop restores it
        // from the context.
        RNodeFactory _pFactory; // current factory (!= pCurrent->pParentFactory).

        HRESULT push(XML_NODE_INFO& info);
        HRESULT pushAttribute(XML_NODE_INFO& info);
        HRESULT pushAttributeValue(XML_NODE_INFO& info);

        HRESULT pop(const WCHAR* tag, ULONG len);
        HRESULT pop();
        HRESULT popAttributes();
        void    popAttribute();
        HRESULT popDTDAttribute() { return E_NOTIMPL; }

		HRESULT CopyContext();
		HRESULT CopyText(PMY_XML_NODE_INFO pNodeInfo);
		HRESULT ReportUnclosedTags(int index);
        HRESULT GrowBuffer(PMY_XML_NODE_INFO pNodeInfo, long newlen);
        HRESULT GrowNodeInfo();
		
        
        CRITSEC_COOKIE _cs;

        HRESULT init();
        
        HRESULT PushStream(IStream* pStm, bool fpe);
        //Download* FindDownload(URLStream* pStream);
        WCHAR*   getSecureBaseURL() 
                {
                    if (_pszSecureBaseURL)
                        return _pszSecureBaseURL;
                    else if (_dwSafetyOptions)
                        return _pszBaseURL;
                    return NULL;
                 }


        WCHAR*  _pszSecureBaseURL;
        WCHAR*  _pszCurrentURL;
        WCHAR*  _pszBaseURL;
        bool    _fIgnoreEncodingAttr;
        DWORD   _dwSafetyOptions;
};


#endif // _FUSION_XMLPARSER__XMLPARSER_H_INCLUDE_
