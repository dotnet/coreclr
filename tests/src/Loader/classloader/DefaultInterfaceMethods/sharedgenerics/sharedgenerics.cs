using System;

interface IFoo<T>
{
    Type Foo(T a);
}

interface IBar<in T>
{
    Type Bar(T b);
}

class FooBar<T, U> : IFoo<T>, IBar<U>
{
    public Type Foo(T a)
    {
        Console.WriteLine("Calling IFoo.Foo:" + a.ToString());
        return typeof(T);            
    }

    public Type Bar(U b)
    {
        Console.WriteLine("Calling IBar.Bar:" + b.ToString());
        return typeof(U);
    }
}

class Program
{
    public static int Main()
    {
        FooBar<string, object> fooBar = new FooBar<string, object>();
        IFoo<string> foo = (IFoo<string>) fooBar;
        IBar<string[]> bar = (IBar<string[]>) fooBar;

        Test.Assert(foo.Foo("ABC") == typeof(string), "Calling IFoo.Foo on FooBar");
        Test.Assert(bar.Bar(new string[] { "ABC" }) == typeof(object), "Calling IBar.Bar on FooBar");

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

