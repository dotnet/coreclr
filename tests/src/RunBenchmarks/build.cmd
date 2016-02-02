rem *** Build for Desktop CLR

mkdir bin\Debug\desktop
csc /define:DESKTOP /nologo /debug /target:exe /out:bin\Debug\desktop\RunBenchmarks.exe RunBenchmarks.cs 

rem *** Build for Core CLR

dotnet restore
dotnet compile

