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
    void FireAssemblyLoadStart(const BinderTracing::AssemblyBindOperation::BindRequest &request)
    {
#ifdef FEATURE_EVENT_TRACE
        if (!EventEnabledAssemblyLoadStart())
            return;

        GUID activityId = GUID_NULL;
        GUID relatedActivityId = GUID_NULL;
        ActivityTracker::Start(&activityId, &relatedActivityId);

        FireEtwAssemblyLoadStart(
            GetClrInstanceId(),
            request.AssemblyName,
            request.AssemblyPath,
            request.RequestingAssembly,
            request.AssemblyLoadContext,
            request.RequestingAssemblyLoadContext,
            &activityId,
            &relatedActivityId);
#endif // FEATURE_EVENT_TRACE
    }

    void FireAssemblyLoadStop(const BinderTracing::AssemblyBindOperation::BindRequest &request, PEAssembly *resultAssembly, bool cached)
    {
#ifdef FEATURE_EVENT_TRACE
        if (!EventEnabledAssemblyLoadStop())
            return;

        GUID activityId = GUID_NULL;
        ActivityTracker::Stop(&activityId);

        SString resultName;
        SString resultPath;
        bool success = resultAssembly != nullptr;
        if (success)
        {
            resultPath = resultAssembly->GetPath();
            resultAssembly->GetDisplayName(resultName);
        }

        FireEtwAssemblyLoadStop(
            GetClrInstanceId(),
            request.AssemblyName,
            request.AssemblyPath,
            request.RequestingAssembly,
            request.AssemblyLoadContext,
            request.RequestingAssemblyLoadContext,
            success,
            resultName,
            resultPath,
            cached,
            &activityId);
#endif // FEATURE_EVENT_TRACE
    }

    void GetAssemblyLoadContextNameFromManagedALC(INT_PTR managedALC, /* out */ SString &alcName)
    {
#ifdef CROSSGEN_COMPILE
        alcName.Set(W("Custom"));
#else // CROSSGEN_COMPILE
        OBJECTREF *alc = reinterpret_cast<OBJECTREF *>(managedALC);

        GCX_COOP();
        struct _gc {
            STRINGREF alcName;
        } gc;
        ZeroMemory(&gc, sizeof(gc));

        GCPROTECT_BEGIN(gc);

        PREPARE_VIRTUAL_CALLSITE(METHOD__OBJECT__TO_STRING, *alc);
        DECLARE_ARGHOLDER_ARRAY(args, 1);
        args[ARGNUM_0] = OBJECTREF_TO_ARGHOLDER(*alc);
        CALL_MANAGED_METHOD_RETREF(gc.alcName, STRINGREF, args);
        gc.alcName->GetSString(alcName);

        GCPROTECT_END();
#endif // CROSSGEN_COMPILE
    }

    void GetAssemblyLoadContextNameFromBinderID(UINT_PTR binderID, AppDomain *domain, /*out*/ SString &alcName)
    {
        ICLRPrivBinder *binder = reinterpret_cast<ICLRPrivBinder *>(binderID);
#ifdef FEATURE_COMINTEROP
        if (AreSameBinderInstance(binder, domain->GetTPABinderContext()) || AreSameBinderInstance(binder, domain->GetWinRtBinder()))
#else
        if (AreSameBinderInstance(binder, domain->GetTPABinderContext()))
#endif // FEATURE_COMINTEROP
        {
            alcName.Set(W("Default"));
        }
        else
        {
#ifdef CROSSGEN_COMPILE
            GetAssemblyLoadContextNameFromManagedALC(0, alcName);
#else // CROSSGEN_COMPILE
            CLRPrivBinderAssemblyLoadContext *alcBinder = static_cast<CLRPrivBinderAssemblyLoadContext *>(binder);

            GetAssemblyLoadContextNameFromManagedALC(alcBinder->GetManagedAssemblyLoadContext(), alcName);
#endif // CROSSGEN_COMPILE
        }
    }

    void GetAssemblyLoadContextNameFromBindContext(ICLRPrivBinder *bindContext, AppDomain *domain, /*out*/ SString &alcName)
    {
        _ASSERTE(bindContext != nullptr);

        UINT_PTR binderID = 0;
        HRESULT hr = bindContext->GetBinderID(&binderID);
        _ASSERTE(SUCCEEDED(hr));
        if (SUCCEEDED(hr))
            GetAssemblyLoadContextNameFromBinderID(binderID, domain, alcName);
    }

    void GetAssemblyLoadContextNameFromSpec(AssemblySpec *spec, /*out*/ SString &alcName)
    {
        _ASSERTE(spec != nullptr);

        AppDomain *domain = spec->GetAppDomain();
        ICLRPrivBinder* bindContext = spec->GetBindingContext();
        if (bindContext == nullptr)
            bindContext = spec->GetBindingContextFromParentAssembly(domain);

        GetAssemblyLoadContextNameFromBindContext(bindContext, domain, alcName);
    }

    void PopulateBindRequest(/*inout*/ BinderTracing::AssemblyBindOperation::BindRequest &request)
    {
        AssemblySpec *spec = request.AssemblySpec;
        _ASSERTE(spec != nullptr);

        if (request.AssemblyPath.IsEmpty())
            request.AssemblyPath = spec->GetCodeBase();

        if (spec->GetName() != nullptr)
            spec->GetDisplayName(ASM_DISPLAYF_VERSION | ASM_DISPLAYF_CULTURE | ASM_DISPLAYF_PUBLIC_KEY_TOKEN, request.AssemblyName);

        DomainAssembly *parentAssembly = spec->GetParentAssembly();
        if (parentAssembly != nullptr)
        {
            PEAssembly *peAssembly = parentAssembly->GetFile();
            _ASSERTE(peAssembly != nullptr);
            peAssembly->GetDisplayName(request.RequestingAssembly);

            AppDomain *domain = parentAssembly->GetAppDomain();
            ICLRPrivBinder *bindContext = peAssembly->GetBindingContext();
            if (bindContext == nullptr)
                bindContext = domain->GetTPABinderContext(); // System.Private.CoreLib returns null

            GetAssemblyLoadContextNameFromBindContext(bindContext, domain, request.RequestingAssemblyLoadContext);
        }

        GetAssemblyLoadContextNameFromSpec(spec, request.AssemblyLoadContext);
    }

    void FireResolutionAttempted(const BinderTracing::ResolutionAttemptedOperation::ResolutionAttempt& attempt)
    {
#ifdef FEATURE_EVENT_TRACE
        if (!EventEnabledResolutionAttempted())
            return;

        FireEtwResolutionAttempted(
            GetClrInstanceId(),
            attempt.AssemblyName,
            attempt.Stage,
            attempt.AssemblyLoadContext,
            attempt.Result,
            attempt.ResultAssemblyName,
            attempt.ResultAssemblyPath,
            attempt.ErrorMessage);
#endif // FEATURE_EVENT_TRACE
    }
}

bool BinderTracing::IsEnabled()
{
#ifdef FEATURE_EVENT_TRACE
    // Just check for the AssemblyLoadStart event being enabled.
    return EventEnabledAssemblyLoadStart();
#endif // FEATURE_EVENT_TRACE
    return false;
}

namespace BinderTracing
{
    AssemblyBindOperation::AssemblyBindOperation(AssemblySpec *assemblySpec, const WCHAR *assemblyPath)
        : m_bindRequest { assemblySpec, nullptr, assemblyPath }
        , m_populatedBindRequest { false }
        , m_checkedIgnoreBind { false }
        , m_ignoreBind { false }
        , m_resultAssembly { nullptr }
        , m_cached { false }
    {
        _ASSERTE(assemblySpec != nullptr);

        if (!BinderTracing::IsEnabled() || ShouldIgnoreBind())
            return;

        PopulateBindRequest(m_bindRequest);
        m_populatedBindRequest = true;
        FireAssemblyLoadStart(m_bindRequest);
    }

    AssemblyBindOperation::~AssemblyBindOperation()
    {
        if (!BinderTracing::IsEnabled() || ShouldIgnoreBind())
            return;

        // Make sure the bind request is populated. Tracing may have been enabled mid-bind.
        if (!m_populatedBindRequest)
            PopulateBindRequest(m_bindRequest);

        FireAssemblyLoadStop(m_bindRequest, m_resultAssembly, m_cached);
    }

    void AssemblyBindOperation::SetResult(PEAssembly *assembly, bool cached)
    {
        _ASSERTE(m_resultAssembly == nullptr);
        m_resultAssembly = assembly;
        if (m_resultAssembly != nullptr)
            m_resultAssembly->AddRef();

        m_cached = cached;
    }

    bool AssemblyBindOperation::ShouldIgnoreBind()
    {
        if (m_checkedIgnoreBind)
            return m_ignoreBind;

        // ActivityTracker or EventSource may have triggered the system satellite load.
        // Don't track system satellite binding to avoid potential infinite recursion.
        m_ignoreBind = m_bindRequest.AssemblySpec->IsMscorlibSatellite();
        m_checkedIgnoreBind = true;
        return m_ignoreBind;
    }
}

namespace BinderTracing
{
    ResolutionAttemptedOperation::ResolutionAttemptedOperation(BINDER_SPACE::AssemblyName *assemblyName)
        : m_pAssemblyName{assemblyName}
    {
    }

    ResolutionAttemptedOperation::~ResolutionAttemptedOperation()
    {
        TraceCurrentStage();
    }

    void ResolutionAttemptedOperation::SetResult(HRESULT hr, BINDER_SPACE::Assembly *assembly)
    {
        if (SUCCEEDED(hr))
        {
            m_result = Result::Success;
        }
        else if (hr == HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND))
        {
            m_result = Result::AssemblyNotFound;
        }
        else if (hr == FUSION_E_APP_DOMAIN_LOCKED)
        {
            m_result = Result::IncompatibleVersion;
        }
        else if (hr == FUSION_E_REF_DEF_MISMATCH)
        {
            m_result = Result::MismatchedAssemblyName;
        }
        else
        {
            m_result = Result::Unknown;
            UNREACHABLE();
        }

        m_pFoundAssembly = assembly;
    }

    void ResolutionAttemptedOperation::Trace(Stage stage, INT_PTR managedALC, AssemblySpec *assemblySpec)
    {
        m_pAssemblySpec = assemblySpec;
        m_pManagedALC = managedALC;
        m_stage = stage;
        TraceCurrentStage();
    }

    void ResolutionAttemptedOperation::TraceCurrentStage()
    {
        if (m_stage == Stage::NotYetStarted)
        {
            return;
        }

        ResolutionAttempt attempt;

        attempt.AssemblyName = m_pAssemblyName->GetSimpleName();
        attempt.Stage = static_cast<uint16_t>(m_stage);
        attempt.Result = static_cast<uint16_t>(m_result);

        if (m_pManagedALC != 0)
        {
            GetAssemblyLoadContextNameFromManagedALC(m_pManagedALC, attempt.AssemblyLoadContext);
        }
        else if (m_pAssemblySpec != nullptr)
        {
            GetAssemblyLoadContextNameFromSpec(m_pAssemblySpec, attempt.AssemblyLoadContext);
        }
        else
        {
            attempt.AssemblyLoadContext.Set(W("Default"));
        }

        if (m_pFoundAssembly)
        {
            attempt.ResultAssemblyName = m_pFoundAssembly->GetAssemblyName()->GetSimpleName();
            attempt.ResultAssemblyPath = m_pFoundAssembly->GetPath();
        }

        StackSString errorMsg;
        switch (m_result)
        {
            case Result::Success:
                // No need to write any error message for success.
                break;
            case Result::Attempt:
                // No error yet.
                break;
            case Result::Unknown:
                errorMsg.Set(W("Unknown"));
                break;
            case Result::AssemblyNotFound:
                errorMsg.Printf(W("Could not locate assembly %s"),
                                m_pAssemblyName->GetSimpleName().GetUnicode());
                break;
            case Result::MismatchedAssemblyName:
                errorMsg.Printf(W("Name mismatch while trying to resolve assembly %s: found %s instead"),
                                m_pAssemblyName->GetSimpleName().GetUnicode(),
                                m_pFoundAssembly->GetAssemblyName()->GetSimpleName().GetUnicode());
                break;
            case Result::IncompatibleVersion:
                errorMsg.Printf(W("Assembly %s has been found, but requested version %d.%d.%d.%d is incompatible with found version %d.%d.%d.%d"),
                                m_pAssemblyName->GetSimpleName().GetUnicode(),
                                m_pAssemblyName->GetVersion()->GetMajor(),
                                m_pAssemblyName->GetVersion()->GetMinor(),
                                m_pAssemblyName->GetVersion()->GetBuild(),
                                m_pAssemblyName->GetVersion()->GetRevision(),
                                m_pFoundAssembly->GetAssemblyName()->GetVersion()->GetMajor(),
                                m_pFoundAssembly->GetAssemblyName()->GetVersion()->GetMinor(),
                                m_pFoundAssembly->GetAssemblyName()->GetVersion()->GetBuild(),
                                m_pFoundAssembly->GetAssemblyName()->GetVersion()->GetRevision());
                break;
        }

        attempt.ErrorMessage = errorMsg;

        FireResolutionAttempted(attempt);
    }
}
