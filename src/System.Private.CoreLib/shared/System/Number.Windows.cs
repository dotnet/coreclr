// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Runtime.CompilerServices;
#if !CORECLR
using DoubleToStringWindows = System.Runtime.RuntimeImports._ecvt_s;
#endif

namespace System
{
    internal partial class Number
    {
#if CORECLR
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern unsafe void DoubleToStringWindows(byte* buffer, int sizeInBytes, double value, int count, int* dec, int* sign);
#endif

        private static unsafe void DoubleToNumber(double value, int precision, ref NumberBuffer number)
        {
            number.precision = precision;
            if (!Double.IsFinite(value))
            {
                number.scale = Double.IsNaN(value) ? ScaleNAN : ScaleINF;
                number.sign = Double.IsNegative(value);
                number.digits[0] = '\0';
            }
            else
            {
                byte* src = stackalloc byte[_CVTBUFSIZE];
                int sign;
                fixed (NumberBuffer* pNumber = &number)
                {
                    DoubleToStringWindows(src, _CVTBUFSIZE, value, precision, &pNumber->scale, &sign);
                }
                number.sign = sign != 0;

                char* dst = number.digits;
                if ((char)*src != '0')
                {
                    while (*src != 0)
                        *dst++ = (char)*src++;
                }
                *dst = '\0';
            }
        }
    }
}

