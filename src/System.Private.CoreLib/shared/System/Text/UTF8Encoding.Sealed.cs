// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Text
{
    public partial class UTF8Encoding
    {
        /// <summary>
        /// A special instance of <see cref="UTF8Encoding"/> that is initialized with "don't throw on invalid sequences;
        /// perform <see cref="Rune.ReplacementChar"/> substitution instead" semantics. This type allows for devirtualization
        /// of calls made directly off of <see cref="Encoding.UTF8"/>. See https://github.com/dotnet/coreclr/pull/9230.
        /// </summary>
        internal sealed class UTF8EncodingSealed : UTF8Encoding
        {
            public UTF8EncodingSealed(bool encoderShouldEmitUTF8Identifier) : base(encoderShouldEmitUTF8Identifier) { }

            public override ReadOnlySpan<byte> Preamble => _emitUTF8Identifier ? PreambleSpan : default;

            public override byte[] GetBytes(string s)
            {
                // This method is short and can be inlined, meaning that the null check below
                // might be elided if the JIT can prove not-null at the call site.

                if (s != null && s.Length <= 32)
                {
                    return GetBytesForSmallInput(s);
                }
                else
                {
                    return base.GetBytes(s!); // make the base method responsible for the null check
                }
            }

            private unsafe byte[] GetBytesForSmallInput(string s)
            {
                Debug.Assert(s != null);
                Debug.Assert(s.Length <= 32);

                byte* pDestination = stackalloc byte[32 * 3]; // each char produces at most 3 bytes (2-char supplementary code points -> 4 bytes total)

                int sourceLength = s.Length; // hoist this to avoid having the JIT auto-insert null checks
                int bytesWritten;

                fixed (char* pSource = s)
                {
                    bytesWritten = GetBytesCommon(pSource, sourceLength, pDestination, 32 * 3);
                    Debug.Assert(0 <= bytesWritten && bytesWritten <= 32 * 3);
                }

                return new Span<byte>(ref *pDestination, bytesWritten).ToArray(); // this overload of Span ctor doesn't validate length
            }

            public override string GetString(byte[] bytes)
            {
                // This method is short and can be inlined, meaning that the null check below
                // might be elided if the JIT can prove not-null at the call site.

                if (bytes != null && bytes.Length <= 32)
                {
                    return GetStringForSmallInput(bytes);
                }
                else
                {
                    return base.GetString(bytes!); // make the base method responsible for the null check
                }
            }

            private unsafe string GetStringForSmallInput(byte[] bytes)
            {
                Debug.Assert(bytes != null);
                Debug.Assert(bytes.Length <= 32);

                char* pDestination = stackalloc char[32]; // each byte produces at most one char

                int sourceLength = bytes.Length; // hoist this to avoid having the JIT auto-insert null checks
                int charsWritten;

                fixed (byte* pSource = bytes)
                {
                    charsWritten = GetCharsCommon(pSource, sourceLength, pDestination, 32);
                    Debug.Assert(0 <= charsWritten && charsWritten <= 32);
                }

                return new string(new ReadOnlySpan<char>(ref *pDestination, charsWritten)); // this overload of ROS ctor doesn't validate length
            }
        }
    }
}
