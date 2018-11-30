// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.FusedMultiplyAdd(double, double, double) over 5000 iterations for the domain x: +2, +1; y: -2, -1, z: +1, -1

        private const double fusedMultiplyAddDoubleDeltaX = -0.0004;
        private const double fusedMultiplyAddDoubleDeltaY = 0.0004;
        private const double fusedMultiplyAddDoubleDeltaZ = -0.0004;
        private const double fusedMultiplyAddDoubleExpectedResult = -6667.6668000005066;

        [Benchmark(InnerIterationCount = FusedMultiplyAddDoubleIterations)]
        public static void FusedMultiplyAddDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        FusedMultiplyAddDoubleTest();
                    }
                }
            }
        }

        public static void FusedMultiplyAddDoubleTest()
        {
            var result = 0.0; var valueX = 2.0; var valueY = -2.0; var valueZ = 1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.FusedMultiplyAdd(valueX, valueY, valueZ);
                valueX += fusedMultiplyAddDoubleDeltaX; valueY += fusedMultiplyAddDoubleDeltaY; valueZ += fusedMultiplyAddDoubleDeltaZ;
            }

            var diff = Math.Abs(fusedMultiplyAddDoubleExpectedResult - result);

            if (double.IsNaN(result) || (diff > doubleEpsilon))
            {
                throw new Exception($"Expected Result {fusedMultiplyAddDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
