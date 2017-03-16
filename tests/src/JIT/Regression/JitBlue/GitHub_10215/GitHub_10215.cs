// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Runtime.CompilerServices;


// This test represent deep execution tree that could case C stack overflow 
// in recursive tree walkers. It should work if the compiler spills deep tree periodically or
// recursive walkers are replaced with non-recursive versions.
class GitHub_10215
{
    private static int CalcBigExpressionWithoutStores()
    {
        int a = 0;
        int b = 1;
        a +=
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b + b +
        -1760 + 100;
        return a;

    }

    public static int Main()
    {
        if (CalcBigExpressionWithoutStores() == 100)
        {
            Console.WriteLine("Pass");
            return 100;
        }
        else
        {
            Console.WriteLine("Fail");
            return 0;
        }

    }
}