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
// \xmlparser\xmlhelper.hxx
//
/////////////////////////////////////////////////////////////////////////////////
#ifndef _FUSION_XMLPARSER__XMLHELPER_H_INCLUDE_
#define _FUSION_XMLPARSER__XMLHELPER_H_INCLUDE_

#include <stdio.h>
#include <string.h>
#include <windows.h>

#include "core.h"

#define checknull(a) if (!(a)) { hr = E_OUTOFMEMORY; goto error; }
#define breakhr(a) hr = (a); if (hr != S_OK) break;
#define checkhr2(a) hr = a; if (hr != S_OK) return hr;

// resolve built-in entities.
WCHAR BuiltinEntity(const WCHAR* text, ULONG len);

HRESULT HexToUnicode(const WCHAR* text, ULONG len, WCHAR& ch);
HRESULT DecimalToUnicode(const WCHAR* text, ULONG len, WCHAR& ch);

// --------------------------------------------------------------------
// A little helper class for setting a boolean flag and clearing it
// on destruction.
class BoolLock
{
    bool* _pFlag;
public:
    BoolLock(bool* pFlag)
    {
        _pFlag = pFlag;
        *pFlag = true;
    }
    ~BoolLock()
    {
        *_pFlag = false;
    }
};

//helper Functions
bool StringEquals(const WCHAR*, const WCHAR*, long, bool); 

//////////////////////////////////////////////////////////
enum
{
    FWHITESPACE    = 1,
    FDIGIT         = 2,
    FLETTER        = 4,
    FMISCNAME      = 8,
    FSTARTNAME     = 16,
    FCHARDATA      = 32
};

static const short TABLE_SIZE = 128;

static const BYTE g_anCharType[TABLE_SIZE] = { 
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0 | FWHITESPACE | FCHARDATA,
    0 | FWHITESPACE | FCHARDATA,
    0,
    0,
    0 | FWHITESPACE | FCHARDATA,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0 | FWHITESPACE | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FMISCNAME | FCHARDATA,
    0 | FMISCNAME | FCHARDATA,
    0 | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FDIGIT | FCHARDATA,
    0 | FSTARTNAME | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FMISCNAME | FSTARTNAME | FCHARDATA,
    0 | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FLETTER | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
    0 | FCHARDATA,
};

bool isDigit(WCHAR ch);
bool isHexDigit(WCHAR ch);
bool isLetter(WCHAR ch);
int isStartNameChar(WCHAR ch);
bool isCombiningChar(WCHAR ch);
bool isExtender(WCHAR ch);
bool isAlphaNumeric(WCHAR ch);
int isNameChar(WCHAR ch);
int isCharData(WCHAR ch);
int CompareUnicodeStrings(PCWSTR string1, PCWSTR string2, int length, bool fCaseInsensitive);

#endif  // _FUSION_XMLPARSER__XMLHELPER_H_INCLUDE_
