// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Tracing.Tests.Common;

namespace Tracing.Tests.GCEEStartStop
{
    public class ProviderValidation
    {
        public static int Main(string[] args)
        {
            var providers = new List<Provider>()
            {
                new Provider("Microsoft-DotNETCore-SampleProfiler"),
                new Provider("Microsoft-Windows-DotNETRuntime", 0b1, EventLevel.Informational)
            };
            
            var configuration = new SessionConfiguration(circularBufferSizeMB: 1024, format: EventPipeSerializationFormat.NetTrace,  providers: providers);
            return IpcTraceTest.RunAndValidateEventCounts(_expectedEventCounts, _eventGeneratingAction, configuration, _DoesTraceContainEvents);
        }

        private static Dictionary<string, ExpectedEventCount> _expectedEventCounts = new Dictionary<string, ExpectedEventCount>()
        {
            { "Microsoft-Windows-DotNETRuntime", -1 },
            { "Microsoft-Windows-DotNETRuntimeRundown", -1 },
            { "Microsoft-DotNETCore-SampleProfiler", -1 }
        };

        private static Action _eventGeneratingAction = () => 
        {
            for (int i = 0; i < 1000; i++)
            {
                if (i % 100 == 0)
                    Logger.logger.Log($"Called GC.Collect() {i} times...");
                ProviderValidation providerValidation = new ProviderValidation();
                providerValidation = null;
                GC.Collect();
            }
        };

        private static Func<EventPipeEventSource, Func<int>> _DoesTraceContainEvents = (source) => 
        {
            int GCRestartEEStartEvents =0;
            int GCRestartEEStopEvents =0;           
            source.Clr.GCRestartEEStart += (eventData) => GCRestartEEStartEvents +=1;
            source.Clr.GCRestartEEStop += (eventData) => GCRestartEEStopEvents +=1; 
            return () => {
                Logger.logger.Log("Event counts validation");
                Logger.logger.Log("GCRestartEEStartEvents: " + GCRestartEEStartEvents);
                Logger.logger.Log("GCRestartEEStopEvents: " + GCRestartEEStopEvents);
                return GCRestartEEStartEvents >= 1000 && GCRestartEEStopEvents >= 1000 && GCRestartEEStartEvents == GCRestartEEStopEvents ? 100 : -1;
            };
        };
    }
}