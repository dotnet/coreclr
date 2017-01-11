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

#ifdef DEBUG

//------------------------------------------------------------------------
// DumpUnwindInfo: Dump the unwind data.
//
// Arguments:
//    isHotCode   - true if this unwind data is for the hot section, false otherwise.
//    startOffset - byte offset of the code start that this unwind data represents.
//    endOffset   - byte offset of the code end   that this unwind data represents.
//    pHeader     - pointer to the unwind data blob.
//
void DumpUnwindInfo(bool                     isHotCode,
                    UNATIVE_OFFSET           startOffset,
                    UNATIVE_OFFSET           endOffset,
                    const UNWIND_INFO* const pHeader)
{
    printf("Unwind Info%s:\n", isHotCode ? "" : " COLD");
    printf("  >> Start offset   : 0x%06x (not in unwind data)\n", dspOffset(startOffset));
    printf("  >>   End offset   : 0x%06x (not in unwind data)\n", dspOffset(endOffset));

    if (pHeader == nullptr)
    {
        // Cold AMD64 code doesn't have unwind info; the VM creates chained unwind info.
        assert(!isHotCode);
        return;
    }

    printf("  Version           : %u\n", pHeader->Version);
    printf("  Flags             : 0x%02x", pHeader->Flags);
    if (pHeader->Flags)
    {
        const UCHAR flags = pHeader->Flags;
        printf(" (");
        if (flags & UNW_FLAG_EHANDLER)
        {
            printf(" UNW_FLAG_EHANDLER");
        }
        if (flags & UNW_FLAG_UHANDLER)
        {
            printf(" UNW_FLAG_UHANDLER");
        }
        if (flags & UNW_FLAG_CHAININFO)
        {
            printf(" UNW_FLAG_CHAININFO");
        }
        printf(")");
    }
    printf("\n");
    printf("  SizeOfProlog      : 0x%02X\n", pHeader->SizeOfProlog);
    printf("  CountOfUnwindCodes: %u\n", pHeader->CountOfUnwindCodes);
    printf("  FrameRegister     : %s (%u)\n",
           (pHeader->FrameRegister == 0) ? "none" : getRegName(pHeader->FrameRegister),
           pHeader->FrameRegister); // RAX (0) is not allowed as a frame register
    if (pHeader->FrameRegister == 0)
    {
        printf("  FrameOffset       : N/A (no FrameRegister) (Value=%u)\n", pHeader->FrameOffset);
    }
    else
    {
        printf("  FrameOffset       : %u * 16 = 0x%02X\n", pHeader->FrameOffset, pHeader->FrameOffset * 16);
    }
    printf("  UnwindCodes       :\n");

    for (unsigned i = 0; i < pHeader->CountOfUnwindCodes; i++)
    {
        unsigned                 offset;
        const UNWIND_CODE* const pCode = &(pHeader->UnwindCode[i]);
        switch (pCode->UnwindOp)
        {
            case UWOP_PUSH_NONVOL:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_PUSH_NONVOL (%u)     OpInfo: %s (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, getRegName(pCode->OpInfo), pCode->OpInfo);
                break;

            case UWOP_ALLOC_LARGE:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_ALLOC_LARGE (%u)     OpInfo: %u - ", pCode->CodeOffset,
                       pCode->UnwindOp, pCode->OpInfo);
                if (pCode->OpInfo == 0)
                {
                    i++;
                    printf("Scaled small  \n      Size: %u * 8 = %u = 0x%05X\n", pHeader->UnwindCode[i].FrameOffset,
                           pHeader->UnwindCode[i].FrameOffset * 8, pHeader->UnwindCode[i].FrameOffset * 8);
                }
                else if (pCode->OpInfo == 1)
                {
                    i++;
                    printf("Unscaled large\n      Size: %u = 0x%08X\n\n", *(ULONG*)&(pHeader->UnwindCode[i]),
                           *(ULONG*)&(pHeader->UnwindCode[i]));
                    i++;
                }
                else
                {
                    printf("Unknown\n");
                }
                break;

            case UWOP_ALLOC_SMALL:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_ALLOC_SMALL (%u)     OpInfo: %u * 8 + 8 = %u = 0x%02X\n",
                       pCode->CodeOffset, pCode->UnwindOp, pCode->OpInfo, pCode->OpInfo * 8 + 8, pCode->OpInfo * 8 + 8);
                break;

            case UWOP_SET_FPREG:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SET_FPREG (%u)       OpInfo: Unused (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, pCode->OpInfo); // This should be zero
                break;

#ifdef PLATFORM_UNIX

            case UWOP_SET_FPREG_LARGE:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SET_FPREG_LARGE (%u) OpInfo: Unused (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, pCode->OpInfo); // This should be zero
                i++;
                offset = *(ULONG*)&(pHeader->UnwindCode[i]);
                i++;
                printf("      Scaled Offset: %u * 16 = %u = 0x%08X\n", offset, offset * 16, offset * 16);
                if ((offset & 0xF0000000) != 0)
                {
                    printf("      Illegal unscaled offset: too large\n");
                }
                break;

#endif // PLATFORM_UNIX

            case UWOP_SAVE_NONVOL:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SAVE_NONVOL (%u)     OpInfo: %s (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, getRegName(pCode->OpInfo), pCode->OpInfo);
                i++;
                printf("      Scaled Small Offset: %u * 8 = %u = 0x%05X\n", pHeader->UnwindCode[i].FrameOffset,
                       pHeader->UnwindCode[i].FrameOffset * 8, pHeader->UnwindCode[i].FrameOffset * 8);
                break;

            case UWOP_SAVE_NONVOL_FAR:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SAVE_NONVOL_FAR (%u) OpInfo: %s (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, getRegName(pCode->OpInfo), pCode->OpInfo);
                i++;
                printf("      Unscaled Large Offset: 0x%08X\n\n", *(ULONG*)&(pHeader->UnwindCode[i]));
                i++;
                break;

            case UWOP_SAVE_XMM128:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SAVE_XMM128 (%u)     OpInfo: XMM%u (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, pCode->OpInfo, pCode->OpInfo);
                i++;
                printf("      Scaled Small Offset: %u * 16 = %u = 0x%05X\n", pHeader->UnwindCode[i].FrameOffset,
                       pHeader->UnwindCode[i].FrameOffset * 16, pHeader->UnwindCode[i].FrameOffset * 16);
                break;

            case UWOP_SAVE_XMM128_FAR:
                printf("    CodeOffset: 0x%02X UnwindOp: UWOP_SAVE_XMM128_FAR (%u) OpInfo: XMM%u (%u)\n",
                       pCode->CodeOffset, pCode->UnwindOp, pCode->OpInfo, pCode->OpInfo);
                i++;
                printf("      Unscaled Large Offset: 0x%08X\n\n", *(ULONG*)&(pHeader->UnwindCode[i]));
                i++;
                break;

            case UWOP_EPILOG:
            case UWOP_SPARE_CODE:
            case UWOP_PUSH_MACHFRAME:
            default:
                printf("    Unrecognized UNWIND_CODE: 0x%04X\n", *(USHORT*)pCode);
                break;
        }
    }
}

#ifdef UNIX_AMD64_ABI
//------------------------------------------------------------------------
// DumpCfiInfo: Dump the Cfi data.
//
// Arguments:
//    isHotCode   - true if this cfi data is for the hot section, false otherwise.
//    startOffset - byte offset of the code start that this cfi data represents.
//    endOffset   - byte offset of the code end   that this cfi data represents.
//    pcFiCode    - pointer to the cfi data blob.
//
void DumpCfiInfo(bool                  isHotCode,
                 UNATIVE_OFFSET        startOffset,
                 UNATIVE_OFFSET        endOffset,
                 DWORD                 cfiCodeBytes,
                 const CFI_CODE* const pCfiCode)
{
    printf("Cfi Info%s:\n", isHotCode ? "" : " COLD");
    printf("  >> Start offset   : 0x%06x \n", dspOffset(startOffset));
    printf("  >>   End offset   : 0x%06x \n", dspOffset(endOffset));

    for (int i = 0; i < cfiCodeBytes / sizeof(CFI_CODE); i++)
    {
        const CFI_CODE* const pCode = &(pCfiCode[i]);

        UCHAR codeOffset = pCode->CodeOffset;
        SHORT dwarfReg   = pCode->DwarfReg;
        INT   offset     = pCode->Offset;

        switch (pCode->CfiOpCode)
        {
            case CFI_REL_OFFSET:
                printf("    CodeOffset: 0x%02X Op: RelOffset DwarfReg:0x%x Offset:0x%X\n", codeOffset, dwarfReg,
                       offset);
                break;
            case CFI_DEF_CFA_REGISTER:
                assert(offset == 0);
                printf("    CodeOffset: 0x%02X Op: DefCfaRegister DwarfReg:0x%X\n", codeOffset, dwarfReg);
                break;
            case CFI_ADJUST_CFA_OFFSET:
                assert(dwarfReg == DWARF_REG_ILLEGAL);
                printf("    CodeOffset: 0x%02X Op: AdjustCfaOffset Offset:0x%X\n", codeOffset, offset);
                break;
            default:
                printf("    Unrecognized CFI_CODE: 0x%IX\n", *(UINT64*)pCode);
                break;
        }
    }
}
#endif // UNIX_AMD64_ABI
#endif // DEBUG

//------------------------------------------------------------------------
// Compiler::unwindBegProlog: Initialize the unwind info data structures.
// Called at the beginning of main function or funclet prolog generation.
//
void Compiler::unwindBegProlog()
{
#if FEATURE_EH_FUNCLETS
#ifdef UNIX_AMD64_ABI
    if (generateCFIUnwindCodes())
    {
        unwindBegPrologCFI();
    }
    else
#endif // UNIX_AMD64_ABI
    {
        unwindBegPrologWindows();
    }
#endif // FEATURE_EH_FUNCLETS
}

void Compiler::unwindBegPrologWindows()
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    // There is only one prolog for a function/funclet, and it comes first. So now is
    // a good time to initialize all the unwind data structures.

    unwindGetFuncLocations(func, true, &func->startLoc, &func->endLoc);

    if (fgFirstColdBlock != nullptr)
    {
        unwindGetFuncLocations(func, false, &func->coldStartLoc, &func->coldEndLoc);
    }

    func->unwindCodeSlot                  = sizeof(func->unwindCodes);
    func->unwindHeader.Version            = 1;
    func->unwindHeader.Flags              = 0;
    func->unwindHeader.CountOfUnwindCodes = 0;
    func->unwindHeader.FrameRegister      = 0;
    func->unwindHeader.FrameOffset        = 0;
}

#ifdef UNIX_AMD64_ABI
template <typename T>
inline static T* allocate_any(jitstd::allocator<void>& alloc, size_t count = 5)
{
    return jitstd::allocator<T>(alloc).allocate(count);
}
typedef jitstd::vector<CFI_CODE> CFICodeVector;

void Compiler::unwindBegPrologCFI()
{
    assert(compGeneratingProlog);

    FuncInfoDsc* func = funCurrentFunc();

    // There is only one prolog for a function/funclet, and it comes first. So now is
    // a good time to initialize all the unwind data structures.

    unwindGetFuncLocations(func, true, &func->startLoc, &func->endLoc);

    if (fgFirstColdBlock != nullptr)
    {
        unwindGetFuncLocations(func, false, &func->coldStartLoc, &func->coldEndLoc);
    }

    jitstd::allocator<void> allocator(getAllocator());

    func->cfiCodes = new (allocate_any<CFICodeVector>(allocator), jitstd::placement_t()) CFICodeVector(allocator);
}
#endif // UNIX_AMD64_ABI

//------------------------------------------------------------------------
// Compiler::unwindEndProlog: Called at the end of main function or funclet
// prolog generation to indicate there is no more unwind information for this prolog.
//
void Compiler::unwindEndProlog()
{
#if FEATURE_EH_FUNCLETS
    assert(compGeneratingProlog);
#endif // FEATURE_EH_FUNCLETS
}

//------------------------------------------------------------------------
// Compiler::unwindBegEpilog: Called at the beginning of main function or funclet
// epilog generation.
//
void Compiler::unwindBegEpilog()
{
#if FEATURE_EH_FUNCLETS
    assert(compGeneratingEpilog);
#endif // FEATURE_EH_FUNCLETS
}

//------------------------------------------------------------------------
// Compiler::unwindEndEpilog: Called at the end of main function or funclet
// epilog generation.
//
void Compiler::unwindEndEpilog()
{
#if FEATURE_EH_FUNCLETS
    assert(compGeneratingEpilog);
#endif // FEATURE_EH_FUNCLETS
}


//------------------------------------------------------------------------
// Compiler::unwindEmit: Report all the unwind information to the VM.
//
// Arguments:
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//
void Compiler::unwindEmit(void* pHotCode, void* pColdCode)
{
#if FEATURE_EH_FUNCLETS
    assert(!compGeneratingProlog);
    assert(!compGeneratingEpilog);

    assert(compFuncInfoCount > 0);
    for (unsigned funcIdx = 0; funcIdx < compFuncInfoCount; funcIdx++)
    {
        unwindEmitFunc(funGetFunc(funcIdx), pHotCode, pColdCode);
    }
#endif
}

//------------------------------------------------------------------------
// Compiler::unwindEmitFunc: Report the unwind information to the VM for a
// given main function or funclet. Reports the hot section, then the cold
// section if necessary.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//
void Compiler::unwindEmitFunc(FuncInfoDsc* func, void* pHotCode, void* pColdCode)
{
    // Verify that the JIT enum is in sync with the JIT-EE interface enum
    static_assert_no_msg(FUNC_ROOT == (FuncKind)CORJIT_FUNC_ROOT);
    static_assert_no_msg(FUNC_HANDLER == (FuncKind)CORJIT_FUNC_HANDLER);
    static_assert_no_msg(FUNC_FILTER == (FuncKind)CORJIT_FUNC_FILTER);

    unwindEmitFuncHelper(func, pHotCode, pColdCode, true);

    if (pColdCode != nullptr)
    {
        unwindEmitFuncHelper(func, pHotCode, pColdCode, false);
    }
}

//------------------------------------------------------------------------
// Compiler::unwindEmitFuncHelper: Report the unwind information to the VM for a
// given main function or funclet, for either the hot or cold section.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    pHotCode  - Pointer to the beginning of the memory with the function and funclet hot  code.
//    pColdCode - Pointer to the beginning of the memory with the function and funclet cold code.
//                Ignored if 'isHotCode' is true.
//    isHotCode - 'true' to report the hot section, 'false' to report the cold section.
//
void Compiler::unwindEmitFuncHelper(FuncInfoDsc* func, void* pHotCode, void* pColdCode, bool isHotCode)
{
    UNATIVE_OFFSET startOffset;
    UNATIVE_OFFSET endOffset;
    DWORD          unwindCodeBytes = 0;
    BYTE*          pUnwindBlock    = nullptr;

    if (isHotCode)
    {
        if (func->startLoc == nullptr)
        {
            startOffset = 0;
        }
        else
        {
            startOffset = func->startLoc->CodeOffset(genEmitter);
        }

        if (func->endLoc == nullptr)
        {
            endOffset = info.compNativeCodeSize;
        }
        else
        {
            endOffset = func->endLoc->CodeOffset(genEmitter);
        }

#ifdef UNIX_AMD64_ABI
        if (generateCFIUnwindCodes())
        {
            int size = func->cfiCodes->size();
            if (size > 0)
            {
                unwindCodeBytes = size * sizeof(CFI_CODE);
                pUnwindBlock    = (BYTE*)&(*func->cfiCodes)[0];
            }
        }
        else
#endif // UNIX_AMD64_ABI
        {
            unwindCodeBytes = sizeof(func->unwindCodes) - func->unwindCodeSlot;

#ifdef DEBUG
            UNWIND_INFO* pUnwindInfo = (UNWIND_INFO*)(&func->unwindCodes[func->unwindCodeSlot]);
            DWORD        unwindCodeBytesSpecified =
                offsetof(UNWIND_INFO, UnwindCode) +
                pUnwindInfo->CountOfUnwindCodes * sizeof(UNWIND_CODE); // This is what the unwind codes themselves say;
                                                                       // it better match what we tell the VM.
            assert(unwindCodeBytes == unwindCodeBytesSpecified);
#endif // DEBUG

            pUnwindBlock = &func->unwindCodes[func->unwindCodeSlot];
        }
    }
    else
    {
        assert(fgFirstColdBlock != nullptr);
        assert(func->funKind == FUNC_ROOT); // No splitting of funclets.

        if (func->coldStartLoc == nullptr)
        {
            startOffset = 0;
        }
        else
        {
            startOffset = func->coldStartLoc->CodeOffset(genEmitter);
        }

        if (func->coldEndLoc == nullptr)
        {
            endOffset = info.compNativeCodeSize;
        }
        else
        {
            endOffset = func->coldEndLoc->CodeOffset(genEmitter);
        }
    }

#ifdef DEBUG
    if (opts.dspUnwind)
    {
#ifdef UNIX_AMD64_ABI
        if (generateCFIUnwindCodes())
        {
            DumpCfiInfo(isHotCode, startOffset, endOffset, unwindCodeBytes, (const CFI_CODE* const)pUnwindBlock);
        }
        else
#endif // UNIX_AMD64_ABI
        {
            DumpUnwindInfo(isHotCode, startOffset, endOffset, (const UNWIND_INFO* const)pUnwindBlock);
        }
    }
#endif // DEBUG

    // Adjust for cold or hot code:
    // 1. The VM doesn't want the cold code pointer unless this is cold code.
    // 2. The startOffset and endOffset need to be from the base of the hot section for hot code
    //    and from the base of the cold section for cold code

    if (isHotCode)
    {
        assert(endOffset <= info.compTotalHotCodeSize);
        pColdCode = nullptr;
    }
    else
    {
        assert(startOffset >= info.compTotalHotCodeSize);
        startOffset -= info.compTotalHotCodeSize;
        endOffset -= info.compTotalHotCodeSize;
    }

    eeAllocUnwindInfo((BYTE*)pHotCode, (BYTE*)pColdCode, startOffset, endOffset, unwindCodeBytes, pUnwindBlock,
                      (CorJitFuncKind)func->funKind);
}

//------------------------------------------------------------------------
// Compiler::unwindReserve: Ask the VM to reserve space for the unwind information
// for the function and all its funclets. Called once, just before asking the VM
// for memory and emitting the generated code. Calls unwindReserveFunc() to handle
// the main function and each of the funclets, in turn.
//
void Compiler::unwindReserve()
{
#if FEATURE_EH_FUNCLETS
    assert(!compGeneratingProlog);
    assert(!compGeneratingEpilog);

    assert(compFuncInfoCount > 0);
    for (unsigned funcIdx = 0; funcIdx < compFuncInfoCount; funcIdx++)
    {
        unwindReserveFunc(funGetFunc(funcIdx));
    }
#endif
}

//------------------------------------------------------------------------
// Compiler::unwindReserveFunc: Reserve the unwind information from the VM for a
// given main function or funclet.
//
// Arguments:
//    func - The main function or funclet to reserve unwind info for.
//
void Compiler::unwindReserveFunc(FuncInfoDsc* func)
{
    unwindReserveFuncHelper(func, true);

    if (fgFirstColdBlock != nullptr)
    {
        unwindReserveFuncHelper(func, false);
    }
}

//------------------------------------------------------------------------
// Compiler::unwindReserveFuncHelper: Reserve the unwind information from the VM for a
// given main function or funclet, for either the hot or the cold section.
//
// Arguments:
//    func      - The main function or funclet to reserve unwind info for.
//    isHotCode - 'true' to reserve the hot section, 'false' to reserve the cold section.
//
void Compiler::unwindReserveFuncHelper(FuncInfoDsc* func, bool isHotCode)
{
    DWORD unwindCodeBytes = 0;
    if (isHotCode)
    {
#ifdef UNIX_AMD64_ABI
        if (generateCFIUnwindCodes())
        {
            unwindCodeBytes = func->cfiCodes->size() * sizeof(CFI_CODE);
        }
        else
#endif // UNIX_AMD64_ABI
        {
            assert(func->unwindHeader.Version == 1);            // Can't call this before unwindBegProlog
            assert(func->unwindHeader.CountOfUnwindCodes == 0); // Only call this once per prolog

            // Set the size of the prolog to be the last encoded action
            if (func->unwindCodeSlot < sizeof(func->unwindCodes))
            {
                UNWIND_CODE* code               = (UNWIND_CODE*)&func->unwindCodes[func->unwindCodeSlot];
                func->unwindHeader.SizeOfProlog = code->CodeOffset;
            }
            else
            {
                func->unwindHeader.SizeOfProlog = 0;
            }
            func->unwindHeader.CountOfUnwindCodes =
                (BYTE)((sizeof(func->unwindCodes) - func->unwindCodeSlot) / sizeof(UNWIND_CODE));

            // Prepend the unwindHeader onto the unwind codes
            assert(func->unwindCodeSlot >= offsetof(UNWIND_INFO, UnwindCode));

            func->unwindCodeSlot -= offsetof(UNWIND_INFO, UnwindCode);
            UNWIND_INFO* pHeader = (UNWIND_INFO*)&func->unwindCodes[func->unwindCodeSlot];
            memcpy(pHeader, &func->unwindHeader, offsetof(UNWIND_INFO, UnwindCode));

            unwindCodeBytes = sizeof(func->unwindCodes) - func->unwindCodeSlot;
        }
    }

    BOOL isFunclet  = (func->funKind != FUNC_ROOT);
    BOOL isColdCode = isHotCode ? FALSE : TRUE;

    eeReserveUnwindInfo(isFunclet, isColdCode, unwindCodeBytes);
}
