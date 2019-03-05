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
    /// Provides the ability to collect statistics through EventSource
    /// 
    /// See https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.Tracing/documentation/EventCounterTutorial.md
    /// for a tutorial guide.  
    /// 
    /// See https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.Tracing/tests/BasicEventSourceTest/TestEventCounter.cs
    /// which shows tests, which are also useful in seeing actual use.  
    /// </summary>
    public partial class IncrementingPollingCounter : BaseCounter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementingPollingCounter"/> class.
        /// EVentCounters live as long as the EventSource that they are attached to unless they are
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

        private float _increment;
        private Func<float> _getMetricFunction;

        // TODO: BLow this shit up
        internal override void OnMetricWritten(float value)
        {
            lock(MyLock)
            {
                _increment += _getMetricFunction();
            }
        }

        /// <summary>
        /// Calls "_getMetricFunction" to enqueue the counter value to the queue. 
        /// </summary>
        public void UpdateMetric()
        {
            lock(MyLock)
            {
                _increment += _getMetricFunction();
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
                Flush();
                IncrementingCounterPayload result = new IncrementingCounterPayload();
                result.Name = name;
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
