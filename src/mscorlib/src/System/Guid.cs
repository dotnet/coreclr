// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System {

    using System;
    using System.Globalization;
    using System.Text;
    using Microsoft.Win32;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.Contracts;

    // Represents a Globally Unique Identifier.
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.Versioning.NonVersionable] // This only applies to field layout
    public struct Guid : IFormattable, IComparable
        , IComparable<Guid>, IEquatable<Guid>
    {
        public static readonly Guid Empty = new Guid();
        ////////////////////////////////////////////////////////////////////////////////
        //  Member variables
        ////////////////////////////////////////////////////////////////////////////////
        private int         _a;
        private short       _b;
        private short       _c;
        private byte       _d;
        private byte       _e;
        private byte       _f;
        private byte       _g;
        private byte       _h;
        private byte       _i;
        private byte       _j;
        private byte       _k;



        ////////////////////////////////////////////////////////////////////////////////
        //  Constructors
        ////////////////////////////////////////////////////////////////////////////////

        // Creates a new guid from an array of bytes.
        //
        public Guid(byte[] b)
        {
            if (b==null)
                throw new ArgumentNullException(nameof(b));
            if (b.Length != 16)
                throw new ArgumentException(Environment.GetResourceString("Arg_GuidArrayCtor", "16"), nameof(b));
            Contract.EndContractBlock();

            _a = ((int)b[3] << 24) | ((int)b[2] << 16) | ((int)b[1] << 8) | b[0];
            _b = (short)(((int)b[5] << 8) | b[4]);
            _c = (short)(((int)b[7] << 8) | b[6]);
            _d = b[8];
            _e = b[9];
            _f = b[10];
            _g = b[11];
            _h = b[12];
            _i = b[13];
            _j = b[14];
            _k = b[15];
        }

        [CLSCompliant(false)]
        public Guid (uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = (int)a;
            _b = (short)b;
            _c = (short)c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }


        // Creates a new GUID initialized to the value represented by the arguments.
        //
        public Guid(int a, short b, short c, byte[] d)
        {
            if (d==null)
                throw new ArgumentNullException(nameof(d));
            // Check that array is not too big
            if(d.Length != 8)
                throw new ArgumentException(Environment.GetResourceString("Arg_GuidArrayCtor", "8"), nameof(d));
            Contract.EndContractBlock();

            _a  = a;
            _b  = b;
            _c  = c;
            _d = d[0];
            _e = d[1];
            _f = d[2];
            _g = d[3];
            _h = d[4];
            _i = d[5];
            _j = d[6];
            _k = d[7];
        }

        // Creates a new GUID initialized to the value represented by the
        // arguments.  The bytes are specified like this to avoid endianness issues.
        //
        public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }

        [Flags]
        private enum GuidStyles {
            None                  = 0x00000000, 
            AllowParenthesis      = 0x00000001, //Allow the guid to be enclosed in parens
            AllowBraces           = 0x00000002, //Allow the guid to be enclosed in braces
            AllowDashes           = 0x00000004, //Allow the guid to contain dash group separators
            AllowHexPrefix        = 0x00000008, //Allow the guid to contain {0xdd,0xdd}
            RequireParenthesis    = 0x00000010, //Require the guid to be enclosed in parens
            RequireBraces         = 0x00000020, //Require the guid to be enclosed in braces
            RequireDashes         = 0x00000040, //Require the guid to contain dash group separators
            RequireHexPrefix      = 0x00000080, //Require the guid to contain {0xdd,0xdd}

            HexFormat             = RequireBraces | RequireHexPrefix,                      /* X */
            NumberFormat          = None,                                                  /* N */
            DigitFormat           = RequireDashes,                                         /* D */
            BraceFormat           = RequireBraces | RequireDashes,                         /* B */
            ParenthesisFormat     = RequireParenthesis | RequireDashes,                    /* P */
                       
            Any                   = AllowParenthesis | AllowBraces | AllowDashes | AllowHexPrefix,
        }
        private enum GuidParseThrowStyle {
            None                     = 0,
            All                      = 1,
            AllButOverflow           = 2
        }
        private enum ParseFailureKind {
            None                     = 0,
            ArgumentNull             = 1,
            Format                   = 2,
            FormatWithParameter      = 3,
            NativeException          = 4,
            FormatWithInnerException = 5
        }

        // This will store the result of the parsing.  And it will eventually be used to construct a Guid instance.
        private struct GuidResult {
            internal Guid parsedGuid;
            internal GuidParseThrowStyle throwStyle;

            internal ParseFailureKind m_failure;
            internal string m_failureMessageID;
            internal object m_failureMessageFormatArgument;
            internal string m_failureArgumentName;
            internal Exception m_innerException;

            internal void Init(GuidParseThrowStyle canThrow) {
                parsedGuid = Guid.Empty;
                throwStyle = canThrow;               
            }
            internal void SetFailure(Exception nativeException) {
                m_failure = ParseFailureKind.NativeException;
                m_innerException = nativeException;
            }
            internal void SetFailure(ParseFailureKind failure, string failureMessageID) {
                SetFailure(failure, failureMessageID, null, null, null);
            }
            internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument) {
                SetFailure(failure, failureMessageID, failureMessageFormatArgument, null, null);
            }
            internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument,
                                     string failureArgumentName, Exception innerException) {
                Contract.Assert(failure != ParseFailureKind.NativeException, "ParseFailureKind.NativeException should not be used with this overload");
                m_failure = failure;
                m_failureMessageID = failureMessageID;
                m_failureMessageFormatArgument = failureMessageFormatArgument;
                m_failureArgumentName = failureArgumentName;
                m_innerException = innerException;
                if (throwStyle != GuidParseThrowStyle.None) {
                    throw GetGuidParseException();
                }
            }

            internal Exception GetGuidParseException() {
                switch (m_failure) {
                case ParseFailureKind.ArgumentNull:
                    return new ArgumentNullException(m_failureArgumentName, Environment.GetResourceString(m_failureMessageID));

                case ParseFailureKind.FormatWithInnerException:
                    return new FormatException(Environment.GetResourceString(m_failureMessageID), m_innerException);

                case ParseFailureKind.FormatWithParameter:
                    return new FormatException(Environment.GetResourceString(m_failureMessageID, m_failureMessageFormatArgument));

                case ParseFailureKind.Format:
                    return new FormatException(Environment.GetResourceString(m_failureMessageID));

                case ParseFailureKind.NativeException:
                    return m_innerException;

                default:
                    Contract.Assert(false, "Unknown GuidParseFailure: " + m_failure);
                    return new FormatException(Environment.GetResourceString("Format_GuidUnrecognized"));
                }
            }
        }

        // Creates a new guid based on the value in the string.  The value is made up
        // of hex digits speared by the dash ("-"). The string may begin and end with
        // brackets ("{", "}").
        //
        // The string must be of the form dddddddd-dddd-dddd-dddd-dddddddddddd. where
        // d is a hex digit. (That is 8 hex digits, followed by 4, then 4, then 4,
        // then 12) such as: "CA761232-ED42-11CE-BACD-00AA0057B223"
        //
        public Guid(String g)
        {
            if (g==null) {
                throw new ArgumentNullException(nameof(g));
            }
            Contract.EndContractBlock();
            this = Guid.Empty;

            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.All);
            if (TryParseGuid(g, GuidStyles.Any, ref result)) {
                this = result.parsedGuid;
            }
            else {
                throw result.GetGuidParseException();
            }
        }


        public static Guid Parse(String input)
        {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }
            Contract.EndContractBlock();

            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.AllButOverflow);
            if (TryParseGuid(input, GuidStyles.Any, ref result)) {
                return result.parsedGuid;
            }
            else {
                throw result.GetGuidParseException();
            }
        }

        public static bool TryParse(String input, out Guid result)
        {
            GuidResult parseResult = new GuidResult();
            parseResult.Init(GuidParseThrowStyle.None);
            if (TryParseGuid(input, GuidStyles.Any, ref parseResult)) {
                result = parseResult.parsedGuid;
                return true;
            }
            else {
                result = Guid.Empty;
                return false;
            }
        }

        public static Guid ParseExact(String input, String format)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if( format.Length != 1) {
                // all acceptable format strings are of length 1
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }

            GuidStyles style;
            char formatCh = format[0];
            if (formatCh == 'D' || formatCh == 'd') {
                style = GuidStyles.DigitFormat;
            }            
            else if (formatCh == 'N' || formatCh == 'n') {
                style = GuidStyles.NumberFormat;
            }
            else if (formatCh == 'B' || formatCh == 'b') {
                style = GuidStyles.BraceFormat;
            }
            else if (formatCh == 'P' || formatCh == 'p') {
                style = GuidStyles.ParenthesisFormat;
            }
            else if (formatCh == 'X' || formatCh == 'x') {
                style = GuidStyles.HexFormat;
            }
            else {
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }

            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.AllButOverflow);
            if (TryParseGuid(input, style, ref result)) {
                return result.parsedGuid;
            }
            else {
                throw result.GetGuidParseException();
            }
        }

        public static bool TryParseExact(String input, String format, out Guid result)
        {
            if (format == null || format.Length != 1) {
                result = Guid.Empty;
                return false;
            }

            GuidStyles style;
            char formatCh = format[0];

            if (formatCh == 'D' || formatCh == 'd') {
                style = GuidStyles.DigitFormat;
            }            
            else if (formatCh == 'N' || formatCh == 'n') {
                style = GuidStyles.NumberFormat;
            }
            else if (formatCh == 'B' || formatCh == 'b') {
                style = GuidStyles.BraceFormat;
            }
            else if (formatCh == 'P' || formatCh == 'p') {
                style = GuidStyles.ParenthesisFormat;
            }
            else if (formatCh == 'X' || formatCh == 'x') {
                style = GuidStyles.HexFormat;
            }
            else {
                // invalid guid format specification
                result = Guid.Empty;
                return false;
            }

            GuidResult parseResult = new GuidResult();
            parseResult.Init(GuidParseThrowStyle.None);
            if (TryParseGuid(input, style, ref parseResult)) {
                result = parseResult.parsedGuid;
                return true;
            }
            else {
                result = Guid.Empty;
                return false;
            }
        }

        private static bool TryParseGuid(string g, GuidStyles flags, ref GuidResult result)
        {
            if (g == null)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            
            // Consume leading and trailing whitespace
            int start = 0;
            while (start < g.Length && char.IsWhiteSpace(g[start]))
            {
                start++;
            }

            int end = g.Length;
            while (end > start && char.IsWhiteSpace(g[end - 1]))
            {
                end--;
            }

            int length = end - start;
            if (length == 0)
            {
                // Empty string (or only whitespace)
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }

            bool dashesExistInString = false;
            bool bracesExistInString = false;
            bool parenthesisExistInString = false;
            for (int i = start; i < end; i++)
            {
                char c = g[i];
                if (c == '-')
                {
                    dashesExistInString = true;
                }
                else if (c == '{')
                {
                    bracesExistInString = true;
                }
                else if (c == '(')
                {
                    parenthesisExistInString  = true;
                }
            }

            if (dashesExistInString && (flags & (GuidStyles.AllowDashes | GuidStyles.RequireDashes)) == 0)
            {
                // dashes are not allowed
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            else if (!dashesExistInString && (flags & GuidStyles.RequireDashes) != 0)
            {
                // dashes are required
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }

            if (bracesExistInString && (flags & (GuidStyles.AllowBraces | GuidStyles.RequireBraces)) == 0)
            {
                // braces are not allowed
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            else if (!bracesExistInString && (flags & GuidStyles.RequireBraces) != 0)
            {
                // braces are required
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }

            if (parenthesisExistInString && (flags & (GuidStyles.AllowParenthesis | GuidStyles.RequireParenthesis)) == 0)
            {
                // parenthesis are not allowed
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            else if (!parenthesisExistInString  && (flags & GuidStyles.RequireParenthesis) != 0)
            {
                // parenthesis are required
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }

            try
            {
                // let's get on with the parsing
                if (dashesExistInString)
                {
                    // Check if it's of the form [{|(]dddddddd-dddd-dddd-dddd-dddddddddddd[}|)]
                    return TryParseGuidWithDashes(g, start, length, ref result);
                }
                else if (bracesExistInString)
                {
                    // Check if it's of the form {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
                    return TryParseGuidWithHexPrefix(g, start, length, ref result);
                }
                else
                {
                    // Check if it's of the form dddddddddddddddddddddddddddddddd
                    return TryParseGuidWithNoStyle(g, start, length, ref result);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                result.SetFailure(ParseFailureKind.FormatWithInnerException, "Format_GuidUnrecognized", null, null, ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                result.SetFailure(ParseFailureKind.FormatWithInnerException, "Format_GuidUnrecognized", null, null, ex);
                return false;
            }
        }

        // Check if it's of the form {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
        private static bool TryParseGuidWithHexPrefix(string guidString, int start, int length, ref GuidResult result)
        {
            // Check for leading '{'
            ConsumeWhitespace(guidString, ref start);
            if (guidString[start] != '{')
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidBrace");
                return false;
            }

            // Check for '0x'
            start++;
            if (!IsHexPrefix(guidString, ref start))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, etc}");
                return false;
            }

            // Find the end of this hex number (since it is not fixed length)
            int numEnd = -1;
            for (int i = start; i < start + length; i++)
            {
                if (guidString[i] == ',')
                {
                    numEnd = i;
                    break;
                }
            }
            if (numEnd <= start)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }

            if (!StringToInt(guidString, start, numEnd - start, true, out result.parsedGuid._a, ref result))
                return false;

            // Check for '0x'
            start = numEnd + 1;
            if (!IsHexPrefix(guidString, ref start))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, 0xdddd, etc}");
                return false;
            }

            // +3 to get by ',0x'
            numEnd = -1;
            for (int i = start; i < start + length; i++)
            {
                if (guidString[i] == ',')
                {
                    numEnd = i;
                    break;
                }
            }
            if (numEnd <= start)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }

            // Read in the number
            if (!StringToShort(guidString, start, numEnd - start, true, out result.parsedGuid._b, ref result))
                return false;

            // Check for '0x'
            start = numEnd + 1;
            if (!IsHexPrefix(guidString, ref start))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, 0xdddd, 0xdddd, etc}");
                return false;
            }
            // +3 to get by ',0x'
            numEnd = -1;
            for (int i = start; i < start + length; i++)
            {
                if (guidString[i] == ',')
                {
                    numEnd = i;
                    break;
                }
            }
            if (numEnd <= start)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }

            // Read in the number
            if (!StringToShort(guidString, start, numEnd - start, true, out result.parsedGuid._c, ref result))
                return false;

            // Check for '{'
            start = numEnd + 1;
            ConsumeWhitespace(guidString, ref start);
            if (start >= length || guidString[start] != '{')
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidBrace");
                return false;
            }

            // Prepare for loop
            start++;
            byte[] bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                // Check for '0x'
                if (!IsHexPrefix(guidString, ref start))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{... { ... 0xdd, ...}}");
                    return false;
                }

                // Calculate number length
                if (i < 7)  // first 7 cases
                {
                    numEnd = -1;
                    for (int j = start; j < start + length; j++)
                    {
                        if (guidString[j] == ',')
                        {
                            numEnd = j;
                            break;
                        }
                    }
                    if (numEnd <= start)
                    {
                        result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                        return false;
                    }
                }
                else       // last case ends with '}', not ','
                {
                    numEnd = -1;
                    for (int j = start; j < start + length; j++)
                    {
                        if (guidString[j] == '}')
                        {
                            numEnd = j;
                            break;
                        }
                    }
                    if (numEnd <= start)
                    {
                        result.SetFailure(ParseFailureKind.Format, "Format_GuidBraceAfterLastNumber");
                        return false;
                    }
                }

                // Read in the number
                int signedNumber;
                if (!StringToInt(guidString, start, numEnd - start, true, out signedNumber, ref result))
                {
                    return false;
                }
                uint number = (uint)signedNumber;
                // check for overflow
                if (number > 255)
                {
                    result.SetFailure(ParseFailureKind.Format, "Overflow_Byte");
                    return false;
                }
                bytes[i] = (byte)number;
                if (i < 7)
                {
                    start = numEnd + 1;
                }
                else
                {
                    start = numEnd;
                }
            }

            result.parsedGuid._d = bytes[0];
            result.parsedGuid._e = bytes[1];
            result.parsedGuid._f = bytes[2];
            result.parsedGuid._g = bytes[3];
            result.parsedGuid._h = bytes[4];
            result.parsedGuid._i = bytes[5];
            result.parsedGuid._j = bytes[6];
            result.parsedGuid._k = bytes[7];

            // Check for last '}'
            ConsumeWhitespace(guidString, ref start);
            if (start >= length || guidString[start] != '}')
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidEndBrace");
                return false;
            }

            // Check if we have extra characters at the end
            ConsumeWhitespace(guidString, ref start);
            if (start + 1 != length - 1)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_ExtraJunkAtEnd");
                return false;
            }

            return true;
        }

        // Check if it's of the form dddddddddddddddddddddddddddddddd
        private static bool TryParseGuidWithNoStyle(string guidString, int start, int length, ref GuidResult result)
        {
            if (length != 32)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }

            for (int i = start; i < start + length; i++)
            {
                char c = guidString[i];
                if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f') && !(c >= 'A' && c <= 'F'))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidInvalidChar");
                    return false;
                }
            }

            int currentPosition = start;
            int tempInt;
            long tempLong;

            if (!StringToInt(guidString, currentPosition, 8, false, out result.parsedGuid._a, ref result))
                return false;
            currentPosition += 8;

            if (!StringToShort(guidString, currentPosition, 4, false, out result.parsedGuid._b, ref result))
                return false;
            currentPosition += 4;

            if (!StringToShort(guidString, currentPosition, 4, false, out result.parsedGuid._c, ref result))
                return false;
            currentPosition += 4;

            if (!StringToInt(guidString, currentPosition, 4, false, out tempInt, ref result))
                return false;
            currentPosition += 4;

            if (!StringToLong(guidString, currentPosition, 12, false, long.MaxValue, out tempLong, ref result))
                return false;

            result.parsedGuid._d = (byte)(tempInt >> 8);
            result.parsedGuid._e = (byte)(tempInt);
            tempInt = (int)(tempLong >> 32);
            result.parsedGuid._f = (byte)(tempInt >> 8);
            result.parsedGuid._g = (byte)(tempInt);
            tempInt = (int)(tempLong);
            result.parsedGuid._h = (byte)(tempInt >> 24);
            result.parsedGuid._i = (byte)(tempInt >> 16);
            result.parsedGuid._j = (byte)(tempInt >> 8);
            result.parsedGuid._k = (byte)(tempInt);

            return true;
        }

        // Check if it's of the form [{|(]dddddddd-dddd-dddd-dddd-dddddddddddd[}|)]
        private static bool TryParseGuidWithDashes(string guidString, int start, int length, ref GuidResult result)
        {
            int currentPos = start;

            // Check to see that it's the proper length
            char firstChar = guidString[currentPos];
            if (firstChar == '{')
            {
                if (length != 38 || guidString[currentPos + 37] != '}')
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                    return false;
                }
                currentPos += 1;
            }
            else if (firstChar == '(')
            {
                if (length != 38 || guidString[currentPos + 37] != ')')
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                    return false;
                }
                currentPos += 1;
            }
            else if (length != 36)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }

            if (guidString[8 + currentPos] != '-' ||
                guidString[13 + currentPos] != '-' ||
                guidString[18 + currentPos] != '-' ||
                guidString[23 + currentPos] != '-')
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidDashes");
                return false;
            }

            int tempInt;
            long tempLong;
            if (!StringToInt(guidString, currentPos, 8, false, out tempInt, ref result))
                return false;
            result.parsedGuid._a = tempInt;
            currentPos += 9; // Increment past the '-'

            if (!StringToInt(guidString, currentPos, 4, false, out tempInt, ref result))
                return false;
            result.parsedGuid._b = (short)tempInt;
            currentPos += 5; // Increment past the '-'

            if (!StringToInt(guidString, currentPos, 4, false, out tempInt, ref result))
                return false;
            result.parsedGuid._c = (short)tempInt;
            currentPos += 5; // Increment past the '-'

            if (!StringToInt(guidString, currentPos, 4, false, out tempInt, ref result))
                return false;
            currentPos += 5; // Increment past the '-'

            if (!StringToLong(guidString, currentPos, 12, false, long.MaxValue, out tempLong, ref result))
                return false;
            currentPos += 12;

            result.parsedGuid._d = (byte)(tempInt >> 8);
            result.parsedGuid._e = (byte)(tempInt);
            tempInt = (int)(tempLong >> 32);
            result.parsedGuid._f = (byte)(tempInt >> 8);
            result.parsedGuid._g = (byte)(tempInt);
            tempInt = (int)(tempLong);
            result.parsedGuid._h = (byte)(tempInt >> 24);
            result.parsedGuid._i = (byte)(tempInt >> 16);
            result.parsedGuid._j = (byte)(tempInt >> 8);
            result.parsedGuid._k = (byte)(tempInt);

            return true;
        }

        private static bool StringToShort(string s, int parsePos, int requiredLength, bool ignoreWhitespace, out short result, ref GuidResult parseResult)
        {
            int resultInt;
            bool succeeded = StringToInt(s, parsePos, requiredLength, ignoreWhitespace, out resultInt, ref parseResult);
            result = (short)resultInt;
            return succeeded;
        }

        private static bool StringToInt(string s, int parsePos, int requiredLength, bool ignoreWhitespace, out int result, ref GuidResult parseResult)
        {
            long resultLong;
            bool succeeded = StringToLong(s, parsePos, requiredLength, ignoreWhitespace, uint.MaxValue, out resultLong, ref parseResult);
            // We are guaranteed to get a result of <= int.MaxValue so this cast works
            result = (int)resultLong;
            return succeeded;
        }

        private static bool StringToLong(string s, int parsePos, int requiredLength, bool ignoreWhitespace, long maxValue, out long result, ref GuidResult parseResult)
        {
            result = 0;
            try
            {
                result = StringToLongThrows(s, parsePos, requiredLength, ignoreWhitespace, maxValue);
                return true;
            }
            catch (OverflowException ex)
            {
                if (parseResult.throwStyle == GuidParseThrowStyle.All)
                {
                    throw;
                }
                else if (parseResult.throwStyle == GuidParseThrowStyle.AllButOverflow)
                {
                    throw new FormatException();
                }
                else
                {
                    parseResult.SetFailure(ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (parseResult.throwStyle == GuidParseThrowStyle.None)
                {
                    parseResult.SetFailure(ex);
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        private static long StringToLongThrows(string s, int parsePos, int requiredLength, bool ignoreWhitespace, long maxValue)
        {
            long result = 0;
            for (int i = parsePos; i < parsePos + requiredLength; i++)
            {
                int digitValue = 0;
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    digitValue = c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    digitValue = c + 10 - 'a';
                }
                else if (c >= 'A' && c <= 'F')
                {
                    digitValue = c + 10 - 'A';
                }
                else if (ignoreWhitespace && char.IsWhiteSpace(c))
                {
                    continue;
                }
                else
                {
                    // Invalid character
                    throw new FormatException();
                }
                long previousResult = result;
                result = (result * 16) + digitValue;
                if (result < previousResult || result > maxValue)
                {
                    // Overflow occured
                    throw new OverflowException();
                }
            }
            return result;
        }

        private static void ConsumeWhitespace(string guidString, ref int start)
        {
            while (start < guidString.Length && char.IsWhiteSpace(guidString[start]))
            {
                start++;
            }
        }

        private static bool IsHexPrefix(string guidString, ref int start)
        {
            ConsumeWhitespace(guidString, ref start);
            if (guidString[start] != '0')
            {
                return false;
            }
            start++;
            ConsumeWhitespace(guidString, ref start);
            if (start < guidString.Length && (guidString[start] == 'x' || guidString[start] == 'X'))
            {
                start++;
                return true;
            }
            return false;
        }

        // Returns an unsigned byte array containing the GUID.
        public byte[] ToByteArray()
        {
            byte[] g = new byte[16];

            g[0] = (byte)(_a);
            g[1] = (byte)(_a >> 8);
            g[2] = (byte)(_a >> 16);                        
            g[3] = (byte)(_a >> 24);
            g[4] = (byte)(_b);
            g[5] = (byte)(_b >> 8);
            g[6] = (byte)(_c);
            g[7] = (byte)(_c >> 8);
            g[8] = _d;
            g[9] = _e;
            g[10] = _f;
            g[11] = _g;
            g[12] = _h;
            g[13] = _i;
            g[14] = _j;
            g[15] = _k;

            return g;
        }


        // Returns the guid in "registry" format.
        public override String ToString()
        {
            return ToString("D",null);
        }

        [System.Security.SecuritySafeCritical]
        public unsafe override int GetHashCode()
        {
            // Simply XOR all the bits of the GUID 32 bits at a time.
            fixed (int* ptr = &this._a)
                 return ptr[0] ^ ptr[1] ^ ptr[2] ^ ptr[3];
        }

        // Returns true if and only if the guid represented
        //  by o is the same as this instance.
        public override bool Equals(Object o)
        {
            Guid g;
            // Check that o is a Guid first
            if(o == null || !(o is Guid))
                return false;
            else g = (Guid) o;

            // Now compare each of the elements
            if(g._a != _a)
                return false;
            if(g._b != _b)
                return false;
            if(g._c != _c)
                return false;
            if (g._d != _d)
                return false;
            if (g._e != _e)
                return false;
            if (g._f != _f)
                return false;
            if (g._g != _g)
                return false;
            if (g._h != _h)
                return false;
            if (g._i != _i)
                return false;
            if (g._j != _j)
                return false;
            if (g._k != _k)
                return false;

            return true;
        }

        public bool Equals(Guid g)
        {
            // Now compare each of the elements
            if(g._a != _a)
                return false;
            if(g._b != _b)
                return false;
            if(g._c != _c)
                return false;
            if (g._d != _d)
                return false;
            if (g._e != _e)
                return false;
            if (g._f != _f)
                return false;
            if (g._g != _g)
                return false;
            if (g._h != _h)
                return false;
            if (g._i != _i)
                return false;
            if (g._j != _j)
                return false;
            if (g._k != _k)
                return false;

            return true;
        }

        private int GetResult(uint me, uint them) {
            if (me<them) {
                return -1;
            }
            return 1;
        }

        public int CompareTo(Object value) {
            if (value == null) {
                return 1;
            }
            if (!(value is Guid)) {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeGuid"), nameof(value));
            }
            Guid g = (Guid)value;

            if (g._a!=this._a) {
                return GetResult((uint)this._a, (uint)g._a);
            }

            if (g._b!=this._b) {
                return GetResult((uint)this._b, (uint)g._b);
            }

            if (g._c!=this._c) {
                return GetResult((uint)this._c, (uint)g._c);
            }

            if (g._d!=this._d) {
                return GetResult((uint)this._d, (uint)g._d);
            }

            if (g._e!=this._e) {
                return GetResult((uint)this._e, (uint)g._e);
            }

            if (g._f!=this._f) {
                return GetResult((uint)this._f, (uint)g._f);
            }

            if (g._g!=this._g) {
                return GetResult((uint)this._g, (uint)g._g);
            }

            if (g._h!=this._h) {
                return GetResult((uint)this._h, (uint)g._h);
            }

            if (g._i!=this._i) {
                return GetResult((uint)this._i, (uint)g._i);
            }

            if (g._j!=this._j) {
                return GetResult((uint)this._j, (uint)g._j);
            }

            if (g._k!=this._k) {
                return GetResult((uint)this._k, (uint)g._k);
            }

            return 0;
        }

        public int CompareTo(Guid value)
        {
            if (value._a!=this._a) {
                return GetResult((uint)this._a, (uint)value._a);
            }

            if (value._b!=this._b) {
                return GetResult((uint)this._b, (uint)value._b);
            }

            if (value._c!=this._c) {
                return GetResult((uint)this._c, (uint)value._c);
            }

            if (value._d!=this._d) {
                return GetResult((uint)this._d, (uint)value._d);
            }

            if (value._e!=this._e) {
                return GetResult((uint)this._e, (uint)value._e);
            }

            if (value._f!=this._f) {
                return GetResult((uint)this._f, (uint)value._f);
            }

            if (value._g!=this._g) {
                return GetResult((uint)this._g, (uint)value._g);
            }

            if (value._h!=this._h) {
                return GetResult((uint)this._h, (uint)value._h);
            }

            if (value._i!=this._i) {
                return GetResult((uint)this._i, (uint)value._i);
            }

            if (value._j!=this._j) {
                return GetResult((uint)this._j, (uint)value._j);
            }

            if (value._k!=this._k) {
                return GetResult((uint)this._k, (uint)value._k);
            }

            return 0;
        }

        public static bool operator ==(Guid a, Guid b)
        {
            // Now compare each of the elements
            if(a._a != b._a)
                return false;
            if(a._b != b._b)
                return false;
            if(a._c != b._c)
                return false;
            if(a._d != b._d)
                return false;
            if(a._e != b._e)
                return false;
            if(a._f != b._f)
                return false;
            if(a._g != b._g)
                return false;
            if(a._h != b._h)
                return false;
            if(a._i != b._i)
                return false;
            if(a._j != b._j)
                return false;
            if(a._k != b._k)
                return false;

            return true;
        }

        public static bool operator !=(Guid a, Guid b)
        {
            return !(a == b);
        }

        // This will create a new guid.  Since we've now decided that constructors should 0-init,
        // we need a method that allows users to create a guid.
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static Guid NewGuid() {
            // CoCreateGuid should never return Guid.Empty, since it attempts to maintain some
            // uniqueness guarantees.  It should also never return a known GUID, but it's unclear
            // how extensively it checks for known values.
            Contract.Ensures(Contract.Result<Guid>() != Guid.Empty);

            Guid guid;
            Marshal.ThrowExceptionForHR(Win32Native.CoCreateGuid(out guid), new IntPtr(-1));
            return guid;
        }

        public String ToString(String format) {
            return ToString(format, null);
        }

        private static char HexToChar(int a)
        {
            a = a & 0xf;
            return (char) ((a > 9) ? a - 10 + 0x61 : a + 0x30);
        }

        [System.Security.SecurityCritical]
        unsafe private static int HexsToChars(char* guidChars, int offset, int a, int b)
        {
            return HexsToChars(guidChars, offset, a, b, false);
        }

        [System.Security.SecurityCritical]
        unsafe private static int HexsToChars(char* guidChars, int offset, int a, int b, bool hex)
        {
            if (hex) {
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(a>>4);
            guidChars[offset++] = HexToChar(a);
            if (hex) {
                guidChars[offset++] = ',';
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(b>>4);
            guidChars[offset++] = HexToChar(b);
            return offset;
        }

        // IFormattable interface
        // We currently ignore provider
        [System.Security.SecuritySafeCritical]
        public String ToString(String format, IFormatProvider provider)
        {
            if (format == null || format.Length == 0)
                format = "D";

            string guidString;
            int offset = 0;
            bool dash = true;
            bool hex = false;

            if( format.Length != 1) {
                // all acceptable format strings are of length 1
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }
            
            char formatCh = format[0];
            if (formatCh == 'D' || formatCh == 'd') {
                guidString = string.FastAllocateString(36);
            }            
            else if (formatCh == 'N' || formatCh == 'n') {
                guidString = string.FastAllocateString(32);
                dash = false;
            }
            else if (formatCh == 'B' || formatCh == 'b') {
                guidString = string.FastAllocateString(38);
                unsafe {
                    fixed (char* guidChars = guidString) {
                        guidChars[offset++] = '{';
                        guidChars[37] = '}';
                    }
                }
            }
            else if (formatCh == 'P' || formatCh == 'p') {
                guidString = string.FastAllocateString(38);
                unsafe {
                    fixed (char* guidChars = guidString) {
                        guidChars[offset++] = '(';
                        guidChars[37] = ')';
                    }
                }
            }
            else if (formatCh == 'X' || formatCh == 'x') {
                guidString = string.FastAllocateString(68);
                unsafe {
                    fixed (char* guidChars = guidString) {
                        guidChars[offset++] = '{';
                        guidChars[67] = '}';
                    }
                }
                dash = false;
                hex = true;
            }
            else {
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }

            unsafe {
                fixed (char* guidChars = guidString) {
                    if (hex) {
                        // {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
                        offset = HexsToChars(guidChars, offset, _a >> 8, _a);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _b >> 8, _b);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _c >> 8, _c);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '{';
                        offset = HexsToChars(guidChars, offset, _d, _e, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _f, _g, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _h, _i, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _j, _k, true);
                        guidChars[offset++] = '}';
                    }
                    else {
                        // [{|(]dddddddd[-]dddd[-]dddd[-]dddd[-]dddddddddddd[}|)]
                        offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
                        offset = HexsToChars(guidChars, offset, _a >> 8, _a);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _b >> 8, _b);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _c >> 8, _c);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _d, _e);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _f, _g);
                        offset = HexsToChars(guidChars, offset, _h, _i);
                        offset = HexsToChars(guidChars, offset, _j, _k);
                    }
                }
            }
            return guidString;
        }
    }
}
