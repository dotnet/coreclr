// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace ABIStress
{
    internal interface IAbi
    {
        Type[] TailCalleeCandidateArgTypes { get; }
        CallingConvention[] PInvokeConventions { get; }
        int ApproximateArgStackAreaSize(List<TypeEx> parameters);
    }

    internal class Win86Abi : IAbi
    {
        public Type[] TailCalleeCandidateArgTypes { get; } =
            new[]
            {
                typeof(byte), typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double),
                typeof(Vector<int>), typeof(Vector128<int>), typeof(Vector256<int>),
                typeof(S1P), typeof(S2P), typeof(S2U), typeof(S3U),
                typeof(S4P), typeof(S4U), typeof(S5U), typeof(S6U),
                typeof(S7U), typeof(S8P), typeof(S8U), typeof(S9U),
                typeof(S10U), typeof(S11U), typeof(S12U), typeof(S13U),
                typeof(S14U), typeof(S15U), typeof(S16U), typeof(S17U),
                typeof(S31U), typeof(S32U),
            };

        public CallingConvention[] PInvokeConventions { get; } = { CallingConvention.Cdecl, CallingConvention.StdCall, };

        public int ApproximateArgStackAreaSize(List<TypeEx> parameters)
        {
            int size = 0;
            foreach (TypeEx pm in parameters)
                size += (pm.Size + 3) & ~3;

            return size;
        }
    }

    internal class Win64Abi : IAbi
    {
        // On Win x64, only 1, 2, 4, and 8-byte sized structs can be passed on the stack.
        // Other structs will be passed by reference and will require helper.
        public Type[] TailCalleeCandidateArgTypes { get; } =
            new[]
            {
                typeof(byte), typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double),
                typeof(S1P), typeof(S2P), typeof(S2U), typeof(S4P),
                typeof(S4U), typeof(S8P), typeof(S8U),
            };

        public CallingConvention[] PInvokeConventions { get; } = { CallingConvention.Cdecl };

        public int ApproximateArgStackAreaSize(List<TypeEx> parameters)
        {
            int size = 0;
            foreach (TypeEx pm in parameters)
            {
                // 1, 2, 4 and 8 byte structs are passed directly by value, everything else
                // by ref.
                size += 8;
            }

            // On win64 there's always 32 bytes of stack space allocated.
            size = Math.Max(size, 32);
            return size;
        }
    }

    internal class SysVAbi : IAbi
    {
        // For SysV everything can be passed everything by value.
        public Type[] TailCalleeCandidateArgTypes { get; } =
            new[]
            {
                typeof(byte), typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double),
                // Vector128 is disabled for now due to
                // https://github.com/dotnet/coreclr/issues/26022
                typeof(Vector<int>), /*typeof(Vector128<int>),*/ typeof(Vector256<int>),
                typeof(S1P), typeof(S2P), typeof(S2U), typeof(S3U),
                typeof(S4P), typeof(S4U), typeof(S5U), typeof(S6U),
                typeof(S7U), typeof(S8P), typeof(S8U), typeof(S9U),
                typeof(S10U), typeof(S11U), typeof(S12U), typeof(S13U),
                typeof(S14U), typeof(S15U), typeof(S16U), typeof(S17U),
                typeof(S31U), typeof(S32U),
            };

        public CallingConvention[] PInvokeConventions { get; } = { CallingConvention.Cdecl };

        public int ApproximateArgStackAreaSize(List<TypeEx> parameters)
        {
            int size = 0;
            foreach (TypeEx pm in parameters)
                size += (pm.Size + 7) & ~7;

            return size;
        }
    }

    internal class Arm64Abi : IAbi
    {
        // For Arm64 structs larger than 16 bytes are passed by-ref and will inhibit tailcalls,
        // so we exclude those.
        public Type[] TailCalleeCandidateArgTypes { get; } =
            new[]
            {
                typeof(byte), typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double),
                typeof(Vector<int>), typeof(Vector128<int>), typeof(Vector256<int>),
                typeof(S1P), typeof(S2P), typeof(S2U), typeof(S3U),
                typeof(S4P), typeof(S4U), typeof(S5U), typeof(S6U),
                typeof(S7U), typeof(S8P), typeof(S8U), typeof(S9U),
                typeof(S10U), typeof(S11U), typeof(S12U), typeof(S13U),
                typeof(S14U), typeof(S15U), typeof(S16U),
                typeof(Hfa1), typeof(Hfa2),
            };

        public CallingConvention[] PInvokeConventions { get; } = { CallingConvention.Cdecl };

        public int ApproximateArgStackAreaSize(List<TypeEx> parameters)
        {
            int size = 0;
            foreach (TypeEx pm in parameters)
                size += (pm.Size + 7) & ~7;

            return size;
        }
    }

    internal class Arm32Abi : IAbi
    {
        // For arm32 everything can be passed by value
        public Type[] TailCalleeCandidateArgTypes { get; } =
            new[]
            {
                typeof(byte), typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double),
                typeof(S1P), typeof(S2P), typeof(S2U), typeof(S3U),
                typeof(S4P), typeof(S4U), typeof(S5U), typeof(S6U),
                typeof(S7U), typeof(S8P), typeof(S8U), typeof(S9U),
                typeof(S10U), typeof(S11U), typeof(S12U), typeof(S13U),
                typeof(S14U), typeof(S15U), typeof(S16U), typeof(S17U),
                typeof(S31U), typeof(S32U),
                typeof(Hfa1), typeof(Hfa2),
            };

        public CallingConvention[] PInvokeConventions { get; } = { CallingConvention.Cdecl };

        public int ApproximateArgStackAreaSize(List<TypeEx> parameters)
        {
            int size = 0;
            foreach (TypeEx pm in parameters)
            {
                size += (pm.Size + 3) & ~3;
            }

            return size;
        }
    }
}
