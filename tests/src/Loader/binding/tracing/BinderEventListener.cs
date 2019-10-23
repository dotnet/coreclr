﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Reflection;
using Xunit;

using Assert = Xunit.Assert;

namespace BinderTracingTests
{
    internal class BindEvent
    {
        internal AssemblyName AssemblyName;
        internal bool Success;

        internal Guid ActivityId;
        internal Guid ParentActivityId;

        internal bool Completed;
        internal bool Nested;
    }

    internal sealed class BinderEventListener : EventListener
    {
        public Dictionary<Guid, BindEvent> BindEvents = new Dictionary<Guid, BindEvent>();

        private const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;
        private const EventKeywords BinderKeyword = (EventKeywords)0x4;

        private object eventsLock = new object();

        public BindEvent[] WaitAndGetEventsForAssembly(string simpleName, int waitTimeoutInMs = 10000)
        {
            const int waitIntervalInMs = 50;
            int timeWaitedInMs = 0;
            do
            {
                lock (eventsLock)
                {
                    var events = BindEvents.Values.Where(e => e.Completed && e.AssemblyName.Name == simpleName && !e.Nested);
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
                EnableEvents(eventSource, EventLevel.Verbose, BinderKeyword);
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
            string GetDataString(string name) => GetData(name).ToString();

            lock (eventsLock)
            {
                switch (data.EventName)
                {
                    case "AssemblyBindStart":
                        Assert.True(!BindEvents.ContainsKey(data.ActivityId), "AssemblyBindStart should not exist for same activity ID ");
                        var bindEvent = new BindEvent()
                        {
                            AssemblyName = new AssemblyName(GetDataString("AssemblyName")),
                            ActivityId = data.ActivityId,
                            ParentActivityId = data.RelatedActivityId,
                            Nested = BindEvents.ContainsKey(data.RelatedActivityId)
                        };
                        BindEvents.Add(data.ActivityId, bindEvent);
                        break;
                    case "AssemblyBindStop":
                        Assert.True(BindEvents.ContainsKey(data.ActivityId), "AssemblyBindStop should have a matching AssemblyBindStart");
                        BindEvents[data.ActivityId].Success = (bool)GetData("Success");
                        BindEvents[data.ActivityId].Completed = true;
                        break;
                }
            }
        }
    }
}
