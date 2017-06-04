#include <stdexcept>
#include <system_error>
#include <new>
#include <iostream>
#include <memory>

#include <errno.h>
#include <string.h>

#include "profilermanager.h"
#include "profiler.h"

// static
HRESULT Profiler::CreateObject(
    REFIID riid,
    void **ppInterface) noexcept
{
    //
    // We should perform some prechecks to avoid unnecessary initialization.
    //

    if (ppInterface == nullptr)
        return E_POINTER;

    *ppInterface = nullptr;

    if (
        (riid != IID_ICorProfilerCallback3) &&
        (riid != IID_ICorProfilerCallback2) &&
        (riid != IID_ICorProfilerCallback) &&
        (riid != IID_IUnknown)
    )
    {
        return E_NOINTERFACE;
    }

    // This profiler implements the "profile-first" alternative of dealing
    // with multiple in-process side-by-side CLR instances. First CLR
    // to try to load us into this process wins.
    // TODO: remove this when Global Profiler Manager will be implemented
    // (see https://blogs.msdn.microsoft.com/davbr/2010/08/25/profilers-in-process-side-by-side-clr-instances-and-a-free-test-harness/).
    {
        static volatile LONG s_nFirstTime = 1;
        if (s_nFirstTime == 0)
        {
            // Someone beat us to it.
            return CORPROF_E_PROFILER_CANCEL_ACTIVATION;
        }

        // Dirty-read says this is the first load. Double-check that
        // with a clean-read.
        if (InterlockedCompareExchange(&s_nFirstTime, 0, 1) == 0)
        {
            // Someone beat us to it.
            return CORPROF_E_PROFILER_CANCEL_ACTIVATION;
        }
    }

    //
    // Profiler instantiation.
    //

    Profiler *pProfiler;
    try
    {
        pProfiler = new (std::nothrow) Profiler();
        if (!pProfiler)
            return E_OUTOFMEMORY;
    }
    catch (...)
    {
        // Exceptions in the Profiler's constructor.
        return E_FAIL;
    }

    //
    // Profiler registration.
    //

    HRESULT hr = S_OK;
    try
    {
        ProfilerManager::Instance().RegisterProfiler(pProfiler);
    }
    catch (...)
    {
        hr = pProfiler->HandleException(std::current_exception());
        delete pProfiler;
        return hr;
    }

    //
    // Preparing results for returning.
    //

    hr = pProfiler->QueryInterface(riid, ppInterface);
    if (FAILED(hr))
    {
        pProfiler->HandleHresult("Profiler::CreateObject()", hr);
    }
    // Profiler already had 1 on reference counter so we need to call
    // Release() after QueryInterface() to retrieve it to 1.
    pProfiler->Release();

    return hr;
}

// static
void Profiler::RemoveObject(
    Profiler *pProfiler) noexcept
{
    //
    // We should unregister profiler, destruct it and remove it from the memory.
    //
    _ASSERTE(ProfilerManager::Instance().IsProfilerRegistered(pProfiler));
    ProfilerManager::Instance().UnregisterProfiler(pProfiler);
    delete pProfiler;
}

Profiler::Profiler()
    : m_cRef(1)
    , m_logger()
    , m_initialized(false)
    , m_shutdowned(false)
    , m_loggerConfig()
    , m_traceLogConfig()
    , m_profConfig()
    , m_info()
    , m_traceLog()
    , m_commonTrace(*this, m_info)    // Should be after m_logger, m_info and
                                      // m_traceLog.
    , m_executionTrace(*this, m_info) // Should be after m_commonTrace.
    , m_firstTickCount(0)
{
}

Profiler::~Profiler()
{
}

Log &Profiler::LOG() const noexcept
{
    return const_cast<Log&>(m_logger);
}

ITraceLog &Profiler::TRACE() const noexcept
{
    // NOTE: default-constructed TraceLog object should not be used for output!
    _ASSERTE(m_traceLog != nullptr);
    return const_cast<ITraceLog&>(*m_traceLog);
}

DWORD Profiler::GetTickCountFromInit() const noexcept
{
    return GetTickCount() - m_firstTickCount;
}

HRESULT Profiler::HandleException(std::exception_ptr eptr) const noexcept
{
    // std::rethrow_exception() expects that eptr is non-null.
    if (eptr == nullptr)
        return S_OK;

    // Send information about the exception to the log.

    try
    {
        try
        {
            std::rethrow_exception(eptr);
        }
        catch (const HresultException &e)
        {
            LOG().Error() << "Exception: " << e.what()
                << " (HR = " << e.hresult() << ")";
        }
        catch (const std::system_error &e)
        {
            LOG().Error() << "Exception: " << e.what()
                << " (EC = " << e.code() << ")";
        }
        catch (const std::ios_base::failure &e)
        {
            // std::ios_base::failure should be inherited from std::system_error
            // for C++11. This workaround applied if it is not true.
            LOG().Error() << "Exception: " << e.what() << ": "
                << strerror(errno);
        }
        catch (const std::exception &e)
        {
            LOG().Error() << "Exception: " << e.what();
        }
        catch (...)
        {
            LOG().Error() << "Unspecified exception";
        }
    }
    catch (...)
    {
        // We can do nothing with information about exception if logging failed.
    }

    // Return appropriate for the exception HRESULT.

    try
    {
        std::rethrow_exception(eptr);
    }
    catch (const HresultException &e)
    {
        return e.hresult();
    }
    catch (const std::bad_alloc &e)
    {
        return E_OUTOFMEMORY;
    }
    catch (const std::logic_error &e)
    {
        return E_UNEXPECTED;
    }
    catch (...)
    {
        return E_FAIL;
    }
}

void Profiler::HandleSysErr(
    const std::string& what_arg, int ev) const noexcept
{
    try
    {
        throw std::system_error(ev, std::system_category(), what_arg);
    }
    catch (...)
    {
        this->HandleException(std::current_exception());
    }
}

void Profiler::HandleHresult(
    const std::string& what_arg, HRESULT hr) const noexcept
{
    try
    {
        throw HresultException(what_arg, hr);
    }
    catch (...)
    {
        this->HandleException(std::current_exception());
    }
}

ProfilerConfig &Profiler::GetConfig() noexcept
{
    return m_profConfig;
}

const ProfilerInfo &Profiler::GetProfilerInfo() const noexcept
{
    return m_info;
}

CommonTrace &Profiler::GetCommonTrace() noexcept
{
    return m_commonTrace;
}

ExecutionTrace &Profiler::GetExecutionTrace() noexcept
{
    return m_executionTrace;
}

void Profiler::SetupLogging(LoggerConfig &config)
{
    if (config.OutputStream == LoggerOutputStream::Stdout)
    {
        m_logger = Log(config.Level, std::cout);
    }
    else if (config.OutputStream == LoggerOutputStream::Stderr)
    {
        m_logger = Log(config.Level, std::cerr);
    }
    else if (config.OutputStream == LoggerOutputStream::File)
    {
        m_logger = Log(config.Level, config.FileName);
    }

    // Disabling exceptions so Logger can be used without exceptions checking.
    m_logger.exceptions(std::ostream::goodbit);
}

void Profiler::SetupTraceLog(TraceLogConfig &config)
{
    if (config.OutputStream == TraceLogOutputStream::Stdout)
    {
        m_traceLog.reset(ITraceLog::Create(ITraceLog::StdOutStream));
    }
    else if (config.OutputStream == TraceLogOutputStream::Stderr)
    {
        m_traceLog.reset(ITraceLog::Create(ITraceLog::StdErrStream));
    }
    else if (config.OutputStream == TraceLogOutputStream::File)
    {
        m_traceLog.reset(
            ITraceLog::Create(ITraceLog::FileStream, config.FileName));
    }
}

void Profiler::ProcessConfig(ProfilerConfig &config)
{
    // Check method is called during initialization phase.
    _ASSERTE(m_initialized == false);

    // Ensure the Profiler Info is initialized.
    _ASSERTE(m_info.v1() != nullptr);

    m_commonTrace.ProcessConfig(config);
    m_executionTrace.ProcessConfig(config);

    // TODO: handle settings for Memory Trace.
}

HRESULT STDMETHODCALLTYPE Profiler::QueryInterface(
    REFIID riid,
    void **ppvObject)
{
    if (ppvObject == nullptr)
        return E_POINTER;

    // Pick the right v-table based on the IID passed in.
    if (riid == IID_ICorProfilerCallback3)
    {
        *ppvObject = static_cast<ICorProfilerCallback3*>(this);
    }
    else if (riid == IID_ICorProfilerCallback2)
    {
        *ppvObject = static_cast<ICorProfilerCallback2*>(this);
    }
    else if (riid == IID_ICorProfilerCallback)
    {
        *ppvObject = static_cast<ICorProfilerCallback*>(this);
    }
    else if (riid == IID_IUnknown)
    {
        *ppvObject = static_cast<IUnknown*>(this);
    }
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    // If successful, add a reference for out pointer and return.
    this->AddRef();

    return S_OK;
}

ULONG STDMETHODCALLTYPE Profiler::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

ULONG STDMETHODCALLTYPE Profiler::Release()
{
    LONG result = InterlockedDecrement(&m_cRef);
    if (result == 0)
    {
        // Notify the Global Area that the profiler instance is not longer used.
        Profiler::RemoveObject(this);
    }

    return result;
}

HRESULT STDMETHODCALLTYPE Profiler::Initialize(
    IUnknown *pICorProfilerInfoUnk)
{
    // Check method is called only once.
    _ASSERTE(m_initialized == false);

    m_firstTickCount = GetTickCount();

    HRESULT hr = S_OK;

    try {
        //
        // Fetching the Configuration and setup logging.
        //

        m_loggerConfig = ProfilerManager::Instance().FetchLoggerConfig(this);
        this->SetupLogging(m_loggerConfig);

        m_traceLogConfig =
            ProfilerManager::Instance().FetchTraceLogConfig(this);
        m_profConfig  = ProfilerManager::Instance().FetchProfilerConfig(this);

        //
        // Applying the Configuration to the TraceLog.
        //

        this->SetupTraceLog(m_traceLogConfig);

        //
        // Initializing the Profiler Info.
        //

        hr = m_info.Initialize(pICorProfilerInfoUnk);
        if (FAILED(hr))
        {
            throw HresultException("ProfilerInfo::Initialize()", hr);
        }
        _ASSERTE(m_info.version() > 0);

        //
        // Applying the Configuration to the Profiler.
        //

        this->ProcessConfig(m_profConfig);

        //
        // Initialization completion.
        //

        m_initialized = true;
    }
    catch (...)
    {
        hr = this->HandleException(std::current_exception());
    }

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::Shutdown()
{
    // Check method is called only once.
    _ASSERTE(m_shutdowned == false);

    m_executionTrace.Shutdown();
    m_commonTrace.Shutdown();

    // ProfilerInfo can't be used after Shutdown event.
    m_info.Reset();

    m_shutdowned = true;

    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationStarted(
    AppDomainID appDomainId)
{
    LOG().Trace() << "AppDomainCreationStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationFinished(
    AppDomainID appDomainId,
    HRESULT hrStatus)
{
    LOG().Trace() << "AppDomainCreationFinished()";

    HRESULT hr;
    hr = m_commonTrace.AppDomainCreationFinished(appDomainId, hrStatus);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownStarted(
    AppDomainID appDomainId)
{
    LOG().Trace() << "AppDomainShutdownStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownFinished(
    AppDomainID appDomainId,
    HRESULT hrStatus)
{
    LOG().Trace() << "AppDomainShutdownFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadStarted(
    AssemblyID assemblyId)
{
    LOG().Trace() << "AssemblyLoadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadFinished(
    AssemblyID assemblyId,
    HRESULT hrStatus)
{
    LOG().Trace() << "AssemblyLoadFinished()";

    HRESULT hr;
    hr = m_commonTrace.AssemblyLoadFinished(assemblyId, hrStatus);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadStarted(
    AssemblyID assemblyId)
{
    LOG().Trace() << "AssemblyUnloadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadFinished(
    AssemblyID assemblyId,
    HRESULT hrStatus)
{
    LOG().Trace() << "AssemblyUnloadFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadStarted(
    ModuleID moduleId)
{
    LOG().Trace() << "ModuleLoadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadFinished(
    ModuleID moduleId,
    HRESULT hrStatus)
{
    LOG().Trace() << "ModuleLoadFinished()";

    HRESULT hr;
    hr = m_commonTrace.ModuleLoadFinished(moduleId, hrStatus);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadStarted(
    ModuleID moduleId)
{
    LOG().Trace() << "ModuleUnloadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadFinished(
    ModuleID moduleId,
    HRESULT hrStatus)
{
    LOG().Trace() << "ModuleUnloadFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleAttachedToAssembly(
    ModuleID moduleId,
    AssemblyID assemblyId)
{
    LOG().Trace() << "ModuleAttachedToAssembly()";

    HRESULT hr;
    hr = m_commonTrace.ModuleAttachedToAssembly(moduleId, assemblyId);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadStarted(
    ClassID classId)
{
    LOG().Trace() << "ClassLoadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadFinished(
    ClassID classId,
    HRESULT hrStatus)
{
    LOG().Trace() << "ClassLoadFinished()";

    HRESULT hr;
    hr = m_commonTrace.ClassLoadFinished(classId, hrStatus);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadStarted(
    ClassID classId)
{
    LOG().Trace() << "ClassUnloadStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadFinished(
    ClassID classId,
    HRESULT hrStatus)
{
    LOG().Trace() << "ClassUnloadFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FunctionUnloadStarted(
    FunctionID functionId)
{
    LOG().Trace() << "FunctionUnloadStarted()";

    HRESULT hr;
    hr = m_executionTrace.FunctionUnloadStarted(functionId);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationStarted(
    FunctionID functionId,
    BOOL fIsSafeToBlock)
{
    LOG().Trace() << "JITCompilationStarted()";

    HRESULT hr;
    hr = m_executionTrace.JITCompilationStarted(functionId, fIsSafeToBlock);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationFinished(
    FunctionID functionId,
    HRESULT hrStatus, BOOL fIsSafeToBlock)
{
    LOG().Trace() << "JITCompilationFinished()";

    HRESULT hr;
    hr = m_executionTrace.JITCompilationFinished(
        functionId, hrStatus, fIsSafeToBlock);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchStarted(
    FunctionID functionId,
    BOOL *pbUseCachedFunction)
{
    LOG().Trace() << "JITCachedFunctionSearchStarted()";

    HRESULT hr;
    hr = m_executionTrace.JITCachedFunctionSearchStarted(
        functionId, pbUseCachedFunction);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchFinished(
    FunctionID functionId,
    COR_PRF_JIT_CACHE result)
{
    LOG().Trace() << "JITCachedFunctionSearchFinished()";

    HRESULT hr;
    hr = m_executionTrace.JITCachedFunctionSearchFinished(functionId, result);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::JITFunctionPitched(
    FunctionID functionId)
{
    LOG().Trace() << "JITFunctionPitched()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITInlining(
    FunctionID callerId,
    FunctionID calleeId,
    BOOL *pfShouldInline)
{
    LOG().Trace() << "JITInlining()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadCreated(
    ThreadID threadId)
{
    LOG().Trace() << "ThreadCreated()";

    HRESULT hr;
    hr = m_commonTrace.ThreadCreated(threadId);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadDestroyed(
    ThreadID threadId)
{
    LOG().Trace() << "ThreadDestroyed()";

    HRESULT hr;
    hr = m_commonTrace.ThreadDestroyed(threadId);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadAssignedToOSThread(
    ThreadID managedThreadId,
    DWORD osThreadId)
{
    LOG().Trace() << "ThreadAssignedToOSThread()";

    HRESULT hr;
    hr = m_commonTrace.ThreadAssignedToOSThread(managedThreadId, osThreadId);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadNameChanged(
    ThreadID threadId,
    ULONG cchName,
    _In_reads_opt_(cchName) WCHAR name[])
{
    LOG().Trace() << "ThreadNameChanged()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationStarted()
{
    LOG().Trace() << "RemotingClientInvocationStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientSendingMessage(
    GUID *pCookie,
    BOOL fIsAsync)
{
    LOG().Trace() << "RemotingClientSendingMessage()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientReceivingReply(
    GUID *pCookie,
    BOOL fIsAsync)
{
    LOG().Trace() << "RemotingClientReceivingReply()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationFinished()
{
    LOG().Trace() << "RemotingClientInvocationFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerReceivingMessage(
    GUID *pCookie,
    BOOL fIsAsync)
{
    LOG().Trace() << "RemotingServerReceivingMessage()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationStarted()
{
    LOG().Trace() << "RemotingServerInvocationStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationReturned()
{
    LOG().Trace() << "RemotingServerInvocationReturned()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerSendingReply(
    GUID *pCookie,
    BOOL fIsAsync)
{
    LOG().Trace() << "RemotingServerSendingReply()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::UnmanagedToManagedTransition(
    FunctionID functionId,
    COR_PRF_TRANSITION_REASON reason)
{
    LOG().Trace() << "UnmanagedToManagedTransition()";

    HRESULT hr;
    hr = m_executionTrace.UnmanagedToManagedTransition(functionId, reason);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ManagedToUnmanagedTransition(
    FunctionID functionId,
    COR_PRF_TRANSITION_REASON reason)
{
    LOG().Trace() << "ManagedToUnmanagedTransition()";

    HRESULT hr;
    hr = m_executionTrace.ManagedToUnmanagedTransition(functionId, reason);

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendStarted(
    COR_PRF_SUSPEND_REASON suspendReason)
{
    LOG().Trace() << "RuntimeSuspendStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendFinished()
{
    LOG().Trace() << "RuntimeSuspendFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendAborted()
{
    LOG().Trace() << "RuntimeSuspendAborted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeStarted()
{
    LOG().Trace() << "RuntimeResumeStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeFinished()
{
    LOG().Trace() << "RuntimeResumeFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadSuspended(
    ThreadID threadId)
{
    LOG().Trace() << "RuntimeThreadSuspended()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadResumed(
    ThreadID threadId)
{
    LOG().Trace() << "RuntimeThreadResumed()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::MovedReferences(
    ULONG cMovedObjectIDRanges,
    ObjectID oldObjectIDRangeStart[],
    ObjectID newObjectIDRangeStart[],
    ULONG cObjectIDRangeLength[])
{
    LOG().Trace() << "MovedReferences()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectAllocated(
    ObjectID objectId,
    ClassID classId)
{
    LOG().Trace() << "ObjectAllocated()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectsAllocatedByClass(
    ULONG cClassCount,
    ClassID classIds[],
    ULONG cObjects[])
{
    LOG().Trace() << "ObjectsAllocatedByClass()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectReferences(
    ObjectID objectId,
    ClassID classId,
    ULONG cObjectRefs,
    ObjectID objectRefIds[])
{
    LOG().Trace() << "ObjectReferences()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences(
    ULONG cRootRefs,
    ObjectID rootRefIds[])
{
    LOG().Trace() << "RootReferences()";
    return S_OK;
}


HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionStarted(
    int cGenerations,
    BOOL generationCollected[],
    COR_PRF_GC_REASON reason)
{
    LOG().Trace() << "GarbageCollectionStarted()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::SurvivingReferences(
    ULONG cSurvivingObjectIDRanges,
    ObjectID objectIDRangeStart[],
    ULONG cObjectIDRangeLength[])
{
    LOG().Trace() << "SurvivingReferences()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionFinished()
{
    LOG().Trace() << "GarbageCollectionFinished()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FinalizeableObjectQueued(
    DWORD finalizerFlags,
    ObjectID objectID)
{
    LOG().Trace() << "FinalizeableObjectQueued()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences2(
    ULONG cRootRefs,
    ObjectID rootRefIds[],
    COR_PRF_GC_ROOT_KIND rootKinds[],
    COR_PRF_GC_ROOT_FLAGS rootFlags[],
    UINT_PTR rootIds[])
{
    LOG().Trace() << "RootReferences2()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleCreated(
    GCHandleID handleId,
    ObjectID initialObjectId)
{
    LOG().Trace() << "HandleCreated()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleDestroyed(
    GCHandleID handleId)
{
    LOG().Trace() << "HandleDestroyed()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionThrown(
    ObjectID thrownObjectId)
{
    LOG().Trace() << "ExceptionThrown()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionEnter(
    FunctionID functionId)
{
    LOG().Trace() << "ExceptionSearchFunctionEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionLeave()
{
    LOG().Trace() << "ExceptionSearchFunctionLeave()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterEnter(
    FunctionID functionId)
{
    LOG().Trace() << "ExceptionSearchFilterEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterLeave()
{
    LOG().Trace() << "ExceptionSearchFilterLeave()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchCatcherFound(
    FunctionID functionId)
{
    LOG().Trace() << "ExceptionSearchCatcherFound()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerEnter(
    UINT_PTR __unused)
{
    LOG().Trace() << "ExceptionOSHandlerEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerLeave(
    UINT_PTR __unused)
{
    LOG().Trace() << "ExceptionOSHandlerLeave()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionEnter(
    FunctionID functionId)
{
    LOG().Trace() << "ExceptionUnwindFunctionEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionLeave()
{
    LOG().Trace() << "ExceptionUnwindFunctionLeave()";

    HRESULT hr;
    hr = m_executionTrace.ExceptionUnwindFunctionLeave();

    return hr;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyEnter(
    FunctionID functionId)
{
    LOG().Trace() << "ExceptionUnwindFinallyEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyLeave()
{
    LOG().Trace() << "ExceptionUnwindFinallyLeave()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherEnter(
    FunctionID functionId,
    ObjectID objectId)
{
    LOG().Trace() << "ExceptionCatcherEnter()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherLeave()
{
    LOG().Trace() << "ExceptionCatcherLeave()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableCreated(
    ClassID wrappedClassId,
    REFGUID implementedIID,
    void *pVTable, ULONG cSlots)
{
    LOG().Trace() << "COMClassicVTableCreated()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableDestroyed(
    ClassID wrappedClassId,
    REFGUID implementedIID,
    void *pVTable)
{
    LOG().Trace() << "COMClassicVTableDestroyed()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::InitializeForAttach(
    IUnknown *pCorProfilerInfoUnk,
    void *pvClientData,
    UINT cbClientData)
{
    LOG().Trace() << "InitializeForAttach()";
    // TODO: implement attaching functionality.
    return CORPROF_E_PROFILER_NOT_ATTACHABLE;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerAttachComplete()
{
    LOG().Trace() << "ProfilerAttachComplete()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerDetachSucceeded()
{
    LOG().Trace() << "ProfilerDetachSucceeded()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherFound()
{
    LOG().Trace() << "ExceptionCLRCatcherFound()";
    return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherExecute()
{
    LOG().Trace() << "ExceptionCLRCatcherExecute()";
    return S_OK;
}
