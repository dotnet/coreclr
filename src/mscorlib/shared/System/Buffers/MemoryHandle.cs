// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    public unsafe struct MemoryHandle : IDisposable
    {
        IRetainable _owner;
        void* _pointer;
        GCHandle _handle;

        bool _disposed = false;

        public MemoryHandle(IRetainable owner, void* pinnedPointer = null, GCHandle handle = default)
        {
            _pointer = pinnedPointer;
            _handle = handle;
            _owner = owner;
        }

        public void* PinnedPointer 
        {
            get 
            {
                if (_pointer == null) ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Memory_PointerIsNull);
                return _pointer;
            }
        }

        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return; 

            if (disposing) 
            {
                if (_handle.IsAllocated) 
                {
                    _handle.Free();
                }

                if (_owner != null) 
                {
                    _owner.Release();
                    _owner = null;
                }
            }

            _pointer = null;

            _disposed = true;
        }
        
    }
}