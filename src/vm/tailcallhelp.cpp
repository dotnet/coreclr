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
    void* ReturnAddress;
    void* NextCall;
};

struct TailCallTls
{
    NewTailCallFrame* Frame;
    char* ArgBuffer;
    void* ArgBufferGCDesc;
};

static NewTailCallFrame g_sentinelTailCallFrame;
// TODO: This should maybe be on Thread instead.
static __thread TailCallTls g_tailCallTls = { &g_sentinelTailCallFrame, NULL, NULL };
static INT32 g_argBufferSize;

FCIMPL2(void*, TailCallHelp::AllocTailCallArgBuffer, INT32 size, void* gcDesc)
{
    FCALL_CONTRACT;

    TailCallTls* tls = &g_tailCallTls;

    if (size > g_argBufferSize)
    {
        delete[] tls->ArgBuffer;
        tls->ArgBuffer = new char[size];
        g_argBufferSize = size;
    }

    if (gcDesc)
    {
        memset(tls->ArgBuffer, 0, g_argBufferSize);
        tls->ArgBufferGCDesc = gcDesc;
    }

    return tls->ArgBuffer;
}
FCIMPLEND

FCIMPL0(void, TailCallHelp::FreeTailCallArgBuffer)
{
    FCALL_CONTRACT;
    g_tailCallTls.ArgBufferGCDesc = NULL;
}
FCIMPLEND

FCIMPL0(void*, TailCallHelp::GetTailCallTls)
{
    FCALL_CONTRACT;
    return &g_tailCallTls;
}
FCIMPLEND

struct ArgBufferValue
{
    TypeHandle TyHnd;
    unsigned int Offset;

    ArgBufferValue(TypeHandle tyHnd = TypeHandle(), unsigned int offset = 0)
        : TyHnd(tyHnd)
        , Offset(offset)
    {
    }
};

struct ArgBufferLayout
{
    bool HasTargetAddress;
    unsigned int TargetAddressOffset;
    InlineSArray<ArgBufferValue, 8> Values;
    bool HasGCDescriptor;
    GCRefMapBuilder GCRefMapBuilder;
    unsigned int Size;

    ArgBufferLayout()
        : HasTargetAddress(false)
        , TargetAddressOffset(0)
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
    bool CallSiteIsVirtual;
    TypeHandle RetTyHnd;
    ArgBufferLayout ArgBufLayout;

    TailCallInfo(
        MethodDesc* pCallerMD, MethodDesc* pCalleeMD,
        MetaSig* callSiteSig, bool callSiteIsVirtual,
        TypeHandle retTyHnd)
        : Caller(pCallerMD)
        , Callee(pCalleeMD)
        , CallSiteSig(callSiteSig)
        , CallSiteIsVirtual(callSiteIsVirtual)
        , RetTyHnd(retTyHnd)
    {
    }
};

void TailCallHelp::CreateTailCallHelperStubs(
    MethodDesc* pCallerMD, MethodDesc* pCalleeMD,
    MetaSig& callSiteSig, bool virt,
    MethodDesc** storeArgsStub, bool* storeArgsNeedsTarget,
    MethodDesc** callTargetStub)
{
    STANDARD_VM_CONTRACT;

#ifdef _DEBUG
    SigFormat incSig(callSiteSig, NULL);
    printf("Incoming sig: %s\n", incSig.GetCString());
#endif

    // If we don't know a target MD, i.e. if this is a calli, then we need to be
    // passed the target address from the tail call site.
    *storeArgsNeedsTarget = pCalleeMD == NULL;

    TypeHandle retTyHnd = NormalizeSigType(callSiteSig.GetRetTypeHandleThrowing());
    TailCallInfo info(pCallerMD, pCalleeMD, &callSiteSig, virt, retTyHnd);
    LayOutArgBuffer(callSiteSig, *storeArgsNeedsTarget, &info.ArgBufLayout);

    *storeArgsStub = CreateStoreArgsStub(info);
    *callTargetStub = CreateCallTargetStub(info);
}

void TailCallHelp::LayOutArgBuffer(MetaSig& callSiteSig, bool storeTarget, ArgBufferLayout* layout)
{
    unsigned int offs = 0;

    if (storeTarget)
    {
        layout->TargetAddressOffset = offs;
        layout->HasTargetAddress = true;
        offs += TARGET_POINTER_SIZE;
    }

    _ASSERTE(!callSiteSig.IsGenericMethod()); // TODO

    // User args
    if (callSiteSig.HasThis())
    {
        TypeHandle objHnd = TypeHandle(g_pObjectClass);
        layout->Values.Append(ArgBufferValue(objHnd, offs));
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

        layout->Values.Append(ArgBufferValue(tyHnd, offs));

        offs += tyHnd.GetSize();
    }

    layout->HasGCDescriptor = GenerateGCDescriptor(layout->Values, &layout->GCRefMapBuilder);
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

bool TailCallHelp::GenerateGCDescriptor(const SArray<ArgBufferValue>& values, GCRefMapBuilder* builder)
{
    bool anyGC = false;
    for (COUNT_T i = 0; i < values.GetCount(); i++)
    {
        TypeHandle tyHnd = values[i].TyHnd;
        if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
            continue;

        if (tyHnd.IsValueType() && !tyHnd.GetMethodTable()->ContainsPointers())
            continue;

        anyGC = true;
    }

    return anyGC;
}

MethodDesc* TailCallHelp::CreateStoreArgsStub(TailCallInfo& info)
{
    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(info, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = AllocateSignature(
        info.Caller->GetLoaderAllocator(), sigBuilder, &cbSig);

    SigTypeContext emptyCtx;

    ILStubLinker sl(info.Caller->GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);

    DWORD bufferLcl = pCode->NewLocal(ELEMENT_TYPE_I);

    void* pGcDesc = NULL;
    if (info.ArgBufLayout.HasGCDescriptor)
    {
        DWORD gcDescLen;
        PVOID gcDesc = info.ArgBufLayout.GCRefMapBuilder.GetBlob(&gcDescLen);
        pGcDesc = AllocateBlob(info.Caller->GetLoaderAllocator(), gcDesc, gcDescLen);
    }

    pCode->EmitLDC(info.ArgBufLayout.Size);
    pCode->EmitLDC(DWORD_PTR(pGcDesc));
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
    if (info.ArgBufLayout.HasTargetAddress)
    {
        emitOffs(info.ArgBufLayout.TargetAddressOffset);
        pCode->EmitLDARG(argIndex++);
        pCode->EmitSTIND_I();
    }

    for (COUNT_T i = 0; i < info.ArgBufLayout.Values.GetCount(); i++)
    {
        const ArgBufferValue& arg = info.ArgBufLayout.Values[i];
        emitOffs(arg.Offset);
        pCode->EmitLDARG(argIndex++);
        // TODO: We should be able to avoid write barriers as we are always
        // writing into TLS which needs special GC treatment anyway. However JIT
        // currently asserts if we use CPBLK or similar.
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

#ifndef CROSSGEN_COMPILE
    JitILStub(pStoreArgsMD);
#endif

    return pStoreArgsMD;
}

void TailCallHelp::CreateStoreArgsStubSig(
    const TailCallInfo& info, SigBuilder* sig)
{
    // The store-args stub will be different depending on the tailcall site.
    // Specifically the following things might be conditionally inserted:
    // * Call target address (for calli or generic calls resolved at tailcall site)
    // * This pointer (for instance calls)
    // * Generic context (for generic calls requiring context)

    // See MethodDefSig in ECMA-335 for the format.
    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    ULONG paramCount = info.ArgBufLayout.Values.GetCount();
    if (info.ArgBufLayout.HasTargetAddress)
    {
        paramCount++;
    }
    
    sig->AppendData(paramCount);

    sig->AppendElementType(ELEMENT_TYPE_VOID);

    if (info.ArgBufLayout.HasTargetAddress)
    {
        sig->AppendElementType(ELEMENT_TYPE_I);
    }

    for (COUNT_T i = 0; i < info.ArgBufLayout.Values.GetCount(); i++)
    {
        sig->AppendElementType(ELEMENT_TYPE_INTERNAL);
        sig->AppendPointer(info.ArgBufLayout.Values[i].TyHnd.AsPtr());
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
    PCCOR_SIGNATURE pSig = AllocateSignature(info.Caller->GetLoaderAllocator(), sigBuilder, &cbSig);

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
    ILCodeLabel* unchained = pCode->NewCodeLabel();

    // tls = RuntimeHelpers.GetTailcallTls();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__GET_TAILCALL_TLS, 0, 1);
    pCode->EmitSTLOC(tlsLcl);

    // prevFrame = tls.Frame;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_TLS__FRAME);
    pCode->EmitSTLOC(prevFrameLcl);

    // if (prevFrame.ReturnAddress != ReturnAddress()) goto noUnwindLbl;
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__RETURN_ADDRESS);
    pCode->EmitCALL(METHOD__STUBHELPERS__RETURN_ADDRESS, 0, 1);
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

    StackSArray<DWORD> argLocals;
    for (COUNT_T i = 0; i < info.ArgBufLayout.Values.GetCount(); i++)
    {
        const ArgBufferValue& arg = info.ArgBufLayout.Values[i];
        DWORD argLcl = pCode->NewLocal(LocalDesc(arg.TyHnd));
        argLocals.Append(argLcl);

        // arg = args->Arg_i
        emitOffs(arg.Offset);
        pCode->EmitLDOBJ(pCode->GetToken(arg.TyHnd));
        pCode->EmitSTLOC(argLcl);
    }

    // RuntimeHelpers.FreeTailCallArgBuf();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__FREE_TAILCALL_ARG_BUFFER, 0, 0);

    // newFrameEntry.ReturnAddress = NextCallReturnAddress();
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitCALL(METHOD__STUBHELPERS__NEXT_CALL_RETURN_ADDRESS, 0, 1);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__RETURN_ADDRESS);

    // Normally there will not be any target and we just emit a normal
    // call/callvirt.
    if (!info.ArgBufLayout.HasTargetAddress)
    {
        _ASSERTE(info.Callee != NULL);
        // TODO: enable for varargs. We need to fix the TokenLookupMap to build
        // the proper MethodRef.
        _ASSERTE(!info.CallSiteSig->IsVarArg());

        for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
        {
            pCode->EmitLDLOC(argLocals[i]);
        }

        if (info.CallSiteIsVirtual)
        {
            pCode->EmitCALLVIRT(
                pCode->GetToken(info.Callee),
                static_cast<int>(argLocals.GetCount()),
                1);
        }
        else
        {
            pCode->EmitCALL(
                pCode->GetToken(info.Callee),
                static_cast<int>(argLocals.GetCount()),
                1);
        }
    }
    else
    {
        // Otherwise we need to emit a calli. If the original tailcall site was
        // a calli then the target cannot be generic as we do not support
        // generics in standalone method signatures.
        _ASSERTE(!info.CallSiteSig->IsGenericMethod());

        SigBuilder calliSig;

        if (info.CallSiteSig->HasThis())
        {
            _ASSERTE(info.ArgBufLayout.Values.GetCount() > 0);

            calliSig.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT_HASTHIS);
            calliSig.AppendData(info.ArgBufLayout.Values.GetCount() - 1);
        }
        else
        {
            calliSig.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);
            calliSig.AppendData(info.ArgBufLayout.Values.GetCount());
        }

        // Return type
        calliSig.AppendElementType(ELEMENT_TYPE_INTERNAL);
        calliSig.AppendPointer(info.RetTyHnd.AsPtr());

        for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
        {
            calliSig.AppendElementType(ELEMENT_TYPE_INTERNAL);
            calliSig.AppendPointer(info.ArgBufLayout.Values[i].TyHnd.AsPtr());
        }

        DWORD cbCalliSig;
        PCCOR_SIGNATURE pCalliSig = (PCCOR_SIGNATURE)calliSig.GetSignature(&cbCalliSig);

        for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
        {
            pCode->EmitLDLOC(argLocals[i]);
        }

        emitOffs(info.ArgBufLayout.TargetAddressOffset);
        pCode->EmitLDIND_I();

        pCode->EmitCALLI(
            pCode->GetSigToken(pCalliSig, cbCalliSig),
            static_cast<int>(argLocals.GetCount()), 1);
    }

    pCode->EmitSTLOC(resultLcl);

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
    MethodDesc* pCallTargetMD =
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

#ifndef CROSSGEN_COMPILE
    JitILStub(pCallTargetMD);
#endif

    return pCallTargetMD;
}

PCCOR_SIGNATURE TailCallHelp::AllocateSignature(LoaderAllocator* alloc, SigBuilder& sig, DWORD* sigLen)
{
    PCCOR_SIGNATURE pBuilderSig = (PCCOR_SIGNATURE)sig.GetSignature(sigLen);
    return (PCCOR_SIGNATURE)AllocateBlob(alloc, pBuilderSig, *sigLen);
}

void* TailCallHelp::AllocateBlob(LoaderAllocator* alloc, const void* blob, size_t blobLen)
{
    AllocMemTracker pamTracker;
    PVOID newBlob = pamTracker.Track(alloc->GetHighFrequencyHeap()->AllocMem(S_SIZE_T(blobLen)));
    memcpy(newBlob, blob, blobLen);

    pamTracker.SuppressRelease();
    return newBlob;
}