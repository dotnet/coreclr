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
    /// 
    /// </summary>
    public abstract class BaseCounter : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCounter"/> class.
        /// BaseCounter live as long as the EventSource that they are attached to unless they are
        /// explicitly Disposed.   
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="eventSource">The event source.</param>
        public BaseCounter(string name, EventSource eventSource)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            InitializeBuffer();
            _group = CounterGroup.GetCounterGroup(eventSource);
            _group.Add(this);
            this.name = name;
        }

        /// <summary>
        /// Removes the counter from set that the EventSource will report on.  After being disposed, this
        /// counter will do nothing and its resource will be reclaimed if all references to it are removed.
        /// If an EventCounter is not explicitly disposed it will be cleaned up automatically when the
        /// EventSource it is attached to dies.  
        /// </summary>
        public void Dispose()
        {
            var group = _group;
            if (group != null)
            {
                group.Remove(this);
                _group = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void AddMetaData(string key, string value)
        {
            lock (MyLock)
            {
                _metaData.Add(key, value);
            }
        }

        internal string DisplayName { get; }
        internal TimeSpan DisplayRateTimeScale { get; }

        #region private implementation

        protected readonly string name;
        private CounterGroup _group;

        #region Buffer Management

        // Values buffering
        private const int BufferedSize = 10;
        private const float UnusedBufferSlotValue = float.NegativeInfinity;
        private const int UnsetIndex = -1;
        private volatile float[] _bufferedValues;
        private volatile int _bufferedValuesIndex;
        private volatile Dictionary<string, string> _metaData;

        // Abstract methods that behave differently across different Counter APIs
        internal abstract void OnMetricWritten(float value);
        internal abstract void WritePayload(EventSource _eventSource, float intervalSec);

        // arbitrarily we use _bufferedValues as the lock object.  
        protected object MyLock { get { return _bufferedValues; } }

        protected string GetMetaDataString()
        {
            String metaDataString = "";
            foreach(KeyValuePair<string, string> kvPair in _metaData)
            {
                metaDataString += kvPair.Key + ":" + kvPair.Value + ",";
            }
            return metaDataString.Substring(0, metaDataString.Length - 1); // Get rid of the last ","
        }

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

        #endregion // Buffer Management

        #endregion // private implementation
    }
}
