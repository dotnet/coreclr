// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    public abstract partial class Enum
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public override extern bool Equals(object? obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern object InternalBoxEnum(RuntimeType enumType, long value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int InternalCompareTo(object thisRef, object? target);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern CorElementType InternalGetCorElementType();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool InternalHasFlag(Enum flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EnumInfo? GetEnumInfo(RuntimeType rtType) => rtType.GenericCache as EnumInfo ?? InitializeGenericCache(rtType);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static EnumInfo? InitializeGenericCache(RuntimeType enumType)
        {
            EnumInfo? info = CreateEnumInfo(enumType);
            if (info != null)
            {
                enumType.GenericCache = info;
            }
            return info;
        }

        private static EnumInfo? CreateEnumInfo(RuntimeType enumType)
        {
            CorElementType? corElementType = GetCorElementType(enumType);
            if (!corElementType.HasValue)
            {
                return null;
            }

            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

            string[] names = new string[fields.Length];
            ulong[] values = new ulong[fields.Length];
            for (int i = 0; i < fields.Length; ++i)
            {
                ulong value = ToUInt64(fields[i].GetRawConstantValue());
                names[i] = fields[i].Name;
                values[i] = value;
            }

            bool isFlagEnum = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

            switch (corElementType)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    return new EnumInfo<bool, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return new EnumInfo<char, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_I1:
                    return new EnumInfo<sbyte, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_U1:
                    return new EnumInfo<byte, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_I2:
                    return new EnumInfo<short, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_U2:
                    return new EnumInfo<ushort, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_I4:
                    return new EnumInfo<int, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_U4:
                    return new EnumInfo<uint, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_I8:
                    return new EnumInfo<long, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_U8:
                    return new EnumInfo<ulong, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_R4:
                    return new EnumInfo<float, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_R8:
                    return new EnumInfo<double, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_I:
                    return new EnumInfo<IntPtr, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                case CorElementType.ELEMENT_TYPE_U:
                    return new EnumInfo<UIntPtr, UnderlyingOperations>(enumType, names, values, isFlagEnum);
                default:
                    return null;
            }
        }

        private static CorElementType? GetCorElementType(RuntimeType enumType)
        {
            if (!enumType.IsEnum)
            {
                return null;
            }

            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields.Length != 1)
            {
                return null;
            }

            Type underlyingType = fields[0].FieldType;

            // Allow underlying type of another enum type as is done in a System.Reflection.Emit test
            if (underlyingType.IsEnum && underlyingType != enumType)
            {
                underlyingType = underlyingType.GetEnumUnderlyingType();
            }

            return RuntimeTypeHandle.GetCorElementType((RuntimeType)underlyingType);
        }

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

        public static string? GetName(Type enumType, object value)
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

        public static bool IsDefined(Type enumType, object value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            return enumType.IsEnumDefined(value);
        }

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

        public int CompareTo(object? target)
        {
            const int retIncompatibleMethodTables = 2;  // indicates that the method tables did not match
            const int retInvalidEnumType = 3; // indicates that the enum was of an unknown/unsupported underlying type

            // Unlike C#, IL does not prevent you from calling a method with null this pointer.
            // Accessing the null this pointer in managed code produces NullReferenceException that the caller
            // can handle like any other exception.
            // Accessing null this pointer in the unmanaged runtime causes immediate fatal crash that does not produce
            // NullReferenceException. This explicit check for null before calling unmanaged runtime call is there to
            // still throw NullReferenceException instead of the fatal crash.
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
                Type targetType = target!.GetType();

                throw new ArgumentException(SR.Format(SR.Arg_EnumAndObjectMustBeSameType, targetType, thisType));
            }
            else
            {
                // assert valid return code (3)
                Debug.Assert(ret == retInvalidEnumType, "Enum.InternalCompareTo return code was invalid");

                throw new InvalidOperationException(SR.InvalidOperation_UnknownEnumType);
            }
        }
    }
}
