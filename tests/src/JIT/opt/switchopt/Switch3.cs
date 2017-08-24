// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal class Switch3
{
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int value(int a)
    {
        return a;
    }

    private static int Main(string[] args)
    {
        int i = 0;
        int val = value(2);

        switch (val)
        {
            case 1:
            {
                i = value(1);
            }
            break;
            case 2:
            {
                i = value(99);
            }
            break;
            case 3:
            {
                i = value(68);
            }
            break;
        }

        bool failed = !(i == 99);    

        if (failed)
        {
            Console.WriteLine("Test Failed");
            return 101;
        }
        else
        {
            Console.WriteLine("Passed");
            return 100;
        }
    }
}
