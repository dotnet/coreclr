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
/*****************************************************************************
 **                                                                         **
 ** Cor.h - general header for the Runtime.                                 **
 **                                                                         **
 *****************************************************************************/


#ifndef _MSCORCFG_H_
#define _MSCORCFG_H_
#include <ole2.h>                       // Definitions of OLE types.    
#include <xmlparser.h>
#include <specstrings.h>

#ifdef __cplusplus
extern "C" {
#endif

// -----------------------------------------------------------------------
// Returns an XMLParsr object. This can be used to parse any XML file.
STDAPI GetXMLElementAttribute(LPCWSTR pwszAttributeName, __out_ecount(cchBuffer) LPWSTR pbuffer, DWORD cchBuffer, DWORD* dwLen);
STDAPI GetXMLElement(LPCWSTR wszFileName, LPCWSTR pwszTag);

STDAPI GetXMLObject(IXMLParser **ppv);
STDAPI CreateConfigStream(LPCWSTR pszFileName, IStream** ppStream);

#ifdef __cplusplus
}
#endif  // __cplusplus

#endif
