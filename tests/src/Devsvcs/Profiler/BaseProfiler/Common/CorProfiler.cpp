// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include "CorProfiler.h"
#include "corhlpr.h"
#include "CComPtr.h"
#include "profiler_pal.h"

CorProfiler * CorProfiler::This = NULL;

#ifdef ELT3
#pragma region Enter(2)/Leave(2)/Tailcall(2)

EXTERN_C UINT_PTR STDMETHODCALLTYPE FunctionIDMapperStub(FunctionID functionId, BOOL *pbHookFunction)
{
	return CorProfiler::Instance()->FunctionIDMapper(functionId, pbHookFunction);
}

EXTERN_C UINT_PTR STDMETHODCALLTYPE FunctionIDMapper2Stub(FunctionID functionId, void * clientData, BOOL *pbHookFunction)
{
	return CorProfiler::Instance()->FunctionIDMapper2(functionId, clientData, pbHookFunction);
}

EXTERN_C VOID STDMETHODCALLTYPE EnterStub(FunctionID funcId)
{
	CorProfiler::Instance()->FunctionEnterCallBack(funcId);
}

EXTERN_C VOID STDMETHODCALLTYPE LeaveStub(FunctionID funcId)
{
	CorProfiler::Instance()->FunctionLeaveCallBack(funcId);
}

EXTERN_C VOID STDMETHODCALLTYPE TailcallStub(FunctionID funcId)
{
	CorProfiler::Instance()->FunctionTailcallCallBack(funcId);
}

VOID STDMETHODCALLTYPE Enter2Stub(FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO *pArgInfo)
{
	CorProfiler::Instance()->FunctionEnter2CallBack(funcId, clientData, frameInfo, pArgInfo);
}

EXTERN_C
DECLSPEC_EXPORT
VOID STDMETHODCALLTYPE Leave2Stub(FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE *pRetVal)
{
	CorProfiler::Instance()->FunctionLeave2CallBack(funcId, clientData, frameInfo, pRetVal);
}

EXTERN_C
DECLSPEC_EXPORT
VOID STDMETHODCALLTYPE Tailcall2Stub(FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo)
{
	CorProfiler::Instance()->FunctionTailcall2CallBack(funcId, clientData, frameInfo);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionEnter3Stub(FunctionIDOrClientID functionIDOrClientID)
{
	CorProfiler::Instance()->FunctionEnter3CallBack(functionIDOrClientID);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionLeave3Stub(FunctionIDOrClientID functionIDOrClientID)
{
	CorProfiler::Instance()->FunctionLeave3CallBack(functionIDOrClientID);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionTailCall3Stub(FunctionIDOrClientID functionIDOrClientID)
{
	CorProfiler::Instance()->FunctionTailCall3CallBack(functionIDOrClientID);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionEnter3WithInfoStub(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	CorProfiler::Instance()->FunctionEnter3WithInfoCallBack(functionIDOrClientID, eltInfo);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionLeave3WithInfoStub(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	CorProfiler::Instance()->FunctionLeave3WithInfoCallBack(functionIDOrClientID, eltInfo);
}

EXTERN_C VOID STDMETHODCALLTYPE FunctionTailCall3WithInfoStub(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	CorProfiler::Instance()->FunctionTailCall3WithInfoCallBack(functionIDOrClientID, eltInfo);
}


#pragma endregion // Enter(2)/Leave(2)/Tailcall(2)

#endif//ELT3

static void STDMETHODCALLTYPE Enter(FunctionID functionId)
{
    printf("\r\nEnter %" UINT_PTR_FORMAT "", (UINT64)functionId);
}

static void STDMETHODCALLTYPE Leave(FunctionID functionId)
{
    printf("\r\nLeave %" UINT_PTR_FORMAT "", (UINT64)functionId);
}

COR_SIGNATURE enterLeaveMethodSignature             [] = { IMAGE_CEE_CS_CALLCONV_STDCALL, 0x01, ELEMENT_TYPE_VOID, ELEMENT_TYPE_I };

void(STDMETHODCALLTYPE *EnterMethodAddress)(FunctionID) = &Enter;
void(STDMETHODCALLTYPE *LeaveMethodAddress)(FunctionID) = &Leave;

CorProfiler::CorProfiler() : refCount(0), m_pProfilerInfo(nullptr)
{
	CorProfiler::This = this;
}


CorProfiler::~CorProfiler()
{
    if (this->m_pProfilerInfo != nullptr)
    {
        this->m_pProfilerInfo->Release();
        this->m_pProfilerInfo = nullptr;
    }
	CorProfiler::This = NULL;
}


HRESULT STDMETHODCALLTYPE CorProfiler::Initialize(IUnknown *pICorProfilerInfoUnk)
{
    HRESULT queryInterfaceResult = pICorProfilerInfoUnk->QueryInterface(__uuidof(ICorProfilerInfo7), reinterpret_cast<void **>(&this->m_pProfilerInfo));

    if (FAILED(queryInterfaceResult))
    {
        return E_FAIL;
    }

	PPRFCOM = new PrfCommon(m_pProfilerInfo);

	HRESULT hr = CommonInit();
	if (hr != S_OK)
	{
		CorProfiler::This = NULL;
		DISPLAY(L"BaseProfilerDriver::Initialize exits with" << HEX(hr));
		return E_FAIL;
	}

#ifdef ELT3
	// Set the ELT hooks and FunctionID mapper
	hr = SetELTHooks();
#endif//ELT3
	return hr;
}

HRESULT STDMETHODCALLTYPE CorProfiler::Shutdown()
{
    if (this->m_pProfilerInfo != nullptr)
    {
        this->m_pProfilerInfo->Release();
        this->m_pProfilerInfo = nullptr;
    }

	HRESULT hr = S_OK;
	InterlockedIncrement(&PPRFCOM->m_Shutdown);
	//TODO
	hr = VerificationRoutine();
	
	m_methodTable.FUNCTIONIDMAPPER = NULL;
	m_methodTable.FUNCTIONIDMAPPER2 = NULL;
	m_methodTable.FUNCTIONENTER = NULL;
	m_methodTable.FUNCTIONENTER2 = NULL;
	m_methodTable.FUNCTIONENTER3 = NULL;
	m_methodTable.FUNCTIONENTER3WITHINFO = NULL;
	m_methodTable.VERIFY = NULL;

	m_methodTable.APPDOMAINCREATIONSTARTED = NULL;
	m_methodTable.APPDOMAINCREATIONFINISHED = NULL;
	m_methodTable.APPDOMAINSHUTDOWNSTARTED = NULL;
	m_methodTable.APPDOMAINSHUTDOWNFINISHED = NULL;
	m_methodTable.ASSEMBLYLOADSTARTED = NULL;
	m_methodTable.ASSEMBLYLOADFINISHED = NULL;
	m_methodTable.ASSEMBLYUNLOADSTARTED = NULL;
	m_methodTable.ASSEMBLYUNLOADFINISHED = NULL;
	m_methodTable.MODULELOADSTARTED = NULL;
	m_methodTable.MODULELOADFINISHED = NULL;
	m_methodTable.MODULEUNLOADSTARTED = NULL;
	m_methodTable.MODULEUNLOADFINISHED = NULL;
	m_methodTable.MODULEATTACHEDTOASSEMBLY = NULL;
	m_methodTable.CLASSLOADSTARTED = NULL;
	m_methodTable.CLASSLOADFINISHED = NULL;
	m_methodTable.CLASSUNLOADSTARTED = NULL;
	m_methodTable.CLASSUNLOADFINISHED = NULL;
	m_methodTable.FUNCTIONUNLOADSTARTED = NULL;
	m_methodTable.JITCOMPILATIONSTARTED = NULL;
	m_methodTable.JITCOMPILATIONFINISHED = NULL;
	m_methodTable.JITCACHEDFUNCTIONSEARCHSTARTED = NULL;
	m_methodTable.JITCACHEDFUNCTIONSEARCHFINISHED = NULL;
	m_methodTable.JITFUNCTIONPITCHED = NULL;
	m_methodTable.JITINLINING = NULL;
	m_methodTable.THREADCREATED = NULL;
	m_methodTable.THREADDESTROYED = NULL;
	m_methodTable.THREADASSIGNEDTOOSTHREAD = NULL;
	m_methodTable.THREADNAMECHANGED = NULL;
	m_methodTable.REMOTINGCLIENTINVOCATIONSTARTED = NULL;
	m_methodTable.REMOTINGCLIENTSENDINGMESSAGE = NULL;
	m_methodTable.REMOTINGCLIENTRECEIVINGREPLY = NULL;
	m_methodTable.REMOTINGCLIENTINVOCATIONFINISHED = NULL;
	m_methodTable.REMOTINGSERVERRECEIVINGMESSAGE = NULL;
	m_methodTable.REMOTINGSERVERINVOCATIONSTARTED = NULL;
	m_methodTable.REMOTINGSERVERINVOCATIONRETURNED = NULL;
	m_methodTable.REMOTINGSERVERSENDINGREPLY = NULL;
	m_methodTable.UNMANAGEDTOMANAGEDTRANSITION = NULL;
	m_methodTable.MANAGEDTOUNMANAGEDTRANSITION = NULL;
	m_methodTable.RUNTIMESUSPENDSTARTED = NULL;
	m_methodTable.RUNTIMESUSPENDFINISHED = NULL;
	m_methodTable.RUNTIMESUSPENDABORTED = NULL;
	m_methodTable.RUNTIMERESUMESTARTED = NULL;
	m_methodTable.RUNTIMERESUMEFINISHED = NULL;
	m_methodTable.RUNTIMETHREADSUSPENDED = NULL;
	m_methodTable.RUNTIMETHREADRESUMED = NULL;
	m_methodTable.MOVEDREFERENCES = NULL;
	m_methodTable.MOVEDREFERENCES2 = NULL;
	m_methodTable.OBJECTALLOCATED = NULL;
	m_methodTable.OBJECTSALLOCATEDBYCLASS = NULL;
	m_methodTable.OBJECTREFERENCES = NULL;
	m_methodTable.ROOTREFERENCES = NULL;
	m_methodTable.EXCEPTIONTHROWN = NULL;
	m_methodTable.EXCEPTIONSEARCHFUNCTIONENTER = NULL;
	m_methodTable.EXCEPTIONSEARCHFUNCTIONLEAVE = NULL;
	m_methodTable.EXCEPTIONSEARCHFILTERENTER = NULL;
	m_methodTable.EXCEPTIONSEARCHFILTERLEAVE = NULL;
	m_methodTable.EXCEPTIONSEARCHCATCHERFOUND = NULL;
	m_methodTable.EXCEPTIONOSHANDLERENTER = NULL;
	m_methodTable.EXCEPTIONOSHANDLERLEAVE = NULL;
	m_methodTable.EXCEPTIONUNWINDFUNCTIONENTER = NULL;
	m_methodTable.EXCEPTIONUNWINDFUNCTIONLEAVE = NULL;
	m_methodTable.EXCEPTIONUNWINDFINALLYENTER = NULL;
	m_methodTable.EXCEPTIONUNWINDFINALLYLEAVE = NULL;
	m_methodTable.EXCEPTIONCATCHERENTER = NULL;
	m_methodTable.EXCEPTIONCATCHERLEAVE = NULL;
	m_methodTable.EXCEPTIONCLRCATCHERFOUND = NULL;
	m_methodTable.EXCEPTIONCLRCATCHEREXECUTE = NULL;
	m_methodTable.COMCLASSICVTABLECREATED = NULL;
	m_methodTable.COMCLASSICVTABLEDESTROYED = NULL;
	m_methodTable.GARBAGECOLLECTIONSTARTED = NULL;
	m_methodTable.GARBAGECOLLECTIONFINISHED = NULL;
	m_methodTable.FINALIZEABLEOBJECTQUEUED = NULL;
	m_methodTable.ROOTREFERENCES2 = NULL;
	m_methodTable.HANDLECREATED = NULL;
	m_methodTable.HANDLEDESTROYED = NULL;
	m_methodTable.SURVIVINGREFERENCES = NULL;
	m_methodTable.SURVIVINGREFERENCES2 = NULL;
	m_methodTable.PROFILERATTACHCOMPLETE = NULL;
	m_methodTable.FUNCTIONENTER = NULL;
	m_methodTable.FUNCTIONLEAVE = NULL;
	m_methodTable.FUNCTIONTAILCALL = NULL;
	m_methodTable.FUNCTIONENTER2 = NULL;
	m_methodTable.FUNCTIONLEAVE2 = NULL;
	m_methodTable.FUNCTIONTAILCALL2 = NULL;
	m_methodTable.FUNCTIONENTER3 = NULL;
	m_methodTable.FUNCTIONLEAVE3 = NULL;
	m_methodTable.FUNCTIONTAILCALL3 = NULL;
	m_methodTable.FUNCTIONENTER3WITHINFO = NULL;
	m_methodTable.FUNCTIONLEAVE3WITHINFO = NULL;
	m_methodTable.FUNCTIONTAILCALL3WITHINFO = NULL;

	return hr;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AppDomainCreationStarted(AppDomainID appDomainId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AppDomainShutdownStarted(AppDomainID appDomainId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyLoadStarted(AssemblyID assemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyUnloadStarted(AssemblyID assemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleLoadStarted(ModuleID moduleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleUnloadStarted(ModuleID moduleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ClassLoadStarted(ClassID classId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ClassUnloadStarted(ClassID classId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ClassUnloadFinished(ClassID classId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::FunctionUnloadStarted(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL *pbUseCachedFunction)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITFunctionPitched(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITInlining(FunctionID callerId, FunctionID calleeId, BOOL *pfShouldInline)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ThreadCreated(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ThreadDestroyed(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingClientInvocationStarted()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingClientInvocationFinished()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingServerInvocationStarted()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingServerInvocationReturned()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeSuspendFinished()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeSuspendAborted()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeResumeStarted()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeResumeFinished()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeThreadSuspended(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RuntimeThreadResumed(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ObjectAllocated(ObjectID objectId, ClassID classId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[], ULONG cObjects[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID objectRefIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RootReferences(ULONG cRootRefs, ObjectID rootRefIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionThrown(ObjectID thrownObjectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionSearchFunctionEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionSearchFunctionLeave()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionSearchFilterEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionSearchFilterLeave()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionSearchCatcherFound(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionOSHandlerEnter(UINT_PTR __unused)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionOSHandlerLeave(UINT_PTR __unused)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionUnwindFunctionEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionUnwindFunctionLeave()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionUnwindFinallyEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionUnwindFinallyLeave()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionCatcherLeave()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable, ULONG cSlots)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionCLRCatcherFound()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ExceptionCLRCatcherExecute()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ThreadNameChanged(ThreadID threadId, ULONG cchName, WCHAR name[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::GarbageCollectionFinished()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::HandleCreated(GCHandleID handleId, ObjectID initialObjectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::HandleDestroyed(GCHandleID handleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::InitializeForAttach(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ProfilerAttachComplete()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ProfilerDetachSucceeded()
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ReJITCompilationStarted(FunctionID functionId, ReJITID rejitId, BOOL fIsSafeToBlock)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::GetReJITParameters(ModuleID moduleId, mdMethodDef methodId, ICorProfilerFunctionControl *pFunctionControl)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ReJITCompilationFinished(FunctionID functionId, ReJITID rejitId, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ReJITError(ModuleID moduleId, mdMethodDef methodId, FunctionID functionId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::MovedReferences2(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], SIZE_T cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::SurvivingReferences2(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], SIZE_T cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ConditionalWeakTableElementReferences(ULONG cRootRefs, ObjectID keyRefIds[], ObjectID valueRefIds[], GCHandleID rootIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::GetAssemblyReferences(const WCHAR *wszAssemblyPath, ICorProfilerAssemblyReferenceProvider *pAsmRefProvider)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::ModuleInMemorySymbolsUpdated(ModuleID moduleId)
{
    return S_OK;
}

#ifdef CEE
HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionThrown(ObjectID thrownObjectId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionThrown);
	if (m_methodTable.EXCEPTIONTHROWN != NULL) return m_methodTable.EXCEPTIONTHROWN(PPRFCOM, thrownObjectId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionSearchFunctionEnter(FunctionID functionId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionSearchFunctionEnter);
	if (m_methodTable.EXCEPTIONSEARCHFUNCTIONENTER != NULL) return m_methodTable.EXCEPTIONSEARCHFUNCTIONENTER(PPRFCOM, functionId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionSearchFunctionLeave()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionSearchFunctionLeave);
	if (m_methodTable.EXCEPTIONSEARCHFUNCTIONLEAVE != NULL) return m_methodTable.EXCEPTIONSEARCHFUNCTIONLEAVE(PPRFCOM);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionSearchFilterEnter(FunctionID functionId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionSearchFilterEnter);
	if (m_methodTable.EXCEPTIONSEARCHFILTERENTER != NULL) return m_methodTable.EXCEPTIONSEARCHFILTERENTER(PPRFCOM, functionId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionSearchFilterLeave()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionSearchFilterLeave);
	if (m_methodTable.EXCEPTIONSEARCHFILTERLEAVE != NULL) return m_methodTable.EXCEPTIONSEARCHFILTERLEAVE(PPRFCOM);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionSearchCatcherFound(FunctionID functionId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionSearchCatcherFound);
	if (m_methodTable.EXCEPTIONSEARCHCATCHERFOUND != NULL) return m_methodTable.EXCEPTIONSEARCHCATCHERFOUND(PPRFCOM, functionId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionOSHandlerEnter(UINT_PTR __unused)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionOSHandlerEnter);
	if (m_methodTable.EXCEPTIONOSHANDLERENTER != NULL) return m_methodTable.EXCEPTIONOSHANDLERENTER(PPRFCOM, __unused);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionOSHandlerLeave(UINT_PTR __unused)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionOSHandlerLeave);
	if (m_methodTable.EXCEPTIONOSHANDLERLEAVE != NULL) return m_methodTable.EXCEPTIONOSHANDLERLEAVE(PPRFCOM, __unused);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionUnwindFunctionEnter(FunctionID functionId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionUnwindFunctionEnter);
	if (m_methodTable.EXCEPTIONUNWINDFUNCTIONENTER != NULL) return m_methodTable.EXCEPTIONUNWINDFUNCTIONENTER(PPRFCOM, functionId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionUnwindFunctionLeave()
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}


	InterlockedIncrement(&PPRFCOM->m_ExceptionUnwindFunctionLeave);
	if (m_methodTable.EXCEPTIONUNWINDFUNCTIONLEAVE != NULL) return m_methodTable.EXCEPTIONUNWINDFUNCTIONLEAVE(PPRFCOM);


	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionUnwindFinallyEnter(FunctionID functionId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionUnwindFinallyEnter);
	if (m_methodTable.EXCEPTIONUNWINDFINALLYENTER != NULL) return m_methodTable.EXCEPTIONUNWINDFINALLYENTER(PPRFCOM, functionId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionUnwindFinallyLeave()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionUnwindFinallyLeave);
	if (m_methodTable.EXCEPTIONUNWINDFINALLYLEAVE != NULL) return m_methodTable.EXCEPTIONUNWINDFINALLYLEAVE(PPRFCOM);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionCatcherEnter(FunctionID functionId,
	ObjectID objectId)
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionCatcherEnter);
	if (m_methodTable.EXCEPTIONCATCHERENTER != NULL) return m_methodTable.EXCEPTIONCATCHERENTER(PPRFCOM, functionId, objectId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionCatcherLeave()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionCatcherLeave);
	if (m_methodTable.EXCEPTIONCATCHERLEAVE != NULL) return m_methodTable.EXCEPTIONCATCHERLEAVE(PPRFCOM);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionCLRCatcherFound()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionCLRCatcherFound);
	if (m_methodTable.EXCEPTIONCLRCATCHERFOUND != NULL) return m_methodTable.EXCEPTIONCLRCATCHERFOUND(PPRFCOM);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE BaseProfilerDriver::ExceptionCLRCatcherExecute()
{

	InterlockedIncrement(&PPRFCOM->m_ExceptionCLRCatcherExecute);
	if (m_methodTable.EXCEPTIONCLRCATCHEREXECUTE != NULL) return m_methodTable.EXCEPTIONCLRCATCHEREXECUTE(PPRFCOM);
	return S_OK;
}

#endif//CEE

#ifdef ELT3

HRESULT CorProfiler::SetELTHooks()
{
	HRESULT hr = S_OK;

	// Set the FunctionIDMapper
	if (m_methodTable.FUNCTIONIDMAPPER != NULL)
	{
		MUST_PASS(PINFO->SetFunctionIDMapper(&FunctionIDMapperStub));
	}

	// Set the FunctionIDMapper2
	if (m_methodTable.FUNCTIONIDMAPPER2 != NULL)
	{
		// pass the pointer to the profiler instance for the second argument

		MUST_PASS(PINFO->SetFunctionIDMapper2(&FunctionIDMapper2Stub, m_methodTable.TEST_POINTER));
	}

	// Set Enter/Leave/Tailcall hooks if callbacks were requested
	if (m_methodTable.FUNCTIONENTER != NULL ||
		m_methodTable.FUNCTIONLEAVE != NULL ||
		m_methodTable.FUNCTIONTAILCALL != NULL)
	{
		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks(m_methodTable.FUNCTIONENTER == NULL ? NULL : &EnterStub,
			m_methodTable.FUNCTIONLEAVE == NULL ? NULL : &LeaveStub,
			m_methodTable.FUNCTIONTAILCALL == NULL ? NULL : &TailcallStub));
	}

	if (m_methodTable.FUNCTIONENTER2 != NULL ||
		m_methodTable.FUNCTIONLEAVE2 != NULL ||
		m_methodTable.FUNCTIONTAILCALL2 != NULL)
	{
		bool fFastEnter2 = FALSE;
		bool fFastLeave2 = FALSE;
		bool fFastTail2 = FALSE;

		FunctionEnter2 * pFE2 = NULL;
		FunctionLeave2 * pFL2 = NULL;
		FunctionTailcall2 * pFTC2 = NULL;

		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_FUNCTION_ARGS) ? fFastEnter2 = FALSE : fFastEnter2 = TRUE;
		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_FUNCTION_RETVAL) ? fFastLeave2 = FALSE : fFastLeave2 = TRUE;
		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO) ? fFastTail2 = FALSE : fFastTail2 = TRUE;

#if defined(_X86_)
		//TODO
		//pFE2 = reinterpret_cast<FunctionEnter2 *>(fFastEnter2 ? &Enter2Naked : &Enter2Stub);
		//pFL2 = reinterpret_cast<FunctionLeave2 *>(fFastLeave2 ? &Leave2Naked : &Leave2Stub);
		//pFTC2 = reinterpret_cast<FunctionTailcall2 *>(fFastTail2 ? &Tailcall2Naked : &Tailcall2Stub);

		pFE2 = reinterpret_cast<FunctionEnter2 *>(&Enter2Stub);
		pFL2 = reinterpret_cast<FunctionLeave2 *>(&Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(&Tailcall2Stub);


#elif defined(_AMD64_) || defined(_ARM_)

		pFE2 = reinterpret_cast<FunctionEnter2 *>(&Enter2Stub);
		//TODO
		//pFL2 = reinterpret_cast<FunctionLeave2 *>(fFastLeave2 ? &Leave2Naked : &Leave2Stub);
		//pFTC2 = reinterpret_cast<FunctionTailcall2 *>(fFastTail2 ? &Tailcall2Naked : &Tailcall2Stub);

		pFL2 = reinterpret_cast<FunctionLeave2 *>(&Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(&Tailcall2Stub);

#elif defined(_IA64_)

		pFE2 = reinterpret_cast<FunctionEnter2 *>(&Enter2Stub);
		pFL2 = reinterpret_cast<FunctionLeave2 *>(&Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(&Tailcall2Stub);

#endif

		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks2(m_methodTable.FUNCTIONENTER2 ? pFE2 : NULL,
			m_methodTable.FUNCTIONLEAVE2 ? pFL2 : NULL,
			m_methodTable.FUNCTIONTAILCALL2 ? pFTC2 : NULL));
	}
	if (m_methodTable.FUNCTIONENTER3 != NULL ||
		m_methodTable.FUNCTIONLEAVE3 != NULL ||
		m_methodTable.FUNCTIONTAILCALL3 != NULL)
	{
		// TODO
		//FunctionEnter3 * pFE3 = reinterpret_cast<FunctionEnter3 *>(&FunctionEnter3Naked);
		//FunctionLeave3 * pFL3 = reinterpret_cast<FunctionLeave3 *>(&FunctionLeave3Naked);
		//FunctionTailcall3 * pFTC3 = reinterpret_cast<FunctionTailcall3 *>(&FunctionTailCall3Naked);

		//MUST_PASS(PINFO->SetEnterLeaveFunctionHooks3(m_methodTable.FUNCTIONENTER3 ? pFE3 : NULL,
		//	m_methodTable.FUNCTIONLEAVE3 ? pFL3 : NULL,
		//	m_methodTable.FUNCTIONTAILCALL3 ? pFTC3 : NULL));
	}

	if (m_methodTable.FUNCTIONENTER3WITHINFO != NULL ||
		m_methodTable.FUNCTIONLEAVE3WITHINFO != NULL ||
		m_methodTable.FUNCTIONTAILCALL3WITHINFO != NULL)
	{
		FunctionEnter3WithInfo * pFEWI3 = reinterpret_cast<FunctionEnter3WithInfo *>(&FunctionEnter3WithInfoStub);
		FunctionLeave3WithInfo * pFLWI3 = reinterpret_cast<FunctionLeave3WithInfo *>(&FunctionLeave3WithInfoStub);
		FunctionTailcall3WithInfo * pFTCWI3 = reinterpret_cast<FunctionTailcall3WithInfo *>(&FunctionTailCall3WithInfoStub);
		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks3WithInfo(m_methodTable.FUNCTIONENTER3WITHINFO ? pFEWI3 : NULL,
			m_methodTable.FUNCTIONLEAVE3WITHINFO ? pFLWI3 : NULL,
			m_methodTable.FUNCTIONTAILCALL3WITHINFO ? pFTCWI3 : NULL));
	}

	return S_OK;
}


UINT_PTR CorProfiler::FunctionIDMapper(FunctionID functionId, BOOL *pbHookFunction)
{
	if (m_methodTable.FUNCTIONIDMAPPER != NULL) return m_methodTable.FUNCTIONIDMAPPER(functionId, pbHookFunction);
	return functionId;
}
UINT_PTR CorProfiler::FunctionIDMapper2(FunctionID functionId, void * clientData, BOOL *pbHookFunction)
{
	if (m_methodTable.FUNCTIONIDMAPPER2 != NULL) return m_methodTable.FUNCTIONIDMAPPER2(functionId, clientData, pbHookFunction);
	return functionId;
}


VOID CorProfiler::FunctionEnterCallBack(FunctionID funcID)

{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionEnter);
	if (m_methodTable.FUNCTIONENTER != NULL) m_methodTable.FUNCTIONENTER(PPRFCOM, funcID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionLeaveCallBack(FunctionID funcID)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionLeave);
	if (m_methodTable.FUNCTIONLEAVE != NULL) m_methodTable.FUNCTIONLEAVE(PPRFCOM, funcID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionTailcallCallBack(FunctionID funcID)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}
	// It should be updating m_FunctionTailcall and not m_FunctionTailcall2 since this is ELT1 callback and not ELT2 callback

	InterlockedIncrement(&PPRFCOM->m_FunctionTailcall);
	if (m_methodTable.FUNCTIONTAILCALL != NULL) m_methodTable.FUNCTIONTAILCALL(PPRFCOM, funcID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}


VOID CorProfiler::FunctionEnter2CallBack(FunctionID funcID,
	UINT_PTR   mappedFuncID,
	COR_PRF_FRAME_INFO frame,
	COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)

{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionEnter2);
	if (m_methodTable.FUNCTIONENTER2 != NULL) m_methodTable.FUNCTIONENTER2(PPRFCOM, funcID, mappedFuncID, frame, argumentInfo);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionLeave2CallBack(FunctionID funcID,
	UINT_PTR   mappedFuncID,
	COR_PRF_FRAME_INFO frame,
	COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionLeave2);
	if (m_methodTable.FUNCTIONLEAVE2 != NULL) m_methodTable.FUNCTIONLEAVE2(PPRFCOM, funcID, mappedFuncID, frame, retvalRange);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionTailcall2CallBack(FunctionID funcID,
	UINT_PTR   mappedFuncID,
	COR_PRF_FRAME_INFO frame)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionTailcall2);
	if (m_methodTable.FUNCTIONTAILCALL2 != NULL) m_methodTable.FUNCTIONTAILCALL2(PPRFCOM, funcID, mappedFuncID, frame);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionEnter3CallBack(FunctionIDOrClientID functionIDOrClientID)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionEnter3);
	if (m_methodTable.FUNCTIONENTER3 != NULL) m_methodTable.FUNCTIONENTER3(PPRFCOM, functionIDOrClientID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionLeave3CallBack(FunctionIDOrClientID functionIDOrClientID)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionLeave3);
	if (m_methodTable.FUNCTIONLEAVE3 != NULL) m_methodTable.FUNCTIONLEAVE3(PPRFCOM, functionIDOrClientID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionTailCall3CallBack(FunctionIDOrClientID functionIDOrClientID)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionTailCall3);
	if (m_methodTable.FUNCTIONTAILCALL3 != NULL) m_methodTable.FUNCTIONTAILCALL3(PPRFCOM, functionIDOrClientID);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionEnter3WithInfoCallBack(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionEnter3WithInfo);
	if (m_methodTable.FUNCTIONENTER3WITHINFO != NULL) m_methodTable.FUNCTIONENTER3WITHINFO(PPRFCOM, functionIDOrClientID, eltInfo);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionLeave3WithInfoCallBack(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionLeave3WithInfo);
	if (m_methodTable.FUNCTIONLEAVE3WITHINFO != NULL) m_methodTable.FUNCTIONLEAVE3WITHINFO(PPRFCOM, functionIDOrClientID, eltInfo);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

VOID CorProfiler::FunctionTailCall3WithInfoCallBack(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// If Sampling is enabled, first check to see if Signaled to wait for a Hard Suspend
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		//EnterCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
		//WaitForSingleObject(PPRFCOM->m_hAsynchEvent, INFINITE);
		InterlockedIncrement(&PPRFCOM->m_ActiveCallbacks);
		//LeaveCriticalSection(&PPRFCOM->m_asynchronousCriticalSection);
	}

	InterlockedIncrement(&PPRFCOM->m_FunctionTailCall3WithInfo);
	if (m_methodTable.FUNCTIONTAILCALL3WITHINFO != NULL) m_methodTable.FUNCTIONTAILCALL3WITHINFO(PPRFCOM, functionIDOrClientID, eltInfo);

	// Decrement callback count for Sampling
	if (PPRFCOM->m_fEnableAsynch == TRUE)
	{
		InterlockedDecrement(&PPRFCOM->m_ActiveCallbacks);
	}
}

#endif//ELT3


HRESULT CorProfiler::CommonInit()
{

	// NULL out a new MethodTable for this satellite.
	memset(&m_methodTable, '\0', sizeof(MODULEMETHODTABLE));

	/*
	// Grab a pointer to the init func for this DLL.  We'll use this later to init the MethodTables
	initFunc = reinterpret_cast<FC_INITIALIZE>(GetProcAddress(m_profilerSatellite, "Satellite_Initialize"));

	// Call DLL init func of the DLL module to initialize the MethodTable of the current satellite.
	if (initFunc != NULL)
	{
		initFunc(PPRFCOM, &m_methodTable, strTestName);
	}
	else
	{
		FAILURE(L"Initialize function failed.  Was a bad test name supplied? " << strTestName);
		return FALSE;
	}
	*/
#ifdef CEE
	wstring strTestName = L"ThreadNameChanged";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName); 
/*	wstring strTestName = L"Exceptions";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName); 
	wstring strTestName = L"Exceptions";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);
	wstring strTestName = L"ExceptionsASP";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);*/
#endif//CEE

#ifdef ELT3
	
	wstring strTestName = L"FastPathELT3";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName); 
		
		/*wstring strTestName = L"SlowPathELT2";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName); 
	wstring strTestName = L"SlowPathELT3";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);
	wstring strTestName = L"SlowPathELT3IncorrectFlags";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);
	wstring strTestName = L"FastPathELT3IncorrectFlags";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);
	wstring strTestName = L"SlowPathELT3IncorrectFlagsDSS";
	Satellite_Initialize(PPRFCOM, &m_methodTable, strTestName);*/
#endif//ELT3
	// Set EventMask flags returned from this satellite module.
	// TODO What does this even mean? Why are we setting it back to itself? Commenting it here.
	//m_methodTable.FLAGS = m_methodTable.FLAGS; 

	// If the satellite implements its own IPrfCom, free our copy and use theirs. They must have already 
	// called IPrfCom::FreeOutputFiles() or the test run will be hosed.
	if (m_methodTable.IPPRFCOM != NULL)
	{
		delete PPRFCOM;
		PPRFCOM = m_methodTable.IPPRFCOM;
	}

	if (m_methodTable.TEST_POINTER != NULL)
	{
		PPRFCOM->m_pTestClassInstance = m_methodTable.TEST_POINTER;
	}

	DISPLAY(L"\nProfilerEventMask: " << HEX(m_methodTable.FLAGS) << L"\n");
	HRESULT hr = PINFO->SetEventMask(m_methodTable.FLAGS | COR_PRF_MONITOR_APPDOMAIN_LOADS);

	if (S_OK != hr)
	{
		FAILURE(L"ICorProfilerInfo::SetEventMask() failed with 0x" << HEX(hr) << L"\n");
		return E_FAIL;
	}

	// Keep track of Initialize callbacks received
	InterlockedIncrement(&PPRFCOM->m_Startup);

	return S_OK;
}

#ifdef CEE
void ThreadNameChangedInit(IPrfCom * pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void ex_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable, BOOL testASP);

void Satellite_Initialize(IPrfCom * pPrfCom, PMODULEMETHODTABLE pModuleMethodTable, const wstring& testName)
{
	DISPLAY(L"Initialize CEE Module : " << testName);

	if (testName == L"ThreadNameChanged") ThreadNameChangedInit(pPrfCom, pModuleMethodTable);
	else if (testName == L"Exceptions")        ex_Initialize(pPrfCom, pModuleMethodTable, FALSE);
	else if (testName == L"ExceptionsASP")     ex_Initialize(pPrfCom, pModuleMethodTable, TRUE);
	else FAILURE(L"Test name not recognized!");

	pCallTable = pModuleMethodTable;
	g_satellite_pPrfCom = pPrfCom;

	return;
}

#endif//CEE

#ifdef ELT3
#pragma region Init_Func_Declaraions
void SlowPathELT2_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void FastPathELT3_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void SlowPathELT3_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void SlowPathELT3IncorrectFlags_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void FastPathELT3IncorrectFlags_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
void SlowPathELT3IncorrectFlagsDSS_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable);
#pragma endregion

void CorProfiler::Satellite_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable, const wstring& testName)
{
	// TODO
	//DISPLAY(L"ELT Test Module : " << testName);

	if (testName == L"SlowPathELT2")        							    SlowPathELT2_Initialize(pPrfCom, pModuleMethodTable);
	else if (testName == L"FastPathELT3")        							FastPathELT3_Initialize(pPrfCom, pModuleMethodTable);
	else if (testName == L"SlowPathELT3")        							SlowPathELT3_Initialize(pPrfCom, pModuleMethodTable);
	else if (testName == L"SlowPathELT3IncorrectFlags")						SlowPathELT3IncorrectFlags_Initialize(pPrfCom, pModuleMethodTable);
	else if (testName == L"FastPathELT3IncorrectFlags")						FastPathELT3IncorrectFlags_Initialize(pPrfCom, pModuleMethodTable);
	else if (testName == L"SlowPathELT3IncorrectFlagsDSS")					SlowPathELT3IncorrectFlagsDSS_Initialize(pPrfCom, pModuleMethodTable);
	else
		FAILURE(L"Test name not recognized!");
	//pCallTable = pModuleMethodTable;
	//g_satellite_pPrfCom = pPrfCom;

	return;
}

#endif//ELT3

HRESULT CorProfiler::VerificationRoutine()
{
	HRESULT hr = S_OK;

// Call the verification function
if (m_methodTable.VERIFY != NULL)
{
	if (m_methodTable.VERIFY(PPRFCOM) != S_OK)
	{
		DISPLAY(L"");
		DISPLAY(L"TEST FAILED");
		hr = E_FAIL;
	}
	else if (PPRFCOM->m_ulError > 0)
	{
		DISPLAY(L"");
		DISPLAY(L"TEST FAILED");
		DISPLAY(L"Search \"ERROR: Test Failure\" in the log file for details.");
		hr = E_FAIL;
	}
	else
	{
		DISPLAY(L"");
		DISPLAY(L"TEST PASSED");
	}
}

return hr;
}
