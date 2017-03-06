// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace System.Text
{
    // This internal class wraps up our normalization behavior

    internal class Normalization
    {
        internal static bool IsNormalized(String strInput, NormalizationForm normForm)
        {
            Debug.Assert(strInput != null);

            // The only way to know if IsNormalizedString failed is through checking the Win32 last error
            // IsNormalizedString pinvoke has SetLastError attribute property which will set the last error 
            // to 0 (ERROR_SUCCESS) before executing the calls.
            bool result = Interop.Normaliz.IsNormalizedString((int)normForm, strInput, strInput.Length);

            int lastError = Marshal.GetLastWin32Error();
            switch (lastError)
            {
                case Interop.Errors.ERROR_SUCCESS:
                    break;

                case Interop.Errors.ERROR_INVALID_PARAMETER:
                case Interop.Errors.ERROR_NO_UNICODE_TRANSLATION:
                    throw new ArgumentException(SR.Argument_InvalidCharSequenceNoIndex, nameof(strInput));

                case Interop.Errors.ERROR_NOT_ENOUGH_MEMORY:
                    throw new OutOfMemoryException(SR.Arg_OutOfMemoryException);

                default:
                    throw new InvalidOperationException(SR.Format(SR.UnknownError_Num, lastError));
            }

            return result;
        }

        internal static String Normalize(String strInput, NormalizationForm normForm)
        {
            Debug.Assert(strInput != null);

            // we depend on Win32 last error when calling NormalizeString
            // NormalizeString pinvoke has SetLastError attribute property which will set the last error 
            // to 0 (ERROR_SUCCESS) before executing the calls.

            // Guess our buffer size first
            int iLength = Interop.Normaliz.NormalizeString((int)normForm, strInput, strInput.Length, null, 0);

            int lastError = Marshal.GetLastWin32Error();
            // Could have an error (actually it'd be quite hard to have an error here)
            if ((lastError != Interop.Errors.ERROR_SUCCESS) || iLength < 0)
            {
                if (lastError == Interop.Errors.ERROR_INVALID_PARAMETER)
                    throw new ArgumentException(SR.Argument_InvalidCharSequenceNoIndex, nameof(strInput));

                // We shouldn't really be able to get here..., guessing length is
                // a trivial math function...
                // Can't really be Out of Memory, but just in case:
                if (lastError == Interop.Errors.ERROR_NOT_ENOUGH_MEMORY)
                    throw new OutOfMemoryException(SR.Arg_OutOfMemoryException);

                // Who knows what happened?  Not us!
                throw new InvalidOperationException(SR.Format(SR.UnknownError_Num, lastError));
            }

            // Don't break for empty strings (only possible for D & KD and not really possible at that)
            if (iLength == 0) return string.Empty;

            // Someplace to stick our buffer
            char[] cBuffer = null;

            for (;;)
            {
                // (re)allocation buffer and normalize string
                cBuffer = new char[iLength];

                // NormalizeString pinvoke has SetLastError attribute property which will set the last error 
                // to 0 (ERROR_SUCCESS) before executing the calls.
                iLength = Interop.Normaliz.NormalizeString((int)normForm, strInput, strInput.Length, cBuffer, cBuffer.Length);
                lastError = Marshal.GetLastWin32Error();

                if (lastError == Interop.Errors.ERROR_SUCCESS)
                    break;

                // Could have an error (actually it'd be quite hard to have an error here)
                switch (lastError)
                {
                    // Do appropriate stuff for the individual errors:
                    case Interop.Errors.ERROR_INSUFFICIENT_BUFFER:
                        iLength = Math.Abs(iLength);
                        Debug.Assert(iLength > cBuffer.Length, "Buffer overflow should have iLength > cBuffer.Length");
                        continue;

                    case Interop.Errors.ERROR_INVALID_PARAMETER:
                    case Interop.Errors.ERROR_NO_UNICODE_TRANSLATION:
                        // Illegal code point or order found.  Ie: FFFE or D800 D800, etc.
                        throw new ArgumentException(SR.Argument_InvalidCharSequenceNoIndex, nameof(strInput));

                    case Interop.Errors.ERROR_NOT_ENOUGH_MEMORY:
                        throw new OutOfMemoryException(SR.Arg_OutOfMemoryException);

                    default:
                        // We shouldn't get here...
                        throw new InvalidOperationException(SR.Format(SR.UnknownError_Num, lastError));
                }
            }

            // Copy our buffer into our new string, which will be the appropriate size
            return new string(cBuffer, 0, iLength);
        }
    }
}
