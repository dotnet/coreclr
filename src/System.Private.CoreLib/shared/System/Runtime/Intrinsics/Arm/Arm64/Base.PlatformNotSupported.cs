// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable IDE0060 // unused parameters
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.Arm.Arm64
{
    /// <summary>
    /// This class provides access to the Arm64 Base intrinsics
    ///
    /// These intrinsics are supported by all Arm64 CPUs
    /// </summary>
    [CLSCompliant(false)]
    public static class Base
    {
        public static bool IsSupported { [Intrinsic] get => false; }

        /// <summary>
        /// Scalar LeadingSignCount
        /// Corresponds to integer forms of ARM64 CLS
        /// </summary>
        public static int LeadingSignCount(int value) { throw new PlatformNotSupportedException(); }
        public static int LeadingSignCount(long value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Scalar LeadingZeroCount
        /// Corresponds to integer forms of ARM64 CLZ
        /// </summary>
        public static int LeadingZeroCount(int value) { throw new PlatformNotSupportedException(); }
        public static int LeadingZeroCount(uint value) { throw new PlatformNotSupportedException(); }
        public static int LeadingZeroCount(long value) { throw new PlatformNotSupportedException(); }
        public static int LeadingZeroCount(ulong value) { throw new PlatformNotSupportedException(); }
        /// <summary>
        /// Scalar ReverseBitOrder
        /// Corresponds to integer forms of ARM64 CLZ
        /// </summary>
        public static int ReverseBitOrder(int   value) { throw new PlatformNotSupportedException(); }
        public static int ReverseBitOrder(uint  value) { throw new PlatformNotSupportedException(); }
        public static int ReverseBitOrder(long  value) { throw new PlatformNotSupportedException(); }
        public static int ReverseBitOrder(ulong value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Scalar Absolute Compare GE
        ///
        /// |left| >= |right|
        ///
        /// Corresponds to scalar forms of ARM64 FACGE
        /// </summary>
        public static float  AbsoluteCompareGreatherThanOrEqual (float left , float  right) { throw new PlatformNotSupportedException(); }
        public static double AbsoluteCompareGreatherThanOrEqual (double left, double right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Scalar Absolute Compare GT
        ///
        /// |left| > |right|
        ///
        /// Corresponds to scalar forms of ARM64 FACGT
        /// </summary>
        public static float  AbsoluteCompareGreatherThan (float left , float  right) { throw new PlatformNotSupportedException(); }
        public static double AbsoluteCompareGreatherThan (double left, double right) { throw new PlatformNotSupportedException(); }


        /// <summary>
        /// Left Shift and Insert
        ///
        /// Corresponds to scalar forms of ARM64 SLI
        /// </summary>
        public static ulong LeftShiftAndInsert (ulong left, ulong right, uint shift) { throw new PlatformNotSupportedException(); }
        public static long  LeftShiftAndInsert (long left , long  right, uint shift) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Right Shift and Insert
        ///
        /// Corresponds to scalar forms of ARM64 SRI
        /// </summary>
        public static ulong RightShiftAndInsert (ulong left, ulong right, uint shift) { throw new PlatformNotSupportedException(); }
        public static long  RightShiftAndInsert (long left , long  right, uint shift) { throw new PlatformNotSupportedException(); }
    }
}
