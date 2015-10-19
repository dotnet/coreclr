// <Area> Generics - Expressions - specific catch clauses </Area>
// <Title> 
// catch type parameters bound by Exception or a subclass of it in the form catch(T)
// </Title>
// <RelatedBugs> </RelatedBugs>  

//<Expects Status=success></Expects>

// <Code> 

using System;

public class GenException<T> : Exception {}

public class GenExceptionSub<T> : GenException<T> {}

public class Gen<Ex,T> where Ex : GenException<T> 
{
	public static void ExceptionTest(Ex e)
	{
		try
		{
			throw e;
		}
		catch(Ex E)
		{
			Test.Eval(Object.ReferenceEquals(e,E));
		}
		catch
		{
			Console.WriteLine("Caught Wrong Exception");
			Test.Eval(false);
		}
	}
}

public class Test
{
	public static int counter = 0;
	public static bool result = true;
	public static void Eval(bool exp)
	{
		counter++;
		if (!exp)
		{
			result = exp;
			Console.WriteLine("Test Failed at location: " + counter);
		}
	
	}
	
	public static int Main()
	{
		Gen<GenException<int>,int>.ExceptionTest(new GenExceptionSub<int>());
		Gen<GenException<string>,string>.ExceptionTest(new GenExceptionSub<string>());
		Gen<GenException<Guid>,Guid>.ExceptionTest(new GenExceptionSub<Guid>());
		
		if (result)
		{
			Console.WriteLine("Test Passed");
			return 100;
		}
		else
		{
			Console.WriteLine("Test Failed");
			return 1;
		}
	}
		
}

// </Code>
