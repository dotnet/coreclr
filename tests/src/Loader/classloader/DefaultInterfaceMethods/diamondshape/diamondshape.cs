using System;

interface IFoo
{
    int Foo(int a);
}

class IFoo_Impl
{
    int Foo(int a)
    {
        return a;
    }
}

interface IFoo2 : IFoo
{
}

class IFoo2_Impl : IFoo
{
    int IFoo.Foo(int a)
    {
        Console.WriteLine("At IFoo2.Foo");
        return a + 1;
    }        
}

interface IFooEx : IFoo
{
}

class IFooEx_Impl : IFoo
{
    int IFoo.Foo(int a)
    {
        Console.WriteLine("At IFooEx.Foo");
        return a + 2;
    }        
}

class FooClass : IFoo2, IFooEx
{
    // Dummy
    public int Foo(int a)
    {
        return 0;
    }
}

interface I1
{
    int Func(int a);    
}

interface I2 : I1
{
    // int I1.Func(int a);
}

interface I3 : I1
{
    // int I1.Func(int a);
}

interface I4 : I2, I3
{
    // int I1.Func(int a);
}

class I4Class : I4
{
    // @REMOVE
    int I1.Func(int a)
    {
        Console.WriteLine("At I4Class.Func");
        return a + 2;
    }        
}

class Program
{
    public static void Negative()
    {
        FooClass fooObj = new FooClass();
        IFoo foo = (IFoo) fooObj;

        Console.WriteLine("Calling IFoo.Foo on Foo - expecting exception.");
        try
        {
             foo.Foo(10);
             Test.Assert(false, "Expecting exception");
        }
        catch(Exception)
        {
        }
    }

    public static void Positive()
    {
        I4Class i4 = new I4Class();
        I1 i1 = (I1) i4;
        Test.Assert(i1.Func(10) == 12, "Expecting I1.Func to land on I4.Func");
    }

    public static int Main()
    {
        Negative();
        Positive();
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

