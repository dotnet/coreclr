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
//+---------------------------------------------------------------------------
//
//  Microsoft Forms
//  File:       intl.hxx
//
//  Contents:   Codepage definitions
//
//----------------------------------------------------------------------------

#ifndef _CODEPAGE_H_
#define _CODEPAGE_H_

typedef UINT CODEPAGE;              // Codepage corresponds to Mlang ID

#define CP_UNDEFINED    CODEPAGE(-1)
#define CP_UCS_2        1200
#define CP_1250         1250
#define CP_1251         1251
#define CP_1252         1252
#define CP_1253         1253
#define CP_1254         1254
#define CP_1255         1255
#define CP_1256         1256
#define CP_1257         1257
#define CP_1258         1258

#define CP_UTF_8        65001
#define CP_UCS_4        12000

#endif  // _CODEPAGE_H_
