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

// This should match the managed Overlapped object.
// If you make any change here, you need to change the managed part Overlapped.
class OverlappedDataObject : public Object
{
public:
    OBJECTREF m_asyncResult;
    OBJECTREF m_callback;
    OBJECTREF m_overlapped;
    OBJECTREF m_userObject;
    OBJECTREF m_pinnedData;
    LPOVERLAPPED m_pNativeOverlapped;
    ULONG_PTR m_eventHandle;
    int m_offsetLow;
    int m_offsetHigh;

#ifndef DACCESS_COMPILE
    static OverlappedDataObject* GetOverlapped(LPOVERLAPPED nativeOverlapped)
    {
        LIMITED_METHOD_CONTRACT;

        _ASSERTE (nativeOverlapped != NULL);
        return (OverlappedDataObject*)OBJECTREFToObject(ObjectFromHandle(((NATIVEOVERLAPPED_AND_HANDLE*)nativeOverlapped)->m_handle));
    }

    // Return the raw OverlappedDataObject* without going into cooperative mode for tracing
    static OverlappedDataObject* GetOverlappedForTracing(LPOVERLAPPED nativeOverlapped)
    {
        LIMITED_METHOD_CONTRACT;

        _ASSERTE(nativeOverlapped != NULL);
        return *(OverlappedDataObject**)(((NATIVEOVERLAPPED_AND_HANDLE*)nativeOverlapped)->m_handle);
    }
#endif // DACCESS_COMPILE
};

#ifdef USE_CHECKED_OBJECTREFS

typedef REF<OverlappedDataObject> OVERLAPPEDDATAREF;
#define ObjectToOVERLAPPEDDATAREF(obj)     (OVERLAPPEDDATAREF(obj))
#define OVERLAPPEDDATAREFToObject(objref)  (OBJECTREFToObject (objref))

#else

typedef OverlappedDataObject* OVERLAPPEDDATAREF;
#define ObjectToOVERLAPPEDDATAREF(obj)    ((OverlappedDataObject*) (obj))
#define OVERLAPPEDDATAREFToObject(objref) ((OverlappedDataObject*) (objref))

#endif

#endif
