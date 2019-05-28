@if not defined _echo @echo off
setlocal EnableDelayedExpansion EnableExtensions

:: Define a prefix for most output progress messages that come from this script. That makes
:: it easier to see where these are coming from. Note that there is a trailing space here.
set "__MsgPrefix=BUILDTOOLS: "

echo %__MsgPrefix%Starting Build at %TIME%

set __ThisScriptDir="%~dp0"

call "%__ThisScriptDir%"\setup_vs_tools.cmd
if NOT '%ERRORLEVEL%' == '0' exit /b 1

if defined VS160COMNTOOLS (
    set "__VSToolsRoot=%VS160COMNTOOLS%"
    set "__VCToolsRoot=%VS160COMNTOOLS%\..\..\VC\Auxiliary\Build"
    set __VSVersion=vs2019
) else if defined VS150COMNTOOLS (
    set "__VSToolsRoot=%VS150COMNTOOLS%"
    set "__VCToolsRoot=%VS150COMNTOOLS%\..\..\VC\Auxiliary\Build"
    set __VSVersion=vs2017
)

:: Set the default arguments for build
set __BuildArch=x64
set __BuildType=Debug
set __BuildOS=Windows_NT

set "__ProjectDir=%~dp0"
:: remove trailing slash
if %__ProjectDir:~-1%==\ set "__ProjectDir=%__ProjectDir:~0,-1%"
set "__SourceDir=%__ProjectDir%\src"
set "__PackagesDir=%__ProjectDir%\.packages"
set "__RootBinDir=%__ProjectDir%\bin"
set "__LogsDir=%__RootBinDir%\Logs"
set "__MsbuildDebugLogsDir=%__LogsDir%\MsbuildDebugLogs"

REM __UnprocessedBuildArgs are args that we pass to msbuild (e.g. /p:__BuildArch=x64)
set "__args= %*"
set processedArgs=
set __UnprocessedBuildArgs=
set __CommonMSBuildArgs=

set __SkipRestorePackages=

@REM CMD has a nasty habit of eating "=" on the argument list, so passing:
@REM    -priority=1
@REM appears to CMD parsing as "-priority 1". Handle -priority specially to avoid problems,
@REM and allow the "-priority=1" syntax.
set __Priority=0
set __PriorityArg=

:Arg_Loop
if "%1" == "" goto ArgsDone

if /i "%1" == "/?"     goto Usage
if /i "%1" == "-?"     goto Usage
if /i "%1" == "/h"     goto Usage
if /i "%1" == "-h"     goto Usage
if /i "%1" == "/help"  goto Usage
if /i "%1" == "-help"  goto Usage
if /i "%1" == "--help" goto Usage

if /i "%1" == "x64"                   (set __BuildArch=x64&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "x86"                   (set __BuildArch=x86&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "arm"                   (set __BuildArch=arm&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "arm64"                 (set __BuildArch=arm64&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)

if /i "%1" == "debug"                 (set __BuildType=Debug&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "release"               (set __BuildType=Release&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "checked"               (set __BuildType=Checked&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)

if /i "%1" == "skiprestorepackages"   (set __SkipRestorePackages=1&set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)
if /i "%1" == "--"                    (set processedArgs=!processedArgs! %1&shift&goto Arg_Loop)

if [!processedArgs!]==[] (
    set __UnprocessedBuildArgs=%__args%
) else (
    set __UnprocessedBuildArgs=%__args%
    for %%t in (!processedArgs!) do (
        set __UnprocessedBuildArgs=!__UnprocessedBuildArgs:*%%t=!
    )
)

:ArgsDone

@if defined _echo @echo on

set __CommonMSBuildArgs=/p:__BuildOS=%__BuildOS% /p:__BuildType=%__BuildType% /p:__BuildArch=%__BuildArch%
REM As we move from buildtools to arcade, __RunArgs should be replaced with __msbuildArgs
set __msbuildArgs=/p:__BuildOS=%__BuildOS% /p:__BuildType=%__BuildType% /p:__BuildArch=%__BuildArch% /nologo /verbosity:minimal /clp:Summary /maxcpucount

echo %__MsgPrefix%Commencing CoreCLR tools build

set "__BinDir=%__RootBinDir%\Product\%__BuildOS%.%__BuildArch%.%__BuildType%"
set "__ToolsRootDir=%__RootBinDir%\tools"
set "__ToolsBinDir=%__ToolsRootDir%\%__BuildOS%.%__BuildArch%.%__BuildType%"
set "__ToolsIntermediateDir=%__ToolsRootDir%\obj\%__BuildOS%.%__BuildArch%.%__BuildType%"

if not exist "%__ToolsBinDir%"                  md "%__ToolsBinDir%"
if not exist "%__ToolsIntermediateDir%"         md "%__ToolsIntermediateDir%"
if not exist "%__LogsDir%"                      md "%__LogsDir%"

echo %__MsgPrefix%Checking prerequisites

REM =========================================================================================
REM ===
REM === Restore Build Tools
REM ===
REM =========================================================================================

call "%__ProjectDir%\init-tools.cmd"
@if defined _echo @echo on

set "__ToolsDir=%__ProjectDir%\Tools"

REM =========================================================================================
REM ===
REM === Restore product binaries from packages
REM ===
REM =========================================================================================

if "%__SkipRestorePackages%" == 1 goto SkipRestoreProduct

echo %__MsgPrefix%Restoring CoreCLR product from packages

set __BuildLogRootName=Restore_Product
set __BuildLog=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.log
set __BuildWrn=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.wrn
set __BuildErr=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.err
set __MsbuildLog=/flp:Verbosity=normal;LogFile="%__BuildLog%"
set __MsbuildWrn=/flp1:WarningsOnly;LogFile="%__BuildWrn%"
set __MsbuildErr=/flp2:ErrorsOnly;LogFile="%__BuildErr%"
set __Logging=!__MsbuildLog! !__MsbuildWrn! !__MsbuildErr!

call "%__ProjectDir%\dotnet.cmd" msbuild /nologo /verbosity:minimal /clp:Summary /nodeReuse:false^
  /l:BinClashLogger,Tools/Microsoft.DotNet.Build.Tasks.dll;LogFile=binclash.log^
  /p:RestoreDefaultOptimizationDataPackage=false /p:PortableBuild=true^
  /p:UsePartialNGENOptimization=false /maxcpucount^
  %__ProjectDir%\src\tools\build.proj /t:BatchRestorePackages^
  !__Logging! %__CommonMSBuildArgs% %__PriorityArg% %__UnprocessedBuildArgs%

:SkipRestoreProduct

REM =========================================================================================
REM ===
REM === Build the tools
REM ===
REM =========================================================================================

echo %__MsgPrefix%Starting the Managed Tools Build

if not defined VSINSTALLDIR (
    echo %__MsgPrefix%Error: build-tools.cmd should be run from a Visual Studio Command Prompt.  Please see https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/developer-guide.md for build instructions.
    exit /b 1
)
set __AppendToLog=false
set __BuildLogRootName=Tools_Managed
set __BuildLog=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.log
set __BuildWrn=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.wrn
set __BuildErr=%__LogsDir%\%__BuildLogRootName%_%__BuildOS%__%__BuildArch%__%__BuildType%.err

REM Execute msbuild test build in stages - workaround for excessive data retention in MSBuild ConfigCache
REM See https://github.com/Microsoft/msbuild/issues/2993

set __SkipPackageRestore=false
set __SkipTargetingPackBuild=false

set __MsbuildLog=/flp:Verbosity=normal;LogFile="%__BuildLog%";Append=!__AppendToLog!
set __MsbuildWrn=/flp1:WarningsOnly;LogFile="%__BuildWrn%";Append=!__AppendToLog!
set __MsbuildErr=/flp2:ErrorsOnly;LogFile="%__BuildErr%";Append=!__AppendToLog!

echo Running: msbuild %__ProjectDir%\tests\build.proj !__MsbuildLog! !__MsbuildWrn! !__MsbuildErr! %__msbuildArgs% !__PriorityArg! %__UnprocessedBuildArgs%

call "%__ProjectDir%\dotnet.cmd" msbuild %__ProjectDir%\src\tools\build.proj !__MsbuildLog! !__MsbuildWrn! !__MsbuildErr! %__msbuildArgs% !__PriorityArg! %__UnprocessedBuildArgs%

if errorlevel 1 (
    echo %__MsgPrefix%Error: build failed. Refer to the build log files for details:
    echo     %__BuildLog%
    echo     %__BuildWrn%
    echo     %__BuildErr%
    REM This is necessary because of a(n apparent) bug in the FOR /L command.  Under certain circumstances,
    REM such as when this script is invoke with CMD /C "build-test.cmd", a non-zero exit directly from
    REM within the loop body will not propagate to the caller.  For some reason, goto works around it.
    goto     :Exit_Failure
)

REM =========================================================================================
REM ===
REM === All builds complete!
REM ===
REM =========================================================================================

echo %__MsgPrefix%Tools build succeeded.  Finished at %TIME%
echo %__MsgPrefix%Tools binaries are available at !__ToolsBinDir!
exit /b 0

:Usage
echo.
echo Build the CoreCLR tools.
echo.
echo Usage:
echo     %0 [option1] [option2] ...
echo All arguments are optional. Options are case-insensitive. The options are:
echo.
echo.-? -h -help --help: view this message.
echo Build architecture: one of x64, x86, arm, arm64 ^(default: x64^).
echo Build type: one of Debug, Checked, Release ^(default: Debug^).
echo skiprestorepackages: skip package restore
echo targetsNonWindows:
echo -- ... : all arguments following this tag will be passed directly to msbuild.
echo -verbose: enables detailed file logging for the msbuild tasks into the msbuild log file.
exit /b 1

:NoDIA
echo Error: DIA SDK is missing at "%VSINSTALLDIR%DIA SDK". ^
This is due to a bug in the Visual Studio installer. It does not install DIA SDK at "%VSINSTALLDIR%" but rather ^
at the install location of previous Visual Studio version. The workaround is to copy the DIA SDK folder from the Visual Studio install location ^
of the previous version to "%VSINSTALLDIR%" and then build.
REM DIA SDK not included in Express editions
echo Visual Studio Express does not include the DIA SDK. ^
You need Visual Studio 2017 or 2019 (Community is free).
echo See: https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/developer-guide.md#prerequisites
exit /b 1

:Exit_Failure
exit /b 1
