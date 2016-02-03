// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Pinning many objects
// Here we create 2500 arrays and pin them all

using System;
using System.Runtime.InteropServices;
public class Test
{
    public static int Main()
    {
        int NUM = 2500;
        int[][] arr = new int[NUM][];
        GCHandle[] handle = new GCHandle[NUM];

        IntPtr[] oldaddr = new IntPtr[NUM];

        int[] temp;
        for (int i = 0; i < NUM; i++)
        {
            arr[i] = new int[NUM];
            temp = new int[NUM];
            handle[i] = GCHandle.Alloc(arr[i], GCHandleType.Pinned);
            oldaddr[i] = handle[i].AddrOfPinnedObject();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        for (int i = 0; i < NUM; i++)
        {
            if (handle[i].AddrOfPinnedObject() != oldaddr[i])
            {
                Console.WriteLine("Test failed! Address of pinned objects changed");
                return 1;
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        for (int i = 0; i < NUM; i++)
        {
            if (handle[i].IsAllocated != true)
            {
                Console.WriteLine("Test failed!Handle should be allocated");
                return 2;
            }
        }

        Console.WriteLine("Test passed!");
        return 100;
    }
}
