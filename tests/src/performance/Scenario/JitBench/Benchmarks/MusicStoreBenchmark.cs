﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xunit.Performance.Api;

namespace JitBench
{
    class MusicStoreBenchmark : Benchmark
    {
        public MusicStoreBenchmark() : base("MusicStore")
        {
        }

        public override async Task Setup(DotNetInstallation dotNetInstall, string outputDir, bool useExistingSetup, ITestOutputHelper output)
        {
            if(!useExistingSetup)
            {
                using (var setupSection = new IndentedTestOutputHelper("Setup " + Name, output))
                {
                    await DownloadAndExtractJitBenchRepo(outputDir, setupSection);
                    await CreateStore(dotNetInstall, outputDir, setupSection);
                    await Publish(dotNetInstall, outputDir, setupSection);
                }
            }
            string musicStoreSrcDirectory = GetMusicStoreSrcDirectory(outputDir);
            string tfm = DotNetSetup.GetTargetFrameworkMonikerForFrameworkVersion(dotNetInstall.FrameworkVersion);
            ExePath = "MusicStore.dll";
            WorkingDirPath = GetMusicStorePublishDirectory(outputDir, tfm);
            EnvironmentVariables.Add("DOTNET_SHARED_STORE", GetMusicStoreStoreDir(outputDir));
        }

        async Task DownloadAndExtractJitBenchRepo(string outputDir, ITestOutputHelper output)
        {
            // If the repo already exists, we delete it and extract it again.
            string jitBenchRepoRootDir = GetJitBenchRepoRootDir(outputDir);
            FileTasks.DeleteDirectory(jitBenchRepoRootDir, output);

            string localJitBenchRepo = GetLocalJitBenchRepoDirectory();
            if (localJitBenchRepo == null)
            {
                var url = $"{JitBenchRepoUrl}/archive/{JitBenchCommitSha1Id}.zip";
                FileTasks.DeleteDirectory(jitBenchRepoRootDir + "_temp", output);
                await FileTasks.DownloadAndUnzip(url, jitBenchRepoRootDir+"_temp", output);
                FileTasks.MoveDirectory(Path.Combine(jitBenchRepoRootDir + "_temp", $"JitBench-{JitBenchCommitSha1Id}"), jitBenchRepoRootDir, output);
            }
            else
            {
                if (!Directory.Exists(localJitBenchRepo))
                {
                    throw new Exception("Local JitBench repo " + localJitBenchRepo + " does not exist");
                }
                FileTasks.DirectoryCopy(localJitBenchRepo, jitBenchRepoRootDir, output);
            }
        }

        private static async Task CreateStore(DotNetInstallation dotNetInstall, string outputDir, ITestOutputHelper output)
        {
            string tfm = DotNetSetup.GetTargetFrameworkMonikerForFrameworkVersion(dotNetInstall.FrameworkVersion);
            string rid = $"win7-{dotNetInstall.Architecture}";
            string storeDirName = ".store";
            await new ProcessRunner("powershell.exe", $".\\AspNet-GenerateStore.ps1 -InstallDir {storeDirName} -Architecture {dotNetInstall.Architecture} -Runtime {rid}")
                .WithWorkingDirectory(GetJitBenchRepoRootDir(outputDir))
                .WithEnvironmentVariable("PATH", $"{dotNetInstall.DotNetDir};{Environment.GetEnvironmentVariable("PATH")}")
                .WithEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0")
                .WithEnvironmentVariable("JITBENCH_TARGET_FRAMEWORK_MONIKER", tfm)
                .WithEnvironmentVariable("JITBENCH_FRAMEWORK_VERSION", dotNetInstall.FrameworkVersion)
                .WithLog(output)
                .Run();
        }

        private static async Task<string> Publish(DotNetInstallation dotNetInstall, string outputDir, ITestOutputHelper output)
        {
            string tfm = DotNetSetup.GetTargetFrameworkMonikerForFrameworkVersion(dotNetInstall.FrameworkVersion);
            string publishDir = GetMusicStorePublishDirectory(outputDir, tfm);
            string manifestPath = Path.Combine(GetMusicStoreStoreDir(outputDir), dotNetInstall.Architecture, tfm, "artifact.xml");
            FileTasks.DeleteDirectory(publishDir, output);
            string dotNetExePath = dotNetInstall.DotNetExe;
            await new ProcessRunner(dotNetExePath, $"publish -c Release -f {tfm} --manifest {manifestPath}")
                .WithWorkingDirectory(GetMusicStoreSrcDirectory(outputDir))
                .WithEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0")
                .WithEnvironmentVariable("JITBENCH_ASPNET_VERSION", "2.0")
                .WithEnvironmentVariable("JITBENCH_TARGET_FRAMEWORK_MONIKER", tfm)
                .WithEnvironmentVariable("JITBENCH_TARGET_FRAMEWORK_VERSION", dotNetInstall.FrameworkVersion)
                .WithEnvironmentVariable("UseSharedCompilation", "false")
                .WithLog(output)
                .Run();
            return publishDir;
        }

        public override Metric[] GetDefaultDisplayMetrics()
        {
            return new Metric[]
            {
                StartupMetric,
                FirstRequestMetric,
                MedianResponseMetric
            };
        }

        protected override IterationResult RecordIterationMetrics(ScenarioExecutionResult scenarioIteration, string stdout, string stderr, ITestOutputHelper output)
        {
            IterationResult result = base.RecordIterationMetrics(scenarioIteration, stdout, stderr, output);
            AddConsoleMetrics(result, stdout, output);
            return result;
        }

        void AddConsoleMetrics(IterationResult result, string stdout, ITestOutputHelper output)
        {
            output.WriteLine("Processing iteration results.");

            double? startupTime = null;
            double? firstRequestTime = null;
            double? steadyStateMedianTime = null;

            using (var reader = new StringReader(stdout))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = Regex.Match(line, @"^Server start \(ms\): \s*(\d+)\s*$");
                    if (match.Success && match.Groups.Count == 2)
                    {
                        startupTime = Convert.ToDouble(match.Groups[1].Value);
                        continue;
                    }

                    match = Regex.Match(line, @"^1st Request \(ms\): \s*(\d+)\s*$");
                    if (match.Success && match.Groups.Count == 2)
                    {
                        firstRequestTime = Convert.ToDouble(match.Groups[1].Value);
                        continue;
                    }

                    //the steady state output chart looks like:
                    //   Requests    Aggregate Time(ms)    Req/s   Req Min(ms)   Req Mean(ms)   Req Median(ms)   Req Max(ms)   SEM(%)
                    // ----------    ------------------    -----   -----------   ------------   --------------   -----------   ------
                    //    2-  100                 5729   252.60          3.01           3.96             3.79          9.81     1.86
                    //  101-  250                 6321   253.76          3.40           3.94             3.84          5.25     0.85
                    //  ... many more rows ...

                    //                              Requests       Agg     req/s        min          mean           median         max          SEM
                    match = Regex.Match(line, @"^\s*\d+-\s*\d+ \s* \d+ \s* \d+\.\d+ \s* \d+\.\d+ \s* (\d+\.\d+) \s* (\d+\.\d+) \s* \d+\.\d+ \s* \d+\.\d+$");
                    if (match.Success && match.Groups.Count == 3)
                    {
                        //many lines will match, but the final values of these variables will be from the last batch which is presumably the
                        //best measurement of steady state performance
                        steadyStateMedianTime = Convert.ToDouble(match.Groups[2].Value);
                        continue;
                    }
                }
            }

            if (!startupTime.HasValue)
                throw new FormatException("Startup time was not found.");
            if (!firstRequestTime.HasValue)
                throw new FormatException("First Request time was not found.");
            if (!steadyStateMedianTime.HasValue)
                throw new FormatException("Steady state median response time not found.");
                

            result.Measurements.Add(StartupMetric, startupTime.Value);
            result.Measurements.Add(FirstRequestMetric, firstRequestTime.Value);
            result.Measurements.Add(MedianResponseMetric, steadyStateMedianTime.Value);

            output.WriteLine($"Server started in {startupTime}ms");
            output.WriteLine($"Request took {firstRequestTime}ms");
            output.WriteLine($"Median steady state response {steadyStateMedianTime.Value}ms");
        }

        /// <summary>
        /// When serializing the result data to benchview this is called to determine if any of the metrics should be reported differently
        /// than they were collected. MusicStore uses this to collect several measurements in each iteration, then present those measurements
        /// to benchview as if each was the Duration metric of a distinct scenario test with its own set of iterations.
        /// </summary>
        public override bool TryGetBenchviewCustomMetricReporting(Metric originalMetric, out Metric newMetric, out string newScenarioModelName)
        {
            if(originalMetric.Equals(StartupMetric))
            {
                newScenarioModelName = "Startup";
            }
            else if (originalMetric.Equals(FirstRequestMetric))
            {
                newScenarioModelName = "First Request";
            }
            else if (originalMetric.Equals(MedianResponseMetric))
            {
                newScenarioModelName = "Median Response";
            }
            else
            {
                return base.TryGetBenchviewCustomMetricReporting(originalMetric, out newMetric, out newScenarioModelName);
            }
            newMetric = Metric.ElapsedTimeMilliseconds;
            return true;
        }

        static string GetJitBenchRepoRootDir(string outputDir)
        {
            return Path.Combine(outputDir, "J"); 
        }

        static string GetMusicStoreSrcDirectory(string outputDir)
        {
            return Path.Combine(GetJitBenchRepoRootDir(outputDir), "src", "MusicStore");
        }

        static string GetMusicStorePublishDirectory(string outputDir, string tfm)
        {
            return Path.Combine(GetMusicStoreSrcDirectory(outputDir), "bin", "Release", tfm, "publish");
        }

        static string GetMusicStoreStoreDir(string outputDir)
        {
            return Path.Combine(GetJitBenchRepoRootDir(outputDir), StoreDirName);
        }

        string GetLocalJitBenchRepoDirectory()
        {
            return Environment.GetEnvironmentVariable("MUSICSTORE_PRIVATE_REPO");
        }

        private const string JitBenchRepoUrl = "https://github.com/aspnet/JitBench";
        private const string JitBenchCommitSha1Id = "6e1327b633e2d7d45f4c13f498fc27698ea5735a";
        private const string EnvironmentFileName = "JitBenchEnvironment.txt";
        private const string StoreDirName = ".store";
        private readonly Metric StartupMetric = new Metric("Startup", "ms");
        private readonly Metric FirstRequestMetric = new Metric("First Request", "ms");
        private readonly Metric MedianResponseMetric = new Metric("Median Response", "ms");
        private readonly Metric MeanResponseMetric = new Metric("Mean Response", "ms");
    }
}
