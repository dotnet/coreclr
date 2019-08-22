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
    public class Logger
    {
        private TextWriter _log;
        private Stopwatch _sw;
        public Logger(TextWriter log = null)
        {
            _log = log ?? Console.Out;
            _sw = new Stopwatch();
        }

        public void Log(string message)
        {
            if (!_sw.IsRunning)
                _sw.Start();
            _log.WriteLine($"{_sw.Elapsed.TotalSeconds,5:f1}s: {message}");
        }
    }

    public class ExpectedEventCount
    {
        // The acceptable percent error on the expected value
        // represented as a floating point value in [0,1].
        public float Error { get; private set; }

        // The expected count of events. A value of -1 indicates
        // that count does not matter, and we are simply testing
        // that the provider exists in the trace.
        public int Count { get; private set; }

        public ExpectedEventCount(int count, float error = 0.0f)
        {
            Count = count;
            Error = error;
        }

        public bool Validate(int actualValue)
        {
            return Count == -1 || CheckErrorBounds(actualValue);
        }

        public bool CheckErrorBounds(int actualValue)
        {
            return Math.Abs(actualValue - Count) <= (Count * Error);
        }

        public static implicit operator ExpectedEventCount(int i)
        {
            return new ExpectedEventCount(i);
        }

        public override string ToString()
        {
            return $"{Count} +- {Count * Error}";
        }
    }

    // This event source is used by the test infra to
    // to insure that providers have finished being enabled
    // for the session being observed. Since the client API
    // returns the pipe for reading _before_ it finishes
    // enabling the providers to write to that session,
    // we need to guarantee that our providers are on before
    // sending events. This is a _unique_ problem I imagine
    // should _only_ affect scenarios like these tests
    // where the reading and sending of events are required
    // to synchronize.
    public sealed class SentinelEventSource : EventSource
    {
        private SentinelEventSource() {}
        public static SentinelEventSource Log = new SentinelEventSource();
        public void SentinelEvent() { WriteEvent(1, "SentinelEvent"); }
    }

    public static class SessionConfigurationExtensions
    {
        public static SessionConfiguration InjectSentinel(this SessionConfiguration sessionConfiguration)
        {
            var newProviderList = new List<Provider>(sessionConfiguration.Providers);
            newProviderList.Add(new Provider("SentinelEventSource"));
            return new SessionConfiguration(sessionConfiguration.CircularBufferSizeInMB, sessionConfiguration.Format, newProviderList.AsReadOnly());
        }
    }

    public class IpcTraceTest
    {
        // This Action is executed while the trace is being collected.
        private Action _eventGeneratingAction;

        // A dictionary of event providers to number of events.
        // A count of -1 indicates that you are only testing for the presence of the provider
        // and don't care about the number of events sent
        private Dictionary<string, ExpectedEventCount> _expectedEventCounts;
        private Dictionary<string, int> _actualEventCounts = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, List<TraceEvent>>> _events = new Dictionary<string, Dictionary<string, List<TraceEvent>>>();
        private SessionConfiguration _sessionConfiguration;
        private static Logger _log = new Logger();

        // A function to be called with the EventPipeEventSource _before_
        // the call to `source.Process()`.  The function should return another
        // function that will be called to check whether the optional test was validated.
        // Example in situ: providervalidation.cs
        private Func<EventPipeEventSource, Func<int>> _optionalTraceValidator;

        IpcTraceTest(
            Dictionary<string, ExpectedEventCount> expectedEventCounts,
            Action eventGeneratingAction,
            SessionConfiguration? sessionConfiguration = null,
            Func<EventPipeEventSource, Func<int>> optionalTraceValidator = null)
        {
            _eventGeneratingAction = eventGeneratingAction;
            _expectedEventCounts = expectedEventCounts;
            _sessionConfiguration = sessionConfiguration?.InjectSentinel() ?? new SessionConfiguration(
                circularBufferSizeMB: 1000,
                format: EventPipeSerializationFormat.NetTrace,
                providers: new List<Provider> { 
                    new Provider("Microsoft-Windows-DotNETRuntime"),
                    new Provider("SentinelEventSource")
                });
            _optionalTraceValidator = optionalTraceValidator;
        }

        private int Fail(string message = "")
        {
            _log.Log("Test FAILED!");
            _log.Log(message);
            _log.Log("Configuration:");
            _log.Log("{");
            _log.Log($"\tbufferSize: {_sessionConfiguration.CircularBufferSizeInMB},");
            _log.Log("\tproviders: [");
            foreach (var provider in _sessionConfiguration.Providers)
            {
                _log.Log($"\t\t{provider.ToString()},");
            }
            _log.Log("\t]");
            _log.Log("}\n");
            _log.Log("Expected:");
            _log.Log("{");
            foreach (var (k, v) in _expectedEventCounts)
            {
                _log.Log($"\t\"{k}\" = {v}");
            }
            _log.Log("}\n");

            _log.Log("Actual:");
            _log.Log("{");
            foreach (var (k, v) in _actualEventCounts)
            {
                _log.Log($"\t\"{k}\" = {v}");
            }
            _log.Log("}");

            return -1;
        }

        private int Validate()
        {
            var processId = Process.GetCurrentProcess().Id;
            _log.Log("Connecting to EventPipe...");
            var binaryReader = EventPipeClient.CollectTracing(processId, _sessionConfiguration, out var eventpipeSessionId);
            if (eventpipeSessionId == 0)
            {
                _log.Log("Failed to connect to EventPipe!");
                return -1;
            }
            _log.Log($"Connected to EventPipe with sessionID '0x{eventpipeSessionId:x}'");
            
            // CollectTracing returns before EventPipe::Enable has returned, so the
            // the sources we want to listen for may not have been enabled yet.
            // We'll use this sentinel EventSource to check if Enable has finished
            ManualResetEvent sentinelEventReceived = new ManualResetEvent(false);
            var sentinelTask = new Task(() =>
            {
                _log.Log("Started sending sentinel events...");
                while (!sentinelEventReceived.WaitOne(50))
                {
                    SentinelEventSource.Log.SentinelEvent();
                }
                _log.Log("Stopped sending sentinel events");
            });
            sentinelTask.Start();

            EventPipeEventSource source = null;
            Func<int> optionalTraceValidationCallback = null;
            var readerTask = new Task(() =>
            {
                _log.Log("Creating EventPipeEventSource...");
                source = new EventPipeEventSource(binaryReader);
                _log.Log("EventPipeEventSource created");

                source.Dynamic.All += (eventData) =>
                {
                    try
                    {
                        if (eventData.ProviderName == "SentinelEventSource")
                        {
                            if (!sentinelEventReceived.WaitOne(0))
                                _log.Log("Saw sentinel event");
                            sentinelEventReceived.Set();
                        }

                        else if (_actualEventCounts.TryGetValue(eventData.ProviderName, out _))
                        {
                            _actualEventCounts[eventData.ProviderName]++;
                        }
                        else
                        {
                            _log.Log($"Saw new provider '{eventData.ProviderName}'");
                            _actualEventCounts[eventData.ProviderName] = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Log("Exception in Dynamic.All callback " + e.ToString());
                    }
                };
                _log.Log("Dynamic.All callback registered");

                if (_optionalTraceValidator != null)
                {
                    _log.Log("Running optional trace validator");
                    optionalTraceValidationCallback = _optionalTraceValidator(source);
                    _log.Log("Finished running optional trace validator");
                }

                _log.Log("Starting stream processing...");
                source.Process();
                _log.Log("Stopping stream processing");
            });

            readerTask.Start();
            sentinelEventReceived.WaitOne();

            _log.Log("Starting event generating action...");
            _eventGeneratingAction();
            _log.Log("Stopping event generating action");

            _log.Log("Sending StopTracing command...");
            EventPipeClient.StopTracing(processId, eventpipeSessionId);
            _log.Log("Finished StopTracing command");

            readerTask.Wait();
            _log.Log("Reader task finished");

            foreach (var (provider, expectedCount) in _expectedEventCounts)
            {
                if (_actualEventCounts.TryGetValue(provider, out var actualCount))
                {
                    if (!expectedCount.Validate(actualCount))
                    {
                        return Fail($"Event count mismatch for provider \"{provider}\": expected {expectedCount}, but saw {actualCount}");
                    }
                }
                else
                {
                    return Fail($"No events for provider \"{provider}\"");
                }
            }

            if (optionalTraceValidationCallback != null)
            {
                _log.Log("Validating optional callback...");
                return optionalTraceValidationCallback();
            }
            else
            {
                return 100;
            }
        }

        public static int RunAndValidateEventCounts(
            Dictionary<string, ExpectedEventCount> expectedEventCounts,
            Action eventGeneratingAction,
            SessionConfiguration? sessionConfiguration = null,
            Func<EventPipeEventSource, Func<int>> optionalTraceValidator = null)
        {
            _log.Log("==TEST STARTING==");
            var test = new IpcTraceTest(expectedEventCounts, eventGeneratingAction, sessionConfiguration, optionalTraceValidator);
            try
            {
                var ret = test.Validate();
                if (ret == 100)
                    _log.Log("==TEST FINISHED: PASSED!==");
                return ret;
            }
            catch (Exception e)
            {
                _log.Log(e.ToString());
                _log.Log("==TEST FINISHED: FAILED!==");
                return -1;
            }
        }
    }
}