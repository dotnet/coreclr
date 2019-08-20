// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.IO;
using Microsoft.NET.HostModel.Bundle;

namespace Bundle
{
    /// <summary>
    ///  The main driver for Bundle and Extract operations.
    /// </summary>
    public static class Program
    {
        enum RunMode
        {
            Help,
            Bundle,
            Extract
        };

        static RunMode Mode = RunMode.Bundle;

        // Common Options:
        static bool Verbose = false;
        static string OutputDir;

        // Bundle options:
        static bool EmbedPDBs = false;
        static string SourceDir;
        static string Host;

        // Extract options:
        static string BundleToExtract;

        static void Usage()
        {
            Console.WriteLine($".NET Core Bundler (version {Bundler.Version})");
            Console.WriteLine("Usage: bundle <options>");
            Console.WriteLine("");
            Console.WriteLine("Bundle options:");
            Console.WriteLine("  --source <PATH>   Directory containing files to bundle (required).");
            Console.WriteLine("  --host <NAME>     Application host within source directory (required).");
            Console.WriteLine("  --pdb             Embed PDB files.");
            Console.WriteLine("");
            Console.WriteLine("Extract options:");
            Console.WriteLine("  --extract <PATH>  Extract files from the specified bundle.");
            Console.WriteLine("");

            Console.WriteLine("Common options:");
            Console.WriteLine("  -o|--output <PATH> Output directory (default: current).");
            Console.WriteLine("  -d|--diagnostics   Enable diagnostic output.");
            Console.WriteLine("  -?|-h|--help       Display usage information.");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("Bundle:  bundle --source <publish-dir> --host <host-exe> -o <output-dir>");
            Console.WriteLine("Extract: bundle --extract <bundle-exe> -o <output-dir>");
        }

        static void Fail(string type, string message)
        {
            Console.Error.WriteLine($"{type}: {message}");
        }

        static void ParseArgs(string[] args)
        {
            int i = 0;
            Func<string, string> NextArg = (string option) =>
            {
                if (++i >= args.Length)
                {
                    throw new BundleException("Argument missing for" + option);
                }
                return args[i];
            };

            for (; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg.ToLower())
                {
                    case "-?":
                    case "-h":
                    case "--help":
                        Mode = RunMode.Help;
                        break;

                    case "--extract":
                        Mode = RunMode.Extract;
                        BundleToExtract = NextArg(arg);
                        break;

                    case "-d":
                    case "--diagnostics":
                        Verbose = true;
                        break;

                    case "--host":
                        Host = NextArg(arg);
                        break;

                    case "--source":
                        SourceDir = NextArg(arg);
                        break;

                    case "-o":
                    case "--output":
                        OutputDir = NextArg(arg);
                        break;

                    case "--pdb":
                        EmbedPDBs = true;
                        break;

                    default:
                        throw new BundleException("Invalid option: " + arg);
                }
            }

            if (Mode == RunMode.Bundle)
            {
                if (SourceDir == null)
                {
                    throw new BundleException("Missing argument: --source");
                }

                if (Host == null)
                {
                    throw new BundleException("Missing argument: --host");
                }
            }

            if (OutputDir == null)
            {
                OutputDir = Environment.CurrentDirectory;
            }
        }

        static void Run()
        {
            switch (Mode)
            {
                case RunMode.Help:
                    Usage();
                    break;

                case RunMode.Bundle:
                    new Bundler(Host, OutputDir, EmbedPDBs, Verbose).GenerateBundle(SourceDir);
                    break;

                case RunMode.Extract:
                    new Extractor(BundleToExtract, OutputDir, Verbose).ExtractFiles();
                    break;
            }
        }

        public static int Main(string[] args)
        {
            try
            {
                ParseArgs(args);
            }
            catch (BundleException e)
            {
                Fail("ERROR", e.Message);
                Usage();
                return -1;
            }

            try
            {
                Run();
            }
            catch (BundleException e)
            {
                Fail("ERROR", e.Message);
                return -2;
            }

            return 0;
        }
    }
}
