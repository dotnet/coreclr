// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Tracing.Tests.Common;

namespace Tracing.Tests.GCFinalizers
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
                    Logger.logger.Log($"Called GC.WaitForPendingFinalizers() {i} times...");
                ProviderValidation providerValidation = new ProviderValidation();
                providerValidation = null;
                GC.WaitForPendingFinalizers();
            }
        };

        private static Func<EventPipeEventSource, Func<int>> _DoesTraceContainEvents = (source) => 
        {
            int GCFinalizersEndEvents =0;
            source.Clr.GCFinalizersStop += (eventData) => GCFinalizersEndEvents += 1;
            int GCFinalizersStartEvents =0;
            source.Clr.GCFinalizersStart += (eventData) => GCFinalizersStartEvents += 1;
            return () => {
                Logger.logger.Log("Event counts validation");
                Logger.logger.Log("GCFinalizersEndEvents: " + GCFinalizersEndEvents);
                Logger.logger.Log("GCFinalizersStartEvents: " + GCFinalizersStartEvents);
                return GCFinalizersEndEvents >= 1000 && GCFinalizersStartEvents >= 1000 ? 100 : -1;
            };
        };
    }
}