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
        internal static ulong ToUInt64(object value) => ToUInt64(value, throwInvalidOperationException: true);

        internal static ulong ToUInt64(object value, bool throwInvalidOperationException)
        {
            Debug.Assert(value != null);

            // Helper function to silently convert the value to UInt64 from the other base types for enum without throwing an exception.
            // This is needed since the Convert functions do overflow checks.
            TypeCode typeCode = (value as IConvertible)?.GetTypeCode() ?? TypeCode.Object;
            if (typeCode == TypeCode.Int32)
            {
                return (ulong)(int)value;
            }
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    // direct cast from bool to byte is not allowed
                    return Convert.ToByte((bool)value);
                case TypeCode.Char:
                    return (char)value;
                case TypeCode.SByte:
                    return (ulong)(sbyte)value;
                case TypeCode.Byte:
                    return (byte)value;
                case TypeCode.Int16:
                    return (ulong)(short)value;
                case TypeCode.UInt16:
                    return (ushort)value;
                case TypeCode.UInt32:
                    return (uint)value;
                case TypeCode.Int64:
                    return (ulong)(long)value;
                case TypeCode.UInt64:
                    return (ulong)value;
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
            EnumBridge.Get(enumType).TryParse(value, ignoreCase, throwOnFailure: true, out object result);
            return result;
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct =>
            Parse<TEnum>(value, ignoreCase: false);

        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnumBridge<TEnum> bridge = EnumBridge<TEnum>.Instance;
            if (bridge == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            bridge.TryParse(value, ignoreCase, throwOnFailure: true, out TEnum result);
            return result;
        }

        public static bool TryParse(Type enumType, string value, out object result) =>
            TryParse(enumType, value, ignoreCase: false, out result);

        public static bool TryParse(Type enumType, string value, bool ignoreCase, out object result) =>
            EnumBridge.Get(enumType).TryParse(value, ignoreCase, throwOnFailure: false, out result);

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct =>
            TryParse(value, ignoreCase: false, out result);

        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
            EnumBridge<TEnum> bridge = EnumBridge<TEnum>.Instance;
            if (bridge == null)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            }
            return bridge.TryParse(value, ignoreCase, throwOnFailure: false, out result);
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
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            return EnumBridge.Get(enumType).Format(value, format);
        }
        #endregion

        #region ToObject
        public static object ToObject(Type enumType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return ToObject(enumType, ToUInt64(value, throwInvalidOperationException: false));
        }

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, sbyte value) =>
            ToObject(enumType, (ulong)value);

        public static object ToObject(Type enumType, short value) =>
            ToObject(enumType, (ulong)value);

        public static object ToObject(Type enumType, int value) =>
            ToObject(enumType, (ulong)value);

        public static object ToObject(Type enumType, byte value) =>
            ToObject(enumType, (ulong)value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ushort value) =>
            ToObject(enumType, (ulong)value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, uint value) =>
            ToObject(enumType, (ulong)value);

        public static object ToObject(Type enumType, long value) =>
            ToObject(enumType, (ulong)value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ulong value) =>
            EnumBridge.Get(enumType).ToObjectNonGeneric(value);
        #endregion

        #region Definitions
        #region EnumBridge
        // Bridges the enum type with it's underlying type and cache
        internal abstract class EnumBridge
        {
            internal static EnumBridge Get(Type enumType)
            {
                if (!(enumType is RuntimeType rtType))
                {
                    return ThrowGetBridgeException(enumType);
                }

                if (!rtType.IsEnum)
                {
                    throw new ArgumentException(SR.Arg_MustBeEnum, nameof(enumType));
                }

                return Get(rtType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static EnumBridge ThrowGetBridgeException(Type enumType)
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
            internal static EnumBridge Get(RuntimeType rtType)
            {
                Debug.Assert(rtType.IsEnum);

                return rtType.GenericCache as EnumBridge ?? InitializeGenericCache(rtType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static EnumBridge InitializeGenericCache(RuntimeType rtType)
            {
                EnumBridge bridge = (EnumBridge)typeof(EnumBridge<>).MakeGenericType(rtType).GetField(nameof(EnumBridge<DayOfWeek>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) ?? throw new ArgumentException(SR.Argument_InvalidEnum, "enumType");
                rtType.GenericCache = bridge;
                return bridge;
            }

            protected static EnumBridge Create(Type enumType)
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

                return (EnumBridge)Activator.CreateInstance(typeof(EnumBridge<,,>).MakeGenericType(enumType, underlyingType, typeof(UnderlyingOperations)));
            }

            #region Static Method Generic Implementation
            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static string Format<TEnum, TUnderlying, TUnderlyingOperations>(object value, string format)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                EnumCache<TUnderlying, TUnderlyingOperations> cache = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance;
                return value is TEnum enumValue ?
                    cache.Format(Unsafe.As<TEnum, TUnderlying>(ref enumValue), format) :
                    cache.Format(value, format);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static string GetName<TEnum, TUnderlying, TUnderlyingOperations>(object value)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                EnumCache<TUnderlying, TUnderlyingOperations> cache = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance;
                return value is TEnum enumValue ?
                    cache.GetName(Unsafe.As<TEnum, TUnderlying>(ref enumValue)) :
                    cache.GetName(value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static Array GetValuesNonGeneric<TEnum, TUnderlying, TUnderlyingOperations>()
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                ulong[] values = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance._values;
                TEnum[] array = new TEnum[values.Length];
                TUnderlyingOperations operations = default;
                for (int i = 0; i < values.Length; ++i)
                {
                    TUnderlying underlying = operations.ToObject(values[i]);
                    array[i] = Unsafe.As<TUnderlying, TEnum>(ref underlying);
                }
                return array;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static bool IsDefined<TEnum, TUnderlying, TUnderlyingOperations>(object value)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                EnumCache<TUnderlying, TUnderlyingOperations> cache = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance;
                return value is TEnum enumValue ?
                    cache.IsDefined(Unsafe.As<TEnum, TUnderlying>(ref enumValue)) :
                    cache.IsDefined(value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static object ToObject<TEnum, TUnderlying, TUnderlyingOperations>(ulong value)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                TUnderlying underlying = default(TUnderlyingOperations).ToObject(value);
                return Unsafe.As<TUnderlying, TEnum>(ref underlying);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static bool TryParse<TEnum, TUnderlying, TUnderlyingOperations>(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out TEnum result)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                bool success = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance.TryParse(value, ignoreCase, throwOnFailure, out TUnderlying underlying);
                result = Unsafe.As<TUnderlying, TEnum>(ref underlying);
                return success;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            protected static bool TryParse<TEnum, TUnderlying, TUnderlyingOperations>(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out object result)
                where TEnum : struct, Enum
                where TUnderlying : struct, IEquatable<TUnderlying>
                where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
            {
                bool success = EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance.TryParse(value, ignoreCase, throwOnFailure, out TUnderlying underlying);
                TEnum enumResult = Unsafe.As<TUnderlying, TEnum>(ref underlying);
                result = success ? (object)enumResult : null;
                return success;
            }
            #endregion

            public readonly Type UnderlyingType;
            private EnumCache _cache;

            public EnumCache Cache
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _cache ?? InitializeCache();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private EnumCache InitializeCache() => _cache = GetCache();

            protected EnumBridge(Type underlyingType)
            {
                UnderlyingType = underlyingType;
            }

            public abstract string Format(object value, string format);
            public abstract string GetName(object value);
            public abstract Array GetValuesNonGeneric();
            public abstract bool IsDefined(object value);
            public abstract object ToObjectNonGeneric(ulong value);
            public abstract bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out object result);
            protected abstract EnumCache GetCache();
        }

        // Try to minimize code here due to generic code explosion with an Enum generic type argument
        internal abstract class EnumBridge<TEnum> : EnumBridge
        {
            public static readonly EnumBridge<TEnum> Instance = (EnumBridge<TEnum>)Create(typeof(TEnum));

            protected EnumBridge(Type underlyingType)
                : base(underlyingType)
            {
            }

            public abstract bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out TEnum result);
        }

        // Try to minimize code here due to generic code explosion with an Enum generic type argument.
        // Many methods in here delegate their logic to generic static methods to avoid JITing their
        // entire bodies on any use of the enum.
        // Not storing the generic Cache as a static field here allows methods like ToObject to not
        // require the cache to be generated.
        private sealed class EnumBridge<TEnum, TUnderlying, TUnderlyingOperations> : EnumBridge<TEnum>
            where TEnum : struct, Enum
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            public EnumBridge()
                : base(typeof(TUnderlying))
            {
            }

            public override string Format(object value, string format) => Format<TEnum, TUnderlying, TUnderlyingOperations>(value, format);

            public override string GetName(object value) => GetName<TEnum, TUnderlying, TUnderlyingOperations>(value);

            public override Array GetValuesNonGeneric() => GetValuesNonGeneric<TEnum, TUnderlying, TUnderlyingOperations>();

            public override bool IsDefined(object value) => IsDefined<TEnum, TUnderlying, TUnderlyingOperations>(value);

            public override object ToObjectNonGeneric(ulong value) => ToObject<TEnum, TUnderlying, TUnderlyingOperations>(value);

            public override bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out TEnum result) => TryParse<TEnum, TUnderlying, TUnderlyingOperations>(value, ignoreCase, throwOnFailure, out result);

            public override bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out object result) => TryParse<TEnum, TUnderlying, TUnderlyingOperations>(value, ignoreCase, throwOnFailure, out result);

            protected override EnumCache GetCache() => EnumCache<TEnum, TUnderlying, TUnderlyingOperations>.Instance;
        }
        #endregion

        #region EnumCache
        internal abstract class EnumCache
        {
            protected readonly Type _enumType;
            protected readonly bool _isFlagEnum;
            internal readonly string[] _names;
            internal readonly ulong[] _values;

            protected EnumCache(Type enumType)
            {
                _enumType = enumType;
                _isFlagEnum = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
                FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

                string[] names = new string[fields.Length];
                ulong[] values = new ulong[fields.Length];
                for (int i = 0; i < fields.Length; ++i)
                {
                    ulong value = ToUInt64(fields[i].GetRawConstantValue());
                    int index = Array.BinarySearch(values, 0, i, value, null);
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

            public string ToString(ulong value)
            {
                if (_isFlagEnum)
                {
                    return ToStringFlags(value);
                }
                int index = HybridSearch(_values, value);
                if (index >= 0)
                {
                    return _names[index];
                }
                return ValueToString(value);
            }

            public string ToStringFlags(ulong value)
            {
                string[] names = _names;
                ulong[] values = _values;

                
                // Values are sorted, so if the incoming value is 0, we can check to see whether
                // the first entry matches it, in which case we can return its name;
                // otherwise, we return the integral value.
                if (value == 0)
                {
                    return values.Length > 0 && values[0] == 0 ?
                        names[0] :
                        ValueToString(value);
                }

                // It's common to have a flags enum with a single value that matches a single
                // entry, in which case we can just return the existing name string.
                int index = HybridSearch(values, value);
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

            protected abstract string ValueToString(ulong value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected static int HybridSearch(ulong[] values, ulong value)
            {
                const int HybridSearchCutoffLength = 22; // number determined from benchmarking linear vs binary search in the worst case scenario
                if (values.Length <= HybridSearchCutoffLength)
                {
                    for (int i = values.Length - 1; i >= 0; --i)
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
                return Array.BinarySearch(values, 0, values.Length, value, null);
            }
        }

        private sealed class EnumCache<TUnderlying, TUnderlyingOperations> : EnumCache
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            private readonly TUnderlying _max;
            private readonly TUnderlying _min;
            private readonly bool _isContiguous;

            public EnumCache(Type enumType)
                : base(enumType)
            {
                ulong[] values = _values;
                TUnderlyingOperations operations = default;
                TUnderlying max = default;
                TUnderlying min = default;
                for (int i = 0; i < values.Length; ++i)
                {
                    TUnderlying value = operations.ToObject(values[i]);
                    if (i == 0)
                    {
                        max = value;
                        min = value;
                    }
                    else if (operations.LessThan(max, value))
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

            public string GetName(TUnderlying value)
            {
                int index = HybridSearch(_values, default(TUnderlyingOperations).ToUInt64(value));
                if (index >= 0)
                {
                    return _names[index];
                }
                return null;
            }

            public string GetName(object value)
            {
                Debug.Assert(value.GetType() != _enumType);

                ulong uint64Value = ToUInt64(value, throwInvalidOperationException: false);
                TUnderlyingOperations operations = default;
                if (operations.IsInValueRange(uint64Value))
                {
                    int index = HybridSearch(_values, uint64Value);
                    if (index >= 0)
                    {
                        return _names[index];
                    }
                }
                return null;
            }

            public bool IsDefined(TUnderlying value)
            {
                TUnderlyingOperations operations = default;
                if (_isContiguous)
                {
                    return !(operations.LessThan(value, _min) || operations.LessThan(_max, value));
                }
                return HybridSearch(_values, operations.ToUInt64(value)) >= 0;
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
                        // is handled in EnumBridge.
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
                Debug.Assert(format != null);

                if (format.Length == 1)
                {
                    TUnderlyingOperations operations = default;
                    switch (format[0])
                    {
                        case 'G':
                        case 'g':
                            return ToString(operations.ToUInt64(value));
                        case 'D':
                        case 'd':
                            return value.ToString();
                        case 'X':
                        case 'x':
                            return operations.ToHexStr(value);
                        case 'F':
                        case 'f':
                            return ToStringFlags(operations.ToUInt64(value));
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

            protected override string ValueToString(ulong value) => default(TUnderlyingOperations).ToObject(value).ToString();

            public bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out TUnderlying result)
            {
                value = value.TrimStart();
                
                if (value.Length == 0)
                {
                    if (throwOnFailure)
                    {
                        throw new ArgumentException(SR.Arg_MustContainEnumInfo);
                    }
                    result = default;
                    return false;
                }
                
                TUnderlyingOperations operations = default;
                char firstNonWhitespaceChar = value[0];
                if (char.IsInRange(firstNonWhitespaceChar, '0', '9') || firstNonWhitespaceChar == '-' || firstNonWhitespaceChar == '+')
                {
                    Number.ParsingStatus status = operations.TryParse(value, out result);
                    if (status == Number.ParsingStatus.OK)
                    {
                        return true;
                    }
                    else
                    {
                        result = default;
                        if (status == Number.ParsingStatus.Overflow)
                        {
                            if (throwOnFailure)
                            {
                                throw new OverflowException(operations.OverflowMessage);
                            }
                            return false;
                        }
                    }
                }
                    
                ulong uint64Result = default;
                string[] names = _names;
                ulong[] values = _values;
                bool parsed;
                do
                {
                    parsed = false;
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
                                uint64Result |= values[i];
                                parsed = true;
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
                                uint64Result |= values[i];
                                parsed = true;
                                break;
                            }
                        }
                    }
                } while (value.Length > 0 && parsed);

                if (parsed)
                {
                    result = operations.ToObject(uint64Result);
                    return true;
                }

                if (throwOnFailure)
                {
                    throw new ArgumentException(SR.Arg_EnumValueNotFound, nameof(value));
                }

                result = default;
                return false;
            }
        }

        // Simply stores the static instance of the generic Cache
        private sealed class EnumCache<TEnum, TUnderlying, TUnderlyingOperations>
            where TEnum : struct, Enum
            where TUnderlying : struct, IEquatable<TUnderlying>
            where TUnderlyingOperations : struct, IUnderlyingOperations<TUnderlying>
        {
            internal static readonly EnumCache<TUnderlying, TUnderlyingOperations> Instance = new EnumCache<TUnderlying, TUnderlyingOperations>(typeof(TEnum));
        }
        #endregion

        #region UnderlyingOperations
        internal interface IUnderlyingOperations<TUnderlying>
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

            bool IUnderlyingOperations<float>.IsInValueRange(ulong value) => value <= uint.MaxValue;

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

            public bool LessThan(float left, float right) => Unsafe.As<float, uint>(ref left) < Unsafe.As<float, uint>(ref right);

            public bool LessThan(double left, double right) => Unsafe.As<double, ulong>(ref left) < Unsafe.As<double, ulong>(ref right);

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

            public float Subtract(float left, float right)
            {
                uint result = Unsafe.As<float, uint>(ref left) - Unsafe.As<float, uint>(ref right);
                return Unsafe.As<uint, float>(ref result);
            }

            public double Subtract(double left, double right)
            {
                ulong result = Unsafe.As<double, ulong>(ref left) - Unsafe.As<double, ulong>(ref right);
                return Unsafe.As<ulong, double>(ref result);
            }

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

            public string ToHexStr(float value) => Unsafe.As<float, uint>(ref value).ToString("X8", null);

            public string ToHexStr(double value) => Unsafe.As<double, ulong>(ref value).ToString("X16", null);

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

            float IUnderlyingOperations<float>.ToObject(ulong value)
            {
                uint uint32Value = (uint)value;
                return Unsafe.As<uint, float>(ref uint32Value);
            }

            double IUnderlyingOperations<double>.ToObject(ulong value) => Unsafe.As<ulong, double>(ref value);

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

            public ulong ToUInt64(float value) => Unsafe.As<float, uint>(ref value);

            public ulong ToUInt64(double value) => Unsafe.As<double, ulong>(ref value);

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
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return Unsafe.As<byte, int>(ref data);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data);
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data);
                case CorElementType.ELEMENT_TYPE_U1:
                    return data;
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data);
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data);
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data);
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data);
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data);
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
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return Unsafe.As<byte, int>(ref data).ToString();
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U1:
                    return data.ToString();
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data).ToString();
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data).ToString();
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
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return Unsafe.As<byte, uint>(ref data).ToString("X8", null);
            }
            switch (corElementType)
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
                case CorElementType.ELEMENT_TYPE_U4:
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, uint>(ref data).ToString("X8", null);
                case CorElementType.ELEMENT_TYPE_I8:
                case CorElementType.ELEMENT_TYPE_U8:
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, ulong>(ref data).ToString("X16", null);
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
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return (ulong)Unsafe.As<byte, int>(ref data);
            }
            switch (corElementType)
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
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return Unsafe.As<byte, int>(ref data).GetHashCode();
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U1:
                    return data.GetHashCode();
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data).GetHashCode();
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data).GetHashCode();
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
            return EnumBridge.Get(Unsafe.As<RuntimeType>(GetType())).Cache.ToString(ToUInt64());
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
                        return EnumBridge.Get((RuntimeType)GetType()).Cache.ToStringFlags(ToUInt64());
                }
            }

            throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
        }

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
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return TypeCode.Int32;
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return TypeCode.Boolean;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return TypeCode.Char;
                case CorElementType.ELEMENT_TYPE_I1:
                    return TypeCode.SByte;
                case CorElementType.ELEMENT_TYPE_U1:
                    return TypeCode.Byte;
                case CorElementType.ELEMENT_TYPE_I2:
                    return TypeCode.Int16;
                case CorElementType.ELEMENT_TYPE_U2:
                    return TypeCode.UInt16;
                case CorElementType.ELEMENT_TYPE_U4:
                    return TypeCode.UInt32;
                case CorElementType.ELEMENT_TYPE_I8:
                    return TypeCode.Int64;
                case CorElementType.ELEMENT_TYPE_U8:
                    return TypeCode.UInt64;
                case CorElementType.ELEMENT_TYPE_R4:
                    return TypeCode.Single;
                case CorElementType.ELEMENT_TYPE_R8:
                    return TypeCode.Double;
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        [Obsolete("The provider argument is not used. Please use ToString().")]
        public string ToString(IFormatProvider provider) =>
            ToString();

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return Unsafe.As<byte, bool>(ref data);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToBoolean(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToChar(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return Unsafe.As<byte, char>(ref data);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToChar(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToChar(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToSByte(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return Unsafe.As<byte, sbyte>(ref data);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToSByte(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToByte(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return data;
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToByte(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToByte(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToInt16(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return Unsafe.As<byte, short>(ref data);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt16(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return Unsafe.As<byte, ushort>(ref data);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt16(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return Unsafe.As<byte, int>(ref data);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt32(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return Unsafe.As<byte, uint>(ref data);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt32(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToInt64(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Unsafe.As<byte, long>(ref data);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToInt64(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return Unsafe.As<byte, ulong>(ref data);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToUInt64(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToSingle(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return Unsafe.As<byte, float>(ref data);
                case CorElementType.ELEMENT_TYPE_R8:
                    return ((IConvertible)Unsafe.As<byte, double>(ref data)).ToSingle(CultureInfo.CurrentCulture);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToDouble(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToDouble(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R8:
                    return Unsafe.As<byte, double>(ref data);
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            ref byte data = ref this.GetRawData();
            CorElementType corElementType = InternalGetCorElementType();
            if (corElementType == CorElementType.ELEMENT_TYPE_I4)
            {
                return ((IConvertible)Unsafe.As<byte, int>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
            }
            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return ((IConvertible)Unsafe.As<byte, bool>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return ((IConvertible)Unsafe.As<byte, char>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I1:
                    return ((IConvertible)Unsafe.As<byte, sbyte>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U1:
                    return ((IConvertible)data).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I2:
                    return ((IConvertible)Unsafe.As<byte, short>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U2:
                    return ((IConvertible)Unsafe.As<byte, ushort>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U4:
                    return ((IConvertible)Unsafe.As<byte, uint>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_I8:
                    return ((IConvertible)Unsafe.As<byte, long>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_U8:
                    return ((IConvertible)Unsafe.As<byte, ulong>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
                case CorElementType.ELEMENT_TYPE_R4:
                    return ((IConvertible)Unsafe.As<byte, float>(ref data)).ToDecimal(CultureInfo.CurrentCulture);
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
    }
}
