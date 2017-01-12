// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

#include "stdafx.h"
#include "unwinder_xarch.h"

#ifdef WIN64EXCEPTIONS
ULONG OOPStackUnwinderXARCH::UnwindOpSlots(__in UNWIND_CODE UnwindCode)
/*++

Routine Description:

    This routine determines the number of unwind code slots ultimately
    consumed by an unwind code sequence.

Arguments:

    UnwindCode - Supplies the first unwind code in the sequence.

Return Value:

    Returns the total count of the number of slots consumed by the unwind
    code sequence.

--*/
{

    ULONG Slots;
    ULONG UnwindOp;

    //
    // UWOP_SPARE_CODE may be found in very old x64 images.
    //

    UnwindOp = UnwindCode.UnwindOp;

    UNWINDER_ASSERT(UnwindOp != UWOP_SPARE_CODE);
    UNWINDER_ASSERT(UnwindOp < sizeof(UnwindOpExtraSlotTable));

    Slots = UnwindOpExtraSlotTable[UnwindOp] + 1;
    if ((UnwindOp == UWOP_ALLOC_LARGE) && (UnwindCode.OpInfo != 0)) {
        Slots += 1;
    }

    return Slots;
}

#ifdef DACCESS_COMPILE

//---------------------------------------------------------------------------------------
//
// Given the target address of an UNWIND_INFO structure, this function retrieves all the memory used for
// the UNWIND_INFO, including the variable size array of UNWIND_CODE.  The function returns a host copy
// of the UNWIND_INFO.
//
// Arguments:
//    taUnwindInfo - the target address of an UNWIND_INFO
//
// Return Value:
//    Return a host copy of the UNWIND_INFO, including the array of UNWIND_CODE.
//
// Notes:
//    The host copy of UNWIND_INFO is created from DAC memory, which will be flushed when the DAC cache
//    is flushed (i.e. when the debugee is continued).  Thus, the caller doesn't need to worry about freeing
//    this memory.
//
UNWIND_INFO * DacGetUnwindInfo(TADDR taUnwindInfo)
{
    PTR_UNWIND_INFO pUnwindInfo = PTR_UNWIND_INFO(taUnwindInfo);
    DWORD cbUnwindInfo = offsetof(UNWIND_INFO, UnwindCode) +
        pUnwindInfo->CountOfUnwindCodes * sizeof(UNWIND_CODE);

    // Check if there is a chained unwind info.  If so, it has an extra RUNTIME_FUNCTION tagged to the end.
    if ((pUnwindInfo->Flags & UNW_FLAG_CHAININFO) != 0)
    {
        // If there is an odd number of UNWIND_CODE, we need to adjust for alignment.
        if ((pUnwindInfo->CountOfUnwindCodes & 1) != 0)
        {
            cbUnwindInfo += sizeof(UNWIND_CODE);
        }
        cbUnwindInfo += sizeof(T_RUNTIME_FUNCTION);
    }
    return reinterpret_cast<UNWIND_INFO *>(DacInstantiateTypeByAddress(taUnwindInfo, cbUnwindInfo, true));
}

//---------------------------------------------------------------------------------------
//
// This function just wraps the DacGetUnwindInfo.
// The DacGetUnwindInfo is called from other places outside of the unwinder, so it
// cannot be merged into the body of this method.
//
UNWIND_INFO * OOPStackUnwinderXARCH::GetUnwindInfo(TADDR taUnwindInfo)
{
    return DacGetUnwindInfo(taUnwindInfo);
}

#else // DACCESS_COMPILE

//---------------------------------------------------------------------------------------
//
// Return UNWIND_INFO pointer for the given address.
//
UNWIND_INFO * OOPStackUnwinderXARCH::GetUnwindInfo(TADDR taUnwindInfo)
{
    return (UNWIND_INFO *)taUnwindInfo;
}

#endif // DACCESS_COMPILE
#endif // WIN64EXCEPTIONS
