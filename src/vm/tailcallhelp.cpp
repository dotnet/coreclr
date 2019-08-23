// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "tailcallhelp.h"
#include "dllimport.h"
#include "formattype.h"
#include "sigformat.h"
#include "gcrefmap.h"
#include "intrin.h" // TODO: we use this for _AddressOfReturnAddress

struct ArgBufferOrigArg
{
    TypeHandle TyHnd;
    SigPointer SigProps;
    unsigned int Offset;

    ArgBufferOrigArg(
        TypeHandle tyHnd = TypeHandle(),
        SigPointer sigProps = SigPointer(),
        unsigned int offset = 0)
        : TyHnd(tyHnd)
        , SigProps(sigProps)
        , Offset(offset)
    {
    }
};

struct ArgBufferLayout
{
    unsigned int CallStubOffset;
    unsigned int TargetAddressOffset;
    bool HasThisPointer;
    unsigned int ThisPointerOffset;
    InlineSArray<ArgBufferOrigArg, 8> OrigArgs;
    bool HasGCDescriptor;
    GCRefMapBuilder GCRefMapBuilder;
    unsigned int Size;

    ArgBufferLayout()
        : CallStubOffset(0)
        , TargetAddressOffset(0)
        , HasThisPointer(false)
        , ThisPointerOffset(0)
        , HasGCDescriptor(false)
        , Size(0)
    {
    }
};

struct TailCallInfo
{
    MethodDesc* Caller;
    MethodDesc* Callee;
    MetaSig* CallSiteSig;
    TypeHandle RetTyHnd;
    ArgBufferLayout ArgBufLayout;

    TailCallInfo(MethodDesc* pCallerMD, MethodDesc* pCalleeMD, MetaSig* callSiteSig, TypeHandle retTyHnd)
        : Caller(pCallerMD)
        , Callee(pCalleeMD)
        , CallSiteSig(callSiteSig)
        , RetTyHnd(retTyHnd)
    {
    }
};

bool TailCallHelp::GenerateGCDescriptor(const SArray<ArgBufferOrigArg>& args, GCRefMapBuilder* builder)
{
    bool anyGC = false;
    for (COUNT_T i = 0; i < args.GetCount(); i++)
    {
        TypeHandle tyHnd = args[i].TyHnd;
        if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
            continue;

        if (tyHnd.IsValueType() && !tyHnd.GetMethodTable()->ContainsPointers())
            continue;

        anyGC = true;
        _ASSERTE(!"Requires GC descriptor");
    }

    return anyGC;
}

TypeHandle TailCallHelp::NormalizeSigType(TypeHandle tyHnd)
{
    if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_I));
    }

    if (tyHnd.IsByRef())
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_U1))
               .MakeByRef();
    }
    if (!tyHnd.IsValueType())
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_OBJECT));
    }

    // Value type -- retain it to preserve its size
    return tyHnd;
}

void TailCallHelp::LayOutArgBuffer(MetaSig& callSiteSig, ArgBufferLayout* layout)
{
    unsigned int offs = 0;
    layout->CallStubOffset = offs;
    offs += TARGET_POINTER_SIZE;

    layout->TargetAddressOffset = offs;
    offs += TARGET_POINTER_SIZE;

    _ASSERTE(!callSiteSig.IsGenericMethod()); // TODO

    // User args
    if (callSiteSig.HasThis())
    {
        layout->HasThisPointer = true;
        layout->ThisPointerOffset = offs;
        offs += TARGET_POINTER_SIZE;
    }

    callSiteSig.Reset();
    CorElementType ty;
    while ((ty = callSiteSig.NextArg()) != ELEMENT_TYPE_END)
    {
        TypeHandle tyHnd = callSiteSig.GetLastTypeHandleThrowing();
        tyHnd = NormalizeSigType(tyHnd);
        unsigned int alignment = CEEInfo::getClassAlignmentRequirementStatic(tyHnd);
        offs = AlignUp(offs, alignment);

        layout->OrigArgs.Append(ArgBufferOrigArg(tyHnd, callSiteSig.GetArgProps(), offs));

        offs += tyHnd.GetSize();
    }

    layout->HasGCDescriptor = GenerateGCDescriptor(layout->OrigArgs, &layout->GCRefMapBuilder);
    // Should GC descriptor go in arg buffer (in which case it should probably
    // be at the beginning) or be passed as an arg to AllocTailCallArgBuffer?

    layout->Size = offs;
}

void TailCallHelp::CreateStoreArgsStubSig(const TailCallInfo& info, SigBuilder* sig)
{
    // The store-args stub will be different from the target signature.
    // Specifically we insert the following things at the beginning of the
    // signature:
    // * Call target address
    // * This pointer
    // * Generic context

    // See MethodDefSig in ECMA-335 for the format.
    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    ULONG paramCount = 1; // Target address
    paramCount += info.ArgBufLayout.OrigArgs.GetCount(); // user args
    if (info.ArgBufLayout.HasThisPointer)
        paramCount++;
    
    sig->AppendData(paramCount);

    // Returns void always
    sig->AppendElementType(ELEMENT_TYPE_VOID);

    // First arg is the target pointer
    sig->AppendElementType(ELEMENT_TYPE_I);

    if (info.ArgBufLayout.HasThisPointer)
    {
        // TODO: Handle value type (this pointer should be a by-ref)
        _ASSERTE(info.Callee == nullptr || !info.Callee->GetCanonicalMethodTable()->IsValueType());
        sig->AppendElementType(ELEMENT_TYPE_OBJECT);
    }

    for (COUNT_T i = 0; i < info.ArgBufLayout.OrigArgs.GetCount(); i++)
    {
        sig->AppendElementType(ELEMENT_TYPE_INTERNAL);
        sig->AppendPointer(info.ArgBufLayout.OrigArgs[i].TyHnd.AsPtr());
    }

#ifdef _DEBUG
    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sig->GetSignature(&cbSig);
    SigTypeContext emptyContext;
    MetaSig outMsig(pSig, cbSig, info.CallSiteSig->GetModule(), &emptyContext);
    SigFormat outSig(outMsig, NULL);
    printf("StoreArgs sig: %s\n", outSig.GetCString());
#endif // _DEBUG
}

MethodDesc* TailCallHelp::CreateStoreArgsStub(MethodDesc* pCallerMD,
                                              MethodDesc* pCalleeMD,
                                              MetaSig& callSiteSig)
{
    STANDARD_VM_CONTRACT;

#ifdef _DEBUG
    SigFormat incSig(callSiteSig, NULL);
    printf("Incoming sig: %s\n", incSig.GetCString());
#endif

    TypeHandle retTyHnd = NormalizeSigType(callSiteSig.GetRetTypeHandleThrowing());
    TailCallInfo info(pCallerMD, pCalleeMD, &callSiteSig, retTyHnd);
    LayOutArgBuffer(callSiteSig, &info.ArgBufLayout);

    MethodDesc* callTargetStubMD = CreateCallTargetStub(info);

    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(info, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sigBuilder.GetSignature(&cbSig);

    SigTypeContext emptyCtx;

    ILStubLinker sl(pCallerMD->GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);

    DWORD bufferLcl = pCode->NewLocal(ELEMENT_TYPE_I);

    pCode->EmitLDC(info.ArgBufLayout.Size);
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

    // Tail call helper requires the call stub offset to be 0.
    _ASSERTE(info.ArgBufLayout.CallStubOffset == 0);
    // Call stub
    emitOffs(info.ArgBufLayout.CallStubOffset);
    pCode->EmitLDFTN(pCode->GetToken(callTargetStubMD));
    pCode->EmitCONV_I();
    pCode->EmitSTIND_I();

    _ASSERTE(!info.ArgBufLayout.HasGCDescriptor);

    unsigned int argIndex = 0;
    emitOffs(info.ArgBufLayout.TargetAddressOffset);
    pCode->EmitLDARG(argIndex++);
    pCode->EmitSTIND_I();

    if (info.ArgBufLayout.HasThisPointer)
    {
        emitOffs(info.ArgBufLayout.ThisPointerOffset);
        pCode->EmitLDARG(argIndex++);
        pCode->EmitSTIND_I();
    }

    for (COUNT_T i = 0; i < info.ArgBufLayout.OrigArgs.GetCount(); i++)
    {
        const ArgBufferOrigArg& arg = info.ArgBufLayout.OrigArgs[i];
        emitOffs(arg.Offset);
        pCode->EmitLDARG(argIndex++);
        pCode->EmitSTOBJ(pCode->GetToken(arg.TyHnd));
    }

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

void TailCallHelp::CreateCallTargetStubSig(const TailCallInfo& info, SigBuilder* sig)
{
    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    // No parameters
    sig->AppendData(0); 

    // Returns same as original
    sig->AppendElementType(ELEMENT_TYPE_INTERNAL);
    sig->AppendPointer(info.RetTyHnd.AsPtr());

#ifdef _DEBUG
    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sig->GetSignature(&cbSig);
    SigTypeContext emptyContext;
    MetaSig outMsig(pSig, cbSig, info.CallSiteSig->GetModule(), &emptyContext);
    SigFormat outSig(outMsig, NULL);
    printf("CallTarget sig: %s\n", outSig.GetCString());
#endif // _DEBUG
}

// Note: Layout of this must match with TailCallFrame in jithelpers.cs
struct NewTailCallFrame
{
    NewTailCallFrame* Next;
    void* StackPointer;
    bool ChainedCall;
};

MethodDesc* TailCallHelp::CreateCallTargetStub(const TailCallInfo& info)
{
    SigBuilder sigBuilder;
    CreateCallTargetStubSig(info, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sigBuilder.GetSignature(&cbSig);

    SigTypeContext emptyCtx;

    ILStubLinker sl(info.Caller->GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);
    TypeHandle tailCallFrameTyHnd = MscorlibBinder::GetClass(CLASS__TAIL_CALL_FRAME);

    DWORD tcFrameLcl = pCode->NewLocal(LocalDesc(tailCallFrameTyHnd));
    DWORD tcArgBufferLcl = pCode->NewLocal(ELEMENT_TYPE_I);

    pCode->EmitLDLOCA(tcFrameLcl);
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__PUSH_NEW_TAILCALL_FRAME, 1, 0);

    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__GET_TAILCALL_ARG_BUFFER, 0, 1);
    pCode->EmitSTLOC(tcArgBufferLcl);

    auto emitOffs = [&](UINT offs)
    {
        pCode->EmitLDLOC(tcArgBufferLcl);
        pCode->EmitLDC(info.ArgBufLayout.TargetAddressOffset);
        pCode->EmitADD();
    };

    DWORD targetAddrLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    emitOffs(info.ArgBufLayout.TargetAddressOffset);
    pCode->EmitLDIND_I();
    pCode->EmitSTLOC(targetAddrLcl);

    SigBuilder calliSig;

    StackSArray<DWORD> argLocals;
    if (info.ArgBufLayout.HasThisPointer)
    {
        DWORD argLcl = pCode->NewLocal(ELEMENT_TYPE_OBJECT);
        argLocals.Append(argLcl);
        emitOffs(info.ArgBufLayout.ThisPointerOffset);
        pCode->EmitLDIND_REF();
        pCode->EmitSTLOC(argLcl);

        calliSig.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT_HASTHIS);
    }
    else
        calliSig.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    // Num params
    calliSig.AppendData(info.ArgBufLayout.OrigArgs.GetCount());
    // Return type
    calliSig.AppendElementType(ELEMENT_TYPE_INTERNAL);
    calliSig.AppendPointer(info.RetTyHnd.AsPtr());

    for (COUNT_T i = 0; i < info.ArgBufLayout.OrigArgs.GetCount(); i++)
    {
        const ArgBufferOrigArg& arg = info.ArgBufLayout.OrigArgs[i];
        DWORD argLcl = pCode->NewLocal(LocalDesc(arg.TyHnd));
        argLocals.Append(argLcl);
        emitOffs(arg.Offset);
        pCode->EmitLDOBJ(pCode->GetToken(arg.TyHnd));
        pCode->EmitSTLOC(argLcl);

        calliSig.AppendElementType(ELEMENT_TYPE_INTERNAL);
        calliSig.AppendPointer(arg.TyHnd.AsPtr());
    }

    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__FREE_TAILCALL_ARG_BUFFER, 0, 0);

    // TODO: try-finally

    for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
    {
        pCode->EmitLDLOC(argLocals[i]);
    }

    emitOffs(info.ArgBufLayout.TargetAddressOffset);
    pCode->EmitLDIND_I();

    DWORD retValueLcl = pCode->NewLocal(LocalDesc(info.RetTyHnd));
    // TODO: Handle void return
    pCode->EmitCALLI(TOKEN_ILSTUB_TARGET_SIG, argLocals.GetCount(), 1);
    pCode->EmitSTLOC(retValueLcl);

    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__POP_TAILCALL_FRAME, 0, 0);

    ILCodeLabel* unchained = pCode->NewCodeLabel();
    pCode->EmitLDLOC(tcFrameLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__CHAINED_CALL);
    pCode->EmitBRFALSE(unchained);

    pCode->EmitTAIL();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__TAILCALL_HELPER, 0, 1);
    pCode->EmitRET();

    pCode->EmitLabel(unchained);
    pCode->EmitLDLOC(retValueLcl); // TODO: How to handle bad gc refs in ret value unwind case?
    pCode->EmitRET();

    Module* pLoaderModule = info.Caller->GetLoaderModule();
    MethodDesc* pStoreArgsMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            info.Caller->GetLoaderAllocator(),
            pLoaderModule->GetILStubCache()->GetOrCreateStubMethodTable(pLoaderModule),
            ILSTUB_TAILCALL_CALLTARGET,
            info.Caller->GetModule(),
            pSig, cbSig,
            &emptyCtx,
            &sl);

    ILStubResolver* pResolver = pStoreArgsMD->AsDynamicMethodDesc()->GetILStubResolver();

    DWORD cbCalliSig;
    PCCOR_SIGNATURE pCalliSig = (PCCOR_SIGNATURE)calliSig.GetSignature(&cbCalliSig);
    pResolver->SetStubTargetMethodSig(pCalliSig, cbCalliSig);

    return pStoreArgsMD;
}

#ifndef CROSSGEN_COMPILE
// TODO: TLS
extern "C" char* g_tailCallArgBuffer = NULL;

FCIMPL1(void*, TailCallHelp::AllocTailCallArgBuffer, INT32 size)
{
    FCALL_CONTRACT;
    return (g_tailCallArgBuffer = new char[size]);
}
FCIMPLEND

FCIMPL0(void*, TailCallHelp::GetTailCallArgBuffer)
{
    FCALL_CONTRACT;
    return g_tailCallArgBuffer;
}
FCIMPLEND

FCIMPL0(void, TailCallHelp::FreeTailCallArgBuffer)
{
    FCALL_CONTRACT;
    delete[] g_tailCallArgBuffer;
}
FCIMPLEND

NewTailCallFrame g_sentinelTailCallFrame;
// TODO: TLS
extern "C" NewTailCallFrame* g_curTailCallFrame = &g_sentinelTailCallFrame;

FCIMPL1(void, TailCallHelp::PushNewTailCallFrame, NewTailCallFrame* tcFrame)
{
    FCALL_CONTRACT;

    tcFrame->Next = g_curTailCallFrame;
    // TODO: This should probably use a helper in the emitted IL instead..
    tcFrame->StackPointer = (char*)__builtin_frame_address(0) + sizeof(void*);
    tcFrame->ChainedCall = false;
    g_curTailCallFrame = tcFrame;
}
FCIMPLEND

FCIMPL0(void, TailCallHelp::PopTailCallFrame)
{
    FCALL_CONTRACT;

    g_curTailCallFrame = g_curTailCallFrame->Next;
}
FCIMPLEND
#endif