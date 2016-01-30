// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;


public class Test
{
    public static void Usage()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("plug.exe [numIterations]");
    }

    public static int Main(string[] args)
    {
        int size = 10000;
        int power = 20;
        int numIterations = 0;
        GCHandle[] list = new GCHandle[size];

        if (args.Length == 0)
        {
            //using defaults
            numIterations = 100;
        }
        else if (args.Length == 1)
        {
            if (!Int32.TryParse(args[0], out numIterations))
            {
                Usage();
                return 1;
            }
        }
        else
        {
            Usage();
            return 1;
        }

        Console.Write("Iterations count: ");
        Console.WriteLine(numIterations);

        bool OOM = false;

        for (int j = 0; j < numIterations; j++)
        {
            for (int i = 0; i < size; i++)
            {
                GCHandleType type = GCHandleType.Normal;

                if (i % 5 == 0)
                {
                    // pin every 5th handle
                    type = GCHandleType.Pinned;
                }

                if (!list[i].IsAllocated)
                {
                    OOM = false;

                    int arraySize = 1 << (i % power);
                    try
                    {
                        byte[] b = new byte[arraySize];
                        list[i] = (GCHandle.Alloc(b, type));
                    }
                    catch (OutOfMemoryException)
                    {
                        OOM = true;
                    }
                    if (OOM)
                    {
                        Console.Write("OOM Trying to allocate array of size: ");
                        Console.WriteLine(arraySize);
                        return 1;
                    }
                }
                else
                {
                    list[i].Free();
                }
            }
        }

        return 100;
    }
}

