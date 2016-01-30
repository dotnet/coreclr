// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//This is modeled after a server executing requests
//which pin some of their newly allocated objects.

using System;
using System.Runtime.InteropServices;

public class Request
{
    private Object[] _survivors;
    private GCHandle _pin;
    public static Random r;

    public Request(int alloc_volume, float surv_fraction)
    {
        _survivors = new Object[1 + (int)(alloc_volume * surv_fraction) / 1000];
        int i = 0;
        int volume = 0;
        //allocate half of the request size.
        while (volume < alloc_volume / 2)
        {
            int alloc_surv = r.Next(1000, 2000 + 2 * i);
            //Console.WriteLine ("alloc_surv {0}", alloc_surv);
            int alloc = (int)(alloc_surv / surv_fraction) - alloc_surv;
            //Console.WriteLine ("alloc {0}", alloc);
            int j = 0;
            while (j < alloc)
            {
                int s = r.Next(100, 200 + 2 * j);

                Object x = new byte[s];
                j += s;
            }
            _survivors[i] = new byte[alloc_surv];
            i++;
            volume += alloc_surv + alloc;
        }
        //allocate one pinned buffer
        _pin = GCHandle.Alloc(new byte[100], GCHandleType.Pinned);
        //allocate the rest of the request
        while (volume < alloc_volume)
        {
            int alloc_surv = r.Next(1000, 2000 + 2 * i);
            //Console.WriteLine ("alloc_surv {0}", alloc_surv);
            int alloc = (int)(alloc_surv / surv_fraction) - alloc_surv;
            //Console.WriteLine ("alloc {0}", alloc);
            int j = 0;
            while (j < alloc)
            {
                int s = r.Next(100, 200 + 2 * j);

                Object x = new byte[s];
                j += s;
            }
            _survivors[i] = new byte[alloc_surv];
            i++;
            volume += alloc_surv + alloc;
        }
    }
    public void retire()
    {
        _pin.Free();
    }

    public static void Usage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("Fragment <num iterations> <num requests> <allocation volume> [random seed]");
    }



    static public int Main(String[] args)
    {
        int numIterations = 0;
        int allocationVolume = 0;
        int numRequests = 0;
        int randomSeed = 0;

        switch (args.Length)
        {
            case 0:
                // use defaults
                numIterations = 1000;
                numRequests = 300;
                allocationVolume = 100000;
                randomSeed = Environment.TickCount;

                Console.Write("Using defaults: Iterations=");
                Console.Write(numIterations);
                Console.Write(" Num requests:");
                Console.Write(numRequests);
                Console.Write(" Allocation volume:");
                Console.WriteLine(allocationVolume);

                break;
            case 3:
            case 4:
                if ((!Int32.TryParse(args[0], out numIterations)) ||
                     (!Int32.TryParse(args[1], out numRequests)) ||
                     (!Int32.TryParse(args[2], out allocationVolume)))
                {
                    goto default;
                }

                if (args.Length == 4)
                {
                    if (!Int32.TryParse(args[3], out randomSeed))
                    {
                        goto default;
                    }
                }
                else
                {
                    randomSeed = Environment.TickCount;
                }

                break;
            default:
                Usage();
                return 1;
        }

        Console.Write("Using random seed: ");
        Console.WriteLine(randomSeed);
        r = new Random(randomSeed);

        Request[] requests = new Request[numRequests];

        bool OOM = false;
        try
        {
            for (int j = 0; j < numIterations; j++)
            {
                int i = r.Next(0, numRequests);
                if (requests[i] != null)
                {
                    requests[i].retire();
                }
                requests[i] = new Request(allocationVolume, 0.6f);
            }
        }
        catch (OutOfMemoryException)
        {
            OOM = true;
        }

        if (OOM)
        {
            Console.WriteLine("OOM");
            Console.WriteLine("Test Failed");
            return 1;
        }

        Console.WriteLine("Test Passed");
        return 100;
    }
}


