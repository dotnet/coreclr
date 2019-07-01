// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This test validates that if we compile two assemblies in a large version bubble
// and then we swap one of them for another with a different MVID, we get a
// FileLoadException.

using System;
using System.IO;
using System.Runtime.CompilerServices;

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int LoadTestAssembly()
    {
        return Fragile.GetInt();
    }

    static int Main()
    {
#if V2
        try
#endif
        {
            LoadTestAssembly();
        }
#if V2
        catch (FileLoadException ex)
        {
            Console.WriteLine("---- CAUGHT EXPECTED EXCEPTION ----");
            Console.WriteLine("Message: " + ex.Message);
            string expectedMessage = "Native images generated against multiple versions of assembly test";
            return ex.Message.Contains(expectedMessage) ? 100 : 50;
        }

        return 1;
#else
        return 100;
#endif
    }
}
