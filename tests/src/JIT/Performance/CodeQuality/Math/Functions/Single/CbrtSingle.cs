// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Cbrt(float) over 5000 iterations for the domain +0, +PI

        private const float cbrtSingleDelta = 0.000628318531f;
        private const float cbrtSingleExpectedResult = 5491.4541f;

        [Benchmark(InnerIterationCount = CbrtSingleIterations)]
        public static void CbrtSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        CbrtSingleTest();
                    }
                }
            }
        }

        public static void CbrtSingleTest()
        {
            var result = 0.0f; var value = 0.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.Cbrt(value);
                value += cbrtSingleDelta;
            }

            var diff = MathF.Abs(cbrtSingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {cbrtSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
