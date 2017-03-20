using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface IFoo
{
    Type Foo<T>();
}

interface IBar<T>
{
    Type Bar1<P>();
    Type Bar2<K>();
    void Bar3<P, K>(out Type t, out Type u);
}

class FooBar<V> : IFoo, IBar<V>
{
    public Type Foo<T>()
    {
        return typeof(T);
    }

    public Type Bar1<P>()
    {
        return typeof(P);
    }

    public Type Bar2<K>()
    {
        return typeof(K);
    }

    public void Bar3<P, K>(out Type t, out Type u)
    {
        t = typeof(P);
        u = typeof(K);
    }
}


class Program
{
    static int Main(string[] args)
    {
        FooBar<object> fooBar = new FooBar<object>();
        IFoo foo = (IFoo) fooBar;
        IBar<object> bar = (IBar<object>) fooBar;

        Console.WriteLine("Calling IFoo.Foo<String>");
        Test.Assert(foo.Foo<string>() == typeof(string), "Expecting foo.Foo<string>() returning typeof(string)");

        Console.WriteLine("Calling IBar.Bar1<String>");
        Test.Assert(bar.Bar1<string>() == typeof(string), "Expecting bar.Bar1<string>() returning typeof(string)");

        Console.WriteLine("Calling IBar.Bar2<String[]>");
        Test.Assert(bar.Bar2<string[]>() == typeof(string[]), "Expecting bar.Bar2<string[]>() returning typeof(string[])");

        Type p, k;
        Console.WriteLine("Calling IBar.Bar3<String, String[]>");
        bar.Bar3<string, string[]>(out p, out k);
        Test.Assert(p == typeof(string) && k == typeof(string[]), "Expecting bar.Bar3<string>() returning typeof(string)");

        return Test.Ret();
    }
}

class Test
{
    private static bool Pass = true;

    public static int Ret()
    {
        return Pass ? 100 : 101;
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

