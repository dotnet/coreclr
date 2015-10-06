//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//


#ifndef __CLRPRIVBINDERASSEMBLYLOADCONTEXT_H__
#define __CLRPRIVBINDERASSEMBLYLOADCONTEXT_H__

#include "coreclrbindercommon.h"
#include "applicationcontext.hpp"
#include "clrprivbindercoreclr.h"

#if defined(FEATURE_HOST_ASSEMBLY_RESOLVER) && !defined(DACCESS_COMPILE) && !defined(CROSSGEN_COMPILE) && !defined(MDILNIGEN)

namespace BINDER_SPACE
{
    class AssemblyIdentityUTF8;
};

class AppDomain;

#ifdef FEATURE_COLLECTIBLE_ALC

class Object;
class Assembly;
class LoaderAllocator;

class DECLSPEC_UUID("68220E65-3D3F-42E2-BAD6-2D07419DAB5E") IAssemblyLoadContext : public IUnknown
{
public:
    STDMETHOD(GetIsCollectible)(
            /* [retval][out] */ BOOL *pIsCollectible) = 0;

    STDMETHOD(GetLoaderAllocator)(
            /* [retval][out] */ LoaderAllocator **ppLoaderAllocator) = 0;
};

#endif // FEATURE_COLLECTIBLE_ALC

class CLRPrivBinderAssemblyLoadContext :
#ifndef FEATURE_COLLECTIBLE_ALC
    public IUnknownCommon<ICLRPrivBinder>
#else // !FEATURE_COLLECTIBLE_ALC
    public IUnknownCommon<ICLRPrivBinder, IAssemblyLoadContext>
#endif // FEATURE_COLLECTIBLE_ALC
{
public:

    //=========================================================================
    // ICLRPrivBinder functions
    //-------------------------------------------------------------------------
    STDMETHOD(BindAssemblyByName)( 
            /* [in] */ IAssemblyName *pIAssemblyName,
            /* [retval][out] */ ICLRPrivAssembly **ppAssembly);
        
    STDMETHOD(VerifyBind)( 
            /* [in] */ IAssemblyName *pIAssemblyName,
            /* [in] */ ICLRPrivAssembly *pAssembly,
            /* [in] */ ICLRPrivAssemblyInfo *pAssemblyInfo);

    STDMETHOD(GetBinderFlags)( 
            /* [retval][out] */ DWORD *pBinderFlags);
         
    STDMETHOD(GetBinderID)( 
            /* [retval][out] */ UINT_PTR *pBinderId);
         
    STDMETHOD(FindAssemblyBySpec)( 
            /* [in] */ LPVOID pvAppDomain,
            /* [in] */ LPVOID pvAssemblySpec,
            /* [out] */ HRESULT *pResult,
            /* [out] */ ICLRPrivAssembly **ppAssembly);

#ifdef FEATURE_COLLECTIBLE_ALC

    //=========================================================================
    // IAssemblyLoadContext functions
    //-------------------------------------------------------------------------
    STDMETHOD(GetIsCollectible)(
            /* [retval][out] */ BOOL *pIsCollectible);

    STDMETHOD(GetLoaderAllocator)(
            /* [retval][out] */ LoaderAllocator **ppLoaderAllocator);

#endif // FEATURE_COLLECTIBLE_ALC

public:
    //=========================================================================
    // Class functions
    //-------------------------------------------------------------------------

    static HRESULT SetupContext(AppDomain *pAppDomain, CLRPrivBinderCoreCLR *pTPABinder, 
                                UINT_PTR ptrAssemblyLoadContext,
#ifdef FEATURE_COLLECTIBLE_ALC
                                BOOL fIsCollectible,
                                Object *pAssemblyName,
                                ::Assembly **ppDummyAssembly,
#endif // FEATURE_COLLECTIBLE_ALC
                                CLRPrivBinderAssemblyLoadContext **ppBindContext);
    
    CLRPrivBinderAssemblyLoadContext();
    
    inline BINDER_SPACE::ApplicationContext *GetAppContext()
    {
        return &m_appContext;
    }
    
    inline INT_PTR GetManagedAssemblyLoadContext()
    {
        return m_ptrManagedAssemblyLoadContext;
    }

    HRESULT BindUsingPEImage( /* in */ PEImage *pPEImage, 
                              /* in */ BOOL fIsNativeImage, 
                              /* [retval][out] */ ICLRPrivAssembly **ppAssembly);
                              
    //=========================================================================
    // Internal implementation details
    //-------------------------------------------------------------------------
private:
    HRESULT BindAssemblyByNameWorker(BINDER_SPACE::AssemblyName *pAssemblyName, BINDER_SPACE::Assembly **ppCoreCLRFoundAssembly);
            
    BINDER_SPACE::ApplicationContext m_appContext;    
    
    CLRPrivBinderCoreCLR *m_pTPABinder;
    
    INT_PTR m_ptrManagedAssemblyLoadContext;

#ifdef FEATURE_COLLECTIBLE_ALC
    BOOL m_isCollectible;

    LoaderAllocator *m_pLoaderAllocator;
#endif // FEATURE_COLLECTIBLE_ALC
};

#endif // defined(FEATURE_HOST_ASSEMBLY_RESOLVER) && !defined(DACCESS_COMPILE) && !defined(CROSSGEN_COMPILE) && !defined(MDILNIGEN)
#endif // __CLRPRIVBINDERASSEMBLYLOADCONTEXT_H__
