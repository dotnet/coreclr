// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    public readonly partial struct DateTime
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool ValidateSystemTime(in Interop.Kernel32.SYSTEMTIME time, bool localTime);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool FileTimeToSystemTime(long fileTime, out FullSystemTime time);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void GetSystemTimeWithLeapSecondsHandling(out FullSystemTime time);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool SystemTimeToFileTime(in Interop.Kernel32.SYSTEMTIME time, out long fileTime);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern long GetSystemTimeAsFileTime();
    }
}
