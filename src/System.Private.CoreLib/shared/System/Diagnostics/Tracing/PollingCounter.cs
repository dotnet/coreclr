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
    /// <summary>
    /// PollingCounter is a variant of EventCounter - it collects and calculates similar statistics 
    /// as EventCounter. PollingCounter differs from EventCounter in that it takes in a callback
    /// function to collect metrics on its own rather than the user having to call WriteMetric() 
    /// every time.
    /// </summary>
    internal partial class PollingCounter : BaseCounter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PollingCounter"/> class.
        /// PollingCounter live as long as the EventSource that they are attached to unless they are
        /// explicitly Disposed.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="eventSource">The event source.</param>
        public PollingCounter(string name, EventSource eventSource, Func<float> getMetricFunction) : base(name, eventSource)
        {
            _min = float.PositiveInfinity;
            _max = float.NegativeInfinity;
            _getMetricFunction = getMetricFunction;

            InitializeBuffer();
        }

        /// <summary>
        /// Calls the callback function to get the value to enqueue. 
        /// </summary>
        public void UpdateMetric()
        {
            try
            {
                Enqueue(_getMetricFunction());    
            }
            catch (Exception)
            {
                // Swallow all exceptions that we may get from calling _getMetricFunction();
            }
        }

        public override string ToString() => $"PollingCounter '{name}' Count {_count} Mean {(((double)_sum) / _count).ToString("n3")}";

        #region Statistics Calculation

        // Statistics
        private int _count;
        private float _sum;
        private float _sumSquared;
        private float _min;
        private float _max;

        private Func<float> _getMetricFunction;

        internal void OnMetricWritten(float value)
        {
            Debug.Assert(Monitor.IsEntered(MyLock));
            _sum += value;
            _sumSquared += value * value;
            if (value > _max)
                _max = value;

            if (value < _min)
                _min = value;

            _count++;
        }

        internal override void WritePayload(EventSource _eventSource, float intervalSec)
        {
            CounterPayload payload = GetCounterPayload();
            payload.IntervalSec = intervalSec;
            _eventSource.Write("EventCounters", new EventSourceOptions() { Level = EventLevel.LogAlways }, new PollingPayloadType(payload));
        }

        internal CounterPayload GetCounterPayload()
        {
            lock (MyLock)     // Lock the counter
            {
                Flush();
                CounterPayload result = new CounterPayload();
                result.Name = name;
                result.Count = _count;
                if (0 < _count)
                {
                    result.Mean = _sum / _count;
                    result.StandardDeviation = (float)Math.Sqrt(_sumSquared / _count - _sum * _sum / _count / _count);
                }
                else
                {
                    result.Mean = 0;
                    result.StandardDeviation = 0;
                }
                result.Min = _min;
                result.Max = _max;
                ResetStatistics();
                return result;
            }
        }

        private void ResetStatistics()
        {
            Debug.Assert(Monitor.IsEntered(MyLock));
            _count = 0;
            _sum = 0;
            _sumSquared = 0;
            _min = float.PositiveInfinity;
            _max = float.NegativeInfinity;
        }

        #endregion // Statistics Calculation


        // Values buffering
        private const int BufferedSize = 10;
        private const float UnusedBufferSlotValue = float.NegativeInfinity;
        private const int UnsetIndex = -1;
        private volatile float[] _bufferedValues;
        private volatile int _bufferedValuesIndex;

        private void InitializeBuffer()
        {
            _bufferedValues = new float[BufferedSize];
            for (int i = 0; i < _bufferedValues.Length; i++)
            {
                _bufferedValues[i] = UnusedBufferSlotValue;
            }
        }

        protected void Enqueue(float value)
        {
            // It is possible that two threads read the same bufferedValuesIndex, but only one will be able to write the slot, so that is okay.
            int i = _bufferedValuesIndex;
            while (true)
            {
                float result = Interlocked.CompareExchange(ref _bufferedValues[i], value, UnusedBufferSlotValue);
                i++;
                if (_bufferedValues.Length <= i)
                {
                    // It is possible that two threads both think the buffer is full, but only one get to actually flush it, the other
                    // will eventually enter this code path and potentially calling Flushing on a buffer that is not full, and that's okay too.
                    lock (MyLock) // Lock the counter
                        Flush();
                    i = 0;
                }

                if (result == UnusedBufferSlotValue)
                {
                    // CompareExchange succeeded 
                    _bufferedValuesIndex = i;
                    return;
                }
            }
        }

        protected void Flush()
        {
            Debug.Assert(Monitor.IsEntered(MyLock));
            for (int i = 0; i < _bufferedValues.Length; i++)
            {
                var value = Interlocked.Exchange(ref _bufferedValues[i], UnusedBufferSlotValue);
                if (value != UnusedBufferSlotValue)
                {
                    OnMetricWritten(value);
                }
            }

            _bufferedValuesIndex = 0;
        }
    }

    
    /// <summary>
    /// This is the payload that is sent in the with EventSource.Write
    /// </summary>
    [EventData]
    class PollingPayloadType
    {
        public PollingPayloadType(CounterPayload payload) { Payload = payload; }
        public CounterPayload Payload { get; set; }
    }

}
