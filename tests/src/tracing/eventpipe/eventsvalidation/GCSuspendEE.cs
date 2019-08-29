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
            Console.WriteLine("EventPipe validation test");
            var providers = new List<Provider>()
            {
                new Provider("Microsoft-DotNETCore-SampleProfiler"),
                new Provider("Microsoft-Windows-DotNETRuntime", 0b1, EventLevel.Informational)
            };
            
            var configuration = new SessionConfiguration(circularBufferSizeMB: 1024, format: EventPipeSerializationFormat.NetTrace,  providers: providers);
            Console.WriteLine("Validation method: RunAndValidateEventCounts");
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
            Console.WriteLine("Event generating method: _eventGeneratingAction start");
            for (int i = 0; i < 1000; i++)
            {
                ProviderValidation providerValidation = new ProviderValidation();
                providerValidation.Temp();
            }
            Console.WriteLine("Event generating method: _eventGeneratingAction end");
        };

        private void Temp()
        {            
            GC.SuppressFinalize(this);
        }

        private static Func<EventPipeEventSource, Func<int>> _DoesTraceContainEvents = (source) => 
        {
            Console.WriteLine("Callback method: _DoesTraceContainEvents");
            int GCSuspendEEEvents =0;
            source.Clr.GCSuspendEEStart += (eventData) => GCSuspendEEEvents += 1;
            
            int GCSuspendEEEndEvents =0;
            source.Clr.GCSuspendEEStop += (eventData) => GCSuspendEEEndEvents += 1;

            return () => {
                Console.WriteLine("Event counts validation");
                Console.WriteLine("GCSuspendEEEvents: " + GCSuspendEEEvents);
                Console.WriteLine("GCSuspendEEEndEvents: " + GCSuspendEEEndEvents);
                return GCSuspendEEEvents >= 1000 && GCSuspendEEEndEvents >= 1000 && GCSuspendEEEvents==GCSuspendEEEndEvents ? 100 : -1;
            };
        };
    }
}