@if not defined _echo echo off

setlocal

    set "ERRORLEVEL="

    set "ARCHITECTURE=x64"
    REM set "ARCHITECTURE=x86"

    REM set "CONFIGURATIONS=Debug Checked Release"
    REM set "CONFIGURATIONS=Debug"
    REM set "CONFIGURATIONS=Checked"
    set "CONFIGURATIONS=Release"

    cls
    for %%c in (%CONFIGURATIONS%) do (
        call :run_cmd .\build.cmd %ARCHITECTURE% %%c skiptests skipbuildpackages                                                || exit /b 1
        REM Deprecated: .\run.cmd build -Project="tests\build.proj" -BuildOS=Windows_NT -BuildType=%%c -BuildArch=%ARCHITECTURE% -BatchRestorePackages
        call :run_cmd MSBuild.exe .\tests\build.proj /p:Configuration=%%c /p:Platform=%ARCHITECTURE% /t:BatchRestorePackages    || exit /b 1
        call :run_cmd .\tests\runtest.cmd %ARCHITECTURE% %%c generatelayoutonly                                                 || exit /b 1
    )

endlocal& exit /b 0


:run_cmd
    if "%~1" == "" exit /b 1

    echo/
    echo/-------------------------------------------------------------------------------
    echo/ %USERNAME%@%COMPUTERNAME% "%CD%"
    echo/ [%DATE% %TIME%] $ %*
    echo/-------------------------------------------------------------------------------
    echo/
    call %*
    exit /b %ERRORLEVEL%
