// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System
{
    internal static partial class Number
    {
        private const int NumberMaxDigits = 50;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal unsafe ref struct NumberBuffer
        {
            public int precision;
            public int scale;
            public bool sign;
            public NumberBufferKind kind;
            public fixed char digits[NumberMaxDigits + 1];

            public char* GetDigitsPointer()
            {
                // This is safe to do since we are a ref struct
                return (char*)(Unsafe.AsPointer(ref digits[0]));
            }
        }

        internal enum NumberBufferKind : byte
        {
            Unknown = 0,
            Integer = 1,
            Decimal = 2,
            Double = 3
        }
    }
}
