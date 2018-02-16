// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Tests GC.Collect()

using System;
using System.Diagnostics;
using System.Collections.Generic;

public class Test 
{
    static Random Rand = new Random();

    public static void GetAllocatedBytesForCurrentThread(int size)
    {
        int startCount = GC.CollectionCount(0);
        long start = GC.GetAllocatedBytesForCurrentThread();

        GC.KeepAlive(new String('a', size));

        long end = GC.GetAllocatedBytesForCurrentThread();
        int endCount = GC.CollectionCount(0);

        if (start == end)
        {
            Console.WriteLine("start: {0} same as end??! attach debugger, GC {1}->{2}",
                start, startCount, endCount);
            Console.ReadLine();
        }
    }

    static int Alloc(List<object> list, int size)
    {
        int toAlloc = Rand.Next(size / 2 , (int)((float)size * 1.5));
        Console.WriteLine("allocating {0} bytes", toAlloc);
        int allocated = 0;

        while (allocated < toAlloc)
        {
            int s = Rand.Next(100, 1000);
            allocated += s + 24;
            byte[] b = new byte[s];
            list.Add((object)b);
        }
        return allocated;
    }

    static void TestWithAlloc()
    {
        int allocatedBytes = 0;
        for (int i = 0; i < 100; i++)
        {
            List<object> list = new List<object>();
            allocatedBytes = Alloc(list, 80*1024*1024);

            GetAllocatedBytesForCurrentThread (100000);
            Console.WriteLine("iter {0} allocated {1} bytes", i, allocatedBytes);
        }
    }

    // In core 1.0 we didn't have the API exposed so needed to use reflection to get it.
    // This should be split into 2 tests, with and without GC.Collect.
    static void TestCore1()
    {
        const string name = "GetAllocatedBytesForCurrentThread";
        var typeInfo = typeof(GC).GetTypeInfo();
        var method = typeInfo.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        long nBytesBefore = 0;
        long nBytesAfter = 0;

        int countBefore = GC.CollectionCount(0);

        for (int i = 0; i < 10000; ++i)
        {
            nBytesBefore = (long)method.Invoke(null, null);
            // Test with collection.
            // GC.Collect();
            nBytesAfter = (long)method.Invoke(null, null);

            if ((nBytesBefore + 24) != nBytesAfter)
            {
                int countAfter = GC.CollectionCount(0);
                Console.WriteLine("b: {0}, a: {1}, iter {2}, {3}->{4}", nBytesBefore, nBytesAfter, i, countBefore, countAfter);
                Debug.Assert(false);
            }
        }
    }

    public static int Main() 
    {
        TestCore1();

        TestWithAlloc();

        return 0;
    }
}
