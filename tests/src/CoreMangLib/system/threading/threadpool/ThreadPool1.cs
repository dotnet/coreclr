// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class ThreadPoolTest
{
    public static int Main()
    {
        var test = new ThreadPoolTest();

        TestLibrary.TestFramework.BeginTestCase("ThreadPool");

        if (test.RunTests())
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("PASS");
            return 100;
        }
        else
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("FAIL");
            return 0;
        }
    }

    public bool RunTests()
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal &= Test1();

        return retVal;
    }

    public bool Test1()
    {
        bool retVal = true;

        int counter;
        void SleepingWorker()
        {
            while (true)
            {
                var cnt = Interlocked.Decrement(ref counter);
                if (cnt <= 0)
                    break;

                Task.Run(() => SleepingWorker());
                if (cnt % 10 == 0)
                {
                    System.Console.Write('.');
                }
                Thread.Sleep(500);
            }
        }

        TestLibrary.TestFramework.BeginScenario("Sleeping workers");
        
        try
        {
            counter = Environment.ProcessorCount * 5;

            var sw = Stopwatch.StartNew();
            Task.Run(() => SleepingWorker()).Wait();
            sw.Stop();

            // should take just above 0.5 sec.
            if (sw.ElapsedMilliseconds > 2000)
                throw new Exception($"timeout, expected: {2000}ms, took:{sw.ElapsedMilliseconds}");
        }
        catch (Exception e)
        {
            // any exception causes test failure.
            TestLibrary.TestFramework.LogError("001", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }
}
