// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 
// File: COMDependentHandle.cpp
//

//
// FCall's for the DependentHandle class
//
// Handle functions require cooperative mode, making these fcalls poor candidates for QCall conversion.
//


#include "common.h"
#include "comdependenthandle.h"



FCIMPL2(OBJECTHANDLE, DependentHandle::nInitialize, Object *_primary, Object *_secondary)
{
    FCALL_CONTRACT;

    OBJECTREF primary(_primary);
    OBJECTREF secondary(_secondary);
    OBJECTHANDLE result = NULL;

    HELPER_METHOD_FRAME_BEGIN_RET_NOPOLL();
    
    // Create the handle.
    result = GetAppDomain()->CreateDependentHandle(primary, secondary);

    HELPER_METHOD_FRAME_END_POLL();

    return result;
}
FCIMPLEND



FCIMPL1(VOID, DependentHandle::nFree, OBJECTHANDLE handle)
{
    FCALL_CONTRACT;

    _ASSERTE(handle != NULL);
    
    HELPER_METHOD_FRAME_BEGIN_0();

    DestroyDependentHandle(handle);

    HELPER_METHOD_FRAME_END();

}
FCIMPLEND



FCIMPL1(Object*, DependentHandle::nGetPrimary, OBJECTHANDLE handle)
{
    FCALL_CONTRACT;
    FCUnique(0x54);
    _ASSERTE(handle != NULL);
    return OBJECTREFToObject(ObjectFromHandle(handle));
}
FCIMPLEND



FCIMPL2(Object*, DependentHandle::nGetPrimaryAndSecondary, OBJECTHANDLE handle, Object **outSecondary)
{
    FCALL_CONTRACT;
    *outSecondary = OBJECTREFToObject(GetDependentHandleSecondary(handle));
    return OBJECTREFToObject(ObjectFromHandle(handle));
}
FCIMPLEND

FCIMPL2(VOID, DependentHandle::nSetPrimary, OBJECTHANDLE handle, Object *_primary)
{
    FCALL_CONTRACT;

    _ASSERTE(handle != NULL);

    OBJECTREF primary(_primary);
    StoreObjectInHandle(handle, primary);
}
FCIMPLEND

FCIMPL2(VOID, DependentHandle::nSetSecondary, OBJECTHANDLE handle, Object *_secondary)
{
    FCALL_CONTRACT;

    _ASSERTE(handle != NULL);

    OBJECTREF secondary(_secondary);
    SetDependentHandleSecondary(handle, secondary);
}
FCIMPLEND
