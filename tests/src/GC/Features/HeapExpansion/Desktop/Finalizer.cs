// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
This test stimulates heap expansion on the finalizer thread
*/

using System;
using System.Runtime.InteropServices;

public class Test
{
    ~Test()
    {
        Console.WriteLine("First Alloc in Finalizer");
        Alloc2(1024 * 512, 30);
        FreeNonPins2();
        GC.Collect();

        Console.WriteLine("Second Alloc in Finalizer");
        Alloc2(1024 * 512, 50);
        FreePins2();
    }

    public static List<GCHandle> list = new List<GCHandle>();
    public static List<byte[]> blist = new List<byte[]>();
    public static List<GCHandle> list2 = new List<GCHandle>();
    public static List<byte[]> blist2 = new List<byte[]>();

    public static int Main()
    {
        Test t = new Test();
        Console.WriteLine("First Alloc");
        Alloc(1024 * 1024, 50);
        t = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine("Second Alloc");
        Alloc(1024 * 1024, 50);
        FreePins();

        Console.WriteLine("Test passed");
        return 100;
    }


    public static void Alloc(int numNodes, int percentPinned)
    {
        for (int i = 0; i < numNodes; i++)
        {
            byte[] b = new byte[10];
            b[0] = 0xC;

            if (i % ((int)(numNodes * (100 / percentPinned))) == 0)
            {
                list.Add(GCHandle.Alloc(b, GCHandleType.Pinned));
            }

            blist.Add(b);
        }
    }

    public static void FreePins()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Free();
        }
        list.Clear();
        blist.Clear();
    }

    public static void FreeNonPins()
    {
        blist.Clear();
    }

    public static void Alloc2(int numNodes, int percentPinned)
    {
        for (int i = 0; i < numNodes; i++)
        {
            byte[] b = new byte[10];
            b[0] = 0xC;

            if (i % ((int)(numNodes * (100 / percentPinned))) == 0)
            {
                list2.Add(GCHandle.Alloc(b, GCHandleType.Pinned));
            }

            blist2.Add(b);
        }
    }

    public static void FreePins2()
    {
        for (int i = 0; i < list2.Count; i++)
        {
            list2[i].Free();
        }
        list2.Clear();
        blist2.Clear();
    }

    public static void FreeNonPins2()
    {
        blist2.Clear();
    }
}

public class List<T>
{
    private int _capacity = 20; //default capacity
    private int _size = 0;
    private T[] _array;

    public List()
    {
        _array = new T[_capacity];
    }
    public List(int capacity)
    {
        _capacity = capacity;
        _array = new T[_capacity];
    }

    public int Count
    {
        get
        {
            return _size;
        }
    }

    public int Capacity
    {
        get
        {
            return _capacity;
        }
    }

    //Add an item; returns the array index at which the object was added;
    public void Add(T item)
    {
        if (_size >= _capacity) //increase capacity
        {
            int newCapacity = _capacity * 2;
            T[] newArray = new T[newCapacity];
            for (int i = 0; i < _size; i++)
            {
                newArray[i] = _array[i];
            }
            _array = newArray;
            _capacity = newCapacity;
        }


        _array[_size] = item;
        _size++;
    }

    public void RemoveAt(int position)
    {
        if (position < 0 || position >= _size)
            throw new ArgumentOutOfRangeException();

        _array[position] = default(T);

        //shift elements to fill the empty slot
        for (int i = position; i < _size - 1; i++)
        {
            _array[i] = _array[i + 1];
        }
        _size--;
    }

    //indexer
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();

            return _array[index];
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _size; i++)
        {
            _array[i] = default(T);
        }
        _size = 0;
    }
}
