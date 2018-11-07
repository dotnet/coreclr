// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    public readonly partial struct DateTime
    {
        internal static readonly bool s_isLeapSecondsSupportedSystem = IsLeapSecondsSupportedSystem();

        public static DateTime UtcNow
        {
            get
            {
                // following code is tuned for speed. Don't change it without running benchmark.
                long ticks = 0;

                if (s_isLeapSecondsSupportedSystem)
                {
                    FullSystemTime time = new FullSystemTime();
                    GetSystemTimeWithLeapSecondsHandling(ref time);
                    return CreateDateTimeFromSystemTime(ref time);
                }

                ticks = GetSystemTimeAsFileTime();
                return new DateTime(((ulong)(ticks + FileTimeOffset)) | KindUtc);
            }
        }

        public static DateTime FromFileTimeUtc(long fileTime)
        {
            if (fileTime < 0 || fileTime > MaxTicks - FileTimeOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(fileTime), SR.ArgumentOutOfRange_FileTimeInvalid);
            }

            if (s_isLeapSecondsSupportedSystem)
            {
                return InternalFromFileTime(fileTime);
            }

            // This is the ticks in Universal time for this fileTime.
            long universalTicks = fileTime + FileTimeOffset;
            return new DateTime(universalTicks, DateTimeKind.Utc);
        }

        public long ToFileTimeUtc() {
            // Treats the input as universal if it is not specified
            long ticks = ((InternalKind & LocalMask) != 0) ? ToUniversalTime().InternalTicks : this.InternalTicks;

            if (s_isLeapSecondsSupportedSystem)
            {
                return InternalToFileTime(ticks);
            }

            ticks -= FileTimeOffset;
            if (ticks < 0) {
                throw new ArgumentOutOfRangeException(null, SR.ArgumentOutOfRange_FileTimeInvalid);
            }

            return ticks;
        }

        internal static bool IsValidTimeWithLeapSeconds(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            DateTime dt = new DateTime(year, month, day);
            FullSystemTime time = new FullSystemTime(year, month, dt.DayOfWeek, day, hour, minute, second);

            switch (kind)
            {
                case DateTimeKind.Local: return ValidateSystemTime(ref time, localTime: true);
                case DateTimeKind.Utc:   return ValidateSystemTime(ref time, localTime: false);
                default:
                    return ValidateSystemTime(ref time, localTime: true) || ValidateSystemTime(ref time, localTime: false);
            }
        }

        internal static DateTime InternalFromFileTime(long fileTime)
        {
            FullSystemTime time = new FullSystemTime();
            if (SystemFileTimeToSystemTime(fileTime, ref time))
            {
                time.hundredNanoSecond = fileTime % TicksPerMillisecond;
                return CreateDateTimeFromSystemTime(ref time);
            }

            throw new ArgumentOutOfRangeException("fileTime", SR.ArgumentOutOfRange_DateTimeBadTicks);
        }

        internal static long InternalToFileTime(long ticks)
        {
            long fileTime = 0;
            FullSystemTime time = new FullSystemTime(ticks);
            if (SystemTimeToSystemFileTime(ref time, ref fileTime))
            {
                return fileTime + ticks % TicksPerMillisecond;
            }

            throw new ArgumentOutOfRangeException(null, SR.ArgumentOutOfRange_FileTimeInvalid);
        }

        // Just in case for any reason CreateDateTimeFromSystemTime not get inlined,
        // we are passing time by ref to avoid copying the structure while calling the method.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DateTime CreateDateTimeFromSystemTime(ref FullSystemTime time)
        {
            long ticks  = DateToTicks(time.wYear, time.wMonth, time.wDay);
            ticks += TimeToTicks(time.wHour, time.wMinute, time.wSecond);
            ticks += time.wMillisecond * TicksPerMillisecond;
            ticks += time.hundredNanoSecond;
            return new DateTime( ((UInt64)(ticks)) | KindUtc);
        }

        // FullSystemTime struct matches Windows SYSTEMTIME struct, except we added the extra nanoSeconds field to store
        // more precise time.
        [StructLayout(LayoutKind.Sequential)]
        internal struct FullSystemTime
        {
            internal FullSystemTime(int year, int month, DayOfWeek dayOfWeek, int day, int hour, int minute, int second)
            {
                wYear = (ushort) year;
                wMonth = (ushort) month;
                wDayOfWeek = (ushort) dayOfWeek;
                wDay = (ushort) day;
                wHour = (ushort) hour;
                wMinute = (ushort) minute;
                wSecond = (ushort) second;
                wMillisecond = 0;
                hundredNanoSecond = 0;
            }

            internal FullSystemTime(long ticks)
            {
                DateTime dt = new DateTime(ticks);

                int year, month, day;
                dt.GetDatePart(out year, out month, out day);

                wYear = (ushort) year;
                wMonth = (ushort) month;
                wDayOfWeek = (ushort) dt.DayOfWeek;
                wDay = (ushort) day;
                wHour = (ushort) dt.Hour;
                wMinute = (ushort) dt.Minute;
                wSecond = (ushort) dt.Second;
                wMillisecond = (ushort) dt.Millisecond;
                hundredNanoSecond = 0;
            }

            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMillisecond;
            internal long   hundredNanoSecond;
        };

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ValidateSystemTime(ref FullSystemTime time, bool localTime);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern bool IsLeapSecondsSupportedSystem();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool SystemFileTimeToSystemTime(long fileTime, ref FullSystemTime time);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GetSystemTimeWithLeapSecondsHandling(ref FullSystemTime time);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool SystemTimeToSystemFileTime(ref FullSystemTime time, ref long fileTime);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern long GetSystemTimeAsFileTime();
    }
}
