// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tools.RuntimeClient;

namespace Tracing.Tests.Common
{
    public class IpcTraceTest
    {
        // This Action is executed while the trace is being collected.
        private Action _eventGeneratingAction;

        // A dictionary of event providers to number of events.
        // A count of -1 indicates that you are only testing for the presence of the provider
        // and don't care about the number of events sent
        private Dictionary<string, int> _expectedEventCounts;
        private Dictionary<string, int> _actualEventCounts = new Dictionary<string, int>();
        private SessionConfiguration _sessionConfiguration;
        // The acceptable +- on the count of events
        private int _errorRange;
        private Func<EventPipeEventSource, int> _optionalTraceValidator;

        IpcTraceTest(
            Dictionary<string, int> expectedEventCounts,
            Action eventGeneratingAction,
            int errorRange = 0,
            SessionConfiguration? sessionConfiguration = null,
            Func<EventPipeEventSource, int> optionalTraceValidator = null)
        {
            _eventGeneratingAction = eventGeneratingAction;
            _expectedEventCounts = expectedEventCounts;
            _errorRange = errorRange;
            _sessionConfiguration = sessionConfiguration ?? new SessionConfiguration(
                circularBufferSizeMB: 1000,
                format: EventPipeSerializationFormat.NetTrace,
                providers: new List<Provider> { new Provider("Microsoft-Windows-DotNETRuntime") });
            _optionalTraceValidator = _optionalTraceValidator;
        }

        private int Fail(string message = "")
        {
            Console.WriteLine("Test FAILED!");
            Console.WriteLine(message);
            Console.WriteLine("Configuration:");
            Console.WriteLine("{");
            Console.WriteLine($"\tbufferSize: {_sessionConfiguration.CircularBufferSizeInMB},");
            Console.WriteLine("\tproviders: [");
            foreach (var provider in _sessionConfiguration.Providers)
            {
                Console.WriteLine($"\t\t{provider.ToString()},");
            }
            Console.WriteLine("\t]");
            Console.WriteLine("}\n");
            Console.WriteLine("Expected:");
            Console.WriteLine("{");
            foreach (var (k,v) in _expectedEventCounts)
            {
                Console.WriteLine($"\t\"{k}\" = {v}");
            }
            Console.WriteLine("}\n");

            Console.WriteLine("Actual:");
            Console.WriteLine("{");
            foreach (var (k,v) in _actualEventCounts)
            {
                Console.WriteLine($"\t\"{k}\" = {v}");
            }
            Console.WriteLine("}");

            return -1;
        }

        private int Validate()
        {
            var processId = Process.GetCurrentProcess().Id;
            var binaryReader = EventPipeClient.CollectTracing(processId, _sessionConfiguration, out var eventpipeSessionId);
            if (eventpipeSessionId == 0)
                return -1;

            var mre = new ManualResetEvent(false);
            using (var memoryStream = new MemoryStream())
            {

                var eventGeneratorTask = new Task(() =>
                {
                    mre.WaitOne();
                    _eventGeneratingAction();
                    EventPipeClient.StopTracing(processId, eventpipeSessionId);
                });

                var readerTask = new Task(() =>
                {
                    int b;
                    mre.Set();
                    while ((b = binaryReader.ReadByte()) != -1)
                    {
                        memoryStream.WriteByte((byte)b);
                    }
                });

                readerTask.Start();
                eventGeneratorTask.Start();

                Task.WaitAll(readerTask, eventGeneratorTask);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var source = new EventPipeEventSource(memoryStream);
                source.Dynamic.All += (eventData) =>
                {
                    if (_actualEventCounts.TryGetValue(eventData.ProviderName, out _))
                    {
                        _actualEventCounts[eventData.ProviderName]++;
                    }
                    else
                    {
                        _actualEventCounts[eventData.ProviderName] = 1;
                    }
                };

                source.Process();

                foreach (var (provider, expectedCount) in _expectedEventCounts)
                {
                    if (_actualEventCounts.TryGetValue(provider, out var actualCount))
                    {
                        if (expectedCount != -1 && Math.Abs(expectedCount - actualCount) > _errorRange)
                        {
                            return Fail($"Event count mismatch for provider \"{provider}\": expected {expectedCount}, but saw {actualCount}");
                        }
                    }
                    else
                    {
                        return Fail($"No events for provider \"{provider}\"");
                    }
                }

                if (_optionalTraceValidator != null)
                {
                    return _optionalTraceValidator(source);
                }
                else
                {
                    return 100;
                }
            }
        }

        public static int RunAndValidateEventCounts(
            Dictionary<string, int> expectedEventCounts,
            Action eventGeneratingAction,
            int errorRange = 0,
            SessionConfiguration? sessionConfiguration = null,
            Func<EventPipeEventSource, int> optionalTraceValidator = null)
        {
            Console.WriteLine("TEST STARTING");
            var test = new IpcTraceTest(expectedEventCounts, eventGeneratingAction, errorRange, sessionConfiguration, optionalTraceValidator);
            try
            {
                var ret = test.Validate();
                if (ret == 100)
                    Console.WriteLine("TEST PASSED!");
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine("TEST FAILED!");
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}