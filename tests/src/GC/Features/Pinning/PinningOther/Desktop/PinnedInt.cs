// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Pinning of Int

using System;
using System.Runtime.InteropServices;

public class Test
{
    public static int Main()
    {
        int i = 10;
        Object temp1, temp2;

        GCHandle handle = GCHandle.Alloc(i, GCHandleType.Pinned);
        Console.WriteLine(handle.Target);

        temp1 = handle.Target;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine(handle.Target);
        temp2 = handle.Target;

        if (temp1 == temp2)
        {
            Console.WriteLine("Test passed");
            return 100;
        }
        else
        {
            Console.WriteLine("Test failed");
            return 1;
        }
    }
}
