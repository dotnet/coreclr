using System;
using System.IO;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Threading;
using Tracing.Tests.Common;

namespace Tracing.Tests
{
    public sealed class RuntimeEventSourceTest
    {
        static int Main(string[] args)
        {
            // Get the RuntimeEventSource.
            EventSource eventSource = RuntimeEventSource.Log;

            // Create of EventListener.
            using (SimpleEventListener listener = new SimpleEventListener())
            {
                // Trigger the allocator task.
                System.Threading.Tasks.Task.Run(new Action(Allocator));

                // Enable events.
                listener.EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords)(-1));

                // Wait for events.
                Thread.Sleep(1000);

                GC.Collect(2, GCCollectionMode.Forced);

                // Wait for more events.
                Thread.Sleep(1000);

                // Ensure that we've seen some events.
                Assert.True("listener.EventCount > 0", listener.EventCount > 0);
            }

            return 100;
        }

        private static void Allocator()
        {
            while (true)
            {
                for(int i=0; i<1000; i++)
                    GC.KeepAlive(new object());

                Thread.Sleep(10);
            }
        }
    }

    internal sealed class SimpleEventListener : EventListener
    {
        public int EventCount { get; private set; } = 0;

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine($"ID = {eventData.EventId} Name = {eventData.EventName}");
            for (int i = 0; i < eventData.Payload.Count; i++)
            {
                string payloadString = eventData.Payload[i] != null ? eventData.Payload[i].ToString() : string.Empty;
                Console.WriteLine($"\tName = \"{eventData.PayloadNames[i]}\" Value = \"{payloadString}\"");
            }
            Console.WriteLine("\n");


            EventCount++;
        }
    }
}
