Implementation of Hardware Intrinsics in CoreCLR
================================================
This document describes the implementation of hardware intrinsics in CoreCLR.
For information about how the intrinsic APIs are designed, proposed and approved,
see https://github.com/dotnet/designs/blob/master/accepted/platform-intrinsics.md.

In discussing the hardware intrinsics, we refer to the target platform, such as X86 or Arm64, as the "platform" and each set of extensions that are implemented as a unit (e.g. AVX2 on X64 or Simd on Arm64) as an "ISA".

There is a design document for the Arm64 intrinsics: https://github.com/dotnet/coreclr/blob/master/Documentation/design-docs/arm64-intrinsics.md. It should be updated to reflect current (and ongoing) progress.

## Overview

The reference assemblies for the hardware intrinsics live in corefx, but all of the implementation is in the coreclr repo:

* The C# implementation lives in coreclr/src/shared/System/Runtime/Intrinsics. These are little more than skeleton methods that usually invoke themselves recursively (see Indirect invocation support below).

## C# Implementation

The hardware intrinsics operate on and produce both primitive types (`int`, `float`, etc.) as well as vector types. The vector types are considered platform-agnostic, though not all platforms define intrinsics on all of these types. They are:

* `Vector64<T>` - A 64-bit vector of type `T`. For example, a `Vector64<int>` would hold two 32-bit integers.
* `Vector128<T>` - A 128-bit vector of type `T`
* `Vector256<T>` - A 256-bit vector of type `T`

Note that these are generic types, which distinguishes these from native intrinsic vector types. It also somewhat complicates interop, as the runtime currently doesn't support interop for generic types.
*** Is there an issue for this?? ***

The C# declaration of a hardware intrinsic ISA class is marked with the `[Intrinsic]` attribute, and the implementations of the intrinsic methods on that class are recursive. When the VM encounters such a method, it will communicate to the JIT that this is an intrinsic method, and will also pass a `mustExpand` flag to indicate that the JIT must generate code.

### Platform-agnostic vector types

The vector types supported by one or more target ISAs are supported across platforms, though they extent to which operations on them are available and accelerated is dependent on the target ISA. These are the generic types: `Vector64<T>`, `Vector128<T>` and `Vector256<T>`.

## JIT Implementation

The bulk of the implementation work for hardware intrinsics is in the JIT.

### Hardware Intrinsics Table

There is a hardware intrinsics table for each platform that supports hardware intrinsics: currently `_TARGET_XARCH_` and `TARGET_ARM64_`. They live in hwintrinsiclistxarch.h and hwintrinsiclistarm64.h respectively.

These tables are intended to capture information that can assist in making the implementation as data-driven as possible.

### IR

The hardware intrinsics nodes are generally imported as `GenTreeHWIntrinsic` nodes, with the `GT_HWIntrnsic` operator. On these nodes, the `gtHWIntrinsicId` field contains the intrinsic ID, as declared in the hardware intrinsics table.

### Importation

Hardware intrinsics appear in the IL as calls.

### Lowering

As described here: https://github.com/dotnet/coreclr/blob/master/Documentation/botr/ryujit-overview.md#lowering, Lowering is responsible for transforming the IR in such a way that the control flow, and any register requirements, are fully exposed. This includes determining what instructions can be "contained" in another, such as immediates or addressing modes. For the hardware intrinsics, these are done in the target-specific methods `Lowering::LowerHWIntrinsic()` and `Lowering::ContainCheckHWIntrinsic()`.

### Register Allocation

The register allocator has three main passes.

The `LinearScan::buildNode` method is responsible for identifying all register references in the IR, and constructing the `RefPosition`s that represent those references, for each node. For hardware intrinsics it delegates this function to `LinearScan::buildHWIntrinsic()` and the `LinearScan::getKillSetForHWIntrinsic()` method is responsible for generating kill `RefPositions` for these nodes.

The other thing to be aware of is that the calling convention for large vectors (256-bit vectors on x86, and 128-bit vectors on Arm64) does not preserve the upper half of the callee-save vector registers. As a result, this require some special modeling in the register allocator. See the places where `FEATURE_PARTIAL_SIMD_CALLEE_SAVE` appears in the code. This code, fortunately, requires little differentiation between the two platforms.

## Code Generation

By design, the actual code generation is fairly straightforward, since the hardware intrinsics are intended to each map to a specific target instructions. Much of the implementation of the x86 intrinsics is table-driven. 

## Encoding

The only thing that makes the hardware intrinsics different in the area of instruction encodings is that they depend on many instructions (and their encodings) that are not used in any context other than the implementation of the associated hardware intrinsic.

The encodings are largely specified by `coreclr\src\jit\instrs{arch}.h`, and most of the target-specific code is in the `emit{arch}.*` files.

This is an area of the JIT that could use some redesign and refactoring.

## Testing

The tests for the hardware intrinsics reside in the coreclr/tests/src/JIT/HardwareIntrinsics directory.

Many of the tests generated programmatically from templates. See `coreclr\tests\src\JIT\HardwareIntrinsics\General\Shared\GenerateTests.csx`. We would like to see most, if not all, of the remaining tests converted to use this mechanism.

