// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Pinning of objects
// Cannot pin objects or array of objects

using System;
using System.Runtime.InteropServices;

public class Test
{
    public static int Main()
    {
        Object[] arr = new Object[100];
        Object obj = new Object();
        int exceptionCount = 0;

        Console.WriteLine("This test should throw 2 exceptions");

        try
        {
            GCHandle handle1 = GCHandle.Alloc(arr, GCHandleType.Pinned);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Caught: {0}", e);
            exceptionCount++;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("Test failed. Wrong type of exception when trying to allocate a pinned handle for an array of System.Object");
        }
        if (exceptionCount == 0)
        {
            Console.WriteLine("Test failed. no exception thrown when trying to allocate a pinned handle for an array of System.Object");
        }

        bool exc = false;
        try
        {
            GCHandle handle2 = GCHandle.Alloc(obj, GCHandleType.Pinned);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Caught: {0}", e);
            exceptionCount++;
            exc = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("Test failed. Wrong type of exception when trying to allocate a pinned handle for System.Object");
        }
        if (!exc)
        {
            Console.WriteLine("Test failed. no exception thrown when trying to allocate a pinned handle for System.Object");
        }

        if (exceptionCount == 2)
        {
            Console.WriteLine("Test passed");
            return 100;
        }

        Console.WriteLine("Test failed");
        return 1;
    }
}
