// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;


namespace System.Diagnostics.Tracing
{
	internal sealed class RuntimeEventSourceHelper
	{
		internal static long prevProcUserTime = 0;
		internal static long prevProcKernelTime = 0;
		internal static long prevSystemUserTime = 0;
		internal static long prevSystemKernelTime = 0;

		internal static int GetCpuUsage()
		{
			// Returns the current process' CPU usage as a percentage
			long procUserTime;
			long procKernelTime;
			long systemUserTime;
			long systemKernelTime;

			int cpuUsage;
			
			Interop.Kernel32.GetProcessTimes(Interop.Kernel32.GetCurrentProcess(), out _, out _, out procKernelTime, out procUserTime);
			Interop.Kernel32.GetSystemTimes(out _, out systemUserTime, out systemKernelTime);

			if (prevSystemUserTime == 0 && prevSystemKernelTime == 0) // These may be 0 when we report CPU usage for the first time, in which case we should just return 0. 
			{
				cpuUsage = 0;
			}

			else
			{
				long totalProcTime = (procUserTime - prevProcUserTime) + (procKernelTime - prevProcKernelTime);
				long totalSystemTime = (systemUserTime - prevSystemUserTime) + (systemKernelTime - prevSystemKernelTime);
				cpuUsage = (int)(totalProcTime * 100 / totalSystemTime);
			}

			prevProcUserTime = procUserTime;
			prevProcKernelTime = procKernelTime;
			prevSystemUserTime = systemUserTime;
			prevSystemKernelTime = systemKernelTime;

			Debug.Assert(cpuUsage >= 0 && cpuUsage <= 100);
			return cpuUsage;
		}
	}
}