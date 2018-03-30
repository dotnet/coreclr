// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Buffers
{
    /// <summary>
    /// Provides a mechanism for manual lifetime management.
    /// </summary>
    public interface IPinnable
    {
        /// <summary>
        /// Call this method to indicate that the IPinnable object is no longer in use.
        /// The object can now be disposed.
        /// </summary>
        MemoryHandle Pin(int elementIndex);

        /// <summary>
        /// Call this method to indicate that the IPinnable object is no longer in use.
        /// The object can now be disposed.
        /// </summary>
        void Unpin();
    }
}
