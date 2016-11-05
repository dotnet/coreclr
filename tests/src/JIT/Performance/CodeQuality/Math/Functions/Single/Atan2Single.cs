﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Atan2(float, float) over 5000 iterations for the domain y: -1, +1; x: +1, -1

        private const float atan2SingleDeltaX = -0.0004f;
        private const float atan2SingleDeltaY = 0.0004f;
        private const float atan2SingleExpectedResult = 3926.99082f;

        [Benchmark]
        public static void Atan2SingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    Atan2SingleTest();
                }
            }
        }

        public static void Atan2SingleTest()
        {
            var result = 0.0f; var valueX = 1.0f; var valueY = -1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                valueX += atan2SingleDeltaX; valueY += atan2SingleDeltaY;
                result += MathF.Atan2(valueY, valueX);
            }

            var diff = MathF.Abs(atan2SingleExpectedResult - result);

            if (diff > singleEpsilon)
            {
                throw new Exception($"Expected Result {atan2SingleExpectedResult}; Actual Result {result}");
            }
        }
    }
}
