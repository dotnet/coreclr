// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace src.System.Runtime.InteropServices
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MemoryMarshal
    {
        /// <summary>
        /// Creates a span over the range specified. However this does no bounds checking therefore is dangerous
        /// all validation should be done prior to calling this method
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the span.</param>
        /// <param name="length">The number of items in the span.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> DangerousCreateSpan<T>(T[] array, int start, int length) => new Span<T>(ref Unsafe.Add(ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()), start), length);

        /// <summary>
        /// Creates a memory instance over the range. However this does no bounds checking therefore is dangerous
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the span.</param>
        /// <param name="length">The number of items in the span.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> DangerousCreateMemory<T>(T[] array, int start, int length) => new Memory<T>(start, length, array);
    }
}
