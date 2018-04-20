// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

////////////////////////////////////////////////////////////////////////////////
// Types
////////////////////////////////////////////////////////////////////////////////

public struct FourByteStruct 
{
    public int a;
}

public struct EightByteStruct
{
    public int a;
    public int b;
}

public struct EightByteStructB
{
    public long a;
}

public struct SixteenByteStruct
{
    public long a;
    public long b;
}

public struct SixteenByteStructB
{
    public int a;
    public int b;
    public int c;
    public int d;
}

public struct ThirtyTwoByteStruct
{
    public long a;
    public long b;
    public long c;
    public long d;
}

public struct FourByteStructFloat
{
    public float a;
}

public struct EightByteStructFloat
{
    public float a;
    public float b;
}

public struct EightByteStructFloatB
{
    public double a;
}

struct SixteenByteStructFloat
{
    public double a;
    public double b;
}

public struct SixteenByteStructFloatB
{
    public float a;
    public float b;
    public float c;
    public float d;
}

public struct ThirtyTwoByteStructFloat
{
    public double a;
    public double b;
    public double c;
    public double d;
}

////////////////////////////////////////////////////////////////////////////////
// Main
////////////////////////////////////////////////////////////////////////////////

class VarArg
{
    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static int test_passing_ints(int count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static long test_passing_longs(int count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static float test_passing_floats(int count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static double test_passing_doubles(int count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static long test_passing_int_and_longs(int int_count, int long_count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static double test_passing_floats_and_doubles(int float_count, int double_count, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static int check_string_from_format(string expected, string format, __arglist);

    [DllImport("varargnative", CallingConvention = CallingConvention.Cdecl)]
    extern static int check_passing_struct(int count, __arglist);

    [DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
    extern static void printf(string s, __arglist);

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingInts(int[] expectedValues)
    {
        int expectedSum = test_passing_ints(expectedValues.Length, __arglist(expectedValues[0], expectedValues[1], expectedValues[2], expectedValues[3]));

        int sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingLongs(long[] expectedValues)
    {
        long expectedSum = test_passing_longs(expectedValues.Length, __arglist(expectedValues[0], expectedValues[1], expectedValues[2], expectedValues[3]));

        long sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingFloats(float[] expectedValues)
    {
        float expectedSum = test_passing_floats(expectedValues.Length, __arglist(expectedValues[0], expectedValues[1], expectedValues[2], expectedValues[3]));

        float sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingDoubles(double[] expectedValues)
    {
        double expectedSum = test_passing_doubles(expectedValues.Length, __arglist(expectedValues[0], expectedValues[1], expectedValues[2], expectedValues[3]));

        double sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingEmptyInts(int[] expectedValues)
    {
        int expectedSum = test_passing_ints(expectedValues.Length, __arglist());

        int sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingEmptyLongs(long[] expectedValues)
    {
        long expectedSum = test_passing_longs(expectedValues.Length, __arglist());

        long sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingEmptyFloats(float[] expectedValues)
    {
        float expectedSum = test_passing_floats(expectedValues.Length, __arglist());

        float sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingEmptyDouble(double[] expectedValues)
    {
        double expectedSum = test_passing_doubles(expectedValues.Length, __arglist());

        double sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// Note:
    /// 
    /// This test case will fail. See https://github.com/dotnet/coreclr/issues/17609
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingIntsAndLongs(int[] expectedIntValues, long[] expectedLongValues)
    {
        long expectedSum = test_passing_int_and_longs(expectedIntValues.Length, expectedLongValues.Length, __arglist(expectedIntValues[0], expectedIntValues[1], expectedLongValues[0], expectedLongValues[1]));

        long sum = 0;
        for (int i = 0; i < expectedIntValues.Length; ++i)
        {
            sum += expectedIntValues[i];
        }

        for (int i = 0; i < expectedLongValues.Length; ++i)
        {
            sum += expectedLongValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingFloatsAndDoubles(float[] expectedFloatValues, double[] expectedDoubleValues)
    {
        double expectedSum = test_passing_floats_and_doubles(expectedFloatValues.Length, expectedDoubleValues.Length, __arglist(expectedFloatValues[0], expectedFloatValues[1], expectedDoubleValues[0], expectedDoubleValues[1]));

        double sum = 0;
        for (int i = 0; i < expectedFloatValues.Length; ++i)
        {
            sum += expectedFloatValues[i];
        }

        for (int i = 0; i < expectedDoubleValues.Length; ++i)
        {
            sum += expectedDoubleValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// Notes:
    /// 
    /// This is a particularily interesting test case because on every platform it
    /// will force spilling locals to the stack instead of just passing in registers.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingManyInts(int[] expectedValues)
    {
        int expectedSum = test_passing_ints(expectedValues.Length, __arglist(expectedValues[0], 
                                                                             expectedValues[1], 
                                                                             expectedValues[2], 
                                                                             expectedValues[3],
                                                                             expectedValues[4],
                                                                             expectedValues[5],
                                                                             expectedValues[6],
                                                                             expectedValues[7],
                                                                             expectedValues[8],
                                                                             expectedValues[9],
                                                                             expectedValues[10],
                                                                             expectedValues[11],
                                                                             expectedValues[12],
                                                                             expectedValues[13],
                                                                             expectedValues[14],
                                                                             expectedValues[15],
                                                                             expectedValues[16],
                                                                             expectedValues[17],
                                                                             expectedValues[18],
                                                                             expectedValues[19],
                                                                             expectedValues[20],
                                                                             expectedValues[21],
                                                                             expectedValues[22],
                                                                             expectedValues[23],
                                                                             expectedValues[24],
                                                                             expectedValues[25],
                                                                             expectedValues[26],
                                                                             expectedValues[27],
                                                                             expectedValues[28],
                                                                             expectedValues[29],
                                                                             expectedValues[30],
                                                                             expectedValues[31],
                                                                             expectedValues[32],
                                                                             expectedValues[33],
                                                                             expectedValues[34],
                                                                             expectedValues[35],
                                                                             expectedValues[36],
                                                                             expectedValues[37],
                                                                             expectedValues[38],
                                                                             expectedValues[39],
                                                                             expectedValues[40]));

        int sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// Notes:
    /// 
    /// This is a particularily interesting test case because on every platform it
    /// will force spilling locals to the stack instead of just passing in registers.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingManyLongs(long[] expectedValues)
    {
        long expectedSum = test_passing_longs(expectedValues.Length, __arglist(expectedValues[0],
                                                                               expectedValues[1],
                                                                               expectedValues[2],
                                                                               expectedValues[3],
                                                                               expectedValues[4],
                                                                               expectedValues[5],
                                                                               expectedValues[6],
                                                                               expectedValues[7],
                                                                               expectedValues[8],
                                                                               expectedValues[9],
                                                                               expectedValues[10],
                                                                               expectedValues[11],
                                                                               expectedValues[12],
                                                                               expectedValues[13],
                                                                               expectedValues[14],
                                                                               expectedValues[15],
                                                                               expectedValues[16],
                                                                               expectedValues[17],
                                                                               expectedValues[18],
                                                                               expectedValues[19],
                                                                               expectedValues[20],
                                                                               expectedValues[21],
                                                                               expectedValues[22],
                                                                               expectedValues[23],
                                                                               expectedValues[24],
                                                                               expectedValues[25],
                                                                               expectedValues[26],
                                                                               expectedValues[27],
                                                                               expectedValues[28],
                                                                               expectedValues[29],
                                                                               expectedValues[30],
                                                                               expectedValues[31],
                                                                               expectedValues[32],
                                                                               expectedValues[33],
                                                                               expectedValues[34],
                                                                               expectedValues[35],
                                                                               expectedValues[36],
                                                                               expectedValues[37],
                                                                               expectedValues[38],
                                                                               expectedValues[39],
                                                                               expectedValues[40]));

        long sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// Notes:
    /// 
    /// This is a particularily interesting test case because on every platform it
    /// will force spilling locals to the stack instead of just passing in registers.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingManyFloats(float[] expectedValues)
    {
        float expectedSum = test_passing_floats(expectedValues.Length, __arglist(expectedValues[0],
                                                                                 expectedValues[1],
                                                                                 expectedValues[2],
                                                                                 expectedValues[3],
                                                                                 expectedValues[4],
                                                                                 expectedValues[5],
                                                                                 expectedValues[6],
                                                                                 expectedValues[7],
                                                                                 expectedValues[8],
                                                                                 expectedValues[9],
                                                                                 expectedValues[10],
                                                                                 expectedValues[11],
                                                                                 expectedValues[12],
                                                                                 expectedValues[13],
                                                                                 expectedValues[14],
                                                                                 expectedValues[15],
                                                                                 expectedValues[16],
                                                                                 expectedValues[17],
                                                                                 expectedValues[18],
                                                                                 expectedValues[19],
                                                                                 expectedValues[20],
                                                                                 expectedValues[21],
                                                                                 expectedValues[22],
                                                                                 expectedValues[23],
                                                                                 expectedValues[24],
                                                                                 expectedValues[25],
                                                                                 expectedValues[26],
                                                                                 expectedValues[27],
                                                                                 expectedValues[28],
                                                                                 expectedValues[29],
                                                                                 expectedValues[30],
                                                                                 expectedValues[31],
                                                                                 expectedValues[32],
                                                                                 expectedValues[33],
                                                                                 expectedValues[34],
                                                                                 expectedValues[35],
                                                                                 expectedValues[36],
                                                                                 expectedValues[37],
                                                                                 expectedValues[38],
                                                                                 expectedValues[39],
                                                                                 expectedValues[40]));

        float sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// The C function will simply loop over the arguments, compute the sum
    /// then return the value.
    /// 
    /// Do a quick check on the value returned, and return whether they equal.
    /// 
    /// Notes:
    /// 
    /// This is a particularily interesting test case because on every platform it
    /// will force spilling locals to the stack instead of just passing in registers.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPassingManyDoubles(double[] expectedValues)
    {
        double expectedSum = test_passing_doubles(expectedValues.Length, __arglist(expectedValues[0],
                                                                                expectedValues[1],
                                                                                expectedValues[2],
                                                                                expectedValues[3],
                                                                                expectedValues[4],
                                                                                expectedValues[5],
                                                                                expectedValues[6],
                                                                                expectedValues[7],
                                                                                expectedValues[8],
                                                                                expectedValues[9],
                                                                                expectedValues[10],
                                                                                expectedValues[11],
                                                                                expectedValues[12],
                                                                                expectedValues[13],
                                                                                expectedValues[14],
                                                                                expectedValues[15],
                                                                                expectedValues[16],
                                                                                expectedValues[17],
                                                                                expectedValues[18],
                                                                                expectedValues[19],
                                                                                expectedValues[20],
                                                                                expectedValues[21],
                                                                                expectedValues[22],
                                                                                expectedValues[23],
                                                                                expectedValues[24],
                                                                                expectedValues[25],
                                                                                expectedValues[26],
                                                                                expectedValues[27],
                                                                                expectedValues[28],
                                                                                expectedValues[29],
                                                                                expectedValues[30],
                                                                                expectedValues[31],
                                                                                expectedValues[32],
                                                                                expectedValues[33],
                                                                                expectedValues[34],
                                                                                expectedValues[35],
                                                                                expectedValues[36],
                                                                                expectedValues[37],
                                                                                expectedValues[38],
                                                                                expectedValues[39],
                                                                                expectedValues[40]));

        double sum = 0;
        for (int i = 0; i < expectedValues.Length; ++i)
        {
            sum += expectedValues[i];
        }

        return sum == expectedSum;
    }

    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// This is mostly to simulate calling printf from Managed code.
    /// Based on the % values the c function will create and return a cstring
    /// 
    /// Compare the c string with the value expected, return true if they
    /// are equal.
    /// 
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <returns>bool</returns>
    static bool TestPrintF()
    {
        bool success = true;

        success = TestPrintFIntLongDouble("%d,%d,%f", 10, 11, 24.1) ? success : false;
        success = TestPrintFDoubleDoubleIntIntLongDouble("%f,%f,%d,%d,%d,%f", 10.1, 11.1, 12, 2, 4L, 0.21) ? success : false;
        success = TestPrintFManyLong() ? success : false;

        return success;
    }

    /// <summary>
    /// This is a helper for TestPrintF
    ///
    /// This will create a string with three arguments:
    ///
    /// int,
    /// long,
    /// double
    /// 
    /// </summary>
    static bool TestPrintFIntLongDouble(string format, int val1, long val2, double val3)
    {
        printf(format + "\n", __arglist(val1, val2, val3));

        int returnedVal = check_string_from_format(String.Format("{0},{1},{2:0.00}", val1, val2, val3), 
                                                      format, 
                                                      __arglist(val1, val2, val3));

        return returnedVal == 0;
    }

    /// <summary>
    /// This is a helper for TestPrintF
    ///
    /// This will create a string with three arguments:
    ///
    /// int,
    /// long,
    /// double
    /// 
    /// </summary>
    static bool TestPrintFDoubleDoubleIntIntLongDouble(string format, double val1, double val2, int val3, int val4, long val5, double val6)
    {
        printf(format + "\n", __arglist(val1, val2, val3, val4, val5, val6));
        int returnedVal = check_string_from_format(String.Format("{0:0.00},{1:0.00},{2},{3},{4},{5:0.00}", val1, val2, val3, val4, val5, val6), 
                                                      format, 
                                                      __arglist(val1, val2, val3, val4, val5, val6));

        return returnedVal == 0;
    }

    /// <summary>
    /// This is a helper for TestPrintF
    ///
    /// This will create a string with three arguments:
    ///
    /// int,
    /// long,
    /// double
    /// 
    /// </summary>
    static bool TestPrintFManyLong()
    {
        long[] values = {
            100,
            200,
            30,
            400,
            500,
            600,
            7,
            80,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            2200,
            234,
            24,
            25,
            26,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40
        };

        string format = "%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d";

        printf(format + "\n", __arglist(values[0],
                                 values[1],
                                 values[2],
                                 values[3],
                                 values[4],
                                 values[5],
                                 values[6],
                                 values[7],
                                 values[8],
                                 values[9],
                                 values[10],
                                 values[11],
                                 values[12],
                                 values[13],
                                 values[14],
                                 values[15],
                                 values[16],
                                 values[17],
                                 values[18],
                                 values[19],
                                 values[20],
                                 values[21],
                                 values[22],
                                 values[23],
                                 values[24],
                                 values[25],
                                 values[26],
                                 values[27],
                                 values[28],
                                 values[29],
                                 values[30],
                                 values[31],
                                 values[32],
                                 values[33],
                                 values[34],
                                 values[35],
                                 values[36],
                                 values[37],
                                 values[38],
                                 values[39]
                                 ));

        int returnedVal = check_string_from_format(String.Join(",", values), 
                                                   format, 
                                                   __arglist(
                                                       values[0],
                                                       values[1],
                                                       values[2],
                                                       values[3],
                                                       values[4],
                                                       values[5],
                                                       values[6],
                                                       values[7],
                                                       values[8],
                                                       values[9],
                                                       values[10],
                                                       values[11],
                                                       values[12],
                                                       values[13],
                                                       values[14],
                                                       values[15],
                                                       values[16],
                                                       values[17],
                                                       values[18],
                                                       values[19],
                                                       values[20],
                                                       values[21],
                                                       values[22],
                                                       values[23],
                                                       values[24],
                                                       values[25],
                                                       values[26],
                                                       values[27],
                                                       values[28],
                                                       values[29],
                                                       values[30],
                                                       values[31],
                                                       values[32],
                                                       values[33],
                                                       values[34],
                                                       values[35],
                                                       values[36],
                                                       values[37],
                                                       values[38],
                                                       values[39]
                                                   ));

        return returnedVal == 0;
    }
 
    /// <summary>
    /// Given an input set create an arglist to pass to a vararg C function.
    /// 
    /// This function will test passing struct through varargs.
    /// 
    /// </summary>
    /// <returns>bool</returns>
    static int TestPassingStructs()
    {
        int success = 100;

        success = TestPassingEightByteStructs();
        success = TestPassingSixteenByteStructs();
        success = TestPassingThirtyTwoByteStructs();

        return success;
    }

    /// <summary>
    /// This is a helper for TestPassingStructs
    /// 
    /// </summary>
    static int TestPassingEightByteStructs()
    {
        int success = 100;

        EightByteStruct first = new EightByteStruct();
        EightByteStructB second = new EightByteStructB();
        EightByteStructFloat third = new EightByteStructFloat();
        EightByteStructFloatB fourth = new EightByteStructFloatB();

        first.a = 20;
        first.b = -8;

        second.a = 4020120;

        third.a = 10.223f;
        third.b = 10331.1f;

        fourth.a = 120.1321321;

        int firstExpectedValue = first.a + first.b;
        long secondExpectedValue = second.a;
        float thirdExpectedValue = third.a + third.b;
        double fourthExpectedValue = fourth.a;

        success = check_passing_struct(6, __arglist(0, 0, 0, 8, 1, firstExpectedValue, first)) == 0 ? success : 16;
        success = check_passing_struct(6, __arglist(1, 0, 0, 8, 1, secondExpectedValue, second)) == 0 ? success : 17;
        success = check_passing_struct(6, __arglist(0, 1, 0, 8, 1, thirdExpectedValue, third)) == 0 ? success : 18;
        success = check_passing_struct(6, __arglist(1, 1, 0, 8, 1, fourthExpectedValue, fourth)) == 0 ? success : 19;

        return success;
    }

    /// <summary>
    /// This is a helper for TestPassingStructs
    /// 
    /// </summary>
    static int TestPassingSixteenByteStructs()
    {
        int success = 100;

        SixteenByteStruct first = new SixteenByteStruct();
        SixteenByteStructB second = new SixteenByteStructB();
        SixteenByteStructFloat third = new SixteenByteStructFloat();
        SixteenByteStructFloatB fourth = new SixteenByteStructFloatB();

        first.a = 30;
        first.b = -20;

        second.a = 10;
        second.b = 50;
        second.c = 80;
        second.b = 12000;

        third.a = 10.223;
        third.b = 10331.1;

        fourth.a = 120.1321321f;
        fourth.b = 10.2f;
        fourth.c = 11.02f;
        fourth.d = 8910.22f;

        long firstExpectedValue = first.a + first.b;
        long secondExpectedValue = second.a + second.b + second.c + second.d;
        double thirdExpectedValue = third.a + third.b;
        float fourthExpectedValue = fourth.a + fourth.b + fourth.c + fourth.d;

        success = check_passing_struct(6, __arglist(0, 0, 0, 16, 1, firstExpectedValue, first)) == 0 ? success : 20;
        success = check_passing_struct(6, __arglist(1, 0, 0, 16, 1, secondExpectedValue, second)) == 0 ? success : 21;
        success = check_passing_struct(6, __arglist(0, 1, 0, 16, 1, thirdExpectedValue, third)) == 0 ? success : 22;
        success = check_passing_struct(6, __arglist(1, 1, 0, 16, 1, fourthExpectedValue, fourth)) == 0 ? success : 23;

        return success;
    }

    /// <summary>
    /// This is a helper for TestPassingStructs
    /// 
    /// </summary>
    static int TestPassingThirtyTwoByteStructs()
    {
        int success = 100;

        ThirtyTwoByteStruct first = new ThirtyTwoByteStruct();
        ThirtyTwoByteStructFloat second = new ThirtyTwoByteStructFloat();

        first.a = 20241231;
        first.b = -8213123;
        first.c = 1202;
        first.c = 1231;

        second.a = 10.102;
        second.b = 50.55;
        second.c = 80.341;
        second.b = 12000.00000000001;

        long firstExpectedValue = first.a + first.b + first.c + first.d;
        double secondExpectedValue = second.a + second.b + second.c + second.d;

        success = check_passing_struct(6, __arglist(0, 0, 0, 32, 1, firstExpectedValue, first)) == 0 ? success : 24;
        success = check_passing_struct(6, __arglist(0, 1, 0, 32, 1, secondExpectedValue, second)) == 0 ? success : 25;

        return success;
    }

    static int Main(string[] args)
    {
        int success = 100;

        if (Environment.Is64BitProcess == true) return -1;
        
        success = TestPassingInts(new int[] { 100, 299, -100, 50 }) ? success : 1;
        success = TestPassingLongs(new long[] { 100L, 299L, -100L, 50L }) ? success : 2;
        success = TestPassingFloats(new float[] { 100.0f, 299.0f, -100.0f, 50.0f }) ? success : 3;
        success = TestPassingDoubles(new double[] { 100.0d, 299.0d, -100.0d, 50.0d }) ? success : 4;

        success = TestPassingManyInts(new int[]
        {
            1002,
            40,
            39,
            12,
            14,
            -502,
            -13,
            11,
            98,
            45,
            3,
            80,
            7,
            -1,
            48,
            66,
            23,
            62,
            1092,
            -890,
            -20,
            -41,
            88,
            98,
            1,
            2,
            3,
            4012,
            16,
            673,
            873,
            45,
            85,
            -3041,
            22,
            62,
            401,
            901,
            501,
            1001,
            1002
        }) ? success : 5;

        success = TestPassingManyLongs(new long[]
        {
            1002L,
            40L,
            39L,
            12L,
            14L,
            -502L,
            -13L,
            11L,
            98L,
            45L,
            3L,
            80L,
            7L,
            -1L,
            48L,
            66L,
            23L,
            62L,
            1092L,
            -890L,
            -20L,
            -41L,
            88L,
            98L,
            1L,
            2L,
            3L,
            4012L,
            16L,
            673L,
            873L,
            45L,
            85L,
            -3041L,
            22L,
            62L,
            401L,
            901L,
            501L,
            1001L,
            1002L
        }) ? success : 6;

        success = TestPassingManyFloats(new float[]
        {
            1002,
            40,
            39,
            12,
            14,
            -502,
            -13,
            11,
            98,
            45,
            3,
            80,
            7,
            -1,
            48,
            66,
            23,
            62,
            1092,
            -890,
            -20,
            -41,
            88,
            98,
            1,
            2,
            3,
            4012,
            16,
            673,
            873,
            45,
            85,
            -3041,
            22,
            62,
            401,
            901,
            501,
            1001,
            1002
        }) ? success : 7;

        success = TestPassingManyDoubles(new double[]
        {
            1002,
            40,
            39,
            12,
            14,
            -502,
            -13,
            11,
            98,
            45,
            3,
            80,
            7,
            -1,
            48,
            66,
            23,
            62,
            1092,
            -890,
            -20,
            -41,
            88,
            98,
            1,
            2,
            3,
            4012,
            16,
            673,
            873,
            45,
            85,
            -3041,
            22,
            62,
            401,
            901,
            501,
            1001,
            1002
        }) ? success : 8;

        success = TestPassingIntsAndLongs(new int[] { 100, 200 }, new long[] { 102312131L, 91239191L }) ? success : 9;
        success = TestPassingFloatsAndDoubles(new float[] { 100.0F, 200.0F }, new double[] { 12.1231321, 441.2332132335342321 }) ? success : 10;

        // Try passing empty varargs.
        success = TestPassingEmptyInts(new int[] { }) ? success : 11;
        success = TestPassingEmptyLongs(new long[] { }) ? success : 12;
        success = TestPassingEmptyFloats(new float[] { }) ? success : 13;
        success = TestPassingEmptyDouble(new double[] { }) ? success : 14;

        //success = TestPrintF() ? success : 15;

        int returnValue = TestPassingStructs();
        success = returnValue == 100 ? success : returnValue;

        return success;
    }
}