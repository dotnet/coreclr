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
            if (default(T) == null) // Arrays of valuetypes are never covariant
            {
                if (array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
            }

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
            if ((uint)start >= (uint)array.Length || (uint)length > (uint)(array.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();

            if (default(T) == null) // Arrays of valuetypes are nnever covariant
            {
                if (array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
            }

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
            System.Diagnostics.Contracts.Contract.Requires(length >= 0);
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

        internal uint SizeInBytes
        {
            get { return (uint)Length * (uint)JitHelpers.SizeOf<T>(); }
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
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified index is not in range (&lt;0 or &gt;&eq;_length).
        /// </exception>
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException();
                return JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), index);
            }
            set
            {
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException();
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

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', and
        /// ending at 'end' (exclusive).
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="end">The index at which to end this slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;_length).
        /// </exception>
        public Span<T> Slice(int start, int length)
        {
            if ((uint)start >= (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();

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

        public bool TryCopyTo(Span<T> destination)
        {
            if (Length > destination.Length)
                return false;

            if (JitHelpers.ContainsReferences<T>())
            {
                for (int i = 0; i < Length; i++)
                    JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref destination._rawPointer), i) = JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), i);

                return true;
            }

            unsafe
            {
                Memmove((byte*)destination._rawPointer, (byte*)_rawPointer);
                return true;
            }
        }

        public bool TryCopyTo(T[] destination)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (Length > destination.Length)
                return false;

            if (JitHelpers.ContainsReferences<T>())
            {
                for (int i = 0; i < Length; i++)
                    destination[i] = JitHelpers.AddByRef(ref JitHelpers.GetByRef<T>(ref _rawPointer), i);

                return true;
            }

            unsafe
            {
                IntPtr destinationPointer;
                JitHelpers.SetByRef(out destinationPointer, ref JitHelpers.GetArrayData(destination));
                Memmove((byte*)destinationPointer, (byte*)_rawPointer);
                return true;
            }
        }

        /// <param name="destination">An unmanaged pointer to memory.</param>
        /// <param name="elementsCount">The number of T elements the memory contains.</param>
        [System.Security.SecurityCritical]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public unsafe bool TryCopyTo(void* destination, int elementsCount)
        {
            if (Length > (uint)elementsCount || JitHelpers.ContainsReferences<T>())
            {
                return false;
            }

#if BIT64
            Buffer.Memmove((byte*)destination, (byte*)_rawPointer, (ulong)(elementsCount * (uint)JitHelpers.SizeOf<T>()));
#else
            Buffer.Memmove((byte*)destination, (byte*)_rawPointer, checked(elementsCount * (uint)JitHelpers.SizeOf<T>()));
#endif
            return true;
        }

        private unsafe void Memmove(byte* destination, byte* source)
        {
#if BIT64
            Buffer.Memmove(destination, source, (ulong)SizeInBytes);
#else
            Buffer.Memmove(destination, source, checked(SizeInBytes));
#endif
        }
    }
}
