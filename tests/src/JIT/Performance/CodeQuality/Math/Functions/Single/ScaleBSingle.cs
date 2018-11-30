// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.ScaleB(float, int) over 5000 iterations for the domain x: -1, +1; y: +0, +5000

        private const float scaleBSingleDeltaX = -0.0004f;
        private const int scaleBSingleDeltaY = 1;
        private const float scaleBSingleExpectedResult = float.NegativeInfinity;

        [Benchmark(InnerIterationCount = ScaleBSingleIterations)]
        public static void ScaleBSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        ScaleBSingleTest();
                    }
                }
            }
        }

        public static void ScaleBSingleTest()
        {
            var result = 0.0f; var valueX = -1.0f; var valueY = 0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.ScaleB(valueX, valueY);
                valueX += scaleBSingleDeltaX; valueY += scaleBSingleDeltaY;
            }

            var diff = MathF.Abs(scaleBSingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {scaleBSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
