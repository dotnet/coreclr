// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.FusedMultiplyAdd(float, float, float) over 5000 iterations for the domain x: +2, +1; y: -2, -1, z: +1, -1

        private const float fusedMultiplyAddSingleDeltaX = -0.0004f;
        private const float fusedMultiplyAddSingleDeltaY = 0.0004f;
        private const float fusedMultiplyAddSingleDeltaZ = -0.0004f;
        private const float fusedMultiplyAddSingleExpectedResult = -6668.49072f;

        [Benchmark(InnerIterationCount = FusedMultiplyAddSingleIterations)]
        public static void FusedMultiplyAddSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        FusedMultiplyAddSingleTest();
                    }
                }
            }
        }

        public static void FusedMultiplyAddSingleTest()
        {
            var result = 0.0f; var valueX = 2.0f; var valueY = -2.0f; var valueZ = 1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.FusedMultiplyAdd(valueX, valueY, valueZ);
                valueX += fusedMultiplyAddSingleDeltaX; valueY += fusedMultiplyAddSingleDeltaY; valueZ += fusedMultiplyAddSingleDeltaZ;
            }

            var diff = MathF.Abs(fusedMultiplyAddSingleExpectedResult - result);

            if (float.IsNaN(result) || (diff > singleEpsilon))
            {
                throw new Exception($"Expected Result {fusedMultiplyAddSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
