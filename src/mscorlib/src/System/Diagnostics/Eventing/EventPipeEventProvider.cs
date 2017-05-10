// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;

namespace System.Diagnostics.Tracing
{
    internal sealed class EventPipeEventProvider : IEventProvider
    {
        // The EventPipeProvider handle.
        private IntPtr m_provHandle = IntPtr.Zero;

        // The dictionary of EventPipeEvent handles.
        // Key: EventID.
        // Value: EventPipeEvent handle.
        Dictionary<uint, IntPtr> m_events = new Dictionary<uint, IntPtr>();
        
        // Register an event provider.
        unsafe uint IEventProvider.EventRegister(
            ref Guid providerId,
            UnsafeNativeMethods.ManifestEtw.EtwEnableCallback enableCallback,
            void* callbackContext,
            ref long registrationHandle)
        {
            uint returnStatus = 0;
            m_provHandle = EventPipeInternal.CreateProvider(providerId, enableCallback);
            if(m_provHandle != IntPtr.Zero)
            {
                // Fixed registration handle because a new EventPipeEventProvider
                // will be created for each new EventSource.
                registrationHandle = 1;
            }
            else
            {
                // Unable to create the provider.
                returnStatus = 1;
            }

            return returnStatus;
        }

        // Unregister an event provider.
        uint IEventProvider.EventUnregister(long registrationHandle)
        {
            EventPipeInternal.DeleteProvider(m_provHandle);
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
            uint eventID = (uint)eventDescriptor.EventId;
            if(m_events.Count != 0)
            {
                Debug.Assert(m_events.ContainsKey(eventID));
                EventPipeInternal.WriteEvent(m_events[eventID], userData, (uint)userDataCount);
            }
            return 0;
        }

        // Get or set the per-thread activity ID.
        int IEventProvider.EventActivityIdControl(UnsafeNativeMethods.ManifestEtw.ActivityControl ControlCode, ref Guid ActivityId)
        {
            return 0;
        }

        // Register an EventPipeEvent handle to this EventPipeEventProvider.
        unsafe void IEventProvider.AddEventHandle(Int64 keywords, uint eventID, uint eventVersion, uint level, bool needStack)
        {
            IntPtr eventHandle = EventPipeInternal.AddEvent(m_provHandle, keywords, eventID, eventVersion, level, needStack);
            m_events.Add(eventID, eventHandle);
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
        internal static extern unsafe void WriteEvent(IntPtr eventHandle, void* data, uint dataCount);
    }
}
