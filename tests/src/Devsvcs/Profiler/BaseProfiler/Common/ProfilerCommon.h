// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// ProfilerCommon.h
//
// Defines the interface and default implementation of the ProfilerCommon class.  This 
// class contains common routines used by all profiler tests such display and IT to name
// routines.
// 
// ======================================================================================

#ifndef __PROFILER_DRIVER_COMMON__
#define __PROFILER_DRIVER_COMMON__
#pragma region Includes


    #ifndef DECLSPEC_EXPORT
    #define DECLSPEC_EXPORT __declspec(dllexport)
    #endif//DECLSPEC_EXPORT

    #ifndef DECLSPEC_IMPORT
    #define DECLSPEC_IMPORT __declspec(dllimport)
    #endif//DECLSPEC_IMPORT

    #ifndef EXTERN_C
    #define EXTERN_C extern "C"
    #endif//EXTERN_C

    #ifndef NAKED 
    #define NAKED __declspec(naked)
    #endif//NAKED

#include "cor.h"
#include "corprof.h"
//#include <string.h>
#include <string>
#include <iostream>
    using namespace std;
        

#pragma endregion // Includes

#pragma region Utility Macros

    // Standard array and string lengths
    #define SHORT_LENGTH    32
    #define STRING_LENGTH  256
    #define LONG_LENGTH   1024

    // Macros for satellites to call ICorProfilerInfo & IPrfCom interfaces.
    #define PINFO pPrfCom->m_pInfo
    #define PPRFCOM pPrfCom

    // C string comparison made easy
    #define STRING_EQUAL(inputString, testString) _wcsicmp(inputString, testString)==0 ?TRUE:FALSE

    // Use in Test_Initialize(): REGISTER_CALLBACK(FUNCTIONENTER2, CTestClass::FuncEnter2Wrapper);
    #define REGISTER_CALLBACK(callback, function) pModuleMethodTable->callback = (FC_##callback) &function

    // Use in Test_Initialize(): SET_CLASS_POINTER(new CNewTest(pPrfCom));
    // Created new test class instance and stores it so it will be passed into each Callback function.
    #define SET_CLASS_POINTER( _initString_ ) pModuleMethodTable->TEST_POINTER = reinterpret_cast<VOID *>( _initString_ )

    // Use in CallbackFuncion(): LOCAL_CLASS_POINTER(CNewTest);
    // Creates variable named pCNewTest for use in that callback
    #define LOCAL_CLASS_POINTER( _testClass_ ) _testClass_ * p##_testClass_ = reinterpret_cast< _testClass_ * >( pPrfCom->m_pTestClassInstance )

    // Use in Test_Verify(): FREE_CLASS_POINTER(CNewTest);
    // Deletes variable named pCNewTest and sets IPrfCom class pointer to NULL.
    #define FREE_CLASS_POINTER( _testClass_ ) {     \
        delete p##_testClass_ ;                     \
        pPrfCom->m_pTestClassInstance = NULL ;      \
    }                                               \

    // Use in static callback wrappers: STATIC_CLASS_CALL(CNewTest)->NewTestProfilerCallback(pPrfCom, classId);
    // Makes writing wrappers easier and cleaner.
    #define STATIC_CLASS_CALL( _testClass_ ) reinterpret_cast< _testClass_ * >( pPrfCom->m_pTestClassInstance )

    // Use to inform BaseProfilerDriver of the derived IPrfCom to use for the rest of the test run.
    //
    //  Test_Initialize(IPrfCom * pPrfCom, PMODULEMETHODTABLE pModuleMethodTable, const wstring& testName)
    //  {
    //      pPrfCom->FreeOutputFiles(TRUE);
    //      INewCommon * pINewCommon = new INewCommon(pPrfCom->m_fProfilingASPorService);
    //      SET_DERIVED_POINTER(pINewCommon);
    //      ...
    //
    #define SET_DERIVED_POINTER( _newPrfCom_ ) pModuleMethodTable->IPPRFCOM = dynamic_cast<IPrfCom *>( _newPrfCom_ );
    
    // Use in CallbackFuncions(): DERIVED_COMMON_POINTER(INewCom);
    // When a derived class is used instead of PrfCom, this will create local pINewCom pointer for use in the function.
    //#define DERIVED_COMMON_POINTER( _newComm_ ) _newComm_ * p##_newComm_ = dynamic_cast< _newComm_ *>(pPrfCom);

    // MUST_PASS will assert if the call returns failing HR.
    #define MUST_PASS(call) {                                                                           \
                                HRESULT __hr = (call);                                                  \
                                if( FAILED(__hr) )                                                      \
                                {                                                                       \
                                    FAILURE(L"\nCall '" << #call << L"' failed with HR=" << HEX(__hr)   \
                                            << L"\nIn " << __FILE__ << L" at line " << __LINE__);       \
                                }                                                                       \
                            } 

    // MUST_RETURN_VALUE will assert if the call does not return value
    #define MUST_RETURN_VALUE(call, value) {                                                            \
                                HRESULT __hr = static_cast<HRESULT>(call);                              \
                                if( __hr != static_cast<HRESULT>(value) )                               \
                                {                                                                       \
                                    FAILURE(L"\nCall '" << #call << L"' returned " << HEX(__hr)           \
                                            << L"Instead of expected " << HEX(value)                    \
                                            << L"\nIn " << __FILE__ << L" at line " << __LINE__);       \
                                }                                                                       \
                            } 

    // MUST_NOT_RETURN_VALUE will assert if the call returns value
    #define MUST_NOT_RETURN_VALUE(call, value) {                                                        \
                                HRESULT __hr = static_cast<HRESULT>(call);                              \
                                if( __hr == static_cast<HRESULT>(value) )                               \
                                {                                                                       \
                                    FAILURE(L"\nCall '" << #call << L"' returned " << HEX(__hr)           \
                                            << L"\nIn " << __FILE__ << L" at line " << __LINE__);       \
                                }                                                                       \
                            } 
	#define WCHAR_STR(name) wstring name;                                            \
                            name.reserve(STRING_LENGTH);

    // Print macros used throughout framework
    #define DEC std::dec
    // #define HEX(__num) L"0x" << std::hex << std::uppercase << __num << std::dec

	#define HEX
    #define FLT std::fixed

    // Prints to screen and log file if either profiling ASP or debug logging is enabled
    // Assert helps locate where these macros are used with a null m_pPrfCom pointer. 
    #define DISPLAY( message )  {                                                                       \
                                    _ASSERTE(pPrfCom != NULL);                                          \
                                    cout << message;                             \
                                }

    // Displays failure dialog.
    #define FAILURE( message )  {                                                                       \
                                    _ASSERTE(pPrfCom != NULL);                                          \
                                    cout << message;                               \
                                }    // Prints to log file when logging is enabled. (set PRF_DBG=1)

#pragma endregion // Utility Macros

// Environment variables which are used by BaseProfilerDriver for test initialization
#define ENVVAR_BPD_SATELLITEMODULE  L"bpd_satellitemodule"
#define ENVVAR_BPD_TESTNAME         L"bpd_testname"
#define ENVVAR_BPD_OUTPUTFILE       L"bpd_outputfile"
//#define ENVVAR_BPD_ENABLEASYNC      L"bpd_enableasync"
#define ENVVAR_BPD_ATTACHMODE       L"bpd_attachmode"
#define ENVVAR_BPD_USEDSSWITHEBP    L"bpd_usedsswithebp"
	

class DECLSPEC_NOVTABLE /*DECLSPEC_EXPORT*/ IPrfCom
{
    public:

        // Common ICorProfilerInfo3 pointer
        ICorProfilerInfo3 *m_pInfo;

        // Common ICorProfilerInfo4 pointer
        // I'm using a separate variable to avoid updating existing testcases
        ICorProfilerInfo4 *m_pInfo4;

        // Critical Sections 
        CRITICAL_SECTION m_criticalSection;             // Public for satellites to use.  Do not use in PrfCommon.
        CRITICAL_SECTION m_asynchronousCriticalSection; // For synchronization in callbacks.
        
        // Output critical sections to protect output string streams
        CRITICAL_SECTION m_printingDisplayCriticalSection;
        CRITICAL_SECTION m_printingErrorCriticalSection;

        // Test class instance pointer.
        PVOID m_pTestClassInstance;

        // Running in startup mode or attach mode?
        BOOL m_bAttachMode;

        // old DSS implementation or new EBP-based one?
        BOOL m_bUseDSSWithEBP;

        // Event and Methods for synchronizing Sampling Hard Suspends DO NOT USE! Only used by
        // DoStackSnapshot hijacking tests.
        HANDLE m_hAsynchEvent;
        BOOL   m_fEnableAsynch;

		virtual ~IPrfCom()
		{
			delete m_pInfo;
			delete m_pInfo4;
			m_pTestClassInstance = nullptr;

		}

        virtual HRESULT InitializeForAsynch() = 0;
        virtual HRESULT RequestHardSuspend()  = 0;
        virtual HRESULT ReleaseHardSuspend()  = 0;

        // Output functions
        virtual VOID FreeOutputFiles(BOOL delFile)  = 0; // Only used when deriving a new PrfCom.

        // Common utility routines
        virtual HRESULT GetAppDomainIDName(AppDomainID appdomainId, wstring &name, const BOOL full = FALSE) = 0;
        virtual HRESULT GetAssemblyIDName(AssemblyID assemblyId, wstring &name, const BOOL full = FALSE) = 0;
        virtual HRESULT GetModuleIDName(ModuleID funcId, wstring &name, const BOOL full = FALSE) = 0;
        virtual HRESULT GetClassIDName(ClassID classId, wstring &name, const BOOL full = FALSE) = 0;
        virtual HRESULT GetFunctionIDName(FunctionID funcId, wstring &name, const COR_PRF_FRAME_INFO frameInfo = 0, const BOOL full = FALSE) = 0;
        virtual BOOL IsFunctionStatic(FunctionID funcId, const COR_PRF_FRAME_INFO frameInfo = 0) = 0;

        // Unregister for Profiler Callbacks
        virtual HRESULT RemoveEvent(DWORD event) = 0;

#pragma region Callback Counters
        
        // startup - shutdown
        LONG m_Startup;
        LONG m_Shutdown;
        LONG m_ProfilerAttachComplete;
        LONG m_ProfilerDetachSucceeded;

        // threads
        LONG m_ThreadCreated;
        LONG m_ThreadDestroyed;
        LONG m_ThreadAssignedToOSThread;
        LONG m_ThreadNameChanged;

        // appdomains
        LONG m_AppDomainCreationStarted;
        LONG m_AppDomainCreationFinished;
        LONG m_AppDomainShutdownStarted;
        LONG m_AppDomainShutdownFinished;

        // assemblies
        LONG m_AssemblyLoadStarted;
        LONG m_AssemblyLoadFinished;
        LONG m_AssemblyUnloadStarted;
        LONG m_AssemblyUnloadFinished;

        // modules
        LONG m_ModuleLoadStarted;
        LONG m_ModuleLoadFinished;
        LONG m_ModuleUnloadStarted;
        LONG m_ModuleUnloadFinished;
        LONG m_ModuleAttachedToAssembly;

        // classes
        LONG m_ClassLoadStarted;
        LONG m_ClassLoadFinished;
        LONG m_ClassUnloadStarted;
        LONG m_ClassUnloadFinished;
        LONG m_FunctionUnloadStarted;

        // JIT
        LONG m_JITCompilationStarted;
        LONG m_JITCompilationFinished;
        LONG m_JITCachedFunctionSearchStarted;
        LONG m_JITCachedFunctionSearchFinished;
        LONG m_JITFunctionPitched;
        LONG m_JITInlining;

        // exceptions
        LONG m_ExceptionThrown;

        LONG m_ExceptionSearchFunctionEnter;
        LONG m_ExceptionSearchFunctionLeave;
        LONG m_ExceptionSearchFilterEnter;
        LONG m_ExceptionSearchFilterLeave;

        LONG m_ExceptionSearchCatcherFound;
        LONG m_ExceptionCLRCatcherFound;
        LONG m_ExceptionCLRCatcherExecute;

        LONG m_ExceptionOSHandlerEnter;
        LONG m_ExceptionOSHandlerLeave;

        LONG m_ExceptionUnwindFunctionEnter;
        LONG m_ExceptionUnwindFunctionLeave;
        LONG m_ExceptionUnwindFinallyEnter;
        LONG m_ExceptionUnwindFinallyLeave;
        LONG m_ExceptionCatcherEnter;
        LONG m_ExceptionCatcherLeave;

         // transitions
        LONG m_ManagedToUnmanagedTransition;
        LONG m_UnmanagedToManagedTransition;

        // ccw
        LONG m_COMClassicVTableCreated;
        LONG m_COMClassicVTableDestroyed;

            // suspend
        LONG m_RuntimeSuspendStarted;
        LONG m_RuntimeSuspendFinished;
        LONG m_RuntimeSuspendAborted;
        LONG m_RuntimeResumeStarted;
        LONG m_RuntimeResumeFinished;
        LONG m_RuntimeThreadSuspended;
        LONG m_RuntimeThreadResumed;

        // gc
        LONG m_MovedReferences;
        LONG m_MovedReferences2;
        LONG m_ObjectAllocated;
        LONG m_ObjectReferences;
        LONG m_RootReferences;
        LONG m_ObjectsAllocatedByClass;

        // remoting
        LONG m_RemotingClientInvocationStarted;
        LONG m_RemotingClientInvocationFinished;
        LONG m_RemotingClientSendingMessage;
        LONG m_RemotingClientReceivingReply;

        LONG m_RemotingServerInvocationStarted;
        LONG m_RemotingServerInvocationReturned;
        LONG m_RemotingServerSendingReply;
        LONG m_RemotingServerReceivingMessage;

        // suspension counter array
        //DWORD m_dwSuspensionCounters[(DWORD)COR_PRF_SUSPEND_FOR_GC_PREP+1];
        LONG  m_ForceGCEventCounter;
        DWORD m_dwForceGCSucceeded;

        // enter-leave counters
        LONG m_FunctionEnter;
        LONG m_FunctionLeave;
        LONG m_FunctionTailcall;

        LONG m_FunctionEnter2;
        LONG m_FunctionLeave2;
        LONG m_FunctionTailcall2;

		LONG m_FunctionEnter3WithInfo;		// counter for m_FunctionEnter3WithInfo (slowpath)
        LONG m_FunctionLeave3WithInfo;		// counter for m_FunctionLeave3WithInfo (slowpath)
        LONG m_FunctionTailCall3WithInfo;	// counter for m_FunctionTailcall3WithInfo (slowpath)

		LONG m_FunctionEnter3;				// counter for m_FunctionEnter3 (fastpath)
        LONG m_FunctionLeave3;				// counter for m_FunctionLeave3 (fastpath)
        LONG m_FunctionTailCall3;			// counter for m_FunctionTailcall3 (fastpath)

        LONG m_FunctionIDMapper;			// TODO we dont track this/use this. Do we need it?
        LONG m_FunctionIDMapper2;			// TODO do we need a counter for this. See m_FunctionMapper for relevance.

        LONG m_GarbageCollectionStarted;
        LONG m_GarbageCollectionFinished;
        LONG m_FinalizeableObjectQueued;
        LONG m_RootReferences2;
        LONG m_HandleCreated;
        LONG m_HandleDestroyed;
        LONG m_SurvivingReferences;
        LONG m_SurvivingReferences2;

        // Keep track of active callbacks to prepare for Hard Suspend
        LONG m_ActiveCallbacks;

#pragma endregion // Callback Counters
		ULONG m_ulError;
};


class /*DECLSPEC_EXPORT*/ PrfCommon: public IPrfCom
{
    public:

        PrfCommon(ICorProfilerInfo3 *pInfo3);
        ~PrfCommon();

        virtual HRESULT InitializeForAsynch();
        virtual HRESULT RequestHardSuspend();
        virtual HRESULT ReleaseHardSuspend();

        virtual VOID FreeOutputFiles(BOOL delFile);

        virtual HRESULT GetAppDomainIDName(AppDomainID appdomainId, wstring &name, const BOOL full = FALSE);
        virtual HRESULT GetAssemblyIDName(AssemblyID assemblyId, wstring &name, const BOOL full = FALSE);
        virtual HRESULT GetModuleIDName(ModuleID funcId, wstring &name, BOOL const full = FALSE);
        virtual HRESULT GetClassIDName(ClassID classId, wstring &name, BOOL const full = FALSE);
        virtual HRESULT GetFunctionIDName(FunctionID funcId, wstring &name, const COR_PRF_FRAME_INFO frameInfo = 0, const BOOL full = FALSE);

        virtual BOOL IsFunctionStatic(FunctionID funcId, const COR_PRF_FRAME_INFO frameInfo = 0);

        virtual HRESULT RemoveEvent(DWORD event);
private:
        // log file
        WCHAR     m_wszOutputFile[LONG_LENGTH];
        // HANDLE    m_OutputFileHandle;
};

#include "ProfilerCallbackTypedefs.h"

#endif //__PROFILER_DRIVER_COMMON__

