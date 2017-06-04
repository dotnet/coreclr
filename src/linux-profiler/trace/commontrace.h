#ifndef _COMMON_TRACE_H_
#define _COMMON_TRACE_H_

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "basetrace.h"

#include "sharedresource.h"
#include "threadstorage.h"

#include "shared_iterator_range.h"

class CommonTrace final : public BaseTrace
{
public:
    CommonTrace(Profiler &profiler, ProfilerInfo &info);

    ~CommonTrace();

    void ProcessConfig(ProfilerConfig &config) noexcept;

    void Shutdown() noexcept;

    ThreadInfo *GetThreadInfo() noexcept;

    // Simple and safety version of GetThreadInfo() that can be used in signal
    // handlers.
    ThreadInfo *GetThreadInfoR() noexcept;

    HRESULT AppDomainCreationFinished(
        AppDomainID appDomainId,
        HRESULT hrStatus) noexcept;

    HRESULT AssemblyLoadFinished(
        AssemblyID assemblyId,
        HRESULT hrStatus) noexcept;

    HRESULT ModuleLoadFinished(
        ModuleID moduleId,
        HRESULT hrStatus) noexcept;

    HRESULT ModuleAttachedToAssembly(
        ModuleID moduleId,
        AssemblyID assemblyId) noexcept;

    HRESULT ClassLoadFinished(
        ClassID classId,
        HRESULT hrStatus) noexcept;

    HRESULT ThreadCreated(
        ThreadID threadId) noexcept;

    HRESULT ThreadDestroyed(
        ThreadID threadId) noexcept;

    HRESULT ThreadAssignedToOSThread(
        ThreadID managedThreadId,
        DWORD osThreadId) noexcept;

private:
    int m_tlsThreadInfoIndex;

    SharedResource<ThreadStorage> m_threadStorage;

public:
    auto GetThreads() ->
        shared_iterator_range<
            decltype(m_threadStorage.lock()->begin()),
            decltype(m_threadStorage.lock())
        >;

    auto GetThreads() const ->
        shared_iterator_range<
            decltype(m_threadStorage.lock_shared()->begin()),
            decltype(m_threadStorage.lock_shared())
        >;
};

#endif // _COMMON_TRACE_H_
