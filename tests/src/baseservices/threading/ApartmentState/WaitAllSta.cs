// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

public static class WaitAllStaTests
{
    [STAThread]
    public static int Main()
    {
        if (WaitAllNotSupportedOnSta_Test0() &&
            WaitAllNotSupportedOnSta_Test1())
        {
            return 100; // pass
        }
        return 101; // fail
    }

    private static bool WaitAllNotSupportedOnSta_Test0()
    {
        var wh = new ManualResetEvent[2];
        wh[0] = new ManualResetEvent(true);
        wh[1] = new ManualResetEvent(true);
        try
        {
            bool result = WaitHandle.WaitAll(wh, 0);

            Console.WriteLine($"WaitAllNotSupportedOnSta_Test0: WaitAll did not throw but returned {result}");
        }
        catch (NotSupportedException)
        {
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WaitAllNotSupportedOnSta_Test0: WaitAll threw unexpected exception: {ex}");
        }

        return false;
    }

    private static bool WaitAllNotSupportedOnSta_Test1()
    {
        var wh = new ManualResetEvent[2];
        wh[0] = new ManualResetEvent(true);
        wh[1] = wh[0];
        try
        {
            bool result = WaitHandle.WaitAll(wh, 0);

            Console.WriteLine($"WaitAllNotSupportedOnSta_Test1: WaitAll did not throw but returned {result}");
        }
        catch (NotSupportedException)
        {
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WaitAllNotSupportedOnSta_Test1: WaitAll threw unexpected exception: {ex}");
        }

        return false;
    }
}
