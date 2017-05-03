// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
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
            return 0;
        }

        // Unregister an event provider.
        uint IEventProvider.EventUnregister(long registrationHandle)
        {
            // TODO
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
}
