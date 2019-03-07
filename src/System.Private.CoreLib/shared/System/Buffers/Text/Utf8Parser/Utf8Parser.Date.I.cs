// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Buffers.Text
{
    public static partial class Utf8Parser
    {
        //
        // Flexible ISO 8601 format. One of
        //
        // ---------------------------------
        // YYYY-MM-DD (eg 1997-07-16)
        // YYYY-MM-DDThh:mm (eg 1997-07-16T19:20)
        // YYYY-MM-DDThh:mm:ss (eg 1997-07-16T19:20:30)
        // YYYY-MM-DDThh:mm:ss.s (eg 1997-07-16T19:20:30.45)
        // YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
        // YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45Z)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01:00)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45-01:00)
        private static bool TryParseDateTimeOffsetI(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, out DateTimeKind kind)
        {
            // Source does not have enough characters for YYYY-MM-DD
            if (source.Length < 10)
            {
                goto ReturnFalse;
            }

            int year;
            {
                uint digit1 = source[0] - (uint)'0';
                uint digit2 = source[1] - (uint)'0';
                uint digit3 = source[2] - (uint)'0';
                uint digit4 = source[3] - (uint)'0';

                if (digit1 > 9 || digit2 > 9 || digit3 > 9 || digit4 > 9)
                {
                    goto ReturnFalse;
                }

                year = (int)(digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4);
            }

            if (source[4] != Utf8Constants.Hyphen)
            {
                goto ReturnFalse;
            }

            int month;
            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: 5, length: 2), out month))
            {
                goto ReturnFalse;
            }

            if (source[7] != Utf8Constants.Hyphen)
            {
                goto ReturnFalse;
            }

            int day;
            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: 8, length: 2), out day))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DD
            bytesConsumed = 10;

            int hour = 0;
            int minute = 0;
            int second = 0;
            int fraction = 0;
            int offsetHours = 0;
            int offsetMinutes = 0;
            byte offsetToken = default;

            if (source.Length < 11)
            {
                goto FinishedParsing;
            }

            byte curByte = source[10];

            if (curByte == Utf8Constants.UtcOffsetToken || curByte == Utf8Constants.Plus || curByte == Utf8Constants.Minus)
            {
                goto ReturnFalse;
            }
            else if (curByte != Utf8Constants.TimePrefix)
            {
                goto FinishedParsing;
            }

            // Source does not have enough characters for YYYY-MM-DDThh:mm
            if (source.Length < 16)
            {
                goto ReturnFalse;
            }

            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: 11, length: 2), out hour))
            {
                goto ReturnFalse;
            }

            if (source[13] != Utf8Constants.Colon)
            {
                goto ReturnFalse;
            }

            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: 14, length: 2), out minute))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DDThh:mm
            bytesConsumed = 16;

            if (source.Length < 17)
            {
                goto FinishedParsing;
            }

            curByte = source[16];

            int sourceIndex = 16;

            if (curByte == Utf8Constants.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = Utf8Constants.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == Utf8Constants.Plus || curByte == Utf8Constants.Minus)
            {
                offsetToken = curByte;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curByte != Utf8Constants.Colon)
            {
                goto FinishedParsing;
            }

            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: 17, length: 2), out second))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DDThh:mm:ss
            bytesConsumed = 19;

            if (source.Length < 20)
            {
                goto FinishedParsing;
            }

            curByte = source[19];
            sourceIndex = 19;

            if (curByte == Utf8Constants.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = Utf8Constants.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == Utf8Constants.Plus || curByte == Utf8Constants.Minus)
            {
                offsetToken = curByte;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curByte != Utf8Constants.Period)
            {
                goto FinishedParsing;
            }

            // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s
            if (source.Length < 21)
            {
                value = default;
                bytesConsumed = 0;
                kind = default;
                return false;
            }

            sourceIndex = 20;

            {
                while (sourceIndex < source.Length && ParserHelpers.IsDigit(curByte = source[sourceIndex]))
                {
                    int prevFractionTimesTen = fraction * 10;

                    if (!(prevFractionTimesTen + (int)(curByte - (uint)'0') <= Utf8Constants.MaxDateTimeFraction))
                    {
                        sourceIndex++;
                        break;
                    }

                    fraction = prevFractionTimesTen + (int)(curByte - (uint)'0');
                    sourceIndex++;
                }
            }

            if (fraction != 0)
            {
                // Note this is 1 order of magnitude less than Utf8Constants.MaxDateTimeFraction
                while (fraction <= Utf8Constants.MaxDateTimeFractionDiv10)
                {
                    fraction *= 10;
                }
            }

            // We now have YYYY-MM-DDThh:mm:ss.s
            bytesConsumed = sourceIndex;

            if (sourceIndex == source.Length)
            {
                goto FinishedParsing;
            }

            if (curByte == Utf8Constants.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = Utf8Constants.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == Utf8Constants.Plus || curByte == Utf8Constants.Minus)
            {
                offsetToken = source[sourceIndex++];
                goto ParseOffset;
            }

            goto FinishedParsing;

        ParseOffset:
            // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s+|-hh:mm
            if (source.Length - sourceIndex < 5)
            {
                goto ReturnFalse;
            }

            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: sourceIndex, length: 2), out offsetHours))
            {
                goto ReturnFalse;
            }
            sourceIndex += 2;

            if (source[sourceIndex++] != Utf8Constants.Colon)
            {
                goto ReturnFalse;
            }

            if (!ParserHelpers.TryGetNextTwoDigits(source.Slice(start: sourceIndex, length: 2), out offsetMinutes))
            {
                goto ReturnFalse;
            }
            sourceIndex += 2;

            // We now have YYYY-MM-DDThh:mm:ss.s+|-hh:mm
            bytesConsumed = sourceIndex;

        FinishedParsing:
            if ((offsetToken != Utf8Constants.UtcOffsetToken) && (offsetToken != Utf8Constants.Plus) && (offsetToken != Utf8Constants.Minus))
            {
                if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Unspecified;
                return true;
            }

            if (offsetToken == Utf8Constants.UtcOffsetToken)
            {
                // Same as specifying an offset of "+00:00", except that DateTime's Kind gets set to UTC rather than Local
                if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: false, offsetHours: 0, offsetMinutes: 0, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Utc;
                return true;
            }

            Debug.Assert(offsetToken == Utf8Constants.Plus || offsetToken == Utf8Constants.Minus);

            if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: offsetToken == Utf8Constants.Minus, offsetHours: offsetHours, offsetMinutes: offsetMinutes, out value))
            {
                goto ReturnFalse;
            }

            kind = DateTimeKind.Local;
            return true;

        ReturnFalse:
            value = default;
            bytesConsumed = 0;
            kind = default;
            return false;
        }
    }
}
