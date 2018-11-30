// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.ILogB(double) over 5000 iterations for the domain +1, +3

        private const double iLogBDoubleDelta = 0.0004;
        private const int iLogBDoubleExpectedResult = 2499;

        [Benchmark(InnerIterationCount = ILogBDoubleIterations)]
        public static void ILogBDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        ILogBDoubleTest();
                    }
                }
            }
        }

        public static void ILogBDoubleTest()
        {
            var result = 0; var value = 1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += Math.ILogB(value);
                value += iLogBDoubleDelta;
            }

            if (result != iLogBDoubleExpectedResult)
            {
                throw new Exception($"Expected Result {iLogBDoubleExpectedResult,20:g17}; Actual Result {result,20:g17}");
            }
        }
    }
}
