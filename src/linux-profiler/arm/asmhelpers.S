#include "unixasmmacros.inc"

.syntax unified
.thumb

//
// EXTERN_C void EnterNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY EnterNaked3, _TEXT, NoHandler
    push    {r0-r7, lr}
    .save   {r0-r7, lr}
    bl      C_FUNC(EnterStub)
    pop     {r0-r7, pc}
NESTED_END EnterNaked3, _TEXT

//
// EXTERN_C void LeaveNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY LeaveNaked3, _TEXT, NoHandler
    push    {r0-r7, lr}
    .save   {r0-r7, lr}
    bl      C_FUNC(LeaveStub)
    pop     {r0-r7, pc}
NESTED_END LeaveNaked3, _TEXT

//
// EXTERN_C void TailcallNaked3(FunctionIDOrClientID functionIDOrClientID);
//
NESTED_ENTRY TailcallNaked3, _TEXT, NoHandler
    push    {r0-r7, lr}
    .save   {r0-r7, lr}
    bl      C_FUNC(TailcallStub)
    pop     {r0-r7, pc}
NESTED_END TailcallNaked3, _TEXT

//.arm
NESTED_ENTRY unw_getcontext, _TEXT, NoHandler
	stmia r0, {r0-r12}
        add r0, r0, #(13*4)
        mov r1, sp
        mov r2, lr
	stmia r0, {r1, r2, r14} // dont' use pc
        //str lr, [r0, #(15*4)]
	bx lr
NESTED_END unw_getcontext, _TEXT

NESTED_ENTRY getPrevPC, _TEXT, NoHandler
        ldr r0, [r11, #4]
        bx lr
NESTED_END getPrevPC, _TEXT
