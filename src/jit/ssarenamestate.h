// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "jitstd.h"

struct SsaRenameStateForBlock
{
    BasicBlock* m_block;
    unsigned    m_ssaNum;

    SsaRenameStateForBlock(BasicBlock* block, unsigned ssaNum) : m_block(block), m_ssaNum(ssaNum)
    {
    }
    SsaRenameStateForBlock() : m_block(nullptr), m_ssaNum(0)
    {
    }
};

// A record indicating that local "m_lclNum" was defined in block "m_block".
struct SsaRenameStateLocDef
{
    BasicBlock* m_block;
    unsigned    m_lclNum;

    SsaRenameStateLocDef(BasicBlock* block, unsigned lclNum) : m_block(block), m_lclNum(lclNum)
    {
    }
};

struct SsaRenameState
{
    typedef jitstd::list<SsaRenameStateForBlock> Stack;
    typedef Stack**                              Stacks;
    typedef jitstd::list<SsaRenameStateLocDef>   DefStack;

    SsaRenameState(CompAllocator allocator, unsigned lvaCount);

    void EnsureStacks();

    // Get the SSA number at the top of the stack for the specified variable.
    unsigned Top(unsigned lclNum);

    // Push a SSA number onto the stack for the specified variable.
    void Push(BasicBlock* block, unsigned lclNum, unsigned ssaNum);

    // Pop all stacks that have an entry for "block" on top.
    void PopBlockStacks(BasicBlock* block);

    // Similar functions for the special implicit memory variable.
    unsigned TopMemory(MemoryKind memoryKind)
    {
        return memoryStack[memoryKind].back().m_ssaNum;
    }

    void PushMemory(MemoryKind memoryKind, BasicBlock* block, unsigned ssaNum)
    {
        memoryStack[memoryKind].push_back(SsaRenameStateForBlock(block, ssaNum));
    }

    void PopBlockMemoryStack(MemoryKind memoryKind, BasicBlock* block);

private:
    INDEBUG(void DumpStack(Stack* stack, const char* name, ...);)

    // Map of lclNum -> SsaRenameStateForBlock.
    Stacks stacks;

    // This list represents the set of locals defined in the current block.
    DefStack definedLocs;

    // Same state for the special implicit memory variables.
    Stack memoryStack[MemoryKindCount];

    // Number of stacks/counts to allocate.
    unsigned lvaCount;

    // Allocator to allocate stacks.
    CompAllocator m_alloc;
};
