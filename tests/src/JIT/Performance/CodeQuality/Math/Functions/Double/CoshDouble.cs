﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Xunit.Performance;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests Math.Cosh(double) over 5000 iterations for the domain -1, +1

        private const double coshDoubleDelta = 0.0004;
        private const double coshDoubleExpectedResult = 5876.0060465657216;

        [Benchmark]
        public static void CoshDoubleBenchmark()
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    CoshDoubleTest();
                }
            }
        }

        public static void CoshDoubleTest()
        {
            var result = 0.0; var value = -1.0;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                value += coshDoubleDelta;
                result += Math.Cosh(value);
            }

            var diff = Math.Abs(coshDoubleExpectedResult - result);

            if (diff > doubleEpsilon)
            {
                throw new Exception($"Expected Result {coshDoubleExpectedResult}; Actual Result {result}");
            }
        }
    }
}
