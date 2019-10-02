// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;

/// <summary>
/// Performance events releated to the dependency graph.
/// </summary>
namespace ILCompiler.DependencyAnalysisFramework
{
    // The event IDs here must not collide with the ones used by ReadyToRunPerfEventSource.cs
    struct StartStopEvents : IDisposable
    {
        private Action _stopAction;
        public void Dispose()
        {
            _stopAction();
        }

        private StartStopEvents(Action startAction, Action stopAction)
        {
            startAction();
            _stopAction = stopAction;
        }

        [EventSource(Name = "Microsoft-ILCompiler-Perf")]
        public class PerfEventSource : EventSource
        {
            public static PerfEventSource Log = new PerfEventSource();

            private PerfEventSource() {}

            public StartStopEvents GraphProcessingEvents()
            {
                return new StartStopEvents(GraphProcessingStart, GraphProcessingStop);
            }

            public StartStopEvents DependencyAnalysisEvents()
            {
                return new StartStopEvents(DependencyAnalysisStart, DependencyAnalysisStop);
            }

            [Event(1001, Level = EventLevel.Informational)]
            private void GraphProcessingStart() { WriteEvent(1001); }
            [Event(1002, Level = EventLevel.Informational)]
            private void GraphProcessingStop() { WriteEvent(1002); }

            [Event(1003, Level = EventLevel.Informational)]
            private void DependencyAnalysisStart() { WriteEvent(1003); }
            [Event(1004, Level = EventLevel.Informational)]
            private void DependencyAnalysisStop() { WriteEvent(1004); }

            [Event(1005, Level = EventLevel.Informational)]
            public void AddedNodeToMarkStack() { WriteEvent(1005); }
        }
    }
}
