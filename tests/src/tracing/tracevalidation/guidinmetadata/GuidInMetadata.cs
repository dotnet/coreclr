using System;
using System.IO;
using Tracing.Tests.Common;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing.Parsers.Tpl;
using System.Threading.Tasks;

namespace Tracing.Tests
{
    public static class TraceValidationGuidInMetadata
    {
        private static readonly Guid ExpectedActivityId = new Guid("10000000-0000-0000-0000-000000000001");

        public static int Main(string[] args)
        {
            using (var netPerfFile = NetperfFile.Create(args))
            {
                Console.WriteLine("\tStart: Enable tracing.");
                TraceConfiguration config = new TraceConfiguration(netPerfFile.Path, circularBufferMB: 1024);
                config.EnableProvider(TplEtwProviderTraceEventParser.ProviderName, ulong.MaxValue, (uint)TraceEventLevel.Verbose);
                TraceControl.Enable(config);
                Console.WriteLine("\tEnd: Enable tracing.\n");

                Console.WriteLine($"\tSetting the activity ID to {ExpectedActivityId.ToString()}.");
                EventSource.SetCurrentThreadActivityId(ExpectedActivityId);
                Debug.Assert(ExpectedActivityId == EventSource.CurrentThreadActivityId);

                Console.WriteLine("\tStart: Disable tracing.");
                TraceControl.Disable();
                Console.WriteLine("\tEnd: Disable tracing.\n");

                Console.WriteLine("\tStart: Process the trace file.");

                int matchingEventCount = 0, nonMatchingEventCount = 0;
                using (var trace = TraceEventDispatcher.GetDispatcherFromFileName(netPerfFile.Path))
                {
                    var tplProvider = new TplEtwProviderTraceEventParser(trace);

                    tplProvider.All += (TraceEvent @event) =>
                    {
                        Console.WriteLine($"\tEvent! {@event.EventName} {@event.ID}");

                        if (@event.ID != (TraceEventID)25) // SetActivityId https://github.com/dotnet/coreclr/blob/c67c29d6e226e4cca1f1efb4d57b7f498d58b534/src/mscorlib/src/System/Threading/Tasks/TPLETWProvider.cs#L524
                            return;

                        if (@event.ActivityID == ExpectedActivityId)
                            matchingEventCount++;
                        else
                            nonMatchingEventCount++;
                    };

                    trace.Process();
                }
                Console.WriteLine("\tEnd: Processing events from file.\n");

                Assert.Equal(nameof(nonMatchingEventCount), 0, nonMatchingEventCount);
                Assert.Equal(nameof(matchingEventCount), 1, matchingEventCount);
            }

            return 100;
        }
    }
}
