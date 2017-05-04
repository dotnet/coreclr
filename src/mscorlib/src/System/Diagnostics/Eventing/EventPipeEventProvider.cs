// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

#if ES_BUILD_STANDALONE
namespace Microsoft.Diagnostics.Tracing
#else
namespace System.Diagnostics.Tracing
#endif
{
    internal sealed class EventPipeEventProvider : IEventProvider
    {
        // Register an event provider.
        unsafe uint IEventProvider.EventRegister(
            ref Guid providerId,
            UnsafeNativeMethods.ManifestEtw.EtwEnableCallback enableCallback,
            void* callbackContext,
            ref long registrationHandle)
        {
            // TODO
            EventPipeInternal.CreateProvider(providerId, enableCallback);
            return 0;
        }

        // Unregister an event provider.
        uint IEventProvider.EventUnregister(long registrationHandle)
        {
            return 0;
        }

        // Write an event.
        unsafe int IEventProvider.EventWriteTransferWrapper(
            long registrationHandle,
            ref EventDescriptor eventDescriptor,
            Guid* activityId,
            Guid* relatedActivityId,
            int userDataCount,
            EventProvider.EventData* userData)
        {
            // TODO
            return 0;
        }

        // Get or set the per-thread activity ID.
        int IEventProvider.EventActivityIdControl(UnsafeNativeMethods.ManifestEtw.ActivityControl ControlCode, ref Guid ActivityId)
        {
            // TODO
            return 0;
        }
    }

    // PInvokes into the runtime used to interact with the EventPipe.
    internal static class EventPipeInternal
    {
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
