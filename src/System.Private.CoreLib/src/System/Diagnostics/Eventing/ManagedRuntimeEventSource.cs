// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

using System.Runtime.InteropServices.ComTypes;
using System.StubHelpers;


namespace System.Diagnostics.Tracing
{
#if FEATURE_PERFTRACING
    /// <summary>
    /// ManagedRuntimeEventSource is an EventSource that represents events emitted by the managed runtime.
    /// </summary>
    [EventSource(Name = "System.Runtime")]
    internal sealed class ManagedRuntimeEventSource : EventSource
    {
        // Process-level EventCounters
        private EventCounter _totalProcessTimeCounter;
        private EventCounter _workingSetCounter;
        private EventCounter _virtualMemorySizeCounter;
        private EventCounter _handleCountCounter;
        private EventCounter _threadCountCounter;

        // GC Counters
        private EventCounter _gcTotalMemoryCounter;
        private EventCounter _gcGen0CollectionCounter;
        private EventCounter _gcGen1CollectionCounter;
        private EventCounter _gcGen2CollectionCounter;

        // Exception
        private EventCounter _exceptionCounter;

        private Timer m_timer;

        private const int EnabledPollingIntervalMilliseconds = 1000; // 1 second

        // Defines the singleton instance for the Resources ETW provider
        internal static ManagedRuntimeEventSource Log = new ManagedRuntimeEventSource();

        // Threads
        // TODO
        public ManagedRuntimeEventSource(): base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {
            // TODO: These are all returning fake stuff now
            _totalProcessTimeCounter = new EventCounter("Total Process Time", this, () => 1);
            _workingSetCounter = new EventCounter("Working Set", this, () => 2);
            _virtualMemorySizeCounter = new EventCounter("Virtual Memory Size", this, () => 3);
            _handleCountCounter = new EventCounter("Handle Count", this, () => 4);
            _threadCountCounter = new EventCounter("Thread Count", this, () => 5);

            // GC counters
            _gcTotalMemoryCounter = new EventCounter("Total Memory by GC", this, () => GC.GetTotalMemory(false));
            _gcGen0CollectionCounter = new EventCounter("Gen 0 GC Count", this, () => GC.CollectionCount(0));
            _gcGen1CollectionCounter = new EventCounter("Gen 1 GC Count", this, () => GC.CollectionCount(1));
            _gcGen2CollectionCounter = new EventCounter("Gen 2 GC Count", this, () => GC.CollectionCount(2));

            // TODO: Expose a managed API for computing this
            _exceptionCounter = new EventCounter("Exception Count", this, () => 6);
            

            // Initialize the timer, but don't set it to run.
            // The timer will be set to run each time PollForTracingCommand is called.
            m_timer = new Timer(
                callback: new TimerCallback(PollForCounterUpdate),
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite,
                flowExecutionContext: false);

            // Trigger the first poll operation on the start-up path.
            PollForCounterUpdate(null);
        }

        public void UpdateAllCounters()
        {
            // Process level counters
            _totalProcessTimeCounter.UpdateMetric();
            _workingSetCounter.UpdateMetric();
            _virtualMemorySizeCounter.UpdateMetric();
            _handleCountCounter.UpdateMetric();
            _threadCountCounter.UpdateMetric();

            // GC counters
            _gcTotalMemoryCounter.UpdateMetric();
            _gcGen0CollectionCounter.UpdateMetric();
            _gcGen1CollectionCounter.UpdateMetric();
            _gcGen2CollectionCounter.UpdateMetric();

            // Exception
            _exceptionCounter.UpdateMetric();
        }

        private void PollForCounterUpdate(object state)
        {
            // Make sure that any transient errors don't cause the listener thread to exit.
            try
            {
                UpdateAllCounters();

                // Schedule the timer to run again.
                m_timer.Change(EnabledPollingIntervalMilliseconds, Timeout.Infinite);
            }
            catch { }
        }
    }
#endif // FEATURE_PERFTRACING
}
