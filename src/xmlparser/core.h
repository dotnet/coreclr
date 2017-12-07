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
#ifndef _FUSION_XMLPARSER_XMLCORE_H_INCLUDE_
#define _FUSION_XMLPARSER_XMLCORE_H_INCLUDE_

#ifdef _MSC_VER
#pragma warning ( disable : 4201 )
#pragma warning ( disable : 4214 )
#pragma warning ( disable : 4251 )
#pragma warning ( disable : 4275 )
#endif

#define STRICT 1
//#include "fusioneventlog.h"
#ifdef _CRTIMP
#undef _CRTIMP
#endif
#define _CRTIMP
#include "utilcode.h"
#include <windows.h>
#define NOVTABLE __declspec(novtable)

#define UNUSED(x)

#define CHECKTYPEID(x,y) (&typeid(x)==&typeid(y))
#define AssertPMATCH(p,c) Assert(p == null || CHECKTYPEID(*p, c))

#define LENGTH(A) (sizeof(A)/sizeof(A[0]))
#include "unknwn.h"
#include "_reference.h"
#include "_unknown.h"

//#include "fusionheap.h"
//#include "util.h"

#endif // end of #ifndef _FUSION_XMLPARSER_XMLCORE_H_INCLUDE_

#define NEW(x) new (nothrow) x
#define FUSION_DBG_LEVEL_ERROR 0
#define CODEPAGE UINT
#ifndef Assert
#define Assert(x)
#endif
#ifndef ASSERT
#define ASSERT(x)
#endif
#define FN_TRACE_HR(x)
