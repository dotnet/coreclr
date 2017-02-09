// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pal/dbgmsg.h"
SET_DEFAULT_DEBUG_CHANNEL(EXCEPT); // some headers have code with asserts, so do this first

#include "pal/palinternal.h"
#include "pal/context.h"
#include "pal/signal.hpp"
#include <sys/ucontext.h>

/*++
Function :
    signal_handler_worker

    Handles signal on the original stack where the signal occured. 
    Invoked via setcontext.

Parameters :
    POSIX signal handler parameter list ("man sigaction" for details)
    returnPoint - context to which the function returns if the common_signal_handler returns

    (no return value)
--*/
void ExecuteHandlerOnOriginalStack(int code, siginfo_t *siginfo, void *context, SignalHandlerWorkerReturnPoint* returnPoint)
{
    ucontext_t *ucontext = (ucontext_t *)context;
    size_t* sp = (size_t*)MCREG_Sp(ucontext->uc_mcontext);
    _ASSERTE((((size_t)sp) & 0xf) == 0);

    sp -= 128 / sizeof(size_t); // red zone

    // Build fake stack frame to enable the stack unwinder to unwind from signal_handler_worker to the faulting instruction
    // pushed LR
    *--sp = (size_t)MCREG_Pc(ucontext->uc_mcontext);
    // pushed frame pointer
    *--sp = (size_t)MCREG_Fp(ucontext->uc_mcontext); 

    // Switch the current context to the signal_handler_worker and the original stack
    ucontext_t ucontext2;
    getcontext(&ucontext2);

    // We don't care about the other registers state since the stack unwinding restores
    // them for the target frame directly from the signal context.
    MCREG_Sp(ucontext2.uc_mcontext) = (size_t)sp;
    MCREG_Fp(ucontext2.uc_mcontext) = (size_t)sp; // Fp and Sp are the same
    MCREG_Lr(ucontext2.uc_mcontext) = (size_t)SignalHandlerWorkerReturnOffset + (size_t)CallSignalHandlerWrapper
    MCREG_Pc(ucontext2.uc_mcontext) = (size_t)signal_handler_worker;
    MCREG_X0(ucontext2.uc_mcontext) = code;
    MCREG_X1(ucontext2.uc_mcontext) = (size_t)siginfo;
    MCREG_X2(ucontext2.uc_mcontext) = (size_t)context;
    MCREG_X3(ucontext2.uc_mcontext) = (size_t)returnPoint;

    setcontext(&ucontext2);
}
