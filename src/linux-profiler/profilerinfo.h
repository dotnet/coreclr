#ifndef _PROFILER_INFO_H_
#define _PROFILER_INFO_H_

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

class ProfilerInfo final
{
public:
    ProfilerInfo() noexcept;

    ~ProfilerInfo();

    // Initialize ProfilerInfo with specific pointer to Profiler Info interface.
    HRESULT Initialize(IUnknown *pICorProfilerInfoUnk) noexcept;

    // Reset ProfilerInfo to initial state.
    void Reset() noexcept;

    // Get version of the Profiler Info API. Zero value means that no API
    // versions is supported.
    unsigned int version() const noexcept;

    //
    // These methods provide access to a specific version of the Profiler Info
    // interface. You should be sure that the requested version is supported.
    // Requesting of unsupported interface version invokes undefined behavior.
    //
    ICorProfilerInfo  *v1() const noexcept;
    ICorProfilerInfo2 *v2() const noexcept;
    ICorProfilerInfo3 *v3() const noexcept;
    ICorProfilerInfo4 *v4() const noexcept;
    ICorProfilerInfo5 *v5() const noexcept;
    ICorProfilerInfo6 *v6() const noexcept;
    ICorProfilerInfo7 *v7() const noexcept;

private:
    // Pointers to the implementation of the ProfilerInfo interface(s).
    ICorProfilerInfo  *m_pProfilerInfo;
    ICorProfilerInfo2 *m_pProfilerInfo2;
    ICorProfilerInfo3 *m_pProfilerInfo3;
    ICorProfilerInfo4 *m_pProfilerInfo4;
    ICorProfilerInfo5 *m_pProfilerInfo5;
    ICorProfilerInfo6 *m_pProfilerInfo6;
    ICorProfilerInfo7 *m_pProfilerInfo7;

    // Version of the Profiler Info API.
    unsigned int m_version;
};

#endif // _PROFILER_INFO_H_
