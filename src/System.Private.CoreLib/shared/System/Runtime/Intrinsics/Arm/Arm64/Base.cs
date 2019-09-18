// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.Arm.Arm64
{
    /// <summary>
    /// This class provides access to the Arm64 Base intrinsics
    ///
    /// These intrinsics are supported by all Arm64 CPUs
    /// </summary>
    [Intrinsic]
    [CLSCompliant(false)]
    public static class Base
    {
        public static bool IsSupported { get { return IsSupported; }}

        /// <summary>
        /// Scalar LeadingSignCount
        /// Corresponds to integer forms of ARM64 CLS
        /// </summary>
        public static int LeadingSignCount(int  value) => LeadingSignCount(value);
        public static int LeadingSignCount(long value) => LeadingSignCount(value);

        /// <summary>
        /// Scalar LeadingZeroCount
        /// Corresponds to integer forms of ARM64 CLZ
        /// </summary>
        public static int LeadingZeroCount(int   value) => LeadingZeroCount(value);
        public static int LeadingZeroCount(uint  value) => LeadingZeroCount(value);
        public static int LeadingZeroCount(long  value) => LeadingZeroCount(value);
        public static int LeadingZeroCount(ulong value) => LeadingZeroCount(value);

        /// <summary>
        /// Scalar ReverseBitOrder
        /// Corresponds to integer forms of ARM64 RBIT
        /// </summary>
        public static int ReverseBitOrder(int   value) => ReverseBitOrder(value);
        public static int ReverseBitOrder(uint  value) => ReverseBitOrder(value);
        public static int ReverseBitOrder(long  value) => ReverseBitOrder(value);
        public static int ReverseBitOrder(ulong value) => ReverseBitOrder(value);

        /// <summary>
        /// Scalar Absolute Compare GE
        ///
        /// |left| >= |right|
        ///
        /// Corresponds to vector forms of ARM64 FACGE
        /// </summary>
        public static float  AbsoluteCompareGreatherThanOrEqual (float left , float  right) => AbsoluteCompareGreatherThanOrEqual(left, right);
        public static double AbsoluteCompareGreatherThanOrEqual (double left, double right) => AbsoluteCompareGreatherThanOrEqual(left, right);

        /// <summary>
        /// Scalar Absolute Compare GT
        ///
        /// |left| > |right|
        ///
        /// Corresponds to vector forms of ARM64 FACGT
        /// </summary>
        public static float  AbsoluteCompareGreatherThan (float left , float  right) => AbsoluteCompareGreatherThan(left, right);
        public static double AbsoluteCompareGreatherThan (double left, double right) => AbsoluteCompareGreatherThan(left, right);

        /// <summary>
        /// Left Shift and Insert
        ///
        /// Corresponds to scalar forms of ARM64 SLI
        /// </summary>
        public static ulong LeftShiftAndInsert (ulong left, ulong right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static long  LeftShiftAndInsert (long left , long  right, uint shift) => LeftShiftAndInsert(left, right, shift);

        /// <summary>
        /// Right Shift and Insert
        ///
        /// Corresponds to scalar forms of ARM64 SRI
        /// </summary>
        public static ulong RightShiftAndInsert (ulong left, ulong right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static long  RightShiftAndInsert (long left , long  right, uint shift) => RightShiftAndInsert(left, right, shift);
    }
}
