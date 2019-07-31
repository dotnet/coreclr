// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using TestLibrary;

class LPTStrTest
{
    public static int Main()
    {
        try
        {
            CommonStringTests.RunTests();
            RunStringBuilderTests();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return 101;
        }
        return 100;
    }

    private static void RunStringBuilderTests()
    {
        int length = 10;
        StringBuilder nullTerminatorBuilder = new StringBuilder(length);
        Assert.IsTrue(LPTStrTestNative.Verify_NullTerminators_PastEnd(nullTerminatorBuilder, length));
        Assert.IsTrue(LPTStrTestNative.Verify_NullTerminators_PastEnd_Out(nullTerminatorBuilder, length));
    }
}
