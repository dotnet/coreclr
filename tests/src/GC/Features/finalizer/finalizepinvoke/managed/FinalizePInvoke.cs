// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Finalize() and WaitForPendingFinalizers()

using System;
using System.Threading;
using System.Runtime.InteropServices;

public class Test
{
    [DllImport("Unmanaged.dll")]
    public static extern int myadd(int a, int b);
    [DllImport("Unmanaged.dll")]
    public static extern int mysub(int a, int b);
    [DllImport("Unmanaged.dll")]
    public static extern int mydiv(int a, int b);
    [DllImport("Unmanaged.dll")]
    public static extern int mymul(int a, int b);

    public class Dummy
    {
        public static bool visited;
        ~Dummy()
        {
            int c;
            Console.WriteLine("In Finalize() of Dummy");

            c = myadd(5, 4);
            Console.WriteLine("c is {0}", c);
            c = mysub(5, 4);
            Console.WriteLine("c is {0}", c);
            c = mymul(5, 4);
            Console.WriteLine("c is {0}", c);

            try
            {
                c = mydiv(5, 0); // should throw a DivideByZero exception in Unmanaged.dll
                Console.WriteLine("c is {0}", c);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught: " + e);
            }
            visited = true;
        }
    }

    public class CreateObj
    {
        // disabling unused variable warning
#pragma warning disable 0414
        private Dummy _obj;
#pragma warning restore 0414

        public CreateObj()
        {
            _obj = new Dummy();
        }

        public bool RunTest()
        {
            _obj = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();  // makes sure Finalize() is called.

            return Dummy.visited;
        }
    }

    public static int Main()
    {
        CreateObj temp = new CreateObj();

        if (temp.RunTest())
        {
            Console.WriteLine("Test Passed");
            return 100;
        }
        Console.WriteLine("Test Failed");
        return 1;
    }
}


