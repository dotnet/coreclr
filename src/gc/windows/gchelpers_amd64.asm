

push_nonvol_reg macro Reg

        .errnz ___STACK_ADJUSTMENT_FORBIDDEN, <push_nonvol_reg cannot be used after save_reg_postrsp>

        push    Reg
        .pushreg Reg

        endm

END_PROLOGUE macro

        .endprolog

        endm

NESTED_ENTRY macro Name, Section, Handler

Section segment para 'CODE'

        align   16

        public  Name

ifb <Handler>

Name    proc    frame

else

Name    proc    frame:Handler

endif

        ___FRAME_REG_SET = 0
        ___STACK_ADJUSTMENT_FORBIDDEN = 0

        endm

NESTED_END macro Name, section

Name    endp

Section ends

        endm

;; extern "C" DWORD __stdcall getcpuid(DWORD arg, unsigned char result[16]);
NESTED_ENTRY getcpuid, _TEXT

        push_nonvol_reg    rbx
        push_nonvol_reg    rsi
    END_PROLOGUE

        mov     eax, ecx                ; first arg
        mov     rsi, rdx                ; second arg (result)
        xor     ecx, ecx                ; clear ecx - needed for "Structured Extended Feature Flags"
        cpuid
        mov     [rsi+ 0], eax
        mov     [rsi+ 4], ebx
        mov     [rsi+ 8], ecx
        mov     [rsi+12], edx
        pop     rsi
        pop     rbx
        ret
NESTED_END getcpuid, _TEXT

;The following function uses Deterministic Cache Parameter leafs to determine the cache hierarchy information on Prescott & Above platforms. 
;  This function takes 3 arguments:
;     Arg1 is an input to ECX. Used as index to specify which cache level to return information on by CPUID.
;         Arg1 is already passed in ECX on call to getextcpuid, so no explicit assignment is required;  
;     Arg2 is an input to EAX. For deterministic code enumeration, we pass in 4H in arg2.
;     Arg3 is a pointer to the return dwbuffer
NESTED_ENTRY getextcpuid, _TEXT
        push_nonvol_reg    rbx
        push_nonvol_reg    rsi
    END_PROLOGUE
        
        mov     eax, edx                ; second arg (input to  EAX)
        mov     rsi, r8                 ; third arg  (pointer to return dwbuffer)       
        cpuid
        mov     [rsi+ 0], eax
        mov     [rsi+ 4], ebx
        mov     [rsi+ 8], ecx
        mov     [rsi+12], edx
        pop     rsi
        pop     rbx

        ret
NESTED_END getextcpuid, _TEXT

    end
