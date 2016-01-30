// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* TEST:        delete_next_card_table
 * 
 * DESCRIPTION: gains 14 blocks in gc.cpp
                mscorwks!WKS::delete_next_card_table: (7 blocks, 11 arcs)
                mscorwks!SVR::delete_next_card_table: (7 blocks, 11 arcs)
 */

using System;
using System.Collections;
using System.Runtime.InteropServices;

public class delete_next_card_table
{
    [DllImport("ProcessMemoryLimit.dll")]
    public static extern int SetProcessMemoryLimit(int MB, double percent);

    public static int Main()
    {
        int mem = SetProcessMemoryLimit(1024, 75);
        Console.WriteLine("Max Mem was set to {0} bytes", mem);

        new delete_next_card_table().DoMemoryChurn();
        return 100;
    }

    // this function attempts to allocate & free large amounts
    // of memory to ensure our objects remain pinned, don't get
    // relocated, etc...
    private void DoMemoryChurn()
    {
        Random r = new Random();
        bool OOM = false;
        for (int j = 0; j < 10; j++)
        {
            Console.Write("Churn loop {0}", j);

            OOM = false;
            try
            {
                // arraylist keeps everything rooted until we run out of memory
                ArrayList al = new ArrayList();
                int len = 1;

                for (int i = 0; i < 32; i++)        // todo: this should be based upon size of IntPtr (32 bits on 32 bit platforms, 64 on 64 bit platforms)
                {
                    Console.Write(".");

                    if (i < 30)
                    {
                        // Random.Next cannot handle negative (0x80000000) numbers
                        len *= 2;
                    }
                    al.Add(new Guid[len + r.Next(len)]);
                }
            }
            catch (OutOfMemoryException)
            {
                OOM = true;
                GC.Collect(2);
            }
            if (OOM)
                Console.WriteLine("OOM while Churning");
            Console.WriteLine();
        }
    }
}

