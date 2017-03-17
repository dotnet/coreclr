using System;

interface IFoo
{
    int Foo(int a);
}

interface IBar
{
    int Bar(int b);
}

interface IFooBar : IFoo, IBar
{
    int Foo(int a);
}

class Temp : IFoo
{
    int IFoo.Foo(int a)
    {
        Console.WriteLine("IFoo.Foo");
        return a + 30;
    }
}

class FooBar : IFooBar
{
    public int Foo(int a)
    {
        Console.WriteLine("Calling IFoo.Foo");
        return a+10;            
    }

    public int Bar(int b)
    {
        Console.WriteLine("Calling IBar.Bar");
        return b+20;
    }
}

class Program
{
    public static int Main()
    {
        FooBar fooBar = new FooBar();
        IFoo foo = (IFoo) fooBar;
        IBar bar = (IBar) fooBar;

        Test.Assert(foo.Foo(10) == 30, "Calling IFoo.Foo on FooBar");
        Test.Assert(bar.Bar(10) == 20, "Calling IBar.Bar on FooBar");

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


