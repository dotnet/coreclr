// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    // This is an experimental type and not referenced from CoreFx but needs to exists and be public so we can prototype in CoreFxLab.
    public sealed class Utf8String
    {
        // Do not reorder these fields. Must match layout of Utf8StringObject in object.h.
        private int _length;
        [CLSCompliant(false)]
        public byte _firstByte; // TODO: Is public for experimentation in CoreFxLab. Will be private in its ultimate form.

        private Utf8String() { } // Suppress creation of the public constructor. No one actually calls this.

        public int Length => _length;

        // Creates a new zero-initialized instance of the specified length. Actual storage allocated is "length + 1" bytes (the extra
        // +1 is for the NUL terminator.)
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Utf8String FastAllocate(int length);  //TODO: Is public for experimentation in CoreFxLab. Will be private in its ultimate form.
    }
}
