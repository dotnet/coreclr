@echo off
powershell -ExecutionPolicy ByPass -NoProfile %~dp0eng\build.ps1 -CI -InitTools
echo Build.cmd ErrorLevel=%ERRORLEVEL%
exit /b %ERRORLEVEL%
