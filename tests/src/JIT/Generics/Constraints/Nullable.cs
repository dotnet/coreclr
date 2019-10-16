// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.Reflection;

public class Test
{
    class G<T, U> where T : U
    {
    }

    static object s_x;

    static int Main()
    {
        try
        {
            typeof(G<,>).MakeGenericType(typeof(int), typeof(int?));

            Console.WriteLine("Test Failed1");
            return 1;
        }
        catch (ArgumentException)
        {
        }

        ///////////////////////////////////
        // Test object cast

        s_x = (object)1234;

        if (!(s_x is int?) || (int?)s_x != 1234)
        {
            Console.WriteLine("Test Failed2");
            return 1;
        }

        ///////////////////////////////////////
        ///  Test reflection

        Type nubInt = typeof(Nullable<int>);
        Type intType = typeof(int);
        Type objType = typeof(object);
        Type valTypeType = typeof(ValueType);

        // sanity checks
        // Nullable<T>  is assignable from  int
        if (!nubInt.IsAssignableFrom(intType))
        {
            Console.WriteLine("Test Failed3");
            return 1;
        }

        if (intType.IsAssignableFrom(nubInt))
        {
            Console.WriteLine("Test Failed4");
            return 1;
        }

        Type nubOfT = nubInt.GetGenericTypeDefinition();
        Type T = nubOfT.GetTypeInfo().GenericTypeParameters[0];

        // should be true
        if (!T.IsAssignableFrom(T))
        {
            Console.WriteLine("Test Failed5");
            return 1;
        }

        if (!objType.IsAssignableFrom(T))
        {
            Console.WriteLine("Test Failed6");
            return 1;
        }

        if (!valTypeType.IsAssignableFrom(T))
        {
            Console.WriteLine("Test Failed7");
            return 1;
        }

        // should be false
        // Nullable<T> is not assignable from T
        if (nubOfT.IsAssignableFrom(T))
        {
            Console.WriteLine("Test Failed8");
            return 1;
        }

        if (T.IsAssignableFrom(nubOfT))
        {
            Console.WriteLine("Test Failed9");
            return 1;
        }

        ////////////////////////////////////////
        // test again to cath caching issues
        try
        {
            typeof(G<,>).MakeGenericType(typeof(int), typeof(int?));

            Console.WriteLine("Test Failed10");
            return 1;
        }
        catch (ArgumentException)
        {
        }



        Console.WriteLine("Test Passed");
        return 100;

    }
}
