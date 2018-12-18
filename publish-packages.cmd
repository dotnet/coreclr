@if not defined _echo @echo off
setlocal EnableDelayedExpansion


set "__args=%*"
set processedArgs=
set unprocessedArgs=
set __MSBuildArgs=

:Arg_Loop
if "%1" == "" goto ArgsDone
if /I [%1] == [-?] goto Usage
if /I [%1] == [-help] goto Usage

REM CMD eats "=" on the argument list.
if /i "%1" == "-AzureAccount"     (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:CloudDropAccountName=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-AzureToken"       (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:CloudDropAccessToken=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-BuildArch"        (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:__BuildArch=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-BuildType"        (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:__BuildType=%2&shift&shift&goto Arg_Loop)
if /i "%1" == "-Container"        (set processedArgs=!processedArgs! %1=%2&set __MSBuildArgs=!__MSBuildArgs! /p:ContainerName=%2&shift&shift&goto Arg_Loop)

if /i "%1" == "-PublishPackages"  (set processedArgs=!processedArgs! %1&set __MSBuildArgs=!__MSBuildArgs! /p:__PublishPackages=true&shift&goto Arg_Loop)
if /i "%1" == "-PublishSymbols"   (set processedArgs=!processedArgs! %1&set __MSBuildArgs=!__MSBuildArgs! /p:__PublishSymbols=true&shift&goto Arg_Loop)

if /i "%1" == "--"                (set processedArgs=!processedArgs! %1&shift)

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

call %~dp0msbuild.cmd /nologo /verbosity:minimal /clp:Summary /nodeReuse:false /p:__BuildOS=Windows_NT .\src\publish.proj /flp:v=detailed;LogFile=publish-packages.log /clp:v=detailed %__MSBuildArgs% %unprocessedArgs%
@exit /b %ERRORLEVEL%

:Usage
echo.
echo Publishes the NuGet packages to the specified location.
echo   -?     - Prints Usage
echo   -help  - Prints Usage
echo For publishing to Azure the following properties are required.
echo   -AzureAccount="account name"
echo   -AzureToken="access token"
echo   -BuildType="Configuration"
echo   -BuildArch="Architecture"
echo For publishing to Azure, one of the following properties is required.
echo   -PublishPackages        Pass this switch to publish product packages 
echo   -PublishSymbols         Pass this switch to publish symbol packages
echo To specify the name of the container to publish into, use the following property:
echo   -Container="container name"
echo Architecture can be x64, x86, arm, or arm64
echo Configuration can be Release, Debug, or Checked
exit /b