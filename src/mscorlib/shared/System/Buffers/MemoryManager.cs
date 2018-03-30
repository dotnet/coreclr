// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Runtime.CompilerServices;

namespace System.Buffers
{
    /// <summary>
    /// Manager of Memory<typeparamref name="T"/> that provides the implementation.
    /// </summary>
    public abstract class MemoryManager<T> : IMemoryOwner<T>, IPinnable
    {
        /// <summary>
        /// The number of items in the Memory<typeparamref name="T"/>.
        /// </summary>
        public virtual int Length => GetSpan().Length;

        /// <summary>
        /// Returns a Memory<typeparamref name="T"/> if the underlying memory has not been freed.
        /// </summary>
        public virtual Memory<T> Memory => new Memory<T>(this, 0, Length);

        /// <summary>
        /// Returns a span wrapping the underlying memory.
        /// </summary>
        public abstract Span<T> GetSpan();

        /// <summary>
        /// Returns a handle for the array that has been pinned and hence its address can be taken
        /// </summary>
        public abstract MemoryHandle Pin(int elementIndex = 0);

        public abstract void Unpin();

        /// <summary>
        /// Returns an array segment.
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
