// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Collections.Generic
{
    internal static class RareComparers
    {
        internal static object TryCreateNullableComparer(RuntimeType nullableType)
        {
            Debug.Assert(nullableType != null);
            Debug.Assert(nullableType.IsGenericType && nullableType.GetGenericTypeDefinition() == typeof(Nullable<>));
            
            var embeddedType = (RuntimeType)nullableType.GetGenericArguments()[0];

            if (typeof(IComparable<>).MakeGenericType(embeddedType).IsAssignableFrom(embeddedType))
            {
                return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(NullableComparer<int>), embeddedType);
            }

            return null;
        }

        internal static object TryCreateEnumComparer(RuntimeType enumType)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);

            // Explicitly call Enum.GetUnderlyingType here. Although GetTypeCode
            // ends up doing this anyway, we end up avoiding an unnecessary P/Invoke
            // and virtual method call.
            TypeCode underlyingTypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(enumType));
            
            // Depending on the enum type, we need to special case the comparers so that we avoid boxing.
            // Specialize differently for signed/unsigned types so we avoid problems with large numbers.
            switch (underlyingTypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(Int32EnumComparer<int>), enumType);
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(UInt32EnumComparer<uint>), enumType);
                // 64-bit enums: Use `UnsafeEnumCastLong`
                case TypeCode.Int64:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(Int64EnumComparer<long>), enumType);
                case TypeCode.UInt64:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(UInt64EnumComparer<ulong>), enumType);
            }
            
            return null;
        }

        internal static object TryCreateNullableEqualityComparer(RuntimeType nullableType)
        {
            Debug.Assert(nullableType != null);
            Debug.Assert(nullableType.IsGenericType && nullableType.GetGenericTypeDefinition() == typeof(Nullable<>));

            var embeddedType = (RuntimeType)nullableType.GetGenericArguments()[0];

            if (typeof(IEquatable<>).MakeGenericType(embeddedType).IsAssignableFrom(embeddedType))
            {
                return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(NullableEqualityComparer<int>), embeddedType);
            }
            
            return null;
        }

        internal static object TryCreateEnumEqualityComparer(RuntimeType enumType)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);

            // See the METHOD__JIT_HELPERS__UNSAFE_ENUM_CAST and METHOD__JIT_HELPERS__UNSAFE_ENUM_CAST_LONG cases in getILIntrinsicImplementation
            // for how we cast the enum types to integral values in the comparer without boxing.

            TypeCode underlyingTypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(enumType));

            // Depending on the enum type, we need to special case the comparers so that we avoid boxing.
            // Note: We have different comparers for short and sbyte, since for those types GetHashCode does not simply return the value.
            // We need to preserve what they would return.
            switch (underlyingTypeCode)
            {
                case TypeCode.Int16:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(ShortEnumEqualityComparer<short>), enumType);
                case TypeCode.SByte:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(SByteEnumEqualityComparer<sbyte>), enumType);
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(EnumEqualityComparer<int>), enumType);
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(LongEnumEqualityComparer<long>), enumType);
            }
            
            return null;
        }
    }
}
