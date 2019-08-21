// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "tailcallhelp.h"
#include "dllimport.h"

void TailCallHelp::CreateStoreArgsStubSig(MethodDesc* pCalleeMD, MetaSig& callSiteSig, SigBuilder* sig)
{
    // The store-args stub will be different from the target signature.
    // Specifically we insert the following things at the beginning of the
    // signature:
    // * Call target address
    // * This pointer
    // * Generic context

    // See MethodDefSig in ECMA-335 for the format.
    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    ULONG paramCount = callSiteSig.NumFixedArgs() + 1; // +1 for target address
    if (callSiteSig.HasThis())
        paramCount++;
    
    // TODO: generic context
    _ASSERTE(!(callSiteSig.GetCallingConventionInfo() & CORINFO_CALLCONV_PARAMTYPE));
    sig->AppendData(paramCount);

    // Returns void always
    sig->AppendElementType(ELEMENT_TYPE_VOID);

    // First arg is the target pointer
    sig->AppendElementType(ELEMENT_TYPE_I);

    if (callSiteSig.HasThis())
    {
        // TODO: Handle value type (this pointer should be a by-ref)
        _ASSERTE(pCalleeMD == nullptr || !pCalleeMD->GetCanonicalMethodTable()->IsValueType());
        sig->AppendElementType(ELEMENT_TYPE_OBJECT);
    }

    callSiteSig.Reset();
    TypeHandle tyHnd;
    CorElementType ty;
    while ((ty = callSiteSig.NextArgNormalized(&tyHnd)) != ELEMENT_TYPE_END)
    {
        if (tyHnd.IsNull())
        {
            sig->AppendElementType(ty);
            continue;
        }

        sig->AppendElementType(ELEMENT_TYPE_INTERNAL);
        sig->AppendPointer(tyHnd.AsPtr());
    }
}

MethodDesc* TailCallHelp::CreateStoreArgsStub(MethodDesc* pCallerMD,
                                              MethodDesc* pCalleeMD,
                                              MetaSig& callSiteSig)
{
    STANDARD_VM_CONTRACT;

    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(pCalleeMD, callSiteSig, &sigBuilder);
    SigTypeContext emptyCtx;

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sigBuilder.GetSignature(&cbSig);

    ILStubCache* stubCache = pCallerMD->GetLoaderModule()->GetILStubCache();
    AllocMemTracker memTracker;
    bool bCreatedStub = false;
    MethodDesc* pStubMD = stubCache->GetStubMethodDesc(
        nullptr,
        nullptr,
        ILSTUB_TAILCALL_STOREARGS,
        callSiteSig.GetModule(),
        pSig, cbSig,
        &memTracker,
        bCreatedStub,
        nullptr);

    return nullptr;
}