// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// ResourcesEtwProvider.cs
//
//
// Managed event source for things that can version with MSCORLIB.  
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.Tracing {

    // To use the framework provider
    // 
    //     \\clrmain\tools\Perfmonitor /nokernel /noclr /provider:8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1 start
    //     Run run your app
    //     \\clrmain\tools\Perfmonitor stop
    //     \\clrmain\tools\Perfmonitor print
    //
    // This will produce an XML file, where each event is pretty-printed with all its arguments nicely parsed.
    //
    [FriendAccessAllowed]
    [EventSource(Guid = "8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1", Name = "System.Diagnostics.Eventing.FrameworkEventSource")]
    sealed internal class FrameworkEventSource : EventSource {
        // Defines the singleton instance for the Resources ETW provider
        public static readonly FrameworkEventSource Log = new FrameworkEventSource();

        // Keyword definitions.  These represent logical groups of events that can be turned on and off independently
        // Often each task has a keyword, but where tasks are determined by subsystem, keywords are determined by
        // usefulness to end users to filter.  Generally users don't mind extra events if they are not high volume
        // so grouping low volume events together in a single keywords is OK (users can post-filter by task if desired)
        public static class Keywords {
            public const EventKeywords Loader     = (EventKeywords)0x0001; // This is bit 0
            public const EventKeywords ThreadPool = (EventKeywords)0x0002; 
            public const EventKeywords NetClient  = (EventKeywords)0x0004;
            //
            // This is a private event we do not want to expose to customers.  It is to be used for profiling
            // uses of dynamic type loading by ProjectN applications running on the desktop CLR
            //
            public const EventKeywords DynamicTypeUsage = (EventKeywords)0x0008;
            public const EventKeywords ThreadTransfer   = (EventKeywords)0x0010;
        }

        /// <summary>ETW tasks that have start/stop events.</summary>
        [FriendAccessAllowed]
        public static class Tasks // this name is important for EventSource
        {
            /// <summary>Begin / End - GetResponse.</summary>
            public const EventTask GetResponse      = (EventTask)1;
            /// <summary>Begin / End - GetRequestStream</summary>
            public const EventTask GetRequestStream = (EventTask)2;
            /// <summary>Send / Receive - begin transfer/end transfer</summary>
            public const EventTask ThreadTransfer = (EventTask)3;
        }

        [FriendAccessAllowed]
        public static class Opcodes
        {
            public const EventOpcode ReceiveHandled = (EventOpcode)11;
        }

        // This predicate is used by consumers of this class to deteremine if the class has actually been initialized,
        // and therefore if the public statics are available for use. This is typically not a problem... if the static
        // class constructor fails, then attempts to access the statics (or even this property) will result in a 
        // TypeInitializationException. However, that is not the case while the class loader is actually trying to construct
        // the TypeInitializationException instance to represent that failure, and some consumers of this class are on
        // that code path, specifically the resource manager. 
        public static bool IsInitialized
        {
            get
            {
                return Log != null;
            }
        }

        // The FrameworkEventSource GUID is {8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1}
        private FrameworkEventSource() : base(new Guid(0x8e9f5090, 0x2d75, 0x4d03, 0x8a, 0x81, 0xe5, 0xaf, 0xbf, 0x85, 0xda, 0xf1), "System.Diagnostics.Eventing.FrameworkEventSource") { }

        // WriteEvent overloads (to avoid the "params" EventSource.WriteEvent

        // optimized for common signatures (used by the ThreadTransferSend/Receive events)
        [NonEvent, System.Security.SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        private unsafe void WriteEvent(int eventId, long arg1, int arg2, string arg3, bool arg4)
        {
            if (IsEnabled())
            {
                if (arg3 == null) arg3 = "";
                fixed (char* string3Bytes = arg3)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[4];
                    descrs[0].DataPointer = (IntPtr)(&arg1);
                    descrs[0].Size = 8;
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    descrs[2].DataPointer = (IntPtr)string3Bytes;
                    descrs[2].Size = ((arg3.Length + 1) * 2);
                    descrs[3].DataPointer = (IntPtr)(&arg4);
                    descrs[3].Size = 4;
                    WriteEventCore(eventId, 4, descrs);
                }
            }
        }

        // optimized for common signatures (used by the ThreadTransferSend/Receive events)
        [NonEvent, System.Security.SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        private unsafe void WriteEvent(int eventId, long arg1, int arg2, string arg3)
        {
            if (IsEnabled())
            {
                if (arg3 == null) arg3 = "";
                fixed (char* string3Bytes = arg3)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                    descrs[0].DataPointer = (IntPtr)(&arg1);
                    descrs[0].Size = 8;
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    descrs[2].DataPointer = (IntPtr)string3Bytes;
                    descrs[2].Size = ((arg3.Length + 1) * 2);
                    WriteEventCore(eventId, 3, descrs);
                }
            }
        }

        [Event(30, Level = EventLevel.Verbose, Keywords = Keywords.ThreadPool|Keywords.ThreadTransfer)]
        public void ThreadPoolEnqueueWork(long workID) {
            WriteEvent(30, workID);
        }
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadPoolEnqueueWorkObject(object workID) {
            // convert the Object Id to a long
            ThreadPoolEnqueueWork((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref workID)));
        }

        [Event(31, Level = EventLevel.Verbose, Keywords = Keywords.ThreadPool|Keywords.ThreadTransfer)]
        public void ThreadPoolDequeueWork(long workID) {
            WriteEvent(31, workID);
        }

        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadPoolDequeueWorkObject(object workID) {
            // convert the Object Id to a long
            ThreadPoolDequeueWork((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref workID)));
        }

        // id -   represents a correlation ID that allows correlation of two activities, one stamped by 
        //        ThreadTransferSend, the other by ThreadTransferReceive
        // kind - identifies the transfer: values below 64 are reserved for the runtime. Currently used values:
        //        1 - managed Timers ("roaming" ID)
        //        2 - managed async IO operations (FileStream, PipeStream, a.o.)
        //        3 - WinRT dispatch operations
        // info - any additional information user code might consider interesting
        [Event(150, Level = EventLevel.Informational, Keywords = Keywords.ThreadTransfer, Task = Tasks.ThreadTransfer, Opcode = EventOpcode.Send)]
        public void ThreadTransferSend(long id, int kind, string info, bool multiDequeues) {
            if (IsEnabled())
                WriteEvent(150, id, kind, info, multiDequeues);
        }
        // id - is a managed object. it gets translated to the object's address. ETW listeners must
        //      keep track of GC movements in order to correlate the value passed to XyzSend with the
        //      (possibly changed) value passed to XyzReceive
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadTransferSendObj(object id, int kind, string info, bool multiDequeues) {
            ThreadTransferSend((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), kind, info, multiDequeues);
        }

        // id -   represents a correlation ID that allows correlation of two activities, one stamped by 
        //        ThreadTransferSend, the other by ThreadTransferReceive
        // kind - identifies the transfer: values below 64 are reserved for the runtime. Currently used values:
        //        1 - managed Timers ("roaming" ID)
        //        2 - managed async IO operations (FileStream, PipeStream, a.o.)
        //        3 - WinRT dispatch operations
        // info - any additional information user code might consider interesting
        [Event(151, Level = EventLevel.Informational, Keywords = Keywords.ThreadTransfer, Task = Tasks.ThreadTransfer, Opcode = EventOpcode.Receive)]
        public void ThreadTransferReceive(long id, int kind, string info) {
            if (IsEnabled())
                WriteEvent(151, id, kind, info);
        }
        // id - is a managed object. it gets translated to the object's address. ETW listeners must
        //      keep track of GC movements in order to correlate the value passed to XyzSend with the
        //      (possibly changed) value passed to XyzReceive
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadTransferReceiveObj(object id, int kind, string info) {
            ThreadTransferReceive((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), kind, info);
        }
    }
}

