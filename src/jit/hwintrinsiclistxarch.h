// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************/
#ifndef HARDWARE_INTRINSIC
#error Define HARDWARE_INTRINSIC before including this file
#endif
/*****************************************************************************/

// clang-format off

#if FEATURE_HW_INTRINSICS
//                 Intrinsic ID                         Function name                   ISA             Instructions (flt, dbl, i8, i16, i32, i64)                                          ival
//  SSE Intrinsics 
HARDWARE_INTRINSIC(SSE_IsSupported,                     "get_IsSupported",              SSE,            {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(SSE_Add,                             "Add",                          SSE,            {INS_addps,   INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  SSE2 Intrinsics 
HARDWARE_INTRINSIC(SSE2_IsSupported,                    "get_IsSupported",              SSE2,           {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(SSE2_Add,                            "Add",                          SSE2,           {INS_invalid, INS_addpd,   INS_paddb,   INS_paddw,   INS_paddd,   INS_paddq},       -1)

//  SSE3 Intrinsics 
HARDWARE_INTRINSIC(SSE3_IsSupported,                    "get_IsSupported",              SSE3,           {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  SSSE3 Intrinsics 
HARDWARE_INTRINSIC(SSSE3_IsSupported,                   "get_IsSupported",              SSSE3,          {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  SSE41 Intrinsics 
HARDWARE_INTRINSIC(SSE41_IsSupported,                   "get_IsSupported",              SSE41,          {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  SSE42 Intrinsics 
HARDWARE_INTRINSIC(SSE42_IsSupported,                   "get_IsSupported",              SSE42,          {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(SSE42_Crc32,                         "Crc32",                        SSE42,          {INS_invalid, INS_invalid, INS_crc32,   INS_crc32,   INS_crc32,   INS_crc32},       -1)

//  AVX Intrinsics 
HARDWARE_INTRINSIC(AVX_IsSupported,                     "get_IsSupported",              AVX,            {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(AVX_Add,                             "Add",                          AVX,            {INS_addps,   INS_addpd,   INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  AVX2 Intrinsics 
HARDWARE_INTRINSIC(AVX2_IsSupported,                    "get_IsSupported",              AVX2,           {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(AVX2_Add,                            "Add",                          AVX2,           {INS_invalid, INS_invalid, INS_paddb,   INS_paddw,   INS_paddd,   INS_paddq},       -1)

//  AES Intrinsics 
HARDWARE_INTRINSIC(AES_IsSupported,                     "get_IsSupported",              AES,            {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  BMI1 Intrinsics 
HARDWARE_INTRINSIC(BMI1_IsSupported,                    "get_IsSupported",              BMI1,           {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  BMI2 Intrinsics 
HARDWARE_INTRINSIC(BMI2_IsSupported,                    "get_IsSupported",              BMI2,           {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  FMA Intrinsics 
HARDWARE_INTRINSIC(FMA_IsSupported,                     "get_IsSupported",              FMA,            {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  LZCNT Intrinsics 
HARDWARE_INTRINSIC(LZCNT_IsSupported,                   "get_IsSupported",              LZCNT,          {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(LZCNT_LeadingZeroCount,              "LeadingZeroCount",             LZCNT,          {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_lzcnt,   INS_lzcnt},       -1)

//  PCLMULQDQ Intrinsics 
HARDWARE_INTRINSIC(PCLMULQDQ_IsSupported,               "get_IsSupported",              PCLMULQDQ,      {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)

//  POPCNT Intrinsics 
HARDWARE_INTRINSIC(POPCNT_IsSupported,                  "get_IsSupported",              POPCNT,         {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_invalid},     -1)
HARDWARE_INTRINSIC(POPCNT_PopCount,                     "PopCount",                     POPCNT,         {INS_invalid, INS_invalid, INS_invalid, INS_invalid, INS_popcnt,  INS_popcnt},      -1)
#endif // FEATURE_HW_INTRINSICS

#undef HARDWARE_INTRINSIC

// clang-format on
