//
// Copyright (c) Microsoft. All rights reserved.
// Copyright (c) Geoff Norton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

// ==++==
//

//
// ==--==

#include "unixasmmacros.inc"
#include "asmconstants.h"

//
// METHODDESC_REGISTER: UMEntryThunk*
//
NESTED_ENTRY TheUMEntryPrestub, _TEXT, UMEntryPrestubUnwindFrameChainHandler

#define TheUMEntryPrestub_XMM_SAVE_OFFSET SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES

// Ensure that the new rsp will be 16-byte aligned.  Note that the caller has
// already pushed the return address.
#if (((SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES + SIZEOF_MAX_FP_ARG_SPILL) + 8) % 16) != 0
#define TheUMEntryPrestub_STACK_FRAME_SIZE (SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES + SIZEOF_MAX_FP_ARG_SPILL + 8)
#else
#define TheUMEntryPrestub_STACK_FRAME_SIZE (SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES + SIZEOF_MAX_FP_ARG_SPILL)
#endif

        alloc_stack     TheUMEntryPrestub_STACK_FRAME_SIZE

        save_reg_postrsp    rdi, TheUMEntryPrestub_STACK_FRAME_SIZE + 0x8
        save_reg_postrsp    rsi, TheUMEntryPrestub_STACK_FRAME_SIZE + 0x10
        save_reg_postrsp    rdx,  TheUMEntryPrestub_STACK_FRAME_SIZE + 0x18
        save_reg_postrsp    rcx,  TheUMEntryPrestub_STACK_FRAME_SIZE + 0x20
        save_reg_postrsp    r8,  TheUMEntryPrestub_STACK_FRAME_SIZE + 0x28
        save_reg_postrsp    r9,  TheUMEntryPrestub_STACK_FRAME_SIZE + 0x30

        save_xmm128_postrsp xmm0, TheUMEntryPrestub_XMM_SAVE_OFFSET
        save_xmm128_postrsp xmm1, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x10
        save_xmm128_postrsp xmm2, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x20
        save_xmm128_postrsp xmm3, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x30
        save_xmm128_postrsp xmm4, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x40
        save_xmm128_postrsp xmm5, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x50
        save_xmm128_postrsp xmm6, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x60
        save_xmm128_postrsp xmm7, TheUMEntryPrestub_XMM_SAVE_OFFSET + 0x70

        END_PROLOGUE

        //
        // Do prestub-specific stuff
        //
        mov             rdi, METHODDESC_REGISTER
        call            C_FUNC(TheUMEntryPrestubWorker)

        //
        // we're going to tail call to the exec stub that we just setup
        //

        mov             rdi, [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 8h]
        mov             rsi, [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 10h]
        mov             rdx,  [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 18h]
        mov             rcx,  [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 20h]
        mov             r8,  [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 28h]
        mov             r9,  [rsp + TheUMEntryPrestub_STACK_FRAME_SIZE + 30h]

        movdqa          xmm0, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET]
        movdqa          xmm1, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 10h]
        movdqa          xmm2, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 20h]
        movdqa          xmm3, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 30h]
        movdqa          xmm4, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 40h]
        movdqa          xmm5, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 50h]
        movdqa          xmm6, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 60h]
        movdqa          xmm7, [rsp + TheUMEntryPrestub_XMM_SAVE_OFFSET + 70h]

        //
        // epilogue
        //
        add             rsp, TheUMEntryPrestub_STACK_FRAME_SIZE
        TAILJMP_RAX
        
NESTED_END TheUMEntryPrestub, _TEXT


//
// METHODDESC_REGISTER: UMEntryThunk*
//
NESTED_ENTRY UMThunkStub, _TEXT, UMThunkStubUnwindFrameChainHandler

// number of integer registers saved in prologue
#define UMThunkStubAMD64_NUM_REG_PUSHES 2

#define UMThunkStubAMD64_METHODDESC_SPILL_SIZE 0x10
#define UMThunkStubAMD64_METHODDESC_SPILL_NEGOFFSET 0x20

// HOST_NOTIFY_FLAG
#define UMThunkStubAMD64_HOST_NOTIFY_FLAG_NEGOFFSET 0x28

// XMM save area 
#define UMThunkStubAMD64_XMM_SAVE_NEGOFFSET 0xa8

// Add in the callee scratch area size.
#define UMThunkStubAMD64_CALLEE_SCRATCH_SIZE SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES
#define UMThunkStubAMD64_STACK_FRAME_SIZE 0xd8

// Now we have the full size of the stack frame.  The offsets have been computed relative to the
// top, so negate them to make them relative to the post-prologue rsp.
#define UMThunkStubAMD64_FRAME_OFFSET UMThunkStubAMD64_CALLEE_SCRATCH_SIZE
#define UMThunkStubAMD64_METHODDESC_SPILL_OFFSET (UMThunkStubAMD64_STACK_FRAME_SIZE - UMThunkStubAMD64_FRAME_OFFSET - UMThunkStubAMD64_METHODDESC_SPILL_NEGOFFSET)
#define UMThunkStubAMD64_HOST_NOTIFY_FLAG_OFFSET (UMThunkStubAMD64_STACK_FRAME_SIZE - UMThunkStubAMD64_FRAME_OFFSET - UMThunkStubAMD64_HOST_NOTIFY_FLAG_NEGOFFSET)
#define UMThunkStubAMD64_XMM_SAVE_OFFSET (UMThunkStubAMD64_STACK_FRAME_SIZE - UMThunkStubAMD64_FRAME_OFFSET - UMThunkStubAMD64_XMM_SAVE_NEGOFFSET)
#define UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET (UMThunkStubAMD64_STACK_FRAME_SIZE + 8 - UMThunkStubAMD64_FRAME_OFFSET)
#define UMThunkStubAMD64_FIXED_STACK_ALLOC_SIZE (UMThunkStubAMD64_STACK_FRAME_SIZE - (UMThunkStubAMD64_NUM_REG_PUSHES * 8))

#if (UMTHUNKSTUB_HOST_NOTIFY_FLAG_RBPOFFSET - UMThunkStubAMD64_HOST_NOTIFY_FLAG_OFFSET) != 0
#error update UMTHUNKSTUB_HOST_NOTIFY_FLAG_RBPOFFSET
#endif

        push_nonvol_reg r12
        push_nonvol_reg rbp                                                                     // stack_args
        alloc_stack     UMThunkStubAMD64_FIXED_STACK_ALLOC_SIZE
        set_frame       rbp, UMThunkStubAMD64_FRAME_OFFSET                                      // stack_args
        mov             byte ptr [rbp + UMThunkStubAMD64_HOST_NOTIFY_FLAG_OFFSET], 0            // hosted

        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h], rdi
        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  8h], rsi
        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 10h], rdx
        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 18h], rcx
        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 20h], r8
        mov             [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 28h], r9

        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET +  0h], xmm0
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 10h], xmm1
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 20h], xmm2
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 30h], xmm3
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 40h], xmm4
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 50h], xmm5
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 60h], xmm6
        movdqa          [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 70h], xmm7

        mov             [rbp + UMThunkStubAMD64_METHODDESC_SPILL_OFFSET], METHODDESC_REGISTER

        END_PROLOGUE

        //
        // Call GetThread()
        //
        call            C_FUNC(GetThread)
        mov             METHODDESC_REGISTER, [rbp + UMThunkStubAMD64_METHODDESC_SPILL_OFFSET]
        test            rax, rax
        jz              LOCAL_LABEL(DoThreadSetup)

LOCAL_LABEL(HaveThread):

        mov             r12, rax                // r12 <- Thread*

        //
        // disable preemptive GC
        //
        mov             dword ptr [r12 + OFFSETOF__Thread__m_fPreemptiveGCDisabled], 1

        //
        // catch returning thread here if a GC is in progress
        //
        cmp             [rip + C_FUNC(g_TrapReturningThreads)@GOTPCREL], 0
        jnz             LOCAL_LABEL(DoTrapReturningThreadsTHROW)

LOCAL_LABEL(InCooperativeMode):

        mov             rax, [r12 + OFFSETOF__Thread__m_pDomain]
        mov             eax, [rax + OFFSETOF__AppDomain__m_dwId]

        mov             r11d, [METHODDESC_REGISTER + OFFSETOF__UMEntryThunk__m_dwDomainId]

        cmp             rax, r11
        jne             LOCAL_LABEL(WrongAppDomain)

        mov             r11, [METHODDESC_REGISTER + OFFSETOF__UMEntryThunk__m_pUMThunkMarshInfo]
        mov             eax, [r11 + OFFSETOF__UMThunkMarshInfo__m_cbActualArgSize]                      // stack_args
        test            rax, rax                                                                        // stack_args
        jnz             LOCAL_LABEL(UMThunkStub_CopyStackArgs)                                                                   // stack_args
        
LOCAL_LABEL(UMThunkStub_ArgumentsSetup):
        mov             rdi,  [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h] 
        mov             rsi,  [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  8h] 
        mov             rdx,   [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 10h] 
        mov             rcx,   [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 18h]
        mov             r8,   [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 20h]
        mov             r9,   [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 28h]

        // @CONSIDER: mark UMEntryThunks that have FP params and only save/restore xmm regs on those calls
        movdqa          xmm0, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET +  0h]
        movdqa          xmm1, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 10h]
        movdqa          xmm2, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 20h]
        movdqa          xmm3, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 30h]
        movdqa          xmm4, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 40h]
        movdqa          xmm5, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 50h]
        movdqa          xmm6, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 60h]
        movdqa          xmm7, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET + 70h]

        mov             rax, [r11 + OFFSETOF__UMThunkMarshInfo__m_pILStub]                              // rax <- Stub*
        call            rax

LOCAL_LABEL(PostCall):
        //
        // enable preemptive GC
        //
        mov             dword ptr [r12 + OFFSETOF__Thread__m_fPreemptiveGCDisabled], 0

        // epilog
        lea             rsp, [rbp - UMThunkStubAMD64_FRAME_OFFSET + UMThunkStubAMD64_FIXED_STACK_ALLOC_SIZE]
        pop             rbp                                                                             // stack_args
        pop             r12
        ret


LOCAL_LABEL(DoThreadSetup):
        call            C_FUNC(CreateThreadBlockThrow)
        mov             METHODDESC_REGISTER, [rbp + UMThunkStubAMD64_METHODDESC_SPILL_OFFSET]
        jmp             LOCAL_LABEL(HaveThread)
        
LOCAL_LABEL(DoTrapReturningThreadsTHROW):
        mov             rdi, r12                                                                  // Thread* pThread
        mov             rsi, METHODDESC_REGISTER                                                  // UMEntryThunk* pUMEntry
        call            C_FUNC(UMThunkStubRareDisableWorker)
        mov             METHODDESC_REGISTER, [rbp + UMThunkStubAMD64_METHODDESC_SPILL_OFFSET]

        jmp             LOCAL_LABEL(InCooperativeMode)

LOCAL_LABEL(UMThunkStub_CopyStackArgs):
        // rax = cbStackArgs (with 20h for register args subtracted out already)

        sub             rsp, rax
        and             rsp, -16

        // rax = number of bytes

        lea             rdi, [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES] 
        lea             rsi, [rsp + UMThunkStubAMD64_CALLEE_SCRATCH_SIZE]

LOCAL_LABEL(CopyLoop):
        // rax = number of bytes
        // rdi = src
        // rsi = dest
        // rdx = sratch

        add             rax, -8
        mov             rdx, [rdi + rax]
        mov             [rsi + rax], rdx
        jnz             LOCAL_LABEL(CopyLoop)

        mov             rdi, [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h]
        mov             rsi, [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  8h]
        mov             rdx, [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 10h]
        
        jmp             LOCAL_LABEL(UMThunkStub_ArgumentsSetup)

LOCAL_LABEL(WrongAppDomain):
        //
        // call our helper to perform the AD transtion 
        //
        mov             rdi, METHODDESC_REGISTER
        lea             rdx,  [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET]
        mov             rax, [METHODDESC_REGISTER + OFFSETOF__UMEntryThunk__m_pUMThunkMarshInfo]
        mov             rcx, [rax + OFFSETOF__UMThunkMarshInfo__m_cbActualArgSize]
        mov             rsi,  [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  8h] 
        call            C_FUNC(UM2MDoADCallBack)

        // restore return value
        mov             rax,  [rbp + UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h]
        movdqa          xmm0, [rbp + UMThunkStubAMD64_XMM_SAVE_OFFSET +  0h]

        jmp             LOCAL_LABEL(PostCall)

NESTED_END UMThunkStub, _TEXT

//
// EXTERN_C void __stdcall UM2MThunk_WrapperHelper(
//       void *pThunkArgs,               // rdi
//       int argLen,                     // rsi
//       void *pAddr,                    // rdx            // not used
//       UMEntryThunk *pEntryThunk,      // rcx
//       Thread *pThread);               // r8
//
NESTED_ENTRY UM2MThunk_WrapperHelper, _TEXT, NoHandler

// number of integer registers saved in prologue
#define UM2MThunk_WrapperHelper_NUM_REG_PUSHES 1

#define UM2MThunk_WrapperHelper_CALLEE_SCRATCH_SIZE SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES
#define UM2MThunk_WrapperHelper_STACK_FRAME_SIZE 0x38

#define UM2MThunk_WrapperHelper_FRAME_OFFSET UM2MThunk_WrapperHelper_CALLEE_SCRATCH_SIZE
#define UM2MThunk_WrapperHelper_FIXED_STACK_ALLOC_SIZE (UM2MThunk_WrapperHelper_STACK_FRAME_SIZE - (UM2MThunk_WrapperHelper_NUM_REG_PUSHES * 8))

        push_nonvol_reg rbp
        alloc_stack     UM2MThunk_WrapperHelper_FIXED_STACK_ALLOC_SIZE
        set_frame       rbp, UM2MThunk_WrapperHelper_FRAME_OFFSET
        END_PROLOGUE

        //
        // We are in cooperative mode and in the correct domain. 
        // The host has also been notified that we've entered the 
        // runtime.  All we have left to do is to copy the stack, 
        // setup the register args and then call the managed target
        //

        test            rsi, rsi
        jg              LOCAL_LABEL(UM2MThunk_WrapperHelper_CopyStackArgs)

LOCAL_LABEL(UM2MThunk_WrapperHelper_ArgumentsSetup):
        mov             METHODDESC_REGISTER, rcx

        mov             r11, rdi                // rsi <- pThunkArgs
        mov             rdi, [r11 +  0h]
        mov             rsi, [r11 +  8h]
        mov             rdx, [r11 + 10h]
        mov             rcx, [r11 + 18h]
        mov             r8, [r11 + 20h]
        mov             r9, [r11 + 28h]

        movdqa          xmm0, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h]
        movdqa          xmm1, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 10h]
        movdqa          xmm2, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 20h]
        movdqa          xmm3, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 30h]
        movdqa          xmm4, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 40h]
        movdqa          xmm5, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 50h]
        movdqa          xmm6, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 60h]
        movdqa          xmm7, [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET + 70h]

        mov             rax, [METHODDESC_REGISTER + OFFSETOF__UMEntryThunk__m_pUMThunkMarshInfo]      // rax <- UMThunkMarshInfo*
        mov             rax, [rax + OFFSETOF__UMThunkMarshInfo__m_pILStub]                            // rax <- Stub*
        call            rax

        // make sure we don't trash the return value
        mov             [r11 + 0h], rax
        movdqa          [r11 + UMThunkStubAMD64_XMM_SAVE_OFFSET - UMThunkStubAMD64_ARGUMENTS_STACK_HOME_OFFSET +  0h], xmm0

        lea             rsp, [rbp - UM2MThunk_WrapperHelper_FRAME_OFFSET + UM2MThunk_WrapperHelper_FIXED_STACK_ALLOC_SIZE]
        pop             rbp
        ret
        

LOCAL_LABEL(UM2MThunk_WrapperHelper_CopyStackArgs):
        // rsi = cbStackArgs (with 20h for register args subtracted out already)
        // rdi = pSrcArgStack

        sub             rsp, rsi
        and             rsp, -16

        mov             rdx, rdi
        mov             r11, rcx
        
        lea             rsi, [rsi + SIZEOF_MAX_OUTGOING_ARGUMENT_HOMES]
        lea             rdi, [rsp + UM2MThunk_WrapperHelper_CALLEE_SCRATCH_SIZE]

        mov             rcx, rsi
        shr             rcx, 3
        
        rep movsq
        
        mov             rsi, rdx
        mov             rcx, r11
        
        jmp             LOCAL_LABEL(UM2MThunk_WrapperHelper_ArgumentsSetup)
        
NESTED_END UM2MThunk_WrapperHelper, _TEXT
