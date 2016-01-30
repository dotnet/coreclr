// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * NOTE: must set COMPLUS_GCSegmentSize=0x400000
 */

using System;
using System.Collections.Generic;
using System.Threading;

public class Test
{
    public static bool fail = false;

    public static int Main()
    {
        Console.WriteLine("Please ensure COMPLUS_GCSegmentSize=0x400000");
        Console.WriteLine();

        Thread[] threads = new Thread[Math.Max(Environment.ProcessorCount * 2, 64)];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(new ThreadStart(Allocate));
            threads[i].Name = i.ToString();
            threads[i].Start();
        }

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Join();
        }

        if (fail)
        {
            Console.WriteLine("OOM. Test Failed");
            return 0;
        }

        Console.WriteLine("Test Passed");
        return 100;
    }

    public static void Allocate()
    {
        try
        {
            List<byte[]> list = new List<byte[]>();
            for (int i = 0; i < 10000; i++)
            {
                byte[] b = new byte[8000];
                if (i % 10 == 0)
                {
                    list.Add(b);
                }
            }
        }
        catch (OutOfMemoryException)
        {
            fail = true;
        }
    }
}
