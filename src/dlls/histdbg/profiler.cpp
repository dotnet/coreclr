#include "profiler.h"
#include "log.h"
#include "historicaldebugging.h"

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}

MIDL_DEFINE_GUID(IID, IID_IUnknown, 0x00000000, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
MIDL_DEFINE_GUID(IID, IID_IClassFactory, 0x00000001, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
MIDL_DEFINE_GUID(IID, IID_ICorProfilerCallback,0x176FBED1,0xA55C,0x4796,0x98,0xCA,0xA9,0xDA,0x0E,0xF8,0x83,0xE7);
MIDL_DEFINE_GUID(IID, IID_ICorProfilerCallback2,0x8A8CC829,0xCCF2,0x49fe,0xBB,0xAE,0x0F,0x02,0x22,0x28,0x07,0x1A);
MIDL_DEFINE_GUID(IID, IID_ICorProfilerCallback3,0x4FD2ED52,0x7731,0x4b8d,0x94,0x69,0x03,0xD2,0xCC,0x30,0x86,0xC5);
MIDL_DEFINE_GUID(IID, IID_ICorProfilerInfo3,0xB555ED4F,0x452A,0x4E54,0x8B,0x39,0xB5,0x36,0x0B,0xAD,0x32,0xA0);

Profiler::Profiler()
	: m_referenceCount(1)
{
}

Profiler::~Profiler()
{
}

HRESULT STDMETHODCALLTYPE Profiler::QueryInterface(REFIID riid, void **ppvObject)
{
	if (riid == IID_ICorProfilerCallback3 ||
		riid == IID_ICorProfilerCallback2 ||
		riid == IID_ICorProfilerCallback ||
		riid == IID_IUnknown)
	{
		*ppvObject = this;
		this->AddRef();

		return S_OK;
	}

	*ppvObject = NULL;
	return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE Profiler::AddRef(void)
{
	return __sync_fetch_and_add(&m_referenceCount, 1) + 1;
}

ULONG STDMETHODCALLTYPE Profiler::Release(void)
{
	LONG result = __sync_fetch_and_sub(&m_referenceCount, 1) - 1;
	if (result == 0)
	{
		delete this;
	}

	return result;
}

static IUnknown *g_pICorProfilerInfoUnknown;

void OnFunctionEnter(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo) {
	ICorProfilerInfo3* info;
	HRESULT hr = g_pICorProfilerInfoUnknown->QueryInterface(IID_ICorProfilerInfo3, (void **) &info);
	if (hr != S_OK)
	{
		assert(false && "Failed to retreive ICorProfilerInfo3");
	}
	ThreadID threadID;
	info->GetCurrentThreadID(&threadID);
	NotifySave(threadID, info, (void*)eltInfo);
	info->Release();
}
void OnFunctionLeave(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo) {
	(void)eltInfo;
	ICorProfilerInfo3* info;
	HRESULT hr = g_pICorProfilerInfoUnknown->QueryInterface(IID_ICorProfilerInfo3, (void **) &info);
	if (hr != S_OK)
	{
		assert(false && "Failed to retreive ICorProfilerInfo3");
	}
	ThreadID threadID;
	info->GetCurrentThreadID(&threadID);
	NotifyPop(threadID, info, (void*)eltInfo);
	info->Release();
}

HRESULT STDMETHODCALLTYPE Profiler::Initialize(IUnknown *pICorProfilerInfoUnk)
{
	g_pICorProfilerInfoUnknown = pICorProfilerInfoUnk;
	HRESULT hr = pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo3, (void **) &info);
	if (hr == S_OK && info != NULL)
	{
		info->SetEventMask(COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_STACK_SNAPSHOT);
		info->SetEnterLeaveFunctionHooks3WithInfo(OnFunctionEnter, OnFunctionLeave, NULL);
		info->Release();
		info = NULL;
	}
	LogInitialize();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::Shutdown(void)
{
	LogFinalize();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationStarted(AppDomainID appDomainId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownStarted(AppDomainID appDomainId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadStarted(AssemblyID assemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadStarted(AssemblyID assemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadStarted(ModuleID moduleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadStarted(ModuleID moduleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadStarted(ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadStarted(ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadFinished(ClassID classId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FunctionUnloadStarted(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL *pbUseCachedFunction)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITFunctionPitched(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITInlining(FunctionID callerId, FunctionID calleeId, BOOL *pfShouldInline)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadCreated(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadDestroyed(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationStarted(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationFinished(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationStarted(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationReturned(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendFinished(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendAborted(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeStarted(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeFinished(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadSuspended(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadResumed(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectAllocated(ObjectID objectId, ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[], ULONG cObjects[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID objectRefIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences(ULONG cRootRefs, ObjectID rootRefIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionThrown(ObjectID thrownObjectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionLeave(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterLeave(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchCatcherFound(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerEnter(UINT_PTR __unused)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerLeave(UINT_PTR __unused)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionLeave(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyLeave(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherLeave(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable, ULONG cSlots)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherFound(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherExecute(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadNameChanged(ThreadID threadId, ULONG cchName, _In_reads_opt_(cchName) WCHAR name[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionFinished(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleCreated(GCHandleID handleId, ObjectID initialObjectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleDestroyed(GCHandleID handleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::InitializeForAttach(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerAttachComplete(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerDetachSucceeded(void)
{
	return S_OK;
}
