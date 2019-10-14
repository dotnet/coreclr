// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;

class highperformancecounter
{
    static int Main()
    {
        if (HighPerformanceCounter_Frequency() <= 0)
        {
            Console.WriteLine("ERROR: Frequency is not a positive number");
            return -1;
        }

        ulong count1 = HighPerformanceCounter_TickCount();
        if (count1 == 0)
        {
            Console.WriteLine("ERROR: TickCount returned 0.");
            return -1;
        }

        System.Threading.Thread.Sleep(1);

        ulong count2 = HighPerformanceCounter_TickCount();

        if (count2 <= count1)
        {
            Console.WriteLine("ERROR: TickCount did not advance.");
            return -1;
        }

        Console.WriteLine("PASSED");
        return 100;
    }

    public static ulong HighPerformanceCounter_Frequency()
    {
        Type HighPerformanceCounter = typeof(object).Assembly.GetType("System.HighPerformanceCounter");
        var Frequency = HighPerformanceCounter.GetProperty("Frequency",
            bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        return (ulong)Frequency.GetValue(null);
    }

    public static ulong HighPerformanceCounter_TickCount()
    {
        Type HighPerformanceCounter = typeof(object).Assembly.GetType("System.HighPerformanceCounter");
        var TickCount = HighPerformanceCounter.GetProperty("TickCount",
            bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        return (ulong)TickCount.GetValue(null);
    }
}
