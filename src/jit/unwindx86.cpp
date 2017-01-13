// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                              UnwindInfo                                   XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#ifndef _TARGET_X86_
#error "This should be included only for x86"
#endif // _TARGET_X86_

void Compiler::unwindBegProlog()
{
#if FEATURE_EH_FUNCLETS
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    // There is only one prolog for a function/funclet, and it comes first. So now is
    // a good time to initialize all the unwind data structures.

    unwindGetFuncLocations(func, true, &func->startLoc, &func->endLoc);

    if (fgFirstColdBlock != nullptr)
    {
        unwindGetFuncLocations(func, false, &func->coldStartLoc, &func->coldEndLoc);
    }
#endif // FEATURE_EH_FUNCLETS
}

void Compiler::unwindEndProlog()
{
}

void Compiler::unwindBegEpilog()
{
}

void Compiler::unwindEndEpilog()
{
}

void Compiler::unwindPush(regNumber reg)
{
}

void Compiler::unwindAllocStack(unsigned size)
{
}

void Compiler::unwindSetFrameReg(regNumber reg, unsigned offset)
{
}

void Compiler::unwindSaveReg(regNumber reg, unsigned offset)
{
}

//------------------------------------------------------------------------
// Compiler::unwindReserve: Ask the VM to reserve space for the unwind information
// for the function and all its funclets. Called once, just before asking the VM
// for memory and emitting the generated code. Calls unwindReserveFunc() to handle
// the main function and each of the funclets, in turn.
//
void Compiler::unwindReserve()
{
#if FEATURE_EH_FUNCLETS
    assert(!compGeneratingProlog);
    assert(!compGeneratingEpilog);

    assert(compFuncInfoCount > 0);
    for (unsigned funcIdx = 0; funcIdx < compFuncInfoCount; funcIdx++)
    {
        unwindReserveFunc(funGetFunc(funcIdx));
    }
#endif
}

//------------------------------------------------------------------------
// Compiler::unwindEmit: Report all the unwind information to the VM.
//
// Arguments:
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//
void Compiler::unwindEmit(void* pHotCode, void* pColdCode)
{
    assert(!compGeneratingProlog);
    assert(!compGeneratingEpilog);

    assert(compFuncInfoCount > 0);
    for (unsigned funcIdx = 0; funcIdx < compFuncInfoCount; funcIdx++)
    {
        unwindEmitFunc(funGetFunc(funcIdx), pHotCode, pColdCode);
    }
}

//------------------------------------------------------------------------
// Compiler::unwindEmitFunc: Report the unwind information to the VM for a
// given main function or funclet. Reports the hot section, then the cold
// section if necessary.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//
void Compiler::unwindEmitFunc(FuncInfoDsc* func, void* pHotCode, void* pColdCode)
{
    // Verify that the JIT enum is in sync with the JIT-EE interface enum
    static_assert_no_msg(FUNC_ROOT == (FuncKind)CORJIT_FUNC_ROOT);
    static_assert_no_msg(FUNC_HANDLER == (FuncKind)CORJIT_FUNC_HANDLER);
    static_assert_no_msg(FUNC_FILTER == (FuncKind)CORJIT_FUNC_FILTER);

    unwindEmitFuncHelper(func, pHotCode, pColdCode, true);

    if (pColdCode != nullptr)
    {
        unwindEmitFuncHelper(func, pHotCode, pColdCode, false);
    }
}

#if FEATURE_EH_FUNCLETS
//------------------------------------------------------------------------
// Compiler::unwindReserveFunc: Reserve the unwind information from the VM for a
// given main function or funclet.
//
// Arguments:
//    func - The main function or funclet to reserve unwind info for.
//
void Compiler::unwindReserveFunc(FuncInfoDsc* func)
{
    unwindReserveFuncHelper(func, true);

    if (fgFirstColdBlock != nullptr)
    {
        unwindReserveFuncHelper(func, false);
    }
}

//------------------------------------------------------------------------
// Compiler::unwindReserveFuncHelper: Reserve the unwind information from the VM for a
// given main function or funclet, for either the hot or the cold section.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    isHotCode - 'true' to reserve the hot section, 'false' to reserve the cold section.
//
void Compiler::unwindReserveFuncHelper(FuncInfoDsc* func, bool isHotCode)
{
    BOOL isFunclet  = (func->funKind != FUNC_ROOT);
    BOOL isColdCode = isHotCode ? FALSE : TRUE;

    eeReserveUnwindInfo(isFunclet, isColdCode, 0);
}

//------------------------------------------------------------------------
// Compiler::unwindEmitFuncHelper: Report the unwind information to the VM for a
// given main function or funclet, for either the hot or cold section.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//                Ignored if 'isHotCode' is true.
//    isHotCode - 'true' to report the hot section, 'false' to report the cold section.
//
void Compiler::unwindEmitFuncHelper(FuncInfoDsc* func, void* pHotCode, void* pColdCode, bool isHotCode)
{
    UNATIVE_OFFSET startOffset;
    UNATIVE_OFFSET endOffset;

    if (isHotCode)
    {
        if (func->startLoc == nullptr)
        {
            startOffset = 0;
        }
        else
        {
            startOffset = func->startLoc->CodeOffset(genEmitter);
        }

        if (func->endLoc == nullptr)
        {
            endOffset = info.compNativeCodeSize;
        }
        else
        {
            endOffset = func->endLoc->CodeOffset(genEmitter);
        }
    }
    else
    {
        assert(fgFirstColdBlock != nullptr);
        assert(func->funKind == FUNC_ROOT); // No splitting of funclets.

        if (func->coldStartLoc == nullptr)
        {
            startOffset = 0;
        }
        else
        {
            startOffset = func->coldStartLoc->CodeOffset(genEmitter);
        }

        if (func->coldEndLoc == nullptr)
        {
            endOffset = info.compNativeCodeSize;
        }
        else
        {
            endOffset = func->coldEndLoc->CodeOffset(genEmitter);
        }
    }

    // Adjust for cold or hot code:
    // 1. The VM doesn't want the cold code pointer unless this is cold code.
    // 2. The startOffset and endOffset need to be from the base of the hot section for hot code
    //    and from the base of the cold section for cold code

    if (isHotCode)
    {
        assert(endOffset <= info.compTotalHotCodeSize);
        pColdCode = nullptr;
    }
    else
    {
        assert(startOffset >= info.compTotalHotCodeSize);
        startOffset -= info.compTotalHotCodeSize;
        endOffset -= info.compTotalHotCodeSize;
    }

    eeAllocUnwindInfo((BYTE*)pHotCode, (BYTE*)pColdCode, startOffset, endOffset, 0, NULL,
                      (CorJitFuncKind)func->funKind);
}
#endif // FEATURE_EH_FUNCLETS
