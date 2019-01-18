// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using static System.RuntimeTypeHandle;

namespace System.Collections.Generic
{
    /// <summary>
    /// Helper class for creating the default <see cref="Comparer{T}"/> and <see cref="EqualityComparer{T}"/>.
    /// </summary>
    /// <remarks>
    /// This class is intentionally type-unsafe and non-generic to minimize the generic instantiation overhead of creating
    /// the default comparer/equality comparer for a new type parameter. Efficiency of the methods in here does not matter too
    /// much since they will only be run once per type parameter, but generic code involved in creating the comparers needs to be
    /// kept to a minimum.
    /// </remarks>
    internal static class ComparerHelpers
    {
        /// <summary>
        /// Creates the default <see cref="Comparer{T}"/>.
        /// </summary>
        /// <param name="type">The type to create the default comparer for.</param>
        /// <remarks>
        /// The logic in this method is replicated in vm/compile.cpp to ensure that NGen saves the right instantiations,
        /// and in vm/jitinterface.cpp so the jit can model the behavior of this method.
        /// </remarks>
        internal static object CreateDefaultComparer(Type type)
        {
            Debug.Assert(type != null && type is RuntimeType);

            object result = null;
            var runtimeType = (RuntimeType)type;

            // If T implements IComparable<T> return a GenericComparer<T>
            if (typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type))
            {
                result = CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(GenericComparer<int>), runtimeType);
            }
            // Nullable does not implement IComparable<T?> directly because that would add an extra interface call per comparison.
            // Instead, it relies on Comparer<T?>.Default to specialize for nullables and do the lifted comparisons if T implements IComparable.
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    result = TryCreateNullableComparer(runtimeType);
                }
            }
            // The comparer for enums is specialized to avoid boxing.
            else if (type.IsEnum)
            {
                result = CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(EnumComparer<DayOfWeek>), runtimeType);
            }
            
            return result ?? CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(ObjectComparer<object>), runtimeType);
        }

        /// <summary>
        /// Creates the default <see cref="Comparer{T}"/> for a nullable type.
        /// </summary>
        /// <param name="nullableType">The nullable type to create the default comparer for.</param>
        private static object TryCreateNullableComparer(RuntimeType nullableType)
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

        /// <summary>
        /// Creates the default <see cref="EqualityComparer{T}"/>.
        /// </summary>
        /// <param name="type">The type to create the default equality comparer for.</param>
        /// <remarks>
        /// The logic in this method is replicated in vm/compile.cpp to ensure that NGen saves the right instantiations.
        /// </remarks>
        internal static object CreateDefaultEqualityComparer(Type type)
        {
            Debug.Assert(type != null && type is RuntimeType);

            object result = null;
            var runtimeType = (RuntimeType)type;

            // Specialize for byte so Array.IndexOf is faster.
            if (type == typeof(byte))
            {
                result = new ByteEqualityComparer();
            }
            // If T implements IEquatable<T> return a GenericEqualityComparer<T>
            else if (typeof(IEquatable<>).MakeGenericType(type).IsAssignableFrom(type))
            {
                result = CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(GenericEqualityComparer<int>), runtimeType);
            }
            // Nullable does not implement IEquatable<T?> directly because that would add an extra interface call per comparison.
            // Instead, it relies on EqualityComparer<T?>.Default to specialize for nullables and do the lifted comparisons if T implements IEquatable.
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    result = TryCreateNullableEqualityComparer(runtimeType);
                }
            }
            // The equality comparer for enums is specialized to avoid boxing.
            else if (type.IsEnum)
            {
                result = CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(EnumEqualityComparer<DayOfWeek>), runtimeType);
            }
            
            return result ?? CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(ObjectEqualityComparer<object>), runtimeType);
        }

        /// <summary>
        /// Creates the default <see cref="EqualityComparer{T}"/> for a nullable type.
        /// </summary>
        /// <param name="nullableType">The nullable type to create the default equality comparer for.</param>
        private static object TryCreateNullableEqualityComparer(RuntimeType nullableType)
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
    }
}
