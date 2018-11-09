// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public readonly struct Index
    {
        private readonly int _value;

        public int Value => _value < 0 ? ~_value : _value;
        public bool FromEnd => _value < 0;

        public Index(int value, bool fromEnd)
        {
            if (value < 0)
            {
                throw new ArgumentException(SR.ArgumentOutOfRange_NeedNonNegNum, nameof(value));
            }

            _value = fromEnd ? ~value : value;
        }

        public static implicit operator Index(int value)
            => new Index(value, fromEnd: false);
    }
}
