// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

#if CORERT
using CorElementType = System.Runtime.RuntimeImports.RhCorElementType;
#endif

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
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract partial class Enum : ValueType, IComparable, IFormattable, IConvertible
    {
#region Private Constants
        private const char EnumSeparatorChar = ',';
#endregion

#region Private Static Methods

        private string ValueToString()
        {
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
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        private static string ValueToHexString(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                    return ((byte)(sbyte)value).ToString("X2", null);
                case TypeCode.Byte:
                    return ((byte)value).ToString("X2", null);
                case TypeCode.Boolean:
                    // direct cast from bool to byte is not allowed
                    return Convert.ToByte((bool)value).ToString("X2", null);
                case TypeCode.Int16:
                    return ((ushort)(short)value).ToString("X4", null);
                case TypeCode.UInt16:
                    return ((ushort)value).ToString("X4", null);
                case TypeCode.Char:
                    return ((ushort)(char)value).ToString("X4", null);
                case TypeCode.UInt32:
                    return ((uint)value).ToString("X8", null);
                case TypeCode.Int32:
                    return ((uint)(int)value).ToString("X8", null);
                case TypeCode.UInt64:
                    return ((ulong)value).ToString("X16", null);
                case TypeCode.Int64:
                    return ((ulong)(long)value).ToString("X16", null);
                // All unsigned types will be directly cast
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }

        internal static ulong ToUInt64(object value)
        {
            // Helper function to silently convert the value to UInt64 from the other base types for enum without throwing an exception.
            // This is need since the Convert functions do overflow checks.
            TypeCode typeCode = Convert.GetTypeCode(value);

            ulong result;
            switch (typeCode)
            {
                case TypeCode.SByte:
                    result = (ulong)(sbyte)value;
                    break;
                case TypeCode.Byte:
                    result = (byte)value;
                    break;
                case TypeCode.Boolean:
                    // direct cast from bool to byte is not allowed
                    result = Convert.ToByte((bool)value);
                    break;
                case TypeCode.Int16:
                    result = (ulong)(short)value;
                    break;
                case TypeCode.UInt16:
                    result = (ushort)value;
                    break;
                case TypeCode.Char:
                    result = (ushort)(char)value;
                    break;
                case TypeCode.UInt32:
                    result = (uint)value;
                    break;
                case TypeCode.Int32:
                    result = (ulong)(int)value;
                    break;
                case TypeCode.UInt64:
                    result = (ulong)value;
                    break;
                case TypeCode.Int64:
                    result = (ulong)(long)value;
                    break;
                // All unsigned types will be directly cast
                default:
                    throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }

            return result;
        }
        #endregion

#region Public Static Methods
        public static bool IsDefined(Type enumType, object value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.IsEnumDefined(value);
        }

        public static Type GetUnderlyingType(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.GetEnumUnderlyingType();
        }

        public static Array GetValues(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.GetEnumValues();
        }

        public static string GetName(Type enumType, object value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.GetEnumName(value);
        }

        public static string[] GetNames(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.GetEnumNames();
        }

        public static object Parse(Type enumType, string value) =>
            Parse(enumType, value, ignoreCase: false);

        public static object Parse(Type enumType, string value, bool ignoreCase)
        {
            bool success = TryParse(enumType, value, ignoreCase, throwOnFailure: true, out object result);
            Debug.Assert(success);
            return result;
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct =>
            Parse<TEnum>(value, ignoreCase: false);

        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct
        {
            bool success = TryParse<TEnum>(value, ignoreCase, throwOnFailure: true, out TEnum result);
            Debug.Assert(success);
            return result;
        }

        public static bool TryParse(Type enumType, string value, out object result) =>
            TryParse(enumType, value, ignoreCase: false, out result);

        public static bool TryParse(Type enumType, string value, bool ignoreCase, out object result) =>
            TryParse(enumType, value, ignoreCase, throwOnFailure: false, out result);

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct =>
            TryParse<TEnum>(value, ignoreCase: false, out result);

        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct =>
            TryParse<TEnum>(value, ignoreCase, throwOnFailure: false, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StartsNumber(char c) => char.IsInRange(c, '0', '9') || c == '-' || c == '+';

        public static object ToObject(Type enumType, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Delegate rest of error checking to the other functions
            TypeCode typeCode = Convert.GetTypeCode(value);

            switch (typeCode)
            {
                case TypeCode.Int32:
                    return ToObject(enumType, (int)value);

                case TypeCode.SByte:
                    return ToObject(enumType, (sbyte)value);

                case TypeCode.Int16:
                    return ToObject(enumType, (short)value);

                case TypeCode.Int64:
                    return ToObject(enumType, (long)value);

                case TypeCode.UInt32:
                    return ToObject(enumType, (uint)value);

                case TypeCode.Byte:
                    return ToObject(enumType, (byte)value);

                case TypeCode.UInt16:
                    return ToObject(enumType, (ushort)value);

                case TypeCode.UInt64:
                    return ToObject(enumType, (ulong)value);

                case TypeCode.Char:
                    return ToObject(enumType, (char)value);

                case TypeCode.Boolean:
                    return ToObject(enumType, (bool)value);

                default:
                    // All unsigned types will be directly cast
                    throw new ArgumentException(SR.Arg_MustBeEnumBaseTypeOrEnum, nameof(value));
            }
        }
#endregion

#region Definitions
        private class TypeValuesAndNames
        {
            public readonly bool IsFlag;
            public readonly ulong[] Values;
            public readonly string[] Names;

            // Each entry contains a list of sorted pair of enum field names and values, sorted by values
            public TypeValuesAndNames(bool isFlag, ulong[] values, string[] names)
            {
                IsFlag = isFlag;
                Values = values;
                Names = names;
            }
        }
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

#endregion

#region Object Overrides

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

#endregion

#region IFormattable
        [Obsolete("The provider argument is not used. Please use ToString(String).")]
        public string ToString(string format, IFormatProvider provider)
        {
            return ToString(format);
        }
#endregion

#region Public Methods

        [Obsolete("The provider argument is not used. Please use ToString().")]
        public string ToString(IFormatProvider provider)
        {
            return ToString();
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

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(GetValue(), CultureInfo.CurrentCulture);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(GetValue(), CultureInfo.CurrentCulture);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(GetValue(), CultureInfo.CurrentCulture);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(GetValue(), CultureInfo.CurrentCulture);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(GetValue(), CultureInfo.CurrentCulture);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(GetValue(), CultureInfo.CurrentCulture);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(GetValue(), CultureInfo.CurrentCulture);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(GetValue(), CultureInfo.CurrentCulture);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(GetValue(), CultureInfo.CurrentCulture);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(GetValue(), CultureInfo.CurrentCulture);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(GetValue(), CultureInfo.CurrentCulture);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(GetValue(), CultureInfo.CurrentCulture);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(GetValue(), CultureInfo.CurrentCulture);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(SR.Format(SR.InvalidCast_FromTo, "Enum", "DateTime"));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }
        #endregion

        #region ToObject
        [CLSCompliant(false)]
        public static object ToObject(Type enumType, sbyte value) =>
            ToObjectWorker(enumType, value);

        public static object ToObject(Type enumType, short value) =>
            ToObjectWorker(enumType, value);

        public static object ToObject(Type enumType, int value) =>
            ToObjectWorker(enumType, value);

        public static object ToObject(Type enumType, byte value) =>
            ToObjectWorker(enumType, value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ushort value) =>
            ToObjectWorker(enumType, value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, uint value) =>
            ToObjectWorker(enumType, value);

        public static object ToObject(Type enumType, long value) =>
            ToObjectWorker(enumType, value);

        [CLSCompliant(false)]
        public static object ToObject(Type enumType, ulong value) =>
            ToObjectWorker(enumType, unchecked((long)value));

        private static object ToObject(Type enumType, char value) =>
            ToObjectWorker(enumType, value);

        private static object ToObject(Type enumType, bool value) =>
            ToObjectWorker(enumType, value ? 1 : 0);
        #endregion
    }
}
