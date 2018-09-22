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
        private static void ConvertToUInt32Vector128UInt32()
        {
            var test = new SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32();

            if (test.IsSupported)
            {
                // Validates basic functionality works, using Unsafe.Read
                test.RunBasicScenario_UnsafeRead();

                if (Sse2.IsSupported)
                {
                    // Validates basic functionality works, using Load
                    test.RunBasicScenario_Load();

                    // Validates basic functionality works, using LoadAligned
                    test.RunBasicScenario_LoadAligned();
                }

                // Validates calling via reflection works, using Unsafe.Read
                test.RunReflectionScenario_UnsafeRead();

                if (Sse2.IsSupported)
                {
                    // Validates calling via reflection works, using Load
                    test.RunReflectionScenario_Load();

                    // Validates calling via reflection works, using LoadAligned
                    test.RunReflectionScenario_LoadAligned();
                }

                // Validates passing a static member works
                test.RunClsVarScenario();

                // Validates passing a local works, using Unsafe.Read
                test.RunLclVarScenario_UnsafeRead();

                if (Sse2.IsSupported)
                {
                    // Validates passing a local works, using Load
                    test.RunLclVarScenario_Load();

                    // Validates passing a local works, using LoadAligned
                    test.RunLclVarScenario_LoadAligned();
                }

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

    public sealed unsafe class SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32
    {
        private struct TestStruct
        {
            public Vector128<UInt32> _fld;

            public static TestStruct Create()
            {
                var testStruct = new TestStruct();

                for (var i = 0; i < Op1ElementCount; i++) { _data[i] = TestLibrary.Generator.GetUInt32(); }
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref testStruct._fld), ref Unsafe.As<UInt32, byte>(ref _data[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());

                return testStruct;
            }

            public void RunStructFldScenario(SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32 testClass)
            {
                var result = Sse2.ConvertToUInt32(_fld);
                testClass.ValidateResult(_fld, result);
            }
        }

        private static readonly int LargestVectorSize = 16;

        private static readonly int Op1ElementCount = Unsafe.SizeOf<Vector128<UInt32>>() / sizeof(UInt32);

        private static UInt32[] _data = new UInt32[Op1ElementCount];

        private static Vector128<UInt32> _clsVar;

        private Vector128<UInt32> _fld;

        private SimdScalarUnaryOpTest__DataTable<UInt32> _dataTable;

        static SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32()
        {
            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = TestLibrary.Generator.GetUInt32(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref _clsVar), ref Unsafe.As<UInt32, byte>(ref _data[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());
        }

        public SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32()
        {
            Succeeded = true;

            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = TestLibrary.Generator.GetUInt32(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref _fld), ref Unsafe.As<UInt32, byte>(ref _data[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());

            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = TestLibrary.Generator.GetUInt32(); }
            _dataTable = new SimdScalarUnaryOpTest__DataTable<UInt32>(_data, LargestVectorSize);
        }

        public bool IsSupported => Sse2.IsSupported && (Environment.Is64BitProcess || ((typeof(UInt32) != typeof(long)) && (typeof(UInt32) != typeof(ulong))));

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_UnsafeRead));

            var result = Sse2.ConvertToUInt32(
                Unsafe.Read<Vector128<UInt32>>(_dataTable.inArrayPtr)
            );

            ValidateResult(_dataTable.inArrayPtr, result);
        }

        public void RunBasicScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_Load));

            var result = Sse2.ConvertToUInt32(
                Sse2.LoadVector128((UInt32*)(_dataTable.inArrayPtr))
            );

            ValidateResult(_dataTable.inArrayPtr, result);
        }

        public void RunBasicScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_LoadAligned));

            var result = Sse2.ConvertToUInt32(
                Sse2.LoadAlignedVector128((UInt32*)(_dataTable.inArrayPtr))
            );

            ValidateResult(_dataTable.inArrayPtr, result);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_UnsafeRead));

            var result = typeof(Sse2).GetMethod(nameof(Sse2.ConvertToUInt32), new Type[] { typeof(Vector128<UInt32>) })
                                     .Invoke(null, new object[] {
                                        Unsafe.Read<Vector128<UInt32>>(_dataTable.inArrayPtr)
                                     });

            ValidateResult(_dataTable.inArrayPtr, (UInt32)(result));
        }

        public void RunReflectionScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_Load));

            var result = typeof(Sse2).GetMethod(nameof(Sse2.ConvertToUInt32), new Type[] { typeof(Vector128<UInt32>) })
                                     .Invoke(null, new object[] {
                                        Sse2.LoadVector128((UInt32*)(_dataTable.inArrayPtr))
                                     });

            ValidateResult(_dataTable.inArrayPtr, (UInt32)(result));
        }

        public void RunReflectionScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_LoadAligned));

            var result = typeof(Sse2).GetMethod(nameof(Sse2.ConvertToUInt32), new Type[] { typeof(Vector128<UInt32>) })
                                     .Invoke(null, new object[] {
                                        Sse2.LoadAlignedVector128((UInt32*)(_dataTable.inArrayPtr))
                                     });

            ValidateResult(_dataTable.inArrayPtr, (UInt32)(result));
        }

        public void RunClsVarScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClsVarScenario));

            var result = Sse2.ConvertToUInt32(
                _clsVar
            );

            ValidateResult(_clsVar, result);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_UnsafeRead));

            var firstOp = Unsafe.Read<Vector128<UInt32>>(_dataTable.inArrayPtr);
            var result = Sse2.ConvertToUInt32(firstOp);

            ValidateResult(firstOp, result);
        }

        public void RunLclVarScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_Load));

            var firstOp = Sse2.LoadVector128((UInt32*)(_dataTable.inArrayPtr));
            var result = Sse2.ConvertToUInt32(firstOp);

            ValidateResult(firstOp, result);
        }

        public void RunLclVarScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_LoadAligned));

            var firstOp = Sse2.LoadAlignedVector128((UInt32*)(_dataTable.inArrayPtr));
            var result = Sse2.ConvertToUInt32(firstOp);

            ValidateResult(firstOp, result);
        }

        public void RunClassLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassLclFldScenario));

            var test = new SimdScalarUnaryOpConvertTest__ConvertToUInt32Vector128UInt32();
            var result = Sse2.ConvertToUInt32(test._fld);

            ValidateResult(test._fld, result);
        }

        public void RunClassFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassFldScenario));

            var result = Sse2.ConvertToUInt32(_fld);

            ValidateResult(_fld, result);
        }

        public void RunStructLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunStructLclFldScenario));

            var test = TestStruct.Create();
            var result = Sse2.ConvertToUInt32(test._fld);

            ValidateResult(test._fld, result);
        }

        public void RunStructFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunStructFldScenario));

            var test = TestStruct.Create();
            test.RunStructFldScenario(this);
        }

        public void RunUnsupportedScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunUnsupportedScenario));

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

        private void ValidateResult(Vector128<UInt32> firstOp, UInt32 result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray = new UInt32[Op1ElementCount];
            Unsafe.WriteUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray[0]), firstOp);
            ValidateResult(inArray, result, method);
        }

        private void ValidateResult(void* firstOp, UInt32 result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray = new UInt32[Op1ElementCount];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray[0]), ref Unsafe.AsRef<byte>(firstOp), (uint)Unsafe.SizeOf<Vector128<UInt32>>());
            ValidateResult(inArray, result, method);
        }

        private void ValidateResult(UInt32[] firstOp, UInt32 result, [CallerMemberName] string method = "")
        {
            if (firstOp[0] != result)
            {
                Succeeded = false;
            }

            if (!Succeeded)
            {
                TestLibrary.TestFramework.LogInformation($"{nameof(Sse2)}.{nameof(Sse2.ConvertToUInt32)}<UInt32>(Vector128<UInt32>): {method} failed:");
                TestLibrary.TestFramework.LogInformation($"  firstOp: ({string.Join(", ", firstOp)})");
                TestLibrary.TestFramework.LogInformation($"   result: result");
                TestLibrary.TestFramework.LogInformation(string.Empty);
            }
        }
    }
}
