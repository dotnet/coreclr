// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests pinned objects

using System;
using System.Runtime.InteropServices;

public class Test
{
    public static int Main()
    {
        int[] arr = new int[10];
        GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
        IntPtr temp;

        IntPtr addr = handle.AddrOfPinnedObject();
        Console.WriteLine("Address of obj: {0}", addr);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine("Address of obj: {0}", handle.AddrOfPinnedObject());

        for (int i = 1; i <= 10; i++)
        {
            temp = (Marshal.UnsafeAddrOfPinnedArrayElement(arr, i - 1));
            Console.WriteLine("temp={0}", temp);
            long t = (long)addr + (4 * (i - 1));
            Console.WriteLine("t={0}", t);
            if (t != (long)temp)
            {
                Console.WriteLine("Test failed");
                return 1;
            }
        }

        Console.WriteLine("Test passed");
        return 100;
    }
}
