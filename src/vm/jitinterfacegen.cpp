// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ===========================================================================
// File: JITinterfaceGen.CPP
//
// This contains the AMD64 version of InitJITHelpers1().
//
// ===========================================================================


#include "common.h"
#include "clrtypes.h"
#include "jitinterface.h"
#include "eeconfig.h"
#include "excep.h"
#include "comdelegate.h"
#include "field.h"
#include "ecall.h"

#ifdef _WIN64

// These are the fastest(?) versions of JIT helpers as they have the code to GetThread patched into them
// that does not make a call.
EXTERN_C Object* JIT_TrialAllocSFastMP_InlineGetThread(CORINFO_CLASS_HANDLE typeHnd_);
EXTERN_C Object* JIT_BoxFastMP_InlineGetThread (CORINFO_CLASS_HANDLE type, void* unboxedData);
EXTERN_C Object* AllocateStringFastMP_InlineGetThread (CLR_I4 cch);
EXTERN_C Object* JIT_NewArr1OBJ_MP_InlineGetThread (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);
EXTERN_C Object* JIT_NewArr1VC_MP_InlineGetThread (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);

// This next set is the fast version that invoke GetThread but is still faster than the VM implementation (i.e.
// the "slow" versions).
EXTERN_C Object* JIT_TrialAllocSFastMP(CORINFO_CLASS_HANDLE typeHnd_);
EXTERN_C Object* JIT_TrialAllocSFastSP(CORINFO_CLASS_HANDLE typeHnd_);
EXTERN_C Object* JIT_BoxFastMP (CORINFO_CLASS_HANDLE type, void* unboxedData);
EXTERN_C Object* JIT_BoxFastUP (CORINFO_CLASS_HANDLE type, void* unboxedData);
EXTERN_C Object* AllocateStringFastMP (CLR_I4 cch);
EXTERN_C Object* AllocateStringFastUP (CLR_I4 cch);

EXTERN_C Object* JIT_NewArr1OBJ_MP (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);
EXTERN_C Object* JIT_NewArr1OBJ_UP (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);
EXTERN_C Object* JIT_NewArr1VC_MP (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);
EXTERN_C Object* JIT_NewArr1VC_UP (CORINFO_CLASS_HANDLE arrayMT, INT_PTR size);

#ifdef _TARGET_AMD64_
extern WriteBarrierManager g_WriteBarrierManager;
#endif // _TARGET_AMD64_

#endif // _WIN64

/*********************************************************************/ 
// Initialize the part of the JIT helpers that require very little of
// EE infrastructure to be in place.
/*********************************************************************/
#ifndef _TARGET_X86_

void InitJITHelpers1()
{
    STANDARD_VM_CONTRACT;

    _ASSERTE(g_SystemInfo.dwNumberOfProcessors != 0);

#if defined(_TARGET_AMD64_)

    g_WriteBarrierManager.Initialize();

    // Allocation helpers, faster but non-logging
    if (!((TrackAllocationsEnabled()) || 
        (LoggingOn(LF_GCALLOC, LL_INFO10))
#ifdef _DEBUG 
        || (g_pConfig->ShouldInjectFault(INJECTFAULT_GCHEAP) != 0)
#endif // _DEBUG
        ))
    {
#ifdef FEATURE_PAL
        SetJitHelperFunction(CORINFO_HELP_NEWSFAST, JIT_NewS_MP_FastPortable);
        SetJitHelperFunction(CORINFO_HELP_NEWSFAST_ALIGN8, JIT_NewS_MP_FastPortable);
        SetJitHelperFunction(CORINFO_HELP_NEWARR_1_VC, JIT_NewArr1VC_MP_FastPortable);
        SetJitHelperFunction(CORINFO_HELP_NEWARR_1_OBJ, JIT_NewArr1OBJ_MP_FastPortable);

        ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateString_MP_FastPortable), ECall::FastAllocateString);
#ifdef FEATURE_UTF8STRING
        ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateUtf8String_MP_FastPortable), ECall::FastAllocateUtf8String);
#endif // FEATURE_UTF8STRING
#else // FEATURE_PAL
        // if (multi-proc || server GC)
        if (GCHeapUtilities::UseThreadAllocationContexts())
        {
            SetJitHelperFunction(CORINFO_HELP_NEWSFAST, JIT_TrialAllocSFastMP_InlineGetThread);
            SetJitHelperFunction(CORINFO_HELP_NEWSFAST_ALIGN8, JIT_TrialAllocSFastMP_InlineGetThread);
            SetJitHelperFunction(CORINFO_HELP_BOX, JIT_BoxFastMP_InlineGetThread);
            SetJitHelperFunction(CORINFO_HELP_NEWARR_1_VC, JIT_NewArr1VC_MP_InlineGetThread);
            SetJitHelperFunction(CORINFO_HELP_NEWARR_1_OBJ, JIT_NewArr1OBJ_MP_InlineGetThread);

            ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateStringFastMP_InlineGetThread), ECall::FastAllocateString);
#ifdef FEATURE_UTF8STRING
            ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateUtf8String_MP_FastPortable), ECall::FastAllocateUtf8String);
#endif // FEATURE_UTF8STRING
        }
        else
        {
            // Replace the 1p slow allocation helpers with faster version
            //
            // When we're running Workstation GC on a single proc box we don't have 
            // InlineGetThread versions because there is no need to call GetThread
            SetJitHelperFunction(CORINFO_HELP_NEWSFAST, JIT_TrialAllocSFastSP);
            SetJitHelperFunction(CORINFO_HELP_NEWSFAST_ALIGN8, JIT_TrialAllocSFastSP);
            SetJitHelperFunction(CORINFO_HELP_BOX, JIT_BoxFastUP);
            SetJitHelperFunction(CORINFO_HELP_NEWARR_1_VC, JIT_NewArr1VC_UP);
            SetJitHelperFunction(CORINFO_HELP_NEWARR_1_OBJ, JIT_NewArr1OBJ_UP);

            ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateStringFastUP), ECall::FastAllocateString);
#ifdef FEATURE_UTF8STRING
            ECall::DynamicallyAssignFCallImpl(GetEEFuncEntryPoint(AllocateUtf8String_MP_FastPortable), ECall::FastAllocateUtf8String);
#endif // FEATURE_UTF8STRING
        }
#endif // FEATURE_PAL
    }
#endif // _TARGET_AMD64_
}

#endif // !_TARGET_X86_
