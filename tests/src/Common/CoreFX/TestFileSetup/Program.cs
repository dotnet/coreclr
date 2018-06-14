using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Text;
using CoreFX.TestUtils.TestFileSetup.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace CoreFX.TestUtils.TestFileSetup
{
    public class Program
    {
        private static TestFileHelper testFileHelper;
        private static NetCoreTestRunHelper testRunHelper;

        // Test Set-up Options
        private static string outputDir;
        private static string testUrl;
        private static string testListPath;
        private static bool cleanTestBuild = false;

        // Test Run Options
        private static string dotnetPath;
        private static bool runTests = false;
        private static int maximumDegreeOfParalellization;
        private static string logRootOutputPath;

        private static ExitCode exitCode;
        private static string executableName;
        private static IReadOnlyList<string> traitExclusions = Array.Empty<string>();

        public static void Main(string[] args)
        {
            exitCode = ExitCode.Success;
            ArgumentSyntax argSyntax = ParseCommandLine(args);
            try
            {
                SetupTests();

                if (runTests)
                {
                    if (String.IsNullOrEmpty(dotnetPath))
                        throw new ArgumentException("Please supply a test host location to run tests.");

                    if (!File.Exists(dotnetPath))
                        throw new ArgumentException("Invalid testhost path. Please supply a test host location to run tests.");

                    exitCode = RunTests();

                }
            }
            catch (AggregateException e)
            {
                e.Handle(innerExc =>
                {

                    if (innerExc is HttpRequestException)
                    {
                        exitCode = ExitCode.HttpError;
                        Console.WriteLine("Error downloading tests from: " + testUrl);
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    else if (innerExc is IOException)
                    {
                        exitCode = ExitCode.IOError;
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    else if (innerExc is JSchemaValidationException || innerExc is JsonSerializationException)
                    {
                        exitCode = ExitCode.JsonSchemaValidationError;
                        Console.WriteLine("Error validating test list: ");
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    else
                    {
                        exitCode = ExitCode.UnknownError;
                    }
                    return false;
                });
            }

            Environment.Exit((int)exitCode);
        }


        private static ArgumentSyntax ParseCommandLine(string[] args)
        {
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("out|outDir|outputDirectory", ref outputDir, "Directory where tests are downloaded");
                syntax.DefineOption("testUrl", ref testUrl, "URL, pointing to the list of tests");
                syntax.DefineOption("testListJsonPath", ref testListPath, "JSON-formatted list of test assembly names to download");
                syntax.DefineOption("clean|cleanOutputDir", ref cleanTestBuild, "Clean test assembly output directory");
                syntax.DefineOption("run|runTests", ref runTests, "Run Tests after setup");
                syntax.DefineOption("dotnet|dotnetPath", ref dotnetPath, "Path to dotnet executable used to run tests.");
                syntax.DefineOption("executable|executableName", ref executableName, "Name of the test executable to start");
                syntax.DefineOption("log|logPath|logRootOutputPath", ref logRootOutputPath, "Run Tests after setup");
                syntax.DefineOption("maxProcessCount|numberOfParallelTests|maximumDegreeOfParalellization", ref maximumDegreeOfParalellization, "Maximum number of concurrently executing processes");
                syntax.DefineOptionList("notrait", ref traitExclusions, "Traits to be excluded from test runs");

            });
            return argSyntax;
        }

        private static void SetupTests()
        {
            testFileHelper = new TestFileHelper();

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (cleanTestBuild)
            {
                testFileHelper.CleanBuild(outputDir);
            }

            // Map test names to their definitions
            Dictionary<string, XUnitTestAssembly> testAssemblyDefinitions = testFileHelper.DeserializeTestJson(testListPath);

            testFileHelper.SetupTests(testUrl, outputDir, testAssemblyDefinitions).Wait();
        }

        private static ExitCode RunTests()
        {
            testRunHelper = new NetCoreTestRunHelper(dotnetPath, logRootOutputPath);
            int result = testRunHelper.RunAllExecutablesInDirectory(outputDir, executableName, traitExclusions, maximumDegreeOfParalellization, logRootOutputPath);

            return result == 0 ? ExitCode.Success : ExitCode.TestFailure;
        }
    }
}
