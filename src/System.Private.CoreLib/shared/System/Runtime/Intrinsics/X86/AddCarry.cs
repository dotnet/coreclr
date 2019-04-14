// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.X86
{
    /// <summary>
    /// This class provides access to Intel ADDCARRY hardware instructions via intrinsics
    /// </summary>
    [Intrinsic]
    [CLSCompliant(false)]
    public abstract class AddCarry : Sse42
    {
        internal AddCarry() { }

        public new static bool IsSupported { get => IsSupported; }

        [Intrinsic]
        public new abstract class X64 : Sse41.X64
        {
            internal X64() { }

            public new static bool IsSupported { get => IsSupported; }

            /// <summary>
            /// unsigned char _addcarry_u64 (unsigned char c_in, unsigned __int64 a, unsigned __int64 b, unsigned __int64 * out)
            ///   ADC reg64, reg64
            /// This intrinisc is only available on 64-bit processes
            /// </summary>
            public static unsafe byte AddWithCarry(byte carryIn, ulong left, ulong right, ulong* result) => AddWithCarry(carryIn, left, right, result);
        }

        /// <summary>
        /// unsigned char _addcarry_u32 (unsigned char c_in, unsigned int a, unsigned int b, unsigned int * out)
        ///   ADC reg, reg
        /// </summary>
        public static unsafe byte AddWithCarry(byte carryIn, uint left, uint right, uint* result) => AddWithCarry(carryIn, left, right, result);
    }
}
