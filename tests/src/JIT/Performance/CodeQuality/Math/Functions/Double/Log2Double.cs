// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.Log2(double) over 5000 iterations for the domain +1, +3

        private const double log2DoubleDelta = 0.0004;
        private const double log2DoubleExpectedResult = 4672.9510376532398;

        [Benchmark(InnerIterationCount = Log2DoubleIterations)]
        public static void Log2DoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        Log2DoubleTest();
                    }
                }
            }
        }

        public static void Log2DoubleTest()
        {
            var result = 0.0; var value = 1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.Log2(value);
                value += log2DoubleDelta;
            }

            var diff = Math.Abs(log2DoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {log2DoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
