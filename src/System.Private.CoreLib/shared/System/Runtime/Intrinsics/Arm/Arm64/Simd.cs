// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.Arm.Arm64
{
    /// <summary>
    /// This class provides access to the Arm64 AdvSIMD intrinsics
    ///
    /// Arm64 CPU indicate support for this feature by setting
    /// ID_AA64PFR0_EL1.AdvSIMD == 0 or better.
    /// </summary>
    [Intrinsic]
    [CLSCompliant(false)]
    public static class Simd
    {
        /// <summary>
        /// IsSupported property indicates whether any method provided
        /// by this class is supported by the current runtime.
        /// </summary>
        public static bool IsSupported { get => IsSupported; }

        /// <summary>
        /// Vector abs
        /// Corresponds to vector forms of ARM64 ABS &amp; FABS
        /// </summary>
        public static Vector64<byte>    Abs(Vector64<sbyte>   value) => Abs(value);
        public static Vector64<ushort>  Abs(Vector64<short>   value) => Abs(value);
        public static Vector64<uint>    Abs(Vector64<int>     value) => Abs(value);
        public static Vector64<float>   Abs(Vector64<float>   value) => Abs(value);
        public static Vector128<byte>   Abs(Vector128<sbyte>  value) => Abs(value);
        public static Vector128<ushort> Abs(Vector128<short>  value) => Abs(value);
        public static Vector128<uint>   Abs(Vector128<int>    value) => Abs(value);
        public static Vector128<ulong>  Abs(Vector128<long>   value) => Abs(value);
        public static Vector128<float>  Abs(Vector128<float>  value) => Abs(value);
        public static Vector128<double> Abs(Vector128<double> value) => Abs(value);

        /// <summary>
        /// Vector add
        /// Corresponds to vector forms of ARM64 ADD &amp; FADD
        /// </summary>
        public static Vector64<T>  Add<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => Add(left, right);
        public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct => Add(left, right);

        /// <summary>
        /// Vector and
        /// Corresponds to vector forms of ARM64 AND
        /// </summary>
        public static Vector64<T>  And<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => And(left, right);
        public static Vector128<T> And<T>(Vector128<T> left, Vector128<T> right) where T : struct => And(left, right);

        /// <summary>
        /// Vector and not
        /// Corresponds to vector forms of ARM64 BIC
        /// </summary>
        public static Vector64<T>  AndNot<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => AndNot(left, right);
        public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct => AndNot(left, right);

        /// <summary>
        /// Vector BitwiseSelect
        /// For each bit in the vector result[bit] = sel[bit] ? left[bit] : right[bit]
        /// Corresponds to vector forms of ARM64 BSL (Also BIF &amp; BIT)
        /// </summary>
        public static Vector64<T>  BitwiseSelect<T>(Vector64<T>  sel, Vector64<T>  left, Vector64<T>  right) where T : struct => BitwiseSelect(sel, left, right);
        public static Vector128<T> BitwiseSelect<T>(Vector128<T> sel, Vector128<T> left, Vector128<T> right) where T : struct => BitwiseSelect(sel, left, right);

        /// <summary>
        /// Vector CompareEqual
        /// For each element result[elem] = (left[elem] == right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMEQ &amp; FCMEQ
        /// </summary>
        public static Vector64<T>  CompareEqual<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => CompareEqual(left, right);
        public static Vector128<T> CompareEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct => CompareEqual(left, right);

        /// <summary>
        /// Vector CompareEqualZero
        /// For each element result[elem] = (left[elem] == 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMEQ &amp; FCMEQ
        /// </summary>
        public static Vector64<T>  CompareEqualZero<T>(Vector64<T>  value) where T : struct => CompareEqualZero(value);
        public static Vector128<T> CompareEqualZero<T>(Vector128<T> value) where T : struct => CompareEqualZero(value);

        /// <summary>
        /// Vector CompareGreaterThan
        /// For each element result[elem] = (left[elem] > right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT/CMHI &amp; FCMGT
        /// </summary>
        public static Vector64<T>  CompareGreaterThan<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => CompareGreaterThan(left, right);
        public static Vector128<T> CompareGreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct => CompareGreaterThan(left, right);

        /// <summary>
        /// Vector CompareGreaterThanZero
        /// For each element result[elem] = (left[elem] > 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T>  CompareGreaterThanZero<T>(Vector64<T>  value) where T : struct => CompareGreaterThanZero(value);
        public static Vector128<T> CompareGreaterThanZero<T>(Vector128<T> value) where T : struct => CompareGreaterThanZero(value);

        /// <summary>
        /// Vector CompareGreaterThanOrEqual
        /// For each element result[elem] = (left[elem] >= right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGE/CMHS &amp; FCMGE
        /// </summary>
        public static Vector64<T>  CompareGreaterThanOrEqual<T>(Vector64<T>  left, Vector64<T>    right) where T : struct => CompareGreaterThanOrEqual(left, right);
        public static Vector128<T> CompareGreaterThanOrEqual<T>(Vector128<T> left, Vector128<T>   right) where T : struct => CompareGreaterThanOrEqual(left, right);

        /// <summary>
        /// Vector CompareGreaterThanOrEqualZero
        /// For each element result[elem] = (left[elem] >= 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGE &amp; FCMGE
        /// </summary>
        public static Vector64<T>  CompareGreaterThanOrEqualZero<T>(Vector64<T>  value) where T : struct => CompareGreaterThanOrEqualZero(value);
        public static Vector128<T> CompareGreaterThanOrEqualZero<T>(Vector128<T> value) where T : struct => CompareGreaterThanOrEqualZero(value);

        /// <summary>
        /// Vector CompareLessThanZero
        /// For each element result[elem] = (left[elem] &lt; 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T>  CompareLessThanZero<T>(Vector64<T>  value) where T : struct => CompareLessThanZero(value);
        public static Vector128<T> CompareLessThanZero<T>(Vector128<T> value) where T : struct => CompareLessThanZero(value);

        /// <summary>
        /// Vector CompareLessThanOrEqualZero
        /// For each element result[elem] = (left[elem] &lt; 0) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMGT &amp; FCMGT
        /// </summary>
        public static Vector64<T>  CompareLessThanOrEqualZero<T>(Vector64<T>  value) where T : struct => CompareLessThanOrEqualZero(value);
        public static Vector128<T> CompareLessThanOrEqualZero<T>(Vector128<T> value) where T : struct => CompareLessThanOrEqualZero(value);

        /// <summary>
        /// Vector CompareTest
        /// For each element result[elem] = (left[elem] &amp; right[elem]) ? ~0 : 0
        /// Corresponds to vector forms of ARM64 CMTST
        /// </summary>
        public static Vector64<T>  CompareTest<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => CompareTest(left, right);
        public static Vector128<T> CompareTest<T>(Vector128<T> left, Vector128<T> right) where T : struct => CompareTest(left, right);

        /// TBD Convert...

        /// <summary>
        /// Vector Divide
        /// Corresponds to vector forms of ARM64 FDIV
        /// </summary>
        public static Vector64<float>   Divide(Vector64<float>   left, Vector64<float>   right) => Divide(left, right);
        public static Vector128<float>  Divide(Vector128<float>  left, Vector128<float>  right) => Divide(left, right);
        public static Vector128<double> Divide(Vector128<double> left, Vector128<double> right) => Divide(left, right);

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
        public static T Extract<T>(Vector64<T>  vector, byte index) where T : struct => Extract(vector, index);
        public static T Extract<T>(Vector128<T> vector, byte index) where T : struct => Extract(vector, index);

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
        public static Vector64<T>  Insert<T>(Vector64<T>  vector, byte index, T data) where T : struct => Insert(vector, index, data);
        public static Vector128<T> Insert<T>(Vector128<T> vector, byte index, T data) where T : struct => Insert(vector, index, data);

        /// <summary>
        /// Vector LeadingSignCount
        /// Corresponds to vector forms of ARM64 CLS
        /// </summary>
        public static Vector64<sbyte>  LeadingSignCount(Vector64<sbyte>  value) => LeadingSignCount(value);
        public static Vector64<short>  LeadingSignCount(Vector64<short>  value) => LeadingSignCount(value);
        public static Vector64<int>    LeadingSignCount(Vector64<int>    value) => LeadingSignCount(value);
        public static Vector128<sbyte> LeadingSignCount(Vector128<sbyte> value) => LeadingSignCount(value);
        public static Vector128<short> LeadingSignCount(Vector128<short> value) => LeadingSignCount(value);
        public static Vector128<int>   LeadingSignCount(Vector128<int>   value) => LeadingSignCount(value);

        /// <summary>
        /// Vector LeadingZeroCount
        /// Corresponds to vector forms of ARM64 CLZ
        /// </summary>
        public static Vector64<byte>    LeadingZeroCount(Vector64<byte>    value) => LeadingZeroCount(value);
        public static Vector64<sbyte>   LeadingZeroCount(Vector64<sbyte>   value) => LeadingZeroCount(value);
        public static Vector64<ushort>  LeadingZeroCount(Vector64<ushort>  value) => LeadingZeroCount(value);
        public static Vector64<short>   LeadingZeroCount(Vector64<short>   value) => LeadingZeroCount(value);
        public static Vector64<uint>    LeadingZeroCount(Vector64<uint>    value) => LeadingZeroCount(value);
        public static Vector64<int>     LeadingZeroCount(Vector64<int>     value) => LeadingZeroCount(value);
        public static Vector128<byte>   LeadingZeroCount(Vector128<byte>   value) => LeadingZeroCount(value);
        public static Vector128<sbyte>  LeadingZeroCount(Vector128<sbyte>  value) => LeadingZeroCount(value);
        public static Vector128<ushort> LeadingZeroCount(Vector128<ushort> value) => LeadingZeroCount(value);
        public static Vector128<short>  LeadingZeroCount(Vector128<short>  value) => LeadingZeroCount(value);
        public static Vector128<uint>   LeadingZeroCount(Vector128<uint>   value) => LeadingZeroCount(value);
        public static Vector128<int>    LeadingZeroCount(Vector128<int>    value) => LeadingZeroCount(value);

        /// <summary>
        /// Vector max
        /// Corresponds to vector forms of ARM64 SMAX, UMAX &amp; FMAX
        /// </summary>
        public static Vector64<byte>    Max(Vector64<byte>    left, Vector64<byte>    right) => Max(left, right);
        public static Vector64<sbyte>   Max(Vector64<sbyte>   left, Vector64<sbyte>   right) => Max(left, right);
        public static Vector64<ushort>  Max(Vector64<ushort>  left, Vector64<ushort>  right) => Max(left, right);
        public static Vector64<short>   Max(Vector64<short>   left, Vector64<short>   right) => Max(left, right);
        public static Vector64<uint>    Max(Vector64<uint>    left, Vector64<uint>    right) => Max(left, right);
        public static Vector64<int>     Max(Vector64<int>     left, Vector64<int>     right) => Max(left, right);
        public static Vector64<float>   Max(Vector64<float>   left, Vector64<float>   right) => Max(left, right);
        public static Vector128<byte>   Max(Vector128<byte>   left, Vector128<byte>   right) => Max(left, right);
        public static Vector128<sbyte>  Max(Vector128<sbyte>  left, Vector128<sbyte>  right) => Max(left, right);
        public static Vector128<ushort> Max(Vector128<ushort> left, Vector128<ushort> right) => Max(left, right);
        public static Vector128<short>  Max(Vector128<short>  left, Vector128<short>  right) => Max(left, right);
        public static Vector128<uint>   Max(Vector128<uint>   left, Vector128<uint>   right) => Max(left, right);
        public static Vector128<int>    Max(Vector128<int>    left, Vector128<int>    right) => Max(left, right);
        public static Vector128<float>  Max(Vector128<float>  left, Vector128<float>  right) => Max(left, right);
        public static Vector128<double> Max(Vector128<double> left, Vector128<double> right) => Max(left, right);

        /// <summary>
        /// Vector min
        /// Corresponds to vector forms of ARM64 SMIN, UMIN &amp; FMIN
        /// </summary>
        public static Vector64<byte>    Min(Vector64<byte>    left, Vector64<byte>    right) => Min(left, right);
        public static Vector64<sbyte>   Min(Vector64<sbyte>   left, Vector64<sbyte>   right) => Min(left, right);
        public static Vector64<ushort>  Min(Vector64<ushort>  left, Vector64<ushort>  right) => Min(left, right);
        public static Vector64<short>   Min(Vector64<short>   left, Vector64<short>   right) => Min(left, right);
        public static Vector64<uint>    Min(Vector64<uint>    left, Vector64<uint>    right) => Min(left, right);
        public static Vector64<int>     Min(Vector64<int>     left, Vector64<int>     right) => Min(left, right);
        public static Vector64<float>   Min(Vector64<float>   left, Vector64<float>   right) => Min(left, right);
        public static Vector128<byte>   Min(Vector128<byte>   left, Vector128<byte>   right) => Min(left, right);
        public static Vector128<sbyte>  Min(Vector128<sbyte>  left, Vector128<sbyte>  right) => Min(left, right);
        public static Vector128<ushort> Min(Vector128<ushort> left, Vector128<ushort> right) => Min(left, right);
        public static Vector128<short>  Min(Vector128<short>  left, Vector128<short>  right) => Min(left, right);
        public static Vector128<uint>   Min(Vector128<uint>   left, Vector128<uint>   right) => Min(left, right);
        public static Vector128<int>    Min(Vector128<int>    left, Vector128<int>    right) => Min(left, right);
        public static Vector128<float>  Min(Vector128<float>  left, Vector128<float>  right) => Min(left, right);
        public static Vector128<double> Min(Vector128<double> left, Vector128<double> right) => Min(left, right);

        /// TBD MOV, FMOV

        /// <summary>
        /// Vector multiply
        ///
        /// For each element result[elem] = left[elem] * right[elem]
        ///
        /// Corresponds to vector forms of ARM64 MUL &amp; FMUL
        /// </summary>
        public static Vector64<byte>    Multiply(Vector64<byte>    left, Vector64<byte>    right) => Multiply(left, right);
        public static Vector64<sbyte>   Multiply(Vector64<sbyte>   left, Vector64<sbyte>   right) => Multiply(left, right);
        public static Vector64<ushort>  Multiply(Vector64<ushort>  left, Vector64<ushort>  right) => Multiply(left, right);
        public static Vector64<short>   Multiply(Vector64<short>   left, Vector64<short>   right) => Multiply(left, right);
        public static Vector64<uint>    Multiply(Vector64<uint>    left, Vector64<uint>    right) => Multiply(left, right);
        public static Vector64<int>     Multiply(Vector64<int>     left, Vector64<int>     right) => Multiply(left, right);
        public static Vector64<float>   Multiply(Vector64<float>   left, Vector64<float>   right) => Multiply(left, right);
        public static Vector128<byte>   Multiply(Vector128<byte>   left, Vector128<byte>   right) => Multiply(left, right);
        public static Vector128<sbyte>  Multiply(Vector128<sbyte>  left, Vector128<sbyte>  right) => Multiply(left, right);
        public static Vector128<ushort> Multiply(Vector128<ushort> left, Vector128<ushort> right) => Multiply(left, right);
        public static Vector128<short>  Multiply(Vector128<short>  left, Vector128<short>  right) => Multiply(left, right);
        public static Vector128<uint>   Multiply(Vector128<uint>   left, Vector128<uint>   right) => Multiply(left, right);
        public static Vector128<int>    Multiply(Vector128<int>    left, Vector128<int>    right) => Multiply(left, right);
        public static Vector128<float>  Multiply(Vector128<float>  left, Vector128<float>  right) => Multiply(left, right);
        public static Vector128<double> Multiply(Vector128<double> left, Vector128<double> right) => Multiply(left, right);

        /// <summary>
        /// Vector negate
        /// Corresponds to vector forms of ARM64 NEG &amp; FNEG
        /// </summary>
        public static Vector64<sbyte>   Negate(Vector64<sbyte>   value) => Negate(value);
        public static Vector64<short>   Negate(Vector64<short>   value) => Negate(value);
        public static Vector64<int>     Negate(Vector64<int>     value) => Negate(value);
        public static Vector64<float>   Negate(Vector64<float>   value) => Negate(value);
        public static Vector128<sbyte>  Negate(Vector128<sbyte>  value) => Negate(value);
        public static Vector128<short>  Negate(Vector128<short>  value) => Negate(value);
        public static Vector128<int>    Negate(Vector128<int>    value) => Negate(value);
        public static Vector128<long>   Negate(Vector128<long>   value) => Negate(value);
        public static Vector128<float>  Negate(Vector128<float>  value) => Negate(value);
        public static Vector128<double> Negate(Vector128<double> value) => Negate(value);

        /// <summary>
        /// Vector not
        /// Corresponds to vector forms of ARM64 NOT
        /// </summary>
        public static Vector64<T>  Not<T>(Vector64<T>  value) where T : struct => Not(value);
        public static Vector128<T> Not<T>(Vector128<T> value) where T : struct => Not(value);

        /// <summary>
        /// Vector or
        /// Corresponds to vector forms of ARM64 ORR
        /// </summary>
        public static Vector64<T>  Or<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => Or(left, right);
        public static Vector128<T> Or<T>(Vector128<T> left, Vector128<T> right) where T : struct => Or(left, right);

        /// <summary>
        /// Vector or not
        /// Corresponds to vector forms of ARM64 ORN
        /// </summary>
        public static Vector64<T>  OrNot<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => OrNot(left, right);
        public static Vector128<T> OrNot<T>(Vector128<T> left, Vector128<T> right) where T : struct => OrNot(left, right);

        /// <summary>
        /// Vector PopCount
        /// Corresponds to vector forms of ARM64 CNT
        /// </summary>
        public static Vector64<byte>    PopCount(Vector64<byte>    value) => PopCount(value);
        public static Vector64<sbyte>   PopCount(Vector64<sbyte>   value) => PopCount(value);
        public static Vector128<byte>   PopCount(Vector128<byte>   value) => PopCount(value);
        public static Vector128<sbyte>  PopCount(Vector128<sbyte>  value) => PopCount(value);

        /// <summary>
        /// SetVector* Fill vector elements by replicating element value
        ///
        /// Corresponds to vector forms of ARM64 DUP (general), DUP (element 0), FMOV (vector, immediate)
        /// </summary>
        public static Vector64<T>    SetAllVector64<T>(T value)  where T : struct => SetAllVector64(value);
        public static Vector128<T>   SetAllVector128<T>(T value) where T : struct => SetAllVector128(value);

        /// <summary>
        /// Vector square root
        /// Corresponds to vector forms of ARM64 FRSQRT
        /// </summary>
        public static Vector64<float>   Sqrt(Vector64<float>   value) => Sqrt(value);
        public static Vector128<float>  Sqrt(Vector128<float>  value) => Sqrt(value);
        public static Vector128<double> Sqrt(Vector128<double> value) => Sqrt(value);

        /// <summary>
        /// Vector subtract
        /// Corresponds to vector forms of ARM64 SUB &amp; FSUB
        /// </summary>
        public static Vector64<T>  Subtract<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => Subtract(left, right);
        public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct => Subtract(left, right);


        /// <summary>
        /// Vector exclusive or
        /// Corresponds to vector forms of ARM64 EOR
        /// </summary>
        public static Vector64<T>  Xor<T>(Vector64<T>  left, Vector64<T>  right) where T : struct => Xor(left, right);
        public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct => Xor(left, right);

	/// <summary>
        /// Vector Add across (Sum)
        ///
        /// For each element result += value[elem]
        ///
        /// Corresponds to vector forms of ARM64 ADDV &amp; FADDP
        /// </summary>
        public static byte   AddAcross(Vector64<byte>    value) => AddAcross(value);
        public static sbyte  AddAcross(Vector64<sbyte>   value) => AddAcross(value);
        public static ushort AddAcross(Vector64<ushort>  value) => AddAcross(value);
        public static short  AddAcross(Vector64<short>   value) => AddAcross(value);
        public static uint   AddAcross(Vector64<uint>    value) => AddAcross(value);
        public static int    AddAcross(Vector64<int>     value) => AddAcross(value);
        public static float  AddAcross(Vector64<float>   value) => AddAcross(value);
        public static byte   AddAcross(Vector128<byte>   value) => AddAcross(value);
        public static sbyte  AddAcross(Vector128<sbyte>  value) => AddAcross(value);
        public static ushort AddAcross(Vector128<ushort> value) => AddAcross(value);
        public static short  AddAcross(Vector128<short>  value) => AddAcross(value);
        public static uint   AddAcross(Vector128<uint>   value) => AddAcross(value);
        public static int    AddAcross(Vector128<int>    value) => AddAcross(value);
        public static long   AddAcross(Vector128<long>   value) => AddAcross(value);
        public static float  AddAcross(Vector128<float>  value) => AddAcross(value);

        /// <summary>
        /// Vector Absolute Compare GE
        ///
        /// for each elem
        /// |left[elem]| >= |right[elem]|
        ///
        /// Corresponds to vector forms of ARM64 FACGE
        /// </summary>
        public static Vector64<float>   AbsoluteCompareGreatherThanOrEqual (Vector64<float> left  , Vector64<float>   right) => AbsoluteCompareGreatherThanOrEqual(left, right);
        public static Vector128<float>  AbsoluteCompareGreatherThanOrEqual (Vector128<float> left , Vector128<float>  right) => AbsoluteCompareGreatherThanOrEqual(left, right);
        public static Vector128<double> AbsoluteCompareGreatherThanOrEqual (Vector128<double> left, Vector128<double> right) => AbsoluteCompareGreatherThanOrEqual(left, right);

        /// <summary>
        /// Vector Absolute Compare GT
        ///
        /// for each elem
        /// |left[elem]| > |right[elem]|
        ///
        /// Corresponds to vector forms of ARM64 FACGT
        /// </summary>
        public static Vector64<float>   AbsoluteCompareGreatherThan (Vector64<float> left  , Vector64<float>   right) => AbsoluteCompareGreatherThan(left, right);
        public static Vector128<float>  AbsoluteCompareGreatherThan (Vector128<float> left , Vector128<float>  right) => AbsoluteCompareGreatherThan(left, right);
        public static Vector128<double> AbsoluteCompareGreatherThan (Vector128<double> left, Vector128<double> right) => AbsoluteCompareGreatherThan(left, right);

        /// <summary>
        /// Left Shift and Insert
        ///
        /// Corresponds to vector forms of ARM64 SLI
        /// </summary>
        public static Vector64<byte>  LeftShiftAndInsert (Vector64<byte> left , Vector64<byte>  right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector64<uint>  LeftShiftAndInsert (Vector64<uint> left , Vector64<uint>  right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector64<sbyte> LeftShiftAndInsert (Vector64<sbyte> left, Vector64<sbyte> right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector64<int>   LeftShiftAndInsert (Vector64<int> left  , Vector64<int>   right, uint shift) => LeftShiftAndInsert(left, right, shift);

        public static Vector128<byte>  LeftShiftAndInsert (Vector128<byte> left  , Vector128<byte>  right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector128<uint>  LeftShiftAndInsert (Vector128<uint> left  , Vector128<uint>  right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector128<ulong> LeftShiftAndInsert (Vector128<ulong> left, Vector128<ulong> right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector128<sbyte> LeftShiftAndInsert (Vector128<sbyte> left , Vector128<sbyte> right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector128<int>   LeftShiftAndInsert (Vector128<int> left   , Vector128<int>   right, uint shift) => LeftShiftAndInsert(left, right, shift);
        public static Vector128<long>  LeftShiftAndInsert (Vector128<long> left  , Vector128<long>  right, uint shift) => LeftShiftAndInsert(left, right, shift);

        /// <summary>
        /// Right Shift and Insert
        ///
        /// Corresponds to vector forms of ARM64 SRI
        /// </summary>
        public static Vector64<byte>  RightShiftAndInsert (Vector64<byte> left , Vector64<byte>  right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector64<uint>  RightShiftAndInsert (Vector64<uint> left , Vector64<uint>  right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector64<sbyte> RightShiftAndInsert (Vector64<sbyte> left, Vector64<sbyte> right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector64<int>   RightShiftAndInsert (Vector64<int> left  , Vector64<int>   right, uint shift) => RightShiftAndInsert(left, right, shift);

        public static Vector128<byte>  RightShiftAndInsert (Vector128<byte> left  , Vector128<byte>  right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector128<uint>  RightShiftAndInsert (Vector128<uint> left  , Vector128<uint>  right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector128<ulong> RightShiftAndInsert (Vector128<ulong> left , Vector128<ulong> right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector128<sbyte> RightShiftAndInsert (Vector128<sbyte> left , Vector128<sbyte> right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector128<int>   RightShiftAndInsert (Vector128<int> left   , Vector128<int>   right, uint shift) => RightShiftAndInsert(left, right, shift);
        public static Vector128<long>  RightShiftAndInsert (Vector128<long> left  , Vector128<long>  right, uint shift) => RightShiftAndInsert(left, right, shift);

        /// <summary>
        /// Extract and Narrow (Low part)
        ///
        /// Corresponds to vector forms of ARM64 XTN
        /// </summary>
        public static Vector64<int> ExtractAndNarrowLow (Vector128<long> value) => ExtractAndNarrowLow (value);
        public static Vector64<uint> ExtractAndNarrowLow (Vector128<ulong> value) => ExtractAndNarrowLow (value);

        /// <summary>
        /// Extract and Narrow (High part)
        ///
        /// Corresponds to vector forms of ARM64 XTN2
        /// </summary>
        public static Vector64<int> ExtractAndNarrowHigh (Vector128<long> value) => ExtractAndNarrowHigh (value);
        public static Vector64<uint> ExtractAndNarrowHigh (Vector128<ulong> value) => ExtractAndNarrowHigh (value);

        /// <summary>
        /// Unzip vectors (Even elements)
        ///
        /// Corresponds to vector forms of ARM64 UZP1
        /// </summary>
        public static Vector64<sbyte> UnzipEven (Vector64<sbyte> left, Vector64<sbyte> right) => UnzipEven (left, right);
        public static Vector64<int> UnzipEven (Vector64<int> left, Vector64<int> right) => UnzipEven (left, right);
        public static Vector64<byte> UnzipEven (Vector64<byte> left, Vector64<byte> right) => UnzipEven (left, right);
        public static Vector64<uint> UnzipEven (Vector64<uint> left, Vector64<uint> right) => UnzipEven (left, right);
        public static Vector64<float> UnzipEven (Vector64<float> left, Vector64<float> right) => UnzipEven (left, right);

        public static Vector128<sbyte> UnzipEven (Vector128<sbyte> left, Vector128<sbyte> right) => UnzipEven (left, right);
        public static Vector128<int> UnzipEven (Vector128<int> left, Vector128<int> right) => UnzipEven (left, right);
        public static Vector128<long> UnzipEven (Vector128<long> left, Vector128<long> right) => UnzipEven (left, right);
        public static Vector128<byte> UnzipEven (Vector128<byte> left, Vector128<byte> right) => UnzipEven (left, right);
        public static Vector128<uint> UnzipEven (Vector128<uint> left, Vector128<uint> right) => UnzipEven (left, right);
        public static Vector128<ulong> UnzipEven (Vector128<ulong> left, Vector128<ulong> right) => UnzipEven (left, right);
        public static Vector128<float> UnzipEven (Vector128<float> left, Vector128<float> right) => UnzipEven (left, right);
        public static Vector128<double> UnzipEven (Vector128<double> left, Vector128<double> right) => UnzipEven (left, right);

        /// <summary>
        /// Unzip vectors (Odd elements)
        ///
        /// Corresponds to vector forms of ARM64 UZP2
        /// </summary>
        public static Vector64<sbyte> UnzipOdd (Vector64<sbyte> left, Vector64<sbyte> right) => UnzipOdd (left, right);
        public static Vector64<int> UnzipOdd (Vector64<int> left, Vector64<int> right) => UnzipOdd (left, right);
        public static Vector64<byte> UnzipOdd (Vector64<byte> left, Vector64<byte> right) => UnzipOdd (left, right);
        public static Vector64<uint> UnzipOdd (Vector64<uint> left, Vector64<uint> right) => UnzipOdd (left, right);
        public static Vector64<float> UnzipOdd (Vector64<float> left, Vector64<float> right) => UnzipOdd (left, right);

        public static Vector128<sbyte> UnzipOdd (Vector128<sbyte> left, Vector128<sbyte> right) => UnzipOdd (left, right);
        public static Vector128<int> UnzipOdd (Vector128<int> left, Vector128<int> right) => UnzipOdd (left, right);
        public static Vector128<long> UnzipOdd (Vector128<long> left, Vector128<long> right) => UnzipOdd (left, right);
        public static Vector128<byte> UnzipOdd (Vector128<byte> left, Vector128<byte> right) => UnzipOdd (left, right);
        public static Vector128<uint> UnzipOdd (Vector128<uint> left, Vector128<uint> right) => UnzipOdd (left, right);
        public static Vector128<ulong> UnzipOdd (Vector128<ulong> left, Vector128<ulong> right) => UnzipOdd (left, right);
        public static Vector128<float> UnzipOdd (Vector128<float> left, Vector128<float> right) => UnzipOdd (left, right);
        public static Vector128<double> UnzipOdd (Vector128<double> left, Vector128<double> right) => UnzipOdd (left, right);

        /// <summary>
        /// Zip vectors (Low half)
        ///
        /// Corresponds to vector forms of ARM64 ZIP1
        /// </summary>
        public static Vector64<sbyte> ZipLow (Vector64<sbyte> left, Vector64<sbyte> right) => ZipLow (left, right);
        public static Vector64<int> ZipLow (Vector64<int> left, Vector64<int> right) => ZipLow (left, right);
        public static Vector64<byte> ZipLow (Vector64<byte> left, Vector64<byte> right) => ZipLow (left, right);
        public static Vector64<uint> ZipLow (Vector64<uint> left, Vector64<uint> right) => ZipLow (left, right);
        public static Vector64<float> ZipLow (Vector64<float> left, Vector64<float> right) => ZipLow (left, right);

        public static Vector128<sbyte> ZipLow (Vector128<sbyte> left, Vector128<sbyte> right) => ZipLow (left, right);
        public static Vector128<int> ZipLow (Vector128<int> left, Vector128<int> right) => ZipLow (left, right);
        public static Vector128<long> ZipLow (Vector128<long> left, Vector128<long> right) => ZipLow (left, right);
        public static Vector128<byte> ZipLow (Vector128<byte> left, Vector128<byte> right) => ZipLow (left, right);
        public static Vector128<uint> ZipLow (Vector128<uint> left, Vector128<uint> right) => ZipLow (left, right);
        public static Vector128<ulong> ZipLow (Vector128<ulong> left, Vector128<ulong> right) => ZipLow (left, right);
        public static Vector128<float> ZipLow (Vector128<float> left, Vector128<float> right) => ZipLow (left, right);
        public static Vector128<double> ZipLow (Vector128<double> left, Vector128<double> right) => ZipLow (left, right);

        /// <summary>
        /// Unzip vectors (Top half)
        ///
        /// Corresponds to vector forms of ARM64 ZIP2
        /// </summary>
        public static Vector64<sbyte> ZipHigh (Vector64<sbyte> left, Vector64<sbyte> right) => ZipHigh (left, right);
        public static Vector64<int> ZipHigh (Vector64<int> left, Vector64<int> right) => ZipHigh (left, right);
        public static Vector64<byte> ZipHigh (Vector64<byte> left, Vector64<byte> right) => ZipHigh (left, right);
        public static Vector64<uint> ZipHigh (Vector64<uint> left, Vector64<uint> right) => ZipHigh (left, right);
        public static Vector64<float> ZipHigh (Vector64<float> left, Vector64<float> right) => ZipHigh (left, right);

        public static Vector128<sbyte> ZipHigh (Vector128<sbyte> left, Vector128<sbyte> right) => ZipHigh (left, right);
        public static Vector128<int> ZipHigh (Vector128<int> left, Vector128<int> right) => ZipHigh (left, right);
        public static Vector128<long> ZipHigh (Vector128<long> left, Vector128<long> right) => ZipHigh (left, right);
        public static Vector128<byte> ZipHigh (Vector128<byte> left, Vector128<byte> right) => ZipHigh (left, right);
        public static Vector128<uint> ZipHigh (Vector128<uint> left, Vector128<uint> right) => ZipHigh (left, right);
        public static Vector128<ulong> ZipHigh (Vector128<ulong> left, Vector128<ulong> right) => ZipHigh (left, right);
        public static Vector128<float> ZipHigh (Vector128<float> left, Vector128<float> right) => ZipHigh (left, right);
        public static Vector128<double> ZipHigh (Vector128<double> left, Vector128<double> right) => ZipHigh (left, right);

        /// <summary>
        /// Transpose vectors (Even elements)
        ///
        /// Corresponds to vector forms of ARM64 TRN1
        /// </summary>
        public static Vector64<sbyte> TransposeVectorEven (Vector64<sbyte> left, Vector64<sbyte> right) => TransposeVectorEven (left, right);
        public static Vector64<int> TransposeVectorEven (Vector64<int> left, Vector64<int> right) => TransposeVectorEven (left, right);
        public static Vector64<byte> TransposeVectorEven (Vector64<byte> left, Vector64<byte> right) => TransposeVectorEven (left, right);
        public static Vector64<uint> TransposeVectorEven (Vector64<uint> left, Vector64<uint> right)  => TransposeVectorEven (left, right);
        public static Vector64<float> TransposeVectorEven (Vector64<float> left, Vector64<float> right) => TransposeVectorEven (left, right);

        public static Vector128<sbyte> TransposeVectorEven (Vector128<sbyte> left, Vector128<sbyte> right) => TransposeVectorEven (left, right);
        public static Vector128<int> TransposeVectorEven (Vector128<int> left, Vector128<int> right) => TransposeVectorEven (left, right);
        public static Vector128<long> TransposeVectorEven (Vector128<long> left, Vector128<long> right) => TransposeVectorEven (left, right);
        public static Vector128<byte> TransposeVectorEven (Vector128<byte> left, Vector128<byte> right) => TransposeVectorEven (left, right);
        public static Vector128<uint> TransposeVectorEven (Vector128<uint> left, Vector128<uint> right) => TransposeVectorEven (left, right);
        public static Vector128<ulong> TransposeVectorEven (Vector128<ulong> left, Vector128<ulong> right) => TransposeVectorEven (left, right);
        public static Vector128<float> TransposeVectorEven (Vector128<float> left, Vector128<float> right) => TransposeVectorEven (left, right);
        public static Vector128<double> TransposeVectorEven (Vector128<double> left, Vector128<double> right) => TransposeVectorEven (left, right);

        /// <summary>
        /// Transpose vectors (Odd elements)
        ///
        /// Corresponds to vector forms of ARM64 TRN2
        /// </summary>
        public static Vector64<sbyte> TransposeVectorOdd (Vector64<sbyte> left, Vector64<sbyte> right) => TransposeVectorOdd (left, right);
        public static Vector64<int> TransposeVectorOdd (Vector64<int> left, Vector64<int> right) => TransposeVectorOdd (left, right);
        public static Vector64<byte> TransposeVectorOdd (Vector64<byte> left, Vector64<byte> right) => TransposeVectorOdd (left, right);
        public static Vector64<uint> TransposeVectorOdd (Vector64<uint> left, Vector64<uint> right) => TransposeVectorOdd (left, right);
        public static Vector64<float> TransposeVectorOdd (Vector64<float> left, Vector64<float> right) => TransposeVectorOdd (left, right);

        public static Vector128<sbyte> TransposeVectorOdd (Vector128<sbyte> left, Vector128<sbyte> right) => TransposeVectorOdd (left, right);
        public static Vector128<int> TransposeVectorOdd (Vector128<int> left, Vector128<int> right) => TransposeVectorOdd (left, right);
        public static Vector128<long> TransposeVectorOdd (Vector128<long> left, Vector128<long> right) => TransposeVectorOdd (left, right);
        public static Vector128<byte> TransposeVectorOdd (Vector128<byte> left, Vector128<byte> right) => TransposeVectorOdd (left, right);
        public static Vector128<uint> TransposeVectorOdd (Vector128<uint> left, Vector128<uint> right) => TransposeVectorOdd (left, right);
        public static Vector128<ulong> TransposeVectorOdd (Vector128<ulong> left, Vector128<ulong> right) => TransposeVectorOdd (left, right);
        public static Vector128<float> TransposeVectorOdd (Vector128<float> left, Vector128<float> right) => TransposeVectorOdd (left, right);
        public static Vector128<double> TransposeVectorOdd (Vector128<double> left, Vector128<double> right) => TransposeVectorOdd (left, right);

        /// <summary>
        /// Multiply and subtract
        ///
        /// Corresponds to vector forms of ARM64 FMLS and MLS.
        /// </summary>
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector64<float> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector64<float> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, Vector128<float> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<float> MultiplyAndSubtract (Vector64<float> acc, Vector64<float> left, float value) => MultiplyAndSubtract (acc, left, value);

        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector128<float> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector64<float> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, Vector128<float> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<float> MultiplyAndSubtract (Vector128<float> acc, Vector128<float> left, float value) => MultiplyAndSubtract (acc, left, value);

        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, Vector128<double> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, Vector128<double> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<double> MultiplyAndSubtract (Vector128<double> acc, Vector128<double> left, double value) => MultiplyAndSubtract (acc, left, value);

        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector64<int> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector64<int> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, Vector128<int> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<int> MultiplyAndSubtract (Vector64<int> acc, Vector64<int> left, int value) => MultiplyAndSubtract (acc, left, value);

        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector128<int> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector64<int> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, Vector128<int> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<int> MultiplyAndSubtract (Vector128<int> acc, Vector128<int> left, int value) => MultiplyAndSubtract (acc, left, value);

        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, Vector128<uint> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector64<uint> MultiplyAndSubtract (Vector64<uint> acc, Vector64<uint> left, uint value) => MultiplyAndSubtract (acc, left, value);

        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector64<uint> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> sel, byte index) => MultiplyAndSubtract (acc, left, sel, index);
        public static Vector128<uint> MultiplyAndSubtract (Vector128<uint> acc, Vector128<uint> left, uint value) => MultiplyAndSubtract (acc, left, value);

        public static Vector128<byte> MultiplyAndSubtract (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<byte> MultiplyAndSubtract (Vector128<byte> acc, Vector128<byte> left, Vector64<byte> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<sbyte> MultiplyAndSubtract (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) => MultiplyAndSubtract (acc, left, right);
        public static Vector128<sbyte> MultiplyAndSubtract (Vector128<sbyte> acc, Vector128<sbyte> left, Vector64<sbyte> right) => MultiplyAndSubtract (acc, left, right);


        /// <summary>
        /// Multiply and Add
        ///
        /// Corresponds to vector forms of ARM64 FMLA and MLA.
        /// </summary>
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector64<float> right) => MultiplyAndAdd (acc, left, right);
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector64<float> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, Vector128<float> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<float> MultiplyAndAdd (Vector64<float> acc, Vector64<float> left, float value) => MultiplyAndAdd (acc, left, value);

        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector128<float> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector64<float> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, Vector128<float> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<float> MultiplyAndAdd (Vector128<float> acc, Vector128<float> left, float value) => MultiplyAndAdd (acc, left, value);

        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, Vector128<double> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, Vector128<double> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<double> MultiplyAndAdd (Vector128<double> acc, Vector128<double> left, double value) => MultiplyAndAdd (acc, left, value);

        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector64<int> right) => MultiplyAndAdd (acc, left, right);
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector64<int> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, Vector128<int> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<int> MultiplyAndAdd (Vector64<int> acc, Vector64<int> left, int value) => MultiplyAndAdd (acc, left, value);

        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector128<int> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector64<int> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, Vector128<int> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<int> MultiplyAndAdd (Vector128<int> acc, Vector128<int> left, int value) => MultiplyAndAdd (acc, left, value);

        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> right) => MultiplyAndAdd (acc, left, right);
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector64<uint> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, Vector128<uint> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector64<uint> MultiplyAndAdd (Vector64<uint> acc, Vector64<uint> left, uint value) => MultiplyAndAdd (acc, left, value);

        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector64<uint> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, Vector128<uint> sel, byte index) => MultiplyAndAdd (acc, left, sel, index);
        public static Vector128<uint> MultiplyAndAdd (Vector128<uint> acc, Vector128<uint> left, uint value) => MultiplyAndAdd (acc, left, value);

        public static Vector128<byte> MultiplyAndAdd (Vector128<byte> acc, Vector128<byte> left, Vector128<byte> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<byte> MultiplyAndAdd (Vector128<byte> acc, Vector128<byte> left, Vector64<byte> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<sbyte> MultiplyAndAdd (Vector128<sbyte> acc, Vector128<sbyte> left, Vector128<sbyte> right) => MultiplyAndAdd (acc, left, right);
        public static Vector128<sbyte> MultiplyAndAdd (Vector128<sbyte> acc, Vector128<sbyte> left, Vector64<sbyte> right) => MultiplyAndAdd (acc, left, right);
    }
}
