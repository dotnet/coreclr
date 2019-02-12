// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public class DllMapTest
{
    public static int Main()
    {
        try
        {
            DllMap.Register(Assembly.GetExecutingAssembly());
            int thirty = NativeSum(10, 20);

            if (thirty != 30)
            {
                Console.WriteLine("Wrong result from native call");
                return 101;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Unexpected exception: {0}", e.Message);
            return 102;
        }

        return 100;
    }

    [DllImport("OldLib")]
    static extern int NativeSum(int arg1, int arg2);
}
