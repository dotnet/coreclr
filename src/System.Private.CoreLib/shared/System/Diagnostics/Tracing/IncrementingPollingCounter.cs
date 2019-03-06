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
    /// IncrementingPollingCounter is a variant of EventCounter for variables that are ever-increasing. 
    /// Ex) # of exceptions in the runtime.
    /// It does not calculate statistics like mean, standard deviation, etc. because it only accumulates
    /// the counter value.
    /// Unlike IncrementingEventCounter, this takes in a polling callback that it can call to update 
    /// its own metric periodically.
    /// </summary>
    internal partial class IncrementingPollingCounter : BaseCounter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementingPollingCounter"/> class.
        /// IncrementingPollingCounter live as long as the EventSource that they are attached to unless they are
        /// explicitly Disposed.   
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="eventSource">The event source.</param>
        public IncrementingPollingCounter(string name, EventSource eventSource, Func<float> getCountFunction) : base(name, eventSource)
        {
            _increment = 0;
        }

        public override string ToString()
        {
            return "IncrementingPollingCounter '" + name + "' Increment " + _increment;
        }

        private volatile float _increment;
        private Func<float> _getMetricFunction;

        /// <summary>
        /// Calls "_getMetricFunction" to enqueue the counter value to the queue. 
        /// </summary>
        public void UpdateMetric()
        {
            try
            {
                lock(MyLock)
                {
                    _increment += _getMetricFunction();
                }
            }
            catch (Exception)
            {
                // Swallow all exceptions that we may get from calling _getMetricFunction();
            }
        }

        internal override void WritePayload(EventSource _eventSource, float intervalSec)
        {
            IncrementingCounterPayload payload = GetCounterPayload();
            payload.IntervalSec = intervalSec;
            _eventSource.Write("EventCounters", new EventSourceOptions() { Level = EventLevel.LogAlways }, new IncrementingPollingCounterPayloadType(payload));
        }

        internal IncrementingCounterPayload GetCounterPayload()
        {
            lock (MyLock)     // Lock the counter
            {
                IncrementingCounterPayload result = new IncrementingCounterPayload();
                result.Name = name;
                result.DisplayName = DisplayName;
                result.DisplayRateTimeScale = DisplayRateTimeScale;
                result.MetaDataString = GetMetaDataString();
                result.Increment = _increment;
                return result;
            }
        }
    }


    /// <summary>
    /// This is the payload that is sent in the with EventSource.Write
    /// </summary>
    [EventData]
    class IncrementingPollingCounterPayloadType
    {
        public IncrementingPollingCounterPayloadType(IncrementingCounterPayload payload) { Payload = payload; }
        public IncrementingCounterPayload Payload { get; set; }
    }

}
