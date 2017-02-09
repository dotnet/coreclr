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
    size_t* sp = (size_t*)MCREG_Esp(ucontext->uc_mcontext);

    // Hmm, we will need to ensure proper alignment, we don't have any guarantee, do we?
    _ASSERTE((((size_t)sp) & 0xf) == 0);

    // Build fake stack frame to enable the stack unwinder to unwind from signal_handler_worker to the faulting instruction
    *--sp = (size_t)MCREG_Eip(ucontext->uc_mcontext);
    *--sp = (size_t)MCREG_Ebp(ucontext->uc_mcontext);
    size_t fp = (size_t)sp;
    *--sp = (size_t)returnPoint;
    *--sp = (size_t)context;
    *--sp = (size_t)siginfo;
    *--sp = code;
    *--sp = (size_t)SignalHandlerWorkerReturnOffset + (size_t)CallSignalHandlerWrapper;

    // Switch the current context to the signal_handler_worker and the original stack
    ucontext_t ucontext2;
    getcontext(&ucontext2);

    // We don't care about the other registers state since the stack unwinding restores
    // them for the target frame directly from the signal context.
    MCREG_Esp(ucontext2.uc_mcontext) = (size_t)sp;
    MCREG_Ebp(ucontext2.uc_mcontext) = (size_t)fp;
    MCREG_Eip(ucontext2.uc_mcontext) = (size_t)signal_handler_worker;

    setcontext(&ucontext2);
}
