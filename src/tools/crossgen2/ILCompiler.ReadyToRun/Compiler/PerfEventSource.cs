// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;

/// <summary>
/// Performance events specific to ReadyToRun.
/// </summary>
namespace ILCompiler
{
    // The event IDs here must not collide with the ones used by DependencyAnalysis' PerfEventSource
    public struct StartStopEvents : IDisposable
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

            private PerfEventSource() { }

            public StartStopEvents LoadingEvents()
            {
                return new StartStopEvents(LoadingStart, LoadingStop);
            }

            public StartStopEvents EmittingEvents()
            {
                return new StartStopEvents(EmittingStart, EmittingStop);
            }

            public StartStopEvents CompilationEvents()
            {
                return new StartStopEvents(CompilationStart, CompilationStop);
            }

            public StartStopEvents JitEvents()
            {
                return new StartStopEvents(JitStart, JitStop);
            }

            public StartStopEvents JitMethodEvents()
            {
                return new StartStopEvents(JitMethodStart, JitMethodStop);
            }

            [Event(1, Level = EventLevel.Informational)]
            private void LoadingStart() { WriteEvent(1); }
            [Event(2, Level = EventLevel.Informational)]
            private void LoadingStop() { WriteEvent(2); }

            [Event(3, Level = EventLevel.Informational)]
            private void EmittingStart() { WriteEvent(3); }
            [Event(4, Level = EventLevel.Informational)]
            private void EmittingStop() { WriteEvent(4); }

            [Event(5, Level = EventLevel.Informational)]
            private void CompilationStart() { WriteEvent(5); }
            [Event(6, Level = EventLevel.Informational)]
            private void CompilationStop() { WriteEvent(6); }

            [Event(7, Level = EventLevel.Informational)]
            private void JitStart() { WriteEvent(7); }
            [Event(8, Level = EventLevel.Informational)]
            private void JitStop() { WriteEvent(8); }

            [Event(9, Level = EventLevel.Informational)]
            private void JitMethodStart() { WriteEvent(9); }
            [Event(10, Level = EventLevel.Informational)]
            private void JitMethodStop() { WriteEvent(10); }
        }
    }
}
