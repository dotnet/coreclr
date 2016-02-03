/******************************
 * SharedRef2.cs
 * 
 * Tests the size returned by SizedReference different objects share reference the same object:
 * Object a references b and c;
 * b references d;
 * c references d;
 * Verify that the size of d is added only once to a.
 * 
 * 
 * *****************************/

using System;
using System.Threading;
using System.Reflection;

public class A
{
    public A(Object o1, Object o2)
    {
        m_b = o1;
        m_c = o2;
    }

    public Object m_b;
    public Object m_c;
}

public class B
{
    public B(Object o1, Object o2)
    {
        m_d = o1;
        m_e = o2;
    }
    public Object m_d;
    public Object m_e;
}

public class C
{
    public C(Object o1)
    {
        m_d = o1;
    }

    public Object m_d;
}

public class Program
{
    public static int Main(string[] args)
    {
        byte[] d;
        byte[] e;

        try
        {
            d = new byte[40000000];
            e = new byte[20000000];
        }
        catch (OutOfMemoryException)
        {
            Console.WriteLine("Out of memory exception");
            return 1;

        }

        C c = new C(d);
        B b = new B(d, e);
        A a = new A(b, c);
   
        MySizedReference sr_c = new MySizedReference(c);
        long sizeof_c = sr_c.ApproximateSize;
        Console.WriteLine("Size of c is: {0}", sizeof_c);
        sr_c.Dispose();
        sr_c = null;

        //find the base size of a
        A baseA = new A(null, null);
        MySizedReference sr_baseA = new MySizedReference(baseA);
        long sizeof_baseA = sr_baseA.ApproximateSize;
        Console.WriteLine("Size of base a is: {0}", sizeof_baseA);
        sr_baseA.Dispose();
        sr_baseA = null;

        //find the size of b not including d
        B baseB = new B(null, e);
        MySizedReference sr_baseB = new MySizedReference(baseB);
        long sizeof_baseB = sr_baseB.ApproximateSize;
        Console.WriteLine("Size of b without d is: {0}", sizeof_baseB);
        sr_baseB.Dispose();
        sr_baseB = null;


        MySizedReference sr_a = new MySizedReference(a);
        long sizeof_a = sr_a.ApproximateSize;
        Console.WriteLine("Size of a is: {0}", sizeof_a);
        sr_a.Dispose();
        sr_a = null;
        if (sizeof_a != sizeof_baseA + sizeof_baseB + sizeof_c)
        {
            Console.WriteLine("Incorrect size! Test failed");
            return 101;
        }


        Console.WriteLine("------------ Test passed ---------------");
        return 100;
    }

}