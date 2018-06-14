// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;

namespace System.Runtime.Intrinsics.X86
{
    public enum FloatComparisonMode : byte
    {
        /// <summary>
        /// _CMP_EQ_OQ
        /// </summary>
        EqualOrderedNonSignaling = 0,

        /// <summary>
        /// _CMP_LT_OS
        /// </summary>
        LessThanOrderedSignaling = 1,

        /// <summary>
        /// _CMP_LE_OS
        /// </summary>
        LessThanOrEqualOrderedSignaling = 2,

        /// <summary>
        /// _CMP_UNORD_Q
        /// </summary>
        UnorderedNonSignaling = 3,

        /// <summary>
        /// _CMP_NEQ_UQ
        /// </summary>
        NotEqualUnorderedNonSignaling = 4,

        /// <summary>
        /// _CMP_NLT_US
        /// </summary>
        NotLessThanUnorderedSignaling = 5,

        /// <summary>
        /// _CMP_NLE_US
        /// </summary>
        NotLessThanOrEqualUnorderedSignaling = 6,

        /// <summary>
        /// _CMP_ORD_Q
        /// </summary>
        OrderedNonSignaling = 7,

        /// <summary>
        /// _CMP_EQ_UQ
        /// </summary>
        EqualUnorderedNonSignaling = 8,

        /// <summary>
        /// _CMP_NGE_US
        /// </summary>
        NotGreaterThanOrEqualUnorderedSignaling = 9,

        /// <summary>
        /// _CMP_NGT_US
        /// </summary>
        NotGreaterThanUnorderedSignaling = 10,

        /// <summary>
        /// _CMP_FALSE_OQ
        /// </summary>
        FalseOrderedNonSignaling = 11,

        /// <summary>
        /// _CMP_NEQ_OQ
        /// </summary>
        NotEqualOrderedNonSignaling = 12,

        /// <summary>
        /// _CMP_GE_OS
        /// </summary>
        GreaterThanOrEqualOrderedSignaling = 13,

        /// <summary>
        /// _CMP_GT_OS
        /// </summary>
        GreaterThanOrderedSignaling = 14,

        /// <summary>
        /// _CMP_TRUE_UQ
        /// </summary>
        TrueUnorderedNonSignaling = 15,

        /// <summary>
        /// _CMP_EQ_OS
        /// </summary>
        EqualOrderedSignaling = 16,

        /// <summary>
        /// _CMP_LT_OQ
        /// </summary>
        LessThanOrderedNonSignaling = 17,
        
        /// <summary>
        /// _CMP_LE_OQ
        /// </summary>
        LessThanOrEqualOrderedNonSignaling = 18,

        /// <summary>
        /// _CMP_UNORD_S
        /// </summary>
        UnorderedSignaling = 19,

        /// <summary>
        /// _CMP_NEQ_US
        /// </summary>
        NotEqualUnorderedSignaling = 20,

        /// <summary>
        /// _CMP_NLT_UQ
        /// </summary>
        NotLessThanUnorderedNonSignaling = 21,

        /// <summary>
        /// _CMP_NLE_UQ
        /// </summary>
        NotLessThanOrEqualUnorderedNonSignaling = 22,

        /// <summary>
        /// _CMP_ORD_S
        /// </summary>
        OrderedSignaling = 23,

        /// <summary>
        /// _CMP_EQ_US
        /// </summary>
        EqualUnorderedSignaling = 24,

        /// <summary>
        /// _CMP_NGE_UQ 
        /// </summary>
        NotGreaterThanOrEqualUnorderedNonSignaling = 25,

        /// <summary>
        /// _CMP_NGT_UQ 
        /// </summary>
        NotGreaterThanUnorderedNonSignaling = 26,

        /// <summary>
        /// _CMP_FALSE_OS 
        /// </summary>
        FalseOrderedSignaling = 27,

        /// <summary>
        /// _CMP_NEQ_OS
        /// </summary>
        NotEqualOrderedSignaling = 28,

        /// <summary>
        /// _CMP_GE_OQ
        /// </summary>
        GreaterThanOrEqualOrderedNonSignaling = 29,

        /// <summary>
        /// _CMP_GT_OQ
        /// </summary>
        GreaterThanOrderedNonSignaling = 30,

        /// <summary>
        /// _CMP_TRUE_US
        /// </summary>
        TrueUnorderedSignaling = 31,
    }

    [Flags]
    public enum StringComparisonMode : byte {
        /// <summary>
        /// _SIDD_CMP_EQUAL_ANY
        /// </summary>
        EqualAny = 0x00,

        /// <summary>
        /// _SIDD_CMP_RANGES
        /// </summary>
        Ranges = 0x04,

        /// <summary>
        /// _SIDD_CMP_EQUAL_EACH
        /// </summary>
        EqualEach = 0x08,

        /// <summary>
        /// _SIDD_CMP_EQUAL_ORDERED
        /// </summary>
        EqualOrdered = 0x0c,

        /// <summary>
        /// _SIDD_NEGATIVE_POLARITY
        /// </summary>
        NegativeResult = 0x10,

        /// <summary>
        /// _SIDD_MASKED_NEGATIVE_POLARITY
        /// </summary>
        NegativeUsefulResult = 0x30,

        /// <summary>
        /// _SIDD_LEAST_SIGNIFICANT
        /// </summary>
        LeastSignificant = 0x00,

        /// <summary>
        /// _SIDD_MOST_SIGNIFICANT
        /// </summary>
        MostSignificant = 0x40,

        /// <summary>
        /// _SIDD_BIT_MASK
        /// </summary>
        BitMask = 0x00,

        /// <summary>
        /// _SIDD_UNIT_MASK
        /// </summary>
        UnitMask = 0x40,
    }
}
