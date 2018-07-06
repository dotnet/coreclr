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
        private static void ParallelBitExtractUInt64()
        {
            var test = new ScalarBinaryOpTest__ParallelBitExtractUInt64();

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

            if (!test.Succeeded)
            {
                throw new Exception("One or more scenarios did not complete as expected.");
            }
        }
    }

    public sealed unsafe class ScalarBinaryOpTest__ParallelBitExtractUInt64
    {
        private static UInt64 _data1;
        private static UInt64 _data2;

        private static UInt64 _clsVar1;
        private static UInt64 _clsVar2;

        private UInt64 _fld1;
        private UInt64 _fld2;

        static ScalarBinaryOpTest__ParallelBitExtractUInt64()
        {
            var random = new Random();
            _clsVar1 = (ulong)(random.Next(0, int.MaxValue));
            _clsVar2 = (ulong)(random.Next(0, int.MaxValue));
        }

        public ScalarBinaryOpTest__ParallelBitExtractUInt64()
        {
            Succeeded = true;

            var random = new Random();
            
            _fld1 = (ulong)(random.Next(0, int.MaxValue));
            _fld2 = (ulong)(random.Next(0, int.MaxValue));

            _data1 = (ulong)(random.Next(0, int.MaxValue));
            _data2 = (ulong)(random.Next(0, int.MaxValue));
        }

        public bool IsSupported => Bmi2.IsSupported && (Environment.Is64BitProcess || ((typeof(UInt64) != typeof(long)) && (typeof(UInt64) != typeof(ulong))));

        public bool Succeeded { get; set; }

        public void RunBasicScenario_UnsafeRead()
        {
            var result = Bmi2.ParallelBitExtract(
                Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data1)),
                Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data2))
            );

            ValidateResult(_data1, _data2, result);
        }

        public void RunReflectionScenario_UnsafeRead()
        {
            var result = typeof(Bmi2).GetMethod(nameof(Bmi2.ParallelBitExtract), new Type[] { typeof(UInt64), typeof(UInt64) })
                                     .Invoke(null, new object[] {
                                        Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data1)),
                                        Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data2))
                                     });

            ValidateResult(_data1, _data2, (UInt64)result);
        }

        public void RunClsVarScenario()
        {
            var result = Bmi2.ParallelBitExtract(
                _clsVar1,
                _clsVar2
            );

            ValidateResult(_clsVar1, _clsVar2, result);
        }

        public void RunLclVarScenario_UnsafeRead()
        {
            var data1 = Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data1));
            var data2 = Unsafe.ReadUnaligned<UInt64>(ref Unsafe.As<UInt64, byte>(ref _data2));
            var result = Bmi2.ParallelBitExtract(data1, data2);

            ValidateResult(data1, data2, result);
        }

        public void RunLclFldScenario()
        {
            var test = new ScalarBinaryOpTest__ParallelBitExtractUInt64();
            var result = Bmi2.ParallelBitExtract(test._fld1, test._fld2);

            ValidateResult(test._fld1, test._fld2, result);
        }

        public void RunFldScenario()
        {
            var result = Bmi2.ParallelBitExtract(_fld1, _fld2);
            ValidateResult(_fld1, _fld2, result);
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

        private void ValidateResult(UInt64 left, UInt64 right, UInt64 result, [CallerMemberName] string method = "")
        {
            var isUnexpectedResult = false;

            
// The validation logic defined here for Bmi2.ParallelBitDeposit and Bmi2.ParallelBitExtract is
// based on the 'Operation' pseudo-code defined for the pdep and pext instruction in the 'Intel®
// 64 and IA-32 Architectures Software Developer’s Manual; Volume 2 (2A, 2B, 2C & 2D): Instruction
// Set Reference, A-Z'

ulong temp = left;
ulong mask = right;
ulong dest = 0;
byte m = 0, k = 0;

while (m < 64)
{
    if (((mask >> m) & 1) == 1) // Extract bit at index m of mask
    {
        dest |= (((temp >> m) & 1) << k); // Extract bit at index m of temp and insert to index k of dest
        k++;
    }
    m++;
}

isUnexpectedResult = (dest != result);


            if (isUnexpectedResult)
            {
                Console.WriteLine($"{nameof(Bmi2)}.{nameof(Bmi2.ParallelBitExtract)}<UInt64>(UInt64, UInt64): ParallelBitExtract failed:");
                Console.WriteLine($"    left: {left}");
                Console.WriteLine($"   right: {right}");
                Console.WriteLine($"  result: {result}");
                Console.WriteLine();
            }
        }
    }
}
