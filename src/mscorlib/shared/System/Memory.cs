// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

namespace System
{
    public struct Memory<T>
    {
        // The highest order bit of _index is used to discern whether _arrayOrOwnedMemory is an array or an owned memory
        // if (_index >> 31) == 1, object _arrayOrOwnedMemory is an OwnedMemory<T>
        // else, object _arrayOrOwnedMemory is a T[]
        readonly object _arrayOrOwnedMemory;
        readonly int _index;
        readonly int _length;

        private const int bitMask = 0x7FFFFFFF;

        /// <summary>
        /// Creates a new memory over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="array"/> is a null
        /// reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[] array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (default(T) == null && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();

            _arrayOrOwnedMemory = array;
            _index = 0;
            _length = array.Length;
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and covering the remainder of the array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="array"/> is a null
        /// reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[] array, int start)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (default(T) == null && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();

            int arrayLength = array.Length;
            if ((uint)start > (uint)arrayLength)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            _arrayOrOwnedMemory = array;
            _index = start;
            _length = arrayLength - start;
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="array"/> is a null
        /// reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[] array, int start, int length)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (default(T) == null && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            _arrayOrOwnedMemory = array;
            _index = start;
            _length = length;
        }
        
        // Constructor for internal use only.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(OwnedMemory<T> owner, int index, int length)
        {
            _arrayOrOwnedMemory = owner;
            _index = index | (1 << 31); // Before using _index, check if _index < 0, then 'and' it with bitMask
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Memory{T}"/>
        /// </summary>
        public static implicit operator Memory<T>(T[] array) => new Memory<T>(array);
        
        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="Memory{T}"/>
        /// </summary>
        public static implicit operator Memory<T>(ArraySegment<T> arraySegment) => new Memory<T>(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Memory{T}"/> to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(Memory<T> memory)
        {
            // There is no need to 'and' _index by the bit mask here 
            // since the constructor will set the highest order bit again anyway
            if (memory._index < 0)
                return new ReadOnlyMemory<T>(Unsafe.As<OwnedMemory<T>>(memory._arrayOrOwnedMemory), memory._index, memory._length);
            return new ReadOnlyMemory<T>(Unsafe.As<T[]>(memory._arrayOrOwnedMemory), memory._index, memory._length);
        }

        /// <summary>
        /// Returns an empty <see cref="Memory{T}"/>
        /// </summary>
        public static Memory<T> Empty { get; } = OwnedMemory<T>.EmptyArray;

        /// <summary>
        /// The number of items in the memory.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // There is no need to 'and' _index by the bit mask here 
            // since the constructor will set the highest order bit again anyway
            if (_index < 0)
                return new Memory<T>(Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory), _index + start, _length - start);
            return new Memory<T>(Unsafe.As<T[]>(_arrayOrOwnedMemory), _index + start, _length - start);
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
        public Memory<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // There is no need to 'and' _index by the bit mask here 
            // since the constructor will set the highest order bit again anyway
            if (_index < 0)
                return new Memory<T>(Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory), _index + start, length);
            return new Memory<T>(Unsafe.As<T[]>(_arrayOrOwnedMemory), _index + start, length);
        }

        /// <summary>
        /// Returns a span from the memory.
        /// </summary>
        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_index < 0)
                    return Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory).AsSpan(_index & bitMask, _length);
                return new Span<T>(Unsafe.As<T[]>(_arrayOrOwnedMemory), _index, _length);
            }
        }

        public MemoryHandle Retain(bool pin = false)
        {
            MemoryHandle memoryHandle;
            if (pin)
            {
                if (_index < 0)
                {
                    memoryHandle = Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory).Pin();
                }
                else
                {
                    var handle = GCHandle.Alloc(Unsafe.As<T[]>(_arrayOrOwnedMemory), GCHandleType.Pinned);
                    unsafe
                    {
                        var pointer = Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _index);
                        memoryHandle = new MemoryHandle(null, pointer, handle);
                    }
                }
            }
            else
            {
                if (_index < 0)
                {
                    Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory).Retain();
                    memoryHandle = new MemoryHandle(Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory));
                }
                else
                {
                    memoryHandle = new MemoryHandle(null);
                }
            }
            return memoryHandle;
        }

        /// <summary>
        /// Get an array segment from the underlying memory. 
        /// If unable to get the array segment, return false with a default array segment.
        /// </summary>
        public bool TryGetArray(out ArraySegment<T> arraySegment)
        {
            if (_index < 0)
            {
                if (Unsafe.As<OwnedMemory<T>>(_arrayOrOwnedMemory).TryGetArray(out var segment))
                {
                    arraySegment = new ArraySegment<T>(segment.Array, segment.Offset + (_index & bitMask), _length);
                    return true;
                }
            }
            else
            {
                arraySegment = new ArraySegment<T>(Unsafe.As<T[]>(_arrayOrOwnedMemory), _index, _length);
                return true;
            }

            arraySegment = default;
            return false;
        }

        /// <summary>
        /// Copies the contents of this span from the memory into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray() => Span.ToArray();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T> readOnlyMemory)
            {
                return readOnlyMemory.Equals(this);
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
        public bool Equals(Memory<T> other)
        {
            return
                _arrayOrOwnedMemory == other._arrayOrOwnedMemory &&
                (_index & bitMask) == (other._index & bitMask) &&
                _length == other._length;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return HashingHelper.CombineHashCodes(_arrayOrOwnedMemory.GetHashCode(), (_index & bitMask).GetHashCode(), _length.GetHashCode());
        }

    }
}