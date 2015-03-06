*.NET Innovation Examples*
**GENERICS**
===================

We use generics implicitly all the time in  C#. When you use LINQ in C#, do you notice you are working with <code>IEnumerable\<T\></code>? Ever wonder what <code>T</code> is and what it means?

First introduced to the .NET Framework 2.0, generics involved changes to both the C# language and the Common Language Runtime (CLR). **Generics** are essentially a "code template" that allows developers define [type-safe](https://msdn.microsoft.com/en-us/library/hbzz1a9a%28v=vs.110%29.aspx) data structures without committing to an actual data type. For example, <code>List\<T\></code> is a [Generic Collection](https://msdn.microsoft.com/en-us/library/System.Collections.Generic(v=vs.110).aspx) that can be declared and used with any type: <code>List\<int\></code>, <code>List\<string\></code>, <code>List\<Person\></code>, etc.  

So, what's the point? Why are generics useful? The general idea in C# is that developers should have a **strongly-typed experience** with data structures. 
> A [strongly-typed ](https://msdn.microsoft.com/en-us/library/ms173104.aspx) experience means that the language predefines types of data and every variable or constant has to be described as one of those predefined types. This is also referred to as **"type-safe."** You can read more about the importance of type-safety [here](http://www.gibraltarsoftware.com/vistadb/what-you-get/technical/managed-code).

To tie this principle back to generics, you can only add <code>ints</code> to <code>List\<int\></code> and only add <code>Persons</code> to <code>List\<Person\></code>, etc. Without generics, there would be no efficient way to guarantee your <code>List</code> only contains certain types or to prevent others from adding different types to your <code>List</code>. 

Generics are also available at runtime, or **reified**. This means the runtime knows what type of data structure you are using and can store it in memory more efficiently. In Java, generics' type are unknown at runtime, so each value has to be "boxed" as an object type and stored as an object array. Type casts have to be constantly applied to prevent errors.

Here is a small program that illustrates the efficiency of knowing the data structure type at runtime:


```c#

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GenericsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            //generic list
            List<int> ListGeneric = new List<int> { 5, 9, 1, 4 };
            //non-generic list
            ArrayList ListNonGeneric = new ArrayList { 5, 9, 1, 4 };

            //timer for generic list sort
            Stopwatch s = Stopwatch.StartNew();
            ListGeneric.Sort();
            s.Stop();
            Console.WriteLine($"Generic Sort: {ListGeneric}  \n Time taken: {s.Elapsed.TotalMilliseconds}ms");

            //timer for non-generic list sort
            Stopwatch s2 = Stopwatch.StartNew();
            ListNonGeneric.Sort();
            s2.Stop();
            Console.WriteLine($"Non-Generic Sort: {ListNonGeneric}  \n Time taken: {s2.Elapsed.TotalMilliseconds}ms");

            Console.ReadLine();
        }
    }
}

```

This program yields the following output:
**Generic Sort: System.Collections.Generic.List`1[System.Int32]
 	Time taken: 0.0789ms
Non-Generic Sort: System.Collections.ArrayList
 	Time taken: 2.4324ms**

The first thing you notice here is that sorting the generic list is significantly faster than for the non-generic list. You might also notice that the type for the generic list is distinct (<code>[System.Int32]</code>) whereas the type for the non-generic list is generalized. Because the runtime knows the generic <code>List\<int\></code> is of type <code>int</code>, it can store the list elements in an underlying integer array in memory while the non-generic <code>ArrayList</code> has to cast each list element as an object as stored in an object array in memory. As shown through this example, the extra castings take up time and slow down the list sort.

The last useful thing about the runtime knowing the type of your generic is a better debugging experience. When you are debugging a generic in C#, you know what type each element is in your data structure. Without generics, you would have no idea what type each element was. 