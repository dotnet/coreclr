// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;

public class ModesTest
{
    [DllImport("ProcessMemoryLimit.dll")]
    public static extern void SetProcessMemoryLimit(int MB, double percent);

    public static int Main()
    {
        SetProcessMemoryLimit(1024, 75);

        if (GCSettings.IsServerGC)
        {
            Console.WriteLine("LowLatency is disabled in Server GC mode.");
            Console.WriteLine("Test not run.");
            return 100;
        }

        GCLatencyMode oldMode = GCSettings.LatencyMode;
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

        List<byte[]> list = new List<byte[]>();

        bool OOM = false;
        try
        {
            for (int i = 0; i < 10000; i++)
            {
                list.Add(new byte[84950]);
            }
        }
        catch (OutOfMemoryException)
        {
            OOM = true;
        }
        finally
        {
            list.Clear();
            list.TrimExcess();
        }

        if (OOM)
            Console.WriteLine("OOM!");

        int initialCount = GC.CollectionCount(2);

        Console.WriteLine("{0} Gen2 Collections in LowLatency mode", initialCount);

        GC.Collect();

        GCSettings.LatencyMode = oldMode;

        OOM = false;
        try
        {
            for (int i = 0; i < 10000; i++)
            {
                list.Add(new byte[84950]);
            }
        }
        catch (OutOfMemoryException)
        {
            OOM = true;
        }
        finally
        {
            list.Clear();
            list.TrimExcess();
        }

        if (OOM)
            Console.WriteLine("OOM!");

        int finalCount = GC.CollectionCount(2);

        Console.WriteLine("{0} Gen2 Collections since returning to {1}", finalCount, oldMode);
        Console.WriteLine();

        if (finalCount <= initialCount)
        {
            Console.WriteLine("Test Failed");
            return 1;
        }

        Console.WriteLine("Test Passed");
        return 100;
    }
}
