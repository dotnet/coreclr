// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if BIT64
using nuint = System.UInt64;
#else
    using nuint = System.UInt32;
#endif

namespace System.Runtime
{
    public class RuntimeImports
    {
        // Non-inlinable wrapper around the QCall that avoids poluting the fast path
        // with P/Invoke prolog/epilog.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal unsafe static void RhZeroMemory(ref byte b, nuint byteLength)
        {
            fixed (byte* bytePointer = &b)
            {
                RhZeroMemory(bytePointer, byteLength);
            }
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        extern private unsafe static void RhZeroMemory(byte* b, nuint byteLength);

        // Non-inlinable wrapper around the QCall that avoids poluting the fast path
        // with P/Invoke prolog/epilog.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal unsafe static bool RhCopyMemoryWithReferences<T>(ref T destination, ref T source, int elementCount)
        {
            fixed (void* destinationPtr = &Unsafe.As<T, byte>(ref destination))
            {
                fixed (void* sourcePtr = &Unsafe.As<T, byte>(ref source))
                {
                    return
                        RhCopyMemoryWithReferences(
                            destinationPtr,
                            sourcePtr,
                            (nuint)elementCount * (nuint)Unsafe.SizeOf<T>());
                }
            }
        }

        [DllImport(JitHelpers.QCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        extern private unsafe static bool RhCopyMemoryWithReferences(void* destination, void* source, nuint byteCount);
    }
}
