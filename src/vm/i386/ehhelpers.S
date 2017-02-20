// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

.intel_syntax noprefix
#include "unixasmmacros.inc"
#include "asmconstants.h"

// DWORD_PTR STDCALL CallEHFunclet(Object *pThrowable, UINT_PTR pFuncletToInvoke, UINT_PTR *pFirstNonVolReg, UINT_PTR *pFuncletCallerSP);
NESTED_ENTRY CallEHFunclet, _TEXT, NoHandler

    PROLOG_BEG
    PROLOG_PUSH ebx
    PROLOG_PUSH esi
    PROLOG_PUSH edi
    PROLOG_END

    // On entry:
    //
    // [ebp+ 8] = throwable
    // [ebp+12] = PC to invoke
    // [ebp+16] = address of EDI register in CONTEXT record // used to restore the non-volatile registers of CrawlFrame
    // [ebp+20] = address of the location where the SP of funclet's caller (i.e. this helper) should be saved.
    //

    // Save the SP of this function
    mov     eax, [ebp + 20]
    mov     [eax], esp
    // Restore non-volatiles registers (except ebp)
    mov     eax, [ebp + 16]
    mov     edi, [eax]
    mov     esi, [eax +  4]
    mov     ebx, [eax +  8]
    // Pass FP to funclet
	// We don't restore EBP here or it would affect unwinding this helper
    mov     ecx, [eax + 24]
    // Pass throwable object to funclet
    mov     eax, [ebp +  8]
    // Invoke the funclet
    mov     edx, [ebp + 12]
    call    edx

    EPILOG_BEG
    EPILOG_POP edi
    EPILOG_POP esi
    EPILOG_POP ebx
    EPILOG_END

    ret     16

NESTED_END CallEHFunclet, _TEXT

// DWORD_PTR STDCALL CallEHFilterFunclet(Object *pThrowable, TADDR CallerSP, UINT_PTR pFuncletToInvoke, UINT_PTR *pFuncletCallerSP);
NESTED_ENTRY CallEHFilterFunclet, _TEXT, NoHandler

    PROLOG_BEG
    PROLOG_PUSH ebx
    PROLOG_PUSH esi
    PROLOG_PUSH edi
    PROLOG_END

    // On entry:
    //
    // [ebp+ 8] = throwable
    // [ebp+12] = FP to restore
    // [ebp+16] = PC to invoke
    // [ebp+20] = address of the location where the SP of funclet's caller (i.e. this helper) should be saved.
    //

    // Save the SP of this function
    mov     eax, [ebp + 20]
    mov     [eax], esp
    // Pass throwable object to funclet
    mov     eax, [ebp +  8]
    // Pass FP to funclet
    mov     ecx, [ebp + 12]
    // Invoke the funclet
    mov     edx, [ebp + 16]
    call    edx

    EPILOG_BEG
    EPILOG_POP edi
    EPILOG_POP esi
    EPILOG_POP ebx
    EPILOG_END

    ret     16

NESTED_END CallEHFunclet, _TEXT

