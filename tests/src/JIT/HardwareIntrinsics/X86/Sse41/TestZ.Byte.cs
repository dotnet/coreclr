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
        private static void TestZByte()
        {
            var test = new BooleanBinaryOpTest__TestZByte();

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

    public sealed unsafe class BooleanBinaryOpTest__TestZByte
    {
        private struct TestStruct
        {
            public Vector128<Byte> _fld1;
            public Vector128<Byte> _fld2;

            public static TestStruct Create()
            {
                var testStruct = new TestStruct();

                for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = TestLibrary.Generator.GetByte(); }
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref testStruct._fld1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());
                for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = TestLibrary.Generator.GetByte(); }
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref testStruct._fld2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

                return testStruct;
            }

            public void RunStructFldScenario(BooleanBinaryOpTest__TestZByte testClass)
            {
                var result = Sse41.TestZ(_fld1, _fld2);
                testClass.ValidateResult(_fld1, _fld2, result);
            }
        }

        private static readonly int LargestVectorSize = 16;

        private static readonly int Op1ElementCount = Unsafe.SizeOf<Vector128<Byte>>() / sizeof(Byte);
        private static readonly int Op2ElementCount = Unsafe.SizeOf<Vector128<Byte>>() / sizeof(Byte);

        private static Byte[] _data1 = new Byte[Op1ElementCount];
        private static Byte[] _data2 = new Byte[Op2ElementCount];

        private static Vector128<Byte> _clsVar1;
        private static Vector128<Byte> _clsVar2;

        private Vector128<Byte> _fld1;
        private Vector128<Byte> _fld2;

        private BooleanBinaryOpTest__DataTable<Byte, Byte> _dataTable;

        static BooleanBinaryOpTest__TestZByte()
        {
            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = TestLibrary.Generator.GetByte(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _clsVar1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = TestLibrary.Generator.GetByte(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _clsVar2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());
        }

        public BooleanBinaryOpTest__TestZByte()
        {
            Succeeded = true;

            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = TestLibrary.Generator.GetByte(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _fld1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = TestLibrary.Generator.GetByte(); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _fld2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = TestLibrary.Generator.GetByte(); }
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = TestLibrary.Generator.GetByte(); }
            _dataTable = new BooleanBinaryOpTest__DataTable<Byte, Byte>(_data1, _data2, LargestVectorSize);
        }

        public bool IsSupported => Sse41.IsSupported;

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_UnsafeRead));

            var result = Sse41.TestZ(
                Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr),
                Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr)
            );

            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, result);
        }

        public void RunBasicScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_Load));

            var result = Sse41.TestZ(
                Sse2.LoadVector128((Byte*)(_dataTable.inArray1Ptr)),
                Sse2.LoadVector128((Byte*)(_dataTable.inArray2Ptr))
            );

            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, result);
        }

        public void RunBasicScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_LoadAligned));

            var result = Sse41.TestZ(
                Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr)),
                Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr))
            );

            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, result);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_UnsafeRead));

            var method = typeof(Sse41).GetMethod(nameof(Sse41.TestZ), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) });

            if (method != null)
            {
                var result = method.Invoke(null, new object[] {
                                        Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr),
                                        Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr)
                                     });

                ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, (bool)(result));
            }
        }

        public void RunReflectionScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_Load));

            var method = typeof(Sse41).GetMethod(nameof(Sse41.TestZ), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) });

            if (method != null)
            {
                var result = method.Invoke(null, new object[] {
                                        Sse2.LoadVector128((Byte*)(_dataTable.inArray1Ptr)),
                                        Sse2.LoadVector128((Byte*)(_dataTable.inArray2Ptr))
                                     });

                ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, (bool)(result));
            }
        }

        public void RunReflectionScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_LoadAligned));

            var method = typeof(Sse41).GetMethod(nameof(Sse41.TestZ), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) });

            if (method != null)
            {
                var result = method.Invoke(null, new object[] {
                                        Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr)),
                                        Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr))
                                     });

                ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, (bool)(result));
            }
        }

        public void RunClsVarScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClsVarScenario));

            var result = Sse41.TestZ(
                _clsVar1,
                _clsVar2
            );

            ValidateResult(_clsVar1, _clsVar2, result);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_UnsafeRead));

            var left = Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr);
            var right = Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr);
            var result = Sse41.TestZ(left, right);

            ValidateResult(left, right, result);
        }

        public void RunLclVarScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_Load));

            var left = Sse2.LoadVector128((Byte*)(_dataTable.inArray1Ptr));
            var right = Sse2.LoadVector128((Byte*)(_dataTable.inArray2Ptr));
            var result = Sse41.TestZ(left, right);

            ValidateResult(left, right, result);
        }

        public void RunLclVarScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_LoadAligned));

            var left = Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr));
            var right = Sse2.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr));
            var result = Sse41.TestZ(left, right);

            ValidateResult(left, right, result);
        }

        public void RunClassLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassLclFldScenario));

            var test = new BooleanBinaryOpTest__TestZByte();
            var result = Sse41.TestZ(test._fld1, test._fld2);

            ValidateResult(test._fld1, test._fld2, result);
        }

        public void RunClassFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassFldScenario));

            var result = Sse41.TestZ(_fld1, _fld2);

            ValidateResult(_fld1, _fld2, result);
        }

        public void RunStructLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunStructLclFldScenario));

            var test = TestStruct.Create();
            var result = Sse41.TestZ(test._fld1, test._fld2);
            ValidateResult(test._fld1, test._fld2, result);
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

        private void ValidateResult(Vector128<Byte> left, Vector128<Byte> right, bool result, [CallerMemberName] string method = "")
        {
            Byte[] inArray1 = new Byte[Op1ElementCount];
            Byte[] inArray2 = new Byte[Op2ElementCount];

            Unsafe.WriteUnaligned(ref Unsafe.As<Byte, byte>(ref inArray1[0]), left);
            Unsafe.WriteUnaligned(ref Unsafe.As<Byte, byte>(ref inArray2[0]), right);

            ValidateResult(inArray1, inArray2, result, method);
        }

        private void ValidateResult(void* left, void* right, bool result, [CallerMemberName] string method = "")
        {
            Byte[] inArray1 = new Byte[Op1ElementCount];
            Byte[] inArray2 = new Byte[Op2ElementCount];

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Byte, byte>(ref inArray1[0]), ref Unsafe.AsRef<byte>(left), (uint)Unsafe.SizeOf<Vector128<Byte>>());
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Byte, byte>(ref inArray2[0]), ref Unsafe.AsRef<byte>(right), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            ValidateResult(inArray1, inArray2, result, method);
        }

        private void ValidateResult(Byte[] left, Byte[] right, bool result, [CallerMemberName] string method = "")
        {
            var expectedResult = true;

            for (var i = 0; i < Op1ElementCount; i++)
            {
                expectedResult &= ((left[i] & right[i]) == 0);
            }

            if (expectedResult != result)
            {
                Succeeded = false;

                TestLibrary.TestFramework.LogInformation($"{nameof(Sse41)}.{nameof(Sse41.TestZ)}<Byte>(Vector128<Byte>, Vector128<Byte>): {method} failed:");
                TestLibrary.TestFramework.LogInformation($"    left: ({string.Join(", ", left)})");
                TestLibrary.TestFramework.LogInformation($"   right: ({string.Join(", ", right)})");
                TestLibrary.TestFramework.LogInformation($"  result: ({string.Join(", ", result)})");
                TestLibrary.TestFramework.LogInformation(string.Empty);
            }
        }
    }
}
