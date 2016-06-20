#include <pal.h>
#include <ntimage.h>
#include <corhdr.h>
#include <cor.h>
#include <corprof.h>

class Profiler : public ICorProfilerCallback3
{
public:
	Profiler();
	virtual ~Profiler();
	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppvObject) override;
	virtual ULONG STDMETHODCALLTYPE AddRef(void) override;
	virtual ULONG STDMETHODCALLTYPE Release(void) override;
  virtual HRESULT STDMETHODCALLTYPE Initialize(IUnknown *pICorProfilerInfoUnk) override;
  virtual HRESULT STDMETHODCALLTYPE Shutdown(void) override;
  virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted(AppDomainID appDomainId) override;
  virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted(AppDomainID appDomainId) override;
  virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted(AssemblyID assemblyId) override;
  virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted(AssemblyID assemblyId) override;
  virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted(ModuleID moduleId) override;
  virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted(ModuleID moduleId) override;
  virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId) override;
  virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted(ClassID classId) override;
  virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished(ClassID classId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted(ClassID classId) override;
  virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished(ClassID classId, HRESULT hrStatus) override;
  virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock) override;
  virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock) override;
  virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted(FunctionID functionId, BOOL *pbUseCachedFunction) override;
  virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result) override;
  virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE JITInlining(FunctionID callerId, FunctionID calleeId, BOOL *pfShouldInline) override;
  virtual HRESULT STDMETHODCALLTYPE ThreadCreated(ThreadID threadId) override;
  virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(ThreadID threadId) override;
  virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted(void) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished(void) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted(void) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned(void) override;
  virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync) override;
  virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) override;
  virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished(void) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted(void) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted(void) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished(void) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended(ThreadID threadId) override;
  virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed(ThreadID threadId) override;
  virtual HRESULT STDMETHODCALLTYPE MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[]) override;
  virtual HRESULT STDMETHODCALLTYPE ObjectAllocated(ObjectID objectId, ClassID classId) override;
  virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[], ULONG cObjects[]) override;
  virtual HRESULT STDMETHODCALLTYPE ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID objectRefIds[]) override;
  virtual HRESULT STDMETHODCALLTYPE RootReferences(ULONG cRootRefs, ObjectID rootRefIds[]) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionThrown(ObjectID thrownObjectId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave(void) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave(void) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter(UINT_PTR __unused) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave(UINT_PTR __unused) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave(void) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter(FunctionID functionId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave(void) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave(void) override;
  virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable, ULONG cSlots) override;
  virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound(void) override;
  virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute(void) override;
  virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged(ThreadID threadId, ULONG cchName, _In_reads_opt_(cchName) WCHAR name[]) override;
  virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason) override;
  virtual HRESULT STDMETHODCALLTYPE SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[]) override;
  virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished(void) override;
  virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID) override;
  virtual HRESULT STDMETHODCALLTYPE RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[]) override;
  virtual HRESULT STDMETHODCALLTYPE HandleCreated(GCHandleID handleId, ObjectID initialObjectId) override;
  virtual HRESULT STDMETHODCALLTYPE HandleDestroyed(GCHandleID handleId) override;
	virtual HRESULT STDMETHODCALLTYPE InitializeForAttach(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData) override;
  virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete(void) override;
  virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded(void) override;

private:
	LONG m_referenceCount;
  ICorProfilerInfo3 *info;
};
