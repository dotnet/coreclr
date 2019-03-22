// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class GitHub_18144
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void DoThis() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void DoThat() { }

    static int Main(string[] args)
    {
        var xA = Vector256<byte>.Zero;
        var xB = Vector256<byte>.Zero;
        var xC = Vector256<byte>.Zero;
        var xD = Vector256<byte>.Zero;
        var xE = Vector256<byte>.Zero;
        var xF = Vector256<byte>.Zero;
        var xG = Vector256<byte>.Zero;
        var xH = Vector256<byte>.Zero;

        DoThis();
        DoThat();

        Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", xA, xB, xC, xD, xE, xF, xG, xH);
        return 100;
    }
}
