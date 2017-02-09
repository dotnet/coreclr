// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    include/pal/signal.hpp

Abstract:
    Private signal handling utilities for SEH



--*/

#ifndef _PAL_SIGNAL_HPP_
#define _PAL_SIGNAL_HPP_

#if !HAVE_MACH_EXCEPTIONS

struct SignalHandlerWorkerReturnPoint;

/*++
Function :
    CallSignalHandlerWrapper

    This function is never called, only a fake stack frame will be setup to have a return
    address set to SignalHandlerWorkerReturn during SIGSEGV handling.
    It enables the unwinder to unwind stack from the handling code to the actual failure site.

Parameters :
    none

    (no return value)
--*/
extern "C" void CallSignalHandlerWrapper();

// Offset of the return address from the SignalHandlerWorker in the CallSignalHandlerWrapper 
// relative to the start of the function
extern "C" int SignalHandlerWorkerReturnOffset;

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
extern "C" void signal_handler_worker(int code, siginfo_t *siginfo, void *context, SignalHandlerWorkerReturnPoint* returnPoint);

/*++
Function :
    ExecuteHandlerOnOriginalStack

    Executes signal_handler_worker on the original stack where the signal occured.
    It installs fake stack frame to enable stack unwinding to the signal source location.

Parameters :
    POSIX signal handler parameter list ("man sigaction" for details)
    returnPoint - context to which the function returns if the common_signal_handler returns

    (no return value)
--*/
void ExecuteHandlerOnOriginalStack(int code, siginfo_t *siginfo, void *context, SignalHandlerWorkerReturnPoint* returnPoint);

/*++
Function :
    EnsureSignalAlternateStack

    Ensure that alternate stack for signal handling is allocated for the current thread

Parameters :
    None

Return :
    TRUE in case of a success, FALSE otherwise
--*/
BOOL EnsureSignalAlternateStack();

/*++
Function :
    FreeSignalAlternateStack

    Free alternate stack for signal handling

Parameters :
    None

Return :
    None
--*/
void FreeSignalAlternateStack();

/*++
Function :
    SEHInitializeSignals

    Set-up signal handlers to catch signals and translate them to exceptions

Parameters :
    flags: PAL initialization flags

Return :
    TRUE in case of a success, FALSE otherwise
--*/
BOOL SEHInitializeSignals(DWORD flags);

/*++
Function :
    SEHCleanupSignals

    Restore default signal handlers

    (no parameters, no return value)
--*/
void SEHCleanupSignals();

#endif // !HAVE_MACH_EXCEPTIONS

#endif /* _PAL_SIGNAL_HPP_ */

