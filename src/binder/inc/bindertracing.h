// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// bindertracing.h
//

#ifndef __BINDER_TRACING_H__
#define __BINDER_TRACING_H__

class Assembly;
class AssemblySpec;
class PEAssembly;

namespace BinderTracing
{
    bool IsEnabled();

    // If tracing is enabled, this class fires an assembly bind start event on construction
    // and the corresponding stop event on destruction
    class AssemblyBindOperation
    {
    public:
        // This class assumes the assembly spec will have a longer lifetime than itself
        AssemblyBindOperation(AssemblySpec *assemblySpec, const WCHAR *assemblyPath = nullptr);
        ~AssemblyBindOperation();

        void SetResult(PEAssembly *assembly, bool cached = false);

        struct BindRequest
        {
            AssemblySpec *AssemblySpec;
            SString AssemblyName;
            SString AssemblyPath;
            SString RequestingAssembly;
            SString AssemblyLoadContext;
            SString RequestingAssemblyLoadContext;
        };

    private:
        bool ShouldIgnoreBind();

    private:
        BindRequest m_bindRequest;
        bool m_populatedBindRequest;

        bool m_checkedIgnoreBind;
        bool m_ignoreBind;

        ReleaseHolder<PEAssembly> m_resultAssembly;
        bool m_cached;
    };


    class ResolutionAttemptedOperation
    {
    private:
        enum class Result : uint16_t
        {
            Attempt,
            Success,
            AssemblyNotFound,
            MismatchedAssemblyName,
            IncompatibleVersion,
            Unknown,
        };

    public:
        enum class Stage : uint16_t
        {
            NotYetStarted,
            FindInLoadContext,
            ALCLoad,
            PlatformAssemblies,
            DefaultALCFallback,
            AppDomainAssemblyResolveEvent,
        };

        struct ResolutionAttempt
        {
            SString AssemblyName;
            uint16_t Stage;
            uint16_t Result;
            SString ResultAssemblyName;
            SString ResultAssemblyPath;
            SString ErrorMessage;
            SString AssemblyLoadContext;
        };

        ResolutionAttemptedOperation(BINDER_SPACE::AssemblyName *assemblyName);
        ~ResolutionAttemptedOperation();

        void SetResult(HRESULT hr, BINDER_SPACE::Assembly *assembly);

        void Trace(Stage s, INT_PTR managedALC=0, AssemblySpec *assemblySpec=nullptr);

    private:
        BINDER_SPACE::AssemblyName *m_pAssemblyName;
        Stage m_stage{Stage::NotYetStarted};

        BINDER_SPACE::Assembly *m_pFoundAssembly{nullptr};
        AssemblySpec *m_pAssemblySpec{nullptr};
        INT_PTR m_pManagedALC{0};
        Result m_result{Result::Attempt};

        void TraceCurrentStage();
    };
};

#endif // __BINDER_TRACING_H__
