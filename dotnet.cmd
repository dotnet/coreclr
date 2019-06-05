@if not defined _echo @echo off
setlocal

set "__ProjectDir=%~dp0"

:: Clear the 'Platform' env variable for this session, as it's a per-project setting within the build, and
:: misleading value (such as 'MCD' in HP PCs) may lead to build breakage (issue: #69).
set Platform=
set __ProjectDir=
set __DotnetDir=

:: Don't resolve runtime, shared framework, or SDK from other locations to ensure build determinism
set DOTNET_MULTILEVEL_LOOKUP=0

:: Disable first run since we do not need all ASP.NET packages restored.
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

:: Install dotnet
call %~dp0\init-dotnet.cmd
if NOT [%ERRORLEVEL%]==[0] (
  exit /b %ERRORLEVEL%
)

if "%DOTNET_TOOL_DIR%"=="" (
  set __DotnetDir=%~dp0
) else (
  set __DotnetDir=%DOTNET_TOOL_DIR%
)

pushd %~dp0
echo Running: dotnet %*
call "%__DotnetDir%\dotnet.exe" %*
popd

if NOT [%ERRORLEVEL%]==[0] (
  exit /b 1
)

exit /b 0
