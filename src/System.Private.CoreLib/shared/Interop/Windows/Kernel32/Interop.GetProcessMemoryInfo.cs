// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
	// Used for retrieving working set information directly via pinvoke.
	[StructLayout(LayoutKind.Sequential, Size=72)]
	internal struct PROCESS_MEMORY_COUNTERS
	{
	    public uint cb;
	    public uint PageFaultCount;
	    public UInt64 PeakWorkingSetSize;
	    public UInt64 WorkingSetSize;
	    public UInt64 QuotaPeakPagedPoolUsage;
	    public UInt64 QuotaPagedPoolUsage;
	    public UInt64 QuotaPeakNonPagedPoolUsage;
	    public UInt64 QuotaNonPagedPoolUsage;
	    public UInt64 PagefileUsage;
	    public UInt64 PeakPagefileUsage;
	}

    internal partial class Kernel32
    {
        [DllImport("psapi.dll", SetLastError = true)]
        internal static extern bool GetProcessMemoryInfo(IntPtr handleProcess, out PROCESS_MEMORY_COUNTERS pmCounter, uint cb);
    }
}
