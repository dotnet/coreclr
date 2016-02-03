// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Regression test for Dev 10 bug 479239: GC hangs on x86 rather than throwing OOM

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;

internal class TestClass
{
    [DllImport("ProcessMemoryLimit.dll")]
    public static extern int SetProcessMemoryLimit(int MB, double percent);

    public static int Main()
    {
        int mem = SetProcessMemoryLimit(1024, 80);
        Console.WriteLine("Memory limit set to {0} bytes", mem);

        List<byte[]> list = new List<byte[]>();

        try
        {
            while (true)
            {
                list.Add(new byte[84500]);
            }
        }
        catch (OutOfMemoryException)
        {
            list = null;
        }
        GC.Collect();
        return 100;
    }
}