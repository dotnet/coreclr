using System;

interface IFoo
{
    int Foo(int a);
}

interface IBetterFoo : IFoo
{
    int Foo(int a);
}

class FooBar : IBetterFoo
{
    public int Foo(int a)
    {
        Console.WriteLine("Calling IFoo.Foo");
        return a+1;            
    }
}

class Program
{
    public static int Main()
    {
        FooBar fooBar = new FooBar();
        IFoo foo = (IFoo) fooBar;

        Test.Assert(foo.Foo(10) == 11, "Calling IFoo.Foo on Foo");

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

