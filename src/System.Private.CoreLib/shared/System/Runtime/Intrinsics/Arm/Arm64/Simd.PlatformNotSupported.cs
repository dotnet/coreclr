// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable IDE0060 // unused parameters
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.Arm.Arm64
{
    /// <summary>
    /// This class provides access to the Arm64 AdvSIMD intrinsics
    ///
    /// Arm64 CPU indicate support for this feature by setting
    /// ID_AA64PFR0_EL1.AdvSIMD == 0 or better.
    /// </summary>
    [CLSCompliant(false)]
    public static class Simd
    {
        /// <summary>
        /// IsSupported property indicates whether any method provided
        /// by this class is supported by the current runtime.
        /// </summary>
        public static bool IsSupported { [Intrinsic] get => false; }

        /// <summary>
        /// Vector abs
        /// Corresponds to vector forms of ARM64 ABS &amp; FABS
        /// </summary>
        public static Vector64<byte> Abs(Vector64<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<ushort> Abs(Vector64<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> Abs(Vector64<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> Abs(Vector64<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> Abs(Vector128<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<ushort> Abs(Vector128<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> Abs(Vector128<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> Abs(Vector128<long> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Abs(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Abs(Vector128<double> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector add
        /// Corresponds to vector forms of ARM64 ADD &amp; FADD
        /// </summary>
        public static Vector64<T> Add<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector and
        /// Corresponds to vector forms of ARM64 AND
        /// </summary>
        public static Vector64<T> And<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> And<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector and not
        /// Corresponds to vector forms of ARM64 BIC
        /// </summary>
        public static Vector64<T> AndNot<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector BitwiseSelect
        /// For each bit in the vector result[bit] = sel[bit] ? left[bit] : right[bit]
        /// Corresponds to vector forms of ARM64 BSL (Also BIF &amp; BIT)
        /// </summary>
        public static Vector64<T> BitwiseSelect<T>(Vector64<T> sel, Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> BitwiseSelect<T>(Vector128<T> sel, Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareEqual
        /// For each element result[elem] = (left[elem] == right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMEQ &amp; FCMEQ
        /// </summary>
        public static Vector64<T> CompareEqual<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareEqualZero
        /// For each element result[elem] = (left[elem] == 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMEQ &amp; FCMEQ
        /// </summary>
        public static Vector64<T> CompareEqualZero<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareEqualZero<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareGreaterThan
        /// For each element result[elem] = (left[elem] > right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT/CMHI &amp; FCMGT
        /// </summary>
        public static Vector64<T> CompareGreaterThan<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareGreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareGreaterThanZero
        /// For each element result[elem] = (left[elem] > 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T> CompareGreaterThanZero<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareGreaterThanZero<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareGreaterThanOrEqual
        /// For each element result[elem] = (left[elem] >= right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGE/CMHS &amp; FCMGE
        /// </summary>
        public static Vector64<T> CompareGreaterThanOrEqual<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareGreaterThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareGreaterThanOrEqualZero
        /// For each element result[elem] = (left[elem] >= 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGE &amp; FCMGE
        /// </summary>
        public static Vector64<T> CompareGreaterThanOrEqualZero<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareGreaterThanOrEqualZero<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareLessThanZero
        /// For each element result[elem] = (left[elem] &lt; 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T> CompareLessThanZero<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareLessThanZero<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareLessThanOrEqualZero
        /// For each element result[elem] = (left[elem] &lt; 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T> CompareLessThanOrEqualZero<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareLessThanOrEqualZero<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector CompareTest
        /// For each element result[elem] = (left[elem] &amp; right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMTST
        /// </summary>
        public static Vector64<T> CompareTest<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> CompareTest<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// TBD Convert...

        /// <summary>
        /// Vector Divide
        /// Corresponds to vector forms of ARM64 FDIV
        /// </summary>
        public static Vector64<float> Divide(Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Divide(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Divide(Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector extract item
        ///
        /// result = vector[index]
        ///
        /// Note: In order to be inlined, index must be a JIT time const expression which can be used to
        /// populate the literal immediate field.  Use of a non constant will result in generation of a switch table
        ///
        /// Corresponds to vector forms of ARM64 MOV
        /// </summary>
        public static T Extract<T>(Vector64<T> vector, byte index) where T : struct { throw new PlatformNotSupportedException(); }
        public static T Extract<T>(Vector128<T> vector, byte index) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector insert item
        ///
        /// result = vector;
        /// result[index] = data;
        ///
        /// Note: In order to be inlined, index must be a JIT time const expression which can be used to
        /// populate the literal immediate field.  Use of a non constant will result in generation of a switch table
        ///
        /// Corresponds to vector forms of ARM64 INS
        /// </summary>
        public static Vector64<T> Insert<T>(Vector64<T> vector, byte index, T data) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Insert<T>(Vector128<T> vector, byte index, T data) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector LeadingSignCount
        /// Corresponds to vector forms of ARM64 CLS
        /// </summary>
        public static Vector64<sbyte> LeadingSignCount(Vector64<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> LeadingSignCount(Vector64<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> LeadingSignCount(Vector64<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> LeadingSignCount(Vector128<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> LeadingSignCount(Vector128<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> LeadingSignCount(Vector128<int> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector LeadingZeroCount
        /// Corresponds to vector forms of ARM64 CLZ
        /// </summary>
        public static Vector64<byte> LeadingZeroCount(Vector64<byte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> LeadingZeroCount(Vector64<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<ushort> LeadingZeroCount(Vector64<ushort> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> LeadingZeroCount(Vector64<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> LeadingZeroCount(Vector64<uint> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> LeadingZeroCount(Vector64<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> LeadingZeroCount(Vector128<byte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> LeadingZeroCount(Vector128<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<ushort> LeadingZeroCount(Vector128<ushort> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> LeadingZeroCount(Vector128<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> LeadingZeroCount(Vector128<uint> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> LeadingZeroCount(Vector128<int> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector max
        /// Corresponds to vector forms of ARM64 SMAX, UMAX &amp; FMAX
        /// </summary>
        public static Vector64<byte> Max(Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> Max(Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<ushort> Max(Vector64<ushort> left, Vector64<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> Max(Vector64<short> left, Vector64<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> Max(Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> Max(Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> Max(Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> Max(Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> Max(Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ushort> Max(Vector128<ushort> left, Vector128<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> Max(Vector128<short> left, Vector128<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> Max(Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> Max(Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Max(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Max(Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector min
        /// Corresponds to vector forms of ARM64 SMIN, UMIN &amp; FMIN
        /// </summary>
        public static Vector64<byte> Min(Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> Min(Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<ushort> Min(Vector64<ushort> left, Vector64<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> Min(Vector64<short> left, Vector64<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> Min(Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> Min(Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> Min(Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> Min(Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> Min(Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ushort> Min(Vector128<ushort> left, Vector128<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> Min(Vector128<short> left, Vector128<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> Min(Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> Min(Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Min(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Min(Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// TBD MOV, FMOV

        /// <summary>
        /// Vector multiply
        ///
        /// For each element result[elem] = left[elem] * right[elem]
        ///
        /// Corresponds to vector forms of ARM64 MUL &amp; FMUL
        /// </summary>
        public static Vector64<byte> Multiply(Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> Multiply(Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<ushort> Multiply(Vector64<ushort> left, Vector64<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> Multiply(Vector64<short> left, Vector64<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> Multiply(Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> Multiply(Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> Multiply(Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> Multiply(Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> Multiply(Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ushort> Multiply(Vector128<ushort> left, Vector128<ushort> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> Multiply(Vector128<short> left, Vector128<short> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> Multiply(Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> Multiply(Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Multiply(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Multiply(Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector negate
        /// Corresponds to vector forms of ARM64 NEG &amp; FNEG
        /// </summary>
        public static Vector64<sbyte> Negate(Vector64<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<short> Negate(Vector64<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> Negate(Vector64<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> Negate(Vector64<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> Negate(Vector128<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<short> Negate(Vector128<short> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> Negate(Vector128<int> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> Negate(Vector128<long> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Negate(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Negate(Vector128<double> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector not
        /// Corresponds to vector forms of ARM64 NOT
        /// </summary>
        public static Vector64<T> Not<T>(Vector64<T> value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Not<T>(Vector128<T> value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector or
        /// Corresponds to vector forms of ARM64 ORR
        /// </summary>
        public static Vector64<T> Or<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Or<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector or not
        /// Corresponds to vector forms of ARM64 ORN
        /// </summary>
        public static Vector64<T> OrNot<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> OrNot<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector PopCount
        /// Corresponds to vector forms of ARM64 CNT
        /// </summary>
        public static Vector64<byte> PopCount(Vector64<byte> value) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> PopCount(Vector64<sbyte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> PopCount(Vector128<byte> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> PopCount(Vector128<sbyte> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// SetVector* Fill vector elements by replicating element value
        ///
        /// Corresponds to vector forms of ARM64 DUP (general), DUP (element 0), FMOV (vector, immediate)
        /// </summary>
        public static Vector64<T> SetAllVector64<T>(T value) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> SetAllVector128<T>(T value) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector square root
        /// Corresponds to vector forms of ARM64 FRSQRT
        /// </summary>
        public static Vector64<float> Sqrt(Vector64<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> Sqrt(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> Sqrt(Vector128<double> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector subtract
        /// Corresponds to vector forms of ARM64 SUB &amp; FSUB
        /// </summary>
        public static Vector64<T> Subtract<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }


        /// <summary>
        /// Vector exclusive or
        /// Corresponds to vector forms of ARM64 EOR
        /// </summary>
        public static Vector64<T> Xor<T>(Vector64<T> left, Vector64<T> right) where T : struct { throw new PlatformNotSupportedException(); }
        public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector Add across (Sum)
        ///
        /// For each element result += value[elem]
        ///
        /// Corresponds to vector forms of ARM64 ADDV &amp; FADDP
        /// </summary>
        public static byte   AddAcross(Vector64<byte>    value) { throw new PlatformNotSupportedException(); }
        public static sbyte  AddAcross(Vector64<sbyte>   value) { throw new PlatformNotSupportedException(); }
        public static ushort AddAcross(Vector64<ushort>  value) { throw new PlatformNotSupportedException(); }
        public static short  AddAcross(Vector64<short>   value) { throw new PlatformNotSupportedException(); }
        public static uint   AddAcross(Vector64<uint>    value) { throw new PlatformNotSupportedException(); }
        public static int    AddAcross(Vector64<int>     value) { throw new PlatformNotSupportedException(); }
        public static float  AddAcross(Vector64<float>   value) { throw new PlatformNotSupportedException(); }
        public static byte   AddAcross(Vector128<byte>   value) { throw new PlatformNotSupportedException(); }
        public static sbyte  AddAcross(Vector128<sbyte>  value) { throw new PlatformNotSupportedException(); }
        public static ushort AddAcross(Vector128<ushort> value) { throw new PlatformNotSupportedException(); }
        public static short  AddAcross(Vector128<short>  value) { throw new PlatformNotSupportedException(); }
        public static uint   AddAcross(Vector128<uint>   value) { throw new PlatformNotSupportedException(); }
        public static int    AddAcross(Vector128<int>    value) { throw new PlatformNotSupportedException(); }
        public static long   AddAcross(Vector128<long>   value) { throw new PlatformNotSupportedException(); }
        public static float  AddAcross(Vector128<float>  value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector Absolute Compare GE
        ///
        /// for each elem
        /// |left[elem]| >= |right[elem]|
        ///
        /// Corresponds to vector forms of ARM64 FACGE
        /// </summary>
        public static Vector64<float>   AbsoluteCompareGreatherThanOrEqual (Vector64<float> left  , Vector64<float>   right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float>  AbsoluteCompareGreatherThanOrEqual (Vector128<float> left , Vector128<float>  right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> AbsoluteCompareGreatherThanOrEqual (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Vector Absolute Compare GT
        ///
        /// for each elem
        /// |left[elem]| > |right[elem]|
        ///
        /// Corresponds to vector forms of ARM64 FACGT
        /// </summary>
        public static Vector64<float>   AbsoluteCompareGreatherThan (Vector64<float> left  , Vector64<float>   right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float>  AbsoluteCompareGreatherThan (Vector128<float> left , Vector128<float>  right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> AbsoluteCompareGreatherThan (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Left Shift and Insert
        ///
        /// Corresponds to vector forms of ARM64 SLI
        /// </summary>
        public static Vector64<byte>  LeftShiftAndInsert (Vector64<byte> left , Vector64<byte>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint>  LeftShiftAndInsert (Vector64<uint> left , Vector64<uint>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> LeftShiftAndInsert (Vector64<sbyte> left, Vector64<sbyte> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<int>   LeftShiftAndInsert (Vector64<int> left  , Vector64<int>   right, uint shift) { throw new PlatformNotSupportedException(); }

        public static Vector128<byte>  LeftShiftAndInsert (Vector128<byte>   left, Vector128<byte>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint>  LeftShiftAndInsert (Vector128<uint>   left, Vector128<uint>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong>  LeftShiftAndInsert (Vector128<ulong> left, Vector128<ulong> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> LeftShiftAndInsert (Vector128<sbyte>  left, Vector128<sbyte> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<int>   LeftShiftAndInsert (Vector128<int>    left, Vector128<int>   right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<long>  LeftShiftAndInsert (Vector128<long>   left, Vector128<long>  right, uint shift) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Right Shift and Insert
        ///
        /// Corresponds to vector forms of ARM64 SRI
        /// </summary>
        public static Vector64<byte>  RightShiftAndInsert (Vector64<byte> left , Vector64<byte>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint>  RightShiftAndInsert (Vector64<uint> left , Vector64<uint>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<sbyte> RightShiftAndInsert (Vector64<sbyte> left, Vector64<sbyte> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector64<int>   RightShiftAndInsert (Vector64<int>   left, Vector64<int>   right, uint shift) { throw new PlatformNotSupportedException(); }

        public static Vector128<byte>  RightShiftAndInsert (Vector128<byte> left , Vector128<byte>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint>  RightShiftAndInsert (Vector128<uint> left , Vector128<uint>  right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> RightShiftAndInsert (Vector128<ulong> left, Vector128<ulong> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> RightShiftAndInsert (Vector128<sbyte> left, Vector128<sbyte> right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<int>   RightShiftAndInsert (Vector128<int> left  , Vector128<int>   right, uint shift) { throw new PlatformNotSupportedException(); }
        public static Vector128<long>  RightShiftAndInsert (Vector128<long> left , Vector128<long>  right, uint shift) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Extract and Narrow (Low part)
        ///
        /// Corresponds to vector forms of ARM64 XTN
        /// </summary>
        public static Vector64<int> ExtractAndNarrowLow (Vector128<long> value) { throw new PlatformNotSupportedException(); }

        public static Vector64<uint> ExtractAndNarrowLow (Vector128<ulong> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Extract and Narrow (High part)
        ///
        /// Corresponds to vector forms of ARM64 XTN2
        /// </summary>
        public static Vector64<int> ExtractAndNarrowHigh (Vector128<long> value) { throw new PlatformNotSupportedException(); }

        public static Vector64<uint> ExtractAndNarrowHigh (Vector128<ulong> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Unzip vectors (Even elements)
        ///
        /// Corresponds to vector forms of ARM64 UZP1
        /// </summary>
        public static Vector64<sbyte> UnzipEven (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> UnzipEven (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> UnzipEven (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> UnzipEven (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> UnzipEven (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> UnzipEven (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> UnzipEven (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> UnzipEven (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> UnzipEven (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> UnzipEven (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> UnzipEven (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> UnzipEven (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> UnzipEven (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Unzip vectors (Odd elements)
        ///
        /// Corresponds to vector forms of ARM64 UZP2
        /// </summary>
        public static Vector64<sbyte> UnzipOdd (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> UnzipOdd (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> UnzipOdd (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> UnzipOdd (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> UnzipOdd (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> UnzipOdd (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> UnzipOdd (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> UnzipOdd (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> UnzipOdd (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> UnzipOdd (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> UnzipOdd (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> UnzipOdd (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> UnzipOdd (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Zip vectors (Low half)
        ///
        /// Corresponds to vector forms of ARM64 ZIP1
        /// </summary>
        public static Vector64<sbyte> ZipLow (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> ZipLow (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> ZipLow (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> ZipLow (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> ZipLow (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> ZipLow (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> ZipLow (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> ZipLow (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> ZipLow (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> ZipLow (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> ZipLow (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> ZipLow (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> ZipLow (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Unzip vectors (Top half)
        ///
        /// Corresponds to vector forms of ARM64 ZIP2
        /// </summary>
        public static Vector64<sbyte> ZipHigh (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> ZipHigh (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> ZipHigh (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> ZipHigh (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> ZipHigh (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> ZipHigh (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> ZipHigh (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> ZipHigh (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> ZipHigh (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> ZipHigh (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> ZipHigh (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> ZipHigh (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> ZipHigh (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Transpose vectors (Even elements)
        ///
        /// Corresponds to vector forms of ARM64 TRN1
        /// </summary>
        public static Vector64<sbyte> TransposeVectorEven (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> TransposeVectorEven (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> TransposeVectorEven (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> TransposeVectorEven (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> TransposeVectorEven (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> TransposeVectorEven (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> TransposeVectorEven (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> TransposeVectorEven (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> TransposeVectorEven (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> TransposeVectorEven (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> TransposeVectorEven (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> TransposeVectorEven (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> TransposeVectorEven (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Transpose vectors (Odd elements)
        ///
        /// Corresponds to vector forms of ARM64 TRN2
        /// </summary>
        public static Vector64<sbyte> TransposeVectorOdd (Vector64<sbyte> left, Vector64<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> TransposeVectorOdd (Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<byte> TransposeVectorOdd (Vector64<byte> left, Vector64<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> TransposeVectorOdd (Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> TransposeVectorOdd (Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }

        public static Vector128<sbyte> TransposeVectorOdd (Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> TransposeVectorOdd (Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<long> TransposeVectorOdd (Vector128<long> left, Vector128<long> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> TransposeVectorOdd (Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> TransposeVectorOdd (Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<ulong> TransposeVectorOdd (Vector128<ulong> left, Vector128<ulong> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> TransposeVectorOdd (Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> TransposeVectorOdd (Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }


        /// <summary>
        /// Multiply and subtract
        ///
        /// Corresponds to vector forms of ARM64 FMLS and MLS.
        /// </summary>
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector64<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector128<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, float value) { throw new PlatformNotSupportedException(); }

        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector64<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector128<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, float value) { throw new PlatformNotSupportedException(); }

        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, Vector128<double> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, double value) { throw new PlatformNotSupportedException(); }

        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector64<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector128<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, int value) { throw new PlatformNotSupportedException(); }

        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector64<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector128<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, int value) { throw new PlatformNotSupportedException(); }

        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector128<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, uint value) { throw new PlatformNotSupportedException(); }

        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector64<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, uint value) { throw new PlatformNotSupportedException(); }

        public static Vector128<byte> MultiplyAndSubtract (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> MultiplyAndSubtract (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> MultiplyAndSubtract (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> MultiplyAndSubtract (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }


        /// <summary>
        /// Multiply and Add
        ///
        /// Corresponds to vector forms of ARM64 FMLA and MLA.
        /// </summary>
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector64<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector64<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector128<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, float value) { throw new PlatformNotSupportedException(); }

        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector64<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector128<float> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, float value) { throw new PlatformNotSupportedException(); }

        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, Vector128<double> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, Vector128<double> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, double value) { throw new PlatformNotSupportedException(); }

        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector64<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector64<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector128<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, int value) { throw new PlatformNotSupportedException(); }

        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector128<int> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector64<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector128<int> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, int value) { throw new PlatformNotSupportedException(); }

        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector128<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, uint value) { throw new PlatformNotSupportedException(); }

        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector64<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> sel, byte index) { throw new PlatformNotSupportedException(); }
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, uint value) { throw new PlatformNotSupportedException(); }

        public static Vector128<byte> MultiplyAndAdd (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<byte> MultiplyAndAdd (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> MultiplyAndAdd (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
        public static Vector128<sbyte> MultiplyAndAdd (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) { throw new PlatformNotSupportedException(); }
    }
}
