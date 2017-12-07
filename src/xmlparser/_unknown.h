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
#ifndef _UNKNOWN_HXX
#define _UNKNOWN_HXX

#include "core.h"
//===========================================================================
// This template implements the IUnknown portion of a given COM interface.

template <class I, const IID* I_IID> class _unknown : public I
{
private:    LONG _refcount;

public:        
        _unknown() 
        { 
            _refcount = 0;
        }

        virtual ~_unknown()
        {
        }

        virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void ** ppvObject)
        {
            if (riid == IID_IUnknown)
            {
                *ppvObject = static_cast<IUnknown*>(this);
            }
            else if (riid == *I_IID)
            {
                *ppvObject = static_cast<I*>(this);
            }
            else
                return E_NOINTERFACE;
            
            reinterpret_cast<IUnknown*>(*ppvObject)->AddRef();
            return S_OK;
        }
    
        virtual ULONG STDMETHODCALLTYPE AddRef( void)
        {
            return InterlockedIncrement(&_refcount);
        }
    
        virtual ULONG STDMETHODCALLTYPE Release( void)
        {
            ULONG count;
            if ((count = InterlockedDecrement(&_refcount)) == 0)
            {
                delete this;
                return 0;
            }
            return count;
        }
};    

#endif // _UNKNOWN_HXX
