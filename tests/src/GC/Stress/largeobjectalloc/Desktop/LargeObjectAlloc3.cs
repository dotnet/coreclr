// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Allocate nested objects of increasing size ranging from 200 KB to ~ 25 MB
// If memory is low, after every loop, the large objects should be collected
// and committed from the LargeObjectHeap
// The Finalizer makes sure that the GC is actually collecting the large objects


using System;

namespace LargeObjectTest
{
    public class OtherLargeObject
    {
        // disabling unused variable warning
#pragma warning disable 0414
        private int[] _otherarray;
#pragma warning restore 0414
        public OtherLargeObject()
        {
            _otherarray = new int[1024 * 50];
        }
    }

    public class LargeObject
    {
        private OtherLargeObject[] _olargeobj;

        public LargeObject(int size)
        {
            _olargeobj = new OtherLargeObject[size];
            for (int i = 0; i < size; i++)
            {
                _olargeobj[i] = new OtherLargeObject();
            }
        }

        ~LargeObject()
        {
            Console.WriteLine("In finalizer");
            Test.exitCode = 100;
        }
    }

    public class Test
    {
        public static int exitCode = 1;
        public static int Main()
        {
            int size = 1;
            int loop = 1;
            LargeObject largeobj;


            Console.WriteLine("Test should pass with ExitCode 100\n");
            exitCode = 1;

            while (loop < 100)
            {
                Console.WriteLine("Loop: {0}", loop);
                for (int i = 0; i <= 7; i++)
                {
                    try
                    {
                        largeobj = new LargeObject(size);
                        Console.WriteLine("Allocated LargeObject: {0} bytes", size * 4 * 1024 * 50);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failure to allocate in loop {0}\n", loop);
                        Console.WriteLine("Caught Exception: {0}", e);
                        return exitCode;
                    }
                    largeobj = null;
                    GC.Collect();
                    GC.Collect();
                    Console.WriteLine("LargeObject Collected");
                    size *= 2;
                }
                size = 1;
                loop++;
            }

            Console.WriteLine("Test Passed");
            GC.Collect();

            return exitCode;
        }
    }
}

