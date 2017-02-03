// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Diagnostics.Contracts;

    [Serializable]
    public sealed class EncoderExceptionFallback : EncoderFallback
    {
        // Construction
        public EncoderExceptionFallback()
        {
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new EncoderExceptionFallbackBuffer();
        }

        // Maximum number of characters that this instance of this fallback could return
        public override int MaxCharCount => 0;

        public override bool Equals(Object value)
        {
            EncoderExceptionFallback that = value as EncoderExceptionFallback;
            if (that != null)
            {
                return (true);
            }
            return (false);
        }

        public override int GetHashCode()
        {
            return 654;
        }
    }


    public sealed class EncoderExceptionFallbackBuffer : EncoderFallbackBuffer
    {
        public EncoderExceptionFallbackBuffer(){}
        public override bool Fallback(char charUnknown, int index)
        {
            // Fall back our char
            throw new EncoderFallbackException(
                Environment.GetResourceString("Argument_InvalidCodePageConversionIndex",
                    (int)charUnknown, index), charUnknown, index);
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            if (!Char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownHigh),
                    Environment.GetResourceString("ArgumentOutOfRange_Range",
                    0xD800, 0xDBFF));
            }
            if (!Char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownLow),
                    Environment.GetResourceString("ArgumentOutOfRange_Range",
                    0xDC00, 0xDFFF));
            }
            Contract.EndContractBlock();

            int iTemp = Char.ConvertToUtf32(charUnknownHigh, charUnknownLow);

            // Fall back our char
            throw new EncoderFallbackException(
                Environment.GetResourceString("Argument_InvalidCodePageConversionIndex",
                    iTemp, index), charUnknownHigh, charUnknownLow, index);
        }

        public override char GetNextChar()
        {
            return (char)0;
        }

        public override bool MovePrevious()
        {
            // Exception fallback doesn't have anywhere to back up to.
            return false;
        }

        // Exceptions are always empty
        public override int Remaining => 0;
    }

    [Serializable]
    public sealed class EncoderFallbackException : ArgumentException
    {
        readonly char charUnknown;
        readonly char charUnknownHigh;
        readonly char charUnknownLow;
        readonly int  index;

        public EncoderFallbackException()
            : base(Environment.GetResourceString("Arg_ArgumentException"))
        {
            SetErrorCode(__HResults.COR_E_ARGUMENT);
        }

        public EncoderFallbackException(String message)
            : base(message)
        {
            SetErrorCode(__HResults.COR_E_ARGUMENT);
        }

        public EncoderFallbackException(String message, Exception innerException)
            : base(message, innerException)
        {
            SetErrorCode(__HResults.COR_E_ARGUMENT);
        }

        internal EncoderFallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal EncoderFallbackException(
            String message, char charUnknown, int index) : base(message)
        {
            this.charUnknown = charUnknown;
            this.index = index;
        }

        internal EncoderFallbackException(
            String message, char charUnknownHigh, char charUnknownLow, int index) : base(message)
        {
            if (!Char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownHigh),
                    Environment.GetResourceString("ArgumentOutOfRange_Range",
                    0xD800, 0xDBFF));
            }
            if (!Char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException(nameof(CharUnknownLow),
                    Environment.GetResourceString("ArgumentOutOfRange_Range",
                    0xDC00, 0xDFFF));
            }
            Contract.EndContractBlock();

            this.charUnknownHigh = charUnknownHigh;
            this.charUnknownLow = charUnknownLow;
            this.index = index;
        }

        public char CharUnknown => (charUnknown);

        public char CharUnknownHigh => (charUnknownHigh);

        public char CharUnknownLow => (charUnknownLow);

        public int Index => index;

        // Return true if the unknown character is a surrogate pair.
        public bool IsUnknownSurrogate()
        {
            return (this.charUnknownHigh != '\0');
        }
    }
}
