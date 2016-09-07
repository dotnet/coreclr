#include "profilerinfo.h"

ProfilerInfo::ProfilerInfo() noexcept
    : m_pProfilerInfo (nullptr)
    , m_pProfilerInfo2(nullptr)
    , m_pProfilerInfo3(nullptr)
    , m_pProfilerInfo4(nullptr)
    , m_pProfilerInfo5(nullptr)
    , m_pProfilerInfo6(nullptr)
    , m_pProfilerInfo7(nullptr)
    , m_version(0)
{
}

ProfilerInfo::~ProfilerInfo()
{
    if (m_pProfilerInfo != nullptr)
        m_pProfilerInfo->Release();
}

HRESULT ProfilerInfo::Initialize(IUnknown *pICorProfilerInfoUnk) noexcept
{
    this->Reset(); // Ensure ProfilerInfo is in initial state.

    HRESULT hr;

    if (m_pProfilerInfo7 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo7,
            (void**)&m_pProfilerInfo7);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 7;

            m_pProfilerInfo6 = static_cast<ICorProfilerInfo6*>(
                m_pProfilerInfo7);
            m_pProfilerInfo5 = static_cast<ICorProfilerInfo5*>(
                m_pProfilerInfo7);
            m_pProfilerInfo4 = static_cast<ICorProfilerInfo4*>(
                m_pProfilerInfo7);
            m_pProfilerInfo3 = static_cast<ICorProfilerInfo3*>(
                m_pProfilerInfo7);
            m_pProfilerInfo2 = static_cast<ICorProfilerInfo2*>(
                m_pProfilerInfo7);
            m_pProfilerInfo  = static_cast<ICorProfilerInfo* >(
                m_pProfilerInfo7);
        }
    }

    if (m_pProfilerInfo6 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo6,
            (void**)&m_pProfilerInfo6);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 6;

            m_pProfilerInfo5 = static_cast<ICorProfilerInfo5*>(
                m_pProfilerInfo6);
            m_pProfilerInfo4 = static_cast<ICorProfilerInfo4*>(
                m_pProfilerInfo6);
            m_pProfilerInfo3 = static_cast<ICorProfilerInfo3*>(
                m_pProfilerInfo6);
            m_pProfilerInfo2 = static_cast<ICorProfilerInfo2*>(
                m_pProfilerInfo6);
            m_pProfilerInfo  = static_cast<ICorProfilerInfo* >(
                m_pProfilerInfo6);
        }
    }

    if (m_pProfilerInfo5 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo5,
            (void**)&m_pProfilerInfo5);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 5;

            m_pProfilerInfo4 = static_cast<ICorProfilerInfo4*>(
                m_pProfilerInfo5);
            m_pProfilerInfo3 = static_cast<ICorProfilerInfo3*>(
                m_pProfilerInfo5);
            m_pProfilerInfo2 = static_cast<ICorProfilerInfo2*>(
                m_pProfilerInfo5);
            m_pProfilerInfo  = static_cast<ICorProfilerInfo* >(
                m_pProfilerInfo5);
        }
    }

    if (m_pProfilerInfo4 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo4,
            (void**)&m_pProfilerInfo4);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 4;

            m_pProfilerInfo3 = static_cast<ICorProfilerInfo3*>(
                m_pProfilerInfo4);
            m_pProfilerInfo2 = static_cast<ICorProfilerInfo2*>(
                m_pProfilerInfo4);
            m_pProfilerInfo  = static_cast<ICorProfilerInfo* >(
                m_pProfilerInfo4);
        }
    }

    if (m_pProfilerInfo3 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo3,
            (void**)&m_pProfilerInfo3);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 3;

            m_pProfilerInfo2 = static_cast<ICorProfilerInfo2*>(
                m_pProfilerInfo3);
            m_pProfilerInfo  = static_cast<ICorProfilerInfo* >(
                m_pProfilerInfo3);
        }
    }

    if (m_pProfilerInfo2 == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo2,
            (void**)&m_pProfilerInfo2);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 2;

            m_pProfilerInfo = static_cast<ICorProfilerInfo*>(
                m_pProfilerInfo2);
        }
    }

    if (m_pProfilerInfo == nullptr)
    {
        hr = pICorProfilerInfoUnk->QueryInterface(
            IID_ICorProfilerInfo,
            (void**)&m_pProfilerInfo);
        if (SUCCEEDED(hr)) {
            _ASSERTE(m_version == 0);
            m_version = 1;
        }
    }

    _ASSERTE(m_version < 7 || m_pProfilerInfo7 != nullptr);
    _ASSERTE(m_version < 6 || m_pProfilerInfo6 != nullptr);
    _ASSERTE(m_version < 5 || m_pProfilerInfo5 != nullptr);
    _ASSERTE(m_version < 4 || m_pProfilerInfo4 != nullptr);
    _ASSERTE(m_version < 3 || m_pProfilerInfo3 != nullptr);
    _ASSERTE(m_version < 2 || m_pProfilerInfo2 != nullptr);
    _ASSERTE(m_version < 1 || m_pProfilerInfo  != nullptr);

    return hr;
}

void ProfilerInfo::Reset() noexcept
{
    if (m_version != 0)
    {
        _ASSERTE(m_pProfilerInfo != nullptr);
        m_pProfilerInfo->Release();

        m_pProfilerInfo  = nullptr;
        m_pProfilerInfo2 = nullptr;
        m_pProfilerInfo3 = nullptr;
        m_pProfilerInfo4 = nullptr;
        m_pProfilerInfo5 = nullptr;
        m_pProfilerInfo6 = nullptr;
        m_pProfilerInfo7 = nullptr;
        m_version = 0;
    }
}

unsigned int ProfilerInfo::version() const noexcept
{
    return m_version;
}

ICorProfilerInfo  *ProfilerInfo::v1() const noexcept
{
    _ASSERTE(m_version >= 1);
    return m_pProfilerInfo;
}

ICorProfilerInfo2 *ProfilerInfo::v2() const noexcept
{
    _ASSERTE(m_version >= 2);
    return m_pProfilerInfo2;
}

ICorProfilerInfo3 *ProfilerInfo::v3() const noexcept
{
    _ASSERTE(m_version >= 3);
    return m_pProfilerInfo3;
}

ICorProfilerInfo4 *ProfilerInfo::v4() const noexcept
{
    _ASSERTE(m_version >= 4);
    return m_pProfilerInfo4;
}

ICorProfilerInfo5 *ProfilerInfo::v5() const noexcept
{
    _ASSERTE(m_version >= 5);
    return m_pProfilerInfo5;
}

ICorProfilerInfo6 *ProfilerInfo::v6() const noexcept
{
    _ASSERTE(m_version >= 6);
    return m_pProfilerInfo6;
}

ICorProfilerInfo7 *ProfilerInfo::v7() const noexcept
{
    _ASSERTE(m_version >= 7);
    return m_pProfilerInfo7;
}
