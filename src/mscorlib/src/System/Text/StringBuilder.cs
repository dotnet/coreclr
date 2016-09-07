// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: implementation of the StringBuilder
** class.
**
===========================================================*/
namespace System.Text {
    using System.Text;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Threading;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    // This class represents a mutable string.  It is convenient for situations in
    // which it is desirable to modify a string, perhaps by removing, replacing, or 
    // inserting characters, without creating a new String subsequent to
    // each modification. 
    // 
    // The methods contained within this class do not return a new StringBuilder
    // object unless specified otherwise.  This class may be used in conjunction with the String
    // class to carry out modifications upon strings.
    // 
    // When passing null into a constructor in VJ and VC, the null
    // should be explicitly type cast.
    // For Example:
    // StringBuilder sb1 = new StringBuilder((StringBuilder)null);
    // StringBuilder sb2 = new StringBuilder((String)null);
    // Console.WriteLine(sb1);
    // Console.WriteLine(sb2);
    // 
    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public sealed class StringBuilder : ISerializable
    {
        private string _string = string.Empty;

        public StringBuilder()
        {
        }

        // Create a new empty string builder (i.e., it represents String.Empty)
        // with the specified capacity.
        public StringBuilder(int capacity)
            : this(capacity, int.MaxValue)
        {
        }

        // Creates a new string builder from the specified string.  If value
        // is a null String (i.e., if it represents String.NullString)
        // then the new string builder will also be null (i.e., it will also represent
        //  String.NullString).
        // 
        public StringBuilder(String value)
        {
            _string = value;
        }

        // Creates a new string builder from the specified string with the specified 
        // capacity.  If value is a null String (i.e., if it represents 
        // String.NullString) then the new string builder will also be null 
        // (i.e., it will also represent String.NullString).
        // The maximum number of characters this string may contain is set by capacity.
        // 
        public StringBuilder(String value, int capacity)
            : this(value, 0, ((value != null) ? value.Length : 0), capacity) {
        }

        // Creates a new string builder from the specifed substring with the specified
        // capacity.  The maximum number of characters is set by capacity.
        // 
        [System.Security.SecuritySafeCritical] // auto-generated
        public StringBuilder(String value, int startIndex, int length, int capacity)
        {
            _string = value.Substring(startIndex, length);
        }

        // Creates an empty StringBuilder with a minimum capacity of capacity
        // and a maximum capacity of maxCapacity.
        public StringBuilder(int capacity, int maxCapacity) {
        }

        [System.Security.SecurityCritical]  // auto-generated
        private StringBuilder(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");
            Contract.EndContractBlock();

            int persistedCapacity = 0;
            string persistedString = null;
            int persistedMaxCapacity = Int32.MaxValue;
            bool capacityPresent = false;

            // Get the data
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext()) {
                switch (enumerator.Name)
                {
                }

            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
 
        }

        [System.Diagnostics.Conditional("_DEBUG")]
        private void VerifyClassInvariant() {
    
        }

        public int Capacity {
            get { return _string.Length; }
            set {
                
            }
        }

        public int MaxCapacity {
            get { return int.MaxValue; }
        }

        // Read-Only Property 
        // Ensures that the capacity of this string builder is at least the specified value.  
        // If capacity is greater than the capacity of this string builder, then the capacity
        // is set to capacity; otherwise the capacity is unchanged.
        // 
        public int EnsureCapacity(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
            }

            return Capacity;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override String ToString() {
            if (Length == 0)
                return String.Empty;

            return _string;
        }


        // Converts a substring of this string builder to a String.
        [System.Security.SecuritySafeCritical] // auto-generated
        public String ToString(int startIndex, int length)
        {
            return _string.Substring(startIndex, length);
        }

        // Convenience method for sb.Length=0;
        public StringBuilder Clear() {
            this.Length = 0;
            return this;
        }

        // Sets the length of the String in this buffer.  If length is less than the current
        // instance, the StringBuilder is truncated.  If length is greater than the current 
        // instance, nulls are appended.  The capacity is adjusted to be the same as the length.

        public int Length {
            get
            {
                return _string.Length;
            }
            set {
                // TODO: Do nothing ok?
            }
        }

        [System.Runtime.CompilerServices.IndexerName("Chars")]
        public char this[int index] {
            get { return _string[index]; }
            set
            {
                _string = _string.Insert(index, "" + value); // TODO: This would be expensive in JS as well?
            }
        }

        public unsafe StringBuilder Append(char* p, int count)
        {
            return Append(new String(p), count);
        }


        // Appends a character at the end of this string builder. The capacity is adjusted as needed.
        public StringBuilder Append(char value, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                _string += value;
            }
            return this;
        }

        public StringBuilder Append( string value, int repeatCount ) {
            for ( int i = 0; i < repeatCount; i++ ) {
                _string += value;
            }
            return this;
        }


        [MethodImplAttribute( MethodImplOptions.InternalCall )] 
         [SecurityCritical] 
         internal unsafe extern void ReplaceBufferInternal( char* newBuffer, int newLength ); 
 
 
         [MethodImplAttribute( MethodImplOptions.InternalCall )] 
         [SecurityCritical] 
         internal unsafe extern void ReplaceBufferAnsiInternal( sbyte* newBuffer, int newLength );

        internal unsafe void InternalCopy(IntPtr dest, int len)
        {
            throw new NotImplementedException();
        }

        // Appends an array of characters at the end of this string builder. The capacity is adjusted as needed. 
        [System.Security.SecuritySafeCritical]  // auto-generated
        public StringBuilder Append(char[] value, int startIndex, int charCount) {
            if (startIndex < 0) {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (charCount<0) {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }

            return this;
        }


        // Appends a copy of this string at the end of this string builder.
        [System.Security.SecuritySafeCritical]  // auto-generated
        public StringBuilder Append(String value)
        {
            _string += value;
            return this;
        }

        // Appends a copy of the characters in value from startIndex to startIndex +
        // count at the end of this string builder.
        [System.Security.SecuritySafeCritical] // auto-generated
        public StringBuilder Append(String value, int startIndex, int count)
        {
            _string += value.Substring(startIndex, count);
            return this;
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public StringBuilder AppendLine() {
            return Append(Environment.NewLine);
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public StringBuilder AppendLine(string value) {
            Append(value);
            return Append(Environment.NewLine);
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        [SecuritySafeCritical]
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }

            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Arg_NegativeArgCount"));
            }

            if (destinationIndex < 0) {
                throw new ArgumentOutOfRangeException("destinationIndex",
                    Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", "destinationIndex"));
            }

            if (destinationIndex > destination.Length - count) {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_OffsetOut"));
            }

            if ((uint)sourceIndex > (uint)Length) {
                throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            if (sourceIndex > Length - count) {
                throw new ArgumentException(Environment.GetResourceString("Arg_LongerThanSrcString"));
            }

            for (int i = 0; i < count; i++)
            {
                destination[destinationIndex + i] = _string[sourceIndex + i];
            }
        }

        // Inserts multiple copies of a string into this string builder at the specified position.
        // Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, this
        // string builder is not changed. 
        // 
        [System.Security.SecuritySafeCritical]  // auto-generated
        public StringBuilder Insert(int index, String value, int count)
        {
            string tmp = string.Empty;
            for (int i = 0; i < count; i++)
            {
                tmp += value;
            }
            
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            _string = _string.Insert( index, tmp );
            return this;
        }

        // Removes the specified characters from this string builder.
        // The length of this string builder is reduced by 
        // length, but the capacity is unaffected.
        // 
        public StringBuilder Remove(int startIndex, int length)
        {

            _string = _string.Remove( startIndex, length );
            return this;
        }

        //
        // PUBLIC INSTANCE FUNCTIONS
        //
        //

        /*====================================Append====================================
        **
        ==============================================================================*/
        // Appends a boolean to the end of this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append(bool value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString());
        }

        // Appends an sbyte to this string builder.
        // The capacity is adjusted as needed. 
        [CLSCompliant(false)]
        public StringBuilder Append(sbyte value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends a ubyte to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append(byte value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends a character at the end of this string builder. The capacity is adjusted as needed.
        public StringBuilder Append(char value)
        {
            _string += value;
            return this;
        }

        // Appends a short to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append(short value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends an int to this string builder.
        // The capacity is adjusted as needed. 
        public StringBuilder Append(int value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends a long to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append(long value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends a float to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append(float value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends a double to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append(double value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public StringBuilder Append(decimal value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends an ushort to this string builder. 
        // The capacity is adjusted as needed. 
        [CLSCompliant(false)]
        public StringBuilder Append(ushort value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends an uint to this string builder. 
        // The capacity is adjusted as needed. 
        [CLSCompliant(false)]
        public StringBuilder Append(uint value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends an unsigned long to this string builder. 
        // The capacity is adjusted as needed. 
        [CLSCompliant(false)]
        public StringBuilder Append(ulong value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        // Appends an Object to this string builder. 
        // The capacity is adjusted as needed. 
        public StringBuilder Append(Object value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);

            if (null==value) {
                //Appending null is now a no-op.
                return this;
            }
            return Append(value.ToString());
        }

        // Appends all of the characters in value to the current instance.
        [System.Security.SecuritySafeCritical] // auto-generated
        public StringBuilder Append(char[] value)
        {
            _string += new string(value);
            return this;
        }

        /*====================================Insert====================================
        **
        ==============================================================================*/

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        [System.Security.SecuritySafeCritical]  // auto-generated
        public StringBuilder Insert(int index, String value) {
            if ((uint)index > (uint)Length) {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            _string = _string.Insert(index, value);

            return this;
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert( int index, bool value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        [CLSCompliant(false)]
        public StringBuilder Insert(int index, sbyte value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, byte value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, short value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        [SecuritySafeCritical]
        public StringBuilder Insert(int index, char value)
        {
            return Insert(index, "" + value);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, char[] value) {
            if ((uint)index > (uint)Length) {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            Contract.EndContractBlock();

            if (value != null)
                Insert(index, value, 0, value.Length);
            return this;
        }

        // Returns a reference to the StringBuilder with charCount characters from 
        // value inserted into the buffer at index.  Existing characters are shifted
        // to make room for the new text and capacity is adjusted as required.  If value is null, the StringBuilder
        // is unchanged.  Characters are taken from value starting at position startIndex.
        [System.Security.SecuritySafeCritical]  // auto-generated
        public StringBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            _string = _string.Insert(index, new String(value, startIndex, charCount));
            return this;
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, int value){
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, long value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed.
        // 
        public StringBuilder Insert(int index, float value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with ; value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed. 
        // 
        public StringBuilder Insert(int index, double value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, decimal value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant(false)]
        public StringBuilder Insert(int index, ushort value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant(false)]
        public StringBuilder Insert(int index, uint value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to the StringBuilder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the new text.
        // The capacity is adjusted as needed. 
        // 
        [CLSCompliant(false)]
        public StringBuilder Insert(int index, ulong value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        // Returns a reference to this string builder with value inserted into 
        // the buffer at index. Existing characters are shifted to make room for the
        // new text.  The capacity is adjusted as needed. If value equals String.Empty, the
        // StringBuilder is not changed. No changes are made if value is null.
        // 
        public StringBuilder Insert(int index, Object value) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            if (null == value) {
                return this;
            }
            return Insert(index, value.ToString(), 1);
        }

        public StringBuilder AppendFormat(String format, Object arg0) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(null, format, new ParamsArray(arg0));
        }

        public StringBuilder AppendFormat(String format, Object arg0, Object arg1) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1));
        }

        public StringBuilder AppendFormat(String format, Object arg0, Object arg1, Object arg2) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
        }

        public StringBuilder AppendFormat(String format, params Object[] args) {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();
            
            return AppendFormatHelper(null, format, new ParamsArray(args));
        }

        public StringBuilder AppendFormat(IFormatProvider provider, String format, Object arg0) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(provider, format, new ParamsArray(arg0));
        }
        
        public StringBuilder AppendFormat(IFormatProvider provider, String format, Object arg0, Object arg1) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1));
        }
        
        public StringBuilder AppendFormat(IFormatProvider provider, String format, Object arg0, Object arg1, Object arg2) {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
        }
        
        public StringBuilder AppendFormat(IFormatProvider provider, String format, params Object[] args) {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();
            
            return AppendFormatHelper(provider, format, new ParamsArray(args));
        }
        
        private static void FormatError() {
            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
        }

        // undocumented exclusive limits on the range for Argument Hole Index and Argument Hole Alignment.
        private const int Index_Limit = 1000000; // Note:            0 <= ArgIndex < Index_Limit
        private const int Width_Limit = 1000000; // Note: -Width_Limit <  ArgAlign < Width_Limit

        internal StringBuilder AppendFormatHelper(IFormatProvider provider, String format, ParamsArray args) {
            if (format == null) {
                throw new ArgumentNullException("format");
            }
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            Contract.EndContractBlock();

            int pos = 0;
            int len = format.Length;
            char ch = '\x0';
            StringBuilder unescapedItemFormat = null;

            ICustomFormatter cf = null;
            if (provider != null) {
                cf = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            while (true) {
                while (pos < len) {
                    ch = format[pos];

                    pos++;
                    // Is it a closing brace?
                    if (ch == '}')
                    {
                        // Check next character (if there is one) to see if it is escaped. eg }}
                        if (pos < len && format[pos] == '}')
                            pos++;
                        else
                            // Otherwise treat it as an error (Mismatched closing brace)
                            FormatError();
                    }
                    // Is it a opening brace?
                    if (ch == '{')
                    {
                        // Check next character (if there is one) to see if it is escaped. eg {{
                        if (pos < len && format[pos] == '{')
                            pos++;
                        else
                        {
                            // Otherwise treat it as the opening brace of an Argument Hole.
                            pos--;
                            break;
                        }
                    }
                    // If it neither then treat the character as just text.
                    Append(ch);
                }

                //
                // Start of parsing of Argument Hole.
                // Argument Hole ::= { Index (, WS* Alignment WS*)? (: Formatting)? }
                //
                if (pos == len) break;
                
                //
                //  Start of parsing required Index parameter.
                //  Index ::= ('0'-'9')+ WS*
                //
                pos++;
                // If reached end of text then error (Unexpected end of text)
                // or character is not a digit then error (Unexpected Character)
                if (pos == len || (ch = format[pos]) < '0' || ch > '9') FormatError();
                int index = 0;
                do {
                    index = index * 10 + ch - '0';
                    pos++;
                    // If reached end of text then error (Unexpected end of text)
                    if (pos == len) FormatError();
                    ch = format[pos];
                    // so long as character is digit and value of the index is less than 1000000 ( index limit )
                } while (ch >= '0' && ch <= '9' && index < Index_Limit);

                // If value of index is not within the range of the arguments passed in then error (Index out of range)
                if (index >= args.Length) throw new FormatException(Environment.GetResourceString("Format_IndexOutOfRange"));
                
                // Consume optional whitespace.
                while (pos < len && (ch = format[pos]) == ' ') pos++;
                // End of parsing index parameter.

                //
                //  Start of parsing of optional Alignment
                //  Alignment ::= comma WS* minus? ('0'-'9')+ WS*
                //
                bool leftJustify = false;
                int width = 0;
                // Is the character a comma, which indicates the start of alignment parameter.
                if (ch == ',') {
                    pos++;
 
                    // Consume Optional whitespace
                    while (pos < len && format[pos] == ' ') pos++;
                    
                    // If reached the end of the text then error (Unexpected end of text)
                    if (pos == len) FormatError();
                    
                    // Is there a minus sign?
                    ch = format[pos];
                    if (ch == '-') {
                        // Yes, then alignment is left justified.
                        leftJustify = true;
                        pos++;
                        // If reached end of text then error (Unexpected end of text)
                        if (pos == len) FormatError();
                        ch = format[pos];
                    }
 
                    // If current character is not a digit then error (Unexpected character)
                    if (ch < '0' || ch > '9') FormatError();
                    // Parse alignment digits.
                    do {
                        width = width * 10 + ch - '0';
                        pos++;
                        // If reached end of text then error. (Unexpected end of text)
                        if (pos == len) FormatError();
                        ch = format[pos];
                        // So long a current character is a digit and the value of width is less than 100000 ( width limit )
                    } while (ch >= '0' && ch <= '9' && width < Width_Limit);
                    // end of parsing Argument Alignment
                }

                // Consume optional whitespace
                while (pos < len && (ch = format[pos]) == ' ') pos++;

                //
                // Start of parsing of optional formatting parameter.
                //
                Object arg = args[index];
                String itemFormat = null;
                // Is current character a colon? which indicates start of formatting parameter.
                if (ch == ':') {
                    pos++;
                    int startPos = pos;

                    while (true) {
                        // If reached end of text then error. (Unexpected end of text)
                        if (pos == len) FormatError();
                        ch = format[pos];
                        pos++;

                        // Is character a opening or closing brace?
                        if (ch == '}' || ch == '{')
                        {
                            if (ch == '{')
                            {
                                // Yes, is next character also a opening brace, then treat as escaped. eg {{
                                if (pos < len && format[pos] == '{')
                                    pos++;
                                else
                                    // Error Argument Holes can not be nested.
                                    FormatError();
                            }
                            else
                            {
                                // Yes, is next character also a closing brace, then treat as escaped. eg }}
                                if (pos < len && format[pos] == '}')
                                    pos++;
                                else
                                {
                                    // No, then treat it as the closing brace of an Arg Hole.
                                    pos--;
                                    break;
                                }
                            }

                            // Reaching here means the brace has been escaped
                            // so we need to build up the format string in segments
                            if (unescapedItemFormat == null)
                            {
                                unescapedItemFormat = new StringBuilder();
                            }
                            unescapedItemFormat.Append(format, startPos, pos - startPos - 1);
                            startPos = pos;
                        }
                    }

                    if (unescapedItemFormat == null || unescapedItemFormat.Length == 0)
                    {
                        if (startPos != pos)
                        {
                            // There was no brace escaping, extract the item format as a single string
                            itemFormat = format.Substring(startPos, pos - startPos);
                        }
                    }
                    else
                    {
                        unescapedItemFormat.Append(format, startPos, pos - startPos);
                        itemFormat = unescapedItemFormat.ToString();
                        unescapedItemFormat.Clear();
                    }
                }
                // If current character is not a closing brace then error. (Unexpected Character)
                if (ch != '}') FormatError();
                // Construct the output for this arg hole.
                pos++;
                String s = null;
                if (cf != null) {
                    s = cf.Format(itemFormat, arg, provider);
                }

                if (s == null) {
                    IFormattable formattableArg = arg as IFormattable;

                    if (formattableArg != null) {
                        s = formattableArg.ToString(itemFormat, provider);
                    } else if (arg != null) {
                        s = arg.ToString();
                    }
                }
                // Append it to the final output of the Format String.
                if (s == null) s = String.Empty;
                int pad = width - s.Length;
                if (!leftJustify && pad > 0) Append(' ', pad);
                Append(s);
                if (leftJustify && pad > 0) Append(' ', pad);
                // Continue to parse other characters.
            }
            return this;
        }

        // Returns a reference to the current StringBuilder with all instances of oldString 
        // replaced with newString.  If startIndex and count are specified,
        // we only replace strings completely contained in the range of startIndex to startIndex + 
        // count.  The strings to be replaced are checked on an ordinal basis (e.g. not culture aware).  If 
        // newValue is null, instances of oldValue are removed (e.g. replaced with nothing.).
        //
        public StringBuilder Replace(String oldValue, String newValue) {
            return Replace(oldValue, newValue, 0, Length);
        }

        public bool Equals(StringBuilder sb) 
        {
            if (sb == null)
                return false;
            return sb._string == _string;
        }

        public StringBuilder Replace(String oldValue, String newValue, int startIndex, int count)
        {
            _string = _string.Substring(0, startIndex) +
                      _string.Substring(startIndex, count).Replace(oldValue, newValue) +
                      _string.Substring(startIndex + count);
            return this;
        }

        // Returns a StringBuilder with all instances of oldChar replaced with 
        // newChar.  The size of the StringBuilder is unchanged because we're only
        // replacing characters.  If startIndex and count are specified, we 
        // only replace characters in the range from startIndex to startIndex+count
        //
        public StringBuilder Replace(char oldChar, char newChar) {
            return Replace(oldChar, newChar, 0, Length);
        }

        public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            return Replace(oldChar + "", newChar + "", startIndex, count);
        }
    }
}
