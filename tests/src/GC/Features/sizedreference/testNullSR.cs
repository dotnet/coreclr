/******************************
 * UniqueSizedRef.cs
 * 
 * Check if SizedReferences that are garbage do not
 * influence the size
 * 
 * 
 * *****************************/

using System;

public class A
{
    public Object m_obj;
}

public class TestClass
{
    public static int Main(string[] args)
    {
        A a = new A();
        MySizedReference sr1 = new MySizedReference(a);
        long sz_a = sr1.ApproximateSize;
        Console.WriteLine("Size of a = {0} ", sz_a);
        sr1.Dispose();
        sr1 = null;

        for (int i = 0; i < 500; i++)
        {
            MySizedReference sr = new MySizedReference(a);
            sr.Dispose();
            sr = null;
        }

        MySizedReference sr2 = new MySizedReference(a);
        long sz2_a = sr2.ApproximateSize;
        Console.WriteLine("Size of a (2) = {0} ", sz2_a);

        if (sz_a != sz2_a)
        {
            Console.WriteLine("Test failed!");
            return 101;
        }

        Console.WriteLine("------------ Test passed ---------------");
        return 100;
    }
}