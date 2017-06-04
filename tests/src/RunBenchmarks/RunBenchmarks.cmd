setlocal

set BENCHMARK_ROOT_DIR=\git\repos\DotNet\coreclr\bin\tests\Windows_NT.x64.Release\JIT\Performance\CodeQuality
set BENCHMARK_HOME=\git\repos\DotNet\coreclr\tests\src\RunBenchmarks
set BENCHMARK_HOST=%BENCHMARK_HOME%\bin\Debug\dnxcore50\RunBenchmarks.exe
set BENCHMARK_HOST=%BENCHMARK_HOME%\bin\Debug\desktop\RunBenchmarks.exe
set BENCHMARK_RUNNER=-runner CoreRun.exe

start DHandler.exe

set BENCHMARK_CONTROLS=-v -w -n 5
set BENCHMARK_CONTROLS=-v -n 1 -s Bytemark
set BENCHMARK_SET=-f %BENCHMARK_HOME%\coreclr_benchmarks.xml -notags broken
set BENCHMARK_SWITCHES=%BENCHMARK_CONTROLS% -r %BENCHMARK_ROOT_DIR%

%BENCHMARK_HOST% %BENCHMARK_RUNNER% %BENCHMARK_SET% %BENCHMARK_SWITCHES%

taskkill /im DHandler.exe

endlocal
