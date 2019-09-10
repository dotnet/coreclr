// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.Threading;

class C
{
    static int Main()
    {
        Throw();
        return 100;
    }

    private static void Throw()
    {
        throw new Exception();
    }
}
