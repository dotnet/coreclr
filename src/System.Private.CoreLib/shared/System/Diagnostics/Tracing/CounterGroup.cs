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
            _pollingEnabledEvent = new ManualResetEvent(false);
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

                if (e.Arguments.TryGetValue("EventCounterIntervalSec", out string? valueStr) && float.TryParse(valueStr, out float value))
                {
                    lock (this)      // Lock the CounterGroup
                    {
                        _isPollingEnabled = true;
                        EnablePollingThread(value);
                    }
                }
            }
            else if (e.Command == EventCommand.Disable)
            {
                lock (this)
                {
                    _isPollingEnabled = false;
                    _pollingIntervalInMilliseconds = 0;
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
        private Thread? _pollingThread;
        private ManualResetEvent _pollingEnabledEvent;
        private bool _isPollingEnabled;

        private void EnablePollingThread(float pollingIntervalInSeconds)
        {
            Debug.Assert(Monitor.IsEntered(this));
            if (pollingIntervalInSeconds <= 0)
            {
                _pollingIntervalInMilliseconds = 0;
                _isPollingEnabled = false;
            }
            else if (_pollingIntervalInMilliseconds == 0 || pollingIntervalInSeconds * 1000 < _pollingIntervalInMilliseconds)
            {
                Debug.WriteLine("Polling interval changed at " + DateTime.UtcNow.ToString("mm.ss.ffffff"));
                _pollingIntervalInMilliseconds = (int)(pollingIntervalInSeconds * 1000);
                DisposeTimer();
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

                    if (_pollingThread == null)
                    {
                        _pollingThread = new Thread(() => PollForValues());
                        _pollingThread.Start();    
                    }
                    else
                    {
                        _pollingEnabledEvent.Set();
                    }
                }
                finally
                {
                    // Restore the current ExecutionContext
                    if (restoreFlow)
                        ExecutionContext.RestoreFlow();
                }
            }
            // Always fire the timer event (so you see everything up to this time).  
            OnTimer();
        }

        private void PollForValues()
        {
            while(true)
            {
                while(_isPollingEnabled)
                {
                    if (!OnTimer())
                    {
                        break;
                    }
                    Thread.Sleep(_pollingIntervalInMilliseconds);
                }

                _pollingEnabledEvent.WaitOne(); // Block until polling is enabled again
                _pollingEnabledEvent.Reset(); // Reset this.
            }
        }

        private bool OnTimer()
        {
            Debug.WriteLine("Polling thread fired at " + DateTime.UtcNow.ToString("mm.ss.ffffff"));
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
                }
                else
                {
                    return false; // Stop polling if counter's EventSource is disabled.
                }
            }
            return true;
        }

        #endregion // Timer Processing

    }
}
