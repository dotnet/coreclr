// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices; 
using System.Runtime.InteropServices;

////////////////////////////////////////////////////////////////////////////////
// Types
////////////////////////////////////////////////////////////////////////////////

public class FastTailCallCandidates
{
    ////////////////////////////////////////////////////////////////////////////
    // Globals
    ////////////////////////////////////////////////////////////////////////////

    public static int s_ret_value = 100;

    ////////////////////////////////////////////////////////////////////////////
    // Helpers
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Check the return value of the test and set the ret_value if incorrect
    /// </summary>
    public static void CheckOutput(int code)
    {
        // If there has been a previous failure then do not reset the first
        // failure this will be the return value.
        if (s_ret_value != 100)
        {
            return;
        }

        if (code != 100)
        {
            s_ret_value = code;
        }
    }

    /// <summary>
    /// Run each individual test case
    /// </summary>
    ///
    /// <remarks>
    /// If you add any new test case scenarios please use the create a new scenario
    /// and call the method from Tester. Please increment the return value so it
    /// is easy to determine in the future which scenario is failing.
    /// </remarks>
    public static int Tester(int a)
    {
        CheckOutput(SimpleTestCase());
        CheckOutput(IntegerArgs(10, 11, 12, 13, 14, 15));
        CheckOutput(FloatArgs(10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f));
        CheckOutput(IntAndFloatArgs(10, 11, 12, 13, 14, 15, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f));
        CheckOutput(DoNotFastTailCallSimple(10, 11));
        CheckOutput(DoNotFastTailCallSimple(1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
        CheckOutput(StackBasedCaller(16, new StructSizeThirtyTwo(1, 2, 3, 4)));

        return s_ret_value;

    }

    ////////////////////////////////////////////////////////////////////////////
    // Simple fast tail call case
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Simple fast tail call case.
    /// </summary>
    ///
    /// <remarks>
    /// This is mostly supposed to be a smoke test. It can also be seen as a
    /// constant
    ///
    /// Return 100 is a pass.
    ///
    /// </remarks>
    public static int SimpleTestCase(int retValue = 10)
    {
        retValue += 1;

        if (retValue == 100)
        {
            return retValue;
        }
        else
        {
            return SimpleTestCase(retValue);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Integer args
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Simple fast tail call case that includes integer args
    /// </summary>
    /// <remarks>
    ///
    /// Return 100 is a pass.
    /// Return 101 is a failure.
    ///
    /// </remarks>
    public static int IntegerArgs(int arg1,
                                  int arg2,
                                  int arg3,
                                  int arg4,
                                  int arg5,
                                  int arg6,
                                  int retValue = 10)
    {
        retValue += 1;

        if (retValue == 100)
        {
            if (arg1 != 10 ||
                arg2 != 11 ||
                arg3 != 12 ||
                arg4 != 13 ||
                arg5 != 14 ||
                arg6 != 15)
            {
                return 101;
            }

            return retValue;
        }
        else
        {
            return IntegerArgs(arg1,
                               arg2,
                               arg3,
                               arg4,
                               arg5,
                               arg6, 
                               retValue);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Float args
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Simple fast tail call case that includes floating point args
    /// </summary>
    /// <remarks>
    ///
    /// Return 100 is a pass.
    /// Return 102 is a failure.
    ///
    /// </remarks>
    public static int FloatArgs(float arg1,
                                float arg2,
                                float arg3,
                                float arg4,
                                float arg5,
                                float arg6,
                                int retValue = 10)
    {
        retValue += 1;

        if (retValue == 100)
        {
            if (arg1 != 10.0f ||
                arg2 != 11.0f ||
                arg3 != 12.0f ||
                arg4 != 13.0f ||
                arg5 != 14.0f ||
                arg6 != 15.0f)
            {
                return 102;
            }

            return retValue;
        }
        else
        {
            return FloatArgs(arg1,
                             arg2,
                             arg3,
                             arg4,
                             arg5,
                             arg6, 
                             retValue);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Integer and Float args
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Simple fast tail call case that includes integer and floating point args
    /// </summary>
    /// <remarks>
    ///
    /// Return 100 is a pass.
    /// Return 103 is a failure.
    ///
    /// </remarks>
    public static int IntAndFloatArgs(int argi1,
                                      int argi2,
                                      int argi3,
                                      int argi4,
                                      int argi5,
                                      int argi6,
                                      float argf1,
                                      float argf2,
                                      float argf3,
                                      float argf4,
                                      float argf5,
                                      float argf6,
                                      int retValue = 10)
    {
        retValue += 1;

        if (retValue == 100)
        {
            if (argi1 != 10 ||
                argi2 != 11 ||
                argi3 != 12 ||
                argi4 != 13 ||
                argi5 != 14 ||
                argi6 != 15 ||
                argf1 != 10.0f ||
                argf2 != 11.0f ||
                argf3 != 12.0f ||
                argf4 != 13.0f ||
                argf5 != 14.0f ||
                argf6 != 15.0f)
            {
                return 103;
            }

            return retValue;
        }
        else
        {
            return IntAndFloatArgs(argi1,
                                   argi2,
                                   argi3,
                                   argi4,
                                   argi5,
                                   argi6,
                                   argf1,
                                   argf2,
                                   argf3,
                                   argf4,
                                   argf5,
                                   argf6, 
                                   retValue);
        }
    }

    /// <summary>
    /// Decision not to tail call.
    /// </summary>
    /// <remarks>
    ///
    /// The callee has 6 register arguments on x64 linux. with 8 * 8 (64) bytes
    /// stack size
    ///
    /// Return 100 is a pass.
    /// Return 104 is a failure.
    ///
    /// </remarks>
    public static int DoNotFastTailCallHelper(int one,
                                              int two,
                                              int three,
                                              int four,
                                              int five,
                                              int six,
                                              int seven,
                                              int eight,
                                              int nine,
                                              int ten,
                                              int eleven,
                                              int twelve,
                                              int thirteen,
                                              int fourteen)
    {
        if (one == 1)
        {
            two = one + two;
        }

        if  (two == 3)
        {
            three = two + three;
        }

        if (three == 6)
        {
            four = four + three;
        }

        if (three != 10)
        {
            return 104;
        }

        if (four != 4)
        {
            return 104;
        }

        if (five != 5)
        {
            return 104;
        }

        if (six != 6)
        {
            return 104;
        }
        
        if (seven != 7)
        {
            return 104;
        }

        if (eight != 8)
        {
            return 104;
        }

        if (nine != 9)
        {
            return 104;
        }

        if (ten != 10)
        {
            return 104;
        }

        if (eleven != 11)
        {
            return 104;
        }

        if (twelve != 12)
        {
            return 104;
        }

        if (thirteen != 13)
        {
            return 104;
        }

        if (fourteen != 14)
        {
            return 104;
        }
    }

    /// <summary>
    /// Decision not to tail call.
    /// </summary>
    /// <remarks>
    ///
    /// The callee has 14 register arguments on x64 linux. with 0 bytes
    /// stack size
    ///
    /// The callee should not fast tail call
    ///
    /// Return 100 is a pass.
    /// Return 104 is a failure.
    ///
    /// </remarks>
    public static int DoNotFastTailCallSimple(float one,
                                              float two,
                                              float three,
                                              float four,
                                              float five,
                                              float six,
                                              float seven,
                                              float eight,
                                              int first,
                                              int second,
                                              int third,
                                              int fourth,
                                              int fifth,
                                              int sixth)
    {
        if (one % 2 == 0)
        {
            return DoNotFastTailCallHelper((int) one,
                                           (int) two,
                                           (int) three,
                                           (int) four,
                                           (int) five,
                                           (int) six,
                                           (int) seven,
                                           (int) eight,
                                           first,
                                           second,
                                           third,
                                           fourth,
                                           fifth,
                                           sixth); // Cannot fastTailCall
        }
        else
        {
            return DoNotFastTailCallHelper((int) two,
                                           (int) one,
                                           (int) three,
                                           (int) four,
                                           (int) five,
                                           (int) six,
                                           (int) seven,
                                           (int) eight,
                                           first,
                                           second,
                                           third,
                                           fourth,
                                           fifth,
                                           sixth); // Cannot fastTailCall
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Stack Based args.
    ////////////////////////////////////////////////////////////////////////////
    
    [StructLayout(LayoutKind.Explicit, Size=8, CharSet=CharSet.Ansi)]
    public struct StructSizeThirtyTwo
    {
        [FieldOffset(0)]  public int a;
        [FieldOffset(8)] public int b;
        [FieldOffset(16)] public int c;
        [FieldOffset(24)] public int d;

        public StructSizeThirtyTwo(int a, int b, int c, int d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    };

    [StructLayout(LayoutKind.Explicit, Size=8, CharSet=CharSet.Ansi)]
    public struct EightByteStruct
    {
        [FieldOffset(0)] public int a;

        public EightByteStruct(int a)
        {
            this.a = a;
        }
    }

    /// <summary>
    /// Decision to fast tail call
    /// </summary>
    /// <remarks>
    ///
    /// The callee has one stack argument of size 8
    ///
    /// The callee should not fast tail call
    ///
    /// Return 100 is a pass.
    /// Return 105 is a failure.
    ///
    /// </remarks>
    public static void StackBasedCallee(int a, int b, EightByteStruct ebs)
    {
        if (ebs.a == 10)
        {
            return 100;
        }

        else
        {
            return 105;
        }
    }

    /// <summary>
    /// Decision to fast tail call
    /// </summary>
    /// <remarks>
    ///
    /// The caller has one stack argument of size 32
    ///
    /// The calle4 should not fast tail call
    ///
    /// Return 100 is a pass.
    /// Return 105 is a failure.
    ///
    /// </remarks>
    public static void StackBasedCaller(int i, StructSizeThirtyTwo sstt)
    {
        if (i % 2 == 0)
        {
            int a = i * 100;
            int b = i + 1100;
            return StackBasedCallee(a, b, sstt);
        }
        else
        {
            int b = i + 829;
            int a = i + 16;
            return StackBasedCallee(b, a, sstt);
        }
    }

    public static int Main()
    {
        return Tester(1);
    }
}