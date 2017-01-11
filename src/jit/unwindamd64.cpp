// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                              UnwindInfo                                   XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#if defined(_TARGET_AMD64_)
#ifdef UNIX_AMD64_ABI
int Compiler::mapRegNumToDwarfReg(regNumber reg)
{
    int dwarfReg = DWARF_REG_ILLEGAL;

    switch (reg)
    {
        case REG_RAX:
            dwarfReg = 0;
            break;
        case REG_RCX:
            dwarfReg = 2;
            break;
        case REG_RDX:
            dwarfReg = 1;
            break;
        case REG_RBX:
            dwarfReg = 3;
            break;
        case REG_RSP:
            dwarfReg = 7;
            break;
        case REG_RBP:
            dwarfReg = 6;
            break;
        case REG_RSI:
            dwarfReg = 4;
            break;
        case REG_RDI:
            dwarfReg = 5;
            break;
        case REG_R8:
            dwarfReg = 8;
            break;
        case REG_R9:
            dwarfReg = 9;
            break;
        case REG_R10:
            dwarfReg = 10;
            break;
        case REG_R11:
            dwarfReg = 11;
            break;
        case REG_R12:
            dwarfReg = 12;
            break;
        case REG_R13:
            dwarfReg = 13;
            break;
        case REG_R14:
            dwarfReg = 14;
            break;
        case REG_R15:
            dwarfReg = 15;
            break;
        case REG_XMM0:
            dwarfReg = 17;
            break;
        case REG_XMM1:
            dwarfReg = 18;
            break;
        case REG_XMM2:
            dwarfReg = 19;
            break;
        case REG_XMM3:
            dwarfReg = 20;
            break;
        case REG_XMM4:
            dwarfReg = 21;
            break;
        case REG_XMM5:
            dwarfReg = 22;
            break;
        case REG_XMM6:
            dwarfReg = 23;
            break;
        case REG_XMM7:
            dwarfReg = 24;
            break;
        case REG_XMM8:
            dwarfReg = 25;
            break;
        case REG_XMM9:
            dwarfReg = 26;
            break;
        case REG_XMM10:
            dwarfReg = 27;
            break;
        case REG_XMM11:
            dwarfReg = 28;
            break;
        case REG_XMM12:
            dwarfReg = 29;
            break;
        case REG_XMM13:
            dwarfReg = 30;
            break;
        case REG_XMM14:
            dwarfReg = 31;
            break;
        case REG_XMM15:
            dwarfReg = 32;
            break;
        default:
            noway_assert(!"unexpected REG_NUM");
    }

    return dwarfReg;
}

void Compiler::createCfiCode(FuncInfoDsc* func, UCHAR codeOffset, UCHAR cfiOpcode, USHORT dwarfReg, INT offset)
{
    CFI_CODE cfiEntry(codeOffset, cfiOpcode, dwarfReg, offset);
    func->cfiCodes->push_back(cfiEntry);
}
#endif // UNIX_AMD64_ABI

//------------------------------------------------------------------------
// Compiler::unwindGetCurrentOffset: Calculate the current byte offset of the
// prolog being generated.
//
// Arguments:
//    func - The main function or funclet of interest.
//
// Return Value:
//    The byte offset of the prolog currently being generated.
//
UNATIVE_OFFSET Compiler::unwindGetCurrentOffset(FuncInfoDsc* func)
{
    assert(compGeneratingProlog);
    UNATIVE_OFFSET offset;
    if (func->funKind == FUNC_ROOT)
    {
        offset = genEmitter->emitGetPrologOffsetEstimate();
    }
    else
    {
        assert(func->startLoc != nullptr);
        offset = func->startLoc->GetFuncletPrologOffset(genEmitter);
    }

    return offset;
}

//------------------------------------------------------------------------
// Compiler::unwindPush: Record a push/save of a register.
//
// Arguments:
//    reg - The register being pushed/saved.
//
void Compiler::unwindPush(regNumber reg)
{
#ifdef UNIX_AMD64_ABI
    if (generateCFIUnwindCodes())
    {
        unwindPushCFI(reg);
    }
    else
#endif // UNIX_AMD64_ABI
    {
        unwindPushWindows(reg);
    }
}

void Compiler::unwindPushWindows(regNumber reg)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    assert(func->unwindHeader.Version == 1);            // Can't call this before unwindBegProlog
    assert(func->unwindHeader.CountOfUnwindCodes == 0); // Can't call this after unwindReserve
    assert(func->unwindCodeSlot > sizeof(UNWIND_CODE));
    UNWIND_CODE* code     = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);
    code->CodeOffset = (BYTE)cbProlog;

    if ((RBM_CALLEE_SAVED & genRegMask(reg))
#if ETW_EBP_FRAMED
        // In case of ETW_EBP_FRAMED defined the REG_FPBASE (RBP)
        // is excluded from the callee-save register list.
        // Make sure the register gets PUSH unwind info in this case,
        // since it is pushed as a frame register.
        || (reg == REG_FPBASE)
#endif // ETW_EBP_FRAMED
            )
    {
        code->UnwindOp = UWOP_PUSH_NONVOL;
        code->OpInfo   = (BYTE)reg;
    }
    else
    {
        // Push of a volatile register is just a small stack allocation
        code->UnwindOp = UWOP_ALLOC_SMALL;
        code->OpInfo   = 0;
    }
}

#ifdef UNIX_AMD64_ABI
void Compiler::unwindPushCFI(regNumber reg)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);

    createCfiCode(func, cbProlog, CFI_ADJUST_CFA_OFFSET, DWARF_REG_ILLEGAL, 8);
    if ((RBM_CALLEE_SAVED & genRegMask(reg))
#if ETW_EBP_FRAMED
        // In case of ETW_EBP_FRAMED defined the REG_FPBASE (RBP)
        // is excluded from the callee-save register list.
        // Make sure the register gets PUSH unwind info in this case,
        // since it is pushed as a frame register.
        || (reg == REG_FPBASE)
#endif // ETW_EBP_FRAMED
            )
    {
        createCfiCode(func, cbProlog, CFI_REL_OFFSET, mapRegNumToDwarfReg(reg));
    }
}
#endif // UNIX_AMD64_ABI

//------------------------------------------------------------------------
// Compiler::unwindAllocStack: Record a stack frame allocation (sub sp, X).
//
// Arguments:
//    size - The size of the stack frame allocation (the amount subtracted from the stack pointer).
//
void Compiler::unwindAllocStack(unsigned size)
{
#ifdef UNIX_AMD64_ABI
    if (generateCFIUnwindCodes())
    {
        unwindAllocStackCFI(size);
    }
    else
#endif // UNIX_AMD64_ABI
    {
        unwindAllocStackWindows(size);
    }
}

void Compiler::unwindAllocStackWindows(unsigned size)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    assert(func->unwindHeader.Version == 1);            // Can't call this before unwindBegProlog
    assert(func->unwindHeader.CountOfUnwindCodes == 0); // Can't call this after unwindReserve
    assert(size % 8 == 0);                              // Stack size is *always* 8 byte aligned
    UNWIND_CODE* code;
    if (size <= 128)
    {
        assert(func->unwindCodeSlot > sizeof(UNWIND_CODE));
        code           = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
        code->UnwindOp = UWOP_ALLOC_SMALL;
        code->OpInfo   = (size - 8) / 8;
    }
    else if (size <= 0x7FFF8)
    {
        assert(func->unwindCodeSlot > (sizeof(UNWIND_CODE) + sizeof(USHORT)));
        USHORT* codedSize = (USHORT*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(USHORT)];
        *codedSize        = (USHORT)(size / 8);
        code              = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
        code->UnwindOp    = UWOP_ALLOC_LARGE;
        code->OpInfo      = 0;
    }
    else
    {
        assert(func->unwindCodeSlot > (sizeof(UNWIND_CODE) + sizeof(ULONG)));
        ULONG* codedSize = (ULONG*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(ULONG)];
        *codedSize       = size;
        code             = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
        code->UnwindOp   = UWOP_ALLOC_LARGE;
        code->OpInfo     = 1;
    }
    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);
    code->CodeOffset = (BYTE)cbProlog;
}

#ifdef UNIX_AMD64_ABI
void Compiler::unwindAllocStackCFI(unsigned size)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);
    createCfiCode(func, cbProlog, CFI_ADJUST_CFA_OFFSET, DWARF_REG_ILLEGAL, size);
}
#endif // UNIX_AMD64_ABI

//------------------------------------------------------------------------
// Compiler::unwindSetFrameReg: Record a frame register.
//
// Arguments:
//    reg    - The register being set as the frame register.
//    offset - The offset from the current stack pointer that the frame pointer will point at.
//
void Compiler::unwindSetFrameReg(regNumber reg, unsigned offset)
{
#ifdef UNIX_AMD64_ABI
    if (generateCFIUnwindCodes())
    {
        unwindSetFrameRegCFI(reg, offset);
    }
    else
#endif // UNIX_AMD64_ABI
    {
        unwindSetFrameRegWindows(reg, offset);
    }
}

void Compiler::unwindSetFrameRegWindows(regNumber reg, unsigned offset)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    assert(func->unwindHeader.Version == 1);            // Can't call this before unwindBegProlog
    assert(func->unwindHeader.CountOfUnwindCodes == 0); // Can't call this after unwindReserve
    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);

    func->unwindHeader.FrameRegister = (BYTE)reg;

#ifdef PLATFORM_UNIX
    if (offset > 240)
    {
        // On Unix only, we have a CLR-only extension to the AMD64 unwind codes: UWOP_SET_FPREG_LARGE.
        // It has a 32-bit offset (scaled). You must set UNWIND_INFO.FrameOffset to 15. The 32-bit
        // offset follows in 2 UNWIND_CODE fields.

        assert(func->unwindCodeSlot > (sizeof(UNWIND_CODE) + sizeof(ULONG)));
        ULONG* codedSize = (ULONG*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(ULONG)];
        assert(offset % 16 == 0);
        *codedSize = offset / 16;

        UNWIND_CODE* code              = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
        code->CodeOffset               = (BYTE)cbProlog;
        code->OpInfo                   = 0;
        code->UnwindOp                 = UWOP_SET_FPREG_LARGE;
        func->unwindHeader.FrameOffset = 15;
    }
    else
#endif // PLATFORM_UNIX
    {
        assert(func->unwindCodeSlot > sizeof(UNWIND_CODE));
        UNWIND_CODE* code = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
        code->CodeOffset  = (BYTE)cbProlog;
        code->OpInfo      = 0;
        code->UnwindOp    = UWOP_SET_FPREG;
        assert(offset <= 240);
        assert(offset % 16 == 0);
        func->unwindHeader.FrameOffset = offset / 16;
    }
}

#ifdef UNIX_AMD64_ABI
//------------------------------------------------------------------------
// Compiler::unwindSetFrameRegCFI: Record a cfi info for a frame register set.
//
// Arguments:
//    reg    - The register being set as the frame register.
//    offset - The offset from the current stack pointer that the frame pointer will point at.
//
void Compiler::unwindSetFrameRegCFI(regNumber reg, unsigned offset)
{
    assert(compGeneratingProlog);
    FuncInfoDsc* func = funCurrentFunc();

    unsigned int cbProlog = unwindGetCurrentOffset(func);
    noway_assert((BYTE)cbProlog == cbProlog);

    createCfiCode(func, cbProlog, CFI_DEF_CFA_REGISTER, mapRegNumToDwarfReg(reg));
    if (offset != 0)
    {
        // before: cfa = rsp + old_cfa_offset;
        //         rbp = rsp + offset;
        // after: cfa should be based on rbp, but points to the old address:
        //         rsp + old_cfa_offset == rbp + old_cfa_offset + adjust;
        // adjust = -offset;
        int adjust = -(int)offset;
        createCfiCode(func, cbProlog, CFI_ADJUST_CFA_OFFSET, DWARF_REG_ILLEGAL, adjust);
    }
}
#endif // UNIX_AMD64_ABI

//------------------------------------------------------------------------
// Compiler::unwindSaveReg: Record a register save.
//
// Arguments:
//    reg    - The register being saved.
//    offset - The offset from the current stack pointer where the register is being saved.
//
void Compiler::unwindSaveReg(regNumber reg, unsigned offset)
{
#ifdef UNIX_AMD64_ABI
    if (generateCFIUnwindCodes())
    {
        unwindSaveRegCFI(reg, offset);
    }
    else
#endif // UNIX_AMD64_ABI
    {
        unwindSaveRegWindows(reg, offset);
    }
}

void Compiler::unwindSaveRegWindows(regNumber reg, unsigned offset)
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    assert(func->unwindHeader.Version == 1);            // Can't call this before unwindBegProlog
    assert(func->unwindHeader.CountOfUnwindCodes == 0); // Can't call this after unwindReserve
    if (RBM_CALLEE_SAVED & genRegMask(reg))
    {
        UNWIND_CODE* code;
        if (offset < 0x80000)
        {
            assert(func->unwindCodeSlot > (sizeof(UNWIND_CODE) + sizeof(USHORT)));
            USHORT* codedSize = (USHORT*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(USHORT)];
            code              = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];

            // As per AMD64 ABI, if saving entire xmm reg, then offset need to be scaled by 16.
            if (genIsValidFloatReg(reg))
            {
                *codedSize     = (USHORT)(offset / 16);
                code->UnwindOp = UWOP_SAVE_XMM128;
            }
            else
            {
                *codedSize     = (USHORT)(offset / 8);
                code->UnwindOp = UWOP_SAVE_NONVOL;
            }
        }
        else
        {
            assert(func->unwindCodeSlot > (sizeof(UNWIND_CODE) + sizeof(ULONG)));
            ULONG* codedSize = (ULONG*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(ULONG)];
            *codedSize       = offset;
            code             = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot -= sizeof(UNWIND_CODE)];
            code->UnwindOp   = (genIsValidFloatReg(reg)) ? UWOP_SAVE_XMM128_FAR : UWOP_SAVE_NONVOL_FAR;
        }
        code->OpInfo          = (BYTE)reg;
        unsigned int cbProlog = unwindGetCurrentOffset(func);
        noway_assert((BYTE)cbProlog == cbProlog);
        code->CodeOffset = (BYTE)cbProlog;
    }
}

#ifdef UNIX_AMD64_ABI
void Compiler::unwindSaveRegCFI(regNumber reg, unsigned offset)
{
    assert(compGeneratingProlog);

    if (RBM_CALLEE_SAVED & genRegMask(reg))
    {
        FuncInfoDsc* func = funCurrentFunc();

        unsigned int cbProlog = unwindGetCurrentOffset(func);
        noway_assert((BYTE)cbProlog == cbProlog);
        createCfiCode(func, cbProlog, CFI_REL_OFFSET, mapRegNumToDwarfReg(reg), offset);
    }
}
#endif // UNIX_AMD64_ABI
#endif // _TARGET_AMD64_
