# DllMap
DllMap is a mechanism to influence native library load resolution via a name mapping. DllMap feature facilitates writing platform-agnostic library invocations (`pInvoke`) in managed code, while separating the platform-specific naming details to a separate configuration file. This document describes how DllMap can be realized in .Net Core.

### Dllmap in Mono

Mono implements [Dllmap](http://www.mono-project.com/docs/advanced/pinvoke/dllmap/)  using an XML configuration for name mappings.  For example:

```xml
<configuration>
    <dllmap dll="MyLib.dll" target="YourLib.dll"/>
    <dllmap os="windows" dll="libc.so.6" target="cygwin1.dll"/>
</configuration>
```

Mono also permits mapping of method names within libraries, but with the restriction that the original and mapped methods must have the same signature. This document does not deal with mapping method names or signatures.

### Dllmap in .Net Core

#### NativeLibrary APIs

.Net Core 3 provides a rich set of APIs to manage native libraries, as well as callbacks to influence native library resolution. 

- [NativeLibrary APIs](https://github.com/dotnet/corefx/blob/master/src/System.Runtime.InteropServices/ref/System.Runtime.InteropServices.cs#L728-L738): Perform operations on native libraries (such as `Load()`, `Free()`, get the address of an exported  symbol, etc.) in a platform-independent way from managed code.
- [DllImport Resolver callback](https://github.com/dotnet/corefx/blob/master/src/System.Runtime.InteropServices/ref/System.Runtime.InteropServices.cs#L734):  Get a call-back for first-chance native library resolution using custom logic. 
- [Native Library Resolve event](https://github.com/dotnet/corefx/blob/master/src/System.Runtime.Loader/ref/System.Runtime.Loader.cs#L39): Get an event for last-chance native library resolution using custom logic.   

These APIs can be used to implement custom native library resolution logic, including Mono-style DllMap.

#### DllMap Sample

A sample implementation of DLLMap using the NativeLibrary APIs is here: [DllMap Sample](../../tests/src/Interop/DllMap). The sample demonstrates:

* An [app](../../tests/src/Interop/DllMap/DllMapTest.cs) that pInvokes a method in `OldLib`, but runs in an environment where only [`NewLib`](../../tests/src/Interop/DllMap/NewLib.cpp) is available.
* The [XML file](../../tests/src/Interop/DllMap/DllMapTest.xml) that maps the library name from `OldLib` to `NewLib`. 
* The [DllMap](../../tests/src/Interop/DllMap/DllMapTest.cs) implementation, which parses the above mapping, and uses NativeLibrary APIs to load the correct library.

