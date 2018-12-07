// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    public readonly partial struct DateTime
    {
        internal const bool s_systemSupportsLeapSeconds = false;

        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(((ulong)(GetSystemTimeAsFileTime() + FileTimeOffset)) | KindUtc);
            }
        }

        public static DateTime FromFileTimeUtc(long fileTime)
        {
            if (fileTime < 0 || fileTime > MaxTicks - FileTimeOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(fileTime), SR.ArgumentOutOfRange_FileTimeInvalid);
            }

            // This is the ticks in Universal time for this fileTime.
            long universalTicks = fileTime + FileTimeOffset;
            return new DateTime(universalTicks, DateTimeKind.Utc);
        }
        public long ToFileTimeUtc()
        {
            // Treats the input as universal if it is not specified
            long ticks = ((InternalKind & LocalMask) != 0) ? ToUniversalTime().InternalTicks : this.InternalTicks;
            ticks -= FileTimeOffset;
            if (ticks < 0)
            {
                throw new ArgumentOutOfRangeException(null, SR.ArgumentOutOfRange_FileTimeInvalid);
            }
            return ticks;
        }

        // IsValidTimeWithLeapSeconds is not expected to be called at all for now on non-Windows platforms
        internal static bool IsValidTimeWithLeapSeconds(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind) => false;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern long GetSystemTimeAsFileTime();
    }
}
