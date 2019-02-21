// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************************************************************
 * This file is auto-generated from a template file by the GenerateTests.csx  *
 * script in tests\src\JIT\HardwareIntrinsics\General\Shared. In order to make    *
 * changes, please update the corresponding template and run according to the *
 * directions listed in the file.                                             *
 ******************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace JIT.HardwareIntrinsics.General
{
    public static partial class Program
    {
        private static void AsUInt64()
        {
            var test = new VectorAs__AsUInt64();

            // Validates basic functionality works
            test.RunBasicScenario();

            // Validates basic functionality works using the generic form, rather than the type-specific form of the method
            test.RunGenericScenario();

            // Validates calling via reflection works
            test.RunReflectionScenario();

            if (!test.Succeeded)
            {
                throw new Exception("One or more scenarios did not complete as expected.");
            }
        }
    }

    public sealed unsafe class VectorAs__AsUInt64
    {
        private static readonly int LargestVectorSize = 16;

        private static readonly int ElementCount = Unsafe.SizeOf<Vector128<UInt64>>() / sizeof(UInt64);

        public bool Succeeded { get; set; } = true;

        public void RunBasicScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario));
            Vector128<UInt64> value;

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<byte> byteResult = value.AsByte();
            ValidateResult(byteResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<double> doubleResult = value.AsDouble();
            ValidateResult(doubleResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<short> shortResult = value.AsInt16();
            ValidateResult(shortResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<int> intResult = value.AsInt32();
            ValidateResult(intResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<long> longResult = value.AsInt64();
            ValidateResult(longResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<sbyte> sbyteResult = value.AsSByte();
            ValidateResult(sbyteResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<float> floatResult = value.AsSingle();
            ValidateResult(floatResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<ushort> ushortResult = value.AsUInt16();
            ValidateResult(ushortResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<uint> uintResult = value.AsUInt32();
            ValidateResult(uintResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<ulong> ulongResult = value.AsUInt64();
            ValidateResult(ulongResult, value);
        }

        public void RunGenericScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunGenericScenario));
            Vector128<UInt64> value;

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<byte> byteResult = value.As<UInt64, byte>();
            ValidateResult(byteResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<double> doubleResult = value.As<UInt64, double>();
            ValidateResult(doubleResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<short> shortResult = value.As<UInt64, short>();
            ValidateResult(shortResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<int> intResult = value.As<UInt64, int>();
            ValidateResult(intResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<long> longResult = value.As<UInt64, long>();
            ValidateResult(longResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<sbyte> sbyteResult = value.As<UInt64, sbyte>();
            ValidateResult(sbyteResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<float> floatResult = value.As<UInt64, float>();
            ValidateResult(floatResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<ushort> ushortResult = value.As<UInt64, ushort>();
            ValidateResult(ushortResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<uint> uintResult = value.As<UInt64, uint>();
            ValidateResult(uintResult, value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            Vector128<ulong> ulongResult = value.As<UInt64, ulong>();
            ValidateResult(ulongResult, value);
        }

        public void RunReflectionScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario));
            Vector128<UInt64> value;

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object byteResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsByte))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<byte>)(byteResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object doubleResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsDouble))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<double>)(doubleResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object shortResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsInt16))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<short>)(shortResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object intResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsInt32))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<int>)(intResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object longResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsInt64))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<long>)(longResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object sbyteResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsSByte))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<sbyte>)(sbyteResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object floatResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsSingle))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<float>)(floatResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object ushortResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsUInt16))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<ushort>)(ushortResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object uintResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsUInt32))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<uint>)(uintResult), value);

            value = Vector128.Create(TestLibrary.Generator.GetUInt64());
            object ulongResult = typeof(Vector128)
                                    .GetMethod(nameof(Vector128.AsUInt64))
                                    .MakeGenericMethod(typeof(UInt64))
                                    .Invoke(null, new object[] { value });
            ValidateResult((Vector128<ulong>)(ulongResult), value);
        }

        private void ValidateResult<T>(Vector128<T> result, Vector128<UInt64> value, [CallerMemberName] string method = "")
            where T : struct
        {
            UInt64[] resultElements = new UInt64[ElementCount];
            Unsafe.WriteUnaligned(ref Unsafe.As<UInt64, byte>(ref resultElements[0]), result);

            UInt64[] valueElements = new UInt64[ElementCount];
            Unsafe.WriteUnaligned(ref Unsafe.As<UInt64, byte>(ref valueElements[0]), value);

            ValidateResult(resultElements, valueElements, typeof(T), method);
        }

        private void ValidateResult(UInt64[] resultElements, UInt64[] valueElements, Type targetType, [CallerMemberName] string method = "")
        {
            bool succeeded = true;

            for (var i = 0; i < ElementCount; i++)
            {
                if (resultElements[i] != valueElements[i])
                {
                    succeeded = false;
                    break;
                }
            }

            if (!succeeded)
            {
                TestLibrary.TestFramework.LogInformation($"Vector128<UInt64>.As{targetType.Name}: {method} failed:");
                TestLibrary.TestFramework.LogInformation($"   value: ({string.Join(", ", valueElements)})");
                TestLibrary.TestFramework.LogInformation($"  result: ({string.Join(", ", resultElements)})");
                TestLibrary.TestFramework.LogInformation(string.Empty);

                Succeeded = false;
            }
        }
    }
}
