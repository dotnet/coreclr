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
//                 Intrinsic ID                                     Function name                               ISA         Instructions (flt, dbl, i8, i16, i32, i64)                                                  ival
//  SSE Intrinsics
HARDWARE_INTRINSIC(SSE_IsSupported,                                 "get_IsSupported",                          SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Add,                                         "Add",                                      SSE,        {INS_addps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_AddScalar,                                   "AddScalar",                                SSE,        {INS_addss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_And,                                         "And",                                      SSE,        {INS_andps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_AndNot,                                      "AndNot",                                   SSE,        {INS_andnps,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareEqual,                                "CompareEqual",                             SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    0)
HARDWARE_INTRINSIC(SSE_CompareEqualOrderedScalar,                   "CompareEqualOrderedScalar",                SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareEqualScalar,                          "CompareEqualScalar",                       SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    0)
HARDWARE_INTRINSIC(SSE_CompareEqualUnorderedScalar,                 "CompareEqualUnorderedScalar",              SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareGreaterThan,                          "CompareGreaterThan",                       SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    6)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanOrderedScalar,             "CompareGreaterThanOrderedScalar",          SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanScalar,                    "CompareGreaterThanScalar",                 SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    6)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanUnorderedScalar,           "CompareGreaterThanUnorderedScalar",        SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanOrEqual,                   "CompareGreaterThanOrEqual",                SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    5)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanOrEqualOrderedScalar,      "CompareGreaterThanOrEqualOrderedScalar",   SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanOrEqualScalar,             "CompareGreaterThanOrEqualScalar",          SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    5)
HARDWARE_INTRINSIC(SSE_CompareGreaterThanOrEqualUnorderedScalar,    "CompareGreaterThanOrEqualUnorderedScalar", SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareLessThan,                             "CompareLessThan",                          SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    1)
HARDWARE_INTRINSIC(SSE_CompareLessThanOrderedScalar,                "CompareLessThanOrderedScalar",             SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareLessThanScalar,                       "CompareLessThanScalar",                    SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    1)
HARDWARE_INTRINSIC(SSE_CompareLessThanUnorderedScalar,              "CompareLessThanUnorderedScalar",           SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareLessThanOrEqual,                      "CompareLessThanOrEqual",                   SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    2)
HARDWARE_INTRINSIC(SSE_CompareLessThanOrEqualOrderedScalar,         "CompareLessThanOrEqualOrderedScalar",      SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareLessThanOrEqualScalar,                "CompareLessThanOrEqualScalar",             SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    2)
HARDWARE_INTRINSIC(SSE_CompareLessThanOrEqualUnorderedScalar,       "CompareLessThanOrEqualUnorderedScalar",    SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareNotEqual,                             "CompareNotEqual",                          SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    4)
HARDWARE_INTRINSIC(SSE_CompareNotEqualOrderedScalar,                "CompareNotEqualOrderedScalar",             SSE,        {INS_comiss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareNotEqualScalar,                       "CompareNotEqualScalar",                    SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    4)
HARDWARE_INTRINSIC(SSE_CompareNotEqualUnorderedScalar,              "CompareNotEqualUnorderedScalar",           SSE,        {INS_ucomiss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_CompareNotGreaterThan,                       "CompareNotGreaterThan",                    SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    2)
HARDWARE_INTRINSIC(SSE_CompareNotGreaterThanScalar,                 "CompareNotGreaterThanScalar",              SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    2)
HARDWARE_INTRINSIC(SSE_CompareNotGreaterThanOrEqual,                "CompareNotGreaterThanOrEqual",             SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    1)
HARDWARE_INTRINSIC(SSE_CompareNotGreaterThanOrEqualScalar,          "CompareNotGreaterThanOrEqualScalar",       SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    1)
HARDWARE_INTRINSIC(SSE_CompareNotLessThan,                          "CompareNotLessThan",                       SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    5)
HARDWARE_INTRINSIC(SSE_CompareNotLessThanScalar,                    "CompareNotLessThanScalar",                 SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    5)
HARDWARE_INTRINSIC(SSE_CompareNotLessThanOrEqual,                   "CompareNotLessThanOrEqual",                SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    6)
HARDWARE_INTRINSIC(SSE_CompareNotLessThanOrEqualScalar,             "CompareNotLessThanOrEqualScalar",          SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    6)
HARDWARE_INTRINSIC(SSE_CompareOrdered,                              "CompareOrdered",                           SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    7)
HARDWARE_INTRINSIC(SSE_CompareOrderedScalar,                        "CompareOrderedScalar",                     SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    7)
HARDWARE_INTRINSIC(SSE_CompareUnordered,                            "CompareUnordered",                         SSE,        {INS_cmpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    3)
HARDWARE_INTRINSIC(SSE_CompareUnorderedScalar,                      "CompareUnorderedScalar",                   SSE,        {INS_cmpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},    3)
HARDWARE_INTRINSIC(SSE_ConvertToInt32,                              "ConvertToInt32",                           SSE,        {INS_cvtss2si,  INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ConvertToInt64,                              "ConvertToInt64",                           SSE,        {INS_cvtss2si,  INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ConvertToSingle,                             "ConvertToSingle",                          SSE,        {INS_movss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ConvertToVector128SingleScalar,              "ConvertToVector128SingleScalar",           SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_cvtsi2ss,  INS_cvtsi2ss},  -1)
HARDWARE_INTRINSIC(SSE_ConvertToInt32WithTruncation,                "ConvertToInt32WithTruncation",             SSE,        {INS_cvttss2si, INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ConvertToInt64WithTruncation,                "ConvertToInt64WithTruncation",             SSE,        {INS_cvttss2si, INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Divide,                                      "Divide",                                   SSE,        {INS_divps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_DivideScalar,                                "DivideScalar",                             SSE,        {INS_divss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Load,                                        "Load",                                     SSE,        {INS_movups,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_LoadAligned,                                 "LoadAligned",                              SSE,        {INS_movaps,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_LoadScalar,                                  "LoadScalar",                               SSE,        {INS_movss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Max,                                         "Max",                                      SSE,        {INS_maxps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MaxScalar,                                   "MaxScalar",                                SSE,        {INS_maxss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Min,                                         "Min",                                      SSE,        {INS_minps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MinScalar,                                   "MinScalar",                                SSE,        {INS_minss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MoveHighToLow,                               "MoveHighToLow",                            SSE,        {INS_movhlps,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MoveLowToHigh,                               "MoveLowToHigh",                            SSE,        {INS_movlhps,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MoveScalar,                                  "MoveScalar",                               SSE,        {INS_movss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Multiply,                                    "Multiply",                                 SSE,        {INS_mulps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_MultiplyScalar,                              "MultiplyScalar",                           SSE,        {INS_mulss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Or,                                          "Or",                                       SSE,        {INS_orps,      INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Reciprocal,                                  "Reciprocal",                               SSE,        {INS_rcpps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ReciprocalScalar,                            "ReciprocalScalar",                         SSE,        {INS_rcpss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ReciprocalSqrt,                              "ReciprocalSqrt",                           SSE,        {INS_rsqrtps,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_ReciprocalSqrtScalar,                        "ReciprocalSqrtScalar",                     SSE,        {INS_rsqrtss,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Set,                                         "Set",                                      SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_SetAll,                                      "Set1",                                     SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_SetScalar,                                   "SetScalar",                                SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_SetZero,                                     "SetZero",                                  SSE,        {INS_xorps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Shuffle,                                     "Shuffle",                                  SSE,        {INS_shufps,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Sqrt,                                        "Sqrt",                                     SSE,        {INS_sqrtps,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_SqrtScalar,                                  "SqrtScalar",                               SSE,        {INS_sqrtss,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_StaticCast,                                  "StaticCast",                               SSE,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Store,                                       "Store",                                    SSE,        {INS_movups,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_StoreAligned,                                "StoreAligned",                             SSE,        {INS_movaps,    INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_StoreAlignedNonTemporal,                     "StoreAlignedNonTemporal",                  SSE,        {INS_movntps,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_StoreScalar,                                 "StoreScalar",                              SSE,        {INS_movss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Subtract,                                    "Subtract",                                 SSE,        {INS_subps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_SubtractScalar,                              "SubtractScalar",                           SSE,        {INS_subss,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_UnpackHigh,                                  "UnpackHigh",                               SSE,        {INS_unpckhps,  INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_UnpackLow,                                   "UnpackLow",                                SSE,        {INS_unpcklps,  INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE_Xor,                                         "Xor",                                      SSE,        {INS_xorps,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  SSE2 Intrinsics
HARDWARE_INTRINSIC(SSE2_IsSupported,                                "get_IsSupported",                          SSE2,       {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE2_Add,                                        "Add",                                      SSE2,       {INS_invalid,   INS_addpd,     INS_paddb,     INS_paddw,     INS_paddd,     INS_paddq},     -1)

//  SSE3 Intrinsics
HARDWARE_INTRINSIC(SSE3_IsSupported,                                "get_IsSupported",                          SSE3,       {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  SSSE3 Intrinsics
HARDWARE_INTRINSIC(SSSE3_IsSupported,                               "get_IsSupported",                          SSSE3,      {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  SSE41 Intrinsics
HARDWARE_INTRINSIC(SSE41_IsSupported,                               "get_IsSupported",                          SSE41,      {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  SSE42 Intrinsics
HARDWARE_INTRINSIC(SSE42_IsSupported,                               "get_IsSupported",                          SSE42,      {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(SSE42_Crc32,                                     "Crc32",                                    SSE42,      {INS_invalid,   INS_invalid,   INS_crc32,     INS_crc32,     INS_crc32,     INS_crc32},     -1)

//  AVX Intrinsics
HARDWARE_INTRINSIC(AVX_IsSupported,                                 "get_IsSupported",                          AVX,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(AVX_Add,                                         "Add",                                      AVX,        {INS_addps,     INS_addpd,     INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  AVX2 Intrinsics
HARDWARE_INTRINSIC(AVX2_IsSupported,                                "get_IsSupported",                          AVX2,       {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(AVX2_Add,                                        "Add",                                      AVX2,       {INS_invalid,   INS_invalid,   INS_paddb,     INS_paddw,     INS_paddd,     INS_paddq},     -1)

//  AES Intrinsics
HARDWARE_INTRINSIC(AES_IsSupported,                                 "get_IsSupported",                          AES,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  BMI1 Intrinsics
HARDWARE_INTRINSIC(BMI1_IsSupported,                                "get_IsSupported",                          BMI1,       {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  BMI2 Intrinsics
HARDWARE_INTRINSIC(BMI2_IsSupported,                                "get_IsSupported",                          BMI2,       {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  FMA Intrinsics
HARDWARE_INTRINSIC(FMA_IsSupported,                                 "get_IsSupported",                          FMA,        {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  LZCNT Intrinsics
HARDWARE_INTRINSIC(LZCNT_IsSupported,                               "get_IsSupported",                          LZCNT,      {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(LZCNT_LeadingZeroCount,                          "LeadingZeroCount",                         LZCNT,      {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_lzcnt,     INS_lzcnt},     -1)

//  PCLMULQDQ Intrinsics
HARDWARE_INTRINSIC(PCLMULQDQ_IsSupported,                           "get_IsSupported",                          PCLMULQDQ,  {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)

//  POPCNT Intrinsics
HARDWARE_INTRINSIC(POPCNT_IsSupported,                              "get_IsSupported",                          POPCNT,     {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid},   -1)
HARDWARE_INTRINSIC(POPCNT_PopCount,                                 "PopCount",                                 POPCNT,     {INS_invalid,   INS_invalid,   INS_invalid,   INS_invalid,   INS_popcnt,    INS_popcnt},    -1)
#endif // FEATURE_HW_INTRINSICS

#undef HARDWARE_INTRINSIC

// clang-format on
