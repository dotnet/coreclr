// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace ILCompiler.DependencyAnalysisFramework
{
    [EventSource(Name = "Microsoft-ILCompiler-Perf")]
    public class PerfEventSource : EventSource
    {
        [Event(1, Level = EventLevel.Informational)]
        public void LoadingStart() { WriteEvent(1); }
        [Event(2, Level = EventLevel.Informational)]
        public void LoadingStop() { WriteEvent(2); }

        [Event(3, Level = EventLevel.Informational)]
        public void GraphProcessingStart() { WriteEvent(3); }
        [Event(4, Level = EventLevel.Informational)]
        public void GraphProcessingStop() { WriteEvent(4); }

        [Event(5, Level = EventLevel.Informational)]
        public void EmittingStart() { WriteEvent(5); }
        [Event(6, Level = EventLevel.Informational)]
        public void EmittingStop() { WriteEvent(6); }

        [Event(7, Level = EventLevel.Informational)]
        public void CompilationStart() { WriteEvent(7); }
        [Event(8, Level = EventLevel.Informational)]
        public void CompilationStop() { WriteEvent(8); }

        [Event(9, Level = EventLevel.Informational)]
        public void JitStart() { WriteEvent(9); }
        [Event(10, Level = EventLevel.Informational)]
        public void JitStop() { WriteEvent(10); }

        [Event(11, Level = EventLevel.Informational)]
        public void DependencyAnalysisStart() { WriteEvent(11); }
        [Event(12, Level = EventLevel.Informational)]
        public void DependencyAnalysisStop() { WriteEvent(12); }

        [Event(13, Level = EventLevel.Informational)]
        public void AddedNodeToMarkStack() { WriteEvent(13); }

        public static PerfEventSource Log = new PerfEventSource();
    }
}
