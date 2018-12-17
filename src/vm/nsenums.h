// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// NSENUMS.H -
//

//
// Defines NStruct-related enums
//

// NStruct Field Type's
//
// Columns:
//    Name            - name of enum
//    FieldMarshaler  - the FieldMarshaler_* subclass this NFT corresponds to.
//    Size            - the native size (in bytes) of the field.
//                      for some fields, this value cannot be computed
//                      without more information. if so, put a zero here
//                      and make sure CollectNStructFieldMetadata()
//                      has code to compute the size.
//    WinRTSupported  - true if the field type is supported in WinRT
//                      scenarios.
//
//    PS - Append new entries only at the end of the enum to avoid phone versioning break.
//         Name (COM+ - Native)   Size

DEFINE_NFT(NFT_NONE,                        FieldMarshaler,                    0,                      false)
                                                                               
DEFINE_NFT(NFT_STRINGUNI,                   FieldMarshaler_StringUni,          sizeof(LPVOID),         false)
DEFINE_NFT(NFT_STRINGANSI,                  FieldMarshaler_StringAnsi,         sizeof(LPVOID),         false)
DEFINE_NFT(NFT_FIXEDSTRINGUNI,              FieldMarshaler_FixedStringUni,     0,                      false)
DEFINE_NFT(NFT_FIXEDSTRINGANSI,             FieldMarshaler_FixedStringAnsi,    0,                      false)

DEFINE_NFT(NFT_FIXEDCHARARRAYANSI,          FieldMarshaler_FixedCharArrayAnsi, 0,                      false)
DEFINE_NFT(NFT_FIXEDARRAY,                  FieldMarshaler_FixedArray,         0,                      false)

DEFINE_NFT(NFT_DELEGATE,                    FieldMarshaler_Delegate,           sizeof(LPVOID),         false)

DEFINE_NFT(NFT_COPY1,                       FieldMarshaler_Copy1,              1,                      true)
DEFINE_NFT(NFT_COPY2,                       FieldMarshaler_Copy2,              2,                      true)
DEFINE_NFT(NFT_COPY4,                       FieldMarshaler_Copy4,              4,                      true)
DEFINE_NFT(NFT_COPY8,                       FieldMarshaler_Copy8,              8,                      true)

DEFINE_NFT(NFT_ANSICHAR,                    FieldMarshaler_Ansi,               1,                      false)
DEFINE_NFT(NFT_WINBOOL,                     FieldMarshaler_WinBool,            sizeof(BOOL),           false)

DEFINE_NFT(NFT_NESTEDLAYOUTCLASS,           FieldMarshaler_NestedLayoutClass,  0,                      false)
DEFINE_NFT(NFT_NESTEDVALUECLASS,            FieldMarshaler_NestedValueClass,   0,                      true)

DEFINE_NFT(NFT_CBOOL,                       FieldMarshaler_CBool,              1,                      true)

DEFINE_NFT(NFT_DATE,                        FieldMarshaler_Date,               sizeof(DATE),           false)
DEFINE_NFT(NFT_DECIMAL,                     FieldMarshaler_Decimal,            sizeof(DECIMAL),        false)

#ifdef FEATURE_COMINTEROP
DEFINE_NFT(NFT_INTERFACE,                   FieldMarshaler_Interface,          sizeof(IUnknown*),      false)
#endif

DEFINE_NFT(NFT_SAFEHANDLE,                  FieldMarshaler_SafeHandle,         sizeof(LPVOID),         false)
DEFINE_NFT(NFT_CRITICALHANDLE,              FieldMarshaler_CriticalHandle,     sizeof(LPVOID),         false)
DEFINE_NFT(NFT_BSTR,                        FieldMarshaler_BSTR,               sizeof(BSTR),           false)

#ifdef FEATURE_COMINTEROP
DEFINE_NFT(NFT_SAFEARRAY,                   FieldMarshaler_SafeArray,          0,                      false)
DEFINE_NFT(NFT_HSTRING,                     FieldMarshaler_HSTRING,            sizeof(HSTRING),        true)
DEFINE_NFT(NFT_VARIANT,                     FieldMarshaler_Variant,            sizeof(VARIANT),        false)
DEFINE_NFT(NFT_VARIANTBOOL,                 FieldMarshaler_VariantBool,        sizeof(VARIANT_BOOL),   false)
DEFINE_NFT(NFT_CURRENCY,                    FieldMarshaler_Currency,           sizeof(CURRENCY),       false)
DEFINE_NFT(NFT_DATETIMEOFFSET,              FieldMarshaler_DateTimeOffset,     sizeof(INT64),          true)
DEFINE_NFT(NFT_SYSTEMTYPE,                  FieldMarshaler_SystemType,         sizeof(TypeNameNative), true)  // System.Type -> Windows.UI.Xaml.Interop.TypeName
DEFINE_NFT(NFT_WINDOWSFOUNDATIONHRESULT,    FieldMarshaler_Exception,          sizeof(int),            true)  // Windows.Foundation.HResult is marshaled to System.Exception.
#endif // FEATURE_COMINTEROP
DEFINE_NFT(NFT_STRINGUTF8,                  FieldMarshaler_StringUtf8,         sizeof(LPVOID),         false)
DEFINE_NFT(NFT_ILLEGAL,                     FieldMarshaler_Illegal,            1,                      true)

#ifdef FEATURE_COMINTEROP
DEFINE_NFT(NFT_WINDOWSFOUNDATIONIREFERENCE, FieldMarshaler_Nullable,           sizeof(IUnknown*),      true)  // Windows.Foundation.IReference`1 is marshaled to System.Nullable`1.
#endif // FEATURE_COMINTEROP

// Append new entries only at the end of the enum to avoid phone versioning break.
