// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if ES_BUILD_PCL
    using System.Threading.Tasks;
#endif

#if ES_BUILD_STANDALONE
namespace Microsoft.Diagnostics.Tracing
#else
namespace System.Diagnostics.Tracing
#endif
{
    internal class CounterGroup
    {
        private readonly EventSource _eventSource;
        private readonly List<DiagnosticCounter> _counters;

        internal CounterGroup(EventSource eventSource)
        {
            _eventSource = eventSource;
            _counters = new List<DiagnosticCounter>();
            RegisterCommandCallback();
        }

        internal void Add(DiagnosticCounter eventCounter)
        {
            lock (this) // Lock the CounterGroup
                _counters.Add(eventCounter);
        }

        internal void Remove(DiagnosticCounter eventCounter)
        {
            lock (this) // Lock the CounterGroup
                _counters.Remove(eventCounter);
        }

        #region EventSource Command Processing

        private void RegisterCommandCallback()
        {
            _eventSource.EventCommandExecuted += OnEventSourceCommand;
        }

        private void OnEventSourceCommand(object? sender, EventCommandEventArgs e)
        {
            if (e.Command == EventCommand.Enable || e.Command == EventCommand.Update)
            {
                Debug.Assert(e.Arguments != null);

                if (s_pollingThreadEvent == null)
                {
                    s_pollingThreadEvent = new ManualResetEvent(false);
                }


                if (e.Arguments.TryGetValue("EventCounterIntervalSec", out string? valueStr) && float.TryParse(valueStr, out float value))
                {
                    lock (this)      // Lock the CounterGroup
                    {
                        EnableTimer(value);
                    }

                    lock (s_pollingThreadLock)
                    {
                        if (s_counterGroupCnt == 0 && s_pollingThreadEvent != null)
                        {
                            s_pollingThreadEvent.Set();
                        }
                        s_counterGroupCnt++;
                    }
                }
            }
            else if (e.Command == EventCommand.Disable)
            {
                lock (this)
                {
                    _pollingIntervalInMilliseconds = 0;   
                }

                lock (s_pollingThreadLock)
                {
                    s_counterGroupCnt--;

                    if (s_counterGroupCnt == 0 && s_pollingThreadEvent != null)
                    {
                        s_pollingThreadEvent.Reset();
                    }
                }
            }
        }

        #endregion // EventSource Command Processing

        #region Global CounterGroup Array management

        // We need eventCounters to 'attach' themselves to a particular EventSource.   
        // this table provides the mapping from EventSource -> CounterGroup 
        // which represents this 'attached' information.   
        private static WeakReference<CounterGroup>[]? s_counterGroups;
        private static readonly object s_counterGroupsLock = new object();

        private static void EnsureEventSourceIndexAvailable(int eventSourceIndex)
        {
            Debug.Assert(Monitor.IsEntered(s_counterGroupsLock));
            if (CounterGroup.s_counterGroups == null)
            {
                CounterGroup.s_counterGroups = new WeakReference<CounterGroup>[eventSourceIndex + 1];
            }
            else if (eventSourceIndex >= CounterGroup.s_counterGroups.Length)
            {
                WeakReference<CounterGroup>[] newCounterGroups = new WeakReference<CounterGroup>[eventSourceIndex + 1];
                Array.Copy(CounterGroup.s_counterGroups, 0, newCounterGroups, 0, CounterGroup.s_counterGroups.Length);
                CounterGroup.s_counterGroups = newCounterGroups;
            }
        }

        internal static CounterGroup GetCounterGroup(EventSource eventSource)
        {
            lock (s_counterGroupsLock)
            {
                int eventSourceIndex = EventListener.EventSourceIndex(eventSource);
                EnsureEventSourceIndexAvailable(eventSourceIndex);
                Debug.Assert(s_counterGroups != null);
                WeakReference<CounterGroup> weakRef = CounterGroup.s_counterGroups[eventSourceIndex];
                CounterGroup? ret = null;
                if (weakRef == null || !weakRef.TryGetTarget(out ret))
                {
                    ret = new CounterGroup(eventSource);
                    CounterGroup.s_counterGroups[eventSourceIndex] = new WeakReference<CounterGroup>(ret);
                }
                return ret;
            }
        }

        #endregion // Global CounterGroup Array management

        #region Timer Processing

        private DateTime _timeStampSinceCollectionStarted;
        private int _pollingIntervalInMilliseconds;
        internal DateTime nextpollingTimeStamp;
        
        private void EnableTimer(float pollingIntervalInSeconds)
        {
            Debug.Assert(Monitor.IsEntered(this));
            if (pollingIntervalInSeconds <= 0)
            {
                _pollingIntervalInMilliseconds = 0;
            }
            else if (_pollingIntervalInMilliseconds == 0 || pollingIntervalInSeconds * 1000 < _pollingIntervalInMilliseconds)
            {
                _pollingIntervalInMilliseconds = (int)(pollingIntervalInSeconds * 1000);
                ResetCounters(); // Reset statistics for counters before we start the thread.
                _timeStampSinceCollectionStarted = DateTime.UtcNow;
                // Don't capture the current ExecutionContext and its AsyncLocals onto the timer causing them to live forever
                bool restoreFlow = false;
                try
                {
                    if (!ExecutionContext.IsFlowSuppressed())
                    {
                        ExecutionContext.SuppressFlow();
                        restoreFlow = true;
                    }

                    lock (s_pollingThreadLock)
                    {
                        nextpollingTimeStamp = DateTime.UtcNow + new TimeSpan(0, 0, (int)pollingIntervalInSeconds);

                        // If we are already polling for some other EventSource, check the next polling time and update if necessary (i.e. if we need to poll before the next earliest polling time.)
                        // Otherwise, create a new polling thread and set the next sleep time.
                        
                        if (s_pollingThread == null)
                        {
                            s_pollingThread = new Thread(PollForValues) { IsBackground = true };
                            s_sleepDurationInMilliseconds = (int)pollingIntervalInSeconds * 1000;
                            s_pollingThread.Start();
                        }
                    }
                }
                finally
                {
                    // Restore the current ExecutionContext
                    if (restoreFlow)
                        ExecutionContext.RestoreFlow();
                }
            }
        }

        private void ResetCounters()
        {
            lock (this) // Lock the CounterGroup
            {
                foreach (var counter in _counters)
                {
                    if (counter is IncrementingEventCounter ieCounter)
                    {
                        ieCounter.UpdateMetric();
                    }
                    else if (counter is IncrementingPollingCounter ipCounter)
                    {
                        ipCounter.UpdateMetric();
                    }
                    else if (counter is EventCounter eCounter)
                    {
                        eCounter.ResetStatistics();
                    }
                }
            }
        }

        internal void OnTimer()
        {
            lock (this) // Lock the CounterGroup
            {
                if (_eventSource.IsEnabled())
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan elapsed = now - _timeStampSinceCollectionStarted;

                    foreach (var counter in _counters)
                    {
                        counter.WritePayload((float)elapsed.TotalSeconds, _pollingIntervalInMilliseconds);
                    }
                    _timeStampSinceCollectionStarted = now;
                    nextpollingTimeStamp = now + new TimeSpan(0, 0, 0, 0, _pollingIntervalInMilliseconds);
                }
            }
        }


        private static readonly object s_pollingThreadLock  = new object(); // This lock protects the three static obj below
        private static Thread? s_pollingThread;
        private static int s_sleepDurationInMilliseconds;
        private static ManualResetEvent? s_pollingThreadEvent;
        private static int s_counterGroupCnt = 0;

        private static void PollForValues()
        {
            while (true)
            {
                while (s_counterGroupCnt > 0)
                {
                    DateTime nextPoll = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, s_sleepDurationInMilliseconds);

                    if (s_counterGroups == null)
                    {
                        break;
                    }
                    lock (s_pollingThreadLock)
                    {
                        foreach (WeakReference<CounterGroup> counterGroupRef in s_counterGroups)
                        {
                            if (counterGroupRef == null)
                                continue;
                            if (!counterGroupRef.TryGetTarget(out CounterGroup? counterGroup))
                                continue;

                            if (counterGroup != null)
                            {
                                if (counterGroup.nextpollingTimeStamp < DateTime.UtcNow)
                                {
                                    counterGroup.OnTimer();
                                }

                                int millisecondsTillNextPoll = (int)(counterGroup.nextpollingTimeStamp - DateTime.UtcNow).TotalMilliseconds;
                                if (millisecondsTillNextPoll < s_sleepDurationInMilliseconds && millisecondsTillNextPoll > 0)
                                {
                                    s_sleepDurationInMilliseconds = millisecondsTillNextPoll;
                                }
                            }
                        }
                    }
                    Thread.Sleep(s_sleepDurationInMilliseconds);                    
                }
                if (s_pollingThreadEvent != null)
                {
                    s_pollingThreadEvent.WaitOne(); // Block until polling is enabled again
                }
            }
        }


        #endregion // Timer Processing

    }
}
