@echo off
powershell -ExecutionPolicy ByPass -NoProfile %~dp0common\msbuild.ps1 %*
echo msbuild.cmd ErrorLevel=%ERRORLEVEL%
exit /b %ERRORLEVEL%
