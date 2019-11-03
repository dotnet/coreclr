// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "compiler.h"
#include "phase.h"

class CopyPropogation : public Phase
{
public:
    CopyPropogation(Compiler* compiler);

    virtual void DoPhase() override;

    static int optCopyProp_LclVarScore(LclVarDsc* lclVarDsc, LclVarDsc* copyVarDsc, bool preferOp2);

private:
    typedef JitHashTable<unsigned, JitSmallPrimitiveKeyFuncs<unsigned>, GenTreePtrStack*> LclNumToGenTreePtrStack;

    void optVnCopyProp();
    void optBlockCopyProp(BasicBlock* block);
    void optBlockCopyPropPopStacks(BasicBlock* block);
    INDEBUG(void optDumpCopyPropStack());
    void optCopyProp(BasicBlock* block, Statement* stmt, GenTree* tree);
    bool optIsSsaLocal(GenTree* tree);

private:
    CompAllocator m_memAllocator;

    // Kill set to track variables with intervening definitions.
    VARSET_TP m_currKillSet;

    // The map from lclNum to its recently live definitions as a stack.
    LclNumToGenTreePtrStack m_liveLclVarDefs;
};
