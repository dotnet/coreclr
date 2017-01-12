// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

#ifndef __unwinder_xarch_h__
#define __unwinder_xarch_h__

#include "unwinder.h"

#define UNWIND_CHAIN_LIMIT 32

// Report failure in the unwinder if the condition is FALSE
#ifdef DACCESS_COMPILE
#define UNWINDER_ASSERT(Condition) if (!(Condition)) DacError(CORDBG_E_TARGET_INCONSISTENT)

//---------------------------------------------------------------------------------------
//
// The InstructionBuffer class abstracts accessing assembler instructions in the function
// being unwound. It behaves as a memory byte pointer, but it reads the instruction codes
// from the target process being debugged and removes all changes that the debugger
// may have made to the code, e.g. breakpoint instructions.
//
class InstructionBuffer
{
    UINT m_offset;
    SIZE_T m_address;
    UCHAR m_buffer[32];

    // Load the instructions from the target process being debugged
    HRESULT Load()
    {
        HRESULT hr = DacReadAll(TO_TADDR(m_address), m_buffer, sizeof(m_buffer), false);
        if (SUCCEEDED(hr))
        {
            // On X64, we need to replace any patches which are within the requested memory range.
            // This is because the X64 unwinder needs to disassemble the native instructions in order to determine
            // whether the IP is in an epilog.
            MemoryRange range(dac_cast<PTR_VOID>((TADDR)m_address), sizeof(m_buffer));
            hr = DacReplacePatchesInHostMemory(range, m_buffer);
        }

        return hr;
    }

public:

    // Construct the InstructionBuffer for the given address in the target process
    InstructionBuffer(SIZE_T address)
      : m_offset(0),
        m_address(address)
    {
        HRESULT hr = Load();
        if (FAILED(hr))
        {
            // If we have failed to read from the target process, just pretend
            // we've read zeros.
            // The InstructionBuffer is used in code driven epilogue unwinding
            // when we read processor instructions and simulate them.
            // It's very rare to be stopped in an epilogue when
            // getting a stack trace, so if we can't read the
            // code just assume we aren't in an epilogue instead of failing
            // the unwind.
            memset(m_buffer, 0, sizeof(m_buffer));
        }
    }

    // Move to the next byte in the buffer
    InstructionBuffer& operator++()
    {
        m_offset++;
        return *this;
    }

    // Skip delta bytes in the buffer
    InstructionBuffer& operator+=(INT delta)
    {
        m_offset += delta;
        return *this;
    }

    // Return address of the current byte in the buffer
    explicit operator ULONG64()
    {
        return m_address + m_offset;
    }

    // Get the byte at the given index from the current position
    // Invoke DacError if the index is out of the buffer
    UCHAR operator[](int index)
    {
        int realIndex = m_offset + index;
        UNWINDER_ASSERT(realIndex < sizeof(m_buffer));
        return m_buffer[realIndex];
    }
};

#else // DACCESS_COMPILE

#define UNWINDER_ASSERT _ASSERTE

// For unwinding of the jitted code on non-Windows platforms, the Instruction buffer is
// just a plain pointer to the instruction data.
typedef UCHAR * InstructionBuffer;

#endif // DACCESS_COMPILE

class OOPStackUnwinderXARCH : public OOPStackUnwinder
{
protected:
    static ULONG UnwindOpSlots(__in UNWIND_CODE UnwindCode);

    static UNWIND_INFO * GetUnwindInfo(TADDR taUnwindInfo);
};

#endif // __unwinder_xarch_h__
