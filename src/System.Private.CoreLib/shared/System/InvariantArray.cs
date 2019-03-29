// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// A wrapper around an "invariant" <typeparamref name="T"/> array; that is, without
    /// any covariance or contravariance.
    /// </summary>
    internal readonly struct InvariantArray<T>
    {
        private readonly T[] _array;

        /// <summary>
        /// Creates an invariant wrapper around an existing array.
        /// <paramref name="array"/> must be null or exactly T[] (no covariance or contravariance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InvariantArray(T[] array)
        {
            if (RuntimeHelpers.IsReference<T>() && (array != null && array.GetType() != typeof(T[])))
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            _array = array;
        }

        /// <summary>
        /// Allocates a new array of the specified length and creates an invariant
        /// wrapper around it.
        /// </summary>
        /// <param name="count"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InvariantArray(int count)
        {
            _array = new T[count];
        }

        /// <summary>
        /// Provides access to the backing array.
        /// </summary>
        public T[] Array
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array;
        }

        /// <summary>
        /// Returns true if the backing array is null.
        /// </summary>
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array is null;
        }

        /// <summary>
        /// Returns the length of the backing array.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// If <see cref="IsNull"/> is true.
        /// </exception>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [StackTraceHidden]
            get => _array.Length;
        }

        /// <summary>
        /// Returns the backing array as a mutable <see cref="Span{T}"/>; or
        /// <see cref="Span{T}.Empty"/> if <see cref="IsNull"/> is true.
        /// </summary>
        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsNull)
                {
                    return default;
                }
                else
                {
                    return new Span<T>(ref Unsafe.As<byte, T>(ref _array.GetRawSzArrayData()), _array.Length);
                }
            }
        }

        /// <summary>
        /// Returns the backing array as a mutable <see cref="Span{T}"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// If <see cref="IsNull"/> is true.
        /// </exception>
        public Span<T> SpanNotNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [StackTraceHidden]
            get
            {
                T[] array = _array;

                // Note: Accessing Length first avoids an unnecessary cmp instruction.

                return new Span<T>(
                    length: array.Length,
                    ptr: ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()));
            }
        }

        /// <summary>
        /// Reads a value from or writes a value to the backing array at the specified index.
        /// The access uses normal array bounds checking.
        /// </summary>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array[index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (RuntimeHelpers.IsReference<T>())
                {
                    // Normally when a write takes place to a SzArray of reference type T,
                    // the JIT emits a call to JIT_Stelem_Ref, which performs five actions:
                    //
                    // 1) check that the array isn't null,
                    // 2) check that the index isn't out of bounds,
                    // 3) check that the value to insert is the correct type,
                    // 4) write the value to memory, then
                    // 5) update the GC card table.
                    //
                    // One workaround is to get a `ref T` and to write directly to that,
                    // but this causes the JIT to emit a call to JIT_CheckedWriteBarrier,
                    // which performs three actions:
                    //
                    // 1) check that the address to write to is within the GC heap,
                    // 2) write the value to memory, then
                    // 3) update the GC card table.
                    //
                    // Since we're backed by a managed array (which must always be in the
                    // GC heap), we can get the JIT to emit a call to the faster
                    // JIT_WriteBarrier helper method, which only performs two actions:
                    //
                    // 1) write the value to memory, then
                    // 2) update the GC card table.
                    //
                    // The way we get the JIT to emit a call to this faster helper is to
                    // tell it that we're writing to a special array whose elements are
                    // value types that contain a single reference field. The runtime
                    // can reason that the array itself must exist in the GC heap and
                    // will elide that check entirely.

                    Unsafe.As<InvariantArrayObjectWrapper[]>(_array)[index] = new InvariantArrayObjectWrapper(value);
                }
                else
                {
                    _array[index] = value;
                }
            }
        }
    }

    internal readonly struct InvariantArrayObjectWrapper
    {
        private readonly object _value;

        public InvariantArrayObjectWrapper(object value)
        {
            _value = value;
        }
    }
}
