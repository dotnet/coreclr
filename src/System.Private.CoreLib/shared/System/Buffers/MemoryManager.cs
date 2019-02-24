// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    /// <summary>
    /// Manager of <see cref="System.Memory{T}"/> that provides the implementation.
    /// </summary>
    public abstract class MemoryManager<T> : IMemoryOwner<T>, IPinnable
    {
#pragma warning disable CS0414
        // It's imperative that this be the first field in the MemoryManager<T> instance
        // and that it have a negative value. Because it's the first field in the object,
        // it occupies the same position as String.Length and SzArray.Length; but because
        // this value is negative and those values are not, we can easily determine whether
        // any given Memory<T> is backed by a variable-length object (string, array) or
        // a fixed-length object (MemoryManager).
        private readonly int _dummy = -1;
#if BIT64
        private readonly int _dummyPadding = 0;
#endif
#pragma warning restore CS0414

        /// <summary>
        /// Returns a <see cref="System.Memory{T}"/>.
        /// </summary>
        public virtual Memory<T> Memory => new Memory<T>(this, GetSpan().Length);

        /// <summary>
        /// Returns a span wrapping the underlying memory.
        /// </summary>
        public abstract Span<T> GetSpan();

        /// <summary>
        /// A special helper method which serves as a stub for the call to GetSpan(). It's
        /// non-virtual and forbids inlining to keep the call site as compact as possible.
        /// Additionally, since the span is deconstructed rather than returned by value,
        /// it means the caller only has to allocate space for a single Int32 on the stack.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ByReference<T> GetSpanAndDeconstruct(out int spanLength)
        {
            // Returns ByReference<T> because the C# compiler sees the 'out' parameter and
            // reasons that returning 'ref T' might lead to a reference escaping a local
            // scope. The ByReference<T> return type prevents this false positive error.

            Span<T> span = GetSpan();
            spanLength = span.Length;
            return new ByReference<T>(ref MemoryMarshal.GetReference(span));
        }

        /// <summary>
        /// Returns a handle to the memory that has been pinned and hence its address can be taken.
        /// </summary>
        /// <param name="elementIndex">The offset to the element within the memory at which the returned <see cref="MemoryHandle"/> points to. (default = 0)</param>
        public abstract MemoryHandle Pin(int elementIndex = 0);

        /// <summary>
        /// Lets the garbage collector know that the object is free to be moved now.
        /// </summary>
        public abstract void Unpin();

        /// <summary>
        /// Returns a <see cref="System.Memory{T}"/> for the current <see cref="MemoryManager{T}"/>.
        /// </summary>
        /// <param name="length">The element count in the memory, starting at offset 0.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Memory<T> CreateMemory(int length) => new Memory<T>(this, length);

        /// <summary>
        /// Returns a <see cref="System.Memory{T}"/> for the current <see cref="MemoryManager{T}"/>.
        /// </summary>
        /// <param name="start">The offset to the element which the returned memory starts at.</param>
        /// <param name="length">The element count in the memory, starting at element offset <paramref name="start"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Memory<T> CreateMemory(int start, int length) => new Memory<T>(this, start, length);

        /// <summary>
        /// Returns an array segment.
        /// <remarks>Returns the default array segment if not overriden.</remarks>
        /// </summary>
        protected internal virtual bool TryGetArray(out ArraySegment<T> segment)
        {
            segment = default;
            return false;
        }

        /// <summary>
        /// Implements IDisposable.
        /// </summary>
        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up of any leftover managed and unmanaged resources.
        /// </summary>
        protected abstract void Dispose(bool disposing);

    }
}
