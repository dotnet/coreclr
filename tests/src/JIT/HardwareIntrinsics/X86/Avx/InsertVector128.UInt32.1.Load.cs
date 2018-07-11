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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.Intrinsics.X86.Sse2;

namespace JIT.HardwareIntrinsics.X86
{
    public static partial class Program
    {
        private static void InsertVector128UInt321Load()
        {
            var test = new InsertLoadTest__InsertVector128UInt321();

            if (test.IsSupported)
            {
                // Validates basic functionality works, using Unsafe.Read
                test.RunBasicScenario_UnsafeRead();

                if (Avx.IsSupported)
                {
                    // Validates basic functionality works, using Load
                    test.RunBasicScenario_Load();

                    // Validates basic functionality works, using LoadAligned
                    test.RunBasicScenario_LoadAligned();
                }

                // Validates calling via reflection works, using Unsafe.Read
                test.RunReflectionScenario_UnsafeRead();

                if (Avx.IsSupported)
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

                if (Avx.IsSupported)
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

    public sealed unsafe class InsertLoadTest__InsertVector128UInt321
    {
        private struct TestStruct
        {
            public Vector256<UInt32> _fld1;
            public Vector128<UInt32> _fld2;

            public static TestStruct Create()
            {
                var testStruct = new TestStruct();
                var random = new Random();

                for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = (uint)(random.Next(0, int.MaxValue)); }
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector256<UInt32>, byte>(ref testStruct._fld1), ref Unsafe.As<UInt32, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector256<UInt32>>());
                for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = (uint)(random.Next(0,int.MaxValue)); }
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref testStruct._fld2), ref Unsafe.As<UInt32, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());

                return testStruct;
            }

            public void RunStructFldScenario(InsertLoadTest__InsertVector128UInt321 testClass)
            {
                var result = Avx.InsertVector128(_fld1, (UInt32*)testClass._dataTable.inArray2Ptr, 1);

                Unsafe.Write(testClass._dataTable.outArrayPtr, result);
                testClass.ValidateResult(_fld1, (UInt32*)testClass._dataTable.inArray2Ptr, testClass._dataTable.outArrayPtr);
            }
        }

        private static readonly int LargestVectorSize = 32;

        private static readonly int Op1ElementCount = Unsafe.SizeOf<Vector256<UInt32>>() / sizeof(UInt32);
        private static readonly int Op2ElementCount = Unsafe.SizeOf<Vector128<UInt32>>() / sizeof(UInt32);
        private static readonly int RetElementCount = Unsafe.SizeOf<Vector256<UInt32>>() / sizeof(UInt32);

        private static UInt32[] _data1 = new UInt32[Op1ElementCount];
        private static UInt32[] _data2 = new UInt32[Op2ElementCount];

        private static Vector256<UInt32> _clsVar1;
        private static Vector128<UInt32> _clsVar2;

        private Vector256<UInt32> _fld1;
        private Vector128<UInt32> _fld2;

        private SimpleBinaryOpTest__DataTable<UInt32, UInt32, UInt32> _dataTable;

        static InsertLoadTest__InsertVector128UInt321()
        {
            var random = new Random();

            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = (uint)(random.Next(0, int.MaxValue)); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector256<UInt32>, byte>(ref _clsVar1), ref Unsafe.As<UInt32, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector256<UInt32>>());
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = (uint)(random.Next(0,int.MaxValue)); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref _clsVar2), ref Unsafe.As<UInt32, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());
        }

        public InsertLoadTest__InsertVector128UInt321()
        {
            Succeeded = true;

            var random = new Random();

            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = (uint)(random.Next(0, int.MaxValue)); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector256<UInt32>, byte>(ref _fld1), ref Unsafe.As<UInt32, byte>(ref _data1[0]), (uint)Unsafe.SizeOf<Vector256<UInt32>>());
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = (uint)(random.Next(0,int.MaxValue)); }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<UInt32>, byte>(ref _fld2), ref Unsafe.As<UInt32, byte>(ref _data2[0]), (uint)Unsafe.SizeOf<Vector128<UInt32>>());

            for (var i = 0; i < Op1ElementCount; i++) { _data1[i] = (uint)(random.Next(0, int.MaxValue)); }
            for (var i = 0; i < Op2ElementCount; i++) { _data2[i] = (uint)(random.Next(0,int.MaxValue)); }
            _dataTable = new SimpleBinaryOpTest__DataTable<UInt32, UInt32, UInt32>(_data1, _data2, new UInt32[RetElementCount], LargestVectorSize);
        }

        public bool IsSupported => Avx.IsSupported;

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            var result = Avx.InsertVector128(
                Unsafe.Read<Vector256<UInt32>>(_dataTable.inArray1Ptr),
                (UInt32*)_dataTable.inArray2Ptr,
                1
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunBasicScenario_Load()
        {
            var result = Avx.InsertVector128(
                Avx.LoadVector256((UInt32*)(_dataTable.inArray1Ptr)),
                (UInt32*)(_dataTable.inArray2Ptr),
                1
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunBasicScenario_LoadAligned()
        {
            var result = Avx.InsertVector128(
                Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArray1Ptr)),
                (UInt32*)(_dataTable.inArray2Ptr),
                1
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.InsertVector128), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32*), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Unsafe.Read<Vector256<UInt32>>(_dataTable.inArray1Ptr),
                                        Pointer.Box(_dataTable.inArray2Ptr, typeof(UInt32*)),
                                        (byte)1
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_Load()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.InsertVector128), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32*), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Avx.LoadVector256((UInt32*)(_dataTable.inArray1Ptr)),
                                        Pointer.Box(_dataTable.inArray2Ptr, typeof(UInt32*)),
                                        (byte)1
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_LoadAligned()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.InsertVector128), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32*), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArray1Ptr)),
                                        Pointer.Box(_dataTable.inArray2Ptr, typeof(UInt32*)),
                                        (byte)1
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArray1Ptr, _dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunClsVarScenario()
        {
            var result = Avx.InsertVector128(
                _clsVar1,
                (UInt32*)_dataTable.inArray2Ptr,
                1
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_clsVar1, (UInt32*)_dataTable.inArray2Ptr, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            var left = Unsafe.Read<Vector256<UInt32>>(_dataTable.inArray1Ptr);
            var right = (UInt32*)_dataTable.inArray2Ptr;
            var result = Avx.InsertVector128(left, right, 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(left, right, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_Load()
        {
            var left = Avx.LoadVector256((UInt32*)(_dataTable.inArray1Ptr));
            var right = (UInt32*)(_dataTable.inArray2Ptr);
            var result = Avx.InsertVector128(left, right, 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(left, right, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_LoadAligned()
        {
            var left = Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArray1Ptr));
            var right = (UInt32*)(_dataTable.inArray2Ptr);
            var result = Avx.InsertVector128(left, right, 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(left, right, _dataTable.outArrayPtr);
        }

        public void RunClassLclFldScenario()
        {
            var test = new InsertLoadTest__InsertVector128UInt321();
            var result = Avx.InsertVector128(test._fld1, (UInt32*)(_dataTable.inArray2Ptr), 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(test._fld1, (UInt32*)(_dataTable.inArray2Ptr), _dataTable.outArrayPtr);
        }

        public void RunClassFldScenario()
        {
            var result = Avx.InsertVector128(_fld1, (UInt32*)(_dataTable.inArray2Ptr), 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_fld1, (UInt32*)(_dataTable.inArray2Ptr), _dataTable.outArrayPtr);
        }

        public void RunStructLclFldScenario()
        {
            var test = TestStruct.Create();
            var result = Avx.InsertVector128(test._fld1, (UInt32*)(_dataTable.inArray2Ptr), 1);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(test._fld1, (UInt32*)(_dataTable.inArray2Ptr), _dataTable.outArrayPtr);
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

        private void ValidateResult(Vector256<UInt32> left, void* right, void* result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray1 = new UInt32[Op1ElementCount];
            UInt32[] inArray2 = new UInt32[Op2ElementCount];
            UInt32[] outArray = new UInt32[RetElementCount];

            Unsafe.WriteUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray1[0]), left);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray2[0]), ref Unsafe.AsRef<byte>(right), (uint)Unsafe.SizeOf<Vector128<UInt32>>());
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref outArray[0]), ref Unsafe.AsRef<byte>(result), (uint)Unsafe.SizeOf<Vector256<UInt32>>());

            ValidateResult(inArray1, inArray2, outArray, method);
        }

        private void ValidateResult(void* left, void* right, void* result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray1 = new UInt32[Op1ElementCount];
            UInt32[] inArray2 = new UInt32[Op2ElementCount];
            UInt32[] outArray = new UInt32[RetElementCount];

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray1[0]), ref Unsafe.AsRef<byte>(left), (uint)Unsafe.SizeOf<Vector256<UInt32>>());
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray2[0]), ref Unsafe.AsRef<byte>(right), (uint)Unsafe.SizeOf<Vector128<UInt32>>());
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref outArray[0]), ref Unsafe.AsRef<byte>(result), (uint)Unsafe.SizeOf<Vector256<UInt32>>());

            ValidateResult(inArray1, inArray2, outArray, method);
        }

        private void ValidateResult(UInt32[] left, UInt32[] right, UInt32[] result, [CallerMemberName] string method = "")
        {
            if (result[0] != left[0])
            {
                Succeeded = false;
            }
            else
            {
                for (var i = 1; i < RetElementCount; i++)
                {
                    if ((i > 3 ? result[i] != right[i - 4] : result[i] != left[i]))
                    {
                        Succeeded = false;
                        break;
                    }
                }
            }

            if (!Succeeded)
            {
                Console.WriteLine($"{nameof(Avx)}.{nameof(Avx.InsertVector128)}<UInt32>(Vector256<UInt32>, Vector128<UInt32>.1): {method} failed:");
                Console.WriteLine($"    left: ({string.Join(", ", left)})");
                Console.WriteLine($"   right: ({string.Join(", ", right)})");
                Console.WriteLine($"  result: ({string.Join(", ", result)})");
                Console.WriteLine();
            }
        }
    }
}
