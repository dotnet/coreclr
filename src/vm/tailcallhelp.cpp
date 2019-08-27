// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "tailcallhelp.h"
#include "dllimport.h"
#include "formattype.h"
#include "sigformat.h"
#include "gcrefmap.h"

// Note: Layout of this must match similarly named structures in jithelpers.cs
struct NewTailCallFrame
{
    NewTailCallFrame* Prev;
    void* StackPointer;
    void* NextCall;
};

struct TailCallTls
{
    NewTailCallFrame* Frame;
    char* ArgBuffer;
    void* ArgBufferGCDesc;
};

// TODO: TLS
static NewTailCallFrame g_sentinelTailCallFrame;
static TailCallTls g_tailCallTls = { &g_sentinelTailCallFrame, NULL, NULL };

FCIMPL2(void*, TailCallHelp::AllocTailCallArgBuffer, INT32 size, void* gcDesc)
{
    FCALL_CONTRACT;
    g_tailCallTls.ArgBuffer = new char[size];
    if (gcDesc)
    {
        memset(g_tailCallTls.ArgBuffer, 0, size);
        g_tailCallTls.ArgBufferGCDesc = gcDesc;
    }

    return g_tailCallTls.ArgBuffer;
}
FCIMPLEND

FCIMPL0(void, TailCallHelp::FreeTailCallArgBuffer)
{
    FCALL_CONTRACT;
    g_tailCallTls.ArgBufferGCDesc = NULL;
    delete[] g_tailCallTls.ArgBuffer;
    g_tailCallTls.ArgBuffer = NULL;
}
FCIMPLEND

FCIMPL0(void*, TailCallHelp::GetTailCallTls)
{
    FCALL_CONTRACT;
    return &g_tailCallTls;
}
FCIMPLEND

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
    unsigned int TargetAddressOffset;
    bool HasThisPointer;
    unsigned int ThisPointerOffset;
    InlineSArray<ArgBufferOrigArg, 8> OrigArgs;
    bool HasGCDescriptor;
    GCRefMapBuilder GCRefMapBuilder;
    unsigned int Size;

    ArgBufferLayout()
        : TargetAddressOffset(0)
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

void TailCallHelp::CreateTailCallHelperStubs(
    MethodDesc* pCallerMD, MethodDesc* pCalleeMD,
    MetaSig& callSiteSig,
    MethodDesc** storeArgsStub, MethodDesc** callTargetStub)
{
    STANDARD_VM_CONTRACT;

#ifdef _DEBUG
    SigFormat incSig(callSiteSig, NULL);
    printf("Incoming sig: %s\n", incSig.GetCString());
#endif

    TypeHandle retTyHnd = NormalizeSigType(callSiteSig.GetRetTypeHandleThrowing());
    TailCallInfo info(pCallerMD, pCalleeMD, &callSiteSig, retTyHnd);
    LayOutArgBuffer(callSiteSig, &info.ArgBufLayout);

    *storeArgsStub = CreateStoreArgsStub(info);
    *callTargetStub = CreateCallTargetStub(info);
}

void TailCallHelp::LayOutArgBuffer(MetaSig& callSiteSig, ArgBufferLayout* layout)
{
    unsigned int offs = 0;

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
    layout->Size = offs;
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

MethodDesc* TailCallHelp::CreateStoreArgsStub(const TailCallInfo& info)
{
    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(info, &sigBuilder);

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

    DWORD bufferLcl = pCode->NewLocal(ELEMENT_TYPE_I);

    pCode->EmitLDC(info.ArgBufLayout.Size);
    pCode->EmitLDC(0);
    pCode->EmitCONV_I();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__ALLOC_TAILCALL_ARG_BUFFER, 2, 1);
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

    Module* pLoaderModule = info.Caller->GetLoaderModule();
    MethodDesc* pStoreArgsMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            info.Caller->GetLoaderAllocator(),
            pLoaderModule->GetILStubCache()->GetOrCreateStubMethodTable(pLoaderModule),
            ILSTUB_TAILCALL_STOREARGS,
            info.Caller->GetModule(),
            pSig, cbSig,
            &emptyCtx,
            &sl);

#ifdef _DEBUG
    StackSString ilStub;
    sl.LogILStub(CORJIT_FLAGS(), &ilStub);
    StackScratchBuffer ssb;
    printf("%s\n", ilStub.GetUTF8(ssb));
#endif

    return pStoreArgsMD;
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

    DWORD resultLcl = pCode->NewLocal(LocalDesc(info.RetTyHnd));
    DWORD tlsLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    DWORD prevFrameLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    ILCodeLabel* noUnwindLbl = pCode->NewCodeLabel();
    TypeHandle frameTyHnd = MscorlibBinder::GetClass(CLASS__TAIL_CALL_FRAME);
    DWORD newFrameEntryLcl = pCode->NewLocal(LocalDesc(frameTyHnd));
    DWORD argsLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    DWORD targetAddrLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    ILCodeLabel* unchained = pCode->NewCodeLabel();

    // tls = RuntimeHelpers.GetTailcallTls();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__GET_TAILCALL_TLS, 0, 1);
    pCode->EmitSTLOC(tlsLcl);

    // prevFrame = tls.Frame;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_TLS__FRAME);
    pCode->EmitSTLOC(prevFrameLcl);

    // TODO: Check previous frame. Intrinsic?
    // if (1 != 1) goto noUnwindLbl;
    pCode->EmitLDC(1);
    pCode->EmitLDC(1);
    pCode->EmitBNE_UN(noUnwindLbl);

    // prevFrame->NextCall = &ThisStub;
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitLDFTN(TOKEN_ILSTUB_METHODDEF);
    pCode->EmitCONV_I();
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);

    // return result;
    pCode->EmitLDLOC(resultLcl);
    pCode->EmitRET();

    // Do actual call
    pCode->EmitLabel(noUnwindLbl);
    
    // newFrameEntry.Prev = prevFrame;
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__PREV);

    // newFrameEntry.StackPointer = GetStackPointer();
    // TODO
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDC(0);
    pCode->EmitCONV_I();
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__STACK_POINTER);

    // newFrameEntry.NextCall = 0
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDC(0);
    pCode->EmitCONV_I();
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);

    // tls->Frame = &newFrameEntry;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_TLS__FRAME);

    // args = tls->ArgBuffer
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_TLS__ARG_BUFFER);
    pCode->EmitSTLOC(argsLcl);

    auto emitOffs = [&](UINT offs)
    {
        pCode->EmitLDLOC(argsLcl);
        if (offs != 0)
        {
            pCode->EmitLDC(offs);
            pCode->EmitADD();
        }
    };

    // targetAddr = args->TargetAddress
    emitOffs(info.ArgBufLayout.TargetAddressOffset);
    pCode->EmitLDIND_I();
    pCode->EmitSTLOC(targetAddrLcl);

    SigBuilder calliSig;

    StackSArray<DWORD> argLocals;
    if (info.ArgBufLayout.HasThisPointer)
    {
        DWORD argLcl = pCode->NewLocal(ELEMENT_TYPE_OBJECT);
        argLocals.Append(argLcl);

        // arg = args->ThisPointer
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

        // arg = args->Arg_i
        emitOffs(arg.Offset);
        pCode->EmitLDOBJ(pCode->GetToken(arg.TyHnd));
        pCode->EmitSTLOC(argLcl);

        calliSig.AppendElementType(ELEMENT_TYPE_INTERNAL);
        calliSig.AppendPointer(arg.TyHnd.AsPtr());
    }

    // RuntimeHelpers.FreeTailCallArgBuf();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__FREE_TAILCALL_ARG_BUFFER, 0, 0);

    // -----
    // result = Calli(target, args)
    for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
    {
        pCode->EmitLDLOC(argLocals[i]);
    }

    pCode->EmitLDLOC(targetAddrLcl);

    DWORD cbCalliSig;
    PCCOR_SIGNATURE pCalliSig = (PCCOR_SIGNATURE)calliSig.GetSignature(&cbCalliSig);

    pCode->EmitCALLI(pCode->GetSigToken(pCalliSig, cbCalliSig), argLocals.GetCount(), 1);
    pCode->EmitSTLOC(resultLcl);
    // -----

    // tls->Frame = newFrameEntry.Prev
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDLOC(newFrameEntryLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__PREV);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_TLS__FRAME);

    // if (newFrameEntry.NextCall == IntPtr.Zero) return result
    pCode->EmitLDLOC(newFrameEntryLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);
    pCode->EmitBRFALSE(unchained);

    // return newFrameEntry.NextCall();
    pCode->EmitLDLOC(newFrameEntryLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);
    pCode->EmitTAIL();
    pCode->EmitCALLI(pCode->GetSigToken(pSig, cbSig), 0, 1);
    pCode->EmitRET();

    // unchained: return result;
    pCode->EmitLabel(unchained);
    pCode->EmitLDLOC(resultLcl);
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

#ifdef _DEBUG
    StackSString ilStub;
    sl.LogILStub(CORJIT_FLAGS(), &ilStub);
    StackScratchBuffer ssb;
    printf("%s\n", ilStub.GetUTF8(ssb));
#endif

    return pStoreArgsMD;
}