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
    /// RuntimeEventSource is an EventSource that represents events emitted by the managed runtime.
    /// </summary>
    [EventSource(Name = "System.Runtime")]
    internal sealed class RuntimeEventSource : EventSource
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

        // Threads
        // TODO

        
        public RuntimeEventSource(): base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {

        }

        protected override void OnEventCommand(System.Diagnostics.Tracing.EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                // TODO: These are all returning fake stuff now
                _totalProcessTimeCounter = new EventCounter("Total Process Time", this);
                _workingSetCounter = new EventCounter("Working Set", this);
                _virtualMemorySizeCounter = new EventCounter("Virtual Memory Size", this);
                _handleCountCounter = new EventCounter("Handle Count", this);
                _threadCountCounter = new EventCounter("Thread Count", this);

                // GC counters
                _gcTotalMemoryCounter = new EventCounter("Total Memory by GC", this);
                _gcGen0CollectionCounter = new EventCounter("Gen 0 GC Count", this);
                _gcGen1CollectionCounter = new EventCounter("Gen 1 GC Count", this);
                _gcGen2CollectionCounter = new EventCounter("Gen 2 GC Count", this);

                // TODO: Expose a managed API for computing this
                _exceptionCounter = new EventCounter("Exception Count", this);
                

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
            else if (command.Command == EventCommand.Disable)
            {
                // Dispose counters when perfcounters are disabled

                _totalProcessTimeCounter.Dispose();
                _workingSetCounter.Dispose();
                _virtualMemorySizeCounter.Dispose();
                _handleCountCounter.Dispose();
                _threadCountCounter.Dispose();

                _gcTotalMemoryCounter.Dispose();
                _gcGen0CollectionCounter.Dispose();
                _gcGen1CollectionCounter.Dispose();
                _gcGen2CollectionCounter.Dispose();

                _exceptionCounter.Dispose();

                if (m_timer != null)
                {
                    m_timer.Dispose();
                    m_timer = null;
                }
            }
        }

        public void UpdateAllCounters()
        {
            // Process level counters
            _totalProcessTimeCounter.WriteMetric(1);
            _workingSetCounter.WriteMetric(2);
            _virtualMemorySizeCounter.WriteMetric(3);
            _handleCountCounter.WriteMetric(4);
            _threadCountCounter.WriteMetric(5);

            // GC counters
            _gcTotalMemoryCounter.WriteMetric(GC.GetTotalMemory(false));
            _gcGen0CollectionCounter.WriteMetric(GC.CollectionCount(0));
            _gcGen1CollectionCounter.WriteMetric(GC.CollectionCount(1));
            _gcGen2CollectionCounter.WriteMetric(GC.CollectionCount(2));

            // Exception
            _exceptionCounter.WriteMetric(6);
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
