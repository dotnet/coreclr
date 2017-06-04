.intel_syntax noprefix
#include "unixasmmacros.inc"

//
// EXTERN_C void EnterNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY EnterNaked3, _TEXT, NoHandler
    push rbp
    mov  rbp, rsp
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push r12
    push r13
    push r14
    push r15
    sub  rsp, 16*8 /* xmm0-xmm7 */
    and  rsp, (-16) /* must be alligned */
    movdqa	[rsp+0x00], xmm0
    movdqa	[rsp+0x10], xmm1
    movdqa	[rsp+0x20], xmm2
    movdqa	[rsp+0x30], xmm3
    movdqa	[rsp+0x40], xmm4
    movdqa	[rsp+0x50], xmm5
    movdqa	[rsp+0x60], xmm6
    movdqa	[rsp+0x70], xmm7
    call    EXTERNAL_C_FUNC(EnterStub)
    movdqa	xmm0, [rsp+0x00]
    movdqa	xmm1, [rsp+0x10]
    movdqa	xmm2, [rsp+0x20]
    movdqa	xmm3, [rsp+0x30]
    movdqa	xmm4, [rsp+0x40]
    movdqa	xmm5, [rsp+0x50]
    movdqa	xmm6, [rsp+0x60]
    movdqa	xmm7, [rsp+0x70]
    lea  rsp, [rbp - (14 * 8)]
    pop  r15
    pop  r14
    pop  r13
    pop  r12
    pop  r11
    pop  r10
    pop  r9
    pop  r8
    pop  rdi
    pop  rsi
    pop  rdx
    pop  rcx
    pop  rbx
    pop  rax
    pop  rbp
    ret
NESTED_END EnterNaked3, _TEXT

//
// EXTERN_C void LeaveNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY LeaveNaked3, _TEXT, NoHandler
    push rbp
    mov  rbp, rsp
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push r12
    push r13
    push r14
    push r15
    sub  rsp, 16*8 /* xmm0-xmm7 */
    and  rsp, (-16) /* must be alligned */
    movdqa	[rsp+0x00], xmm0
    movdqa	[rsp+0x10], xmm1
    movdqa	[rsp+0x20], xmm2
    movdqa	[rsp+0x30], xmm3
    movdqa	[rsp+0x40], xmm4
    movdqa	[rsp+0x50], xmm5
    movdqa	[rsp+0x60], xmm6
    movdqa	[rsp+0x70], xmm7
    call    EXTERNAL_C_FUNC(LeaveStub)
    movdqa	xmm0, [rsp+0x00]
    movdqa	xmm1, [rsp+0x10]
    movdqa	xmm2, [rsp+0x20]
    movdqa	xmm3, [rsp+0x30]
    movdqa	xmm4, [rsp+0x40]
    movdqa	xmm5, [rsp+0x50]
    movdqa	xmm6, [rsp+0x60]
    movdqa	xmm7, [rsp+0x70]
    lea  rsp, [rbp - (14 * 8)]
    pop  r15
    pop  r14
    pop  r13
    pop  r12
    pop  r11
    pop  r10
    pop  r9
    pop  r8
    pop  rdi
    pop  rsi
    pop  rdx
    pop  rcx
    pop  rbx
    pop  rax
    pop  rbp
    ret
NESTED_END LeaveNaked3, _TEXT

//
// EXTERN_C void TailcallNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY TailcallNaked3, _TEXT, NoHandler
    push rbp
    mov  rbp, rsp
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push r12
    push r13
    push r14
    push r15
    sub  rsp, 16*8 /* xmm0-xmm7 */
    and  rsp, (-16) /* must be alligned */
    movdqa	[rsp+0x00], xmm0
    movdqa	[rsp+0x10], xmm1
    movdqa	[rsp+0x20], xmm2
    movdqa	[rsp+0x30], xmm3
    movdqa	[rsp+0x40], xmm4
    movdqa	[rsp+0x50], xmm5
    movdqa	[rsp+0x60], xmm6
    movdqa	[rsp+0x70], xmm7
    call    EXTERNAL_C_FUNC(TailcallStub)
    movdqa	xmm0, [rsp+0x00]
    movdqa	xmm1, [rsp+0x10]
    movdqa	xmm2, [rsp+0x20]
    movdqa	xmm3, [rsp+0x30]
    movdqa	xmm4, [rsp+0x40]
    movdqa	xmm5, [rsp+0x50]
    movdqa	xmm6, [rsp+0x60]
    movdqa	xmm7, [rsp+0x70]
    lea  rsp, [rbp - (14 * 8)]
    pop  r15
    pop  r14
    pop  r13
    pop  r12
    pop  r11
    pop  r10
    pop  r9
    pop  r8
    pop  rdi
    pop  rsi
    pop  rdx
    pop  rcx
    pop  rbx
    pop  rax
    pop  rbp
    ret
NESTED_END TailcallNaked3, _TEXT
