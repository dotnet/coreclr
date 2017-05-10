// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

namespace System.Diagnostics.Tracing
{
    internal static class EventPipe
    {
        internal static void Enable()
        {
            EventPipeInternal.Enable();
        }

        internal static void Disable()
        {
            EventPipeInternal.Disable();
        }
    }

    internal static class EventPipeInternal
    {
        //
        // These PInvokes are used by the configuration APIs to interact with EventPipe.
        //
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void Enable();

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void Disable();

        //
        // These PInvokes are used by EventSource to interact with the EventPipe.
        //
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr CreateProvider(Guid providerID, UnsafeNativeMethods.ManifestEtw.EtwEnableCallback callbackFunc);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr AddEvent(IntPtr provHandle, Int64 keywords, uint eventID, uint eventVersion, uint level, bool needStack);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern void DeleteProvider(IntPtr provHandle);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern unsafe void WriteEvent(IntPtr eventHandle, void* data, uint length);
    }
}
