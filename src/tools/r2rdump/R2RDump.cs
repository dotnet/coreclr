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
        private bool _help = false;
        private IReadOnlyList<string> _inputFilenames = Array.Empty<string>();
        private string _outputFilename = null;
        private bool _raw = false;
        private bool _header = false;
        private bool _disasm = false;
        private IReadOnlyList<string> _queries = Array.Empty<string>();
        private IReadOnlyList<string> _keywords = Array.Empty<string>();
        private IReadOnlyList<int> _runtimeFunctions = Array.Empty<int>();
        private IReadOnlyList<string> _sections = Array.Empty<string>();
        private bool _diff = false;

        private R2RDump()
        {
        }

        private ArgumentSyntax ParseCommandLine(string[] args)
        {
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = "R2RDump";
                syntax.HandleHelp = false;
                syntax.HandleErrors = false;

                syntax.DefineOption("h|help", ref _help, "Help message for R2RDump");
                syntax.DefineOptionList("i|in", ref _inputFilenames, "Input file(s) to dump. Expects them to by ReadyToRun images");
                syntax.DefineOption("o|out", ref _outputFilename, "Output file path. Dumps everything to the specified file except help message and exception messages");
                syntax.DefineOption("v|verbose|raw", ref _raw, "Dump the raw bytes of each section or runtime function");
                syntax.DefineOption("l|less", ref _header, "Only dump R2R header");
                syntax.DefineOption("d|disasm", ref _disasm, "Show disassembly of methods or runtime functions");
                syntax.DefineOptionList("q|query", ref _queries, "Query method by exact name, signature, row id or token");
                syntax.DefineOptionList("k|keyword", ref _keywords, "Search method by keyword");
                syntax.DefineOptionList("r|rtf", ref _runtimeFunctions, ArgStringToInt, "Get one runtime function by id or relative virtual address");
                syntax.DefineOptionList("s|section", ref _sections, "Get section by keyword");
                syntax.DefineOption("diff", ref _diff, "Compare two R2R images"); // not yet implemented
            });

            return argSyntax;
        }

        private int ArgStringToInt(string arg)
        {
            bool isNum;
            return ArgStringToInt(arg, out isNum);
        }

        /// <summary>
        /// Converts string passed as cmd line args into int, works for hexidecimal with 0x as prefix
        /// </summary>
        /// <param name="arg">The argument string to convert</param>
        /// <param name="isNum">Set to false if conversion failed</param>
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

        private void WriteDivider(string title)
        {
            Console.WriteLine("============== " + title + " ==============");
            Console.WriteLine();
        }

        private void WriteSubDivider()
        {
            Console.WriteLine("------------------");
            Console.WriteLine();
        }

        /// <summary>
        /// Dumps the R2RHeader and all the sections in the header
        /// </summary>
        private void DumpHeader(R2RReader r2r, bool dumpSections)
        {
            Console.WriteLine(r2r.R2RHeader.ToString());
            if (_raw)
            {
                Console.WriteLine(r2r.R2RHeader.DumpBytes(r2r));
            }
            if (dumpSections)
            {
                WriteDivider("R2R Sections");
                foreach (R2RSection section in r2r.R2RHeader.Sections.Values)
                {
                    DumpSection(r2r, section);
                }
            }
        }

        /// <summary>
        /// Dumps one R2RSection
        /// </summary>
        private void DumpSection(R2RReader r2r, R2RSection section)
        {
            WriteSubDivider();
            Console.WriteLine(section.ToString());
            if (_raw)
            {
                Console.WriteLine(section.DumpBytes(r2r));
            }
        }

        /// <summary>
        /// Dumps one R2RMethod. 
        /// If <param>rtf</param> is not null, output the specified runtime function. Otherwise output all runtime functions of the method
        /// </summary>
        private void DumpMethod(R2RReader r2r, R2RMethod method, RuntimeFunction rtf = null)
        {
            WriteSubDivider();
            Console.WriteLine(method.ToString());

            if (rtf == null)
            {
                foreach (RuntimeFunction runtimeFunction in method.RuntimeFunctions)
                {
                    Console.WriteLine($"{runtimeFunction}");
                    if (_raw)
                    {
                        Console.WriteLine(runtimeFunction.DumpBytes(r2r));
                    }
                }
            }
            else
            {
                Console.WriteLine(rtf);
                if (_raw)
                {
                    Console.WriteLine(rtf.DumpBytes(r2r));
                }
            }
        }

        // <summary>
        /// For each query in the list of queries, search for all methods matching the query by name, signature or id
        /// </summary>
        /// <param name="r2r">Contains all the extracted info about the ReadyToRun image</param>
        /// <param name="title">The title to print, "R2R Methods by Query" or "R2R Methods by Keyword"</param>
        /// <param name="queries">The keywords/ids to search for</param>
        /// <param name="exact">Specifies whether to look for methods with names/signatures/ids matching the method exactly or partially</param>
        private void QueryMethod(R2RReader r2r, string title, IReadOnlyList<string> queries, bool exact)
        {
            if (queries.Count > 0)
            {
                WriteDivider(title);
            }
            foreach (string q in queries)
            {
                List<R2RMethod> res = new List<R2RMethod>();
                FindMethod(r2r, q, exact, res);

                Console.WriteLine(res.Count + " result(s) for \"" + q + "\"");
                Console.WriteLine();
                foreach (R2RMethod method in res)
                {
                    DumpMethod(r2r, method);
                }
            }
        }

        // <summary>
        /// For each query in the list of queries, search for all sections by the name or value of the ReadyToRunSectionType enum
        /// </summary>
        /// <param name="r2r">Contains all the extracted info about the ReadyToRun image</param>
        /// <param name="queries">The names/values to search for</param>
        private void QuerySection(R2RReader r2r, IReadOnlyList<string> queries)
        {
            if (queries.Count > 0)
            {
                WriteDivider("R2R Section");
            }
            foreach (string q in queries)
            {
                List<R2RSection> res = new List<R2RSection>();
                FindSection(r2r, q, res);

                Console.WriteLine(res.Count + " result(s) for \"" + q + "\"");
                Console.WriteLine();
                foreach (R2RSection section in res)
                {
                    DumpSection(r2r, section);
                }
            }
        }

        // <summary>
        /// For each query in the list of queries, search for a runtime function by id. 
        /// The method containing the runtime function gets outputted, along with the single runtime function that was searched
        /// </summary>
        /// <param name="r2r">Contains all the extracted info about the ReadyToRun image</param>
        /// <param name="queries">The ids to search for</param>
        private void QueryRuntimeFunction(R2RReader r2r, IReadOnlyList<int> queries)
        {
            if (queries.Count > 0)
            {
                WriteDivider("Runtime Functions");
            }
            foreach (int q in queries)
            {
                Console.WriteLine("id: " + q);
                Console.WriteLine();

                R2RMethod method = null;
                RuntimeFunction rtf = FindRuntimeFunction(r2r, q, out method);

                if (method == null)
                {
                    WriteWarning("Unable to find by id " + q);
                    continue;
                }
                DumpMethod(r2r, method, rtf);
            }
        }

        /// <summary>
        /// Outputs specified headers, sections, methods or runtime functions for one ReadyToRun image
        /// </summary>
        /// <param name="r2r">The structure containing the info of the ReadyToRun image</param>
        public void Dump(R2RReader r2r)
        {
            Console.WriteLine($"Filename: {r2r.Filename}");
            Console.WriteLine($"Machine: {r2r.Machine}");
            Console.WriteLine($"ImageBase: 0x{r2r.ImageBase:X8}");
            Console.WriteLine();

            if (_queries.Count == 0 && _keywords.Count == 0 && _runtimeFunctions.Count == 0 && _sections.Count == 0) //dump all sections and methods
            {
                WriteDivider("R2R Header");
                DumpHeader(r2r, true);
                
                if (!_header)
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
                if (_header)
                {
                    DumpHeader(r2r, false);
                }

                QuerySection(r2r, _sections);
                QueryRuntimeFunction(r2r, _runtimeFunctions);
                QueryMethod(r2r, "R2R Methods by Query", _queries, true);
                QueryMethod(r2r, "R2R Methods by Keyword", _keywords, false);
            }

            Console.WriteLine("========================================================");
            Console.WriteLine();
        }

        /// <summary>
        /// Returns true if the name/signature/id of <param>method</param> matches <param>query</param>
        /// </summary>
        /// <param name="exact">Specifies exact or partial match</param>
        /// <remarks>Case-insensitive and ignores whitespace</remarks>
        private bool Match(R2RMethod method, string query, bool exact)
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

        /// <summary>
        /// Returns true if the name or value of the ReadyToRunSectionType of <param>section</param> matches <param>query</param>
        /// </summary>
        /// <remarks>Case-insensitive</remarks>
        private bool Match(R2RSection section, string query)
        {
            bool isNum;
            int queryInt = ArgStringToInt(query, out isNum);
            string typeName = Enum.GetName(typeof(R2RSection.SectionType), section.Type);

            return (isNum && (int)section.Type == queryInt) || typeName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Finds all R2RMethods by name/signature/id matching <param>query</param>
        /// </summary>
        /// <param name="r2r">Contains all extracted info about the ReadyToRun image</param>
        /// <param name="query">The name/signature/id to search for</param>
        /// <param name="exact">Specifies exact or partial match</param>
        /// <out name="res">List of all matching methods</out>
        /// <remarks>Case-insensitive and ignores whitespace</remarks>
        public void FindMethod(R2RReader r2r, string query, bool exact, List<R2RMethod> res)
        {
            foreach (R2RMethod method in r2r.R2RMethods)
            {
                if (Match(method, query, exact))
                {
                    res.Add(method);
                }
            }
        }

        /// <summary>
        /// Finds all R2RSections by name or value of the ReadyToRunSectionType matching <param>query</param>
        /// </summary>
        /// <param name="r2r">Contains all extracted info about the ReadyToRun image</param>
        /// <param name="query">The name or value to search for</param>
        /// <out name="res">List of all matching sections</out>
        /// <remarks>Case-insensitive</remarks>
        public void FindSection(R2RReader r2r, string query, List<R2RSection> res)
        {
            foreach (R2RSection section in r2r.R2RHeader.Sections.Values)
            {
                if (Match(section, query))
                {
                    res.Add(section);
                }
            }
        }

        /// <summary>
        /// Returns the runtime function with id matching <param>rtfQuery</param>
        /// </summary>
        /// <param name="r2r">Contains all extracted info about the ReadyToRun image</param>
        /// <param name="rtfQuery">The name or value to search for</param>
        /// <out name="method">Gets set to the method containing the runtime function</out>
        public RuntimeFunction FindRuntimeFunction(R2RReader r2r, int rtfQuery, out R2RMethod method)
        {
            foreach (R2RMethod m in r2r.R2RMethods)
            {
                foreach (RuntimeFunction rtf in m.RuntimeFunctions)
                {
                    if (rtf.Id == rtfQuery || (rtf.StartAddress >= rtfQuery && rtf.StartAddress + rtf.Size < rtfQuery))
                    {
                        method = m;
                        return rtf;
                    }
                }
            }
            method = null;
            return null;
        }

        private int Run(string[] args)
        {
            ArgumentSyntax syntax = ParseCommandLine(args);
            
            if (_help)
            {
                Console.WriteLine(syntax.GetHelpText());
                return 0;
            }

            if (_inputFilenames.Count == 0)
                throw new ArgumentException("Input filename must be specified (--in <file>)");

            // open output file
            FileStream fileStream = null;
            StreamWriter writer = null;
            if (_outputFilename != null)
            {
                fileStream = new FileStream(_outputFilename, FileMode.Create, FileAccess.Write);
                writer = new StreamWriter(fileStream);
                Console.SetOut(writer);
            }

            foreach (string filename in _inputFilenames)
            {
                R2RReader r2r = new R2RReader(filename);
                Dump(r2r);
            }

            // close output file
            if (_outputFilename != null)
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
