// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System
{
    internal partial class Number
    {
        // buffer size to hold digits (40), decimal point, number sign, exponent, exponent symbol 'e', exponent sign and null.
        private const int MAX_BUFFER_SIZE = 50;

        private static unsafe void DoubleToNumber(double value, int precision, ref NumberBuffer number)
        {
            Debug.Assert(precision > 0 && precision < 40);

            number.precision = precision;
            if (!Double.IsFinite(value))
            {
                number.scale = Double.IsNaN(value) ? ScaleNAN : ScaleINF;
                number.sign = Double.IsNegative(value);
                number.digits[0] = '\0';
                return;
            }

            byte* tempBuffer = stackalloc byte[MAX_BUFFER_SIZE];
            char* dst = number.digits;

            number.scale = 0;
            number.sign = false;
            *dst = '\0';

            if (value < 0.0)
            {
                number.sign = true;
            }

            if (value == 0.0)
            {
                for (int j = 0; j < precision; j++)
                {
                    dst[j] = '0';
                }
                dst[precision] = '\0';
                return;
            }

            //
            // Get the number formatted as a string in the form x.xxxxxxexxxx
            //

            // "%.40e"
            byte* format = stackalloc byte[6];
            format[0] = (byte)'%';
            format[1] = (byte)'.';
            format[2] = (byte)'4';
            format[3] = (byte)'0';
            format[4] = (byte)'e';
            format[5] = 0;

            int tempBufferLength = Interop.Sys.DoubleToStringUnix(value, format, tempBuffer, MAX_BUFFER_SIZE);
            Debug.Assert(tempBufferLength > 0 && MAX_BUFFER_SIZE > tempBufferLength);

            //
            // Calculate the exponent value
            //

            int exponentIndex = tempBufferLength - 1;
            while (tempBuffer[exponentIndex] != (byte)'e' && exponentIndex > 0)
            {
                exponentIndex--;
            }

            Debug.Assert(exponentIndex > 0 && (exponentIndex < tempBufferLength - 1));

            int i = exponentIndex + 1;
            int exponentSign = 1;
            if (tempBuffer[i] == '-')
            {
                exponentSign = -1;
                i++;
            }
            else if (tempBuffer[i] == '+')
            {
                i++;
            }

            int exponentValue = 0;
            while (i < tempBufferLength)
            {
                Debug.Assert(tempBuffer[i] >= (byte)'0' && tempBuffer[i] <= (byte)'9');
                exponentValue = exponentValue * 10 + (tempBuffer[i] - (byte)'0');
                i++;
            }
            exponentValue *= exponentSign;

            //
            // Determine decimal location.
            // 

            if (exponentValue == 0)
            {
                number.scale = 1;
            }
            else
            {
                number.scale = exponentValue + 1;
            }

            //
            // Copy the string from the temp buffer upto precision characters, removing the sign, and decimal as required.
            // 

            i = 0;
            int mantissaIndex = 0;
            while (i < precision && mantissaIndex < exponentIndex)
            {
                if (tempBuffer[mantissaIndex] >= (byte)'0' && tempBuffer[mantissaIndex] <= (byte)'9')
                {
                    dst[i] = (char)tempBuffer[mantissaIndex];
                    i++;
                }
                mantissaIndex++;
            }

            while (i < precision)
            {
                dst[i] = '0'; // append zeros as needed
                i++;
            }

            dst[i] = '\0';

            //
            // Round if needed
            //

            if (mantissaIndex >= exponentIndex || tempBuffer[mantissaIndex] < (byte)'5')
            {
                return; // rounding is not needed
            }

            i = precision - 1;
            while (dst[i] == '9' && i > 0)
            {
                dst[i] = '0';
                i--;
            }

            if (i == 0 && dst[i] == '9')
            {
                dst[i] = '1';
                number.scale++;
            }
            else
            {
                dst[i]++;
            }
        }
    }
}