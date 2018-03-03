// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================
// File: TieredCompilation.CPP
//
// ===========================================================================



#include "common.h"
#include "excep.h"
#include "log.h"
#include "win32threadpool.h"
#include "threadsuspend.h"
#include "tieredcompilation.h"

// TieredCompilationManager determines which methods should be recompiled and
// how they should be recompiled to best optimize the running code. It then
// handles logistics of getting new code created and installed.
//
//
// # Current feature state
//
// This feature is incomplete and currently experimental. To enable it
// you need to set COMPLUS_EXPERIMENTAL_TieredCompilation = 1. When the environment
// variable is unset the runtime should work as normal, but when it is there are a few
// known issues
//   ETW - Native to IL maps aren't correctly emitted (probably tier1 wrong, tier0 right)
//   Profiler - Still missing APIs that allow profilers to correctly get native to IL
//              maps for all code bodies.
//
//  Diagnostic tools have minimal testing that we are aware of and its possible they
//  made additional assumptions about runtime implementation that have been invalidated
//  by this feature. VS debugging does appear to work at a basic level at least.
//   
//  I aim to keep this comment updated as things change.
//
//
// # Important entrypoints in this code:
//
// 
// a) .ctor and Init(...) -  called once during AppDomain initialization
// b) OnMethodCalled(...) -  called when a method is being invoked. When a method
//                           has been called enough times this is currently the only
//                           trigger that initiates re-compilation.
// c) Shutdown() -           called during AppDomain::Exit() to begin the process
//                           of stopping tiered compilation. After this point no more
//                           background optimization work will be initiated but in-progress
//                           work still needs to complete.
// d) ShutdownAllDomains() - Called from EEShutdownHelper to block until all async work is
//                           complete. We must do this before we shutdown the JIT.
//
// # Overall workflow
//
// Methods initially call into OnMethodCalled() and once the call count exceeds
// a fixed limit we queue work on to our internal list of methods needing to
// be recompiled (m_methodsToOptimize). If there is currently no thread
// servicing our queue asynchronously then we use the runtime threadpool
// QueueUserWorkItem to recruit one. During the callback for each threadpool work
// item we handle as many methods as possible in a fixed period of time, then
// queue another threadpool work item if m_methodsToOptimize hasn't been drained.
//
// The background thread enters at StaticOptimizeMethodsCallback(), enters the
// appdomain, and then begins calling OptimizeMethod on each method in the
// queue. For each method we jit it, then update the precode so that future
// entrypoint callers will run the new code.
// 
// # Error handling
//
// The overall principle is don't swallow terminal failures that may have corrupted the
// process (AV for example), but otherwise for any transient issue or functional limitation
// that prevents us from optimizing log it for diagnostics and then back out gracefully,
// continuing to run the less optimal code. The feature should be constructed so that
// errors are limited to OS resource exhaustion or poorly behaved managed code
// (for example within an AssemblyResolve event or static constructor triggered by the JIT).

#ifdef FEATURE_TIERED_COMPILATION

// Called at AppDomain construction
TieredCompilationManager::TieredCompilationManager() :
    m_isAppDomainShuttingDown(FALSE),
    m_countOptimizationThreadsRunning(0),
    m_callCountOptimizationThreshhold(1),
    m_optimizationQuantumMs(50),
    m_methodsPendingCountingForTier1(nullptr),
    m_tier1CountingDelayTimerHandle(nullptr),
    m_wasTier0JitInvokedSinceCountingDelayReset(false)
{
    LIMITED_METHOD_CONTRACT;
    m_lock.Init(LOCK_TYPE_DEFAULT);

    // On Unix, we can reach here before EEConfig is initialized, so defer config-based initialization to Init()
}

// Called at AppDomain Init
void TieredCompilationManager::Init(ADID appDomainId)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        CAN_TAKE_LOCK;
        MODE_PREEMPTIVE;
    }
    CONTRACTL_END;

    SpinLockHolder holder(&m_lock);
    m_domainId = appDomainId;
    m_callCountOptimizationThreshhold = g_pConfig->TieredCompilation_Tier1CallCountThreshold();
}

void TieredCompilationManager::InitiateTier1CountingDelay()
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(g_pConfig->TieredCompilation());
    _ASSERTE(m_methodsPendingCountingForTier1 == nullptr);
    _ASSERTE(m_tier1CountingDelayTimerHandle == nullptr);

    DWORD delayMs = g_pConfig->TieredCompilation_Tier1CallCountingDelayMs();
    if (delayMs == 0)
    {
        return;
    }

    m_tier1CountingDelayLock.Init(LOCK_TYPE_DEFAULT);

    NewHolder<SArray<MethodDesc*>> methodsPendingCountingHolder = new(nothrow) SArray<MethodDesc*>();
    if (methodsPendingCountingHolder == nullptr)
    {
        return;
    }

    NewHolder<ThreadpoolMgr::TimerInfoContext> timerContextHolder = new(nothrow) ThreadpoolMgr::TimerInfoContext();
    if (timerContextHolder == nullptr)
    {
        return;
    }

    timerContextHolder->AppDomainId = m_domainId;
    timerContextHolder->TimerId = 0;
    if (!ThreadpoolMgr::CreateTimerQueueTimer(
            &m_tier1CountingDelayTimerHandle,
            Tier1DelayTimerCallback,
            timerContextHolder,
            delayMs,
            (DWORD)-1 /* Period, non-repeating */,
            0 /* flags */))
    {
        _ASSERTE(m_tier1CountingDelayTimerHandle == nullptr);
        return;
    }

    m_methodsPendingCountingForTier1 = methodsPendingCountingHolder.Extract();
    timerContextHolder.SuppressRelease(); // the timer context is automatically deleted by the timer infrastructure
}

void TieredCompilationManager::OnTier0JitInvoked()
{
    LIMITED_METHOD_CONTRACT;

    if (m_methodsPendingCountingForTier1 != nullptr)
    {
        m_wasTier0JitInvokedSinceCountingDelayReset = true;
    }
}

// Called each time code in this AppDomain has been run. This is our sole entrypoint to begin
// tiered compilation for now. Returns TRUE if no more notifications are necessary, but
// more notifications may come anyways.
//
// currentCallCount is pre-incremented, that is to say the value is 1 on first call for a given
//      method.
void TieredCompilationManager::OnMethodCalled(
    MethodDesc* pMethodDesc,
    DWORD currentCallCount,
    BOOL* shouldStopCountingCallsRef,
    BOOL* wasPromotedToTier1Ref)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(pMethodDesc->IsEligibleForTieredCompilation());
    _ASSERTE(shouldStopCountingCallsRef != nullptr);
    _ASSERTE(wasPromotedToTier1Ref != nullptr);

    *shouldStopCountingCallsRef =
        m_methodsPendingCountingForTier1 != nullptr || currentCallCount >= m_callCountOptimizationThreshhold;
    *wasPromotedToTier1Ref = currentCallCount >= m_callCountOptimizationThreshhold;

    if (currentCallCount == m_callCountOptimizationThreshhold)
    {
        AsyncPromoteMethodToTier1(pMethodDesc);
    }
}

void TieredCompilationManager::OnMethodCallCountingStoppedWithoutTier1Promotion(MethodDesc* pMethodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(pMethodDesc != nullptr);
    _ASSERTE(pMethodDesc->IsEligibleForTieredCompilation());

    if (g_pConfig->TieredCompilation_Tier1CallCountingDelayMs() == 0)
    {
        return;
    }

    {
        SpinLockHolder holder(&m_tier1CountingDelayLock);
        if (m_methodsPendingCountingForTier1 != nullptr)
        {
            // Record the method to resume counting later (see Tier1DelayTimerCallback)
            m_methodsPendingCountingForTier1->Append(pMethodDesc);
            return;
        }
    }

    // Rare race condition with the timer callback
    ResumeCountingCalls(pMethodDesc);
}

void TieredCompilationManager::AsyncPromoteMethodToTier1(MethodDesc* pMethodDesc)
{
    STANDARD_VM_CONTRACT;

    NativeCodeVersion t1NativeCodeVersion;

    // Add an inactive native code entry in the versioning table to track the tier1 
    // compilation we are going to create. This entry binds the compilation to a
    // particular version of the IL code regardless of any changes that may
    // occur between now and when jitting completes. If the IL does change in that
    // interval the new code entry won't be activated.
    {
        CodeVersionManager* pCodeVersionManager = pMethodDesc->GetCodeVersionManager();
        CodeVersionManager::TableLockHolder lock(pCodeVersionManager);
        ILCodeVersion ilVersion = pCodeVersionManager->GetActiveILCodeVersion(pMethodDesc);
        NativeCodeVersionCollection nativeVersions = ilVersion.GetNativeCodeVersions(pMethodDesc);
        for (NativeCodeVersionIterator cur = nativeVersions.Begin(), end = nativeVersions.End(); cur != end; cur++)
        {
            if (cur->GetOptimizationTier() == NativeCodeVersion::OptimizationTier1)
            {
                // we've already promoted
                LOG((LF_TIEREDCOMPILATION, LL_INFO100000, "TieredCompilationManager::AsyncPromoteMethodToTier1 Method=0x%pM (%s::%s) ignoring already promoted method\n",
                    pMethodDesc, pMethodDesc->m_pszDebugClassName, pMethodDesc->m_pszDebugMethodName));
                return;
            }
        }

        HRESULT hr = S_OK;
        if (FAILED(hr = ilVersion.AddNativeCodeVersion(pMethodDesc, &t1NativeCodeVersion)))
        {
            // optimization didn't work for some reason (presumably OOM)
            // just give up and continue on
            STRESS_LOG2(LF_TIEREDCOMPILATION, LL_WARNING, "TieredCompilationManager::AsyncPromoteMethodToTier1: "
                "AddNativeCodeVersion failed hr=0x%x, method=%pM\n",
                hr, pMethodDesc);
            return;
        }
        t1NativeCodeVersion.SetOptimizationTier(NativeCodeVersion::OptimizationTier1);
    }

    // Insert the method into the optimization queue and trigger a thread to service
    // the queue if needed.
    //
    // Terminal exceptions escape as exceptions, but all other errors should gracefully
    // return to the caller. Non-terminal error conditions should be rare (ie OOM,
    // OS failure to create thread) and we consider it reasonable for some methods
    // to go unoptimized or have their optimization arbitrarily delayed under these
    // circumstances. Note an error here could affect concurrent threads running this
    // code. Those threads will observe m_countOptimizationThreadsRunning > 0 and return,
    // then QueueUserWorkItem fails on this thread lowering the count and leaves them 
    // unserviced. Synchronous retries appear unlikely to offer any material improvement 
    // and complicating the code to narrow an already rare error case isn't desirable.
    {
        SListElem<NativeCodeVersion>* pMethodListItem = new (nothrow) SListElem<NativeCodeVersion>(t1NativeCodeVersion);
        SpinLockHolder holder(&m_lock);
        if (pMethodListItem != NULL)
        {
            m_methodsToOptimize.InsertTail(pMethodListItem);
        }

        LOG((LF_TIEREDCOMPILATION, LL_INFO10000, "TieredCompilationManager::AsyncPromoteMethodToTier1 Method=0x%pM (%s::%s), code version id=0x%x queued\n",
            pMethodDesc, pMethodDesc->m_pszDebugClassName, pMethodDesc->m_pszDebugMethodName,
            t1NativeCodeVersion.GetVersionId()));

        if (0 == m_countOptimizationThreadsRunning && !m_isAppDomainShuttingDown)
        {
            // Our current policy throttles at 1 thread, but in the future we
            // could experiment with more parallelism.
            IncrementWorkerThreadCount();
        }
        else
        {
            return;
        }
    }

    EX_TRY
    {
        if (!ThreadpoolMgr::QueueUserWorkItem(StaticOptimizeMethodsCallback, this, QUEUE_ONLY, TRUE))
        {
            SpinLockHolder holder(&m_lock);
            DecrementWorkerThreadCount();
            STRESS_LOG1(LF_TIEREDCOMPILATION, LL_WARNING, "TieredCompilationManager::OnMethodCalled: "
                "ThreadpoolMgr::QueueUserWorkItem returned FALSE (no thread will run), method=%pM\n",
                pMethodDesc);
        }
    }
    EX_CATCH
    {
        SpinLockHolder holder(&m_lock);
        DecrementWorkerThreadCount();
        STRESS_LOG2(LF_TIEREDCOMPILATION, LL_WARNING, "TieredCompilationManager::OnMethodCalled: "
            "Exception queuing work item to threadpool, hr=0x%x, method=%pM\n",
            GET_EXCEPTION()->GetHR(), pMethodDesc);
    }
    EX_END_CATCH(RethrowTerminalExceptions);

    return;
}

void TieredCompilationManager::Shutdown()
{
    STANDARD_VM_CONTRACT;

    SpinLockHolder holder(&m_lock);
    m_isAppDomainShuttingDown = TRUE;
}

VOID WINAPI TieredCompilationManager::Tier1DelayTimerCallback(PVOID parameter, BOOLEAN timerFired)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(timerFired);

    GCX_COOP();
    ThreadpoolMgr::TimerInfoContext* timerContext = (ThreadpoolMgr::TimerInfoContext*)parameter;
    ManagedThreadBase::ThreadPool(timerContext->AppDomainId, Tier1DelayTimerCallbackInAppDomain, nullptr);
}

void TieredCompilationManager::Tier1DelayTimerCallbackInAppDomain(LPVOID parameter)
{
    WRAPPER_NO_CONTRACT;
    GetAppDomain()->GetTieredCompilationManager()->Tier1DelayTimerCallbackWorker();
}

void TieredCompilationManager::Tier1DelayTimerCallbackWorker()
{
    WRAPPER_NO_CONTRACT;

    // Reschedule the timer if a tier 0 JIT has been invoked since the timer was started to further delay call counting
    if (m_wasTier0JitInvokedSinceCountingDelayReset)
    {
        m_wasTier0JitInvokedSinceCountingDelayReset = false;

        _ASSERTE(m_tier1CountingDelayTimerHandle != nullptr);
        if (ThreadpoolMgr::ChangeTimerQueueTimer(
                m_tier1CountingDelayTimerHandle,
                g_pConfig->TieredCompilation_Tier1CallCountingDelayMs(),
                (DWORD)-1 /* Period, non-repeating */))
        {
            return;
        }
    }

    // Exchange the list of methods pending counting for tier 1
    SArray<MethodDesc*>* methodsPendingCountingForTier1;
    {
        SpinLockHolder holder(&m_tier1CountingDelayLock);
        methodsPendingCountingForTier1 = m_methodsPendingCountingForTier1;
        _ASSERTE(methodsPendingCountingForTier1 != nullptr);
        m_methodsPendingCountingForTier1 = nullptr;
    }

    // Install call counters
    MethodDesc** methods = methodsPendingCountingForTier1->GetElements();
    COUNT_T methodCount = methodsPendingCountingForTier1->GetCount();
    for (COUNT_T i = 0; i < methodCount; ++i)
    {
        ResumeCountingCalls(methods[i]);
    }
    delete methodsPendingCountingForTier1;

    // Delete the timer
    _ASSERTE(m_tier1CountingDelayTimerHandle != nullptr);
    ThreadpoolMgr::DeleteTimerQueueTimer(m_tier1CountingDelayTimerHandle, nullptr);
    m_tier1CountingDelayTimerHandle = nullptr;
}

void TieredCompilationManager::ResumeCountingCalls(MethodDesc* pMethodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(pMethodDesc != nullptr);
    _ASSERTE(pMethodDesc->IsVersionableWithPrecode());

    pMethodDesc->GetPrecode()->ResetTargetInterlocked();
}

// This is the initial entrypoint for the background thread, called by
// the threadpool.
DWORD WINAPI TieredCompilationManager::StaticOptimizeMethodsCallback(void *args)
{
    STANDARD_VM_CONTRACT;

    TieredCompilationManager * pTieredCompilationManager = (TieredCompilationManager *)args;
    pTieredCompilationManager->OptimizeMethodsCallback();

    return 0;
}

//This method will process one or more methods from optimization queue
// on a background thread. Each such method will be jitted with code
// optimizations enabled and then installed as the active implementation
// of the method entrypoint.
// 
// We need to be carefuly not to work for too long in a single invocation
// of this method or we could starve the threadpool and force
// it to create unnecessary additional threads.
void TieredCompilationManager::OptimizeMethodsCallback()
{
    STANDARD_VM_CONTRACT;

    // This app domain shutdown check isn't required for correctness
    // but it should reduce some unneeded exceptions trying
    // to enter a closed AppDomain
    {
        SpinLockHolder holder(&m_lock);
        if (m_isAppDomainShuttingDown)
        {
            DecrementWorkerThreadCount();
            return;
        }
    }

    ULONGLONG startTickCount = CLRGetTickCount64();
    NativeCodeVersion nativeCodeVersion;
    EX_TRY
    {
        GCX_COOP();
        ENTER_DOMAIN_ID(m_domainId);
        {
            GCX_PREEMP();
            while (true)
            {
                {
                    SpinLockHolder holder(&m_lock); 
                    nativeCodeVersion = GetNextMethodToOptimize();
                    if (nativeCodeVersion.IsNull() ||
                        m_isAppDomainShuttingDown)
                    {
                        DecrementWorkerThreadCount();
                        break;
                    }
                    
                }
                OptimizeMethod(nativeCodeVersion);

                // If we have been running for too long return the thread to the threadpool and queue another event
                // This gives the threadpool a chance to service other requests on this thread before returning to
                // this work.
                ULONGLONG currentTickCount = CLRGetTickCount64();
                if (currentTickCount >= startTickCount + m_optimizationQuantumMs)
                {
                    if (!ThreadpoolMgr::QueueUserWorkItem(StaticOptimizeMethodsCallback, this, QUEUE_ONLY, TRUE))
                    {
                        SpinLockHolder holder(&m_lock);
                        DecrementWorkerThreadCount();
                        STRESS_LOG0(LF_TIEREDCOMPILATION, LL_WARNING, "TieredCompilationManager::OptimizeMethodsCallback: "
                            "ThreadpoolMgr::QueueUserWorkItem returned FALSE (no thread will run)\n");
                    }
                    break;
                }
            }
        }
        END_DOMAIN_TRANSITION;
    }
    EX_CATCH
    {
        STRESS_LOG2(LF_TIEREDCOMPILATION, LL_ERROR, "TieredCompilationManager::OptimizeMethodsCallback: "
            "Unhandled exception during method optimization, hr=0x%x, last method=%pM\n",
            GET_EXCEPTION()->GetHR(), nativeCodeVersion.GetMethodDesc());
    }
    EX_END_CATCH(RethrowTerminalExceptions);
}

// Jit compiles and installs new optimized code for a method.
// Called on a background thread.
void TieredCompilationManager::OptimizeMethod(NativeCodeVersion nativeCodeVersion)
{
    STANDARD_VM_CONTRACT;

    _ASSERTE(nativeCodeVersion.GetMethodDesc()->IsEligibleForTieredCompilation());
    if (CompileCodeVersion(nativeCodeVersion))
    {
        ActivateCodeVersion(nativeCodeVersion);
    }
}

// Compiles new optimized code for a method.
// Called on a background thread.
BOOL TieredCompilationManager::CompileCodeVersion(NativeCodeVersion nativeCodeVersion)
{
    STANDARD_VM_CONTRACT;

    PCODE pCode = NULL;
    MethodDesc* pMethod = nativeCodeVersion.GetMethodDesc();
    EX_TRY
    {
        pCode = pMethod->PrepareCode(nativeCodeVersion);
        LOG((LF_TIEREDCOMPILATION, LL_INFO10000, "TieredCompilationManager::CompileCodeVersion Method=0x%pM (%s::%s), code version id=0x%x, code ptr=0x%p\n",
            pMethod, pMethod->m_pszDebugClassName, pMethod->m_pszDebugMethodName,
            nativeCodeVersion.GetVersionId(),
            pCode));
    }
    EX_CATCH
    {
        // Failing to jit should be rare but acceptable. We will leave whatever code already exists in place.
        STRESS_LOG2(LF_TIEREDCOMPILATION, LL_INFO10, "TieredCompilationManager::CompileCodeVersion: Method %pM failed to jit, hr=0x%x\n", 
            pMethod, GET_EXCEPTION()->GetHR());
    }
    EX_END_CATCH(RethrowTerminalExceptions)

    return pCode != NULL;
}

// Updates the MethodDesc and precode so that future invocations of a method will
// execute the native code pointed to by pCode.
// Called on a background thread.
void TieredCompilationManager::ActivateCodeVersion(NativeCodeVersion nativeCodeVersion)
{
    STANDARD_VM_CONTRACT;

    MethodDesc* pMethod = nativeCodeVersion.GetMethodDesc();
    CodeVersionManager* pCodeVersionManager = pMethod->GetCodeVersionManager();

    // If the ilParent version is active this will activate the native code version now.
    // Otherwise if the ilParent version becomes active again in the future the native
    // code version will activate then.
    ILCodeVersion ilParent;
    HRESULT hr = S_OK;
    {
        // As long as we are exclusively using precode publishing for tiered compilation
        // methods this first attempt should succeed
        CodeVersionManager::TableLockHolder lock(pCodeVersionManager);
        ilParent = nativeCodeVersion.GetILCodeVersion();
        hr = ilParent.SetActiveNativeCodeVersion(nativeCodeVersion, FALSE);
        LOG((LF_TIEREDCOMPILATION, LL_INFO10000, "TieredCompilationManager::ActivateCodeVersion Method=0x%pM (%s::%s), code version id=0x%x. SetActiveNativeCodeVersion ret=0x%x\n",
            pMethod, pMethod->m_pszDebugClassName, pMethod->m_pszDebugMethodName,
            nativeCodeVersion.GetVersionId(),
            hr));
    }
    if (hr == CORPROF_E_RUNTIME_SUSPEND_REQUIRED)
    {
        // if we start using jump-stamp publishing for tiered compilation, the first attempt
        // without the runtime suspended will fail and then this second attempt will
        // succeed.
        // Even though this works performance is likely to be quite bad. Realistically
        // we are going to need batched updates to makes tiered-compilation + jump-stamp
        // viable. This fallback path is just here as a proof-of-concept.
        ThreadSuspend::SuspendEE(ThreadSuspend::SUSPEND_FOR_REJIT);
        {
            CodeVersionManager::TableLockHolder lock(pCodeVersionManager);
            hr = ilParent.SetActiveNativeCodeVersion(nativeCodeVersion, TRUE);
            LOG((LF_TIEREDCOMPILATION, LL_INFO10000, "TieredCompilationManager::ActivateCodeVersion Method=0x%pM (%s::%s), code version id=0x%x. [Suspended] SetActiveNativeCodeVersion ret=0x%x\n",
                pMethod, pMethod->m_pszDebugClassName, pMethod->m_pszDebugMethodName,
                nativeCodeVersion.GetVersionId(),
                hr));
        }
        ThreadSuspend::RestartEE(FALSE, TRUE);
    }
    if (FAILED(hr))
    {
        STRESS_LOG2(LF_TIEREDCOMPILATION, LL_INFO10, "TieredCompilationManager::ActivateCodeVersion: Method %pM failed to publish native code for native code version %d\n",
            pMethod, nativeCodeVersion.GetVersionId());
    }
}

// Dequeues the next method in the optmization queue.
// This should be called with m_lock already held and runs
// on the background thread.
NativeCodeVersion TieredCompilationManager::GetNextMethodToOptimize()
{
    STANDARD_VM_CONTRACT;

    SListElem<NativeCodeVersion>* pElem = m_methodsToOptimize.RemoveHead();
    if (pElem != NULL)
    {
        NativeCodeVersion nativeCodeVersion = pElem->GetValue();
        delete pElem;
        return nativeCodeVersion;
    }
    return NativeCodeVersion();
}

void TieredCompilationManager::IncrementWorkerThreadCount()
{
    STANDARD_VM_CONTRACT;
    //m_lock should be held

    m_countOptimizationThreadsRunning++;
}

void TieredCompilationManager::DecrementWorkerThreadCount()
{
    STANDARD_VM_CONTRACT;
    //m_lock should be held
    
    m_countOptimizationThreadsRunning--;
}

//static
CORJIT_FLAGS TieredCompilationManager::GetJitFlags(NativeCodeVersion nativeCodeVersion)
{
    LIMITED_METHOD_CONTRACT;

    CORJIT_FLAGS flags;
    if (!nativeCodeVersion.GetMethodDesc()->IsEligibleForTieredCompilation())
    {
#ifdef FEATURE_INTERPRETER
        flags.Set(CORJIT_FLAGS::CORJIT_FLAG_MAKEFINALCODE);
#endif
        return flags;
    }
    
    if (nativeCodeVersion.GetOptimizationTier() == NativeCodeVersion::OptimizationTier0)
    {
        flags.Set(CORJIT_FLAGS::CORJIT_FLAG_TIER0);
    }
    else
    {
        flags.Set(CORJIT_FLAGS::CORJIT_FLAG_TIER1);
#ifdef FEATURE_INTERPRETER
        flags.Set(CORJIT_FLAGS::CORJIT_FLAG_MAKEFINALCODE);
#endif
    }
    return flags;
}

#endif // FEATURE_TIERED_COMPILATION
