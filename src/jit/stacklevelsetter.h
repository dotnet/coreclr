// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "compiler.h"
#include "phase.h"

class StackLevelSetter : public Phase
{
public:
    StackLevelSetter(Compiler* compiler);

    virtual void DoPhase() override;

private:
    void ProcessBlock(BasicBlock* block);

#if !FEATURE_FIXED_OUT_ARGS
    void SetThrowHelperBlocks(GenTree* node, BasicBlock* block);
    void SetThrowHelperBlock(SpecialCodeKind kind, BasicBlock* block);
#endif // !FEATURE_FIXED_OUT_ARGS

    unsigned PopArgumentsFromCall(GenTreeCall* call);
    void AddStackLevel(unsigned value);
    void SubStackLevel(unsigned value);

    bool NodePutsOnStack(GenTree* node);

private:
    unsigned currentStackLevel;
    unsigned maxStackLevel;

    CompAllocator memAllocator;

    typedef JitHashTable<GenTreePutArgStk*, JitPtrKeyFuncs<GenTreePutArgStk>, unsigned> PutArgNumSlotsMap;
    PutArgNumSlotsMap putArgNumSlots;

#if !FEATURE_FIXED_OUT_ARGS
    bool framePointerRequired;
#endif // !FEATURE_FIXED_OUT_ARGS
};
