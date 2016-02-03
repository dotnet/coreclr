// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Pinned handle
// Should throw an InvalidOperationException for accessing the AddrOfPinnedObject()
// for a different type of handle.

using System;
using System.Runtime.InteropServices;

public class Test
{
    public static int Main()
    {
        int[] arr = new int[100];
        GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
        GCHandle hhnd = GCHandle.Alloc(handle, GCHandleType.Normal);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine("Address of obj: {0}", handle.AddrOfPinnedObject());
        try
        {
            Console.WriteLine("Address of handle {0}", hhnd.AddrOfPinnedObject());
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine("Expected: " + e);
            Console.WriteLine("Test passed");
            return 100;
        }
        catch (Exception e)
        {
            Console.WriteLine("Not expected: " + e);
            Console.WriteLine("Test failed! Incorrect type exception");
            return 2;
        }

        Console.WriteLine("Test failed! no exception");
        return 3;
    }
}
