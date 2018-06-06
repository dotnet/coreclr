// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _INSTR_XARCH_H_
#define _INSTR_XARCH_H_

#if !defined(_TARGET_XARCH_)
#error Unexpected target type
#endif

enum InsEncodingFmt : unsigned int
{
    // "R/M, [reg]" addressing mode
    InsEncodingFmt_mr,

    // "R/M, icon" addressing mode
    InsEncodingFmt_mi,

    // "reg, R/M" addression mode
    InsEncodingFmt_rm,

    // "eax, i32" addressing mode
    InsEncodingFmt_a4,

    // "reg, reg" addressing mode
    InsEncodingFmt_rr,

    InsEncodingFmt_none,
    InsEncodingFmt_count = InsEncodingFmt_none
};

enum InsFlag : uint8_t
{
    InsFlag_None = 0,

    // Floating point instructions
    InsFlag_FloatingPoint = 1,

    // Reads flags
    InsFlag_ReadsFlags = 2,

    // Writes flags
    InsFlag_WritesFlags = 4,
};

struct InstructionInfo
{
#ifdef DEBUG
    instruction id;
    const char* name;
#endif

    size_t         encoding[5];
    insUpdateModes updateMode;
    InsFlag        flags;

    static const InstructionInfo& get(instruction id);

// Member lookup

#ifdef DEBUG
    static const char* getName(instruction id)
    {
        return get(id).name;
    }
#endif

    static size_t getEncoding(instruction id, InsEncodingFmt format)
    {
        assert(supportsEncoding(id, format));
        return get(id).encoding[format];
    }

    static insUpdateModes getUpdateMode(instruction id)
    {
        return get(id).updateMode;
    }

    static bool supportsEncoding(instruction id, InsEncodingFmt format)
    {
        assert((format >= 0) && (format < InsEncodingFmt_count));
        return get(id).encoding[format] != BAD_CODE;
    }

    // Flags lookup

    static bool isFloatingPoint(instruction id)
    {
        InsFlag flags = get(id).flags;
        return (flags & InsFlag_FloatingPoint) != 0;
    }

    static bool readsFlags(instruction id)
    {
        InsFlag flags = get(id).flags;
        return (flags & InsFlag_ReadsFlags) != 0;
    }

    static bool writesFlags(instruction id)
    {
        InsFlag flags = get(id).flags;
        return (flags & InsFlag_WritesFlags) != 0;
    }
};

#endif // _INSTR_XARCH_H_
