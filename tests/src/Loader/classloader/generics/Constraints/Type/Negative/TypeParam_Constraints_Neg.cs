// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* Test various combinations of constraints with illegal parameter types by instantiating the type

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

		Console.WriteLine("NEGATIVE TESTS");
		
		Console.WriteLine("\nType: GClass<T> where T : U where U : class \n");
		
		Check(new Case(RunGClassTest.Test1), "T: class A, U: class B", "0", "A",  "GClass`2[T,U]", "T");
		Check(new Case(RunGClassTest.Test2), "T: interface I, U: class C",  "0", "I", "GClass`2[T,U]", "T");
		Check(new Case(RunGClassTest.Test3), "T: Object, U: class A[]",  "0", "System.Object", "GClass`2[T,U]", "T" );
		Check(new Case(RunGClassTest.Test4), "T: Object, U: ValueType",  "0", "System.Object", "GClass`2[T,U]", "T");
		Check(new Case(RunGClassTest.Test5), "T: int, U: int",  "1", "System.Int32", "GClass`2[T,U]", "U" );

		Console.WriteLine("\nType: GStruct<T> where T : U where U : struct \n");
		
		Check(new Case(RunGStructTest.Test1), "T: class A, U: class B", "0", "A",  "GStruct`2[T,U]", "T");
		Check(new Case(RunGStructTest.Test2), "T: interface I, U: class C",  "0", "I", "GStruct`2[T,U]", "T");
		Check(new Case(RunGStructTest.Test3), "T: Object, U: valuetype S",  "0", "System.Object", "GStruct`2[T,U]", "T" );
		Check(new Case(RunGStructTest.Test4), "T: valuetype S, U: ValueType",  "1", "System.ValueType", "GStruct`2[T,U]", "U");
		Check(new Case(RunGStructTest.Test5), "T: int U: Object",  "1", "System.Object", "GStruct`2[T,U]", "U" );




		Console.WriteLine("\nType: GNew<T> where T : U where U : new() \n");

		Check(new Case(RunGNewTest.Test1), "T: Enum1, U: ValueType", "1", "System.ValueType",  "GNew`2[T,U]", "U");
		Check(new Case(RunGNewTest.Test2), "T: class C, U: interface I",  "1", "I", "GNew`2[T,U]", "U");
		Check(new Case(RunGNewTest.Test3), "T: class A[], U: class A[]",  "1", "A[]", "GNew`2[T,U]", "U" );
		Check(new Case(RunGNewTest.Test4), "T: Object, U: class A",  "0", "System.Object", "GNew`2[T,U]", "T");
		Check(new Case(RunGNewTest.Test5), "T: int, U: int",  "1", "System.ValueType", "GNew`2[T,U]", "U" );
		
		Console.WriteLine("\nType: GClassType<T> where T : U where U : A \n");

		Check(new Case(RunGClassTypeTest.Test1), "T: String, U: Object", "1", "System.Object",  "GClassType`2[T,U]", "U");
		Check(new Case(RunGClassTypeTest.Test2), "T: class A, U: Object",  "1", "System.Object", "GClassType`2[T,U]", "U");
		Check(new Case(RunGClassTypeTest.Test3), "T: class A, U: class A[]",  "1", "A[]", "GClassType`2[T,U]", "U" );
		Check(new Case(RunGClassTypeTest.Test4), "T: Object, U: class A",  "0", "System.Object", "GClassType`2[T,U]", "T");
		Check(new Case(RunGClassTypeTest.Test5), "T: int, U: int",  "1", "System.Int32", "GClassType`2[T,U]", "U" );


		Console.WriteLine("\nType: GClassType<T> where T : U where U : I \n");

		Check(new Case(RunGInterfaceTypeTest.Test1), "T: String, U: Object", "1", "System.Object",  "GInterfaceType`2[T,U]", "U");
		Check(new Case(RunGInterfaceTypeTest.Test2), "T: interface I, U: Object",  "1", "System.Object", "GInterfaceType`2[T,U]", "U");
		Check(new Case(RunGInterfaceTypeTest.Test3), "T: interface I[], U: interface I[]",  "1", "I[]", "GInterfaceType`2[T,U]", "U" );
		Check(new Case(RunGInterfaceTypeTest.Test4), "T: Object, U: interface I",  "0", "System.Object", "GInterfaceType`2[T,U]", "T");
		Check(new Case(RunGInterfaceTypeTest.Test5), "T: int, U: int",  "1", "System.Int32", "GInterfaceType`2[T,U]", "U" );

		
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


