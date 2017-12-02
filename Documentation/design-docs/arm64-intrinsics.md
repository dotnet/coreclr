## Arm64 Intrinsics

This document is intended to document proposed design decision related to the introduction
of Arm64 Intrinsics

### Intrinsics in general

Use of intrinsics in general is a CoreCLR design decision to allow low level platform
specific optimizations.  

At first glance, such a decision seems to violate the fundamental principles of .NET
code running on any platform.  However, the intent is not for the vast majority of
apps to use such optimizations.  The intended usage model is to allow library
developers access to low level functions which enable optimization of key
functions.  As such the use is expected to be limited, but performance critical.

#### Intrinsic `IsSupported` granularity

The existing implementation is using `IsSupported` to determine whether an existing
namespace is supported on a given platform.

The granularity is kept at a large scale to have large blocks of functionality controlled
by a single `IsSupported`.

#### Intrinsic granularity

In general individual intrinsic will be chosen to be fine grained.  These will generally correspond to a single assembly instruction.

#### Intrinsics & Crossgen

For any intrinsic which may not be supported on all variants of a platform.  Crossgen
Method compilation must be trapped, so that the JIT is forced to generate optimal platform dependent 
code at runtime.

#### API review

Intrinsics will extend the API of CoreCLR.  They will need to follow standard API review practices.

### Choice of Arm64 naming conventions

#### Namespaces

`ARM32` and `ARM64` will follow similar namespace conventions.

+ `System.Runtime.Intrinsics` is used for type definitions useful across multiple platforms
+ `System.Runtime.Intrinsics.Arm` is used for enumerations shared across `ARM32` and `ARM64` platforms
+ `System.Runtime.Intrinsics.Arm.Base` is recommended for functionality included in all `ARM32` and `ARM64` platforms
  + Do we need this or could we assume `ARMv8` is minimum supported platform?
  + Should we assume `ARMv7` or `ARMv5` is minimum supported platform?
+ `System.Runtime.Intrinsics.Arm.ARMv8` is used for `ARMv8` added functionality included in both `ARM32` and `ARM64` platforms 
+ `System.Runtime.Intrinsics.Arm.ARMv8.Arm64` is used for `ARMv8` functionality only included in `ARM64` platform
+ `System.Runtime.Intrinsics.Arm.ARMv8_1....` is used for `ARMv8.1` functionality.  
  + Use similar sub namespace names for only included in `ARM64` platform ...
+ `System.Runtime.Intrinsics.Arm.XXX` is used for optional features `CRC32`, `AES`, `SHA1` ... 
  + All extensions which were not required when they were initially introduced
  + For example, `CRC32` became required with `ARMv8.1`, but since it was introduced as optional in `ARMv8` it would go in its own namespace
  
If an extension is only supported on `Arm32`, there is no reason to add a sub namespace.

+ `System.Runtime.Intrinsics.Arm.ARMv7` is used for `ARMv7` added functionality not included in `Base`

If an extension is only supported on `Arm64`, there is no reason to add a sub namespace.  Hypothetically:

+ `System.Runtime.Intrinsics.Arm.ARMv10` is used for `ARMv10` added functionality (Hypothetically `Arm64` only)

Namespace is chosen based on what could be implemented, not on what is or will be implemented.

#### Names

Intrinsics will be named to describe functionality.  Names will not correspond to specific named assembly instructions.

`IsSupported` will be used in any namespace which may not be implemented on all CoreCLR platforms

### Intrinsic Interface Documentation

+ Documentation will be minimal preferring the underlying ARM documentation
+ Namespaces will briefly document corresponding specification
+ Intrinsic methods will briefly document corresponding assembly instruction(s) for each platform

### Test coverage

As intrinsic support is added test coverage must be extended to provide basic testing

### Phased Implementation

#### Implementation Priorities

As guidelines:

+ Baseline functionality will be prioritized over architectural extensions
+ Architectural extensions will typically be prioritized in age order.  Earlier extensions will be added first
+ `ARM64` only and `ARM*` features will have equal priority
+ Priorities will be driven by optimization efforts and requests
+ Priority will be given to intrinsics which have already been implemented for other platforms
+ Priority will be given to intrinsics which are equivalent/similar to those actively used in libraries for other platforms

#### Addition of new intrinsics namespaces
When an intrinsic namespace is introduced code will likely be implemented on different platforms at different times.

It is explicitly OK for ARM64 to implement a namespace which may be implemented on `ARM32` w/o `ARM32` implementation.
The opposite is also true.

Any platform which does not implement a namespace, must:

+ Return `false` for `IsSupported`
+ `throw` on any unimplemented function

#### Partial implementation of intrinsic namespace

+ It is preferred that `IsSupported` represents the state of an entire namespace
+ It is certainly required at time of a release, `IsSupported` represents the state of an entire namespace
+ Allow for writing tests and implementing code, `IsSupported == false` may be used when namespace is partially implemented

#### Addition of new intrinsics to existing namespace
When an intrinsic namespace is introduced, a best guess will be made about the set of useful intrinsics.
However, the intrinsics will be added as needed or requested for various optimization efforts.

+ The set of intrinsics instructions supported by a namespace should only be allowed
if all platforms reporting `IsSupported` are supported (at least at time of release)
+ It may be preferable to add new namespaces in cases where this creates significant burden

### Half precision floating point

This document will refer to half precision floating point as `Half`.

+ Machine learning and Artificial intelligence often use `Half` type to simplify storage and improve processing time.
+ CoreCLR and `CIL` in general do not have general support for a `Half` type
+ There is an open request to expose `Half` intrinsics
+ ARM64 will introduce `System.Runtime.Intrinsics.Half` to support this request 
+ `Half` will be defined as:

```
    [StructLayout(LayoutKind.Sequential, Size = 2)]
    public struct Half : struct {}
``` 

#### ARMv8 Half precision support

ARMv8 baseline support for `Half` is limited.  The following operations are supported

+ Loads and Stores
+ Conversion to/from `Float`
+ Widening from `Vector128<Half>` to two `Vector128<Float>`
+ Narrowing from two `Vector128<Float>` to `Vector128<Float>`

It is presumed that even this minimal support could be helpful

Optional `Half` extensions add more complete support

#### `Half` and ARM64 ABI

The proposed `Half` implementation will treat `Half` as raw bits.  
It is likely therefore that this will not conform to the ARM64 ABI specifically with respect to the HFA
calling convention.  This will hinder use of `Half` in `PInvoke` calls.  Resolving this ABI issue will
be a low priority.

### Scalable Vector Extension Support

`SVE`, the Scalable Vector Extension introduces its own complexity.  

The extension 
+ Creates a new set of SVE registers 
+ Each register has a platform specific length
  + Any multiple of 128 bits

Therefore implementation will not be trivial.

+ Register allocator may need changes
+ Crossgen of SVE intrinsics must be delayed until runtime JIT
+ SIMD support will face similar issues
+ Open issue: Should we use `Vector<T>` or SVE<T> in user interface design?
+ SVE probably requires a separate design

### Miscellaneous
#### Choice of Arm64 naming conventions -- Rationale

##### Naming conventions

+ Namespaces are generally using Pascal case
+ Namespaces and Names must not start with a number
+ Names are descriptive.  Therefore intrinsics are generally named to describe functionality

`ARMv8` is assumed to be an abbreviation for `ARM Reference Manual Version 8`

##### Reference x86 & x64 choice of namespaces

+ `System.Runtime.Intrinsics` is used for type definitions useful across multiple platforms
+ `System.Runtime.Intrinsics.X86` is used for enumerations shared across both platforms
+ `System.Runtime.Intrinsics.AVX` is used for the instructions which are included in a specific hardware extension

##### ARM Version history

Looking at `ARM` version history, 

+ The `ARM` instruction set is regularly extended: `ARMv5`, `ARMv7`, `ARMv8`
+ New version are generally supersets of prior versions
  + Deprecated features are preserved for at least one generation
  + Deprecated features are eventually completely removed
+ `ARMv8` was the first version to introduce 64-bit support
  + Legacy ARMv7 was extended to become `ARMv8` `AARCH32` 
  + `ARMv8` `AARCH64` was introduced
  + `AARCH64` used similar but distinctly different instruction set
  + `AARCH64` naming convention varied slightly from `AARCH32`
+ `ARMv8.1` & `ARMv8.2` extensions have been introduced
+ New instructions introduced by extensions have generally been asymmetric.  New `AARCH64` instructions ARMv8New extensions have generally made It is not uncommon for an extensions to include new instructions for 
`AARCH64`
....  With the introduction of `ARMv8`, `aarch64` (`ARM64`) and `aarch32` were
split.  The legacy ARMv7 instruction encodings were only supported in `aarch32`

#### Handling Instruction Deprecation

Deprecation of instructions should be relatively rare

+ Do not introduce an intrinsic for a feature that is currently deprecated
+ In event an assembly instruction is deprecated
  1. Prefer emulation using alternate instructions if practical
  2. Add `SetThrowOnDeprecated()` interface to allow developpers to find these issues
