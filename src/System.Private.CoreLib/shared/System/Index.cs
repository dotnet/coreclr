// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public readonly struct Index
    {
        private readonly int _value;

        public Index(int value, bool fromEnd)
        {
            if (value < 0)
            {
                ThrowHelper.ThrowValueArgumentOutOfRange_NeedNonNegNumException();
            }

            _value = fromEnd ? ~value : value;
        }

        public int Value => _value < 0 ? ~_value : _value;
        public bool FromEnd => _value < 0;

        public override bool Equals(object value)
        {
            if (value is Index)
            {
                return _value == ((Index) value)._value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            return FromEnd ? "^" + Value.ToString() : Value.ToString();
        }

        public static implicit operator Index(int value)
            => new Index(value, fromEnd: false);
    }
}
