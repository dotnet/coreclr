Using EventPipe
===============

# Capabilities #
EventPipe provides the ability to collect traces using an in-process event logger instead of depending on a platform-level logger.  The following data can be collected:

 - Sampled thread-time traces (Samples of every thread collected on a recurring basis at the specified (or default) rate)
 - Runtime events (Events emitted by the native components of the runtime - e.g. GC, JIT, Interop, etc.)
 - EventSource events
 - Managed call stacks for all events.

# Collection Examples #

## Collect From Process Start to Process End with Default Collection Parameters ##

This is the minimal set of steps required to collect a trace from process start to process end.  This will collect CPU/blocked time sample events and a mixture of runtime events (GC, JIT, Exceptions, etc.)

1. Tell the runtime to start tracing at process start.

    > ```cmd
    > export COMPlus_EnableEventPipe=1
    > ```
2. Start the process.

3. On process shutdown, the trace file will be written to CurrentWorkingDirectory/Process-$pid.netperf.

## Start/Stop Collection at Any Time with Default Collection Parameters ##

This will collect CPU/blocked time sample events and a mixture of runtime events (GC, JIT, Exceptions, etc.)  The configuration must be specified before the process starts, but has no performance impact until the control file is created.

1. Tell the runtime to listen for enable/disable commands:

    > ```cmd
    > export COMPlus_EnableEventPipe=4
    > ```

2. Specify the output file path:

    This is optional.  If not specified it will be CurrentWorkingDirectory/Process-$pid.netperf.

    > ```cmd
    > export COMPlus_EventPipeOutputFile=/path/to/trace.netperf
    > ```

3. Run the application.

4. Start collection by creating an empty control file.  This tells the runtime to start collection:

    > ```cmd
    > touch /path/to/trace.netperf.ctl.
    > ```

    If the output file path wasn't specified in step 2 then the control file is CurrentWorkingDirectory/Process-$pid.netperf.ctl.

    NOTE: The runtime polls every 10 seconds, so it can take up to 10 seconds for tracing to start.  You'll see the netperf file get created when tracing has started.

5. Reproduce the behavior you want to capture.

6. Stop collection by deleting the control file.  This tells the runtime to stop collection:

    > ```cmd
    > rm /path/to/trace.netperf.ctl
    > ```

This produces /path/to/trace.netperf which can be consumed using [TraceEvent and PerfView](http://github.com/microsoft/perfview).

## Collect a Heap Dump ##

1. Tell the runtime to listen for enable/disable commands:

    > ```cmd
    > export COMPlus_EnableEventPipe=4
    > ```

2. Specify the output file path:

    > ```cmd
    > export COMPlus_EventPipeOutputFile=/path/to/trace.netperf
    > ```

3. Specify the following collection configuration:

    > ```cmd
    > export COMPlus_EventPipeConfig=Microsoft-Windows-DotNETRuntime:0x1D80001:4
    > ```

4. Run the application.

5. Start collection by creating an empty control file.  This tells the runtime to start collection:

    > ```cmd
    > touch /path/to/trace.netperf.ctl.
    > ```

    NOTE: The runtime polls every 10 seconds, so it can take up to 10 seconds for tracing to start.  You'll see the netperf file get created when tracing has started.

6. Reproduce the behavior you want to capture.

7. Stop collection by deleting the control file.  This tells the runtime to stop collection:

    > ```cmd
    > rm /path/to/trace.netperf.ctl
    > ```

This produces /path/to/trace.netperf which can be consumed using [TraceEvent and PerfView](http://github.com/microsoft/perfview).

# Control COMPlus Variables #

1. COMPlus_EnableEventPipe: Describes the desired state of EventPipe.
    > - 0: Disabled (Default)
    > - 1: Enabled
    > - 4: Initially disabled, but can be enabled via control file.

2. COMPlus_EventPipeConfig: Describes the set of providers/keywords/levels to be collected.  Uses xperf-style format: provider:keywords:level,provider:keywords:level...
    > - Optional.  If not specified, the default configuration is used.

3. COMPlus_EventPipeOutputFile: The full path to the netperf file to be written.
    > - Optional.  If not specified, /currentworkingdirectory/Process-$pid.netperf is used.

4. COMPlus_EventPipeCircularMB: The size of the circular buffer used when collecting events.
    > - Optional.  If not specified, 1 GB is used.

# Analyzing NetPerf Files #
NetPerf files can be analyzing using [TraceEvent and PerfView](http://github.com/microsoft/perfview).  Both TraceEvent and PerfView have native support for opening NetPerf files.

