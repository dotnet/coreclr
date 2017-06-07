// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// ProfilerCallbackTypedefs.h
//
// Defines function pointer types and method table used to pass ICorProfilerCallback 
// implementations in the test module to BaseProfilerDriver.
// 
// ======================================================================================

#ifndef __PROFILER_CALLBACK_TYPEDEFS__
#define __PROFILER_CALLBACK_TYPEDEFS__

typedef HRESULT ( *FC_VERIFY)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_APPDOMAINCREATIONSTARTED)(IPrfCom * pPrfCom, 
                                        AppDomainID appDomainId);

typedef HRESULT (STDMETHODCALLTYPE *FC_APPDOMAINCREATIONFINISHED)(IPrfCom * pPrfCom,
                                        AppDomainID appDomainId,
                                        HRESULT hrStatus);    
    
typedef HRESULT (STDMETHODCALLTYPE *FC_APPDOMAINSHUTDOWNSTARTED)(IPrfCom * pPrfCom,
                                        AppDomainID appDomainId);     
    
typedef HRESULT (STDMETHODCALLTYPE *FC_APPDOMAINSHUTDOWNFINISHED)(IPrfCom * pPrfCom,
                                        AppDomainID appDomainId,
                                        HRESULT hrStatus);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_ASSEMBLYLOADSTARTED)(IPrfCom * pPrfCom,
                                        AssemblyID assemblyId);        
    
typedef HRESULT (STDMETHODCALLTYPE *FC_ASSEMBLYLOADFINISHED)(IPrfCom * pPrfCom,
                                        AssemblyID assemblyId,
                                        HRESULT hrStatus);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_ASSEMBLYUNLOADSTARTED)(IPrfCom * pPrfCom,
                                        AssemblyID assemblyId);       
    
typedef HRESULT (STDMETHODCALLTYPE *FC_ASSEMBLYUNLOADFINISHED)(IPrfCom * pPrfCom,
                                        AssemblyID assemblyId,
                                        HRESULT hrStatus);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_MODULELOADSTARTED)(IPrfCom * pPrfCom,
                                        ModuleID moduleId);      
    
typedef HRESULT (STDMETHODCALLTYPE *FC_MODULELOADFINISHED)(IPrfCom * pPrfCom,
                                        ModuleID moduleId,
                                        HRESULT hrStatus);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_MODULEUNLOADSTARTED)(IPrfCom * pPrfCom,
                                        ModuleID moduleId);        
        
typedef HRESULT (STDMETHODCALLTYPE *FC_MODULEUNLOADFINISHED)(IPrfCom * pPrfCom,
                                        ModuleID moduleId,
                                        HRESULT hrStatus);
       
typedef HRESULT (STDMETHODCALLTYPE *FC_MODULEATTACHEDTOASSEMBLY)(IPrfCom * pPrfCom,
                                        ModuleID moduleId,
                                        AssemblyID AssemblyId);

typedef HRESULT (STDMETHODCALLTYPE *FC_CLASSLOADSTARTED)(IPrfCom * pPrfCom,
                                        ClassID classId);

typedef HRESULT (STDMETHODCALLTYPE *FC_CLASSLOADFINISHED)(IPrfCom * pPrfCom,
                                        ClassID classId,
                                        HRESULT hrStatus);

typedef HRESULT (STDMETHODCALLTYPE *FC_CLASSUNLOADSTARTED)(IPrfCom * pPrfCom,
                                        ClassID classId);

typedef HRESULT (STDMETHODCALLTYPE *FC_CLASSUNLOADFINISHED)(IPrfCom * pPrfCom,
                                        ClassID classId,
                                        HRESULT hrStatus);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONUNLOADSTARTED)(IPrfCom * pPrfCom,
                                        FunctionID functionId);

typedef HRESULT (STDMETHODCALLTYPE *FC_JITCOMPILATIONSTARTED)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        BOOL fIsSafeToBlock);

typedef HRESULT (STDMETHODCALLTYPE *FC_JITCOMPILATIONFINISHED)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        HRESULT hrStatus,
                                        BOOL fIsSafeToBlock);
        
typedef HRESULT (STDMETHODCALLTYPE *FC_JITCACHEDFUNCTIONSEARCHSTARTED)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        BOOL *pbUseCachedFunction);

typedef HRESULT (STDMETHODCALLTYPE *FC_JITCACHEDFUNCTIONSEARCHFINISHED)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        COR_PRF_JIT_CACHE result);

typedef HRESULT (STDMETHODCALLTYPE *FC_JITFUNCTIONPITCHED)(IPrfCom * pPrfCom,
                                        FunctionID functionId);

typedef HRESULT (STDMETHODCALLTYPE *FC_JITINLINING)(IPrfCom * pPrfCom,
                                        FunctionID callerId,
                                        FunctionID calleeId,
                                        BOOL *pfShouldInline);

typedef HRESULT (STDMETHODCALLTYPE *FC_THREADCREATED)(IPrfCom * pPrfCom,
                                        ThreadID managedThreadId); 

typedef HRESULT (STDMETHODCALLTYPE *FC_THREADDESTROYED)(IPrfCom * pPrfCom,
                                        ThreadID threadID);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_THREADASSIGNEDTOOSTHREAD)(IPrfCom * pPrfCom,
                                        ThreadID managedThreadId,
                                        DWORD osThreadId);

typedef HRESULT (STDMETHODCALLTYPE *FC_THREADNAMECHANGED)(IPrfCom * pPrfCom,
                                        ThreadID managedThreadId,
                                        ULONG cchName,
                                        WCHAR name[]); 

typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGCLIENTINVOCATIONSTARTED)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGCLIENTSENDINGMESSAGE)(IPrfCom * pPrfCom,
                                        GUID *pCookie,
                                        BOOL fIsAsync);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGCLIENTRECEIVINGREPLY)(IPrfCom * pPrfCom,
                                        GUID *pCookie,
                                        BOOL fIsAsync);

typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGCLIENTINVOCATIONFINISHED)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGSERVERRECEIVINGMESSAGE)(IPrfCom * pPrfCom,
                                        GUID *pCookie,
                                        BOOL fIsAsync);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGSERVERINVOCATIONSTARTED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGSERVERINVOCATIONRETURNED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_REMOTINGSERVERSENDINGREPLY)(IPrfCom * pPrfCom,
                                        GUID *pCookie,
                                        BOOL fIsAsync);

typedef HRESULT (STDMETHODCALLTYPE *FC_UNMANAGEDTOMANAGEDTRANSITION)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        COR_PRF_TRANSITION_REASON reason);

typedef HRESULT (STDMETHODCALLTYPE *FC_MANAGEDTOUNMANAGEDTRANSITION)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        COR_PRF_TRANSITION_REASON reason);

typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMESUSPENDSTARTED)(IPrfCom * pPrfCom,
                                        COR_PRF_SUSPEND_REASON suspendReason);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMESUSPENDFINISHED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMESUSPENDABORTED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMERESUMESTARTED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMERESUMEFINISHED)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMETHREADSUSPENDED)(IPrfCom * pPrfCom,
                                                    ThreadID threadID);

typedef HRESULT (STDMETHODCALLTYPE *FC_RUNTIMETHREADRESUMED)(IPrfCom * pPrfCom,
                                        ThreadID threadID);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_MOVEDREFERENCES)(IPrfCom * pPrfCom,
                                        ULONG cMovedObjectIDRanges,
                                        ObjectID oldObjectIDRangeStart[  ],
                                        ObjectID newObjectIDRangeStart[  ],
                                        ULONG cObjectIDRangeLength[  ]);

typedef HRESULT (STDMETHODCALLTYPE *FC_MOVEDREFERENCES2)(IPrfCom * pPrfCom,
                                        ULONG cMovedObjectIDRanges,
                                        ObjectID oldObjectIDRangeStart[  ],
                                        ObjectID newObjectIDRangeStart[  ],
                                        SIZE_T cObjectIDRangeLength[  ]);

typedef HRESULT (STDMETHODCALLTYPE *FC_OBJECTALLOCATED)(IPrfCom * pPrfCom,
                                        ObjectID objectId,
                                        ClassID classId); 
        
typedef HRESULT (STDMETHODCALLTYPE *FC_OBJECTSALLOCATEDBYCLASS)(IPrfCom * pPrfCom,
                                        ULONG cClassCount,
                                        ClassID classIds[  ],
                                        ULONG cObjects[  ]);

typedef HRESULT (STDMETHODCALLTYPE *FC_OBJECTREFERENCES)(IPrfCom * pPrfCom,
                                        ObjectID objectId,
                                        ClassID classId,
                                        ULONG cObjectRefs,
                                        ObjectID objectRefIds[  ]);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_ROOTREFERENCES)(IPrfCom * pPrfCom,
                                        ULONG cRootRefs,
                                        ObjectID rootRefIds[  ]);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONTHROWN)(IPrfCom * pPrfCom,
                                        ObjectID thrownObjectId);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONSEARCHFUNCTIONENTER)(IPrfCom * pPrfCom,
                                        FunctionID functionId);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONSEARCHFUNCTIONLEAVE)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONSEARCHFILTERENTER)(IPrfCom * pPrfCom,
                                        FunctionID functionId);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONSEARCHFILTERLEAVE)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONSEARCHCATCHERFOUND)(IPrfCom * pPrfCom,
                                        FunctionID functionId);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONOSHANDLERENTER)(IPrfCom * pPrfCom,
                                        UINT_PTR __unused);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONOSHANDLERLEAVE)(IPrfCom * pPrfCom,
                                        UINT_PTR __unused);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONUNWINDFUNCTIONENTER)(IPrfCom * pPrfCom,
                                        FunctionID functionId);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONUNWINDFUNCTIONLEAVE)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONUNWINDFINALLYENTER)(IPrfCom * pPrfCom,
                                        FunctionID functionId);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONUNWINDFINALLYLEAVE)(IPrfCom * pPrfCom);
    
typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONCATCHERENTER)(IPrfCom * pPrfCom,
                                        FunctionID functionId,
                                        ObjectID objectId);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONCATCHERLEAVE)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONCLRCATCHERFOUND)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_EXCEPTIONCLRCATCHEREXECUTE)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_COMCLASSICVTABLECREATED)(IPrfCom * pPrfCom,
                                        ClassID wrappedClassID,
                                        REFGUID implementedIID,
                                        VOID *pVTable,
                                        ULONG cSlots);

typedef HRESULT (STDMETHODCALLTYPE *FC_COMCLASSICVTABLEDESTROYED)(IPrfCom * pPrfCom,
                                        ClassID wrappedClassID,
                                        REFGUID implementedIID,
                                        VOID *pVTable);

typedef HRESULT (STDMETHODCALLTYPE *FC_GARBAGECOLLECTIONSTARTED)(IPrfCom * pPrfCom,
                                        INT cGenerations,
                                        BOOL generationCollected[],
                                        COR_PRF_GC_REASON reason);

typedef HRESULT (STDMETHODCALLTYPE *FC_GARBAGECOLLECTIONFINISHED)(IPrfCom * pPrfCom );

typedef HRESULT (STDMETHODCALLTYPE *FC_FINALIZEABLEOBJECTQUEUED)(IPrfCom * pPrfCom,
                                        DWORD finalizerFlags,
                                        ObjectID objectID);

typedef HRESULT (STDMETHODCALLTYPE *FC_ROOTREFERENCES2)(IPrfCom * pPrfCom,
                                        ULONG    cRootRefs,
                                        ObjectID rootRefIds[],
                                        COR_PRF_GC_ROOT_KIND rootKinds[],
                                        COR_PRF_GC_ROOT_FLAGS rootFlags[],
                                        UINT_PTR rootIds[]);

typedef HRESULT (STDMETHODCALLTYPE *FC_HANDLECREATED)(IPrfCom * pPrfCom,
                                        GCHandleID handleId,
                                        ObjectID initialObjectId);

typedef HRESULT (STDMETHODCALLTYPE *FC_HANDLEDESTROYED)(IPrfCom * pPrfCom,
                                        GCHandleID handleId);

typedef HRESULT (STDMETHODCALLTYPE *FC_SURVIVINGREFERENCES)(IPrfCom * pPrfCom,
                                        ULONG    cSurvivingObjectIDRanges,
                                        ObjectID objectIDRangeStart[],
                                        ULONG    cObjectIDRangeLength[]);

typedef HRESULT (STDMETHODCALLTYPE *FC_SURVIVINGREFERENCES2)(IPrfCom * pPrfCom,
                                        ULONG    cSurvivingObjectIDRanges,
                                        ObjectID objectIDRangeStart[],
                                        SIZE_T   cObjectIDRangeLength[]);

typedef HRESULT (STDMETHODCALLTYPE *FC_PROFILERATTACHCOMPLETE)(IPrfCom * pPrfCom);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONENTER)(IPrfCom * pPrfCom,
                                        FunctionID funcId);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONTAILCALL)(IPrfCom * pPrfCom,
                                        FunctionID funcId);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONLEAVE)(IPrfCom * pPrfCom,
                                        FunctionID funcId);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONENTER2)(IPrfCom * pPrfCom,
                                        FunctionID funcId,
                                        UINT_PTR mappedFuncID,
                                        COR_PRF_FRAME_INFO frame,
                                        COR_PRF_FUNCTION_ARGUMENT_INFO * argumentInfo);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONTAILCALL2)(IPrfCom * pPrfCom,
                                        FunctionID funcId,
                                        UINT_PTR mappedFuncID,
                                        COR_PRF_FRAME_INFO frame);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONLEAVE2)(IPrfCom * pPrfCom,
                                        FunctionID funcId,
                                        UINT_PTR mappedFuncID,
                                        COR_PRF_FRAME_INFO frame,
                                        COR_PRF_FUNCTION_ARGUMENT_RANGE * retvalRange);

typedef UINT_PTR (STDMETHODCALLTYPE *FC_FUNCTIONIDMAPPER)(FunctionID funcId,
                                        BOOL *pbHookFunction);

// ELT3 Fast Path
typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONENTER3) (IPrfCom * pPrfCom, 
										FunctionIDOrClientID functionIDOrClientID);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONLEAVE3) (IPrfCom * pPrfCom,
										FunctionIDOrClientID functionIDOrClientID);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONTAILCALL3) (IPrfCom * pPrfCom,
										FunctionIDOrClientID functionIDOrClientID);

// ELT3 Slow Path
typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONENTER3WITHINFO) (IPrfCom * pPrfCom,
										FunctionIDOrClientID functionIDOrClientID,
										COR_PRF_ELT_INFO eltInfo);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONLEAVE3WITHINFO) (IPrfCom * pPrfCom,
										FunctionIDOrClientID functionIDOrClientID,
										COR_PRF_ELT_INFO eltInfo);

typedef HRESULT (STDMETHODCALLTYPE *FC_FUNCTIONTAILCALL3WITHINFO) (IPrfCom * pPrfCom,
										FunctionIDOrClientID functionIDOrClientID,
										COR_PRF_ELT_INFO eltInfo);

// ELT3 FunctionIDMapper2
typedef UINT_PTR (STDMETHODCALLTYPE *FC_FUNCTIONIDMAPPER2) (FunctionID funcId, 
										void *clientData,
										BOOL *pbHookFunction);

// ICorProfilerCallback4
typedef HRESULT (STDMETHODCALLTYPE *FC_REJITCOMPILATIONSTARTED)(
    IPrfCom * pPrfCom,
    FunctionID functionId,
    ReJITID rejitId,
    BOOL fIsSafeToBlock);
typedef HRESULT (STDMETHODCALLTYPE *FC_GETREJITPARAMETERS)(
    IPrfCom * pPrfCom,
    ModuleID moduleId,
    mdMethodDef methodId,
    ICorProfilerFunctionControl *pFunctionControl);
typedef HRESULT (STDMETHODCALLTYPE *FC_REJITCOMPILATIONFINISHED)(
    IPrfCom * pPrfCom,
    FunctionID functionId,
    ReJITID rejitId,
    HRESULT hrStatus,
    BOOL fIsSafeToBlock);
typedef HRESULT (STDMETHODCALLTYPE *FC_REJITERROR)(
    IPrfCom * pPrfCom,
    ModuleID moduleId,
    mdMethodDef methodId,
    FunctionID functionId,
    HRESULT hrStatus);


typedef struct _MODULEMETHODTABLE_ 
{
    FC_APPDOMAINCREATIONSTARTED APPDOMAINCREATIONSTARTED;
    FC_APPDOMAINCREATIONFINISHED APPDOMAINCREATIONFINISHED;
    FC_APPDOMAINSHUTDOWNSTARTED APPDOMAINSHUTDOWNSTARTED;
    FC_APPDOMAINSHUTDOWNFINISHED APPDOMAINSHUTDOWNFINISHED;
    FC_ASSEMBLYLOADSTARTED ASSEMBLYLOADSTARTED;
    FC_ASSEMBLYLOADFINISHED ASSEMBLYLOADFINISHED;
    FC_ASSEMBLYUNLOADSTARTED ASSEMBLYUNLOADSTARTED;
    FC_ASSEMBLYUNLOADFINISHED ASSEMBLYUNLOADFINISHED;
    FC_MODULELOADSTARTED MODULELOADSTARTED;
    FC_MODULELOADFINISHED MODULELOADFINISHED;
    FC_MODULEUNLOADSTARTED MODULEUNLOADSTARTED;
    FC_MODULEUNLOADFINISHED MODULEUNLOADFINISHED;
    FC_MODULEATTACHEDTOASSEMBLY MODULEATTACHEDTOASSEMBLY;
    FC_CLASSLOADSTARTED CLASSLOADSTARTED;
    FC_CLASSLOADFINISHED CLASSLOADFINISHED;
    FC_CLASSUNLOADSTARTED CLASSUNLOADSTARTED;
    FC_CLASSUNLOADFINISHED CLASSUNLOADFINISHED;
    FC_FUNCTIONUNLOADSTARTED FUNCTIONUNLOADSTARTED;
    FC_JITCOMPILATIONSTARTED JITCOMPILATIONSTARTED;
    FC_JITCOMPILATIONFINISHED JITCOMPILATIONFINISHED;
    FC_JITCACHEDFUNCTIONSEARCHSTARTED JITCACHEDFUNCTIONSEARCHSTARTED;
    FC_JITCACHEDFUNCTIONSEARCHFINISHED JITCACHEDFUNCTIONSEARCHFINISHED;
    FC_JITFUNCTIONPITCHED JITFUNCTIONPITCHED;
    FC_JITINLINING JITINLINING;
    FC_THREADCREATED THREADCREATED;
    FC_THREADDESTROYED THREADDESTROYED;
    FC_THREADASSIGNEDTOOSTHREAD THREADASSIGNEDTOOSTHREAD;
    FC_THREADNAMECHANGED THREADNAMECHANGED;
    FC_REMOTINGCLIENTINVOCATIONSTARTED REMOTINGCLIENTINVOCATIONSTARTED;
    FC_REMOTINGCLIENTSENDINGMESSAGE REMOTINGCLIENTSENDINGMESSAGE;
    FC_REMOTINGCLIENTRECEIVINGREPLY REMOTINGCLIENTRECEIVINGREPLY;
    FC_REMOTINGCLIENTINVOCATIONFINISHED REMOTINGCLIENTINVOCATIONFINISHED;
    FC_REMOTINGSERVERRECEIVINGMESSAGE REMOTINGSERVERRECEIVINGMESSAGE;
    FC_REMOTINGSERVERINVOCATIONSTARTED REMOTINGSERVERINVOCATIONSTARTED;
    FC_REMOTINGSERVERINVOCATIONRETURNED REMOTINGSERVERINVOCATIONRETURNED;
    FC_REMOTINGSERVERSENDINGREPLY REMOTINGSERVERSENDINGREPLY;
    FC_UNMANAGEDTOMANAGEDTRANSITION UNMANAGEDTOMANAGEDTRANSITION;
    FC_MANAGEDTOUNMANAGEDTRANSITION MANAGEDTOUNMANAGEDTRANSITION;
    FC_RUNTIMESUSPENDSTARTED RUNTIMESUSPENDSTARTED;
    FC_RUNTIMESUSPENDFINISHED RUNTIMESUSPENDFINISHED;
    FC_RUNTIMESUSPENDABORTED RUNTIMESUSPENDABORTED;
    FC_RUNTIMERESUMESTARTED RUNTIMERESUMESTARTED;
    FC_RUNTIMERESUMEFINISHED RUNTIMERESUMEFINISHED;
    FC_RUNTIMETHREADSUSPENDED RUNTIMETHREADSUSPENDED;
    FC_RUNTIMETHREADRESUMED RUNTIMETHREADRESUMED;
    FC_MOVEDREFERENCES MOVEDREFERENCES;
    FC_OBJECTALLOCATED OBJECTALLOCATED;
    FC_OBJECTSALLOCATEDBYCLASS OBJECTSALLOCATEDBYCLASS;
    FC_OBJECTREFERENCES OBJECTREFERENCES;
    FC_ROOTREFERENCES ROOTREFERENCES;
    FC_EXCEPTIONTHROWN EXCEPTIONTHROWN;
    FC_EXCEPTIONSEARCHFUNCTIONENTER EXCEPTIONSEARCHFUNCTIONENTER;
    FC_EXCEPTIONSEARCHFUNCTIONLEAVE EXCEPTIONSEARCHFUNCTIONLEAVE;
    FC_EXCEPTIONSEARCHFILTERENTER EXCEPTIONSEARCHFILTERENTER;
    FC_EXCEPTIONSEARCHFILTERLEAVE EXCEPTIONSEARCHFILTERLEAVE;
    FC_EXCEPTIONSEARCHCATCHERFOUND EXCEPTIONSEARCHCATCHERFOUND;
    FC_EXCEPTIONOSHANDLERENTER EXCEPTIONOSHANDLERENTER;
    FC_EXCEPTIONOSHANDLERLEAVE EXCEPTIONOSHANDLERLEAVE;
    FC_EXCEPTIONUNWINDFUNCTIONENTER EXCEPTIONUNWINDFUNCTIONENTER;
    FC_EXCEPTIONUNWINDFUNCTIONLEAVE EXCEPTIONUNWINDFUNCTIONLEAVE;
    FC_EXCEPTIONUNWINDFINALLYENTER EXCEPTIONUNWINDFINALLYENTER;
    FC_EXCEPTIONUNWINDFINALLYLEAVE EXCEPTIONUNWINDFINALLYLEAVE;
    FC_EXCEPTIONCATCHERENTER EXCEPTIONCATCHERENTER;
    FC_EXCEPTIONCATCHERLEAVE EXCEPTIONCATCHERLEAVE;
    FC_EXCEPTIONCLRCATCHERFOUND EXCEPTIONCLRCATCHERFOUND;
    FC_EXCEPTIONCLRCATCHEREXECUTE EXCEPTIONCLRCATCHEREXECUTE;
    FC_COMCLASSICVTABLECREATED COMCLASSICVTABLECREATED;
    FC_COMCLASSICVTABLEDESTROYED COMCLASSICVTABLEDESTROYED;
    FC_GARBAGECOLLECTIONSTARTED GARBAGECOLLECTIONSTARTED;
    FC_GARBAGECOLLECTIONFINISHED GARBAGECOLLECTIONFINISHED;
    FC_FINALIZEABLEOBJECTQUEUED FINALIZEABLEOBJECTQUEUED;
    FC_ROOTREFERENCES2 ROOTREFERENCES2;
    FC_HANDLECREATED HANDLECREATED;
    FC_HANDLEDESTROYED HANDLEDESTROYED;
    FC_SURVIVINGREFERENCES SURVIVINGREFERENCES;
    FC_PROFILERATTACHCOMPLETE PROFILERATTACHCOMPLETE;
    FC_FUNCTIONENTER FUNCTIONENTER;
    FC_FUNCTIONTAILCALL FUNCTIONTAILCALL;
    FC_FUNCTIONLEAVE FUNCTIONLEAVE;
    FC_FUNCTIONENTER2 FUNCTIONENTER2;
    FC_FUNCTIONTAILCALL2 FUNCTIONTAILCALL2;
    FC_FUNCTIONLEAVE2 FUNCTIONLEAVE2;
    FC_FUNCTIONIDMAPPER FUNCTIONIDMAPPER;
	// ELT3 FastPath
	FC_FUNCTIONENTER3 FUNCTIONENTER3;
	FC_FUNCTIONLEAVE3 FUNCTIONLEAVE3;
	FC_FUNCTIONTAILCALL3 FUNCTIONTAILCALL3;
	// ELT3 SlowPath
	FC_FUNCTIONENTER3WITHINFO FUNCTIONENTER3WITHINFO;
	FC_FUNCTIONLEAVE3WITHINFO FUNCTIONLEAVE3WITHINFO;
	FC_FUNCTIONTAILCALL3WITHINFO FUNCTIONTAILCALL3WITHINFO;
	// ELT3 FunctionIDMapper2
	FC_FUNCTIONIDMAPPER2 FUNCTIONIDMAPPER2;
    // ICorProfilerCallback4
    FC_REJITCOMPILATIONSTARTED REJITCOMPILATIONSTARTED;
    FC_GETREJITPARAMETERS GETREJITPARAMETERS;
    FC_REJITCOMPILATIONFINISHED REJITCOMPILATIONFINISHED;
    FC_REJITERROR REJITERROR;
    FC_MOVEDREFERENCES2 MOVEDREFERENCES2;
    FC_SURVIVINGREFERENCES2 SURVIVINGREFERENCES2;
    // Common base stuff
    FC_VERIFY VERIFY;
    DWORD FLAGS;
    PVOID TEST_POINTER;
    IPrfCom * IPPRFCOM;
} MODULEMETHODTABLE;

typedef MODULEMETHODTABLE *PMODULEMETHODTABLE;

// Initialization routine provided by each extension module 
typedef VOID (STDMETHODCALLTYPE *FC_INITIALIZE)(IPrfCom *, PMODULEMETHODTABLE, const wstring&);

#endif // __PROFILER_CALLBACK_TYPEDEFS__
