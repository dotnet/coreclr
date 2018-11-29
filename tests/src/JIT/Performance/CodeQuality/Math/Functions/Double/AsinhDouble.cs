// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.Asinh(double) over 5000 iterations for the domain -1, +1

        private const double asinhDoubleDelta = 0.0004;
        private const double asinhDoubleExpectedResult = -0.88137358721605752;

        [Benchmark(InnerIterationCount = AsinhDoubleIterations)]
        public static void AsinhDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        AsinhDoubleTest();
                    }
                }
            }
        }

        public static void AsinhDoubleTest()
        {
            var result = 0.0; var value = -1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.Asinh(value);
                value += asinhDoubleDelta;
            }

            var diff = Math.Abs(asinhDoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {asinhDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
