// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.InteropServices;

// Represents a problem with contained nodes chaines, that contain lclVar reads, that were moved through lclVar stores.
// Notice that the project file sets complus_JitStressModeNames.

[StructLayout(LayoutKind.Explicit)]
internal class AA
{

    [FieldOffset(8)]
    public QQ q;


    public static AA[] a_init = new AA[101];

    public static AA[] a_zero = new AA[101];


    public AA(int qq)
    {

        this.q = new QQ(qq);
    }


    public static void reset()
    {
        AA.a_init[100] = new AA(1);
        AA.a_zero[100] = new AA(2);
    }
}

internal class QQ
{
    public int val;

    public QQ(int vv)
    {
        this.val = vv;
    }

    public int ret_code()
    {
        return 100;
    }
}

internal class TestApp
{

    private static int test_2_2(int num)
    {
        int result;
        if (AA.a_init[num].q != AA.a_zero[num].q) 
        // Access field with contained IND instruction.
        // EQ marks its operands as contained too. 
        // AA.a_init[num].q and AA.a_zero[num].q are allocated to the same lclVar.
        // So we calculate AA.a_init[num].q and store as tmp0, use this temp to do nullCheck.
        // Then store AA.a_zero[num].q as tmo0, destroy the old value and try to do EQ thinking that 
        // tmp0 is AA.a_init[num].q.
        // It needs stress (complus_JitStressModeNames=STRESS_NULL_OBJECT_CHECK, STRESS_MAKE_CSE)
        // to force the compiler to do implicit null checks and store values as local variables.
        {
            result = 100;
        }
        else
        {
            result = AA.a_zero[num].q.val;
        }
        return result;
    }

    private static int Main()
    {
        AA.reset();
        int result;

        int r = TestApp.test_2_2(100);
        if (r != 100)
        {
            Console.WriteLine("Failed.");
            result = 101;
        }
        else
        {
            Console.WriteLine("Passed.");
            result = 100;
        }
        return result;
    }
}
