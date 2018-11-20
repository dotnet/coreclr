// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Buffers;
using System.Text;

internal static partial class Interop
{
    /// <summary>
    /// Helper for making interop calls that return a string, but we don't know
    /// the correct size of buffer to make. So invoke the interop call with an
    /// increasing buffer until the size is big enough.
    /// </summary>
    internal static bool CallStringMethod<TArg1, TArg2, TArg3>(
        SpanFunc<char, TArg1, TArg2, TArg3, Interop.Globalization.ResultCode> interopCall,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out string result)
    {
        const int InitialStringSize = 256;
        const int MaxDoubleAttempts = 3;

        Span<char> buffer = stackalloc char[InitialStringSize];

        for (int i = 0; i < MaxDoubleAttempts; i++)
        {
            Interop.Globalization.ResultCode resultCode = interopCall(buffer, arg1, arg2, arg3);

            if (resultCode == Interop.Globalization.ResultCode.Success)
            {
                int length = buffer.IndexOf('\0');
                Debug.Assert(length >= 0);
                if (length >= 0)
                {
                    buffer = buffer.Slice(0, length);
                }
                result = buffer.ToString();
                return true;
            }
            else if (resultCode == Interop.Globalization.ResultCode.InsufficentBuffer && i < MaxDoubleAttempts - 1)
            {
                // increase the string size and loop
                buffer = new char[buffer.Length * 2];
            }
            else
            {
                // if there is an unknown error (or we're going to exit anyway), don't proceed
                break;
            }
        }

        result = null;
        return false;
    }
}
