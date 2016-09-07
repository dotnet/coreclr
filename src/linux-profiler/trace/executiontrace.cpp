#include <utility>
#include <system_error>
#include <exception>
#include <stdexcept>
#include <tuple>

#include <signal.h>
#include <pthread.h>
#include <errno.h>

#include "profiler.h"
#include "executiontrace.h"

#define LOG_SIGNAL      (SIGRTMIN+1)
#define LOG_SIGNAL_STOP (SIGRTMIN+2)
#define SAMPLE_SIGNAL   (SIGRTMIN+3)

EXTERN_C UINT_PTR __stdcall FunctionIDMapStub(
    FunctionID funcId,
    void *clientData,
    BOOL *pbHookFunction)
{
    return reinterpret_cast<ExecutionTrace*>(clientData)->
        FunctionIDMap(funcId, pbHookFunction);
}

EXTERN_C void EnterNaked3(FunctionIDOrClientID functionIDOrClientID);
EXTERN_C void LeaveNaked3(FunctionIDOrClientID functionIDOrClientID);
EXTERN_C void TailcallNaked3(FunctionIDOrClientID functionIDOrClientID);

#ifdef _TARGET_ARM_
EXTERN_C UINT_PTR getPrevPC();
#endif // _TARGET_ARM_

EXTERN_C void __stdcall EnterStub(FunctionIDOrClientID functionIDOrClientID)
{
    UINT_PTR ip = 0;

#ifdef _TARGET_ARM_
    ip = getPrevPC();
#endif // _TARGET_ARM_

    FunctionInfo *funcInfo = reinterpret_cast<FunctionInfo*>(
        functionIDOrClientID.clientID);
    funcInfo->executionTrace->Enter(*funcInfo, ip);
}

EXTERN_C void __stdcall LeaveStub(FunctionIDOrClientID functionIDOrClientID)
{
    FunctionInfo *funcInfo = reinterpret_cast<FunctionInfo*>(
        functionIDOrClientID.clientID);
    funcInfo->executionTrace->Leave(*funcInfo);
}

EXTERN_C void __stdcall TailcallStub(FunctionIDOrClientID functionIDOrClientID)
{
    FunctionInfo *funcInfo = reinterpret_cast<FunctionInfo*>(
        functionIDOrClientID.clientID);
    funcInfo->executionTrace->Tailcall(*funcInfo);
}

static void SampleHandlerStub(
    int code, siginfo_t *siginfo, void *context)
{
    if (code == SAMPLE_SIGNAL)
    {
        ExecutionTrace *trace =
            reinterpret_cast<ExecutionTrace*>(siginfo->si_value.sival_ptr);
        trace->HandleSample(context);
    }
}

ExecutionTrace::ExecutionTrace(Profiler &profiler, ProfilerInfo &info)
    : BaseTrace(profiler, info)
    , m_functionStorage(this)
    , m_pUnmanagedFunctionInfo(nullptr)
    , m_pJitFunctionInfo(nullptr)
    , m_logThread()
    , m_samplingThread()
{
    auto storage_lock = m_functionStorage.lock();

    m_pUnmanagedFunctionInfo = &storage_lock->Add(W("UNMANAGED"));
    m_pJitFunctionInfo       = &storage_lock->Add(W("JIT"));
}

ExecutionTrace::~ExecutionTrace()
{
    // NOTE: we are dealing with a partially destroyed m_profiler!
    this->Shutdown();
}

void ExecutionTrace::ProcessConfig(ProfilerConfig &config) noexcept
{
    try
    {
        //
        // Check activation condition.
        //

        if (config.ExecutionTraceEnabled)
        {
            m_disabled = false;
        }
        else
        {
            return;
        }

        //
        // Announce names of the special functions.
        //

        TRACE().DumpJITFunctionName(*m_pUnmanagedFunctionInfo);
        TRACE().DumpJITFunctionName(*m_pJitFunctionInfo);

        //
        // Setup signal handlers.
        //

        if (config.HighGranularityEnabled)
        {
            try
            {
                struct sigaction action;

                if (sigaction(SAMPLE_SIGNAL, nullptr, &action))
                {
                    throw std::system_error(errno, std::system_category(),
                        "ExecutionTrace::ProcessConfig(): sigaction()");
                }

                if (action.sa_handler == SIG_DFL)
                {
                    action.sa_sigaction = SampleHandlerStub;
                    sigemptyset(&action.sa_mask);
                    // sigfillset(&action.sa_mask); // Do we need this?
                    action.sa_flags = SA_RESTART | SA_SIGINFO;
                    if (sigaction(SAMPLE_SIGNAL, &action, nullptr)) {
                        throw std::system_error(errno, std::system_category(),
                            "ExecutionTrace::ProcessConfig(): sigaction()");
                    }
                }
                else
                {
                    throw std::runtime_error(
                        "ExecutionTrace::ProcessConfig(): "
                        "SAMPLE_SIGNAL handler already exists");
                }
            }
            catch (...)
            {
                m_profiler.HandleException(std::current_exception());
                config.HighGranularityEnabled = false;
                LOG().Warn() << "Hight granularity option is disabled";
            }
        }

        //
        // Starting service threads.
        //

#ifdef _TARGET_AMD64_
        std::promise<void> logThreadInitializedPromise;
        std::future<void>  logThreadInitializedFuture =
            logThreadInitializedPromise.get_future();
        m_logThread = std::thread(
            &ExecutionTrace::LogThread, this,
            std::move(logThreadInitializedPromise)
        );
        logThreadInitializedFuture.get();
#else // !_TARGET_AMD64_
        m_logThread = std::thread(&ExecutionTrace::LogThread, this);
        Sleep(1000);
#endif // !_TARGET_AMD64_

        if (config.CollectionMethod == CollectionMethod::Sampling)
        {
            m_samplingThread = std::thread(
                &ExecutionTrace::SamplingThread, this);
        }

        //
        // Line Tracing.
        //

#ifndef _TARGET_ARM_
        if (config.LineTraceEnabled)
        {
            config.LineTraceEnabled = false;
            LOG().Warn() <<
                "Line tracing currently is not supported at this platform";
        }
#endif // _TARGET_ARM_

        //
        // Event Mask calculation.
        //

        HRESULT hr;
        DWORD events;
        hr = m_info.v1()->GetEventMask(&events);
        if (FAILED(hr))
        {
            throw HresultException(
                "ExecutionTrace::ProcessConfig(): GetEventMask()", hr
            );
        }

        // This events are common for execution tracing.
        events = events
            | COR_PRF_MONITOR_JIT_COMPILATION
            | COR_PRF_MONITOR_CACHE_SEARCHES
            | COR_PRF_MONITOR_FUNCTION_UNLOADS;

        if (config.CollectionMethod == CollectionMethod::Instrumentation ||
            config.CollectionMethod == CollectionMethod::Sampling)
        {
            if (m_info.version() < 3)
            {
                LOG().Warn() <<
                    "ICorProfilerInfo3 is required for current configuration";
                goto next_stage;
            }

            m_info.v3()->SetFunctionIDMapper2(FunctionIDMapStub, this);

            hr = m_info.v3()->SetEnterLeaveFunctionHooks3(
                EnterNaked3, LeaveNaked3, TailcallNaked3);
            if (FAILED(hr))
            {
                m_profiler.HandleHresult(
                    "ExecutionTrace::ProcessConfig(): "
                    "SetEnterLeaveFunctionHooks3()", hr
                );
                goto next_stage;
            }

            // This events are required for tracing of call stack dynamics.
            events = events
                | COR_PRF_MONITOR_ENTERLEAVE
                | COR_PRF_MONITOR_CODE_TRANSITIONS
                | COR_PRF_MONITOR_EXCEPTIONS;
        }

    next_stage:
        //
        // Set Event Mask.
        //

        hr = m_info.v1()->SetEventMask(events);
        if (FAILED(hr))
        {
            throw HresultException(
                "ExecutionTrace::ProcessConfig(): SetEventMask()", hr);
        }
    }
    catch (...)
    {
        this->Shutdown();
        m_profiler.HandleException(std::current_exception());
    }
}

void ExecutionTrace::Shutdown() noexcept
{
    m_disabled = true;
    // Ensure service threads are joined before this object will be destroyed.
    if (m_samplingThread.joinable())
    {
        m_samplingThread.join();
    }
    if (m_logThread.joinable())
    {
        this->StopLog();
        m_logThread.join();
    }
}

void ExecutionTrace::GetAdditionalFunctionInfo(FunctionInfo &info)
{
    info.InitializeCodeInfo(m_profiler);
    info.InitializeILToNativeMapping(m_profiler);
    info.InitializeNameAndSignature(m_profiler);
}

__forceinline void ExecutionTrace::SendToLog(ThreadInfo &thrInfo) noexcept
{
    _ASSERTE(m_logThread.joinable());

    union sigval val;
    val.sival_ptr = &thrInfo;
    int ev = pthread_sigqueue(m_logThread.native_handle(), LOG_SIGNAL, val);
    // It is OK if the limit of signals which may be queued has been reached.
    if (ev && ev != EAGAIN)
    {
        m_profiler.HandleSysErr(
            "ExecutionTrace::SendToLog(): pthread_sigqueue()", ev);
    }
}

__forceinline void ExecutionTrace::StopLog() noexcept
{
    _ASSERTE(m_logThread.joinable());

    int ev = pthread_kill(m_logThread.native_handle(), LOG_SIGNAL_STOP);
    // It is OK if pthread_kill() is called after finishing of m_logThread.
    if (ev && ev != ESRCH)
    {
        m_profiler.HandleSysErr(
            "ExecutionTrace::StopLog(): pthread_kill()", ev);
    }
}

__forceinline void ExecutionTrace::SendDoStackTraceSample(
    ThreadInfo &thrInfo) noexcept
{
    union sigval val;
    val.sival_ptr = this;
    int ev = pthread_sigqueue(thrInfo.nativeHandle, SAMPLE_SIGNAL, val);
    // It is OK if the limit of signals which may be queued has been reached.
    if (ev && ev != EAGAIN)
    {
        m_profiler.HandleSysErr(
            "ExecutionTrace::SendDoStackTraceSample(): pthread_sigqueue()", ev);
    }
}

__forceinline bool ExecutionTrace::DoStackTraceSample(
    ThreadInfo &thrInfo, ULONG ticks, ChanCanRealloc canRealloc) noexcept
{
    if (thrInfo.stackChannel.Sample(
        m_profiler.GetTickCountFromInit(), ticks, canRealloc))
    {
        this->SendToLog(thrInfo);
        return true;
    }
    return false;
}

UINT_PTR GetCurrentIPFromHandler(
    COR_PRF_CODE_INFO codeInfo, const void *context) noexcept;

__forceinline void ExecutionTrace::DoStackTraceSampleFromHandler(
    ThreadInfo &thrInfo, void *context) noexcept
{
    _ASSERTE(thrInfo.interruptible);
    ULONG genTicks = thrInfo.genTicks; // Local copy of volatile data.
    if (thrInfo.fixTicks == genTicks)
    {
        return;
    }

    bool enoughtSpace = true;
    if (m_profiler.GetConfig().LineTraceEnabled &&
        thrInfo.stackChannel.GetStackSize() > 0)
    {
        // NOTE: we always use only first block of native code.
        COR_PRF_CODE_INFO codeInfo = thrInfo.stackChannel.Top().firstCodeInfo;
        if (codeInfo.size != 0)
        {
            UINT_PTR ip = GetCurrentIPFromHandler(codeInfo, context);
            enoughtSpace = thrInfo.stackChannel.ChIP(
                // NOTE: we can't reallocate memory from signal handler.
                ip, ChanCanRealloc::NO);
        }
    }

    if (enoughtSpace)
    {
        enoughtSpace = this->DoStackTraceSample(
            // OK with unsigned overflows in ticks.
            thrInfo, genTicks - thrInfo.fixTicks,
            // NOTE: we can't reallocate memory from signal handler.
            ChanCanRealloc::NO);
    }

    if (!enoughtSpace)
    {
        thrInfo.stackChannel.PlanToIncreaseBufferCapacity();
    }

    thrInfo.fixTicks = genTicks;
}

__forceinline void ExecutionTrace::DoStackTraceSampleOnStackChange(
    ThreadInfo &thrInfo, bool force) noexcept
{
    _ASSERTE(!thrInfo.interruptible);
    ULONG genTicks = thrInfo.genTicks; // Local copy of volatile data.
    if (force || thrInfo.fixTicks != genTicks)
    {
        // OK with unsigned overflows in ticks.
        this->DoStackTraceSample(thrInfo, genTicks - thrInfo.fixTicks);
        thrInfo.fixTicks = genTicks;
    }
}

__forceinline void ExecutionTrace::UpdateCallStack(
    std::function<void(StackChannel&)> action) noexcept
{
    ThreadInfo *pThreadInfo = m_profiler.GetCommonTrace().GetThreadInfo();
    if (pThreadInfo != nullptr)
    {
        pThreadInfo->interruptible = false;

        // Do it before action!
        this->DoStackTraceSampleOnStackChange(*pThreadInfo);

        action(pThreadInfo->stackChannel);

        // Call with force mode when Instrumentation.
        this->DoStackTraceSampleOnStackChange(*pThreadInfo,
            m_profiler.GetConfig().CollectionMethod ==
                CollectionMethod::Instrumentation);

        pThreadInfo->interruptible = true;
    }
}

void ExecutionTrace::UpdateCallStackPush(const FunctionInfo &funcInfo) noexcept
{
    this->UpdateCallStack(
        [&funcInfo](StackChannel &channel) {
            channel.Push(funcInfo);
        }
    );
}

void ExecutionTrace::UpdateCallStackPush(
    const FunctionInfo &funcInfo, UINT_PTR prevIP) noexcept
{
    this->UpdateCallStack(
        [&funcInfo, prevIP](StackChannel &channel) {
            if (channel.GetStackSize() > 0)
            {
                channel.ChIP(prevIP);
            }
            channel.Push(funcInfo);
        }
    );
}

void ExecutionTrace::UpdateCallStackPop() noexcept
{
    this->UpdateCallStack(
        [](StackChannel &channel) {
            channel.Pop();
        }
    );
}

#ifdef _TARGET_AMD64_
void ExecutionTrace::LogThread(std::promise<void> initialized) noexcept
#else // !_TARGET_AMD64_
void ExecutionTrace::LogThread() noexcept
#endif // !_TARGET_AMD64_
{
    try
    {
        //
        // Initialization.
        //

        int ev;
        sigset_t set;
        try
        {
            sigemptyset(&set);
            sigaddset(&set, LOG_SIGNAL);
            sigaddset(&set, LOG_SIGNAL_STOP);
            ev = pthread_sigmask(SIG_BLOCK, &set, NULL);
            if (ev)
            {
                throw std::system_error(ev, std::system_category(),
                    "ExecutionTrace::LogThread(): pthread_sigmask()");
            }

#ifdef _TARGET_AMD64_
            initialized.set_value();
#endif // _TARGET_AMD64_
        }
        catch (...)
        {
#ifdef _TARGET_AMD64_
            initialized.set_exception(std::current_exception());
#endif // _TARGET_AMD64_
            return; // NOTE: exception should be handled in parent thread.
        }

        //
        // Working loop.
        //

        bool stop = false;
        for (;;)
        {
            siginfo_t siginfo;
            if (stop)
            {
                struct timespec timeout = {};
                if (sigtimedwait(&set, &siginfo, &timeout) && errno == EAGAIN)
                {
                    break; // NOTE: end of loop!
                }
            }
            else
            {
                sigwaitinfo(&set, &siginfo);
            }
            if (siginfo.si_signo == LOG_SIGNAL_STOP)
            {
                stop = true;
                continue;
            }
            else if (siginfo.si_signo != LOG_SIGNAL)
            {
                continue;
            }

            _ASSERTE(siginfo.si_signo == LOG_SIGNAL);

            ThreadInfo *pThreadInfo =
                reinterpret_cast<ThreadInfo*>(siginfo.si_value.sival_ptr);
            SampleInfo     info;
            StackTraceDiff diff;
            for (
                // Local copy of volatile data.
                size_t count = pThreadInfo->stackChannel.GetSampleInfoCount();
                count > 0; --count)
            {
                std::tie(info, diff) =
                    pThreadInfo->stackChannel.GetNextSampleInfo();
                TRACE().DumpStackTraceSample(
                    pThreadInfo->internalId, info, diff);
            }
        }
    }
    catch (...)
    {
        m_profiler.HandleException(std::current_exception());
    }
}

void ExecutionTrace::SamplingThread() noexcept
{
    try
    {
        while(m_disabled == false)
        {
            Sleep(m_profiler.GetConfig().SamplingTimeoutMs);
            {
                // NOTE: the Thread Storage is locked until the end of cycle.
                for (ThreadInfo &thrInfo :
                    m_profiler.GetCommonTrace().GetThreads())
                {
                    if (m_disabled == true)
                        break;

                    // We update all live threads if they are attached to OS
                    // threads.
                    if (thrInfo.id != 0 && thrInfo.nativeHandle != 0)
                    {
                        thrInfo.genTicks++; // OK with unsigned overflows.
                        if (m_profiler.GetConfig().HighGranularityEnabled)
                        {
                            SendDoStackTraceSample(thrInfo);
                        }
                    }
                }
            }
        }
    }
    catch (...)
    {
        m_profiler.HandleException(std::current_exception());
    }
}

UINT_PTR ExecutionTrace::FunctionIDMap(
    FunctionID funcId,
    BOOL *pbHookFunction) noexcept
{
    LOG().Trace() << "FunctionIDMap()";

    try
    {
        FunctionInfo *pFuncInfo =
            &m_functionStorage.lock()->Place(funcId).first;
        pFuncInfo->executionTrace = this;
        *pbHookFunction = true;
        // This pointer should be stable during all lifetime of the Profiler.
        // It is important feature guaranteed by the BaseStorage class.
        return reinterpret_cast<UINT_PTR>(pFuncInfo);
    }
    catch (...)
    {
        m_profiler.HandleException(std::current_exception());
        *pbHookFunction = false;
        return reinterpret_cast<UINT_PTR>(nullptr);
    }
}

__forceinline void ExecutionTrace::Enter(
    FunctionInfo &funcInfo, UINT_PTR prevIP) noexcept
{
    LOG().Trace() << "EnterStub()";
    if (m_profiler.GetConfig().LineTraceEnabled)
    {
        this->UpdateCallStackPush(funcInfo, prevIP);
    }
    else
    {
        this->UpdateCallStackPush(funcInfo);
    }
}

__forceinline void ExecutionTrace::Leave(FunctionInfo &funcInfo) noexcept
{
    LOG().Trace() << "LeaveStub()";
    this->UpdateCallStackPop();
}

__forceinline void ExecutionTrace::Tailcall(FunctionInfo &funcInfo) noexcept
{
    LOG().Trace() << "TailcallStub()";
    this->UpdateCallStackPop();
}

__forceinline void ExecutionTrace::HandleSample(void *context) noexcept
{
    ThreadInfo *pThreadInfo = m_profiler.GetCommonTrace().GetThreadInfoR();
    if (pThreadInfo && pThreadInfo->interruptible)
    {
        DoStackTraceSampleFromHandler(*pThreadInfo, context);
    }
}

HRESULT ExecutionTrace::FunctionUnloadStarted(
    FunctionID functionId) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        m_functionStorage.lock()->Unlink(functionId);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT ExecutionTrace::JITCompilationStarted(
    FunctionID functionId,
    BOOL fIsSafeToBlock) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        if (m_profiler.GetConfig().LineTraceEnabled)
        {
#ifdef _TARGET_ARM_
            // TODO: get IP of caller.
            // this->UpdateCallStackPush(*m_pJitFunctionInfo, 0);
#else // !_TARGET_ARM_
            this->UpdateCallStackPush(*m_pJitFunctionInfo, 0);
#endif // !_TARGET_ARM_
        }
        else
        {
            this->UpdateCallStackPush(*m_pJitFunctionInfo);
        }
        m_functionStorage.lock()->Place(functionId);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT ExecutionTrace::JITCompilationFinished(
    FunctionID functionId,
    HRESULT hrStatus, BOOL fIsSafeToBlock) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;
    try
    {
        ClassID  classId = 0;
        ModuleID moduleId = 0;
        mdToken  token = 0;

        hr = m_info.v1()->GetFunctionInfo(
            functionId, &classId, &moduleId, &token);

        auto storage_lock = m_functionStorage.lock();
        FunctionInfo &funcInfo = storage_lock->Get(functionId);

        this->GetAdditionalFunctionInfo(funcInfo);

        TRACE().DumpJITCompilationFinished(
            functionId, classId, moduleId, token, hrStatus, funcInfo);
        TRACE().DumpJITFunctionName(funcInfo);

#ifdef _TARGET_ARM_
        if (!m_profiler.GetConfig().LineTraceEnabled)
        {
            this->UpdateCallStackPop();
        }
#else // !_TARGET_ARM_
        this->UpdateCallStackPop();
#endif // !_TARGET_ARM_

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "ExecutionTrace::JITCompilationFinished(): GetFunctionInfo()",
                hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT ExecutionTrace::JITCachedFunctionSearchStarted(
    FunctionID functionId,
    BOOL *pbUseCachedFunction) noexcept
{
    if (m_disabled)
        return S_OK;

    *pbUseCachedFunction = TRUE;

    HRESULT hr = S_OK;
    try
    {
        m_functionStorage.lock()->Place(functionId);
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT ExecutionTrace::JITCachedFunctionSearchFinished(
    FunctionID functionId,
    COR_PRF_JIT_CACHE result) noexcept
{
    if (m_disabled)
        return S_OK;

    HRESULT hr = S_OK;

    if (result != COR_PRF_CACHED_FUNCTION_FOUND)
    {
        return hr;
    }

    try
    {
        ClassID  classId = 0;
        ModuleID moduleId = 0;
        mdToken  token = 0;

        hr = m_info.v1()->GetFunctionInfo(
            functionId, &classId, &moduleId, &token);

        auto storage_lock = m_functionStorage.lock();
        FunctionInfo &funcInfo = storage_lock->Get(functionId);

        this->GetAdditionalFunctionInfo(funcInfo);

        TRACE().DumpJITCachedFunctionSearchFinished(
            functionId, classId, moduleId, token, funcInfo);

        // Do it after dump.
        if (FAILED(hr))
        {
            throw HresultException(
                "ExecutionTrace::JITCachedFunctionSearchFinished(): "
                "GetFunctionInfo()", hr);
        }
    }
    catch (...)
    {
        hr = m_profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT ExecutionTrace::UnmanagedToManagedTransition(
    FunctionID functionId,
    COR_PRF_TRANSITION_REASON reason) noexcept
{
    if (m_disabled)
        return S_OK;

    if (reason == COR_PRF_TRANSITION_RETURN)
    {
        this->UpdateCallStackPop();
    }

    return S_OK;
}

HRESULT ExecutionTrace::ManagedToUnmanagedTransition(
    FunctionID functionId,
    COR_PRF_TRANSITION_REASON reason) noexcept
{
    if (m_disabled)
        return S_OK;

    if (reason == COR_PRF_TRANSITION_CALL)
    {
        if (m_profiler.GetConfig().LineTraceEnabled)
        {
            // TODO: get IP of caller.
            this->UpdateCallStackPush(*m_pUnmanagedFunctionInfo, 0);
        }
        else
        {
            this->UpdateCallStackPush(*m_pUnmanagedFunctionInfo);
        }
    }

    return S_OK;
}

HRESULT ExecutionTrace::ExceptionUnwindFunctionLeave() noexcept
{
    if (m_disabled)
        return S_OK;

    this->UpdateCallStackPop();

    return S_OK;
}
