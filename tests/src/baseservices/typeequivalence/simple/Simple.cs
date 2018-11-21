// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

using TestLibrary;
using TypeEquivalenceTypes;

public class Simple
{
    private class EmptyType2 : IEmptyType
    {
        // Empty
    }

    private static void InterfaceTypesFromDifferentAssembliesAreEqual()
    {
        Console.WriteLine($"Interfaces are the same");
        var impl = (IEmptyType)new EmptyType();
        var test = (IEmptyType)new EmptyType2();

        Assert.AreEqual(impl.GetType(), impl.GetType());
    }

    public static int Main(string[] noArgs)
    {
        try
        {
            InterfaceTypesFromDifferentAssembliesAreEqual();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Test Failure: {e}");
            return 101;
        }

        return 100;
    }
}