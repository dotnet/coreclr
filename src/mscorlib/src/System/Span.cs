// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Span is a uniform API for dealing with arrays and subarrays, strings
    /// and substrings, and unmanaged memory buffers.  It adds minimal overhead
    /// to regular accesses and is a struct so that creation and subslicing do
    /// not require additional allocations.  It is type- and memory-safe.
    /// </summary>
    public struct Span<T>
    {
        /// <summary>A byref or a native ptr. Do not access directly</summary>
        internal /* readonly */ IntPtr _rawPointer;
        /// <summary>The number of elements this Span contains.</summary>
        internal readonly int _length;

        /// <summary>
        /// Creates a new span over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        public Span(T[] array)
        {
            SpanContracts.RequiresNonNullArray(array);
            SpanContracts.RequiresNoCovariance(array);

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
        public Span(T[] array, int start, int length)
        {
            SpanContracts.RequiresNonNullArray(array);
            SpanContracts.RequiresNoCovariance(array);
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
        public unsafe Span(void* ptr, int length)
        {
            SpanContracts.RequresNoReferences<T>();
            SpanContracts.RequiresNonNegative(length);

            _rawPointer = (IntPtr)ptr;
            _length = length;
        }

        /// <summary>
        /// An internal helper for creating spans. Not for public use.
        /// </summary>
        private Span(ref T ptr, int length)
        {
            JitHelpers.SetByRef(out _rawPointer, ref ptr);
            _length = length;
        }

        public int Length
        {
            get { return _length; }
        }

        public static Span<T> Empty
        {
            get { return default(Span<T>); }
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
            set
            {
                SpanContracts.RequiresIndexInRange(index, _length);

                JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), index) = value;
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
        public Span<T> Slice(int start)
        {
            SpanContracts.RequiresInInclusiveRange(start, _length);

            return new Span<T>(ref JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), start), Length - start);
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="end">The index at which to end this slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;_length).
        /// </exception>
        public Span<T> Slice(int start, int length)
        {
            SpanContracts.RequiresInRange(start, length);
            SpanContracts.RequiresInInclusiveRange(length, _length - start);

            return new Span<T>(ref JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), start), length);
        }

        /// <summary>
        /// Checks to see if two spans point at the same memory.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public bool Equals(Span<T> other)
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

        public void Set(ReadOnlySpan<T> values)
        {
            SpanContracts.RequiresInInclusiveRange(values._length, _length);

            SpanHelper.CopyTo<T>(ref _rawPointer, ref values._rawPointer, values.Length);
        }

        public void Set(T[] values)
        {
            Set(new ReadOnlySpan<T>(values));
        }

        [CLSCompliant(false)]
        public unsafe void Set(void* source, int elementsCount)
        {
            SpanContracts.RequresNoReferences<T>();
            SpanContracts.RequiresNonNegative(elementsCount);
            SpanContracts.RequiresInInclusiveRange(_length, elementsCount);

            SpanHelper.Memmove<T>((byte*)_rawPointer, (byte*)source, elementsCount);
        }
    }

    internal static class SpanContracts
    {
        internal static void RequiresIndexInRange(int index, int length) {
            if ((uint)index >= (uint)length)
                ThrowHelper.ThrowIndexOutOfRangeException();
        }

        internal static void RequiresInRange(int start, int length) {
            if ((uint)start >= (uint)length)
                ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        public static void RequiresInInclusiveRange(int start, int length) {
            if ((uint)start > (uint)length)
                ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        public static void RequiresNonNegative(int number) {
            if (number < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        internal static void RequiresNonNullArray<T>(T[] array) {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        internal static void RequiresNoCovariance<T>(T[] array) {
            if (default(T) == null) { // Arrays of valuetypes are never covariant
                if (array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
            }
        }

        /// <summary>
        /// throws for reference types and value types with references, they must not be put to unmanaged memory
        /// </summary>
        internal static void RequresNoReferences<T>() {
            if (JitHelpers.ContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeForUnmanagedMemory(typeof(T));
        }
    }

    internal static class SpanHelper
    {
        internal static void CopyTo<T>(ref IntPtr destination, ref IntPtr source, int elementsCount)
        {
            if (!JitHelpers.ContainsReferences<T>())
            {
                unsafe
                {
                    Memmove<T>((byte*)destination, (byte*)source, elementsCount);
                }
            }
            else
            {
                ref T dest = ref JitHelpers.GetByRef<T>(ref destination);
                ref T src = ref JitHelpers.GetByRef<T>(ref source);

                for (int i = 0; i < elementsCount; i++)
                {
                    JitHelpers.AddByRef(ref dest, i) = JitHelpers.AddByRef(ref src, i);
                }
            }
        }

        internal static unsafe void Memmove<T>(byte* destination, byte* source, int elementsCount)
        {
#if BIT64
            Buffer.Memmove(destination, source, (ulong)(elementsCount * (uint)JitHelpers.SizeOf<T>()));
#else
            Buffer.Memmove(destination, source, checked(elementsCount * (uint)JitHelpers.SizeOf<T>()));
#endif
        }
    }
}