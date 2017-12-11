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
#ifdef _MSC_VER
#pragma hdrstop
#endif

#include "_rawstack.h"

//===========================================================================
RawStack::RawStack(long entrySize, long growth)
{
    _lEntrySize = entrySize;
    _pStack = NULL;
    _ncUsed = _ncSize = 0;
    _lGrowth = growth;
}
 
RawStack::~RawStack()
{
    delete [] _pStack;
}

char* 
RawStack::__push()
{
    // No magic object construction -- user has to do this.
	char* newStack = NEW (char[_lEntrySize * ( _ncSize + _lGrowth) ]);
    if (newStack == NULL)
    {
        return NULL;
    }
    ::memset(newStack, 0, _lEntrySize * (_ncSize + _lGrowth));
    if (_ncUsed > 0)
    {
        ::memcpy(newStack, _pStack, _lEntrySize * _ncUsed);
    }
    _ncSize += _lGrowth;
    delete [] _pStack;
    _pStack = newStack;

    return &_pStack[_lEntrySize * _ncUsed++];
}
