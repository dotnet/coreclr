// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "jitstd.h"

// Fixed-size array that can hold elements with no default constructor;
// it will construct them all by forwarding whatever arguments are
// supplied to its constructor.
template <typename T, int N>
class ConstructedArray
{
    union {
        // Storage that gets used to hold the T objects.
        unsigned char bytes[N * sizeof(T)];

#if defined(_MSC_VER) && (_MSC_VER < 1900)
        // With MSVC pre-VS2015, the code in the #else branch would hit error C2621,
        // so in that case just count on pointer alignment being sufficient
        // (currently T is only ever instantiated as jitstd::list<SsaRenameStateForBlock>)

        // Unused (except to impart alignment requirement)
        void* pointer;
#else
        // Unused (except to impart alignment requirement)
        T alignedArray[N];
#endif // defined(_MSC_VER) && (_MSC_VER < 1900)
    };

public:
    T& operator[](size_t i)
    {
        return *(reinterpret_cast<T*>(bytes + i * sizeof(T)));
    }

    template <typename... Args>
    ConstructedArray(Args&&... args)
    {
        for (int i = 0; i < N; ++i)
        {
            new (bytes + i * sizeof(T), jitstd::placement_t()) T(jitstd::forward<Args>(args)...);
        }
    }

    ~ConstructedArray()
    {
        for (int i = 0; i < N; ++i)
        {
            operator[](i).~T();
        }
    }
};

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
    ConstructedArray<Stack, MemoryKindCount> memoryStack;

    // Number of stacks/counts to allocate.
    unsigned lvaCount;

    // Allocator to allocate stacks.
    CompAllocator m_alloc;
};
