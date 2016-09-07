#ifndef _EXECUTION_TRACE_H_
#define _EXECUTION_TRACE_H_

#include <thread>
#include <functional>

#ifdef _TARGET_AMD64_
#include <future>
#endif // _TARGET_AMD64_

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "basetrace.h"

#include "sharedresource.h"
#include "threadinfo.h"
#include "functionstorage.h"

// #include "shared_iterator_range.h"

class ExecutionTrace final : public BaseTrace
{
public:
    ExecutionTrace(Profiler &profiler, ProfilerInfo &info);

    ~ExecutionTrace();

    void ProcessConfig(ProfilerConfig &config) noexcept;

    void Shutdown() noexcept;

private:
    //
    // Various useful instance methods.
    //

    void GetAdditionalFunctionInfo(FunctionInfo &info);

    void SendToLog(ThreadInfo &thrInfo) noexcept;

    void StopLog() noexcept;

    void SendDoStackTraceSample(ThreadInfo &thrInfo) noexcept;

    bool DoStackTraceSample(
        ThreadInfo &thrInfo, ULONG ticks,
        ChanCanRealloc canRealloc = ChanCanRealloc::YES) noexcept;

    void DoStackTraceSampleFromHandler(
        ThreadInfo &thrInfo, void *context) noexcept;

    void DoStackTraceSampleOnStackChange(
        ThreadInfo &thrInfo, bool force = false) noexcept;

    void UpdateCallStack(std::function<void(StackChannel&)> action) noexcept;

    void UpdateCallStackPush(const FunctionInfo &funcInfo) noexcept;

    void UpdateCallStackPush(
        const FunctionInfo &funcInfo, UINT_PTR prevIP) noexcept;

    void UpdateCallStackPop() noexcept;

#ifdef _TARGET_AMD64_
    void LogThread(std::promise<void> initialized) noexcept;
#else // !_TARGET_AMD64_
    void LogThread() noexcept;
#endif // !_TARGET_AMD64_

    void SamplingThread() noexcept;

public:
    //
    // Function Hooks, mapper function and signal handlers.
    // Used by stub function so have to be public.
    //

    UINT_PTR FunctionIDMap(
        FunctionID funcId,
        BOOL *pbHookFunction) noexcept;

    void Enter(FunctionInfo &funcInfo, UINT_PTR prevIP) noexcept;

    void Leave(FunctionInfo &funcInfo) noexcept;

    void Tailcall(FunctionInfo &funcInfo) noexcept;

    void HandleSample(void *context) noexcept;

    //
    // Events.
    //

    HRESULT FunctionUnloadStarted(
        FunctionID functionId) noexcept;

    HRESULT JITCompilationStarted(
        FunctionID functionId,
        BOOL fIsSafeToBlock) noexcept;

    HRESULT JITCompilationFinished(
        FunctionID functionId,
        HRESULT hrStatus, BOOL fIsSafeToBlock) noexcept;

    HRESULT JITCachedFunctionSearchStarted(
        FunctionID functionId,
        BOOL *pbUseCachedFunction) noexcept;

    HRESULT JITCachedFunctionSearchFinished(
        FunctionID functionId,
        COR_PRF_JIT_CACHE result) noexcept;

    HRESULT UnmanagedToManagedTransition(
        FunctionID functionId,
        COR_PRF_TRANSITION_REASON reason) noexcept;

    HRESULT ManagedToUnmanagedTransition(
        FunctionID functionId,
        COR_PRF_TRANSITION_REASON reason) noexcept;

    HRESULT ExceptionUnwindFunctionLeave() noexcept;

private:
    SharedResource<FunctionStorage> m_functionStorage;

    FunctionInfo *m_pUnmanagedFunctionInfo;
    FunctionInfo *m_pJitFunctionInfo;

    std::thread m_logThread;
    std::thread m_samplingThread;
};

#endif // _EXECUTION_TRACE_H_
