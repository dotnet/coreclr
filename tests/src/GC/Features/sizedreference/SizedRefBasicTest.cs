/******************************
 * SizedRefBasicTest.cs
 * 
 * Basic test for SizedReference.
 * The test must be run with non-concurrent GC.
 * Tests that:
 * - size of null object is 0;
 * - base size of Object is 12 (on 32bits)
 * - the size of a byte array is base size + number of bytes
 * - if object a references object b, then size of a = size of a without b + size of b
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
    const int MAX_BYTE_ARRAY_SIZE = 500000000;
    const int BASE_OBJECT_SIZE_32bit = 12;  //This should be the base size of Object (on 32bits)
    const int BASE_OBJECT_SIZE_64bit = 24;  //This should be the base size of Object (on 64bits)

    static unsafe int GetPointerSize()
    {
        return sizeof(void*);
    }

    public static int Main(string[] args)
    {
        bool is64bit = false;   //32 bit if false

        //find out if the test is running on 32bit or 64bit
        int pointerSize = GetPointerSize();
        if (pointerSize == 4)
        {
            Console.WriteLine("Running on 32 bit machine");
        }
        else if (pointerSize == 8)
        {
            is64bit = true;
            Console.WriteLine("Running on 64 bit machine");
        }
        else
        {
            Console.WriteLine("Error");
            return -1;
        }

        int byteArraySize;
        if (args.Length > 0)
        {
            byteArraySize = Convert.ToInt32(args[0]);

            if (byteArraySize < 0)
            {
                Console.WriteLine("Error! Invalid argument");
                return -1;
            }
        }
        else
        {
            //find a random number for byteArraySize
            Random rand = new Random();

            byteArraySize = rand.Next(0, MAX_BYTE_ARRAY_SIZE);

            Console.WriteLine("Random number is {0}; Repro with argument {1}", byteArraySize, byteArraySize);

        }

        MySizedReference srNull = new MySizedReference(null);
        long nullObjSize = srNull.ApproximateSize;
        Console.WriteLine("Null object size = {0}", nullObjSize);
        if (nullObjSize != 0)
        {
            Console.WriteLine("Error! The size of a null object is not zero!");
            return 2;
        }

        //check the size of Object; it must be BASE_OBJECT_SIZE
        Object o = new Object();

        MySizedReference srBaseObject = new MySizedReference(o);

        long baseObjSize = srBaseObject.ApproximateSize;
        srBaseObject.Dispose();
        Console.WriteLine("Base object size = {0}", baseObjSize);
        if (is64bit && baseObjSize != BASE_OBJECT_SIZE_64bit)
        {
            Console.WriteLine("Error! Base object size should be = {0}", BASE_OBJECT_SIZE_64bit);
            return 3;
        }
        else if (!is64bit && baseObjSize != BASE_OBJECT_SIZE_32bit)
        {
            Console.WriteLine("Error! Base object size should be = {0}", BASE_OBJECT_SIZE_32bit);
            return 4;
        }

        //check the size of a byte array
        byte[] b;
        try
        {
            b = new byte[byteArraySize];
        }
        catch (OutOfMemoryException)
        {
            Console.WriteLine("Out of memory when trying to allocate a byte array of size {0}", byteArraySize);
            return 5;
        }
        //check the size of b
        MySizedReference srByteArrayObject = new MySizedReference(b);

        long size_b = srByteArrayObject.ApproximateSize;
        Console.WriteLine("The size of the byte array is {0} ", size_b);
        srByteArrayObject.Dispose();

        if (size_b != byteArraySize + baseObjSize)
        {
            Console.WriteLine("Error! The size of the byte array should be = {0}", byteArraySize + baseObjSize);
            return 6;
        }

        //if object a references object b, then size of a = size of a without b + size of b
        A a = new A();
        MySizedReference sra = new MySizedReference(a);
        long baseSize_a = sra.ApproximateSize;
        Console.WriteLine("Base size of a = {0}", baseSize_a);
        sra.Dispose();

        a.m_obj = b;
        MySizedReference sra2 = new MySizedReference(a);
        long size_a = sra2.ApproximateSize;
        Console.WriteLine("Size of a = {0}", size_a);
        if (size_a != baseSize_a + size_b)
        {
            Console.WriteLine("Error! Incorrect size");
            return 7;
        }


        Console.WriteLine("------------ Test passed ---------------");
        return 100;
    }
}