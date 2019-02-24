// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

using Internal.Runtime.CompilerServices;

#if BIT64
using nuint = System.UInt64;
using nint = System.Int64;
#else
using nuint = System.UInt32;
using nint = System.Int32;
#endif

namespace System
{
    /// <summary>
    /// Represents a contiguous region of memory, similar to <see cref="ReadOnlySpan{T}"/>.
    /// Unlike <see cref="ReadOnlySpan{T}"/>, it is not a byref-like type.
    /// </summary>
    [DebuggerTypeProxy(typeof(MemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct ReadOnlyMemory<T>
    {
        // NOTE: With the current implementation, Memory<T> and ReadOnlyMemory<T> must have the same layout,
        // as code uses Unsafe.As to cast between them.

        // The highest order bit of _index is used to discern whether _object is a pre-pinned array.
        // (_index < 0) => _object is a pre-pinned array, so Pin() will not allocate a new GCHandle
        //       (else) => Pin() needs to allocate a new GCHandle to pin the object.
        private readonly object _object;
        private readonly int _index;
        private readonly int _length;

        internal const int RemoveFlagsBitMask = 0x7FFFFFFF;

        /// <summary>
        /// Creates a new memory over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory(T[] array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }

            _object = array;
            _index = 0;
            _length = array.Length;
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
#if BIT64
            // See comment in Span<T>.Slice for how this works.
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException();
#else
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();
#endif

            _object = array;
            _index = start;
            _length = length;
        }

        /// <summary>Creates a new memory over the existing object, start, and length. No validation is performed.</summary>
        /// <param name="obj">The target object.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyMemory(object obj, int start, int length)
        {
            // No validation performed in release builds; caller must provide any necessary validation.

            // 'obj is T[]' below also handles things like int[] <-> uint[] being convertible
            Debug.Assert((obj == null) || (typeof(T) == typeof(char) && obj is string) || (obj is T[]) || (obj is MemoryManager<T>));

            _object = obj;
            _index = start;
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory<T>(T[] array) => new ReadOnlyMemory<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory<T>(ArraySegment<T> segment) => new ReadOnlyMemory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns an empty <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static ReadOnlyMemory<T> Empty => default;

        /// <summary>
        /// The number of items in the memory.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// For <see cref="ReadOnlyMemory{Char}"/>, returns a new instance of string that represents the characters pointed to by the memory.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </summary>
        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                return (_object is string str) ? str.Substring(_index, _length) : Span.ToString();
            }
            return string.Format("System.ReadOnlyMemory<{0}>[{1}]", typeof(T).Name, _length);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            }

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyMemory<T>(_object, _index + start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start, int length)
        {
#if BIT64
            // See comment in Span<T>.Slice for how this works.
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
#else
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
#endif

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyMemory<T>(_object, _index + start, length);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'startIndex'
        /// </summary>
        /// <param name="startIndex">The index at which to begin this slice.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(Index startIndex)
        {
            int actualIndex = startIndex.GetOffset(_length);
            return Slice(actualIndex);
        }

        /// <summary>
        /// Forms a slice out of the given memory using the range start and end indexes.
        /// </summary>
        /// <param name="range">The range used to slice the memory using its start and end indexes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(Range range)
        {
            (int start, int length) = range.GetOffsetAndLength(_length);
            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyMemory<T>(_object, _index + start, length);
        }

        /// <summary>
        /// Forms a slice out of the given memory using the range start and end indexes.
        /// </summary>
        /// <param name="range">The range used to slice the memory using its start and end indexes.</param>
        public ReadOnlyMemory<T> this[Range range] => Slice(range);

        /// <summary>
        /// Returns a span from the memory.
        /// </summary>
        public unsafe ReadOnlySpan<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref T refToReturn = ref Unsafe.AsRef<T>(null);
                uint desiredLength = 0;

                // Copy this field into a local so that it can't change out from under us mid-operation.
                object tmpObject = _object;

                if (tmpObject != null)
                {
                    uint lengthOfUnderlyingSpan;

                    // Our underlying object can be a string (only when T = char), an SzArray U[] where
                    // T = U or T and U are convertible (e.g., int[] <-> uint[]), or a MemoryManager<T>.
                    // On 32-bit platforms, string and array have the exact same shape: length immediately
                    // followed by the string / array data. On 64-bit platforms, arrays have one extra
                    // 32-bit padding value between the length field and the start of the data. This means
                    // that we only need to special-case string when T is char _and_ we're running 64-bit.
                    // 
                    // We're going to optimistically treat the underlying object as a string or an array
                    // first, since it's almost always the case that this reflects reality anyway.

                    if (IntPtr.Size == 8 && typeof(T) == typeof(char))
                    {
                        // Optimistically assume it's a string for now. If this isn't the case, 'refToReturn'
                        // will point to the 32-bit padding field in the SzArray or MemoryManager<T>.

                        refToReturn = ref Unsafe.As<char, T>(ref Unsafe.As<string>(tmpObject).GetRawStringData());

                        // Read the length field of the string or SzArray (will be non-negative) or the
                        // _dummy field of the MemoryManager<T> (will be negative). We do it via the
                        // GetRawData() extension method rather than unsafe casting to string or SzArray
                        // and accessing its Length property because the JIT might try to out-clever us
                        // and assume Length can never return a non-negative value, which would break
                        // the checks later in this method.

                        lengthOfUnderlyingSpan = Unsafe.As<byte, uint>(ref tmpObject.GetRawData());

                        // If our initial guess that this was a string is correct, skip straight to the
                        // error checking at the end of the method.

                        if (tmpObject.GetType() == typeof(string))
                        {
                            goto CheckOffsetsAndReturn;
                        }

                        // Otherwise, assume it's an SzArray instead of a string, and skip over the
                        // 32-bit padding. If our guess is still wrong and the underlying object is
                        // actually a MemoryManager<T>, this will point to the first field of the derived
                        // MemoryManager<T> instance, or it will point just past the end of the object
                        // if no fields are present in derived types. In either case the GC tracks it properly.

                        refToReturn = ref Unsafe.AddByteOffset(ref refToReturn, 4);

#if DEBUG
                        // Make sure the layout of string / SzArray didn't change on us.

                        Debug.Assert(
                            Unsafe.AreSame(
                                ref Unsafe.As<T, byte>(ref refToReturn),
                                ref Unsafe.As<Array>(tmpObject).GetRawSzArrayData()),
                            "We miscalculated where the start of data of the SzArray is.");
#endif
                    }
                    else
                    {
                        // T != char so we don't need to check for string, _or_ we're in a 32-bit proc
                        // where string and SzArray have the same layout. In either case optimistically
                        // assume the underlying object is SzArray. See comments above for why code is
                        // written this way.

                        refToReturn = ref Unsafe.As<byte, T>(ref Unsafe.As<Array>(tmpObject).GetRawSzArrayData());
                        lengthOfUnderlyingSpan = Unsafe.As<byte, uint>(ref tmpObject.GetRawData());
                    }

                    // If the underlying object really was a MemoryManager<T>, the "length" we read earlier
                    // will actually read the _dummy field on the base class, which is set to a negative
                    // value. Since string and SzArray can't have a negative length, this serves as a very
                    // inexpensive check to see if a MemoryManager<T> is in use.

                    if ((int)lengthOfUnderlyingSpan < 0)
                    {
                        // We know the object is not null, and it's not variable-length, so it must be a MemoryManager<T>.
                        // Otherwise somebody used private reflection to set this field, and we're not too worried about
                        // type safety violations at that point. Note that it can't be a MemoryManager<U>, even if U and
                        // T are blittable (e.g., MemoryManager<int> to MemoryManager<uint>), since there exists no
                        // constructor or other public API which would allow such a conversion.

                        Debug.Assert(tmpObject is MemoryManager<T>);
                        refToReturn = ref Unsafe.As<MemoryManager<T>>(tmpObject).GetSpanAndDeconstruct(out int tempLengthOfUnderlyingSpan).Value;
                        lengthOfUnderlyingSpan = (uint)tempLengthOfUnderlyingSpan;
                    }

                CheckOffsetsAndReturn:

                    // If the Memory<T> or ReadOnlyMemory<T> instance is torn, this property getter has undefined behavior.
                    // We try to detect this condition and throw an exception, but it's possible that a torn struct might
                    // appear to us to be valid, and we'll return an undesired span. Such a span is always guaranteed at
                    // least to be in-bounds when compared with the original Memory<T> instance, so using the span won't
                    // AV the process.

                    nuint desiredStartIndex = (uint)(_index & RemoveFlagsBitMask);
                    desiredLength = (uint)_length;

#if BIT64
                    // This is a modified version of the code in Span<T>.Slice to check for out-of-bounds access.
                    // Since we know all inputs are at most UInt32.MaxValue, signed arithmetic over the Int64
                    // domain will work since we can't integer overflow or underflow. This allows us to save a
                    // register compared to the normal Span<T>.Slice logic, which requires a temporary register
                    // to hold the result of the addition.

                    if ((nint)desiredStartIndex > (nint)(nuint)lengthOfUnderlyingSpan - (nint)(nuint)desiredLength)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException();
                    }
#else
                    if ((uint)desiredStartIndex > (uint)lengthOfUnderlyingSpan || (uint)desiredLength > (uint)(lengthOfUnderlyingSpan - desiredStartIndex))
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException();
                    }
#endif

                    refToReturn = ref Unsafe.Add(ref refToReturn, (IntPtr)(void*)desiredStartIndex);
                }

                return new ReadOnlySpan<T>(ref refToReturn, (int)desiredLength);
            }
        }

        /// <summary>
        /// Copies the contents of the read-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        ///
        /// <param name="destination">The Memory to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination is shorter than the source.
        /// </exception>
        /// </summary>
        public void CopyTo(Memory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of the readonly-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        ///
        /// <returns>If the destination is shorter than the source, this method
        /// return false and no data is written to the destination.</returns>
        /// </summary>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(Memory<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// <exception cref="System.ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        /// </summary>
        public unsafe MemoryHandle Pin()
        {
            // It's possible that the below logic could result in an AV if the struct
            // is torn. This is ok since the caller is expecting to use raw pointers,
            // and we're not required to keep this as safe as the other Span-based APIs.

            object tmpObject = _object;
            if (tmpObject != null)
            {
                if (typeof(T) == typeof(char) && tmpObject is string s)
                {
                    GCHandle handle = GCHandle.Alloc(tmpObject, GCHandleType.Pinned);
                    ref char stringData = ref Unsafe.Add(ref s.GetRawStringData(), _index);
                    return new MemoryHandle(Unsafe.AsPointer(ref stringData), handle);
                }
                else if (RuntimeHelpers.ObjectHasComponentSize(tmpObject))
                {
                    // 'tmpObject is T[]' below also handles things like int[] <-> uint[] being convertible
                    Debug.Assert(tmpObject is T[]);

                    // Array is already pre-pinned
                    if (_index < 0)
                    {
                        void* pointer = Unsafe.Add<T>(Unsafe.AsPointer(ref Unsafe.As<T[]>(tmpObject).GetRawSzArrayData()), _index & RemoveFlagsBitMask);
                        return new MemoryHandle(pointer);
                    }
                    else
                    {
                        GCHandle handle = GCHandle.Alloc(tmpObject, GCHandleType.Pinned);
                        void* pointer = Unsafe.Add<T>(Unsafe.AsPointer(ref Unsafe.As<T[]>(tmpObject).GetRawSzArrayData()), _index);
                        return new MemoryHandle(pointer, handle);
                    }
                }
                else
                {
                    Debug.Assert(tmpObject is MemoryManager<T>);
                    return Unsafe.As<MemoryManager<T>>(tmpObject).Pin(_index);
                }
            }

            return default;
        }

        /// <summary>
        /// Copies the contents from the memory into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray() => Span.ToArray();

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T> readOnlyMemory)
            {
                return Equals(readOnlyMemory);
            }
            else if (obj is Memory<T> memory)
            {
                return Equals(memory);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the memory points to the same array and has the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public bool Equals(ReadOnlyMemory<T> other)
        {
            return
                _object == other._object &&
                _index == other._index &&
                _length == other._length;
        }

        /// <summary>Returns the hash code for this <see cref="ReadOnlyMemory{T}"/></summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            // We use RuntimeHelpers.GetHashCode instead of Object.GetHashCode because the hash
            // code is based on object identity and referential equality, not deep equality (as common with string).
            return (_object != null) ? HashCode.Combine(RuntimeHelpers.GetHashCode(_object), _index, _length) : 0;
        }

        /// <summary>Gets the state of the memory as individual fields.</summary>
        /// <param name="start">The offset.</param>
        /// <param name="length">The count.</param>
        /// <returns>The object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object GetObjectStartLength(out int start, out int length)
        {
            start = _index;
            length = _length;
            return _object;
        }
    }
}
