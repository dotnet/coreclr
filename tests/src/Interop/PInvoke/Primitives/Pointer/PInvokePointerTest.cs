// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using TestLibrary;

unsafe class Program
{
    [StructLayout(LayoutKind.Sequential)]
    struct NonBlittable
    {
        bool _nonBlittable;
    }

    [DllImport("Unused")]
    private static extern void PointerToNonBlittableType(NonBlittable* pNonBlittable);

    static int Main()
    {
        Assert.Throws<MarshalDirectiveException>(() => PointerToNonBlittableType(null));

        return 100;
    }
}
