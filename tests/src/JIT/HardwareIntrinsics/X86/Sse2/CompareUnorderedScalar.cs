// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IntelHardwareIntrinsicTest
{
    internal static partial class Program
    {
        private const int Pass = 100;
        private const int Fail = 0;

        static unsafe int Main(string[] args)
        {
            int testResult = Pass;
            int testsCount = 21;
            string methodUnderTestName = nameof(Sse2.CompareUnorderedScalar);

            if (Sse2.IsSupported)
            {
                using (var doubleTable = TestTableScalarSse2<double, double>.Create(testsCount))
                {
                    for (int i = 0; i < testsCount; i++)
                    {
                        (Vector128<double>, Vector128<double>) value = doubleTable[i];
                        Vector128<double> result = Sse2.CompareUnorderedScalar(value.Item1, value.Item2);
                        doubleTable.SetOutArray(result);
                    }

                    CheckMethodEight<double, double> checkDouble = (Span<double> x, Span<double> y, Span<double> z, Span<double> a) =>
                    {
                        if (double.IsNaN(x[0]) || double.IsNaN(y[0]))
                        {
                            a[0] = BitConverter.Int64BitsToDouble(-1);
                            a[1] = x[1];
                        }
                        else
                        {
                            a[0] = 0;
                            a[1] = x[1];
                        }
                        return BitConverter.DoubleToInt64Bits(a[0]) == BitConverter.DoubleToInt64Bits(z[0]) &&
                               BitConverter.DoubleToInt64Bits(a[1]) == BitConverter.DoubleToInt64Bits(z[1]);
                    };

                    if (!doubleTable.CheckResult(checkDouble))
                    {
                        PrintError(doubleTable, methodUnderTestName, "(Span<double> x, Span<double> y, Span<double> z, Span<double> a) => CompareUnorderedScalar", checkDouble);
                        testResult = Fail;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Sse2.IsSupported: {Sse2.IsSupported}, skipped tests of {typeof(Sse2)}.{methodUnderTestName}");
            }

            return testResult;
        }
    }
}
