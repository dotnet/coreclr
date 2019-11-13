// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

public class Test
{

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task CompletedTask()
    {
        for (int i = 0; i < 100; i++)
            await Task.CompletedTask;
    }

    public static int Main()
    {
        CompletedTask();
        return 100;
    }
}
