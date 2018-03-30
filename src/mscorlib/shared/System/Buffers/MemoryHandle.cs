// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    /// <summary>
    /// A handle for the memory.
    /// </summary>
    public unsafe struct MemoryHandle : IDisposable
    {
        private IPinnable _pinnable;
        private void* _pointer;
        private GCHandle _handle;

        /// <summary>
        /// Creates a new memory handle for the memory.
        /// </summary>
        /// <param name="pointer">pointer to memory</param>
        /// <param name="pinnable">reference to manually managed object, or default if there is no memory manager</param>
        /// <param name="handle">handle used to pin array buffers</param>
        [CLSCompliant(false)]
        public MemoryHandle(void* pointer, IPinnable pinnable = default, GCHandle handle = default)
        {
            _pinnable = pinnable;
            _pointer = pointer;
            _handle = handle;
        }

        /// <summary>
        /// Returns the pointer to memory, or null if a pointer was not provided when the handle was created.
        /// </summary>
        [CLSCompliant(false)]
        public void* Pointer => _pointer;

        /// <summary>
        /// Frees the pinned handle and releases IPinnable.
        /// </summary>
       public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }

            if (_pinnable != null)
            {
                _pinnable.Unpin();
                _pinnable = null;
            }

            _pointer = null;
        }

    }
}
