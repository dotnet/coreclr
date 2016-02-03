// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************
 *
 * Test get_Target returns the correct thing;
 * Test that an object is not cleaned up when SizedReference is the only reference to it (That basically tests that SizedReference is a strong handle)
 * Test that SizedReference is cleaned out when it gets out of scope (this is handled by the finalizer)
 * 
 * 
 * *****************************/

using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;


public class Program
{
    public static int Main(string[] args)
    {
        int bytesCount = 3000000;
        object obj = new byte[bytesCount];

        Type sizedRefType = Type.GetType("System.SizedReference");
        if (sizedRefType == null)
        {
            Console.WriteLine("Error! Can't get type");
        }
        ConstructorInfo constructor = sizedRefType.GetConstructor(new Type[] { typeof(object) });
        object sizedRefObject = constructor.Invoke(new object[] { obj });
        MethodInfo getTarget = sizedRefType.GetMethod("get_Target");

        object target = getTarget.Invoke(sizedRefObject, null);
        if (target != obj)
        {
            Console.WriteLine("Wrong target!");
            return 1;
        }

        WeakReference wr = new WeakReference(obj);
        WeakReference wrsr = new WeakReference(sizedRefObject);

        obj = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        System.Threading.Thread.Sleep(1000);
        GC.Collect();

        object target1 = wr.Target;
        if (target1 == null)
        {
            Console.WriteLine("obj was collected!");
            return 2;
        }
        target1 = null;

        MethodInfo approxSizeMeth = sizedRefType.GetMethod("get_ApproximateSize");
        long size = (long)approxSizeMeth.Invoke(sizedRefObject, null);

        //expect size to be a bit over bytesCount, since that's what we allocated in obj
        Console.WriteLine("object size = {0}", size);
        if ((size < bytesCount) || (size > bytesCount + 64))
        {
            Console.WriteLine("incorrect size for a byte array of {0}!", bytesCount);
            return 5;
        }
        sizedRefObject = null;

        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Threading.Thread.Sleep(1000);
        }
        object sro = wrsr.Target;
        if (sro != null)
        {
            Console.WriteLine("sizedRefObject was not cleaned!");
            return 3;
        }
        object target2 = wr.Target;
        if (target2 != null)
        {
            Console.WriteLine("obj was not collected!");
            return 4;
        }

        Console.WriteLine(" ------------ Test passed ---------------");
        return 100;
    }
}