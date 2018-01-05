using System;

internal static class Program
{
    public static int Main()
    {
        try
        {
            Console.WriteLine("ClassStaticSpanTest");
            SpanDisallowedTests.ClassStaticSpanTest();
            Console.WriteLine("ClassInstanceSpanTest");
            SpanDisallowedTests.ClassInstanceSpanTest();
            Console.WriteLine("GenericClassInstanceSpanTest");
            SpanDisallowedTests.GenericClassInstanceSpanTest();
            Console.WriteLine("GenericInterfaceOfSpanTest");
            SpanDisallowedTests.GenericInterfaceOfSpanTest();
            Console.WriteLine("GenericStructInstanceSpanTest");
            SpanDisallowedTests.GenericStructInstanceSpanTest();
            Console.WriteLine("GenericDelegateOfSpanTest");
            SpanDisallowedTests.GenericDelegateOfSpanTest();
            Console.WriteLine("ArrayOfSpanTest");
            SpanDisallowedTests.ArrayOfSpanTest();
            Console.WriteLine("BoxSpanTest");
            SpanDisallowedTests.BoxSpanTest();
            return 100; // pass
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAIL: {0}", ex);
            return 1; // fail
        }
    }
}
