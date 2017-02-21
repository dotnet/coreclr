using System;
using Xunit;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.Loader;

namespace ConsoleApplication
{
    public class Program
    {
        // This string templates will generate a scrip.
        // They take following parameters:
        //  {0} - Method name.
        //  {1} - Path to the Debugger.Tests\dotnet folder.
        static string cmdTemplate = @"@echo off
setlocal ENABLEDELAYEDEXPANSION
set __ThisScriptPath=%~dp0
pushd {1}
%CORE_ROOT%\corerun.exe xunit.console.netcore.exe DebuggerTest.dll -method {0} -xml %__ThisScriptPath%\{0}.log
set CLRTestExitCode=!ERRORLEVEL!
popd
IF NOT ""%CLRTestExitCode%"" == ""0"" (
  ECHO END EXECUTION - FAILED
  ECHO FAILED
  Exit /b 1
) ELSE (
  ECHO END EXECUTION - PASSED
  ECHO PASSED
  Exit /b 0
)";
    static string shTemplate= @"#!/bin/bash
ulimit -n 2048
scriptPath=$(dirname $0)

pushd $scriptPath
scriptFullPath=`pwd`
testsRootPath=$scriptFullPath/../..
popd

pushd {1}
$CORE_ROOT/corerun xunit.console.netcore.exe DebuggerTest.dll -method {0} -xml $scriptFullPath/{0}.log

# PostCommands
CLRTestExitCode=$?
CLRTestExpectedExitCode=0

popd
echo Expected: $CLRTestExpectedExitCode
echo Actual: $CLRTestExitCode
if [ $CLRTestExitCode -ne $CLRTestExpectedExitCode ]
then
  echo END EXECUTION - FAILED
  exit 1
else
  echo END EXECUTION - PASSED
  exit 0
fi";

        public static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Usage: {0} <tests root path> <tests library path>", Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name));
                return;
            }
           
            string DeployPath = Path.Combine(Path.GetFullPath(args[0]), "tests");
            string dotnetTestsPath = Path.GetFullPath(args[1]);
            AssemblyName an = new AssemblyName("DebuggerTest, Version=999.999.999.999, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Assembly assembly = null;
            try
            {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(dotnetTestsPath, "DebuggerTest.dll"));
                //assembly = Assembly.Load(an);
            }
            catch (System.IO.FileNotFoundException)
            {
                string curAssemblyDir = Path.GetDirectoryName(new System.Uri(Assembly.GetEntryAssembly().CodeBase).AbsolutePath);
                Console.WriteLine("Error: can't find DebuggerTest.dll. Please put DebuggerTest.dll to {0}", curAssemblyDir);
                return;
            }
           
            var methods = assembly.GetTypes().SelectMany(t => t.GetMethods());
            foreach (var item in methods)
            {
                TheoryAttribute att = (TheoryAttribute)item.GetCustomAttribute(typeof(TheoryAttribute));
                if(att != null && att.Skip == null)
                {
                    string xunitTestMethodName = item.DeclaringType.FullName + '.' + item.Name;
                    string destDir = Path.Combine(DeployPath, xunitTestMethodName);
                    Console.WriteLine(destDir);
                    if(!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    
                    File.WriteAllText(Path.Combine(destDir, xunitTestMethodName+".cmd"), string.Format(cmdTemplate, xunitTestMethodName, dotnetTestsPath));
                    File.WriteAllText(Path.Combine(destDir, xunitTestMethodName+".sh"), string.Format(shTemplate, xunitTestMethodName, dotnetTestsPath));
                }
            }
        }
    }
}
