// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* Test various combinations of constraints with legal parameter types by instantiating the type
   These tests are similar to TypeParam2* only here we have a generic interface I<T> with a constraint on T 
   and in TypeParam2* we have a generic class G<T> with a constraint on T.
*/

using System;


public class A {}
public class B {}
public struct S {}
public interface I {}


// ============================================================
interface  IClass<T> where T : class {}

// TEST 1 : special constraints match
class G1<P> : IClass<P> where P : class {}

// REGRESSION TEST FOR VSW 524168
// TEST 2 : reference type constraint (class)
class G2<P> : IClass<P> where P : A {}

// ============================================================
// IStruct is defined in RunTest2.il 
// interface IStruct<T> where T : struct {}

// TEST 3 : struct constraint
class G3<P> : IStruct<P> where P : struct {}

// TEST 4: specific struct constraint
// in RunTestPos2.il 
//class G4<P> : IStruct<P> where P : S {}

// ============================================================


interface INew<T> where T : new() {}

// TEST 5 : new() constraint
class G5<P> : INew<P> where P : new() {}


// TEST 6 : struct constraint
class G6<P> : INew<P> where P : struct {}

// ============================================================


public class RunTest
{
	public static void Test1()
	{
		G1<B> obj = new G1<B>();
	}

	
	public static void Test2()
	{
		G2<A> obj = new G2<A>();
	}

	public static void Test3()
	{
		G3<S> obj = new G3<S>();
	}

	/*public static void Test4()
	{
		G4<S> obj = new G4<S>();
	}*/
	
	public static void Test5()
	{
		G5<A> obj = new G5<A>();
	}

	public static void Test6()
	{
		G6<S> obj = new G6<S>();
	}

	public static void Test7()
	{
		G6<S_PrivCtor> obj = new G6<S_PrivCtor>();
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

		Console.WriteLine("\nType: IClass<T> where T : class\n");
		Console.WriteLine("POSITIVE TESTS");
		
		Check(new Case(RunTest.Test1), "class G1<P> : IClass<P> where P : class {}");
		Check(new Case(RunTest.Test2), "class G2<P> : IClass<P> where P : A {}");

		Console.WriteLine("\nType: IStruct<T> where T : struct\n");
		Console.WriteLine("POSITIVE TESTS");

		Check(new Case(RunTest.Test3), "class G3<P> : IStruct<P> where P : struct {}");
		//Check(new Case(RunTest.Test4), "class G4<P> : IStruct<P> where P : S {}");



		Console.WriteLine("\nType: INew<T> where T : new\n");
		Console.WriteLine("POSITIVE TESTS");

		Check(new Case(RunTest.Test5), "class G5<P> : INew<P> where P : new() {}");
		Check(new Case(RunTest.Test6), "class G6<P> : INew<P> where P : struct {} - instantiate with struct");
		Check(new Case(RunTest.Test7), "class G6<P> : INew<P> where P : struct {} - instantiate with struct that has private .ctor");

		
		
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


