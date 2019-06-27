// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Tracing.Tests.Common;

namespace Tracing.Tests.RundownValidation
{

    public class RundownValidation
    {
        private static Dictionary<string, int> _expectedEventCounts = new Dictionary<string, int>()
        {
            { "Microsoft-Windows-DotNETRuntimeRundown", -1 }
        };

        private static Action _eventGeneratingAction = () => 
        {
            Thread.Sleep(500);
        };

        private static Func<EventPipeEventSource, int> _DoesRundownContainMethodEvents = (source) =>
        {
            bool hasMethodDCStopVerbose = false;
            bool hasMethodILToNativeMap = false;
            source.Clr.MethodDCStopVerboseV2 += (eventData) => hasMethodDCStopVerbose = true;
            source.Clr.MethodILToNativeMap += (eventData) => hasMethodILToNativeMap = true;
            source.Process();
            return hasMethodDCStopVerbose && hasMethodILToNativeMap ? 100 : -1;
        };

        public static int Main(string[] args)
        {
            // This test validates that the rundown events are present
            // and that the rundown contains the necessary events to get
            // symbols in a nettrace file.

            var providers = new List<Provider>()
            {
                new Provider("Microsoft-DotNETCore-SampleProfiler")
            };

            var tests = Enumerable.Range(0,12)
                .Select(x => (uint)Math.Pow(2, x))
                .Select(bufferSize => new SessionConfiguration(circularBufferSizeMB: bufferSize, format: EventPipeSerializationFormat.NetTrace,  providers: providers))
                .Select<SessionConfiguration, Func<int>>(configuration => () => IpcTraceTest.RunAndValidateEventCounts(_expectedEventCounts, _eventGeneratingAction, 1, configuration, _DoesRundownContainMethodEvents));

            foreach (var test in tests)
            {
                var ret = test();
                if (ret < 0)
                    return ret;
            }

            return 100;
        }
    }
}