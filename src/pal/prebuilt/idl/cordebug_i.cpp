

/* this ALWAYS GENERATED file contains the IIDs and CLSIDs */

/* link this file in with the server and any clients */


 /* File created by MIDL compiler version 8.01.0622 */
/* at Mon Jan 18 19:14:07 2038
 */
/* Compiler settings for E:/repos/coreclr2/src/inc/cordebug.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.01.0622 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


#ifdef __cplusplus
extern "C"{
#endif 


#include <rpc.h>
#include <rpcndr.h>

#ifdef _MIDL_USE_GUIDDEF_

#ifndef INITGUID
#define INITGUID
#include <guiddef.h>
#undef INITGUID
#else
#include <guiddef.h>
#endif

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        DEFINE_GUID(name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8)

#else // !_MIDL_USE_GUIDDEF_

#ifndef __IID_DEFINED__
#define __IID_DEFINED__

typedef struct _IID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
typedef IID CLSID;
#endif // CLSID_DEFINED

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        EXTERN_C __declspec(selectany) const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}

#endif // !_MIDL_USE_GUIDDEF_

MIDL_DEFINE_GUID(IID, IID_ICorDebugDataTarget,0xFE06DC28,0x49FB,0x4636,0xA4,0xA3,0xE8,0x0D,0xB4,0xAE,0x11,0x6C);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStaticFieldSymbol,0xCBF9DA63,0xF68D,0x4BBB,0xA2,0x1C,0x15,0xA4,0x5E,0xAA,0xDF,0x5B);


MIDL_DEFINE_GUID(IID, IID_ICorDebugInstanceFieldSymbol,0xA074096B,0x3ADC,0x4485,0x81,0xDA,0x68,0xC7,0xA4,0xEA,0x52,0xDB);


MIDL_DEFINE_GUID(IID, IID_ICorDebugVariableSymbol,0x707E8932,0x1163,0x48D9,0x8A,0x93,0xF5,0xB1,0xF4,0x80,0xFB,0xB7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugMemoryBuffer,0x677888B3,0xD160,0x4B8C,0xA7,0x3B,0xD7,0x9E,0x6A,0xAA,0x1D,0x13);


MIDL_DEFINE_GUID(IID, IID_ICorDebugMergedAssemblyRecord,0xFAA8637B,0x3BBE,0x4671,0x8E,0x26,0x3B,0x59,0x87,0x5B,0x92,0x2A);


MIDL_DEFINE_GUID(IID, IID_ICorDebugSymbolProvider,0x3948A999,0xFD8A,0x4C38,0xA7,0x08,0x8A,0x71,0xE9,0xB0,0x4D,0xBB);


MIDL_DEFINE_GUID(IID, IID_ICorDebugSymbolProvider2,0xF9801807,0x4764,0x4330,0x9E,0x67,0x4F,0x68,0x50,0x94,0x16,0x5E);


MIDL_DEFINE_GUID(IID, IID_ICorDebugVirtualUnwinder,0xF69126B7,0xC787,0x4F6B,0xAE,0x96,0xA5,0x69,0x78,0x6F,0xC6,0x70);


MIDL_DEFINE_GUID(IID, IID_ICorDebugDataTarget2,0x2eb364da,0x605b,0x4e8d,0xb3,0x33,0x33,0x94,0xc4,0x82,0x8d,0x41);


MIDL_DEFINE_GUID(IID, IID_ICorDebugLoadedModule,0x817F343A,0x6630,0x4578,0x96,0xC5,0xD1,0x1B,0xC0,0xEC,0x5E,0xE2);


MIDL_DEFINE_GUID(IID, IID_ICorDebugDataTarget3,0xD05E60C3,0x848C,0x4E7D,0x89,0x4E,0x62,0x33,0x20,0xFF,0x6A,0xFA);


MIDL_DEFINE_GUID(IID, IID_ICorDebugDataTarget4,0xE799DC06,0xE099,0x4713,0xBD,0xD9,0x90,0x6D,0x3C,0xC0,0x2C,0xF2);


MIDL_DEFINE_GUID(IID, IID_ICorDebugMutableDataTarget,0xA1B8A756,0x3CB6,0x4CCB,0x97,0x9F,0x3D,0xF9,0x99,0x67,0x3A,0x59);


MIDL_DEFINE_GUID(IID, IID_ICorDebugMetaDataLocator,0x7cef8ba9,0x2ef7,0x42bf,0x97,0x3f,0x41,0x71,0x47,0x4f,0x87,0xd9);


MIDL_DEFINE_GUID(IID, IID_ICorDebugManagedCallback,0x3d6f5f60,0x7538,0x11d3,0x8d,0x5b,0x00,0x10,0x4b,0x35,0xe7,0xef);


MIDL_DEFINE_GUID(IID, IID_ICorDebugManagedCallback3,0x264EA0FC,0x2591,0x49AA,0x86,0x8E,0x83,0x5E,0x65,0x15,0x32,0x3F);


MIDL_DEFINE_GUID(IID, IID_ICorDebugManagedCallback4,0x322911AE,0x16A5,0x49BA,0x84,0xA3,0xED,0x69,0x67,0x81,0x38,0xA3);


MIDL_DEFINE_GUID(IID, IID_ICorDebugManagedCallback2,0x250E5EEA,0xDB5C,0x4C76,0xB6,0xF3,0x8C,0x46,0xF1,0x2E,0x32,0x03);


MIDL_DEFINE_GUID(IID, IID_ICorDebugUnmanagedCallback,0x5263E909,0x8CB5,0x11d3,0xBD,0x2F,0x00,0x00,0xF8,0x08,0x49,0xBD);


MIDL_DEFINE_GUID(IID, IID_ICorDebug,0x3d6f5f61,0x7538,0x11d3,0x8d,0x5b,0x00,0x10,0x4b,0x35,0xe7,0xef);


MIDL_DEFINE_GUID(IID, IID_ICorDebugRemoteTarget,0xC3ED8383,0x5A49,0x4cf5,0xB4,0xB7,0x01,0x86,0x4D,0x9E,0x58,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugRemote,0xD5EBB8E2,0x7BBE,0x4c1d,0x98,0xA6,0xA3,0xC0,0x4C,0xBD,0xEF,0x64);


MIDL_DEFINE_GUID(IID, IID_ICorDebug2,0xECCCCF2E,0xB286,0x4b3e,0xA9,0x83,0x86,0x0A,0x87,0x93,0xD1,0x05);


MIDL_DEFINE_GUID(IID, IID_ICorDebugController,0x3d6f5f62,0x7538,0x11d3,0x8d,0x5b,0x00,0x10,0x4b,0x35,0xe7,0xef);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAppDomain,0x3d6f5f63,0x7538,0x11d3,0x8d,0x5b,0x00,0x10,0x4b,0x35,0xe7,0xef);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAppDomain2,0x096E81D5,0xECDA,0x4202,0x83,0xF5,0xC6,0x59,0x80,0xA9,0xEF,0x75);


MIDL_DEFINE_GUID(IID, IID_ICorDebugEnum,0xCC7BCB01,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugGuidToTypeEnum,0x6164D242,0x1015,0x4BD6,0x8C,0xBE,0xD0,0xDB,0xD4,0xB8,0x27,0x5A);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAppDomain3,0x8CB96A16,0xB588,0x42E2,0xB7,0x1C,0xDD,0x84,0x9F,0xC2,0xEC,0xCC);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAppDomain4,0xFB99CC40,0x83BE,0x4724,0xAB,0x3B,0x76,0x8E,0x79,0x6E,0xBA,0xC2);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAssembly,0xdf59507c,0xd47a,0x459e,0xbc,0xe2,0x64,0x27,0xea,0xc8,0xfd,0x06);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAssembly2,0x426d1f9e,0x6dd4,0x44c8,0xae,0xc7,0x26,0xcd,0xba,0xf4,0xe3,0x98);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAssembly3,0x76361AB2,0x8C86,0x4FE9,0x96,0xF2,0xF7,0x3D,0x88,0x43,0x57,0x0A);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHeapEnum,0x76D7DAB8,0xD044,0x11DF,0x9A,0x15,0x7E,0x29,0xDF,0xD7,0x20,0x85);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHeapSegmentEnum,0xA2FA0F8E,0xD045,0x11DF,0xAC,0x8E,0xCE,0x2A,0xDF,0xD7,0x20,0x85);


MIDL_DEFINE_GUID(IID, IID_ICorDebugGCReferenceEnum,0x7F3C24D3,0x7E1D,0x4245,0xAC,0x3A,0xF7,0x2F,0x88,0x59,0xC8,0x0C);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess,0x3d6f5f64,0x7538,0x11d3,0x8d,0x5b,0x00,0x10,0x4b,0x35,0xe7,0xef);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess2,0xAD1B3588,0x0EF0,0x4744,0xA4,0x96,0xAA,0x09,0xA9,0xF8,0x03,0x71);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess3,0x2EE06488,0xC0D4,0x42B1,0xB2,0x6D,0xF3,0x79,0x5E,0xF6,0x06,0xFB);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess5,0x21e9d9c0,0xfcb8,0x11df,0x8c,0xff,0x08,0x00,0x20,0x0c,0x9a,0x66);


MIDL_DEFINE_GUID(IID, IID_ICorDebugDebugEvent,0x41BD395D,0xDE99,0x48F1,0xBF,0x7A,0xCC,0x0F,0x44,0xA6,0xD2,0x81);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess6,0x11588775,0x7205,0x4CEB,0xA4,0x1A,0x93,0x75,0x3C,0x31,0x53,0xE9);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess7,0x9B2C54E4,0x119F,0x4D6F,0xB4,0x02,0x52,0x76,0x03,0x26,0x6D,0x69);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess8,0x2E6F28C1,0x85EB,0x4141,0x80,0xAD,0x0A,0x90,0x94,0x4B,0x96,0x39);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcess10,0x8F378F6F,0x1017,0x4461,0x98,0x90,0xEC,0xF6,0x4C,0x54,0x07,0x9F);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModuleDebugEvent,0x51A15E8D,0x9FFF,0x4864,0x9B,0x87,0xF4,0xFB,0xDE,0xA7,0x47,0xA2);


MIDL_DEFINE_GUID(IID, IID_ICorDebugExceptionDebugEvent,0xAF79EC94,0x4752,0x419C,0xA6,0x26,0x5F,0xB1,0xCC,0x1A,0x5A,0xB7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugBreakpoint,0xCC7BCAE8,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFunctionBreakpoint,0xCC7BCAE9,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModuleBreakpoint,0xCC7BCAEA,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugValueBreakpoint,0xCC7BCAEB,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStepper,0xCC7BCAEC,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStepper2,0xC5B6E9C3,0xE7D1,0x4a8e,0x87,0x3B,0x7F,0x04,0x7F,0x07,0x06,0xF7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugRegisterSet,0xCC7BCB0B,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugRegisterSet2,0x6DC7BA3F,0x89BA,0x4459,0x9E,0xC1,0x9D,0x60,0x93,0x7B,0x46,0x8D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugThread,0x938c6d66,0x7fb6,0x4f69,0xb3,0x89,0x42,0x5b,0x89,0x87,0x32,0x9b);


MIDL_DEFINE_GUID(IID, IID_ICorDebugThread2,0x2BD956D9,0x7B07,0x4bef,0x8A,0x98,0x12,0xAA,0x86,0x24,0x17,0xC5);


MIDL_DEFINE_GUID(IID, IID_ICorDebugThread3,0xF8544EC3,0x5E4E,0x46c7,0x8D,0x3E,0xA5,0x2B,0x84,0x05,0xB1,0xF5);


MIDL_DEFINE_GUID(IID, IID_ICorDebugThread4,0x1A1F204B,0x1C66,0x4637,0x82,0x3F,0x3E,0xE6,0xC7,0x44,0xA6,0x9C);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStackWalk,0xA0647DE9,0x55DE,0x4816,0x92,0x9C,0x38,0x52,0x71,0xC6,0x4C,0xF7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugChain,0xCC7BCAEE,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFrame,0xCC7BCAEF,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugInternalFrame,0xB92CC7F7,0x9D2D,0x45c4,0xBC,0x2B,0x62,0x1F,0xCC,0x9D,0xFB,0xF4);


MIDL_DEFINE_GUID(IID, IID_ICorDebugInternalFrame2,0xC0815BDC,0xCFAB,0x447e,0xA7,0x79,0xC1,0x16,0xB4,0x54,0xEB,0x5B);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILFrame,0x03E26311,0x4F76,0x11d3,0x88,0xC6,0x00,0x60,0x97,0x94,0x54,0x18);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILFrame2,0x5D88A994,0x6C30,0x479b,0x89,0x0F,0xBC,0xEF,0x88,0xB1,0x29,0xA5);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILFrame3,0x9A9E2ED6,0x04DF,0x4FE0,0xBB,0x50,0xCA,0xB6,0x41,0x26,0xAD,0x24);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILFrame4,0xAD914A30,0xC6D1,0x4AC5,0x9C,0x5E,0x57,0x7F,0x3B,0xAA,0x8A,0x45);


MIDL_DEFINE_GUID(IID, IID_ICorDebugNativeFrame,0x03E26314,0x4F76,0x11d3,0x88,0xC6,0x00,0x60,0x97,0x94,0x54,0x18);


MIDL_DEFINE_GUID(IID, IID_ICorDebugNativeFrame2,0x35389FF1,0x3684,0x4c55,0xA2,0xEE,0x21,0x0F,0x26,0xC6,0x0E,0x5E);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModule3,0x86F012BF,0xFF15,0x4372,0xBD,0x30,0xB6,0xF1,0x1C,0xAA,0xE1,0xDD);


MIDL_DEFINE_GUID(IID, IID_ICorDebugRuntimeUnwindableFrame,0x879CAC0A,0x4A53,0x4668,0xB8,0xE3,0xCB,0x84,0x73,0xCB,0x18,0x7F);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModule,0xdba2d8c1,0xe5c5,0x4069,0x8c,0x13,0x10,0xa7,0xc6,0xab,0xf4,0x3d);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModule2,0x7FCC5FB5,0x49C0,0x41de,0x99,0x38,0x3B,0x88,0xB5,0xB9,0xAD,0xD7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFunction,0xCC7BCAF3,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFunction2,0xEF0C490B,0x94C3,0x4e4d,0xB6,0x29,0xDD,0xC1,0x34,0xC5,0x32,0xD8);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFunction3,0x09B70F28,0xE465,0x482D,0x99,0xE0,0x81,0xA1,0x65,0xEB,0x05,0x32);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFunction4,0x72965963,0x34fd,0x46e9,0x94,0x34,0xb8,0x17,0xfe,0x6e,0x7f,0x43);


MIDL_DEFINE_GUID(IID, IID_ICorDebugCode,0xCC7BCAF4,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugCode2,0x5F696509,0x452F,0x4436,0xA3,0xFE,0x4D,0x11,0xFE,0x7E,0x23,0x47);


MIDL_DEFINE_GUID(IID, IID_ICorDebugCode3,0xD13D3E88,0xE1F2,0x4020,0xAA,0x1D,0x3D,0x16,0x2D,0xCB,0xE9,0x66);


MIDL_DEFINE_GUID(IID, IID_ICorDebugCode4,0x18221fa4,0x20cb,0x40fa,0xb1,0x9d,0x9f,0x91,0xc4,0xfa,0x8c,0x14);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILCode,0x598D46C2,0xC877,0x42A7,0x89,0xD2,0x3D,0x0C,0x7F,0x1C,0x12,0x64);


MIDL_DEFINE_GUID(IID, IID_ICorDebugILCode2,0x46586093,0xD3F5,0x4DB6,0xAC,0xDB,0x95,0x5B,0xCE,0x22,0x8C,0x15);


MIDL_DEFINE_GUID(IID, IID_ICorDebugClass,0xCC7BCAF5,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugClass2,0xB008EA8D,0x7AB1,0x43f7,0xBB,0x20,0xFB,0xB5,0xA0,0x40,0x38,0xAE);


MIDL_DEFINE_GUID(IID, IID_ICorDebugEval,0xCC7BCAF6,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugEval2,0xFB0D9CE7,0xBE66,0x4683,0x9D,0x32,0xA4,0x2A,0x04,0xE2,0xFD,0x91);


MIDL_DEFINE_GUID(IID, IID_ICorDebugValue,0xCC7BCAF7,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugValue2,0x5E0B54E7,0xD88A,0x4626,0x94,0x20,0xA6,0x91,0xE0,0xA7,0x8B,0x49);


MIDL_DEFINE_GUID(IID, IID_ICorDebugValue3,0x565005FC,0x0F8A,0x4F3E,0x9E,0xDB,0x83,0x10,0x2B,0x15,0x65,0x95);


MIDL_DEFINE_GUID(IID, IID_ICorDebugGenericValue,0xCC7BCAF8,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugReferenceValue,0xCC7BCAF9,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHeapValue,0xCC7BCAFA,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHeapValue2,0xE3AC4D6C,0x9CB7,0x43e6,0x96,0xCC,0xB2,0x15,0x40,0xE5,0x08,0x3C);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHeapValue3,0xA69ACAD8,0x2374,0x46e9,0x9F,0xF8,0xB1,0xF1,0x41,0x20,0xD2,0x96);


MIDL_DEFINE_GUID(IID, IID_ICorDebugObjectValue,0x18AD3D6E,0xB7D2,0x11d2,0xBD,0x04,0x00,0x00,0xF8,0x08,0x49,0xBD);


MIDL_DEFINE_GUID(IID, IID_ICorDebugObjectValue2,0x49E4A320,0x4A9B,0x4eca,0xB1,0x05,0x22,0x9F,0xB7,0xD5,0x00,0x9F);


MIDL_DEFINE_GUID(IID, IID_ICorDebugDelegateObjectValue,0x3AF70CC7,0x6047,0x47F6,0xA5,0xC5,0x09,0x0A,0x1A,0x62,0x26,0x38);


MIDL_DEFINE_GUID(IID, IID_ICorDebugBoxValue,0xCC7BCAFC,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStringValue,0xCC7BCAFD,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugArrayValue,0x0405B0DF,0xA660,0x11d2,0xBD,0x02,0x00,0x00,0xF8,0x08,0x49,0xBD);


MIDL_DEFINE_GUID(IID, IID_ICorDebugVariableHome,0x50847b8d,0xf43f,0x41b0,0x92,0x4c,0x63,0x83,0xa5,0xf2,0x27,0x8b);


MIDL_DEFINE_GUID(IID, IID_ICorDebugHandleValue,0x029596E8,0x276B,0x46a1,0x98,0x21,0x73,0x2E,0x96,0xBB,0xB0,0x0B);


MIDL_DEFINE_GUID(IID, IID_ICorDebugContext,0xCC7BCB00,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugComObjectValue,0x5F69C5E5,0x3E12,0x42DF,0xB3,0x71,0xF9,0xD7,0x61,0xD6,0xEE,0x24);


MIDL_DEFINE_GUID(IID, IID_ICorDebugObjectEnum,0xCC7BCB02,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugBreakpointEnum,0xCC7BCB03,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugStepperEnum,0xCC7BCB04,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugProcessEnum,0xCC7BCB05,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugThreadEnum,0xCC7BCB06,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugFrameEnum,0xCC7BCB07,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugChainEnum,0xCC7BCB08,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugModuleEnum,0xCC7BCB09,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugValueEnum,0xCC7BCB0A,0x8A68,0x11d2,0x98,0x3C,0x00,0x00,0xF8,0x08,0x34,0x2D);


MIDL_DEFINE_GUID(IID, IID_ICorDebugVariableHomeEnum,0xe76b7a57,0x4f7a,0x4309,0x85,0xa7,0x5d,0x91,0x8c,0x3d,0xea,0xf7);


MIDL_DEFINE_GUID(IID, IID_ICorDebugCodeEnum,0x55E96461,0x9645,0x45e4,0xA2,0xFF,0x03,0x67,0x87,0x7A,0xBC,0xDE);


MIDL_DEFINE_GUID(IID, IID_ICorDebugTypeEnum,0x10F27499,0x9DF2,0x43ce,0x83,0x33,0xA3,0x21,0xD7,0xC9,0x9C,0xB4);


MIDL_DEFINE_GUID(IID, IID_ICorDebugType,0xD613F0BB,0xACE1,0x4c19,0xBD,0x72,0xE4,0xC0,0x8D,0x5D,0xA7,0xF5);


MIDL_DEFINE_GUID(IID, IID_ICorDebugType2,0xe6e91d79,0x693d,0x48bc,0xb4,0x17,0x82,0x84,0xb4,0xf1,0x0f,0xb5);


MIDL_DEFINE_GUID(IID, IID_ICorDebugErrorInfoEnum,0xF0E18809,0x72B5,0x11d2,0x97,0x6F,0x00,0xA0,0xC9,0xB4,0xD5,0x0C);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAppDomainEnum,0x63ca1b24,0x4359,0x4883,0xbd,0x57,0x13,0xf8,0x15,0xf5,0x87,0x44);


MIDL_DEFINE_GUID(IID, IID_ICorDebugAssemblyEnum,0x4a2a1ec9,0x85ec,0x4bfb,0x9f,0x15,0xa8,0x9f,0xdf,0xe0,0xfe,0x83);


MIDL_DEFINE_GUID(IID, IID_ICorDebugBlockingObjectEnum,0x976A6278,0x134A,0x4a81,0x81,0xA3,0x8F,0x27,0x79,0x43,0xF4,0xC3);


MIDL_DEFINE_GUID(IID, IID_ICorDebugMDA,0xCC726F2F,0x1DB7,0x459b,0xB0,0xEC,0x05,0xF0,0x1D,0x84,0x1B,0x42);


MIDL_DEFINE_GUID(IID, IID_ICorDebugEditAndContinueErrorInfo,0x8D600D41,0xF4F6,0x4cb3,0xB7,0xEC,0x7B,0xD1,0x64,0x94,0x40,0x36);


MIDL_DEFINE_GUID(IID, IID_ICorDebugEditAndContinueSnapshot,0x6DC3FA01,0xD7CB,0x11d2,0x8A,0x95,0x00,0x80,0xC7,0x92,0xE5,0xD8);


MIDL_DEFINE_GUID(IID, IID_ICorDebugExceptionObjectCallStackEnum,0xED775530,0x4DC4,0x41F7,0x86,0xD0,0x9E,0x2D,0xEF,0x7D,0xFC,0x66);


MIDL_DEFINE_GUID(IID, IID_ICorDebugExceptionObjectValue,0xAE4CA65D,0x59DD,0x42A2,0x83,0xA5,0x57,0xE8,0xA0,0x8D,0x87,0x19);


MIDL_DEFINE_GUID(IID, LIBID_CORDBLib,0x53D13620,0xF417,0x11d1,0x97,0x62,0xA6,0x38,0x26,0xA4,0xF2,0x55);


MIDL_DEFINE_GUID(CLSID, CLSID_CorDebug,0x6fef44d0,0x39e7,0x4c77,0xbe,0x8e,0xc9,0xf8,0xcf,0x98,0x86,0x30);


MIDL_DEFINE_GUID(CLSID, CLSID_EmbeddedCLRCorDebug,0x211f1254,0xbc7e,0x4af5,0xb9,0xaa,0x06,0x73,0x08,0xd8,0x3d,0xd1);

#undef MIDL_DEFINE_GUID

#ifdef __cplusplus
}
#endif

BIND_UUID_OF(ICorDebugDataTarget)
BIND_UUID_OF(ICorDebugStaticFieldSymbol)
BIND_UUID_OF(ICorDebugInstanceFieldSymbol)
BIND_UUID_OF(ICorDebugVariableSymbol)
BIND_UUID_OF(ICorDebugMemoryBuffer)
BIND_UUID_OF(ICorDebugMergedAssemblyRecord)
BIND_UUID_OF(ICorDebugSymbolProvider)
BIND_UUID_OF(ICorDebugSymbolProvider2)
BIND_UUID_OF(ICorDebugVirtualUnwinder)
BIND_UUID_OF(ICorDebugDataTarget2)
BIND_UUID_OF(ICorDebugLoadedModule)
BIND_UUID_OF(ICorDebugDataTarget3)
BIND_UUID_OF(ICorDebugDataTarget4)
BIND_UUID_OF(ICorDebugMutableDataTarget)
BIND_UUID_OF(ICorDebugMetaDataLocator)
BIND_UUID_OF(ICorDebugManagedCallback)
BIND_UUID_OF(ICorDebugManagedCallback3)
BIND_UUID_OF(ICorDebugManagedCallback4)
BIND_UUID_OF(ICorDebugManagedCallback2)
BIND_UUID_OF(ICorDebugUnmanagedCallback)
BIND_UUID_OF(ICorDebug)
BIND_UUID_OF(ICorDebugRemoteTarget)
BIND_UUID_OF(ICorDebugRemote)
BIND_UUID_OF(ICorDebug2)
BIND_UUID_OF(ICorDebugController)
BIND_UUID_OF(ICorDebugAppDomain)
BIND_UUID_OF(ICorDebugAppDomain2)
BIND_UUID_OF(ICorDebugEnum)
BIND_UUID_OF(ICorDebugGuidToTypeEnum)
BIND_UUID_OF(ICorDebugAppDomain3)
BIND_UUID_OF(ICorDebugAppDomain4)
BIND_UUID_OF(ICorDebugAssembly)
BIND_UUID_OF(ICorDebugAssembly2)
BIND_UUID_OF(ICorDebugAssembly3)
BIND_UUID_OF(ICorDebugHeapEnum)
BIND_UUID_OF(ICorDebugHeapSegmentEnum)
BIND_UUID_OF(ICorDebugGCReferenceEnum)
BIND_UUID_OF(ICorDebugProcess)
BIND_UUID_OF(ICorDebugProcess2)
BIND_UUID_OF(ICorDebugProcess3)
BIND_UUID_OF(ICorDebugProcess5)
BIND_UUID_OF(ICorDebugDebugEvent)
BIND_UUID_OF(ICorDebugProcess6)
BIND_UUID_OF(ICorDebugProcess7)
BIND_UUID_OF(ICorDebugProcess8)
BIND_UUID_OF(ICorDebugProcess10)
BIND_UUID_OF(ICorDebugModuleDebugEvent)
BIND_UUID_OF(ICorDebugExceptionDebugEvent)
BIND_UUID_OF(ICorDebugBreakpoint)
BIND_UUID_OF(ICorDebugFunctionBreakpoint)
BIND_UUID_OF(ICorDebugModuleBreakpoint)
BIND_UUID_OF(ICorDebugValueBreakpoint)
BIND_UUID_OF(ICorDebugStepper)
BIND_UUID_OF(ICorDebugStepper2)
BIND_UUID_OF(ICorDebugRegisterSet)
BIND_UUID_OF(ICorDebugRegisterSet2)
BIND_UUID_OF(ICorDebugThread)
BIND_UUID_OF(ICorDebugThread2)
BIND_UUID_OF(ICorDebugThread3)
BIND_UUID_OF(ICorDebugThread4)
BIND_UUID_OF(ICorDebugStackWalk)
BIND_UUID_OF(ICorDebugChain)
BIND_UUID_OF(ICorDebugFrame)
BIND_UUID_OF(ICorDebugInternalFrame)
BIND_UUID_OF(ICorDebugInternalFrame2)
BIND_UUID_OF(ICorDebugILFrame)
BIND_UUID_OF(ICorDebugILFrame2)
BIND_UUID_OF(ICorDebugILFrame3)
BIND_UUID_OF(ICorDebugILFrame4)
BIND_UUID_OF(ICorDebugNativeFrame)
BIND_UUID_OF(ICorDebugNativeFrame2)
BIND_UUID_OF(ICorDebugModule3)
BIND_UUID_OF(ICorDebugRuntimeUnwindableFrame)
BIND_UUID_OF(ICorDebugModule)
BIND_UUID_OF(ICorDebugModule2)
BIND_UUID_OF(ICorDebugFunction)
BIND_UUID_OF(ICorDebugFunction2)
BIND_UUID_OF(ICorDebugFunction3)
BIND_UUID_OF(ICorDebugFunction4)
BIND_UUID_OF(ICorDebugCode)
BIND_UUID_OF(ICorDebugCode2)
BIND_UUID_OF(ICorDebugCode3)
BIND_UUID_OF(ICorDebugCode4)
BIND_UUID_OF(ICorDebugILCode)
BIND_UUID_OF(ICorDebugILCode2)
BIND_UUID_OF(ICorDebugClass)
BIND_UUID_OF(ICorDebugClass2)
BIND_UUID_OF(ICorDebugEval)
BIND_UUID_OF(ICorDebugEval2)
BIND_UUID_OF(ICorDebugValue)
BIND_UUID_OF(ICorDebugValue2)
BIND_UUID_OF(ICorDebugValue3)
BIND_UUID_OF(ICorDebugGenericValue)
BIND_UUID_OF(ICorDebugReferenceValue)
BIND_UUID_OF(ICorDebugHeapValue)
BIND_UUID_OF(ICorDebugHeapValue2)
BIND_UUID_OF(ICorDebugHeapValue3)
BIND_UUID_OF(ICorDebugObjectValue)
BIND_UUID_OF(ICorDebugObjectValue2)
BIND_UUID_OF(ICorDebugBoxValue)
BIND_UUID_OF(ICorDebugStringValue)
BIND_UUID_OF(ICorDebugArrayValue)
BIND_UUID_OF(ICorDebugVariableHome)
BIND_UUID_OF(ICorDebugHandleValue)
BIND_UUID_OF(ICorDebugContext)
BIND_UUID_OF(ICorDebugComObjectValue)
BIND_UUID_OF(ICorDebugObjectEnum)
BIND_UUID_OF(ICorDebugBreakpointEnum)
BIND_UUID_OF(ICorDebugStepperEnum)
BIND_UUID_OF(ICorDebugProcessEnum)
BIND_UUID_OF(ICorDebugThreadEnum)
BIND_UUID_OF(ICorDebugFrameEnum)
BIND_UUID_OF(ICorDebugChainEnum)
BIND_UUID_OF(ICorDebugModuleEnum)
BIND_UUID_OF(ICorDebugValueEnum)
BIND_UUID_OF(ICorDebugVariableHomeEnum)
BIND_UUID_OF(ICorDebugCodeEnum)
BIND_UUID_OF(ICorDebugTypeEnum)
BIND_UUID_OF(ICorDebugType)
BIND_UUID_OF(ICorDebugType2)
BIND_UUID_OF(ICorDebugErrorInfoEnum)
BIND_UUID_OF(ICorDebugAppDomainEnum)
BIND_UUID_OF(ICorDebugAssemblyEnum)
BIND_UUID_OF(ICorDebugBlockingObjectEnum)
BIND_UUID_OF(ICorDebugMDA)
BIND_UUID_OF(ICorDebugEditAndContinueErrorInfo)
BIND_UUID_OF(ICorDebugEditAndContinueSnapshot)
BIND_UUID_OF(ICorDebugExceptionObjectCallStackEnum)
BIND_UUID_OF(ICorDebugExceptionObjectValue)

