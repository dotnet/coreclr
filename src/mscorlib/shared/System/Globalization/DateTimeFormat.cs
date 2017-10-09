// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System
{
    /*  
     Customized format patterns:
     P.S. Format in the table below is the internal number format used to display the pattern.

     Patterns   Format      Description                           Example
     =========  ==========  ===================================== ========
        "h"     "0"         hour (12-hour clock)w/o leading zero  3
        "hh"    "00"        hour (12-hour clock)with leading zero 03
        "hh*"   "00"        hour (12-hour clock)with leading zero 03

        "H"     "0"         hour (24-hour clock)w/o leading zero  8
        "HH"    "00"        hour (24-hour clock)with leading zero 08
        "HH*"   "00"        hour (24-hour clock)                  08

        "m"     "0"         minute w/o leading zero
        "mm"    "00"        minute with leading zero
        "mm*"   "00"        minute with leading zero

        "s"     "0"         second w/o leading zero
        "ss"    "00"        second with leading zero
        "ss*"   "00"        second with leading zero

        "f"     "0"         second fraction (1 digit)
        "ff"    "00"        second fraction (2 digit)
        "fff"   "000"       second fraction (3 digit)
        "ffff"  "0000"      second fraction (4 digit)
        "fffff" "00000"         second fraction (5 digit)
        "ffffff"    "000000"    second fraction (6 digit)
        "fffffff"   "0000000"   second fraction (7 digit)

        "F"     "0"         second fraction (up to 1 digit)
        "FF"    "00"        second fraction (up to 2 digit)
        "FFF"   "000"       second fraction (up to 3 digit)
        "FFFF"  "0000"      second fraction (up to 4 digit)
        "FFFFF" "00000"         second fraction (up to 5 digit)
        "FFFFFF"    "000000"    second fraction (up to 6 digit)
        "FFFFFFF"   "0000000"   second fraction (up to 7 digit)

        "t"                 first character of AM/PM designator   A
        "tt"                AM/PM designator                      AM
        "tt*"               AM/PM designator                      PM

        "d"     "0"         day w/o leading zero                  1
        "dd"    "00"        day with leading zero                 01
        "ddd"               short weekday name (abbreviation)     Mon
        "dddd"              full weekday name                     Monday
        "dddd*"             full weekday name                     Monday
        

        "M"     "0"         month w/o leading zero                2
        "MM"    "00"        month with leading zero               02
        "MMM"               short month name (abbreviation)       Feb
        "MMMM"              full month name                       Febuary
        "MMMM*"             full month name                       Febuary
       
        "y"     "0"         two digit year (year % 100) w/o leading zero           0
        "yy"    "00"        two digit year (year % 100) with leading zero          00
        "yyy"   "D3"        year                                  2000
        "yyyy"  "D4"        year                                  2000
        "yyyyy" "D5"        year                                  2000
        ...

        "z"     "+0;-0"     timezone offset w/o leading zero      -8
        "zz"    "+00;-00"   timezone offset with leading zero     -08
        "zzz"      "+00;-00" for hour offset, "00" for minute offset  full timezone offset   -07:30
        "zzz*"  "+00;-00" for hour offset, "00" for minute offset   full timezone offset   -08:00
        
        "K"    -Local       "zzz", e.g. -08:00
               -Utc         "'Z'", representing UTC
               -Unspecified ""               
               -DateTimeOffset      "zzzzz" e.g -07:30:15

        "g*"                the current era name                  A.D.

        ":"                 time separator                        : -- DEPRECATED - Insert separator directly into pattern (eg: "H.mm.ss")
        "/"                 date separator                        /-- DEPRECATED - Insert separator directly into pattern (eg: "M-dd-yyyy")
        "'"                 quoted string                         'ABC' will insert ABC into the formatted string.
        '"'                 quoted string                         "ABC" will insert ABC into the formatted string.
        "%"                 used to quote a single pattern characters      E.g.The format character "%y" is to print two digit year.
        "\"                 escaped character                     E.g. '\d' insert the character 'd' into the format string.
        other characters    insert the character into the format string. 

    Pre-defined format characters: 
        (U) to indicate Universal time is used.
        (G) to indicate Gregorian calendar is used.
    
        Format              Description                             Real format                             Example
        =========           =================================       ======================                  =======================
        "d"                 short date                              culture-specific                        10/31/1999
        "D"                 long data                               culture-specific                        Sunday, October 31, 1999
        "f"                 full date (long date + short time)      culture-specific                        Sunday, October 31, 1999 2:00 AM
        "F"                 full date (long date + long time)       culture-specific                        Sunday, October 31, 1999 2:00:00 AM
        "g"                 general date (short date + short time)  culture-specific                        10/31/1999 2:00 AM
        "G"                 general date (short date + long time)   culture-specific                        10/31/1999 2:00:00 AM
        "m"/"M"             Month/Day date                          culture-specific                        October 31
(G)     "o"/"O"             Round Trip XML                          "yyyy-MM-ddTHH:mm:ss.fffffffK"          1999-10-31 02:00:00.0000000Z
(G)     "r"/"R"             RFC 1123 date,                          "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"   Sun, 31 Oct 1999 10:00:00 GMT
(G)     "s"                 Sortable format, based on ISO 8601.     "yyyy-MM-dd'T'HH:mm:ss"                 1999-10-31T02:00:00
                                                                    ('T' for local time)
        "t"                 short time                              culture-specific                        2:00 AM
        "T"                 long time                               culture-specific                        2:00:00 AM
(G)     "u"                 Universal time with sortable format,    "yyyy'-'MM'-'dd HH':'mm':'ss'Z'"        1999-10-31 10:00:00Z
                            based on ISO 8601.
(U)     "U"                 Universal time with full                culture-specific                        Sunday, October 31, 1999 10:00:00 AM
                            (long date + long time) format
                            "y"/"Y"             Year/Month day                          culture-specific                        October, 1999

    */

    //This class contains only static members and does not require the serializable attribute.    
    internal static
    class DateTimeFormat
    {
        internal const int MaxSecondsFractionDigits = 7;
        internal static readonly TimeSpan NullOffset = TimeSpan.MinValue;

        internal static char[] allStandardFormats =
        {
            'd', 'D', 'f', 'F', 'g', 'G',
            'm', 'M', 'o', 'O', 'r', 'R',
            's', 't', 'T', 'u', 'U', 'y', 'Y',
        };

        internal const String RoundtripFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";
        internal const String RoundtripDateTimeUnfixed = "yyyy'-'MM'-'ddTHH':'mm':'ss zzz";

        private const int DEFAULT_ALL_DATETIMES_SIZE = 132;

        internal static readonly DateTimeFormatInfo InvariantFormatInfo = CultureInfo.InvariantCulture.DateTimeFormat;
        internal static readonly string[] InvariantAbbreviatedMonthNames = InvariantFormatInfo.AbbreviatedMonthNames;
        internal static readonly string[] InvariantAbbreviatedDayNames = InvariantFormatInfo.AbbreviatedDayNames;
        internal const string Gmt = "GMT";

        internal static String[] fixedNumberFormats = new String[] {
            "0",
            "00",
            "000",
            "0000",
            "00000",
            "000000",
            "0000000",
        };

        ////////////////////////////////////////////////////////////////////////////
        // 
        // Format the positive integer value to a string and perfix with assigned 
        // length of leading zero.
        //
        // Parameters:
        //  value: The value to format
        //  len: The maximum length for leading zero.  
        //  If the digits of the value is greater than len, no leading zero is added.
        //
        // Notes: 
        //  The function can format to Int32.MaxValue.
        //
        ////////////////////////////////////////////////////////////////////////////
        internal static void FormatDigits(StringBuilder outputBuffer, int value, int len)
        {
            Debug.Assert(value >= 0, "DateTimeFormat.FormatDigits(): value >= 0");
            FormatDigits(outputBuffer, value, len, false);
        }

        internal unsafe static void FormatDigits(StringBuilder outputBuffer, int value, int len, bool overrideLengthLimit)
        {
            Debug.Assert(value >= 0, "DateTimeFormat.FormatDigits(): value >= 0");

            // Limit the use of this function to be two-digits, so that we have the same behavior
            // as RTM bits.
            if (!overrideLengthLimit && len > 2)
            {
                len = 2;
            }

            char* buffer = stackalloc char[16];
            char* p = buffer + 16;
            int n = value;
            do
            {
                *--p = (char)(n % 10 + '0');
                n /= 10;
            } while ((n != 0) && (p > buffer));

            int digits = (int)(buffer + 16 - p);

            //If the repeat count is greater than 0, we're trying
            //to emulate the "00" format, so we have to prepend
            //a zero if the string only has one character.
            while ((digits < len) && (p > buffer))
            {
                *--p = '0';
                digits++;
            }
            outputBuffer.Append(p, digits);
        }

        private static void HebrewFormatDigits(StringBuilder outputBuffer, int digits)
        {
            outputBuffer.Append(HebrewNumber.ToString(digits));
        }

        internal static int ParseRepeatPattern(String format, int pos, char patternChar)
        {
            int len = format.Length;
            int index = pos + 1;
            while ((index < len) && (format[index] == patternChar))
            {
                index++;
            }
            return (index - pos);
        }

        private static String FormatDayOfWeek(int dayOfWeek, int repeat, DateTimeFormatInfo dtfi)
        {
            Debug.Assert(dayOfWeek >= 0 && dayOfWeek <= 6, "dayOfWeek >= 0 && dayOfWeek <= 6");
            if (repeat == 3)
            {
                return (dtfi.GetAbbreviatedDayName((DayOfWeek)dayOfWeek));
            }
            // Call dtfi.GetDayName() here, instead of accessing DayNames property, because we don't
            // want a clone of DayNames, which will hurt perf.
            return (dtfi.GetDayName((DayOfWeek)dayOfWeek));
        }

        private static String FormatMonth(int month, int repeatCount, DateTimeFormatInfo dtfi)
        {
            Debug.Assert(month >= 1 && month <= 12, "month >=1 && month <= 12");
            if (repeatCount == 3)
            {
                return (dtfi.GetAbbreviatedMonthName(month));
            }
            // Call GetMonthName() here, instead of accessing MonthNames property, because we don't
            // want a clone of MonthNames, which will hurt perf.
            return (dtfi.GetMonthName(month));
        }

        //
        //  FormatHebrewMonthName
        //
        //  Action: Return the Hebrew month name for the specified DateTime.
        //  Returns: The month name string for the specified DateTime.
        //  Arguments: 
        //        time   the time to format
        //        month  The month is the value of HebrewCalendar.GetMonth(time).         
        //        repeat Return abbreviated month name if repeat=3, or full month name if repeat=4
        //        dtfi    The DateTimeFormatInfo which uses the Hebrew calendars as its calendar.
        //  Exceptions: None.
        // 

        /* Note:
            If DTFI is using Hebrew calendar, GetMonthName()/GetAbbreviatedMonthName() will return month names like this:            
            1   Hebrew 1st Month
            2   Hebrew 2nd Month
            ..  ...
            6   Hebrew 6th Month
            7   Hebrew 6th Month II (used only in a leap year)
            8   Hebrew 7th Month
            9   Hebrew 8th Month
            10  Hebrew 9th Month
            11  Hebrew 10th Month
            12  Hebrew 11th Month
            13  Hebrew 12th Month

            Therefore, if we are in a regular year, we have to increment the month name if moth is greater or eqaul to 7.            
        */
        private static String FormatHebrewMonthName(DateTime time, int month, int repeatCount, DateTimeFormatInfo dtfi)
        {
            Debug.Assert(repeatCount != 3 || repeatCount != 4, "repeateCount should be 3 or 4");
            if (dtfi.Calendar.IsLeapYear(dtfi.Calendar.GetYear(time)))
            {
                // This month is in a leap year
                return (dtfi.internalGetMonthName(month, MonthNameStyles.LeapYear, (repeatCount == 3)));
            }
            // This is in a regular year.
            if (month >= 7)
            {
                month++;
            }
            if (repeatCount == 3)
            {
                return (dtfi.GetAbbreviatedMonthName(month));
            }
            return (dtfi.GetMonthName(month));
        }

        //
        // The pos should point to a quote character. This method will
        // append to the result StringBuilder the string encloed by the quote character.
        //
        internal static int ParseQuoteString(String format, int pos, StringBuilder result)
        {
            //
            // NOTE : pos will be the index of the quote character in the 'format' string.
            //
            int formatLen = format.Length;
            int beginPos = pos;
            char quoteChar = format[pos++]; // Get the character used to quote the following string.

            bool foundQuote = false;
            while (pos < formatLen)
            {
                char ch = format[pos++];
                if (ch == quoteChar)
                {
                    foundQuote = true;
                    break;
                }
                else if (ch == '\\')
                {
                    // The following are used to support escaped character.
                    // Escaped character is also supported in the quoted string.
                    // Therefore, someone can use a format like "'minute:' mm\"" to display:
                    //  minute: 45"
                    // because the second double quote is escaped.
                    if (pos < formatLen)
                    {
                        result.Append(format[pos++]);
                    }
                    else
                    {
                        //
                        // This means that '\' is at the end of the formatting string.
                        //
                        throw new FormatException(SR.Format_InvalidString);
                    }
                }
                else
                {
                    result.Append(ch);
                }
            }

            if (!foundQuote)
            {
                // Here we can't find the matching quote.
                throw new FormatException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            SR.Format_BadQuote, quoteChar));
            }

            //
            // Return the character count including the begin/end quote characters and enclosed string.
            //
            return (pos - beginPos);
        }

        //
        // Get the next character at the index of 'pos' in the 'format' string.
        // Return value of -1 means 'pos' is already at the end of the 'format' string.
        // Otherwise, return value is the int value of the next character.
        //
        internal static int ParseNextChar(String format, int pos)
        {
            if (pos >= format.Length - 1)
            {
                return (-1);
            }
            return ((int)format[pos + 1]);
        }

        //
        //  IsUseGenitiveForm
        //
        //  Actions: Check the format to see if we should use genitive month in the formatting.
        //      Starting at the position (index) in the (format) string, look back and look ahead to
        //      see if there is "d" or "dd".  In the case like "d MMMM" or "MMMM dd", we can use 
        //      genitive form.  Genitive form is not used if there is more than two "d".
        //  Arguments:
        //      format      The format string to be scanned.
        //      index       Where we should start the scanning.  This is generally where "M" starts.
        //      tokenLen    The len of the current pattern character.  This indicates how many "M" that we have.
        //      patternToMatch  The pattern that we want to search. This generally uses "d"
        //
        private static bool IsUseGenitiveForm(String format, int index, int tokenLen, char patternToMatch)
        {
            int i;
            int repeat = 0;
            //
            // Look back to see if we can find "d" or "ddd"
            //

            // Find first "d".
            for (i = index - 1; i >= 0 && format[i] != patternToMatch; i--) {  /*Do nothing here */ };

            if (i >= 0)
            {
                // Find a "d", so look back to see how many "d" that we can find.
                while (--i >= 0 && format[i] == patternToMatch)
                {
                    repeat++;
                }
                //
                // repeat == 0 means that we have one (patternToMatch)
                // repeat == 1 means that we have two (patternToMatch)
                //
                if (repeat <= 1)
                {
                    return (true);
                }
                // Note that we can't just stop here.  We may find "ddd" while looking back, and we have to look
                // ahead to see if there is "d" or "dd".
            }

            //
            // If we can't find "d" or "dd" by looking back, try look ahead.
            //

            // Find first "d"
            for (i = index + tokenLen; i < format.Length && format[i] != patternToMatch; i++) { /* Do nothing here */ };

            if (i < format.Length)
            {
                repeat = 0;
                // Find a "d", so contine the walk to see how may "d" that we can find.
                while (++i < format.Length && format[i] == patternToMatch)
                {
                    repeat++;
                }
                //
                // repeat == 0 means that we have one (patternToMatch)
                // repeat == 1 means that we have two (patternToMatch)
                //
                if (repeat <= 1)
                {
                    return (true);
                }
            }
            return (false);
        }


        //
        //  FormatCustomized
        //
        //  Actions: Format the DateTime instance using the specified format.
        // 
        private static StringBuilder FormatCustomized(DateTime dateTime, String format, DateTimeFormatInfo dtfi, TimeSpan offset)
        {
            Calendar cal = dtfi.Calendar;
            StringBuilder result = StringBuilderCache.Acquire();
            // This is a flag to indicate if we are format the dates using Hebrew calendar.

            bool isHebrewCalendar = (cal.ID == CalendarId.HEBREW);
            // This is a flag to indicate if we are formating hour/minute/second only.
            bool bTimeOnly = true;

            int i = 0;
            int tokenLen, hour12;

            while (i < format.Length)
            {
                char ch = format[i];
                int nextChar;
                switch (ch)
                {
                    case 'g':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        result.Append(dtfi.GetEraName(cal.GetEra(dateTime)));
                        break;
                    case 'h':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        hour12 = dateTime.Hour % 12;
                        if (hour12 == 0)
                        {
                            hour12 = 12;
                        }
                        FormatDigits(result, hour12, tokenLen);
                        break;
                    case 'H':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        FormatDigits(result, dateTime.Hour, tokenLen);
                        break;
                    case 'm':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        FormatDigits(result, dateTime.Minute, tokenLen);
                        break;
                    case 's':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        FormatDigits(result, dateTime.Second, tokenLen);
                        break;
                    case 'f':
                    case 'F':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen <= MaxSecondsFractionDigits)
                        {
                            long fraction = (dateTime.Ticks % Calendar.TicksPerSecond);
                            fraction = fraction / (long)Math.Pow(10, 7 - tokenLen);
                            if (ch == 'f')
                            {
                                result.Append(((int)fraction).ToString(fixedNumberFormats[tokenLen - 1], CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                int effectiveDigits = tokenLen;
                                while (effectiveDigits > 0)
                                {
                                    if (fraction % 10 == 0)
                                    {
                                        fraction = fraction / 10;
                                        effectiveDigits--;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (effectiveDigits > 0)
                                {
                                    result.Append(((int)fraction).ToString(fixedNumberFormats[effectiveDigits - 1], CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    // No fraction to emit, so see if we should remove decimal also.
                                    if (result.Length > 0 && result[result.Length - 1] == '.')
                                    {
                                        result.Remove(result.Length - 1, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            StringBuilderCache.Release(result);
                            throw new FormatException(SR.Format_InvalidString);
                        }
                        break;
                    case 't':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen == 1)
                        {
                            if (dateTime.Hour < 12)
                            {
                                if (dtfi.AMDesignator.Length >= 1)
                                {
                                    result.Append(dtfi.AMDesignator[0]);
                                }
                            }
                            else
                            {
                                if (dtfi.PMDesignator.Length >= 1)
                                {
                                    result.Append(dtfi.PMDesignator[0]);
                                }
                            }
                        }
                        else
                        {
                            result.Append((dateTime.Hour < 12 ? dtfi.AMDesignator : dtfi.PMDesignator));
                        }
                        break;
                    case 'd':
                        //
                        // tokenLen == 1 : Day of month as digits with no leading zero.
                        // tokenLen == 2 : Day of month as digits with leading zero for single-digit months.
                        // tokenLen == 3 : Day of week as a three-leter abbreviation.
                        // tokenLen >= 4 : Day of week as its full name.
                        //
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (tokenLen <= 2)
                        {
                            int day = cal.GetDayOfMonth(dateTime);
                            if (isHebrewCalendar)
                            {
                                // For Hebrew calendar, we need to convert numbers to Hebrew text for yyyy, MM, and dd values.
                                HebrewFormatDigits(result, day);
                            }
                            else
                            {
                                FormatDigits(result, day, tokenLen);
                            }
                        }
                        else
                        {
                            int dayOfWeek = (int)cal.GetDayOfWeek(dateTime);
                            result.Append(FormatDayOfWeek(dayOfWeek, tokenLen, dtfi));
                        }
                        bTimeOnly = false;
                        break;
                    case 'M':
                        // 
                        // tokenLen == 1 : Month as digits with no leading zero.
                        // tokenLen == 2 : Month as digits with leading zero for single-digit months.
                        // tokenLen == 3 : Month as a three-letter abbreviation.
                        // tokenLen >= 4 : Month as its full name.
                        //
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        int month = cal.GetMonth(dateTime);
                        if (tokenLen <= 2)
                        {
                            if (isHebrewCalendar)
                            {
                                // For Hebrew calendar, we need to convert numbers to Hebrew text for yyyy, MM, and dd values.
                                HebrewFormatDigits(result, month);
                            }
                            else
                            {
                                FormatDigits(result, month, tokenLen);
                            }
                        }
                        else
                        {
                            if (isHebrewCalendar)
                            {
                                result.Append(FormatHebrewMonthName(dateTime, month, tokenLen, dtfi));
                            }
                            else
                            {
                                if ((dtfi.FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != 0 && tokenLen >= 4)
                                {
                                    result.Append(
                                        dtfi.internalGetMonthName(
                                            month,
                                            IsUseGenitiveForm(format, i, tokenLen, 'd') ? MonthNameStyles.Genitive : MonthNameStyles.Regular,
                                            false));
                                }
                                else
                                {
                                    result.Append(FormatMonth(month, tokenLen, dtfi));
                                }
                            }
                        }
                        bTimeOnly = false;
                        break;
                    case 'y':
                        // Notes about OS behavior:
                        // y: Always print (year % 100). No leading zero.
                        // yy: Always print (year % 100) with leading zero.
                        // yyy/yyyy/yyyyy/... : Print year value.  No leading zero.

                        int year = cal.GetYear(dateTime);
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        if (dtfi.HasForceTwoDigitYears)
                        {
                            FormatDigits(result, year, tokenLen <= 2 ? tokenLen : 2);
                        }
                        else if (cal.ID == CalendarId.HEBREW)
                        {
                            HebrewFormatDigits(result, year);
                        }
                        else
                        {
                            if (tokenLen <= 2)
                            {
                                FormatDigits(result, year % 100, tokenLen);
                            }
                            else
                            {
                                String fmtPattern = "D" + tokenLen.ToString();
                                result.Append(year.ToString(fmtPattern, CultureInfo.InvariantCulture));
                            }
                        }
                        bTimeOnly = false;
                        break;
                    case 'z':
                        tokenLen = ParseRepeatPattern(format, i, ch);
                        FormatCustomizedTimeZone(dateTime, offset, format, tokenLen, bTimeOnly, result);
                        break;
                    case 'K':
                        tokenLen = 1;
                        FormatCustomizedRoundripTimeZone(dateTime, offset, result);
                        break;
                    case ':':
                        result.Append(dtfi.TimeSeparator);
                        tokenLen = 1;
                        break;
                    case '/':
                        result.Append(dtfi.DateSeparator);
                        tokenLen = 1;
                        break;
                    case '\'':
                    case '\"':
                        tokenLen = ParseQuoteString(format, i, result);
                        break;
                    case '%':
                        // Optional format character.
                        // For example, format string "%d" will print day of month 
                        // without leading zero.  Most of the cases, "%" can be ignored.
                        nextChar = ParseNextChar(format, i);
                        // nextChar will be -1 if we already reach the end of the format string.
                        // Besides, we will not allow "%%" appear in the pattern.
                        if (nextChar >= 0 && nextChar != (int)'%')
                        {
                            result.Append(FormatCustomized(dateTime, ((char)nextChar).ToString(), dtfi, offset));
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '%' is at the end of the format string or
                            // "%%" appears in the format string.
                            //
                            StringBuilderCache.Release(result);
                            throw new FormatException(SR.Format_InvalidString);
                        }
                        break;
                    case '\\':
                        // Escaped character.  Can be used to insert character into the format string.
                        // For exmple, "\d" will insert the character 'd' into the string.
                        //
                        // NOTENOTE : we can remove this format character if we enforce the enforced quote 
                        // character rule.
                        // That is, we ask everyone to use single quote or double quote to insert characters,
                        // then we can remove this character.
                        //
                        nextChar = ParseNextChar(format, i);
                        if (nextChar >= 0)
                        {
                            result.Append(((char)nextChar));
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '\' is at the end of the formatting string.
                            //
                            StringBuilderCache.Release(result);
                            throw new FormatException(SR.Format_InvalidString);
                        }
                        break;
                    default:
                        // NOTENOTE : we can remove this rule if we enforce the enforced quote
                        // character rule.
                        // That is, if we ask everyone to use single quote or double quote to insert characters,
                        // then we can remove this default block.
                        result.Append(ch);
                        tokenLen = 1;
                        break;
                }
                i += tokenLen;
            }
            return result;
        }


        // output the 'z' famliy of formats, which output a the offset from UTC, e.g. "-07:30"
        private static void FormatCustomizedTimeZone(DateTime dateTime, TimeSpan offset, String format, Int32 tokenLen, Boolean timeOnly, StringBuilder result)
        {
            // See if the instance already has an offset
            Boolean dateTimeFormat = (offset == NullOffset);
            if (dateTimeFormat)
            {
                // No offset. The instance is a DateTime and the output should be the local time zone

                if (timeOnly && dateTime.Ticks < Calendar.TicksPerDay)
                {
                    // For time only format and a time only input, the time offset on 0001/01/01 is less 
                    // accurate than the system's current offset because of daylight saving time.
                    offset = TimeZoneInfo.GetLocalUtcOffset(DateTime.Now, TimeZoneInfoOptions.NoThrowOnInvalidTime);
                }
                else if (dateTime.Kind == DateTimeKind.Utc)
                {
                    offset = TimeSpan.Zero;
                }
                else
                {
                    offset = TimeZoneInfo.GetLocalUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
                }
            }
            if (offset >= TimeSpan.Zero)
            {
                result.Append('+');
            }
            else
            {
                result.Append('-');
                // get a positive offset, so that you don't need a separate code path for the negative numbers.
                offset = offset.Negate();
            }

            if (tokenLen <= 1)
            {
                // 'z' format e.g "-7"
                result.AppendFormat(CultureInfo.InvariantCulture, "{0:0}", offset.Hours);
            }
            else
            {
                // 'zz' or longer format e.g "-07"
                result.AppendFormat(CultureInfo.InvariantCulture, "{0:00}", offset.Hours);
                if (tokenLen >= 3)
                {
                    // 'zzz*' or longer format e.g "-07:30"
                    result.AppendFormat(CultureInfo.InvariantCulture, ":{0:00}", offset.Minutes);
                }
            }
        }

        // output the 'K' format, which is for round-tripping the data
        private static void FormatCustomizedRoundripTimeZone(DateTime dateTime, TimeSpan offset, StringBuilder result)
        {
            // The objective of this format is to round trip the data in the type
            // For DateTime it should round-trip the Kind value and preserve the time zone. 
            // DateTimeOffset instance, it should do so by using the internal time zone.                        

            if (offset == NullOffset)
            {
                // source is a date time, so behavior depends on the kind.
                switch (dateTime.Kind)
                {
                    case DateTimeKind.Local:
                        // This should output the local offset, e.g. "-07:30"
                        offset = TimeZoneInfo.GetLocalUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
                        // fall through to shared time zone output code
                        break;
                    case DateTimeKind.Utc:
                        // The 'Z' constant is a marker for a UTC date
                        result.Append("Z");
                        return;
                    default:
                        // If the kind is unspecified, we output nothing here
                        return;
                }
            }
            if (offset >= TimeSpan.Zero)
            {
                result.Append('+');
            }
            else
            {
                result.Append('-');
                // get a positive offset, so that you don't need a separate code path for the negative numbers.
                offset = offset.Negate();
            }

            AppendNumber(result, offset.Hours, 2);
            result.Append(':');
            AppendNumber(result, offset.Minutes, 2);
        }


        internal static String GetRealFormat(String format, DateTimeFormatInfo dtfi)
        {
            String realFormat = null;

            switch (format[0])
            {
                case 'd':       // Short Date
                    realFormat = dtfi.ShortDatePattern;
                    break;
                case 'D':       // Long Date
                    realFormat = dtfi.LongDatePattern;
                    break;
                case 'f':       // Full (long date + short time)
                    realFormat = dtfi.LongDatePattern + " " + dtfi.ShortTimePattern;
                    break;
                case 'F':       // Full (long date + long time)
                    realFormat = dtfi.FullDateTimePattern;
                    break;
                case 'g':       // General (short date + short time)
                    realFormat = dtfi.GeneralShortTimePattern;
                    break;
                case 'G':       // General (short date + long time)
                    realFormat = dtfi.GeneralLongTimePattern;
                    break;
                case 'm':
                case 'M':       // Month/Day Date
                    realFormat = dtfi.MonthDayPattern;
                    break;
                case 'o':
                case 'O':
                    realFormat = RoundtripFormat;
                    break;
                case 'r':
                case 'R':       // RFC 1123 Standard                    
                    realFormat = dtfi.RFC1123Pattern;
                    break;
                case 's':       // Sortable without Time Zone Info
                    realFormat = dtfi.SortableDateTimePattern;
                    break;
                case 't':       // Short Time
                    realFormat = dtfi.ShortTimePattern;
                    break;
                case 'T':       // Long Time
                    realFormat = dtfi.LongTimePattern;
                    break;
                case 'u':       // Universal with Sortable format
                    realFormat = dtfi.UniversalSortableDateTimePattern;
                    break;
                case 'U':       // Universal with Full (long date + long time) format
                    realFormat = dtfi.FullDateTimePattern;
                    break;
                case 'y':
                case 'Y':       // Year/Month Date
                    realFormat = dtfi.YearMonthPattern;
                    break;
                default:
                    throw new FormatException(SR.Format_InvalidString);
            }
            return (realFormat);
        }


        // Expand a pre-defined format string (like "D" for long date) to the real format that
        // we are going to use in the date time parsing.
        // This method also convert the dateTime if necessary (e.g. when the format is in Universal time),
        // and change dtfi if necessary (e.g. when the format should use invariant culture).
        //
        private static String ExpandPredefinedFormat(String format, ref DateTime dateTime, ref DateTimeFormatInfo dtfi, ref TimeSpan offset)
        {
            switch (format[0])
            {
                case 'o':
                case 'O':       // Round trip format
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    break;
                case 'r':
                case 'R':       // RFC 1123 Standard                    
                    if (offset != NullOffset)
                    {
                        // Convert to UTC invariants mean this will be in range
                        dateTime = dateTime - offset;
                    }
                    else if (dateTime.Kind == DateTimeKind.Local)
                    {
                        InvalidFormatForLocal(format, dateTime);
                    }
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    break;
                case 's':       // Sortable without Time Zone Info                
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    break;
                case 'u':       // Universal time in sortable format.
                    if (offset != NullOffset)
                    {
                        // Convert to UTC invariants mean this will be in range
                        dateTime = dateTime - offset;
                    }
                    else if (dateTime.Kind == DateTimeKind.Local)
                    {
                        InvalidFormatForLocal(format, dateTime);
                    }
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    break;
                case 'U':       // Universal time in culture dependent format.
                    if (offset != NullOffset)
                    {
                        // This format is not supported by DateTimeOffset
                        throw new FormatException(SR.Format_InvalidString);
                    }
                    // Universal time is always in Greogrian calendar.
                    //
                    // Change the Calendar to be Gregorian Calendar.
                    //
                    dtfi = (DateTimeFormatInfo)dtfi.Clone();
                    if (dtfi.Calendar.GetType() != typeof(GregorianCalendar))
                    {
                        dtfi.Calendar = GregorianCalendar.GetDefaultInstance();
                    }
                    dateTime = dateTime.ToUniversalTime();
                    break;
            }
            format = GetRealFormat(format, dtfi);
            return (format);
        }

        internal static String Format(DateTime dateTime, String format, DateTimeFormatInfo dtfi)
        {
            return Format(dateTime, format, dtfi, NullOffset);
        }

        internal static string Format(DateTime dateTime, String format, DateTimeFormatInfo dtfi, TimeSpan offset) =>
            StringBuilderCache.GetStringAndRelease(FormatStringBuilder(dateTime, format, dtfi, offset));

        internal static bool TryFormat(DateTime dateTime, Span<char> destination, out int charsWritten, string format, DateTimeFormatInfo dtfi) =>
            TryFormat(dateTime, destination, out charsWritten, format, dtfi, NullOffset);

        internal static bool TryFormat(DateTime dateTime, Span<char> destination, out int charsWritten, string format, DateTimeFormatInfo dtfi, TimeSpan offset)
        {
            StringBuilder sb = FormatStringBuilder(dateTime, format, dtfi, offset);

            bool success = sb.Length <= destination.Length;
            if (success)
            {
                sb.CopyTo(0, destination, sb.Length);
                charsWritten = sb.Length;
            }
            else
            {
                charsWritten = 0;
            }

            StringBuilderCache.Release(sb);
            return success;
        }

        internal static StringBuilder FormatStringBuilder(DateTime dateTime, String format, DateTimeFormatInfo dtfi, TimeSpan offset)
        {
            Debug.Assert(dtfi != null);
            if (format == null || format.Length == 0)
            {
                Boolean timeOnlySpecialCase = false;
                if (dateTime.Ticks < Calendar.TicksPerDay)
                {
                    // If the time is less than 1 day, consider it as time of day.
                    // Just print out the short time format.
                    //
                    // This is a workaround for VB, since they use ticks less then one day to be 
                    // time of day.  In cultures which use calendar other than Gregorian calendar, these
                    // alternative calendar may not support ticks less than a day.
                    // For example, Japanese calendar only supports date after 1868/9/8.
                    // This will pose a problem when people in VB get the time of day, and use it
                    // to call ToString(), which will use the general format (short date + long time).
                    // Since Japanese calendar does not support Gregorian year 0001, an exception will be
                    // thrown when we try to get the Japanese year for Gregorian year 0001.
                    // Therefore, the workaround allows them to call ToString() for time of day from a DateTime by
                    // formatting as ISO 8601 format.
                    switch (dtfi.Calendar.ID)
                    {
                        case CalendarId.JAPAN:
                        case CalendarId.TAIWAN:
                        case CalendarId.HIJRI:
                        case CalendarId.HEBREW:
                        case CalendarId.JULIAN:
                        case CalendarId.UMALQURA:
                        case CalendarId.PERSIAN:
                            timeOnlySpecialCase = true;
                            dtfi = DateTimeFormatInfo.InvariantInfo;
                            break;
                    }
                }
                if (offset == NullOffset)
                {
                    // Default DateTime.ToString case.
                    if (timeOnlySpecialCase)
                    {
                        format = "s";
                    }
                    else
                    {
                        format = "G";
                    }
                }
                else
                {
                    // Default DateTimeOffset.ToString case.
                    if (timeOnlySpecialCase)
                    {
                        format = RoundtripDateTimeUnfixed;
                    }
                    else
                    {
                        format = dtfi.DateTimeOffsetPattern;
                    }
                }
            }

            if (format.Length == 1)
            {
                switch (format[0])
                {
                    case 'O':
                    case 'o':
                        return FastFormatRoundtrip(dateTime, offset);
                    case 'R':
                    case 'r':
                        return FastFormatRfc1123(dateTime, offset, dtfi);
                }

                format = ExpandPredefinedFormat(format, ref dateTime, ref dtfi, ref offset);
            }

            return FormatCustomized(dateTime, format, dtfi, offset);
        }

        internal static StringBuilder FastFormatRfc1123(DateTime dateTime, TimeSpan offset, DateTimeFormatInfo dtfi)
        {
            // ddd, dd MMM yyyy HH:mm:ss GMT
            const int Rfc1123FormatLength = 29;
            StringBuilder result = StringBuilderCache.Acquire(Rfc1123FormatLength);

            if (offset != NullOffset)
            {
                // Convert to UTC invariants
                dateTime = dateTime - offset;
            }

            dateTime.GetDatePart(out int year, out int month, out int day);
            result.Append(InvariantAbbreviatedDayNames[(int)dateTime.DayOfWeek]);
            result.Append(',');
            result.Append(' ');
            AppendNumber(result, day, 2);
            result.Append(' ');
            result.Append(InvariantAbbreviatedMonthNames[month - 1]);
            result.Append(' ');
            AppendNumber(result, year, 4);
            result.Append(' ');
            AppendHHmmssTimeOfDay(result, dateTime);
            result.Append(' ');
            result.Append(Gmt);

            return result;
        }

        internal static StringBuilder FastFormatRoundtrip(DateTime dateTime, TimeSpan offset)
        {
            // yyyy-MM-ddTHH:mm:ss.fffffffK
            const int roundTripFormatLength = 28;
            StringBuilder result = StringBuilderCache.Acquire(roundTripFormatLength);

            dateTime.GetDatePart(out int year, out int month, out int day);
            AppendNumber(result, year, 4);
            result.Append('-');
            AppendNumber(result, month, 2);
            result.Append('-');
            AppendNumber(result, day, 2);
            result.Append('T');
            AppendHHmmssTimeOfDay(result, dateTime);
            result.Append('.');

            long fraction = dateTime.Ticks % TimeSpan.TicksPerSecond;
            AppendNumber(result, fraction, 7);

            FormatCustomizedRoundripTimeZone(dateTime, offset, result);

            return result;
        }

        private static void AppendHHmmssTimeOfDay(StringBuilder result, DateTime dateTime)
        {
            // HH:mm:ss
            AppendNumber(result, dateTime.Hour, 2);
            result.Append(':');
            AppendNumber(result, dateTime.Minute, 2);
            result.Append(':');
            AppendNumber(result, dateTime.Second, 2);
        }

        internal static void AppendNumber(StringBuilder builder, long val, int digits)
        {
            for (int i = 0; i < digits; i++)
            {
                builder.Append('0');
            }

            int index = 1;
            while (val > 0 && index <= digits)
            {
                builder[builder.Length - index] = (char)('0' + (val % 10));
                val = val / 10;
                index++;
            }

            Debug.Assert(val == 0, "DateTimeFormat.AppendNumber(): digits less than size of val");
        }

        internal static String[] GetAllDateTimes(DateTime dateTime, char format, DateTimeFormatInfo dtfi)
        {
            Debug.Assert(dtfi != null);
            String[] allFormats = null;
            String[] results = null;

            switch (format)
            {
                case 'd':
                case 'D':
                case 'f':
                case 'F':
                case 'g':
                case 'G':
                case 'm':
                case 'M':
                case 't':
                case 'T':
                case 'y':
                case 'Y':
                    allFormats = dtfi.GetAllDateTimePatterns(format);
                    results = new String[allFormats.Length];
                    for (int i = 0; i < allFormats.Length; i++)
                    {
                        results[i] = Format(dateTime, allFormats[i], dtfi);
                    }
                    break;
                case 'U':
                    DateTime universalTime = dateTime.ToUniversalTime();
                    allFormats = dtfi.GetAllDateTimePatterns(format);
                    results = new String[allFormats.Length];
                    for (int i = 0; i < allFormats.Length; i++)
                    {
                        results[i] = Format(universalTime, allFormats[i], dtfi);
                    }
                    break;
                //
                // The following ones are special cases because these patterns are read-only in
                // DateTimeFormatInfo.
                //
                case 'r':
                case 'R':
                case 'o':
                case 'O':
                case 's':
                case 'u':
                    results = new String[] { Format(dateTime, new String(format, 1), dtfi) };
                    break;
                default:
                    throw new FormatException(SR.Format_InvalidString);
            }
            return (results);
        }

        internal static String[] GetAllDateTimes(DateTime dateTime, DateTimeFormatInfo dtfi)
        {
            List<String> results = new List<String>(DEFAULT_ALL_DATETIMES_SIZE);

            for (int i = 0; i < allStandardFormats.Length; i++)
            {
                String[] strings = GetAllDateTimes(dateTime, allStandardFormats[i], dtfi);
                for (int j = 0; j < strings.Length; j++)
                {
                    results.Add(strings[j]);
                }
            }
            String[] value = new String[results.Count];
            results.CopyTo(0, value, 0, results.Count);
            return (value);
        }

        // This is a placeholder for an MDA to detect when the user is using a
        // local DateTime with a format that will be interpreted as UTC.
        internal static void InvalidFormatForLocal(String format, DateTime dateTime)
        {
        }
    }
}
