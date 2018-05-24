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
        private Dictionary<string, Argument> _args = new Dictionary<string, Argument>();
        bool _raw;
        bool _verbose;
        bool _disasm;

        private R2RDump()
        {
        }

        private ArgumentSyntax ParseCommandLine(string[] args)
        {
            bool help = false;
            IReadOnlyList<string> inputFilenames = Array.Empty<string>();
            string outputFilePath = null;
            bool diff = false;
            bool header = false;
            _raw = false;
            _verbose = false;
            _disasm = false;
            IReadOnlyList<string> queries = Array.Empty<string>();
            IReadOnlyList<string> keywords = Array.Empty<string>();
            IReadOnlyList<int> rtfids = Array.Empty<int>();
            IReadOnlyList<string> sections = Array.Empty<string>();

            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = "R2RDump";
                syntax.HandleHelp = false;
                syntax.HandleErrors = false;

                syntax.DefineOption("h|help", ref help, "Help message for R2RDump");
                syntax.DefineOptionList("i|in", ref inputFilenames, "Input file(s) to compile");
                syntax.DefineOption("o|out", ref outputFilePath, "Output file path");
                syntax.DefineOption("v|verbose", ref _verbose, "Dump the contents of each section or the native code of each method");
                syntax.DefineOption("raw", ref _raw, "Dump the raw bytes of each section or runtime function");
                syntax.DefineOption("l|less", ref header, "Only dump R2R header");
                syntax.DefineOption("d|disasm", ref _disasm, "Show disassembly of methods or runtime functions");
                syntax.DefineOptionList("q|query", ref queries, "Query method by exact name, signature, row id or token");
                syntax.DefineOptionList("k|keyword", ref keywords, "Search method by keyword");
                syntax.DefineOptionList("r|rtf", ref rtfids, ArgStringToInt, "Get one runtime function by id");
                syntax.DefineOptionList("s|section", ref sections, "Get section by keyword");
                syntax.DefineOption("diff", ref diff, "Compare two R2R images");
            });

            var options = argSyntax.GetOptions();
            foreach (Argument option in options)
            {
                _args[option.Name] = option;
            }

            return argSyntax;
        }

        private int ArgStringToInt(string arg)
        {
            bool isNum;
            return ArgStringToInt(arg, out isNum);
        }

        private int ArgStringToInt(string arg, out bool isNum)
        {
            int n = -1;
            if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                isNum = int.TryParse(arg.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out n);
                return n;
            }
            isNum = int.TryParse(arg, out n);
            return n;
        }

        public static void WriteWarning(string warning)
        {
            Console.WriteLine($"Warning: {warning}");
        }

        private void WriteDivider(string title = null)
        {
            if (title != null)
            {
                Console.WriteLine("============== " + title + " ==============");
            }
            else
            {
                Console.WriteLine("------------------");
            }
            
            Console.WriteLine();
        }

        private void DumpHeader(R2RReader r2r)
        {
            Console.WriteLine(r2r.R2RHeader.ToString());
            if (_raw)
            {
                Console.WriteLine(r2r.DumpBytes(r2r.R2RHeader.RelativeVirtualAddress, (uint)r2r.R2RHeader.Size));
            }
            WriteDivider("R2R Sections");
            foreach (R2RSection section in r2r.R2RHeader.Sections.Values)
            {
                DumpSection(r2r, section);
            }
        }

        private void DumpSection(R2RReader r2r, R2RSection section)
        {
            WriteDivider();
            Console.WriteLine(section.ToString());
            if (_raw)
            {
                Console.WriteLine(r2r.DumpBytes(section.RelativeVirtualAddress, (uint)section.Size));
            }
        }

        private void DumpMethod(R2RReader r2r, R2RMethod method, RuntimeFunction rtf = null)
        {
            Console.WriteLine(method.ToString());

            if (rtf == null)
            {
                foreach (RuntimeFunction runtimeFunction in method.RuntimeFunctions)
                {
                    Console.WriteLine($"{runtimeFunction}");
                    if (_raw)
                    {
                        Console.WriteLine(r2r.DumpBytes(runtimeFunction.StartAddress, (uint)runtimeFunction.Size));
                    }
                }
            }
            else
            {
                Console.WriteLine(rtf);
                if (_raw)
                {
                    Console.WriteLine(r2r.DumpBytes(rtf.StartAddress, (uint)rtf.Size));
                }
            }
            WriteDivider();
        }

        private void MethodQuery(R2RReader r2r, string title, IReadOnlyList<string> queries, bool exact)
        {
            if (queries.Count > 0)
            {
                WriteDivider(title);
            }
            foreach (string q in queries)
            {
                List<R2RMethod> res = new List<R2RMethod>();
                GetMethod(r2r, q, exact, res);

                Console.WriteLine(res.Count + " result(s) for \"" + q + "\"");
                Console.WriteLine();
                WriteDivider();
                foreach (R2RMethod method in res)
                {
                    DumpMethod(r2r, method);
                }
                
            }
        }

        private void SectionQuery(R2RReader r2r, IReadOnlyList<string> queries)
        {
            if (queries.Count > 0)
            {
                WriteDivider("R2R Section");
            }
            foreach (string q in queries)
            {
                List<R2RSection> res = new List<R2RSection>();
                GetSection(r2r, q, res);

                Console.WriteLine(res.Count + " result(s) for \"" + q + "\"");
                Console.WriteLine();
                foreach (R2RSection section in res)
                {
                    DumpSection(r2r, section);
                }

            }
        }

        private void RTFQuery(R2RReader r2r, IReadOnlyList<int> queries)
        {
            if (queries.Count > 0)
            {
                WriteDivider("Runtime Functions");
            }
            foreach (int q in queries)
            {
                Console.WriteLine("id: " + q);
                Console.WriteLine();
                WriteDivider();

                R2RMethod method = null;
                RuntimeFunction rtf = GetRuntimeFunction(r2r, q, out method);

                if (method == null)
                {
                    WriteWarning("Unable to find by id " + q);
                    continue;
                }
                DumpMethod(r2r, method, rtf);
            }
        }

        public void Dump(R2RReader r2r)
        {
            IReadOnlyList<string> queries = (IReadOnlyList<string>)_args["q"].Value;
            IReadOnlyList<string> keywords = (IReadOnlyList<string>)_args["k"].Value;
            IReadOnlyList<int> rtfids = (IReadOnlyList<int>)_args["r"].Value;
            IReadOnlyList<string> sections = (IReadOnlyList<string>)_args["s"].Value;

            bool dumpHeader = (bool)_args["l"].Value;

            Console.WriteLine($"Filename: {r2r.Filename}");
            Console.WriteLine($"Machine: {r2r.Machine}");
            Console.WriteLine($"ImageBase: 0x{r2r.ImageBase:X8}");
            Console.WriteLine();

            if (queries.Count == 0 && keywords.Count == 0 && rtfids.Count == 0) //dump all sections and methods
            {
                WriteDivider("R2R Header");
                DumpHeader(r2r);
                
                if (!dumpHeader)
                {
                    WriteDivider("R2R Methods");
                    Console.WriteLine();
                    foreach (R2RMethod method in r2r.R2RMethods)
                    {
                        DumpMethod(r2r, method);
                    }
                }
            }
            else //dump queried sections/methods/runtimeFunctions
            {
                if (dumpHeader)
                {
                    Console.WriteLine(r2r.R2RHeader.ToString());
                    if (_raw)
                    {
                        Console.WriteLine(r2r.DumpBytes(r2r.R2RHeader.RelativeVirtualAddress, (uint)r2r.R2RHeader.Size));
                    }
                }

                SectionQuery(r2r, sections);
                RTFQuery(r2r, rtfids);
                MethodQuery(r2r, "R2R Methods by Query", queries, true);
                MethodQuery(r2r, "R2R Methods by Keyword", keywords, false);
            }

            Console.WriteLine("========================================================");
            Console.WriteLine();
        }

        private bool match(R2RMethod method, string query, bool exact)
        {
            bool isNum;
            int id = ArgStringToInt(query, out isNum);
            bool idMatch = isNum && (method.Rid == id || method.Token == id);

            bool sigMatch = false;
            if (exact)
            {
                sigMatch = method.Name.Equals(query, StringComparison.OrdinalIgnoreCase);
                if (!sigMatch)
                {
                    string sig = method.Signature.Replace(" ", "");
                    string q = query.Replace(" ", "");
                    int iMatch = sig.IndexOf(q, StringComparison.OrdinalIgnoreCase);
                    sigMatch = (iMatch == 0 || (iMatch > 0 && iMatch == (sig.Length - q.Length) && sig[iMatch - 1] == '.'));
                }
            }
            else
            {
                string sig = method.ReturnType + method.Signature.Replace(" ", "");
                sigMatch = (sig.IndexOf(query.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return idMatch || sigMatch;
        }

        private bool match(R2RSection section, string query)
        {
            bool isNum;
            int queryInt = ArgStringToInt(query, out isNum);
            string typeName = Enum.GetName(typeof(R2RSection.SectionType), section.Type);

            return (isNum && (int)section.Type == queryInt) || typeName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void GetMethod(R2RReader r2r, string query, bool exact, List<R2RMethod> res)
        {
            foreach (R2RMethod method in r2r.R2RMethods)
            {
                if (match(method, query, exact))
                {
                    res.Add(method);
                }
            }
        }

        public void GetSection(R2RReader r2r, string query, List<R2RSection> res)
        {
            foreach (R2RSection section in r2r.R2RHeader.Sections.Values)
            {
                if (match(section, query))
                {
                    res.Add(section);
                }
            }
        }

        public RuntimeFunction GetRuntimeFunction(R2RReader r2r, int rtfid, out R2RMethod m)
        {
            foreach (R2RMethod method in r2r.R2RMethods)
            {
                foreach (RuntimeFunction rtf in method.RuntimeFunctions)
                {
                    if (rtf.Id == rtfid)
                    {
                        m = method;
                        return rtf;
                    }
                }
            }
            m = null;
            return null;
        }

        private int Run(string[] args)
        {
            ArgumentSyntax syntax = ParseCommandLine(args);
            
            if ((bool)_args["h"].Value)
            {
                Console.WriteLine(syntax.GetHelpText());
                return 0;
            }

            IReadOnlyList<string> inputFilenames = (IReadOnlyList<string>)_args["i"].Value;
            if (inputFilenames.Count == 0)
                throw new ArgumentException("Input filename must be specified (--in <file>)");

            FileStream fileStream = null;
            StreamWriter writer = null;
            string outputFilename = (string)_args["o"].Value;
            if (outputFilename != null)
            {
                fileStream = new FileStream((string)_args["o"].Value, FileMode.Create, FileAccess.Write);
                writer = new StreamWriter(fileStream);
                Console.SetOut(writer);
            }

            foreach (string filename in inputFilenames)
            {
                R2RReader r2r = new R2RReader(filename);
                Dump(r2r);
            }

            if (outputFilename != null)
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
