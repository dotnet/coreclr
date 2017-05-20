// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

// This test is intended to ensure that the JIT properly incorporates divide-by-zero and null-reference exceptions into
// value numbers. Prior to the fix for GitHub #8648, value numbers for indirect loads and divide/modulus operators did
// not incorporate these exceptions, which could lead to the optimizer improperly elminiating side effects.

class Program
{
    int y;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int Case1(sbyte x)
    {
        return x % 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Case2(Program p, int x, int z)
    {
        return (x * 2) + (p.y * z);
    }

    static int Main()
    {
        bool success = true;
        try
        {
            Case1((sbyte)42);
            Console.WriteLine("Case 1 failed: expected a DivideByZeroException, but none was thrown");
            success = false;
        }
        catch (DivideByZeroException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine("Case 1 failed: expected a DivideByZeroException, but {0} was thrown instead", e);
            success = false;
        }

        try
        {
            Case2(null, 5, 0);
            Console.WriteLine("Case 2 failed: expected a NullReferenceException, but none was thrown");
            success = false;
        }
        catch (NullReferenceException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine("Case 2 failed: expected a NullReferenceException, but {0} was thrown instead", e);
            success = false;
        }

        return success ? 100 : 0;
    }
}
