// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace ABIStress
{
    internal class TailCallEventListener : EventListener
    {
        public int NumCallersSeen { get; set; }
        public int NumSuccessfulTailCalls { get; set; }
        public Dictionary<string, int> FailureReasons { get; } = new Dictionary<string, int>();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name != "Microsoft-Windows-DotNETRuntime")
                return;

            EventKeywords jitTracing = (EventKeywords)0x1000; // JITSymbols | JITTracing
            EnableEvents(eventSource, EventLevel.Verbose, jitTracing);
        }

        protected override void OnEventWritten(EventWrittenEventArgs data)
        {
            string GetData(string name) => data.Payload[data.PayloadNames.IndexOf(name)].ToString();

            switch (data.EventName)
            {
                case "MethodJitTailCallFailed":
                    if (GetData("MethodBeingCompiledName").StartsWith(Config.TailCallerPrefix))
                    {
                        NumCallersSeen++;
                        string failReason = GetData("FailReason");
                        lock (FailureReasons)
                        {
                            FailureReasons[failReason] = FailureReasons.GetValueOrDefault(failReason) + 1;
                        }
                    }
                    break;
                case "MethodJitTailCallSucceeded":
                    if (GetData("MethodBeingCompiledName").StartsWith(Config.TailCallerPrefix))
                    {
                        NumCallersSeen++;
                        NumSuccessfulTailCalls++;
                    }
                    break;
            }
        }
    }
}
