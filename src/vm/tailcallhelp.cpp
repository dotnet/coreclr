// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "tailcallhelp.h"
#include "dllimport.h"
#include <formattype.h>
#include <sigformat.h>

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
    CorElementType ty;
    while (true)
    {
        CorElementType ty;
        if ((ty = callSiteSig.NextArgNormalized()) == ELEMENT_TYPE_END)
            break;

        TypeHandle tyHnd = callSiteSig.GetLastTypeHandleThrowing();
        if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
        {
            sig->AppendElementType(ELEMENT_TYPE_I);
            continue;
        }

        if (tyHnd.IsByRef())
        {
            sig->AppendElementType(ELEMENT_TYPE_BYREF);
            sig->AppendElementType(ELEMENT_TYPE_U1);
            continue;
        }

        if (!tyHnd.IsValueType())
        {
            sig->AppendElementType(ELEMENT_TYPE_OBJECT);
            continue;
        }

        SigPointer pArg = callSiteSig.GetArgProps();
        pArg.ConvertToInternalExactlyOne(callSiteSig.GetModule(), callSiteSig.GetSigTypeContext(), sig);
    }

#ifdef _DEBUG
    SigFormat incSig(callSiteSig, NULL);
    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sig->GetSignature(&cbSig);
    SigTypeContext emptyContext;
    MetaSig outMsig(pSig, cbSig, callSiteSig.GetModule(), &emptyContext);
    SigFormat outSig(outMsig, NULL);
    printf("Incoming sig: %s\n", incSig.GetCString());
    printf("Outgoing sig: %s\n", outSig.GetCString());
#endif // _DEBUG
}

namespace
{
    template<typename Func>
    void ProcessArgsAligned(MetaSig& callSiteSig, Func func)
    {
        callSiteSig.Reset();
        UINT offset = 0;
        CorElementType ty;
        while ((ty = callSiteSig.NextArg()) != ELEMENT_TYPE_END)
        {
            TypeHandle tyHnd = callSiteSig.GetLastTypeHandleThrowing();
            unsigned int alignment = CEEInfo::getClassAlignmentRequirementStatic(tyHnd);
            offset = AlignUp(offset, alignment);
            unsigned int size = tyHnd.GetSize();

            func(tyHnd, offset, size);
            offset += size;
        }
    }
}

MethodDesc* TailCallHelp::CreateStoreArgsStub(MethodDesc* pCallerMD,
                                              MethodDesc* pCalleeMD,
                                              MetaSig& callSiteSig)
{
    STANDARD_VM_CONTRACT;

    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(pCalleeMD, callSiteSig, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sigBuilder.GetSignature(&cbSig);
    SigTypeContext emptyCtx;

    MetaSig msig(pSig, cbSig, pCallerMD->GetModule(), &emptyCtx);

    ILStubLinker sl(pCallerMD->GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);

    DWORD bufferLcl = pCode->NewLocal(ELEMENT_TYPE_I);

    UINT argsSize = 0;
    bool requiresGCDescriptor = false;
    ProcessArgsAligned(
        msig,
        [&](TypeHandle tyHnd, UINT offset, UINT size)
        {
            argsSize = offset + size;

            if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
                return;

            // Not a value type here means it is either a by-ref or a class type.
            if (!tyHnd.IsValueType() || tyHnd.GetMethodTable()->ContainsPointers())
                requiresGCDescriptor = true;
        });

    _ASSERTE(!requiresGCDescriptor); // TODO

    UINT bufferSize = TARGET_POINTER_SIZE; // call stub
    if (requiresGCDescriptor)
        bufferSize += TARGET_POINTER_SIZE; // GC descriptor
    bufferSize += argsSize; // target address and user args

    pCode->EmitLDC(bufferSize);
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__ALLOC_TAILCALL_ARG_BUFFER, 1, 1);
    pCode->EmitSTLOC(bufferLcl);

    auto emitOffs = [&](UINT offs)
    {
        pCode->EmitLDLOC(bufferLcl);
        if (offs != 0)
        {
            pCode->EmitLDC(offs);
            pCode->EmitADD();
        }
    };

    UINT offs = 0;
    // Call stub
    emitOffs(offs); offs += TARGET_POINTER_SIZE;
    pCode->EmitLDC(0x1234); // TODO: should be LDFTN
    pCode->EmitCONV_I();
    pCode->EmitSTIND_I();

    if (requiresGCDescriptor)
    {
        // GC descriptor
        emitOffs(offs); offs += TARGET_POINTER_SIZE;
        pCode->EmitLDC(0x1234); // TODO
        pCode->EmitCONV_I();
        pCode->EmitSTIND_I();
    }

    unsigned argIndex = 0;
    ProcessArgsAligned(
        callSiteSig,
        [&](TypeHandle tyHnd, UINT argOffs, UINT size)
        {
            emitOffs(offs + argOffs);
            pCode->EmitLDARG(argIndex);
            pCode->EmitSTOBJ(pCode->GetToken(tyHnd));

            argIndex++;
        });

    pCode->EmitRET();

    Module* pLoaderModule = pCallerMD->GetLoaderModule();
    MethodDesc* pStoreArgsMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            pCallerMD->GetLoaderAllocator(),
            pLoaderModule->GetILStubCache()->GetOrCreateStubMethodTable(pLoaderModule),
            ILSTUB_TAILCALL_STOREARGS,
            pCallerMD->GetModule(),
            pSig, cbSig,
            &emptyCtx,
            &sl);

    return pStoreArgsMD;
}

char* g_argBuffer;
void* TailCallHelp::AllocTailCallArgBuffer(INT32 size)
{
    // TODO: TLS
    return (g_argBuffer = new char[size]);
}

void* TailCallHelp::GetTailCallArgBuffer()
{
    return g_argBuffer;
}

void TailCallHelp::FreeTailCallArgBuffer()
{
    delete[] g_argBuffer;
}