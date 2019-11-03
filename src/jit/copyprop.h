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
    void optBlockCopyProp(BasicBlock* block, LclNumToGenTreePtrStack* curSsaName);
    void optBlockCopyPropPopStacks(BasicBlock* block, LclNumToGenTreePtrStack* curSsaName);
    INDEBUG(void optDumpCopyPropStack(LclNumToGenTreePtrStack* curSsaName));
    void optCopyProp(BasicBlock* block, Statement* stmt, GenTree* tree, LclNumToGenTreePtrStack* curSsaName);
    bool optIsSsaLocal(GenTree* tree);

private:
    CompAllocator memAllocator;

    // Kill set to track variables with intervening definitions.
    VARSET_TP optCopyPropKillSet;
};
