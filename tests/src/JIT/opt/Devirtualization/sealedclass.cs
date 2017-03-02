// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

public class Base
{
    public virtual int GetValue(int value)
    {
        return 0x33;
    }
}

public sealed class Derived : Base
{
    public override int GetValue(int value)
    {
        return value;
    }
}

public class F
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TestSealedMethodInlining(Derived obj)
    {
        return obj.GetValue(3);
    }

    public static int Main(string[] args)
    {
        Derived d = new Derived();
        int v = TestSealedMethodInlining(d);
        return (v == 3 ? 100 : -1);
    }
}
