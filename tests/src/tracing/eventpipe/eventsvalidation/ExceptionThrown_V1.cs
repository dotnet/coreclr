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

namespace Tracing.Tests.ExceptionThrown_V1
{
    public sealed class MyEvents
    {
        private MyEvents() {}
        public static MyEvents Log = new MyEvents();
        public void MyEvent1() 
        {
            try
            {
                throw new ArgumentNullException("Throw ArgumentNullException");
            } 
            catch (Exception e)
            {
                //Do nonthing
            }
        }
    }

    public class ProviderValidation
    {
        public static int Main(string[] args)
        {
            var providers = new List<Provider>()
            {
                new Provider("Microsoft-DotNETCore-SampleProfiler"),
                new Provider("Microsoft-Windows-DotNETRuntime", 1000000000000000, EventLevel.Warning)
            };
            
            var tests = new int[] { 4, 10 }
                .Select(x => (uint)Math.Pow(2, x))
                .Select(bufferSize => new SessionConfiguration(circularBufferSizeMB: bufferSize, format: EventPipeSerializationFormat.NetTrace,  providers: providers))
                .Select<SessionConfiguration, Func<int>>(configuration => () => IpcTraceTest.RunAndValidateEventCounts(_expectedEventCounts, _eventGeneratingAction, configuration));

            foreach (var test in tests)
            {
                var ret = test();
                if (ret < 0)
                    return ret;
            }

            return 100;
        }

        private static Dictionary<string, ExpectedEventCount> _expectedEventCounts = new Dictionary<string, ExpectedEventCount>()
        {
            { "Microsoft-Windows-DotNETRuntime", 1000 },
            { "Microsoft-Windows-DotNETRuntimeRundown", -1 },
            { "Microsoft-DotNETCore-SampleProfiler", -1 }
        };

        private static Action _eventGeneratingAction = () => 
        {
            foreach (var _ in Enumerable.Range(0,1000))
            {
                MyEvents.Log.MyEvent1();
            }
        };
    }
}