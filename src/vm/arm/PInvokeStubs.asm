; Licensed to the .NET Foundation under one or more agreements.
; The .NET Foundation licenses this file to you under the MIT license.
; See the LICENSE file in the project root for more information.

;; ==++==
;;

;;
;; ==--==
#include "ksarm.h"

#include "asmconstants.h"

#include "asmmacros.h"


    IMPORT VarargPInvokeStubWorker
    IMPORT GenericPInvokeCalliStubWorker
    IMPORT JIT_RareDisableHelper

    IMPORT s_gsCookie
    IMPORT g_TrapReturningThreads

    SETALIAS InlinedCallFrame_vftable, ??_7InlinedCallFrame@@6B@
    IMPORT $InlinedCallFrame_vftable

; ------------------------------------------------------------------
; Macro to generate PInvoke Stubs.
; $__PInvokeStubFuncName : function which calls the actual stub obtained from VASigCookie
; $__PInvokeGenStubFuncName : function which generates the IL stubs for PInvoke
; 
; Params :-
; $FuncPrefix : prefix of the function name for the stub
;                     Eg. VarargPinvoke, GenericPInvokeCalli
; $VASigCookieReg : register which contains the VASigCookie
; $SaveFPArgs : "Yes" or "No" . For varidic functions FP Args are not present in FP regs 
;                        So need not save FP Args registers for vararg Pinvoke
        MACRO

        PINVOKE_STUB $FuncPrefix,$VASigCookieReg,$SaveFPArgs

        GBLS __PInvokeStubFuncName
        GBLS __PInvokeGenStubFuncName
        GBLS __PInvokeStubWorkerName

        IF "$FuncPrefix" == "GenericPInvokeCalli"
__PInvokeStubFuncName SETS "$FuncPrefix":CC:"Helper"
        ELSE
__PInvokeStubFuncName SETS "$FuncPrefix":CC:"Stub"
        ENDIF
__PInvokeGenStubFuncName SETS "$FuncPrefix":CC:"GenILStub"
__PInvokeStubWorkerName SETS "$FuncPrefix":CC:"StubWorker"

       IF "$VASigCookieReg" == "r1"
__PInvokeStubFuncName SETS "$__PInvokeStubFuncName":CC:"_RetBuffArg"
__PInvokeGenStubFuncName SETS "$__PInvokeGenStubFuncName":CC:"_RetBuffArg"
        ENDIF

        NESTED_ENTRY $__PInvokeStubFuncName

        ; save reg value before using the reg
        PROLOG_PUSH         {$VASigCookieReg}

        ; get the stub
        ldr                 $VASigCookieReg, [$VASigCookieReg,#VASigCookie__pNDirectILStub]

        ; if null goto stub generation
        cbz                 $VASigCookieReg, %0

        EPILOG_STACK_FREE   4

        EPILOG_BRANCH_REG   $VASigCookieReg

0

        EPILOG_POP          {$VASigCookieReg}
        EPILOG_BRANCH       $__PInvokeGenStubFuncName

        NESTED_END

        
        NESTED_ENTRY $__PInvokeGenStubFuncName

        PROLOG_WITH_TRANSITION_BLOCK 0, $SaveFPArgs

        ; r2 = UnmanagedTarget\ MethodDesc
        mov                 r2, r12

        ; r1 = VaSigCookie
        IF "$VASigCookieReg" != "r1"
        mov                 r1, $VASigCookieReg
        ENDIF

        ; r0 =  pTransitionBlock
        add                 r0, sp, #__PWTB_TransitionBlock     

        ; save hidden arg
        mov                 r4, r12

        bl                  $__PInvokeStubWorkerName

        ; restore hidden arg (method desc or unmanaged target)
        mov                 r12, r4

        EPILOG_WITH_TRANSITION_BLOCK_TAILCALL

        EPILOG_BRANCH   $__PInvokeStubFuncName
     
        NESTED_END
        
        MEND

; ------------------------------------------------------------------
;
        MACRO

        REMOVE_FRAME_FROM_THREAD $frameReg, $threadReg, $trashReg

        ;; pThread->m_pFrame = pFrame->m_Next;
        ldr     $trashReg, [$frameReg, #Frame__m_Next]
        str     $trashReg, [$threadReg, #Thread_m_pFrame]

        mov     $trashReg, 0
        str     $trashReg, [$frameReg, #InlinedCallFrame__m_pCallerReturnAddress]

        MEND


    TEXTAREA
; ------------------------------------------------------------------
; JIT_PInvokeBegin helper
;
; in:
; r0 = InlinedCallFrame*: pointer to the InlinedCallFrame data, including the GS cookie slot (GS cookie right 
;                         before actual InlinedCallFrame data)
; 
        LEAF_ENTRY JIT_PInvokeBegin

            ldr     r1, =s_gsCookie
            ldr     r1, [r1]
            str     r1, [r0]
            add     r0, r0, SIZEOF__GSCookie
                        
            ;; r0 = pFrame
            
            ;; set first slot to the value of InlinedCallFrame::`vftable' (checked by runtime code)
            ldr     r1, =$InlinedCallFrame_vftable
            str     r1, [r0]

            mov     r1, 0
            str     r1, [r0, #InlinedCallFrame__m_Datum]
        
            str     sp, [r0, #InlinedCallFrame__m_pCallSiteSP]
            str     r11, [r0, #InlinedCallFrame__m_pCalleeSavedFP]
            str     lr, [r0, #InlinedCallFrame__m_pCallerReturnAddress]

            ;; r1 = GetThread(), TRASHES r2
            INLINE_GETTHREAD r1, r2

            ;; pFrame->m_Next = pThread->m_pFrame;
            ldr     r2, [r1, #Thread_m_pFrame]
            str     r2, [r0, #Frame__m_Next]

            ;; pThread->m_pFrame = pFrame;
            str     r0, [r1, #Thread_m_pFrame]

            ;; pThread->m_fPreemptiveGCDisabled = 0
            mov     r2, 0
            str     r2, [r1, #Thread_m_fPreemptiveGCDisabled]

            bx      lr
            
        LEAF_END

; ------------------------------------------------------------------
; JIT_PInvokeEnd helper
;
; in:
; r0 = InlinedCallFrame*
; 
        LEAF_ENTRY JIT_PInvokeEnd

            add     r0, r0, SIZEOF__GSCookie

            ;; r1 = GetThread(), TRASHES r2
            INLINE_GETTHREAD r1, r2

            ;; r0 = pFrame
            ;; r1 = pThread
            
            ;; pThread->m_fPreemptiveGCDisabled = 1
            mov     r2, 1
            str     r2, [r1, #Thread_m_fPreemptiveGCDisabled]

            ;; Check return trap
            ldr     r2, =g_TrapReturningThreads
            ldr     r2, [r2]
            cbnz    r2, JIT_PInvokeEndRarePath

            ;; pThread->m_pFrame = pFrame->m_Next
            REMOVE_FRAME_FROM_THREAD r0, r1, r2

            bx      lr
        
        LEAF_END
        
; ------------------------------------------------------------------
; JIT_PInvokeEndRarePath helper
;
; in:
; r0 = InlinedCallFrame*
; r1 = Thread*
; 
        NESTED_ENTRY JIT_PInvokeEndRarePath

            PROLOG_PUSH         {r4,r5,r7,lr}
            PROLOG_STACK_SAVE   r7

            ;; Save thread and frame in callee saved registers
            mov         r4, r0
            mov         r5, r1

            ;; Call GC helper
            bl          JIT_RareDisableHelper
            
            ;; pThread->m_pFrame = pFrame->m_Next
            REMOVE_FRAME_FROM_THREAD r4, r5, r0
        
            EPILOG_STACK_RESTORE    r7
            EPILOG_POP              {r4,r5,r7,lr}
            EPILOG_RETURN
            
        NESTED_END

        INLINE_GETTHREAD_CONSTANT_POOL

; ------------------------------------------------------------------
; VarargPInvokeStub & VarargPInvokeGenILStub
; There is a separate stub when the method has a hidden return buffer arg.
;
; in:
; r0 = VASigCookie*
; r12 = MethodDesc *       
;
        PINVOKE_STUB VarargPInvoke, r0, {false}


; ------------------------------------------------------------------
; GenericPInvokeCalliHelper & GenericPInvokeCalliGenILStub
; Helper for generic pinvoke calli instruction 
;
; in:
; r4 = VASigCookie*
; r12 = Unmanaged target
;
        PINVOKE_STUB GenericPInvokeCalli, r4, {true}

; ------------------------------------------------------------------
; VarargPInvokeStub_RetBuffArg & VarargPInvokeGenILStub_RetBuffArg
; Vararg PInvoke Stub when the method has a hidden return buffer arg
;
; in:
; r1 = VASigCookie*
; r12 = MethodDesc*       
; 
        PINVOKE_STUB VarargPInvoke, r1, {false}


; Must be at very end of file 
        END
