// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;

namespace R2RDump
{
    class R2RDump
    {

        IReadOnlyList<string> _inputFilePaths = Array.Empty<string>();
        private string _outputFilePath;
        Dictionary<string, Argument> _args = new Dictionary<string, Argument>();

        private bool _help;
        private bool _diff;

        private R2RDump()
        {
        }

        private ArgumentSyntax ParseCommandLine(string[] args)
        {
            bool all = false;
            bool verbose = false;
            bool header = false;
            bool disasm = false;
            int rid = -1;
            int rtfid = -1;

            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = "R2RDump";
                syntax.HandleHelp = false;
                syntax.HandleErrors = false;

                syntax.DefineOption("h|help", ref _help, "Help message for R2RDump");
                syntax.DefineOptionList("i|in", ref _inputFilePaths, "Input file(s) to compile");
                syntax.DefineOption("o|out", ref _outputFilePath, "Output file path");
                syntax.DefineOption("a|all", ref all, "Dump all info");
                syntax.DefineOption("v|verbose", ref verbose, "Dump the contents of each section and the native code of each method");
                syntax.DefineOption("l|less", ref header, "Only dump R2R header");
                syntax.DefineOption("d|disasm", ref disasm, "Show disassembly of method or runtime function");
                syntax.DefineOption("m|rid", ref rid, "Show one method by rid");
                syntax.DefineOption("r|rtf", ref rtfid, "Show one runtime function by rid");
                syntax.DefineOption("diff", ref _diff, "Compare R2R images");
            });

            var options = argSyntax.GetOptions();
            foreach (Argument option in options)
            {
                _args[option.Name] = option;
            }

            return argSyntax;
        }

        public static void OutputWarning(string warning)
        {
            Console.WriteLine($"Warning: {warning}");
        }

        private int Run(string[] args)
        {
            ArgumentSyntax syntax = ParseCommandLine(args);
            
            if (_help)
            {
                Console.WriteLine(syntax.GetHelpText());
                return 0;
            }

            if (_inputFilePaths.Count == 0)
                throw new ArgumentException("Input filename must be specified (--in <file>)");

            FileStream fileStream = null;
            StreamWriter writer = null;
            if (_outputFilePath != null)
            {
                fileStream = new FileStream(_outputFilePath, FileMode.Create, FileAccess.Write);
                writer = new StreamWriter(fileStream);
                Console.SetOut(writer);
            }

            R2RReader r2r = new R2RReader(_inputFilePaths[0]);
            if (r2r.IsR2R)
            {
                Console.WriteLine($"Filename: {r2r.Filename}");
                Console.WriteLine($"Machine: {r2r.Machine}");
                Console.WriteLine($"ImageBase: 0x{r2r.ImageBase:X8}");

                Console.WriteLine("============== R2R Header ==============");
                Console.WriteLine(r2r.R2RHeader.ToString());
                foreach (KeyValuePair<R2RSection.SectionType, R2RSection> section in r2r.R2RHeader.Sections)
                {
                    Console.WriteLine("------------------");
                    Console.WriteLine();
                    Console.WriteLine(section.Value.ToString());
                }

                Console.WriteLine("============== R2R Methods ==============");
                Console.WriteLine();
                foreach (R2RMethod method in r2r.R2RMethods)
                {
                    Console.Write(method.ToString());
                    Console.WriteLine("------------------");
                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            if (_outputFilePath != null)
            {
                Console.SetOut(Console.Out);
                writer.Close();
                fileStream.Close();
            }

            return 0;
        }

        public static int Main(string[] args)
        {
            try
            {
                return new R2RDump().Run(args);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Error: " + e.ToString());
                return 1;
            }
        }
    }
}
