// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace System.Text
{
    public struct Utf8Transcoder
    {
        private Utf8StringSegment _remainingUtf8Data;
        private char _lowSurrogate; // UTF-16 low surrogate that still needs to be written to the output
        private bool _fixupInvalidSequence;

        public Utf8Transcoder(Utf8String value, InvalidSequenceBehavior behavior = InvalidSequenceBehavior.ReplaceInvalidSequence)
        {
            if ((uint)behavior >= (uint)InvalidSequenceBehavior.LeaveUnchanged)
            {
                throw new ArgumentOutOfRangeException(nameof(behavior));
            }

            _remainingUtf8Data = value;

            // TODO: Fully implement me.
            throw new NotImplementedException();
        }

        public Utf8Transcoder(Utf8StringSegment value, InvalidSequenceBehavior behavior = InvalidSequenceBehavior.ReplaceInvalidSequence)
        {
            if ((uint)behavior >= (uint)InvalidSequenceBehavior.LeaveUnchanged)
            {
                throw new ArgumentOutOfRangeException(nameof(behavior));
            }

            _remainingUtf8Data = value;

            // TODO: Fully implement me.
            throw new NotImplementedException();
        }

        public bool IsFinished => _remainingUtf8Data.IsEmpty;

        public int ReadNext(Span<char> chars)
        {
            // If the input buffer is already fully consumed, nothing to do.

            if (IsFinished)
            {
                return -1;
            }

            // We're going to fill as much of the output buffer as we can, until
            // one of the following occurs:
            //
            // - We run out of input (EOF).
            // - We run out of space in the output buffer.
            // - We encounter an error in transcoding and are configured to fail.

            if (chars.IsEmpty)
            {
                return 0;
            }

            // It's possible that the last character we were asked to write in the previous
            // iteration was a supplementary character but we only had enough room in the
            // output buffer to emit the leading UTF-16 surrogate. In this case emit the
            // trailing UTF-16 surrogate now.

            if (_lowSurrogate != default)
            {
                chars[0] = _lowSurrogate;
                chars = chars.Slice(1);
                _lowSurrogate = default;
            }

            // TODO: Fill in the remainder of the implementation.
            throw new NotImplementedException();
        }
    }
}
