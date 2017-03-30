@if not defined _echo @echo off
setlocal

set INIT_TOOLS_LOG=%~dp0init-tools.log
set PACKAGES_DIR=%~dp0packages\
set TOOLRUNTIME_DIR=%~dp0Tools
:: This is an isolated script that handles the download of dotnet binaries and shall be removed after bootstrap.ps1/sh refactor (dotnet/corefx#15427)
set DOTNET_DOWNLOAD_PATH=%~dp0dotnet-download.ps1
set DOTNET_PATH=%TOOLRUNTIME_DIR%\dotnetcli\
set DOTNET_CMD=%DOTNET_PATH%dotnet.exe
if [%BUILDTOOLS_SOURCE%]==[] set BUILDTOOLS_SOURCE=https://dotnet.myget.org/F/dotnet-buildtools/api/v3/index.json
set /P BUILDTOOLS_VERSION=< "%~dp0BuildToolsVersion.txt"
set BUILD_TOOLS_PATH=%PACKAGES_DIR%Microsoft.DotNet.BuildTools\%BUILDTOOLS_VERSION%\lib\
set PROJECT_JSON_PATH=%TOOLRUNTIME_DIR%\%BUILDTOOLS_VERSION%
set PROJECT_JSON_FILE=%PROJECT_JSON_PATH%\project.json
set PROJECT_JSON_CONTENTS={ "dependencies": { "Microsoft.DotNet.BuildTools": "%BUILDTOOLS_VERSION%" , "Microsoft.DotNet.BuildTools.Coreclr": "1.0.4-prerelease"}, "frameworks": { "dnxcore50": { } } }
set BUILD_TOOLS_SEMAPHORE=%PROJECT_JSON_PATH%\init-tools.completed0
set TOOLS_INIT_RETURN_CODE=0

:: if force option is specified then clean the tool runtime and build tools package directory to force it to get recreated
if [%1]==[force] (
  if exist "%TOOLRUNTIME_DIR%" rmdir /S /Q "%TOOLRUNTIME_DIR%"
  if exist "%PACKAGES_DIR%Microsoft.DotNet.BuildTools" rmdir /S /Q "%PACKAGES_DIR%Microsoft.DotNet.BuildTools"
)

:: If sempahore exists do nothing
if exist "%BUILD_TOOLS_SEMAPHORE%" (
  echo Tools are already initialized.
  goto :DONE
)

if exist "%TOOLRUNTIME_DIR%" rmdir /S /Q "%TOOLRUNTIME_DIR%"

:: Download Nuget.exe
if NOT exist "%PACKAGES_DIR%NuGet.exe" (
  if NOT exist "%PACKAGES_DIR%" mkdir "%PACKAGES_DIR%"
  powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://www.nuget.org/nuget.exe', '%PACKAGES_DIR%NuGet.exe')
)

if NOT exist "%PROJECT_JSON_PATH%" mkdir "%PROJECT_JSON_PATH%"
echo %PROJECT_JSON_CONTENTS% > "%PROJECT_JSON_FILE%"
echo Running %0 > "%INIT_TOOLS_LOG%"

set /p DOTNET_VERSION=< "%~dp0DotnetCLIVersion.txt"
if exist "%DOTNET_CMD%" goto :afterdotnetrestore

echo Installing dotnet cli...
if NOT exist "%DOTNET_PATH%" mkdir "%DOTNET_PATH%"
if [%PROCESSOR_ARCHITECTURE%]==[x86] (set DOTNET_ZIP_NAME=dotnet-dev-win-x86.%DOTNET_VERSION%.zip) else (set DOTNET_ZIP_NAME=dotnet-dev-win-x64.%DOTNET_VERSION%.zip)
set DOTNET_REMOTE_PATH=https://dotnetcli.blob.core.windows.net/dotnet/preview/Binaries/%DOTNET_VERSION%/%DOTNET_ZIP_NAME%
set DOTNET_LOCAL_PATH=%DOTNET_PATH%%DOTNET_ZIP_NAME%
echo Installing '%DOTNET_REMOTE_PATH%' to '%DOTNET_LOCAL_PATH%' >> "%INIT_TOOLS_LOG%"
powershell -NoProfile -ExecutionPolicy unrestricted -File %DOTNET_DOWNLOAD_PATH% -DotnetRemotePath %DOTNET_REMOTE_PATH% -DotnetLocalPath %DOTNET_LOCAL_PATH% -DotnetPath %DOTNET_PATH% >> "%INIT_TOOLS_LOG%"
if NOT exist "%DOTNET_LOCAL_PATH%" (
  echo ERROR: Could not install dotnet cli correctly. See '%INIT_TOOLS_LOG%' for more details.
  set TOOLS_INIT_RETURN_CODE=1
  goto :DONE
)

:afterdotnetrestore

if exist "%BUILD_TOOLS_PATH%" goto :afterbuildtoolsrestore
echo Restoring BuildTools version %BUILDTOOLS_VERSION%...
echo Running: "%DOTNET_CMD%" restore "%PROJECT_JSON_FILE%" --packages "%PACKAGES_DIR% " --source "%BUILDTOOLS_SOURCE%" >> "%INIT_TOOLS_LOG%"
call "%DOTNET_CMD%" restore "%PROJECT_JSON_FILE%" --packages "%PACKAGES_DIR% " --source "%BUILDTOOLS_SOURCE%" >> "%INIT_TOOLS_LOG%"
if NOT exist "%BUILD_TOOLS_PATH%init-tools.cmd" (
  echo ERROR: Could not restore build tools correctly. See '%INIT_TOOLS_LOG%' for more details.
  set TOOLS_INIT_RETURN_CODE=1
  goto :DONE
)

:afterbuildtoolsrestore

echo Initializing BuildTools ...
echo Running: "%BUILD_TOOLS_PATH%init-tools.cmd" "%~dp0" "%DOTNET_CMD%" "%TOOLRUNTIME_DIR%" >> "%INIT_TOOLS_LOG%"
call "%BUILD_TOOLS_PATH%init-tools.cmd" "%~dp0" "%DOTNET_CMD%" "%TOOLRUNTIME_DIR%" >> "%INIT_TOOLS_LOG%"

echo Updating CLI NuGet Frameworks map...
robocopy "%TOOLRUNTIME_DIR%" "%TOOLRUNTIME_DIR%\dotnetcli\sdk\%DOTNET_VERSION%" NuGet.Frameworks.dll /XO >> "%INIT_TOOLS_LOG%"
set UPDATE_CLI_ERRORLEVEL=%ERRORLEVEL%
if %UPDATE_CLI_ERRORLEVEL% GTR 1 (
  echo ERROR: Failed to update Nuget for CLI {Error level %UPDATE_CLI_ERRORLEVEL%}. Please check '%INIT_TOOLS_LOG%' for more details. 1>&2
  exit /b %UPDATE_CLI_ERRORLEVEL%
)

:: Create sempahore file
echo Done initializing tools.
echo Init-Tools.cmd completed for BuildTools Version: %BUILDTOOLS_VERSION% > "%BUILD_TOOLS_SEMAPHORE%"

:DONE

exit /b %TOOLS_INIT_RETURN_CODE%
