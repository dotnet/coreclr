// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Reflection;

using TestLibrary;

namespace BinderTracingTests
{
    internal class BindOperation
    {
        internal AssemblyName AssemblyName;
        internal string AssemblyPath;
        internal AssemblyName RequestingAssembly;
        internal string AssemblyLoadContext;
        internal string RequestingAssemblyLoadContext;

        internal bool Success;
        internal AssemblyName ResultAssemblyName;
        internal string ResultAssemblyPath;
        internal bool Cached;

        internal Guid ActivityId;
        internal Guid ParentActivityId;

        internal bool Completed;
        internal bool Nested;

        internal List<HandlerInvocation> ALCResolvingHandlers = new List<HandlerInvocation>();
    }

    internal class HandlerInvocation
    {
        internal AssemblyName AssemblyName;
        internal string HandlerName;
        internal string AssemblyLoadContext;

        internal AssemblyName ResultAssemblyName;
        internal string ResultAssemblyPath;

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{HandlerName} - ");
            sb.Append($"Request: Name={AssemblyName.FullName}");
            if (!string.IsNullOrEmpty(AssemblyLoadContext))
                sb.Append($", ALC={AssemblyLoadContext}");

            sb.Append($" - Result: Name={ResultAssemblyName?.FullName}, Path={ResultAssemblyPath}");
            return sb.ToString();
        }
    }

    internal sealed class BinderEventListener : EventListener
    {
        private const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;
        private const EventKeywords AssemblyLoaderKeyword = (EventKeywords)0x4;

        private readonly object eventsLock = new object();
        private readonly Dictionary<Guid, BindOperation> bindOperations = new Dictionary<Guid, BindOperation>();

        public BindOperation[] WaitAndGetEventsForAssembly(string simpleName, int waitTimeoutInMs = 10000)
        {
            const int waitIntervalInMs = 50;
            int timeWaitedInMs = 0;
            do
            {
                lock (eventsLock)
                {
                    var events = bindOperations.Values.Where(e => e.Completed && e.AssemblyName.Name == simpleName && !e.Nested);
                    if (events.Any())
                    {
                        return events.ToArray();
                    }
                }

                Thread.Sleep(waitIntervalInMs);
                timeWaitedInMs += waitIntervalInMs;
            } while (timeWaitedInMs < waitTimeoutInMs);

            throw new TimeoutException($"Timed out waiting for bind events for {simpleName}");
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
            {
                EnableEvents(eventSource, EventLevel.Verbose, AssemblyLoaderKeyword);
            }
            else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
            {
                EnableEvents(eventSource, EventLevel.Verbose, TasksFlowActivityIds);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs data)
        {
            if (data.EventSource.Name != "Microsoft-Windows-DotNETRuntime")
                return;

            object GetData(string name) => data.Payload[data.PayloadNames.IndexOf(name)];
            string GetDataString(string name) => GetData(name) as string;

            switch (data.EventName)
            {
                case "AssemblyLoadStart":
                {
                    var bindOperation = new BindOperation()
                    {
                        AssemblyName = new AssemblyName(GetDataString("AssemblyName")),
                        AssemblyPath = GetDataString("AssemblyPath"),
                        AssemblyLoadContext = GetDataString("AssemblyLoadContext"),
                        RequestingAssemblyLoadContext = GetDataString("RequestingAssemblyLoadContext"),
                        ActivityId = data.ActivityId,
                        ParentActivityId = data.RelatedActivityId,
                    };
                    string requestingAssembly = GetDataString("RequestingAssembly");
                    if (!string.IsNullOrEmpty(requestingAssembly))
                    {
                        bindOperation.RequestingAssembly = new AssemblyName(requestingAssembly);
                    }

                    lock (eventsLock)
                    {
                        Assert.IsTrue(!bindOperations.ContainsKey(data.ActivityId), "AssemblyLoadStart should not exist for same activity ID ");
                        bindOperation.Nested = bindOperations.ContainsKey(data.RelatedActivityId);
                        bindOperations.Add(data.ActivityId, bindOperation);
                    }
                    break;
                }
                case "AssemblyLoadStop":
                {
                    lock (eventsLock)
                    {
                        Assert.IsTrue(bindOperations.ContainsKey(data.ActivityId), "AssemblyLoadStop should have a matching AssemblyBindStart");
                        BindOperation bind = bindOperations[data.ActivityId];
                        bind.Success = (bool)GetData("Success");
                        string resultName = GetDataString("ResultAssemblyName");
                        if (!string.IsNullOrEmpty(resultName))
                        {
                            bind.ResultAssemblyName = new AssemblyName(resultName);
                        }
                        bind.ResultAssemblyPath = GetDataString("ResultAssemblyPath");
                        bind.Cached = (bool)GetData("Cached");
                        bind.Completed = true;
                    }
                    break;
                }
                case "ALCResolvingHandlerInvoked":
                {
                    var handlerInvocation = new HandlerInvocation()
                    {
                        AssemblyName = new AssemblyName(GetDataString("AssemblyName")),
                        HandlerName = GetDataString("HandlerName"),
                        AssemblyLoadContext = GetDataString("AssemblyLoadContext"),
                        ResultAssemblyPath = GetDataString("ResultAssemblyPath")
                    };
                    string resultName = GetDataString("ResultAssemblyName");
                    if (!string.IsNullOrEmpty(resultName))
                    {
                        handlerInvocation.ResultAssemblyName = new AssemblyName(resultName);
                    }

                    lock (eventsLock)
                    {
                        Assert.IsTrue(bindOperations.ContainsKey(data.ActivityId), "ALCResolvingHandlerInvoked should have a matching AssemblyBindStart");
                        BindOperation bind = bindOperations[data.ActivityId];
                        bind.ALCResolvingHandlers.Add(handlerInvocation);
                    }
                    break;
                }
            }

                var sb = new System.Text.StringBuilder($"{data.EventName}");
                sb.AppendLine();
                sb.AppendLine("  Payload:");
                for (int i = 0; i < data.Payload.Count; i++)
                {
                    string payloadString = data.Payload[i] != null ? data.Payload[i].ToString() : string.Empty;
                    sb.AppendLine($"    {data.PayloadNames[i]} = {payloadString}");
                }

                sb.AppendLine($"  Activity: {data.ActivityId}");
                sb.AppendLine($"  Related Activity: {data.RelatedActivityId}");

                // Console.WriteLine(sb.ToString());
        }
    }
}
