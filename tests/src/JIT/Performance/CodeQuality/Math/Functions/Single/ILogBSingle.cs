// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.ILogB(float) over 5000 iterations for the domain +1, +3

        private const float iLogBSingleDelta = 0.0004f;
        private const int iLogBSingleExpectedResult = 2499;

        [Benchmark(InnerIterationCount = ILogBSingleIterations)]
        public static void ILogBSingleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        ILogBSingleTest();
                    }
                }
            }
        }

        public static void ILogBSingleTest()
        {
            var result = 0; var value = 1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                result += MathF.ILogB(value);
                value += iLogBSingleDelta;
            }

            if (result != iLogBSingleExpectedResult)
            {
                throw new Exception($"Expected Result {iLogBSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
