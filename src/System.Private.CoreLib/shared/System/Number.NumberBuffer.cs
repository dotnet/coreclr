// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System
{
    internal static partial class Number
    {
        //  We need 1 additional byte, per length, for the terminating null
        private const int DecimalNumberBufferLength = 50 + 1;
        private const int DoubleNumberBufferLength = 50 + 1;
        private const int Int32NumberBufferLength = 50 + 1;
        private const int Int64NumberBufferLength = 50 + 1;
        private const int SingleNumberBufferLength = 50 + 1;
        private const int UInt32NumberBufferLength = 50 + 1;
        private const int UInt64NumberBufferLength = 50 + 1;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal unsafe ref struct NumberBuffer
        {
            public int Precision;
            public int Scale;
            public bool Sign;
            public NumberBufferKind Kind;
            public Span<char> Digits;

            public NumberBuffer(NumberBufferKind kind, char* pDigits, int digitsLength)
            {
                Precision = 0;
                Scale = 0;
                Sign = false;
                Kind = kind;
                Digits = new Span<char>(pDigits, digitsLength);
            }

            public char* GetDigitsPointer()
            {
                // This is safe to do since we are a ref struct
                return (char*)(Unsafe.AsPointer(ref Digits[0]));
            }
        }

        internal enum NumberBufferKind : byte
        {
            Unknown = 0,
            Integer = 1,
            Decimal = 2,
            Double = 3,
        }
    }
}
