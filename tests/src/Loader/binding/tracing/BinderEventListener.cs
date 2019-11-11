﻿// Licensed to the .NET Foundation under one or more agreements.
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
        public AssemblyName AssemblyName { get; internal set; }
        public string AssemblyPath { get; internal set; }
        public AssemblyName RequestingAssembly { get; internal set; }
        public string AssemblyLoadContext { get; internal set; }
        public string RequestingAssemblyLoadContext { get; internal set; }

        public bool Success { get; internal set; }
        public AssemblyName ResultAssemblyName { get; internal set; }
        public string ResultAssemblyPath { get; internal set; }
        public bool Cached { get; internal set; }

        public Guid ActivityId { get; internal set; }
        public Guid ParentActivityId { get; internal set; }

        public bool Completed { get; internal set; }
        public bool Nested { get; internal set; }

        public List<HandlerInvocation> AssemblyLoadContextResolvingHandlers { get; internal set; }
        public List<HandlerInvocation> AppDomainAssemblyResolveHandlers { get; internal set; }

        public List<BindOperation> NestedBinds { get; internal set; }

        public BindOperation()
        {
            AssemblyLoadContextResolvingHandlers = new List<HandlerInvocation>();
            AppDomainAssemblyResolveHandlers = new List<HandlerInvocation>();
            NestedBinds = new List<BindOperation>();
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(AssemblyName);
            sb.Append($" - Request: Path={AssemblyPath}, ALC={AssemblyLoadContext}, RequestingAssembly={RequestingAssembly}, RequestingALC={RequestingAssemblyLoadContext}");
            sb.Append($" - Result: Success={Success}, Name={ResultAssemblyName}, Path={ResultAssemblyPath}, Cached={Cached}");
            return sb.ToString();
        }
    }

    internal class HandlerInvocation
    {
        public AssemblyName AssemblyName { get; internal set; }
        public string HandlerName { get; internal set; }
        public string AssemblyLoadContext { get; internal set; }

        public AssemblyName ResultAssemblyName { get; internal set; }
        public string ResultAssemblyPath { get; internal set; }

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

            object GetData(string name)
            {
                int index = data.PayloadNames.IndexOf(name);
                return index >= 0 ? data.Payload[index] : null;
            };
            string GetDataString(string name) => GetData(name) as string;

            switch (data.EventName)
            {
                case "AssemblyLoadStart":
                {
                    BindOperation bindOperation = ParseAssemblyLoadStartEvent(data, GetDataString);
                    lock (eventsLock)
                    {
                        Assert.IsTrue(!bindOperations.ContainsKey(data.ActivityId), "AssemblyLoadStart should not exist for same activity ID ");
                        bindOperation.Nested = bindOperations.ContainsKey(data.RelatedActivityId);
                        bindOperations.Add(data.ActivityId, bindOperation);
                        if (bindOperation.Nested)
                        {
                            bindOperations[data.RelatedActivityId].NestedBinds.Add(bindOperation);
                        }
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
                case "AssemblyLoadContextResolvingHandlerInvoked":
                {
                    HandlerInvocation handlerInvocation = ParseHandlerInvokedEvent(GetDataString);
                    lock (eventsLock)
                    {
                        Assert.IsTrue(bindOperations.ContainsKey(data.ActivityId), "AssemblyLoadContextResolvingHandlerInvoked should have a matching AssemblyBindStart");
                        BindOperation bind = bindOperations[data.ActivityId];
                        bind.AssemblyLoadContextResolvingHandlers.Add(handlerInvocation);
                    }
                    break;
                }
                case "AppDomainAssemblyResolveHandlerInvoked":
                {
                    HandlerInvocation handlerInvocation = ParseHandlerInvokedEvent(GetDataString);
                    lock (eventsLock)
                    {
                        Assert.IsTrue(bindOperations.ContainsKey(data.ActivityId), "AppDomainAssemblyResolveHandlerInvoked should have a matching AssemblyBindStart");
                        BindOperation bind = bindOperations[data.ActivityId];
                        bind.AppDomainAssemblyResolveHandlers.Add(handlerInvocation);
                    }
                    break;
                }
            }
        }

        private BindOperation ParseAssemblyLoadStartEvent(EventWrittenEventArgs data, Func<string, string> getDataString)
        {
            var bindOperation = new BindOperation()
            {
                AssemblyName = new AssemblyName(getDataString("AssemblyName")),
                AssemblyPath = getDataString("AssemblyPath"),
                AssemblyLoadContext = getDataString("AssemblyLoadContext"),
                RequestingAssemblyLoadContext = getDataString("RequestingAssemblyLoadContext"),
                ActivityId = data.ActivityId,
                ParentActivityId = data.RelatedActivityId,
            };
            string requestingAssembly = getDataString("RequestingAssembly");
            if (!string.IsNullOrEmpty(requestingAssembly))
            {
                bindOperation.RequestingAssembly = new AssemblyName(requestingAssembly);
            }

            return bindOperation;
        }

        private HandlerInvocation ParseHandlerInvokedEvent(Func<string, string> getDataString)
        {
            var handlerInvocation = new HandlerInvocation()
            {
                AssemblyName = new AssemblyName(getDataString("AssemblyName")),
                HandlerName = getDataString("HandlerName"),
                AssemblyLoadContext = getDataString("AssemblyLoadContext"),
                ResultAssemblyPath = getDataString("ResultAssemblyPath")
            };
            string resultName = getDataString("ResultAssemblyName");
            if (!string.IsNullOrEmpty(resultName))
            {
                handlerInvocation.ResultAssemblyName = new AssemblyName(resultName);
            }

            return handlerInvocation;
        }
    }
}
