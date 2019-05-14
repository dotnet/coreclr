// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __COMMON_LANGUAGE_RUNTIME_HRESULTS__
#define __COMMON_LANGUAGE_RUNTIME_HRESULTS__

#include <winerror.h>


//
//This file is AutoGenerated -- Do Not Edit by hand!!!
//
//Add new HRESULTS along with their corresponding error messages to
//corerror.xml
//

#ifndef FACILITY_URT
#define FACILITY_URT            0x13
#endif
#ifndef EMAKEHR
#define SMAKEHR(val) MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_URT, val)
#define EMAKEHR(val) MAKE_HRESULT(SEVERITY_ERROR, FACILITY_URT, val)
#endif

#define CLDB_S_TRUNCATION SMAKEHR(0x1106)
#define META_S_DUPLICATE SMAKEHR(0x1197)
#define CORDBG_S_BAD_START_SEQUENCE_POINT SMAKEHR(0x130b)
#define CORDBG_S_BAD_END_SEQUENCE_POINT SMAKEHR(0x130c)
#define CORDBG_S_FUNC_EVAL_HAS_NO_RESULT SMAKEHR(0x1316)
#define CORDBG_S_VALUE_POINTS_TO_VOID SMAKEHR(0x1317)
#define CORDBG_S_FUNC_EVAL_ABORTED SMAKEHR(0x1319)
#define CORDBG_S_AT_END_OF_STACK SMAKEHR(0x1324)
#define CORDBG_S_NOT_ALL_BITS_SET SMAKEHR(0x1c13)
#define CEE_E_CVTRES_NOT_FOUND EMAKEHR(0x1001)
#define COR_E_TYPEUNLOADED EMAKEHR(0x1013)
#define COR_E_APPDOMAINUNLOADED EMAKEHR(0x1014)
#define COR_E_CANNOTUNLOADAPPDOMAIN EMAKEHR(0x1015)
#define MSEE_E_ASSEMBLYLOADINPROGRESS EMAKEHR(0x1016)
#define COR_E_ASSEMBLYEXPECTED EMAKEHR(0x1018)
#define COR_E_FIXUPSINEXE EMAKEHR(0x1019)
#define COR_E_NEWER_RUNTIME EMAKEHR(0x101b)
#define COR_E_MULTIMODULEASSEMBLIESDIALLOWED EMAKEHR(0x101e)
#define HOST_E_DEADLOCK EMAKEHR(0x1020)
#define HOST_E_INVALIDOPERATION EMAKEHR(0x1022)
#define HOST_E_CLRNOTAVAILABLE EMAKEHR(0x1023)
#define HOST_E_EXITPROCESS_THREADABORT EMAKEHR(0x1027)
#define HOST_E_EXITPROCESS_ADUNLOAD EMAKEHR(0x1028)
#define HOST_E_EXITPROCESS_TIMEOUT EMAKEHR(0x1029)
#define HOST_E_EXITPROCESS_OUTOFMEMORY EMAKEHR(0x102a)
#define COR_E_MODULE_HASH_CHECK_FAILED EMAKEHR(0x1039)
#define FUSION_E_REF_DEF_MISMATCH EMAKEHR(0x1040)
#define FUSION_E_INVALID_PRIVATE_ASM_LOCATION EMAKEHR(0x1041)
#define FUSION_E_ASM_MODULE_MISSING EMAKEHR(0x1042)
#define FUSION_E_PRIVATE_ASM_DISALLOWED EMAKEHR(0x1044)
#define FUSION_E_SIGNATURE_CHECK_FAILED EMAKEHR(0x1045)
#define FUSION_E_INVALID_NAME EMAKEHR(0x1047)
#define FUSION_E_CODE_DOWNLOAD_DISABLED EMAKEHR(0x1048)
#define FUSION_E_HOST_GAC_ASM_MISMATCH EMAKEHR(0x1050)
#define FUSION_E_LOADFROM_BLOCKED EMAKEHR(0x1051)
#define FUSION_E_CACHEFILE_FAILED EMAKEHR(0x1052)
#define FUSION_E_APP_DOMAIN_LOCKED EMAKEHR(0x1053)
#define FUSION_E_CONFIGURATION_ERROR EMAKEHR(0x1054)
#define FUSION_E_MANIFEST_PARSE_ERROR EMAKEHR(0x1055)
#define COR_E_LOADING_REFERENCE_ASSEMBLY EMAKEHR(0x1058)
#define COR_E_NI_AND_RUNTIME_VERSION_MISMATCH EMAKEHR(0x1059)
#define COR_E_LOADING_WINMD_REFERENCE_ASSEMBLY EMAKEHR(0x1069)
#define COR_E_AMBIGUOUSIMPLEMENTATION EMAKEHR(0x106a)
#define CLDB_E_FILE_BADREAD EMAKEHR(0x1100)
#define CLDB_E_FILE_BADWRITE EMAKEHR(0x1101)
#define CLDB_E_FILE_OLDVER EMAKEHR(0x1107)
#define CLDB_E_SMDUPLICATE EMAKEHR(0x110a)
#define CLDB_E_NO_DATA EMAKEHR(0x110b)
#define CLDB_E_INCOMPATIBLE EMAKEHR(0x110d)
#define CLDB_E_FILE_CORRUPT EMAKEHR(0x110e)
#define CLDB_E_BADUPDATEMODE EMAKEHR(0x1110)
#define CLDB_E_INDEX_NOTFOUND EMAKEHR(0x1124)
#define CLDB_E_RECORD_NOTFOUND EMAKEHR(0x1130)
#define CLDB_E_RECORD_OUTOFORDER EMAKEHR(0x1135)
#define CLDB_E_TOO_BIG EMAKEHR(0x1154)
#define META_E_INVALID_TOKEN_TYPE EMAKEHR(0x115f)
#define TLBX_E_LIBNOTREGISTERED EMAKEHR(0x1165)
#define META_E_BADMETADATA EMAKEHR(0x118a)
#define META_E_BAD_SIGNATURE EMAKEHR(0x1192)
#define META_E_BAD_INPUT_PARAMETER EMAKEHR(0x1193)
#define META_E_CANNOTRESOLVETYPEREF EMAKEHR(0x1196)
#define META_E_STRINGSPACE_FULL EMAKEHR(0x1198)
#define META_E_HAS_UNMARKALL EMAKEHR(0x119a)
#define META_E_MUST_CALL_UNMARKALL EMAKEHR(0x119b)
#define META_E_CA_INVALID_TARGET EMAKEHR(0x11c0)
#define META_E_CA_INVALID_VALUE EMAKEHR(0x11c1)
#define META_E_CA_INVALID_BLOB EMAKEHR(0x11c2)
#define META_E_CA_REPEATED_ARG EMAKEHR(0x11c3)
#define META_E_CA_UNKNOWN_ARGUMENT EMAKEHR(0x11c4)
#define META_E_CA_UNEXPECTED_TYPE EMAKEHR(0x11c7)
#define META_E_CA_INVALID_ARGTYPE EMAKEHR(0x11c8)
#define META_E_CA_INVALID_ARG_FOR_TYPE EMAKEHR(0x11c9)
#define META_E_CA_INVALID_UUID EMAKEHR(0x11ca)
#define META_E_CA_INVALID_MARSHALAS_FIELDS EMAKEHR(0x11cb)
#define META_E_CA_NT_FIELDONLY EMAKEHR(0x11cc)
#define META_E_CA_NEGATIVE_PARAMINDEX EMAKEHR(0x11cd)
#define META_E_CA_NEGATIVE_CONSTSIZE EMAKEHR(0x11cf)
#define META_E_CA_FIXEDSTR_SIZE_REQUIRED EMAKEHR(0x11d0)
#define META_E_CA_CUSTMARSH_TYPE_REQUIRED EMAKEHR(0x11d1)
#define META_E_NOT_IN_ENC_MODE EMAKEHR(0x11d4)
#define META_E_CA_BAD_FRIENDS_ARGS EMAKEHR(0x11e5)
#define META_E_CA_FRIENDS_SN_REQUIRED EMAKEHR(0x11e6)
#define VLDTR_E_RID_OUTOFRANGE EMAKEHR(0x1203)
#define VLDTR_E_STRING_INVALID EMAKEHR(0x1206)
#define VLDTR_E_GUID_INVALID EMAKEHR(0x1207)
#define VLDTR_E_BLOB_INVALID EMAKEHR(0x1208)
#define VLDTR_E_MR_BADCALLINGCONV EMAKEHR(0x1224)
#define VLDTR_E_SIGNULL EMAKEHR(0x1237)
#define VLDTR_E_MD_BADCALLINGCONV EMAKEHR(0x1239)
#define VLDTR_E_MD_THISSTATIC EMAKEHR(0x123a)
#define VLDTR_E_MD_NOTTHISNOTSTATIC EMAKEHR(0x123b)
#define VLDTR_E_MD_NOARGCNT EMAKEHR(0x123c)
#define VLDTR_E_SIG_MISSELTYPE EMAKEHR(0x123d)
#define VLDTR_E_SIG_MISSTKN EMAKEHR(0x123e)
#define VLDTR_E_SIG_TKNBAD EMAKEHR(0x123f)
#define VLDTR_E_SIG_MISSFPTR EMAKEHR(0x1240)
#define VLDTR_E_SIG_MISSFPTRARGCNT EMAKEHR(0x1241)
#define VLDTR_E_SIG_MISSRANK EMAKEHR(0x1242)
#define VLDTR_E_SIG_MISSNSIZE EMAKEHR(0x1243)
#define VLDTR_E_SIG_MISSSIZE EMAKEHR(0x1244)
#define VLDTR_E_SIG_MISSNLBND EMAKEHR(0x1245)
#define VLDTR_E_SIG_MISSLBND EMAKEHR(0x1246)
#define VLDTR_E_SIG_BADELTYPE EMAKEHR(0x1247)
#define VLDTR_E_TD_ENCLNOTNESTED EMAKEHR(0x1256)
#define VLDTR_E_FMD_PINVOKENOTSTATIC EMAKEHR(0x1277)
#define VLDTR_E_SIG_SENTINMETHODDEF EMAKEHR(0x12df)
#define VLDTR_E_SIG_SENTMUSTVARARG EMAKEHR(0x12e0)
#define VLDTR_E_SIG_MULTSENTINELS EMAKEHR(0x12e1)
#define VLDTR_E_SIG_MISSARG EMAKEHR(0x12e3)
#define VLDTR_E_SIG_BYREFINFIELD EMAKEHR(0x12e4)
#define CORDBG_E_UNRECOVERABLE_ERROR EMAKEHR(0x1300)
#define CORDBG_E_PROCESS_TERMINATED EMAKEHR(0x1301)
#define CORDBG_E_PROCESS_NOT_SYNCHRONIZED EMAKEHR(0x1302)
#define CORDBG_E_CLASS_NOT_LOADED EMAKEHR(0x1303)
#define CORDBG_E_IL_VAR_NOT_AVAILABLE EMAKEHR(0x1304)
#define CORDBG_E_BAD_REFERENCE_VALUE EMAKEHR(0x1305)
#define CORDBG_E_FIELD_NOT_AVAILABLE EMAKEHR(0x1306)
#define CORDBG_E_NON_NATIVE_FRAME EMAKEHR(0x1307)
#define CORDBG_E_CODE_NOT_AVAILABLE EMAKEHR(0x1309)
#define CORDBG_E_FUNCTION_NOT_IL EMAKEHR(0x130a)
#define CORDBG_E_CANT_SET_IP_INTO_FINALLY EMAKEHR(0x130e)
#define CORDBG_E_CANT_SET_IP_OUT_OF_FINALLY EMAKEHR(0x130f)
#define CORDBG_E_CANT_SET_IP_INTO_CATCH EMAKEHR(0x1310)
#define CORDBG_E_SET_IP_NOT_ALLOWED_ON_NONLEAF_FRAME EMAKEHR(0x1311)
#define CORDBG_E_SET_IP_IMPOSSIBLE EMAKEHR(0x1312)
#define CORDBG_E_FUNC_EVAL_BAD_START_POINT EMAKEHR(0x1313)
#define CORDBG_E_INVALID_OBJECT EMAKEHR(0x1314)
#define CORDBG_E_FUNC_EVAL_NOT_COMPLETE EMAKEHR(0x1315)
#define CORDBG_E_STATIC_VAR_NOT_AVAILABLE EMAKEHR(0x131a)
#define CORDBG_E_CANT_SETIP_INTO_OR_OUT_OF_FILTER EMAKEHR(0x131c)
#define CORDBG_E_CANT_CHANGE_JIT_SETTING_FOR_ZAP_MODULE EMAKEHR(0x131d)
#define CORDBG_E_CANT_SET_IP_OUT_OF_FINALLY_ON_WIN64 EMAKEHR(0x131e)
#define CORDBG_E_CANT_SET_IP_OUT_OF_CATCH_ON_WIN64 EMAKEHR(0x131f)
#define CORDBG_E_CANT_SET_TO_JMC EMAKEHR(0x1323)
#define CORDBG_E_NO_CONTEXT_FOR_INTERNAL_FRAME EMAKEHR(0x1325)
#define CORDBG_E_NOT_CHILD_FRAME EMAKEHR(0x1326)
#define CORDBG_E_NON_MATCHING_CONTEXT EMAKEHR(0x1327)
#define CORDBG_E_PAST_END_OF_STACK EMAKEHR(0x1328)
#define CORDBG_E_FUNC_EVAL_CANNOT_UPDATE_REGISTER_IN_NONLEAF_FRAME EMAKEHR(0x1329)
#define CORDBG_E_BAD_THREAD_STATE EMAKEHR(0x132d)
#define CORDBG_E_DEBUGGER_ALREADY_ATTACHED EMAKEHR(0x132e)
#define CORDBG_E_SUPERFLOUS_CONTINUE EMAKEHR(0x132f)
#define CORDBG_E_SET_VALUE_NOT_ALLOWED_ON_NONLEAF_FRAME EMAKEHR(0x1330)
#define CORDBG_E_ENC_MODULE_NOT_ENC_ENABLED EMAKEHR(0x1332)
#define CORDBG_E_SET_IP_NOT_ALLOWED_ON_EXCEPTION EMAKEHR(0x1333)
#define CORDBG_E_VARIABLE_IS_ACTUALLY_LITERAL EMAKEHR(0x1334)
#define CORDBG_E_PROCESS_DETACHED EMAKEHR(0x1335)
#define CORDBG_E_ENC_CANT_ADD_FIELD_TO_VALUE_OR_LAYOUT_CLASS EMAKEHR(0x1338)
#define CORDBG_E_FIELD_NOT_STATIC EMAKEHR(0x133b)
#define CORDBG_E_FIELD_NOT_INSTANCE EMAKEHR(0x133c)
#define CORDBG_E_ENC_JIT_CANT_UPDATE EMAKEHR(0x133f)
#define CORDBG_E_ENC_INTERNAL_ERROR EMAKEHR(0x1341)
#define CORDBG_E_ENC_HANGING_FIELD EMAKEHR(0x1342)
#define CORDBG_E_MODULE_NOT_LOADED EMAKEHR(0x1343)
#define CORDBG_E_UNABLE_TO_SET_BREAKPOINT EMAKEHR(0x1345)
#define CORDBG_E_DEBUGGING_NOT_POSSIBLE EMAKEHR(0x1346)
#define CORDBG_E_KERNEL_DEBUGGER_ENABLED EMAKEHR(0x1347)
#define CORDBG_E_KERNEL_DEBUGGER_PRESENT EMAKEHR(0x1348)
#define CORDBG_E_INCOMPATIBLE_PROTOCOL EMAKEHR(0x134b)
#define CORDBG_E_TOO_MANY_PROCESSES EMAKEHR(0x134c)
#define CORDBG_E_INTEROP_NOT_SUPPORTED EMAKEHR(0x134d)
#define CORDBG_E_NO_REMAP_BREAKPIONT EMAKEHR(0x134e)
#define CORDBG_E_OBJECT_NEUTERED EMAKEHR(0x134f)
#define CORPROF_E_FUNCTION_NOT_COMPILED EMAKEHR(0x1350)
#define CORPROF_E_DATAINCOMPLETE EMAKEHR(0x1351)
#define CORPROF_E_FUNCTION_NOT_IL EMAKEHR(0x1354)
#define CORPROF_E_NOT_MANAGED_THREAD EMAKEHR(0x1355)
#define CORPROF_E_CALL_ONLY_FROM_INIT EMAKEHR(0x1356)
#define CORPROF_E_NOT_YET_AVAILABLE EMAKEHR(0x135b)
#define CORPROF_E_TYPE_IS_PARAMETERIZED EMAKEHR(0x135c)
#define CORPROF_E_FUNCTION_IS_PARAMETERIZED EMAKEHR(0x135d)
#define CORPROF_E_STACKSNAPSHOT_INVALID_TGT_THREAD EMAKEHR(0x135e)
#define CORPROF_E_STACKSNAPSHOT_UNMANAGED_CTX EMAKEHR(0x135f)
#define CORPROF_E_STACKSNAPSHOT_UNSAFE EMAKEHR(0x1360)
#define CORPROF_E_STACKSNAPSHOT_ABORTED EMAKEHR(0x1361)
#define CORPROF_E_LITERALS_HAVE_NO_ADDRESS EMAKEHR(0x1362)
#define CORPROF_E_UNSUPPORTED_CALL_SEQUENCE EMAKEHR(0x1363)
#define CORPROF_E_ASYNCHRONOUS_UNSAFE EMAKEHR(0x1364)
#define CORPROF_E_CLASSID_IS_ARRAY EMAKEHR(0x1365)
#define CORPROF_E_CLASSID_IS_COMPOSITE EMAKEHR(0x1366)
#define CORPROF_E_PROFILER_DETACHING EMAKEHR(0x1367)
#define CORPROF_E_PROFILER_NOT_ATTACHABLE EMAKEHR(0x1368)
#define CORPROF_E_UNRECOGNIZED_PIPE_MSG_FORMAT EMAKEHR(0x1369)
#define CORPROF_E_PROFILER_ALREADY_ACTIVE EMAKEHR(0x136a)
#define CORPROF_E_PROFILEE_INCOMPATIBLE_WITH_TRIGGER EMAKEHR(0x136b)
#define CORPROF_E_IPC_FAILED EMAKEHR(0x136c)
#define CORPROF_E_PROFILEE_PROCESS_NOT_FOUND EMAKEHR(0x136d)
#define CORPROF_E_CALLBACK3_REQUIRED EMAKEHR(0x136e)
#define CORPROF_E_UNSUPPORTED_FOR_ATTACHING_PROFILER EMAKEHR(0x136f)
#define CORPROF_E_IRREVERSIBLE_INSTRUMENTATION_PRESENT EMAKEHR(0x1370)
#define CORPROF_E_RUNTIME_UNINITIALIZED EMAKEHR(0x1371)
#define CORPROF_E_IMMUTABLE_FLAGS_SET EMAKEHR(0x1372)
#define CORPROF_E_PROFILER_NOT_YET_INITIALIZED EMAKEHR(0x1373)
#define CORPROF_E_INCONSISTENT_WITH_FLAGS EMAKEHR(0x1374)
#define CORPROF_E_PROFILER_CANCEL_ACTIVATION EMAKEHR(0x1375)
#define CORPROF_E_CONCURRENT_GC_NOT_PROFILABLE EMAKEHR(0x1376)
#define CORPROF_E_DEBUGGING_DISABLED EMAKEHR(0x1378)
#define CORPROF_E_TIMEOUT_WAITING_FOR_CONCURRENT_GC EMAKEHR(0x1379)
#define CORPROF_E_MODULE_IS_DYNAMIC EMAKEHR(0x137a)
#define CORPROF_E_CALLBACK4_REQUIRED EMAKEHR(0x137b)
#define CORPROF_E_REJIT_NOT_ENABLED EMAKEHR(0x137c)
#define CORPROF_E_FUNCTION_IS_COLLECTIBLE EMAKEHR(0x137e)
#define CORPROF_E_CALLBACK6_REQUIRED EMAKEHR(0x1380)
#define CORPROF_E_CALLBACK7_REQUIRED EMAKEHR(0x1382)
#define CORPROF_E_REJIT_INLINING_DISABLED EMAKEHR(0x1383)
#define SECURITY_E_INCOMPATIBLE_SHARE EMAKEHR(0x1401)
#define SECURITY_E_UNVERIFIABLE EMAKEHR(0x1402)
#define SECURITY_E_INCOMPATIBLE_EVIDENCE EMAKEHR(0x1403)
#define CORSEC_E_POLICY_EXCEPTION EMAKEHR(0x1416)
#define CORSEC_E_MIN_GRANT_FAIL EMAKEHR(0x1417)
#define CORSEC_E_NO_EXEC_PERM EMAKEHR(0x1418)
#define CORSEC_E_XMLSYNTAX EMAKEHR(0x1419)
#define CORSEC_E_INVALID_STRONGNAME EMAKEHR(0x141a)
#define CORSEC_E_MISSING_STRONGNAME EMAKEHR(0x141b)
#define CORSEC_E_INVALID_IMAGE_FORMAT EMAKEHR(0x141d)
#define CORSEC_E_INVALID_PUBLICKEY EMAKEHR(0x141e)
#define CORSEC_E_SIGNATURE_MISMATCH EMAKEHR(0x1420)
#define CORSEC_E_CRYPTO EMAKEHR(0x1430)
#define CORSEC_E_CRYPTO_UNEX_OPER EMAKEHR(0x1431)
#define CORSECATTR_E_BAD_ACTION EMAKEHR(0x1442)
#define COR_E_EXCEPTION EMAKEHR(0x1500)
#define COR_E_SYSTEM EMAKEHR(0x1501)
#define COR_E_ARGUMENTOUTOFRANGE EMAKEHR(0x1502)
#define COR_E_ARRAYTYPEMISMATCH EMAKEHR(0x1503)
#define COR_E_CONTEXTMARSHAL EMAKEHR(0x1504)
#define COR_E_TIMEOUT EMAKEHR(0x1505)
#define COR_E_EXECUTIONENGINE EMAKEHR(0x1506)
#define COR_E_FIELDACCESS EMAKEHR(0x1507)
#define COR_E_INDEXOUTOFRANGE EMAKEHR(0x1508)
#define COR_E_INVALIDOPERATION EMAKEHR(0x1509)
#define COR_E_SECURITY EMAKEHR(0x150a)
#define COR_E_SERIALIZATION EMAKEHR(0x150c)
#define COR_E_VERIFICATION EMAKEHR(0x150d)
#define COR_E_METHODACCESS EMAKEHR(0x1510)
#define COR_E_MISSINGFIELD EMAKEHR(0x1511)
#define COR_E_MISSINGMEMBER EMAKEHR(0x1512)
#define COR_E_MISSINGMETHOD EMAKEHR(0x1513)
#define COR_E_MULTICASTNOTSUPPORTED EMAKEHR(0x1514)
#define COR_E_NOTSUPPORTED EMAKEHR(0x1515)
#define COR_E_OVERFLOW EMAKEHR(0x1516)
#define COR_E_RANK EMAKEHR(0x1517)
#define COR_E_SYNCHRONIZATIONLOCK EMAKEHR(0x1518)
#define COR_E_THREADINTERRUPTED EMAKEHR(0x1519)
#define COR_E_MEMBERACCESS EMAKEHR(0x151a)
#define COR_E_THREADSTATE EMAKEHR(0x1520)
#define COR_E_THREADSTOP EMAKEHR(0x1521)
#define COR_E_TYPELOAD EMAKEHR(0x1522)
#define COR_E_ENTRYPOINTNOTFOUND EMAKEHR(0x1523)
#define COR_E_DLLNOTFOUND EMAKEHR(0x1524)
#define COR_E_THREADSTART EMAKEHR(0x1525)
#define COR_E_INVALIDCOMOBJECT EMAKEHR(0x1527)
#define COR_E_NOTFINITENUMBER EMAKEHR(0x1528)
#define COR_E_DUPLICATEWAITOBJECT EMAKEHR(0x1529)
#define COR_E_SEMAPHOREFULL EMAKEHR(0x152b)
#define COR_E_WAITHANDLECANNOTBEOPENED EMAKEHR(0x152c)
#define COR_E_ABANDONEDMUTEX EMAKEHR(0x152d)
#define COR_E_THREADABORTED EMAKEHR(0x1530)
#define COR_E_INVALIDOLEVARIANTTYPE EMAKEHR(0x1531)
#define COR_E_MISSINGMANIFESTRESOURCE EMAKEHR(0x1532)
#define COR_E_SAFEARRAYTYPEMISMATCH EMAKEHR(0x1533)
#define COR_E_TYPEINITIALIZATION EMAKEHR(0x1534)
#define COR_E_MARSHALDIRECTIVE EMAKEHR(0x1535)
#define COR_E_MISSINGSATELLITEASSEMBLY EMAKEHR(0x1536)
#define COR_E_FORMAT EMAKEHR(0x1537)
#define COR_E_SAFEARRAYRANKMISMATCH EMAKEHR(0x1538)
#define COR_E_PLATFORMNOTSUPPORTED EMAKEHR(0x1539)
#define COR_E_INVALIDPROGRAM EMAKEHR(0x153a)
#define COR_E_OPERATIONCANCELED EMAKEHR(0x153b)
#define COR_E_INSUFFICIENTMEMORY EMAKEHR(0x153d)
#define COR_E_RUNTIMEWRAPPED EMAKEHR(0x153e)
#define COR_E_DATAMISALIGNED EMAKEHR(0x1541)
#define COR_E_CODECONTRACTFAILED EMAKEHR(0x1542)
#define COR_E_TYPEACCESS EMAKEHR(0x1543)
#define COR_E_ACCESSING_CCW EMAKEHR(0x1544)
#define COR_E_KEYNOTFOUND EMAKEHR(0x1577)
#define COR_E_INSUFFICIENTEXECUTIONSTACK EMAKEHR(0x1578)
#define COR_E_APPLICATION EMAKEHR(0x1600)
#define COR_E_INVALIDFILTERCRITERIA EMAKEHR(0x1601)
#define COR_E_REFLECTIONTYPELOAD EMAKEHR(0x1602)
#define COR_E_TARGET EMAKEHR(0x1603)
#define COR_E_TARGETINVOCATION EMAKEHR(0x1604)
#define COR_E_CUSTOMATTRIBUTEFORMAT EMAKEHR(0x1605)
#define COR_E_IO EMAKEHR(0x1620)
#define COR_E_FILELOAD EMAKEHR(0x1621)
#define COR_E_OBJECTDISPOSED EMAKEHR(0x1622)
#define COR_E_FAILFAST EMAKEHR(0x1623)
#define COR_E_HOSTPROTECTION EMAKEHR(0x1640)
#define COR_E_ILLEGAL_REENTRANCY EMAKEHR(0x1641)
#define CLR_E_SHIM_RUNTIMELOAD EMAKEHR(0x1700)
#define CLR_E_SHIM_LEGACYRUNTIMEALREADYBOUND EMAKEHR(0x1704)
#define VER_E_FIELD_SIG EMAKEHR(0x1815)
#define VER_E_CIRCULAR_VAR_CONSTRAINTS EMAKEHR(0x18ce)
#define VER_E_CIRCULAR_MVAR_CONSTRAINTS EMAKEHR(0x18cf)
#define COR_E_Data EMAKEHR(0x1920)
#define VLDTR_E_SIG_BADVOID EMAKEHR(0x1b24)
#define VLDTR_E_GP_ILLEGAL_VARIANT_MVAR EMAKEHR(0x1b2d)
#define CORDBG_E_THREAD_NOT_SCHEDULED EMAKEHR(0x1c00)
#define CORDBG_E_HANDLE_HAS_BEEN_DISPOSED EMAKEHR(0x1c01)
#define CORDBG_E_NONINTERCEPTABLE_EXCEPTION EMAKEHR(0x1c02)
#define CORDBG_E_INTERCEPT_FRAME_ALREADY_SET EMAKEHR(0x1c04)
#define CORDBG_E_NO_NATIVE_PATCH_AT_ADDR EMAKEHR(0x1c05)
#define CORDBG_E_MUST_BE_INTEROP_DEBUGGING EMAKEHR(0x1c06)
#define CORDBG_E_NATIVE_PATCH_ALREADY_AT_ADDR EMAKEHR(0x1c07)
#define CORDBG_E_TIMEOUT EMAKEHR(0x1c08)
#define CORDBG_E_CANT_CALL_ON_THIS_THREAD EMAKEHR(0x1c09)
#define CORDBG_E_ENC_INFOLESS_METHOD EMAKEHR(0x1c0a)
#define CORDBG_E_ENC_IN_FUNCLET EMAKEHR(0x1c0c)
#define CORDBG_E_ENC_EDIT_NOT_SUPPORTED EMAKEHR(0x1c0e)
#define CORDBG_E_NOTREADY EMAKEHR(0x1c10)
#define CORDBG_E_CANNOT_RESOLVE_ASSEMBLY EMAKEHR(0x1c11)
#define CORDBG_E_MUST_BE_IN_LOAD_MODULE EMAKEHR(0x1c12)
#define CORDBG_E_CANNOT_BE_ON_ATTACH EMAKEHR(0x1c13)
#define CORDBG_E_NGEN_NOT_SUPPORTED EMAKEHR(0x1c14)
#define CORDBG_E_ILLEGAL_SHUTDOWN_ORDER EMAKEHR(0x1c15)
#define CORDBG_E_CANNOT_DEBUG_FIBER_PROCESS EMAKEHR(0x1c16)
#define CORDBG_E_MUST_BE_IN_CREATE_PROCESS EMAKEHR(0x1c17)
#define CORDBG_E_DETACH_FAILED_OUTSTANDING_EVALS EMAKEHR(0x1c18)
#define CORDBG_E_DETACH_FAILED_OUTSTANDING_STEPPERS EMAKEHR(0x1c19)
#define CORDBG_E_CANT_INTEROP_STEP_OUT EMAKEHR(0x1c20)
#define CORDBG_E_DETACH_FAILED_OUTSTANDING_BREAKPOINTS EMAKEHR(0x1c21)
#define CORDBG_E_ILLEGAL_IN_STACK_OVERFLOW EMAKEHR(0x1c22)
#define CORDBG_E_ILLEGAL_AT_GC_UNSAFE_POINT EMAKEHR(0x1c23)
#define CORDBG_E_ILLEGAL_IN_PROLOG EMAKEHR(0x1c24)
#define CORDBG_E_ILLEGAL_IN_NATIVE_CODE EMAKEHR(0x1c25)
#define CORDBG_E_ILLEGAL_IN_OPTIMIZED_CODE EMAKEHR(0x1c26)
#define CORDBG_E_APPDOMAIN_MISMATCH EMAKEHR(0x1c28)
#define CORDBG_E_CONTEXT_UNVAILABLE EMAKEHR(0x1c29)
#define CORDBG_E_UNCOMPATIBLE_PLATFORMS EMAKEHR(0x1c30)
#define CORDBG_E_DEBUGGING_DISABLED EMAKEHR(0x1c31)
#define CORDBG_E_DETACH_FAILED_ON_ENC EMAKEHR(0x1c32)
#define CORDBG_E_CURRENT_EXCEPTION_IS_OUTSIDE_CURRENT_EXECUTION_SCOPE EMAKEHR(0x1c33)
#define CORDBG_E_HELPER_MAY_DEADLOCK EMAKEHR(0x1c34)
#define CORDBG_E_MISSING_METADATA EMAKEHR(0x1c35)
#define CORDBG_E_TARGET_INCONSISTENT EMAKEHR(0x1c36)
#define CORDBG_E_DETACH_FAILED_OUTSTANDING_TARGET_RESOURCES EMAKEHR(0x1c37)
#define CORDBG_E_TARGET_READONLY EMAKEHR(0x1c38)
#define CORDBG_E_MISMATCHED_CORWKS_AND_DACWKS_DLLS EMAKEHR(0x1c39)
#define CORDBG_E_MODULE_LOADED_FROM_DISK EMAKEHR(0x1c3a)
#define CORDBG_E_SYMBOLS_NOT_AVAILABLE EMAKEHR(0x1c3b)
#define CORDBG_E_DEBUG_COMPONENT_MISSING EMAKEHR(0x1c3c)
#define CORDBG_E_LIBRARY_PROVIDER_ERROR EMAKEHR(0x1c43)
#define CORDBG_E_NOT_CLR EMAKEHR(0x1c44)
#define CORDBG_E_MISSING_DATA_TARGET_INTERFACE EMAKEHR(0x1c45)
#define CORDBG_E_UNSUPPORTED_DEBUGGING_MODEL EMAKEHR(0x1c46)
#define CORDBG_E_UNSUPPORTED_FORWARD_COMPAT EMAKEHR(0x1c47)
#define CORDBG_E_UNSUPPORTED_VERSION_STRUCT EMAKEHR(0x1c48)
#define CORDBG_E_READVIRTUAL_FAILURE EMAKEHR(0x1c49)
#define CORDBG_E_VALUE_POINTS_TO_FUNCTION EMAKEHR(0x1c4a)
#define CORDBG_E_CORRUPT_OBJECT EMAKEHR(0x1c4b)
#define CORDBG_E_GC_STRUCTURES_INVALID EMAKEHR(0x1c4c)
#define CORDBG_E_INVALID_OPCODE EMAKEHR(0x1c4d)
#define CORDBG_E_UNSUPPORTED EMAKEHR(0x1c4e)
#define CORDBG_E_MISSING_DEBUGGER_EXPORTS EMAKEHR(0x1c4f)
#define CORDBG_E_DATA_TARGET_ERROR EMAKEHR(0x1c61)
#define CORDBG_E_NO_IMAGE_AVAILABLE EMAKEHR(0x1c64)
#define PEFMT_E_64BIT EMAKEHR(0x1d02)
#define PEFMT_E_32BIT EMAKEHR(0x1d0b)
#define NGEN_E_SYS_ASM_NI_MISSING EMAKEHR(0x1f06)
#define CLDB_E_INTERNALERROR EMAKEHR(0x1fff)
#define CLR_E_BIND_ASSEMBLY_VERSION_TOO_LOW EMAKEHR(0x2000)
#define CLR_E_BIND_ASSEMBLY_PUBLIC_KEY_MISMATCH EMAKEHR(0x2001)
#define CLR_E_BIND_IMAGE_UNAVAILABLE EMAKEHR(0x2002)
#define CLR_E_BIND_UNRECOGNIZED_IDENTITY_FORMAT EMAKEHR(0x2003)
#define CLR_E_BIND_ASSEMBLY_NOT_FOUND EMAKEHR(0x2004)
#define CLR_E_BIND_TYPE_NOT_FOUND EMAKEHR(0x2005)
#define CLR_E_BIND_SYS_ASM_NI_MISSING EMAKEHR(0x2006)
#define CLR_E_BIND_NI_SECURITY_FAILURE EMAKEHR(0x2007)
#define CLR_E_BIND_NI_DEP_IDENTITY_MISMATCH EMAKEHR(0x2008)
#define CLR_E_GC_OOM EMAKEHR(0x2009)
#define CLR_E_GC_BAD_AFFINITY_CONFIG EMAKEHR(0x200a)
#define CLR_E_GC_BAD_AFFINITY_CONFIG_FORMAT EMAKEHR(0x200b)
#define COR_E_UNAUTHORIZEDACCESS E_ACCESSDENIED
#define COR_E_ARGUMENT E_INVALIDARG
#define COR_E_INVALIDCAST E_NOINTERFACE
#define COR_E_OUTOFMEMORY E_OUTOFMEMORY
#define COR_E_NULLREFERENCE E_POINTER
#define COR_E_ARITHMETIC __HRESULT_FROM_WIN32(ERROR_ARITHMETIC_OVERFLOW)
#define COR_E_PATHTOOLONG __HRESULT_FROM_WIN32(ERROR_FILENAME_EXCED_RANGE)
#define COR_E_FILENOTFOUND __HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)
#define COR_E_ENDOFSTREAM __HRESULT_FROM_WIN32(ERROR_HANDLE_EOF)
#define COR_E_DIRECTORYNOTFOUND __HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND)
#define COR_E_STACKOVERFLOW __HRESULT_FROM_WIN32(ERROR_STACK_OVERFLOW)
#define COR_E_AMBIGUOUSMATCH _HRESULT_TYPEDEF_(0x8000211DL)
#define COR_E_TARGETPARAMCOUNT _HRESULT_TYPEDEF_(0x8002000EL)
#define COR_E_DIVIDEBYZERO _HRESULT_TYPEDEF_(0x80020012L)
#define COR_E_BADIMAGEFORMAT _HRESULT_TYPEDEF_(0x8007000BL)


#endif // __COMMON_LANGUAGE_RUNTIME_HRESULTS__
