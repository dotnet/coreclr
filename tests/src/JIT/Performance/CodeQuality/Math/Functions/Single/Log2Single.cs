// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Log2(float) over 5000 iterations for the domain +1, +3

        private const float log2SingleDelta = 0.0004f;
        private const float log2SingleExpectedResult = 4672.73193f;

        [Benchmark(InnerIterationCount = Log2SingleIterations)]
        public static void Log2SingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        Log2SingleTest();
                    }
                }
            }
        }

        public static void Log2SingleTest()
        {
            var result = 0.0f; var value = 1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.Log2(value);
                value += log2SingleDelta;
            }

            var diff = MathF.Abs(log2SingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {log2SingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
