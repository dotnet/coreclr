// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    internal static partial class JitHelpers
    {
        // From https://software.intel.com/sites/default/files/managed/9e/bc/64-ia-32-architectures-optimization-manual.pdf
        // Example 3.2. We're taking advantage of the fact that bools are passed by value as 32-bit integers,
        // so we'll blit it directly into a 1 or a 0 without a jump.
        // Codegen will emit a movzx, but on Ivy Bridge (and later) the CPU itself elides the instruction.

        // 'valueIfTrue' and 'valueIfFalse' really should be compile-time constants for maximum throughput.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConditionalSelect(bool condition, int valueIfTrue, int valueIfFalse)
        {
            return ((-Unsafe.As<bool, int>(ref condition)) & (valueIfTrue - valueIfFalse)) + valueIfFalse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConditionalSelect(bool condition, uint valueIfTrue, uint valueIfFalse)
        {
            return (uint)(((-Unsafe.As<bool, int>(ref condition)) & ((int)valueIfTrue - (int)valueIfFalse)) + (int)valueIfFalse);
        }
    }
}
