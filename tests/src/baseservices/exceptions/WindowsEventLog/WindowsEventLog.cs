using System;
using System.Diagnostics;
using System.IO;

class Program
{
    private static void UnhandledException()
    {
        A();
    }

    private static void FailFast()
    {
        try 
        {
            A();
        }
        catch (ArgumentException ex)
        {
            Environment.FailFast("failing fast", ex);
        }

    }

    private static void A()
    {
        B();
    }

    private static void B()
    {
        throw new ArgumentException("my ae");
    }

    private static bool LaunchTest(string testName, string[] logEntriesToCheck)
    {
        DateTime dt = DateTime.Now;

        Process testProcess = new Process();
        string currentPath = Directory.GetCurrentDirectory();
        string corerunPath = Environment.GetEnvironmentVariable("CORE_ROOT") + "\\corerun.exe";

        testProcess.StartInfo.FileName = corerunPath;
        testProcess.StartInfo.Arguments = currentPath + "\\WindowsEventLog.exe " + testName;
        testProcess.StartInfo.EnvironmentVariables["CORE_ROOT"] =  Environment.GetEnvironmentVariable("CORE_ROOT");
        testProcess.StartInfo.UseShellExecute = false;

        testProcess.Start();
        testProcess.WaitForExit();

        EventLog log = new EventLog("Application");
        int checkCount = 0;

        foreach (EventLogEntry entry in log.Entries)
        {
            if (entry.TimeGenerated > dt) 
            {
                String source = entry.Source;
                String message = entry.Message;

                foreach (string logEntry in logEntriesToCheck)
                {
                    if (message.Contains(logEntry))
                        checkCount += 1;
                }

                if (source.Contains(".NET Runtime") && checkCount == logEntriesToCheck.Length)
                    return true;
            }
        }
        return false;
    }
    
    private static bool RunUnhandledExceptionTest()
    {
        string[] logEntriesToCheck = {"unhandled exception", "ArgumentException"};
        return LaunchTest("UnhandledException", logEntriesToCheck);
    }

    private static bool RunFailFastTest()
    {
        string[] logEntriesToCheck = {"The application requested process termination through System.Environment.FailFast", "ArgumentException"};
        return LaunchTest("FailFast", logEntriesToCheck);
    }


    public static int Main(string[] args)
    {
        if (args.Length == 0) // When invoked with no args, launch itself with appropriate args to cause various exceptions
        {
            if (!RunUnhandledExceptionTest())
            {
                Console.WriteLine("WindowsEventLog Test: UnhandledExceptionTest failed.");
                return 1;
            }
        
            if (!RunFailFastTest())
            {
                Console.WriteLine("WindowsEventLog Test: FailFastTest failed.");
                return 1;
            }

            return 100;
        }

        Debug.Assert(args.Length == 1);

        if (args[0] == "UnhandledException")
        {
            UnhandledException();
        }
        else if (args[0] == "FailFast")
        {
            FailFast();
        }

        return 100; // Should never reach here 
    }
}
