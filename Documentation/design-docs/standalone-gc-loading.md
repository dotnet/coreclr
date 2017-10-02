# Standalone GC Loader Design

Author: Sean Gillespie (@swgillespie) - 2017

A standalone GC (or "local GC", as it is often called) initiaive has
been in motion for the last few months and is nearing its end stages,
where prototypes have been successfully completed and productization
has begun.

This document aims to provide a specification for how a standalone GC is
to be loaded and what is to happen in the case of version mismatches.

## Definitions

Before diving in to the specification, it's useful to precisely define
some terms that will be used often in this document.

* The **Execution Engine**, or **EE** - The component of the CLR responsible for *executing* programs.
  This is an intentionally vague definition. The GC does not care how (or even *if*) programs are
  compiled or executed, so it is up to the EE to invoke the GC whenever an executing
  program does something that requires the GC's attention. The EE is notable because the implementation
  of an execution engine varies widely between runtimes; the CoreRT EE is primarily in managed code
  (C#), while CoreCLR (and the .NET Framework)'s EE is primarily in C++.
* The **GC**, or **Garbage Collector** - The component of the CLR responsible for allocating managed
  objects and reclaming unused memory. It is written in C++ and the code is shared by multiple runtimes.
  (That is, CoreCLR/CoreRT may have different execution engines, but they share the *same* GC code.)
* The **DAC**, or **Data Access Component** - A subset of the execution engine that is compiled in
  such a way that it can be run *out of process*, when debugging .NET code using a debugger. The DAC
  is used by higher-level components such as SOS (Son of Strike, a windbg/lldb debugger extension for
  debugging managed code) and the DBI (a COM interface). The full details about the DAC are covered in
  [this document](https://github.com/dotnet/coreclr/blob/master/Documentation/botr/dac-notes.md).

## Rationale and Goals of a Standalone GC

A GC that is "standalone" is one that is able to be built as a dynamic shared library and loaded
dynamically at startup. This definition is useful because it provides a number of benefits
to the codebase:

* A standalone GC that can be loaded dynamically at runtime can also be *substituted* easily by
  loading any GC-containing dynamic shared library specified by a user. This is especially interesting
  for prototyping and testing GC changes since it would be possible to make changes to the GC
  without having to re-compile the runtime.
* A standalone GC that can be *built* as a dynamic shared library imposes a strong requirement that
  the interfaces that the GC uses to interact with other runtime components be complete and
  correct. A standalone GC will not link successfully if it refers to symbols defined within
  the EE. This makes the GC codebase significantly easier to share between different execution
  engine implementations; as long as the GC implements its side of the interface and the EE
  implements its side of the interface, we can expect that changes within the GC itself
  will be trivially portable to other runtime implementations.

  This is a significant engineering pain point today. A great deal of time is spent manually porting
  changes between the different branches of the GC codebase. When changes are not ported, the GC
  codebases diverge, which makes future ports harder and less likely to get ported.

Worth noting is that the JIT (both RyuJIT and the legacy JIT(s) before it) can be built standalone
and have realized these same benefits. The existence of an interface and an implementation loadable
from shared libraries has enabled RyuJIT in particular to be used as the code generator for both the
CoreRT compiler and crossgen, while still being flexible enough to be tested using tools that implement
very non-standard execution engines such as [SuperPMI](https://github.com/dotnet/coreclr/blob/master/src/ToolBox/superpmi/readme.txt).

The below loading protocol is inspired directly by the JIT loader and many aspects of the GC loader are identical
to what the JIT does when loading dynamic shared libraries.

## Loading a Standalone GC

Given that it is possible to create a GC that resides in a dynamic shared library, it is important
that the runtime have a protocol for locating and loading such GCs. The JIT is capable of being loaded
in this manner and, because of this, a significant amount of prior art exists for loading components
for shared libraries from the file system. This specification is based heavily on the ways that a
standalone JIT can be loaded.

Fundamentally, the algorithm for loading a standalone GC consists of these steps:

0. Identify whether or not we should be using a standalone GC at all.
1. Identify *where* we should be looking for GC shared libraries.
2. Identify *what* we are looking for - the specific name of a dynamic shared library that
   will be loaded.
3. Load the dynamic shared library and ask it to identify itself (name and version).
4. Check that the version numbers are compatible.
5. If so, initialize the GC and continue on with EE startup. If not, reject the dynamic shared library
   and raise an appropriate user-visible error.

The algorithm for initializing the DAC against a target process using a standalone GC consists of these steps:

1. Identify whether or not the target process is using a standalone GC at all. If not, no further
   checks are necessary.
2. If so, inspect the version number of the standalone GC in the target process and determine whether
   or not the DAC is compatible with that version. If not, present a notification of some kind
   that the debugging experience will be degraded.
3. Continue onwards.

Each one of these steps will be explained in detail below.

### Identifying candidate shared libraries

The question of whether or not the EE should attempt to locate and load a standalone GC
is answered by the EE's configuration system (`EEConfig`). EEConfig has the ability to
query configuration information from a variety of sources, such a registry keys or
environment variables. Using this subsystem, users can specify a specific environment
variable to indicate that they are interested in loading a standalone GC.

There are three configuration keys that control the behavior of loading a standalone GC:

1. `UseStandaloneGC`, (environment variable `COMPLUS_UseStandaloneGC`). If set and equal to "1", the EE
   will understand that it is to load a standalone GC from somewhere and begin to search for the GC to
   load.
2. `StandaloneGCLocation`, (environment variable `COMPlus_StandaloneGCLocation`). If set, it will influence
   the path that the EE will use to probe for a standalone GC dynamic shared library.
3. `StandaloneGCName`, (environment variable `COMPlus_StandaloneGCName`). If set, it will influence the name
   of the dynamic shared library that the EE will look for.

It is not an error to emit either `StandaloneGCLocation` or `StandaloneGCName`. If not specified, and
`UseStandaloneGC` is set, `StandaloneGCLocation` will default to the same directory as CoreCLR and `StandaloneGCName`
will default to `gc` (`gc.dll` on windows, `libgc.so/dylib` on Linux/OSX).

The EE will call `LoadLibrary` using `StandaloneGCLocation` as the directory path and `StandaloneGCName` as the file name.
If this succeeds, the EE will transition to the next state in the loader state machine.

### Verifying the version of a candidate GC

Once the EE has successfully loaded a candidate GC dynamic shared library, it must then check that the candidate GC is
version-compatible with the version of the EE that is doing the loading. It does this in three phases. First, the
candidate GC must expose a function with the given name and signature:

```c++
struct VersionInfo {
  uint32_t MajorVersion;
  uint32_t MinorVersion;
  uint32_t BuildVersion;
  const char* Name;
};

extern "C" void GC_VersionInfo(
  /* Out */ VersionInfo*
);
```

The EE will call `GetProcAddress` on the library, looking for `GC_VersionInfo`. It is a fatal error if this symbol
is not found.

Next, the EE will call this function and receive back a `VersionInfo` structure. Each EE capable of loading 
standalone GCs has a major version number and minor version number that is obtained from the version of 
`gcinterface.h` that the EE built against. It will compare these numbers against the numbers it receives from 
`GC_VersionInfo` in this way:

* If the EE's MajorVersion is not equal to the MajorVersion obtained from the candidate GC, reject. Major version    changes occur when there are breaking changes in the EE/GC interface and it is not possible to interoperate with 
  incompatible interfaces. A change is considered breaking if it alters the semantics of an existing method or if 
  it deletes or renames existing methods so that VTable layouts are not compatible.
* If the EE's MinorVersion is greater than the MinorVersion obtained from the candidate GC, accept
  (Forward compatability). The EE must take care not to call any new APIs that are not present in the version of
  the candidate GC.
* Otherwise, accept (Backward compatibility). It is perfectly safe to use a GC whose MinorVersion exceeds the EE's 
  MinorVersion.

The build version and name are not considered and are provided only for display/debug purposes.

If this succeeds, the EE will transition to the next state in the loader state machine.

### Initializing the GC

Once the EE has verified that the version of the candidate GC is valid, it then proceeds to initialize the
GC. It does so by loading (via `GetProcAddress`) and executing a function with this signature:

```c++
extern "C" HRESULT GC_Initialize(
  /* In  */ IGCToCLR*,
  /* Out */ IGCHeap**.
  /* Out */ IGCHandleManager**,
  /* Out */ GcDacVars*
);
```

The EE will provide its implementation of `IGCToCLR` to the GC and the GC will provide its implementations of
`IGCHeap`, `IGCHandleManager`, and `GcDacVars` to the EE. From here, if `GC_Initialize` returns a successful
HRESULT, the GC is considered initialized and the remainder of EE startup continues. If `GC_Initialize` returns
an error HRESULT, the initialization has failed.

### Initializing the DAC

The existence of a standalone GC is a debuggee process has implications for how the DAC is loaded and
initializes itself. The DAC has access to implementation details of the GC that are not normally exposed as part
of the `GC/EE` interfaces, and as such it is versioned differently.

When the DAC is being initialized and it loads the `GcDacVars` structure from the debuggee process's memory, it
must check the major and minor versions of the DAC, which are itself DAC variables exposed by a standalone GC.
It then decides whether or not the loaded GC is compatible with the DAC that is currently executing. It does this
in the same manner that the EE does:

* If the major versions of the DAC and loaded GC do not agree, reject.
* If the minor version of the DAC is greater than the minor version of the GC, accept but take care
  not to invoke any new code paths not present in the target GC.
* If the minor version of the DAC is less than or equal to the minor version of the GC, accept.

If a DAC rejects a loaded GC, it will return `E_FAIL` from DAC APIs that would otherwise need to interact with the
GC.

### Loader State Machine

The loading of a standalone GC by the EE is governed by a state machine with these states:

* `LOCAL_GC_STATUS_STARTING` - The EE has initiated a standalone GC load, but has not done anything yet.
* `LOCAL_GC_STATUS_DONE_LOAD` - The EE has looked for a file with `StandaloneGCName` in the directory `StandaloneGCLocation`
  and called `LoadLibrary` on it successfully.
* `LOCAL_GC_STATUS_GET_VERSIONINFO` - The EE has successfully located the version information function from the standalone
  GC.
* `LOCAL_GC_STATUS_CALL_VERSIONINFO` - The EE has successfully called the version information function and received a 
  response.
* `LOCAL_GC_STATUS_VERSION_CHECK` - The EE has performed the version check and it succeeded.
* `LOCAL_GC_STATUS_GET_INITIALIZER` - The EE has successfully located the GC initialization function.
* `LOCAL_GC_STATUS_DONE` - The EE has successfully called the GC initialization function and the standalone GC is
  fully initialized.

If loading a standalone GC fails, the combination of which state the loader was in at the point of failure and
the HRESULT that indicates *how* that state failed is enough to log a diagnostic indicative of what occured.

## Outstanding Questions

How can we provide the most useful error message when a standalone GC fails to load? In the past it has been difficult
to determine what preciscely has gone wrong with `coreclr_initialize` returns a HRESULT and no indication of what occured.

Same question for the DAC - Is `E_FAIL` the best we can do? If we could define our own error for DAC/GC version
mismatches, that would be nice; however, that is technically a breaking change in the DAC.
