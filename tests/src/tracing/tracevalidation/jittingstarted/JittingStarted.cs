using System;
using System.IO;
using System.Runtime.CompilerServices;
using Tracing.Tests.Common;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Tracing.Tests
{
    public static class TraceValidationJittingStarted
    {
        public static int Main(string[] args)
        {
            bool pass = true;
            bool keepOutput = false;

            // Use the first arg as an output filename if there is one.
            string outputFilename = null;
            if (args.Length >= 1)
            {
                outputFilename = args[0];
                keepOutput = true;
            }
            else
            {
                outputFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".netperf";
            }

            try
            {
                Console.WriteLine("\tStart: Enable tracing.");
                TraceControl.EnableDefault(outputFilename);
                Console.WriteLine("\tEnd: Enable tracing.\n");

                Console.WriteLine("\tStart: Generate some events.");
                CompiledMethod();
                Console.WriteLine("\tEnd: Generate some events.\n");

                Console.WriteLine("\tStart: Disable tracing.");
                TraceControl.Disable();
                Console.WriteLine("\tEnd: Disable tracing.\n");

                Console.WriteLine("\tStart: Process the trace file.");

                int matchingEventCount = 0;
                int nonMatchingEventCount = 0;

                using (var trace = TraceEventDispatcher.GetDispatcherFromFileName(outputFilename))
                {
                    string methodNamespace = "Tracing.Tests.TraceValidationJittingStarted";
                    string methodName = "CompiledMethod";
                    string providerName = "Microsoft-Windows-DotNETRuntime";
                    string gcTriggeredEventName = "Method/JittingStarted";

                    trace.Clr.MethodJittingStarted += delegate(MethodJittingStartedTraceData data)
                    {
                        if(methodNamespace.Equals(data.MethodNamespace) &&
                           methodName.Equals(data.MethodName) &&
                           providerName.Equals(data.ProviderName) &&
                           gcTriggeredEventName.Equals(data.EventName))
                        {
                            matchingEventCount++;
                        }
                        else
                        {
                            nonMatchingEventCount++;
                        }
                    };

                    trace.Process();
                }
                Console.WriteLine("\tEnd: Processing events from file.\n");

                // CompiledMethod
                Assert.Equal(matchingEventCount, 1);

                // EventPipe.Disable
                Assert.Equal(nonMatchingEventCount, 1);
            }
            finally
            {
                if (keepOutput)
                {
                    Console.WriteLine("\n\tOutput file: {0}", outputFilename);
                }
                else
                {
                    System.IO.File.Delete(outputFilename);
                }
            }

            return 100;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CompiledMethod()
        {
        }
    }
}
