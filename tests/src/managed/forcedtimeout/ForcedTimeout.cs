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
        Thread.Sleep(TimeSpan.FromMinutes(12));
        return 100;
    }
}
