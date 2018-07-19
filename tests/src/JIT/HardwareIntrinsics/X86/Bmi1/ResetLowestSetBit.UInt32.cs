// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************************************************************
 * This file is auto-generated from a template file by the GenerateTests.csx  *
 * script in tests\src\JIT\HardwareIntrinsics\X86\Shared. In order to make    *
 * changes, please update the corresponding template and run according to the *
 * directions listed in the file.                                             *
 ******************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace JIT.HardwareIntrinsics.X86
{
    public static partial class Program
    {
        private static void ResetLowestSetBitUInt32()
        {
            var test = new ScalarUnaryOpTest__ResetLowestSetBitUInt32();

            if (test.IsSupported)
            {
                // Validates basic functionality works, using Unsafe.ReadUnaligned
                test.RunBasicScenario_UnsafeRead();

                // Validates calling via reflection works, using Unsafe.ReadUnaligned
                test.RunReflectionScenario_UnsafeRead();

                // Validates passing a static member works
                test.RunClsVarScenario();

                // Validates passing a local works, using Unsafe.ReadUnaligned
                test.RunLclVarScenario_UnsafeRead();

                // Validates passing the field of a local class works
                test.RunClassLclFldScenario();

                // Validates passing an instance member of a class works
                test.RunClassFldScenario();

                // Validates passing the field of a local struct works
                test.RunStructLclFldScenario();

                // Validates passing an instance member of a struct works
                test.RunStructFldScenario();
            }
            else
            {
                // Validates we throw on unsupported hardware
                test.RunUnsupportedScenario();
            }

            if (!test.Succeeded)
            {
                throw new Exception("One or more scenarios did not complete as expected.");
            }
        }
    }

    public sealed unsafe class ScalarUnaryOpTest__ResetLowestSetBitUInt32
    {
        private struct TestStruct
        {
            public UInt32 _fld;

            public static TestStruct Create()
            {
                var testStruct = new TestStruct();
                var random = new Random();

                testStruct._fld = (uint)(random.Next(0, int.MaxValue));
                return testStruct;
            }

            public void RunStructFldScenario(ScalarUnaryOpTest__ResetLowestSetBitUInt32 testClass)
            {
                var result = Bmi1.ResetLowestSetBit(_fld);
                testClass.ValidateResult(_fld, result);
            }
        }

        private static UInt32 _data;

        private static UInt32 _clsVar;

        private UInt32 _fld;

        static ScalarUnaryOpTest__ResetLowestSetBitUInt32()
        {
            var random = new Random();
            _clsVar = (uint)(random.Next(0, int.MaxValue));
        }

        public ScalarUnaryOpTest__ResetLowestSetBitUInt32()
        {
            Succeeded = true;

            var random = new Random();
            
            _fld = (uint)(random.Next(0, int.MaxValue));
            _data = (uint)(random.Next(0, int.MaxValue));
        }

        public bool IsSupported => Bmi1.IsSupported && (Environment.Is64BitProcess || ((typeof(UInt32) != typeof(long)) && (typeof(UInt32) != typeof(ulong))));

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            var result = Bmi1.ResetLowestSetBit(
                Unsafe.ReadUnaligned<UInt32>(ref Unsafe.As<UInt32, byte>(ref _data))
            );

            ValidateResult(_data, result);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            var result = typeof(Bmi1).GetMethod(nameof(Bmi1.ResetLowestSetBit), new Type[] { typeof(UInt32) })
                                     .Invoke(null, new object[] {
                                        Unsafe.ReadUnaligned<UInt32>(ref Unsafe.As<UInt32, byte>(ref _data))
                                     });

            ValidateResult(_data, (UInt32)result);
        }

        public void RunClsVarScenario()
        {
            var result = Bmi1.ResetLowestSetBit(
                _clsVar
            );

            ValidateResult(_clsVar, result);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            var data = Unsafe.ReadUnaligned<UInt32>(ref Unsafe.As<UInt32, byte>(ref _data));
            var result = Bmi1.ResetLowestSetBit(data);

            ValidateResult(data, result);
        }

        public void RunClassLclFldScenario()
        {
            var test = new ScalarUnaryOpTest__ResetLowestSetBitUInt32();
            var result = Bmi1.ResetLowestSetBit(test._fld);

            ValidateResult(test._fld, result);
        }

        public void RunClassFldScenario()
        {
            var result = Bmi1.ResetLowestSetBit(_fld);
            ValidateResult(_fld, result);
        }

        public void RunStructLclFldScenario()
        {
            var test = TestStruct.Create();
            var result = Bmi1.ResetLowestSetBit(test._fld);

            ValidateResult(test._fld, result);
        }

        public void RunStructFldScenario()
        {
            var test = TestStruct.Create();
            test.RunStructFldScenario(this);
        }

        public void RunUnsupportedScenario()
        {
            Succeeded = false;

            try
            {
                RunBasicScenario_UnsafeRead();
            }
            catch (PlatformNotSupportedException)
            {
                Succeeded = true;
            }
        }

        private void ValidateResult(UInt32 data, UInt32 result, [CallerMemberName] string method = "")
        {
            var isUnexpectedResult = false;

            isUnexpectedResult = (((data - 1) & data) != result);

            if (isUnexpectedResult)
            {
                TestLibrary.TestFramework.LogInformation($"{nameof(Bmi1)}.{nameof(Bmi1.ResetLowestSetBit)}<UInt32>(UInt32): ResetLowestSetBit failed:");
                TestLibrary.TestFramework.LogInformation($"    data: {data}");
                TestLibrary.TestFramework.LogInformation($"  result: {result}");
                TestLibrary.TestFramework.LogInformation(string.Empty);
            }
        }
    }
}
