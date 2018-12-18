@if not defined _echo @echo off
setlocal EnableDelayedExpansion

set "__args=%*"
set processedArgs=
set unprocessedArgs=
set __MSBuildArgs=

REM If no processed arguments are specified, -p is implied.
if "%1" == ""      (set __MSBuildArgs=!__MSBuildArgs! .\build.proj /p:RestoreDuringBuild=true /t:Sync)
if /i [%1] == [--] (set __MSBuildArgs=!__MSBuildArgs! .\build.proj /p:RestoreDuringBuild=true /t:Sync)

:Arg_Loop
if "%1" == "" goto ArgsDone
if /I [%1] == [-?] goto Usage
if /I [%1] == [-help] goto Usage

if /i [%1] == [-p]               (set processedArgs=!processedArgs! %1&set __MSBuildArgs=!__MSBuildArgs! .\build.proj /p:RestoreDuringBuild=true /t:Sync&shift&goto Arg_Loop)
if /i [%1] == [-ab]              (set processedArgs=!processedArgs! %1&set __MSBuildArgs=!__MSBuildArgs! .\src\syncAzure.proj&shift&goto Arg_Loop)

REM CMD eats "=" on the argument list.
if /i "%1" == "-AzureAccount"    (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:CloudDropAccountName=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-AzureToken"      (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:CloudDropAccessToken=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-BuildMajor"      (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:BuildNumberMajor=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-BuildMinor"      (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:BuildNumberMinor=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-Container"       (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:ContainerName=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-BlobNamePrefix"  (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:__BlobNamePrefix=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-RuntimeId"       (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:RuntimeId=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "--"               (set processedArgs=!processedArgs! %1&shift)

REM handle any unprocessed arguments, assumed to go only after the processed arguments above
if [!processedArgs!]==[] (
   set unprocessedArgs=%__args%
) else (
   set unprocessedArgs=%__args%
   for %%t in (!processedArgs!) do (
   REM strip out already-processed arguments from unprocessedArgs
   set unprocessedArgs=!unprocessedArgs:*%%t=!
   )
)

:ArgsDone


@call %~dp0msbuild.cmd /nologo /verbosity:minimal /clp:Summary /nodeReuse:false /flp:v=detailed;LogFile=sync.log %__MSBuildArgs% %unprocessedArgs%
@exit /b %ERRORLEVEL%

:Usage
echo.
echo Repository syncing script.
echo.
echo Options:
echo     -?     - Prints Usage
echo     -help  - Prints Usage
echo     -p     - Restores all nuget packages for repository
echo     -ab    - Downloads the latests product packages from Azure.
echo              The following properties are required:
echo                 -AzureAccount="Account name"
echo                 -AzureToken="Access token"
echo              To download a specific group of product packages, specify:
echo                 -BuildMajor
echo                 -BuildMinor
echo              To download from a specific container, specify:
echo                 -Container="container name"
echo              To download blobs starting with a specific prefix, specify:
echo                 -BlobNamePrefix="Blob name prefix"
echo              To specify which RID you are downloading binaries for (optional):
echo                 -RuntimeId="RID" (Needs to match what's in the container)
echo.
echo.
echo.
echo If no option is specified then sync.cmd -p is implied.