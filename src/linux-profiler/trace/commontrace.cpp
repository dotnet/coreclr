#include <memory>
#include <utility>

#include <pthread.h>

#include <winerror.h>

#include "profiler.h"
#include "commontrace.h"

CommonTrace::CommonTrace(Profiler &profiler, ProfilerInfo &info)
    : BaseTrace(profiler, info)
    , m_tlsThreadInfoIndex(TLS_OUT_OF_INDEXES)
    , m_threadStorage()
{
}

CommonTrace::~CommonTrace()
{
    // NOTE: we are dealing with a partially destroyed m_profiler!

    if (m_tlsThreadInfoIndex != TLS_OUT_OF_INDEXES)
    {
        if (!TlsFree(m_tlsThreadInfoIndex))
        {
            m_profiler.HandleHresult(
                "CommonTrace::~CommonTrace(): TlsFree()",
                HRESULT_FROM_WIN32(GetLastError())
            );
        }
    }
}

void CommonTrace::ProcessConfig(ProfilerConfig &config) noexcept
{
    try
    {
        //
        // Check activation condition.
        //

        if (config.ExecutionTraceEnabled || config.MemoryTraceEnabled)
        {
            m_disabled = false;
        }
        else
        {
            return;
        }

        //
        // Initializing thread local storage.
        //

        m_tlsThreadInfoIndex = TlsAlloc();
        if (m_tlsThreadInfoIndex == TLS_OUT_OF_INDEXES)
        {
            m_profiler.HandleHresult(
                "CommonTrace::ProcessConfig(): TlsAlloc()",
                HRESULT_FROM_WIN32(GetLastError())
            );
        }

        //
        // Event Mask calculation.
        //

        HRESULT hr;
        DWORD events;
        hr = m_info.v1()->GetEventMask(&events);
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::ProcessConfig(): GetEventMask()", hr);
        }

        events = events
            | COR_PRF_MONITOR_APPDOMAIN_LOADS
            | COR_PRF_MONITOR_ASSEMBLY_LOADS
            | COR_PRF_MONITOR_MODULE_LOADS
            | COR_PRF_MONITOR_CLASS_LOADS
            | COR_PRF_MONITOR_THREADS;

        hr = m_info.v1()->SetEventMask(events);
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::ProcessConfig(): SetEventMask()", hr);
        }
    }
    catch (...)
    {
        m_disabled = true;
        m_profiler.HandleException(std::current_exception());
    }
}

void CommonTrace::Shutdown() noexcept
{
    m_disabled = true;
}

ThreadInfo *CommonTrace::GetThreadInfo() noexcept
{
    try {
        //
        // Try to get thread info from the local storage.
        //

        ThreadInfo *threadInfo = reinterpret_cast<ThreadInfo*>(
            TlsGetValue(m_tlsThreadInfoIndex));

        if (threadInfo == nullptr)
        {
            DWORD lastError = GetLastError();
            if (lastError != ERROR_SUCCESS)
            {
                m_profiler.HandleHresult(
                    "CommonTrace::GetThreadInfo(): TlsGetValue()",
                    HRESULT_FROM_WIN32(lastError)
                );
            }
        }

        HRESULT hr;

        //
        // Fast check if current thread is changed.
        //

        ThreadID threadId = 0;
        hr = m_info.v1()->GetCurrentThreadID(&threadId);
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::GetThreadInfo(): GetCurrentThreadID()", hr);
        }

        if (threadInfo == nullptr || threadInfo->id != threadId)
        {
            //
            // We should update thread info.
            //

            // Get or create thread info for current thread ID.
            ThreadInfo *oldThreadInfo = threadInfo;
            auto storage_lock = m_threadStorage.lock();
            threadInfo = &storage_lock->Place(threadId).first;

            // Get current OS thread ID.
            DWORD osThreadId = 0;
            hr = m_info.v1()->GetThreadInfo(threadId, &osThreadId);
            // This is OK if we can't obtain osThreadId in some special cases.
            if (FAILED(hr) && hr != CORPROF_E_UNSUPPORTED_CALL_SEQUENCE)
            {
                m_profiler.HandleHresult(
                    "CommonTrace::GetThreadInfo(): GetThreadInfo()", hr);
            }

            // Check if OS thread ID changed and update it.
            if (oldThreadInfo != nullptr &&
                oldThreadInfo->osThreadId == osThreadId)
            {
                oldThreadInfo->osThreadId   = 0;
                oldThreadInfo->nativeHandle = 0;
            }
            threadInfo->osThreadId   = osThreadId;
            threadInfo->nativeHandle = pthread_self();

            //
            // Save new thead info to the local storage.
            //

            if (!TlsSetValue(m_tlsThreadInfoIndex, threadInfo))
            {
                m_profiler.HandleHresult(
                    "CommonTrace::GetThreadInfo(): TlsSetValue()",
                    HRESULT_FROM_WIN32(GetLastError())
                );
            }
        }

        return threadInfo;
    }
    catch (...)
    {
        m_profiler.HandleException(std::current_exception());
        return nullptr;
    }
}

ThreadInfo *CommonTrace::GetThreadInfoR() noexcept
{
    //
    // Try to get thread info from the local storage.
    //

    ThreadInfo *threadInfo = reinterpret_cast<ThreadInfo*>(
        TlsGetValue(m_tlsThreadInfoIndex));

#ifdef _TARGET_AMD64_
    if (threadInfo == nullptr)
    {
        return nullptr;
    }

    //
    // Fast check if current thread is changed.
    //

    HRESULT hr;
    ThreadID threadId = 0;
    hr = m_info.v1()->GetCurrentThreadID(&threadId);
    if (FAILED(hr) || threadInfo->id != threadId)
    {
        return nullptr;
    }
#endif // _TARGET_AMD64_

    return threadInfo;
}

HRESULT CommonTrace::AppDomainCreationFinished(
    AppDomainID appDomainId,
    HRESULT hrStatus) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        ULONG     size = 0;
        ProcessID processId = 0;

        hr = m_info.v1()->GetAppDomainInfo(
            appDomainId, 0, &size, nullptr, &processId);

        std::unique_ptr<WCHAR[]> name = nullptr;
        if (SUCCEEDED(hr))
        {
            name.reset(new (std::nothrow) WCHAR[size]);
            if (name)
            {
                hr = m_info.v1()->GetAppDomainInfo(
                    appDomainId, size, nullptr, name.get(), nullptr);
            }
        }

        TRACE().DumpAppDomainCreationFinished(
            appDomainId, name.get(), processId, hrStatus);

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::AppDomainCreationFinished()", hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::AssemblyLoadFinished(
    AssemblyID assemblyId,
    HRESULT hrStatus) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        ULONG      size = 0;
        AssemblyID appDomainId = 0;
        ModuleID   moduleId = 0;

        hr = m_info.v1()->GetAssemblyInfo(
            assemblyId, 0, &size, nullptr, &appDomainId, &moduleId);

        std::unique_ptr<WCHAR[]> name = nullptr;
        if (SUCCEEDED(hr))
        {
            name.reset(new (std::nothrow) WCHAR[size]);
            if (name)
            {
                hr = m_info.v1()->GetAssemblyInfo(
                    assemblyId, size, nullptr, name.get(), nullptr, nullptr);
            }
        }

        TRACE().DumpAssemblyLoadFinished(
            assemblyId, name.get(), appDomainId, moduleId, hrStatus);

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::AssemblyLoadFinished(): GetAssemblyInfo()", hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ModuleLoadFinished(
    ModuleID moduleId,
    HRESULT hrStatus) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        ULONG      size = 0;
        LPCBYTE    baseLoadAddress = 0;
        AssemblyID assemblyId = 0;

        hr = m_info.v1()->GetModuleInfo(
            moduleId, &baseLoadAddress, 0, &size, nullptr, &assemblyId);

        std::unique_ptr<WCHAR[]> name = nullptr;
        if (SUCCEEDED(hr))
        {
            name.reset(new (std::nothrow) WCHAR[size]);
            if (name)
            {
                hr = m_info.v1()->GetModuleInfo(
                    moduleId, nullptr, size, nullptr, name.get(), nullptr);
            }
        }

        TRACE().DumpModuleLoadFinished(
            moduleId, baseLoadAddress, name.get(), assemblyId, hrStatus);

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::ModuleLoadFinished(): GetModuleInfo()", hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ModuleAttachedToAssembly(
    ModuleID moduleId,
    AssemblyID assemblyId) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        TRACE().DumpModuleAttachedToAssembly(moduleId, assemblyId);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ClassLoadFinished(
    ClassID classId,
    HRESULT hrStatus) noexcept
{
    if (m_disabled)
        return S_OK;

    if (m_info.v1()->IsArrayClass(classId, nullptr, nullptr, nullptr) == S_OK)
    {
        LOG().Warn() << "Array class in ClassLoadFinished()";
    }

    HRESULT hr = S_OK;
    try
    {
        ModuleID  moduleId = 0;
        mdTypeDef typeDefToken = 0;

        hr = m_info.v1()->GetClassIDInfo(
            classId, &moduleId, &typeDefToken);

        TRACE().DumpClassLoadFinished(
            classId, moduleId, typeDefToken, hrStatus);

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "CommonTrace::ClassLoadFinished(): GetClassIDInfo()", hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ThreadCreated(
    ThreadID threadId) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        InternalID threadIid =
            m_threadStorage.lock()->Place(threadId).first.internalId;
        TRACE().DumpThreadCreated(threadId, threadIid);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ThreadDestroyed(
    ThreadID threadId) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        InternalID threadIid;
        {
            auto storage_lock = m_threadStorage.lock();
            ThreadInfo &thrInfo = storage_lock->Unlink(threadId);
            thrInfo.osThreadId = 0;
            threadIid = thrInfo.internalId;
        }
        TRACE().DumpThreadDestroyed(threadIid);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT CommonTrace::ThreadAssignedToOSThread(
    ThreadID managedThreadId,
    DWORD osThreadId) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        InternalID threadIid;
        {
            auto storage_lock = m_threadStorage.lock();
            ThreadInfo &thrInfo = storage_lock->Get(managedThreadId);

            if (thrInfo.osThreadId != osThreadId)
            {
                // This value will be updated by GetThreadInfo() later.
                thrInfo.nativeHandle = 0;
            }
            thrInfo.osThreadId = osThreadId;
            threadIid = thrInfo.internalId;
        }
        TRACE().DumpThreadAssignedToOSThread(threadIid, osThreadId);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

auto CommonTrace::GetThreads() ->
    shared_iterator_range<
        decltype(m_threadStorage.lock()->begin()),
        decltype(m_threadStorage.lock())
    >
{
    auto storage_lock = m_threadStorage.lock();
    auto begin = storage_lock->begin();
    auto end   = storage_lock->end();
    return make_shared_iterator_range(begin, end, std::move(storage_lock));
}

auto CommonTrace::GetThreads() const ->
    shared_iterator_range<
        decltype(m_threadStorage.lock_shared()->begin()),
        decltype(m_threadStorage.lock_shared())
    >
{
    auto storage_lock = m_threadStorage.lock_shared();
    auto begin = storage_lock->begin();
    auto end   = storage_lock->end();
    return make_shared_iterator_range(begin, end, std::move(storage_lock));
}
