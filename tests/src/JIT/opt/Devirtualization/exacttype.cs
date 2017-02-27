// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

public class Base 
{
    public virtual void Foo() { Console.WriteLine("Base:Foo"); }
}

public class Derived : Base
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override sealed void Foo() { Console.WriteLine("Derived:Foo"); }
}

// The jit ought to be able to devirtualize all calls to Foo
// since the exact type is known.

public class Test
{
    public static Base M()
    {
        return new Derived();
    }

    public static int Main()
    {
        Derived d = new Derived();
        d.Foo();

        M().Foo();

        // Copy via 'b' currently inhibits devirt
        Base b = M();
        b.Foo();

        return 100;
    }
}


        
    
