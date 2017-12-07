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
* 
*/
#include "core.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#include "xmlhelper.h"
#include "xmlstream.h"
#include "bufferedstream.h"
#include "xmlparser.hpp"

const long BLOCK_SIZE = 512;
const long STACK_INCREMENT = 10;

// macros used in this file
#define INTERNALERROR       return XML_E_INTERNALERROR;
#define checkeof(a,b)       if (_fEOF) return b;
#define ADVANCE             hr = _pInput->nextChar(&_chLookahead, &_fEOF); if (hr != S_OK) return hr;
#define ADVANCETO(a)        hr = AdvanceTo(a);  if (hr != S_OK) return hr;
#define ISWHITESPACE(ch)    _pInput->isWhiteSpace(ch) 
#define STATE(state)        { _sSubState = state; return S_OK; }
#define GOTOSTART(state)    { _sSubState = state; goto Start; }
#define DELAYMARK(hr)       (hr == S_OK || (hr >= (HRESULT) XML_E_TOKEN_ERROR && hr < (HRESULT) XML_E_LASTERROR))
#define XML_E_FOUNDPEREF    0x8000e5ff


// The tokenizer has special handling for the following attribute types.
// These values are derived from the XML_AT_XXXX types provided in SetType
// and are also calculated during parsing of an ATTLIST for parsing of
// default values.
typedef enum 
{
    XMLTYPE_CDATA,       // the default.
    XMLTYPE_NAME,
    XMLTYPE_NAMES,
    XMLTYPE_NMTOKEN,
    XMLTYPE_NMTOKENS,
} XML_ATTRIBUTE_TYPE;

//==============================================================================
//                         a simplified table : only deal with comments, not include DOCTYPE, NotationDecl, EntityDecl and ElementDecl.
// Parse an <!^xxxxxxxx Declaration.
const StateEntry g_DeclarationTable[] =
{
// 0    '<' ^ '!' 
    { OP_CHAR, L"!", 1, (DWORD)XML_E_INTERNALERROR,  },                    
// 1    '<!' ^ '-'
    { OP_PEEK, L"-", 2, 4, 0 },                    
// 2    '<!-'
    { OP_COMMENT,  NULL, 3,   },                 
// 3    done !!
    { OP_POP,  NULL, 0, 0 },

// 4    '<!' ^ '['
    { OP_PEEK, L"[", 5, (DWORD)XML_E_BADDECLNAME, 0 },
// 5    '<![...'
    { OP_CONDSECT,  NULL, 3,   }
 
};

//==============================================================================
// Parse an <?xml or <?xml:namespace declaration.
const StateEntry g_XMLDeclarationTable[] =
{
// 0    must be xml declaration - and not xml namespace declaration        
    { OP_TOKEN, NULL, 1, XML_XMLDECL, 0 },
// 1    '<?xml' ^ S version="1.0" ...
    { OP_OWS, NULL, 2 },
// 2    '<?xml' S ^ version="1.0" ...
    { OP_SNCHAR, NULL, 3, (DWORD)XML_E_XMLDECLSYNTAX },	
// 3    '<?xml' S ^ version="1.0" ...
    { OP_NAME, NULL, 4, },
// 4    '<?xml' S version^="1.0" ...
    { OP_STRCMP, L"version", 5, 12, XML_VERSION },
// 5
    { OP_EQUALS, NULL, 6 },
// 6    '<?xml' S version = ^ "1.0" ...
    { OP_ATTRVAL, NULL, 32, 0},
// 7    '<?xml' S version '=' value ^ 
    { OP_TOKEN, NULL, 8, XML_PCDATA, -1 },
// 8    ^ are we done ?
    { OP_CHARWS, L"?", 28, 9 },    // must be '?' or whitespace.
// 9    ^ S? [encoding|standalone] '?>'
    { OP_OWS, NULL, 10 },
// 10
    { OP_CHAR, L"?", 28, 33 },    // may have '?' after skipping whitespace.
// 11    ^ [encoding|standalone] '?>'
    { OP_NAME, NULL, 12, },
// 12
    { OP_STRCMP, L"standalone", 23, 13, XML_STANDALONE },
// 13
    { OP_STRCMP, L"encoding", 14, (DWORD)XML_E_UNEXPECTED_ATTRIBUTE, XML_ENCODING },
// 14
    { OP_EQUALS, NULL, 15 },
// 15   
    { OP_ATTRVAL, NULL, 16, 0 },
// 16
    { OP_ENCODING, NULL, 17, 0, -1 },
// 17
    { OP_TOKEN, NULL, 18, XML_PCDATA, -1 },

// 18    ^ are we done ?
    { OP_CHARWS, L"?", 28, 19 },    // must be '?' or whitespace.
// 19    ^ S? standalone '?>'
    { OP_OWS, NULL, 20 },
// 20
    { OP_CHAR, L"?", 28, 34 },    // may have '?' after skipping whitespace.
// 21    ^ standalone '?>'
    { OP_NAME, NULL, 22, },
// 22 
    { OP_STRCMP, L"standalone", 23, (DWORD)XML_E_UNEXPECTED_ATTRIBUTE, 
XML_STANDALONE },
// 23
    { OP_EQUALS, NULL, 24 },
// 24
    { OP_ATTRVAL, NULL, 25, 0 },
// 25   
    { OP_STRCMP, L"yes", 31, 30, -1  },

// 26    <?xml ....... ^ '?>'   -- now expecting just the closing '?>' chars
    { OP_OWS, NULL, 27 },
// 27    
    { OP_CHAR, L"?", 28, (DWORD)XML_E_XMLDECLSYNTAX, 0 },
// 28   
    { OP_CHAR, L">", 29, (DWORD)XML_E_XMLDECLSYNTAX, 0 },
// 29    done !!
    { OP_POP,  NULL, 0, XMLStream::XML_ENDXMLDECL },

//----------------------- check standalone values  "yes" or "no"
// 30
    { OP_STRCMP, L"no", 31, (DWORD)XML_E_INVALID_STANDALONE, -1  },
// 31
    { OP_TOKEN, NULL, 26, XML_PCDATA, -1 },
    
//----------------------- check version = "1.0"
// 32
    { OP_STRCMP, L"1.0", 7, (DWORD)XML_E_INVALID_VERSION, -1 },
// 33 
    { OP_SNCHAR, NULL, 11, (DWORD)XML_E_XMLDECLSYNTAX },   
// 34 
    { OP_SNCHAR, NULL, 21, (DWORD)XML_E_XMLDECLSYNTAX },  
};

static const WCHAR* g_pstrCDATA = L"CDATA";
////////////////////////////////////////////////////////////////////////
XMLStream::XMLStream(XMLParser * pXMLParser)
:   _pStack(1), _pStreams(1)
{   
    // precondition: 'func' is never NULL
    _fnState = &XMLStream::init;
    _pInput = NULL;
    _pchBuffer = NULL;
    _fDTD = false;
	//_fInternalSubset = false;
    _cStreamDepth = 0;
    _pXMLParser = pXMLParser;

    _init();
    SetFlags(0);
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::init()
{
    HRESULT hr = S_OK;

    if (_pInput == NULL) 
	{
		//haven' called put-stream yet
        return XML_E_ENDOFINPUT;
	}
    
    _init();
    {
        _fnState =  &XMLStream::parseContent;
    }

    checkhr2(push(&XMLStream::firstAdvance,0));

    return hr;
}
////////////////////////////////////////////////////////////////////////
void
XMLStream::_init()
{
    _fEOF = false;
    //_fEOPE = false;
    _chLookahead	= 0;
    _nToken			= XML_PENDING;
    _chTerminator	= 0;
    _lLengthDelta	= 0;
	_lNslen = _lNssep = 0;
    _sSubState		= 0;
    _lMarkDelta		= 0;
	//_nAttrType = XMLTYPE_CDATA;
    _fUsingBuffer	= false;
    _lBufLen		= 0;
    if (_pchBuffer != 0)
	    delete[] _pchBuffer;
    _pchBuffer		= NULL;
    _lBufSize		= 0;
    _fDelayMark		= false;
    _fFoundWhitespace = false;
    _fFoundNonWhitespace = false;
	//_fFoundPEREf = false;
    _fWasUsingBuffer = false;
    _chNextLookahead = 0;
    //_lParseStringLevel = 0;
    //_cConditionalSection = 0;
    //_cIgnoreSectLevel = 0;
    //_fWasDTD = false;

	_fParsingAttDef = false;
    _fFoundFirstElement = false;
    _fReturnAttributeValue = true;
	//_fHandlePE = true;

    _pTable = NULL;
    //_lEOFError = 0;
}
////////////////////////////////////////////////////////////////////////
XMLStream::~XMLStream()
{
    delete _pInput;
    delete[] _pchBuffer;

    InputInfo* pi = _pStreams.peek();
    while (pi != NULL)
    {
        // Previous stream is finished also, so
        // pop it and continue on.
        delete pi->_pInput;
        pi = _pStreams.pop();
    }
}
////////////////////////////////////////////////////////////////////////
HRESULT  
XMLStream::AppendData( 
    /* [in] */ const BYTE  *buffer,
    /* [in] */ long  length,
    /* [in] */ BOOL  last)
{
    if (_pInput == NULL)
    {
        _pInput = NEW (BufferedStream(this));
        if (_pInput == NULL)
            return E_OUTOFMEMORY;
        init();
    }

    HRESULT hr = _pInput->AppendData(buffer, length, last);

    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT  
XMLStream::Reset( void)
{
    init();
    delete _pInput;
    _pInput = NULL;

    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT  
XMLStream::PushStream( 
        /* [unique][in] */ EncodingStream  *p,
        /* [in] */ bool fExternalPE)
{
	UNUSED(fExternalPE);

    if (_pStreams.used() == 0 && _pInput == NULL)
        init();

    _cStreamDepth++;

    if (_fDelayMark && _pInput != NULL)
    {
        mark(_lMarkDelta);
        _lMarkDelta = 0;
        _fDelayMark = false;
    }

    // Save current input stream.
    if (_pInput != NULL)
    {
        InputInfo* pi = _pStreams.push();
        if (pi == NULL)
            return E_OUTOFMEMORY;
 
        pi->_pInput = _pInput;
        pi->_chLookahead = _chLookahead;
        //pi->_fPE = true; // assume this is a parameter entity.
        //pi->_fExternalPE = fExternalPE;
        //pi->_fInternalSubset = _fInternalSubset;
        if (&XMLStream::skipWhiteSpace == _fnState  && _pStack.used() > 0) {
            StateInfo* pSI = _pStack.peek();
            pi->_fnState = pSI->_fnState;
        }
        else
            pi->_fnState = _fnState;
        

        // and prepend pe text with space as per xml spec.
        _chLookahead = L' ';
        _chNextLookahead = _chLookahead;
        _pInput = NULL;
    }

    _pInput = NEW (BufferedStream(this));
    if (_pInput == NULL)
        return E_OUTOFMEMORY;

    if (p != NULL)
        _pInput->Load(p);
    
    if (_chLookahead == L' ')
        _pInput->setWhiteSpace(); // _pInput didn't see this space char.
    
	return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::PopStream()
{
    // This method has to pop all streams until it finds a stream that
    // can deliver the next _chLookahead character.

    HRESULT hr = S_OK;

    InputInfo* pi = NULL;

    pi = _pStreams.peek();
    if (pi == NULL) return S_FALSE;

    _chLookahead = pi->_chLookahead;

    // Found previous stream, so we can continue.
    _fEOF = false;

    // Ok, so we actually got the next character, so
    // we can now safely throw away the previous 
    // lookahead character and return the next
    // non-whitespace character from the previous stream.
    delete _pInput;

    _pInput = pi->_pInput;
    if (_chLookahead == L' ')
        _pInput->setWhiteSpace();

    _pStreams.pop();

    _cStreamDepth--;

    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT  
XMLStream::GetNextToken( 
        /* [out] */ DWORD  *t,
        /* [out] */ const WCHAR  **text,
        /* [out] */ long  *length,
        /* [out] */ long  *nslen)
{
    HRESULT hr;

    if (_fDTD)
        return E_UNEXPECTED;

    if (_fDelayMark)
    {
        mark(_lMarkDelta);
        _lMarkDelta = 0;
        _fDelayMark = false;
    }

    hr = (this->*_fnState)();
    while (hr == S_OK && _nToken == XML_PENDING)
        hr = (this->*_fnState)();
    
    if (hr == S_OK)
    {
        *t = _nToken;
    }
    else if (hr == E_PENDING) 
    {
        *t = XML_PENDING;
        *length = *nslen = 0;
        *text = NULL;
        return hr;
    }
    else
    {
        *t = XML_PENDING;
    }
    
    // At this point hr == S_OK or it is some error.  So we
    // want to return the text of the current token, since this
    // is useful in both cases.

    if (! _fUsingBuffer)
    {
        getToken(text,length);
        if (_lLengthDelta != 0)
        { //                                                               in ParsingAttributeValue, we have to read ahead of one char '"'
            *length += _lLengthDelta;
            _lLengthDelta = 0;
        }
// This can only happen in the context of a DTD.
//        if (_fWasUsingBuffer)
//        {
//            _fUsingBuffer = _fWasUsingBuffer;
//            _fWasUsingBuffer = false;
//        }
    }
    else
    {
        *text = _pchBuffer;
        *length = _lBufLen;
        _fUsingBuffer = false;
        _fFoundWhitespace = false;
        _lBufLen = 0;
        _lLengthDelta = 0;
    }
    
    if (DELAYMARK(hr))
    {
        // Mark next time around so that error information points to the
        // beginning of this token.
        _fDelayMark = true;
    }
    else 
    {
        // otherwise mark this spot right away so we point to the exact
        // source of the error.
        mark(_lMarkDelta);
        _lMarkDelta = 0;
    }
    
    _nToken = XML_PENDING;
    *nslen = _lNslen;
    _lNslen = _lNssep = 0;

    return hr;
}

////////////////////////////////////////////////////////////////////////
ULONG  
XMLStream::GetLine()    
{
    BufferedStream* input = getCurrentStream();
    if (input != NULL)
        return input->getLine();
    return 0;
}
////////////////////////////////////////////////////////////////////////
ULONG  
XMLStream::GetLinePosition( )
{
    BufferedStream* input = getCurrentStream();
    if (input != NULL)
        return input->getLinePos();
    return 0;
}
////////////////////////////////////////////////////////////////////////
ULONG  
XMLStream::GetInputPosition( )
{
    BufferedStream* input = getCurrentStream();
    if (input != NULL)
        return input->getInputPos();
    return 0;
}
////////////////////////////////////////////////////////////////////////
HRESULT  
XMLStream::GetLineBuffer( 
    /* [out] */ const WCHAR  * *buf, ULONG* len, ULONG* startpos)
{
    if (buf == NULL || len == NULL)
        return E_INVALIDARG;

    *buf = NULL;
    BufferedStream* input = getCurrentStream();
    if (input)
        *buf = input->getLineBuf(len, startpos);
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
BufferedStream* 
XMLStream::getCurrentStream()
{
    // Return the most recent stream that
    // actually has somthing to return.
    BufferedStream* input = _pInput;
    if (!_pInput)
    {
        return NULL;
    }
    int i = _pStreams.used()-1;    
    do 
    {
        ULONG len = 0, pos = 0;
        input->getLineBuf(&len, &pos);
        if (len > 0)
            return input;

        if (i >= 0)
            input = _pStreams[i--]->_pInput;
        else
            break;
    }
    while (input != NULL);
    return NULL;
}
////////////////////////////////////////////////////////////////////////
void 
XMLStream::SetFlags( unsigned short usFlags)
{
    _usFlags = usFlags;
    // And break out the flags for performance reasons.
    //_fFloatingAmp = (usFlags & XMLFLAG_FLOATINGAMP) != 0;
    _fShortEndTags = (usFlags & XMLFLAG_SHORTENDTAGS) != 0;
    _fCaseInsensitive = (usFlags & XMLFLAG_CASEINSENSITIVE) != 0;
    _fNoNamespaces = (usFlags & XMLFLAG_NONAMESPACES) != 0;
    //_fNoWhitespaceNodes = false; // this is now bogus.  (usFlags & XMLFLAG_NOWHITESPACE) != 0;
    //_fIE4Quirks = (_usFlags & XMLFLAG_IE4QUIRKS) != 0;
    //_fNoDTDNodes = (_usFlags & XMLFLAG_NODTDNODES) != 0;
}
////////////////////////////////////////////////////////////////////////
unsigned short 
XMLStream::GetFlags()
{
    return _usFlags;
}
////////////////////////////////////////////////////////////////////////


//======================================================================
// Real Implementation
HRESULT 
XMLStream::firstAdvance()
{
    HRESULT hr;

    ADVANCE;
    checkhr2(pop(false));

    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseContent()
{
    HRESULT hr = S_OK;

    if (_fEOF)
        return XML_E_ENDOFINPUT;

    switch (_chLookahead){
    case L'<':
        ADVANCE;
        checkeof(_chLookahead, XML_E_UNCLOSEDDECL);
        switch (_chLookahead)
        {
        case L'!':
            checkhr2(_pInput->Freeze()); // stop shifting data until '>'
            return pushTable( 0, g_DeclarationTable, (DWORD)XML_E_UNCLOSEDDECL);
        case L'?':
            checkhr2(push( &XMLStream::parsePI ));
            return parsePI();
        case L'/':
            checkhr2(push(&XMLStream::parseEndTag));
            return parseEndTag();
        default:
            checkhr2(push( &XMLStream::parseElement )); // push ParseContent, and _fnState = parseElement
            if (_fFoundFirstElement)
            {
                return parseElement();
            }
            else
            {
                // Return special end prolog token and then continue with 
                // with parseElement.
                _fFoundFirstElement = true;
                _nToken = XML_ENDPROLOG;
            }
        }
        break;

    default:
        checkhr2(push(&XMLStream::parsePCData));
        return parsePCData();
        break;
    }
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::skipWhiteSpace()
{
    HRESULT hr = S_OK;

    while (ISWHITESPACE(_chLookahead) && ! _fEOF)
    {
        ADVANCE;        
    }
    checkhr2(pop(false));
    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseElement()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        checkhr2(_pInput->Freeze()); // stop shifting data until '>'
        checkhr2(push( &XMLStream::parseName, 1));
        checkhr2(parseName());
        _sSubState = 1;
        // fall through
    case 1:
        checkeof(_chLookahead, XML_E_UNCLOSEDSTARTTAG);
        _nToken = XML_ELEMENT;
        // and then try and parse the attributes, and return
        // to state 2 to finish up.  With an optimization
        // for the case where there are no attributes.
        if (_chLookahead == L'/' || _chLookahead == L'>')
        {
            _sSubState = 2;
        }
		else {
			if (!ISWHITESPACE(_chLookahead))
			{
				return XML_E_BADNAMECHAR;
			}
			
			_chEndChar = L'/'; // for empty tags.                         used to match ENDTAG
			checkhr2(push(&XMLStream::parseAttributes,2));
		}	
        
        return S_OK;
        break;

    case 2: // finish up with start tag.
        mark(); // only return '>' or '/>' in _nToken text
        if (_chLookahead == L'/')
        {
            // must be empty tag sequence '/>'.
            ADVANCE;
            _nToken = XML_EMPTYTAGEND;
        } 
        else if (_chLookahead == L'>')
        {
            _nToken = XML_TAGEND;
        }
        else if (ISWHITESPACE(_chLookahead))
        {
            return XML_E_UNEXPECTED_WHITESPACE;
        }
        else
            return XML_E_EXPECTINGTAGEND;

        _sSubState = 3;
        // fall through
    case 3:
        checkeof(_chLookahead, XML_E_UNCLOSEDSTARTTAG);
        if (_chLookahead != L'>')
        {
            if (ISWHITESPACE(_chLookahead))
                return XML_E_UNEXPECTED_WHITESPACE;
            else 
                return XML_E_EXPECTINGTAGEND;
        }
        ADVANCE; 
        mark();
        checkhr2(pop());// return to parseContent.

        return _pInput->UnFreeze(); 
        break;

    case 4: // swollow up bad tag
        // Allow the weird CDF madness <PRECACHE="YES"/>
        // For total compatibility we fake out the parser by returning
        // XML_EMPTYTAGEND, this way the rest of the tag becomes PCDATA.
        // YUK -- but it works.
        _nToken = XML_EMPTYTAGEND;
        mark();
        checkhr2(pop());// return to parseContent.
        return _pInput->UnFreeze(); 
        break;

    default:
        INTERNALERROR;
    }
    //return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseEndTag()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        ADVANCE; // soak up the '/'
        mark(); 
        // SHORT END TAG SUPPORT, IE4 Compatibility Mode only.
        if (! _fShortEndTags || _chLookahead != L'>') 
        {
            checkhr2(push( &XMLStream::parseName, 1));
            checkhr2(parseName());
        }
        _sSubState = 1;
        // fall through
        
    case 1: // finish parsing end tag
        checkeof(_chLookahead, XML_E_UNCLOSEDENDTAG);
        _nToken = XML_ENDTAG;
        checkhr2(push(&XMLStream::skipWhiteSpace, 2));
        return S_OK;

    case 2:
        checkeof(_chLookahead, XML_E_UNCLOSEDENDTAG);
        if (_chLookahead != L'>')
        {
            return XML_E_BADNAMECHAR;
        }
        ADVANCE;
        mark();
        checkhr2(pop());// return to parseContent.
        break;

    default:
        INTERNALERROR;
    }
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parsePI()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        //_fWasDTD = _fDTD; // as far as Advance is concerned, the contents
        //_fHandlePE = false;    // of a PI are not special.
        ADVANCE;
        checkhr2(_pInput->Freeze()); // stop shifting data until '?>'
        mark(); // don't include '?' in tag name.
        if (_chLookahead == L'x' || _chLookahead == L'X')
        {
            // perhaps this is the magic <?xml version="1.0"?> declaration.
            STATE(7);  // jump to state 7.
        }
        // fall through
        _sSubState = 1;
    case 1:
        checkhr2(push( &XMLStream::parseName, 2));
        checkhr2(parseName()); 
        _sSubState = 2;
        // fall through
    case 2:
        checkeof(_chLookahead, XML_E_UNCLOSEDPI);
        if (_chLookahead != L'?' && ! ISWHITESPACE(_chLookahead))
        { 
            return XML_E_BADNAMECHAR;
        }
        _nToken = XML_PI;
        STATE(3);   // found startpi _nToken and return to _sSubState 3
        break;

    case 3: // finish with rest of PI
        if (_chLookahead == L'?')
        {
            ADVANCE;
            if (_chLookahead == L'>')
            {
                STATE(6);
            }
            else
            {
                return XML_E_EXPECTINGTAGEND;
            }
        }

        checkhr2(push(&XMLStream::skipWhiteSpace, 4));
        checkhr2( skipWhiteSpace() );
        _sSubState = 4;
        // fall through

    case 4: // support for normalized whitespace
        mark(); // strip whitespace from beginning of PI data, since this is
                // just the separator between the PI target name and the PI  data.
        _sSubState = 5;
        // fallthrough

    case 5:
        while (! _fEOF )
        {
            if (_chLookahead == L'?')
            {
                ADVANCE;
                break;
            }
            if (! isCharData(_chLookahead))
                return XML_E_PIDECLSYNTAX;
            ADVANCE;
        }
        _sSubState = 6; // go to next state
        // fall through.
    case 6:
        checkeof(_chLookahead, XML_E_UNCLOSEDPI);
        if (_chLookahead == L'>')
        {
            ADVANCE;
            _lLengthDelta = -2; // don't include '?>' in PI CDATA.
        }
        else
        {
            // Hmmm.  Must be  a lone '?' so go back to state 5.
            STATE(5);
        }
        _nToken = XML_ENDPI;
        //_fHandlePE = true;
        checkhr2(pop());
        return _pInput->UnFreeze();
        break;      

    case 7: // recognize 'm' in '<?xml' declaration
        ADVANCE;
        if (_chLookahead != L'm' && _chLookahead != L'M')
        {
            STATE(11); // not 'xml' so jump to state 11 to parse name
        }
        _sSubState = 8;
        // fall through                

    case 8: // recognize L'l' in '<?xml' declaration
        ADVANCE;
        if (_chLookahead != L'l' && _chLookahead != L'L')
        {
            STATE(11); // not 'xml' so jump to state 11 to parse name
        }
        _sSubState = 9;
        // fall through                

    case 9: // now need whitespace or ':' or '?' to terminate name.
        ADVANCE;
        if (ISWHITESPACE(_chLookahead))
        {
            if (! _fCaseInsensitive)
            {
                const WCHAR* t;
                long len;
                getToken(&t,&len);
                //if (! StringEquals(L"xml",t,3,false)) // case sensitive
                //if (::FusionpCompareStrings(L"xml", 3, t, 3, false)!=0) // not equal 
				if(wcsncmp(L"xml", t, 3) != 0)
                    return XML_E_BADXMLCASE;
            }
            return pushTable(10, g_XMLDeclarationTable, (DWORD)XML_E_UNCLOSEDPI);
        }
        if (isNameChar(_chLookahead) || _chLookahead == ':')  
        {
            STATE(11); // Hmmm.  Must be something else then so continue parsing name
        }
        else
        {
            return XML_E_XMLDECLSYNTAX;
        }
        break;

    case 10:
        //_fHandlePE = true;
        checkhr2(pop());
        return _pInput->UnFreeze();
        break;

    case 11:
        if (_chLookahead == ':')
            ADVANCE;
        _sSubState = 12;
        // fall through
    case 12:
        if (isNameChar(_chLookahead))
        {
            checkhr2(push( &XMLStream::parseName, 2));
            _sSubState = 1; // but skip IsStartNameChar test
            checkhr2(parseName());
            return S_OK;
        } 
        else
        {
            STATE(2);
        }
        break;

    default:
        INTERNALERROR;
    }

    //return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseComment()
{
    // ok, so '<!-' has been parsed so far
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        //_fWasDTD = _fDTD; // as far as the DTD is concerned, the contents
        //_fHandlePE = false;    // of a COMMENT are not special.
        ADVANCE; // soak up first '-'
        checkeof(_chLookahead, XML_E_UNCLOSEDCOMMENT);
        if (_chLookahead != L'-')
        {
            return XML_E_COMMENTSYNTAX;
        }
        _sSubState = 1;
        // fall through
    case 1:
        ADVANCE; // soak up second '-'
        mark(); // don't include '<!--' in comment text
        _sSubState = 2;
        // fall through;
    case 2:
        while (! _fEOF)
        {
            if (_chLookahead == L'-')
            {
                ADVANCE; // soak up first closing L'-'                
                break;
            }
            if (! isCharData(_chLookahead))
                return XML_E_BADCHARDATA;
            ADVANCE;
        }
        checkeof(_chLookahead, XML_E_UNCLOSEDCOMMENT);
        _sSubState = 3; // advance to next state        
        // fall through.
    case 3:
        if (_chLookahead != L'-')
        {
            // Hmmm, must have been a floating L'-' so go back to state 2
            STATE(2);
        }
        ADVANCE; // soak up second closing L'-'
        _sSubState = 4; 
        // fall through
    case 4:
        checkeof(_chLookahead, XML_E_UNCLOSEDCOMMENT);
        //if (_chLookahead != L'>' && ! _fIE4Quirks)
		if (_chLookahead != L'>')
        {
            // cannot have floating L'--' unless we are in compatibility mode.
            return XML_E_COMMENTSYNTAX;
        }
        ADVANCE; // soak up closing L'>'
        _lLengthDelta = -3; // don't include L'-->' in PI CDATA.
        _nToken = XML_COMMENT;
        checkhr2(pop());
        //_fHandlePE = true;
        break;

    default:
        INTERNALERROR;
    }    
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseName()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        if (! isStartNameChar(_chLookahead))
        {
            if (ISWHITESPACE(_chLookahead))
                hr = XML_E_UNEXPECTED_WHITESPACE;
            else
                hr = XML_E_BADSTARTNAMECHAR;
            goto CleanUp;
        }
        mark(); 
        _sSubState = 1;
        // fall through

    case 1:
		_lNslen = _lNssep = 0;
        while (isNameChar(_chLookahead) && !_fEOF)
        {
            ADVANCE;
        }
        hr = pop(false); // return to the previous state
        break;

    default:
        INTERNALERROR;
    }

CleanUp:
    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseAttributes()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        //_nAttrType = XMLTYPE_CDATA;
        _fCheckAttribute = false;
        checkhr2(push(&XMLStream::skipWhiteSpace, 1));
        checkhr2( skipWhiteSpace() );
        _sSubState = 1;
        // fall through
    case 1:
        if (_chLookahead == _chEndChar || _chLookahead == L'>' )
        {
            checkhr2(pop()); // no attributes.
            return S_OK;
        }
        checkhr2( push( &XMLStream::parseName, 2 ) );
        checkhr2( parseName() );

        if (!ISWHITESPACE(_chLookahead) && _chLookahead != L'=')
        {
            return XML_E_BADNAMECHAR;
        }
        _sSubState = 2;
        // fall through
    case 2:
        if (ISWHITESPACE(_chLookahead))
        {
            // Eq ::= S? '=' S?
            STATE(7);
        }

        checkeof(_chLookahead, XML_E_UNCLOSEDSTARTTAG);
        _nToken = XML_ATTRIBUTE;    
        _sSubState = 3;
        return S_OK;
        break;

    case 3:
        if (ISWHITESPACE(_chLookahead))
            return XML_E_UNEXPECTED_WHITESPACE;
        _fWhitespace = false;
        _sSubState = 4;
        // fall through

    case 4:
        if (_chLookahead != L'=')
        {
            return XML_E_MISSINGEQUALS;
        }
        ADVANCE;
        if (ISWHITESPACE(_chLookahead))
        {
            // allow whitespace between '=' and attribute value.
            checkhr2(push(&XMLStream::skipWhiteSpace, 5));
            checkhr2( skipWhiteSpace() );            
        }
        _sSubState = 5;
        // fall through

    case 5:
        if (ISWHITESPACE(_chLookahead))
            return XML_E_UNEXPECTED_WHITESPACE;
        if (_chLookahead != L'"' && _chLookahead != L'\'')
        {
            return XML_E_MISSINGQUOTE;
        }
        _chTerminator = _chLookahead;
        ADVANCE;
        mark(); 
        return push(&XMLStream::parseAttrValue, 6);
        //_sSubState = 6;
    // fall through;

    case 6:
        checkeof(_chLookahead, XML_E_UNCLOSEDSTARTTAG);
        if (_chLookahead == _chEndChar || _chLookahead == L'>')
        {
            checkhr2(pop());
            return S_OK;
        }
        if (! ISWHITESPACE(_chLookahead) )
        {
            return XML_E_MISSINGWHITESPACE;
        }
        STATE(0); // go back to state 0
        break;

    case 7:
        // allow whitespace between attribute and '='
        _lLengthDelta = _pInput->getTokenLength();
        checkhr2(push(&XMLStream::skipWhiteSpace, 8));
        checkhr2( skipWhiteSpace() );       
        _sSubState = 8;
        // fall through

    case 8:
        checkeof(_chLookahead, XML_E_UNCLOSEDSTARTTAG);
        _lLengthDelta -= _pInput->getTokenLength();
        STATE(2);
        break;

    default:
        INTERNALERROR;
    }
    //return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT XMLStream::parseAttrValue()
{
    HRESULT hr = S_OK;

    switch (_sSubState)
    {
    case 0: 
        _fParsingAttDef = true;        
        // mark beginning of attribute data           
        _sSubState =  2;
        // fall through;

    case 2:
        while ( _chLookahead != _chTerminator && 
                _chLookahead != L'<' &&
                ! _fEOF  ) 
        {
            if (_chLookahead == L'&')
            {
                // then parse entity ref and then return
                // to state 2 to continue with PCDATA.
                return push(&XMLStream::parseEntityRef,2);
            }
            hr = _pInput->scanPCData(&_chLookahead, &_fWhitespace);
            if (FAILED(hr))
            {
                if (hr == E_PENDING)
                {
                    hr = S_OK;
                    ADVANCE;
                }
                return hr;
            }
        }
        _sSubState = 3;
        // fall through
    case 3:
        checkeof(_chLookahead, XML_E_UNCLOSEDSTRING);
        if (_chLookahead == _chTerminator)
        {
            ADVANCE;
            if (_fReturnAttributeValue)
            {
                // return what we have so far - if anything.
                if ((_fUsingBuffer && _lBufLen > 0) ||
                    _pInput->getTokenLength() > 1)
                {
                    _lLengthDelta = -1; // don't include string _chTerminator.
                    _nToken = XML_PCDATA;
                }
            }
            else
            {
                _fReturnAttributeValue = true; // reset to default value.
            }
            _fParsingAttDef = false;
            checkhr2(pop());
            return S_OK;
        } 
        else
        {
            return XML_E_BADCHARINSTRING;
        }        
        break;

    default:
        INTERNALERROR;
    }
    //return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::ScanHexDigits()
{
    HRESULT hr = S_OK;
    while (! _fEOF && _chLookahead != L';')
    {
        if (! isHexDigit(_chLookahead))
        {
            return ISWHITESPACE(_chLookahead) ? XML_E_UNEXPECTED_WHITESPACE : XML_E_BADCHARINENTREF;
        }
        ADVANCE;
    }
    checkeof(_chLookahead, XML_E_UNEXPECTEDEOF);
    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::ScanDecimalDigits()
{
    HRESULT hr = S_OK;
    while (! _fEOF && _chLookahead != L';')
    {
        if (! isDigit(_chLookahead))
        {
            return ISWHITESPACE(_chLookahead) ? XML_E_UNEXPECTED_WHITESPACE : XML_E_BADCHARINENTREF;
        }
        ADVANCE;
    }
    checkeof(_chLookahead, XML_E_UNEXPECTEDEOF);
    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parsePCData()
{
    HRESULT hr = S_OK;

    switch (_sSubState)
    {
    case 0:
        _fWhitespace = true;
        _sSubState = 1;
        // fall through;

    case 1:
        // This state is used when we are not normalizing white space.  This
        // is a separate state for performance reasons.  
        // Normalizing whitespace is about 11% slower.
        while (_chLookahead != L'<' && ! _fEOF )
        {
             if (_chLookahead == L'&')
            {
                // then parse entity ref and then return
                // to state 1 to continue with PCDATA.
                return push(&XMLStream::parseEntityRef,1);
            }

            if (_chLookahead == L'>')
            {
                WCHAR* pText;
                long len;
                _pInput->getToken((const WCHAR**)&pText, &len);
                //if (len >= 2 && StrCmpN(L"]]", pText + len - 2, 2) == 0)
//                if ((len >= 2) && (::FusionpCompareStrings(L"]]", 2, pText + len - 2, 2, false)==0))
                  if ((len >= 2) && (wcsncmp(L"]]", pText + len - 2, 2)==0))
		             return XML_E_INVALID_CDATACLOSINGTAG;               
            }
// This slows us down too much.
//            else if (! isCharData(_chLookahead))
//            {
//                return XML_E_BADCHARDATA;
//            }

            hr = _pInput->scanPCData(&_chLookahead, &_fWhitespace);
            if (FAILED(hr))
            {
                if (hr == E_PENDING)
                {
                    hr = S_OK;
                    ADVANCE;
                }
                return hr;
            }
            checkhr2(hr);
        }
        _sSubState = 2;
        // fall through

    case 2:
        if (_pInput->getTokenLength() > 0 || _fUsingBuffer)
        {
            _nToken = _fWhitespace ? XML_WHITESPACE : XML_PCDATA;
        }
        checkhr2(pop());
        break;

    default:
        INTERNALERROR;
    }   
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseEntityRef()
{
    HRESULT hr = S_OK;
    long entityLen = 0, lLen = 1;
    const WCHAR* t = 0; 
    long len = 0;

Start:
    switch (_sSubState)
    {
    case 0: // ^ ( '&#' [0-9]+ ) | ('&#X' [0-9a-fA-F]+) | ('&' Name) ';'
        _nPreToken = XML_PENDING;
        _lEntityPos = _pInput->getTokenLength(); // record entity position.
        _fPCDataPending = (_lEntityPos > 0);

        if (PreEntityText())
        {
            // remember the pending text before parsing the entity.
            _nPreToken = _nToken;
            _nToken = XML_PENDING;
        }
        _sSubState = 1;
        // fall through
    case 1:
        ADVANCE; // soak up the '&'
        _sSubState = 2;
        // fall through
    case 2:
        checkeof(_chLookahead, XML_E_UNEXPECTEDEOF);
        if (_chLookahead == L'#')
        {
            ADVANCE;
            _sSubState = 3;
            // fall through
        }
        else
        {
            // Loose entity parsing allows "...&6..."
            if (! isStartNameChar(_chLookahead))
            {
				if (ISWHITESPACE(_chLookahead))
                    return XML_E_UNEXPECTED_WHITESPACE;
                else
                    return XML_E_BADSTARTNAMECHAR;
            }
            checkhr2(push(&XMLStream::parseName, 6));
            _sSubState = 1; // avoid doing a mark() so we can return PCDATA if necessary.
            return parseName();
        }
        break;

        // ------------- Numeric entity references --------------------
    case 3:
        checkeof(_chLookahead, XML_E_UNEXPECTEDEOF);
        if (_chLookahead == L'x')
        {
            // hex character reference.
            ADVANCE;
            STATE(5); // go to state 5
        }
        _sSubState = 4;
        // fall through

    case 4: // '&#' ^ [0-9]+ ';'
        checkhr2(ScanDecimalDigits());
        if (_chLookahead != L';')
        {
            STATE(9);
        }

        entityLen = _pInput->getTokenLength() - _lEntityPos;
        getToken(&t, &len);
        checkhr2(DecimalToUnicode(t + _lEntityPos + 2, entityLen - 2, _wcEntityValue));
        lLen = 2;
        _nToken = XML_NUMENTITYREF;
        GOTOSTART(10); // have to use GOTOSTART() because we want to use the values of t and len
        break;

    case 5: // '&#X' ^ [0-9a-fA-F]+
        checkhr2(ScanHexDigits());
        if (_chLookahead != L';')
        {
            STATE(9);
        }

        entityLen = _pInput->getTokenLength() - _lEntityPos;
        getToken(&t, &len);
        checkhr2(HexToUnicode(t + _lEntityPos + 3, entityLen - 3, _wcEntityValue));
        lLen = 3;
        _nToken = XML_HEXENTITYREF;
        GOTOSTART(10);  // have to use GOTOSTART() because we want to use the values of t and len
        break;
        
        // ------------- Named Entity References --------------------
    case 6: // '&' Name ^ ';'
        checkeof(_chLookahead, XML_E_UNEXPECTEDEOF);
        if (_chLookahead != L';')
        {
            STATE(9);
        }

        // If parseName found a namespace then we need to calculate the
        // real nslen taking the pending PC data and '&' into account
        // and remember this in case we have to return the PCDATA.
        _nEntityNSLen = (_lNslen > 0) ? _lNslen - _lEntityPos - 1 : 0;
        _fUsingBuffer = false;

        entityLen = _pInput->getTokenLength() - _lEntityPos;
        getToken(&t, &len);

        if (0 != (_wcEntityValue = BuiltinEntity(t + _lEntityPos + 1, entityLen - 1))) //||
            //(_fIE4Quirks && 0xFFFF != (_wcEntityValue = LookupBuiltinEntity(t + _lEntityPos + 1, entityLen - 1))))
        {
            lLen = 1;
            _nToken = XML_BUILTINENTITYREF;
            GOTOSTART(10);  // have to use GOTOSTART() because we want to use the values of t and len
        }
        else
			 //             if it is not a builtIn ref, we would return error
			return XML_E_MISSINGSEMICOLON;
		break; 
    case 8:
        mark();
        checkhr2(pop());
        return S_OK;
    case 10:
        // Return the text before builtin or char entityref as XML_PCDATA
        if (_nPreToken)
        {
            _nPreToken = _nToken;
            _nToken = XML_PCDATA;
            _lLengthDelta = -entityLen;
            _lMarkDelta = entityLen - lLen; // don't include '&' in _nToken.
            STATE(11);  // return token and resume in state 12.
        }
        else
        {
            _nPreToken = _nToken;
            mark(entityLen - lLen);
            GOTOSTART(11);
        }
        break;

    case 11:
        // push the builtin entity
        _fUsingBuffer = true;
        PushChar(_wcEntityValue);
        _nToken = _nPreToken;
        STATE(12); // return token and resume in state 12.
        break;

    case 12:
        ADVANCE; // soak up the ';'
        STATE(8); // resume in state 8.
        break;

    default:
        INTERNALERROR;
    }   
    return S_OK;      
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::pushTable(short substate, const StateEntry* table, DWORD le)
{
    HRESULT hr = S_OK;

    checkhr2(push(&XMLStream::parseTable, substate));
	_pTable = table;
	UNUSED(le);
    //_lEOFError = le;
    return hr;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::push(StateFunc f, short s)
{
    StateInfo* pSI = _pStack.push();
    if (pSI == NULL)
        return E_OUTOFMEMORY;

    pSI->_sSubState = s;
    pSI->_fnState = _fnState;
	pSI->_pTable = _pTable;
	pSI->_cStreamDepth = _cStreamDepth;


    _sSubState = 0;
    _fnState = f;

    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT
XMLStream::pop(bool boundary)
{
    StateInfo* pSI = _pStack.peek();

    _ASSERTE(pSI);
    
    if (pSI == NULL)
        return E_UNEXPECTED;

    if (_fDTD && 
        ! (_fParsingAttDef) && boundary && _cStreamDepth != pSI->_cStreamDepth) // _fParsingNames || 
    {
        // If we are in a PE and we are popping out to a state that is NOT in a PE
        // and this is a pop where we need to check this condition, then return an error.
        // For example, the following is not well formed because the parameter entity
        // pops us out of the ContentModel state in which the PE was found:
        // <!DOCTYPE foo [
        //      <!ENTITY % foo "a)">
        //      <!ELEMENT bar ( %foo; >
        //  ]>...
        return XML_E_PE_NESTING;
    }

    _fnState	= pSI->_fnState;
    _sSubState	= pSI->_sSubState;
    _pTable		= pSI->_pTable;
    //_lEOFError	= pSI->_lEOFError;
    _pStack.pop();

    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::switchTo(StateFunc f)
{
    HRESULT hr;

    // Make sure we keep the old stream depth.
    StateInfo* pSI = _pStack.peek();

    _ASSERTE(pSI);
    
    if (pSI == NULL)
        return E_UNEXPECTED;
    
    int currentDepth = _cStreamDepth;
    _cStreamDepth = pSI->_cStreamDepth;

    checkhr2(pop(false));
    checkhr2(push(f,_sSubState)); // keep return to _sSubState the same

    _cStreamDepth = currentDepth;

    return (this->*f)();
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseCondSect()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        ADVANCE; // soak up the '[' character
        //if (_fFoundPEREf) return S_OK;
        _sSubState = 1;
        // fall through
    case 1: // now match magic '[CDATA[' sequence.     
        checkeof(_chLookahead, XML_E_UNCLOSEDMARKUPDECL);
        if (_chLookahead == L'C')
        {
            _pchCDataState = g_pstrCDATA;
            STATE(5); // goto state 5
        }
        _sSubState = 2;   // must be IGNORE, INCLUDE or %pe;
        // fall through

    case 2: // must be DTD markup declaration
        // '<![' ^ S? ('INCLUDE' | 'IGNORE' | %pe;) S? [...]]> or 
        // skip optional whitespace
        //if (_fInternalSubset)
        //    return XML_E_CONDSECTINSUBSET;
        checkeof(_chLookahead, XML_E_EXPECTINGOPENBRACKET);
        checkhr2(push(&XMLStream::skipWhiteSpace, 3));
        return skipWhiteSpace(); // must return because of %pe;

    case 3:
        checkeof(_chLookahead, XML_E_UNCLOSEDMARKUPDECL);
        checkhr2(push(&XMLStream::parseName,4));
        return parseName();

    case 4: // scanned 'INCLUDE' or 'IGNORE'
        {
            const WCHAR* t;
            long len;
            getToken(&t,&len);
            //if (StringEquals(L"IGNORE",t,len,false))
            //{
            //    return switchTo(&XMLStream::parseIgnoreSect);
            //}
            //else if (StringEquals(L"INCLUDE",t,len,false))
            //{
            //    return switchTo(&XMLStream::parseIncludeSect);
            //}
            //else
                return XML_E_BADENDCONDSECT;
        }
        break;

    case 5: // parse CDATA name
        while (*_pchCDataState != 0 && _chLookahead == *_pchCDataState && ! _fEOF)
        {
            ADVANCE;            // advance first, before incrementing _pchCDataState
            _pchCDataState++;   // so that this state is re-entrant in the E_PENDING case.
            checkeof(_chLookahead, XML_E_UNCLOSEDMARKUPDECL);
        }
        if (*_pchCDataState != 0)
        {
            // must be INCLUDE or IGNORE section so go to state 2.
            _sSubState = 2;
        } 
        else if (_chLookahead != L'[')
        {
            return XML_E_EXPECTINGOPENBRACKET;
        }
        else if (_fDTD)
            return XML_E_CDATAINVALID;
        else
            return switchTo(&XMLStream::parseCData);

        return S_OK;
        break;        

    default:
        INTERNALERROR;
    }
    return S_OK;
}

////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseCData()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0:
        ADVANCE; // soak up the '[' character.
        mark(); // don't include 'CDATA[' in CDATA text
        _sSubState = 1;
        // fall through
    case 1:
        while (_chLookahead != L']' && ! _fEOF)
        {
            // scanPCData will stop when it sees a ']' character.
            hr = _pInput->scanPCData(&_chLookahead, &_fWhitespace);
            if (FAILED(hr))
            {
                if (hr == E_PENDING)
                {
                    hr = S_OK;
                    ADVANCE;
                }
                return hr;
            }
        }
        checkeof(_chLookahead, XML_E_UNCLOSEDCDATA);
        _sSubState = 2;
        // fall through
    case 2:
        ADVANCE; // soak up first L']' character.
        checkeof(_chLookahead, XML_E_UNCLOSEDCDATA);
        if (_chLookahead != L']')
        {
            // must have been floating ']' character, so
            // return to state 1.
            STATE(1); 
        }
        _sSubState = 3;
        // fall through
    case 3:
        ADVANCE; // soak up second ']' character.
        checkeof(_chLookahead, XML_E_UNCLOSEDCDATA);
        if (_chLookahead == L']')
        {
            // Ah, an extra ']' character, tricky !!  
            // In this case we stay in state 3 until we find a non ']' character
            // so you can terminate a CDATA section with ']]]]]]]]]]]]]]]]>'
            // and everying except the final ']]>' is treated as CDATA.
            STATE(3);
        }
        else if (_chLookahead != L'>')
        {
            // must have been floating "]]" pair, so
            // return to state 1.
            STATE(1);
        }
        _sSubState = 4;
        // fall through
    case 4:
        ADVANCE; // soak up the '>'
        _nToken = XML_CDATA;
        _lLengthDelta = -3; // don't include terminating ']]>' in text.
        checkhr2(pop()); // return to parseContent.
        return S_OK;
        break;

    default:
        INTERNALERROR;
    }
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT
XMLStream::parseEquals()
{
    HRESULT hr = S_OK;
    switch (_sSubState)
    {
    case 0: // Eq ::= S? '=' S? 
        if (ISWHITESPACE(_chLookahead))
        {
            // allow whitespace between attribute and '='
            checkhr2(push(&XMLStream::skipWhiteSpace, 1));
            checkhr2( skipWhiteSpace() );            
        }
        _sSubState = 1;
        // fall through

    case 1:
        if (_chLookahead != L'=')
        {
            return XML_E_MISSINGEQUALS;
        }
        ADVANCE;
        if (ISWHITESPACE(_chLookahead))
        {
            // allow whitespace between '=' and attribute value.
            checkhr2(push(&XMLStream::skipWhiteSpace, 2));
            checkhr2( skipWhiteSpace() );            
        }
        _sSubState = 2;
        // fall through

    case 2:
        checkhr2(pop(false));
        break;

    default:
        INTERNALERROR;

    }
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::parseTable()
{
    HRESULT hr = S_OK;

    while (hr == S_OK && _nToken == XML_PENDING)
    {
        const StateEntry* pSE = &_pTable[_sSubState];

        DWORD newState = pSE->_sGoto;

        switch (pSE->_sOp)
        {
        case OP_WS:
            //checkeof(_chLookahead, _lEOFError);
            if (! ISWHITESPACE(_chLookahead))
                return XML_E_MISSINGWHITESPACE;
            // fall through
        case OP_OWS:
            //checkeof(_chLookahead, _lEOFError);
            checkhr2(push(&XMLStream::skipWhiteSpace, (short)newState));
            checkhr2(skipWhiteSpace());
            //if (_fFoundPEREf) return XML_E_FOUNDPEREF;
            break;
        case OP_CHARWS:
            //if (_fFoundPEREf) return S_OK;
            mark();
            //checkeof(_chLookahead, _lEOFError);
            if (_chLookahead == pSE->_pch[0])
            {
                ADVANCE;
                newState = pSE->_sGoto;
                _nToken = pSE->_lDelta;
            }
            else if (! ISWHITESPACE(_chLookahead))
            {
                return XML_E_WHITESPACEORQUESTIONMARK;
            }
            else
                newState = pSE->_sArg1;
            break;
        case OP_CHAR:
            //if (_fFoundPEREf) return S_OK;
            mark();
        case OP_CHAR2:
            //if (_fFoundPEREf) return S_OK;
            //checkeof(_chLookahead, _lEOFError);
            if (_chLookahead == pSE->_pch[0])
            {
                ADVANCE;
                newState = pSE->_sGoto;
                _nToken = pSE->_lDelta;
                //if (_nToken == XML_GROUP)
                    //_nAttrType = XMLTYPE_NMTOKEN;
            }
            else
            {
                newState = pSE->_sArg1;
                if (newState >= XML_E_PARSEERRORBASE &&
                    ISWHITESPACE(_chLookahead))
                    return XML_E_UNEXPECTED_WHITESPACE;
            }
            break;
        case OP_PEEK:
            //if (_fFoundPEREf) return S_OK;
            //checkeof(_chLookahead, _lEOFError);
            if (_chLookahead == pSE->_pch[0])
            {
                newState = pSE->_sGoto;
            }
            else
                newState = pSE->_sArg1;
            break;

        case OP_NAME:
            //if (_fFoundPEREf) return S_OK;
            //checkeof(_chLookahead, _lEOFError);
            checkhr2(push(&XMLStream::parseName, (short)newState));
            checkhr2(parseName());
            break;
        case OP_TOKEN:
            _nToken = pSE->_sArg1;
            _lLengthDelta = pSE->_lDelta;  
            break;
        case OP_POP:
            _lLengthDelta = pSE->_lDelta;
            if (_lLengthDelta == 0) mark();
            // The _lDelta field contains a boolean flag to tell us whether this
            // pop needs to check for parameter entity boundary or not.
            checkhr2(pop(pSE->_lDelta == 0)); // we're done !
            _nToken = pSE->_sArg1;
            //_nAttrType = XMLTYPE_CDATA;
            return S_OK;
        case OP_STRCMP:
            {
                // 428740: Prefix complained about null ptr deref.
                const WCHAR* t=L"";
                long len=0;
                getToken(&t,&len);
                long delta = (pSE->_lDelta < 0) ? pSE->_lDelta : 0;
                //if (StringEquals(pSE->_pch,t,len+delta,_fCaseInsensitive))
                //if (::FusionpCompareStrings(pSE->_pch, len+delta, t, len+delta, _fCaseInsensitive)==0)
				if (CompareUnicodeStrings(pSE->_pch, t, len+delta, _fCaseInsensitive)==0)
                {
                    if (pSE->_lDelta > 0) 
                    {
                        _nToken = pSE->_lDelta;
                        _lLengthDelta = 0;
                    }

					newState = pSE->_sGoto;
                }
                else
                    newState = pSE->_sArg1;
             }
             break;

        case OP_COMMENT:
            return push(&XMLStream::parseComment, (short)newState);
            break;

        case OP_CONDSECT:
            //if (_fFoundPEREf) return S_OK;
            // parse <![CDATA[...]]> or <![IGNORE[...]]>
            return push(&XMLStream::parseCondSect, (short)newState);

        case OP_SNCHAR:
            //checkeof(_chLookahead, _lEOFError);
            if (isStartNameChar(_chLookahead))
            {
                newState = pSE->_sGoto;
            }
            else
                newState = pSE->_sArg1;
            break;
        case OP_EQUALS:
            //if (_fFoundPEREf) return S_OK;
            //checkeof(_chLookahead, _lEOFError);
            checkhr2(push(&XMLStream::parseEquals, (short)newState));
            checkhr2(parseEquals());
            break;
        case OP_ENCODING:
            {
                // 429011: Prefix complained correctly about unitialized t
                const WCHAR* t = L"";
                long len = 0;
                _pInput->getToken(&t,&len);
                hr =  _pInput->switchEncoding(t, len+pSE->_lDelta);
            }
            break;

        case OP_ATTRVAL:
            //if (_fFoundPEREf) return S_OK;
            if (_chLookahead != L'"' && _chLookahead != L'\'')
            {
                return XML_E_MISSINGQUOTE;
            }  
            _chTerminator = _chLookahead;
            ADVANCE; 
            mark();
            _fReturnAttributeValue = (pSE->_sArg1 == 1);
            //checkeof(_chLookahead, _lEOFError);
            return push(&XMLStream::parseAttrValue, (short)newState);
            break;

        default:
            break;
            
        } // end of switch
        if (_fnState != &XMLStream::parseTable)
            return S_OK;

        if (newState >= XML_E_PARSEERRORBASE)
            return (HRESULT)newState;
        else
            _sSubState = (short)newState;
    } // end of while

    if (_nToken == XMLStream::XML_ENDDECL)
    {
        return _pInput->UnFreeze();
    }
    return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT    
XMLStream::_PushChar(WCHAR ch) 
{
    // buffer needs to grow.
    long   newsize =  (_lBufSize+512)*2 ;
    WCHAR* newbuf = NEW ( WCHAR[newsize]);
    if (newbuf == NULL)
        return E_OUTOFMEMORY;

    if (_pchBuffer != NULL){
        ::memcpy(newbuf, _pchBuffer, sizeof(WCHAR)*_lBufLen);
        delete[] _pchBuffer;
    }

    _lBufSize = newsize;
    _pchBuffer = newbuf;   
    _pchBuffer[_lBufLen++] = ch;
    
	return S_OK;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::AdvanceTo(short substate)
{
    // This method combines and advance with a state switch in one
    // atomic operation that handles the E_PENDING case properly.

    _sSubState = substate;

    //HRESULT hr = (!_fDTD) ? _pInput->nextChar(&_chLookahead, &_fEOF) : DTDAdvance(); 
	HRESULT hr = _pInput->nextChar(&_chLookahead, &_fEOF) ; 
    if (hr != S_OK && (hr == (HRESULT) E_PENDING || hr == (HRESULT) E_DATA_AVAILABLE || hr == (HRESULT) E_DATA_REALLOCATE || hr == (HRESULT) XML_E_FOUNDPEREF))
    {
        // Then we must do an advance next time around before continuing
        // with previous state.  Push will save the _sSubState and return
        // to it.
        push(&XMLStream::firstAdvance,substate);
    }    
    return hr;
}
////////////////////////////////////////////////////////////////////////
bool
XMLStream::PreEntityText()
{
    // This is a helper function that calculates whether or not to
    // return some PCDATA or WHITEPACE before an entity reference.
    if (_fPCDataPending)
    {
        // return what we have so far.
        //if (_fWhitespace && ! _fIE4Quirks) // in IE4 mode we do not have WHITESPACE nodes
                                           // and entities are always resolved, so return
                                           // the leading whitespace as PCDATA.
		if (_fWhitespace )
            _nToken = XML_WHITESPACE;                                
        else                               
            _nToken = XML_PCDATA;

        long entityLen = _pInput->getTokenLength() - _lEntityPos;
        _lLengthDelta = -entityLen;
        _lMarkDelta = entityLen;
        _fPCDataPending = false;
        _fWhitespace = true;
        return true;
    }

    return false;
}
////////////////////////////////////////////////////////////////////////
HRESULT 
XMLStream::ErrorCallback(HRESULT hr)
{
    if (hr == (HRESULT) E_DATA_AVAILABLE)
        hr = XML_DATAAVAILABLE;
    else if (hr == (HRESULT) E_DATA_REALLOCATE)
        hr = XML_DATAREALLOCATE;
    return _pXMLParser->ErrorCallback(hr);
}
