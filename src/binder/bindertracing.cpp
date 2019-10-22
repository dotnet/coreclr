// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ============================================================
//
// bindertracing.cpp
//


//
// Implements helpers for binder tracing
//
// ============================================================

#include "common.h"
#include "bindertracing.h"

#include "activitytracker.h"

#ifdef FEATURE_EVENT_TRACE
#include "eventtracebase.h"
#endif // FEATURE_EVENT_TRACE

using namespace BINDER_SPACE;

namespace
{
    const WCHAR *s_activityName = W("AssemblyBind");

    thread_local bool s_trackingBind = false;
    
    void FireAssemblyBindStart(const BinderTracing::AssemblyBindEvent::BindRequest &request)
    {
#ifdef FEATURE_EVENT_TRACE
        if (!EventEnabledAssemblyBindStart())
            return;

        GUID activityId = GUID_NULL;
        GUID relatedActivityId = GUID_NULL;
        ActivityTracker::Start(MICROSOFT_WINDOWS_DOTNETRUNTIME_PRIVATE_PROVIDER_EVENTPIPE_Context.Name, s_activityName, &activityId, &relatedActivityId);

        FireEtwAssemblyBindStart(
            GetClrInstanceId(),
            request.AssemblyName,
            request.AssemblyPath,
            request.ParentAssembly,
            request.AssemblyLoadContext,
            &activityId,
            &relatedActivityId);
#endif // FEATURE_EVENT_TRACE
    }

    void FireAssemblyBindStop(const BinderTracing::AssemblyBindEvent::BindRequest &request, bool success, const WCHAR *resultName, const WCHAR *resultPath, bool cached)
    {
#ifdef FEATURE_EVENT_TRACE
        if (!EventEnabledAssemblyBindStop())
            return;

        GUID activityId = GUID_NULL;
        ActivityTracker::Stop(MICROSOFT_WINDOWS_DOTNETRUNTIME_PRIVATE_PROVIDER_EVENTPIPE_Context.Name, s_activityName, &activityId);
        
        FireEtwAssemblyBindStop(
            GetClrInstanceId(), 
            request.AssemblyName,
            request.AssemblyPath,
            request.ParentAssembly,
            request.AssemblyLoadContext,
            success,
            resultName,
            resultPath,
            cached,
            &activityId);
#endif // FEATURE_EVENT_TRACE
    }
}

bool BinderTracing::IsEnabled()
{
#ifdef FEATURE_EVENT_TRACE
    // Just check for the AssemblyBindStart event being enabled.
    return EventEnabledAssemblyBindStart();
#endif // FEATURE_EVENT_TRACE
    return false;
}

namespace BinderTracing
{
    AssemblyBindEvent::AssemblyBindEvent(AssemblySpec *assemblySpec)
        : m_bindRequest { assemblySpec }
        , m_prevTrackingBind { s_trackingBind }
        , m_success { false }
        , m_cached { false }
    {
        _ASSERTE(assemblySpec != nullptr);
        
        m_bindRequest.AssemblySpec->GetFileOrDisplayName(ASM_DISPLAYF_VERSION | ASM_DISPLAYF_CULTURE | ASM_DISPLAYF_PUBLIC_KEY_TOKEN, m_bindRequest.AssemblyName);
        
        // ActivityTracker or EventSource may have triggered the system satellite load.
        // Don't track system satellite binding to avoid potential infinite recursion.
        s_trackingBind = !m_bindRequest.AssemblySpec->IsMscorlibSatellite();

        if (s_trackingBind)
            FireAssemblyBindStart(m_bindRequest);
    }

    AssemblyBindEvent::~AssemblyBindEvent()
    {
        if (s_trackingBind)
            FireAssemblyBindStop(m_bindRequest, m_success, m_resultName.GetUnicode(), m_resultPath.GetUnicode(), m_cached);

        s_trackingBind = m_prevTrackingBind;
    }

    void AssemblyBindEvent::SetResult(PEAssembly *assembly)
    {
        m_success = assembly != nullptr;
    }
}