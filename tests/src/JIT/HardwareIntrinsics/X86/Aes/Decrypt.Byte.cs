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
        private static void DecryptByte()
        {
            var test = new SimpleBinaryOpTest__DecryptByte();

            if (test.IsSupported)
            {
                // Validates basic functionality works, using Unsafe.Read
                test.RunBasicScenario_UnsafeRead();

                if (Aes.IsSupported)
                {
                    // Validates basic functionality works, using Load
                    test.RunBasicScenario_Load();

                    // Validates basic functionality works, using LoadAligned
                    test.RunBasicScenario_LoadAligned();
                }

                // Validates calling via reflection works, using Unsafe.Read
                test.RunReflectionScenario_UnsafeRead();

                if (Aes.IsSupported)
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

                if (Aes.IsSupported)
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

    public sealed unsafe class SimpleBinaryOpTest__DecryptByte
    {
        private struct TestStruct
        {
            public Vector128<Byte> _fld1;
            public Vector128<Byte> _fld2;

            public static TestStruct Create()
            {
                var testStruct = new TestStruct();

                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref testStruct._fld1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref testStruct._fld2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

                return testStruct;
            }

            public void RunStructFldScenario(SimpleBinaryOpTest__DecryptByte testClass)
            {
                var result = Aes.Decrypt(_fld1, _fld2);

                Unsafe.Write(testClass._dataTable.outArrayPtr, result);
                testClass.ValidateResult(testClass._dataTable.outArrayPtr);
            }
        }

        private static readonly int LargestVectorSize = 16;

        private static readonly int RetElementCount = Unsafe.SizeOf<Vector128<Byte>>() / sizeof(Byte);
        

        private static Byte[] _data1 = new Byte[16] {0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, 0xff, 0xee, 0xdd, 0xcc, 0xbb, 0xaa, 0x99, 0x88};
        private static Byte[] _data2 = new Byte[16] {0xff, 0xdd, 0xbb, 0x99, 0x77, 0x55, 0x33, 0x11, 0xee, 0xcc, 0xaa, 0x88, 0x66, 0x44, 0x22, 0x00};
        private static Byte[] _expectedRet = new Byte[16] {0x8f, 0xc4, 0xfe, 0x76, 0x51, 0x4f, 0x4e, 0x04, 0xee, 0x39, 0xda, 0x81, 0xa3, 0xcf, 0x7e, 0xb5};

        private static Vector128<Byte> _clsVar1;
        private static Vector128<Byte> _clsVar2;

        private Vector128<Byte> _fld1;
        private Vector128<Byte> _fld2;

        private SimpleBinaryOpTest__DataTable<Byte, Byte, Byte> _dataTable;

        static SimpleBinaryOpTest__DecryptByte()
        {

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _clsVar1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _clsVar2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());
        }

        public SimpleBinaryOpTest__DecryptByte()
        {
            Succeeded = true;

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _fld1), ref Unsafe.As<Byte, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<Byte>, byte>(ref _fld2), ref Unsafe.As<Byte, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            _dataTable = new SimpleBinaryOpTest__DataTable<Byte, Byte, Byte>(_data1, _data2, new Byte[RetElementCount], LargestVectorSize);
        }

        public bool IsSupported => Aes.IsSupported;

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_UnsafeRead));

            var result = Aes.Decrypt(
                Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr),
                Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr)
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunBasicScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_Load));

            var result = Aes.Decrypt(
                Aes.LoadVector128((Byte*)(_dataTable.inArray1Ptr)),
                Aes.LoadVector128((Byte*)(_dataTable.inArray2Ptr))
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunBasicScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunBasicScenario_LoadAligned));

            var result = Aes.Decrypt(
                Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr)),
                Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr))
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_UnsafeRead));

            var result = typeof(Aes).GetMethod(nameof(Aes.Decrypt), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) })
                                     .Invoke(null, new object[] {
                                        Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr),
                                        Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr)
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector128<Byte>)(result));
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_Load));

            var result = typeof(Aes).GetMethod(nameof(Aes.Decrypt), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) })
                                     .Invoke(null, new object[] {
                                        Aes.LoadVector128((Byte*)(_dataTable.inArray1Ptr)),
                                        Aes.LoadVector128((Byte*)(_dataTable.inArray2Ptr))
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector128<Byte>)(result));
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunReflectionScenario_LoadAligned));

            var result = typeof(Aes).GetMethod(nameof(Aes.Decrypt), new Type[] { typeof(Vector128<Byte>), typeof(Vector128<Byte>) })
                                     .Invoke(null, new object[] {
                                        Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr)),
                                        Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr))
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector128<Byte>)(result));
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunClsVarScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClsVarScenario));

            var result = Aes.Decrypt(
                _clsVar1,
                _clsVar2
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_UnsafeRead));

            var left = Unsafe.Read<Vector128<Byte>>(_dataTable.inArray1Ptr);
            var right = Unsafe.Read<Vector128<Byte>>(_dataTable.inArray2Ptr);
            var result = Aes.Decrypt(left, right);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_Load()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_Load));

            var left = Aes.LoadVector128((Byte*)(_dataTable.inArray1Ptr));
            var right = Aes.LoadVector128((Byte*)(_dataTable.inArray2Ptr));
            var result = Aes.Decrypt(left, right);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_LoadAligned()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunLclVarScenario_LoadAligned));

            var left = Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray1Ptr));
            var right = Aes.LoadAlignedVector128((Byte*)(_dataTable.inArray2Ptr));
            var result = Aes.Decrypt(left, right);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunClassLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassLclFldScenario));

            var test = new SimpleBinaryOpTest__DecryptByte();
            var result = Aes.Decrypt(test._fld1, test._fld2);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunClassFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunClassFldScenario));

            var result = Aes.Decrypt(_fld1, _fld2);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
        }

        public void RunStructLclFldScenario()
        {
            TestLibrary.TestFramework.BeginScenario(nameof(RunStructLclFldScenario));

            var test = TestStruct.Create();
            var result = Aes.Decrypt(test._fld1, test._fld2);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.outArrayPtr);
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

        private void ValidateResult(void* result, [CallerMemberName] string method = "")
        {

            Byte[] outArray = new Byte[RetElementCount];

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Byte, byte>(ref outArray[0]), ref Unsafe.AsRef<byte>(result), (uint)Unsafe.SizeOf<Vector128<Byte>>());

            ValidateResult(outArray, method);
        }

        
        private void ValidateResult(Byte[] result, [CallerMemberName] string method = "")
        {
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != _expectedRet[i] )
                {
                    Succeeded = false;
                } 
            }
            if (!Succeeded)
            {
                TestLibrary.TestFramework.LogInformation($"{nameof(Aes)}.{nameof(Aes.Decrypt)}<Byte>(Vector128<Byte>, Vector128<Byte>): {method} failed:");
                TestLibrary.TestFramework.LogInformation($"  expectedRet: ({string.Join(", ", _expectedRet)})");
                TestLibrary.TestFramework.LogInformation($"  result: ({string.Join(", ", result)})");
                TestLibrary.TestFramework.LogInformation(string.Empty);
            }
        }
    }
}
