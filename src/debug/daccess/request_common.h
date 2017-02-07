// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file contains functions used by both request.cpp and request_svr.cpp
// to communicate with the debuggee's GC.

#ifndef _REQUEST_COMMON_H_
#define _REQUEST_COMMON_H_

// Indexes into an array of elements of type T, where the size of type
// T is not (or may not be) known at compile-time. 
// Returns a DPTR to the requested element (the element at the given index).
template<typename T>
DPTR(T) TableIndex(ArrayDPTR(T) base, size_t index, size_t t_size)
{
    TADDR base_addr = base.GetAddr();
    TADDR element_addr = DacTAddrOffset(base_addr, index, t_size);
    return __DPtr<T>(element_addr); 
}

// Indexes into a given generation table, returning a DPTR to the
// requested element (the element at the given index) of the table.
inline DPTR(dac_generation)
GenerationTableIndex(ArrayDPTR(dac_generation) base, size_t index)
{
    return TableIndex(base, index, g_gcDacGlobals->generation_size);
}

// Indexes into a heap's generation table, given the heap instance
// and the desired index. Returns a DPTR to the requested element.
inline DPTR(dac_generation)
ServerGenerationTableIndex(DPTR(dac_gc_heap) heap, size_t index)
{
    TADDR base_addr = dac_cast<TADDR>(heap) + offsetof(dac_gc_heap, generation_table);
    ArrayDPTR(dac_generation) base = __ArrayDPtr<dac_generation>(base_addr);
    return TableIndex(base, index, g_gcDacGlobals->generation_size);
}

// Indexes into the global heap table, returning a DPTR to the requested
// heap instance.
inline DPTR(dac_gc_heap)
HeapTableIndex(ArrayDPTR(dac_gc_heap*) heaps, size_t index)
{
    DPTR(dac_gc_heap*) ptr = TableIndex(heaps, index, sizeof(dac_gc_heap*));
    TADDR gc_heap_base = dac_cast<TADDR>(*ptr);
    return __DPtr<dac_gc_heap>(gc_heap_base);
}

#endif // _REQUEST_COMMON_H_
