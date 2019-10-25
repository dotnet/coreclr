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

void ActivityTracker::Start(/*out*/ GUID *activityId, /*out*/ GUID *relatedActivityId)
{
    GCX_COOP();
    MethodDescCallSite startBind(METHOD__ACTIVITY_TRACKER__START_ASSEMBLY_BIND);
    ARG_SLOT args[] =
    {
        PtrToArgSlot(activityId),
        PtrToArgSlot(relatedActivityId),
    };
    startBind.Call(args);
}

void ActivityTracker::Stop(/*out*/ GUID *activityId)
{
    GCX_COOP();

    MethodDescCallSite stopBind(METHOD__ACTIVITY_TRACKER__STOP_ASSEMBLY_BIND);
    ARG_SLOT args[] =
    {
        PtrToArgSlot(activityId),
    };
    stopBind.Call(args);
}