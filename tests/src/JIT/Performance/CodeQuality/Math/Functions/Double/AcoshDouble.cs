// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.Acosh(double) over 5000 iterations for the domain +1, +3

        private const double acoshDoubleDelta = 0.0004;
        private const double acoshDoubleExpectedResult = 6148.648751739127;

        [Benchmark(InnerIterationCount=AcoshDoubleIterations)]
        public static void AcoshDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        AcoshDoubleTest();
                    }
                }
            }
        }

        public static void AcoshDoubleTest()
        {
            var result = 0.0; var value = 1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.Acosh(value);
                value += acoshDoubleDelta;
            }

            var diff = Math.Abs(acoshDoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {acoshDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
