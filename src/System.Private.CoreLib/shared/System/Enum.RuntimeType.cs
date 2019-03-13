// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Internal.Runtime.CompilerServices;

// The code below includes partial support for float/double and
// pointer sized enums.
//
// The type loader does not prohibit such enums, and older versions of
// the ECMA spec include them as possible enum types.
//
// However there are many things broken throughout the stack for
// float/double/intptr/uintptr enums. There was a conscious decision
// made to not fix the whole stack to work well for them because of
// the right behavior is often unclear, and it is hard to test and
// very low value because of such enums cannot be expressed in C#.

namespace System
{
    public abstract partial class Enum
    {
        [Intrinsic]
        public bool HasFlag(Enum flag)
        {
            if (flag == null)
                throw new ArgumentNullException(nameof(flag));

            if (!this.GetType().IsEquivalentTo(flag.GetType()))
            {
                throw new ArgumentException(SR.Format(SR.Argument_EnumTypeDoesNotMatch, flag.GetType(), this.GetType()));
            }

            return InternalHasFlag(flag);
        }

        private static string InternalFormat(RuntimeType eT, ulong value)
        {
            Debug.Assert(eT != null);

            // These values are sorted by value. Don't change this
            TypeValuesAndNames entry = GetCachedValuesAndNames(eT, true);

            if (!entry.IsFlag) // Not marked with Flags attribute
            {
                return Enum.GetEnumName(eT, value);
            }
            else // These are flags OR'ed together (We treat everything as unsigned types)
            {
                return InternalFlagsFormat(eT, entry, value);
            }
        }

        private static string InternalFlagsFormat(RuntimeType eT, ulong result)
        {
            // These values are sorted by value. Don't change this
            TypeValuesAndNames entry = GetCachedValuesAndNames(eT, true);

            return InternalFlagsFormat(eT, entry, result);
        }

        private static string InternalFlagsFormat(RuntimeType eT, TypeValuesAndNames entry, ulong resultValue)
        {
            Debug.Assert(eT != null);

            string[] names = entry.Names;
            ulong[] values = entry.Values;
            Debug.Assert(names.Length == values.Length);

            // Values are sorted, so if the incoming value is 0, we can check to see whether
            // the first entry matches it, in which case we can return its name; otherwise,
            // we can just return "0".
            if (resultValue == 0)
            {
                return values.Length > 0 && values[0] == 0 ?
                    names[0] :
                    "0";
            }

            // With a ulong result value, regardless of the enum's base type, the maximum
            // possible number of consistent name/values we could have is 64, since every
            // value is made up of one or more bits, and when we see values and incorporate
            // their names, we effectively switch off those bits.
            Span<int> foundItems = stackalloc int[64];

            // Walk from largest to smallest. It's common to have a flags enum with a single
            // value that matches a single entry, in which case we can just return the existing
            // name string.
            int index = values.Length - 1;
            while (index >= 0)
            {
                if (values[index] == resultValue)
                {
                    return names[index];
                }

                if (values[index] < resultValue)
                {
                    break;
                }

                index--;
            }

            // Now look for multiple matches, storing the indices of the values
            // into our span.
            int resultLength = 0, foundItemsCount = 0;
            while (index >= 0)
            {
                ulong currentValue = values[index];
                if (index == 0 && currentValue == 0)
                {
                    break;
                }

                if ((resultValue & currentValue) == currentValue)
                {
                    resultValue -= currentValue;
                    foundItems[foundItemsCount++] = index;
                    resultLength = checked(resultLength + names[index].Length);
                }

                index--;
            }

            // If we exhausted looking through all the values and we still have
            // a non-zero result, we couldn't match the result to only named values.
            // In that case, we return null and let the call site just generate
            // a string for the integral value.
            if (resultValue != 0)
            {
                return null;
            }

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

        internal static string GetEnumName(RuntimeType eT, ulong ulValue)
        {
            Debug.Assert(eT != null);
            ulong[] ulValues = Enum.InternalGetValues(eT);
            int index = Array.BinarySearch(ulValues, ulValue);

            if (index >= 0)
            {
                string[] names = Enum.InternalGetNames(eT);
                return names[index];
            }

            return null; // return null so the caller knows to .ToString() the input
        }

        internal static ulong[] InternalGetValues(RuntimeType enumType)
        {
            // Get all of the values
            return GetCachedValuesAndNames(enumType, false).Values;
        }

        private static bool TryParse(Type enumType, string value, bool ignoreCase, bool throwOnFailure, out object result)
        {
            // Validation on the enum type itself.  Failures here are considered non-parsing failures
            // and thus always throw rather than returning false.
            RuntimeType rt = ValidateRuntimeType(enumType);

            ReadOnlySpan<char> valueSpan = value.AsSpan().TrimStart();
            if (valueSpan.Length == 0)
            {
                if (throwOnFailure)
                {
                    throw value == null ?
                        new ArgumentNullException(nameof(value)) :
                        new ArgumentException(SR.Arg_MustContainEnumInfo, nameof(value));
                }
                result = null;
                return false;
            }

            int intResult;
            uint uintResult;
            bool parsed;

            switch (Type.GetTypeCode(rt))
            {
                case TypeCode.SByte:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, sbyte.MinValue, sbyte.MaxValue, ignoreCase, throwOnFailure, TypeCode.SByte, out intResult);
                    result = parsed ? InternalBoxEnum(rt, intResult) : null;
                    return parsed;

                case TypeCode.Int16:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, short.MinValue, short.MaxValue, ignoreCase, throwOnFailure, TypeCode.Int16, out intResult);
                    result = parsed ? InternalBoxEnum(rt, intResult) : null;
                    return parsed;

                case TypeCode.Int32:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, int.MinValue, int.MaxValue, ignoreCase, throwOnFailure, TypeCode.Int32, out intResult);
                    result = parsed ? InternalBoxEnum(rt, intResult) : null;
                    return parsed;

                case TypeCode.Byte:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, byte.MaxValue, ignoreCase, throwOnFailure, TypeCode.Byte, out uintResult);
                    result = parsed ? InternalBoxEnum(rt, uintResult) : null;
                    return parsed;

                case TypeCode.UInt16:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, ushort.MaxValue, ignoreCase, throwOnFailure, TypeCode.UInt16, out uintResult);
                    result = parsed ? InternalBoxEnum(rt, uintResult) : null;
                    return parsed;

                case TypeCode.UInt32:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, uint.MaxValue, ignoreCase, throwOnFailure, TypeCode.UInt32, out uintResult);
                    result = parsed ? InternalBoxEnum(rt, uintResult) : null;
                    return parsed;

                case TypeCode.Int64:
                    parsed = TryParseInt64Enum(rt, value, valueSpan, ignoreCase, throwOnFailure, out long longResult);
                    result = parsed ? InternalBoxEnum(rt, longResult) : null;
                    return parsed;

                case TypeCode.UInt64:
                    parsed = TryParseUInt64Enum(rt, value, valueSpan, ignoreCase, throwOnFailure, out ulong ulongResult);
                    result = parsed ? InternalBoxEnum(rt, (long)ulongResult) : null;
                    return parsed;

                default:
                    return TryParseRareEnum(rt, value, valueSpan, ignoreCase, throwOnFailure, out result);
            }
        }


        private static bool TryParse<TEnum>(string value, bool ignoreCase, bool throwOnFailure, out TEnum result) where TEnum : struct
        {
            // Validation on the enum type itself.  Failures here are considered non-parsing failures
            // and thus always throw rather than returning false.
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException(SR.Arg_MustBeEnum, nameof(TEnum));
            }

            ReadOnlySpan<char> valueSpan = value.AsSpan().TrimStart();
            if (valueSpan.Length == 0)
            {
                if (throwOnFailure)
                {
                    throw value == null ?
                        new ArgumentNullException(nameof(value)) :
                        new ArgumentException(SR.Arg_MustContainEnumInfo, nameof(value));
                }
                result = default;
                return false;
            }

            int intResult;
            uint uintResult;
            bool parsed;
            RuntimeType rt = (RuntimeType)typeof(TEnum);

            switch (Type.GetTypeCode(typeof(TEnum)))
            {
                case TypeCode.SByte:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, sbyte.MinValue, sbyte.MaxValue, ignoreCase, throwOnFailure, TypeCode.SByte, out intResult);
                    sbyte sbyteResult = (sbyte)intResult;
                    result = Unsafe.As<sbyte, TEnum>(ref sbyteResult);
                    return parsed;

                case TypeCode.Int16:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, short.MinValue, short.MaxValue, ignoreCase, throwOnFailure, TypeCode.Int16, out intResult);
                    short shortResult = (short)intResult;
                    result = Unsafe.As<short, TEnum>(ref shortResult);
                    return parsed;

                case TypeCode.Int32:
                    parsed = TryParseInt32Enum(rt, value, valueSpan, int.MinValue, int.MaxValue, ignoreCase, throwOnFailure, TypeCode.Int32, out intResult);
                    result = Unsafe.As<int, TEnum>(ref intResult);
                    return parsed;

                case TypeCode.Byte:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, byte.MaxValue, ignoreCase, throwOnFailure, TypeCode.Byte, out uintResult);
                    byte byteResult = (byte)uintResult;
                    result = Unsafe.As<byte, TEnum>(ref byteResult);
                    return parsed;

                case TypeCode.UInt16:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, ushort.MaxValue, ignoreCase, throwOnFailure, TypeCode.UInt16, out uintResult);
                    ushort ushortResult = (ushort)uintResult;
                    result = Unsafe.As<ushort, TEnum>(ref ushortResult);
                    return parsed;

                case TypeCode.UInt32:
                    parsed = TryParseUInt32Enum(rt, value, valueSpan, uint.MaxValue, ignoreCase, throwOnFailure, TypeCode.UInt32, out uintResult);
                    result = Unsafe.As<uint, TEnum>(ref uintResult);
                    return parsed;

                case TypeCode.Int64:
                    parsed = TryParseInt64Enum(rt, value, valueSpan, ignoreCase, throwOnFailure, out long longResult);
                    result = Unsafe.As<long, TEnum>(ref longResult);
                    return parsed;

                case TypeCode.UInt64:
                    parsed = TryParseUInt64Enum(rt, value, valueSpan, ignoreCase, throwOnFailure, out ulong ulongResult);
                    result = Unsafe.As<ulong, TEnum>(ref ulongResult);
                    return parsed;

                default:
                    parsed = TryParseRareEnum(rt, value, valueSpan, ignoreCase, throwOnFailure, out object objectResult);
                    result = parsed ? (TEnum)objectResult : default;
                    return parsed;
            }
        }

        /// <summary>Tries to parse the value of an enum with known underlying types that fit in an Int32 (Int32, Int16, and SByte).</summary>
        private static bool TryParseInt32Enum(
            RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, int minInclusive, int maxInclusive, bool ignoreCase, bool throwOnFailure, TypeCode type, out int result)
        {
            Debug.Assert(
                enumType.GetEnumUnderlyingType() == typeof(sbyte) ||
                enumType.GetEnumUnderlyingType() == typeof(short) ||
                enumType.GetEnumUnderlyingType() == typeof(int));

            Number.ParsingStatus status = default;
            if (StartsNumber(value[0]))
            {
                status = Number.TryParseInt32IntegerStyle(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);
                if (status == Number.ParsingStatus.OK)
                {
                    if ((uint)(result - minInclusive) <= (uint)(maxInclusive - minInclusive))
                    {
                        return true;
                    }

                    status = Number.ParsingStatus.Overflow;
                }
            }

            if (status == Number.ParsingStatus.Overflow)
            {
                if (throwOnFailure)
                {
                    Number.ThrowOverflowException(type);
                }
            }
            else if (TryParseByName(enumType, originalValueString, value, ignoreCase, throwOnFailure, out ulong ulongResult))
            {
                result = (int)ulongResult;
                Debug.Assert(result >= minInclusive && result <= maxInclusive);
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>Tries to parse the value of an enum with known underlying types that fit in a UInt32 (UInt32, UInt16, and Byte).</summary>
        private static bool TryParseUInt32Enum(RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, uint maxInclusive, bool ignoreCase, bool throwOnFailure, TypeCode type, out uint result)
        {
            Debug.Assert(
                enumType.GetEnumUnderlyingType() == typeof(byte) ||
                enumType.GetEnumUnderlyingType() == typeof(ushort) ||
                enumType.GetEnumUnderlyingType() == typeof(uint));

            Number.ParsingStatus status = default;
            if (StartsNumber(value[0]))
            {
                status = Number.TryParseUInt32IntegerStyle(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);
                if (status == Number.ParsingStatus.OK)
                {
                    if (result <= maxInclusive)
                    {
                        return true;
                    }

                    status = Number.ParsingStatus.Overflow;
                }
            }

            if (status == Number.ParsingStatus.Overflow)
            {
                if (throwOnFailure)
                {
                    Number.ThrowOverflowException(type);
                }
            }
            else if (TryParseByName(enumType, originalValueString, value, ignoreCase, throwOnFailure, out ulong ulongResult))
            {
                result = (uint)ulongResult;
                Debug.Assert(result <= maxInclusive);
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>Tries to parse the value of an enum with Int64 as the underlying type.</summary>
        private static bool TryParseInt64Enum(RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out long result)
        {
            Debug.Assert(enumType.GetEnumUnderlyingType() == typeof(long));

            Number.ParsingStatus status = default;
            if (StartsNumber(value[0]))
            {
                status = Number.TryParseInt64IntegerStyle(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);
                if (status == Number.ParsingStatus.OK)
                {
                    return true;
                }
            }

            if (status == Number.ParsingStatus.Overflow)
            {
                if (throwOnFailure)
                {
                    Number.ThrowOverflowException(TypeCode.Int64);
                }
            }
            else if (TryParseByName(enumType, originalValueString, value, ignoreCase, throwOnFailure, out ulong ulongResult))
            {
                result = (long)ulongResult;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>Tries to parse the value of an enum with UInt64 as the underlying type.</summary>
        private static bool TryParseUInt64Enum(RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out ulong result)
        {
            Debug.Assert(enumType.GetEnumUnderlyingType() == typeof(ulong));

            Number.ParsingStatus status = default;
            if (StartsNumber(value[0]))
            {
                status = Number.TryParseUInt64IntegerStyle(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture.NumberFormat, out result);
                if (status == Number.ParsingStatus.OK)
                {
                    return true;
                }
            }

            if (status == Number.ParsingStatus.Overflow)
            {
                if (throwOnFailure)
                {
                    Number.ThrowOverflowException(TypeCode.UInt64);
                }
            }
            else if (TryParseByName(enumType, originalValueString, value, ignoreCase, throwOnFailure, out result))
            {
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>Tries to parse the value of an enum with an underlying type that can't be expressed in C# (e.g. char, bool, double, etc.)</summary>
        private static bool TryParseRareEnum(RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out object result)
        {
            Debug.Assert(
                enumType.GetEnumUnderlyingType() != typeof(sbyte) &&
                enumType.GetEnumUnderlyingType() != typeof(byte) &&
                enumType.GetEnumUnderlyingType() != typeof(short) &&
                enumType.GetEnumUnderlyingType() != typeof(ushort) &&
                enumType.GetEnumUnderlyingType() != typeof(int) &&
                enumType.GetEnumUnderlyingType() != typeof(uint) &&
                enumType.GetEnumUnderlyingType() != typeof(long) &&
                enumType.GetEnumUnderlyingType() != typeof(ulong),
                "Should only be used when parsing enums with rare underlying types, those that can't be expressed in C#.");

            if (StartsNumber(value[0]))
            {
                Type underlyingType = GetUnderlyingType(enumType);
                try
                {
                    result = ToObject(enumType, Convert.ChangeType(value.ToString(), underlyingType, CultureInfo.InvariantCulture));
                    return true;
                }
                catch (FormatException)
                {
                    // We need to Parse this as a String instead. There are cases
                    // when you tlbimp enums that can have values of the form "3D".
                }
                catch when (!throwOnFailure)
                {
                    result = null;
                    return false;
                }
            }

            if (TryParseByName(enumType, originalValueString, value, ignoreCase, throwOnFailure, out ulong ulongResult))
            {
                try
                {
                    result = ToObject(enumType, ulongResult);
                    return true;
                }
                catch when (!throwOnFailure) { }
            }

            result = null;
            return false;
        }

        private static bool TryParseByName(RuntimeType enumType, string originalValueString, ReadOnlySpan<char> value, bool ignoreCase, bool throwOnFailure, out ulong result)
        {
            // Find the field. Let's assume that these are always static classes because the class is an enum.
            TypeValuesAndNames entry = GetCachedValuesAndNames(enumType, getNames: true);
            string[] enumNames = entry.Names;
            ulong[] enumValues = entry.Values;

            bool parsed = true;
            ulong localResult = 0;
            while (value.Length > 0)
            {
                // Find the next separator.
                ReadOnlySpan<char> subvalue;
                int endIndex = value.IndexOf(EnumSeparatorChar);
                if (endIndex == -1)
                {
                    // No next separator; use the remainder as the next value.
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
                    parsed = false;
                    break;
                }

                // Try to match this substring against each enum name
                bool success = false;
                if (ignoreCase)
                {
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        if (subvalue.EqualsOrdinalIgnoreCase(enumNames[i]))
                        {
                            localResult |= enumValues[i];
                            success = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        if (subvalue.EqualsOrdinal(enumNames[i]))
                        {
                            localResult |= enumValues[i];
                            success = true;
                            break;
                        }
                    }
                }

                if (!success)
                {
                    parsed = false;
                    break;
                }
            }

            if (parsed)
            {
                result = localResult;
                return true;
            }

            if (throwOnFailure)
            {
                throw new ArgumentException(SR.Format(SR.Arg_EnumValueNotFound, originalValueString));
            }

            result = 0;
            return false;
        }

        internal static string[] InternalGetNames(RuntimeType enumType)
        {
            // Get all of the names
            return GetCachedValuesAndNames(enumType, true).Names;
        }
        public static string Format(Type enumType, object value, string format)
        {
            RuntimeType rtType = ValidateRuntimeType(enumType);

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            // If the value is an Enum then we need to extract the underlying value from it
            Type valueType = value.GetType();
            if (valueType.IsEnum)
            {
                if (!valueType.IsEquivalentTo(enumType))
                    throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, valueType, enumType));

                if (format.Length != 1)
                {
                    // all acceptable format string are of length 1
                    throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
                }
                return ((Enum)value).ToString(format);
            }

            // The value must be of the same type as the Underlying type of the Enum
            Type underlyingType = GetUnderlyingType(enumType);
            if (valueType != underlyingType)
            {
                throw new ArgumentException(SR.Format(SR.Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType, valueType, underlyingType));
            }

            if (format.Length == 1)
            {
                switch (format[0])
                {
                    case 'G':
                    case 'g':
                        return GetEnumName(rtType, ToUInt64(value)) ?? value.ToString();

                    case 'D':
                    case 'd':
                        return value.ToString();

                    case 'X':
                    case 'x':
                        return ValueToHexString(value);

                    case 'F':
                    case 'f':
                        return InternalFlagsFormat(rtType, ToUInt64(value)) ?? value.ToString();
                }
            }

            throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
        }

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
                        return InternalFlagsFormat((RuntimeType)GetType(), ToUInt64()) ?? ValueToString();
                }
            }

            throw new FormatException(SR.Format_InvalidEnumFormatSpecification);
        }

        public override string ToString()
        {
            // Returns the value in a human readable format.  For PASCAL style enums who's value maps directly the name of the field is returned.
            // For PASCAL style enums who's values do not map directly the decimal value of the field is returned.
            // For BitFlags (indicated by the Flags custom attribute): If for each bit that is set in the value there is a corresponding constant
            // (a pure power of 2), then the OR string (ie "Red, Yellow") is returned. Otherwise, if the value is zero or if you can't create a string that consists of
            // pure powers of 2 OR-ed together, you return a hex value

            // Try to see if its one of the enum values, then we return a String back else the value
            return InternalFormat((RuntimeType)GetType(), ToUInt64()) ?? ValueToString();
        }

        #region IComparable
        public int CompareTo(object target)
        {
            const int retIncompatibleMethodTables = 2;  // indicates that the method tables did not match
            const int retInvalidEnumType = 3; // indicates that the enum was of an unknown/unsupported underlying type

            if (this == null)
                throw new NullReferenceException();

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

                throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, targetType, thisType));
            }
            else
            {
                // assert valid return code (3)
                Debug.Assert(ret == retInvalidEnumType, "Enum.InternalCompareTo return code was invalid");

                throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }
        #endregion

        private static RuntimeType ValidateRuntimeType(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum)
                throw new ArgumentException(SR.Arg_MustBeEnum, nameof(enumType));
            if (!(enumType is RuntimeType rtType))
                throw new ArgumentException(SR.Arg_MustBeType, nameof(enumType));
            return rtType;
        }

        private static object ToObjectWorker(Type enumType, long value)
        {
            return InternalBoxEnum(ValidateRuntimeType(enumType), value);
        }
    }
}
