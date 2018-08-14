using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JitBench
{
    class CscEmptyMainBenchmark : CscBenchmark
    {
        public CscEmptyMainBenchmark() : base("Csc_Empty_Main")
        {
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task SetupSourceToCompile(string intermediateOutputDir, string runtimeDirPath, bool useExistingSetup, ITestOutputHelper output)
#pragma warning restore CS1998
        {
            string emptyMainDir = Path.Combine(intermediateOutputDir, "emptyMainSource");
            const string emptyMainFileName = "Program.cs";
            string emptyMainPath = Path.Combine(emptyMainDir, emptyMainFileName);
            string systemPrivateCoreLibPath = Path.Combine(runtimeDirPath, "System.Private.CoreLib.dll");
            string systemRuntimePath = Path.Combine(runtimeDirPath, "System.Runtime.dll");
            string systemConsolePath = Path.Combine(runtimeDirPath, "System.Console.dll");
            CommandLineArguments = $"{emptyMainFileName} /nostdlib /r:" + systemPrivateCoreLibPath + " /r:" + systemRuntimePath + " /r:" + systemConsolePath;
            WorkingDirPath = emptyMainDir;
            if(useExistingSetup)
            {
                return;
            }

            FileTasks.DeleteDirectory(emptyMainDir, output);
            FileTasks.CreateDirectory(emptyMainDir, output);
            File.WriteAllLines(emptyMainPath, new string[]
            {
                "using System;",
                "public static class Program",
                "{",
                "    public static int Main(string[] args) => 0;",
                "}"
            });
        }
    }
}
