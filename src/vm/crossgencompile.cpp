// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================
// File: crosscomp.cpp
//

// ===========================================================================
// This file contains stubbed out implementations for cross-platform NGen.
//
// The stubbed out implementations are concentrated in this file to reduce number
// of ifdefs that has to be sprinkled through the code.
// ===========================================================================

#include "common.h"

#include "comdelegate.h"
#include "compile.h"
#include "invokeutil.h"
#include "comcallablewrapper.h"

//---------------------------------------------------------------------------------------
//
// Pull in some implementation files from other places in the tree
//

#include "../../dlls/mscoree/mscoree.cpp"

//---------------------------------------------------------------------------------------
//
// Helper function for features unsupported under crossgen
//

#undef ExitProcess

void CrossGenNotSupported(const char * message)
{
    _ASSERTE(!"CrossGenNotSupported");
    fprintf(stderr, "Fatal error: %s\n", message);
    ExitProcess(CORSECATTR_E_BAD_ACTION);
}

//---------------------------------------------------------------------------------------
//
// There is always only one thread and one appdomain in crossgen.
//

extern CompilationDomain * theDomain;

AppDomain * GetAppDomain()
{
    return theDomain;
}

Thread theThread;

Thread * GetThread()
{
    return (Thread*)&theThread;
}

Thread * GetThreadNULLOk()
{
    return GetThread();
}

#ifdef _DEBUG
BOOL Debug_IsLockedViaThreadSuspension()
{
    LIMITED_METHOD_CONTRACT;
    return FALSE;
}
#endif // _DEBUG

//---------------------------------------------------------------------------------------
//
// All locks are nops because of there is always only one thread.
//

void CrstBase::InitWorker(INDEBUG_COMMA(CrstType crstType) CrstFlags flags)
{
    m_dwFlags = flags;
}

void CrstBase::Destroy()
{
}

void CrstBase::Enter(INDEBUG(enum CrstBase::NoLevelCheckFlag))
{
}

void CrstBase::Leave()
{
}

BOOL __SwitchToThread(DWORD, DWORD)
{
    return TRUE;
}

//---------------------------------------------------------------------------------------
//
// Globals and misc other
//

GPTR_IMPL(IGCHeap,g_pGCHeap);

BOOL g_fEEOtherStartup=FALSE;
BOOL g_fEEComActivatedStartup=FALSE;

GVAL_IMPL_INIT(DWORD, g_fHostConfig, 0);

GVAL_IMPL_INIT(GCHeapType, g_heap_type, GC_HEAP_WKS);

void UpdateGCSettingFromHost()
{
}

HRESULT GetExceptionHResult(OBJECTREF throwable)
{
    return E_FAIL;
}

//---------------------------------------------------------------------------------------
//
// Dynamically unreachable implementation of profiler callbacks. Note that we can't just 
// disable PROFILING_SUPPORTED for crossgen because of it affects data layout and FCall tables.
//

UINT_PTR EEToProfInterfaceImpl::EEFunctionIDMapper(FunctionID funcId, BOOL * pbHookFunction)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::JITInlining(
    /* [in] */  FunctionID    callerId,
    /* [in] */  FunctionID    calleeId,
    /* [out] */ BOOL *        pfShouldInline)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ModuleLoadStarted(ModuleID moduleId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ModuleLoadFinished(
    ModuleID    moduleId,
    HRESULT        hrStatus)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ModuleUnloadStarted(
    ModuleID    moduleId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ModuleUnloadFinished(
    ModuleID    moduleId,
    HRESULT        hrStatus)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ModuleAttachedToAssembly(
    ModuleID    moduleId,
    AssemblyID  AssemblyId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ClassLoadStarted(
    ClassID     classId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::ClassLoadFinished(
    ClassID     classId,
    HRESULT     hrStatus)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AppDomainCreationFinished(
    AppDomainID appDomainId,
    HRESULT     hrStatus)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AppDomainCreationStarted(
    AppDomainID appDomainId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AppDomainShutdownFinished(
    AppDomainID appDomainId,
    HRESULT     hrStatus)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AppDomainShutdownStarted(
    AppDomainID appDomainId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AssemblyLoadStarted(
    AssemblyID  assemblyId)
{
    UNREACHABLE();
}

HRESULT EEToProfInterfaceImpl::AssemblyLoadFinished(
    AssemblyID  assemblyId,
    HRESULT     hrStatus)
{
    UNREACHABLE();
}

ClassID TypeHandleToClassID(TypeHandle th)
{
    UNREACHABLE();
}

//---------------------------------------------------------------------------------------
//
// Stubed-out implementations of functions that can do anything useful only when we are actually running managed code
//

MethodTable *Object::GetTrueMethodTable()
{
    UNREACHABLE();
}

FuncPtrStubs::FuncPtrStubs()
    : m_hashTableCrst(CrstFuncPtrStubs, CRST_UNSAFE_ANYMODE)
{
}

PCODE MethodDesc::GetMultiCallableAddrOfCode(CORINFO_ACCESS_FLAGS accessFlags)
{
    return 0x321;
}

PCODE MethodDesc::TryGetMultiCallableAddrOfCode(CORINFO_ACCESS_FLAGS accessFlags)
{
    return 0x321;
}

#ifdef _TARGET_X86_
BOOL Runtime_Test_For_SSE2()
{
    return TRUE;
}
#endif

#ifdef _TARGET_AMD64_
INT32 rel32UsingJumpStub(INT32 UNALIGNED * pRel32, PCODE target, MethodDesc *pMethod,
    LoaderAllocator *pLoaderAllocator /* = NULL */, bool throwOnOutOfMemoryWithinRange /*= true*/)
{
    // crossgen does not have jump stubs
    return 0;
}

INT32 rel32UsingPreallocatedJumpStub(INT32 UNALIGNED * pRel32, PCODE target, PCODE jumpStubAddr, bool emitJump)
{
    // crossgen does not have jump stubs
    return 0;
}
#endif


CORINFO_GENERIC_HANDLE JIT_GenericHandleWorker(MethodDesc *  pMD, MethodTable * pMT, LPVOID signature, DWORD dictionaryIndexAndSlot, Module* pModule)
{
    UNREACHABLE();
}

void CrawlFrame::GetExactGenericInstantiations(Instantiation *pClassInst, Instantiation *pMethodInst)
{
    UNREACHABLE();
}

OBJECTREF AppDomain::GetExposedObject()
{
    UNREACHABLE();
}

BOOL Object::SupportsInterface(OBJECTREF pObj, MethodTable* pInterfaceMT)
{
    UNREACHABLE();
}

GCFrame::GCFrame(OBJECTREF *pObjRefs, UINT numObjRefs, BOOL maybeInterior)
{
}

void GCFrame::GcScanRoots(promote_func *fn, ScanContext* sc)
{
    UNREACHABLE();
}

void HijackFrame::GcScanRoots(promote_func *fn, ScanContext* sc)
{
    UNREACHABLE();
}

VOID GCFrame::Pop()
{
}

void Frame::Push()
{
}

void Frame::Pop()
{
}

PCODE COMDelegate::GetSecureInvoke(MethodDesc* pMD)
{
    return (PCODE)(0x12345);
}

Assembly * SystemDomain::GetCallersAssembly(StackCrawlMark * stackMark, AppDomain ** ppAppDomain)
{
    return NULL;
}

void EnableStressHeapHelper()
{ 
    UNREACHABLE();
}

void ReflectionModule::CaptureModuleMetaDataToMemory()
{
}

//---------------------------------------------------------------------------------------
//
// Empty implementations of shutdown-related functions. We don't do any cleanup for shutdown during crossgen.
//

Assembly::~Assembly()
{
}

void Assembly::StartUnload()
{
}

void Module::StartUnload()
{
}

void DynamicMethodTable::Destroy()
{
}

void SyncClean::AddEEHashTable(EEHashEntry** entry)
{ 
}

void SyncClean::AddHashMap(Bucket *bucket)
{ 
}

#ifdef FEATURE_COMINTEROP
LONG ComCallWrapperTemplate::Release()
{
    UNREACHABLE();
}
#endif

extern "C" UINT_PTR STDCALL GetCurrentIP()
{ 
    return 0;
}

void EEPolicy::HandleFatalError(UINT exitCode, UINT_PTR address, LPCWSTR pszMessage, PEXCEPTION_POINTERS pExceptionInfo, LPCWSTR errorSource, const WCHAR * argExceptionString)
{ 
    fprintf(stderr, "Fatal error: %08x\n", exitCode);
    ExitProcess(exitCode);
}

//---------------------------------------------------------------------------------------

Assembly * AppDomain::RaiseAssemblyResolveEvent(AssemblySpec * pSpec, BOOL fIntrospection, BOOL fPreBind)
{
    return NULL;
}

Assembly * AppDomain::RaiseResourceResolveEvent(DomainAssembly* pAssembly, LPCSTR szName)
{
    return NULL;
}

DomainAssembly * AppDomain::RaiseTypeResolveEventThrowing(DomainAssembly* pAssembly, LPCSTR szName, ASSEMBLYREF *pResultingAssemblyRef)
{
    return NULL;
}

void AppDomain::RaiseLoadingAssemblyEvent(DomainAssembly *pAssembly)
{
}

BOOL AppDomain::BindingByManifestFile()
{
    return FALSE;
}
