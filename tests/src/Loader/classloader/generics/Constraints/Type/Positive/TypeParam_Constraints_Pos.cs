// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* Test various combinations of constraints with legal parameter types by instantiating the type

*/

using System;

public class A
{}

public class B : A {}

public interface I  {}

public class C : I  {}

public struct S : I{}

public enum Enum1 {One, Two, Three}

public delegate void Del1();	



// TEST 1
public class GClass<T,U> where T : U where U : class
{}

// TEST 2
public class GNew<T,U> where T : U where U : new()
{}

// TEST 3
public class GClassType<T,U> where T : U where U : A
{}

// TEST 4
public class GInterfaceType<T,U> where T : U where U : I
{}

// TEST 5
// defined in RunTestPos.il
// public class GStruct<T,U> where T : U where U : struct {}


public class Test1
{
	public static void RunTest()
	{
		// GClass<T,U> where T : U where U : class --> T has to be convertible to U and U has to be a reference type
		GClass<Object,Object> obj1 = new GClass<Object,Object>(); 	// mscorlib reference type
		GClass<String,Object> obj2 = new GClass<String,Object>();		// mscorlib reference type
		GClass<A,Object> obj3 = new GClass<A,Object>();				// user defined reference type (Class)
		GClass<I,Object> obj4 = new GClass<I,Object>();				// user defined reference type (Interface)
		GClass<A[],Object> obj5 = new GClass<A[],Object>();			// user defined array

		GClass<Del1,Object> obj6 = new GClass<Del1,Object>();							// user defined delegate
		GClass<Enum1,Object> obj7 = new GClass<Enum1,Object>();						// user defined enum
		GClass<System.ValueType,Object> obj8 = new GClass<System.ValueType,Object>();			
		GClass<Enum1,System.ValueType> obj9 = new GClass<Enum1,System.ValueType>();		
		GClass<Nullable<int>,Object> obj10 = new GClass<Nullable<int>,Object>();			// user defined Nullable<T>
		GClass<B,A> obj11 = new GClass<B,A>();
		GClass<C,I> obj12 = new GClass<C,I>();
	}
}

public class Test2
{
	public static void RunTest()
	{
		// GNew<T,U> where T : U where U : new() --> T has to be convertible to U and U has to have default cctor
		GNew<Object,Object> obj1 = new GNew<Object,Object>(); 	// mscorlib reference type
		GNew<String,Object> obj2 = new GNew<String,Object>();		// mscorlib reference type
		GNew<A,Object> obj3 = new GNew<A,Object>();				// user defined reference type (Class)
		GNew<I,Object> obj4 = new GNew<I,Object>();				// user defined reference type (Interface)
		GNew<A[],Object> obj5 = new GNew<A[],Object>();			// user defined array
		GNew<Del1,Object> obj6 = new GNew<Del1,Object>();							// user defined delegate
		GNew<Enum1,Object> obj7 = new GNew<Enum1,Object>();						// user defined enum
		GNew<System.ValueType,Object> obj8 = new GNew<System.ValueType,Object>();			
		GNew<Nullable<int>,Object> obj9 = new GNew<Nullable<int>,Object>();			// user defined Nullable<T>
		GNew<B,A> obj10 = new GNew<B,A>();
		GNew<int,int> obj11 = new GNew<int,int>();									// primitive type int
	}
}

public class Test3
{
	public static void RunTest()
	{
		// GClassType<T,U> where T : U where U : A --> T has to be castable to U and U has to be castable to A
		GClassType<B,A> obj11 = new GClassType<B,A>();
	}
}


public class Test4
{
	public static void RunTest()
	{
		// public class GInterfaceType<T,U> where T : U where U : I--> T has to be castable to U and U has to be castable to I

		GInterfaceType<C,I> obj12 = new GInterfaceType<C,I>();
		GInterfaceType<S,I> obj13 = new GInterfaceType<S,I>();
	}
}

public class Test5
{
	public static void RunTest()
	{
		
		GStruct<S,S> obj14 = new GStruct<S,S>();
		GStruct<int,int> obj15 = new GStruct<int,int>();
	}
}

public class Test
{
	static bool pass;
	static int testNumber = 1;

       delegate void Case();	

	
	static void Check(Case mytest, string testName)
    	{

		Console.Write("Test"+testNumber + ": " + testName);
		++testNumber;

		
		try
		{
			mytest();

			Console.WriteLine(" : PASS");
			return;
		}
		catch (TypeLoadException e)
		{
			Console.WriteLine("\nFAIL: Caught unexpected TypeLoadException: " + e);
			pass = false;
			return;		
		}
	
		catch (Exception e) 
		{
			Console.WriteLine("\nFAIL: Caught unexpected exception: " + e);
			pass = false;
		}	

	}

	

	public static int Main()
	{
		pass = true;

		Console.WriteLine("POSITIVE TESTS");
		
		Check(new Case(Test1.RunTest), "Generic argument is type param constrainted to 'class'" );
		Check(new Case(Test2.RunTest), "Generic argument is type param constrainted to 'new()'" );
		Check(new Case(Test3.RunTest), "Generic argument is type param constrainted to 'class-type A'" );
		Check(new Case(Test4.RunTest), "Generic argument is type param constrainted to 'interface-type I'" );

		Check(new Case(Test5.RunTest), "Generic argument is type param constrainted to 'struct'" );
		//Check(new Case(Test5.Test2), "Generic argument is type param constrainted to 'struct'" );
		
		if (pass)
		{
			Console.WriteLine("PASS");
			return 100;
		}
		else
		{
			Console.WriteLine("FAIL");
			return 101;
		}
	
	}
}



