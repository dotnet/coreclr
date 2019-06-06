// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace NetClient
{
    class Program
    {
        static int Main(string[] doNotUse)
        {
            // RegFree COM is not supported on Windows Nano
            if (TestLibrary.Utilities.IsWindowsNanoServer)
            {
                return 100;
            }

            try
            {
                Thread.CurrentThread.DisableComObjectEagerCleanup();
                new NumericTests().Run();
                new ArrayTests().Run();
                new StringTests().Run();
                new ErrorTests().Run();
                new ColorTests().Run();
                Marshal.CleanupUnusedObjectsInCurrentContext();
                Assert.IsFalse(Marshal.AreComObjectsAvailableForCleanup());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Test Failure: {e}");
                return 101;
            }

            return 100;
        }
    }
}
