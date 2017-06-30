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
    // int I1.Func(int a) { return a + 2; }
}

interface I3 : I1
{
    // int I1.Func(int a) { return a + 3; }
}

interface I4 : I2, I3
{
    // int I1.Func(int a) { return a + 4; }
}

class I4Class : I4
{
    // @REMOVE
    int I1.Func(int a)
    {
        Console.WriteLine("At I4Class.Func");
        return a + 4;
    }        
}

interface I5: I1 
{
    // int I1.Func(int a) { return a + 5; }
}

interface I6: I1 
{
    // int I1.Func(int a) { return a + 6; }
}

interface I7: I5, I6
{
    // int I1.Func(int a) { return a + 7; }
}

interface I8: I6, I7
{
    // int I1.Func(int a) { return a + 8; }
}

class I47Class: I4, I7
{
    // @REMOVE
    int I1.Func(int a)
    {
        Console.WriteLine("At I4Class.Func");
        return a + 8;
    }            

}

class I8Class: I8
{
    // @REMOVE
    int I1.Func(int a)
    {
        Console.WriteLine("At I4Class.Func");
        return a + 8;
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
             Test.Assert(false, "Expecting exception on Foo");
        }
        catch(Exception)
        {
        }

        I47Class i47Class = new I47Class();
        I1 i1 = (I1) i47Class;
        Console.WriteLine("Calling I1.Func on I47Class - expecting exception");
        try
        {
            i1.Func(10);
            Test.Assert(false, "Expecting exception on I47Class");
        }
        catch(Exception)
        {
        }
    }

    public static void Positive()
    {
        Console.WriteLine("Calling I1.Func on I4Class - expecting I4.Func");

        I4Class i4Class = new I4Class();
        I1 i1 = (I1) i4Class;
        Test.Assert(i1.Func(10) == 14, "Expecting I1.Func to land on I4.Func");
        
        Console.WriteLine("Calling I1.Func on I8Class - expecting I8.Func");

        I8Class i8Class = new I8Class();
        i1 = (I1) i8Class;
        Test.Assert(i1.Func(10) == 18, "Expecting I1.Func to land on I8.Func");
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

