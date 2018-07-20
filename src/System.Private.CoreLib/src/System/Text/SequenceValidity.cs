// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    /// <summary>
    /// Represents the validity of a UTF-8 or UTF-16 code unit sequence.
    /// </summary>
    public enum SequenceValidity
    {
        /// <summary>
        /// The input sequence is well-formed, i.e., it is an unambiguous representation of a Unicode scalar value.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// The UTF-8 sequence [ CE A9 ] is well-formed because it unambiguously represents the Unicode scalar value U+03A9.
        /// The UTF-8 sequence [ F2 AB B3 9E ] is well-formed because it unambiguously represents the Unicode scalar value U+ABCDE.
        /// </remarks>
        Valid,

        /// <summary>
        /// The input sequence is not well-formed, i.e., it does not correspond to a valid Unicode scalar value.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// The UTF-8 sequence [ C0 ] is not well-formed.
        /// The UTF-8 sequence [ C2 20 ] is not well-formed.
        /// The UTF-8 sequence [ ED A0 80 ] is not well-formed.
        /// </remarks>
        Invalid,

        /// <summary>
        /// The input sequence is incomplete (or empty). It is not valid on its own, but it could be the start of a longer valid
        /// sequence. The caller should more input data if available. If no further input data is available, the sequence should
        /// be treated as not well-formed.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// The UTF-8 sequence [ C2 ] is incomplete.
        /// The UTF-8 sequence [ F2 AB B3 ] is incomplete.
        /// </remarks>
        Incomplete
    }
}
