// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//


using System;
using System.Runtime.CompilerServices;
public class BringUpTest
{
    const int Pass = 100;
    const int Fail = -1;

    [MethodImplAttribute(MethodImplOptions.NoInlining)]
    public static int Xor1(int x) { return x ^ 15; }

    public static int Main()
    {
        int y = Xor1(13);
        if (y == 2) return Pass;
        else return Fail;
    }
}
