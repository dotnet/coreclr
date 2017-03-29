using Microsoft.Xunit.Performance;
using Microsoft.Xunit.Performance.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Xml;
using Xunit;


[assembly: MeasureGCAllocations]
[assembly: MeasureGCCounts]
[assembly: MeasureInstructionsRetired]

namespace jitBench
{
    public class Program
    {
        public static int iteration;
        public static double[] startupTimes;
        public static double[] requestTimes;
        
        public static void Main(string[] args)
        {
            using (var h = new XunitPerformanceHarness(args))
            {
                var processStartInfo = new ProcessStartInfo();
                h._performanceTestConfig.Iterations = 10;
                Setup(h._performanceTestConfig, processStartInfo);
                processStartInfo.Arguments = @"MusicStore.dll";
                processStartInfo.RedirectStandardOutput = true;

                h.RunScenario(processStartInfo, preIteration, postIteration, postProcessing);
            }
        }

        public static void Setup(PerformanceTestConfig config, ProcessStartInfo processStartInfo) 
        {
            Console.WriteLine("vvvvvvvvvvvvvvvvvv");
            Console.WriteLine("Starting SETUP");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^");
            Console.WriteLine("Iterations = " + config.Iterations);
            Console.WriteLine("Timeout per iteration = " + config.TimeoutPerIteration.TotalSeconds + "s");

            //Download and extract the repo.
            string TemporaryDirectory = config.TemporaryDirectory;
            string copiedRepoPath = Path.Combine(TemporaryDirectory, @"JitBench-dev");

            string dotnetDir = String.Format("{0}\\JitBench-dev\\.dotnet", TemporaryDirectory);
            string dotnet = String.Format("{0}\\JitBench-dev\\.dotnet\\dotnet.exe", TemporaryDirectory);
            processStartInfo.FileName = dotnet;

            using(var client = new HttpClient() )
            {
                // Download the JitBench repository and extract it.
                var url = @"https://github.com/guhuro/JitBench/archive/dev.zip"; // @"https://github.com/aspnet/JitBench/archive/dev.zip";
                var stream = client.GetStreamAsync(url).Result;
                string zipFile = Path.Combine(TemporaryDirectory, "dev.zip");
                var tmpzip = File.Create(zipFile);
                stream.CopyTo(tmpzip);
                tmpzip.Flush();
                tmpzip.Dispose();
                if(System.IO.Directory.Exists(copiedRepoPath))  // If the repo already exists, we delete it and extract it again.
                {
                    try
                    {
                        System.IO.Directory.Delete(copiedRepoPath, true);
                    }

                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                ZipFile.ExtractToDirectory(zipFile, TemporaryDirectory);
            }

            // first dotnet-install.ps1
            string workingDir = copiedRepoPath;
            ProcessStartInfo installProcessStartInfo = new ProcessStartInfo();
            installProcessStartInfo.WorkingDirectory = workingDir;
            installProcessStartInfo.FileName = @"powershell.exe";
            installProcessStartInfo.Arguments = @".\Dotnet-Install.ps1 -SharedRuntime -InstallDir .dotnet -Channel master -Architecture x64";
            launchProcess(installProcessStartInfo, 180000);

            // second dotnet-install.ps1
            ProcessStartInfo secondInstallProcessStartInfo = new ProcessStartInfo();
            secondInstallProcessStartInfo.WorkingDirectory = workingDir;
            secondInstallProcessStartInfo.FileName = @"powershell.exe";
            secondInstallProcessStartInfo.Arguments = @".\Dotnet-Install.ps1 -InstallDir .dotnet -Channel master -Architecture x64";
            launchProcess(secondInstallProcessStartInfo, 180000);

            // dotnet restore
            workingDir = Path.Combine(TemporaryDirectory, @"JitBench-dev\src\MusicStore");
            ProcessStartInfo restoreProcessStartInfo = new ProcessStartInfo();
            restoreProcessStartInfo.WorkingDirectory = workingDir;
            restoreProcessStartInfo.FileName = dotnet;
            restoreProcessStartInfo.Arguments = "restore";
            launchProcess(restoreProcessStartInfo, 300000);

            // Modifying the shared framework
            string sourcePath = Directory.GetCurrentDirectory();
            string targetPath =  Path.Combine(copiedRepoPath, @".dotnet\shared\Microsoft.NETCore.App");
            DirectoryInfo di = new DirectoryInfo(targetPath);
            targetPath = Path.Combine(targetPath, di.GetDirectories("2.0*")[0].Name);

            DirectoryInfo targetdi = new DirectoryInfo(targetPath);
            DirectoryInfo sourcedi = new DirectoryInfo(sourcePath);

            foreach (var file in targetdi.GetFiles("*.dll")) 
            {
                string sourceFilePath = Path.Combine(sourcePath, file.Name);
                string targetFilePath = Path.Combine(targetPath, file.Name);
                if (File.Exists(sourceFilePath) && (!sourceFilePath.ToLower().Contains("hostfxr") && !sourceFilePath.ToLower().Contains("hostpolicy") && !sourceFilePath.ToLower().Contains("system.threading")))
                {
                    File.Copy(sourceFilePath, targetFilePath, true);
                    Console.WriteLine("copied file " + sourceFilePath + " into " + targetFilePath);
                } 
                else 
                {
                    Console.WriteLine("Couldn't find file " + sourceFilePath);
                }
            }

            // dotnet publish -c Release -f netcoreapp20
            ProcessStartInfo publishProcessStartInfo = new ProcessStartInfo();
            publishProcessStartInfo.WorkingDirectory = workingDir;
            publishProcessStartInfo.FileName = dotnet;
            publishProcessStartInfo.Arguments = "publish -c Release -f netcoreapp20";
            launchProcess(publishProcessStartInfo, 300000);

            // Invoke-Crossgen
            ProcessStartInfo crossgenProcessStartInfo = new ProcessStartInfo();
            workingDir = Path.Combine(workingDir, @"bin\Release\netcoreapp20\publish");
            crossgenProcessStartInfo.WorkingDirectory = workingDir;
            crossgenProcessStartInfo.FileName = "powershell.exe";
            crossgenProcessStartInfo.Arguments = @".\Invoke-Crossgen -crossgen_path .crossgen -dotnet_dir "+dotnetDir;
            launchProcess(crossgenProcessStartInfo, 200000);

            processStartInfo.WorkingDirectory = workingDir;

            // Set variables we will need to store results.
            iteration = 0;
            startupTimes = new double[config.Iterations];
            requestTimes = new double[config.Iterations];
        }

        public static void preIteration(PerformanceTestConfig config){}

        public static void postIteration(PerformanceTestConfig config)
        {
            using (StreamReader file = new StreamReader(File.OpenRead(Path.Combine(config.TemporaryDirectory, @"JitBench-dev\src\MusicStore\bin\Release\netcoreapp20\publish", "measures.txt"))))
            {
                string[] read = file.ReadLine().Split(' ');
                Double startupTime = Convert.ToDouble(read[0]);
                Double requestTime = Convert.ToDouble(read[1]);
                startupTimes[iteration] = startupTime;
                requestTimes[iteration] = requestTime;
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("JitBench Test: Iteration " + iteration + " took " + startupTime + "ms to start and " + requestTime + "ms to complete a single request.");
                Console.WriteLine("----------------------------------------");
            }
            ++iteration;
        }

        public static ScenarioBenchmark postProcessing(PerformanceTestConfig config)
        {
            Console.WriteLine("vvvvvvvvvvvvvvvvvv");
            Console.WriteLine("Starting POST");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^");

            string copiedRepoPath = Path.Combine(config.TemporaryDirectory, @"JitBench-dev");
                if(System.IO.Directory.Exists(copiedRepoPath))
                {
                    try
                    {
                        System.IO.Directory.Delete(copiedRepoPath, true);
                    }

                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

            ScenarioBenchmark scenarioBenchmark = new ScenarioBenchmark("MusicStore");
            scenarioBenchmark.Namespace = "JitBench";
            ScenarioTestModel startup = new ScenarioTestModel("Startup");
            ScenarioTestModel request = new ScenarioTestModel("RequestTime");
            startup.Namespace = "JitBench.MusicStore";
            request.Namespace = "JitBench.MusicStore";
            MetricModel startupExecutionTime = new MetricModel(){Name = "ExecutionTime", DisplayName = "Execution Time", Unit = "ms"};
            MetricModel requestExecutionTime = new MetricModel(){Name = "ExecutionTime", DisplayName = "Execution Time", Unit = "ms"};

            for (int i = 0; i < config.Iterations; i++)
            {
                var startupIteration = new IterationModel { Iteration = new Dictionary<string, double>() };
                startupIteration.Iteration.Add("ExecutionTime", startupTimes[i]);
                startup.Performance.IterationModels.Add(startupIteration);

                var requestIteration = new IterationModel { Iteration = new Dictionary<string, double>() };
                requestIteration.Iteration.Add("ExecutionTime", requestTimes[i]);
                request.Performance.IterationModels.Add(requestIteration);
            }

            startup.Performance.Metrics.Add(startupExecutionTime);
            request.Performance.Metrics.Add(requestExecutionTime);
            scenarioBenchmark.Tests.Add(startup);
            scenarioBenchmark.Tests.Add(request);

            return scenarioBenchmark;
        }

        public static void launchProcess(ProcessStartInfo processStartInfo, int timeout)
        {
            using(var p = new Process())
            {
                p.StartInfo = processStartInfo;
                p.Start();
                if (p.WaitForExit(timeout) == false) 
                {
                    if (p != null)
                    {
                        p.Kill();
                    }
                    Console.Error.WriteLine("The process " + processStartInfo.FileName + " " + processStartInfo.Arguments +  "Timeouted.");
                    return;
                }
            }
        }
    }
}