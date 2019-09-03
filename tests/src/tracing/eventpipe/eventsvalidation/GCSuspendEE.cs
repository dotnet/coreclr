// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Tracing.Tests.Common;

namespace Tracing.Tests.GCSuspendEE
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
                GC.Collect();
            }
        };

        private static Func<EventPipeEventSource, Func<int>> _DoesTraceContainEvents = (source) => 
        {
            int GCSuspendEEEvents =0;
            source.Clr.GCSuspendEEStart += (eventData) => GCSuspendEEEvents += 1;
            int GCSuspendEEEndEvents =0;
            source.Clr.GCSuspendEEStop += (eventData) => GCSuspendEEEndEvents += 1;
            return () => {
                Logger.logger.Log("Event counts validation");
                Logger.logger.Log("GCSuspendEEEvents: " + GCSuspendEEEvents);
                Logger.logger.Log("GCSuspendEEEndEvents: " + GCSuspendEEEndEvents);
                return GCSuspendEEEvents >= 1000 && GCSuspendEEEndEvents >= 1000 && GCSuspendEEEvents==GCSuspendEEEndEvents ? 100 : -1;
            };
        };
    }
}