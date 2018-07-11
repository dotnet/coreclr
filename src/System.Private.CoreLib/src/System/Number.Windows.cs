// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    internal partial class Number
    {
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
                    Interop.Sys.DoubleToStringWindows(src, _CVTBUFSIZE, value, precision, &pNumber->scale, &sign);
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

