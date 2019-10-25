// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CommandLine;
using System.IO;

namespace ILCompiler
{
    public class CommandLineOptions
    {
        public FileInfo[] InputFilePaths { get; set; }
        public FileInfo[] Reference { get; set; }
        public FileInfo OutputFilePath { get; set; }
        public bool Optimize { get; set; }
        public bool OptimizeSpace { get; set; }
        public bool OptimizeTime { get; set; }
        public bool InputBubble { get; set; }
        public bool CompileBubbleGenerics { get; set; }
        public bool Verbose { get; set; }

        public FileInfo DgmlLogFileName { get; set; }
        public bool GenerateFullDgmlLog { get; set; }

        public string TargetArch { get; set; }
        public string TargetOS { get; set; }
        public string JitPath { get; set; }
        public string SystemModule { get; set; }
        public bool WaitForDebugger { get; set; }
        public bool Tuning { get; set; }
        public bool Partial { get; set; }
        public bool Resilient { get; set; }

        public string SingleMethodTypeName { get; set; }
        public string SingleMethodName { get; set; }
        public string[] SingleMethodGenericArgs { get; set; }

        public string[] CodegenOptions { get; set; }

        public static RootCommand RootCommand()
        {
            RootCommand command = new RootCommand();
            command.AddOption(new Option(new[] { "--reference", "-r" }, "Reference file(s) for compilation", new Argument<FileInfo[]>()));
            command.AddOption(new Option(new[] { "--outputfilepath", "--out", "-o" }, "Output file path", new Argument<FileInfo>()));
            command.AddOption(new Option(new[] { "--optimize", "-O" }, "Enable optimizations", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--optimize-space", "--Os" }, "Enable optimizations, favor code space", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--optimize-time", "--Ot" }, "Enable optimizations, favor code speed"));
            command.AddOption(new Option(new[] { "--inputbubble" }, "True when the entire input forms a version bubble (default = per-assembly bubble)"));
            command.AddOption(new Option(new[] { "--tuning" }, "Generate IBC tuning image", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--partial" }, "Generate partial image driven by profile", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--compilebubblegenerics" }, "Compile instantiations from reference modules used in the current module", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--dgml-log-file-name", "--dmgllog" }, "Save result of dependency analysis as DGML", new Argument<FileInfo>()));
            command.AddOption(new Option(new[] { "--generate-full-dmgl-log", "--fulllog" }, "Save detailed log of dependency analysis", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--verbose" }, "Enable verbose logging", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--systemmodule" }, "System module name (default: System.Private.CoreLib)", new Argument<string>()));
            command.AddOption(new Option(new[] { "--waitfordebugger" }, "Pause to give opportunity to attach debugger", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--codegen-options", "--codegenopt" }, "Define a codegen option", new Argument<string[]>()));
            command.AddOption(new Option(new[] { "--resilient" }, "Disable behavior where unexpected compilation failures cause overall compilation failure", new Argument<bool>()));
            command.AddOption(new Option(new[] { "--targetarch" }, "Target architecture for cross compilation", new Argument<string>()));
            command.AddOption(new Option(new[] { "--targetos" }, "Target OS for cross compilation", new Argument<string>()));
            command.AddOption(new Option(new[] { "--jitpath" }, "Path to JIT compiler library", new Argument<string>()));
            command.AddOption(new Option(new[] { "--singlemethodtypename" }, "Single method compilation: name of the owning type", new Argument<string>()));
            command.AddOption(new Option(new[] { "--singlemethodname" }, "Single method compilation: generic arguments to the method", new Argument<string>()));
            command.AddOption(new Option(new[] { "--singlemethodgenericarg" }, "Single method compilation: generic arguments to the method", new Argument<string[]>()));
            command.Argument = new Argument<FileInfo[]>() { Name = "input-file-paths", Description = "Input file(s) to compile" }; 

            return command;
        }
    }
}
