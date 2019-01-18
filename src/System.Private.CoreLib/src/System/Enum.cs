// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Internal.Runtime.CompilerServices;

namespace System
{
    [Serializable]
    [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible
    {
        #region Private Constants
        private const char EnumSeparatorChar = ',';
        #endregion

        #region Private Static Methods
        internal static IEnumCache GetCache(Type enumType)
        {
            if (!(enumType is RuntimeType rtType))
            {
                return ThrowGetCacheException(enumType);
            }

            if (!rtType.IsEnum)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, nameof(enumType));
            }

            return GetCache(rtType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumCache ThrowGetCacheException(Type enumType)
        {
            Debug.Assert(!(enumType is RuntimeType));

            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, nameof(enumType));
            }

            throw new ArgumentException(SR.Arg_MustBeType, nameof(enumType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumCache GetCache(RuntimeType rtType)
        {
            Debug.Assert(rtType.IsEnum);

            return rtType.GenericCache as IEnumCache ?? InitializeCache(rtType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumCache InitializeCache(RuntimeType rtType)
        {
            IEnumCache cache = (IEnumCache)typeof(EnumCache<>).MakeGenericType(rtType).GetField(nameof(EnumCache<DayOfWeek>.Cache), BindingFlags.Static | BindingFlags.Public).GetValue(null) ?? throw new ArgumentException(SR.Argument_InvalidEnum, "enumType");
            rtType.GenericCache = cache;
            return cache;
        }

        private static IEnumCache CreateEnumCache(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                return null;
            }

            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields?.Length != 1)
            {
                return null;
            }

            Type underlyingType = fields[0].FieldType;
            
            // Allow underlying type of another enum type as is done in a System.Reflection.Emit test
            if (underlyingType.IsEnum && underlyingType != enumType)
            {
                underlyingType = GetUnderlyingType(underlyingType);
            }

            return (IEnumCache)Activator.CreateInstance(typeof(EnumCache<,,>).MakeGenericType(enumType, underlyingType, typeof(UnderlyingOperations)));
        }

        internal static ulong ToUInt64(object value, bool throwInvalidOperationException = true)
        {
            Debug.Assert(value != null);

            // Helper function to silently convert the value to UInt64 from the other base types for enum without throwing an exception.
            // This is needed since the Convert functions do overflow checks.
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                    return (ulong)(sbyte)value;
                case TypeCode.Byte:
                    return (byte)value;
                case TypeCode.Boolean:
                    // direct cast from bool to byte is not allowed
                    return Convert.ToByte((bool)value);
                case TypeCode.Int16:
                    return (ulong)(short)value;
                case TypeCode.UInt16:
                    return (ushort)value;
                case TypeCode.Char:
                    return (char)value;
                case TypeCode.UInt32:
                    return (uint)value;
                case TypeCode.Int32:
                    return (ulong)(int)value;
                case TypeCode.UInt64:
                    return (ulong)value;
                case TypeCode.Int64:
                    return (ulong)(long)value;
                case TypeCode.Single:
                    return (ulong)BitConverter.SingleToInt32Bits((float)value);
                case TypeCode.Double:
                    return (ulong)BitConverter.DoubleToInt64Bits((double)value);
                // All unsigned types will be directly cast
                default:
                    Type type = value.GetType();
                    if (type == typeof(IntPtr))
                    {
                        return (ulong)(long)(IntPtr)value;
                    }
                    if (type == typeof(UIntPtr))
                    {
                        return (ulong)(UIntPtr)value;
                    }
                    if (throwInvalidOperationException)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
                    }
                    throw new ArgumentException(SR.Arg_MustBeEnumBaseTypeOrEnum, nameof(value));
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int InternalCompareTo(object o1, object o2);
        #endregion

        #region Public Static Methods
        public static object Parse(Type enumType, string value) =>
            Parse(enumType, value, ignoreCase: false);

        public static object Parse(Type enumType, string value, bool ignoreCase)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return GetCache(enumType).Parse(value, ignoreCase);
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct =>
            Parse<TEnum>(value, ignoreCase: false);

        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            IEnumCache<TEnum> cache = EnumCache<TEnum>.Cache;
            if (cache == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            return cache.Parse(value, ignoreCase);
        }

        public static bool TryParse(Type enumType, string value, out object result) =>
            TryParse(enumType, value, ignoreCase: false, out result);

        public static bool TryParse(Type enumType, string value, bool ignoreCase, out object result) =>
            GetCache(enumType).TryParse(value, ignoreCase, out result);

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct =>
            TryParse(value, ignoreCase: false, out result);

        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
            IEnumCache<TEnum> cache = EnumCache<TEnum>.Cache;
            if (cache == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            return cache.TryParse(value, ignoreCase, out result);
        }

        public static Type GetUnderlyingType(Type enumType) =>
            GetCache(enumType).UnderlyingType;

        public static Array GetValues(Type enumType) =>
            GetCache(enumType).GetValues();

        public static string GetName(Type enumType, object value) =>
            GetCache(enumType).GetName(value);

        public static string[] GetNames(Type enumType) =>
            GetCache(enumType).GetNames();

        public static object ToObject(Type enumType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return GetCache(enumType).ToObject(value);
        }

        public static bool IsDefined(Type enumType, object value) =>
            GetCache(enumType).IsDefined(value);

        public static string Format(Type enumType, object value, string format)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return GetCache(enumType).Format(value, format);
        }
        #endregion

        #region Definitions
        #region EnumCache
        internal static class EnumCache<TEnum>
        {
            public static readonly IEnumCache<TEnum> Cache = (IEnumCache<TEnum>)CreateEnumCache(typeof(TEnum));
        }

        internal interface IEnumCache
        {
            Type UnderlyingType { get; }

            string Format(object value, string format);
            string GetName(object value);
            string[] GetNames();
            object GetUnderlyingValue(ref byte value);
            Array GetValues();
            bool HasFlag(ref byte value, object flag);
            bool IsDefined(object value);
            object Parse(ReadOnlySpan<char> value, bool ignoreCase);
            bool ToBoolean(ref byte value);
            byte ToByte(ref byte value);
            char ToChar(ref byte value);
            decimal ToDecimal(ref byte value);
            double ToDouble(ref byte value);
            short ToInt16(ref byte value);
            int ToInt32(ref byte value);
            long ToInt64(ref byte value);
            object ToObject(object value);
            object ToObject(ulong value);
            sbyte ToSByte(ref byte value);
            float ToSingle(ref byte value);
            string ToString(ref byte value);
            string ToString(ref byte value, string format);
            ushort ToUInt16(ref byte value);
            uint ToUInt32(ref byte value);
            ulong ToUInt64(ref byte value);
            bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out object result);
        }

        internal interface IEnumCache<TEnum> : IEnumCache
        {
            int CompareTo(TEnum value, TEnum other);
            bool Equals(TEnum value, TEnum other);
            int GetHashCode(TEnum value);
            new TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase);
            bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result);
        }

        private sealed class EnumCache<TEnum, TUnderlying, TUnderlyingOperations> : IEnumCache<TEnum>
            where TEnum : struct, Enum
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            private static TUnderlyingOperations Operations
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => default;
            }

            private static EnumMembers<TUnderlying, TUnderlyingOperations> s_members;

            // Lazy caching of members
            private static EnumMembers<TUnderlying, TUnderlyingOperations> Members
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_members ?? InitializeMembers();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static EnumMembers<TUnderlying, TUnderlyingOperations> InitializeMembers()
            {
                EnumMembers<TUnderlying, TUnderlyingOperations> members = new EnumMembers<TUnderlying, TUnderlyingOperations>(typeof(TEnum));
                return Interlocked.CompareExchange(ref s_members, members, null) ?? members;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TUnderlying ToUnderlying(TEnum value) => Unsafe.As<TEnum, TUnderlying>(ref value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TUnderlying ToUnderlying(ref byte value) => Unsafe.As<byte, TUnderlying>(ref value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TEnum ToEnum(TUnderlying value) => Unsafe.As<TUnderlying, TEnum>(ref value);

            private static TEnum ToEnum(object value)
            {
                Debug.Assert(value != null);

                if (!(value is TEnum enumValue))
                {
                    throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, value.GetType(), typeof(TEnum)));
                }
                return enumValue;
            }

            public Type UnderlyingType { get; } = typeof(TUnderlying);

            public TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase) => ToEnum(Members.Parse(value, ignoreCase));

            public bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
            {
                bool success = Members.TryParse(value, ignoreCase, out TUnderlying underlying);
                result = ToEnum(underlying);
                return success;
            }

            public int CompareTo(TEnum value, TEnum other) => Operations.CompareTo(ToUnderlying(value), ToUnderlying(other));

            public bool Equals(TEnum value, TEnum other) => ToUnderlying(value).Equals(ToUnderlying(other));

            public int GetHashCode(TEnum value) => ToUnderlying(value).GetHashCode();

            public string Format(object value, string format)
            {
                Debug.Assert(value != null);

                return Members.Format(value is TUnderlying underlyingValue ? underlyingValue : ToUnderlying(ToEnum(value)), format);
            }

            public string GetName(object value) => value is TEnum enumValue ? Members.GetName(ToUnderlying(enumValue)) : Members.GetName(value);

            public string[] GetNames() => Members.GetNames();

            public object GetUnderlyingValue(ref byte value) => ToUnderlying(ref value);

            public Array GetValues()
            {
                TUnderlying[] values = Members._values;
                TEnum[] array = new TEnum[values.Length];
                int i = 0;
                foreach (TUnderlying value in values)
                {
                    array[i++] = ToEnum(value);
                }
                return array;
            }

            public bool HasFlag(ref byte value, object flag)
            {
                Debug.Assert(flag != null);

                TUnderlying underlyingFlag = ToUnderlying(ToEnum(flag));
                return Operations.And(ToUnderlying(ref value), underlyingFlag).Equals(underlyingFlag);
            }

            public bool IsDefined(object value) => value is TEnum enumValue ? Members.IsDefined(ToUnderlying(enumValue)) : Members.IsDefined(value);

            object IEnumCache.Parse(ReadOnlySpan<char> value, bool ignoreCase) => ToEnum(Members.Parse(value, ignoreCase));

            public bool ToBoolean(ref byte value) => Operations.ToBoolean(ToUnderlying(ref value));

            public byte ToByte(ref byte value) => Operations.ToByte(ToUnderlying(ref value));

            public char ToChar(ref byte value) => Operations.ToChar(ToUnderlying(ref value));

            public decimal ToDecimal(ref byte value) => Operations.ToDecimal(ToUnderlying(ref value));

            public double ToDouble(ref byte value) => Operations.ToDouble(ToUnderlying(ref value));

            public short ToInt16(ref byte value) => Operations.ToInt16(ToUnderlying(ref value));

            public int ToInt32(ref byte value) => Operations.ToInt32(ToUnderlying(ref value));

            public long ToInt64(ref byte value) => Operations.ToInt64(ToUnderlying(ref value));

            public object ToObject(object value)
            {
                Debug.Assert(value != null);

                return ToEnum(Operations.ToObject(Enum.ToUInt64(value, throwInvalidOperationException: false)));
            }

            public object ToObject(ulong value) => ToEnum(Operations.ToObject(value));

            public sbyte ToSByte(ref byte value) => Operations.ToSByte(ToUnderlying(ref value));

            public float ToSingle(ref byte value) => Operations.ToSingle(ToUnderlying(ref value));

            public string ToString(ref byte value) => Members.ToString(ToUnderlying(ref value));

            public string ToString(ref byte value, string format) => Members.ToString(ToUnderlying(ref value), format);

            public ushort ToUInt16(ref byte value) => Operations.ToUInt16(ToUnderlying(ref value));

            public uint ToUInt32(ref byte value) => Operations.ToUInt32(ToUnderlying(ref value));

            public ulong ToUInt64(ref byte value) => Operations.ToUInt64(ToUnderlying(ref value));

            public bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out object result)
            {
                bool success = Members.TryParse(value, ignoreCase, out TUnderlying underlyingResult);
                result = success ? (object)ToEnum(underlyingResult) : null;
                return success;
            }
        }
        #endregion

        #region EnumMembers
        private sealed class EnumMembers<TUnderlying, TUnderlyingOperations>
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            private readonly Type _enumType;
            private readonly bool _isFlagEnum;
            private readonly string[] _names;
            internal readonly TUnderlying[] _values;
            private readonly TUnderlying _min;
            private readonly TUnderlying _max;
            private readonly bool _isContiguous;

            public EnumMembers(Type enumType)
            {
                _enumType = enumType;
                _isFlagEnum = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

                FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

                string[] names = new string[fields.Length];
                TUnderlying[] values = new TUnderlying[fields.Length];
                TUnderlyingOperations operations = default;
                IComparer<TUnderlying> unsignedComparer = operations.UnsignedComparer;
                TUnderlying max = default;
                TUnderlying min = default;
                for (int i = 0; i < fields.Length; ++i)
                {
                    TUnderlying value = (TUnderlying)fields[i].GetRawConstantValue();
                    if (i == 0)
                    {
                        max = value;
                        min = value;
                    }
                    int index = ArraySortHelper<TUnderlying>.Default.BinarySearch(values, 0, i, value, unsignedComparer);
                    if (index < 0)
                    {
                        index = ~index;
                        if (operations.LessThan(value, min))
                        {
                            min = value;
                        }
                        else if (operations.LessThan(max, value))
                        {
                            max = value;
                        }
                    }
                    else
                    {
                        ++index;
                        while (index < i && value.Equals(values[index]))
                        {
                            ++index;
                        }
                    }

                    Array.Copy(names, index, names, index + 1, i - index);
                    Array.Copy(values, index, values, index + 1, i - index);

                    names[index] = fields[i].Name;
                    values[index] = value;
                }
                _names = names;
                _values = values;
                _max = max;
                _min = min;
                _isContiguous = values.Length > 0 && operations.Subtract(max, operations.ToObject((ulong)values.Length - 1)).Equals(min);
            }

            public string GetName(TUnderlying value)
            {
                int index = IndexOf(_values, value);
                if (index >= 0)
                {
                    return _names[index];
                }
                return null;
            }

            public string GetName(object value)
            {
                Debug.Assert(value?.GetType() != _enumType);
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                ulong uint64Value = ToUInt64(value, throwInvalidOperationException: false);
                TUnderlyingOperations operations = default;
                return operations.IsInValueRange(uint64Value) ? GetName(operations.ToObject(uint64Value)) : null;
            }

            public string[] GetNames()
            {
                string[] names = _names;
                string[] namesCopy = new string[names.Length];
                Array.Copy(names, 0, namesCopy, 0, names.Length);
                return namesCopy;
            }

            public bool IsDefined(TUnderlying value)
            {
                TUnderlyingOperations operations = default;
                return _isContiguous ? !(operations.LessThan(value, _min) || operations.LessThan(_max, value)) : IndexOf(_values, value) >= 0;
            }

            public bool IsDefined(object value)
            {
                Debug.Assert(value?.GetType() != _enumType);

                switch (value)
                {
                    case TUnderlying underlyingValue:
                        return IsDefined(underlyingValue);
                    case string str:
                        return Array.IndexOf(_names, str) >= 0;
                    default:
                        return ThrowIsDefinedException(value);
                }
            }

            private bool ThrowIsDefinedException(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Type valueType = value.GetType();

                // Check if is another type of enum as checking for the current enum type is handled in EnumBridge
                if (valueType.IsEnum)
                {
                    throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, valueType.ToString(), _enumType.ToString()));
                }

                TypeCode typeCode = Convert.GetTypeCode(value);

                switch (typeCode)
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                        throw new ArgumentException(SR.Format(SR.Arg_EnumUnderlyingTypeAndObjectMustBeSameType, valueType.ToString(), typeof(TUnderlying).ToString()));
                    default:
                        throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
                }
            }

            public string ToString(TUnderlying value)
            {
                if (_isFlagEnum)
                {
                    return ToStringFlags(value);
                }
                int index = IndexOf(_values, value);
                if (index >= 0)
                {
                    return _names[index];
                }
                return value.ToString();
            }

            public string ToString(TUnderlying value, string format)
            {
                char formatCh;
                if (format == null || format.Length == 0)
                {
                    formatCh = 'G';
                }
                else if (format.Length != 1)
                {
                    throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
                }
                else
                {
                    formatCh = format[0];
                }

                return FormatInternal(value, formatCh);
            }

            private string FormatInternal(TUnderlying value, char formatCh)
            {
                switch (formatCh)
                {
                    case 'G':
                    case 'g':
                        return ToString(value);
                    case 'D':
                    case 'd':
                        return value.ToString();
                    case 'X':
                    case 'x':
                        return default(TUnderlyingOperations).ToHexStr(value);
                    case 'F':
                    case 'f':
                        return ToStringFlags(value);
                    default:
                        throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
                }
            }

            private string ToStringFlags(TUnderlying value)
            {
                string[] names = _names;
                TUnderlying[] values = _values;
                TUnderlyingOperations operations = default;
                // Values are sorted, so if the incoming value is 0, we can check to see whether
                // the first entry matches it, in which case we can return its name; otherwise,
                // we can just return "0".
                if (value.Equals(operations.Zero))
                {
                    return values.Length > 0 && values[0].Equals(operations.Zero) ?
                        names[0] :
                        value.ToString();
                }

                // It's common to have a flags enum with a single value that matches a single entry,
                // in which case we can just return the existing name string.
                int index = IndexOf(values, value);
                if (index >= 0)
                {
                    return names[index];
                }

                // With a ulong result value, regardless of the enum's base type, the maximum
                // possible number of consistent name/values we could have is 64, since every
                // value is made up of one or more bits, and when we see values and incorporate
                // their names, we effectively switch off those bits.
                Span<int> foundItems = stackalloc int[64];

                // Now look for multiple matches, storing the indices of the values
                // into our span.
                int resultLength = 0;
                int foundItemsCount = 0;
                TUnderlying tempValue = value;
                for (int i = ~index - 1; i >= 0; --i)
                {
                    TUnderlying currentValue = values[i];
                    if (operations.And(tempValue, currentValue).Equals(currentValue))
                    {
                        tempValue = operations.Subtract(tempValue, currentValue);
                        foundItems[foundItemsCount++] = i;
                        resultLength = checked(resultLength + names[i].Length);
                        if (tempValue.Equals(operations.Zero))
                        {
                            break;
                        }
                    }
                }

                // If we exhausted looking through all the values and we still have
                // a non-zero result, we couldn't match the result to only named values.
                // In that case, we return a string for the integral value.
                if (!tempValue.Equals(operations.Zero))
                {
                    return value.ToString();
                }

                // We know what strings to concatenate.  Do so.

                Debug.Assert(foundItemsCount > 0);
                const int SeparatorStringLength = 2; // ", "
                string result = string.FastAllocateString(checked(resultLength + (SeparatorStringLength * (foundItemsCount - 1))));

                Span<char> resultSpan = MemoryMarshal.CreateSpan(ref result.GetRawStringData(), result.Length);
                string name = names[foundItems[--foundItemsCount]];
                name.AsSpan().CopyTo(resultSpan);
                resultSpan = resultSpan.Slice(name.Length);
                while (--foundItemsCount >= 0)
                {
                    resultSpan[0] = EnumSeparatorChar;
                    resultSpan[1] = ' ';
                    resultSpan = resultSpan.Slice(2);

                    name = names[foundItems[foundItemsCount]];
                    name.AsSpan().CopyTo(resultSpan);
                    resultSpan = resultSpan.Slice(name.Length);
                }
                Debug.Assert(resultSpan.IsEmpty);

                return result;
            }

            public string Format(TUnderlying value, string format)
            {
                if (format == null)
                {
                    throw new ArgumentNullException(nameof(format));
                }
                if (format.Length != 1)
                {
                    throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
                }

                return FormatInternal(value, format[0]);
            }

            public TUnderlying Parse(ReadOnlySpan<char> value, bool ignoreCase)
            {
                value = value.TrimStart();

                Number.ParsingStatus status = TryParseInternal(value, ignoreCase, out TUnderlying result);
                if (status == Number.ParsingStatus.OK)
                {
                    return result;
                }
                return ThrowParseException(value, status);
            }

            private static TUnderlying ThrowParseException(ReadOnlySpan<char> value, Number.ParsingStatus status)
            {
                if (status == Number.ParsingStatus.Overflow)
                {
                    throw new OverflowException(default(TUnderlyingOperations).OverflowMessage);
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.Arg_MustContainEnumInfo);
                }
                throw new ArgumentException(SR.Arg_EnumValueNotFound, nameof(value));
            }

            public bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TUnderlying result) => TryParseInternal(value.TrimStart(), ignoreCase, out result) == Number.ParsingStatus.OK;

            private Number.ParsingStatus TryParseInternal(ReadOnlySpan<char> value, bool ignoreCase, out TUnderlying result)
            {
                Number.ParsingStatus status = Number.ParsingStatus.Failed;
                TUnderlying localResult = default;
                if (value.Length > 0)
                {
                    TUnderlyingOperations operations = default;
                    char firstNonWhitespaceChar = value[0];
                    if (char.IsInRange(firstNonWhitespaceChar, '0', '9') || firstNonWhitespaceChar == '-' || firstNonWhitespaceChar == '+')
                    {
                        status = operations.TryParse(value, out localResult);
                        if (status != Number.ParsingStatus.Failed)
                        {
                            result = status == Number.ParsingStatus.OK ? localResult : default;
                            return status;
                        }
                    }

                    string[] names = _names;
                    TUnderlying[] values = _values;
                    do
                    {
                        status = Number.ParsingStatus.Failed;
                        // Find the next separator.
                        ReadOnlySpan<char> subvalue;
                        int endIndex = value.IndexOf(EnumSeparatorChar);
                        if (endIndex == -1)
                        {
                            subvalue = value.Trim();
                            value = default;
                        }
                        else if (endIndex != value.Length - 1)
                        {
                            // Found a separator before the last char.
                            subvalue = value.Slice(0, endIndex).Trim();
                            value = value.Slice(endIndex + 1);
                        }
                        else
                        {
                            // Last char was a separator, which is invalid.
                            break;
                        }

                        // Try to match this substring against each enum name
                        if (ignoreCase)
                        {
                            for (int i = 0; i < names.Length; i++)
                            {
                                if (subvalue.EqualsOrdinalIgnoreCase(names[i]))
                                {
                                    localResult = operations.Or(localResult, values[i]);
                                    status = Number.ParsingStatus.OK;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < names.Length; i++)
                            {
                                if (subvalue.EqualsOrdinal(names[i]))
                                {
                                    localResult = operations.Or(localResult, values[i]);
                                    status = Number.ParsingStatus.OK;
                                    break;
                                }
                            }
                        }
                    } while (value.Length > 0 && status == Number.ParsingStatus.OK);
                }

                result = status == Number.ParsingStatus.OK ? localResult : default;
                return status;
            }

            private static int IndexOf(TUnderlying[] values, TUnderlying value) => ArraySortHelper<TUnderlying>.Default.BinarySearch(values, 0, values.Length, value, default(TUnderlyingOperations).UnsignedComparer);
        }
        #endregion

        #region UnderlyingOperations
        private interface IUnderlyingOperations<TUnderlying>
            where TUnderlying : struct, IEquatable<TUnderlying>
        {
            TUnderlying Zero { get; }
            string OverflowMessage { get; }
            IComparer<TUnderlying> UnsignedComparer { get; }
            TUnderlying And(TUnderlying left, TUnderlying right);
            int CompareTo(TUnderlying left, TUnderlying right);
            bool IsInValueRange(ulong value);
            bool LessThan(TUnderlying left, TUnderlying right);
            TUnderlying Or(TUnderlying left, TUnderlying right);
            TUnderlying Subtract(TUnderlying left, TUnderlying right);
            bool ToBoolean(TUnderlying value);
            byte ToByte(TUnderlying value);
            char ToChar(TUnderlying value);
            decimal ToDecimal(TUnderlying value);
            double ToDouble(TUnderlying value);
            string ToHexStr(TUnderlying value);
            short ToInt16(TUnderlying value);
            int ToInt32(TUnderlying value);
            long ToInt64(TUnderlying value);
            TUnderlying ToObject(ulong value);
            sbyte ToSByte(TUnderlying value);
            float ToSingle(TUnderlying value);
            ushort ToUInt16(TUnderlying value);
            uint ToUInt32(TUnderlying value);
            ulong ToUInt64(TUnderlying value);
            ulong ToUInt64Unchecked(TUnderlying value);
            Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out TUnderlying result);
        }

        private readonly struct UnderlyingOperations : IUnderlyingOperations<byte>, IUnderlyingOperations<sbyte>, IUnderlyingOperations<short>, IUnderlyingOperations<ushort>, IUnderlyingOperations<int>, IUnderlyingOperations<uint>, IUnderlyingOperations<long>, IUnderlyingOperations<ulong>, IUnderlyingOperations<bool>, IUnderlyingOperations<char>, IUnderlyingOperations<float>, IUnderlyingOperations<double>, IUnderlyingOperations<IntPtr>, IUnderlyingOperations<UIntPtr>
        {
            #region Zero
            byte IUnderlyingOperations<byte>.Zero => 0;

            sbyte IUnderlyingOperations<sbyte>.Zero => 0;

            short IUnderlyingOperations<short>.Zero => 0;

            ushort IUnderlyingOperations<ushort>.Zero => 0;

            int IUnderlyingOperations<int>.Zero => 0;

            uint IUnderlyingOperations<uint>.Zero => 0;

            long IUnderlyingOperations<long>.Zero => 0;

            ulong IUnderlyingOperations<ulong>.Zero => 0;

            bool IUnderlyingOperations<bool>.Zero => false;

            char IUnderlyingOperations<char>.Zero => (char)0;

            float IUnderlyingOperations<float>.Zero => default;

            double IUnderlyingOperations<double>.Zero => default;

            IntPtr IUnderlyingOperations<IntPtr>.Zero => IntPtr.Zero;

            UIntPtr IUnderlyingOperations<UIntPtr>.Zero => UIntPtr.Zero;
            #endregion

            #region OverflowMessage
            string IUnderlyingOperations<byte>.OverflowMessage => SR.Overflow_Byte;

            string IUnderlyingOperations<sbyte>.OverflowMessage => SR.Overflow_SByte;

            string IUnderlyingOperations<short>.OverflowMessage => SR.Overflow_Int16;

            string IUnderlyingOperations<ushort>.OverflowMessage => SR.Overflow_UInt16;

            string IUnderlyingOperations<int>.OverflowMessage => SR.Overflow_Int32;

            string IUnderlyingOperations<uint>.OverflowMessage => SR.Overflow_UInt32;

            string IUnderlyingOperations<long>.OverflowMessage => SR.Overflow_Int64;

            string IUnderlyingOperations<ulong>.OverflowMessage => SR.Overflow_UInt64;

            string IUnderlyingOperations<bool>.OverflowMessage => null;

            string IUnderlyingOperations<char>.OverflowMessage => null;

            string IUnderlyingOperations<float>.OverflowMessage => null;

            string IUnderlyingOperations<double>.OverflowMessage => null;

            string IUnderlyingOperations<IntPtr>.OverflowMessage
            {
                get
                {
#if BIT64
                    return SR.Overflow_Int64;
#else
                    return SR.Overflow_Int32;
#endif
                }
            }

            string IUnderlyingOperations<UIntPtr>.OverflowMessage
            {
                get
                {
#if BIT64
                    return SR.Overflow_UInt64;
#else
                    return SR.Overflow_UInt32;
#endif
                }
            }
            #endregion

            #region UnsignedComparer
            IComparer<byte> IUnderlyingOperations<byte>.UnsignedComparer => Comparer<byte>.Default;

            IComparer<sbyte> IUnderlyingOperations<sbyte>.UnsignedComparer => UnsignedComparer<sbyte, UnderlyingOperations>.Default;

            IComparer<short> IUnderlyingOperations<short>.UnsignedComparer => UnsignedComparer<short, UnderlyingOperations>.Default;

            IComparer<ushort> IUnderlyingOperations<ushort>.UnsignedComparer => Comparer<ushort>.Default;

            IComparer<int> IUnderlyingOperations<int>.UnsignedComparer => UnsignedComparer<int, UnderlyingOperations>.Default;

            IComparer<uint> IUnderlyingOperations<uint>.UnsignedComparer => Comparer<uint>.Default;

            IComparer<long> IUnderlyingOperations<long>.UnsignedComparer => UnsignedComparer<long, UnderlyingOperations>.Default;

            IComparer<ulong> IUnderlyingOperations<ulong>.UnsignedComparer => Comparer<ulong>.Default;

            IComparer<bool> IUnderlyingOperations<bool>.UnsignedComparer => Comparer<bool>.Default;

            IComparer<char> IUnderlyingOperations<char>.UnsignedComparer => Comparer<char>.Default;

            IComparer<float> IUnderlyingOperations<float>.UnsignedComparer => UnsignedComparer<float, UnderlyingOperations>.Default;

            IComparer<double> IUnderlyingOperations<double>.UnsignedComparer => UnsignedComparer<double, UnderlyingOperations>.Default;

            IComparer<IntPtr> IUnderlyingOperations<IntPtr>.UnsignedComparer => UnsignedComparer<IntPtr, UnderlyingOperations>.Default;

            IComparer<UIntPtr> IUnderlyingOperations<UIntPtr>.UnsignedComparer => Comparer<UIntPtr>.Default;
            #endregion

            #region And
            public byte And(byte left, byte right) => (byte)(left & right);

            public sbyte And(sbyte left, sbyte right) => (sbyte)(left & right);

            public short And(short left, short right) => (short)(left & right);

            public ushort And(ushort left, ushort right) => (ushort)(left & right);

            public int And(int left, int right) => left & right;

            public uint And(uint left, uint right) => left & right;

            public long And(long left, long right) => left & right;

            public ulong And(ulong left, ulong right) => left & right;

            public bool And(bool left, bool right) => left & right;

            public char And(char left, char right) => (char)(left & right);

            public float And(float left, float right) => BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) & BitConverter.SingleToInt32Bits(right));

            public double And(double left, double right) => BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) & BitConverter.DoubleToInt64Bits(right));

            public IntPtr And(IntPtr left, IntPtr right)
            {
#if BIT64
                return (IntPtr)((long)left & (long)right);
#else
                return (IntPtr)((int)left & (int)right);
#endif
            }

            public UIntPtr And(UIntPtr left, UIntPtr right)
            {
#if BIT64
                return (UIntPtr)((ulong)left & (ulong)right);
#else
                return (UIntPtr)((uint)left & (uint)right);
#endif
            }
            #endregion

            #region CompareTo
            public int CompareTo(byte left, byte right) => left.CompareTo(right);

            public int CompareTo(sbyte left, sbyte right) => left.CompareTo(right);

            public int CompareTo(short left, short right) => left.CompareTo(right);

            public int CompareTo(ushort left, ushort right) => left.CompareTo(right);

            public int CompareTo(int left, int right) => left.CompareTo(right);

            public int CompareTo(uint left, uint right) => left.CompareTo(right);

            public int CompareTo(long left, long right) => left.CompareTo(right);

            public int CompareTo(ulong left, ulong right) => left.CompareTo(right);

            public int CompareTo(bool left, bool right) => left.CompareTo(right);

            public int CompareTo(char left, char right) => left.CompareTo(right);

            public int CompareTo(float left, float right) => BitConverter.SingleToInt32Bits(left).CompareTo(BitConverter.SingleToInt32Bits(right));

            public int CompareTo(double left, double right) => BitConverter.DoubleToInt64Bits(left).CompareTo(BitConverter.DoubleToInt64Bits(right));

            public int CompareTo(IntPtr left, IntPtr right)
            {
#if BIT64
                return ((long)left).CompareTo((long)right);
#else
                return ((int)left).CompareTo((int)right);
#endif
            }

            public int CompareTo(UIntPtr left, UIntPtr right)
            {
#if BIT64
                return ((ulong)left).CompareTo((ulong)right);
#else
                return ((uint)left).CompareTo((uint)right);
#endif
            }
            #endregion

            #region IsInValueRange
            bool IUnderlyingOperations<byte>.IsInValueRange(ulong value) => value <= byte.MaxValue;

            bool IUnderlyingOperations<sbyte>.IsInValueRange(ulong value) => value <= (ulong)sbyte.MaxValue || value >= unchecked((ulong)sbyte.MinValue);

            bool IUnderlyingOperations<short>.IsInValueRange(ulong value) => value <= (ulong)short.MaxValue || value >= unchecked((ulong)short.MinValue);

            bool IUnderlyingOperations<ushort>.IsInValueRange(ulong value) => value <= ushort.MaxValue;

            bool IUnderlyingOperations<int>.IsInValueRange(ulong value) => value <= int.MaxValue || value >= unchecked((ulong)int.MinValue);

            bool IUnderlyingOperations<uint>.IsInValueRange(ulong value) => value <= uint.MaxValue;

            bool IUnderlyingOperations<long>.IsInValueRange(ulong value) => true;

            bool IUnderlyingOperations<ulong>.IsInValueRange(ulong value) => true;

            bool IUnderlyingOperations<bool>.IsInValueRange(ulong value) => value <= bool.True;

            bool IUnderlyingOperations<char>.IsInValueRange(ulong value) => value <= char.MaxValue;

            bool IUnderlyingOperations<float>.IsInValueRange(ulong value) => value <= int.MaxValue || value >= unchecked((ulong)int.MinValue);

            bool IUnderlyingOperations<double>.IsInValueRange(ulong value) => true;

            bool IUnderlyingOperations<IntPtr>.IsInValueRange(ulong value)
            {
#if BIT64
                return true;
#else
                return value <= int.MaxValue || value >= unchecked((ulong)int.MinValue);
#endif
            }

            bool IUnderlyingOperations<UIntPtr>.IsInValueRange(ulong value)
            {
#if BIT64
                return true;
#else
                return value <= uint.MaxValue;
#endif
            }
            #endregion

            #region LessThan
            public bool LessThan(byte left, byte right) => left < right;

            public bool LessThan(sbyte left, sbyte right) => left < right;

            public bool LessThan(short left, short right) => left < right;

            public bool LessThan(ushort left, ushort right) => left < right;

            public bool LessThan(int left, int right) => left < right;

            public bool LessThan(uint left, uint right) => left < right;

            public bool LessThan(long left, long right) => left < right;

            public bool LessThan(ulong left, ulong right) => left < right;

            public bool LessThan(bool left, bool right) => (!left) & right;

            public bool LessThan(char left, char right) => left < right;

            public bool LessThan(float left, float right) => BitConverter.SingleToInt32Bits(left) < BitConverter.SingleToInt32Bits(right);

            public bool LessThan(double left, double right) => BitConverter.DoubleToInt64Bits(left) < BitConverter.DoubleToInt64Bits(right);

            public bool LessThan(IntPtr left, IntPtr right)
            {
#if BIT64
                return (long)left < (long)right;
#else
                return (int)left < (int)right;
#endif
            }

            public bool LessThan(UIntPtr left, UIntPtr right)
            {
#if BIT64
                return (ulong)left < (ulong)right;
#else
                return (uint)left < (uint)right;
#endif
            }
            #endregion

            #region Or
            public byte Or(byte left, byte right) => (byte)(left | right);

            public sbyte Or(sbyte left, sbyte right) => (sbyte)(left | right);

            public short Or(short left, short right) => (short)(left | right);

            public ushort Or(ushort left, ushort right) => (ushort)(left | right);

            public int Or(int left, int right) => left | right;

            public uint Or(uint left, uint right) => left | right;

            public long Or(long left, long right) => left | right;

            public ulong Or(ulong left, ulong right) => left | right;

            public bool Or(bool left, bool right) => left | right;

            public char Or(char left, char right) => (char)(left | right);

            public float Or(float left, float right) => BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) | BitConverter.SingleToInt32Bits(right));

            public double Or(double left, double right) => BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) | BitConverter.DoubleToInt64Bits(right));

            public IntPtr Or(IntPtr left, IntPtr right)
            {
#if BIT64
                return (IntPtr)((long)left | (long)right);
#else
                return (IntPtr)((int)left | (int)right);
#endif
            }

            public UIntPtr Or(UIntPtr left, UIntPtr right)
            {
#if BIT64
                return (UIntPtr)((ulong)left | (ulong)right);
#else
                return (UIntPtr)((uint)left | (uint)right);
#endif
            }
            #endregion

            #region Subtract
            public byte Subtract(byte left, byte right) => (byte)(left - right);

            public sbyte Subtract(sbyte left, sbyte right) => (sbyte)(left - right);

            public short Subtract(short left, short right) => (short)(left - right);

            public ushort Subtract(ushort left, ushort right) => (ushort)(left - right);

            public int Subtract(int left, int right) => left - right;

            public uint Subtract(uint left, uint right) => left - right;

            public long Subtract(long left, long right) => left - right;

            public ulong Subtract(ulong left, ulong right) => left - right;

            public bool Subtract(bool left, bool right) => left ^ right;

            public char Subtract(char left, char right) => (char)(left - right);

            public float Subtract(float left, float right) => BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) - BitConverter.SingleToInt32Bits(right));

            public double Subtract(double left, double right) => BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) - BitConverter.DoubleToInt64Bits(right));

            public IntPtr Subtract(IntPtr left, IntPtr right)
            {
#if BIT64
                return (IntPtr)((long)left - (long)right);
#else
                return (IntPtr)((int)left - (int)right);
#endif
            }

            public UIntPtr Subtract(UIntPtr left, UIntPtr right)
            {
#if BIT64
                return (UIntPtr)((ulong)left - (ulong)right);
#else
                return (UIntPtr)((uint)left - (uint)right);
#endif
            }
            #endregion

            #region ToBoolean
            public bool ToBoolean(byte value) => Convert.ToBoolean(value);

            public bool ToBoolean(sbyte value) => Convert.ToBoolean(value);

            public bool ToBoolean(short value) => Convert.ToBoolean(value);

            public bool ToBoolean(ushort value) => Convert.ToBoolean(value);

            public bool ToBoolean(int value) => Convert.ToBoolean(value);

            public bool ToBoolean(uint value) => Convert.ToBoolean(value);

            public bool ToBoolean(long value) => Convert.ToBoolean(value);

            public bool ToBoolean(ulong value) => Convert.ToBoolean(value);

            public bool ToBoolean(bool value) => value;

            public bool ToBoolean(char value) => Convert.ToBoolean(value);

            public bool ToBoolean(float value) => Convert.ToBoolean(value);

            public bool ToBoolean(double value) => Convert.ToBoolean(value);

            public bool ToBoolean(IntPtr value)
            {
#if BIT64
                return Convert.ToBoolean((long)value);
#else
                return Convert.ToBoolean((int)value);
#endif
            }

            public bool ToBoolean(UIntPtr value)
            {
#if BIT64
                return Convert.ToBoolean((ulong)value);
#else
                return Convert.ToBoolean((uint)value);
#endif
            }
            #endregion

            #region ToByte
            public byte ToByte(byte value) => value;

            public byte ToByte(sbyte value) => Convert.ToByte(value);

            public byte ToByte(short value) => Convert.ToByte(value);

            public byte ToByte(ushort value) => Convert.ToByte(value);

            public byte ToByte(int value) => Convert.ToByte(value);

            public byte ToByte(uint value) => Convert.ToByte(value);

            public byte ToByte(long value) => Convert.ToByte(value);

            public byte ToByte(ulong value) => Convert.ToByte(value);

            public byte ToByte(bool value) => Convert.ToByte(value);

            public byte ToByte(char value) => Convert.ToByte(value);

            public byte ToByte(float value) => Convert.ToByte(value);

            public byte ToByte(double value) => Convert.ToByte(value);

            public byte ToByte(IntPtr value)
            {
#if BIT64
                return Convert.ToByte((long)value);
#else
                return Convert.ToByte((int)value);
#endif
            }

            public byte ToByte(UIntPtr value)
            {
#if BIT64
                return Convert.ToByte((ulong)value);
#else
                return Convert.ToByte((uint)value);
#endif
            }
            #endregion

            #region ToChar
            public char ToChar(byte value) => (char)value;

            public char ToChar(sbyte value) => Convert.ToChar(value);

            public char ToChar(short value) => Convert.ToChar(value);

            public char ToChar(ushort value) => (char)value;

            public char ToChar(int value) => Convert.ToChar(value);

            public char ToChar(uint value) => Convert.ToChar(value);

            public char ToChar(long value) => Convert.ToChar(value);

            public char ToChar(ulong value) => Convert.ToChar(value);

            public char ToChar(bool value) => Convert.ToChar(value);

            public char ToChar(char value) => value;

            public char ToChar(float value) => Convert.ToChar(value);

            public char ToChar(double value) => Convert.ToChar(value);

            public char ToChar(IntPtr value)
            {
#if BIT64
                return Convert.ToChar((long)value);
#else
                return Convert.ToChar((int)value);
#endif
            }

            public char ToChar(UIntPtr value)
            {
#if BIT64
                return Convert.ToChar((ulong)value);
#else
                return Convert.ToChar((uint)value);
#endif
            }
            #endregion

            #region ToDecimal
            public decimal ToDecimal(byte value) => value;

            public decimal ToDecimal(sbyte value) => value;

            public decimal ToDecimal(short value) => value;

            public decimal ToDecimal(ushort value) => value;

            public decimal ToDecimal(int value) => value;

            public decimal ToDecimal(uint value) => value;

            public decimal ToDecimal(long value) => value;

            public decimal ToDecimal(ulong value) => value;

            public decimal ToDecimal(bool value) => Convert.ToDecimal(value);

            public decimal ToDecimal(char value) => value;

            public decimal ToDecimal(float value) => (decimal)value;

            public decimal ToDecimal(double value) => (decimal)value;

            public decimal ToDecimal(IntPtr value)
            {
#if BIT64
                return (long)value;
#else
                return (int)value;
#endif
            }

            public decimal ToDecimal(UIntPtr value)
            {
#if BIT64
                return (ulong)value;
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToDouble
            public double ToDouble(byte value) => value;

            public double ToDouble(sbyte value) => value;

            public double ToDouble(short value) => value;

            public double ToDouble(ushort value) => value;

            public double ToDouble(int value) => value;

            public double ToDouble(uint value) => value;

            public double ToDouble(long value) => value;

            public double ToDouble(ulong value) => value;

            public double ToDouble(bool value) => Convert.ToDouble(value);

            public double ToDouble(char value) => value;

            public double ToDouble(float value) => value;

            public double ToDouble(double value) => value;

            public double ToDouble(IntPtr value)
            {
#if BIT64
                return (long)value;
#else
                return (int)value;
#endif
            }

            public double ToDouble(UIntPtr value)
            {
#if BIT64
                return (ulong)value;
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToHexStr
            public string ToHexStr(byte value) => Number.Int32ToHexStr(value, '7', 2);

            public string ToHexStr(sbyte value) => Number.Int32ToHexStr((byte)value, '7', 2);

            public string ToHexStr(short value) => Number.Int32ToHexStr((ushort)value, '7', 4);

            public string ToHexStr(ushort value) => Number.Int32ToHexStr(value, '7', 4);

            public string ToHexStr(int value) => Number.Int32ToHexStr(value, '7', 8);

            public string ToHexStr(uint value) => Number.Int32ToHexStr((int)value, '7', 8);

            public string ToHexStr(long value) => Number.Int64ToHexStr(value, '7', 16);

            public string ToHexStr(ulong value) => Number.Int64ToHexStr((long)value, '7', 16);

            public string ToHexStr(bool value) => Convert.ToByte(value).ToString("X2");

            public string ToHexStr(char value) => ((ushort)value).ToString("X4");

            public string ToHexStr(float value) => BitConverter.SingleToInt32Bits(value).ToString("X8");

            public string ToHexStr(double value) => BitConverter.DoubleToInt64Bits(value).ToString("X16");

            public string ToHexStr(IntPtr value)
            {
#if BIT64
                return ((long)value).ToString("X16");
#else
                return ((int)value).ToString("X8");
#endif
            }

            public string ToHexStr(UIntPtr value)
            {
#if BIT64
                return ((ulong)value).ToString("X16");
#else
                return ((uint)value).ToString("X8");
#endif
            }
            #endregion

            #region ToInt16
            public short ToInt16(byte value) => value;

            public short ToInt16(sbyte value) => value;

            public short ToInt16(short value) => value;

            public short ToInt16(ushort value) => Convert.ToInt16(value);

            public short ToInt16(int value) => Convert.ToInt16(value);

            public short ToInt16(uint value) => Convert.ToInt16(value);

            public short ToInt16(long value) => Convert.ToInt16(value);

            public short ToInt16(ulong value) => Convert.ToInt16(value);

            public short ToInt16(bool value) => Convert.ToInt16(value);

            public short ToInt16(char value) => Convert.ToInt16(value);

            public short ToInt16(float value) => Convert.ToInt16(value);

            public short ToInt16(double value) => Convert.ToInt16(value);

            public short ToInt16(IntPtr value)
            {
#if BIT64
                return Convert.ToInt16((long)value);
#else
                return Convert.ToInt16((int)value);
#endif
            }

            public short ToInt16(UIntPtr value)
            {
#if BIT64
                return Convert.ToInt16((ulong)value);
#else
                return Convert.ToInt16((uint)value);
#endif
            }
            #endregion

            #region ToInt32
            public int ToInt32(byte value) => value;

            public int ToInt32(sbyte value) => value;

            public int ToInt32(short value) => value;

            public int ToInt32(ushort value) => value;

            public int ToInt32(int value) => value;

            public int ToInt32(uint value) => Convert.ToInt32(value);

            public int ToInt32(long value) => Convert.ToInt32(value);

            public int ToInt32(ulong value) => Convert.ToInt32(value);

            public int ToInt32(bool value) => Convert.ToInt32(value);

            public int ToInt32(char value) => value;

            public int ToInt32(float value) => Convert.ToInt32(value);

            public int ToInt32(double value) => Convert.ToInt32(value);

            public int ToInt32(IntPtr value)
            {
#if BIT64
                return Convert.ToInt32((long)value);
#else
                return (int)value;
#endif
            }

            public int ToInt32(UIntPtr value)
            {
#if BIT64
                return Convert.ToInt32((ulong)value);
#else
                return Convert.ToInt32((uint)value);
#endif
            }
            #endregion

            #region ToInt64
            public long ToInt64(byte value) => value;

            public long ToInt64(sbyte value) => value;

            public long ToInt64(short value) => value;

            public long ToInt64(ushort value) => value;

            public long ToInt64(int value) => value;

            public long ToInt64(uint value) => value;

            public long ToInt64(long value) => value;

            public long ToInt64(ulong value) => Convert.ToInt64(value);

            public long ToInt64(bool value) => Convert.ToInt64(value);

            public long ToInt64(char value) => value;

            public long ToInt64(float value) => Convert.ToInt64(value);

            public long ToInt64(double value) => Convert.ToInt64(value);

            public long ToInt64(IntPtr value)
            {
#if BIT64
                return (long)value;
#else
                return (int)value;
#endif
            }

            public long ToInt64(UIntPtr value)
            {
#if BIT64
                return Convert.ToInt64((ulong)value);
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToObject
            byte IUnderlyingOperations<byte>.ToObject(ulong value) => (byte)value;

            sbyte IUnderlyingOperations<sbyte>.ToObject(ulong value) => (sbyte)value;

            short IUnderlyingOperations<short>.ToObject(ulong value) => (short)value;

            ushort IUnderlyingOperations<ushort>.ToObject(ulong value) => (ushort)value;

            int IUnderlyingOperations<int>.ToObject(ulong value) => (int)value;

            uint IUnderlyingOperations<uint>.ToObject(ulong value) => (uint)value;

            long IUnderlyingOperations<long>.ToObject(ulong value) => (long)value;

            ulong IUnderlyingOperations<ulong>.ToObject(ulong value) => value;

            bool IUnderlyingOperations<bool>.ToObject(ulong value) => value != 0UL;

            char IUnderlyingOperations<char>.ToObject(ulong value) => (char)value;

            float IUnderlyingOperations<float>.ToObject(ulong value) => BitConverter.Int32BitsToSingle((int)value);

            double IUnderlyingOperations<double>.ToObject(ulong value) => BitConverter.Int64BitsToDouble((long)value);

            IntPtr IUnderlyingOperations<IntPtr>.ToObject(ulong value)
            {
#if BIT64
                return (IntPtr)(long)value;
#else
                return (IntPtr)(int)value;
#endif
            }

            UIntPtr IUnderlyingOperations<UIntPtr>.ToObject(ulong value)
            {
#if BIT64
                return (UIntPtr)value;
#else
                return (UIntPtr)(uint)value;
#endif
            }
            #endregion

            #region ToSByte
            public sbyte ToSByte(byte value) => Convert.ToSByte(value);

            public sbyte ToSByte(sbyte value) => value;

            public sbyte ToSByte(short value) => Convert.ToSByte(value);

            public sbyte ToSByte(ushort value) => Convert.ToSByte(value);

            public sbyte ToSByte(int value) => Convert.ToSByte(value);

            public sbyte ToSByte(uint value) => Convert.ToSByte(value);

            public sbyte ToSByte(long value) => Convert.ToSByte(value);

            public sbyte ToSByte(ulong value) => Convert.ToSByte(value);

            public sbyte ToSByte(bool value) => Convert.ToSByte(value);

            public sbyte ToSByte(char value) => Convert.ToSByte(value);

            public sbyte ToSByte(float value) => Convert.ToSByte(value);

            public sbyte ToSByte(double value) => Convert.ToSByte(value);

            public sbyte ToSByte(IntPtr value)
            {
#if BIT64
                return Convert.ToSByte((long)value);
#else
                return Convert.ToSByte((int)value);
#endif
            }

            public sbyte ToSByte(UIntPtr value)
            {
#if BIT64
                return Convert.ToSByte((ulong)value);
#else
                return Convert.ToSByte((uint)value);
#endif
            }
            #endregion

            #region ToSingle
            public float ToSingle(byte value) => value;

            public float ToSingle(sbyte value) => value;

            public float ToSingle(short value) => value;

            public float ToSingle(ushort value) => value;

            public float ToSingle(int value) => value;

            public float ToSingle(uint value) => value;

            public float ToSingle(long value) => value;

            public float ToSingle(ulong value) => value;

            public float ToSingle(bool value) => Convert.ToSingle(value);

            public float ToSingle(char value) => value;

            public float ToSingle(float value) => value;

            public float ToSingle(double value) => (float)value;

            public float ToSingle(IntPtr value)
            {
#if BIT64
                return (long)value;
#else
                return (int)value;
#endif
            }

            public float ToSingle(UIntPtr value)
            {
#if BIT64
                return (ulong)value;
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToUInt16
            public ushort ToUInt16(byte value) => value;

            public ushort ToUInt16(sbyte value) => Convert.ToUInt16(value);

            public ushort ToUInt16(short value) => Convert.ToUInt16(value);

            public ushort ToUInt16(ushort value) => value;

            public ushort ToUInt16(int value) => Convert.ToUInt16(value);

            public ushort ToUInt16(uint value) => Convert.ToUInt16(value);

            public ushort ToUInt16(long value) => Convert.ToUInt16(value);

            public ushort ToUInt16(ulong value) => Convert.ToUInt16(value);

            public ushort ToUInt16(bool value) => Convert.ToUInt16(value);

            public ushort ToUInt16(char value) => value;

            public ushort ToUInt16(float value) => Convert.ToUInt16(value);

            public ushort ToUInt16(double value) => Convert.ToUInt16(value);

            public ushort ToUInt16(IntPtr value)
            {
#if BIT64
                return Convert.ToUInt16((long)value);
#else
                return Convert.ToUInt16((int)value);
#endif
            }

            public ushort ToUInt16(UIntPtr value)
            {
#if BIT64
                return Convert.ToUInt16((ulong)value);
#else
                return Convert.ToUInt16((uint)value);
#endif
            }
            #endregion

            #region ToUInt32
            public uint ToUInt32(byte value) => value;

            public uint ToUInt32(sbyte value) => Convert.ToUInt32(value);

            public uint ToUInt32(short value) => Convert.ToUInt32(value);

            public uint ToUInt32(ushort value) => value;

            public uint ToUInt32(int value) => Convert.ToUInt32(value);

            public uint ToUInt32(uint value) => value;

            public uint ToUInt32(long value) => Convert.ToUInt32(value);

            public uint ToUInt32(ulong value) => Convert.ToUInt32(value);

            public uint ToUInt32(bool value) => Convert.ToUInt32(value);

            public uint ToUInt32(char value) => value;

            public uint ToUInt32(float value) => Convert.ToUInt32(value);

            public uint ToUInt32(double value) => Convert.ToUInt32(value);

            public uint ToUInt32(IntPtr value)
            {
#if BIT64
                return Convert.ToUInt32((long)value);
#else
                return Convert.ToUInt32((int)value);
#endif
            }

            public uint ToUInt32(UIntPtr value)
            {
#if BIT64
                return Convert.ToUInt32((ulong)value);
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToUInt64
            public ulong ToUInt64(byte value) => value;

            public ulong ToUInt64(sbyte value) => Convert.ToUInt64(value);

            public ulong ToUInt64(short value) => Convert.ToUInt64(value);

            public ulong ToUInt64(ushort value) => value;

            public ulong ToUInt64(int value) => Convert.ToUInt64(value);

            public ulong ToUInt64(uint value) => value;

            public ulong ToUInt64(long value) => Convert.ToUInt64(value);

            public ulong ToUInt64(ulong value) => value;

            public ulong ToUInt64(bool value) => Convert.ToUInt64(value);

            public ulong ToUInt64(char value) => value;

            public ulong ToUInt64(float value) => Convert.ToUInt64(value);

            public ulong ToUInt64(double value) => Convert.ToUInt64(value);

            public ulong ToUInt64(IntPtr value)
            {
#if BIT64
                return Convert.ToUInt64((long)value);
#else
                return Convert.ToUInt64((int)value);
#endif
            }

            public ulong ToUInt64(UIntPtr value)
            {
#if BIT64
                return (ulong)value;
#else
                return (uint)value;
#endif
            }
            #endregion

            #region ToUInt64Unchecked
            public ulong ToUInt64Unchecked(byte value) => value;

            public ulong ToUInt64Unchecked(sbyte value) => (ulong)value;

            public ulong ToUInt64Unchecked(short value) => (ulong)value;

            public ulong ToUInt64Unchecked(ushort value) => value;

            public ulong ToUInt64Unchecked(int value) => (ulong)value;

            public ulong ToUInt64Unchecked(uint value) => value;

            public ulong ToUInt64Unchecked(long value) => (ulong)value;

            public ulong ToUInt64Unchecked(ulong value) => value;

            public ulong ToUInt64Unchecked(bool value) => Convert.ToUInt64(value);

            public ulong ToUInt64Unchecked(char value) => value;

            public ulong ToUInt64Unchecked(float value) => (ulong)BitConverter.SingleToInt32Bits(value);

            public ulong ToUInt64Unchecked(double value) => (ulong)BitConverter.DoubleToInt64Bits(value);

            public ulong ToUInt64Unchecked(IntPtr value)
            {
#if BIT64
                return (ulong)(long)value;
#else
                return (ulong)(int)value;
#endif
            }

            public ulong ToUInt64Unchecked(UIntPtr value)
            {
#if BIT64
                return (ulong)value;
#else
                return (uint)value;
#endif
            }
            #endregion

            #region TryParse
            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out byte result)
            {
                Number.ParsingStatus status = Number.TryParseUInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out uint i);
                if (status == Number.ParsingStatus.OK && i > byte.MaxValue)
                {
                    status = Number.ParsingStatus.Overflow;
                }
                result = (byte)i;
                return status;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out sbyte result)
            {
                Number.ParsingStatus status = Number.TryParseInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out int i);
                if (status == Number.ParsingStatus.OK && (uint)(i - sbyte.MinValue) > byte.MaxValue)
                {
                    status = Number.ParsingStatus.Overflow;
                }
                result = (sbyte)i;
                return status;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out short result)
            {
                Number.ParsingStatus status = Number.TryParseInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out int i);
                if (status == Number.ParsingStatus.OK && (uint)(i - short.MinValue) > ushort.MaxValue)
                {
                    status = Number.ParsingStatus.Overflow;
                }
                result = (short)i;
                return status;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out ushort result)
            {
                Number.ParsingStatus status = Number.TryParseUInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out uint i);
                if (status == Number.ParsingStatus.OK && i > ushort.MaxValue)
                {
                    status = Number.ParsingStatus.Overflow;
                }
                result = (ushort)i;
                return status;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out int result) => Number.TryParseInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out uint result) => Number.TryParseUInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out long result) => Number.TryParseInt64IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out ulong result) => Number.TryParseUInt64IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out bool result) => bool.TryParse(span, out result) ? Number.ParsingStatus.OK : Number.ParsingStatus.Failed;

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out char result)
            {
                bool success = span.Length == 1;
                result = success ? span[0] : default;
                return success ? Number.ParsingStatus.OK : Number.ParsingStatus.Failed;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out float result)
            {
                return Number.TryParseSingle(span, NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture.NumberFormat, out result) ? Number.ParsingStatus.OK : Number.ParsingStatus.Failed;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out double result)
            {
                return Number.TryParseDouble(span, NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture.NumberFormat, out result) ? Number.ParsingStatus.OK : Number.ParsingStatus.Failed;
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out IntPtr result)
            {
#if BIT64
                Number.ParsingStatus status = Number.TryParseInt64IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out long int64Result);
                result = (IntPtr)int64Result;
                return status;
#else
                Number.ParsingStatus status = Number.TryParseInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out int int32Result);
                result = (IntPtr)int32Result;
                return status;
#endif
            }

            public Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out UIntPtr result)
            {
#if BIT64
                Number.ParsingStatus status = Number.TryParseUInt64IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out ulong uint64Result);
                result = (UIntPtr)uint64Result;
                return status;
#else
                Number.ParsingStatus status = Number.TryParseUInt32IntegerStyle(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out uint uint32Result);
                result = (UIntPtr)uint32Result;
                return status;
#endif
            }
            #endregion
        }

        private sealed class UnsignedComparer<TUnderlying, TUnderlyingOperations> : IComparer<TUnderlying>
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            public static UnsignedComparer<TUnderlying, TUnderlyingOperations> Default { get; } = new UnsignedComparer<TUnderlying, TUnderlyingOperations>();

            public int Compare(TUnderlying x, TUnderlying y)
            {
                TUnderlyingOperations operations = default;
                return operations.ToUInt64Unchecked(x).CompareTo(operations.ToUInt64Unchecked(y));
            }
        }
        #endregion
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern CorElementType InternalGetCorElementType();
        #endregion

        #region Object Overrides
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern override bool Equals(object obj);

        public override int GetHashCode()
        {
            // CONTRACT with the runtime: GetHashCode of enum types is implemented as GetHashCode of the underlying type.
            // The runtime can bypass calls to Enum::GetHashCode and call the underlying type's GetHashCode directly
            // to avoid boxing the enum.
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U1:
                    return data.GetHashCode();
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I4:
                    return Unsafe.As<byte, int>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, double>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I:
                    return Unsafe.As<byte, IntPtr>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U:
                    return Unsafe.As<byte, UIntPtr>(ref data).GetHashCode();
                default:
                    Debug.Fail("Invalid primitive type");
                    return 0;
            }
        }

        public override string ToString()
        {
            // Returns the value in a human readable format.  For PASCAL style enums who's value maps directly the name of the field is returned.
            // For PASCAL style enums who's values do not map directly the decimal value of the field is returned.
            // For BitFlags (indicated by the Flags custom attribute): If for each bit that is set in the value there is a corresponding constant
            // (a pure power of 2), then the OR string (ie "Red, Yellow") is returned. Otherwise, if the value is zero or if you can't create a string that consists of
            // pure powers of 2 OR-ed together, you return a hex value

            // Try to see if its one of the enum values, then we return a String back else the value
            return GetCache((RuntimeType)GetType()).ToString(ref this.GetRawData());
        }
        #endregion

        #region IFormattable
        [Obsolete("The provider argument is not used. Please use ToString(String).")]
        public string ToString(string format, IFormatProvider provider) =>
            ToString(format);
        #endregion

        #region IComparable
        public int CompareTo(object target)
        {
            const int retIncompatibleMethodTables = 2;  // indicates that the method tables did not match
            const int retInvalidEnumType = 3; // indicates that the enum was of an unknown/unsupported underlying type

            if (this == null)
            {
                throw new NullReferenceException();
            }

            int ret = InternalCompareTo(this, target);

            if (ret < retIncompatibleMethodTables)
            {
                // -1, 0 and 1 are the normal return codes
                return ret;
            }
            else if (ret == retIncompatibleMethodTables)
            {
                Type thisType = this.GetType();
                Type targetType = target.GetType();

                throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, targetType.ToString(), thisType.ToString()));
            }
            else
            {
                // assert valid return code (3)
                Debug.Assert(ret == retInvalidEnumType, "Enum.InternalCompareTo return code was invalid");

                throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }
        #endregion

        #region Public Methods
        public string ToString(string format) =>
            GetCache((RuntimeType)GetType()).ToString(ref this.GetRawData(), format);

        [Obsolete("The provider argument is not used. Please use ToString().")]
        public string ToString(IFormatProvider provider) =>
            ToString();

        [Intrinsic]
        public bool HasFlag(Enum flag)
        {
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }

            return GetCache((RuntimeType)GetType()).HasFlag(ref this.GetRawData(), flag);
        }
        #endregion

        #region IConvertable
        public TypeCode GetTypeCode()
        {
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return TypeCode.SByte;
                case CorElementType.ELEMENT_TYPE_U1:
                    return TypeCode.Byte;
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return TypeCode.Boolean;
                case CorElementType.ELEMENT_TYPE_I2:
                    return TypeCode.Int16;
                case CorElementType.ELEMENT_TYPE_U2:
                    return TypeCode.UInt16;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return TypeCode.Char;
                case CorElementType.ELEMENT_TYPE_I4:
                    return TypeCode.Int32;
                case CorElementType.ELEMENT_TYPE_U4:
                    return TypeCode.UInt32;
                case CorElementType.ELEMENT_TYPE_I8:
                    return TypeCode.Int64;
                case CorElementType.ELEMENT_TYPE_U8:
                    return TypeCode.UInt64;
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        bool IConvertible.ToBoolean(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToBoolean(ref this.GetRawData());

        char IConvertible.ToChar(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToChar(ref this.GetRawData());

        sbyte IConvertible.ToSByte(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToSByte(ref this.GetRawData());

        byte IConvertible.ToByte(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToByte(ref this.GetRawData());

        short IConvertible.ToInt16(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToInt16(ref this.GetRawData());

        ushort IConvertible.ToUInt16(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToUInt16(ref this.GetRawData());

        int IConvertible.ToInt32(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToInt32(ref this.GetRawData());

        uint IConvertible.ToUInt32(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToUInt32(ref this.GetRawData());

        long IConvertible.ToInt64(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToInt64(ref this.GetRawData());

        ulong IConvertible.ToUInt64(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToUInt64(ref this.GetRawData());

        float IConvertible.ToSingle(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToSingle(ref this.GetRawData());

        double IConvertible.ToDouble(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToDouble(ref this.GetRawData());

        decimal IConvertible.ToDecimal(IFormatProvider provider) =>
            GetCache((RuntimeType)GetType()).ToDecimal(ref this.GetRawData());

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(SR.Format(SR.InvalidCast_FromTo, "Enum", "DateTime"));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider) =>
            Convert.DefaultToType(this, type, provider);
        #endregion

        #region ToObject
        [CLSCompliant(false)]
        public static object ToObject(Type enumType, sbyte value) =>
            GetCache(enumType).ToObject((ulong)value);

        public static object ToObject(Type enumType, short value) =>
            GetCache(enumType).ToObject((ulong)value);

        public static object ToObject(Type enumType, int value) =>
            GetCache(enumType).ToObject((ulong)value);

        public static object ToObject(Type enumType, byte value) =>
            GetCache(enumType).ToObject(value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ushort value) =>
            GetCache(enumType).ToObject(value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, uint value) =>
            GetCache(enumType).ToObject(value);

        public static object ToObject(Type enumType, long value) =>
            GetCache(enumType).ToObject((ulong)value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ulong value) =>
            GetCache(enumType).ToObject(value);
        #endregion
    }
}
