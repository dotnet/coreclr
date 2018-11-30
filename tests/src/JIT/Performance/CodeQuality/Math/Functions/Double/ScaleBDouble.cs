// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.ScaleB(double, int) over 5000 iterations for the domain x: -1, +1; y: +0, +5000

        private const double scaleBDoubleDeltaX = -0.0004;
        private const int scaleBDoubleDeltaY = 1;
        private const double scaleBDoubleExpectedResult = double.NegativeInfinity;

        [Benchmark(InnerIterationCount = ScaleBDoubleIterations)]
        public static void ScaleBDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        ScaleBDoubleTest();
                    }
                }
            }
        }

        public static void ScaleBDoubleTest()
        {
            var result = 0.0; var valueX = -1.0; var valueY = 1;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.ScaleB(valueX, valueY);
                valueX += scaleBDoubleDeltaX; valueY += scaleBDoubleDeltaY;
            }

            var diff = Math.Abs(scaleBDoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {scaleBDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
