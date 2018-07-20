// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    /// <summary>
    /// Specifies how to handle invalid sequences when processing text.
    /// </summary>
    public enum InvalidSequenceBehavior
    {
        /// <summary>
        /// Immediately stops processing the buffer when an invalid sequence is detected,
        /// instead reporting failure to the caller.
        /// </summary>
        /// <remarks>
        /// Failure reporting may take different forms depending on the method being called.
        /// Some methods may choose to return a failure status code, while other methods may throw an exception.
        /// </remarks>
        Fail,

        /// <summary>
        /// Replaces invalid sequences with <see cref="UnicodeScalar.ReplacementChar"/> (U+FFFD)
        /// and continues processing the remainder of the buffer.
        /// </summary>
        ReplaceInvalidSequence,

        /// <summary>
        /// Leaves invalid sequences unchanged and continues processing the remainder of the buffer.
        /// </summary>
        /// <remarks>
        /// This value has security implications and should only be used when the caller is prepared to handle potentially
        /// invalid sequences in a secure and predictable fashion. This value cannot be specified for transcoding operations;
        /// e.g., conversions between UTF-8 and UTF-16.
        /// </remarks>
        LeaveUnchanged,
    }
}
