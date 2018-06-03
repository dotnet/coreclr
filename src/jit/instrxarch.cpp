// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "jitpch.h"
#include "instr.h"

// clang-format off
static const InstructionInfo instructionInfoArray[] = {
#ifdef DEBUG
    #define INSTRUCTION(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) \
        {INS_##id, nm, {mr, mi, rm, a4, rr}, um, static_cast<InsFlag>((InsFlag_FloatingPoint*fp)|(InsFlag_ReadsFlags*rf)|(InsFlag_WritesFlags*wf))},
#else
    #define INSTRUCTION(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) \
        {{mr, mi, rm, a4, rr}, um, static_cast<InsFlag>((InsFlag_FloatingPoint*fp) | (InsFlag_ReadsFlags*rf) | (InsFlag_WritesFlags*wf))},
#endif

#include "instrs.h"
};
// clang-format on

//------------------------------------------------------------------------
// get: Gets the InstructionInfo associated with a given instruction
//
// Arguments:
//    id -- The instruction associated with the Instruction to lookup
//
// Return Value:
//    The InstructionInfo associated with id
const InstructionInfo& InstructionInfo::get(instruction id)
{
#ifdef DEBUG
    assert((id >= 0) && (id < INS_count));
    assert(instructionInfoArray[id].id == id);
#endif

    return instructionInfoArray[id];
}
