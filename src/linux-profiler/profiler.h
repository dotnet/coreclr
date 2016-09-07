#ifndef _PROFILER_H_
#define _PROFILER_H_

#include <exception>
#include <memory>

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "log.h"

#include "loggerconfig.h"
#include "tracelogconfig.h"
#include "profilerconfig.h"
#include "profilerinfo.h"

#include "tracelog.h"

#include "commontrace.h"
#include "executiontrace.h"

// TODO: port to the std::system_error approach.
class HresultException : public std::runtime_error
{
public:
    HresultException(const std::string& what_arg, HRESULT hr)
        : std::runtime_error(what_arg + ": UNKNOWN")
        , m_hresult(hr)
    {}

    HRESULT hresult() const
    {
        return m_hresult;
    }

private:
    HRESULT m_hresult;
};

class Profiler final : public ICorProfilerCallback3
{
public:
    // Instantiate an instance of the callback interface in the Global Area.
    static HRESULT CreateObject(
        REFIID riid,
        void   **ppInterface) noexcept;

    // Remove an instance of the callback interface from the Global Area.
    static void RemoveObject(
        Profiler *pProfiler) noexcept;

private:
    //
    // Profiler should be created and destroyed through public API.
    //

    Profiler();

    virtual ~Profiler();

public:
    // Returns mutable reference to the Logger even for a constant
    // reference to the Profiler.
    Log &LOG() const noexcept;

    // Returns mutable reference to the TraceLog even for a constant
    // reference to the Profiler.
    ITraceLog &TRACE() const noexcept;

    // Retrieves the number of milliseconds that have elapsed since the Profiler
    // was initialized.
    DWORD GetTickCountFromInit() const noexcept;

    // Check type of the exception, send corresponding information to the log
    // and return HRESULT related to this exception.
    HRESULT HandleException(std::exception_ptr eptr) const noexcept;

    // Wrap error code ev with std::system_error using std::system_category()
    // and call to HandleException() to handle it.
    void HandleSysErr(const std::string& what_arg, int ev) const noexcept;

    // Wrap HRESULT with HresultException and call to HandleException()
    // to handle it.
    void HandleHresult(const std::string& what_arg, HRESULT hr) const noexcept;

    //
    // Simple Getters.
    //

    ProfilerConfig &GetConfig() noexcept;

    const ProfilerInfo &GetProfilerInfo() const noexcept;

    CommonTrace &GetCommonTrace() noexcept;

    ExecutionTrace &GetExecutionTrace() noexcept;

private:
    //
    // Various useful instance methods.
    //

    // Apply the configuration to the Logger.
    void SetupLogging(LoggerConfig &config);

    // Apply the configuration to the TraceLog.
    void SetupTraceLog(TraceLogConfig &config);

    // Apply the configuration to the Profiler.
    void ProcessConfig(ProfilerConfig &config);

public:
    //
    // IUnknown methods.
    //

    virtual HRESULT STDMETHODCALLTYPE QueryInterface(
        REFIID riid,
        void **ppvObject) override;

    virtual ULONG STDMETHODCALLTYPE AddRef() override;

    virtual ULONG STDMETHODCALLTYPE Release() override;

    //
    // Startup/shutdown events.
    //

    virtual HRESULT STDMETHODCALLTYPE Initialize(
        IUnknown *pICorProfilerInfoUnk) override;

    virtual HRESULT STDMETHODCALLTYPE Shutdown() override;

    //
    // Application domain events.
    //

    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted(
        AppDomainID appDomainId) override;

    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished(
        AppDomainID appDomainId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted(
        AppDomainID appDomainId) override;

    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished(
        AppDomainID appDomainId,
        HRESULT hrStatus) override;

    //
    // Assembly events.
    //

    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted(
        AssemblyID assemblyId) override;

    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished(
        AssemblyID assemblyId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted(
        AssemblyID assemblyId) override;

    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished(
        AssemblyID assemblyId,
        HRESULT hrStatus) override;

    //
    // Module events.
    //

    virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted(
        ModuleID moduleId) override;

    virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished(
        ModuleID moduleId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted(
        ModuleID moduleId) override;

    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished(
        ModuleID moduleId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly(
        ModuleID moduleId,
        AssemblyID assemblyId) override;

    //
    // Class events.
    //

    virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted(
        ClassID classId) override;

    virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished(
        ClassID classId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted(
        ClassID classId) override;

    virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished(
        ClassID classId,
        HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted(
        FunctionID functionId) override;

    //
    // Jit events.
    //

    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted(
        FunctionID functionId,
        BOOL fIsSafeToBlock) override;

    virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished(
        FunctionID functionId,
        HRESULT hrStatus,
        BOOL fIsSafeToBlock) override;

    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted(
        FunctionID functionId,
        BOOL *pbUseCachedFunction) override;

    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished(
        FunctionID functionId,
        COR_PRF_JIT_CACHE result) override;

    virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE JITInlining(
        FunctionID callerId,
        FunctionID calleeId,
        BOOL *pfShouldInline) override;

    //
    // Thread events.
    //

    virtual HRESULT STDMETHODCALLTYPE ThreadCreated(
        ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(
        ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(
        ThreadID managedThreadId,
        DWORD osThreadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged(
        ThreadID threadId,
        ULONG cchName,
        _In_reads_opt_(cchName) WCHAR name[]) override;

    //
    // Remoting events.
    //

    // Client-side events.

    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted() override;

    virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage(
        GUID *pCookie,
        BOOL fIsAsync) override;

    virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply(
        GUID *pCookie,
        BOOL fIsAsync) override;

    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished() override;

    // Server-side events.

    virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage(
        GUID *pCookie,
        BOOL fIsAsync) override;

    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted() override;

    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned() override;

    virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply(
        GUID *pCookie,
        BOOL fIsAsync) override;

    //
    // Transition events.
    //

    virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition(
        FunctionID functionId,
        COR_PRF_TRANSITION_REASON reason) override;

    virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition(
        FunctionID functionId,
        COR_PRF_TRANSITION_REASON reason) override;

    //
    // Runtime suspension events.
    //

    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted(
        COR_PRF_SUSPEND_REASON suspendReason) override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished() override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted() override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted() override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished() override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended(
        ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed(
        ThreadID threadId) override;

    //
    // GC events.
    //

    virtual HRESULT STDMETHODCALLTYPE MovedReferences(
        ULONG cMovedObjectIDRanges,
        ObjectID oldObjectIDRangeStart[],
        ObjectID newObjectIDRangeStart[],
        ULONG cObjectIDRangeLength[]) override;

    virtual HRESULT STDMETHODCALLTYPE ObjectAllocated(
        ObjectID objectId,
        ClassID classId) override;

    virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass(
        ULONG cClassCount,
        ClassID classIds[],
        ULONG cObjects[]) override;

    virtual HRESULT STDMETHODCALLTYPE ObjectReferences(
        ObjectID objectId,
        ClassID classId,
        ULONG cObjectRefs,
        ObjectID objectRefIds[]) override;

    virtual HRESULT STDMETHODCALLTYPE RootReferences(
        ULONG cRootRefs,
        ObjectID rootRefIds[]) override;

    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted(
        int cGenerations,
        BOOL generationCollected[],
        COR_PRF_GC_REASON reason) override;

    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences(
        ULONG cSurvivingObjectIDRanges,
        ObjectID objectIDRangeStart[],
        ULONG cObjectIDRangeLength[]) override;

    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished() override;

    virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued(
        DWORD finalizerFlags,
        ObjectID objectId) override;

    virtual HRESULT STDMETHODCALLTYPE RootReferences2(
        ULONG cRootRefs,
        ObjectID rootRefIds[],
        COR_PRF_GC_ROOT_KIND rootKinds[],
        COR_PRF_GC_ROOT_FLAGS rootFlags[],
        UINT_PTR rootIds[]) override;

    virtual HRESULT STDMETHODCALLTYPE HandleCreated(
        GCHandleID handleId,
        ObjectID initialObjectId) override;

    virtual HRESULT STDMETHODCALLTYPE HandleDestroyed(
        GCHandleID handleId) override;

    //
    // Exception events.
    //

    // Exception creation.

    virtual HRESULT STDMETHODCALLTYPE ExceptionThrown(
        ObjectID thrownObjectId) override;

    // Search phase.

    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave() override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave() override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter(
        UINT_PTR __unused) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave(
        UINT_PTR __unused) override;

    // Unwind phase.

    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave() override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter(
        FunctionID functionId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave() override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter(
        FunctionID functionId, ObjectID objectId) override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave() override;

    //
    // COM classic wrapper.
    //

    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated(
        ClassID wrappedClassId,
        REFGUID implementedIID,
        void *pVTable,
        ULONG cSlots) override;

    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed(
        ClassID wrappedClassId,
        REFGUID implementedIID,
        void *pVTable) override;

    //
    // Attach events.
    //

    virtual HRESULT STDMETHODCALLTYPE InitializeForAttach(
        IUnknown *pCorProfilerInfoUnk,
        void *pvClientData,
        UINT cbClientData) override;

    virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete() override;

    virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded() override;

    //
    // DEPRECATED. These callbacks are no longer delivered.
    //

    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound() override;

    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute() override;

private:
    LONG m_cRef;

    Log  m_logger;

    BOOL m_initialized;
    BOOL m_shutdowned;

    LoggerConfig   m_loggerConfig;
    TraceLogConfig m_traceLogConfig;
    ProfilerConfig m_profConfig;
    ProfilerInfo   m_info;

    std::unique_ptr<ITraceLog> m_traceLog;

    CommonTrace    m_commonTrace;
    ExecutionTrace m_executionTrace;

    DWORD m_firstTickCount;
};

#endif // _PROFILER_H_
