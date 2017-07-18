// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#include "corexcep.h"

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                           CallImporter                                    XX
XX                                                                           XX
XX This is a helper class to import a call-inspiring opcode.                 XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/
class CallImporter
{
public:
    //------------------------------------------------------------------------
    // CallImporter constructor
    //
    // Arguments:
    //    compiler
    //
    // Notes:
    //    The constructor takes care of initializing the variables that are used
    //    during the call importation.
    CallImporter(Compiler* compiler) : compiler(compiler)
    {

        callRetTyp = TYP_COUNT;

        clsFlags = 0;
        mflags   = 0;
        argFlags = 0;

        constraintCallThisTransform = CORINFO_NO_THIS_TRANSFORM;

        sig                     = nullptr;
        methHnd                 = nullptr;
        clsHnd                  = nullptr;
        call                    = nullptr;
        args                    = nullptr;
        exactContextHnd         = nullptr;
        szCanTailCallFailReason = nullptr;
        ldftnToken              = nullptr;
        extraArg                = nullptr;

        exactContextNeedsRuntimeLookup = false;
        bIntrinsicImported             = false;
        readonlyCall                   = false;

        canTailCall = true;
    }

    //------------------------------------------------------------------------
    // impImportCall: see the Compiler::impImportCall description.
    var_types importCall(OPCODE                  opcode,
                         CORINFO_RESOLVED_TOKEN* pResolvedToken,
                         CORINFO_RESOLVED_TOKEN* pConstrainedResolvedToken,
                         GenTreePtr              newobjThis,
                         int                     prefixFlags,
                         CORINFO_CALL_INFO*      callInfo,
                         IL_OFFSET               rawILOffset)
    {
        assert(opcode == CEE_CALL || opcode == CEE_CALLVIRT || opcode == CEE_NEWOBJ || opcode == CEE_CALLI);

        IL_OFFSETX ilOffset = compiler->impCurILOffset(rawILOffset, true);

        int tailCall = prefixFlags & PREFIX_TAILCALL;
        readonlyCall = (prefixFlags & PREFIX_READONLY) != 0;

        // Synchronized methods need to call CORINFO_HELP_MON_EXIT at the end. We could
        // do that before tailcalls, but that is probably not the intended
        // semantic. So just disallow tailcalls from synchronized methods.
        // Also, popping arguments in a varargs function is more work and NYI
        // If we have a security object, we have to keep our frame around for callers
        // to see any imperative security.
        if (compiler->info.compFlags & CORINFO_FLG_SYNCH)
        {
            canTailCall             = false;
            szCanTailCallFailReason = "Caller is synchronized";
        }
#if !FEATURE_FIXED_OUT_ARGS
        else if (info.compIsVarArgs)
        {
            canTailCall             = false;
            szCanTailCallFailReason = "Caller is varargs";
        }
#endif // FEATURE_FIXED_OUT_ARGS
        else if (compiler->opts.compNeedSecurityCheck)
        {
            canTailCall             = false;
            szCanTailCallFailReason = "Caller requires a security check.";
        }

        /*-------------------------------------------------------------------------
        * First create the call node
        */

        if (opcode == CEE_CALLI)
        {
            /* Get the call site sig */
            compiler->eeGetSig(pResolvedToken->token, compiler->info.compScopeHnd,
                               compiler->impTokenLookupContextHandle, &calliSig);

            callRetTyp = JITtype2varType(calliSig.retType);

            call = compiler->impImportIndirectCall(&calliSig, ilOffset);

            // We don't know the target method, so we have to infer the flags, or
            // assume the worst-case.
            mflags = (calliSig.callConv & CORINFO_CALLCONV_HASTHIS) ? 0 : CORINFO_FLG_STATIC;

#ifdef DEBUG
            if (compiler->verbose)
            {
                unsigned structSize =
                    (callRetTyp == TYP_STRUCT) ? compiler->info.compCompHnd->getClassSize(calliSig.retTypeSigClass) : 0;
                printf("\nIn Compiler::impImportCall: opcode is %s, kind=%d, callRetType is %s, structSize is %d\n",
                       opcodeNames[opcode], callInfo->kind, varTypeName(callRetTyp), structSize);
            }
#endif
            // This should be checked in impImportBlockCode.
            assert(!compiler->compIsForInlining() ||
                   !(compiler->impInlineInfo->inlineCandidateInfo->dwRestrictions & INLINE_RESPECT_BOUNDARY));

            sig = &calliSig;

#ifdef DEBUG
            // We cannot lazily obtain the signature of a CALLI call because it has no method
            // handle that we can use, so we need to save its full call signature here.
            assert(call->gtCall.callSig == nullptr);
            call->gtCall.callSig  = new (compiler, CMK_CorSig) CORINFO_SIG_INFO;
            *call->gtCall.callSig = calliSig;
#endif // DEBUG

            if (compiler->IsTargetAbi(CORINFO_CORERT_ABI))
            {
                bool managedCall = (((calliSig.callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_STDCALL) &&
                                    ((calliSig.callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_C) &&
                                    ((calliSig.callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_THISCALL) &&
                                    ((calliSig.callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_FASTCALL));
                if (managedCall)
                {
                    compiler->addFatPointerCandidate(call->AsCall());
                }
            }
        }
        else // (opcode != CEE_CALLI)
        {
            CorInfoIntrinsics intrinsicID = CORINFO_INTRINSIC_Count;

            // Passing CORINFO_CALLINFO_ALLOWINSTPARAM indicates that this JIT is prepared to
            // supply the instantiation parameters necessary to make direct calls to underlying
            // shared generic code, rather than calling through instantiating stubs.  If the
            // returned signature has CORINFO_CALLCONV_PARAMTYPE then this indicates that the JIT
            // must indeed pass an instantiation parameter.

            methHnd = callInfo->hMethod;

            sig        = &(callInfo->sig);
            callRetTyp = JITtype2varType(sig->retType);

            mflags = callInfo->methodFlags;

#ifdef DEBUG
            if (compiler->verbose)
            {
                unsigned structSize =
                    (callRetTyp == TYP_STRUCT) ? compiler->info.compCompHnd->getClassSize(sig->retTypeSigClass) : 0;
                printf("\nIn Compiler::impImportCall: opcode is %s, kind=%d, callRetType is %s, structSize is %d\n",
                       opcodeNames[opcode], callInfo->kind, varTypeName(callRetTyp), structSize);
            }
#endif
            if (compiler->compIsForInlining())
            {
                /* Does this call site have security boundary restrictions? */

                if (compiler->impInlineInfo->inlineCandidateInfo->dwRestrictions & INLINE_RESPECT_BOUNDARY)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLSITE_CROSS_BOUNDARY_SECURITY);
                    return callRetTyp;
                }

                /* Does the inlinee need a security check token on the frame */

                if (mflags & CORINFO_FLG_SECURITYCHECK)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_NEEDS_SECURITY_CHECK);
                    return callRetTyp;
                }

                /* Does the inlinee use StackCrawlMark */

                if (mflags & CORINFO_FLG_DONT_INLINE_CALLER)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_STACK_CRAWL_MARK);
                    return callRetTyp;
                }

                /* For now ignore delegate invoke */

                if (mflags & CORINFO_FLG_DELEGATE_INVOKE)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_HAS_DELEGATE_INVOKE);
                    return callRetTyp;
                }

                /* For now ignore varargs */
                if ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_NATIVEVARARG)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_HAS_NATIVE_VARARGS);
                    return callRetTyp;
                }

                if ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_VARARG)
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_HAS_MANAGED_VARARGS);
                    return callRetTyp;
                }

                if ((mflags & CORINFO_FLG_VIRTUAL) && (sig->sigInst.methInstCount != 0) && (opcode == CEE_CALLVIRT))
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_IS_GENERIC_VIRTUAL);
                    return callRetTyp;
                }
            }

            clsHnd = pResolvedToken->hClass;

            clsFlags = callInfo->classFlags;

#ifdef DEBUG
            // If this is a call to JitTestLabel.Mark, do "early inlining", and record the test attribute.

            // This recognition should really be done by knowing the methHnd of the relevant Mark method(s).
            // These should be in mscorlib.h, and available through a JIT/EE interface call.
            const char* modName;
            const char* className;
            const char* methodName;
            if ((className = compiler->eeGetClassName(clsHnd)) != nullptr &&
                strcmp(className, "System.Runtime.CompilerServices.JitTestLabel") == 0 &&
                (methodName = compiler->eeGetMethodName(methHnd, &modName)) != nullptr &&
                strcmp(methodName, "Mark") == 0)
            {
                return compiler->impImportJitTestLabelMark(sig->numArgs);
            }
#endif // DEBUG

            // <NICE> Factor this into getCallInfo </NICE>
            if ((mflags & CORINFO_FLG_INTRINSIC) && !pConstrainedResolvedToken)
            {
                call = compiler->impIntrinsic(newobjThis, clsHnd, methHnd, sig, pResolvedToken->token, readonlyCall,
                                              (canTailCall && (tailCall != 0)), &intrinsicID);

                if (compiler->compIsForInlining() && compiler->compInlineResult->IsFailure())
                {
                    return callRetTyp;
                }

                if (call != nullptr)
                {
                    assert(!(mflags & CORINFO_FLG_VIRTUAL) || (mflags & CORINFO_FLG_FINAL) ||
                           (clsFlags & CORINFO_FLG_FINAL));

#ifdef FEATURE_READYTORUN_COMPILER
                    if (call->OperGet() == GT_INTRINSIC)
                    {
                        if (compiler->opts.IsReadyToRun())
                        {
                            noway_assert(callInfo->kind == CORINFO_CALL);
                            call->gtIntrinsic.gtEntryPoint = callInfo->codePointerLookup.constLookup;
                        }
                        else
                        {
                            call->gtIntrinsic.gtEntryPoint.addr = nullptr;
                        }
                    }
#endif

                    bIntrinsicImported = true;
                }
            }

#ifdef FEATURE_SIMD
            else if (compiler->featureSIMD)
            {
                call = compiler->impSIMDIntrinsic(opcode, newobjThis, clsHnd, methHnd, sig, pResolvedToken->token);
                if (call != nullptr)
                {
                    bIntrinsicImported = true;
                }
            }
#endif // FEATURE_SIMD
            if (!bIntrinsicImported)
            {

                if ((mflags & CORINFO_FLG_VIRTUAL) && (mflags & CORINFO_FLG_EnC) && (opcode == CEE_CALLVIRT))
                {
                    NO_WAY("Virtual call to a function added via EnC is not supported");
                }

                if ((sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_DEFAULT &&
                    (sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_VARARG &&
                    (sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_NATIVEVARARG)
                {
                    BADCODE("Bad calling convention");
                }

                //-------------------------------------------------------------------------
                //  Construct the call node
                //
                // Work out what sort of call we're making.
                // Dispense with virtual calls implemented via LDVIRTFTN immediately.

                constraintCallThisTransform    = callInfo->thisTransform;
                exactContextHnd                = callInfo->contextHandle;
                exactContextNeedsRuntimeLookup = callInfo->exactContextNeedsRuntimeLookup == TRUE;

                // Recursive call is treaded as a loop to the begining of the method.
                if (methHnd == compiler->info.compMethodHnd)
                {
#ifdef DEBUG
                    if (compiler->verbose)
                    {
                        JITDUMP("\nFound recursive call in the method. Mark BB%02u to BB%02u as having a backward "
                                "branch.\n",
                                compiler->fgFirstBB->bbNum, compiler->compCurBB->bbNum);
                    }
#endif
                    compiler->fgMarkBackwardJump(compiler->fgFirstBB, compiler->compCurBB);
                }

                switch (callInfo->kind)
                {

                    case CORINFO_VIRTUALCALL_STUB:
                    {
                        assert(!(mflags & CORINFO_FLG_STATIC)); // can't call a static method
                        assert(!(clsFlags & CORINFO_FLG_VALUECLASS));
                        if (callInfo->stubLookup.lookupKind.needsRuntimeLookup)
                        {

                            if (compiler->compIsForInlining())
                            {
                                // Don't import runtime lookups when inlining
                                // Inlining has to be aborted in such a case
                                /* XXX Fri 3/20/2009
                                * By the way, this would never succeed.  If the handle lookup is into the generic
                                * dictionary for a candidate, you'll generate different dictionary offsets and the
                                * inlined code will crash.
                                *
                                * To anyone code reviewing this, when could this ever succeed in the future?  It'll
                                * always have a handle lookup.  These lookups are safe intra-module, but we're just
                                * failing here.
                                */
                                compiler->compInlineResult->NoteFatal(InlineObservation::CALLSITE_HAS_COMPLEX_HANDLE);
                                return callRetTyp;
                            }

                            GenTreePtr stubAddr =
                                compiler->impRuntimeLookupToTree(pResolvedToken, &callInfo->stubLookup, methHnd);
                            assert(!compiler->compDonotInline());

                            // This is the rough code to set up an indirect stub call
                            assert(stubAddr != nullptr);

                            // The stubAddr may be a
                            // complex expression. As it is evaluated after the args,
                            // it may cause registered args to be spilled. Simply spill it.

                            unsigned lclNum = compiler->lvaGrabTemp(true DEBUGARG("VirtualCall with runtime lookup"));
                            compiler->impAssignTempGen(lclNum, stubAddr, (unsigned)compiler->CHECK_SPILL_ALL);
                            stubAddr = compiler->gtNewLclvNode(lclNum, TYP_I_IMPL);

                            // Create the actual call node

                            assert((sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_VARARG &&
                                   (sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_NATIVEVARARG);

                            call = compiler->gtNewIndCallNode(stubAddr, callRetTyp, nullptr);

                            call->gtFlags |= GTF_EXCEPT | (stubAddr->gtFlags & GTF_GLOB_EFFECT);
                            call->gtFlags |= GTF_CALL_VIRT_STUB;

#ifdef _TARGET_X86_
                            // No tailcalls allowed for these yet...
                            canTailCall             = false;
                            szCanTailCallFailReason = "VirtualCall with runtime lookup";
#endif
                        }
                        else
                        {
                            // ok, the stub is available at compile type.

                            call =
                                compiler->gtNewCallNode(CT_USER_FUNC, callInfo->hMethod, callRetTyp, nullptr, ilOffset);
                            call->gtCall.gtStubCallStubAddr = callInfo->stubLookup.constLookup.addr;
                            call->gtFlags |= GTF_CALL_VIRT_STUB;
                            assert(callInfo->stubLookup.constLookup.accessType != IAT_PPVALUE);
                            if (callInfo->stubLookup.constLookup.accessType == IAT_PVALUE)
                            {
                                call->gtCall.gtCallMoreFlags |= GTF_CALL_M_VIRTSTUB_REL_INDIRECT;
                            }
                        }

#ifdef FEATURE_READYTORUN_COMPILER
                        if (compiler->opts.IsReadyToRun())
                        {
                            // Null check is sometimes needed for ready to run to handle
                            // non-virtual <-> virtual changes between versions
                            if (callInfo->nullInstanceCheck)
                            {
                                call->gtFlags |= GTF_CALL_NULLCHECK;
                            }
                        }
#endif

                        break;
                    }

                    case CORINFO_VIRTUALCALL_VTABLE:
                    {
                        assert(!(mflags & CORINFO_FLG_STATIC)); // can't call a static method
                        assert(!(clsFlags & CORINFO_FLG_VALUECLASS));
                        call = compiler->gtNewCallNode(CT_USER_FUNC, callInfo->hMethod, callRetTyp, nullptr, ilOffset);
                        call->gtFlags |= GTF_CALL_VIRT_VTABLE;
                        break;
                    }

                    case CORINFO_VIRTUALCALL_LDVIRTFTN:
                    {
                        if (compiler->compIsForInlining())
                        {
                            compiler->compInlineResult->NoteFatal(InlineObservation::CALLSITE_HAS_CALL_VIA_LDVIRTFTN);
                            return callRetTyp;
                        }

                        assert(!(mflags & CORINFO_FLG_STATIC)); // can't call a static method
                        assert(!(clsFlags & CORINFO_FLG_VALUECLASS));
                        // OK, We've been told to call via LDVIRTFTN, so just
                        // take the call now....

                        args = compiler->impPopList(sig->numArgs, &argFlags, sig);

                        GenTreePtr thisPtr = compiler->impPopStack().val;
                        thisPtr =
                            compiler->impTransformThis(thisPtr, pConstrainedResolvedToken, callInfo->thisTransform);
                        if (compiler->compDonotInline())
                        {
                            return callRetTyp;
                        }

                        // Clone the (possibly transformed) "this" pointer
                        GenTreePtr thisPtrCopy;
                        thisPtr = compiler->impCloneExpr(thisPtr, &thisPtrCopy, NO_CLASS_HANDLE,
                                                         (unsigned)compiler->CHECK_SPILL_ALL,
                                                         nullptr DEBUGARG("LDVIRTFTN this pointer"));

                        GenTreePtr fptr = compiler->impImportLdvirtftn(thisPtr, pResolvedToken, callInfo);

                        if (compiler->compDonotInline())
                        {
                            return callRetTyp;
                        }

                        thisPtr = nullptr; // can't reuse it

                        // Now make an indirect call through the function pointer

                        unsigned lclNum = compiler->lvaGrabTemp(true DEBUGARG("VirtualCall through function pointer"));
                        compiler->impAssignTempGen(lclNum, fptr, (unsigned)compiler->CHECK_SPILL_ALL);
                        fptr = compiler->gtNewLclvNode(lclNum, TYP_I_IMPL);

                        // Create the actual call node

                        call                    = compiler->gtNewIndCallNode(fptr, callRetTyp, args, ilOffset);
                        call->gtCall.gtCallObjp = thisPtrCopy;
                        call->gtFlags |= GTF_EXCEPT | (fptr->gtFlags & GTF_GLOB_EFFECT);

                        if ((sig->sigInst.methInstCount != 0) && compiler->IsTargetAbi(CORINFO_CORERT_ABI))
                        {
                            // CoreRT generic virtual method: need to handle potential fat function pointers
                            compiler->addFatPointerCandidate(call->AsCall());
                        }
#ifdef FEATURE_READYTORUN_COMPILER
                        if (compiler->opts.IsReadyToRun())
                        {
                            // Null check is needed for ready to run to handle
                            // non-virtual <-> virtual changes between versions
                            call->gtFlags |= GTF_CALL_NULLCHECK;
                        }
#endif

                        // Sine we are jumping over some code, check that its OK to skip that code
                        assert((sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_VARARG &&
                               (sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_NATIVEVARARG);
                        goto DONE;
                    }

                    case CORINFO_CALL:
                    {
                        // This is for a non-virtual, non-interface etc. call
                        call = compiler->gtNewCallNode(CT_USER_FUNC, callInfo->hMethod, callRetTyp, nullptr, ilOffset);

                        // We remove the nullcheck for the GetType call instrinsic.
                        // TODO-CQ: JIT64 does not introduce the null check for many more helper calls
                        // and instrinsics.
                        if (callInfo->nullInstanceCheck &&
                            !((mflags & CORINFO_FLG_INTRINSIC) != 0 &&
                              (intrinsicID == CORINFO_INTRINSIC_Object_GetType)))
                        {
                            call->gtFlags |= GTF_CALL_NULLCHECK;
                        }

#ifdef FEATURE_READYTORUN_COMPILER
                        if (compiler->opts.IsReadyToRun())
                        {
                            call->gtCall.setEntryPoint(callInfo->codePointerLookup.constLookup);
                        }
#endif
                        break;
                    }

                    case CORINFO_CALL_CODE_POINTER:
                    {
                        // The EE has asked us to call by computing a code pointer and then doing an
                        // indirect call.  This is because a runtime lookup is required to get the code entry point.

                        // These calls always follow a uniform calling convention, i.e. no extra hidden params
                        assert((sig->callConv & CORINFO_CALLCONV_PARAMTYPE) == 0);

                        assert((sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_VARARG);
                        assert((sig->callConv & CORINFO_CALLCONV_MASK) != CORINFO_CALLCONV_NATIVEVARARG);

                        GenTreePtr fptr = compiler->impLookupToTree(pResolvedToken, &callInfo->codePointerLookup,
                                                                    GTF_ICON_FTN_ADDR, callInfo->hMethod);

                        if (compiler->compDonotInline())
                        {
                            return callRetTyp;
                        }

                        // Now make an indirect call through the function pointer

                        unsigned lclNum =
                            compiler->lvaGrabTemp(true DEBUGARG("Indirect call through function pointer"));
                        compiler->impAssignTempGen(lclNum, fptr, (unsigned)compiler->CHECK_SPILL_ALL);
                        fptr = compiler->gtNewLclvNode(lclNum, TYP_I_IMPL);

                        call = compiler->gtNewIndCallNode(fptr, callRetTyp, nullptr, ilOffset);
                        call->gtFlags |= GTF_EXCEPT | (fptr->gtFlags & GTF_GLOB_EFFECT);
                        if (callInfo->nullInstanceCheck)
                        {
                            call->gtFlags |= GTF_CALL_NULLCHECK;
                        }

                        break;
                    }

                    default:
                        assert(!"unknown call kind");
                        break;
                }

                //-------------------------------------------------------------------------
                // Set more flags

                PREFIX_ASSUME(call != nullptr);

                if (mflags & CORINFO_FLG_NOGCCHECK)
                {
                    call->gtCall.gtCallMoreFlags |= GTF_CALL_M_NOGCCHECK;
                }

                // Mark call if it's one of the ones we will maybe treat as an intrinsic
                if (intrinsicID == CORINFO_INTRINSIC_Object_GetType || intrinsicID == CORINFO_INTRINSIC_TypeEQ ||
                    intrinsicID == CORINFO_INTRINSIC_TypeNEQ ||
                    intrinsicID == CORINFO_INTRINSIC_GetCurrentManagedThread ||
                    intrinsicID == CORINFO_INTRINSIC_GetManagedThreadId)
                {
                    call->gtCall.gtCallMoreFlags |= GTF_CALL_M_SPECIAL_INTRINSIC;
                }
            }
        }
        if (!bIntrinsicImported)
        {
            assert(sig);
            assert(clsHnd || (opcode == CEE_CALLI)); // We're never verifying for CALLI, so this is not set.

            /* Some sanity checks */

            // CALL_VIRT and NEWOBJ must have a THIS pointer
            assert((opcode != CEE_CALLVIRT && opcode != CEE_NEWOBJ) || (sig->callConv & CORINFO_CALLCONV_HASTHIS));
            // static bit and hasThis are negations of one another
            assert(((mflags & CORINFO_FLG_STATIC) != 0) == ((sig->callConv & CORINFO_CALLCONV_HASTHIS) == 0));
            assert(call != nullptr);

            /*-------------------------------------------------------------------------
            * Check special-cases etc
            */

            /* Special case - Check if it is a call to Delegate.Invoke(). */

            if (mflags & CORINFO_FLG_DELEGATE_INVOKE)
            {
                assert(!compiler->compIsForInlining());
                assert(!(mflags & CORINFO_FLG_STATIC)); // can't call a static method
                assert(mflags & CORINFO_FLG_FINAL);

                /* Set the delegate flag */
                call->gtCall.gtCallMoreFlags |= GTF_CALL_M_DELEGATE_INV;

                if (callInfo->secureDelegateInvoke)
                {
                    call->gtCall.gtCallMoreFlags |= GTF_CALL_M_SECURE_DELEGATE_INV;
                }

                if (opcode == CEE_CALLVIRT)
                {
                    assert(mflags & CORINFO_FLG_FINAL);

                    /* It should have the GTF_CALL_NULLCHECK flag set. Reset it */
                    assert(call->gtFlags & GTF_CALL_NULLCHECK);
                    call->gtFlags &= ~GTF_CALL_NULLCHECK;
                }
            }

            CORINFO_CLASS_HANDLE actualMethodRetTypeSigClass;
            actualMethodRetTypeSigClass = sig->retTypeSigClass;
            if (varTypeIsStruct(callRetTyp))
            {
                callRetTyp   = compiler->impNormStructType(actualMethodRetTypeSigClass);
                call->gtType = callRetTyp;
            }

#if !FEATURE_VARARG
            /* Check for varargs */
            if ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_VARARG ||
                (sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_NATIVEVARARG)
            {
                BADCODE("Varargs not supported.");
            }
#endif // !FEATURE_VARARG

#ifdef UNIX_X86_ABI
            if (call->gtCall.callSig == nullptr)
            {
                call->gtCall.callSig  = new (this, CMK_CorSig) CORINFO_SIG_INFO;
                *call->gtCall.callSig = *sig;
            }
#endif // UNIX_X86_ABI

            if ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_VARARG ||
                (sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_NATIVEVARARG)
            {
                assert(!compiler->compIsForInlining());

                /* Set the right flags */

                call->gtFlags |= GTF_CALL_POP_ARGS;
                call->gtCall.gtCallMoreFlags |= GTF_CALL_M_VARARGS;

                /* Can't allow tailcall for varargs as it is caller-pop. The caller
                will be expecting to pop a certain number of arguments, but if we
                tailcall to a function with a different number of arguments, we
                are hosed. There are ways around this (caller remembers esp value,
                varargs is not caller-pop, etc), but not worth it. */
                CLANG_FORMAT_COMMENT_ANCHOR;

#ifdef _TARGET_X86_
                if (canTailCall)
                {
                    canTailCall             = false;
                    szCanTailCallFailReason = "Callee is varargs";
                }
#endif

                /* Get the total number of arguments - this is already correct
                * for CALLI - for methods we have to get it from the call site */

                if (opcode != CEE_CALLI)
                {
#ifdef DEBUG
                    unsigned numArgsDef = sig->numArgs;
#endif
                    compiler->eeGetCallSiteSig(pResolvedToken->token, compiler->info.compScopeHnd,
                                               compiler->impTokenLookupContextHandle, sig);

#ifdef DEBUG
                    // We cannot lazily obtain the signature of a vararg call because using its method
                    // handle will give us only the declared argument list, not the full argument list.
                    assert(call->gtCall.callSig == nullptr);
                    call->gtCall.callSig  = new (compiler, CMK_CorSig) CORINFO_SIG_INFO;
                    *call->gtCall.callSig = *sig;
#endif

                    // For vararg calls we must be sure to load the return type of the
                    // method actually being called, as well as the return types of the
                    // specified in the vararg signature. With type equivalency, these types
                    // may not be the same.
                    if (sig->retTypeSigClass != actualMethodRetTypeSigClass)
                    {
                        if (actualMethodRetTypeSigClass != nullptr && sig->retType != CORINFO_TYPE_CLASS &&
                            sig->retType != CORINFO_TYPE_BYREF && sig->retType != CORINFO_TYPE_PTR &&
                            sig->retType != CORINFO_TYPE_VAR)
                        {
                            // Make sure that all valuetypes (including enums) that we push are loaded.
                            // This is to guarantee that if a GC is triggerred from the prestub of this methods,
                            // all valuetypes in the method signature are already loaded.
                            // We need to be able to find the size of the valuetypes, but we cannot
                            // do a class-load from within GC.
                            compiler->info.compCompHnd->classMustBeLoadedBeforeCodeIsRun(actualMethodRetTypeSigClass);
                        }
                    }

                    assert(numArgsDef <= sig->numArgs);
                }

                /* We will have "cookie" as the last argument but we cannot push
                * it on the operand stack because we may overflow, so we append it
                * to the arg list next after we pop them */
            }

            if (mflags & CORINFO_FLG_SECURITYCHECK)
            {
                assert(!compiler->compIsForInlining());

                // Need security prolog/epilog callouts when there is
                // imperative security in the method. This is to give security a
                // chance to do any setup in the prolog and cleanup in the epilog if needed.

                if (compiler->compIsForInlining())
                {
                    // Cannot handle this if the method being imported is an inlinee by itself.
                    // Because inlinee method does not have its own frame.

                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_NEEDS_SECURITY_CHECK);
                    return callRetTyp;
                }
                else
                {
                    compiler->tiSecurityCalloutNeeded = true;

                    // If the current method calls a method which needs a security check,
                    // (i.e. the method being compiled has imperative security)
                    // we need to reserve a slot for the security object in
                    // the current method's stack frame
                    compiler->opts.compNeedSecurityCheck = true;
                }
            }

            //--------------------------- Inline NDirect ------------------------------

            // For inline cases we technically should look at both the current
            // block and the call site block (or just the latter if we've
            // fused the EH trees). However the block-related checks pertain to
            // EH and we currently won't inline a method with EH. So for
            // inlinees, just checking the call site block is sufficient.
            {
                // New lexical block here to avoid compilation errors because of GOTOs.
                BasicBlock* block =
                    compiler->compIsForInlining() ? compiler->impInlineInfo->iciBlock : compiler->compCurBB;
                compiler->impCheckForPInvokeCall(call->AsCall(), methHnd, sig, mflags, block);
            }

            if (call->gtFlags & GTF_CALL_UNMANAGED)
            {
                // We set up the unmanaged call by linking the frame, disabling GC, etc
                // This needs to be cleaned up on return
                if (canTailCall)
                {
                    canTailCall             = false;
                    szCanTailCallFailReason = "Callee is native";
                }

                compiler->impPopArgsForUnmanagedCall(call, sig);

                goto DONE;
            }
            else if ((opcode == CEE_CALLI) && (((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_STDCALL) ||
                                               ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_C) ||
                                               ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_THISCALL) ||
                                               ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_FASTCALL)))
            {
                if (!compiler->info.compCompHnd->canGetCookieForPInvokeCalliSig(sig))
                {
                    // Normally this only happens with inlining.
                    // However, a generic method (or type) being NGENd into another module
                    // can run into this issue as well.  There's not an easy fall-back for NGEN
                    // so instead we fallback to JIT.
                    if (compiler->compIsForInlining())
                    {
                        compiler->compInlineResult->NoteFatal(InlineObservation::CALLSITE_CANT_EMBED_PINVOKE_COOKIE);
                    }
                    else
                    {
                        IMPL_LIMITATION("Can't get PInvoke cookie (cross module generics)");
                    }

                    return callRetTyp;
                }

                GenTreePtr cookie = compiler->eeGetPInvokeCookie(sig);

                // This cookie is required to be either a simple GT_CNS_INT or
                // an indirection of a GT_CNS_INT
                //
                GenTreePtr cookieConst = cookie;
                if (cookie->gtOper == GT_IND)
                {
                    cookieConst = cookie->gtOp.gtOp1;
                }
                assert(cookieConst->gtOper == GT_CNS_INT);

                // Setting GTF_DONT_CSE on the GT_CNS_INT as well as on the GT_IND (if it exists) will ensure that
                // we won't allow this tree to participate in any CSE logic
                //
                cookie->gtFlags |= GTF_DONT_CSE;
                cookieConst->gtFlags |= GTF_DONT_CSE;

                call->gtCall.gtCallCookie = cookie;

                if (canTailCall)
                {
                    canTailCall             = false;
                    szCanTailCallFailReason = "PInvoke calli";
                }
            }

            /*-------------------------------------------------------------------------
            * Create the argument list
            */

            //-------------------------------------------------------------------------
            // Special case - for varargs we have an implicit last argument

            if ((sig->callConv & CORINFO_CALLCONV_MASK) == CORINFO_CALLCONV_VARARG)
            {
                assert(!compiler->compIsForInlining());

                void *varCookie, *pVarCookie;
                if (!compiler->info.compCompHnd->canGetVarArgsHandle(sig))
                {
                    compiler->compInlineResult->NoteFatal(InlineObservation::CALLSITE_CANT_EMBED_VARARGS_COOKIE);
                    return callRetTyp;
                }

                varCookie = compiler->info.compCompHnd->getVarArgsHandle(sig, &pVarCookie);
                assert((!varCookie) != (!pVarCookie));
                GenTreePtr cookie = compiler->gtNewIconEmbHndNode(varCookie, pVarCookie, GTF_ICON_VARG_HDL);

                assert(extraArg == nullptr);
                extraArg = compiler->gtNewArgList(cookie);
            }

            //-------------------------------------------------------------------------
            // Extra arg for shared generic code and array methods
            //
            // Extra argument containing instantiation information is passed in the
            // following circumstances:
            // (a) To the "Address" method on array classes; the extra parameter is
            //     the array's type handle (a TypeDesc)
            // (b) To shared-code instance methods in generic structs; the extra parameter
            //     is the struct's type handle (a vtable ptr)
            // (c) To shared-code per-instantiation non-generic static methods in generic
            //     classes and structs; the extra parameter is the type handle
            // (d) To shared-code generic methods; the extra parameter is an
            //     exact-instantiation MethodDesc
            //
            // We also set the exact type context associated with the call so we can
            // inline the call correctly later on.

            if (sig->callConv & CORINFO_CALLCONV_PARAMTYPE)
            {
                assert(call->gtCall.gtCallType == CT_USER_FUNC);
                if (clsHnd == nullptr)
                {
                    NO_WAY("CALLI on parameterized type");
                }

                assert(opcode != CEE_CALLI);

                GenTreePtr instParam;
                BOOL       runtimeLookup;

                // Instantiated generic method
                if (((SIZE_T)exactContextHnd & CORINFO_CONTEXTFLAGS_MASK) == CORINFO_CONTEXTFLAGS_METHOD)
                {
                    CORINFO_METHOD_HANDLE exactMethodHandle =
                        (CORINFO_METHOD_HANDLE)((SIZE_T)exactContextHnd & ~CORINFO_CONTEXTFLAGS_MASK);

                    if (!exactContextNeedsRuntimeLookup)
                    {
#ifdef FEATURE_READYTORUN_COMPILER
                        if (compiler->opts.IsReadyToRun())
                        {
                            instParam = compiler->impReadyToRunLookupToTree(&callInfo->instParamLookup,
                                                                            GTF_ICON_METHOD_HDL, exactMethodHandle);
                            if (instParam == nullptr)
                            {
                                return callRetTyp;
                            }
                        }
                        else
#endif
                        {
                            instParam = compiler->gtNewIconEmbMethHndNode(exactMethodHandle);
                            compiler->info.compCompHnd->methodMustBeLoadedBeforeCodeIsRun(exactMethodHandle);
                        }
                    }
                    else
                    {
                        instParam =
                            compiler->impTokenToHandle(pResolvedToken, &runtimeLookup, TRUE /*mustRestoreHandle*/);
                        if (instParam == nullptr)
                        {
                            return callRetTyp;
                        }
                    }
                }

                // otherwise must be an instance method in a generic struct,
                // a static method in a generic type, or a runtime-generated array method
                else
                {
                    assert(((SIZE_T)exactContextHnd & CORINFO_CONTEXTFLAGS_MASK) == CORINFO_CONTEXTFLAGS_CLASS);
                    CORINFO_CLASS_HANDLE exactClassHandle =
                        (CORINFO_CLASS_HANDLE)((SIZE_T)exactContextHnd & ~CORINFO_CONTEXTFLAGS_MASK);

                    if (compiler->compIsForInlining() && (clsFlags & CORINFO_FLG_ARRAY) != 0)
                    {
                        compiler->compInlineResult->NoteFatal(InlineObservation::CALLEE_IS_ARRAY_METHOD);
                        return callRetTyp;
                    }

                    if ((clsFlags & CORINFO_FLG_ARRAY) && readonlyCall)
                    {
                        // We indicate "readonly" to the Address operation by using a null
                        // instParam.
                        instParam = compiler->gtNewIconNode(0, TYP_REF);
                    }
                    else if (!exactContextNeedsRuntimeLookup)
                    {
#ifdef FEATURE_READYTORUN_COMPILER
                        if (compiler->opts.IsReadyToRun())
                        {
                            instParam = compiler->impReadyToRunLookupToTree(&callInfo->instParamLookup,
                                                                            GTF_ICON_CLASS_HDL, exactClassHandle);
                            if (instParam == nullptr)
                            {
                                return callRetTyp;
                            }
                        }
                        else
#endif
                        {
                            instParam = compiler->gtNewIconEmbClsHndNode(exactClassHandle);
                            compiler->info.compCompHnd->classMustBeLoadedBeforeCodeIsRun(exactClassHandle);
                        }
                    }
                    else
                    {

                        // If the EE was able to resolve a constrained call, the instantiating parameter to use is the
                        // type
                        // by which the call was constrained with. We embed pConstrainedResolvedToken as the extra
                        // argument
                        // because pResolvedToken is an interface method and interface types make a poor generic
                        // context.
                        if (pConstrainedResolvedToken)
                        {
                            instParam =
                                compiler->impTokenToHandle(pConstrainedResolvedToken, &runtimeLookup,
                                                           TRUE /*mustRestoreHandle*/, FALSE /* importParent */);
                        }
                        else
                        {
                            instParam = compiler->impParentClassTokenToHandle(pResolvedToken, &runtimeLookup,
                                                                              TRUE /*mustRestoreHandle*/);
                        }
                        if (instParam == nullptr)
                        {
                            return callRetTyp;
                        }
                    }
                }

                assert(extraArg == nullptr);
                extraArg = compiler->gtNewArgList(instParam);
            }

            // Inlining may need the exact type context (exactContextHnd) if we're inlining shared generic code, in
            // particular
            // to inline 'polytypic' operations such as static field accesses, type tests and method calls which
            // rely on the exact context. The exactContextHnd is passed back to the JitInterface at appropriate
            // points.
            // exactContextHnd is not currently required when inlining shared generic code into shared
            // generic code, since the inliner aborts whenever shared code polytypic operations are encountered
            // (e.g. anything marked needsRuntimeLookup)
            if (exactContextNeedsRuntimeLookup)
            {
                exactContextHnd = nullptr;
            }

            if ((opcode == CEE_NEWOBJ) && ((clsFlags & CORINFO_FLG_DELEGATE) != 0))
            {
                // Only verifiable cases are supported.
                // dup; ldvirtftn; newobj; or ldftn; newobj.
                // IL test could contain unverifiable sequence, in this case optimization should not be done.
                if (compiler->impStackHeight() > 0)
                {
                    typeInfo delegateTypeInfo = compiler->impStackTop().seTypeInfo;
                    if (delegateTypeInfo.IsToken())
                    {
                        ldftnToken = delegateTypeInfo.GetToken();
                    }
                }
            }

            //-------------------------------------------------------------------------
            // The main group of arguments

            args = call->gtCall.gtCallArgs = compiler->impPopList(sig->numArgs, &argFlags, sig, extraArg);

            if (args)
            {
                call->gtFlags |= args->gtFlags & GTF_GLOB_EFFECT;
            }

            //-------------------------------------------------------------------------
            // The "this" pointer

            if (!(mflags & CORINFO_FLG_STATIC) && !((opcode == CEE_NEWOBJ) && (newobjThis == nullptr)))
            {
                GenTreePtr obj;

                if (opcode == CEE_NEWOBJ)
                {
                    obj = newobjThis;
                }
                else
                {
                    obj = compiler->impPopStack().val;
                    obj = compiler->impTransformThis(obj, pConstrainedResolvedToken, constraintCallThisTransform);
                    if (compiler->compDonotInline())
                    {
                        return callRetTyp;
                    }
                }

                /* Is this a virtual or interface call? */

                if ((call->gtFlags & GTF_CALL_VIRT_KIND_MASK) != GTF_CALL_NONVIRT)
                {
                    /* only true object pointers can be virtual */
                    assert(obj->gtType == TYP_REF);

                    // See if we can devirtualize.
                    compiler->impDevirtualizeCall(call->AsCall(), obj, &callInfo->hMethod, &callInfo->methodFlags,
                                                  &callInfo->contextHandle, &exactContextHnd);
                }
                else
                {
                    if (compiler->impIsThis(obj))
                    {
                        call->gtCall.gtCallMoreFlags |= GTF_CALL_M_NONVIRT_SAME_THIS;
                    }
                }

                /* Store the "this" value in the call */

                call->gtFlags |= obj->gtFlags & GTF_GLOB_EFFECT;
                call->gtCall.gtCallObjp = obj;
            }

            //-------------------------------------------------------------------------
            // The "this" pointer for "newobj"

            if (opcode == CEE_NEWOBJ)
            {
                if (clsFlags & CORINFO_FLG_VAROBJSIZE)
                {
                    assert(!(clsFlags & CORINFO_FLG_ARRAY)); // arrays handled separately
                                                             // This is a 'new' of a variable sized object, wher
                    // the constructor is to return the object.  In this case
                    // the constructor claims to return VOID but we know it
                    // actually returns the new object
                    assert(callRetTyp == TYP_VOID);
                    callRetTyp   = TYP_REF;
                    call->gtType = TYP_REF;
                    compiler->impSpillSpecialSideEff();

                    compiler->impPushOnStack(call, typeInfo(TI_REF, clsHnd));
                }
                else
                {
                    if (clsFlags & CORINFO_FLG_DELEGATE)
                    {
                        // New inliner morph it in impImportCall.
                        // This will allow us to inline the call to the delegate constructor.
                        call = compiler->fgOptimizeDelegateConstructor(call->AsCall(), &exactContextHnd, ldftnToken);
                    }

                    if (!bIntrinsicImported)
                    {

#if defined(DEBUG) || defined(INLINE_DATA)

                        // Keep track of the raw IL offset of the call
                        call->gtCall.gtRawILOffset = rawILOffset;

#endif // defined(DEBUG) || defined(INLINE_DATA)

                        // Is it an inline candidate?
                        compiler->impMarkInlineCandidate(call, exactContextHnd, exactContextNeedsRuntimeLookup,
                                                         callInfo);
                    }

                    // append the call node.
                    compiler->impAppendTree(call, (unsigned)compiler->CHECK_SPILL_ALL, compiler->impCurStmtOffs);

                    // Now push the value of the 'new onto the stack

                    // This is a 'new' of a non-variable sized object.
                    // Append the new node (op1) to the statement list,
                    // and then push the local holding the value of this
                    // new instruction on the stack.

                    if (clsFlags & CORINFO_FLG_VALUECLASS)
                    {
                        assert(newobjThis->gtOper == GT_ADDR && newobjThis->gtOp.gtOp1->gtOper == GT_LCL_VAR);

                        unsigned tmp = newobjThis->gtOp.gtOp1->gtLclVarCommon.gtLclNum;
                        compiler->impPushOnStack(compiler->gtNewLclvNode(tmp, compiler->lvaGetRealType(tmp)),
                                                 compiler->verMakeTypeInfo(clsHnd).NormaliseForStack());
                    }
                    else
                    {
                        if (newobjThis->gtOper == GT_COMMA)
                        {
                            // In coreclr the callout can be inserted even if verification is disabled
                            // so we cannot rely on tiVerificationNeeded alone

                            // We must have inserted the callout. Get the real newobj.
                            newobjThis = newobjThis->gtOp.gtOp2;
                        }

                        assert(newobjThis->gtOper == GT_LCL_VAR);
                        compiler->impPushOnStack(compiler->gtNewLclvNode(newobjThis->gtLclVarCommon.gtLclNum, TYP_REF),
                                                 typeInfo(TI_REF, clsHnd));
                    }
                }
                return callRetTyp;
            }

        DONE:

            if (tailCall)
            {
                // This check cannot be performed for implicit tail calls for the reason
                // that impIsImplicitTailCallCandidate() is not checking whether return
                // types are compatible before marking a call node with PREFIX_TAILCALL_IMPLICIT.
                // As a result it is possible that in the following case, we find that
                // the type stack is non-empty if Callee() is considered for implicit
                // tail calling.
                //      int Caller(..) { .... void Callee(); ret val; ... }
                //
                // Note that we cannot check return type compatibility before ImpImportCall()
                // as we don't have required info or need to duplicate some of the logic of
                // ImpImportCall().
                //
                // For implicit tail calls, we perform this check after return types are
                // known to be compatible.
                if ((tailCall & PREFIX_TAILCALL_EXPLICIT) && (compiler->verCurrentState.esStackDepth != 0))
                {
                    BADCODE("Stack should be empty after tailcall");
                }

                // Note that we can not relax this condition with genActualType() as
                // the calling convention dictates that the caller of a function with
                // a small-typed return value is responsible for normalizing the return val

                if (canTailCall &&
                    !compiler->impTailCallRetTypeCompatible(compiler->info.compRetType,
                                                            compiler->info.compMethodInfo->args.retTypeClass,
                                                            callRetTyp, callInfo->sig.retTypeClass))
                {
                    canTailCall             = false;
                    szCanTailCallFailReason = "Return types are not tail call compatible";
                }

                // Stack empty check for implicit tail calls.
                if (canTailCall && (tailCall & PREFIX_TAILCALL_IMPLICIT) &&
                    (compiler->verCurrentState.esStackDepth != 0))
                {
#ifdef _TARGET_AMD64_
                    // JIT64 Compatibility:  Opportunistic tail call stack mismatch throws a VerificationException
                    // in JIT64, not an InvalidProgramException.
                    compiler->verRaiseVerifyExceptionIfNeeded(INDEBUG("Stack should be empty after tailcall")
                                                                  DEBUGARG(__FILE__) DEBUGARG(__LINE__));
#else  // _TARGET_64BIT_
                    BADCODE("Stack should be empty after tailcall");
#endif //!_TARGET_64BIT_
                }

                // assert(compCurBB is not a catch, finally or filter block);
                // assert(compCurBB is not a try block protected by a finally block);

                // Check for permission to tailcall
                bool explicitTailCall = (tailCall & PREFIX_TAILCALL_EXPLICIT) != 0;

                assert(!explicitTailCall || compiler->compCurBB->bbJumpKind == BBJ_RETURN);

                if (canTailCall)
                {
                    // True virtual or indirect calls, shouldn't pass in a callee handle.
                    CORINFO_METHOD_HANDLE exactCalleeHnd =
                        ((call->gtCall.gtCallType != CT_USER_FUNC) ||
                         ((call->gtFlags & GTF_CALL_VIRT_KIND_MASK) != GTF_CALL_NONVIRT))
                            ? nullptr
                            : methHnd;
                    GenTreePtr thisArg = call->gtCall.gtCallObjp;

                    if (compiler->info.compCompHnd->canTailCall(compiler->info.compMethodHnd, methHnd, exactCalleeHnd,
                                                                explicitTailCall))
                    {
                        canTailCall = true;
                        if (explicitTailCall)
                        {
                            // In case of explicit tail calls, mark it so that it is not considered
                            // for in-lining.
                            call->gtCall.gtCallMoreFlags |= GTF_CALL_M_EXPLICIT_TAILCALL;
#ifdef DEBUG
                            if (compiler->verbose)
                            {
                                printf("\nGTF_CALL_M_EXPLICIT_TAILCALL bit set for call ");
                                compiler->printTreeID(call);
                                printf("\n");
                            }
#endif
                        }
                        else
                        {
#if FEATURE_TAILCALL_OPT
                            // Must be an implicit tail call.
                            assert((tailCall & PREFIX_TAILCALL_IMPLICIT) != 0);

                            // It is possible that a call node is both an inline candidate and marked
                            // for opportunistic tail calling.  In-lining happens before morhphing of
                            // trees.  If in-lining of an in-line candidate gets aborted for whatever
                            // reason, it will survive to the morphing stage at which point it will be
                            // transformed into a tail call after performing additional checks.

                            call->gtCall.gtCallMoreFlags |= GTF_CALL_M_IMPLICIT_TAILCALL;
#ifdef DEBUG
                            if (compiler->verbose)
                            {
                                printf("\nGTF_CALL_M_IMPLICIT_TAILCALL bit set for call ");
                                compiler->printTreeID(call);
                                printf("\n");
                            }
#endif

#else //! FEATURE_TAILCALL_OPT
                            NYI("Implicit tail call prefix on a target which doesn't support opportunistic tail "
                                "calls");

#endif // FEATURE_TAILCALL_OPT
                        }

                        // we can't report success just yet...
                    }
                    else
                    {
                        canTailCall = false;
// canTailCall reported its reasons already
#ifdef DEBUG
                        if (compiler->verbose)
                        {
                            printf("\ninfo.compCompHnd->canTailCall returned false for call ");
                            compiler->printTreeID(call);
                            printf("\n");
                        }
#endif
                    }
                }
                else
                {
                    // If this assert fires it means that canTailCall was set to false without setting a reason!
                    assert(szCanTailCallFailReason != nullptr);

#ifdef DEBUG
                    if (compiler->verbose)
                    {
                        printf("\nRejecting %splicit tail call for call ", explicitTailCall ? "ex" : "im");
                        compiler->printTreeID(call);
                        printf(": %s\n", szCanTailCallFailReason);
                    }
#endif
                    compiler->info.compCompHnd->reportTailCallDecision(compiler->info.compMethodHnd, methHnd,
                                                                       explicitTailCall, TAILCALL_FAIL,
                                                                       szCanTailCallFailReason);
                }
            }

            // Note: we assume that small return types are already normalized by the managed callee
            // or by the pinvoke stub for calls to unmanaged code.

            if (!bIntrinsicImported)
            {
                //
                // Things needed to be checked when bIntrinsicImported is false.
                //

                assert(call->gtOper == GT_CALL);
                assert(sig != nullptr);

                // Tail calls require us to save the call site's sig info so we can obtain an argument
                // copying thunk from the EE later on.
                if (call->gtCall.callSig == nullptr)
                {
                    call->gtCall.callSig  = new (compiler, CMK_CorSig) CORINFO_SIG_INFO;
                    *call->gtCall.callSig = *sig;
                }

                if (compiler->compIsForInlining() && opcode == CEE_CALLVIRT)
                {
                    GenTreePtr callObj = call->gtCall.gtCallObjp;
                    assert(callObj != nullptr);

                    unsigned callKind = call->gtFlags & GTF_CALL_VIRT_KIND_MASK;

                    if (((callKind != GTF_CALL_NONVIRT) || (call->gtFlags & GTF_CALL_NULLCHECK)) &&
                        compiler
                            ->impInlineIsGuaranteedThisDerefBeforeAnySideEffects(call->gtCall.gtCallArgs, callObj,
                                                                                 compiler->impInlineInfo->inlArgInfo))
                    {
                        compiler->impInlineInfo->thisDereferencedFirst = true;
                    }
                }

#if defined(DEBUG) || defined(INLINE_DATA)

                // Keep track of the raw IL offset of the call
                call->gtCall.gtRawILOffset = rawILOffset;

#endif // defined(DEBUG) || defined(INLINE_DATA)

                // Is it an inline candidate?
                compiler->impMarkInlineCandidate(call, exactContextHnd, exactContextNeedsRuntimeLookup, callInfo);
            }
        }
        // Push or append the result of the call
        if (callRetTyp == TYP_VOID)
        {
            if (opcode == CEE_NEWOBJ)
            {
                // we actually did push something, so don't spill the thing we just pushed.
                assert(compiler->verCurrentState.esStackDepth > 0);
                compiler->impAppendTree(call, compiler->verCurrentState.esStackDepth - 1, compiler->impCurStmtOffs);
            }
            else
            {
                compiler->impAppendTree(call, (unsigned)compiler->CHECK_SPILL_ALL, compiler->impCurStmtOffs);
            }
        }
        else
        {
            compiler->impSpillSpecialSideEff();

            if (clsFlags & CORINFO_FLG_ARRAY)
            {
                compiler->eeGetCallSiteSig(pResolvedToken->token, pResolvedToken->tokenScope,
                                           pResolvedToken->tokenContext, sig);
            }

            // Find the return type used for verification by interpreting the method signature.
            // NB: we are clobbering the already established sig.
            if (compiler->tiVerificationNeeded)
            {
                // Actually, we never get the sig for the original method.
                sig = &(callInfo->verSig);
            }

            typeInfo tiRetVal = compiler->verMakeTypeInfo(sig->retType, sig->retTypeClass);
            tiRetVal.NormaliseForStack();

            // The CEE_READONLY prefix modifies the verification semantics of an Address
            // operation on an array type.
            if ((clsFlags & CORINFO_FLG_ARRAY) && readonlyCall && tiRetVal.IsByRef())
            {
                tiRetVal.SetIsReadonlyByRef();
            }

            if (compiler->tiVerificationNeeded)
            {
                // We assume all calls return permanent home byrefs. If they
                // didn't they wouldn't be verifiable. This is also covering
                // the Address() helper for multidimensional arrays.
                if (tiRetVal.IsByRef())
                {
                    tiRetVal.SetIsPermanentHomeByRef();
                }
            }

            if (call->IsCall())
            {
                // Sometimes "call" is not a GT_CALL (if we imported an intrinsic that didn't turn into a call)

                bool fatPointerCandidate = call->AsCall()->IsFatPointerCandidate();
                if (varTypeIsStruct(callRetTyp))
                {
                    call = compiler->impFixupCallStructReturn(call->AsCall(), sig->retTypeClass);
                }

                if ((call->gtFlags & GTF_CALL_INLINE_CANDIDATE) != 0)
                {
                    assert(compiler->opts.OptEnabled(CLFLG_INLINING));
                    assert(!fatPointerCandidate); // We should not try to inline calli.

                    // Make the call its own tree (spill the stack if needed).
                    compiler->impAppendTree(call, (unsigned)compiler->CHECK_SPILL_ALL, compiler->impCurStmtOffs);

                    // TODO: Still using the widened type.
                    call = compiler->gtNewInlineCandidateReturnExpr(call, genActualType(callRetTyp));
                }
                else
                {
                    if (fatPointerCandidate)
                    {
                        // fatPointer candidates should be in statements of the form call() or var = call().
                        // Such form allows to find statements with fat calls without walking through whole trees
                        // and removes problems with cutting trees.
                        assert(!bIntrinsicImported);
                        assert(compiler->IsTargetAbi(CORINFO_CORERT_ABI));
                        if (call->OperGet() != GT_LCL_VAR) // can be already converted by impFixupCallStructReturn.
                        {
                            unsigned   calliSlot  = compiler->lvaGrabTemp(true DEBUGARG("calli"));
                            LclVarDsc* varDsc     = &compiler->lvaTable[calliSlot];
                            varDsc->lvVerTypeInfo = tiRetVal;
                            compiler->impAssignTempGen(calliSlot, call, tiRetVal.GetClassHandle(),
                                                       (unsigned)compiler->CHECK_SPILL_NONE);
                            // impAssignTempGen can change src arg list and return type for call that returns struct.
                            var_types type = genActualType(compiler->lvaTable[calliSlot].TypeGet());
                            call           = compiler->gtNewLclvNode(calliSlot, type);
                        }
                    }

                    // For non-candidates we must also spill, since we
                    // might have locals live on the eval stack that this
                    // call can modify.
                    //
                    // Suppress this for certain well-known call targets
                    // that we know won't modify locals, eg calls that are
                    // recognized in gtCanOptimizeTypeEquality. Otherwise
                    // we may break key fragile pattern matches later on.
                    bool spillStack = true;
                    if (call->IsCall())
                    {
                        GenTreeCall* callNode = call->AsCall();
                        if ((callNode->gtCallType == CT_HELPER) &&
                            compiler->gtIsTypeHandleToRuntimeTypeHelper(callNode))
                        {
                            spillStack = false;
                        }
                        else if ((callNode->gtCallMoreFlags & GTF_CALL_M_SPECIAL_INTRINSIC) != 0)
                        {
                            spillStack = false;
                        }
                    }

                    if (spillStack)
                    {
                        compiler->impSpillSideEffects(true,
                                                      compiler->CHECK_SPILL_ALL DEBUGARG("non-inline candidate call"));
                    }
                }
            }

            if (!bIntrinsicImported)
            {
                //-------------------------------------------------------------------------
                //
                /* If the call is of a small type and the callee is managed, the callee will normalize the result
                before returning.
                However, we need to normalize small type values returned by unmanaged
                functions (pinvoke). The pinvoke stub does the normalization, but we need to do it here
                if we use the shorter inlined pinvoke stub. */

                if (checkForSmallType() && varTypeIsIntegral(callRetTyp) &&
                    genTypeSize(callRetTyp) < genTypeSize(TYP_INT))
                {
                    call = compiler->gtNewCastNode(genActualType(callRetTyp), call, callRetTyp);
                }
            }

            compiler->impPushOnStack(call, tiRetVal);
        }

        return callRetTyp;
    }

private:
    //------------------------------------------------------------------------
    // checkForSmallType: check does the current importing call need
    // a check for small return type.
    //
    //
    // Return Value:
    //    true if it does, false instead.
    bool checkForSmallType()
    {
        // We only need to cast the return value of pinvoke inlined calls that return small types

        // TODO-AMD64-Cleanup: Remove this when we stop interoperating with JIT64, or if we decide to stop
        // widening everything! CoreCLR does not support JIT64 interoperation so no need to widen there.
        // The existing x64 JIT doesn't bother widening all types to int, so we have to assume for
        // the time being that the callee might be compiled by the other JIT and thus the return
        // value will need to be widened by us (or not widened at all...)

        if (compiler->opts.IsJit64Compat())
        {
            return true;
        }

        // ReadyToRun code sticks with default calling convention that does not widen small return types.

        if (compiler->opts.IsReadyToRun())
        {
            return true;
        }
        //-------------------------------------------------------------------------
        //
        /* If the call is of a small type and the callee is managed, the callee will normalize the result
        before returning.
        However, we need to normalize small type values returned by unmanaged
        functions (pinvoke). The pinvoke stub does the normalization, but we need to do it here
        if we use the shorter inlined pinvoke stub. */
        if ((call->gtFlags & GTF_CALL_UNMANAGED) != 0)
        {
            return true;
        }
        return false;
    }

private:
    Compiler*               compiler;
    var_types               callRetTyp;
    CORINFO_SIG_INFO*       sig;
    CORINFO_METHOD_HANDLE   methHnd;
    CORINFO_CLASS_HANDLE    clsHnd;
    unsigned                clsFlags;
    unsigned                mflags;
    unsigned                argFlags;
    GenTreePtr              call;
    GenTreeArgList*         args;
    CORINFO_THIS_TRANSFORM  constraintCallThisTransform;
    CORINFO_CONTEXT_HANDLE  exactContextHnd;
    bool                    exactContextNeedsRuntimeLookup;
    bool                    canTailCall;
    const char*             szCanTailCallFailReason;
    CORINFO_RESOLVED_TOKEN* ldftnToken;
    bool                    bIntrinsicImported;
    bool                    readonlyCall;

    CORINFO_SIG_INFO calliSig;
    GenTreeArgList*  extraArg;
};

//------------------------------------------------------------------------
// impImportCall: import a call-inspiring opcode
//
// Arguments:
//    opcode                    - opcode that inspires the call
//    pResolvedToken            - resolved token for the call target
//    pConstrainedResolvedToken - resolved constraint token (or nullptr)
//    newObjThis                - tree for this pointer or uninitalized newobj temp (or nullptr)
//    prefixFlags               - IL prefix flags for the call
//    callInfo                  - EE supplied info for the call
//    rawILOffset               - IL offset of the opcode
//
// Returns:
//    Type of the call's return value.
//
// Notes:
//    opcode can be CEE_CALL, CEE_CALLI, CEE_CALLVIRT, or CEE_NEWOBJ.
//
//    For CEE_NEWOBJ, newobjThis should be the temp grabbed for the allocated
//    uninitalized object.

#ifdef _PREFAST_
#pragma warning(push)
#pragma warning(disable : 21000) // Suppress PREFast warning about overly large function
#endif

var_types Compiler::impImportCall(OPCODE                  opcode,
                                  CORINFO_RESOLVED_TOKEN* pResolvedToken,
                                  CORINFO_RESOLVED_TOKEN* pConstrainedResolvedToken,
                                  GenTreePtr              newobjThis,
                                  int                     prefixFlags,
                                  CORINFO_CALL_INFO*      callInfo,
                                  IL_OFFSET               rawILOffset)
{
    CallImporter callImporter(this);
    return callImporter.importCall(opcode, pResolvedToken, pConstrainedResolvedToken, newobjThis, prefixFlags, callInfo,
                                   rawILOffset);
}
#ifdef _PREFAST_
#pragma warning(pop)
#endif
