using System;

public class Program 
{
    public static int Main() 
    {
        return RunTest();
    }

    public static int RunTest()
    {
        Helper helper = new Helper();
        string lastMethodName = String.Empty;
        try
        {
            lastMethodName = helper.GetLastMethodName();
        }
        catch (System.MissingMethodException e)
        {
            if((System.Environment.GetEnvironmentVariable("LargeVersionBubble") != null))
            {
                // Cross-Assembly inlining is only allowed in multi-module version bubbles
                Console.WriteLine("FAIL");
                return 101;
            }
            else
            {
                // The missing method is expected in the default crossgen case (i.e. no large version bubble)
                Console.WriteLine("PASS");
                return 100;
            }
        }

        if (lastMethodName != "GetLastMethodName")
        {
            // method in helper.cs has been inlined
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