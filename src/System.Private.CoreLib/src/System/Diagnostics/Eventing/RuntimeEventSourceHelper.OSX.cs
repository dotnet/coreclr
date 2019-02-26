// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace System.Diagnostics.Tracing
{
    internal sealed class RuntimeEventSourceHelper
    {
        // from sys\resource.h
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct rusage_info_v3
        {
            internal fixed byte     ri_uuid[16];
            internal ulong          ri_user_time;
            internal ulong          ri_system_time;
            internal ulong          ri_pkg_idle_wkups;
            internal ulong          ri_interrupt_wkups;
            internal ulong          ri_pageins;
            internal ulong          ri_wired_size;
            internal ulong          ri_resident_size;
            internal ulong          ri_phys_footprint;
            internal ulong          ri_proc_start_abstime;
            internal ulong          ri_proc_exit_abstime;
            internal ulong          ri_child_user_time;
            internal ulong          ri_child_system_time;
            internal ulong          ri_child_pkg_idle_wkups;
            internal ulong          ri_child_interrupt_wkups;
            internal ulong          ri_child_pageins;
            internal ulong          ri_child_elapsed_abstime;
            internal ulong          ri_diskio_bytesread;
            internal ulong          ri_diskio_byteswritten;
            internal ulong          ri_cpu_time_qos_default;
            internal ulong          ri_cpu_time_qos_maintenance;
            internal ulong          ri_cpu_time_qos_background;
            internal ulong          ri_cpu_time_qos_utility;
            internal ulong          ri_cpu_time_qos_legacy;
            internal ulong          ri_cpu_time_qos_user_initiated;
            internal ulong          ri_cpu_time_qos_user_interactive;
            internal ulong          ri_billed_system_time;
            internal ulong          ri_serviced_system_time;
        }

        // Constant from sys\resource.h
        private const int RUSAGE_INFO_V3 = 3;
        private const string OSX_LIBPROC = "libproc";

        internal static long GetProcessTimes()
        {
            int pid = Environment.GetCurrentProcessId();
            rusage_info_v3 info = new rusage_info_v3();
            unsafe
            {
                int hr = proc_pid_rusage(pid, RUSAGE_INFO_V3, &info);
                if (hr < 0)
                {
                    return 0;
                }
            }
            return Convert.ToInt64(info.ri_system_time + info.ri_user_time);
        }

        /// <summary>
        /// Gets the rusage information for the process identified by the PID
        /// </summary>
        /// <param name="pid">The process to retrieve the rusage for</param>
        /// <param name="flavor">Specifies the type of struct that is passed in to <paramref>buffer</paramref>. Should be RUSAGE_INFO_V3 to specify a rusage_info_v3 struct.</param>
        /// <param name="buffer">A buffer to be filled with rusage_info data</param>
        /// <returns>Returns 0 on success; on fail, -1 and errno is set with the error code</returns>
        [DllImport(OSX_LIBPROC, SetLastError = true)]
        private static extern unsafe int proc_pid_rusage(
            int pid,
            int flavor,
            rusage_info_v3* buffer);
        
    }
}