// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* Test various combinations of constraints with illegal parameter types by instantiating the type
   These tests are similar to TypeParam2* only here we have a generic interface I<T> with a constraint on T 
   and in TypeParam2* we have a generic class G<T> with a constraint on T.
*/

using System;



public class Test
{
	static bool pass;
	static int testNumber = 1;

       delegate void Case();	

	
	static void Check(Case mytest,  string testName, string genArgNumber, string violatingType, string type, string typeParam)
    	{

		Console.Write("Test"+testNumber + ": " + testName);
		++testNumber;

		
		try
		{
			mytest();

			Console.WriteLine("\nFAIL: Did not catch expected TypeLoadException");
			pass = false;
		}
		catch (TypeLoadException e)
		{
		  	// Unhandled Exception: System.TypeLoadException: %0, '%1', on '%2' 
		  	// violates the constraint of type parameter '%3'.
		  	
			Test.CheckTypeLoadExceptionMessage(8310, genArgNumber, e, violatingType, type, typeParam);
		}
	
		catch (Exception e) 
		{
			Console.WriteLine("\nFAIL: Caught unexpected exception: " + e);
			pass = false;
		}	

	}


	 public static void CheckTypeLoadExceptionMessage(uint ResourceID, string genArgNumber, TypeLoadException e, string violatingType, string type, string typeParam )
	 {
        bool found1 = e.ToString().IndexOf(genArgNumber) >= 0;
        bool found2 = e.ToString().IndexOf(violatingType) >= 0;
        bool found3 = e.ToString().IndexOf(type) >= 0;
        bool found4 = e.ToString().IndexOf(typeParam) >= 0;
        
		if (!found1 || !found2 || !found3 || !found4)
		{
		    Console.WriteLine(" : Exception message is incorrect");
		    Console.WriteLine("Actual: " + e.Message.ToString());
	   	    pass = false;
		}
		else
		{
		    Console.WriteLine(" : Caught expected exception");
		}	
	}

	public static int Main()
	{
		pass = true;

		Console.WriteLine("\nType: IClass<T> where T : class\n");
		Console.WriteLine("NEGATIVE TESTS");
		
		Check(new Case(RunTest.Test1), "class G1<P> : IClass<P> where P : struct {}, insantiate with class","0",  "P", "IClass`1[T]", "T");
		Check(new Case(RunTest.Test2), "class G1<P> : IClass<P> where P : struct {}, instantiate with valuetype", "0",  "P", "IClass`1[T]", "T");
		Check(new Case(RunTest.Test3), "class G2<P> : IClass<P> where P : I {}", "0",  "P", "IClass`1[T]", "T");
		Check(new Case(RunTest.Test4), "class G3<P> : IClass<P> where P : System.Object {}", "0", "P", "IClass`1[T]", "T");
		Check(new Case(RunTest.Test5), "class G4<P> : IClass<P> where P : System.ValueType {}", "0", "P", "IClass`1[T]", "T");
		Check(new Case(RunTest.Test6), "class G5<P> : IClass<P> where P : System.Enum {}", "0", "P", "IClass`1[T]", "T");		
	
		
		Console.WriteLine("\nType: IStruct<T> where T : struct\n");
		Console.WriteLine("NEGATIVE TESTS");

		
		Check(new Case(RunTest.Test7), "class G6<P> : IStruct<P> where P : class {}, insantiate with class","0",  "P", "IStruct`1[T]", "T");
		Check(new Case(RunTest.Test8), "class G6<P> : IStruct<P> where P : class {}, instantiate with valuetype", "0",  "P", "IStruct`1[T]", "T");
		Check(new Case(RunTest.Test9), "class G7<P> : IStruct<P> where P : A {}", "0",  "P", "IStruct`1[T]", "T");
		Check(new Case(RunTest.Test10), "class G8<P> : IStruct<P> where P : I {}", "0",  "P", "IStruct`1[T]", "T");

		Check(new Case(RunTest.Test14), "class G12<P> : IStruct<P> where P : System.ValueType {}", "0",  "P", "IStruct`1[T]", "T");
		Check(new Case(RunTest.Test15), "class G12<P> : IStruct<P> where P : System.Nullable<int> {}", "0",  "P", "IStruct`1[T]", "T");



		Console.WriteLine("\nType: INew<T> where T : new() \n");
		Console.WriteLine("NEGATIVE TESTS");

		
		Check(new Case(RunTest.Test11), "class G9<P> : INew<P> where P : A {}, insantiate with class","0",  "P", "INew`1[T]", "T");
		Check(new Case(RunTest.Test12), "class G10<P> : INew<P> where P : class {}, instantiate with valuetype", "0",  "P", "INew`1[T]", "T");
		Check(new Case(RunTest.Test13), "class G11<P> : INew<P> where P : I {}", "0",  "P", "INew`1[T]", "T");


		
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


