// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************
 * 
 * Test that if a SizedReference returns the same size whether or not it 
 * is the unique holder of the object A.
 * 
 * 
 * *****************************/

using System;
using System.Reflection;

public class A
{
    public A(Object o1, Object o2, Object o3)
    {
        m_Obj1 = o1;
        m_Obj2 = o2;
        m_Obj3 = o3;
    }

    public Object m_Obj1;
    public Object m_Obj2;
    public Object m_Obj3;
}

public class Program
{
    public static int Main(string[] args)
    {
        Type sizedRefType = Type.GetType("System.SizedReference");
        if (sizedRefType == null)
        {
            Console.WriteLine("Error! Can't get type");
            return 1;
        }

        ConstructorInfo constructor = sizedRefType.GetConstructor(new Type[] { typeof(object) });
        byte[] b1;
        byte[] b2;
        byte[] b3;
        object a;

        try
        {
            b1 = new byte[1000000];
            b2 = new byte[2000000];
            b3 = new byte[300000];
            a = new A(b1, b2, b3);
        }
        catch (OutOfMemoryException)
        {
            Console.WriteLine("Out of memory");
            return 1;
        }

        object sizedRefObj = constructor.Invoke(new object[] { a });

        MethodInfo Mymethodinfo = sizedRefType.GetMethod("get_ApproximateSize");

        GC.Collect();
        long sizeofa = (long)(Mymethodinfo.Invoke(sizedRefObj, null));
        Console.WriteLine("Size of object a is {0}", sizeofa);

        WeakReference wr = new WeakReference(a);
        Console.WriteLine("Make object a null");
        a = null;
        b1 = null;
        b2 = null;
        b3 = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        object target = wr.Target;
        if (target == null)
        {
            Console.WriteLine("Target is null");
            return 103;
        }

        GC.Collect();
        long sizeofa2 = (long)(Mymethodinfo.Invoke(sizedRefObj, null));
        Console.WriteLine("Size of object a is {0}", sizeofa);

        if (sizeofa != sizeofa2)
        {
            Console.WriteLine("Error! Size is not the same after a is made null");
            return 101;
        }


        Console.WriteLine("------------ Test passed ---------------");
        return 100;
    }
}