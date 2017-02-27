// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

public class Base
{
    public virtual int Foo() { return 33; }
    
    static BaseSealed s_Default = new BaseSealed();

    public static Base Default => s_Default;
}

sealed class BaseSealed : Base {}

// The jit ought to be able to devirtualize the call to b.Foo below.

public class Test
{
    public static int Main()
    {
        Base b = Base.Default;
        int x = b.Foo();
        return (x == 33 ? 100 : -1);
    }
}
