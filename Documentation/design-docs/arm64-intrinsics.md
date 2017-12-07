# Arm64 Intrinsics

This document is intended to document proposed design decisions related to the introduction
of Arm64 Intrinsics

## Document Goals

+ Discuss design options
  + Document existing design pattern
  + Draft initial design decisions which are least likely to cause extensive rework
+ Decouple `X86`, `X64`, `ARM32` and `ARM64` development
  + Make some minimal decisions which encourage API similarity between platforms
  + Make some additional minimal decisions which allow `ARM32` and `ARM64` API's to be similar
+ Decouple CoreCLR implementation and testing from API design
+ Allow for best API design
+ Keep implementation simple

## Intrinsics in general

Use of intrinsics in general is a CoreCLR design decision to allow low level platform
specific optimizations.

At first glance, such a decision seems to violate the fundamental principles of .NET
code running on any platform.  However, the intent is not for the vast majority of
apps to use such optimizations.  The intended usage model is to allow library
developers access to low level functions which enable optimization of key
functions.  As such the use is expected to be limited, but performance critical.

## Intrinsic granularity

In general individual intrinsic will be chosen to be fine grained.  These will generally
correspond to a single assembly instruction.

## Logical Sets of Intrinsics

Individual CPU instantiate a specific set of instructions.  For various reasons, an
individual CPU will have a specific set of supported instructions.  for `ARM64` the
set of supported instructions is identified by various `ID_* System registers`.
While these feature registers are only available for the OS to access, they provide
a logical grouping of instructions which are enable/disabled to gether.

### API Logical Set grouping & `IsSupported`

The C# API must provide a mechanism to determine which sets of instructions are supported.

Existing design uses a separate `static class` to group the methods which correspond to each
logical set of instructions.

A single `IsSupported` property is included in each `static class` to allow client code to alter
control flow.

The `IsSupported` properties are design so that JIT can remove code on unused paths.

`ARM64` will use an identical approach.

### API `PlatformNotSupported` Exception

If client code calls an intrinsic which is not supported by the platform a `PlatformNotSupported`
exception must be thrown.

### JIT, VM, PAL & OS requirements

The JIT must use a set of flags corresponding to logical sets of instructions to alter code
generation.

The VM must query the OS to populate the set of JIT flags.  For the special altJit case, a
means must provide for setting the flags.

PAL must provide an OS abstraction layer.

Each OS must provide mechanism for determining which sets of instructions are supported.

+ Linux provides the HWCAP detection mechanism which is able to detect current set of exposed
features
+ Arm64 MAC OS and Arm64 Windows OS must provide an equally capable detection mechanism.

In the event the OS fails to provides a means to detect a support for an instruction set extension
it must be treated as unsupported.

NOTE: Exceptions might be where:

+ CoreCLR is distributed as source and CMake build configuration test is used to detect these features
+ Installer detects features and sets appropriate configuration knobs
+ VM runs code inside safe try/catch blocks to test for instruction support
+ Platform requires a specific minimum set of instructions

### Intrinsics & Crossgen

For any intrinsic which may not be supported on all variants of a platform.  Crossgen Method
compilation must be trapped, so that the JIT is forced to generate optimal platform dependent
code at runtime.

## Choice of Arm64 naming conventions

`x86`, `x64`, `ARM32` and `ARM64` will follow similar naming conventions.

### Namespaces

+ `System.Runtime.Intrinsics` is used for type definitions useful across multiple platforms
+ `System.Runtime.Intrinsics.Arm` is used type definitions shared across `ARM32` and `ARM64` platforms
+ `System.Runtime.Intrinsics.Arm.Arm64` is used for type definitions for the `ARM64` platform
  + The primary implementation of `ARM64` intrinsics wil occur within this namespace
  + While `x86` and `x64` share a common namespace.  This document is recommending a separate namespace
  for `ARM32` and `ARM64`.  This is because `AARCH64` is a separate `ISA` from the `AARCH32` `Arm` & `Thumb`
  instruction sets.  It is not an `ISA` extension, but rather a new `ISA`.  This is different from `x64`
  which could be viewed as a superset of `x86`.
  + The logical grouping of `ARM64` and `ARM32` instruction sets is different.  It is controlled by
  different sets of `System Registers`.

For the convenience of the end user, it may be useful to add convenience API's which expose functionality
which is common across platforms and sets of platforms.  These could be implemented in terms of the
platform specific functionality.  These API's are currently out of scope of this initial design document.

### Logical Set Class Names

Within the `System.Runtime.Intrinsics.Arm.Arm64` namespace there will be a separate `static class` for each
logical set of instructions

The sets will be chosen to match the granularity if the `ARM64` `ID_*` register fields.

#### Specific Class Names

The table below documents the set of known extensions, their identification, and their recommended intrinsic
class names.

| ID Register      | Field   | Values   | Intrinsic `static class` name | Ext. type  |
| ---------------- | ------- | -------- | ----------------------------- | ---------- |
| N/A              | N/A     | N/A      | All                           | Baseline   |
| ID_AA64ISAR0_EL1 | AES     | (1b, 10b)| Aes                           | Crypto     |
| ID_AA64ISAR0_EL1 | Atomic  | (10b)    | Atomics                       | Ordering   |
| ID_AA64ISAR0_EL1 | CRC32   | (1b)     | Crc32                         | Crypto     |
| ID_AA64ISAR1_EL1 | DPB     | (1b)     | Dcpop                         | Ordering   |
| ID_AA64ISAR0_EL1 | DP      | (1b)     | Dp                            | SIMD       |
| ID_AA64ISAR1_EL1 | FCMA    | (1b)     | Fcma                          | SIMD       |
| ID_AA64PFR0_EL1  | FP      | (0b, 1b) | Fp                            | FP         |
| ID_AA64PFR0_EL1  | FP      | (1b)     | Fp16                          | FP, Half   |
| ID_AA64ISAR1_EL1 | JSCVT   | (1b)     | Jscvt                         | SIMD       |
| ID_AA64ISAR1_EL1 | LRCPC   | (1b)     | Lrcpc                         | Ordering   |
| ID_AA64ISAR0_EL1 | AES     | (10b)    | Pmull                         | SIMD       |
| ID_AA64PFR0_EL1  | RAS     | (1b)     | Ras                           | Ordering   |
| ID_AA64ISAR0_EL1 | SHA1    | (1b)     | Sha1                          | Crypto     |
| ID_AA64ISAR0_EL1 | SHA2    | (1b, 10b)| Sha2                          | Crypto     |
| ID_AA64ISAR0_EL1 | SHA3    | (1b)     | Sha3                          | Crypto     |
| ID_AA64ISAR0_EL1 | SHA2    | (10b)    | Sha512                        | Crypto     |
| ID_AA64PFR0_EL1  | AdvSIMD | (0b, 1b) | Simd                          | SIMD       |
| ID_AA64PFR0_EL1  | AdvSIMD | (1b)     | SimdFp16                      | SIMD,Half  |
| ID_AA64ISAR0_EL1 | RDM     | (1b)     | SimdV81                       | SIMD       |
| ID_AA64ISAR0_EL1 | SM3     | (1b)     | Sm3                           | Crypto     |
| ID_AA64ISAR0_EL1 | SM4     | (1b)     | Sm4                           | Crypto     |
| ID_AA64PFR0_EL1  | SVE     | (1b)     | Sve                           | SIMD,SVE   |

The `All`, `Simd`, and `Fp` classes will together contain the bulk of the `ARM64` intrinsics.  Most other extensions
will only add a few instruction so they should be simpler to review.

The `Baseline` `All` `static class` is used to represent any intrinsic which is guaranteed to be implemented on all
`ARM64` platforms.  This set will include general purpose instructions.  Investigation is needed to determine if
any of these need intrinsics or whether this is an empty set.

As further extensions are released, this set of intrinsics will grow.

### Intrinsic Method Names

Intrinsics will be named to describe functionality.  Names will not correspond to specific named
assembly instructions.

Where precedence exists within the `System.Runtime.Intrinsics.X86` namespace, identical method names will be
chosen: `Add`, `Multiply`, `Load`, `Store` ...

It is also worth noting `System.Runtime.Intrinsics.X86` naming conventions will include the suffix `Scalar` for
operations which take vector argument(s), but contain an implicit cast(s) to the base type and therefore operate only
on the first item of the argument vector(s).

### Intinsic Method Argument Types

Intrinsic methods will typically use a standard set of argument types:
+ Integer type: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
+ Floating types: `double`, `single`, `System.Half`
+ Vector types: Vector128<T>
+ SVE will add new vector types: TBD

## Intrinsic Interface Documentation

+ Namespace
+ Each `static class` will
  + Briefly document corresponding `System Register Field and Value` from ARM specification.
  + Document use of IsSupported property
  + Optionally summarize set of methods enabled by the extension
+ Each intrinsic method will
  + Document underlying `ARM64` assembly instruction
  + Optionally, briefly summarize operation performed
    + In many cases this may be unnecessary: `Add`, `Multiply`, `Load`, `Store`
    + In some cases this may be difficult to do correctly. (Crypto instructions)
  + Optionally mention corresponding compiler gcc, clang, and/or MSVC intrinsics
    + Review of existing documentation shows `ARM64` intrinsics are mostly absent or undocumented so
    initially this will not be necessary for `ARM64`
    + See gcc manual "AArch64 Built-in Functions"
    + MSVC ARM64 documentation has not been publically released

## Phased Implementation

### Implementation Priorities

As rough guidelines for order of implementation:

+ Baseline functionality will be prioritized over architectural extensions
+ Architectural extensions will typically be prioritized in age order.  Earlier extensions will be added first
  + This is primarily driven by availability of hardware.  Features released in earlier will be prevalent in
  more hardware.
+ Priorities will be driven by optimization efforts and requests
  + Priority will be given to intrinsics which are equivalent/similar to those actively used in libraries for other platforms
  + Priority will be given to intrinsics which have already been implemented for other platforms

### API review

Intrinsics will extend the API of CoreCLR.  They will need to follow standard API review practices.

#### API review of a new intrinsic `static class`es

Review will be facilitated by GitHub Pull requests to amend the Approved API section of this document.

A separate GitHub Issue will typically created for the review of each intrinsic `static class`.  This allows design and review team to
review separately.  This allows review complexity to be kept manageable.  The O(N^2) nature of the review process will be kept to
reasonable levels and iterations will be finite.

Every effort will be made to completely elaborate all the methods of the intrinsic `static class` for each review.  This will
help minimize reopening the same classes for review.

Implementation will be kept separate from from API review.

### Partial implementation of intrinsic `static class`

+ `IsSupported` must represents the state of an entire intrinsic `static class`
+ Once API review is complete and approved, it is acceptable to implement approved methods in any order provided tests are added
+ The approved API must be completed before the intrinsic `static class` is included in a release

### Test coverage

As intrinsic support is added test coverage must be extended to provide basic testing

## Half precision floating point

This document will refer to half precision floating point as `Half`.

+ Machine learning and Artificial intelligence often use `Half` type to simplify storage and improve processing time.
+ CoreCLR and `CIL` in general do not have general support for a `Half` type
+ There is an open request to expose `Half` intrinsics
+ There is an outstanding proposal to add `System.Half` to support this request
+ Implementation of `Half` features will be adjusted based on
  + Implementation of the `System.Half` proposal
  + Availability of supporting hardware (extensions)
  + General language extensions supporting `Half`

### ARMv8 Half precision support

ARMv8 baseline support for `Half` is limited.  The following operations are supported

+ Loads and Stores
+ Conversion to/from `Float`
+ Widening from `Vector128<Half>` to two `Vector128<Float>`
+ Narrowing from two `Vector128<Float>` to `Vector128<Half>`

Recent extension add support for

+ General operations on `Half` types
+ Vector operations on `Half` types

### `Half` and ARM64 ABI

Any complete `Half` implementation must conform to the `ARM64 ABI`.

The proposed `System.Half` type must be treated as a floating point type for purposes of the ARM64 ABI

As an argument it must be passed in a floating point register.

As a structure member, it must be treated as a floating point type and enter into the HFA determination logic.

Test cases must be written and conformance must be demonstrated.

## Scalable Vector Extension Support

`SVE`, the Scalable Vector Extension introduces its own complexity.

The extension
+ Creates a new set of SVE registers
+ Each register has a platform specific length
  + Any multiple of 128 bits

Therefore implementation will not be trivial.

+ Register allocator may need changes
+ SIMD support will face similar issues
+ Open issue: Should we use `Vector<T>` or SVE<T> in user interface design?

Given lack of available hardware and a lack of thorough understanding of the specification:

+ SVE will require a separate design
+ SVE is considered out of scope for this document

## Miscellaneous
### Handling Instruction Deprecation

Deprecation of instructions should be relatively rare

+ Do not introduce an intrinsic for a feature that is currently deprecated
+ In event an assembly instruction is deprecated
  1. Prefer emulation using alternate instructions if practical
  2. Add `SetThrowOnDeprecated()` interface to allow developers to find these issues

## Approved APIs

The following sections document APIs which have completed the API review process.

Until each API is approved it shall be marked "TBD Not Approved"

### `All`

TBD Not approved

### `Aes`

TBD Not approved

### `Atomics`

TBD Not approved

### `Crc32`

TBD Not approved

### `Dcpop`

TBD Not approved

### `Dp`

TBD Not approved

### `Fcma`

TBD Not approved

### `Fp`

TBD Not approved

### `Fp16`

TBD Not approved

### `Jscvt`

TBD Not approved

### `Lrcpc`

TBD Not approved

### `Pmull`

TBD Not approved

### `Ras`

TBD Not approved

### `Sha1`

TBD Not approved

### `Sha2`

TBD Not approved

### `Sha3`

TBD Not approved

### `Sha512`

TBD Not approved

### `Simd`

TBD Not approved

### `SimdFp16`

TBD Not approved

### `SimdV81`

TBD Not approved

### `Sm3`

TBD Not approved

### `Sm4`

TBD Not approved

### `Sve`

TBD Not approved
