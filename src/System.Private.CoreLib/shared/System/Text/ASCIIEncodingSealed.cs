// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text
{
    public partial class ASCIIEncoding
    {
        // This specialized sealed type has two benefits:
        // 1) it allows for devirtualization (see https://github.com/dotnet/coreclr/pull/9230), and
        // 2) it allows us to provide highly optimized implementations of certain routines because
        //    we can make assumptions about the fallback mechanisms in use (in particular, always
        //    replace with "?").
        internal sealed class ASCIIEncodingSealed : ASCIIEncoding
        {
            public override object Clone()
            {
                // The base implementation of Encoding.Clone calls object.MemberwiseClone and marks the new object mutable.
                // We don't want to do this because it violates the invariants we have set for the sealed type.
                // Instead, we'll create a new instance of the base ASCIIEncoding type and mark it mutable.

                return new ASCIIEncoding()
                {
                    IsReadOnly = false
                };
            }

            public override unsafe int GetByteCount(char* chars, int count)
            {
                // Validate Parameters

                if (chars == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars);
                }

                if (count < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                // There's a 1:1 mapping from ASCII text (as chars) to ASCII bytes. Even if there's
                // invalid data in the incoming array, the particular fallback used by this sealed
                // instance ensures that all non-ASCII chars (even surrogate halves) are replaced
                // by a single ASCII "?" byte. So as an optimization we can simply reflect the incoming
                // char count back to the caller.

                return count;
            }

            public override int GetByteCount(char[] chars)
            {
                // Validate parameters

                if (chars is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars);
                }

                // See comment in GetByteCount(char*, int).

                return chars.Length;
            }

            public override int GetByteCount(char[] chars, int index, int count)
            {
                // Validate input parameters

                if (chars is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars);
                }

                if ((index | count) < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (chars.Length - index < count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
                }

                // See comment in GetByteCount(char*, int).

                return count;
            }

            public override int GetByteCount(ReadOnlySpan<char> chars)
            {
                // See comment in GetByteCount(char*, int).

                return chars.Length;
            }

            public override int GetByteCount(string chars)
            {
                // Validate input parameters

                if (chars is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars);
                }

                // See comment in GetByteCount(char*, int).

                return chars.Length;
            }

            public override unsafe int GetCharCount(byte* bytes, int count)
            {
                // Validate Parameters

                if (bytes == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes);
                }

                if (count < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                // See comment in GetByteCount(char*, int) for a description of the chars -> bytes mapping.
                // This mapping also works the other way, where single ASCII bytes expand to single ASCII
                // chars. If we encounter a non-ASCII byte, the particular fallback used by this sealed
                // instance ensures that it's replaced by a single ASCII "?" char in any generated string.
                // So as an optimization we can simply reflect the incoming byte count back to the caller.

                return count;
            }

            public override int GetCharCount(byte[] bytes)
            {
                // Validate parameters

                if (bytes is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes);
                }

                // See comment in GetCharCount(byte*, int).

                return bytes.Length;
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                // Validate input parameters

                if (bytes is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes);
                }

                if ((index | count) < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (bytes.Length - index < count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.bytes, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
                }

                // See comment in GetCharCount(byte*, int).

                return count;
            }

            public override int GetCharCount(ReadOnlySpan<byte> bytes)
            {
                // See comment in GetCharCount(byte*, int).

                return bytes.Length;
            }
        }
    }
}
