// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
    public readonly partial struct DateTime
    {
        private unsafe delegate void GetSystemTimeAsFileTimeDelegate(long* lpSystemTimeAsFileTime);

        internal static readonly bool s_systemSupportsLeapSeconds = SystemSupportsLeapSeconds();
        internal static readonly bool s_systemSupportsPreciseSystemTime = SystemSupportsPreciseSystemTime();
        private static GetSystemTimeAsFileTimeDelegate _getSystemTimeAsFileTime;

        public static unsafe DateTime UtcNow
        {
            get
            {
                long timestamp;

                if (s_systemSupportsPreciseSystemTime)
                {
                    Interop.Kernel32.GetSystemTimePreciseAsFileTime(&timestamp);
                }
                else
                {
                    Interop.Kernel32.GetSystemTimeAsFileTime(&timestamp);
                }

                if (s_systemSupportsLeapSeconds)
                {
                    GetSystemTimeWithLeapSecondsHandling(timestamp, out FullSystemTime time);
                    return CreateDateTimeFromSystemTime(in time);
                }

                return new DateTime(((ulong)(timestamp + FileTimeOffset)) | KindUtc);
            }
        }

        internal static bool IsValidTimeWithLeapSeconds(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            DateTime dt = new DateTime(year, month, day);
            FullSystemTime time = new FullSystemTime(year, month, dt.DayOfWeek, day, hour, minute, second);

            switch (kind)
            {
                case DateTimeKind.Local: return ValidateSystemTime(in time.systemTime, localTime: true);
                case DateTimeKind.Utc:   return ValidateSystemTime(in time.systemTime, localTime: false);
                default:
                    return ValidateSystemTime(in time.systemTime, localTime: true) || ValidateSystemTime(in time.systemTime, localTime: false);
            }
        }

        internal static DateTime FromFileTimeLeapSecondsAware(long fileTime)
        {
            if (FileTimeToSystemTime(fileTime, out FullSystemTime time))
            {
                return CreateDateTimeFromSystemTime(in time);
            }

            throw new ArgumentOutOfRangeException("fileTime", SR.ArgumentOutOfRange_DateTimeBadTicks);
        }

        internal static long ToFileTimeLeapSecondsAware(long ticks)
        {
            FullSystemTime time = new FullSystemTime(ticks);
            if (Interop.Kernel32.SystemTimeToFileTime(in time.systemTime, out long fileTime))
            {
                return fileTime + ticks % TicksPerMillisecond;
            }

            throw new ArgumentOutOfRangeException(null, SR.ArgumentOutOfRange_FileTimeInvalid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DateTime CreateDateTimeFromSystemTime(in FullSystemTime time)
        {
            long ticks  = DateToTicks(time.systemTime.Year, time.systemTime.Month, time.systemTime.Day);
            ticks += TimeToTicks(time.systemTime.Hour, time.systemTime.Minute, time.systemTime.Second);
            ticks += time.systemTime.Milliseconds * TicksPerMillisecond;
            ticks += time.hundredNanoSecond;
            return new DateTime( ((UInt64)(ticks)) | KindUtc);
        }

        private static unsafe bool SystemSupportsLeapSeconds()
        {
            Interop.NtDll.SYSTEM_LEAP_SECOND_INFORMATION slsi;

            return Interop.NtDll.NtQuerySystemInformation(
                                    Interop.NtDll.SystemLeapSecondInformation,
                                    (void *) &slsi,
                                    sizeof(Interop.NtDll.SYSTEM_LEAP_SECOND_INFORMATION),
                                    null) == 0 && slsi.Enabled;
        }

        private static unsafe bool ValidateSystemTime(in Interop.Kernel32.SYSTEMTIME time, bool localTime)
        {
            if (localTime)
            {
                return Interop.Kernel32.TzSpecificLocalTimeToSystemTime(IntPtr.Zero, in time, out Interop.Kernel32.SYSTEMTIME st);
            }
            else
            {
                return Interop.Kernel32.SystemTimeToFileTime(in time, out long timestamp);
            }
        }

        private static bool FileTimeToSystemTime(long fileTime, out FullSystemTime time)
        {
            time = new FullSystemTime();
            if (Interop.Kernel32.FileTimeToSystemTime(in fileTime, out time.systemTime))
            {
                // to keep the time precision
                time.hundredNanoSecond = fileTime % TicksPerMillisecond;
                if (time.systemTime.Second > 59)
                {
                    // we have a leap second, force it to last second in the minute as DateTime doesn't account for leap seconds in its calculation.
                    // we use the maxvalue from the milliseconds and the 100-nano seconds to avoid reporting two out of order 59 seconds
                    time.systemTime.Second = 59;
                    time.systemTime.Milliseconds = 999;
                    time.hundredNanoSecond = 9999;
                }
                return true;
            }
            return false;
        }

        private static unsafe void GetSystemTimeWithLeapSecondsHandling(long timestamp, out FullSystemTime time)
        {
            if (!FileTimeToSystemTime(timestamp, out time))
            {
                Interop.Kernel32.GetSystemTime(ref time.systemTime);
                time.hundredNanoSecond = 0;
                if (time.systemTime.Second > 59)
                {
                    // we have a leap second, force it to last second in the minute as DateTime doesn't account for leap seconds in its calculation.
                    // we use the maxvalue from the milliseconds and the 100-nano seconds to avoid reporting two out of order 59 seconds
                    time.systemTime.Second = 59;
                    time.systemTime.Milliseconds = 999;
                    time.hundredNanoSecond = 9999;
                }
            }
        }

        private static unsafe bool SystemSupportsPreciseSystemTime()
        {
            if (Environment.IsWindows8OrAbove)
            {
                // GetSystemTimePreciseAsFileTime exists and we'd like to use it.  However, on
                // misconfigured systems, it's possible for the "precise" time to be inaccurate:
                //     https://github.com/dotnet/coreclr/issues/14187
                // If it's inaccurate, though, we expect it to be wildly inaccurate, so as a
                // workaround/heuristic, we get both the "normal" and "precise" times, and as
                // long as they're close, we use the precise one. This workaround can be removed
                // when we better understand what's causing the drift and the issue is no longer
                // a problem or can be better worked around on all targeted OSes.

                long systemTimeResult;
                Interop.Kernel32.GetSystemTimeAsFileTime(&systemTimeResult);

                long preciseSystemTimeResult;
                Interop.Kernel32.GetSystemTimePreciseAsFileTime(&preciseSystemTimeResult);

                return Math.Abs(preciseSystemTimeResult - systemTimeResult) <= 100 * TicksPerMillisecond;
            }

            return false;
        }

        // FullSystemTime struct is the SYSTEMTIME struct with extra hundredNanoSecond field to store more precise time.
        [StructLayout(LayoutKind.Sequential)]
        internal struct FullSystemTime
        {
            internal Interop.Kernel32.SYSTEMTIME systemTime;
            internal long   hundredNanoSecond;

            internal FullSystemTime(int year, int month, DayOfWeek dayOfWeek, int day, int hour, int minute, int second)
            {
                systemTime.Year = (ushort) year;
                systemTime.Month = (ushort) month;
                systemTime.DayOfWeek = (ushort) dayOfWeek;
                systemTime.Day = (ushort) day;
                systemTime.Hour = (ushort) hour;
                systemTime.Minute = (ushort) minute;
                systemTime.Second = (ushort) second;
                systemTime.Milliseconds = 0;
                hundredNanoSecond = 0;
            }

            internal FullSystemTime(long ticks)
            {
                DateTime dt = new DateTime(ticks);

                int year, month, day;
                dt.GetDatePart(out year, out month, out day);

                systemTime.Year = (ushort) year;
                systemTime.Month = (ushort) month;
                systemTime.DayOfWeek = (ushort) dt.DayOfWeek;
                systemTime.Day = (ushort) day;
                systemTime.Hour = (ushort) dt.Hour;
                systemTime.Minute = (ushort) dt.Minute;
                systemTime.Second = (ushort) dt.Second;
                systemTime.Milliseconds = (ushort) dt.Millisecond;
                hundredNanoSecond = 0;
            }
        }
    }
}
