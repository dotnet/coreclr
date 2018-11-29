// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Acosh(float) over 5000 iterations for the domain +1, +3

        private const float acoshSingleDelta = 0.0004f;
        private const float acoshSingleExpectedResult = 6148.45459f;

        [Benchmark(InnerIterationCount = AcoshSingleIterations)]
        public static void AcoshSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        AcoshSingleTest();
                    }
                }
            }
        }

        public static void AcoshSingleTest()
        {
            var result = 0.0f; var value = 1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.Acosh(value);
                value += acoshSingleDelta;
            }

            var diff = MathF.Abs(acoshSingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {acoshSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
