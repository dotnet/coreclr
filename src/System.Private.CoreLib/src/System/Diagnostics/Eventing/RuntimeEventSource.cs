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
    [EventSource(Name = "System.Runtime")]
    internal sealed class RuntimeEventSource : EventSource
    {
        private EventCounter[] _counters;

        private enum Counter {
            TotalProcessTime,
            WorkingSet,
            HandleCount,
            ThreadCount,
            GCHeapSize,
            Gen0GCCount,
            Gen1GCCount,
            Gen2GCCount,
            ExceptionCount
        }

        private Timer _timer;

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
                _counters = new EventCounter[] {
                    // process info counters
                    new EventCounter("Total Process Time", this),
                    new EventCounter("Working Set", this),
                    new EventCounter("Handle Count", this),
                    new EventCounter("Thread Count", this),

                    // GC info counters
                    new EventCounter("Total Memory by GC", this),
                    new EventCounter("Gen 0 GC Count", this),
                    new EventCounter("Gen 1 GC Count", this),
                    new EventCounter("Gen 2 GC Count", this),

                    // TODO: Expose a managed API for computing this
                    new EventCounter("Exception Count", this)
                };

                // Initialize the timer, but don't set it to run.
                // The timer will be set to run each time PollForTracingCommand is called.
                _timer = new Timer(
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
                for (int i = 0; i < _counters.Length; i++)
                {
                    _counters[i] = null;
                }

                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        public void UpdateAllCounters()
        {
            // TODO: These are all returning fake stuff for now
            _counters[(int)Counter.TotalProcessTime].WriteMetric(1);
            _counters[(int)Counter.WorkingSet].WriteMetric(2);
            _counters[(int)Counter.HandleCount].WriteMetric(3);
            _counters[(int)Counter.ThreadCount].WriteMetric(4);

            // GC counters
            _counters[(int)Counter.GCHeapSize].WriteMetric(GC.GetTotalMemory(false));
            _counters[(int)Counter.Gen0GCCount].WriteMetric(GC.CollectionCount(0));
            _counters[(int)Counter.Gen1GCCount].WriteMetric(GC.CollectionCount(1));
            _counters[(int)Counter.Gen2GCCount].WriteMetric(GC.CollectionCount(2));

            // Exception
            _counters[(int)Counter.ExceptionCount].WriteMetric(5);
        }

        private void PollForCounterUpdate(object state)
        {
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
