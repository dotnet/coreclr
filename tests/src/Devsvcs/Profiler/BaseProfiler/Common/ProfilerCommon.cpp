// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
//
// ProfilerCommon.cpp
//
// Implements PrfCommon, the instantiation of the IPrfCom interface.
//
// ======================================================================================

#include "ProfilerCommon.h"
#include <sstream>
#ifndef WIN32
//#include <codecvt>
#endif // !WIN32

// Modified print macros for use in ProfilerCommon.cpp
#undef DISPLAY
#undef LOG
#undef FAILURE
#undef VERBOSE

#define DISPLAY( message )  {                                                            \
                                cout << message;                           \
                            }

#define FAILURE( message )  {                                                            \
                                cout << message;                             \
                            }

// Modified ICorProfilerInfo macro for use in ProfilerCommon
#undef  PINFO
#define PINFO m_pInfo


PrfCommon::PrfCommon(ICorProfilerInfo3 *pInfo3)
{
    DWORD dwRet = 0;
    wchar_t wszEnableAsynch[MAX_PATH] = {0};
    
	// TODO
    // dwRet = GetEnvironmentVariable(ENVVAR_BPD_ENABLEASYNC, wszEnableAsynch, MAX_PATH); 
    // if (0 != dwRet)
    // {
    //     m_fEnableAsynch = true;
    // }    


#pragma region Critical Sections
/*
    // Public critical section supplied by ProfilerCommon for satellite modules to use
    InitializeCriticalSectionAndSpinCount(&m_criticalSection, 0x1000);

    // Critical sections used by ProfilerCommon only
    InitializeCriticalSectionAndSpinCount(&m_asynchronousCriticalSection, 0x1000);

    // Critical sections used to protect the output string streams These critical sections are expected to
    // have high contention, so initialize with a spin count. This allows threads to loop on the critical
    // section before they sleep.
    InitializeCriticalSectionAndSpinCount(&m_printingDisplayCriticalSection, 0x1000);
    InitializeCriticalSectionAndSpinCount(&m_printingErrorCriticalSection,   0x1000);
*/
#pragma endregion // Critical Sections

    m_pTestClassInstance = NULL;
    
    // Initialize ICorProfilerInfo* interfaces
    m_pInfo = pInfo3;
    m_pInfo->AddRef();
    HRESULT hr = pInfo3->QueryInterface(__uuidof(ICorProfilerInfo3), (void **)&m_pInfo4);
    if (FAILED(hr))
    {
        FAILURE(L"QueryInterface(__uuidof(ICorProfilerInfo4) failed with hr=0x" << std::hex << hr);  
    }

	// TODO
    /*if (m_fEnableAsynch)
    {
        // Set up Event used to communicate with Asynchronous satellite when loaded.
        m_hAsynchEvent = CreateEvent(NULL,   // default security attributes
                                     TRUE,   // manual-reset event
                                     TRUE,   // initial state is signaled
                                     NULL);  // object name

        if (m_hAsynchEvent == NULL)
        {
            FAILURE(L"CreateEvent for asynchronous event failed\n");
        }
    }
    else
    {*/
        m_hAsynchEvent = NULL;
    //}

    wchar_t wszOutputFile[MAX_PATH] = {0};

    //dwRet = GetEnvironmentVariable(ENVVAR_BPD_OUTPUTFILE, wszOutputFile, MAX_PATH); 
    if (0 != dwRet)
    {
			//DeleteFile(wszOutputFile);
		    //m_OutputFileHandle = CreateFile(wszOutputFile,                // name of the write
            //           GENERIC_WRITE,          // open for writing
            //           0,                      // do not share
            //           NULL,                   // default security
            //           CREATE_ALWAYS,          // overwrite existing
            //           FILE_ATTRIBUTE_NORMAL,  // normal file
            //           NULL);                  // no attr. template
    }
    else
    {
			//DeleteFile(wszOutputFile);
			//m_OutputFileHandle = CreateFile(L"BPDoutput.log",                // name of the write
            //   GENERIC_WRITE,          // open for writing
            //   0,                      // do not share
            //   NULL,                   // default security
            //   CREATE_ALWAYS,          // overwrite existing
            //   FILE_ATTRIBUTE_NORMAL,  // normal file
            //   NULL);                  // no attr. template
    }

    // Open output file stream.
	//if (m_OutputFileHandle == INVALID_HANDLE_VALUE) 
	//{
    //    FAILURE(L"Cannot open output file for logging");
    //}

    // Are we running in startup mode or attach mode?    
    //dwRet = GetEnvironmentVariable(ENVVAR_BPD_ATTACHMODE, wszOutputFile, MAX_PATH); 
    m_bAttachMode = (0 != dwRet) ? TRUE : FALSE;
    
    wchar_t wszUseDssWithEBP[MAX_PATH] = {0};
    
    // Are we supposed to use regular DSS or the new EBP-based implementation?    
    //dwRet = GetEnvironmentVariable(ENVVAR_BPD_USEDSSWITHEBP, wszUseDssWithEBP, MAX_PATH); 
    m_bUseDSSWithEBP = (0 != dwRet) ? TRUE : FALSE;


#pragma region Callback Counter Initialization
    m_Startup = 0;
    m_Shutdown = 0;

    //attach
    m_ProfilerAttachComplete = 0;
    m_ProfilerDetachSucceeded = 0;

    // threads
    m_ThreadCreated = 0;
    m_ThreadDestroyed = 0;
    m_ThreadAssignedToOSThread = 0;
    m_ThreadNameChanged = 0;

    // appdomains
    m_AppDomainCreationStarted = 0;
    m_AppDomainCreationFinished = 0;
    m_AppDomainShutdownStarted = 0;
    m_AppDomainShutdownFinished = 0;

    // assemblies
    m_AssemblyLoadStarted = 0;
    m_AssemblyLoadFinished = 0;
    m_AssemblyUnloadStarted = 0;
    m_AssemblyUnloadFinished = 0;

    // modules
    m_ModuleLoadStarted = 0;
    m_ModuleLoadFinished = 0;
    m_ModuleUnloadStarted = 0;
    m_ModuleUnloadFinished = 0;
    m_ModuleAttachedToAssembly = 0;

    // classes
    m_ClassLoadStarted = 0;
    m_ClassLoadFinished = 0;
    m_ClassUnloadStarted = 0;
    m_ClassUnloadFinished = 0;
    m_FunctionUnloadStarted = 0;

    // JIT
    m_JITCompilationStarted = 0;
    m_JITCompilationFinished = 0;
    m_JITCachedFunctionSearchStarted = 0;
    m_JITCachedFunctionSearchFinished = 0;
    m_JITFunctionPitched = 0;
    m_JITInlining = 0;

    // exceptions
    m_ExceptionThrown = 0;
    m_ExceptionSearchFunctionEnter = 0;
    m_ExceptionSearchFunctionLeave = 0;
    m_ExceptionSearchFilterEnter = 0;
    m_ExceptionSearchFilterLeave = 0;

    m_ExceptionSearchCatcherFound = 0;
    m_ExceptionCLRCatcherFound = 0;
    m_ExceptionCLRCatcherExecute = 0;

    m_ExceptionOSHandlerEnter = 0;
    m_ExceptionOSHandlerLeave = 0;

    m_ExceptionUnwindFunctionEnter = 0;
    m_ExceptionUnwindFunctionLeave = 0;
    m_ExceptionUnwindFinallyEnter = 0;
    m_ExceptionUnwindFinallyLeave = 0;
    m_ExceptionCatcherEnter = 0;
    m_ExceptionCatcherLeave = 0;

     // transitions
    m_ManagedToUnmanagedTransition = 0;
    m_UnmanagedToManagedTransition = 0;

    // ccw
    m_COMClassicVTableCreated = 0;
    m_COMClassicVTableDestroyed = 0;

        // suspend
    m_RuntimeSuspendStarted = 0;
    m_RuntimeSuspendFinished = 0;
    m_RuntimeSuspendAborted = 0;
    m_RuntimeResumeStarted = 0;
    m_RuntimeResumeFinished = 0;
    m_RuntimeThreadSuspended = 0;
    m_RuntimeThreadResumed = 0;

    // gc
    m_MovedReferences = 0;
    m_ObjectAllocated = 0;
    m_ObjectReferences = 0;
    m_RootReferences = 0;
    m_ObjectsAllocatedByClass = 0;

    // remoting
    m_RemotingClientInvocationStarted = 0;
    m_RemotingClientInvocationFinished = 0;
    m_RemotingClientSendingMessage = 0;
    m_RemotingClientReceivingReply = 0;

    m_RemotingServerInvocationStarted = 0;
    m_RemotingServerInvocationReturned = 0;
    m_RemotingServerSendingReply = 0;
    m_RemotingServerReceivingMessage = 0;

    // suspension counter array
    m_ForceGCEventCounter = 0;
    m_dwForceGCSucceeded = 0;

    // enter-leave counters
    m_FunctionEnter = 0;
    m_FunctionLeave = 0;
    m_FunctionTailcall = 0;

    m_FunctionEnter2 = 0;
    m_FunctionLeave2 = 0;
    m_FunctionTailcall2 = 0;

    m_FunctionIDMapper = 0;

	// ELT3 FastPath Counters
	m_FunctionEnter3 = 0;
	m_FunctionLeave3 = 0;
	m_FunctionTailCall3 = 0;

	// ELT3 SlowPath Counters
	m_FunctionEnter3WithInfo = 0;
	m_FunctionLeave3WithInfo = 0;
	m_FunctionTailCall3WithInfo = 0;

	// FunctionIDMapper2 counter
	m_FunctionIDMapper2 = 0;

    m_GarbageCollectionStarted = 0;
    m_GarbageCollectionFinished = 0;
    m_FinalizeableObjectQueued = 0;
    m_RootReferences2 = 0;
    m_HandleCreated = 0;
    m_HandleDestroyed = 0;
    m_SurvivingReferences = 0;

    // Keep track of active callbacks to prepare for Hard Suspend
    m_ActiveCallbacks = 0;

#pragma endregion // Callback Counter Initialization

	m_ulError = 0;
}


VOID PrfCommon::FreeOutputFiles(BOOL delFile)
{
    // CloseHandle(m_OutputFileHandle);

    if (delFile)
    {
        //DeleteFile(m_wszOutputFile);
    }
}


PrfCommon::~PrfCommon()
{
    if (m_pInfo)
    {
        m_pInfo->Release();
        m_pInfo = NULL;
    }

    if (m_pInfo4)
    {
        m_pInfo4->Release();
        m_pInfo4 = NULL;
    }

    //DeleteCriticalSection(&m_criticalSection);
    //DeleteCriticalSection(&m_asynchronousCriticalSection);
    //DeleteCriticalSection(&m_printingDisplayCriticalSection);
    //DeleteCriticalSection(&m_printingErrorCriticalSection);

    //if (m_hAsynchEvent != NULL)
        //CloseHandle(m_hAsynchEvent);

    FreeOutputFiles(FALSE);
}

// Called by Asynchronous satellite to setup BaseProfilerDriver for Asynchronous Profiling
HRESULT PrfCommon::InitializeForAsynch()
{
   m_fEnableAsynch = TRUE;
   return S_OK;
}


// Called by Asynchronous satellite to signal request for Hard Suspend.  Function returns when
// BaseProfilerDriver is ready for a Hard Suspend.
 HRESULT PrfCommon::RequestHardSuspend()
{
    //EnterCriticalSection(&m_asynchronousCriticalSection);

    // ResetEvent to nonsignaled state.  Callbacks wait on this event.  Signaled allows execution.
    //if(!ResetEvent(m_hAsynchEvent))
        //FAILURE(L"ResetEvent for PrfCom Asynch Event failed " << GetLastError());

    // Wait for threads that started execution before signal to return
	while (InterlockedExchangeAdd(&m_ActiveCallbacks, 0) != 0)
		// Sleep(0);
	{
	}

    //LeaveCriticalSection(&m_asynchronousCriticalSection);

    return S_OK;
}


// Called by Asynchronous satellite to signal HardSuspend is over.
HRESULT PrfCommon::ReleaseHardSuspend()
{
    // Set HardSuspend event so waiting and new BaseProfilerDriver threads can continue
    // if(!SetEvent(m_hAsynchEvent))
        // FAILURE(L"SetEvent for PrfCom Asynch Event failed " << GetLastError());

    return S_OK;
}


HRESULT PrfCommon::RemoveEvent(DWORD eventM)
{
    HRESULT hr = S_OK;
    DWORD eventMask;

    hr = PINFO->GetEventMask(&eventMask);
    if (FAILED(hr))
        return hr;

    eventMask = ~((~eventMask) ^ eventM);
    hr = PINFO->SetEventMask(eventMask | COR_PRF_MONITOR_APPDOMAIN_LOADS);

    return hr;
}

HRESULT PrfCommon::GetAppDomainIDName(AppDomainID appdomainId, wstring &name, const BOOL full)
{
    WCHAR appDomainName[STRING_LENGTH];
    ULONG nameLength = 0;
    ProcessID procId = 0;

    if (appdomainId == NULL)
    {
        return E_FAIL;
    }

    MUST_PASS(PINFO->GetAppDomainInfo(appdomainId,
                                      STRING_LENGTH,
                                      &nameLength,
                                      appDomainName,
                                      &procId));
    if (full)
    {
		std::wstringstream strstream;
		strstream << (ULONG)procId;
		name += L"0x";
		strstream >> name;
		name += L":";
    }
	//    TODO
	//name += std::wstring(appDomainName);
    return S_OK;
}

HRESULT PrfCommon::GetAssemblyIDName(AssemblyID assemblyId, wstring &name, const BOOL full)
{
    WCHAR assemblyName[STRING_LENGTH];
    AppDomainID appID = 0;
    ULONG nameLength = 0;

    if (assemblyId == NULL)
    {
        return E_FAIL;
    }

    MUST_PASS(PINFO->GetAssemblyInfo(assemblyId,
                                     STRING_LENGTH,
                                     &nameLength,
                                     assemblyName,
                                     &appID,
                                     NULL));
    if (full)
    {
        MUST_PASS(GetAppDomainIDName(appID, name, full));
        name += L"-";
    }

#ifndef WIN32
	/*wstring_convert<codecvt_utf16<wchar_t, 0x10ffff, little_endian>,
		wchar_t> conv;
	wstring ws = conv.from_bytes(
		reinterpret_cast<const char*> (&assemblyName[0]),
		reinterpret_cast<const char*> (&assemblyName[0] + STRING_LENGTH));
	name += ws;*/
#else
	name += assemblyName;
#endif //!WIN32

    return S_OK;
}


HRESULT PrfCommon::GetModuleIDName(ModuleID modId, wstring &name, BOOL const full)
{
    WCHAR moduleName[STRING_LENGTH];
    ULONG nameLength = 0;
    AssemblyID assemID;

    if (modId == NULL)
    {
        return E_FAIL;
    }

    MUST_PASS(PINFO->GetModuleInfo(modId,
                                   NULL,
                                   STRING_LENGTH,
                                   &nameLength,
                                   moduleName,
                                   &assemID));

    if (full)
    {
        MUST_PASS(GetAssemblyIDName(assemID, name, full));
        name += L"!";
    }

    // TODO name += moduleName;
    return S_OK;
}


HRESULT PrfCommon::GetClassIDName(ClassID classId, wstring &name, const BOOL full)
{
    ModuleID modId;
    mdTypeDef classToken;
    ClassID parentClassID;
    ULONG32 nTypeArgs;
    ClassID typeArgs[SHORT_LENGTH];
    HRESULT hr = S_OK;

    if (classId == NULL)
    {
        return E_FAIL;
    }

    hr = PINFO->GetClassIDInfo2(classId,
                                &modId,
                                &classToken,
                                &parentClassID,
                                SHORT_LENGTH,
                                &nTypeArgs,
                                typeArgs);
    if (CORPROF_E_CLASSID_IS_ARRAY == hr)
    {
        // We have a ClassID of an array.
        name += L"ArrayClass";
        return S_OK;
    }
    else if (CORPROF_E_CLASSID_IS_COMPOSITE == hr)
    {
        // We have a composite class
        name += L"CompositeClass";
        return S_OK;
    }
    else if (CORPROF_E_DATAINCOMPLETE == hr)
    {
        // type-loading is not yet complete. Cannot do anything about it.
        name += L"DataIncomplete";
        return S_OK;
    }
    else if (FAILED(hr))
    {
        FAILURE(L"GetClassIDInfo returned " << HEX(hr) << L" for ClassID " << HEX(classId));
        return hr;
    }

	IMetaDataImport * pMDImport = 0;

    MUST_PASS(PINFO->GetModuleMetaData(modId,
                                       (ofRead | ofWrite),
                                       IID_IMetaDataImport,
                                       (IUnknown **)&pMDImport ));


    WCHAR wName[LONG_LENGTH];
    DWORD dwTypeDefFlags = 0;
    MUST_PASS(pMDImport->GetTypeDefProps(classToken,
                                         wName,
                                         LONG_LENGTH,
                                         NULL,
                                         &dwTypeDefFlags,
                                         NULL));

    if (full)
    {
        MUST_PASS(GetModuleIDName(modId, name, full));
        name += L".";
    }

    //TODO name += std::wstring(wName);
    if (nTypeArgs > 0)
        name += L"<";

    for(ULONG32 i = 0; i < nTypeArgs; i++)
    {

        wstring typeArgClassName;
        typeArgClassName.clear();
        MUST_PASS(GetClassIDName(typeArgs[i], typeArgClassName, FALSE));
        name += typeArgClassName;

        if ((i + 1) != nTypeArgs)
            name += L", ";
    }

    if (nTypeArgs > 0)
        name += L">";

    return hr;
}


HRESULT PrfCommon::GetFunctionIDName(FunctionID funcId, wstring &name, const COR_PRF_FRAME_INFO frameInfo, const BOOL full)
{
    // If the FunctionID is 0, we could be dealing with a native function.
    if (funcId == NULL)
    {
        name += L"Unknown_Native_Function";
        return S_OK;
    }

    ClassID classId = NULL;
    ModuleID moduleId = NULL;
    mdToken token = NULL;
    ULONG32 nTypeArgs = NULL;
    ClassID typeArgs[SHORT_LENGTH];

    MUST_PASS(PINFO->GetFunctionInfo2(funcId,
                                      frameInfo,
                                      &classId,
                                      &moduleId,
                                      &token,
                                      SHORT_LENGTH,
                                      &nTypeArgs,
                                      typeArgs));

	IMetaDataImport * pIMDImport = 0;

    MUST_PASS(PINFO->GetModuleMetaData(moduleId,
                                       ofRead,
                                       IID_IMetaDataImport,
                                       (IUnknown **)&pIMDImport));

    WCHAR funcName[STRING_LENGTH];
    MUST_PASS(pIMDImport->GetMethodProps(token,
                                         NULL,
                                         funcName,
                                         STRING_LENGTH,
                                         0,
                                         0,
                                         NULL,
                                         NULL,
                                         NULL,
                                         NULL));

    if (full)
    {
        wstring className;

        // If the ClassID returned from GetFunctionInfo is 0, then the function is a shared generic function.
        if (classId != 0)
        {
            MUST_PASS(GetClassIDName(classId, className, full));
        }
        else
        {
            className = L"SharedGenericFunction";
        }
		//TODO
//        name += std::wstring(className);
        name += L"::";
    }

    // TODO
		//name += funcName;

    // Fill in the type parameters of the generic method
    if (nTypeArgs > 0)
        name += L"<";

    for(ULONG32 i = 0; i < nTypeArgs; i++)
    {
        wstring typeArgClassName;
        typeArgClassName.clear();
        MUST_PASS(GetClassIDName(typeArgs[i], typeArgClassName, FALSE));
        name += typeArgClassName;

        if ((i + 1) != nTypeArgs)
            name += L", ";
    }

    if (nTypeArgs > 0)
        name += L">";

    return S_OK;
}


BOOL PrfCommon::IsFunctionStatic(FunctionID funcId, const COR_PRF_FRAME_INFO frameInfo)
{
    if (funcId == NULL)
    {
        FAILURE(L"NULL FunctionID passed to IsFunctionStatic!\n");
        return FALSE;
    }

    ModuleID moduleId = NULL;
    mdToken token = NULL;
    MUST_PASS(PINFO->GetFunctionInfo2(funcId,
                                      frameInfo,
                                      NULL,
                                      &moduleId,
                                      &token,
                                      NULL,
                                      NULL,
                                      NULL));

	IMetaDataImport * pIMDImport = 0;
	MUST_PASS(PINFO->GetModuleMetaData(moduleId,
                                       ofRead,
                                       IID_IMetaDataImport,
                                       (IUnknown **)&pIMDImport));

    DWORD methodAttr = NULL;
    MUST_PASS(pIMDImport->GetMethodProps(token,
                                         NULL,
                                         NULL,
                                         NULL,
                                         0,
                                         &methodAttr,
                                         NULL,
                                         NULL,
                                         NULL,
                                         NULL));

    if (IsMdStatic(methodAttr))
        return TRUE;
    else
        return FALSE;
}
