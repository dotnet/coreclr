// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "unixasmmacros.inc"
#include "asmconstants.h"

.globl C_FUNC(SignalHandlerWorkerReturnOffset)
C_FUNC(SignalHandlerWorkerReturnOffset):
    .int LOCAL_LABEL(SignalHandlerWorkerReturn)-C_FUNC(CallSignalHandlerWrapper)

// This function is never called, only a fake stack frame will be setup to have a return
// address set to SignalHandlerWorkerReturn during SIGSEGV handling.
// It enables the unwinder to unwind stack from the handling code to the actual failure site.
NESTED_ENTRY CallSignalHandlerWrapper, _TEXT, NoHandler
    PROLOG_STACK_ALLOC 144 // red zone + fp + lr
    PROLOG_SAVE_REG_PAIR fp, lr, 0
    bl      EXTERNAL_C_FUNC(signal_handler_worker)
LOCAL_LABEL(SignalHandlerWorkerReturn):
    EPILOG_RESTORE_REG_PAIR fp, lr, 0
    EPILOG_STACK_FREE 144
    ret
NESTED_END CallSignalHandlerWrapper, _TEXT
