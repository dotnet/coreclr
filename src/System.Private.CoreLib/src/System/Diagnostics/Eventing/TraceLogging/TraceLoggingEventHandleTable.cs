// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace System.Diagnostics.Tracing
{
#if FEATURE_PERFTRACING
    /// <summary>
    /// Per-EventSource data structure for caching EventPipe EventHandles associated with TraceLogging events.
    /// </summary>

    internal struct TraceLoggingEventHandleTableEntry
    {
        public IntPtr eventHandle;
        public bool metadataGenerationFailed;

        public TraceLoggingEventHandleTableEntry(IntPtr handle, bool failed)
        {
            eventHandle = handle;
            metadataGenerationFailed = failed;
        }
    }

    internal sealed class TraceLoggingEventHandleTable
    {
        private const int DefaultLength = 10;
        private TraceLoggingEventHandleTableEntry[] m_innerTable;

        internal TraceLoggingEventHandleTable()
        {
            m_innerTable = new TraceLoggingEventHandleTableEntry[DefaultLength];
        }

        internal IntPtr this[int eventID]
        {
            get
            {
                IntPtr ret = IntPtr.Zero;
                TraceLoggingEventHandleTableEntry[] innerTable = Volatile.Read(ref m_innerTable);

                if (eventID >= 0 && eventID < innerTable.Length)
                {
                    ret = innerTable[eventID].eventHandle;
                }

                return ret;
            }
        }

        internal bool MetadataGenerationFailed(int eventID)
        {
            TraceLoggingEventHandleTableEntry[] innerTable = Volatile.Read(ref m_innerTable);

            if (eventID >= 0 && eventID < innerTable.Length)
            {
                return innerTable[eventID].metadataGenerationFailed;
            }

            return false;
        }

        internal void SetEventHandle(int eventID, IntPtr eventHandle, bool metadataGenerationFailed)
        {
            // Set operations must be serialized to ensure that re-size operations don't lose concurrent writes.
            Debug.Assert(Monitor.IsEntered(this));

            if (eventID >= m_innerTable.Length)
            {
                int newSize = m_innerTable.Length * 2;
                if (newSize <= eventID)
                {
                    newSize = eventID + 1;
                }

                TraceLoggingEventHandleTableEntry[] newTable = new TraceLoggingEventHandleTableEntry[newSize];
                Array.Copy(m_innerTable, 0, newTable, 0, m_innerTable.Length);
                Volatile.Write(ref m_innerTable, newTable);
            }

            m_innerTable[eventID] = new TraceLoggingEventHandleTableEntry(eventHandle, metadataGenerationFailed);
        }
    }
#endif
}
