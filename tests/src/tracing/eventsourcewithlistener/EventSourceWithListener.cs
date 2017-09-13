// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Tracing.Tests.Common;

namespace Tracing.Tests
{
    [EventSource(Name = "SimpleEventSource")]
    class SimpleEventSource : EventSource
    {
        public SimpleEventSource() : base(true) { }

        [Event(1)]
        internal void MathResult(int x, int y, int z, string formula) { this.WriteEvent(1, x, y, z, formula); }
    }

    public class SimpleChecksum
    {
        public int? Value { get; private set; } = null;

        public void Update(IEnumerable<object> objs)
        {
            foreach (object o in objs)
            {
                Value = CombineHashCodes(Value ?? 0, o.GetHashCode());
            }
        }
        //
        // From https://stackoverflow.com/a/1646913
        private static int CombineHashCodes(int a, int b)
        {
            int hash = 17;  // seed
            unchecked
            {
                hash = hash * 31 + a;
                hash = hash * 31 + b;
            }
            return hash;
        }

        public override string ToString()
        {
            return Value != null ? ((int)Value).ToString("X8") : "null";
        }
    }

    public class VerificationEventListener : EventListener
    {
        public string TargetName { get; set; }

        public SimpleChecksum Checksum { get; private set; } = new SimpleChecksum();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Checksum.Update(eventData.Payload);
        }
    }

    class EventPipeSmoke
    {
        private static int messageIterations = 10000;
        private static int trivialSize = 0x100000;

        public static TraceConfiguration GetConfig(EventSource eventSource, string outputFile="default.netperf")
        {
            // Setup the configuration values.
            uint circularBufferMB = 1024; // 1 GB
            uint level = 5;//(uint)EventLevel.Informational;
            TimeSpan profSampleDelay = TimeSpan.FromMilliseconds(1);

            // Create a new instance of EventPipeConfiguration.
            TraceConfiguration config = new TraceConfiguration(outputFile, circularBufferMB);
            // Setup the provider values.
            // Public provider.
            string providerName = eventSource.Name;
            UInt64 keywords = 0xffffffffffffffff;

            // Enable the provider.
            config.EnableProvider(providerName, keywords, level);

            // Set the sampling rate.
            config.SetSamplingRate(profSampleDelay);

            return config;
        }

        static int Main(string[] args)
        {
            bool keepOutput = false;
            bool pass = false;
            // Use the first arg as an output filename if there is one
            string outputFilename = null;
            if (args.Length >= 1) {
                outputFilename = args[0];
                keepOutput = true;
            }
            else {
                outputFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".netperf";
            }

            SimpleEventSource eventSource = new SimpleEventSource();

            Console.WriteLine("\tStart: Enable tracing.");
            TraceControl.Enable(GetConfig(eventSource, outputFilename));
            Console.WriteLine("\tEnd: Enable tracing.\n");

            // Send messages
            // Use random numbers and addition as a simple, human readble checksum
            Random generator = new Random();
            using (var eventlistener = new VerificationEventListener())
            {
                var checksum = new SimpleChecksum();

                Console.WriteLine("\tStart: Messaging.");
                for(int i=0; i<messageIterations; i++)
                {
                    int x = generator.Next(1,1000);
                    int y = generator.Next(1,1000);
                    string formula = String.Format("{0} + {1} = {2}", x, y, x+y);

                    eventSource.MathResult(x, y, x+y, formula);

                    checksum.Update(new object[] {x, y, x+y, formula});
                }
                Console.WriteLine("\tEnd: Messaging.\n");

                pass = checksum.Value == eventlistener.Checksum.Value;
                Console.WriteLine($"\tExpected checksum {checksum}\n\tActual checksum {eventlistener.Checksum}\n\tPass {pass}\n");
            }

            Console.WriteLine("\tStart: Disable tracing.");
            TraceControl.Disable();
            Console.WriteLine("\tEnd: Disable tracing.\n");

            if (keepOutput)
            {
                Console.WriteLine(String.Format("\tOutput file: {0}", outputFilename));
            }
            else
            {
                System.IO.File.Delete(outputFilename);
            }

            return pass ? 100 : -1;
        }
    }
}
