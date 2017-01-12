// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

#include "stdafx.h"
#include "unwinder_i386.h"

#ifdef WIN64EXCEPTIONS
//---------------------------------------------------------------------------------------
//
// Read 32 bit unsigned value from the specified address. When the unwinder is built
// for jitted code unwinding on non-Windows systems, this is just a plain memory read.
// When the unwinder is built for DAC though, this reads data from the target debugged
// process.
//
// Arguments:
//    addr - address to read from
//
// Return Value:
//    The value that was read
//
// Notes:
//    If the memory read fails in the DAC mode, the failure is reported as an exception
//    via the DacError function.
//
static ULONG MemoryRead32(PULONG32 addr)
{
    return *dac_cast<PTR_ULONG>((TADDR)addr);
}

/*++

Routine Description:

    This function processes unwind codes and reverses the state change
    effects of a prologue. If the specified unwind information contains
    chained unwind information, then that prologue is unwound recursively.
    As the prologue is unwound state changes are recorded in the specified
    context structure and optionally in the specified context pointers
    structures.

Arguments:

    ImageBase - Supplies the base address of the image that contains the
        function being unwound.

    ControlPc - Supplies the address where control left the specified
        function.

    FrameBase - Supplies the base of the stack frame subject function stack
         frame.

    FunctionEntry - Supplies the address of the function table entry for the
        specified function.

    ContextRecord - Supplies the address of a context record.

    ContextPointers - Supplies an optional pointer to a context pointers
        record.

    FinalFunctionEntry - Supplies a pointer to a variable that receives the
        final function entry after the specified function entry and all
        descendent chained entries have been unwound. This will have been
        probed as appropriate.

Return Value:

    HRESULT.

--*/
HRESULT OOPStackUnwinderX86::UnwindPrologue(
    __in DWORD ImageBase,
    __in DWORD ControlPc,
    __in DWORD FrameBase,
    __in _PIMAGE_RUNTIME_FUNCTION_ENTRY FunctionEntry,
    __inout PCONTEXT ContextRecord,
    __inout_opt PKNONVOLATILE_CONTEXT_POINTERS ContextPointers,
    __deref_out _PIMAGE_RUNTIME_FUNCTION_ENTRY *FinalFunctionEntry
    )
{
    ULONG ChainCount;
    ULONG Index;

    BOOLEAN MachineFrame;

    //
    // Process the unwind codes for the specified function entry and all its
    // descendent chained function entries.
    //

    ChainCount = 0;

    do {
        Index = 0;
        MachineFrame = FALSE;

        PUNWIND_INFO UnwindInfo = GetUnwindInfo(ImageBase + FunctionEntry->UnwindInfoAddress);

        if (UnwindInfo == NULL)
        {
            return HRESULT_FROM_WIN32(ERROR_READ_FAULT);
        }

        while (Index < UnwindInfo->CountOfUnwindCodes) {
            PORTABILITY_ASSERT("UnwindPrologue");
        }

        //
        // If chained unwind information is specified, then set the function
        // entry address to the chained function entry and continue the scan.
        // Otherwise, determine the return address if a machine frame was not
        // encountered during the scan of the unwind codes and terminate the
        // scan.
        //
        if ((UnwindInfo->Flags & UNW_FLAG_CHAININFO) != 0) {
            PORTABILITY_ASSERT("UnwindPrologue");
        } else {
            if (MachineFrame == FALSE) {
                ULONG32 FrameAddress = ContextRecord->Ebp;

                ContextRecord->Ebp = MemoryRead32((PULONG)(FrameAddress + 0));
                ContextRecord->Eip = MemoryRead32((PULONG)(FrameAddress + 4));
                ContextRecord->Esp = FrameAddress + 8;
            }

            break;
        }

        //
        // Limit the number of iterations possible for chained function table
        // entries.
        //

        ChainCount += 1;
        UNWINDER_ASSERT(ChainCount <= UNWIND_CHAIN_LIMIT);

    } while (TRUE);

    *FinalFunctionEntry = FunctionEntry;
    return S_OK;
}

DWORD OOPStackUnwinderX86::GetEstabliserFrame(
    __in PCONTEXT ContextRecord,
    __in PUNWIND_INFO UnwindInfo,
    __in ULONG PrologOffset
)
{
    ULONG Index;
    ULONG FrameOffset;

    UNWIND_CODE UnwindOp;

    //
    // If the specified function does not use a frame pointer, then the
    // establisher frame is the contents of the stack pointer. This may
    // not actually be the real establisher frame if control left the
    // function from within the prologue. In this case the establisher
    // frame may be not required since control has not actually entered
    // the function and prologue entries cannot refer to the establisher
    // frame before it has been established, i.e., if it has not been
    // established, then no save unwind codes should be encountered during
    // the unwind operation.
    //
    if (UnwindInfo->FrameRegister == 0) {
        return ContextRecord->Esp;
    }

    // If the specified function uses a frame pointer and control left the
    // function outside of the prologue or the unwind information contains
    // a chained information structure, then the establisher frame is the
    // contents of the frame pointer.
    //
    if ((PrologOffset >= UnwindInfo->SizeOfProlog) ||
        ((UnwindInfo->Flags & UNW_FLAG_CHAININFO) != 0)) {

        FrameOffset = UnwindInfo->FrameOffset;

#ifdef PLATFORM_UNIX
        // If UnwindInfo->FrameOffset == 15 (the maximum value), then there might be a UWOP_SET_FPREG_LARGE.
        // However, it is still legal for a UWOP_SET_FPREG to set UnwindInfo->FrameOffset == 15 (since this
        // was always part of the specification), so we need to look through the UnwindCode array to determine
        // if there is indeed a UWOP_SET_FPREG_LARGE. If we don't find UWOP_SET_FPREG_LARGE, then just use
        // (scaled) FrameOffset of 240, as before. (We don't verify there is a UWOP_SET_FPREG code, but we could.)
        if (FrameOffset == 15) {
            Index = 0;
            while (Index < UnwindInfo->CountOfUnwindCodes) {
                UnwindOp = UnwindInfo->UnwindCode[Index];
                if (UnwindOp.UnwindOp == UWOP_SET_FPREG_LARGE) {
                    FrameOffset = UnwindInfo->UnwindCode[Index + 1].FrameOffset;
                    FrameOffset += UnwindInfo->UnwindCode[Index + 2].FrameOffset << 16;
                    break;
                }

                Index += UnwindOpSlots(UnwindOp);
            }
        }
#endif // PLATFORM_UNIX

        return (&ContextRecord->Eax)[UnwindInfo->FrameRegister] - FrameOffset * 16;
    }

    //
    // If the specified function uses a frame pointer and control left the
    // function from within the prologue, then the set frame pointer unwind
    // code must be looked up in the unwind codes to determine if the
    // contents of the stack pointer or the contents of the frame pointer
    // should be used for the establisher frame. This may not actually be
    // the real establisher frame. In this case the establisher frame may
    // not be required since control has not actually entered the function
    // and prologue entries cannot refer to the establisher frame before it
    // has been established, i.e., if it has not been established, then no
    // save unwind codes should be encountered during the unwind operation.
    //
    // N.B. The correctness of these assumptions is based on the ordering of
    //      unwind codes.
    //
    FrameOffset = UnwindInfo->FrameOffset;
    Index = 0;

    while (Index < UnwindInfo->CountOfUnwindCodes) {
        UnwindOp = UnwindInfo->UnwindCode[Index];
        if (UnwindOp.UnwindOp == UWOP_SET_FPREG) {
            break;
        }
#ifdef PLATFORM_UNIX
        else if (UnwindOp.UnwindOp == UWOP_SET_FPREG_LARGE) {
            UNWINDER_ASSERT(UnwindInfo->FrameOffset == 15);
            FrameOffset = UnwindInfo->UnwindCode[Index + 1].FrameOffset;
            FrameOffset += UnwindInfo->UnwindCode[Index + 2].FrameOffset << 16;
            break;
        }
#endif // PLATFORM_UNIX

        Index += UnwindOpSlots(UnwindOp);
    }

    if (PrologOffset >= UnwindInfo->UnwindCode[Index].CodeOffset) {
        return (&ContextRecord->Eax)[UnwindInfo->FrameRegister] - FrameOffset * 16;
    }

    return ContextRecord->Esp;
}

BOOL IsInEpilogue(VOID)
{
    // TODO Implement this
    return FALSE;
}

/*++

Routine Description:

    This function virtually unwinds the specified function by executing its
    prologue code backward or its epilogue code forward.

    If a context pointers record is specified, then the address where each
    nonvolatile registers is restored from is recorded in the appropriate
    element of the context pointers record.

Arguments:

    HandlerType - Supplies the handler type expected for the virtual unwind.
        This may be either an exception or an unwind handler. A flag may
        optionally be supplied to avoid epilogue detection if it is known
        the specified control PC is not located inside a function epilogue.

    ImageBase - Supplies the base address of the image that contains the
        function being unwound.

    ControlPc - Supplies the address where control left the specified
        function.

    FunctionEntry - Supplies the address of the function table entry for the
        specified function.

    ContextRecord - Supplies the address of a context record.


    HandlerData - Supplies a pointer to a variable that receives a pointer
        the the language handler data.

    EstablisherFrame - Supplies a pointer to a variable that receives the
        the establisher frame pointer value.

    ContextPointers - Supplies an optional pointer to a context pointers
        record.

    HandlerRoutine - Supplies an optional pointer to a variable that receives
        the handler routine address.  If control did not leave the specified
        function in either the prologue or an epilogue and a handler of the
        proper type is associated with the function, then the address of the
        language specific exception handler is returned. Otherwise, NULL is
        returned.
--*/
HRESULT
OOPStackUnwinderX86::VirtualUnwind(
    __in DWORD HandlerType,
    __in DWORD ImageBase,
    __in DWORD ControlPc,
    __in _PIMAGE_RUNTIME_FUNCTION_ENTRY FunctionEntry,
    __inout PCONTEXT ContextRecord,
    __out PVOID *HandlerData,
    __out PDWORD EstablisherFrame,
    __inout_opt PKNONVOLATILE_CONTEXT_POINTERS ContextPointers,
    __deref_opt_out_opt PEXCEPTION_ROUTINE *HandlerRoutine
    )
{
    PEXCEPTION_ROUTINE FoundHandler;

    ULONG PrologOffset;
    PUNWIND_INFO UnwindInfo;
    ULONG UnwindVersion;

    FoundHandler = NULL;
    UnwindInfo = GetUnwindInfo(ImageBase + FunctionEntry->UnwindInfoAddress);
    if (UnwindInfo == NULL)
    {
        return HRESULT_FROM_WIN32(ERROR_READ_FAULT);
    }

    UnwindVersion = UnwindInfo->Version;

    PrologOffset = (ULONG)(ControlPc - (FunctionEntry->BeginAddress + ImageBase));

    *EstablisherFrame = GetEstabliserFrame(ContextRecord, UnwindInfo, PrologOffset);

    //
    // Check if control left the specified function during an epilogue
    // sequence and emulate the execution of the epilogue forward and
    // return no exception handler.
    //
    // If the unwind version indicates the absence of epilogue unwind codes
    // this is done by emulating the instruction stream. Otherwise, epilogue
    // detection and emulation is performed using the function unwind codes.
    //

    BOOL InEpilogue = FALSE;

    if (UnwindVersion < 2) {
        InEpilogue = IsInEpilogue();

        if (InEpilogue != FALSE) {
            PORTABILITY_ASSERT("OOPStackUnwinderX86::VirtualUnwind");
        }
    } else if (UnwindInfo->CountOfUnwindCodes != 0) {

        UNWINDER_ASSERT(UnwindVersion >= 2);
        PORTABILITY_ASSERT("OOPStackUnwinderX86::VirtualUnwind");

    }

    //
    // Control left the specified function outside an epilogue. Unwind the
    // subject function and any chained unwind information.
    //
    HRESULT Status;

    Status = UnwindPrologue(ImageBase,
                            ControlPc,
                            *EstablisherFrame,
                            FunctionEntry,
                            ContextRecord,
                            ContextPointers,
                            &FunctionEntry);

    if (Status != S_OK) {
        return Status;
    }

    //
    // If control left the specified function outside of the prologue and
    // the function has a handler that matches the specified type, then
    // return the address of the language specific exception handler.
    // Otherwise, return NULL.
    //
    if (HandlerType != 0) {
        UNWINDER_ASSERT(UnwindInfo != NULL);

        if ((PrologOffset >= UnwindInfo->SizeOfProlog) &&
            ((UnwindInfo->Flags & HandlerType) != 0)) {

            ULONG Index = UnwindInfo->CountOfUnwindCodes;

            if ((Index & 1) != 0) {
                Index += 1;
            }

            *HandlerData = &UnwindInfo->UnwindCode[Index + 2];
            FoundHandler = (PEXCEPTION_ROUTINE)(*((PULONG)&UnwindInfo->UnwindCode[Index]) + ImageBase);
        }
    }

    if (ARGUMENT_PRESENT(HandlerRoutine)) {
        *HandlerRoutine = FoundHandler;
    }

    return S_OK;
}

//---------------------------------------------------------------------------------------
//
// This function behaves like the RtlVirtualUnwind in Windows.
// It virtually unwinds the specified function by executing its
// prologue code backward or its epilogue code forward.
//
// If a context pointers record is specified, then the address where each
// nonvolatile registers is restored from is recorded in the appropriate
// element of the context pointers record.
//
// Arguments:
//
//     HandlerType - Supplies the handler type expected for the virtual unwind.
//         This may be either an exception or an unwind handler. A flag may
//         optionally be supplied to avoid epilogue detection if it is known
//         the specified control PC is not located inside a function epilogue.
//
//     ImageBase - Supplies the base address of the image that contains the
//         function being unwound.
//
//     ControlPc - Supplies the address where control left the specified
//         function.
//
//     FunctionEntry - Supplies the address of the function table entry for the
//         specified function.
//
//     ContextRecord - Supplies the address of a context record.
//
//     HandlerData - Supplies a pointer to a variable that receives a pointer
//         the the language handler data.
//
//     EstablisherFrame - Supplies a pointer to a variable that receives the
//         the establisher frame pointer value.
//
//     ContextPointers - Supplies an optional pointer to a context pointers
//         record.
//
// Return value:
//
//     The handler routine address.  If control did not leave the specified
//     function in either the prologue or an epilogue and a handler of the
//     proper type is associated with the function, then the address of the
//     language specific exception handler is returned. Otherwise, NULL is
//     returned.
//
EXTERN_C
NTSYSAPI
PEXCEPTION_ROUTINE
NTAPI
RtlVirtualUnwind (
    __in DWORD HandlerType,
    __in DWORD ImageBase,
    __in DWORD ControlPc,
    __in PRUNTIME_FUNCTION FunctionEntry,
    __inout PT_CONTEXT ContextRecord,
    __out PVOID *HandlerData,
    __out PDWORD EstablisherFrame,
    __inout_opt PT_KNONVOLATILE_CONTEXT_POINTERS ContextPointers
    )
{
    PEXCEPTION_ROUTINE handlerRoutine;

    HRESULT res = OOPStackUnwinderX86::VirtualUnwind(
        HandlerType,
        ImageBase,
        ControlPc,
        (_PIMAGE_RUNTIME_FUNCTION_ENTRY)FunctionEntry,
        ContextRecord,
        HandlerData,
        EstablisherFrame,
        ContextPointers,
        &handlerRoutine);

    _ASSERTE(SUCCEEDED(res));

    return handlerRoutine;
}
#endif // WIN64EXCEPTIONS
