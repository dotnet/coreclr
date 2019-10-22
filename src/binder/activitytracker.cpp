// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ============================================================
//
// activitytracker.cpp
//


//
// Helpers for interaction with the managed ActivityTracker
//
// ============================================================

#include "common.h"
#include "activitytracker.h"

void ActivityTracker::Start(const WCHAR *providerName, const WCHAR *activityName, /*out*/ GUID *activityId, /*out*/ GUID *relatedActivityId)
{
    GCX_COOP();
    struct _gc {
        OBJECTREF activityTracker;
        STRINGREF providerName;
        STRINGREF activityName;
    } gc;
    ZeroMemory(&gc, sizeof(gc));

    GCPROTECT_BEGIN(gc);

    MethodDescCallSite getInstance(METHOD__ACTIVITY_TRACKER__GET_INSTANCE);
    gc.activityTracker = getInstance.Call_RetOBJECTREF(NULL);
    gc.providerName = StringObject::NewString(providerName);
    gc.activityName = StringObject::NewString(activityName);

    MethodDescCallSite onStart(METHOD__ACTIVITY_TRACKER__ON_START, &gc.activityTracker);
    ARG_SLOT args[] =
    {
        ObjToArgSlot(gc.activityTracker),
        ObjToArgSlot(gc.providerName),
        ObjToArgSlot(gc.activityName),
        0,
        PtrToArgSlot(activityId),
        PtrToArgSlot(relatedActivityId),
        0x4 /*EventActivityOptions.Recursive*/
    };
    onStart.Call(args);

    GCPROTECT_END();
}

void ActivityTracker::Stop(const WCHAR *providerName, const WCHAR *activityName, /*out*/ GUID *activityId)
{
    GCX_COOP();
    struct _gc {
        OBJECTREF activityTracker;
        STRINGREF providerName;
        STRINGREF activityName;
    } gc;
    ZeroMemory(&gc, sizeof(gc));

    GCPROTECT_BEGIN(gc);

    MethodDescCallSite getInstance(METHOD__ACTIVITY_TRACKER__GET_INSTANCE);
    gc.activityTracker = getInstance.Call_RetOBJECTREF(NULL);
    gc.providerName = StringObject::NewString(providerName);
    gc.activityName = StringObject::NewString(activityName);

    MethodDescCallSite onStop(METHOD__ACTIVITY_TRACKER__ON_STOP, &gc.activityTracker);
    ARG_SLOT args[] =
    {
        ObjToArgSlot(gc.activityTracker),
        ObjToArgSlot(gc.providerName),
        ObjToArgSlot(gc.activityName),
        0,
        PtrToArgSlot(activityId),
    };
    onStop.Call(args);

    GCPROTECT_END();
}