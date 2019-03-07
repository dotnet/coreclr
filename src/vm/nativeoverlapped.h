// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


/*============================================================
**
** Header: COMNativeOverlapped.h
**
** Purpose: Native methods for allocating and freeing NativeOverlapped
**

** 
===========================================================*/

#ifndef _OVERLAPPED_H
#define _OVERLAPPED_H

struct NATIVEOVERLAPPED_AND_HANDLE
{
    OVERLAPPED m_overlapped;
    OBJECTHANDLE m_handle;
};

static OBJECTREF GetOverlapped(LPOVERLAPPED nativeOverlapped)
{
    LIMITED_METHOD_CONTRACT;

    _ASSERTE (nativeOverlapped != NULL);
    return ObjectFromHandle(((NATIVEOVERLAPPED_AND_HANDLE*)nativeOverlapped)->m_handle);
}

static LPVOID GetOverlappedForTracing(LPOVERLAPPED nativeOverlapped)
{
    LIMITED_METHOD_CONTRACT;

    _ASSERTE(nativeOverlapped != NULL);
    return *(LPVOID*)(((NATIVEOVERLAPPED_AND_HANDLE*)nativeOverlapped)->m_handle);
}

#endif
