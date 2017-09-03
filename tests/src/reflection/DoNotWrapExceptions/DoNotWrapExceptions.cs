// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;

public class Test
{
    const string ExceptionMessage = "What, an exception, in my program?";

    static bool sThrowInCtor = true;

    public Test()
    {
        if (sThrowInCtor)
            throw new Exception(ExceptionMessage);
    }

    public Test(int i)
    {
        throw new Exception(ExceptionMessage);
    }

    public static void ThrowingMethod()
    {
        throw new Exception(ExceptionMessage);
    }

    public static bool ThrowingProperty
    {
        get
        {
            throw new Exception(ExceptionMessage);
        }
        set
        {
            throw new Exception(ExceptionMessage);
        }
    }

    const BindingFlags BindingFlags_DoNotWrapExceptions = (BindingFlags)0x02000000;

    private static bool TestWithAndWithoutFlags(Action<BindingFlags> func)
    {
        try
        {
            //Console.ReadLine();
            func(BindingFlags.Default);
            return false;
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException;
            if (inner == null)
            {
                Console.WriteLine("No wrapped exceptioned");
                Console.WriteLine("FAIL");
                return false;
            }

            if (inner.GetType() != typeof(Exception))
            {
                Console.WriteLine("Exception is not the expected type: {0}", inner);
                Console.WriteLine("FAIL");
                return false;
            }

            if (inner.Message != ExceptionMessage)
            {
                Console.WriteLine("Exception message is not the expected one: {0}", inner);
                Console.WriteLine("FAIL");
                return false;
            }
        }

        try
        {
            //Console.ReadLine();
            func(BindingFlags_DoNotWrapExceptions);
            return false;
        }
        catch (TargetInvocationException)
        {
            Console.WriteLine("Got TargetInvocationException when it was not expected.");
            Console.WriteLine("FAIL");
            return false;
        }
        catch (Exception ex)
        {
            if (ex.GetType() != typeof(Exception))
            {
                Console.WriteLine("Exception is not the expected type: {0}", ex);
                Console.WriteLine("FAIL");
                return false;
            }

            if (ex.Message != ExceptionMessage)
            {
                Console.WriteLine("Exception message is not the expected one: {0}", ex);
                Console.WriteLine("FAIL");
                return false;
            }
        }

        return true;
    }

    public static int Main()
    {
        try
        {
            var mi = typeof(Test).GetMethod(nameof(ThrowingMethod));
            if (!TestWithAndWithoutFlags(flags => mi.Invoke(null, flags, null, null, null)))
                return 101;

            var pi = typeof(Test).GetProperty(nameof(ThrowingProperty));
            if (!TestWithAndWithoutFlags(flags => pi.GetGetMethod().Invoke(null, flags, null, null, null)))
                return 102;
            if (!TestWithAndWithoutFlags(flags => pi.GetSetMethod().Invoke(null, flags, null, new object[] { false }, null)))
                return 103;

            var ci = typeof(Test).GetConstructor(Type.EmptyTypes);
            if (!TestWithAndWithoutFlags(flags => ci.Invoke(flags, null, null, null)))
                return 104;

            if (!TestWithAndWithoutFlags(flags => Activator.CreateInstance(typeof(Test), flags, null, null, null)))
                return 105;
            //Try with activation cache.
            sThrowInCtor = false;
            Activator.CreateInstance(typeof(Test));
            sThrowInCtor = true;
            if (!TestWithAndWithoutFlags(flags => Activator.CreateInstance(typeof(Test), flags, null, null, null)))
                return 106;

            if (!TestWithAndWithoutFlags(flags => Activator.CreateInstance(typeof(Test), flags, null, new object[] { 1 }, null)))
                return 107;

            // no exceptions caught
            Console.WriteLine("PASS");
            return 100;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("{0} \n Caught unexpected exception.", ex);
            Console.WriteLine("FAIL");
            return 99;
        }
    }
}
