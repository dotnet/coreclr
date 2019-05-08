// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System;
using System.Buffers;
using System.Text;

namespace Internal.Runtime
{
    internal class Utf8Utilities
    {
        internal unsafe static int ToLowerInvariant(byte* outBuffer, int cOutBuffer, byte* inBuffer)
        {
            if (cOutBuffer != 0 && outBuffer == null)
            {
                return -Interop.Errors.ERROR_INVALID_PARAMETER;
            }

            return ToLowerInvariant(
                new Span<byte>(outBuffer, cOutBuffer),
                new ReadOnlySpan<byte>(inBuffer, string.strlen(inBuffer)));
        }

        private static int ToLowerInvariant(Span<byte> output, ReadOnlySpan<byte> input)
        {
            Span<byte> lowerRuneBytes = stackalloc byte[4];
            int result = 0;
            Span<byte> originalOutput = output;

            while (!input.IsEmpty)
            {
                if (Rune.DecodeFromUtf8(input, out Rune thisRune, out int bytesConsumed) != OperationStatus.Done)
                {
                    return -Interop.Errors.ERROR_NO_UNICODE_TRANSLATION;
                }

                input = input.Slice(bytesConsumed);

                Rune lowerRune = Rune.ToLowerInvariant(thisRune);
                int cLowerRuneBytes = lowerRune.EncodeToUtf8(lowerRuneBytes);

                result += cLowerRuneBytes;

                if (!originalOutput.IsEmpty)
                {
                    if (output.Length < cLowerRuneBytes)
                    {
                        return -Interop.Errors.ERROR_INSUFFICIENT_BUFFER;
                    }

                    lowerRuneBytes.Slice(0, cLowerRuneBytes).CopyTo(output);
                    output = output.Slice(cLowerRuneBytes);
                }
            }

            // null-terminate
            result++;

            if (!originalOutput.IsEmpty)
            {
                if (output.Length < 1)
                {
                    return -Interop.Errors.ERROR_INSUFFICIENT_BUFFER;
                }

                output[0] = 0;
            }

            return result;
        }
    }

}
