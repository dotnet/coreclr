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
#include "common.h"
#include "fcall.h"
#include "nativeoverlapped.h"
#include "corhost.h"
#include "win32threadpool.h"
#include "mdaassistants.h"
#include "comsynchronizable.h"
#include "comthreadpool.h"
#include "marshalnative.h"

//
//The function is called from managed code to quicly check if a packet is available.
//This is a perf-critical function. Even helper method frames are not created. We fall
//back to the VM to do heavy weight operations like creating a new CP thread. 
//
FCIMPL3(void, CheckVMForIOPacket, LPOVERLAPPED* lpOverlapped, DWORD* errorCode, DWORD* numBytes)
{
    FCALL_CONTRACT;

#ifndef FEATURE_PAL
    Thread *pThread = GetThread();
    size_t key=0;

    _ASSERTE(pThread);

    //Poll and wait if GC is in progress, to avoid blocking GC for too long.
    FC_GC_POLL();

    *lpOverlapped = ThreadpoolMgr::CompletionPortDispatchWorkWithinAppDomain(pThread, errorCode, numBytes, &key, DefaultADID);
    if(*lpOverlapped == NULL)
    {
        return;
    }

    OVERLAPPEDDATAREF overlapped = ObjectToOVERLAPPEDDATAREF(OverlappedDataObject::GetOverlapped(*lpOverlapped));

    if (overlapped->m_callback == NULL)
    {
        //We're not initialized yet, go back to the Vm, and process the packet there.
        ThreadpoolMgr::StoreOverlappedInfoInThread(pThread, *errorCode, *numBytes, key, *lpOverlapped);

        *lpOverlapped = NULL;
        return;
    }
    else
    {
        if(!pThread->IsRealThreadPoolResetNeeded())
        {
            pThread->ResetManagedThreadObjectInCoopMode(ThreadNative::PRIORITY_NORMAL);
            pThread->InternalReset(TRUE, FALSE, FALSE);  
            if(ThreadpoolMgr::ShouldGrowCompletionPortThreadpool(ThreadpoolMgr::CPThreadCounter.DangerousGetDirtyCounts()))
            {
                //We may have to create a CP thread, go back to the Vm, and process the packet there.
                ThreadpoolMgr::StoreOverlappedInfoInThread(pThread, *errorCode, *numBytes, key, *lpOverlapped);
                *lpOverlapped = NULL;
            }
        }
        else
        {
            //A more complete reset is needed (due to change in priority etc), go back to the VM, 
            //and process the packet there.

            ThreadpoolMgr::StoreOverlappedInfoInThread(pThread, *errorCode, *numBytes, key, *lpOverlapped);
            *lpOverlapped = NULL;
        }
    }

    // if this will be "dispatched" to the managed callback fire the IODequeue event:
    if (*lpOverlapped != NULL && ETW_EVENT_ENABLED(MICROSOFT_WINDOWS_DOTNETRUNTIME_PROVIDER_Context, ThreadPoolIODequeue))
        FireEtwThreadPoolIODequeue(*lpOverlapped, OverlappedDataObject::GetOverlapped(*lpOverlapped), GetClrInstanceId());

#else // !FEATURE_PAL
    *lpOverlapped = NULL;
#endif // !FEATURE_PAL

    return;
}
FCIMPLEND
