// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "corpriv.h"
#include "tailcallhelp.h"
#include "dllimport.h"
#include "formattype.h"
#include "sigformat.h"
#include "gcrefmap.h"
#include "threads.h"

#ifndef CROSSGEN_COMPILE

FCIMPL2(void*, TailCallHelp::AllocTailCallArgBuffer, INT32 size, void* gcDesc)
{
    FCALL_CONTRACT;

    _ASSERTE(size >= 0);

    return GetThread()->AllocTailCallArgBuffer(static_cast<size_t>(size), gcDesc);
}
FCIMPLEND

FCIMPL0(void, TailCallHelp::FreeTailCallArgBuffer)
{
    FCALL_CONTRACT;
    GetThread()->FreeTailCallArgBuffer();
}
FCIMPLEND

FCIMPL2(void*, TailCallHelp::GetTailCallInfo, void** retAddrSlot, void** retAddr)
{
    FCALL_CONTRACT;

    Thread* thread = GetThread();

    *retAddr = thread->GetReturnAddress(retAddrSlot);
    return thread->GetTailCallTls();
}
FCIMPLEND

#endif

struct ArgBufferValue
{
    TypeHandle TyHnd;
    unsigned int Offset;

    ArgBufferValue(TypeHandle tyHnd = TypeHandle(), unsigned int offset = 0)
        : TyHnd(tyHnd), Offset(offset)
    {
    }
};

struct ArgBufferLayout
{
    bool HasTargetAddress;
    unsigned int TargetAddressOffset;
    InlineSArray<ArgBufferValue, 8> Values;
    int GenericContextIndex;
    unsigned int Size;

    ArgBufferLayout()
        : HasTargetAddress(false)
        , TargetAddressOffset(0)
        , GenericContextIndex(-1)
        , Size(0)
    {
    }
};

struct TailCallInfo
{
    MethodDesc* Caller;
    MethodDesc* Callee;
    LoaderAllocator* LoaderAlloc;
    MetaSig* CallSiteSig;
    bool CallSiteIsVirtual;
    TypeHandle RetTyHnd;
    ArgBufferLayout ArgBufLayout;
    bool HasGCDescriptor;
    GCRefMapBuilder GCRefMapBuilder;

    TailCallInfo(
        MethodDesc* pCallerMD, MethodDesc* pCalleeMD,
        LoaderAllocator* loaderAlloc,
        MetaSig* callSiteSig, bool callSiteIsVirtual,
        TypeHandle retTyHnd)
        : Caller(pCallerMD)
        , Callee(pCalleeMD)
        , LoaderAlloc(loaderAlloc)
        , CallSiteSig(callSiteSig)
        , CallSiteIsVirtual(callSiteIsVirtual)
        , RetTyHnd(retTyHnd)
        , HasGCDescriptor(false)
    {
    }
};

static MethodDesc* s_tailCallDispatcherMD;
MethodDesc* TailCallHelp::GetTailCallDispatcherMD()
{
    return s_tailCallDispatcherMD;
}


// This creates the dispatcher used to dispatch sequences of tailcalls. In C#
// code it is the following function. Once C# gets function pointer support this
// function can be put in System.Private.CoreLib.
// private static unsafe void DispatchTailCalls(
//     IntPtr callersRetAddrSlot, IntPtr callTarget, IntPtr retVal)
// {
//     IntPtr callersRetAddr;
//     TailCallTls* tls = GetTailCallInfo(callersRetAddrSlot, &callersRetAddr);
//     TailCallFrame* prevFrame = tls->Frame;
//     if (callersRetAddr == prevFrame->TailCallAwareReturnAddress)
//     {
//         prevFrame->NextCall = callTarget;
//         return;
//     }
//
//     TailCallFrame newFrame;
//     newFrame.Prev = prevFrame;
//
//     try
//     {
//         tls->Frame = &newFrame;
//
//         do
//         {
//             newFrame.NextCall = IntPtr.Zero;
//             var fptr = (func* void(IntPtr, IntPtr, void*))callTarget;
//             fptr(tls->ArgBuffer, retVal, &newFrame.TailCallAwareReturnAddress);
//             callTarget = newFrame.NextCall;
//         } while (callTarget != IntPtr.Zero);
//     }
//     finally
//     {
//         tls->Frame = prevFrame;
//     }
// }
MethodDesc* TailCallHelp::GetOrCreateTailCallDispatcherMD()
{
    if (s_tailCallDispatcherMD != NULL)
        return s_tailCallDispatcherMD;

    SigBuilder sigBuilder;
    sigBuilder.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    sigBuilder.AppendData(3);
    sigBuilder.AppendElementType(ELEMENT_TYPE_VOID);

    sigBuilder.AppendElementType(ELEMENT_TYPE_I);
    sigBuilder.AppendElementType(ELEMENT_TYPE_I);
    sigBuilder.AppendElementType(ELEMENT_TYPE_I);

    const int ARG_CALLERS_RET_ADDR_SLOT = 0;
    const int ARG_CALL_TARGET = 1;
    const int ARG_RET_VAL = 2;

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = AllocateSignature(
        MscorlibBinder::GetModule()->GetLoaderAllocator(), sigBuilder, &cbSig);

    SigTypeContext emptyCtx;

    ILStubLinker sl(MscorlibBinder::GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);

    DWORD retAddrLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    DWORD tlsLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    DWORD prevFrameLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    TypeHandle frameTyHnd = MscorlibBinder::GetClass(CLASS__TAIL_CALL_FRAME);
    DWORD newFrameEntryLcl = pCode->NewLocal(LocalDesc(frameTyHnd));
    DWORD argsLcl = pCode->NewLocal(ELEMENT_TYPE_I);
    ILCodeLabel* noUnwindLbl = pCode->NewCodeLabel();
    ILCodeLabel* loopStart = pCode->NewCodeLabel();
    ILCodeLabel* afterTryFinally = pCode->NewCodeLabel();

    // tls = RuntimeHelpers.GetTailcallInfo(callersRetAddrSlot, &retAddr);
    pCode->EmitLDARG(ARG_CALLERS_RET_ADDR_SLOT);
    pCode->EmitLDLOCA(retAddrLcl);
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__GET_TAILCALL_INFO, 2, 1);
    pCode->EmitSTLOC(tlsLcl);

    // prevFrame = tls.Frame;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_TLS__FRAME);
    pCode->EmitSTLOC(prevFrameLcl);

    // if (retAddr != prevFrame.TailCallAwareReturnAddress) goto noUnwindLbl;
    pCode->EmitLDLOC(retAddrLcl);
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__TAILCALL_AWARE_RETURN_ADDRESS);
    pCode->EmitBNE_UN(noUnwindLbl);

    // prevFrame->NextCall = callTarget;
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitLDARG(ARG_CALL_TARGET);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);

    // return;
    pCode->EmitRET();

    // Ok, we are the "first" dispatcher.
    pCode->EmitLabel(noUnwindLbl);

    // newFrameEntry.Prev = prevFrame;
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__PREV);

    // try {
    pCode->BeginTryBlock();

    // tls->Frame = &newFrameEntry;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_TLS__FRAME);

    // do {
    pCode->EmitLabel(loopStart);

    // newFrameEntry.NextCall = 0
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDC(0);
    pCode->EmitCONV_I();
    pCode->EmitSTFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);

    SigBuilder calliSig;
    calliSig.AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);
    calliSig.AppendData(3);
    calliSig.AppendElementType(ELEMENT_TYPE_VOID);
    calliSig.AppendElementType(ELEMENT_TYPE_I);
    calliSig.AppendElementType(ELEMENT_TYPE_I);
    calliSig.AppendElementType(ELEMENT_TYPE_I);

    DWORD cbCalliSig;
    PCCOR_SIGNATURE pCalliSig = (PCCOR_SIGNATURE)calliSig.GetSignature(&cbCalliSig);

    // callTarget(tls->ArgBuffer, retVal, &newFrameEntry.TailCallAwareReturnAddress)
    // arg buffer
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_TLS__ARG_BUFFER);

    // ret val
    pCode->EmitLDARG(ARG_RET_VAL);

    // TailCallAwareReturnAddress
    pCode->EmitLDLOCA(newFrameEntryLcl);
    pCode->EmitLDFLDA(FIELD__TAIL_CALL_FRAME__TAILCALL_AWARE_RETURN_ADDRESS);

    // callTarget
    pCode->EmitLDARG(ARG_CALL_TARGET);

    pCode->EmitCALLI(pCode->GetSigToken(pCalliSig, cbCalliSig), 2, 0);

    // callTarget = newFrameEntry.NextCall;
    pCode->EmitLDLOC(newFrameEntryLcl);
    pCode->EmitLDFLD(FIELD__TAIL_CALL_FRAME__NEXT_CALL);
    pCode->EmitSTARG(ARG_CALL_TARGET);

    // } while (callTarget != IntPtr.Zero);
    pCode->EmitLDARG(ARG_CALL_TARGET);
    pCode->EmitBRTRUE(loopStart);

    // }
    pCode->EmitLEAVE(afterTryFinally);
    pCode->EndTryBlock();

    // finally {
    pCode->BeginFinallyBlock();

    // tls->Frame = prevFrame;
    pCode->EmitLDLOC(tlsLcl);
    pCode->EmitLDLOC(prevFrameLcl);
    pCode->EmitSTFLD(FIELD__TAIL_CALL_TLS__FRAME);

    // }
    pCode->EmitENDFINALLY();
    pCode->EndFinallyBlock();

    // afterTryFinally:
    pCode->EmitLabel(afterTryFinally);

    // return;
    pCode->EmitRET();

    Module* mscorlib = MscorlibBinder::GetModule();
    MethodDesc* pDispatchTailCallsMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            MscorlibBinder::GetModule()->GetLoaderAllocator(),
            mscorlib->GetILStubCache()->GetOrCreateStubMethodTable(mscorlib),
            ILSTUB_TAILCALL_DISPATCH,
            mscorlib,
            pSig, cbSig,
            &emptyCtx,
            &sl);

#ifdef _DEBUG
    StackSString ilStub;
    sl.LogILStub(CORJIT_FLAGS(), &ilStub);
    StackScratchBuffer ssb;
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: DispatchTailCalls IL created\n"));
    LOG((LF_STUBS, LL_INFO1000, "%s\n\n", ilStub.GetUTF8(ssb)));
#endif

    // We might waste a MethodDesc here if we lose the race, but that is very
    // unlikely and since this initialization only happens once not a big deal.
    InterlockedCompareExchangeT(&s_tailCallDispatcherMD, pDispatchTailCallsMD, NULL);
    return s_tailCallDispatcherMD;
}

void TailCallHelp::CreateTailCallHelperStubs(
    MethodDesc* pCallerMD, MethodDesc* pCalleeMD,
    MetaSig& callSiteSig, bool virt,
    MethodDesc** storeArgsStub, bool* storeArgsNeedsTarget,
    MethodDesc** callTargetStub)
{
    STANDARD_VM_CONTRACT;

#ifdef _DEBUG
    SigFormat incSig(callSiteSig, NULL);
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: Incoming sig %s\n", incSig.GetCString()));
#endif

    // There are two cases where we will need the tailcalling site to pass us the target:
    // * It was a calli, for obvious reasons
    // * The target is generic (in which case it might depend on type parameters
    // in the caller). Since the CallTarget stub is non-generic we cannot
    // express a call to it in IL.
    *storeArgsNeedsTarget = pCalleeMD == NULL || callSiteSig.IsGenericMethod();

    // We 'attach' the tailcall stub to the target method except for the case of
    // calli (where the callee MD will be null). We do not reuse stubs for calli
    // tailcalls as those are presumably very rare and not worth the trouble.
    LoaderAllocator* loaderAlloc =
        pCalleeMD == NULL
        ? pCallerMD->GetLoaderAllocator()
        : pCalleeMD->GetLoaderAllocator();

    TypeHandle retTyHnd = NormalizeSigType(callSiteSig.GetRetTypeHandleThrowing());
    TailCallInfo info(pCallerMD, pCalleeMD, loaderAlloc, &callSiteSig, virt, retTyHnd);

    bool hasGenericContext = pCalleeMD != NULL && pCalleeMD->RequiresInstArg();
    LayOutArgBuffer(callSiteSig, pCalleeMD, *storeArgsNeedsTarget, hasGenericContext, &info.ArgBufLayout);
    info.HasGCDescriptor = GenerateGCDescriptor(pCalleeMD, info.ArgBufLayout, &info.GCRefMapBuilder);

    *storeArgsStub = CreateStoreArgsStub(info);
    *callTargetStub = CreateCallTargetStub(info);
}

void TailCallHelp::LayOutArgBuffer(
    MetaSig& callSiteSig, MethodDesc* calleeMD,
    bool storeTarget, bool hasGenericContext,
    ArgBufferLayout* layout)
{
    unsigned int offs = 0;

    auto addValue = [&](TypeHandle th)
    {
        unsigned int alignment = CEEInfo::getClassAlignmentRequirementStatic(th);
        offs = AlignUp(offs, alignment);
        layout->Values.Append(ArgBufferValue(th, offs));
        offs += th.GetSize();
    };

    // User args
    if (callSiteSig.HasThis())
    {
        TypeHandle thisHnd;
        if (calleeMD != NULL && calleeMD->GetMethodTable()->IsValueType())
        {
            thisHnd = TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_U1))
                      .MakeByRef();
        }
        else
        {
            thisHnd = TypeHandle(g_pObjectClass);
        }

        addValue(thisHnd);
    }

    auto addGenCtx = [&]()
    {
        if (!hasGenericContext)
            return;

        layout->GenericContextIndex = static_cast<int>(layout->Values.GetCount());
        addValue(TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_I)));
    };

    // Generic context comes before args on all platforms except X86.
#ifndef _TARGET_X86_
    addGenCtx();
#endif

    callSiteSig.Reset();
    CorElementType ty;
    while ((ty = callSiteSig.NextArg()) != ELEMENT_TYPE_END)
    {
        TypeHandle tyHnd = callSiteSig.GetLastTypeHandleThrowing();
        tyHnd = NormalizeSigType(tyHnd);
        addValue(tyHnd);
    }

#ifdef _TARGET_X86_
    addGenCtx();
#endif

    if (storeTarget)
    {
        offs = AlignUp(offs, TARGET_POINTER_SIZE);
        layout->TargetAddressOffset = offs;
        layout->HasTargetAddress = true;
        offs += TARGET_POINTER_SIZE;
    }

    layout->Size = offs;
}

TypeHandle TailCallHelp::NormalizeSigType(TypeHandle tyHnd)
{
    CorElementType ety = tyHnd.GetSignatureCorElementType();
    if (CorTypeInfo::IsPrimitiveType(ety))
    {
        return tyHnd;
    }
    if (CorTypeInfo::IsObjRef(ety))
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_OBJECT));
    }
    if (tyHnd.IsPointer() || tyHnd.IsFnPtrType())
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_I));
    }

    if (tyHnd.IsByRef())
    {
        return TypeHandle(MscorlibBinder::GetElementType(ELEMENT_TYPE_U1))
               .MakeByRef();
    }

    _ASSERTE(ety == ELEMENT_TYPE_VALUETYPE && tyHnd.IsValueType());
    // Value type -- retain it to preserve its size
    return tyHnd;
}

bool TailCallHelp::GenerateGCDescriptor(
    MethodDesc* pTargetMD, const ArgBufferLayout& layout, GCRefMapBuilder* builder)
{
    auto writeToken = [&](unsigned int offset, int token)
    {
        _ASSERTE(offset % TARGET_POINTER_SIZE == 0);
        builder->WriteToken(
            static_cast<int>(offset / TARGET_POINTER_SIZE),
            token);
    };

    auto writeGCType = [&](unsigned int offset, CorInfoGCType type)
    {
        switch (type)
        {
            case TYPE_GC_REF: writeToken(offset, GCREFMAP_REF); break;
            case TYPE_GC_BYREF: writeToken(offset, GCREFMAP_INTERIOR); break;
            case TYPE_GC_NONE: break;
            default: UNREACHABLE_MSG("Invalid type"); break;
        }
    };

    CQuickBytes gcPtrs;
    for (COUNT_T i = 0; i < layout.Values.GetCount(); i++)
    {
        const ArgBufferValue& val = layout.Values[i];

        if (static_cast<int>(i) == layout.GenericContextIndex)
        {
            if (pTargetMD->RequiresInstMethodDescArg())
            {
                writeToken(val.Offset, GCREFMAP_METHOD_PARAM);
            }
            else
            {
                _ASSERTE(pTargetMD->RequiresInstMethodTableArg());
                writeToken(val.Offset, GCREFMAP_TYPE_PARAM);
            }

            continue;
        }

        TypeHandle tyHnd = val.TyHnd;
        if (tyHnd.IsValueType())
        {
            if (!tyHnd.GetMethodTable()->ContainsPointers())
                continue;

            unsigned int numSlots = (tyHnd.GetSize() + TARGET_POINTER_SIZE - 1) / TARGET_POINTER_SIZE;
            BYTE* ptr = static_cast<BYTE*>(gcPtrs.AllocThrows(numSlots));
            CEEInfo::getClassGClayoutStatic(tyHnd, ptr);
            for (unsigned int i = 0; i < numSlots; i++)
            {
                writeGCType(val.Offset + i * TARGET_POINTER_SIZE, (CorInfoGCType)ptr[i]);
            }

            continue;
        }

        CorElementType ety = tyHnd.GetSignatureCorElementType();
        CorInfoGCType gc = CorTypeInfo::GetGCType(ety);

        writeGCType(val.Offset, gc);
    }

    builder->Flush();

    return builder->GetBlobLength() > 0;
}

MethodDesc* TailCallHelp::CreateStoreArgsStub(TailCallInfo& info)
{
    SigBuilder sigBuilder;
    CreateStoreArgsStubSig(info, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = AllocateSignature(
        info.LoaderAlloc, sigBuilder, &cbSig);

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
    if (info.HasGCDescriptor)
    {
        DWORD gcDescLen;
        PVOID gcDesc = info.GCRefMapBuilder.GetBlob(&gcDescLen);
        pGcDesc = AllocateBlob(info.LoaderAlloc, gcDesc, gcDescLen);
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

    for (COUNT_T i = 0; i < info.ArgBufLayout.Values.GetCount(); i++)
    {
        const ArgBufferValue& arg = info.ArgBufLayout.Values[i];
        CorElementType ty = arg.TyHnd.GetSignatureCorElementType();

        emitOffs(arg.Offset);
        pCode->EmitLDARG(argIndex++);
        EmitStoreTyHnd(pCode, arg.TyHnd);
    }

    if (info.ArgBufLayout.HasTargetAddress)
    {
        emitOffs(info.ArgBufLayout.TargetAddressOffset);
        pCode->EmitLDARG(argIndex++);
        pCode->EmitSTIND_I();
    }

    pCode->EmitRET();

    Module* pLoaderModule = info.Caller->GetLoaderModule();
    MethodDesc* pStoreArgsMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            info.LoaderAlloc,
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
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: StoreArgs IL created\n"));
    LOG((LF_STUBS, LL_INFO1000, "%s\n\n", ilStub.GetUTF8(ssb)));
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

    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    ULONG paramCount = 0;
    paramCount += info.ArgBufLayout.Values.GetCount();
    if (info.ArgBufLayout.HasTargetAddress)
    {
        paramCount++;
    }
    
    sig->AppendData(paramCount);

    sig->AppendElementType(ELEMENT_TYPE_VOID);

    for (COUNT_T i = 0; i < info.ArgBufLayout.Values.GetCount(); i++)
    {
        const ArgBufferValue& val = info.ArgBufLayout.Values[i];
        AppendTypeHandle(*sig, val.TyHnd);
    }

    if (info.ArgBufLayout.HasTargetAddress)
    {
        sig->AppendElementType(ELEMENT_TYPE_I);
    }

#ifdef _DEBUG
    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sig->GetSignature(&cbSig);
    SigTypeContext emptyContext;
    MetaSig outMsig(pSig, cbSig, info.CallSiteSig->GetModule(), &emptyContext);
    SigFormat outSig(outMsig, NULL);
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: StoreArgs sig: %s\n", outSig.GetCString()));
#endif // _DEBUG
}

MethodDesc* TailCallHelp::CreateCallTargetStub(const TailCallInfo& info)
{
    SigBuilder sigBuilder;
    CreateCallTargetStubSig(info, &sigBuilder);

    DWORD cbSig;
    PCCOR_SIGNATURE pSig = AllocateSignature(info.LoaderAlloc, sigBuilder, &cbSig);

    SigTypeContext emptyCtx;

    ILStubLinker sl(info.Caller->GetModule(),
                    Signature(pSig, cbSig),
                    &emptyCtx,
                    NULL,
                    FALSE,
                    FALSE);

    ILCodeStream* pCode = sl.NewCodeStream(ILStubLinker::kDispatch);

    // void CallTarget(void* argBuffer, void* retVal, void** pTailCallAwareRetAddress)
    const int ARG_ARG_BUFFER = 0;
    const int ARG_RET_VAL = 1;
    const int ARG_PTR_TAILCALL_AWARE_RET_ADDR = 2;

    auto emitOffs = [&](UINT offs)
    {
        pCode->EmitLDARG(ARG_ARG_BUFFER);
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
        EmitLoadTyHnd(pCode, arg.TyHnd);
        pCode->EmitSTLOC(argLcl);
    }

    DWORD targetAddrLcl;
    if (info.ArgBufLayout.HasTargetAddress)
    {
        targetAddrLcl = pCode->NewLocal(ELEMENT_TYPE_I);

        emitOffs(info.ArgBufLayout.TargetAddressOffset);
        pCode->EmitLDIND_I();
        pCode->EmitSTLOC(targetAddrLcl);
    }

    // RuntimeHelpers.FreeTailCallArgBuffer();
    pCode->EmitCALL(METHOD__RUNTIME_HELPERS__FREE_TAILCALL_ARG_BUFFER, 0, 0);

    // *pTailCallAwareRetAddr = NextCallReturnAddress();
    pCode->EmitLDARG(ARG_PTR_TAILCALL_AWARE_RET_ADDR);
    pCode->EmitCALL(METHOD__STUBHELPERS__NEXT_CALL_RETURN_ADDRESS, 0, 1);
    pCode->EmitSTIND_I();

    int numRetVals = info.CallSiteSig->IsReturnTypeVoid() ? 0 : 1;
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
                numRetVals);
        }
        else
        {
            pCode->EmitCALL(
                pCode->GetToken(info.Callee),
                static_cast<int>(argLocals.GetCount()),
                numRetVals);
        }
    }
    else
    {
        // Build the signature for the calli. TODO: Should we use the signature
        // from the original calli instead?
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
        AppendTypeHandle(calliSig, info.RetTyHnd);

        COUNT_T firstSigArg = info.CallSiteSig->HasThis() ? 1 : 0;

        for (COUNT_T i = firstSigArg; i < argLocals.GetCount(); i++)
        {
            const ArgBufferValue& val = info.ArgBufLayout.Values[i];
            AppendTypeHandle(calliSig, val.TyHnd);
        }

        DWORD cbCalliSig;
        PCCOR_SIGNATURE pCalliSig = (PCCOR_SIGNATURE)calliSig.GetSignature(&cbCalliSig);

        for (COUNT_T i = 0; i < argLocals.GetCount(); i++)
        {
            pCode->EmitLDLOC(argLocals[i]);
        }

        pCode->EmitLDLOC(targetAddrLcl);

        pCode->EmitCALLI(
            pCode->GetSigToken(pCalliSig, cbCalliSig),
            static_cast<int>(argLocals.GetCount()),
            numRetVals);
    }

    if (!info.CallSiteSig->IsReturnTypeVoid())
    {
        DWORD resultLcl = pCode->NewLocal(LocalDesc(info.RetTyHnd));
        pCode->EmitSTLOC(resultLcl);

        pCode->EmitLDARG(ARG_RET_VAL);
        pCode->EmitLDLOC(resultLcl);
        EmitStoreTyHnd(pCode, info.RetTyHnd);
    }

    pCode->EmitRET();

    Module* pLoaderModule = info.Caller->GetLoaderModule();
    MethodDesc* pCallTargetMD =
        ILStubCache::CreateAndLinkNewILStubMethodDesc(
            info.LoaderAlloc,
            pLoaderModule->GetILStubCache()->GetOrCreateStubMethodTable(pLoaderModule),
            ILSTUB_TAILCALL_CALLTARGET,
            info.Caller->GetModule(),
            pSig, cbSig,
            &emptyCtx,
            &sl);

#ifndef CROSSGEN_COMPILE
    JitILStub(pCallTargetMD);
#endif


#ifdef _DEBUG
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: CallTarget IL created\n"));
    sl.LogILStub(CORJIT_FLAGS());
#endif

    return pCallTargetMD;
}

void TailCallHelp::CreateCallTargetStubSig(const TailCallInfo& info, SigBuilder* sig)
{
    sig->AppendByte(IMAGE_CEE_CS_CALLCONV_DEFAULT);

    // Arg buffer, return value pointer, and pointer to "tail call aware return address" field.
    sig->AppendData(3);

    // Returns void
    sig->AppendElementType(ELEMENT_TYPE_VOID);

    // Arg buffer
    sig->AppendElementType(ELEMENT_TYPE_I);

    // Return value
    sig->AppendElementType(ELEMENT_TYPE_I);

    // Pointer to tail call aware return address
    sig->AppendElementType(ELEMENT_TYPE_I);

#ifdef _DEBUG
    DWORD cbSig;
    PCCOR_SIGNATURE pSig = (PCCOR_SIGNATURE)sig->GetSignature(&cbSig);
    SigTypeContext emptyContext;
    MetaSig outMsig(pSig, cbSig, info.CallSiteSig->GetModule(), &emptyContext);
    SigFormat outSig(outMsig, NULL);
    LOG((LF_STUBS, LL_INFO1000, "TAILCALLHELP: CallTarget sig: %s\n", outSig.GetCString()));
#endif // _DEBUG
}

void TailCallHelp::EmitLoadTyHnd(ILCodeStream* stream, TypeHandle tyHnd)
{
    CorElementType ty = tyHnd.GetSignatureCorElementType();
    if (tyHnd.IsByRef())
    {
        // Note: we can use an "untracked" ldind.i here even with byrefs because
        // we are loading between two tracked positions.
        stream->EmitLDIND_I();
    }
    else
    {
        int token = stream->GetToken(tyHnd);
        stream->EmitLDOBJ(token);
    }
}

void TailCallHelp::EmitStoreTyHnd(ILCodeStream* stream, TypeHandle tyHnd)
{
    CorElementType ty = tyHnd.GetSignatureCorElementType();
    if (tyHnd.IsByRef())
    {
        // Note: we can use an "untracked" stind.i here even with byrefs because
        // we are storing between two tracked positions.
        stream->EmitSTIND_I();
    }
    else
    {
        int token = stream->GetToken(tyHnd);
        stream->EmitSTOBJ(token);
    }
}

void TailCallHelp::AppendTypeHandle(SigBuilder& builder, TypeHandle th)
{
    if (th.IsByRef())
    {
        builder.AppendElementType(ELEMENT_TYPE_BYREF);
        th = th.AsTypeDesc()->GetBaseTypeParam();
    }

    CorElementType ty = th.GetSignatureCorElementType();
    if (CorTypeInfo::IsPrimitiveType(ty) ||
        ty == ELEMENT_TYPE_OBJECT || ty == ELEMENT_TYPE_STRING)
    {
        builder.AppendElementType(ty);
        return;
    }

    _ASSERTE(ty == ELEMENT_TYPE_VALUETYPE || ty == ELEMENT_TYPE_CLASS);
    builder.AppendElementType(ELEMENT_TYPE_INTERNAL);
    builder.AppendPointer(th.AsPtr());
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