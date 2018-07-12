using System;

namespace R2RDumpTest
{
    public class BasicTests
    {
		static int Main(string[] args)
		{
			Console.WriteLine("Starting the test");

			TestHelpers.RunTest("HelloWorld");
			
			Console.WriteLine("PASSED");
			return 100;
		}
    }
}
