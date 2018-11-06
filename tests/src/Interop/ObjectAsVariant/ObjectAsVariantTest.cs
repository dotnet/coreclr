// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using TestLibrary;

class Test
{
    [DllImport("ObjectAsVariantNative")]
    private static extern bool Marshal_ByValue(object obj);
    [DllImport("ObjectAsVariantNative")]
    private static extern bool Marshal_ByValue_Null(object obj);


    private static void TestByValue()
    {
        var obj = new object();

        Assert.IsTrue(Marshal_ByValue(obj));

        Assert.IsTrue(Marshal_ByValue_Null(null));
    }

    public static int Main()
    {
        try
        {
            TestByValue();
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Test failed: {e.ToString()}");
            return 101;
        }
        return 100;
    }
}
