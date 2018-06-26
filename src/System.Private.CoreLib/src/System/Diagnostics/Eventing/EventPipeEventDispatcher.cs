// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics.Tracing
{
#if FEATURE_PERFTRACING
    internal sealed class EventPipeEventDispatcher
    {
        internal static readonly EventPipeEventDispatcher Instance = new EventPipeEventDispatcher();

        private IntPtr m_RuntimeProviderID;

        private bool m_stopDispatchTask;
        private Task m_dispatchTask = null;
        private object m_dispatchTaskLock = new object();

        // For now, only one EventListener at a time gets to control what events are enabled.
        private EventListener m_controllingEventListener = null;

        private EventPipeEventDispatcher()
        {
            // Get the ID of the runtime provider so that it can be used as a filter when processing events.
            m_RuntimeProviderID = EventPipeInternal.GetProvider(RuntimeEventSource.EventSourceName);
        }

        internal void SendCommand(EventListener eventListener, EventCommand command, bool enable, EventLevel level, EventKeywords matchAnyKeywords)
        {
            if (command == EventCommand.Update && enable)
            {
                EventPipeProviderConfiguration[] providerConfiguration = new EventPipeProviderConfiguration[]
                {
                    new EventPipeProviderConfiguration(RuntimeEventSource.EventSourceName, (ulong) matchAnyKeywords, (uint) level)
                };

                lock (m_dispatchTaskLock)
                {
                    if (m_dispatchTask == null)
                    {
                        m_controllingEventListener = eventListener;
                        EventPipeInternal.Enable(null, 1024, 1, providerConfiguration, 1);
                        m_dispatchTask = Task.Run(DispatchEventsToEventListeners);
                    }
                }
            }
            else if (command == EventCommand.Update && !enable)
            {
                RemoveEventListener(eventListener);
            }
        }

        internal void RemoveEventListener(EventListener listener)
        {
            lock (m_dispatchTaskLock)
            {
                if (m_controllingEventListener == listener && m_dispatchTask != null)
                {
                    m_stopDispatchTask = true;
                    m_dispatchTask.Wait();
                    m_dispatchTask = null;

                    EventPipeInternal.Disable();
                }
            }
        }

        private unsafe void DispatchEventsToEventListeners()
        {
            // Struct to fill with the call to GetNextEvent.
            EventPipeEventInstanceData instanceData;

            while (!m_stopDispatchTask)
            {
                // Get the next event.
                while (!m_stopDispatchTask && EventPipeInternal.GetNextEvent(&instanceData))
                {
                    // Filter based on provider.
                    if (instanceData.ProviderID == m_RuntimeProviderID)
                    {
                        // Dispatch the event.
                        ReadOnlySpan<Byte> payload = new ReadOnlySpan<byte>((void*)instanceData.Payload, (int)instanceData.PayloadLength);
                        RuntimeEventSource.Log.ProcessEvent(instanceData.EventID, payload);
                    }
                }

                // Wait for more events.
                if (!m_stopDispatchTask)
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
#endif // FEATURE_PERFTRACING
}
