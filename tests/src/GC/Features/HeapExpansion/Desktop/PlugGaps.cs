// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
This test fragments the heap with ~50 byte holes, then allocats ~50 byte objects to plug them
*/

using System;
using System.Runtime.InteropServices;

public class Test
{
    public static List<GCHandle> gchList = new List<GCHandle>();
    public static List<byte[]> bList = new List<byte[]>();

    public static int Main()
    {
        Console.WriteLine("Beginning phase 1");

        for (int i = 0; i < 1024 * 1024; i++)
        {
            byte[] unpinned = new byte[50];
            byte[] pinned = new byte[10];
            bList.Add(unpinned);
            gchList.Add(GCHandle.Alloc(pinned, GCHandleType.Pinned));
        }

        Console.WriteLine("phase 1 complete");


        // losing all live references to the unpinned byte arrays
        // this will fragment the heap with ~50 byte holes
        bList.Clear();
        bList = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Console.WriteLine("Beginning phase 2");

        bList = new List<byte[]>();
        for (int i = 0; i < 1024 * 1024; i++)
        {
            byte[] unpinned = new byte[50];
            bList.Add(unpinned);
        }

        Console.WriteLine("phase 2 complete");

        GC.KeepAlive(gchList);
        GC.KeepAlive(bList);

        return 100;
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

