// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime
{
    public static partial class GCSettings
    {
        public static bool IsServerGC =>
            GC.IsServerGC();

        private static GCLatencyMode GetLatencyMode() =>
            (GCLatencyMode)GC.GetGCLatencyMode();

        private static SetLatencyModeStatus SetLatencyMode(GCLatencyMode value) =>
            (SetLatencyModeStatus)GC.SetGCLatencyMode((int)value);

        private static GCLargeObjectHeapCompactionMode GetLOHCompactionMode() =>
            (GCLargeObjectHeapCompactionMode)GC.GetLOHCompactionMode();

        private static void SetLOHCompactionMode(GCLargeObjectHeapCompactionMode value) =>
            GC.SetLOHCompactionMode((int)value);
    }
}
