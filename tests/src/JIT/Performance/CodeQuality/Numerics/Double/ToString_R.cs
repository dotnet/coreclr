// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Numerics
{
    public static partial class DoubleTests
    {
        // Tests Double.ToString("R") over 5000 iterations for -1.0 to +1.0

        private const double ToString_RDelta = 0.0004;

        [Benchmark(InnerIterationCount = ToString_RIterations)]
        public static void ToString_RBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        ToString_RTest();
                    }
                }
            }
        }

        public static void ToString_RTest()
        {
            double value = -1.0;

            for (int i = 0; i < ToString_RIterations; i++)
            {
                value += ToString_RDelta;
                value.ToString("R");
            }
        }
    }
}
