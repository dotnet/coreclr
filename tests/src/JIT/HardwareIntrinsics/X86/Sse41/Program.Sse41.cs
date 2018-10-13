// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace JIT.HardwareIntrinsics.X86
{
    public static partial class Program
    {
        static Program()
        {
            TestList = new Dictionary<string, Action>() {
                ["BlendVariable.Byte"] = BlendVariableByte,
                ["BlendVariable.Double"] = BlendVariableDouble,
                ["BlendVariable.SByte"] = BlendVariableSByte,
                ["BlendVariable.Single"] = BlendVariableSingle,
                ["BlendVariable.Int16"] = BlendVariableInt16,
                ["BlendVariable.UInt16"] = BlendVariableUInt16,
                ["BlendVariable.Int32"] = BlendVariableInt32,
                ["BlendVariable.UInt32"] = BlendVariableUInt32,
                ["BlendVariable.Int64"] = BlendVariableInt64,
                ["BlendVariable.UInt64"] = BlendVariableUInt64,
                ["Ceiling.Double"] = CeilingDouble,
                ["Ceiling.Single"] = CeilingSingle,
                ["CeilingScalar.Double"] = CeilingScalarDouble,
                ["CeilingScalar.Single"] = CeilingScalarSingle,
                ["CompareEqual.Int64"] = CompareEqualInt64,
                ["CompareEqual.UInt64"] = CompareEqualUInt64,
                ["Extract.Byte.1"] = ExtractByte1,
                ["Extract.Int32.1"] = ExtractInt321,
                ["Extract.UInt32.1"] = ExtractUInt321,
                ["Extract.Int64.1"] = ExtractInt641,
                ["Extract.UInt64.1"] = ExtractUInt641,
                ["Extract.Single.1"] = ExtractSingle1,
                ["Extract.Byte.129"] = ExtractByte129,
                ["Extract.Int32.129"] = ExtractInt32129,
                ["Extract.UInt32.129"] = ExtractUInt32129,
                ["Extract.Int64.129"] = ExtractInt64129,
                ["Extract.UInt64.129"] = ExtractUInt64129,
                ["Extract.Single.129"] = ExtractSingle129,
                ["Floor.Double"] = FloorDouble,
                ["Floor.Single"] = FloorSingle,
                ["FloorScalar.Double"] = FloorScalarDouble,
                ["FloorScalar.Single"] = FloorScalarSingle,
                ["Insert.Single.0"] = InsertSingle0,
                ["Insert.Byte.1"] = InsertByte1,
                ["Insert.SByte.1"] = InsertSByte1,
                ["Insert.Int32.1"] = InsertInt321,
                ["Insert.UInt32.1"] = InsertUInt321,
                ["Insert.Int64.1"] = InsertInt641,
                ["Insert.UInt64.1"] = InsertUInt641,
                ["Insert.Single.1"] = InsertSingle1,
                ["Insert.Single.2"] = InsertSingle2,
                ["Insert.Single.4"] = InsertSingle4,
                ["Insert.Single.8"] = InsertSingle8,
                ["Insert.Single.16"] = InsertSingle16,
                ["Insert.Single.32"] = InsertSingle32,
                ["Insert.Single.48"] = InsertSingle48,
                ["Insert.Single.64"] = InsertSingle64,
                ["Insert.Single.128"] = InsertSingle128,
                ["Insert.Byte.129"] = InsertByte129,
                ["Insert.SByte.129"] = InsertSByte129,
                ["Insert.Int32.129"] = InsertInt32129,
                ["Insert.UInt32.129"] = InsertUInt32129,
                ["Insert.Int64.129"] = InsertInt64129,
                ["Insert.UInt64.129"] = InsertUInt64129,
                ["Insert.Single.129"] = InsertSingle129,
                ["Insert.Single.192"] = InsertSingle192,
                ["Max.Int32"] = MaxInt32,
                ["Max.SByte"] = MaxSByte,
                ["Max.UInt16"] = MaxUInt16,
                ["Max.UInt32"] = MaxUInt32,
                ["Min.Int32"] = MinInt32,
                ["Min.SByte"] = MinSByte,
                ["Min.UInt16"] = MinUInt16,
                ["Min.UInt32"] = MinUInt32,
                ["MultiplyLow.Int32"] = MultiplyLowInt32,
                ["MultiplyLow.UInt32"] = MultiplyLowUInt32,
                ["PackUnsignedSaturate.UInt16"] = PackUnsignedSaturateUInt16,
                ["RoundCurrentDirection.Double"] = RoundCurrentDirectionDouble,
                ["RoundCurrentDirection.Single"] = RoundCurrentDirectionSingle,
                ["RoundCurrentDirectionScalar.Double"] = RoundCurrentDirectionScalarDouble,
                ["RoundCurrentDirectionScalar.Single"] = RoundCurrentDirectionScalarSingle,
                ["RoundToNearestInteger.Double"] = RoundToNearestIntegerDouble,
                ["RoundToNearestInteger.Single"] = RoundToNearestIntegerSingle,
                ["RoundToNearestIntegerScalar.Double"] = RoundToNearestIntegerScalarDouble,
                ["RoundToNearestIntegerScalar.Single"] = RoundToNearestIntegerScalarSingle,
                ["RoundToNegativeInfinity.Double"] = RoundToNegativeInfinityDouble,
                ["RoundToNegativeInfinity.Single"] = RoundToNegativeInfinitySingle,
                ["RoundToNegativeInfinityScalar.Double"] = RoundToNegativeInfinityScalarDouble,
                ["RoundToNegativeInfinityScalar.Single"] = RoundToNegativeInfinityScalarSingle,
                ["RoundToPositiveInfinity.Double"] = RoundToPositiveInfinityDouble,
                ["RoundToPositiveInfinity.Single"] = RoundToPositiveInfinitySingle,
                ["RoundToPositiveInfinityScalar.Double"] = RoundToPositiveInfinityScalarDouble,
                ["RoundToPositiveInfinityScalar.Single"] = RoundToPositiveInfinityScalarSingle,
                ["RoundToZero.Double"] = RoundToZeroDouble,
                ["RoundToZero.Single"] = RoundToZeroSingle,
                ["RoundToZeroScalar.Double"] = RoundToZeroScalarDouble,
                ["RoundToZeroScalar.Single"] = RoundToZeroScalarSingle,
                ["TestAllOnes.Byte"] = TestAllOnesByte,
                ["TestAllOnes.Int16"] = TestAllOnesInt16,
                ["TestAllOnes.Int32"] = TestAllOnesInt32,
                ["TestAllOnes.Int64"] = TestAllOnesInt64,
                ["TestAllOnes.SByte"] = TestAllOnesSByte,
                ["TestAllOnes.UInt16"] = TestAllOnesUInt16,
                ["TestAllOnes.UInt32"] = TestAllOnesUInt32,
                ["TestAllOnes.UInt64"] = TestAllOnesUInt64,
                ["TestAllZeros.Byte"] = TestAllZerosByte,
                ["TestAllZeros.Int16"] = TestAllZerosInt16,
                ["TestAllZeros.Int32"] = TestAllZerosInt32,
                ["TestAllZeros.Int64"] = TestAllZerosInt64,
                ["TestAllZeros.SByte"] = TestAllZerosSByte,
                ["TestAllZeros.UInt16"] = TestAllZerosUInt16,
                ["TestAllZeros.UInt32"] = TestAllZerosUInt32,
                ["TestAllZeros.UInt64"] = TestAllZerosUInt64,
                ["TestC.Byte"] = TestCByte,
                ["TestC.Int16"] = TestCInt16,
                ["TestC.Int32"] = TestCInt32,
                ["TestC.Int64"] = TestCInt64,
                ["TestC.SByte"] = TestCSByte,
                ["TestC.UInt16"] = TestCUInt16,
                ["TestC.UInt32"] = TestCUInt32,
                ["TestC.UInt64"] = TestCUInt64,
                ["TestMixOnesZeros.Byte"] = TestMixOnesZerosByte,
                ["TestMixOnesZeros.Int16"] = TestMixOnesZerosInt16,
                ["TestMixOnesZeros.Int32"] = TestMixOnesZerosInt32,
                ["TestMixOnesZeros.Int64"] = TestMixOnesZerosInt64,
                ["TestMixOnesZeros.SByte"] = TestMixOnesZerosSByte,
                ["TestMixOnesZeros.UInt16"] = TestMixOnesZerosUInt16,
                ["TestMixOnesZeros.UInt32"] = TestMixOnesZerosUInt32,
                ["TestMixOnesZeros.UInt64"] = TestMixOnesZerosUInt64,
                ["TestNotZAndNotC.Byte"] = TestNotZAndNotCByte,
                ["TestNotZAndNotC.Int16"] = TestNotZAndNotCInt16,
                ["TestNotZAndNotC.Int32"] = TestNotZAndNotCInt32,
                ["TestNotZAndNotC.Int64"] = TestNotZAndNotCInt64,
                ["TestNotZAndNotC.SByte"] = TestNotZAndNotCSByte,
                ["TestNotZAndNotC.UInt16"] = TestNotZAndNotCUInt16,
                ["TestNotZAndNotC.UInt32"] = TestNotZAndNotCUInt32,
                ["TestNotZAndNotC.UInt64"] = TestNotZAndNotCUInt64,
                ["TestZ.Byte"] = TestZByte,
                ["TestZ.Int16"] = TestZInt16,
                ["TestZ.Int32"] = TestZInt32,
                ["TestZ.Int64"] = TestZInt64,
                ["TestZ.SByte"] = TestZSByte,
                ["TestZ.UInt16"] = TestZUInt16,
                ["TestZ.UInt32"] = TestZUInt32,
                ["TestZ.UInt64"] = TestZUInt64,
            };
        }
    }
}
