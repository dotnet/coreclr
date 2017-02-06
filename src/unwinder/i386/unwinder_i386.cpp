// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

#include "stdafx.h"
#include "unwinder_i386.h"

#ifdef WIN64EXCEPTIONS
class UnwindFrameReader : public IUnwindFrameReader
{
private:
    PCONTEXT pContextRecord;

public:
    UnwindFrameReader(PCONTEXT pContextRecord)
    {
        this->pContextRecord = pContextRecord;
    }

public:
    virtual DWORD GetSP(void) { return pContextRecord->Esp; }
    virtual DWORD GetFP(void) { return pContextRecord->Ebp; }
    virtual DWORD GetPC(void) { return pContextRecord->Eip; }
};

struct UnwindFrameListener : public IUnwindFrameListener
{
private:
    PCONTEXT pContextRecord;
    PKNONVOLATILE_CONTEXT_POINTERS pContextPointers;

    T_KNONVOLATILE_CONTEXT_POINTERS ctxPtrs;

public:
    DWORD EstablisherFrame;

public:
    UnwindFrameListener(PCONTEXT pContextRecord, PKNONVOLATILE_CONTEXT_POINTERS pContextPointers)
    {
        this->pContextRecord = pContextRecord;
        this->pContextPointers = (pContextPointers) ? pContextPointers : &ctxPtrs;

        this->EstablisherFrame = 0xdeadbeaf;
    }

    virtual ~UnwindFrameListener() = default;

#define NOTIFY_METHOD(reg) \
    virtual void Notify##reg##Location(PDWORD loc) \
    { \
        pContextRecord->reg = *loc; \
        pContextPointers->reg = loc; \
    }

    NOTIFY_METHOD(Eax);
    NOTIFY_METHOD(Ebx);
    NOTIFY_METHOD(Ecx);
    NOTIFY_METHOD(Edx);
    NOTIFY_METHOD(Esi);
    NOTIFY_METHOD(Edi);
    NOTIFY_METHOD(Ebp);

#undef NOTIFY_METHOD

    virtual void NotifySP(DWORD SP, DWORD stackArgumentSize)
    {
        pContextRecord->Esp = SP;
        EstablisherFrame = SP;
    }

    virtual void NotifyPCLocation(PDWORD loc)
    {
        pContextRecord->Eip = *loc;
    }
};

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
    if (HandlerRoutine != NULL)
    {
        *HandlerRoutine = NULL;
    }

    UnwindFrameReader unwindFrameReader(ContextRecord);
    UnwindFrameListener unwindFrameListener(ContextRecord, ContextPointers);

    CodeManState codeManState;
    codeManState.dwIsSet = 0;

    EECodeInfo codeInfo;
    codeInfo.Init((PCODE) ControlPc);

    if (!UnwindStackFrame(&unwindFrameReader, &unwindFrameListener, &codeInfo, UpdateAllRegs, &codeManState, NULL))
    {
        return HRESULT_FROM_WIN32(ERROR_READ_FAULT);
    }

    ContextRecord->ContextFlags |= CONTEXT_UNWOUND_TO_CALL;

    // For x86, the value of Establisher Frame Pointer is Caller SP
    //
    // (Please refers to CLR ABI for details)
    *EstablisherFrame = unwindFrameListener.EstablisherFrame;
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
