// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    // This is an experimental type and not referenced from CoreFx but needs to exists and be public so we can prototype in CoreFxLab.
    public sealed class Utf8String
    {
        // Do not reorder these fields. Must match layout of Utf8StringObject in object.h.
        private readonly int _length;
        private readonly byte _firstByte;

        public int Length => _length;
        public ref readonly byte GetPinnableReference() => ref _firstByte;

        public static readonly Utf8String Empty = FastAllocate(0);

        // Utf8String constructors
        // These are special. The implementation methods for these have a different signature from the
        // declared constructors.

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<byte> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<byte> value)
        {
            if (value.Length == 0)
                return Empty;

            Utf8String newString = FastAllocate(value.Length);
            unsafe
            {
                fixed (byte* pDst = &newString._firstByte)
                fixed (byte* pSrc = &MemoryMarshal.GetNonNullPinnableReference(value))
                {
                    Buffer.Memcpy(dest: pDst, src: pSrc, len: value.Length);
                }
            }
            return newString;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<char> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
                return Empty;

            Encoding e = Encoding.UTF8;
            int length = e.GetByteCount(value);
            Utf8String newString = FastAllocate(length);
            unsafe
            {
                fixed (byte* pFirstByte = &newString._firstByte)
                fixed (char* pFirstChar = &MemoryMarshal.GetNonNullPinnableReference(value))
                {
                    e.GetBytes(pFirstChar, length, pFirstByte, length);
                }
            }
            return newString;
        }

        // Creates a new zero-initialized instance of the specified length. Actual storage allocated is "length + 1" bytes (the extra
        // +1 is for the NUL terminator.)
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Utf8String FastAllocate(int length);  //TODO: Is public for experimentation in CoreFxLab. Will be private in its ultimate form.
    }
}
