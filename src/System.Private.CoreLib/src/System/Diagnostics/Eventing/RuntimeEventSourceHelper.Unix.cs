// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.IO;


namespace System.Diagnostics.Tracing
{
    internal sealed class RuntimeEventSourceHelper
    {
    	internal static Interop.Sys.ProcessCpuInformation cpuInfo = new Interop.Sys.ProcessCpuInformation();

        internal static long GetProcessTimes()
        {
            return (long)Interop.Sys.GetCpuUtilization(ref cpuInfo);
        }
    }
}