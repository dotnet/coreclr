// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//*****************************************************************************
// DacDbiImpl.inl
// 

//
// Inline functions for DacDbiImpl.h
//
//*****************************************************************************

#ifndef _DACDBI_IMPL_INL_
#define _DACDBI_IMPL_INL_

#include "dacdbiimpl.h"

/// <summary>
/// Helper to write a structure to the target.
/// </summary>
/// <remarks>
/// This just does a raw Byte copy into the Target, but does not do any Marshalling. 
/// This fails if any part of the buffer can't be written.
/// </remarks>
/// <typeparam name="T">Type of structure to read.</typeparam>
/// <param name='pRemotePtr'>Remote pointer into target (dest).</param>
/// <param name='pLocalBuffer'>Local buffer to write (Src).</param>
/// <returns>Throws on error</returns>
template<typename T>
void DacDbiInterfaceImpl::SafeWriteStructOrThrow(CORDB_ADDRESS pRemotePtr, const T * pLocalBuffer)
{
    HRESULT hr = m_pMutableTarget->WriteVirtual(pRemotePtr, 
        (BYTE *)(pLocalBuffer), sizeof(T));
    
    if (FAILED(hr))
    {
        ThrowHR(hr);
    }
}

/// <summary>
/// Helper to read a structure from the target process
/// </summary>
/// <remarks>
/// This just does a raw Byte copy into the Target, but does not do any Marshalling.
/// This fails if any part of the buffer can't be written. 
/// </remarks>
/// <typeparam name="T">Type of structure to read.</typeparam>
/// <param name='pRemotePtr'>remote pointer into the target process (src).</param>
/// <param name='pLocalBuffer'>local buffer to store the structure (dest).</param>
template<typename T>
void DacDbiInterfaceImpl::SafeReadStructOrThrow(CORDB_ADDRESS pRemotePtr, T * pLocalBuffer)
{
    ULONG32 cbRead = 0;

    HRESULT hr = m_pTarget->ReadVirtual(pRemotePtr, 
        reinterpret_cast<BYTE *>(pLocalBuffer), sizeof(T), &cbRead);
    
    if (FAILED(hr))
    {
        ThrowHR(CORDBG_E_READVIRTUAL_FAILURE);
    }
    
    if (cbRead != sizeof(T))
    {
        ThrowWin32(ERROR_PARTIAL_COPY);
    }
}

#endif // _DACDBI_IMPL_INL_
