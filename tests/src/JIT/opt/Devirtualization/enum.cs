// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

enum MyEnum { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten };

// ToString should be devirtualizable

public class Test
{
    public static int Main()
    {
        string s = (MyEnum.Seven).ToString();
        return (s.Length == "Seven".Length ? 100 : -1);
    }
}
