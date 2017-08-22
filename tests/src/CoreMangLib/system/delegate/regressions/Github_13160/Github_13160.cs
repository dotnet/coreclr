// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;

class Program
{
    public virtual void VirtualMethod()
    {
    }

    public void NonVirtualMethod()
    {
    }

    static int Main()
    {
        Program p = new Program();

        Action d1 = p.VirtualMethod;
        Action d2 = p.VirtualMethod;

        if (d1.Equals(d2))
        {
            if (d1.GetHashCode() != d2.GetHashCode())
            {
                Console.WriteLine("d1.Equals(d2) is true, but d1.GetHashCode() != d2.GetHashCode()");
                return 200;
            }
        }

        Action d3 = p.NonVirtualMethod;
        Action d4 = p.NonVirtualMethod;

        if (d3.Equals(d4))
        {
            if (d3.GetHashCode() != d4.GetHashCode())
            {
                Console.WriteLine("d3.Equals(d4) is true, but d3.GetHashCode() != d4.GetHashCode()");
                return 200;
            }
        }

        return 100;
    }
}
