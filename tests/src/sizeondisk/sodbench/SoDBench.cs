using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Microsoft.Xunit.Performance.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SoDBench
{
    class Program
    {
        public static readonly string NugetConfig =
        @"<?xml version='1.0' encoding='utf-8'?>
        <configuration>
        <packageSources>
            <add key='nuget.org' value='https://api.nuget.org/v3/index.json' protocolVersion='3' />
            <add key='myget.org/dotnet-core' value='https://dotnet.myget.org/F/dotnet-core/api/v3/index.json' protocolVersion='3' />
            <add key='myget.org/aspnet-core' value='https://dotnet.myget.org/F/aspnetcore-ci-dev/api/v3/index.json' protocolVersion='3' />
        </packageSources>
        </configuration>";

        public static readonly string[] NewTemplates = new string[] {
            "console",
            "classlib",
            "mstest",
            "xunit",
            "web",
            "mvc",
            "razor",
            "webapi",
            "nugetconfig",
            "webconfig",
            "sln",
            "page",
            "viewimports",
            "viewstart"
        };

        public static readonly string[] OperatingSystems = new string[] {
            "win10-x64",
            "win10-x86",
            "ubuntu.16.10-x64",
            "rhel.7-x64"
        };

        static FileInfo s_dotnetExe;
        static DirectoryInfo s_sandboxDir;
        static DirectoryInfo s_fallbackDir;
        static DirectoryInfo s_corelibsDir;
        static bool s_keepArtifacts;
        static string s_targetArchitecture;
        static string s_dotnetChannel;
        static Dictionary<string, long> s_getDirSizeCache = new Dictionary<string, long>();

        static void Main(string[] args)
        {
            try
            {
                var options = SoDBenchOptions.Parse(args);

                s_targetArchitecture = options.TargetArchitecture;
                s_dotnetChannel = options.DotnetChannel;
                s_keepArtifacts = options.KeepArtifacts;

                if (!String.IsNullOrWhiteSpace(options.DotnetExecutable))
                {
                    s_dotnetExe = new FileInfo(options.DotnetExecutable);
                }

                if (s_sandboxDir == null)
                {
                    // Truncate the Guid used for anti-collision because a full Guid results in expanded paths over 260 chars (the Windows max)
                    s_sandboxDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"sod{Guid.NewGuid().ToString().Substring(0,13)}"));
                    s_sandboxDir.Create();
                    Console.WriteLine($"** Running inside sandbox directory: {s_sandboxDir}");
                }

                if (s_dotnetExe == null)
                {
                    if(!String.IsNullOrEmpty(options.CoreLibariesDirectory))
                    {
                        Console.WriteLine($"** Using core libraries found at {options.CoreLibariesDirectory}");
                        s_corelibsDir = new DirectoryInfo(options.CoreLibariesDirectory);
                    }
                    else 
                    {
                        var coreroot = Environment.GetEnvironmentVariable("CORE_ROOT");
                        if (!String.IsNullOrEmpty(coreroot) && Directory.Exists(coreroot))
                        {
                            Console.WriteLine($"** Using core libraries from CORE_ROOT at {coreroot}");
                            s_corelibsDir = new DirectoryInfo(coreroot);
                        }
                        else
                        {
                            Console.WriteLine("** Using default dotnet-cli core libraries");
                        }
                    }

                    PrintHeader("Installing Dotnet CLI");
                    s_dotnetExe = SetupDotnet();
                }

                if (s_fallbackDir == null)
                {
                    s_fallbackDir = new DirectoryInfo(Path.Combine(s_sandboxDir.FullName, "dotnet-fallback"));
                    s_fallbackDir.Create();
                }

                Console.WriteLine($"** Path to dotnet executable: {s_dotnetExe.FullName}");
                
                PrintHeader("Starting acquisition size test");
                var acquisitionSizes = GetAcquisitionSize();

                PrintHeader("Running deployment size test");
                var deploymentSizes = GetDeploymentSize();

                var formattedStr = FormatAsCsv(acquisitionSizes, deploymentSizes);
               
                File.WriteAllText(options.OutputFilename, formattedStr);

                if (options.Verbose)
                    Console.WriteLine($"** CSV Output:\n{formattedStr}");
            }
            finally
            {
                if (!s_keepArtifacts && s_sandboxDir != null)
                {
                    PrintHeader("Cleaning up sandbox directory");
                    s_sandboxDir.Delete(true);
                    s_sandboxDir = null;
                }
            }
        }

        private static void PrintHeader(string message)
        {
            Console.WriteLine();
            Console.WriteLine("**********************************************************************");
            Console.WriteLine($"** {message}");
            Console.WriteLine("**********************************************************************");
        }

        private static Dictionary<string, long?> GetAcquisitionSize()
        {
            var result = new Dictionary<string, long?>();

            // Arbitrary command to trigger first time setup
            ProcessStartInfo dotnet = new ProcessStartInfo()
            {
                WorkingDirectory = s_sandboxDir.FullName,
                FileName = s_dotnetExe.FullName,
                Arguments = "new"
            };

            // Used to set where the packages will be unpacked to.
            // There is a no gaurentee that this is a stable method, but is the only way currently to set the fallback folder location
            dotnet.Environment["DOTNET_CLI_TEST_FALLBACKFOLDER"] = s_fallbackDir.FullName;

            LaunchProcess(dotnet, 180000);

            Console.WriteLine("\n** Measuring total size of acquired files");

            long fallbackDirSize = GetDirectorySize(s_fallbackDir);
            result["fallback"] = fallbackDirSize;

            foreach (var dir in s_dotnetExe.Directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                result[GetRelativeUri(dir, s_dotnetExe.Directory)] = GetDirectorySize(dir);
            }

            foreach (var file in s_dotnetExe.Directory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                result[GetRelativeUri(file, s_dotnetExe.Directory)] = file.Length;
            }

            return result;
        }
        
        private static Dictionary<string, Dictionary<string, long?> > GetDeploymentSize()
        {
            var nugetConfFile = new FileInfo(Path.Combine(s_sandboxDir.FullName, "NuGet.Config"));
            File.WriteAllText(nugetConfFile.FullName, NugetConfig);

            var result = new Dictionary<string, Dictionary<string, long?> >();
            foreach (string template in NewTemplates)
            {
                result[template] = new Dictionary<string, long?>();
                foreach (var os in OperatingSystems)
                {
                    result[template][os] = null;

                    Console.WriteLine($"\n\n** Deploying {template}/{os}");

                    var deploymentSandbox = new DirectoryInfo(Path.Combine(s_sandboxDir.FullName, template, os));
                    var publishDir = new DirectoryInfo(Path.Combine(deploymentSandbox.FullName, "publish"));
                    deploymentSandbox.Create();

                    ProcessStartInfo dotnetNew = new ProcessStartInfo()
                    {
                        FileName = s_dotnetExe.FullName,
                        Arguments = $"new {template}",
                        UseShellExecute = false,
                        WorkingDirectory = deploymentSandbox.FullName
                    };
                    dotnetNew.Environment["DOTNET_CLI_TEST_FALLBACKFOLDER"] = s_fallbackDir.FullName;

                    ProcessStartInfo dotnetRestore = new ProcessStartInfo()
                    {
                        FileName = s_dotnetExe.FullName,
                        Arguments = $"restore --runtime {os}",
                        UseShellExecute = false,
                        WorkingDirectory = deploymentSandbox.FullName
                    };
                    dotnetRestore.Environment["DOTNET_CLI_TEST_FALLBACKFOLDER"] = s_fallbackDir.FullName;

                    ProcessStartInfo dotnetPublish = new ProcessStartInfo()
                    {
                        FileName = s_dotnetExe.FullName,
                        Arguments = $"publish --runtime {os} --output {publishDir.FullName}", // "out" is an arbitrary project name
                        UseShellExecute = false,
                        WorkingDirectory = deploymentSandbox.FullName
                    };
                    dotnetPublish.Environment["DOTNET_CLI_TEST_FALLBACKFOLDER"] = s_fallbackDir.FullName;

                    try
                    {
                        LaunchProcess(dotnetNew, 180000);
                        if (deploymentSandbox.EnumerateFiles().Any(f => f.Name.EndsWith("proj")))
                        {
                            LaunchProcess(dotnetRestore, 180000);
                            LaunchProcess(dotnetPublish, 180000);
                        }
                        else
                        {
                            Console.WriteLine($"** {template} does not have a project file to restore or publish");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        continue;
                    }

                    long output = 0;
                    if (publishDir.Exists)
                    {
                        output = GetDirectorySize(publishDir);
                    }
                    else
                    {
                        output = GetDirectorySize(deploymentSandbox);
                    }
                    result[template][os] = output;
                }
            }
            return result;
        }

        private static void DownloadDotnetInstaller()
        {
            var psi = new ProcessStartInfo() {
                WorkingDirectory = s_sandboxDir.FullName,
                FileName = @"powershell.exe",
                Arguments = $"wget https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.ps1 -OutFile Dotnet-Install.ps1"
            };
            LaunchProcess(psi, 180000);
        }

        private static void InstallSharedRuntime()
        {
            var psi = new ProcessStartInfo() {
                WorkingDirectory = s_sandboxDir.FullName,
                FileName = @"powershell.exe",
                Arguments = $".\\Dotnet-Install.ps1 -SharedRuntime -InstallDir .dotnet -Channel {s_dotnetChannel} -Architecture {s_targetArchitecture}"
            };
            LaunchProcess(psi, 180000);
        }

        private static void InstallDotnet()
        {
            var psi = new ProcessStartInfo() {
                WorkingDirectory = s_sandboxDir.FullName,
                FileName = @"powershell.exe",
                Arguments = $".\\Dotnet-Install.ps1 -InstallDir .dotnet -Channel {s_dotnetChannel} -Architecture {s_targetArchitecture}"
            };
            LaunchProcess(psi, 180000);
        }

        private static void ModifySharedFramework()
        {
            // Current working directory is the <coreclr repo root>/sandbox directory.
            Console.WriteLine($"** Modifying the shared framework.");

            var sourcedi = s_corelibsDir;

            // Get the directory containing the newest version of Microsodt.NETCore.App libraries
            var targetdi = new DirectoryInfo(
                new DirectoryInfo(Path.Combine(s_sandboxDir.FullName, ".dotnet", "shared", "Microsoft.NETCore.App"))
                .GetDirectories("*")
                .OrderBy(s => s.Name)
                .Last()
                .FullName);

            Console.WriteLine($"| Source : {sourcedi.FullName}");
            Console.WriteLine($"| Target : {targetdi.FullName}");

            var compiledBinariesOfInterest = new string[] {
                "clretwrc.dll",
                "clrjit.dll",
                "coreclr.dll",
                "mscordaccore.dll",
                "mscordbi.dll",
                "mscorrc.debug.dll",
                "mscorrc.dll",
                "sos.dll",
                "SOS.NETCore.dll",
                "System.Private.CoreLib.dll"
            };

            foreach (var compiledBinaryOfInterest in compiledBinariesOfInterest)
            {
                foreach (FileInfo fi in targetdi.GetFiles(compiledBinaryOfInterest))
                {
                    var sourceFilePath = Path.Combine(sourcedi.FullName, fi.Name);
                    var targetFilePath = Path.Combine(targetdi.FullName, fi.Name);

                    if (File.Exists(sourceFilePath))
                    {
                        File.Copy(sourceFilePath, targetFilePath, true);
                        Console.WriteLine($"|   Copied file - '{fi.Name}'");
                    }
                }
            }
        }

        private static FileInfo SetupDotnet()
        {
            DownloadDotnetInstaller();
            InstallSharedRuntime();
            InstallDotnet();
            if (s_corelibsDir != null)
            {
                ModifySharedFramework();
            }

            var dotnetExe = new FileInfo(Path.Combine(s_sandboxDir.FullName, ".dotnet", "dotnet.exe"));
            Debug.Assert(dotnetExe.Exists);

            return dotnetExe;
        }

        private static void LaunchProcess(ProcessStartInfo processStartInfo, int timeoutMilliseconds, IDictionary<string, string> environment = null)
        {
            Console.WriteLine();
            Console.WriteLine($"{System.Security.Principal.WindowsIdentity.GetCurrent().Name}@{Environment.MachineName} \"{processStartInfo.WorkingDirectory}\"");
            Console.WriteLine($"[{DateTime.Now}] $ {processStartInfo.FileName} {processStartInfo.Arguments}");

            if (environment != null)
            {
                foreach (KeyValuePair<string, string> pair in environment)
                {
                    if (!processStartInfo.Environment.ContainsKey(pair.Key))
                        processStartInfo.Environment.Add(pair.Key, pair.Value);
                    else
                        processStartInfo.Environment[pair.Key] = pair.Value;
                }
            }

            using (var p = new Process() { StartInfo = processStartInfo })
            {
                p.Start();
                if (p.WaitForExit(timeoutMilliseconds) == false)
                {
                    // FIXME: What about clean/kill child processes?
                    p.Kill();
                    throw new TimeoutException($"The process '{processStartInfo.FileName} {processStartInfo.Arguments}' timed out.");
                }

                if (p.ExitCode != 0)
                    throw new Exception($"{processStartInfo.FileName} exited with error code {p.ExitCode}");
            }
        }

        public static string FormatAsCsv(Dictionary<string, long?> acquisitionSizes, Dictionary<string, Dictionary<string, long?> > deploymentSizes)
        {
            var toplevelname = "Size on Disk";
            var result = new StringBuilder();

            foreach (var item in acquisitionSizes)
            {
                var namespaces = new string[] {toplevelname, "Acquisition Size"};
                var data = namespaces.Concat(Csv.Escape(item.Key).Split('/')).Concat( new string[] {Convert.ToString(item.Value)} );
                var line = String.Join(",", data);
                result.AppendLine(line);
            }

            foreach (var dict in deploymentSizes)
            {
                foreach (var item in dict.Value)
                {
                    var data = new string[] {toplevelname, "Deployment Size", dict.Key, item.Key, Convert.ToString(item.Value)};
                    var line = String.Join(",", data);
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        // A memoized recursive method for finding folder size
        private static long GetDirectorySize(DirectoryInfo dir, bool useCache = true)
        {
            long result = 0;

            if (!useCache || !s_getDirSizeCache.TryGetValue(dir.FullName, out result))
            {
                result = 0;

                if (dir.Exists)
                {
                    foreach (var subdir in dir.EnumerateDirectories())
                    {
                        result += GetDirectorySize(subdir);
                    }

                    foreach (var file in dir.EnumerateFiles())
                    {
                        result += file.Length;
                    }
                }

                s_getDirSizeCache[dir.FullName] = result;
            }

            return result;
        }

        private static string GetRelativeUri(FileSystemInfo dir, FileSystemInfo root)
        {
            var dirUri = new Uri(dir.FullName);
            var rootUri = new Uri(root.FullName);
            return rootUri.MakeRelativeUri(dirUri).ToString();
        }

        /// <summary>
        /// Provides an interface to parse the command line arguments passed to the SoDBench.
        /// </summary>
        private sealed class SoDBenchOptions
        {
            public SoDBenchOptions() { }

            private static string NormalizePath(string path)
            {
                if (String.IsNullOrWhiteSpace(path))
                    throw new InvalidOperationException($"'{path}' is an invalid path: cannot be null or whitespace");

                if (path.Any(c => Path.GetInvalidPathChars().Contains(c)))
                    throw new InvalidOperationException($"'{path}' is an invalid path: contains invalid characters");

                return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
            }

            [Option('o', Required = false, HelpText = "Specifies the output file name for the csv document")]
            public string OutputFilename
            {
                get { return _outputFilename; }

                set
                {
                    _outputFilename = NormalizePath(value);
                }
            }

            [Option("dotnet", Required = false, HelpText = "Specifies the location of dotnet cli to use.")]
            public string DotnetExecutable
            {
                get { return _dotnetExe; }

                set
                {
                    _dotnetExe = NormalizePath(value);
                }
            }

            [Option("corelibs", Required = false, HelpText = "Specifies the location of .NET Core libaries to patch into dotnet. Cannot be used with --dotnet")]
            public string CoreLibariesDirectory
            {
                get { return _corelibsDir; }

                set
                {
                    _corelibsDir = NormalizePath(value);
                }
            }

            [Option("architecture", Required = false, Default = "x64", HelpText = "JitBench target architecture (It must match the built product that was copied into sandbox).")]
            public string TargetArchitecture { get; set; }

            [Option("channel", Required = false, Default = "master", HelpText = "Specifies the channel to use when installing the dotnet-cli")]
            public string DotnetChannel { get; set; }

            [Option('v', Required = false, HelpText = "Sets output to verbose")]
            public bool Verbose { get; set; }

            [Option("keep-artifacts", Required = false, HelpText = "Specifies that artifacts of this run should be kept")]
            public bool KeepArtifacts { get; set; }

            public static SoDBenchOptions Parse(string[] args)
            {
                using (var parser = new Parser((settings) => {
                    settings.CaseInsensitiveEnumValues = true;
                    settings.CaseSensitive = false;
                    settings.HelpWriter = new StringWriter();
                    settings.IgnoreUnknownArguments = true;
                }))
                {
                    SoDBenchOptions options = null;
                    parser.ParseArguments<SoDBenchOptions>(args)
                        .WithParsed(parsed => options = parsed)
                        .WithNotParsed(errors => {
                            foreach (Error error in errors)
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
                                        Console.WriteLine(new AssemblyName(typeof(SoDBenchOptions).GetTypeInfo().Assembly.FullName).Version);
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

                    if (options != null && !String.IsNullOrEmpty(options.DotnetExecutable) && !String.IsNullOrEmpty(options.CoreLibariesDirectory))
                    {
                        throw new ArgumentException("--dotnet and --corlibs cannot be used together");
                    }

                    return options;
                }
            }

            public static string Usage()
            {
                var parser = new Parser((parserSettings) =>
                {
                    parserSettings.CaseInsensitiveEnumValues = true;
                    parserSettings.CaseSensitive = false;
                    parserSettings.EnableDashDash = true;
                    parserSettings.HelpWriter = new StringWriter();
                    parserSettings.IgnoreUnknownArguments = true;
                });

                var helpTextString = new HelpText
                {
                    AddDashesToOption = true,
                    AddEnumValuesToHelpText = true,
                    AdditionalNewLineAfterOption = false,
                    Heading = "SoDBench",
                    MaximumDisplayWidth = 80,
                }.AddOptions(parser.ParseArguments<SoDBenchOptions>(new string[] { "--help" })).ToString();
                return helpTextString;
            }

            private string _dotnetExe;
            private string _corelibsDir;
            private string _outputFilename = "measurement.csv";
        }
    }

    // A simple class for escaping strings for CSV writing
    // https://stackoverflow.com/a/769713
    // Used instead of a package because only these < 20 lines of code are needed
    public static class Csv
    {
        public static string Escape( string s )
        {
            if ( s.Contains( QUOTE ) )
                s = s.Replace( QUOTE, ESCAPED_QUOTE );

            if ( s.IndexOfAny( CHARACTERS_THAT_MUST_BE_QUOTED ) > -1 )
                s = QUOTE + s + QUOTE;

            return s;
        }

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
    }
}
