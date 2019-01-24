// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        internal static ulong ToUInt64(object value) => ToUInt64(value, true);

        internal static ulong ToUInt64(object value, bool throwInvalidOperationException)
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
            return EnumCache.Get(enumType).ParseNonGeneric(value, ignoreCase);
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct =>
            Parse<TEnum>(value, ignoreCase: false);

        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnumCache<TEnum> cache = EnumCache<TEnum>.Instance;
            if (cache == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            return cache.Parse(value, ignoreCase);
        }

        public static bool TryParse(Type enumType, string value, out object result) =>
            TryParse(enumType, value, ignoreCase: false, out result);

        public static bool TryParse(Type enumType, string value, bool ignoreCase, out object result) =>
            EnumCache.Get(enumType).TryParse(value, ignoreCase, out result);

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct =>
            TryParse(value, ignoreCase: false, out result);

        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
            EnumCache<TEnum> cache = EnumCache<TEnum>.Instance;
            if (cache == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            return cache.TryParse(value, ignoreCase, out result);
        }

        public static Type GetUnderlyingType(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            return enumType.GetEnumUnderlyingType();
        }

        public static Array GetValues(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            return enumType.GetEnumValues();
        }

        public static string GetName(Type enumType, object value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            return enumType.GetEnumName(value);
        }

        public static string[] GetNames(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            return enumType.GetEnumNames();
        }

        public static bool IsDefined(Type enumType, object value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            return enumType.IsEnumDefined(value);
        }

        public static string Format(Type enumType, object value, string format)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return EnumCache.Get(enumType).Format(value, format);
        }
        #endregion

        #region Definitions
        #region EnumCache
        internal abstract class EnumCache
        {
            internal static EnumCache Get(Type enumType)
            {
                if (!(enumType is RuntimeType rtType))
                {
                    return ThrowGetCacheException(enumType);
                }

                if (!rtType.IsEnum)
                {
                    throw new ArgumentException(SR.Arg_MustBeEnum, nameof(enumType));
                }

                return Get(rtType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static EnumCache ThrowGetCacheException(Type enumType)
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
            internal static EnumCache Get(RuntimeType rtType)
            {
                Debug.Assert(rtType.IsEnum);

                return rtType.GenericCache as EnumCache ?? InitializeGenericCache(rtType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static EnumCache InitializeGenericCache(RuntimeType rtType)
            {
                EnumCache cache = (EnumCache)typeof(EnumCache<>).MakeGenericType(rtType).GetField(nameof(EnumCache<DayOfWeek>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) ?? throw new ArgumentException(SR.Argument_InvalidEnum, "enumType");
                rtType.GenericCache = cache;
                return cache;
            }

            private protected static EnumCache Create(Type enumType)
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

                return (EnumCache)Activator.CreateInstance(typeof(EnumCache<,,>).MakeGenericType(enumType, underlyingType, typeof(UnderlyingOperations)));
            }

            public readonly Type UnderlyingType;
            public readonly EnumMembers Members;

            private protected EnumCache(Type underlyingType, EnumMembers members)
            {
                UnderlyingType = underlyingType;
                Members = members;
            }

            public abstract string Format(object value, string format);
            public abstract string GetName(object value);
            public abstract Array GetValuesNonGeneric();
            public abstract bool IsDefined(object value);
            public abstract object ParseNonGeneric(ReadOnlySpan<char> value, bool ignoreCase);
            public abstract object ToObjectNonGeneric(ulong value);
            public abstract bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out object result);
        }

        // Try to minimize code here due to generic code explosion with an Enum generic type argument
        internal abstract class EnumCache<TEnum> : EnumCache
        {
            public static readonly EnumCache<TEnum> Instance = (EnumCache<TEnum>)Create(typeof(TEnum));

            private protected EnumCache(Type underlyingType, EnumMembers members)
                : base(underlyingType, members)
            {
            }

            public abstract bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result);
            public abstract TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase);
        }

        // Try to minimize code here due to generic code explosion with an Enum generic type argument
        private sealed class EnumCache<TEnum, TUnderlying, TUnderlyingOperations> : EnumCache<TEnum>
            where TEnum : struct, Enum
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            private static readonly EnumMembers<TUnderlying, TUnderlyingOperations> s_members = new EnumMembers<TUnderlying, TUnderlyingOperations>(typeof(TEnum));

            public EnumCache()
                : base(typeof(TUnderlying), s_members)
            {
            }

            public override string Format(object value, string format) => value is TEnum enumValue ?
                s_members.Format(Unsafe.As<TEnum, TUnderlying>(ref enumValue), format) :
                s_members.Format(value, format);

            public override string GetName(object value) => value is TEnum enumValue ?
                s_members.GetName(Unsafe.As<TEnum, TUnderlying>(ref enumValue)) :
                s_members.GetName(value);

            public override Array GetValuesNonGeneric()
            {
                ulong[] values = s_members._values;
                TEnum[] array = new TEnum[values.Length];
                TUnderlyingOperations operations = default;
                for (int i = 0; i < values.Length; ++i)
                {
                    TUnderlying underlying = operations.ToObject(values[i]);
                    array[i] = Unsafe.As<TUnderlying, TEnum>(ref underlying);
                }
                return array;
            }

            public override bool IsDefined(object value) => value is TEnum enumValue ?
                s_members.IsDefined(Unsafe.As<TEnum, TUnderlying>(ref enumValue)) :
                s_members.IsDefined(value);

            public override TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase)
            {
                TUnderlying underlying = s_members.Parse(value, ignoreCase);
                return Unsafe.As<TUnderlying, TEnum>(ref underlying);
            }

            public override object ParseNonGeneric(ReadOnlySpan<char> value, bool ignoreCase) => Parse(value, ignoreCase);

            public override object ToObjectNonGeneric(ulong value)
            {
                TUnderlying underlying = default(TUnderlyingOperations).ToObject(value);
                return Unsafe.As<TUnderlying, TEnum>(ref underlying);
            }

            public override bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
            {
                bool success = s_members.TryParse(value, ignoreCase, out TUnderlying underlying);
                result = Unsafe.As<TUnderlying, TEnum>(ref underlying);
                return success;
            }

            public override bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out object result)
            {
                bool success = TryParse(value, ignoreCase, out TEnum enumResult);
                result = success ? (object)enumResult : null;
                return success;
            }
        }
        #endregion

        #region EnumMembers
        internal abstract class EnumMembers
        {
            protected readonly Type _enumType;
            protected readonly bool _isFlagEnum;
            internal readonly string[] _names;
            internal readonly ulong[] _values;

            protected EnumMembers(Type enumType)
            {
                _enumType = enumType;
                _isFlagEnum = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
                FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

                string[] names = new string[fields.Length];
                ulong[] values = new ulong[fields.Length];
                for (int i = 0; i < fields.Length; ++i)
                {
                    ulong value = ToUInt64(fields[i].GetRawConstantValue());
                    int index = HybridSearch(values, i, value);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    else
                    {
                        do
                        {
                            ++index;
                        } while (index < i && value == values[index]);
                    }

                    Array.Copy(names, index, names, index + 1, i - index);
                    Array.Copy(values, index, values, index + 1, i - index);

                    names[index] = fields[i].Name;
                    values[index] = value;
                }
                _names = names;
                _values = values;
            }

            public string GetName(ulong value)
            {
                ulong[] values = _values;
                int index = HybridSearch(values, values.Length, value);
                if (index >= 0)
                {
                    return _names[index];
                }
                return null;
            }

            public string ToString(ulong value)
            {
                if (_isFlagEnum)
                {
                    return ToStringFlags(value);
                }
                ulong[] values = _values;
                int index = HybridSearch(values, values.Length, value);
                if (index >= 0)
                {
                    return _names[index];
                }
                return ValueToString(value);
            }

            public abstract string ValueToString(ulong value);

            public string ToStringFlags(ulong value)
            {
                string[] names = _names;
                ulong[] values = _values;

                // Values are sorted, so if the incoming value is 0, we can check to see whether
                // the first non-negative entry matches it, in which case we can return its name;
                // otherwise, we can just return the integral value.
                if (value == 0)
                {
                    return values.Length > 0 && values[0] == 0 ?
                        names[0] :
                        ValueToString(value);
                }

                // It's common to have a flags enum with a single value that matches a single
                // entry, in which case we can just return the existing name string.
                int index = HybridSearch(values, values.Length, value);
                if (index >= 0)
                {
                    return names[index];
                }

                // With a ulong result value, regardless of the enum's base type, the maximum
                // possible number of consistent name/values we could have is 64, since every
                // value is made up of one or more bits, and when we see values and incorporate
                // their names, we effectively switch off those bits.
                Span<int> foundItems = stackalloc int[64];

                // Now look for matches, storing the indices of the values
                // into our span.
                int resultLength = 0;
                int foundItemsCount = 0;
                ulong tempValue = value;
                for (index = ~index - 1; index >= 0; --index)
                {
                    ulong currentValue = values[index];
                    if ((tempValue & currentValue) == currentValue)
                    {
                        if (currentValue == 0)
                        {
                            break;
                        }
                        tempValue -= currentValue;
                        foundItems[foundItemsCount++] = index;
                        resultLength = checked(resultLength + names[index].Length);
                        if (tempValue == 0)
                        {
                            // We know what strings to concatenate.  Do so.

                            Debug.Assert(foundItemsCount > 0);
                            const int SeparatorStringLength = 2; // ", "
                            string result = string.FastAllocateString(checked(resultLength + (SeparatorStringLength * (foundItemsCount - 1))));

                            Span<char> resultSpan = new Span<char>(ref result.GetRawStringData(), result.Length);
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
                    }
                }

                // We exhausted looking through all the values and we still have
                // a non-zero result, we couldn't match the result to only named values.
                // In that case, we return the integral value.
                return ValueToString(value);
            }

            protected Number.ParsingStatus TryParseName(ReadOnlySpan<char> value, bool ignoreCase, out ulong result)
            {
                Number.ParsingStatus status = Number.ParsingStatus.Failed;
                ulong localResult = default;
                string[] names = _names;
                ulong[] values = _values;
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
                                localResult |= values[i];
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
                                localResult |= values[i];
                                status = Number.ParsingStatus.OK;
                                break;
                            }
                        }
                    }
                } while (value.Length > 0 && status == Number.ParsingStatus.OK);

                result = status == Number.ParsingStatus.OK ? localResult : default;
                return status;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected static int HybridSearch(ulong[] values, int length, ulong value)
            {
                const int HybridSearchCutoffLength = 32; // number determined from benchmarking linear vs binary search in the worst case scenario
                if (length <= HybridSearchCutoffLength)
                {
                    for (int i = length - 1; i >= 0; --i)
                    {
                        if (values[i] == value)
                        {
                            return i;
                        }
                        if (values[i] < value)
                        {
                            return ~(i + 1);
                        }
                    }
                    return -1; // == ~0
                }
                return Array.BinarySearch(values, 0, length, value);
            }
        }

        private sealed class EnumMembers<TUnderlying, TUnderlyingOperations> : EnumMembers
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            private readonly TUnderlying _max;
            private readonly TUnderlying _min;
            private readonly bool _isContiguous;

            public EnumMembers(Type enumType)
                : base(enumType)
            {
                ulong[] values = _values;
                TUnderlyingOperations operations = default;
                TUnderlying max = default;
                TUnderlying min = default;
                for (var i = 0; i < values.Length; ++i)
                {
                    TUnderlying value = operations.ToObject(values[i]);
                    if (i == 0)
                    {
                        max = value;
                        min = value;
                    }
                    if (operations.LessThan(max, value))
                    {
                        max = value;
                    }
                    else if (operations.LessThan(value, min))
                    {
                        min = value;
                    }
                }
                _max = max;
                _min = min;
                _isContiguous = values.Length > 0 && operations.Subtract(max, operations.ToObject((ulong)values.Length - 1)).Equals(min);
            }

            public string GetName(object value)
            {
                Debug.Assert(value.GetType() != _enumType);

                ulong uint64Value = ToUInt64(value, throwInvalidOperationException: false);
                TUnderlyingOperations operations = default;
                return operations.IsInValueRange(uint64Value) ? base.GetName(uint64Value) : null;
            }

            public bool IsDefined(TUnderlying value)
            {
                TUnderlyingOperations operations = default;
                if (_isContiguous)
                {
                    return !(operations.LessThan(value, _min) || operations.LessThan(_max, value));
                }
                ulong[] values = _values;
                return HybridSearch(values, values.Length, operations.ToUInt64(value)) >= 0;
            }

            public bool IsDefined(object value)
            {
                Debug.Assert(value.GetType() != _enumType);

                switch (value)
                {
                    case TUnderlying underlyingValue:
                        return IsDefined(underlyingValue);
                    case string str:
                        string[] names = _names;
                        return Array.IndexOf(names, str, 0, names.Length) >= 0;
                    default:
                        Type valueType = value.GetType();

                        // Check if is another type of enum as checking for the current enum type
                        // is handled in EnumCache.
                        if (valueType.IsEnum)
                        {
                            throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, valueType, _enumType));
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
                                throw new ArgumentException(SR.Format(SR.Arg_EnumUnderlyingTypeAndObjectMustBeSameType, valueType, typeof(TUnderlying)));
                            default:
                                throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
                        }
                }
            }

            public string Format(TUnderlying value, string format)
            {
                if (format == null)
                {
                    throw new ArgumentNullException(nameof(format));
                }

                if (format.Length == 1)
                {
                    switch (format[0])
                    {
                        case 'G':
                        case 'g':
                            return ToString(default(TUnderlyingOperations).ToUInt64(value));
                        case 'D':
                        case 'd':
                            return value.ToString();
                        case 'X':
                        case 'x':
                            return default(TUnderlyingOperations).ToHexStr(value);
                        case 'F':
                        case 'f':
                            return ToStringFlags(default(TUnderlyingOperations).ToUInt64(value));
                    }
                }
                throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
            }

            public string Format(object value, string format)
            {
                Debug.Assert(value.GetType() != _enumType);

                if (value is TUnderlying underlyingValue)
                {
                    return Format(underlyingValue, format);
                }
                throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, value.GetType(), _enumType));
            }

            public TUnderlying Parse(ReadOnlySpan<char> value, bool ignoreCase)
            {
                value = value.TrimStart();

                Number.ParsingStatus status = TryParseInternal(value, ignoreCase, out TUnderlying result);
                if (status == Number.ParsingStatus.OK)
                {
                    return result;
                }
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

                    status = TryParseName(value, ignoreCase, out ulong uint64Result);
                    localResult = operations.ToObject(uint64Result);
                }

                result = status == Number.ParsingStatus.OK ? localResult : default;
                return status;
            }

            public override string ValueToString(ulong value) => default(TUnderlyingOperations).ToObject(value).ToString();
        }
        #endregion

        #region UnderlyingOperations
        private interface IUnderlyingOperations<TUnderlying>
            where TUnderlying : struct, IEquatable<TUnderlying>
        {
            string OverflowMessage { get; }
            bool IsInValueRange(ulong value);
            bool LessThan(TUnderlying left, TUnderlying right);
            TUnderlying Subtract(TUnderlying left, TUnderlying right);
            string ToHexStr(TUnderlying value);
            TUnderlying ToObject(ulong value);
            ulong ToUInt64(TUnderlying value);
            Number.ParsingStatus TryParse(ReadOnlySpan<char> span, out TUnderlying result);
        }

        private readonly struct UnderlyingOperations : IUnderlyingOperations<byte>, IUnderlyingOperations<sbyte>, IUnderlyingOperations<short>, IUnderlyingOperations<ushort>, IUnderlyingOperations<int>, IUnderlyingOperations<uint>, IUnderlyingOperations<long>, IUnderlyingOperations<ulong>, IUnderlyingOperations<bool>, IUnderlyingOperations<char>, IUnderlyingOperations<float>, IUnderlyingOperations<double>, IUnderlyingOperations<IntPtr>, IUnderlyingOperations<UIntPtr>
        {
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

            #region ToHexStr
            public string ToHexStr(byte value) => value.ToString("X2", null);

            public string ToHexStr(sbyte value) => value.ToString("X2", null);

            public string ToHexStr(short value) => value.ToString("X4", null);

            public string ToHexStr(ushort value) => value.ToString("X4", null);

            public string ToHexStr(int value) => value.ToString("X8", null);

            public string ToHexStr(uint value) => value.ToString("X8", null);

            public string ToHexStr(long value) => value.ToString("X16", null);

            public string ToHexStr(ulong value) => value.ToString("X16", null);

            public string ToHexStr(bool value) => Convert.ToByte(value).ToString("X2", null);

            public string ToHexStr(char value) => ((ushort)value).ToString("X4", null);

            public string ToHexStr(float value) => BitConverter.SingleToInt32Bits(value).ToString("X8", null);

            public string ToHexStr(double value) => BitConverter.DoubleToInt64Bits(value).ToString("X16", null);

            public string ToHexStr(IntPtr value)
            {
#if BIT64
                return ((long)value).ToString("X16", null);
#else
                return ((int)value).ToString("X8", null);
#endif
            }

            public string ToHexStr(UIntPtr value)
            {
#if BIT64
                return ((ulong)value).ToString("X16", null);
#else
                return ((uint)value).ToString("X8", null);
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

            #region ToUInt64
            public ulong ToUInt64(byte value) => value;

            public ulong ToUInt64(sbyte value) => (ulong)value;

            public ulong ToUInt64(short value) => (ulong)value;

            public ulong ToUInt64(ushort value) => value;

            public ulong ToUInt64(int value) => (ulong)value;

            public ulong ToUInt64(uint value) => value;

            public ulong ToUInt64(long value) => (ulong)value;

            public ulong ToUInt64(ulong value) => value;

            public ulong ToUInt64(bool value) => Convert.ToUInt64(value);

            public ulong ToUInt64(char value) => value;

            public ulong ToUInt64(float value) => (ulong)BitConverter.SingleToInt32Bits(value);

            public ulong ToUInt64(double value) => (ulong)BitConverter.DoubleToInt64Bits(value);

            public ulong ToUInt64(IntPtr value)
            {
#if BIT64
                return (ulong)(long)value;
#else
                return (ulong)(int)value;
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
        #endregion
        #endregion

        #region Private Methods
        internal object GetValue()
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data);
                case CorElementType.ELEMENT_TYPE_U1:
                    return data;
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data);
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data);
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data);
                case CorElementType.ELEMENT_TYPE_I4:
                    return Unsafe.As<byte, int>(ref data);
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data);
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data);
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data);
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, double>(ref data);
                case CorElementType.ELEMENT_TYPE_I:
                    return Unsafe.As<byte, IntPtr>(ref data);
                case CorElementType.ELEMENT_TYPE_U:
                    return Unsafe.As<byte, UIntPtr>(ref data);
                default:
                    Debug.Fail("Invalid primitive type");
                    return null;
            }
        }

        private string ValueToString()
        {
            // Calling InternalGetCorElementType and switching on the result is faster
            // than getting the cache with a call to EnumCache.Get((RuntimeType)GetType())
            // along with the virtual method ToString call.
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U1:
                    return data.ToString();
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I4:
                    return Unsafe.As<byte, int>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, double>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I:
                    return Unsafe.As<byte, IntPtr>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U:
                    return Unsafe.As<byte, UIntPtr>(ref data).ToString();
                default:
                    Debug.Fail("Invalid primitive type");
                    return null;
            }
        }

        private string ValueToHexString()
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                case CorElementType.ELEMENT_TYPE_U1:
                    return data.ToString("X2", null);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Convert.ToByte(Unsafe.As<byte, bool>(ref data)).ToString("X2", null);
                case CorElementType.ELEMENT_TYPE_I2:
                case CorElementType.ELEMENT_TYPE_U2:
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, ushort>(ref data).ToString("X4", null);
                case CorElementType.ELEMENT_TYPE_I4:
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data).ToString("X8", null);
                case CorElementType.ELEMENT_TYPE_I8:
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data).ToString("X16", null);
                case CorElementType.ELEMENT_TYPE_R4:
                    return BitConverter.SingleToInt32Bits(Unsafe.As<byte, float>(ref data)).ToString("X8", null);
                case CorElementType.ELEMENT_TYPE_R8:
                    return BitConverter.DoubleToInt64Bits(Unsafe.As<byte, double>(ref data)).ToString("X16", null);
                case CorElementType.ELEMENT_TYPE_I:
#if BIT64
                    return ((long)Unsafe.As<byte, IntPtr>(ref data)).ToString("X16", null);
#else
                    return ((int)Unsafe.As<byte, IntPtr>(ref data)).ToString("X8", null);
#endif
                case CorElementType.ELEMENT_TYPE_U:
#if BIT64
                    return ((ulong)Unsafe.As<byte, IntPtr>(ref data)).ToString("X16", null);
#else
                    return ((uint)Unsafe.As<byte, IntPtr>(ref data)).ToString("X8", null);
#endif
                default:
                    Debug.Fail("Invalid primitive type");
                    return null;
            }
        }

        private ulong ToUInt64()
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return (ulong)Unsafe.As<byte, sbyte>(ref data);
                case CorElementType.ELEMENT_TYPE_U1:
                    return data;
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Convert.ToUInt64(Unsafe.As<byte, bool>(ref data), CultureInfo.InvariantCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return (ulong)Unsafe.As<byte, short>(ref data);
                case CorElementType.ELEMENT_TYPE_U2:
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, ushort>(ref data);
                case CorElementType.ELEMENT_TYPE_I4:
                    return (ulong)Unsafe.As<byte, int>(ref data);
                case CorElementType.ELEMENT_TYPE_U4:
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, uint>(ref data);
                case CorElementType.ELEMENT_TYPE_I8:
                    return (ulong)Unsafe.As<byte, long>(ref data);
                case CorElementType.ELEMENT_TYPE_U8:
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, ulong>(ref data);
                case CorElementType.ELEMENT_TYPE_I:
                    return (ulong)Unsafe.As<byte, IntPtr>(ref data);
                case CorElementType.ELEMENT_TYPE_U:
                    return (ulong)Unsafe.As<byte, UIntPtr>(ref data);
                default:
                    Debug.Fail("Invalid primitive type");
                    return 0;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool InternalHasFlag(Enum flags);

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
            return EnumCache.Get((RuntimeType)GetType()).Members.ToString(ToUInt64());
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
                Type thisType = GetType();
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
        public string ToString(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return ToString();
            }

            if (format.Length == 1)
            {
                switch (format[0])
                {
                    case 'G':
                    case 'g':
                        return ToString();
                    case 'D':
                    case 'd':
                        return ValueToString();
                    case 'X':
                    case 'x':
                        return ValueToHexString();
                    case 'F':
                    case 'f':
                        return EnumCache.Get((RuntimeType)GetType()).Members.ToStringFlags(ToUInt64());
                }
            }

            throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
        }

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

            if (!GetType().IsEquivalentTo(flag.GetType()))
            {
                throw new ArgumentException(SR.Format(SR.Argument_EnumTypeDoesNotMatch, flag.GetType(), GetType()));
            }

            return InternalHasFlag(flag);
        }
        #endregion

        #region IConvertible
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

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToChar(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return data;
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToByte(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return Unsafe.As<byte, int>(ref data);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, double>(ref data);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            switch (InternalGetCorElementType())
            {
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I4:
                    return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(SR.Format(SR.InvalidCast_FromTo, "Enum", "DateTime"));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider) =>
            Convert.DefaultToType(this, type, provider);
        #endregion

        #region ToObject
        public static object ToObject(Type enumType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return EnumCache.Get(enumType).ToObjectNonGeneric(ToUInt64(value, throwInvalidOperationException: false));
        }

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, sbyte value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric((ulong)value);

        public static object ToObject(Type enumType, short value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric((ulong)value);

        public static object ToObject(Type enumType, int value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric((ulong)value);

        public static object ToObject(Type enumType, byte value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric(value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ushort value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric(value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, uint value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric(value);

        public static object ToObject(Type enumType, long value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric((ulong)value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ulong value) =>
            EnumCache.Get(enumType).ToObjectNonGeneric(value);
        #endregion
    }
}
