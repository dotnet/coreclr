; Licensed to the .NET Foundation under one or more agreements.
; The .NET Foundation licenses this file to you under the MIT license.
; See the LICENSE file in the project root for more information.

; ***********************************************************************
; File: PInvokeStubs.asm
;
; ***********************************************************************
;
;  *** NOTE:  If you make changes to this file, propagate the changes to
;             PInvokeStubs.s in this directory                            

; This contains JITinterface routines that are 100% x86 assembly

        .586
        .model  flat

        include asmconstants.inc

        option  casemap:none
        .code
        
extern _s_gsCookie:DWORD
extern ??_7InlinedCallFrame@@6B@:DWORD
extern _g_TrapReturningThreads:DWORD

extern _RareDisablePreemptiveGCHelper@4:proc
EXTERN _GetThread@0:proc

.686P
.XMM

;
; in:
; InlinedCallFrame (ecx) = pointer to the InlinedCallFrame data, including the GS cookie slot (GS cookie right 
;                          before actual InlinedCallFrame data)
;
;
_JIT_PInvokeBegin@4 PROC public
        
        mov             eax, dword ptr [_s_gsCookie]
        mov             dword ptr [ecx], eax
        add             ecx, SIZEOF_GSCookie

        ;; set first slot to the value of InlinedCallFrame::`vftable' (checked by runtime code)
        lea             eax,[??_7InlinedCallFrame@@6B@]
        mov             dword ptr [ecx], eax

        mov             dword ptr [ecx + InlinedCallFrame__m_Datum], 0

        
        mov             eax, esp
        add             eax, 4
        mov             dword ptr [ecx + InlinedCallFrame__m_pCallSiteSP], eax
        mov             dword ptr [ecx + InlinedCallFrame__m_pCalleeSavedFP], ebp

        mov             eax, [esp]
        mov             dword ptr [ecx + InlinedCallFrame__m_pCallerReturnAddress], eax

        push            ecx             ; Save pFrame pointer on stack
        call            _GetThread@0    ; eax = Thread*
        pop             ecx

        ;; pFrame->m_Next = pThread->m_pFrame;
        mov             edx, dword ptr [eax + Thread_m_pFrame]
        mov             dword ptr [ecx + Frame__m_Next], edx

        ;; pThread->m_pFrame = pFrame;
        mov             dword ptr [eax + Thread_m_pFrame], ecx

        ;; pThread->m_fPreemptiveGCDisabled = 0
        mov             dword ptr [eax + Thread_m_fPreemptiveGCDisabled], 0

        ret

_JIT_PInvokeBegin@4 ENDP

;
; in:
; InlinedCallFrame (ecx) = pointer to the InlinedCallFrame data, including the GS cookie slot (GS cookie right 
;                          before actual InlinedCallFrame data)
;
;
_JIT_PInvokeEnd@4 PROC public

        add             ecx, SIZEOF_GSCookie

        push            ecx             ; Save pFrame pointer on stack
        call            _GetThread@0    ; eax = Thread*
        pop             ecx

        ;; pThread->m_fPreemptiveGCDisabled = 1
        mov             dword ptr [eax + Thread_m_fPreemptiveGCDisabled], 1

        ;; Check return trap
        cmp             [_g_TrapReturningThreads], 0
        jz              DoNothing

        push            ecx             ; Save pFrame pointer
        push            eax             ; Save pThread pointer

        ; Call GC helper
        push            eax             ; pThread as argument to the call
        call            _RareDisablePreemptiveGCHelper@4

        pop             eax
        pop             ecx

DoNothing:

        ;; pThread->m_pFrame = pFrame->m_Next;
        mov             edx, dword ptr [ecx + Frame__m_Next]
        mov             dword ptr [eax + Thread_m_pFrame], edx

        mov             dword ptr [ecx + InlinedCallFrame__m_pCallerReturnAddress], 0

        ret

_JIT_PInvokeEnd@4 ENDP

        end
