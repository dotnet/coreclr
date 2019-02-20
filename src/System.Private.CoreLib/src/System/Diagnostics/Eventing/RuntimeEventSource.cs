// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;


namespace System.Diagnostics.Tracing
{
    /// <summary>
    /// RuntimeEventSource is an EventSource that represents events emitted by the managed runtime.
    /// </summary>
    [EventSource(Guid="49592C0F-5A05-516D-AA4B-A64E02026C89", Name = "System.Runtime")]
    internal sealed class RuntimeEventSource : EventSource
    {
        private static RuntimeEventSource s_RuntimeEventSource;
        private EventCounter[] _counters;

        private enum Counter {
            GCHeapSize,
            Gen0GCCount,
            Gen1GCCount,
            Gen2GCCount
        }

        private Timer _timer;

        private const int EnabledPollingIntervalMilliseconds = 1000; // 1 second

        public static void Initialize()
        {
            s_RuntimeEventSource = new RuntimeEventSource();
        }
        
        private RuntimeEventSource(): base(new Guid(0x49592C0F, 0x5A05, 0x516D, 0xAA, 0x4B, 0xA6, 0x4E, 0x02, 0x02, 0x6C, 0x89), "System.Runtime", EventSourceSettings.EtwSelfDescribingEventFormat)
        {

        }

        protected override void OnEventCommand(System.Diagnostics.Tracing.EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _counters = new EventCounter[] {
                    // TODO: process info counters

                    // GC info counters
                    new EventCounter("Total Memory by GC", this),
                    new EventCounter("Gen 0 GC Count", this),
                    new EventCounter("Gen 1 GC Count", this),
                    new EventCounter("Gen 2 GC Count", this),

                    // TODO: Exception counter
                };

                // Initialize the timer, but don't set it to run.
                // The timer will be set to run each time PollForTracingCommand is called.

                // TODO: We might not need this timer once we are done settling upon a high-level design for
                // what EventCounter is capable of doing. Once that decision is made, we might be able to 
                // get rid of this.
                _timer = new Timer(
                    callback: new TimerCallback(PollForCounterUpdate),
                    state: null,
                    dueTime: Timeout.Infinite,
                    period: Timeout.Infinite,
                    flowExecutionContext: false);

                // Trigger the first poll operation on when this EventSource is enabled
                PollForCounterUpdate(null);
            }
            else if (command.Command == EventCommand.Disable)
            {

                // TODO: There are some multi-threading issues where this can be modified from a OnEventCommand callback
                // as well as a timer callback, so we need some kind of synchronization. We need to decide on what kind of 
                // thread safety EventCounter provides to determine what kind of synchronization is needed here.

                // Dispose counters when perfcounters are disabled
                _counters = null;

                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        public void UpdateAllCounters()
        {
            // GC counters
            _counters[(int)Counter.GCHeapSize].WriteMetric(GC.GetTotalMemory(false));
            _counters[(int)Counter.Gen0GCCount].WriteMetric(GC.CollectionCount(0));
            _counters[(int)Counter.Gen1GCCount].WriteMetric(GC.CollectionCount(1));
            _counters[(int)Counter.Gen2GCCount].WriteMetric(GC.CollectionCount(2));
        }

        private void PollForCounterUpdate(object state)
        {
            // TODO: Need to confirm with vancem about how to do error-handling here. 
            // This disables to timer from getting rescheduled to run, which may or may not be 
            // what we eventually want. 

            // Make sure that any transient errors don't cause the listener thread to exit.
            try
            {
                UpdateAllCounters();

                // Schedule the timer to run again.
                _timer.Change(EnabledPollingIntervalMilliseconds, Timeout.Infinite);
            }
            catch { }
        }
    }
}
