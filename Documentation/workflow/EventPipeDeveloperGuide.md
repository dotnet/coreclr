
# Event Pipe Developer Guide

The goal of this guide is to give a good introduction for new contributors who would like to develop Event Pipe.

## What is Event Pipe?

It's new, cross-platform mechanism for fast event tracing in .NET Core. [Here](https://github.com/dotnet/designs/blob/master/accepted/cross-platform-performance-monitoring.md) you can read more about its design. 

The key things are:

* no OS-specific dependencies
* writer writes to a binary file, a reader reads directly from the file
* it's not a replacement, but rather an extension

## Where do I find the code?

The code is split into two repositories: [CoreCLR](https://github.com/dotnet/coreclr) and [PerfView](https://github.com/Microsoft/perfview). The core of Event Pipe is implemented in CoreCLR. The most important files are:

CoreCLR (the EventPipes are built-in feature of CoreCLR):

* [src/vm/eventpipe.cpp](../../src/vm/eventpipe.cpp)
* [src/vm/eventpipebuffer.cpp](../../src/vm/eventpipebuffer.cpp)
* [src/vm/eventpipebuffermanager.cpp](../../src/vm/eventpipebuffermanager.cpp)
* [src/vm/eventpipeconfiguration.cpp](../../src/vm/eventpipeconfiguration.cpp)
* [src/vm/eventpipeevent.cpp](../../src/vm/eventpipeevent.cpp)
* [src/vm/eventpipeeventinstance.cpp](../../src/vm/eventpipeeventinstance.cpp)
* [src/vm/eventpipefile.cpp](../../src/vm/eventpipefile.cpp) - creates and writes to Event Pipe file. It also writes the file header and maintains the forward references.
* [src/vm/eventpipejsonfile.cpp](../../src/vm/eventpipejsonfile.cpp) - allows to use JSON instead of binary file format. Used only for debugging purposes.
* [src/vm/eventpipeprovider.cpp](../../src/vm/eventpipeprovider.cpp)
* [src/vm/fastserializer.cpp](../../src/vm/fastserializer.cpp) - fast binary serializer used by `EventPipeFile` to store data
* [src/scripts/genEventPipe.py](../../src/scripts/genEventPipe.py) - Python script run at build time, generates all event definitions
* [tests/src/tracing/eventpipetrace/EventPipeTrace.cs](../../tests/src/tracing/eventpipetrace/EventPipeTrace.cs) - Tests. As of today (18th of January 2018) these testes are not run by the CoreCLR CI.

PerfView repository is the home for `Microsoft.Diagnostics.Tracing.TraceEvent` Library, which provide client classes that allow to consume Event Pipe:

* [src/TraceEvent/EventPipe/EventPipeEventSourceFactory.cs](https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/EventPipe/EventPipeEventSourceFactory.cs) - factory which parses the header of Event Pipe file and creates `EventPipeEventSource`
* [src/TraceEvent/EventPipe/EventPipeEventSource.cs](https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/EventPipe/EventPipeEventSource.cs) - processes all events information using `EventPipeTraceEventParser`
* [src/TraceEvent/EventPipe/EventPipeTraceEventParser.cs](https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/EventPipe/EventPipeTraceEventParser.cs) - parser responsible for the deserialization of data serialized by `FastSerializer` in CoreCLR
* [src/TraceEvent/TraceEvent.Tests/EventPipeParsing.cs](https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/TraceEvent.Tests/EventPipeParsing.cs) - tests

## How to build the code

### CoreCLR

[Here](https://github.com/dotnet/coreclr#building-the-repository) you can find out how to build CoreCLR. 

**Important:**  As of today (18th of January 2018) Event Pipe is disabled by default for Windows build of CoreCLR. To enable it, you need to set `FeaturePerfTracing` to `true` in the [clr.coreclr.props](../../clr.coreclr.props) file.

```xml
<FeaturePerfTracing>true</FeaturePerfTracing>
```

**Note:** The default CoreCLR build is `x64` `DEBUG` build. You need to pass `-x86` to the build script if you want to build the `x86` version. And `-release` if you want to have the `RELEASE` build.

**Hint:** Event Pipe is located only in the native part of CoreCLR. So to save some time you can always build it with `build skipTests skipmscorlib` arguments.

### Microsoft.Diagnostics.Tracing.TraceEvent

You just need to open the solution of PerfView and press "Ctrl+Shift+B". If you prefer console, then it's `dotnet build` executed in `PerfView/src/TraceEvent` folder.

## How to run the tests

### CoreCLR

1. Create runtime layout with all dependencies:

`tests\runtest.cmd x86 GenerateLayoutOnly`

which should result with sth like:

`RUNTEST: Created the runtime layout with all dependencies in c:\Projects\forks\coreclr\tests\..\bin\tests\Windows_NT.x86.Debug\Tests\Core_Root`

2. Set `CORE_ROOT` environment variable to the path from what you got in the previous point. 

`set CORE_ROOT="C:\Projects\forks\coreclr\bin\tests\Windows_NT.x86.Debug\Tests\Core_Root"`

3. Build the test project:

`msbuild.exe /p:Platform=x86 tests\src\tracing\eventpipetrace\eventpipetrace.csproj`

4. Change the directory to the output of test build:

`cd c:\Projects\forks\coreclr\bin\tests\Windows_NT.x86.Debug\tracing\eventpipetrace\eventpipetrace\`

5. Run the script, which runs the test executable with [CoreRun](../../Documentation/workflow/UsingCoreRun.md):

`eventpipetrace.cmd`

**Important**: The [test project file](../../tests/src/tracing/eventpipetrace/eventpipetrace.csproj) references `Microsoft.Diagnostics.Tracing.TraceEvent` which is used to run the test (to consume events from Event Pipe produced by CoreCLR).

In case your CoreCLR changes break the consumer library you need to update the version of `Microsoft.Diagnostics.Tracing.TraceEvent` as well. You can do this in the [dependencies.props](../../dependencies.props)

`<MicrosoftDiagnosticsTracingTraceEventPackageVersion>2.0.2</MicrosoftDiagnosticsTracingTraceEventPackageVersion>`

### Microsoft.Diagnostics.Tracing.TraceEvent

Just open the Test Window in Visual Studio and run the test or run from the console:

`dotnet test .\src\TraceEvent\TraceEvent.Tests\TraceEvent.Tests.csproj`

## How to add new test

When you run the test app from CoreCLR it generates a `.netperf` file. You can modify, rebuild and run this app to generate a `.netperf` file for you and use it as input data for the consumer library tests.

