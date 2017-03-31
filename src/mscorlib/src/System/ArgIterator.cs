// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace System
{
    // This class will not be marked serializable
    [StructLayout(LayoutKind.Sequential)]
    public struct ArgIterator
    {
#if CORECLR
        // Note: This type must have the same layout as the CLR's VARARGS type in CLRVarArgs.h.
        // It also contains an inline SigPointer data structure - must keep those fields in sync.

        private IntPtr ArgCookie;               // Cookie from the EE.

        // The SigPointer structure consists of the following members.  (Note: this is an inline native SigPointer data type)
        private IntPtr sigPtr;                  // Pointer to remaining signature.
        private IntPtr sigPtrLen;               // Remaining length of the pointer

        // Note, sigPtrLen is actually a DWORD, but on 64bit systems this structure becomes
        // 8-byte aligned, which requires us to pad it.

        private IntPtr ArgPtr;                  // Pointer to remaining args.
        private int RemainingArgs;           // # of remaining args.
#endif // CORECLR

        public ArgIterator(RuntimeArgumentHandle arglist)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        [CLSCompliant(false)]
        public unsafe ArgIterator(RuntimeArgumentHandle arglist, void* ptr)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        public void End()
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        public override bool Equals(Object o)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        public override int GetHashCode()
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        [CLSCompliant(false)]
        public TypedReference GetNextArg()
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        [CLSCompliant(false)]
        public TypedReference GetNextArg(RuntimeTypeHandle rth)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        public unsafe RuntimeTypeHandle GetNextArgType()
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }

        public int GetRemainingCount()
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_ArgIterator); // https://github.com/dotnet/coreclr/issues/9204
        }
    }
}
