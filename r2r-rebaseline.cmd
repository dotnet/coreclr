"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" build /p:__BuildArch=x64 /p:__BuildOS=Windows_NT /p:__BuildType=Checked src\tools\r2rdump\R2RDump.csproj

bin\tests\Windows_NT.x64.Checked\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x64.Checked\Tests\Core_Root /out HelloWorld.ni.dll bin\tests\Windows_NT.x64.Checked\readytorun\r2rdump\R2RDumpTest\HelloWorld.dll
bin\tests\Windows_NT.x64.Checked\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x64.Checked\Tests\Core_Root /out GcInfoTransitions.ni.dll bin\tests\Windows_NT.x64.Checked\readytorun\r2rdump\R2RDumpTest\GcInfoTransitions.dll
bin\tests\Windows_NT.x64.Checked\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x64.Checked\Tests\Core_Root /out GenericFunctions.ni.dll bin\tests\Windows_NT.x64.Checked\readytorun\r2rdump\R2RDumpTest\GenericFunctions.dll
bin\tests\Windows_NT.x64.Checked\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x64.Checked\Tests\Core_Root /out MultipleRuntimeFunctions.ni.dll bin\tests\Windows_NT.x64.Checked\readytorun\r2rdump\R2RDumpTest\MultipleRuntimeFunctions.dll

"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x64.Checked\netcoreapp2.0\R2RDump.dll --in HelloWorld.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x64.Checked\HelloWorld.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x64.Checked\netcoreapp2.0\R2RDump.dll --in GcInfoTransitions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x64.Checked\GcInfoTransitions.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x64.Checked\netcoreapp2.0\R2RDump.dll --in GenericFunctions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x64.Checked\GenericFunctions.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x64.Checked\netcoreapp2.0\R2RDump.dll --in MultipleRuntimeFunctions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x64.Checked\MultipleRuntimeFunctions.xml -x -v

"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" build /p:__BuildArch=x86 /p:__BuildOS=Windows_NT /p:__BuildType=Release src\tools\r2rdump\R2RDump.csproj

bin\tests\Windows_NT.x86.Release\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x86.Release\Tests\Core_Root /out HelloWorld.ni.dll bin\tests\Windows_NT.x86.Release\readytorun\r2rdump\R2RDumpTest\HelloWorld.dll
bin\tests\Windows_NT.x86.Release\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x86.Release\Tests\Core_Root /out GcInfoTransitions.ni.dll bin\tests\Windows_NT.x86.Release\readytorun\r2rdump\R2RDumpTest\GcInfoTransitions.dll
bin\tests\Windows_NT.x86.Release\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x86.Release\Tests\Core_Root /out GenericFunctions.ni.dll bin\tests\Windows_NT.x86.Release\readytorun\r2rdump\R2RDumpTest\GenericFunctions.dll
bin\tests\Windows_NT.x86.Release\Tests\Core_Root\crossgen /readytorun /platform_assemblies_paths bin\tests\Windows_NT.x86.Release\Tests\Core_Root /out MultipleRuntimeFunctions.ni.dll bin\tests\Windows_NT.x86.Release\readytorun\r2rdump\R2RDumpTest\MultipleRuntimeFunctions.dll

"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x86.Release\netcoreapp2.0\R2RDump.dll --in HelloWorld.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x86.Release\HelloWorld.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x86.Release\netcoreapp2.0\R2RDump.dll --in GcInfoTransitions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x86.Release\GcInfoTransitions.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x86.Release\netcoreapp2.0\R2RDump.dll --in GenericFunctions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x86.Release\GenericFunctions.xml -x -v
"C:\Repos\coreclr\Tools\dotnetcli\dotnet.exe" C:\Repos\coreclr\bin\Product\Windows_NT.x86.Release\netcoreapp2.0\R2RDump.dll --in MultipleRuntimeFunctions.ni.dll --out tests\src\readytorun\r2rdump\files\Windows_NT.x86.Release\MultipleRuntimeFunctions.xml -x -v

COPY /Y tests\src\readytorun\r2rdump\files\Windows_NT.x86.Release\*.xml tests\src\readytorun\r2rdump\files\Windows_NT.x86.Checked
