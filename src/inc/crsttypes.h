//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

#ifndef __CRST_TYPES_INCLUDED
#define __CRST_TYPES_INCLUDED

// **** THIS IS AN AUTOMATICALLY GENERATED HEADER FILE -- DO NOT EDIT!!! ****

// This file describes the range of Crst types available and their mapping to a numeric level (used by the
// runtime in debug mode to validate we're deadlock free). To modify these settings edit the
// file:CrstTypes.def file and run the clr\bin\CrstTypeTool utility to generate a new version of this file.

// Each Crst type is declared as a value in the following CrstType enum.
enum CrstType
{
    CrstAllowedFiles = 0,
    CrstAppDomainCache = 1,
    CrstAppDomainHandleTable = 2,
    CrstArgBasedStubCache = 3,
    CrstAssemblyDependencyGraph = 4,
    CrstAssemblyIdentityCache = 5,
    CrstAssemblyList = 6,
    CrstAssemblyLoader = 7,
    CrstAvailableClass = 8,
    CrstAvailableParamTypes = 9,
    CrstBaseDomain = 10,
    CrstCCompRC = 11,
    CrstCer = 12,
    CrstClassFactInfoHash = 13,
    CrstClassInit = 14,
    CrstClrNotification = 15,
    CrstCLRPrivBinderMaps = 16,
    CrstCLRPrivBinderMapsAdd = 17,
    CrstCodeFragmentHeap = 18,
    CrstCOMCallWrapper = 19,
    CrstCOMWrapperCache = 20,
    CrstConnectionNameTable = 21,
    CrstContexts = 22,
    CrstCoreCLRBinderLog = 23,
    CrstCrstCLRPrivBinderLocalWinMDPath = 24,
    CrstCSPCache = 25,
    CrstDataTest1 = 26,
    CrstDataTest2 = 27,
    CrstDbgTransport = 28,
    CrstDeadlockDetection = 29,
    CrstDebuggerController = 30,
    CrstDebuggerFavorLock = 31,
    CrstDebuggerHeapExecMemLock = 32,
    CrstDebuggerHeapLock = 33,
    CrstDebuggerJitInfo = 34,
    CrstDebuggerMutex = 35,
    CrstDelegateToFPtrHash = 36,
    CrstDomainLocalBlock = 37,
    CrstDynamicIL = 38,
    CrstDynamicMT = 39,
    CrstDynLinkZapItems = 40,
    CrstEtwTypeLogHash = 41,
    CrstEventPipe = 42,
    CrstEventStore = 43,
    CrstException = 44,
    CrstExecuteManLock = 45,
    CrstExecuteManRangeLock = 46,
    CrstFCall = 47,
    CrstFriendAccessCache = 48,
    CrstFuncPtrStubs = 49,
    CrstFusionAppCtx = 50,
    CrstGCCover = 51,
    CrstGCMemoryPressure = 52,
    CrstGlobalStrLiteralMap = 53,
    CrstHandleTable = 54,
    CrstHostAssemblyMap = 55,
    CrstHostAssemblyMapAdd = 56,
    CrstIbcProfile = 57,
    CrstIJWFixupData = 58,
    CrstIJWHash = 59,
    CrstILStubGen = 60,
    CrstInlineTrackingMap = 61,
    CrstInstMethodHashTable = 62,
    CrstInterfaceVTableMap = 63,
    CrstInterop = 64,
    CrstInteropData = 65,
    CrstIOThreadpoolWorker = 66,
    CrstIsJMCMethod = 67,
    CrstISymUnmanagedReader = 68,
    CrstJit = 69,
    CrstJitGenericHandleCache = 70,
    CrstJitPerf = 71,
    CrstJumpStubCache = 72,
    CrstLeafLock = 73,
    CrstListLock = 74,
    CrstLoaderAllocator = 75,
    CrstLoaderAllocatorReferences = 76,
    CrstLoaderHeap = 77,
    CrstMda = 78,
    CrstMetadataTracker = 79,
    CrstMethodDescBackpatchInfoTracker = 80,
    CrstModIntPairList = 81,
    CrstModule = 82,
    CrstModuleFixup = 83,
    CrstModuleLookupTable = 84,
    CrstMulticoreJitHash = 85,
    CrstMulticoreJitManager = 86,
    CrstMUThunkHash = 87,
    CrstNativeBinderInit = 88,
    CrstNativeImageCache = 89,
    CrstNls = 90,
    CrstNotifyGdb = 91,
    CrstObjectList = 92,
    CrstOnEventManager = 93,
    CrstPatchEntryPoint = 94,
    CrstPEImage = 95,
    CrstPEImagePDBStream = 96,
    CrstPendingTypeLoadEntry = 97,
    CrstPinHandle = 98,
    CrstPinnedByrefValidation = 99,
    CrstProfilerGCRefDataFreeList = 100,
    CrstProfilingAPIStatus = 101,
    CrstPublisherCertificate = 102,
    CrstRCWCache = 103,
    CrstRCWCleanupList = 104,
    CrstRCWRefCache = 105,
    CrstReadyToRunEntryPointToMethodDescMap = 106,
    CrstReDacl = 107,
    CrstReflection = 108,
    CrstReJITDomainTable = 109,
    CrstReJITGlobalRequest = 110,
    CrstRemoting = 111,
    CrstRetThunkCache = 112,
    CrstRWLock = 113,
    CrstSavedExceptionInfo = 114,
    CrstSaveModuleProfileData = 115,
    CrstSecurityStackwalkCache = 116,
    CrstSharedAssemblyCreate = 117,
    CrstSigConvert = 118,
    CrstSingleUseLock = 119,
    CrstSpecialStatics = 120,
    CrstSqmManager = 121,
    CrstStackSampler = 122,
    CrstStressLog = 123,
    CrstStrongName = 124,
    CrstStubCache = 125,
    CrstStubDispatchCache = 126,
    CrstStubUnwindInfoHeapSegments = 127,
    CrstSyncBlockCache = 128,
    CrstSyncHashLock = 129,
    CrstSystemBaseDomain = 130,
    CrstSystemDomain = 131,
    CrstSystemDomainDelayedUnloadList = 132,
    CrstThreadIdDispenser = 133,
    CrstThreadpoolEventCache = 134,
    CrstThreadpoolTimerQueue = 135,
    CrstThreadpoolWaitThreads = 136,
    CrstThreadpoolWorker = 137,
    CrstThreadStaticDataHashTable = 138,
    CrstThreadStore = 139,
    CrstTieredCompilation = 140,
    CrstTPMethodTable = 141,
    CrstTypeEquivalenceMap = 142,
    CrstTypeIDMap = 143,
    CrstUMEntryThunkCache = 144,
    CrstUMThunkHash = 145,
    CrstUniqueStack = 146,
    CrstUnresolvedClassLock = 147,
    CrstUnwindInfoTableLock = 148,
    CrstVSDIndirectionCellLock = 149,
    CrstWinRTFactoryCache = 150,
    CrstWrapperTemplate = 151,
    kNumberOfCrstTypes = 152
};

#endif // __CRST_TYPES_INCLUDED

// Define some debug data in one module only -- vm\crst.cpp.
#if defined(__IN_CRST_CPP) && defined(_DEBUG)

// An array mapping CrstType to level.
int g_rgCrstLevelMap[] =
{
    10,         // CrstAllowedFiles
    10,         // CrstAppDomainCache
    14,         // CrstAppDomainHandleTable
    1,          // CrstArgBasedStubCache
    1,          // CrstAssemblyDependencyGraph
    1,          // CrstAssemblyIdentityCache
    1,          // CrstAssemblyList
    8,          // CrstAssemblyLoader
    4,          // CrstAvailableClass
    4,          // CrstAvailableParamTypes
    8,          // CrstBaseDomain
    -1,         // CrstCCompRC
    10,         // CrstCer
    13,         // CrstClassFactInfoHash
    9,          // CrstClassInit
    -1,         // CrstClrNotification
    1,          // CrstCLRPrivBinderMaps
    4,          // CrstCLRPrivBinderMapsAdd
    7,          // CrstCodeFragmentHeap
    1,          // CrstCOMCallWrapper
    5,          // CrstCOMWrapperCache
    1,          // CrstConnectionNameTable
    17,         // CrstContexts
    -1,         // CrstCoreCLRBinderLog
    1,          // CrstCrstCLRPrivBinderLocalWinMDPath
    8,          // CrstCSPCache
    4,          // CrstDataTest1
    1,          // CrstDataTest2
    1,          // CrstDbgTransport
    1,          // CrstDeadlockDetection
    -1,         // CrstDebuggerController
    4,          // CrstDebuggerFavorLock
    1,          // CrstDebuggerHeapExecMemLock
    1,          // CrstDebuggerHeapLock
    5,          // CrstDebuggerJitInfo
    11,         // CrstDebuggerMutex
    1,          // CrstDelegateToFPtrHash
    16,         // CrstDomainLocalBlock
    1,          // CrstDynamicIL
    4,          // CrstDynamicMT
    4,          // CrstDynLinkZapItems
    8,          // CrstEtwTypeLogHash
    19,         // CrstEventPipe
    1,          // CrstEventStore
    1,          // CrstException
    8,          // CrstExecuteManLock
    1,          // CrstExecuteManRangeLock
    4,          // CrstFCall
    8,          // CrstFriendAccessCache
    8,          // CrstFuncPtrStubs
    6,          // CrstFusionAppCtx
    11,         // CrstGCCover
    1,          // CrstGCMemoryPressure
    13,         // CrstGlobalStrLiteralMap
    2,          // CrstHandleTable
    1,          // CrstHostAssemblyMap
    4,          // CrstHostAssemblyMapAdd
    1,          // CrstIbcProfile
    10,         // CrstIJWFixupData
    1,          // CrstIJWHash
    8,          // CrstILStubGen
    4,          // CrstInlineTrackingMap
    17,         // CrstInstMethodHashTable
    1,          // CrstInterfaceVTableMap
    19,         // CrstInterop
    5,          // CrstInteropData
    13,         // CrstIOThreadpoolWorker
    1,          // CrstIsJMCMethod
    8,          // CrstISymUnmanagedReader
    9,          // CrstJit
    1,          // CrstJitGenericHandleCache
    -1,         // CrstJitPerf
    7,          // CrstJumpStubCache
    0,          // CrstLeafLock
    -1,         // CrstListLock
    15,         // CrstLoaderAllocator
    16,         // CrstLoaderAllocatorReferences
    1,          // CrstLoaderHeap
    1,          // CrstMda
    -1,         // CrstMetadataTracker
    14,         // CrstMethodDescBackpatchInfoTracker
    1,          // CrstModIntPairList
    5,          // CrstModule
    15,         // CrstModuleFixup
    4,          // CrstModuleLookupTable
    1,          // CrstMulticoreJitHash
    13,         // CrstMulticoreJitManager
    1,          // CrstMUThunkHash
    -1,         // CrstNativeBinderInit
    -1,         // CrstNativeImageCache
    1,          // CrstNls
    1,          // CrstNotifyGdb
    3,          // CrstObjectList
    1,          // CrstOnEventManager
    1,          // CrstPatchEntryPoint
    5,          // CrstPEImage
    1,          // CrstPEImagePDBStream
    18,         // CrstPendingTypeLoadEntry
    1,          // CrstPinHandle
    1,          // CrstPinnedByrefValidation
    1,          // CrstProfilerGCRefDataFreeList
    1,          // CrstProfilingAPIStatus
    1,          // CrstPublisherCertificate
    4,          // CrstRCWCache
    1,          // CrstRCWCleanupList
    4,          // CrstRCWRefCache
    5,          // CrstReadyToRunEntryPointToMethodDescMap
    1,          // CrstReDacl
    10,         // CrstReflection
    10,         // CrstReJITDomainTable
    15,         // CrstReJITGlobalRequest
    19,         // CrstRemoting
    4,          // CrstRetThunkCache
    1,          // CrstRWLock
    1,          // CrstSavedExceptionInfo
    1,          // CrstSaveModuleProfileData
    1,          // CrstSecurityStackwalkCache
    5,          // CrstSharedAssemblyCreate
    4,          // CrstSigConvert
    6,          // CrstSingleUseLock
    1,          // CrstSpecialStatics
    1,          // CrstSqmManager
    1,          // CrstStackSampler
    -1,         // CrstStressLog
    1,          // CrstStrongName
    5,          // CrstStubCache
    1,          // CrstStubDispatchCache
    4,          // CrstStubUnwindInfoHeapSegments
    4,          // CrstSyncBlockCache
    1,          // CrstSyncHashLock
    5,          // CrstSystemBaseDomain
    13,         // CrstSystemDomain
    1,          // CrstSystemDomainDelayedUnloadList
    1,          // CrstThreadIdDispenser
    1,          // CrstThreadpoolEventCache
    8,          // CrstThreadpoolTimerQueue
    8,          // CrstThreadpoolWaitThreads
    13,         // CrstThreadpoolWorker
    5,          // CrstThreadStaticDataHashTable
    12,         // CrstThreadStore
    10,         // CrstTieredCompilation
    10,         // CrstTPMethodTable
    4,          // CrstTypeEquivalenceMap
    8,          // CrstTypeIDMap
    4,          // CrstUMEntryThunkCache
    1,          // CrstUMThunkHash
    4,          // CrstUniqueStack
    8,          // CrstUnresolvedClassLock
    1,          // CrstUnwindInfoTableLock
    4,          // CrstVSDIndirectionCellLock
    4,          // CrstWinRTFactoryCache
    4,          // CrstWrapperTemplate
};

// An array mapping CrstType to a stringized name.
LPCSTR g_rgCrstNameMap[] =
{
    "CrstAllowedFiles",
    "CrstAppDomainCache",
    "CrstAppDomainHandleTable",
    "CrstArgBasedStubCache",
    "CrstAssemblyDependencyGraph",
    "CrstAssemblyIdentityCache",
    "CrstAssemblyList",
    "CrstAssemblyLoader",
    "CrstAvailableClass",
    "CrstAvailableParamTypes",
    "CrstBaseDomain",
    "CrstCCompRC",
    "CrstCer",
    "CrstClassFactInfoHash",
    "CrstClassInit",
    "CrstClrNotification",
    "CrstCLRPrivBinderMaps",
    "CrstCLRPrivBinderMapsAdd",
    "CrstCodeFragmentHeap",
    "CrstCOMCallWrapper",
    "CrstCOMWrapperCache",
    "CrstConnectionNameTable",
    "CrstContexts",
    "CrstCoreCLRBinderLog",
    "CrstCrstCLRPrivBinderLocalWinMDPath",
    "CrstCSPCache",
    "CrstDataTest1",
    "CrstDataTest2",
    "CrstDbgTransport",
    "CrstDeadlockDetection",
    "CrstDebuggerController",
    "CrstDebuggerFavorLock",
    "CrstDebuggerHeapExecMemLock",
    "CrstDebuggerHeapLock",
    "CrstDebuggerJitInfo",
    "CrstDebuggerMutex",
    "CrstDelegateToFPtrHash",
    "CrstDomainLocalBlock",
    "CrstDynamicIL",
    "CrstDynamicMT",
    "CrstDynLinkZapItems",
    "CrstEtwTypeLogHash",
    "CrstEventPipe",
    "CrstEventStore",
    "CrstException",
    "CrstExecuteManLock",
    "CrstExecuteManRangeLock",
    "CrstFCall",
    "CrstFriendAccessCache",
    "CrstFuncPtrStubs",
    "CrstFusionAppCtx",
    "CrstGCCover",
    "CrstGCMemoryPressure",
    "CrstGlobalStrLiteralMap",
    "CrstHandleTable",
    "CrstHostAssemblyMap",
    "CrstHostAssemblyMapAdd",
    "CrstIbcProfile",
    "CrstIJWFixupData",
    "CrstIJWHash",
    "CrstILStubGen",
    "CrstInlineTrackingMap",
    "CrstInstMethodHashTable",
    "CrstInterfaceVTableMap",
    "CrstInterop",
    "CrstInteropData",
    "CrstIOThreadpoolWorker",
    "CrstIsJMCMethod",
    "CrstISymUnmanagedReader",
    "CrstJit",
    "CrstJitGenericHandleCache",
    "CrstJitPerf",
    "CrstJumpStubCache",
    "CrstLeafLock",
    "CrstListLock",
    "CrstLoaderAllocator",
    "CrstLoaderAllocatorReferences",
    "CrstLoaderHeap",
    "CrstMda",
    "CrstMetadataTracker",
    "CrstMethodDescBackpatchInfoTracker",
    "CrstModIntPairList",
    "CrstModule",
    "CrstModuleFixup",
    "CrstModuleLookupTable",
    "CrstMulticoreJitHash",
    "CrstMulticoreJitManager",
    "CrstMUThunkHash",
    "CrstNativeBinderInit",
    "CrstNativeImageCache",
    "CrstNls",
    "CrstNotifyGdb",
    "CrstObjectList",
    "CrstOnEventManager",
    "CrstPatchEntryPoint",
    "CrstPEImage",
    "CrstPEImagePDBStream",
    "CrstPendingTypeLoadEntry",
    "CrstPinHandle",
    "CrstPinnedByrefValidation",
    "CrstProfilerGCRefDataFreeList",
    "CrstProfilingAPIStatus",
    "CrstPublisherCertificate",
    "CrstRCWCache",
    "CrstRCWCleanupList",
    "CrstRCWRefCache",
    "CrstReadyToRunEntryPointToMethodDescMap",
    "CrstReDacl",
    "CrstReflection",
    "CrstReJITDomainTable",
    "CrstReJITGlobalRequest",
    "CrstRemoting",
    "CrstRetThunkCache",
    "CrstRWLock",
    "CrstSavedExceptionInfo",
    "CrstSaveModuleProfileData",
    "CrstSecurityStackwalkCache",
    "CrstSharedAssemblyCreate",
    "CrstSigConvert",
    "CrstSingleUseLock",
    "CrstSpecialStatics",
    "CrstSqmManager",
    "CrstStackSampler",
    "CrstStressLog",
    "CrstStrongName",
    "CrstStubCache",
    "CrstStubDispatchCache",
    "CrstStubUnwindInfoHeapSegments",
    "CrstSyncBlockCache",
    "CrstSyncHashLock",
    "CrstSystemBaseDomain",
    "CrstSystemDomain",
    "CrstSystemDomainDelayedUnloadList",
    "CrstThreadIdDispenser",
    "CrstThreadpoolEventCache",
    "CrstThreadpoolTimerQueue",
    "CrstThreadpoolWaitThreads",
    "CrstThreadpoolWorker",
    "CrstThreadStaticDataHashTable",
    "CrstThreadStore",
    "CrstTieredCompilation",
    "CrstTPMethodTable",
    "CrstTypeEquivalenceMap",
    "CrstTypeIDMap",
    "CrstUMEntryThunkCache",
    "CrstUMThunkHash",
    "CrstUniqueStack",
    "CrstUnresolvedClassLock",
    "CrstUnwindInfoTableLock",
    "CrstVSDIndirectionCellLock",
    "CrstWinRTFactoryCache",
    "CrstWrapperTemplate",
};

// Define a special level constant for unordered locks.
#define CRSTUNORDERED (-1)

// Define inline helpers to map Crst types to names and levels.
inline static int GetCrstLevel(CrstType crstType)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(crstType >= 0 && crstType < kNumberOfCrstTypes);
    return g_rgCrstLevelMap[crstType];
}
inline static LPCSTR GetCrstName(CrstType crstType)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(crstType >= 0 && crstType < kNumberOfCrstTypes);
    return g_rgCrstNameMap[crstType];
}

#endif // defined(__IN_CRST_CPP) && defined(_DEBUG)
