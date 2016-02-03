// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime;
using System.Runtime.InteropServices;

public class LargeAllocTest
{
    [DllImport("ProcessMemoryLimit.dll")]
    public static extern void SetProcessMemoryLimit_MB(int MB);


    public const int ArraySize = (int)(1024 * 1024 * 500);

    public static int Main()
    {
        //Limit the memory this test can use to 512MB
        SetProcessMemoryLimit_MB(512);

        if (GCSettings.IsServerGC)
        {
            Console.WriteLine("LowLatency mode is not valid for Server GC.");
            Console.WriteLine("Skipping test.");
            return 100;
        }

        int initialCount, finalCount;
        GCLatencyMode oldMode = GCSettings.LatencyMode;
        byte[] b = null;

        initialCount = GC.CollectionCount(2);
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

        bool OOM = false;
        try
        {
            b = new byte[ArraySize];
        }
        catch (OutOfMemoryException)
        {
            OOM = true;
        }

        if (OOM)
            Console.WriteLine("OOM attempting to allocate {0} byte array", ArraySize);

        finalCount = GC.CollectionCount(2) - initialCount;
        GCSettings.LatencyMode = oldMode;

        Console.WriteLine("{0} gen2 collections occured", finalCount);
        if (finalCount > initialCount)
        {
            Console.WriteLine("Test Failed");
            return 1;
        }

        Console.WriteLine("Test Passed");
        return 100;
    }
}
