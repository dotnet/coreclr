// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "jitpch.h"
#include "ssaconfig.h"
#include "ssarenamestate.h"

/**
 * Constructor - initialize the stacks and counters maps (lclVar -> stack/counter) map.
 *
 * @params alloc The allocator class used to allocate jitstd data.
 */
SsaRenameState::SsaRenameState(CompAllocator alloc, unsigned lvaCount)
    : stacks(nullptr), definedLocs(alloc), memoryStack{Stack(alloc), Stack(alloc)}, lvaCount(lvaCount), m_alloc(alloc)
{
}

/**
 * Allocates memory for holding pointers to lcl's stacks,
 * if not allocated already.
 *
 */
void SsaRenameState::EnsureStacks()
{
    if (stacks == nullptr)
    {
        stacks = m_alloc.allocate<Stack*>(lvaCount);
        for (unsigned i = 0; i < lvaCount; ++i)
        {
            stacks[i] = nullptr;
        }
    }
}

//------------------------------------------------------------------------
// Top: Get the SSA number at the top of the stack for the specified variable.
//
// Arguments:
//    lclNum - The local variable number
//
// Return Value:
//    The SSA number.
//
// Notes:
//    The stack must not be empty. Method parameters and local variables that are live in at
//    the start of the first block must have associated SSA definitions and their SSA numbers
//    must have been pushed first.
//
unsigned SsaRenameState::Top(unsigned lclNum)
{
    DBG_SSA_JITDUMP("[SsaRenameState::Top] V%02u\n", lclNum);

    noway_assert(stacks != nullptr);
    Stack* stack = stacks[lclNum];
    noway_assert((stack != nullptr) && !stack->empty());
    return stack->back().m_ssaNum;
}

//------------------------------------------------------------------------
// Push: Push a SSA number onto the stack for the specified variable.
//
// Arguments:
//    block  - The block where the SSA definition occurs
//    lclNum - The local variable number
//    ssaNum - The SSA number
//
void SsaRenameState::Push(BasicBlock* block, unsigned lclNum, unsigned ssaNum)
{
    EnsureStacks();

    DBG_SSA_JITDUMP("[SsaRenameState::Push] " FMT_BB ", V%02u, count = %d\n", block->bbNum, lclNum, ssaNum);

    Stack* stack = stacks[lclNum];

    if (stack == nullptr)
    {
        stack = stacks[lclNum] = new (m_alloc) Stack(m_alloc);
    }

    if (stack->empty() || stack->back().m_block != block)
    {
        stack->push_back(SsaRenameStateForBlock(block, ssaNum));
        // Remember that we've pushed a def for this loc (so we don't have
        // to traverse *all* the locs to do the necessary pops later).
        definedLocs.push_back(SsaRenameStateLocDef(block, lclNum));
    }
    else
    {
        stack->back().m_ssaNum = ssaNum;
    }

    INDEBUG(DumpStack(stack, "V%02u", lclNum));
}

void SsaRenameState::PopBlockStacks(BasicBlock* block)
{
    DBG_SSA_JITDUMP("[SsaRenameState::PopBlockStacks] " FMT_BB "\n", block->bbNum);
    // Iterate over the stacks for all the variables, popping those that have an entry
    // for "block" on top.
    while (!definedLocs.empty() && definedLocs.back().m_block == block)
    {
        unsigned lclNum = definedLocs.back().m_lclNum;
        assert(stacks != nullptr); // Cannot be empty because definedLocs is not empty.
        Stack* stack = stacks[lclNum];
        assert(stack != nullptr);
        assert(stack->back().m_block == block);
        stack->pop_back();
        definedLocs.pop_back();

        INDEBUG(DumpStack(stack, "V%02u", lclNum));
    }

#ifdef DEBUG
    // It should now be the case that no stack in stacks has an entry for "block" on top --
    // the loop above popped them all.
    for (unsigned i = 0; i < lvaCount; ++i)
    {
        if (stacks != nullptr && stacks[i] != nullptr && !stacks[i]->empty())
        {
            assert(stacks[i]->back().m_block != block);
        }
    }
#endif // DEBUG
}

void SsaRenameState::PopBlockMemoryStack(MemoryKind memoryKind, BasicBlock* block)
{
    auto& stack = memoryStack[memoryKind];
    while (stack.size() > 0 && stack.back().m_block == block)
    {
        stack.pop_back();
    }
}

#ifdef DEBUG
//------------------------------------------------------------------------
// DumpStack: Print the specified stack.
//
// Arguments:
//    stack - The stack to print
//    name - The name stack name (printf format string)
//
void SsaRenameState::DumpStack(Stack* stack, const char* name, ...)
{
    if (JitTls::GetCompiler()->verboseSsa)
    {
        char    buffer[32];
        va_list args;
        va_start(args, name);
        _vsnprintf(buffer, sizeof(buffer), name, args);
        va_end(args);
        printf("%s: ", buffer);

        for (Stack::reverse_iterator i = stack->rbegin(); i != stack->rend(); ++i)
        {
            printf("%s<" FMT_BB ", %u>", (i == stack->rbegin()) ? "" : ", ", i->m_block->bbNum, i->m_ssaNum);
        }

        printf("\n");
    }
}
#endif // DEBUG
