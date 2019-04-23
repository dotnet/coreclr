// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// StackingAllocator.h -
//

//


#ifndef __stacking_allocator_h__
#define __stacking_allocator_h__

#include "util.hpp"
#include "eecontract.h"


// We use zero sized arrays, disable the non-standard extension warning.
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4200)
#endif

#ifdef _DEBUG
    struct Sentinal
    {
        enum { marker1Val = 0xBAD00BAD };
        Sentinal(Sentinal* next) : m_Marker1(marker1Val), m_Next(next) { LIMITED_METHOD_CONTRACT; }

        unsigned  m_Marker1;        // just some data bytes 
        Sentinal* m_Next;           // linked list of these
    };
#endif

    // Blocks from which allocations are carved. Size is determined dynamically,
    // with upper and lower bounds of MinBlockSize and MaxBlockSize respectively
    // (though large allocation requests will cause a block of exactly the right
    // size to be allocated).
    struct StackBlock
    {
        StackBlock     *m_Next;         // Next oldest block in list
        DWORD_PTR   m_Length;       // Length of block excluding header  (needs to be pointer-sized for alignment on IA64)
        INDEBUG(Sentinal*   m_Sentinal;)    // insure that we don't fall of the end of the buffer
        INDEBUG(void**      m_Pad;)    		// keep the size a multiple of 8
        char        m_Data[];       // Start of user allocation space
    };

    // Whenever a checkpoint is requested, a checkpoint structure is allocated
    // (as a normal allocation) and is filled with information about the state
    // of the allocator prior to the checkpoint. When a Collapse request comes
    // in we can therefore restore the state of the allocator.
    // It is the address of the checkpoint structure that we hand out to the
    // caller of GetCheckpoint as an opaque checkpoint marker.
    struct Checkpoint
    {
        StackBlock *m_OldBlock;     // Head of block list before checkpoint
        unsigned    m_OldBytesLeft; // Number of free bytes before checkpoint
    };



// Non-thread safe allocator designed for allocations with the following
// pattern:
//      allocate, allocate, allocate ... deallocate all
// There may also be recursive uses of this allocator (by the same thread), so
// the usage becomes:
//      mark checkpoint, allocate, allocate, ..., deallocate back to checkpoint
//
// Allocations come from a singly linked list of blocks with dynamically
// determined size (the goal is to have fewer block allocations than allocation
// requests).
//
// Allocations are very fast (in the case where a new block isn't allocated)
// since blocks are carved up into packets by simply moving a cursor through
// the block.
//
// Allocations are guaranteed to be quadword aligned.
class StackingAllocator
{
public:

    enum
    {
        MinBlockSize    = 0x2000,
        MaxBlockSize    = 0x2000,
        InitBlockSize   = 0x2000
    };

private:
    struct InitialStackBlock : StackBlock
    {
        InitialStackBlock()
        {
            m_initialBlockHeader.m_Next = NULL;
            m_initialBlockHeader.m_Length = sizeof(m_dataSpace);
        }

        StackBlock m_initialBlockHeader;
        char m_dataSpace[InitBlockSize];
    };

#ifndef DACCESS_COMPILE
    StackingAllocator();
    ~StackingAllocator();
#else
    StackingAllocator() { LIMITED_METHOD_CONTRACT; }
#endif

    void StoreCheckpoint(Checkpoint *c)
    {
        LIMITED_METHOD_CONTRACT;

#ifdef _DEBUG
        m_CheckpointDepth++;
        m_Checkpoints++;
#endif

        // Record previous allocator state in it.
        c->m_OldBlock = m_FirstBlock;
        c->m_OldBytesLeft = m_BytesLeft;
    }

    void* GetCheckpoint();

    // @todo move this into a .inl file as many class users of this class don't need to include this body
    FORCEINLINE void * UnsafeAllocNoThrow(unsigned Size)
    {
        CONTRACT (void*)
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_ANY;
            INJECT_FAULT(CONTRACT_RETURN NULL;);
            PRECONDITION(m_CheckpointDepth > 0);
            POSTCONDITION(CheckPointer(RETVAL, NULL_OK));
        }
        CONTRACT_END;
        
#ifdef _DEBUG
        m_Allocs++;
        m_MaxAlloc = max(Size, m_MaxAlloc);
#endif

        //special case, 0 size alloc, return non-null but invalid pointer
        if (Size == 0)
        {
            RETURN (void*)-1;
        }

        // Round size up to ensure alignment.
        unsigned n = (Size + 7) & ~7;
        if(n < Size)
        {
            return NULL;
        }

        // leave room for sentinal
        INDEBUG(n += sizeof(Sentinal));

        // Is the request too large for the current block?
        if (n > m_BytesLeft)
        {
            if (!AllocNewBlockForBytes(n))
            {
                RETURN NULL;
            }
        }

        // Once we get here we know we have enough bytes left in the block at the
        // head of the chain.
        _ASSERTE(n <= m_BytesLeft);

        void *ret = m_FirstFree;
        m_FirstFree += n;
        m_BytesLeft -= n;

#ifdef _DEBUG
        // Add sentinal to the end
        m_FirstBlock->m_Sentinal = new(m_FirstFree - sizeof(Sentinal)) Sentinal(m_FirstBlock->m_Sentinal);
#endif

        RETURN ret;
    }    
        
    FORCEINLINE void * AllocNoThrow(S_UINT32 size)
    {
        // Safely round size up to ensure alignment.
        if(size.IsOverflow()) return NULL;

        return UnsafeAllocNoThrow(size.Value());
    }

    FORCEINLINE void * AllocSafeThrow(S_UINT32 size){
        WRAPPER_NO_CONTRACT; 

        if(size.IsOverflow()) ThrowOutOfMemory();

        return UnsafeAllocSafeThrow(size.Value());
    }

    FORCEINLINE void * Alloc(S_UINT32 size){
        WRAPPER_NO_CONTRACT;

        if(size.IsOverflow()) ThrowOutOfMemory();

        return UnsafeAlloc(size.Value());
    }

    void  Collapse(void* CheckpointMarker);

    void* UnsafeAllocSafeThrow(UINT32 size);
    void* UnsafeAlloc(UINT32 size);
    void ClearUnallocated();
    
private:

    bool AllocNewBlockForBytes(unsigned n);
    
    StackBlock       *m_FirstBlock;       // Pointer to head of allocation block list
    char             *m_FirstFree;        // Pointer to first free byte in head block
    unsigned          m_BytesLeft;        // Number of free bytes left in head block
    InitialStackBlock m_InitialBlock;     // The first block is special, we never free it
    StackBlock       *m_DeferredFreeBlock; // Avoid going to the OS too often by deferring one free

#ifdef _DEBUG
    unsigned    m_CheckpointDepth;
    unsigned    m_Allocs;
    unsigned    m_Checkpoints;
    unsigned    m_Collapses;
    unsigned    m_BlockAllocs;
    unsigned    m_MaxAlloc;
#endif

    void Init(bool bResetInitBlock)
    {
        WRAPPER_NO_CONTRACT;

        m_FirstBlock = &m_InitialBlock.m_initialBlockHeader;
        m_FirstFree = m_FirstBlock.m_Data;
        m_BytesLeft = static_cast<unsigned>(m_FirstBlock->m_Length)
    }

#ifdef _DEBUG
    void Validate(StackBlock *block, void* spot)
    {
        LIMITED_METHOD_CONTRACT;

        if (!block) 
            return;
        _ASSERTE(m_InitialBlock.m_initialBlockHeader.m_Length == InitBlockSize);
        Sentinal* ptr = block->m_Sentinal;
        _ASSERTE(spot);
        while(ptr >= spot)
        {
                // If this assert goes off then someone overwrote their buffer!
                // A common candidate is PINVOKE buffer run.  To confirm look
                // up on the stack for NDirect.* Look for the MethodDesc
                // associated with it.  Be very suspicious if it is one that
                // has a return string buffer!.  This usually means the end
                // programmer did not allocate a big enough buffer before passing
                // it to the PINVOKE method.
            if (ptr->m_Marker1 != Sentinal::marker1Val)
                _ASSERTE(!"Memory overrun!! May be bad buffer passed to PINVOKE. turn on logging LF_STUBS level 6 to find method");
            ptr = ptr->m_Next;
        }
        block->m_Sentinal = ptr;

    }
#endif

    void Clear(StackBlock *ToBlock)
    {
        LIMITED_METHOD_CONTRACT;

        StackBlock *p = m_FirstBlock;
        StackBlock *q;

        while (p != ToBlock)
        {
            PREFAST_ASSUME(p != NULL);
            
            q = p;
            p = p->m_Next;

            INDEBUG(Validate(q, q));

            // we don't give the tail block back to the OS
            // because we can get into situations where we're growing
            // back and forth over a single seam for a tiny alloc
            // and the perf is a disaster -- VSWhidbey #100462
            if (m_DeferredFreeBlock != NULL)
            {
                delete [] (char *)m_DeferredFreeBlock;
            }

            m_DeferredFreeBlock = q;
            m_DeferredFreeBlock->m_Next = NULL;
        }
    }

private :
    static Checkpoint s_initialCheckpoint;
};

#define ACQUIRE_STACKING_ALLOCATOR(holderName)  \
  Thread *pThread__ACQUIRE_STACKING_ALLOCATOR = GetThread(); \
  StackingAllocator *pStackingAllocator__ACQUIRE_STACKING_ALLOCATOR = pThread->m_stackLocalAllocator; \
  bool allocatorOwner__ACQUIRE_STACKING_ALLOCATOR = false; \
  if (pStackingAllocator__ACQUIRE_STACKING_ALLOCATOR == NULL) \
  { \
      pStackingAllocator__ACQUIRE_STACKING_ALLOCATOR = _alloca(sizeof(StackingAllocator)); \
      allocatorOwner__ACQUIRE_STACKING_ALLOCATOR = true; \
  } \
  StackingAllocatorHolder holderName(pStackingAllocator__ACQUIRE_STACKING_ALLOCATOR, pThread__ACQUIRE_STACKING_ALLOCATOR, allocatorOwner__ACQUIRE_STACKING_ALLOCATOR)

class Thread;
class StackingAllocatorHolder
{
    StackingAllocator *m_pStackingAllocator;
    void* m_checkpointMarker;
    Thread* m_thread;
    bool m_owner;
    ~StackingAllocatorHolder()
    {
        m_pStackingAllocator->Collapse(m_checkpointMarker);
        if (m_owner)
        {
            m_thread->m_stackLocalAllocator = NULL;
        }
    }

    public:
    StackingAllocatorHolder(StackingAllocator *pStackingAllocator, Thread *pThread, bool owner) :
        m_pStackingAllocator(pStackingAllocator),
        m_checkpointMarker(pStackingAllocator->GetCheckpoint()),
        m_thread(pThread),
        m_owner(owner)
    {
        if (m_owner)
        {
            m_thread->m_stackLocalAllocator = pStackingAllocator;
        }
    }
    StackingAllocator *GetStackingAllocator();
};


void * __cdecl operator new(size_t n, StackingAllocator *alloc);
void * __cdecl operator new[](size_t n, StackingAllocator *alloc);
void * __cdecl operator new(size_t n, StackingAllocator *alloc, const NoThrow&) throw();
void * __cdecl operator new[](size_t n, StackingAllocator *alloc, const NoThrow&) throw();

#ifdef _MSC_VER
#pragma warning(pop)
#endif

#endif
