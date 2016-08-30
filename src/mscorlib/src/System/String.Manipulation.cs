// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public partial class String
    {
        [System.Security.SecuritySafeCritical]  // auto-generated
        unsafe private static void FillStringChecked(String dest, int destPos, String src)
        {
            Contract.Requires(dest != null);
            Contract.Requires(src != null);
            if (src.Length > dest.Length - destPos) {
                throw new IndexOutOfRangeException();
            }
            Contract.EndContractBlock();

            fixed(char *pDest = &dest.m_firstChar)
                fixed (char *pSrc = &src.m_firstChar) {
                    wstrcpy(pDest + destPos, pSrc, src.Length);
                }
        }

        public static String Concat(Object arg0) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (arg0 == null)
            {
                return String.Empty;
            }
            return arg0.ToString();
        }
    
        public static String Concat(Object arg0, Object arg1) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (arg0 == null)
            {
                arg0 = String.Empty;
            }
    
            if (arg1==null) {
                arg1 = String.Empty;
            }
            return Concat(arg0.ToString(), arg1.ToString());
        }
    
        public static String Concat(Object arg0, Object arg1, Object arg2) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (arg0 == null)
            {
                arg0 = String.Empty;
            }
    
            if (arg1==null) {
                arg1 = String.Empty;
            }
    
            if (arg2==null) {
                arg2 = String.Empty;
            }
    
            return Concat(arg0.ToString(), arg1.ToString(), arg2.ToString());
        }

        [CLSCompliant(false)] 
        public static String Concat(Object arg0, Object arg1, Object arg2, Object arg3, __arglist) 
        {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            Object[]   objArgs;
            int        argCount;
            
            ArgIterator args = new ArgIterator(__arglist);

            //+4 to account for the 4 hard-coded arguments at the beginning of the list.
            argCount = args.GetRemainingCount() + 4;
    
            objArgs = new Object[argCount];
            
            //Handle the hard-coded arguments
            objArgs[0] = arg0;
            objArgs[1] = arg1;
            objArgs[2] = arg2;
            objArgs[3] = arg3;
            
            //Walk all of the args in the variable part of the argument list.
            for (int i=4; i<argCount; i++) {
                objArgs[i] = TypedReference.ToObject(args.GetNextArg());
            }

            return Concat(objArgs);
        }

        [System.Security.SecuritySafeCritical]
        public static string Concat(params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (args.Length <= 1)
            {
                return args.Length == 0 ?
                    string.Empty :
                    args[0]?.ToString() ?? string.Empty;
            }

            // We need to get an intermediary string array
            // to fill with each of the args' ToString(),
            // and then just concat that in one operation.

            // This way we avoid any intermediary string representations,
            // or buffer resizing if we use StringBuilder (although the
            // latter case is partially alleviated due to StringBuilder's
            // linked-list style implementation)

            var strings = new string[args.Length];
            
            int totalLength = 0;

            for (int i = 0; i < args.Length; i++)
            {
                object value = args[i];

                string toString = value?.ToString() ?? string.Empty; // We need to handle both the cases when value or value.ToString() is null
                strings[i] = toString;

                totalLength += toString.Length;

                if (totalLength < 0) // Check for a positive overflow
                {
                    throw new OutOfMemoryException();
                }
            }

            // If all of the ToStrings are null/empty, just return string.Empty
            if (totalLength == 0)
            {
                return string.Empty;
            }

            string result = FastAllocateString(totalLength);
            int position = 0; // How many characters we've copied so far

            for (int i = 0; i < strings.Length; i++)
            {
                string s = strings[i];

                Contract.Assert(s != null);
                Contract.Assert(position <= totalLength - s.Length, "We didn't allocate enough space for the result string!");

                FillStringChecked(result, position, s);
                position += s.Length;
            }

            return result;
        }

        [ComVisible(false)]
        public static string Concat<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            using (IEnumerator<T> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;
                
                // We called MoveNext once, so this will be the first item
                T currentValue = en.Current;

                // Call ToString before calling MoveNext again, since
                // we want to stay consistent with the below loop
                // Everything should be called in the order
                // MoveNext-Current-ToString, unless further optimizations
                // can be made, to avoid breaking changes
                string firstString = currentValue?.ToString();

                // If there's only 1 item, simply call ToString on that
                if (!en.MoveNext())
                {
                    // We have to handle the case of either currentValue
                    // or its ToString being null
                    return firstString ?? string.Empty;
                }

                StringBuilder result = StringBuilderCache.Acquire();
                
                result.Append(firstString);

                do
                {
                    currentValue = en.Current;

                    if (currentValue != null)
                    {
                        result.Append(currentValue.ToString());
                    }
                }
                while (en.MoveNext());

                return StringBuilderCache.GetStringAndRelease(result);
            }
        }


        [ComVisible(false)]
        public static string Concat(IEnumerable<string> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;
                
                string firstValue = en.Current;

                if (!en.MoveNext())
                {
                    return firstValue ?? string.Empty;
                }

                StringBuilder result = StringBuilderCache.Acquire();
                result.Append(firstValue);

                do
                {
                    result.Append(en.Current);
                }
                while (en.MoveNext());

                return StringBuilderCache.GetStringAndRelease(result);
            }
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public static String Concat(String str0, String str1) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.Ensures(Contract.Result<String>().Length ==
                (str0 == null ? 0 : str0.Length) +
                (str1 == null ? 0 : str1.Length));
            Contract.EndContractBlock();

            if (IsNullOrEmpty(str0)) {
                if (IsNullOrEmpty(str1)) {
                    return String.Empty;
                }
                return str1;
            }

            if (IsNullOrEmpty(str1)) {
                return str0;
            }

            int str0Length = str0.Length;
            
            String result = FastAllocateString(str0Length + str1.Length);
            
            FillStringChecked(result, 0,        str0);
            FillStringChecked(result, str0Length, str1);
            
            return result;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static String Concat(String str0, String str1, String str2) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.Ensures(Contract.Result<String>().Length ==
                (str0 == null ? 0 : str0.Length) +
                (str1 == null ? 0 : str1.Length) +
                (str2 == null ? 0 : str2.Length));
            Contract.EndContractBlock();

            if (IsNullOrEmpty(str0))
            {
                return Concat(str1, str2);
            }

            if (IsNullOrEmpty(str1))
            {
                return Concat(str0, str2);
            }

            if (IsNullOrEmpty(str2))
            {
                return Concat(str0, str1);
            }

            int totalLength = str0.Length + str1.Length + str2.Length;

            String result = FastAllocateString(totalLength);
            FillStringChecked(result, 0, str0);
            FillStringChecked(result, str0.Length, str1);
            FillStringChecked(result, str0.Length + str1.Length, str2);

            return result;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static String Concat(String str0, String str1, String str2, String str3) {
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.Ensures(Contract.Result<String>().Length == 
                (str0 == null ? 0 : str0.Length) +
                (str1 == null ? 0 : str1.Length) +
                (str2 == null ? 0 : str2.Length) +
                (str3 == null ? 0 : str3.Length));
            Contract.EndContractBlock();

            if (IsNullOrEmpty(str0))
            {
                return Concat(str1, str2, str3);
            }

            if (IsNullOrEmpty(str1))
            {
                return Concat(str0, str2, str3);
            }

            if (IsNullOrEmpty(str2))
            {
                return Concat(str0, str1, str3);
            }

            if (IsNullOrEmpty(str3))
            {
                return Concat(str0, str1, str2);
            }

            int totalLength = str0.Length + str1.Length + str2.Length + str3.Length;

            String result = FastAllocateString(totalLength);
            FillStringChecked(result, 0, str0);
            FillStringChecked(result, str0.Length, str1);
            FillStringChecked(result, str0.Length + str1.Length, str2);
            FillStringChecked(result, str0.Length + str1.Length + str2.Length, str3);

            return result;
        }

        [System.Security.SecuritySafeCritical]
        public static String Concat(params String[] values) {
            if (values == null)
                throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();

            if (values.Length <= 1)
            {
                return values.Length == 0 ?
                    string.Empty :
                    values[0] ?? string.Empty;
            }

            // It's possible that the input values array could be changed concurrently on another
            // thread, such that we can't trust that each read of values[i] will be equivalent.
            // Worst case, we can make a defensive copy of the array and use that, but we first
            // optimistically try the allocation and copies assuming that the array isn't changing,
            // which represents the 99.999% case, in particular since string.Concat is used for
            // string concatenation by the languages, with the input array being a params array.

            // Sum the lengths of all input strings
            long totalLengthLong = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (value != null)
                {
                    totalLengthLong += value.Length;
                }
            }

            // If it's too long, fail, or if it's empty, return an empty string.
            if (totalLengthLong > int.MaxValue)
            {
                throw new OutOfMemoryException();
            }
            int totalLength = (int)totalLengthLong;
            if (totalLength == 0)
            {
                return string.Empty;
            }

            // Allocate a new string and copy each input string into it
            string result = FastAllocateString(totalLength);
            int copiedLength = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (!string.IsNullOrEmpty(value))
                {
                    int valueLen = value.Length;
                    if (valueLen > totalLength - copiedLength)
                    {
                        copiedLength = -1;
                        break;
                    }

                    FillStringChecked(result, copiedLength, value);
                    copiedLength += valueLen;
                }
            }

            // If we copied exactly the right amount, return the new string.  Otherwise,
            // something changed concurrently to mutate the input array: fall back to
            // doing the concatenation again, but this time with a defensive copy. This
            // fall back should be extremely rare.
            return copiedLength == totalLength ? result : Concat((string[])values.Clone());
        }
    
        public static String Format(String format, Object arg0) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(null, format, new ParamsArray(arg0));
        }
    
        public static String Format(String format, Object arg0, Object arg1) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(null, format, new ParamsArray(arg0, arg1));
        }
    
        public static String Format(String format, Object arg0, Object arg1, Object arg2) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
        }

        public static String Format(String format, params Object[] args) {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in FormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();
            
            return FormatHelper(null, format, new ParamsArray(args));
        }
        
        public static String Format(IFormatProvider provider, String format, Object arg0) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(provider, format, new ParamsArray(arg0));
        }
    
        public static String Format(IFormatProvider provider, String format, Object arg0, Object arg1) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(provider, format, new ParamsArray(arg0, arg1));
        }
    
        public static String Format(IFormatProvider provider, String format, Object arg0, Object arg1, Object arg2) {
            Contract.Ensures(Contract.Result<String>() != null);
            return FormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
        }

        public static String Format(IFormatProvider provider, String format, params Object[] args) {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in FormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.EndContractBlock();
            
            return FormatHelper(provider, format, new ParamsArray(args));
        }
        
        private static String FormatHelper(IFormatProvider provider, String format, ParamsArray args) {
            if (format == null)
                throw new ArgumentNullException("format");
            
            return StringBuilderCache.GetStringAndRelease(
                StringBuilderCache
                    .Acquire(format.Length + args.Length * 8)
                    .AppendFormatHelper(provider, format, args));
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String Insert(int startIndex, String value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (startIndex < 0 || startIndex > this.Length)
                throw new ArgumentOutOfRangeException("startIndex");
            Contract.Ensures(Contract.Result<String>() != null);
            Contract.Ensures(Contract.Result<String>().Length == this.Length + value.Length);
            Contract.EndContractBlock();
            
            int oldLength = Length;
            int insertLength = value.Length;
            
            if (oldLength == 0)
                return value;
            if (insertLength == 0)
                return this;
            
            // In case this computation overflows, newLength will be negative and FastAllocateString throws OutOfMemoryException
            int newLength = oldLength + insertLength;
            String result = FastAllocateString(newLength);
            unsafe
            {
                fixed (char* srcThis = &m_firstChar)
                {
                    fixed (char* srcInsert = &value.m_firstChar)
                    {
                        fixed (char* dst = &result.m_firstChar)
                        {
                            wstrcpy(dst, srcThis, startIndex);
                            wstrcpy(dst + startIndex, srcInsert, insertLength);
                            wstrcpy(dst + startIndex + insertLength, srcThis + startIndex, oldLength - startIndex);
                        }
                    }
                }
            }
            return result;
        }
    }
}
