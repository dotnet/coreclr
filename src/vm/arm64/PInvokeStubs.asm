; Licensed to the .NET Foundation under one or more agreements.
; The .NET Foundation licenses this file to you under the MIT license.
; See the LICENSE file in the project root for more information.

; ==++==
;;

;;
;; ==--==
#include "ksarm64.h"

#include "asmconstants.h"

#include "asmmacros.h"


    IMPORT VarargPInvokeStubWorker
    IMPORT GenericPInvokeCalliStubWorker
    IMPORT RareDisablePreemptiveGCHelper

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

        PINVOKE_STUB $FuncPrefix,$VASigCookieReg,$HiddenArg,$SaveFPArgs

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

        NESTED_ENTRY $__PInvokeStubFuncName

        ; get the stub
        ldr                 x9, [$VASigCookieReg, #VASigCookie__pNDirectILStub]

        ; if null goto stub generation
        cbz                 x9, %0

        IF "$FuncPrefix" == "GenericPInvokeCalli"
            ;
            ; We need to distinguish between a MethodDesc* and an unmanaged target.
            ; The way we do this is to shift the managed target to the left by one bit and then set the
            ; least significant bit to 1.  This works because MethodDesc* are always 8-byte aligned.
            ;
            lsl             $HiddenArg, $HiddenArg, #1
            orr             $HiddenArg, $HiddenArg, #1
        ENDIF

        EPILOG_BRANCH_REG   x9 

0
        EPILOG_BRANCH       $__PInvokeGenStubFuncName

        NESTED_END

        
        NESTED_ENTRY $__PInvokeGenStubFuncName

        PROLOG_WITH_TRANSITION_BLOCK 0, $SaveFPArgs

        ; x2 = Umanaged Target\MethodDesc
        mov                 x2, $HiddenArg 

        ; x1 = VaSigCookie
        mov                 x1, $VASigCookieReg

        ; x0 = pTransitionBlock
        add                 x0, sp, #__PWTB_TransitionBlock

        ; save hidden arg
        mov                 x19, $HiddenArg 

        ; save VASigCookieReg
        mov                 x20, $VASigCookieReg

        bl                  $__PInvokeStubWorkerName

        ; restore VASigCookieReg
        mov                 $VASigCookieReg, x20

        ; restore hidden arg (method desc or unmanaged target)
        mov                 $HiddenArg, x19


        EPILOG_WITH_TRANSITION_BLOCK_TAILCALL

        EPILOG_BRANCH       $__PInvokeStubFuncName
     
        NESTED_END
        
        MEND

    TEXTAREA

; ------------------------------------------------------------------
; JIT_PInvokeBegin helper
;
; in:
; x0 = InlinedCallFrame*
; 
        NESTED_ENTRY JIT_PInvokeBegin

            PROLOG_SAVE_REG_PAIR           fp, lr, #-32!
            PROLOG_SAVE_REG                x19, #16
           
            ldr     x9, =s_gsCookie
            ldr     x9, [x9]
            str     x9, [x0]
            add     x19, x0, SIZEOF__GSCookie
            
            ;; set first slot to the value of InlinedCallFrame::`vftable' (checked by runtime code)
            ldr     x9, =$InlinedCallFrame_vftable
            str     x9, [x19]

            str     xzr, [x19, #InlinedCallFrame__m_Datum]
        
            add     x9, sp, 32
            str     x9, [x19, #InlinedCallFrame__m_pCallSiteSP]
            ldr     x9, [sp]
            str     x9, [x19, #InlinedCallFrame__m_pCalleeSavedFP]
            str     lr, [x19, #InlinedCallFrame__m_pCallerReturnAddress]

            ;; x0 = GetThread(), TRASHES x9
            INLINE_GETTHREAD x0, x9

            ;; pFrame->m_Next = pThread->m_pFrame;
            ldr     x9, [x0, #Thread_m_pFrame]
            str     x9, [x19, #Frame__m_Next]

            ;; pThread->m_pFrame = pFrame;
            str     x19, [x0, #Thread_m_pFrame]

            ;; pThread->m_fPreemptiveGCDisabled = 0
            str     wzr, [x0, #Thread_m_fPreemptiveGCDisabled]

            EPILOG_RESTORE_REG        x19, #16
            EPILOG_RESTORE_REG_PAIR   fp, lr, #32!
            EPILOG_RETURN
            
        NESTED_END

; ------------------------------------------------------------------
; JIT_PInvokeEnd helper
;
; in:
; x0 = InlinedCallFrame*
; 
        NESTED_ENTRY JIT_PInvokeEnd
    
            PROLOG_SAVE_REG_PAIR           fp, lr, #-32!
            PROLOG_SAVE_REG_PAIR           x19, x20, #16

            add     x19, x0, SIZEOF__GSCookie

            ;; x20 = GetThread(), TRASHES x9
            INLINE_GETTHREAD x20, x9

            ;; x19 = pFrame
            ;; x20 = pThread
            
            ;; pThread->m_fPreemptiveGCDisabled = 1
            mov     x9, 1
            str     w9, [x20, #Thread_m_fPreemptiveGCDisabled]

            ;; Check return trap
            ldr     x9, =g_TrapReturningThreads
            ldr     x9, [x9]
            cbz     x9, DoNothing

            ; Call GC helper
            bl      RareDisablePreemptiveGCHelper
            
DoNothing

            ;; pThread->m_pFrame = pFrame->m_Next;
            ldr     x9, [x19, #Frame__m_Next]
            str     x9, [x20, #Thread_m_pFrame]

            str     xzr, [x19, #InlinedCallFrame__m_pCallerReturnAddress]
        
            EPILOG_RESTORE_REG_PAIR   x19, x20, #16
            EPILOG_RESTORE_REG_PAIR   fp, lr, #32!
            EPILOG_RETURN
            
        NESTED_END

        INLINE_GETTHREAD_CONSTANT_POOL
        
; ------------------------------------------------------------------
; VarargPInvokeStub & VarargPInvokeGenILStub
;
; in:
; x0 = VASigCookie*
; x12 = MethodDesc *       
;
        PINVOKE_STUB VarargPInvoke, x0, x12, {false}


; ------------------------------------------------------------------
; GenericPInvokeCalliHelper & GenericPInvokeCalliGenILStub
; Helper for generic pinvoke calli instruction 
;
; in:
; x15 = VASigCookie*
; x12 = Unmanaged target
;
        PINVOKE_STUB GenericPInvokeCalli, x15, x12, {true}


; Must be at very end of file 
        END
