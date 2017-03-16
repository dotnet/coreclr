using System;

interface IBlah
{
    int Blah(int c);    
}

interface IFoo
{
    int Foo(int a);
}

interface IBar
{
    int Bar(int b);
}

class Base : IBlah
{
    public int Blah(int c)
    {
        Console.WriteLine("Calling IBlah.Blah");
        return c+20;
    }
}

class FooBar : Base, IFoo, IBar
{
    public int Foo(int a)
    {
        Console.WriteLine("Calling IFoo.Foo");
        return a+1;            
    }

    public int Bar(int b)
    {
        Console.WriteLine("Calling IBar.Bar");
        return b+10;
    }
}

class Program
{
    public static int Main()
    {
        FooBar fooBar = new FooBar();
        IFoo foo = (IFoo) fooBar;
        IBar bar = (IBar) fooBar;
        IBlah blah = (IBlah) fooBar;

        Test.Assert(foo.Foo(10) == 11, "Calling IFoo.Foo on FooBar");
        Test.Assert(bar.Bar(10) == 20, "Calling IBar.Bar on FooBar");
        Test.Assert(blah.Blah(10) == 30, "Calling IBlah.Blah on FooBar");

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

