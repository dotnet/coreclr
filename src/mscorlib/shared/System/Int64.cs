// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public struct Int64 : IComparable, IConvertible, IFormattable, IComparable<Int64>, IEquatable<Int64>, ISpanFormattable
    {
        private long m_value; // Do not rename (binary serialization)

        public const long MaxValue = 0x7fffffffffffffffL;
        public const long MinValue = unchecked((long)0x8000000000000000L);

        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Int64, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (value is Int64)
            {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                long i = (long)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException(SR.Arg_MustBeInt64);
        }

        public int CompareTo(Int64 value)
        {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Int64))
            {
                return false;
            }
            return m_value == ((Int64)obj).m_value;
        }

        [NonVersionable]
        public bool Equals(Int64 obj)
        {
            return m_value == obj;
        }

        // The value of the lower 32 bits XORed with the uppper 32 bits.
        public override int GetHashCode()
        {
            return (unchecked((int)((long)m_value)) ^ (int)(m_value >> 32));
        }

        public override String ToString()
        {
            return Number.FormatInt64(m_value, null, NumberFormatInfo.CurrentInfo);
        }

        public String ToString(IFormatProvider provider)
        {
            return Number.FormatInt64(m_value, null, NumberFormatInfo.GetInstance(provider));
        }

        public String ToString(String format)
        {
            return Number.FormatInt64(m_value, format, NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format, IFormatProvider provider)
        {
            return Number.FormatInt64(m_value, format, NumberFormatInfo.GetInstance(provider));
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, string format = null, IFormatProvider provider = null)
        {
            return Number.TryFormatInt64(m_value, format, NumberFormatInfo.GetInstance(provider), destination, out charsWritten);
        }

        public static long Parse(String s)
        {
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseInt64(s.AsReadOnlySpan(), NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(String s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseInt64(s.AsReadOnlySpan(), style, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(String s, IFormatProvider provider)
        {
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseInt64(s.AsReadOnlySpan(), NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }


        // Parses a long from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's 
        // NumberFormatInfo is assumed.
        // 
        public static long Parse(String s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseInt64(s.AsReadOnlySpan(), style, NumberFormatInfo.GetInstance(provider));
        }

        public static long Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static Boolean TryParse(String s, out Int64 result)
        {
            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseInt64(s.AsReadOnlySpan(), NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, out long result)
        {
            return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static Boolean TryParse(String s, NumberStyles style, IFormatProvider provider, out Int64 result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);

            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseInt64(s.AsReadOnlySpan(), style, NumberFormatInfo.GetInstance(provider), out result);
        }

        // TODO https://github.com/dotnet/corefx/issues/23642: Remove once corefx has been updated with new overloads.
        public static bool TryParse(ReadOnlySpan<char> s, out long result, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null) =>
            TryParse(s, style, provider, out result);

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider, out long result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        //
        // IConvertible implementation
        // 

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(m_value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return m_value;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value);
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(SR.Format(SR.InvalidCast_FromTo, "Int64", "DateTime"));
        }

        Object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }
    }
}
