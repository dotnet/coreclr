// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

.intel_syntax noprefix
#include "unixasmmacros.inc"
#include "asmconstants.h"

.globl C_FUNC(SignalHandlerWorkerReturnOffset)
C_FUNC(SignalHandlerWorkerReturnOffset):
    .int LOCAL_LABEL(SignalHandlerWorkerReturn)-C_FUNC(CallSignalHandlerWrapper)

// This function is never called, only a fake stack frame will be setup to have a return
// address set to SignalHandlerWorkerReturn during SIGSEGV handling.
// It enables the unwinder to unwind stack from the handling code to the actual failure site.
NESTED_ENTRY CallSignalHandlerWrapper, _TEXT, NoHandler
    PROLOG_BEG
    PROLOG_END
    // Simulate arguments pushing
    push eax
    push eax
    push eax
    push eax
    call    EXTERNAL_C_FUNC(signal_handler_worker)
LOCAL_LABEL(SignalHandlerWorkerReturn):
    add esp, 16
    EPILOG_BEG
    EPILOG_END
    ret
NESTED_END CallSignalHandlerWrapper, _TEXT
