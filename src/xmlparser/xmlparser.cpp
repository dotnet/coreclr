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
//
/////////////////////////////////////////////////////////////////////////////////

#include "core.h"
#include "xmlhelper.h"
#include "clrhost.h"

#ifdef _MSC_VER
#pragma hdrstop
#endif
#include "xmlparser.hpp"
#include "xmlstream.h"
#include <objbase.h>

#define CRITICALSECTIONLOCK CSLock lock(_cs);
const USHORT STACK_INCREMENT=10;

#define PUSHNODEINFO(pNodeInfo)\
    if (_cNodeInfoAllocated == _cNodeInfoCurrent)\
    {\
        checkhr2(GrowNodeInfo());\
    }\
    _paNodeInfo[_cNodeInfoCurrent++] = _pCurrent;


//////////////////////////////////////////////////////////////////
class CSLock
{
public:
    CSLock(CRITSEC_COOKIE cs); 
    ~CSLock();

private:
    CRITSEC_COOKIE _cs;
};

CSLock::CSLock(CRITSEC_COOKIE cs){ 
        _cs = cs; 
        ClrEnterCriticalSection(_cs);
}
CSLock::~CSLock(){
        ClrLeaveCriticalSection(_cs);
}

/////////////////////////////////////////////////////////////////////////////
XMLParser::XMLParser()
:   _pDownloads(1), _pStack(STACK_INCREMENT), _cs(NULL)
{
}
/////////////////////////////////////////////////////////////////////////////
bool
XMLParser::ctorInit(CrstLevel crstLevel)
{
    _cs = ClrCreateCriticalSection("XMLParser",crstLevel,(CrstFlags)(CRST_REENTRANCY|CRST_HOST_BREAKABLE|CRST_UNSAFE_SAMELEVEL));

    _pTokenizer = NULL;
    _pCurrent = NULL;
    _lCurrentElement = 0;
    _paNodeInfo = NULL;
    _cNodeInfoAllocated = _cNodeInfoCurrent = 0;
    _pdc = NULL;
    _usFlags = 0;
    _fCaseInsensitive = false;
    _bstrError = NULL;
//    _fTokenizerChanged = false;
    _fRunEntryCount = 0;
    _pszSecureBaseURL = NULL;
    _pszCurrentURL = NULL;
    _pszBaseURL = NULL;
    //_fInLoading = false;
    _fInsideRun = false;
    //_fFoundDTDAttribute = false;
    _cAttributes = 0;
    _pRoot = NULL;
    //_fAttemptedURL = NULL;
    _fLastError = S_OK;
    _fStopped = false;
    _fSuspended = false;
    _fStarted = false;
    _fWaiting = false;
    _fIgnoreEncodingAttr = false;
    _dwSafetyOptions = 0;

    // rest of initialization done in the init() method.

    //EnableTag(tagParserCallback, TRUE);
    //EnableTag(tagParserError, TRUE);

	return (_cs!=NULL);
}
/////////////////////////////////////////////////////////////////////////////
XMLParser::~XMLParser()
{
    {
        CRITICALSECTIONLOCK;
        Reset();

        // Cleanup tagname buffers in context for good this time...
        for (long i = _pStack.size()-1; i>=0; i--)
        {
            MY_XML_NODE_INFO* pNodeInfo = _pStack[i];
            if (pNodeInfo->_pwcTagName != NULL)
            {
                delete [] pNodeInfo->_pwcTagName;
                pNodeInfo->_pwcTagName = NULL;
                pNodeInfo->_ulBufLen = 0;
            }
            // NULL out the node pointer in case it point's to a GC'd object :-)
            pNodeInfo->pNode = NULL;
        }
        delete _pszSecureBaseURL;
        delete _pszCurrentURL;

        delete[] _paNodeInfo;        
    }
    if (_cs!=NULL)
    {
        ClrDeleteCriticalSection(_cs);
    }
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::QueryInterface(REFIID riid, void ** ppvObject)
{
    //STACK_ENTRY;

    // Since this one class implements both IXMLNodeSource and
    // IXMLParser, we must override QueryInterface since the
    // IUnknown template doesn't know about the IXMLNodeSource
    // interface.

    HRESULT hr = S_OK;
    if (riid == IID_IXMLNodeSource || riid == IID_Parser)
    {
        *ppvObject = static_cast<IXMLNodeSource*>(this);        
        AddRef();
    }
    else
    {
        hr = _unknown<IXMLParser, &IID_IXMLParser>::QueryInterface(riid, ppvObject);
    }
    return hr;
}
/////////////////////////////////////////////////////////////////////////////
ULONG STDMETHODCALLTYPE
XMLParser::AddRef(void)
{
    //STACK_ENTRY;
    return _unknown<IXMLParser, &IID_IXMLParser>::AddRef();
}
/////////////////////////////////////////////////////////////////////////////
ULONG STDMETHODCALLTYPE
XMLParser::Release(void)
{
//    STACK_ENTRY;
    return _unknown<IXMLParser, &IID_IXMLParser>::Release();
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::SetInput(IUnknown *pStm)
{
    if (pStm == NULL)
        return E_INVALIDARG;

    //STACK_ENTRY_MODEL(_reThreadModel);
    CRITICALSECTIONLOCK;
    if (_pDownloads.used() == 0)
        init();
    HRESULT hr = S_OK;

    //checkhr2(PushTokenizer(NULL));
    checkhr2(PushTokenizer());

    // Get the url path
    // Continue even if we cannot get it
//    STATSTG stat;
    IStream * pStream = NULL;
//    memset(&stat, 0, sizeof(stat));
    hr = pStm->QueryInterface(IID_IStream, (void**)&pStream);
    if (SUCCEEDED(hr))
    {
        hr = PushStream(pStream, false);       
        pStream->Release(); 
    }
    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::PushData(
            /* [in] */ const char __RPC_FAR *pData,
            /* [in] */ ULONG ulChars,
            /* [in] */ BOOL fLastBuffer)
{
    //STACK_ENTRY_MODEL(_reThreadModel);
    CRITICALSECTIONLOCK;
    HRESULT hr;

    if ((NULL == pData) && (ulChars != 0))
    {
        return E_INVALIDARG;
    }

    if (_pTokenizer == NULL)
    {
        init();
        //checkhr2(PushTokenizer(NULL));
        checkhr2(PushTokenizer());
    }
    return _pTokenizer->AppendData((const BYTE*)pData, ulChars, fLastBuffer);
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::SetFactory(IXMLNodeFactory __RPC_FAR *pNodeFactory)
{
    //STACK_ENTRY;

    CRITICALSECTIONLOCK;
    _pFactory = pNodeFactory;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::GetFactory(IXMLNodeFactory** ppNodeFactory)
{
    if (ppNodeFactory == NULL) return E_INVALIDARG;
    if (_pFactory)
    {
        *ppNodeFactory = _pFactory;
        (*ppNodeFactory)->AddRef();
    }
    else
    {
        *ppNodeFactory = NULL;
    }
    return S_OK;
}


void
XMLParser::StackCleanupImpl()
{
    _ASSERTE(S_OK != _fLastError);
    _ASSERTE(E_PENDING != _fLastError);


    // Pass all the XML_NODE_INFO structs to the Error function so the client
    // gets a chance to cleanup the PVOID pNode fields.
    HRESULT hr = _pFactory->Error(this, _fLastError,
                                  (USHORT)(_paNodeInfo ? _lCurrentElement+1 : 0), 
                                  (XML_NODE_INFO**)_paNodeInfo);
    if (hr != S_OK)
    {
        _fLastError = hr;
    }

    if (!_fStopped)
    {
        _fStopped = true;
        _fStarted = false;
        hr = _pFactory->NotifyEvent(this, XMLNF_ENDDOCUMENT);
        if (hr != 0)
        {
            _fLastError = hr; // allow factory to change error code (except to S_OK)
            if (S_OK == _fLastError)
            {
                // Make sure the node factory always finds out about errors.
                hr = _pFactory->Error(this, _fLastError, 0, NULL);
                if (hr != S_OK)
                {
                    _fLastError = hr;
                }
            }
        }
    }
}

void
XMLStackCleanup(XMLParser* self)
{
    self->StackCleanupImpl();
}


namespace
{
    typedef Holder< XMLParser*, DoNothing< XMLParser* >, XMLStackCleanup > StackCleanup;
}

/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::Run(long lChars)
{
    HRESULT hr = NOERROR;

    FN_TRACE_HR(hr);

    //STACK_ENTRY_MODEL(_reThreadModel);
    CSLock lock(_cs);

    XML_NODE_INFO   info;
    XML_NODE_INFO*  aNodeInfo[1];

    USHORT          numRecs;

    bool            fIsAttribute = false;
    bool            stop = false;

    if (_fSuspended)
        _fSuspended = FALSE; // caller must want to resume.

    if (_pFactory == NULL)
    {

        return E_FAIL;
    }

    if (_fStopped)
    {        

        return XML_E_STOPPED;
    }

    if (_pTokenizer == NULL) 
    {
        if (_fLastError != S_OK)
        {
            return _fLastError;
        }
        else
        {

            // must be _fStarted == false
            return XMLPARSER_IDLE;
        }
    }

    // Check for recurrsive entry and whether caller actually
    // wants anything parsed.
    if (_fInsideRun || lChars == 0)
    {
        return E_PENDING;
    }

    BoolLock flock(&_fInsideRun);
    StackCleanup stackCleanup(this);

    if (_fLastError != 0)
    {
        // one more chance to cleanup the parser stack.
        return _fLastError;
    }

    if (! _fStarted)
    {
        _fStarted = true;
        hr = _pFactory->NotifyEvent(this, XMLNF_STARTDOCUMENT);
        if (_fStopped)      // watch for onReadyStateChange handlers fussing with the parser state.
        {
            stackCleanup.SuppressRelease();
            return S_OK;    
        }
    }

    _fWaiting = false;
    if (_fPendingBeginChildren)
    {
        _fPendingBeginChildren = false;
        hr = _pFactory->BeginChildren(this, (XML_NODE_INFO*)_pCurrent);
    }
    if (_fPendingEndChildren)
    {
        _fPendingEndChildren = false;
        hr = _pFactory->EndChildren(this, TRUE, (XML_NODE_INFO*)_pCurrent);
        if (SUCCEEDED(hr))
            hr = pop(); // no match needed
    }

    info.dwSize = sizeof(XML_NODE_INFO);
    info.dwType = XMLStream::XML_PENDING;
    info.dwSubType = 0;
    info.pwcText = NULL;
    info.ulLen = 0;
    info.ulNsPrefixLen = 0;
    info.pNode = NULL;
    info.pReserved = NULL;
    aNodeInfo[0] = &info;

more:
    _fRunEntryCount++; // count of callers inside this loop...

    while (hr == 0 && ! _fSuspended)
    {
        info.dwSubType = 0;

        // The XMLStream error codes have been aligned with the
        // XMLParser error code so no mapping is necessary.
        hr = _pTokenizer->GetNextToken(&info.dwType, (const WCHAR  **)&info.pwcText, (long*)&info.ulLen, (long*)&info.ulNsPrefixLen);
        if (hr == E_PENDING)
        {
            _fWaiting = true;
            break;
        }

        if (! _fFoundNonWS &&
                info.dwType != XMLStream::XML_PENDING &&
                info.dwType != XML_WHITESPACE &&
                info.dwType != XML_XMLDECL)
        {
            _fFoundNonWS = true;
        }

        // Now the NodeType is the same as the XMLToken value.  We set
        // this up by aligning the two enums.
        switch (info.dwType)
        {
        case 0:
            if (hr == (HRESULT) XML_E_INVALIDSWITCH  && _fIgnoreEncodingAttr)
            {
                hr = 0; // ignore it and continue on.
            }
            break;
            // --------- Container Nodes -------------------
        case XML_XMLDECL:
            //if (_fFoundNonWS && ! _fIE4Mode)  // IE4 allowed this...
            if (_fFoundNonWS)
            {
                hr = XML_E_BADXMLDECL;
                break;
            }
//            _fFoundNonWS = true;
            goto containers;

        case XML_ATTRIBUTE:
            fIsAttribute = true;
            goto containers; 

        case XML_VERSION:
            info.dwSubType = info.dwType;
            info.dwType = XML_ATTRIBUTE;
            _fGotVersion = true;
            fIsAttribute = true;
            goto containers;

        case XML_STANDALONE:
        case XML_ENCODING:
            if (! _fGotVersion && _pDownloads.used() == 1)
            {
                hr = XML_E_EXPECTING_VERSION;
                break;
            }
            if (info.dwType == XML_STANDALONE)
            {
                if (_pDownloads.used() > 1)
                {
                    hr = XML_E_UNEXPECTED_STANDALONE;
                    break;
                }
            }
            info.dwSubType = info.dwType;
            info.dwType = XML_ATTRIBUTE;
            fIsAttribute = true;
            goto containers;
            // fall through
        case XML_ELEMENT:
containers:
            if (_fRootLevel)
            {
                // Special rules apply for root level tags.
                if (info.dwType == XML_ELEMENT)
                {
                     // This is a root level element.
                     if (! _fFoundRoot)
                     {
                         _fFoundRoot = true;
                     }
                     else
                     {

                         hr = XML_E_MULTIPLEROOTS;
                         break;
                     }
                }
                else if (info.dwType != XML_PI &&
                         info.dwType != XML_XMLDECL &&
                         info.dwType != XML_DOCTYPE)
                {

                    hr = XML_E_INVALIDATROOTLEVEL;
                    break;
                }
            }

            info.fTerminal = FALSE;

            if (fIsAttribute)
            {
                breakhr( pushAttribute(info));
                fIsAttribute = false;
            }
            else
            {
                breakhr( push(info));
            }
            break;
        case XML_PCDATA:
        case XML_CDATA:
terminals:
            // Special rules apply for root level tags.
            if (_fRootLevel)
            {

                hr = XML_E_INVALIDATROOTLEVEL;
                break;
            }
            // fall through
        case XML_COMMENT:
        case XML_WHITESPACE:
tcreatenode:
            info.fTerminal = TRUE;
            if (_cAttributes != 0)
            {
                // We are inside the attribute list, so we need to push this.
                hr = pushAttributeValue(info);
                break;
            }
            hr = _pFactory->CreateNode(this, _pNode, 1, aNodeInfo);
            info.pNode = NULL;
            break;

        case XML_ENTITYREF:
            if (_fRootLevel)
            {
                hr = XML_E_INVALIDATROOTLEVEL;
                break;
            }

            // We handle builtin entities and char entities in xmlstream
            // so these must be user defined entity, so treat it like a regular terminal node.
            goto terminals;
            break;

        case XMLStream::XML_BUILTINENTITYREF:
        case XMLStream::XML_HEXENTITYREF:
        case XMLStream::XML_NUMENTITYREF:
            // pass real entityref type as subtype so we can publish these
            // subtypes eventually.
            info.dwSubType = info.dwType; // XML_ENTITYREF;
            info.dwType = XML_PCDATA;

            if (_cAttributes == 0)
            {
                goto tcreatenode;
            }

            // We are inside the attribute list, so we need to push this.
            info.fTerminal = TRUE;
            hr = pushAttributeValue(info);
            if (SUCCEEDED(hr))
            {
                hr = CopyText(_pCurrent);
            }
            break;
        
        case XMLStream::XML_TAGEND:     // ">"
            numRecs = 1+_cAttributes;
            if (_cAttributes != 0)  // this is safe because _rawstack does NOT reclaim
            {                       // the popped stack entries.
                popAttributes();
            }
            hr = _pFactory->CreateNode(this, _pNode, numRecs, (XML_NODE_INFO **)&_paNodeInfo[_lCurrentElement]);
            _pNode = _pCurrent->pNode;
            if (FAILED(hr))
            {
                _fPendingBeginChildren = true;
                break;
            }
            breakhr( _pFactory->BeginChildren(this, (XML_NODE_INFO*)_pCurrent));
            break;

            // The ENDXMLDECL is like EMPTYENDTAGs since we've been
            // buffering up their attributes, and we have still got to call CreateNode.
        case XMLStream::XML_ENDXMLDECL:
            _fGotVersion = false; // reset back to initial state.
            // fall through.
        case XMLStream::XML_EMPTYTAGEND:
            numRecs = 1+_cAttributes;
            if (_cAttributes != 0)
            {
                popAttributes();
            }
            hr = _pFactory->CreateNode(this, _pNode, numRecs, (XML_NODE_INFO **)&_paNodeInfo[_lCurrentElement]);
            if (FAILED(hr))
            {
                _fPendingEndChildren = true;
                break;
            }
            breakhr(_pFactory->EndChildren(this, TRUE, (XML_NODE_INFO*)_pCurrent));
            breakhr(pop()); // no match needed
            break;

        case XMLStream::XML_ENDTAG:     // "</"
            if (_pStack.used() == 0)
            {

                hr = XML_E_UNEXPECTEDENDTAG;
            }
            else
            {
                XML_NODE_INFO* pCurrent = (XML_NODE_INFO*)_pCurrent; // save current record
                breakhr(pop(info.pwcText, info.ulLen)); // check tag/match
                breakhr(_pFactory->EndChildren(this, FALSE, (XML_NODE_INFO*)pCurrent));
            }
            break;
        
        case XMLStream::XML_ENDPROLOG:
            // For top level document only, (not for DTD's or
            // entities), call EndProlog on the node factory.
            if (_fRootLevel && ! _pdc->_fEntity && ! _pdc->_fDTD)
                breakhr( _pFactory->NotifyEvent(this, XMLNF_ENDPROLOG));
            break;

        default:
            hr = E_FAIL;
            break; // break from switch()
        }
    }
    _fRunEntryCount--;

    stop = false;
    if (hr == (HRESULT) XML_E_ENDOFINPUT)
    {
        hr = S_OK;
        bool inDTD = _pdc->_fDTD;
        bool inEntity = _pdc->_fEntity;
        bool inPEReference = _pdc->_fPEReference;

        if (inEntity && _pdc->_fDepth != _pStack.used())
        {

            // Entity itself was unbalanced.
            hr = ReportUnclosedTags(_pdc->_fDepth);
        }
        else if (PopDownload() == S_OK)
        {
            // then we must have just finished a DTD and we still have more to do

            if (!inPEReference)
            {
                if (inEntity)
                {
                    hr = _pFactory->NotifyEvent(this, XMLNF_ENDENTITY);
                }
                else if (inDTD)
                {
                    hr = _pFactory->NotifyEvent(this, XMLNF_ENDDTD);                    
                }
            }
            if (FAILED(hr))
            {
                goto cleanup_stack;
            }

            // In a synchronous DTD download, there is another parser
            // parser Run() call on the stack above us, so let's return
            // back to that Run method so we don't complete the parsing
            // out from under it.
            if ((_fRunEntryCount > 0) || _fStopped)
            {
                stackCleanup.SuppressRelease();
                return S_OK;
            }
            goto more;
        }
        else
        {
            if (_pStack.used() > 0)
            {
                hr = ReportUnclosedTags(0);
            }
            else if (! _fFoundRoot)
            {

                hr = XML_E_MISSINGROOT;
            }
            stop = true;
        }
    }

cleanup_stack:

    if ((S_OK == hr) || (E_PENDING == hr))
    {
        stackCleanup.SuppressRelease();
    }
    _fLastError = hr;
    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::popAttributes()
{
    // Now I pop all the attributes that were pushed for this tag.
    // I know we have at least one attribute.
    
    while (_cAttributes > 0)
    {
        popAttribute(); // no match needed
    }
    Assert(_pStack.used() == _lCurrentElement+1);

    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::GetParserState(void)
{
    CRITICALSECTIONLOCK;

    if (_fLastError != 0)
        return (HRESULT)XMLPARSER_ERROR;

    if (_fStopped)
        return (HRESULT)XMLPARSER_STOPPED;

    if (_fSuspended)
        return (HRESULT)XMLPARSER_SUSPENDED;

    if (! _fStarted)
        return (HRESULT)XMLPARSER_IDLE;

    if (_fWaiting)
        return (HRESULT)XMLPARSER_WAITING;

    return (HRESULT)XMLPARSER_BUSY;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::Abort(
            /* [in] */ BSTR bstrErrorInfo)
{
    //STACK_ENTRY_MODEL(_reThreadModel);

    // Have to set these before Critical Section to notify Run()
    _fStopped = true;
    _fSuspended = true; // force Run to terminate...

    CRITICALSECTIONLOCK;
    //TraceTag((tagParserError, "Parser aborted - %ls", bstrErrorInfo));

    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::Suspend( void)
{
    _fSuspended = true; // force Run to suspend
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::Reset( void)
{
//  STACK_ENTRY;

    CRITICALSECTIONLOCK;

    init();

    delete _pszCurrentURL;
    _pszCurrentURL = NULL;
    delete _pszBaseURL;
    _pszBaseURL = NULL;
    _pRoot = NULL;
    _pFactory = NULL;
    _pNode = NULL;
    //if (_bstrError != NULL) ::SysFreeString(_bstrError);
    _bstrError = NULL;
    //if (_fAttemptedURL != NULL) ::SysFreeString(_fAttemptedURL);
    //_fAttemptedURL = NULL;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
ULONG STDMETHODCALLTYPE
XMLParser::GetLineNumber(void)
{
    CRITICALSECTIONLOCK;
    if (_pTokenizer)  return _pTokenizer->GetLine();
    else return 0;
}
/////////////////////////////////////////////////////////////////////////////
ULONG STDMETHODCALLTYPE
XMLParser::GetLinePosition( void)
{
    CRITICALSECTIONLOCK;
    if (_pTokenizer) return _pTokenizer->GetLinePosition();
    else return 0;
}
/////////////////////////////////////////////////////////////////////////////
ULONG STDMETHODCALLTYPE
XMLParser::GetAbsolutePosition( void)
{
    CRITICALSECTIONLOCK;
    if (_pTokenizer) return _pTokenizer->GetInputPosition();
    else return 0;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::GetLineBuffer(
            /* [out] */ const WCHAR __RPC_FAR *__RPC_FAR *ppwcBuf,
            /* [out] */ ULONG __RPC_FAR *pulLen,
            /* [out] */ ULONG __RPC_FAR *pulStartPos)
{
    if (pulLen == NULL || pulStartPos == NULL) return E_INVALIDARG;

    //STACK_ENTRY;

    CRITICALSECTIONLOCK;
    if (_pTokenizer)
    {
        return _pTokenizer->GetLineBuffer(ppwcBuf, pulLen, pulStartPos);
    }
    *ppwcBuf = NULL;
    *pulLen = 0;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT STDMETHODCALLTYPE
XMLParser::GetLastError( void)
{
    return _fLastError;
}

//------------ PRIVATE METHODS --------------------------------------------------
HRESULT
//XMLParser::PushTokenizer(
//                URLStream* stream)
XMLParser::PushTokenizer()
{
    _pTokenizer = NEW (XMLStream(this));
    if (_pTokenizer == NULL)
        return E_OUTOFMEMORY;

    _pTokenizer->SetFlags(_usFlags);
//    _fTokenizerChanged = true;

    //HRESULT hr= PushDownload(stream, _pTokenizer);
    HRESULT hr= PushDownload(_pTokenizer);
    if (FAILED(hr))
    {
        delete _pTokenizer;
        _pTokenizer = NULL;
        return hr;
    }
    return S_OK; 
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
//XMLParser::PushDownload(URLStream* stream, XMLStream* tokenizer)
XMLParser::PushDownload(XMLStream* tokenizer)
{
    // NOTE: tokenizer can be null, in the case of a parameter entity download.

    _pdc = _pDownloads.push();
    if (_pdc == NULL)
    {
        return E_OUTOFMEMORY;
    }
    if (_pDownloads.used() > 1)
        _fRootLevel = false;

    _pdc->_pTokenizer = tokenizer;
    _pdc->_fDTD = false;
    _pdc->_fEntity = false;
    _pdc->_fAsync = false;
    _pdc->_fFoundNonWS = _fFoundNonWS;
    _pdc->_fFoundRoot = _fFoundRoot;
    _pdc->_fRootLevel = _fRootLevel;
    _pdc->_fDepth = _pStack.used();

    _fFoundNonWS = false;
    _fFoundRoot = false;

    _fRootLevel = (_pStack.used() == 0 && _pDownloads.used() == 1);

    HRESULT hr = S_OK;

    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT 
XMLParser::PushStream(IStream* pStm, bool fpe)
{
    EncodingStream* stream = (EncodingStream*)EncodingStream::newEncodingStream(pStm); // refcount = 1
    if (stream == NULL)
        return E_OUTOFMEMORY;
/*
    if (_usFlags & XMLFLAG_RUNBUFFERONLY)
        stream->setReadStream(false);
*/
    _pdc->_pEncodingStream = stream;
    stream->Release(); // Smart pointer is holding a ref

    HRESULT hr = _pTokenizer->PushStream(stream, fpe);
    if (hr == E_PENDING)
    {
        _fWaiting = true;
    }
    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::PopDownload()
{
    // NOTE: tokenizer can be null, in the case of a parameter entity download.
    HRESULT hr = S_OK;

    if (_pdc != NULL)
    {
        if (_pdc->_pTokenizer)
        {
            _pdc->_pTokenizer->Reset();
            delete _pdc->_pTokenizer;
            _pdc->_pTokenizer = NULL;
        }
        _pdc->_pEncodingStream = NULL;
/*
        if (_pdc->_pURLStream)
            _pdc->_pURLStream->Reset();

        _pdc->_pURLStream = NULL;
*/
        // restore saved value of foundnonws.
        _fFoundNonWS = _pdc->_fFoundNonWS;
        _pdc = _pDownloads.pop();
    }
    if (_pdc != NULL)
    {
        if (_pdc->_pTokenizer != NULL)
        {
            _pTokenizer = _pdc->_pTokenizer;
        }
        /*
        if (_pdc->_pURLStream != NULL)
        {
            hr = SetCurrentURL(_pdc->_pURLStream->GetURL()->getResolved());
        }
        */
    }
    else
    {
        _pTokenizer = NULL;
        hr = S_FALSE;
    }

    if (_pStack.used() == 0 && _pDownloads.used() == 1)
        _fRootLevel = true;

    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::GrowNodeInfo()
{
    USHORT newsize = _cNodeInfoAllocated + STACK_INCREMENT;
    MY_XML_NODE_INFO** pNewArray = NEW (PMY_XML_NODE_INFO[newsize]);
    if (pNewArray == NULL)
        return E_OUTOFMEMORY;
    // Now since STACK_INCREMENT is the same for _pStack then _pStack
    // has also re-allocated.  Therefore we need to re-initialize all
    // the pointers in this array - since they point into the _pStack's memory.
    for (int i = _pStack.used() - 1; i >= 0; i--)
    {
        pNewArray[i] = _pStack[i];
    }
    delete[] _paNodeInfo;
    _paNodeInfo = pNewArray;
    _cNodeInfoAllocated = newsize;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::GrowBuffer(PMY_XML_NODE_INFO pNodeInfo, long newlen)
{
    delete [] pNodeInfo->_pwcTagName;
    pNodeInfo->_pwcTagName = NULL;
    // add 50 characters to avoid too many reallocations.
    pNodeInfo->_pwcTagName = NEW (WCHAR[ newlen ]);
    if (pNodeInfo->_pwcTagName == NULL)
        return E_OUTOFMEMORY;
    pNodeInfo->_ulBufLen = newlen;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::push(XML_NODE_INFO& info)
{
    HRESULT hr;
    _lCurrentElement = _pStack.used();

    _pCurrent = _pStack.push();
    if (_pCurrent == NULL)
        return E_OUTOFMEMORY;

    *((XML_NODE_INFO*)_pCurrent) = info;
    PUSHNODEINFO(_pCurrent);

    _fRootLevel = false;

    // Save the tag name into the private buffer so it sticks around until the
    // close tag </foo> which could be anywhere down the road after the
    // BufferedStream been overwritten


    
    if (_pCurrent->_ulBufLen <= info.ulLen)
    {
        checkhr2(GrowBuffer(_pCurrent, info.ulLen + 50));
    }
    Assert(info.ulLen >= 0);
    ::memcpy(_pCurrent->_pwcTagName, info.pwcText, info.ulLen*sizeof(WCHAR));
    _pCurrent->_pwcTagName[info.ulLen] = L'\0';

    // And make the XML_NODE_INFO point to private buffer.
    _pCurrent->pwcText = _pCurrent->_pwcTagName;

    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::pushAttribute(XML_NODE_INFO& info)
{
    HRESULT hr;
    if (_cAttributes != 0)
    {
        // Attributes are special in that they are supposed to be unique.
        // So here we actually check this.
        for (long i = _pStack.used()-1; i > _lCurrentElement; i--)
        {
            XML_NODE_INFO* ptr = _pStack[i];

            if (ptr->dwType != XML_ATTRIBUTE)
                continue; // ignore attribute values.

            if (ptr->ulLen != info.ulLen)
            {
                continue; // we're ok with this one
            }

            // Optimized for the normal case where there is no match
            if (::memcmp(ptr->pwcText, info.pwcText, info.ulLen*sizeof(WCHAR)) == 0)
            {
                if (! _fCaseInsensitive)
                {

                    return XML_E_DUPLICATEATTRIBUTE;
                }
                //else if (StrCmpNI(ptr->pwcText, info.pwcText, info.ulLen) == 0)
//                else if (::FusionpCompareStrings(ptr->pwcText, lstrlen(ptr->pwcText), info.pwcText, info.ulLen, true) == 0)
                  else if (_wcsnicmp(ptr->pwcText, info.pwcText, info.ulLen) == 0)
                {

                    // Duplicate attributes are allowed in IE4 mode!!
                    // But only the latest one shows up
                    // So we have to delete the previous duplication
                    return XML_E_DUPLICATEATTRIBUTE;
                }
            }
        }
    }

    _cAttributes++;

    _pCurrent = _pStack.push();
    if (_pCurrent == NULL)
        return E_OUTOFMEMORY;

    *((XML_NODE_INFO*)_pCurrent) = info;
    PUSHNODEINFO(_pCurrent);

    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::pushAttributeValue(XML_NODE_INFO& info)
{
    HRESULT hr;
    // Attributes are saved in the BufferedStream so we can point to the
    // real text in the buffered stream instead of copying it !!

    _pCurrent = _pStack.push();
    if (_pCurrent == NULL)
        return E_OUTOFMEMORY;

    // store attribute value quote character in the pReserved field.
    info.pReserved = (PVOID)(INT_PTR)_pTokenizer->getAttrValueQuoteChar();

    *((XML_NODE_INFO*)_pCurrent) = info;
    PUSHNODEINFO(_pCurrent);

    // this is really the count of nodes on the stack, not just attributes.
    _cAttributes++;
    return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::pop(const WCHAR* tag, ULONG len)
{
    HRESULT hr = S_OK;

    if (_pCurrent == NULL || _pStack.used() == 0)
    {

        hr = XML_E_UNEXPECTEDENDTAG;
        goto Cleanup;
    }
    if (len != 0)
    {
        if (_pCurrent->ulLen != len)
        {

            hr = XML_E_ENDTAGMISMATCH;
        }
        // Optimized for the normal case where there is no match
        else if (::memcmp(_pCurrent->pwcText, tag, len*sizeof(WCHAR)) != 0)
        {
            if (! _fCaseInsensitive)
            {

                hr = XML_E_ENDTAGMISMATCH;
            }
            //else if ( XML_StrCmpNI(_pCurrent->pwcText, tag, len) != 0 )
            //else if (::FusionpCompareStrings(_pCurrent->pwcText, len, tag, len, true) != 0)
            else if(_wcsnicmp(_pCurrent->pwcText, tag, len) != 0)
            {
                hr = XML_E_ENDTAGMISMATCH;
            }
        }
        if (FAILED(hr))
        {
            /*
            TRY
            {
                String* s = Resources::FormatMessage(hr, String::newString(_pCurrent->pwcText, 0, _pCurrent->ulLen),
                                                         String::newString(tag, 0, len), NULL);
                _bstrError = s->getBSTR();
            }
            CATCH
            {
                hr = ERESULT;
            }
            ENDTRY
            */
            goto Cleanup;
        }
    }

    // We don't delete the fTagName because we're going to reuse this field
    // later to avoid lots of memory allocations.

    _pCurrent = _pStack.pop();
    _cNodeInfoCurrent--;

    if (_pCurrent == 0)
    {
        _pNode = _pRoot;
        if (_pDownloads.used() == 1)
            _fRootLevel = true;
    }
    else
    {
        _pNode = _pCurrent->pNode;
    }

Cleanup:
    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT XMLParser::pop()
{
    // We don't delete the fTagName because we're going to reuse this field
    // later to avoid lots of memory allocations.

    _pCurrent = _pStack.pop();
    _cNodeInfoCurrent--;

    if (_pCurrent == 0)
    {
        _pNode = _pRoot;
        if (_pDownloads.used() == 1)
            _fRootLevel = true;
    }
    else
    {
        _pNode = _pCurrent->pNode;
    }
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
void XMLParser::popAttribute()
{
    Assert(_pStack.used() > 0);

    _pCurrent = _pStack.pop();
    _cNodeInfoCurrent--;

    Assert(_pCurrent != 0);

    _cAttributes--;

}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::CopyText(PMY_XML_NODE_INFO pNodeInfo)
{
    HRESULT hr = S_OK;
    if (pNodeInfo->_pwcTagName != pNodeInfo->pwcText)
    {
        ULONG len = pNodeInfo->ulLen;

        // Copy the current text into the buffer.
        if (pNodeInfo->_ulBufLen <= len)
        {
            checkhr2(GrowBuffer(pNodeInfo, len + 50));
        }
        if (len > 0)
        {
            ::memcpy(pNodeInfo->_pwcTagName, pNodeInfo->pwcText, len*sizeof(WCHAR));
        }
        pNodeInfo->_pwcTagName[len] = L'\0';

        // And make the XML_NODE_INFO point to private buffer.
        pNodeInfo->pwcText = pNodeInfo->_pwcTagName;
    }
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT
XMLParser::CopyContext()
{
    // For performance reasons we try not to copy the data for attributes
    // and their values when we push them on the stack.  We can do this
    // because the tokenizer tries to freeze the internal buffers while
    // parsing attributes and thereby guarentee that the pointers stay
    // good.  But occasionally the BufferedStream has to reallocate when
    // the attributes are right at the end of the buffer.

    long last = _pStack.used();
    for (long i = _cAttributes; i > 0 ; i--)
    {
        long index = last - i;
        MY_XML_NODE_INFO* ptr = _pStack[index];
        CopyText(ptr);
    }
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT XMLParser::ReportUnclosedTags(int start)
{
    HRESULT hr = XML_E_UNCLOSEDTAG;
    // Build a string containing the list of unclosed tags and format an error
    // message containing this text.
    int tags = _pStack.used();

    WCHAR* buffer = NULL;
    WCHAR* msgbuf = NULL;
    unsigned long size = 0;
    unsigned long used = 0;

    for (long i = start; i < tags; i++)
    {
        XML_NODE_INFO* ptr = _pStack[i];
        if (ptr->dwType == XML_ATTRIBUTE)
            break;

        if (used + ptr->ulLen + 3 > size) // +3 for '<','>' and '\0'
        {
            long newsize = used + ptr->ulLen + 500;
            WCHAR* newbuf = NEW (WCHAR[newsize]);
            if (newbuf == NULL)
            {
                goto nomem;
            }
            if (buffer != NULL)
            {
                ::memcpy(newbuf, buffer, used);
                delete[] buffer;
            }

            size = newsize;
            buffer = newbuf;
        }
        if (i > start)
        {
            buffer[used++] = ',';
            buffer[used++] = ' ';
        }
        ::memcpy(&buffer[used], ptr->pwcText, sizeof(WCHAR) * ptr->ulLen);
        used += ptr->ulLen;
        buffer[used] = '\0';
    }
    goto cleanup; 

nomem:
    hr = E_OUTOFMEMORY;

cleanup:    

    delete [] buffer;
    delete [] msgbuf;

    return hr;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT XMLParser::init()
{
    CRITICALSECTIONLOCK;

    _fLastError = 0;
    _fStopped = false;
    _fSuspended = false;
    _pNode = _pRoot;
    _fStarted = false;
    _fStopped = false;
    _fWaiting = false;
    _fFoundRoot = false;
    _fFoundNonWS = false;
    _pTokenizer = NULL;
    _fGotVersion = false;
    _fRootLevel = true;
    _cAttributes = 0;
    

    _fPendingBeginChildren = false;
    _fPendingEndChildren = false;

    while (_pCurrent != NULL)
    {
        _pCurrent = _pStack.pop();
    }

    _cNodeInfoCurrent = 0;
    _lCurrentElement = 0;

    // cleanup downloads
    while (_pdc != NULL)
    {
        PopDownload();
    }

    _pCurrent = NULL;
    return S_OK;
}
/////////////////////////////////////////////////////////////////////////////
HRESULT 
XMLParser::ErrorCallback(HRESULT hr)
{
    Assert(hr == XMLStream::XML_DATAAVAILABLE ||
           hr == XMLStream::XML_DATAREALLOCATE);

    if ((int)hr == XMLStream::XML_DATAREALLOCATE)
    {
        // This is more serious.  We have to actually save away the
        // context because the buffers are about to be reallocated.
        checkhr2(CopyContext());
    }
    checkhr2(_pFactory->NotifyEvent(this, XMLNF_DATAAVAILABLE));
    return hr;
}

STDAPI GetXMLElement(LPCWSTR wszFileName, LPCWSTR pwszTag) {
    return E_FAIL;
}

STDAPI GetXMLElementAttribute(LPCWSTR pwszAttributeName, __out_ecount(cchBuffer) LPWSTR pbuffer, DWORD cchBuffer, DWORD* dwLen){
    return E_FAIL;
}


STDAPI GetXMLObjectEx(IXMLParser **ppv, CrstLevel crstLevel)
{

    if(ppv == NULL) return E_POINTER;
    *ppv = NULL;
    IXMLParser *pParser = new (nothrow) XMLParser();
    if(pParser == NULL) return E_OUTOFMEMORY;
    if (!(((XMLParser*)pParser)->ctorInit(crstLevel)))
    {
        delete pParser;
        return E_OUTOFMEMORY;
    }

    pParser->AddRef();
    *ppv = pParser;
    return S_OK;
}

STDAPI GetXMLObject(IXMLParser **ppv)
{
    HRESULT hr = S_OK;
    BEGIN_ENTRYPOINT_NOTHROW;

    hr = GetXMLObjectEx(ppv, CrstXMLParser);
    END_ENTRYPOINT_NOTHROW;
    return hr;
}


