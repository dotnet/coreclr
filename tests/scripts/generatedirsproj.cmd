@echo off

if not defined VS150COMNTOOLS (
    echo This script has to be run from Visual Studio 2017 developer command prompt
    exit /b 1
)

csi -r:"%VS150COMNTOOLS%..\..\MSBuild\15.0\Bin\Microsoft.Build.dll" -r:"%VS150COMNTOOLS%..\..\MSBuild\15.0\Bin\Microsoft.Build.Framework.dll" -r:"%VS150COMNTOOLS%..\..\MSBuild\15.0\Bin\Microsoft.Build.Tasks.Core.dll" -r:"%VS150COMNTOOLS%..\..\MSBuild\15.0\Bin\Microsoft.Build.Utilities.Core.dll" generatedirsproj.csx
exit /b %ERRORLEVEL%
