// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    internal static class HighPerformanceCounter
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern ulong QueryPerformanceFrequency();

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern ulong QueryPerformanceCounter();

        /// <summary>
        /// Current tick count.
        /// </summary>
        /// <remarks>
        /// Hardware used varies: (TSC, HPET, ACPI, proprietary timers)
        /// On fairly new systems the API should be cheap, monotonic and synchronized.
        /// In some fairly old cases, around Windows XP era, could be not synchronized between cores.
        /// Could have relatively high latency on Windows 7 era environments.
        /// </remarks>
        public static ulong TickCount
        {
            get
            {
                return QueryPerformanceCounter();
            }
        }

        /// <summary>
        /// Ticks per second.
        /// </summary>
        /// <remarks>
        /// It is a process-wide constant.
        /// </remarks>
        public static ulong Frequency { get; } = QueryPerformanceFrequency();
    }
}
