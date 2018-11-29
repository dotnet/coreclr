// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Asinh(float) over 5000 iterations for the domain -1, +1

        private const float asinhSingleDelta = 0.0004f;
        private const float asinhSingleExpectedResult = -0.814757347f;

        [Benchmark(InnerIterationCount = AsinhSingleIterations)]
        public static void AsinhSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        AsinhSingleTest();
                    }
                }
            }
        }

        public static void AsinhSingleTest()
        {
            var result = 0.0f; var value = -1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.Asinh(value);
                value += asinhSingleDelta;
            }

            var diff = MathF.Abs(asinhSingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {asinhSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
