// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.X86
{
    /// <summary>
    /// This class provides access to Intel ADDCARRY hardware instructions via intrinsics
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AddCarry : Sse42
    {
        internal AddCarry() { }

        public new static bool IsSupported { [Intrinsic] get { return false; } }

        public new abstract class X64 : Sse41.X64
        {
            internal X64() { }

            public new static bool IsSupported { [Intrinsic] get { return false; } }

            /// <summary>
            /// unsigned char _addcarry_u64 (unsigned char c_in, unsigned __int64 a, unsigned __int64 b, unsigned __int64 * out)
            ///   ADC reg64, reg64
            /// This intrinisc is only available on 64-bit processes
            /// </summary>
            public static unsafe byte AddWithCarry(byte carryIn, ulong left, ulong right, ulong* result) { throw new PlatformNotSupportedException(); }
        }

        /// <summary>
        /// unsigned char _addcarry_u32 (unsigned char c_in, unsigned int a, unsigned int b, unsigned int * out)
        ///   ADC reg, reg
        /// </summary>
        public static unsafe byte AddWithCarry(byte carryIn, uint left, uint right, uint* result) { throw new PlatformNotSupportedException(); }
    }
}
