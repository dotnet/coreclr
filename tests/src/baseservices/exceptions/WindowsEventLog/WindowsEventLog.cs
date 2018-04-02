using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Process unhandledException = new Process();
        unhandledException.StartInfo.FileName = "..\\UnhandledException.exe";
        unhandledException.Start();

        unhandledException.WaitForExit();

        EventLog log = new
        EventLog("Application");

        DateTime dt = DateTime.Now.AddHours(-5.00);
        Console.WriteLine(dt.ToString());

        foreach (EventLogEntry entry in log.Entries)
        {
            if (entry.TimeGenerated > dt) 
            //if (entry.Source.Equals("Application Hang") && (entry.TimeGenerated > dt))
            //if (entry.Source.Equals(".NET Runtime 2.0 Error Reporting") && (entry.TimeGenerated > dt))
            {
                Console.WriteLine(entry.Source);
                Console.WriteLine(entry.EntryType);
                Console.WriteLine(entry.Message);
                Console.WriteLine("--------");
            }
        }
    }
}
