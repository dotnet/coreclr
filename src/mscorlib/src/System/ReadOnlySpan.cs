// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// ReadOnlySpan is a uniform API for dealing with arrays and subarrays, strings
    /// and substrings, and unmanaged memory buffers.  It adds minimal overhead
    /// to regular accesses and is a struct so that creation and subslicing do
    /// not require additional allocations.  It is type- and memory-safe.
    /// </summary>
    public struct ReadOnlySpan<T>
    {
        /// <summary>A byref or a native ptr. Do not access directly</summary>
        internal /* readonly */ IntPtr _rawPointer;
        /// <summary>The number of elements this ReadOnlySpan contains.</summary>
        internal readonly int _length;

        /// <summary>
        /// Creates a new span over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        public ReadOnlySpan(T[] array)
        {
            SpanContracts.RequiresNonNullArray(array);

            JitHelpers.SetByRef(out _rawPointer, ref JitHelpers.GetArrayData(array));
            _length = array.Length;
        }

        /// <summary>
        /// Creates a new span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the span.</param>
        /// <param name="length">The number of items in the span.</param>
        public ReadOnlySpan(T[] array, int start, int length)
        {
            SpanContracts.RequiresNonNullArray(array);
            SpanContracts.RequiresInRange(start, array.Length);
            SpanContracts.RequiresInInclusiveRange(length, array.Length - start);

            JitHelpers.SetByRef(out _rawPointer, ref JitHelpers.AddByRef(ref JitHelpers.GetArrayData(array), start));
            _length = length;
        }

        /// <summary>
        /// Creates a new span over the target unmanaged buffer.  Clearly this
        /// is quite dangerous, because we are creating arbitrarily typed T's
        /// out of a void*-typed block of memory.  And the length is not checked.
        /// But if this creation is correct, then all subsequent uses are correct.
        /// </summary>
        /// <param name="ptr">An unmanaged pointer to memory.</param>
        /// <param name="length">The number of T elements the memory contains.</param>
        [CLSCompliant(false)]
        public unsafe ReadOnlySpan(void* ptr, int length)
        {
            SpanContracts.RequresNoReferences<T>();
            SpanContracts.RequiresNonNegative(length);

            _rawPointer = (IntPtr)ptr;
            _length = length;
        }

        /// <summary>
        /// An internal helper for creating spans. Not for public use.
        /// </summary>
        private ReadOnlySpan(ref T ptr, int length)
        {
            JitHelpers.SetByRef(out _rawPointer, ref ptr);
            _length = length;
        }

        public static implicit operator ReadOnlySpan<T>(Span<T> slice)
        {
            return new ReadOnlySpan<T>(ref JitHelpers.GetByRef<T>(ref slice._rawPointer), slice.Length);
        }

        public int Length
        {
            get { return _length; }
        }

        public static ReadOnlySpan<T> Empty
        {
            get { return default(ReadOnlySpan<T>); }
        }

        public bool IsEmpty
        {
            get { return _length == 0; }
        }

        /// <summary>
        /// Fetches the element at the specified index.
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when the specified index is not in range (&lt;0 or &gt;&eq;_length).
        /// </exception>
        public T this[int index]
        {
            get
            {
                SpanContracts.RequiresIndexInRange(index, _length);

                return JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), index);
            }
        }

        /// <summary>
        /// Copies the contents of this span into a new array.  This heap
        /// allocates, so should generally be avoided, however is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] CreateArray()
        {
            var destination = new T[_length];
            TryCopyTo(destination);
            return destination;
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start index is not in range (&lt;0 or &gt;length).
        /// </exception>
        public ReadOnlySpan<T> Slice(int start)
        {
            SpanContracts.RequiresInInclusiveRange(start, _length);

            return new ReadOnlySpan<T>(ref JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), start), Length - start);
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="end">The index at which to end this slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;_length).
        /// </exception>
        public ReadOnlySpan<T> Slice(int start, int length)
        {
            SpanContracts.RequiresInRange(start, _length);
            SpanContracts.RequiresInInclusiveRange(length, _length - start);

            return new ReadOnlySpan<T>(ref JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), start), length);
        }

        /// <summary>
        /// Checks to see if two spans point at the same memory.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public bool Equals(ReadOnlySpan<T> other)
        {
            return (_length == other._length) &&
                (_length == 0 || JitHelpers.ByRefEquals(ref JitHelpers.GetByRef<T>(ref _rawPointer), ref JitHelpers.GetByRef<T>(ref other._rawPointer)));
        }

        /// <summary>
        /// Copies the contents of this span into destination span. The destination
        /// must be at least as big as the source, and may be bigger.
        /// </summary>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(Span<T> destination)
        {
            if (Length > destination.Length)
                return false;

            SpanHelper.CopyTo<T>(ref destination._rawPointer, ref _rawPointer, Length);
            return true;
        }

        /// <summary>
        /// Copies the contents of this span into destination array. The destination
        /// must be at least as big as the source, and may be bigger.
        /// </summary>
        /// <param name="destination">The array to copy items into.</param>
        public bool TryCopyTo(T[] destination)
        {
            return TryCopyTo(new Span<T>(destination));
        }

        /// <summary>
        /// Copies the contents of this span into destination memory. 
        /// Only Value Types that contain no pointers are supported.
        /// </summary>
        /// <param name="destination">An unmanaged pointer to memory.</param>
        /// <param name="elementsCount">The number of T elements the memory contains.</param>
        [CLSCompliant(false)]
        public unsafe bool TryCopyTo(void* destination, int elementsCount)
        {
            if (Length > (uint)elementsCount || JitHelpers.ContainsReferences<T>())
            {
                return false;
            }

            SpanHelper.Memmove<T>((byte*)destination, (byte*)_rawPointer, elementsCount);
            return true;
        }
    }
}