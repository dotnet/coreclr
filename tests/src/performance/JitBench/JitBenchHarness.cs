using CommandLine;
using CommandLine.Text;
using Microsoft.Xunit.Performance.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace JitBench
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = JitBenchHarnessOptions.Parse(args);

            s_temporaryDirectory = Path.Combine(options.IntermediateOutputDirectory, "JitBenchHarness");
            if (Directory.Exists(s_temporaryDirectory))
                Directory.Delete(s_temporaryDirectory, true);
            Directory.CreateDirectory(s_temporaryDirectory);

            s_jitBenchDevDirectory = Path.Combine(s_temporaryDirectory, "JitBench-dev");

            using (var h = new XunitPerformanceHarness(args))
            {
                var startInfo = Setup();
                PrintHeader("Running Benchmark Scenario");
                h.RunScenario(startInfo, () => { }, PostIteration, PostProcessing, s_ScenarioConfiguration);
            }
        }

        static Program()
        {
            s_ScenarioConfiguration = new ScenarioConfiguration(TimeSpan.FromMilliseconds(20000)) {
                Iterations = 11
            };

            // Set variables we will need to store results.
            s_iteration = 0;
            s_startupTimes = new double[s_ScenarioConfiguration.Iterations];
            s_requestTimes = new double[s_ScenarioConfiguration.Iterations];
        }

        private static ProcessStartInfo Setup()
        {
            PrintHeader("Starting SETUP");

            var dotnetDirectory = Path.Combine(s_jitBenchDevDirectory, ".dotnet");
            var dotnetProcessFileName = Path.Combine(dotnetDirectory, "dotnet.exe");

            //Download and extract the repo.
            using (var client = new HttpClient())
            {
                // Download the JitBench repository and extract it.
                var url = @"https://github.com/guhuro/JitBench/archive/dev.zip"; // TODO: This should be updated to https://github.com/aspnet/JitBench/archive/dev.zip once the fork is merged.
                var zipFile = Path.Combine(s_temporaryDirectory, "dev.zip");

                using (var tmpzip = File.Create(zipFile))
                {
                    using (var stream = client.GetStreamAsync(url).Result)
                        stream.CopyTo(tmpzip);
                    tmpzip.Flush();
                }

                // If the repo already exists, we delete it and extract it again.
                if (Directory.Exists(s_jitBenchDevDirectory))
                    Directory.Delete(s_jitBenchDevDirectory, true);

                // This step will create s_JitBenchDevDirectory.
                ZipFile.ExtractToDirectory(zipFile, s_temporaryDirectory);
            }

            // first dotnet-install.ps1
            var installProcessStartInfo = new ProcessStartInfo() {
                WorkingDirectory = s_jitBenchDevDirectory,
                FileName = @"powershell.exe",
                Arguments = @".\Dotnet-Install.ps1 -SharedRuntime -InstallDir .dotnet -Channel master -Architecture x64"
            };
            LaunchProcess(installProcessStartInfo, 180000);

            // second dotnet-install.ps1
            var secondInstallProcessStartInfo = new ProcessStartInfo() {
                WorkingDirectory = s_jitBenchDevDirectory,
                FileName = @"powershell.exe",
                Arguments = @".\Dotnet-Install.ps1 -InstallDir .dotnet -Channel master -Architecture x64"
            };
            LaunchProcess(secondInstallProcessStartInfo, 180000);

            // dotnet restore
            var musicStoreDirectory = Path.Combine(s_jitBenchDevDirectory, "src", "MusicStore");
            var restoreProcessStartInfo = new ProcessStartInfo() {
                WorkingDirectory = musicStoreDirectory,
                FileName = dotnetProcessFileName,
                Arguments = "restore"
            };
            LaunchProcess(restoreProcessStartInfo, 300000);

            // Modifying the shared framework
            var sourcePath = Directory.GetCurrentDirectory();
            var targetPath = Path.Combine(s_jitBenchDevDirectory, ".dotnet", "shared", "Microsoft.NETCore.App");
            var di = new DirectoryInfo(targetPath);
            targetPath = Path.Combine(targetPath, di.GetDirectories("2.0*")[0].Name);

            DirectoryInfo targetdi = new DirectoryInfo(targetPath);
            DirectoryInfo sourcedi = new DirectoryInfo(sourcePath);

            var excludedFiles = new List<string>()
            {
                "hostfxr",
                "hostpolicy",
                "System.Threading"
            };

            foreach (var file in targetdi.GetFiles("*.dll"))
            {
                var sourceFilePath = Path.Combine(sourcePath, file.Name);
                var targetFilePath = Path.Combine(targetPath, file.Name);
                var isFileInExcludedList = excludedFiles.Exists(excludedFile => excludedFile.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase));

                if (File.Exists(sourceFilePath) && !isFileInExcludedList)
                {
                    File.Copy(sourceFilePath, targetFilePath, true);
                    Console.WriteLine($"  Copied file '{sourceFilePath}' into '{targetFilePath}'");
                }
                else
                {
                    Console.WriteLine($"  Could not find file '{sourceFilePath}'");
                }
            }

            // dotnet publish -c Release -f netcoreapp20
            var publishProcessStartInfo = new ProcessStartInfo() {
                WorkingDirectory = musicStoreDirectory,
                FileName = dotnetProcessFileName,
                Arguments = "publish -c Release -f netcoreapp20"
            };
            LaunchProcess(publishProcessStartInfo, 300000);

            // Invoke-Crossgen
            var publishDirectory = Path.Combine(musicStoreDirectory, "bin", "Release", "netcoreapp20", "publish");
            var crossgenProcessStartInfo = new ProcessStartInfo() {
                WorkingDirectory = publishDirectory,
                FileName = "powershell.exe",
                Arguments = @".\Invoke-Crossgen -crossgen_path .crossgen -dotnet_dir " + dotnetDirectory
            };
            LaunchProcess(crossgenProcessStartInfo, 200000);

            return new ProcessStartInfo() {
                Arguments = @"MusicStore.dll",
                RedirectStandardOutput = true,
                FileName = dotnetProcessFileName,
                WorkingDirectory = publishDirectory
            };
        }

        private static void PostIteration()
        {
            using (StreamReader file = new StreamReader(File.OpenRead(Path.Combine(s_jitBenchDevDirectory, @"src\MusicStore\bin\Release\netcoreapp20\publish", "measures.txt"))))
            {
                var read = file.ReadLine().Split(' ');
                var startupTime = Convert.ToDouble(read[0]);
                var requestTime = Convert.ToDouble(read[1]);
                s_startupTimes[s_iteration] = startupTime;
                s_requestTimes[s_iteration] = requestTime;

                PrintRunningStepInformation($"JitBench Test: Iteration {s_iteration} took {startupTime}ms to start and {requestTime}ms to complete a single request.");
            }
            ++s_iteration;
        }

        private static ScenarioBenchmark PostProcessing()
        {
            PrintHeader("Starting POST");

            var scenarioBenchmark = new ScenarioBenchmark("MusicStore") {
                Namespace = "JitBench"
            };

            // Create (measured) test entries for this scenario.
            var startup = new ScenarioTestModel("Startup");
            scenarioBenchmark.Tests.Add(startup);

            var request = new ScenarioTestModel("Request Time");
            scenarioBenchmark.Tests.Add(request);

            // Add measured metrics to each test.
            startup.Performance.Metrics.Add(new MetricModel {
                Name = "ExecutionTime",
                DisplayName = "Execution Time",
                Unit = "ms"
            });
            request.Performance.Metrics.Add(new MetricModel {
                Name = "ExecutionTime",
                DisplayName = "Execution Time",
                Unit = "ms"
            });

            for (int i = 0; i < s_ScenarioConfiguration.Iterations; ++i)
            {
                var startupIteration = new IterationModel { Iteration = new Dictionary<string, double>() };
                startupIteration.Iteration.Add("ExecutionTime", s_startupTimes[i]);
                startup.Performance.IterationModels.Add(startupIteration);

                var requestIteration = new IterationModel { Iteration = new Dictionary<string, double>() };
                requestIteration.Iteration.Add("ExecutionTime", s_requestTimes[i]);
                request.Performance.IterationModels.Add(requestIteration);
            }

            return scenarioBenchmark;
        }

        private static void LaunchProcess(ProcessStartInfo processStartInfo, int timeoutMilliseconds)
        {
            PrintRunningStepInformation($"{processStartInfo.FileName} {processStartInfo.Arguments}");

            using (var p = new Process())
            {
                p.StartInfo = processStartInfo;
                p.Start();
                if (p.WaitForExit(timeoutMilliseconds) == false)
                {
                    p.Kill();
                    throw new TimeoutException($"The process '{processStartInfo.FileName} {processStartInfo.Arguments}' timed out.");
                }

                if (p.ExitCode != 0)
                    throw new Exception($"{processStartInfo.FileName} exited with error code {p.ExitCode}");
            }
        }

        private static void PrintHeader(string message)
        {
            Console.WriteLine();
            Console.WriteLine("**********************************************************************");
            Console.WriteLine($"** {message}");
            Console.WriteLine("**********************************************************************");
        }

        private static void PrintRunningStepInformation(string message)
        {
            Console.WriteLine($"-- {message}");
        }

        private static readonly ScenarioConfiguration s_ScenarioConfiguration;

        private static int s_iteration;
        private static double[] s_startupTimes;
        private static double[] s_requestTimes;
        private static string s_temporaryDirectory;
        private static string s_jitBenchDevDirectory;

        /// <summary>
        /// Provides an interface to parse the command line arguments passed to the JitBench harness.
        /// </summary>
        sealed class JitBenchHarnessOptions
        {
            public JitBenchHarnessOptions()
            {
                _tempDirectory = Directory.GetCurrentDirectory();
            }

            [Option('o', Required = false, HelpText = "Specifies the intermediate output directory name.")]
            public string IntermediateOutputDirectory
            {
                get { return _tempDirectory; }

                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new InvalidOperationException("The intermediate output directory name cannot be null, empty or white space.");

                    if (value.Any(c => Path.GetInvalidPathChars().Contains(c)))
                        throw new InvalidOperationException("Specified intermediate output directory name contains invalid path characters.");

                    _tempDirectory = Path.IsPathRooted(value) ? value : Path.GetFullPath(value);
                    Directory.CreateDirectory(_tempDirectory);
                }
            }

            public static JitBenchHarnessOptions Parse(string[] args)
            {
                using (var parser = new Parser((settings) => {
                    settings.CaseInsensitiveEnumValues = true;
                    settings.CaseSensitive = false;
                    settings.HelpWriter = new StringWriter();
                    settings.IgnoreUnknownArguments = true;
                }))
                {
                    JitBenchHarnessOptions options = null;
                    var parserResult = parser.ParseArguments<JitBenchHarnessOptions>(args)
                        .WithParsed(parsed => options = parsed)
                        .WithNotParsed(errors => {
                            foreach (var error in errors)
                            {
                                switch (error.Tag)
                                {
                                    case ErrorType.MissingValueOptionError:
                                        throw new ArgumentException(
                                            $"Missing value option for command line argument '{(error as MissingValueOptionError).NameInfo.NameText}'");
                                    case ErrorType.HelpRequestedError:
                                        Console.WriteLine(Usage());
                                        Environment.Exit(0);
                                        break;
                                    case ErrorType.VersionRequestedError:
                                        Console.WriteLine(new AssemblyName(typeof(JitBenchHarnessOptions).GetTypeInfo().Assembly.FullName).Version);
                                        Environment.Exit(0);
                                        break;
                                    case ErrorType.BadFormatTokenError:
                                    case ErrorType.UnknownOptionError:
                                    case ErrorType.MissingRequiredOptionError:
                                    case ErrorType.MutuallyExclusiveSetError:
                                    case ErrorType.BadFormatConversionError:
                                    case ErrorType.SequenceOutOfRangeError:
                                    case ErrorType.RepeatedOptionError:
                                    case ErrorType.NoVerbSelectedError:
                                    case ErrorType.BadVerbSelectedError:
                                    case ErrorType.HelpVerbRequestedError:
                                        break;
                                }
                            }
                        });
                    return options;
                }
            }

            public static string Usage()
            {
                var parser = new Parser((parserSettings) => {
                    parserSettings.CaseInsensitiveEnumValues = true;
                    parserSettings.CaseSensitive = false;
                    parserSettings.EnableDashDash = true;
                    parserSettings.HelpWriter = new StringWriter();
                    parserSettings.IgnoreUnknownArguments = true;
                });
                var result = parser.ParseArguments<JitBenchHarnessOptions>(new string[] { "--help" });

                var helpTextString = new HelpText {
                    AddDashesToOption = true,
                    AddEnumValuesToHelpText = true,
                    AdditionalNewLineAfterOption = false,
                    Copyright = "Copyright (c) Microsoft Corporation 2015",
                    Heading = "JitBenchHarness",
                    MaximumDisplayWidth = 80,
                }.AddOptions(result).ToString();
                return helpTextString;
            }

            private string _tempDirectory;
        }
    }
}
