// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
	internal partial class Kernel32
	{
		internal const int TIME_ZONE_ID_INVALID = -1;
        internal const int MUI_PREFERRED_UI_LANGUAGES = 0x10;

        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal extern static int GetTimeZoneInformation(out TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport(Libraries.Kernel32, SetLastError = true)]
		internal extern static int GetDynamicTimeZoneInformation(out TIME_DYNAMIC_ZONE_INFORMATION lpDynamicTimeZoneInformation);

        [StructLayout(LayoutKind.Sequential)]
		internal struct SystemTime
		{
			[MarshalAs(UnmanagedType.U2)]
			public short Year;
			[MarshalAs(UnmanagedType.U2)]
			public short Month;
			[MarshalAs(UnmanagedType.U2)]
			public short DayOfWeek;
			[MarshalAs(UnmanagedType.U2)]
			public short Day;
			[MarshalAs(UnmanagedType.U2)]
			public short Hour;
			[MarshalAs(UnmanagedType.U2)]
			public short Minute;
			[MarshalAs(UnmanagedType.U2)]
			public short Second;
			[MarshalAs(UnmanagedType.U2)]
			public short Milliseconds;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TIME_DYNAMIC_ZONE_INFORMATION
		{
			[MarshalAs(UnmanagedType.I4)]
			public int Bias;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string StandardName;
			public SystemTime StandardDate;
			[MarshalAs(UnmanagedType.I4)]
			public int StandardBias;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DaylightName;
			public SystemTime DaylightDate;
			[MarshalAs(UnmanagedType.I4)]
			public int DaylightBias;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string TimeZoneKeyName;
			[MarshalAs(UnmanagedType.Bool)]
			public bool DynamicDaylightTimeDisabled;
		}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;
            public SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;
            public SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;

            public TIME_ZONE_INFORMATION(TIME_DYNAMIC_ZONE_INFORMATION dtzi)
            {
                Bias = dtzi.Bias;
                StandardName = dtzi.StandardName;
                StandardDate = dtzi.StandardDate;
                StandardBias = dtzi.StandardBias;
                DaylightName = dtzi.DaylightName;
                DaylightDate = dtzi.DaylightDate;
                DaylightBias = dtzi.DaylightBias;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct REGISTRY_TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
            public SystemTime StandardDate;
            public SystemTime DaylightDate;

            public REGISTRY_TIME_ZONE_INFORMATION(TIME_ZONE_INFORMATION tzi)
            {
                Bias = tzi.Bias;
                StandardDate = tzi.StandardDate;
                StandardBias = tzi.StandardBias;
                DaylightDate = tzi.DaylightDate;
                DaylightBias = tzi.DaylightBias;
            }

            public REGISTRY_TIME_ZONE_INFORMATION(byte[] bytes)
            {
                //
                // typedef struct _REG_TZI_FORMAT {
                // [00-03]    LONG Bias;
                // [04-07]    LONG StandardBias;
                // [08-11]    LONG DaylightBias;
                // [12-27]    SYSTEMTIME StandardDate;
                // [12-13]        WORD wYear;
                // [14-15]        WORD wMonth;
                // [16-17]        WORD wDayOfWeek;
                // [18-19]        WORD wDay;
                // [20-21]        WORD wHour;
                // [22-23]        WORD wMinute;
                // [24-25]        WORD wSecond;
                // [26-27]        WORD wMilliseconds;
                // [28-43]    SYSTEMTIME DaylightDate;
                // [28-29]        WORD wYear;
                // [30-31]        WORD wMonth;
                // [32-33]        WORD wDayOfWeek;
                // [34-35]        WORD wDay;
                // [36-37]        WORD wHour;
                // [38-39]        WORD wMinute;
                // [40-41]        WORD wSecond;
                // [42-43]        WORD wMilliseconds;
                // } REG_TZI_FORMAT;
                //
                if (bytes == null || bytes.Length != 44)
                {
                    throw new ArgumentException(SR.Argument_InvalidREG_TZI_FORMAT, nameof(bytes));
                }
                Bias = BitConverter.ToInt32(bytes, 0);
                StandardBias = BitConverter.ToInt32(bytes, 4);
                DaylightBias = BitConverter.ToInt32(bytes, 8);

                StandardDate.Year = BitConverter.ToInt16(bytes, 12);
                StandardDate.Month = BitConverter.ToInt16(bytes, 14);
                StandardDate.DayOfWeek = BitConverter.ToInt16(bytes, 16);
                StandardDate.Day = BitConverter.ToInt16(bytes, 18);
                StandardDate.Hour = BitConverter.ToInt16(bytes, 20);
                StandardDate.Minute = BitConverter.ToInt16(bytes, 22);
                StandardDate.Second = BitConverter.ToInt16(bytes, 24);
                StandardDate.Milliseconds = BitConverter.ToInt16(bytes, 26);

                DaylightDate.Year = BitConverter.ToInt16(bytes, 28);
                DaylightDate.Month = BitConverter.ToInt16(bytes, 30);
                DaylightDate.DayOfWeek = BitConverter.ToInt16(bytes, 32);
                DaylightDate.Day = BitConverter.ToInt16(bytes, 34);
                DaylightDate.Hour = BitConverter.ToInt16(bytes, 36);
                DaylightDate.Minute = BitConverter.ToInt16(bytes, 38);
                DaylightDate.Second = BitConverter.ToInt16(bytes, 40);
                DaylightDate.Milliseconds = BitConverter.ToInt16(bytes, 42);
            }
        }
    }
}
