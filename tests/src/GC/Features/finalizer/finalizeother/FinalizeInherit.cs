// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Tests Finalize() with Inheritance

using System;

namespace One
{
    internal abstract class A
    {
    }

    internal class B : A
    {
        ~B()
        {
            Console.WriteLine("In Finalize of B");
        }
    }

    internal class C : B
    {
        public static int count = 0;
        ~C()
        {
            Console.WriteLine("In Finalize of C");
            count++;
        }
    }
}

namespace Two
{
    using One;
    internal class D : C
    {
    }
}

namespace Three
{
    using One;
    using Two;

    internal class CreateObj
    {
        // disabling unused variable warning
#pragma warning disable 0414
        private B _b;
        private D _d;
#pragma warning restore 0414
        private C _c;

        public CreateObj()
        {
            _b = new B();
            _c = new C();
            _d = new D();
        }

        public bool RunTest()
        {
            A a = _c;

            _d = null;
            _b = null;
            a = null;
            _c = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return (C.count == 2);
        }
    }

    internal class Test
    {
        private static int Main()
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
}
