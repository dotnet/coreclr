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
        private static void InsertUInt326()
        {
            var test = new SimpleUnaryOpTest__InsertUInt326();
            
            try
            {
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

                // Validates passing the field of a local works
                test.RunLclFldScenario();

                // Validates passing an instance member works
                test.RunFldScenario();
            }
            else
            {
                // Validates we throw on unsupported hardware
                test.RunUnsupportedScenario();
            }
            }
            catch (PlatformNotSupportedException)
            {
                test.Succeeded = true;
            }

            if (!test.Succeeded)
            {
                throw new Exception("One or more scenarios did not complete as expected.");
            }
        }
    }

    public sealed unsafe class SimpleUnaryOpTest__InsertUInt326
    {
        private const int VectorSize = 32;

        private const int Op1ElementCount = VectorSize / sizeof(UInt32);
        private const int RetElementCount = VectorSize / sizeof(UInt32);

        private static UInt32[] _data = new UInt32[Op1ElementCount];

        private static Vector256<UInt32> _clsVar;

        private Vector256<UInt32> _fld;

        private SimpleUnaryOpTest__DataTable<UInt32, UInt32> _dataTable;

        static SimpleUnaryOpTest__InsertUInt326()
        {
            var random = new Random();

            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = (uint)0; }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector256<UInt32>, byte>(ref _clsVar), ref Unsafe.As<UInt32, byte>(ref _data[0]), VectorSize);
        }

        public SimpleUnaryOpTest__InsertUInt326()
        {
            Succeeded = true;

            var random = new Random();

            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = (uint)0; }
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector256<UInt32>, byte>(ref _fld), ref Unsafe.As<UInt32, byte>(ref _data[0]), VectorSize);

            for (var i = 0; i < Op1ElementCount; i++) { _data[i] = (uint)0; }
            _dataTable = new SimpleUnaryOpTest__DataTable<UInt32, UInt32>(_data, new UInt32[RetElementCount], VectorSize);
        }

        public bool IsSupported => Avx.IsSupported;

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            var result = Avx.Insert(
                Unsafe.Read<Vector256<UInt32>>(_dataTable.inArrayPtr),
                (uint)2,
                6
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunBasicScenario_Load()
        {
            var result = Avx.Insert(
                Avx.LoadVector256((UInt32*)(_dataTable.inArrayPtr)),
                (uint)2,
                6
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunBasicScenario_LoadAligned()
        {
            var result = Avx.Insert(
                Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArrayPtr)),
                (uint)2,
                6
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.Insert), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Unsafe.Read<Vector256<UInt32>>(_dataTable.inArrayPtr),
                                        (uint)2,
                                        (byte)6
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_Load()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.Insert), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Avx.LoadVector256((UInt32*)(_dataTable.inArrayPtr)),
                                        (uint)2,
                                        (byte)6
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunReflectionScenario_LoadAligned()
        {
            var result = typeof(Avx).GetMethod(nameof(Avx.Insert), new Type[] { typeof(Vector256<UInt32>), typeof(UInt32), typeof(byte) })
                                     .Invoke(null, new object[] {
                                        Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArrayPtr)),
                                        (uint)2,
                                        (byte)6
                                     });

            Unsafe.Write(_dataTable.outArrayPtr, (Vector256<UInt32>)(result));
            ValidateResult(_dataTable.inArrayPtr, _dataTable.outArrayPtr);
        }

        public void RunClsVarScenario()
        {
            var result = Avx.Insert(
                _clsVar,
                (uint)2,
                6
            );

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_clsVar, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            var firstOp = Unsafe.Read<Vector256<UInt32>>(_dataTable.inArrayPtr);
            var result = Avx.Insert(firstOp, (uint)2, 6);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(firstOp, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_Load()
        {
            var firstOp = Avx.LoadVector256((UInt32*)(_dataTable.inArrayPtr));
            var result = Avx.Insert(firstOp, (uint)2, 6);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(firstOp, _dataTable.outArrayPtr);
        }

        public void RunLclVarScenario_LoadAligned()
        {
            var firstOp = Avx.LoadAlignedVector256((UInt32*)(_dataTable.inArrayPtr));
            var result = Avx.Insert(firstOp, (uint)2, 6);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(firstOp, _dataTable.outArrayPtr);
        }

        public void RunLclFldScenario()
        {
            var test = new SimpleUnaryOpTest__InsertUInt326();
            var result = Avx.Insert(test._fld, (uint)2, 6);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(test._fld, _dataTable.outArrayPtr);
        }

        public void RunFldScenario()
        {
            var result = Avx.Insert(_fld, (uint)2, 6);

            Unsafe.Write(_dataTable.outArrayPtr, result);
            ValidateResult(_fld, _dataTable.outArrayPtr);
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

        private void ValidateResult(Vector256<UInt32> firstOp, void* result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray = new UInt32[Op1ElementCount];
            UInt32[] outArray = new UInt32[RetElementCount];

            Unsafe.Write(Unsafe.AsPointer(ref inArray[0]), firstOp);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref outArray[0]), ref Unsafe.AsRef<byte>(result), VectorSize);

            ValidateResult(inArray, outArray, method);
        }

        private void ValidateResult(void* firstOp, void* result, [CallerMemberName] string method = "")
        {
            UInt32[] inArray = new UInt32[Op1ElementCount];
            UInt32[] outArray = new UInt32[RetElementCount];

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref inArray[0]), ref Unsafe.AsRef<byte>(firstOp), VectorSize);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<UInt32, byte>(ref outArray[0]), ref Unsafe.AsRef<byte>(result), VectorSize);

            ValidateResult(inArray, outArray, method);
        }

        private void ValidateResult(UInt32[] firstOp, UInt32[] result, [CallerMemberName] string method = "")
        {

            for (var i = 0; i < RetElementCount; i++)
            {
                if ((i == 6 ? result[i] != 2 : result[i] != 0))
                {
                    Succeeded = false;
                    break;
                }
            }

            if (!Succeeded)
            {
                Console.WriteLine($"{nameof(Avx)}.{nameof(Avx.Insert)}<UInt32>(Vector256<UInt32><9>): {method} failed:");
                Console.WriteLine($"  firstOp: ({string.Join(", ", firstOp)})");
                Console.WriteLine($"   result: ({string.Join(", ", result)})");
                Console.WriteLine();
            }
        }
    }
}
