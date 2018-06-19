using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreFX.TestUtils.TestFileSetup.Helpers
{
    public class NetCoreTestRunHelper
    {
        public string DotnetExecutablePath { get; set; }

        public string logRootOutputPath { get; set; }

        public int TestRunExitCode { get; set; }


        public NetCoreTestRunHelper(string DotnetExecutablePath, string logRootOutputPath)
        {
            this.DotnetExecutablePath = DotnetExecutablePath;
            this.logRootOutputPath = logRootOutputPath;
        }

        public int RunExecutable(string workingDirectory, string executableName, IReadOnlyList<string> xunitTestTraits, string logRootOutputPath)
        {
            string logPath = Path.Combine(logRootOutputPath, Path.GetFileName(workingDirectory));
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            string arguments = CalculateCommandLineArguments(workingDirectory, executableName, xunitTestTraits, Path.Combine(logPath,"testResults.xml"));

            ProcessStartInfo startInfo = new ProcessStartInfo(DotnetExecutablePath, arguments)
            {
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            };


            Process executableProcess = new Process();
            executableProcess.StartInfo = startInfo;
            executableProcess.EnableRaisingEvents = true;
            executableProcess.Exited += new EventHandler(ExitEventHandler);
            executableProcess.Start();
            executableProcess.WaitForExit();

            return executableProcess.ExitCode;
        }

        private void ExitEventHandler(object sender, EventArgs e)
        {
            TestRunExitCode = (sender as Process).ExitCode;
        }

        public int RunAllExecutablesInDirectory(string rootDirectory, string executableName, IReadOnlyList<string> xunitTestTraits, int processLimit, string logRootOutputPath = null)
        {
            int result = 0;
            // Do a Depth-First Search to find and run executables with the same name 
            Stack<string> directories = new Stack<string>();
            List<string> testDirectories = new List<string>();
            // Push rootdir
            directories.Push(rootDirectory);

            while (directories.Count > 0)
            {
                string currentDirectory = directories.Pop();

                if (File.Exists(Path.Combine(currentDirectory, executableName)))
                    testDirectories.Add(currentDirectory);

                foreach (string subDir in Directory.GetDirectories(currentDirectory))
                    directories.Push(subDir);
            }

            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = processLimit;

            Parallel.ForEach(testDirectories, parallelOptions,
                (testDirectory) =>
                {
                    if (RunExecutable(testDirectory, executableName, xunitTestTraits, logRootOutputPath) != 0)
                    {
                        Console.WriteLine("Test Run Failed " + testDirectory);
                        result = 1;
                    }
                }
                );
            return result;
        }

        private string CalculateCommandLineArguments(string testDirectory, string executableName, IReadOnlyList<string> xunitTestTraits, string logPath)
        {
            StringBuilder arguments = new StringBuilder();

            arguments.Append("\"");
            arguments.Append(Path.Combine(testDirectory, Path.GetFileName(executableName)));
            arguments.Append("\"");
            arguments.Append(" ");

            // Append test name dll
            arguments.Append("\"");
            arguments.Append(Path.Combine(testDirectory, Path.GetFileName(testDirectory)));
            arguments.Append(".dll");
            arguments.Append("\"");

            arguments.Append(" ");

            // Append RSP file
            arguments.Append("@");
            arguments.Append("\"");
            arguments.Append(Path.Combine(testDirectory, Path.GetFileName(testDirectory)));
            arguments.Append(".rsp");
            arguments.Append("\"");
            arguments.Append(" ");

            if (!String.IsNullOrEmpty(logPath))
            {
                // Add logging information
                arguments.Append("-xml");
                arguments.Append(" ");
                arguments.Append(logPath);
                arguments.Append(" ");
            }

            // Append all additional arguments
            foreach (string traitToExclude in xunitTestTraits)
            {
                arguments.Append("-notrait");
                arguments.Append(" ");
                arguments.Append(traitToExclude);
                arguments.Append(" ");
            }


            return arguments.ToString();
        }
    }
}
