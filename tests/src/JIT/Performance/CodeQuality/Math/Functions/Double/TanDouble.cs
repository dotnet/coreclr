// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.Tan(double) over 5000 iterations for the domain -1, +1

        private const double tanDoubleDelta = 0.0004;
        private const double tanDoubleExpectedResult = -1.5574077250039999;

        [Benchmark(InnerIterationCount = TanDoubleIterations)]
        public static void TanDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        TanDoubleTest();
                    }
                }
            }
        }

        public static void TanDoubleTest()
        {
            var result = 0.0; var value = -1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.Tan(value);
                value += tanDoubleDelta;
            }

            var diff = Math.Abs(tanDoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {tanDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
