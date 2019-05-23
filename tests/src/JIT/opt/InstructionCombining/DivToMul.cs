// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// JIT is able to replace "x / 2" with "x * 0.5" where 2 is a power of two float
// Make sure this optimization doesn't change the results

public class Program
{
    private static int resultCode = 100;

    public static int Main(string[] args)
    {
        // Some corner cases
        var floatValues = new List<float>(
            new [] { 0, 1, MathF.PI, MathF.E, float.MinValue, float.MinValue, int.MaxValue, int.MinValue, float.NegativeInfinity, float.PositiveInfinity, float.NaN, (float)double.MaxValue, (float)double.MinValue });

        var doubleValues = new List<double>(
            new [] { 0, 1, Math.PI, Math.E, double.MinValue, double.MinValue, long.MaxValue, long.MinValue, double.NegativeInfinity, double.PositiveInfinity, double.NaN, 0.0, 1.0 });

        var random = new Random();
        // Also, add some random values
        for (int i = 0; i < 100000; i++)
        {
            floatValues.Add((float)random.NextDouble());
            doubleValues.Add(random.NextDouble());
        }

        for (int i = 0; i < floatValues.Count; i++)
        {
            TestPowOfTwo_Single(floatValues[i]);
            TestPowOfTwo_Single(-floatValues[i]);

            TestPowOfTwo_Double(doubleValues[i]);
            TestPowOfTwo_Double(-doubleValues[i]);

            TestNotPowOfTwo_Single(floatValues[i]);
            TestNotPowOfTwo_Double(doubleValues[i]);

            TestNotPowOfTwo_Single(-floatValues[i]);
            TestNotPowOfTwo_Double(-doubleValues[i]);
        }

        return resultCode;
    }

    private static void TestPowOfTwo_Single(float x)
    {
        // TestPowOfTwo_Single should contain 19 'mul' and 19 'div' instructions
        AssertEquals(expected: x / ConstToVar(2),               actual: x / 2);
        AssertEquals(expected: x / ConstToVar(4),               actual: x / 4);
        AssertEquals(expected: x / ConstToVar(8),               actual: x / 8);
        AssertEquals(expected: x / ConstToVar(16),              actual: x / 16);
        AssertEquals(expected: x / ConstToVar(134217728),       actual: x / 134217728);
        AssertEquals(expected: x / ConstToVar(268435456),       actual: x / 268435456);
        AssertEquals(expected: x / ConstToVar(536870912),       actual: x / 536870912);
        AssertEquals(expected: x / ConstToVar(1073741824),      actual: x / 1073741824);

        // < 1
        AssertEquals(expected: x / ConstToVar(0.5f),            actual: x / 0.5f);
        AssertEquals(expected: x / ConstToVar(0.25f),           actual: x / 0.25f);
        AssertEquals(expected: x / ConstToVar(0.125f),          actual: x / 0.125f);
        AssertEquals(expected: x / ConstToVar(0.0625f),         actual: x / 0.0625f);
        AssertEquals(expected: x / ConstToVar(0.0009765625f),   actual: x / 0.0009765625f);
        AssertEquals(expected: x / ConstToVar(0.00048828125f),  actual: x / 0.00048828125f);
        AssertEquals(expected: x / ConstToVar(0.00024414062f),  actual: x / 0.00024414062f);
        AssertEquals(expected: x / ConstToVar(0.00012207031f),  actual: x / 0.00012207031f);


        // < 0
        AssertEquals(expected: x / ConstToVar(-1073741824),     actual: x / -1073741824);
        AssertEquals(expected: x / ConstToVar(-0.00012207031f), actual: x / -0.00012207031f);
        AssertEquals(expected: x / ConstToVar(-2147483648),     actual: x / -2147483648);
    }


    private static void TestPowOfTwo_Double(double x)
    {
        // TestPowOfTwo_Double should contain 19 'mul' and 19 'div' instructions
        AssertEquals(expected: x / ConstToVar(2),               actual: x / 2);
        AssertEquals(expected: x / ConstToVar(4),               actual: x / 4);
        AssertEquals(expected: x / ConstToVar(8),               actual: x / 8);
        AssertEquals(expected: x / ConstToVar(16),              actual: x / 16);
        AssertEquals(expected: x / ConstToVar(134217728),       actual: x / 134217728);
        AssertEquals(expected: x / ConstToVar(268435456),       actual: x / 268435456);
        AssertEquals(expected: x / ConstToVar(536870912),       actual: x / 536870912);
        AssertEquals(expected: x / ConstToVar(1073741824),      actual: x / 1073741824);

        // < 1
        AssertEquals(expected: x / ConstToVar(0.5),             actual: x / 0.5);
        AssertEquals(expected: x / ConstToVar(0.25),            actual: x / 0.25);
        AssertEquals(expected: x / ConstToVar(0.125),           actual: x / 0.125);
        AssertEquals(expected: x / ConstToVar(0.0625),          actual: x / 0.0625);
        AssertEquals(expected: x / ConstToVar(0.00390625),      actual: x / 0.00390625);
        AssertEquals(expected: x / ConstToVar(0.001953125),     actual: x / 0.001953125);
        AssertEquals(expected: x / ConstToVar(0.0009765625),    actual: x / 0.0009765625);
        AssertEquals(expected: x / ConstToVar(0.00048828125),   actual: x / 0.00048828125);

        // < 0
        AssertEquals(expected: x / ConstToVar(-1073741824),     actual: x / -1073741824);
        AssertEquals(expected: x / ConstToVar(-0.00012207031f), actual: x / -0.00012207031f);
        AssertEquals(expected: x / ConstToVar(-2147483648),     actual: x / -2147483648);
    }

    private static void TestNotPowOfTwo_Single(float x)
    {
        // TestNotPowOfTwo_Single should not contain 'mul' instructions
        AssertEquals(expected: x / ConstToVar(3),       actual: x / 3);
        AssertEquals(expected: x / ConstToVar(9),      actual: x / 9);
        AssertEquals(expected: x / ConstToVar(2.5f),   actual: x / 2.5f);
        AssertEquals(expected: x / ConstToVar(0.51f),  actual: x / 0.51f);

        AssertEquals(expected: x / ConstToVar(-3),     actual: x / -3);
        AssertEquals(expected: x / ConstToVar(-9),     actual: x / -9);
        AssertEquals(expected: x / ConstToVar(-2.5f),  actual: x / -2.5f);
        AssertEquals(expected: x / ConstToVar(-0.51f), actual: x / -0.51f);
    }

    private static void TestNotPowOfTwo_Double(double x)
    {
        // TestNotPowOfTwo_Double should not contain 'mul' instructions
        AssertEquals(expected: x / ConstToVar(3),             actual: x / 3);
        AssertEquals(expected: x / ConstToVar(9),             actual: x / 9);
        AssertEquals(expected: x / ConstToVar(2.5),           actual: x / 2.5);
        AssertEquals(expected: x / ConstToVar(0.51),          actual: x / 0.51);

        AssertEquals(expected: x / ConstToVar(-3),            actual: x / -3);
        AssertEquals(expected: x / ConstToVar(-9),            actual: x / -9);
        AssertEquals(expected: x / ConstToVar(-2.5),          actual: x / -2.5);
        AssertEquals(expected: x / ConstToVar(-0.51),         actual: x / -0.51);

        AssertEquals(expected: x / ConstToVar(0.00024414062), actual: x / 0.00024414062);
        AssertEquals(expected: x / ConstToVar(0.00012207031), actual: x / 0.00012207031);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AssertEquals(float expected, float actual)
    {
        int expectedi = BitConverter.SingleToInt32Bits(expected);
        int actuali = BitConverter.SingleToInt32Bits(actual);
        if (expectedi != actuali)
        {
            resultCode--;
            Console.WriteLine($"AssertEquals: {expected} != {actual}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AssertEquals(double expected, double actual)
    {
        long expectedi = BitConverter.DoubleToInt64Bits(expected);
        long actuali = BitConverter.DoubleToInt64Bits(actual);
        if (expectedi != actuali)
        {
            resultCode--;
            Console.WriteLine($"AssertEquals: {expected} != {actual}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ConstToVar<T>(T v) => v;
}
