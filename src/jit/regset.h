// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************/

#ifndef _REGSET_H
#define _REGSET_H
#include "vartype.h"
#include "target.h"

class LclVarDsc;
class TempDsc;
class Compiler;
class CodeGen;
class GCInfo;

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                           RegSet                                          XX
XX                                                                           XX
XX  Represents the register set, and their states during code generation     XX
XX  Can select an unused register, keeps track of the contents of the        XX
XX  registers, and can spill registers                                       XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************/

class RegSet
{
    friend class CodeGen;
    friend class CodeGenInterface;

private:
    Compiler* m_rsCompiler;
    GCInfo&   m_rsGCInfo;

public:
    RegSet(Compiler* compiler, GCInfo& gcInfo);

#ifdef _TARGET_ARM_
    regMaskTP rsMaskPreSpillRegs(bool includeAlignment)
    {
        return includeAlignment ? (rsMaskPreSpillRegArg | rsMaskPreSpillAlign) : rsMaskPreSpillRegArg;
    }
#endif // _TARGET_ARM_

private:
    // The same descriptor is also used for 'multi-use' register tracking, BTW.
    struct SpillDsc
    {
        SpillDsc* spillNext; // next spilled value of same reg
        GenTree*  spillTree; // the value that was spilled
        TempDsc*  spillTemp; // the temp holding the spilled value

        static SpillDsc* alloc(Compiler* pComp, RegSet* regSet, var_types type);
        static void freeDsc(RegSet* regSet, SpillDsc* spillDsc);
    };

    //-------------------------------------------------------------------------
    //
    //  Track the status of the registers
    //

private:
    bool      rsNeededSpillReg;   // true if this method needed to spill any registers
    regMaskTP rsModifiedRegsMask; // mask of the registers modified by the current function.

#ifdef DEBUG
    bool rsModifiedRegsMaskInitialized; // Has rsModifiedRegsMask been initialized? Guards against illegal use.
#endif                                  // DEBUG

public:
    regMaskTP rsGetModifiedRegsMask() const
    {
        assert(rsModifiedRegsMaskInitialized);
        return rsModifiedRegsMask;
    }

    void rsClearRegsModified();

    void rsSetRegsModified(regMaskTP mask DEBUGARG(bool suppressDump = false));

    void rsRemoveRegsModified(regMaskTP mask);

    bool rsRegsModified(regMaskTP mask) const
    {
        assert(rsModifiedRegsMaskInitialized);
        return (rsModifiedRegsMask & mask) != 0;
    }

    void verifyRegUsed(regNumber reg) const
    {
        assert(rsRegsModified(genRegMask(reg)));
    }

public: // TODO-Cleanup: Should be private, but GCInfo uses them
    __declspec(property(get = GetMaskVars, put = SetMaskVars)) regMaskTP rsMaskVars; // mask of registers currently
                                                                                     // allocated to variables

    regMaskTP GetMaskVars() const // 'get' property function for rsMaskVars property
    {
        return _rsMaskVars;
    }

    void SetMaskVars(regMaskTP newMaskVars); // 'put' property function for rsMaskVars property

    void AddMaskVars(regMaskTP addMaskVars) // union 'addMaskVars' with the rsMaskVars set
    {
        SetMaskVars(_rsMaskVars | addMaskVars);
    }

    void RemoveMaskVars(regMaskTP removeMaskVars) // remove 'removeMaskVars' from the rsMaskVars set (like bitset DiffD)
    {
        SetMaskVars(_rsMaskVars & ~removeMaskVars);
    }

    void ClearMaskVars() // Like SetMaskVars(RBM_NONE), but without any debug output.
    {
        _rsMaskVars = RBM_NONE;
    }

private:
    regMaskTP _rsMaskVars; // backing store for rsMaskVars property

#ifdef _TARGET_ARMARCH_
    regMaskTP rsMaskCalleeSaved; // mask of the registers pushed/popped in the prolog/epilog
#endif                           // _TARGET_ARM_

public:                    // TODO-Cleanup: Should be private, but Compiler uses it
    regMaskTP rsMaskResvd; // mask of the registers that are reserved for special purposes (typically empty)

public: // The PreSpill masks are used in LclVars.cpp
#ifdef _TARGET_ARM_
    regMaskTP rsMaskPreSpillAlign;  // Mask of alignment padding added to prespill to keep double aligned args
                                    // at aligned stack addresses.
    regMaskTP rsMaskPreSpillRegArg; // mask of incoming registers that are spilled at the start of the prolog
                                    // This includes registers used to pass a struct (or part of a struct)
                                    // and all enregistered user arguments in a varargs call
#endif                              // _TARGET_ARM_

private:
    //-------------------------------------------------------------------------
    //
    //  The following tables keep track of spilled register values.
    //

    // When a register gets spilled, the old information is stored here
    SpillDsc* rsSpillDesc[REG_COUNT];
    SpillDsc* rsSpillFree; // list of unused spill descriptors

    void rsSpillChk();
    void rsSpillInit();
    void rsSpillDone();
    void rsSpillBeg();
    void rsSpillEnd();

    void rsSpillTree(regNumber reg, GenTree* tree, unsigned regIdx = 0);

#if defined(_TARGET_X86_)
    void rsSpillFPStack(GenTreeCall* call);
#endif // defined(_TARGET_X86_)

    SpillDsc* rsGetSpillInfo(GenTree* tree, regNumber reg, SpillDsc** pPrevDsc = nullptr);

    TempDsc* rsGetSpillTempWord(regNumber oldReg, SpillDsc* dsc, SpillDsc* prevDsc);

    TempDsc* rsUnspillInPlace(GenTree* tree, regNumber oldReg, unsigned regIdx = 0);

    void rsMarkSpill(GenTree* tree, regNumber reg);
};

//-------------------------------------------------------------------------
//
//  These are used to track the contents of the registers during
//  code generation.
//
//  Only integer registers are tracked.
//

class RegTracker
{
    Compiler* compiler;
    RegSet*   regSet;

public:
    void rsTrackInit(Compiler* comp, RegSet* rs)
    {
        compiler = comp;
        regSet   = rs;
    }

    void rsTrackRegIntCns(regNumber reg, ssize_t val);
    void rsTrackRegLclVar(regNumber reg, unsigned var);
    void rsTrackRegCopy(regNumber reg1, regNumber reg2);
    void rsTrashRegSet(regMaskTP regMask);
};
#endif // _REGSET_H
