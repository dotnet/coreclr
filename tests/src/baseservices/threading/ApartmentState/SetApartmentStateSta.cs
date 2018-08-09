// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

public static class SetApartmentStateStaTests
{
    [STAThread]
    public static int Main()
    {
        if (CannotChangeApartmentStateOfMainThreadTest())
        {
            return 100; // pass
        }
        return 101; // fail
    }

    private static bool CannotChangeApartmentStateOfMainThreadTest()
    {
        var thread = Thread.CurrentThread;
        try
        {
            thread.SetApartmentState(ApartmentState.MTA);

            Console.WriteLine("CannotChangeApartmentStateOfMainThreadTest: Unexpected success (no exception): 'SetApartmentState(ApartmentState.MTA)'");
            return false;
        }
        catch (InvalidOperationException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CannotChangeApartmentStateOfMainThreadTest: Unexpected exception during 'SetApartmentState(ApartmentState.MTA)': {ex}");
            return false;
        }

        try
        {
            thread.SetApartmentState(ApartmentState.STA);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CannotChangeApartmentStateOfMainThreadTest: Unexpected exception during 'SetApartmentState(ApartmentState.STA)': {ex}");
            return false;
        }

        return true;
    }
}
