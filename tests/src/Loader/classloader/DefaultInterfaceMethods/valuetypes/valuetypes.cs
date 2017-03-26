using System;

interface IFoo
{
    int Foo(int a);
}

interface ISum
{
    int Sum();
}

// This class is only needed to spit out IL that assumes 'this' is an object (and therefore don't box)
class FooBarStruct_
{
    public int Foo(int a)
    {
        Console.WriteLine("At ISum::Sum");
        ISum sum = (ISum) this;
        return sum.Sum() + a;
    }
}

struct FooBarStruct : IFoo, ISum
{
    public int a;
    public int b;

    public int Sum()
    {
        Console.WriteLine("At FooBarStruct::Sum");
        return a+b;
    }

    public int Foo(int a)
    {
        // Dummy code - real code is in FooBarStruct_
        return 0;
    }   
}

class Program
{
    public static int Main()
    {
        FooBarStruct fooBar = new FooBarStruct();

        fooBar.a = 10;
        fooBar.b = 20;

        IFoo foo = (IFoo) fooBar;

        Console.WriteLine("Calling IFoo.Foo on FooBarStruct");
        Test.Assert(foo.Foo(10) == 40, "Calling default method IFoo.Foo on FooBarStruct failed");

        return Test.Ret();
    }
}

class Test
{
    private static bool Pass = true;

    public static int Ret()
    {
        return Pass? 100 : 101;
    }

    public static void Assert(bool cond, string msg)
    {
        if (cond)
        {
            Console.WriteLine("PASS");
        }
        else
        {
            Console.WriteLine("FAIL: " + msg);
            Pass = false;
        }
    }
}                    

