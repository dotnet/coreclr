// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32)]
        private extern static unsafe void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);

        [DllImport(Libraries.Kernel32)]
        private extern static unsafe void GetSystemTimePreciseAsFileTime(long* lpSystemTimeAsFileTime);

        private unsafe delegate void GetSystemTimeAsFileTimeFn(long* lpSystemTimeAsFileTime);

        private static Lazy<GetSystemTimeAsFileTimeFn> _getSystemTimeAsFileTime = new Lazy<GetSystemTimeAsFileTimeFn>(GetGetSystemTimeAsFileTimeFn);

        internal static unsafe long GetSystemTimeAsFileTime()
        {
            long ret;
            _getSystemTimeAsFileTime.Value(&ret);
            return ret;
        }

        private static unsafe GetSystemTimeAsFileTimeFn GetGetSystemTimeAsFileTimeFn()
        {
            long tmp;
            try
            {
                GetSystemTimePreciseAsFileTime(&tmp);
                return GetSystemTimePreciseAsFileTime;
            }
            catch (EntryPointNotFoundException)
            {
            }
            return GetSystemTimeAsFileTime;
        }
    }
}
