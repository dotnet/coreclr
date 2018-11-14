// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public sealed partial class String
    {
        // An enumerator for retrieving System.Text.Rune instances from a System.String.
        // This is a nested type under string, which should be ok since we imagine most callers will
        // never see the type name. It also matches things like List<T>.Enumerator.
        public struct RuneEnumerator : IEnumerable<Rune>, IEnumerator<Rune>
        {
            private readonly string _string;
            private Rune _current;
            private int _nextIndex;

            internal RuneEnumerator(string value)
            {
                _string = value;
                _current = default;
                _nextIndex = 0;
            }

            public Rune Current => _current;

            public RuneEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                if ((uint)_nextIndex >= _string.Length)
                {
                    // reached the end of the string
                    _current = default;
                    return false;
                }

                if (!Rune.TryGetRuneAt(_string, _nextIndex, out _current))
                {
                    // replace invalid sequences with U+FFFD
                    _current = Rune.ReplacementChar;
                }

                // In UTF-16 specifically, invalid sequences always have length 1, which is the same
                // length as the replacement character U+FFFD. This means that we can always bump the
                // next index by the current scalar's UTF-16 sequence length. This optimization is not
                // generally applicable; for example, enumerating scalars from UTF-8 cannot utilize
                // this same trick.

                _nextIndex += _current.Utf16SequenceLength;
                return true;
            }

            object IEnumerator.Current => _current;

            void IDisposable.Dispose()
            {
                // no-op
            }

            IEnumerator IEnumerable.GetEnumerator() => this;

            IEnumerator<Rune> IEnumerable<Rune>.GetEnumerator() => this;

            void IEnumerator.Reset()
            {
                _current = default;
                _nextIndex = 0;
            }
        }
    }
}
