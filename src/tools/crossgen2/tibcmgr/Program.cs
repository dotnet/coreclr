// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.CommandLine;
using System.IO;

using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

using Internal.CommandLine;

using ILCompiler;
using ILCompiler.IBC;

namespace tibcmgr
{
    class Program
    {
        private const string DefaultSystemModule = "System.Private.CoreLib";

        private Dictionary<string, string> _inputFilePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _referenceFilePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private bool _isVerbose;
        private bool _help;
        private string _systemModuleName = DefaultSystemModule;
        private string _command;
        private string _inputIbcFile;
        private string _inputILFile;
        private string _outputTibcFile;

        private Program()
        {
        }

        private void Help(string helpText)
        {
            Console.WriteLine();
            Console.Write("Microsoft (R) CoreCLR TIbc Manager");
            Console.Write(" ");
            Console.Write(typeof(Program).GetTypeInfo().Assembly.GetName().Version);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(helpText);
        }

        private ArgumentSyntax ParseCommandLine(string[] args)
        {
            IReadOnlyList<string> referenceFiles = Array.Empty<string>();

            bool waitForDebugger = false;
            AssemblyName name = typeof(Program).GetTypeInfo().Assembly.GetName();
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = name.Name.ToString();

                // HandleHelp writes to error, fails fast with crash dialog and lacks custom formatting.
                syntax.HandleHelp = false;
                syntax.HandleErrors = true;

                syntax.DefineCommand("help", ref _command, "Help message for tibcmgr");
                syntax.DefineCommand("convert", ref _command, "Convert an ibc file to a tibc file");
                syntax.DefineOptionList("r|reference", ref referenceFiles, "Reference file(s) for token reference in ibc file");
                syntax.DefineOption("systemmodule", ref _systemModuleName, "System module name (default: System.Private.CoreLib)");
                syntax.DefineOption("waitfordebugger", ref waitForDebugger, "Pause to give opportunity to attach debugger");
                syntax.DefineOption("verbose", ref _isVerbose, "Enable verbose logging");
                syntax.DefineParameter("ibc", ref _inputIbcFile, "Input ibc file");
                syntax.DefineParameter("il", ref _inputILFile, "Input il file that matches with the ibc file");
                syntax.DefineParameter("output", ref _outputTibcFile, "Output file in tibc format");
            });

            if (_command == "help")
                _help = true;

            if (waitForDebugger)
            {
                Console.WriteLine("Waiting for debugger to attach. Press ENTER to continue");
                Console.ReadLine();
            }

            Helpers.AppendExpandedPaths(_inputFilePaths, _inputILFile, true);

            foreach (var reference in referenceFiles)
                Helpers.AppendExpandedPaths(_referenceFilePaths, reference, false);

            return argSyntax;

        }

        private int Run(string[] args)
        {
            ArgumentSyntax syntax = ParseCommandLine(args);
            if (_help)
            {
                Help(syntax.GetHelpText());
                return 1;
            }

            if (_command != "convert")
                throw new CommandLineException("Command must be specified");

            var targetDetails = new TargetDetails(TargetArchitecture.X64, TargetOS.Linux, TargetAbi.CoreRT, SimdVectorLength.None);
            CompilerTypeSystemContext typeSystemContext = new ReadyToRunCompilerContext(targetDetails, SharedGenericsMode.CanonicalReferenceTypes);

            Dictionary<string, string> inputFilePaths = new Dictionary<string, string>();
            foreach (var inputFile in _inputFilePaths)
            {
                try
                {
                    typeSystemContext.GetModuleFromPath(inputFile.Value);
                    inputFilePaths.Add(inputFile.Key, inputFile.Value);
                }
                catch (TypeSystemException.BadImageFormatException)
                {
                    // Keep calm and carry on.
                }
            }

            typeSystemContext.InputFilePaths = inputFilePaths;
            typeSystemContext.ReferenceFilePaths = _referenceFilePaths;

            typeSystemContext.SetSystemModule(typeSystemContext.GetModuleForSimpleName(_systemModuleName));

            if (typeSystemContext.InputFilePaths.Count == 0)
                throw new CommandLineException("No input files specified");

            var logger = new Logger(Console.Out, _isVerbose);

            List<ModuleDesc> referenceableModules = new List<ModuleDesc>();
            foreach (var inputFile in inputFilePaths)
            {
                try
                {
                    referenceableModules.Add(typeSystemContext.GetModuleFromPath(inputFile.Value));
                }
                catch { } // Ignore non-managed pe files
            }

            foreach (var referenceFile in _referenceFilePaths.Values)
            {
                try
                {
                    referenceableModules.Add(typeSystemContext.GetModuleFromPath(referenceFile));
                }
                catch { } // Ignore non-managed pe files
            }

            IBCProfileParser ibcParser = new IBCProfileParser(logger, referenceableModules);
            EcmaModule module = typeSystemContext.GetModuleFromPath(_inputILFile);
            ProfileData parsedProfileData = ibcParser.ParseIBCDataFromByteArray(module, File.ReadAllBytes(_inputIbcFile));

            using (FileStream fs = new FileStream(_outputTibcFile, FileMode.Create))
            {
                ProfileData.SerializeToJSon(parsedProfileData, fs);
            }

            Console.WriteLine($"Generated {_outputTibcFile}");

            ProfileData reloadedProfData = ProfileData.ReadJsonData(logger, File.ReadAllBytes(_outputTibcFile), typeSystemContext);
            if (!CompareProfileDataEqual(logger, _inputIbcFile, parsedProfileData, _outputTibcFile, reloadedProfData))
            {
                Console.WriteLine($"Error: Compare of {_outputTibcFile} to {_inputIbcFile} failed");
                return -1;
            }

            return 0;
        }

        bool CompareProfileDataEqual(Logger logger, string leftName, ProfileData left, string rightName, ProfileData right)
        {
            if (left.PartialNGen != right.PartialNGen)
            {
                logger.Writer.WriteLine($"PartialNGen: {leftName}:{left.PartialNGen} {rightName}:{right.PartialNGen}");
            }
            List<MethodProfileData> leftDataList = new List<MethodProfileData>(left.GetAllMethodProfileData());
            List<MethodProfileData> rightDataList = new List<MethodProfileData>(right.GetAllMethodProfileData());

            if (leftDataList.Count != rightDataList.Count)
            {
                logger.Writer.WriteLine($"ProfileDataCount: {leftName}:{leftDataList.Count} {rightName}:{rightDataList.Count}");
                return false;
            }

            for (int i = 0; i < leftDataList.Count; i++)
            {
                MethodProfileData leftData = leftDataList[i];
                MethodProfileData rightData = rightDataList[i];
                if (leftData.Method != rightData.Method)
                {
                    logger.Writer.WriteLine($"Method: {leftName}:{leftData.Method} {rightName}:{rightData.Method}");
                    return false;
                }
                if (leftData.Flags != rightData.Flags)
                {
                    logger.Writer.WriteLine($"Flags: {leftName}:{leftData.Flags} {rightName}:{rightData.Flags} on method {leftData.Method}");
                    return false;
                }
            }
            return true;
        }

        static int Main(string[] args)
        {
            try
            {
                return new Program().Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e.Message);
                Console.Error.WriteLine(e.ToString());
                return 1;
            }
        }
    }
}
