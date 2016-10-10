
# Using CoreRun To Run .NET Core Application

In page [Using Your Build](UsingYourBuild.md) gives detailed instructions on using the standard
command line host and SDK, dotnet.exe to run a .NET application with the modified build of the
.NET Core runtime built here.   This is the preferred mechanism for you to officially deploy 
your changes to other people since dotnet.exe and Nuget insure that you end up with a consistent
set of DLLs that can work together.  

However packing and unpacking the runtime DLLs adds extra steps to the deployment process and when 
you are in the tight code-build-debug loop these extra steps are an issue.   

For this situation there is an alternative host to dotnet.exe called coreRun.exe that is well suited
for this.   It does not know about Nuget at all, and has very simple rules.  It needs to find the
.NET Core runtnime (that is coreclr.dll) and additionally any class library DLLs (e.g. System.Runtime.dll  System.IO.dll ...).

It does this by looking at two environment variables.   


 * CORE_ROOT - The directory where to find the runtime DLLs itself (e.g. CoreCLR.dll).   
 Defaults to be next to the coreRun.exe host itself.  
 * CORE_LIBRARIES - A Semicolon separated list of directories to look for DLLS to resolve any assembly references. 
 It defaults CORE_ROOT if it is not specified.  

These simple rules can be used in a number of ways 

## Getting the class library from the shared system-wide runtime  

Consider that you already have a .NET applciation DLL called HelloWorld.dll and wish to run it 
(You could make such a DLL by using 'dotnet new' 'dotnet restore' 'dotnet build' in a 'HelloWorld' directory).

If you execute the following
```bat
    set PATH=%PATH%;%CoreCLR%\bin\Product\Windows_NT.x64.Debug
    set CORE_LIBRARIES=%ProgramFiles%\dotnet\shared\Microsoft.NETCore.App\1.0.0
    

    coreRun HelloWorld.dll
```

Where %CoreCLR% is the base of your CoreCLR repository, then it will run your HelloWorld. application.
You can see why this works.  The first line puts build output directory (Your OS, architecture, and buildType
may be different) and thus CoreRun.exe you just built is on your path. 
The second line tells CoreRun.exe where to find class library files, in this case we tell it
to find them where the installation of dotnet.exe placed its copy.   (Note that version number in the path above may change)

Thus when you run 'coreRun HelloWorld.dll' CoreRun knows where to get the DLLs it needs.   Notice that once
you set up the path and CORE_LIBRARIES environment, after a rebuild you can simply use CoreRun to run your
application (you don't have to move DLLs around)

## Using CoreRun.exe to Execute a Published  Application

When 'dotnet published' publishes an application it deploys all the class libraries needed as well.
Thus if you simply change the CORE_LIBRARIES definition in the previous instructions to point at 
that publication directory but RUN the coreRun from your build output the effect will be that you
run your new runtime getting all the other code needed from that deployed application.   This is 
very convenient because you don't need to modify the deployed application in order to test our 
your new runtime.  

## How CoreCLR Tests use coreRun.exe

When you execute 'tests\runTest.cmd' one of the things that it does is set up a directory where it 
gathers the CoreCLR that has just been built with the pieces of the class library that tests need.
It places this runtime in the directory
```bat
    bin\Product\<OS>.<Arch>.<BuildType>\test
```
off the CoreCLR Repository.    The way the tests are expected to work is that you set the environment 
variable CORE_ROOT to this directory
(you don't have to set CORE_LIBRARIES) and you can run any tests.  For example after building the tests
(running build-test at the repository base) and running 'test\runtest') you can do the following

```bat
    set PATH=%PATH%;%CoreCLR%\bin\Product\Windows_NT.x64.Debug
    set CORE_ROOT=%CoreCLR%\bin\tests\Windows_NT.x64.Debug\Tests\Core_Root 
```
sets you up so that coreRun can run any of the test.   For example
```bat
    coreRun bin\tests\Windows_NT.X64.Debug\GC\Features\Finaliser\finalizerio\finalizerio\finalizerio.exe 
```
runs the finalizerio test.  