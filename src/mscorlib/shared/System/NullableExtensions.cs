// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;

namespace System
{
    public static class NullableExtensions
    {
        /// <summary>
        /// Deconstruct a <see cref="Nullable{T}"/> into a <see cref="Tuple{Boolean, T}"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Deconstruct<T>(this T? nullable, out bool hasValue, out T value)
            where T : struct
        {
            hasValue = nullable.HasValue;
            value = nullable ?? default;
        }
    }
}
