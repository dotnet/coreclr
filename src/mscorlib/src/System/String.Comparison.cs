// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public partial class String
    {
        //
        //Native Static Methods
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        private unsafe static int CompareOrdinalIgnoreCaseHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);
            Contract.EndContractBlock();
            int length = Math.Min(strA.Length, strB.Length);
    
            fixed (char* ap = &strA.m_firstChar) fixed (char* bp = &strB.m_firstChar)
            {
                char* a = ap;
                char* b = bp;

                while (length != 0) 
                {
                    int charA = *a;
                    int charB = *b;

                    Contract.Assert((charA | charB) <= 0x7F, "strings have to be ASCII");

                    // uppercase both chars - notice that we need just one compare per char
                    if ((uint)(charA - 'a') <= (uint)('z' - 'a')) charA -= 0x20;
                    if ((uint)(charB - 'a') <= (uint)('z' - 'a')) charB -= 0x20;

                    //Return the (case-insensitive) difference between them.
                    if (charA != charB)
                        return charA - charB;

                    // Next char
                    a++; b++;
                    length--;
                }

                return strA.Length - strB.Length;
            }
        }

        // native call to COMString::CompareOrdinalEx
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int CompareOrdinalHelper(String strA, int indexA, int countA, String strB, int indexB, int countB);

        //This will not work in case-insensitive mode for any character greater than 0x80.  
        //We'll throw an ArgumentException.
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        unsafe internal static extern int nativeCompareOrdinalIgnoreCaseWC(String strA, sbyte *strBBytes);

        //
        //
        // NATIVE INSTANCE METHODS
        //
        //
    
        //
        // Search/Query methods
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private unsafe static bool EqualsHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);
            Contract.Requires(strA.Length == strB.Length);

            int length = strA.Length;

            fixed (char* ap = &strA.m_firstChar) fixed (char* bp = &strB.m_firstChar)
            {
                char* a = ap;
                char* b = bp;

#if BIT64
                // Single int read aligns pointers for the following long reads
                // PERF: No length check needed as there is always an int32 worth of string allocated
                //       This read can also include the null terminator which both strings will have
                if (*(int*)a != *(int*)b) return false;
                length -= 2; a += 2; b += 2;

                // for AMD64 bit platform we unroll by 12 and
                // check 3 qword at a time. This is less code
                // than the 32 bit case and is a shorter path length.

                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto ReturnFalse;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto ReturnFalse;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto ReturnFalse;
                    length -= 12; a += 12; b += 12;
                }
#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) goto ReturnFalse;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) goto ReturnFalse;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) goto ReturnFalse;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) goto ReturnFalse;
                    length -= 10; a += 10; b += 10;
                }
#endif

                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0) 
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    length -= 2; a += 2; b += 2;
                }

                return true;

                ReturnFalse:
                return false;
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private unsafe static bool StartsWithOrdinalHelper(String str, String startsWith)
        {
            Contract.Requires(str != null);
            Contract.Requires(startsWith != null);
            Contract.Requires(str.Length >= startsWith.Length);

            int length = startsWith.Length;

            fixed (char* ap = &str.m_firstChar) fixed (char* bp = &startsWith.m_firstChar)
            {
                char* a = ap;
                char* b = bp;

#if BIT64
                // Single int read aligns pointers for the following long reads
                // No length check needed as this method is called when length >= 2
                Contract.Assert(length >= 2);
                if (*(int*)a != *(int*)b) goto ReturnFalse;
                length -= 2; a += 2; b += 2;

                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto ReturnFalse;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto ReturnFalse;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto ReturnFalse;
                    length -= 12; a += 12; b += 12;
                }
#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    if (*(int*)(a+2) != *(int*)(b+2)) goto ReturnFalse;
                    if (*(int*)(a+4) != *(int*)(b+4)) goto ReturnFalse;
                    if (*(int*)(a+6) != *(int*)(b+6)) goto ReturnFalse;
                    if (*(int*)(a+8) != *(int*)(b+8)) goto ReturnFalse;
                    length -= 10; a += 10; b += 10;
                }
#endif

                while (length >= 2)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    length -= 2; a += 2; b += 2;
                }

                // PERF: This depends on the fact that the String objects are always zero terminated 
                // and that the terminating zero is not included in the length. For even string sizes
                // this compare can include the zero terminator. Bitwise OR avoids a branch.
                return length == 0 | *a == *b;

                ReturnFalse:
                return false;
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        private unsafe static int CompareOrdinalHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);

            // NOTE: This may be subject to change if eliminating the check
            // in the callers makes them small enough to be inlined by the JIT
            Contract.Assert(strA.m_firstChar == strB.m_firstChar,
                "For performance reasons, callers of this method should " +
                "check/short-circuit beforehand if the first char is the same.");

            int length = Math.Min(strA.Length, strB.Length);

            fixed (char* ap = &strA.m_firstChar) fixed (char* bp = &strB.m_firstChar)
            {
                char* a = ap;
                char* b = bp;

                // Check if the second chars are different here
                // The reason we check if m_firstChar is different is because
                // it's the most common case and allows us to avoid a method call
                // to here.
                // The reason we check if the second char is different is because
                // if the first two chars the same we can increment by 4 bytes,
                // leaving us word-aligned on both 32-bit (12 bytes into the string)
                // and 64-bit (16 bytes) platforms.
        
                // For empty strings, the second char will be null due to padding.
                // The start of the string (not including sync block pointer)
                // is the method table pointer + string length, which takes up
                // 8 bytes on 32-bit, 12 on x64. For empty strings the null
                // terminator immediately follows, leaving us with an object
                // 10/14 bytes in size. Since everything needs to be a multiple
                // of 4/8, this will get padded and zeroed out.
                
                // For one-char strings the second char will be the null terminator.

                // NOTE: If in the future there is a way to read the second char
                // without pinning the string (e.g. System.Runtime.CompilerServices.Unsafe
                // is exposed to mscorlib, or a future version of C# allows inline IL),
                // then do that and short-circuit before the fixed.

                if (*(a + 1) != *(b + 1)) goto DiffOffset1;
                
                // Since we know that the first two chars are the same,
                // we can increment by 2 here and skip 4 bytes.
                // This leaves us 8-byte aligned, which results
                // on better perf for 64-bit platforms.
                length -= 2; a += 2; b += 2;

                // unroll the loop
#if BIT64
                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto DiffOffset0;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto DiffOffset4;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto DiffOffset8;
                    length -= 12; a += 12; b += 12;
                }
#else // BIT64
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto DiffOffset0;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) goto DiffOffset2;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) goto DiffOffset4;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) goto DiffOffset6;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) goto DiffOffset8;
                    length -= 10; a += 10; b += 10; 
                }
#endif // BIT64

                // Fallback loop:
                // go back to slower code path and do comparison on 4 bytes at a time.
                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0)
                {
                    if (*(int*)a != *(int*)b) goto DiffNextInt;
                    length -= 2;
                    a += 2; 
                    b += 2; 
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return strA.Length - strB.Length;
                
#if BIT64
                DiffOffset8: a += 4; b += 4;
                DiffOffset4: a += 4; b += 4;
#else // BIT64
                // Use jumps instead of falling through, since
                // otherwise going to DiffOffset8 will involve
                // 8 add instructions before getting to DiffNextInt
                DiffOffset8: a += 8; b += 8; goto DiffOffset0;
                DiffOffset6: a += 6; b += 6; goto DiffOffset0;
                DiffOffset4: a += 2; b += 2;
                DiffOffset2: a += 2; b += 2;
#endif // BIT64
                
                DiffOffset0:
                // If we reached here, we already see a difference in the unrolled loop above
#if BIT64
                if (*(int*)a == *(int*)b)
                {
                    a += 2; b += 2;
                }
#endif // BIT64

                DiffNextInt:
                if (*a != *b) return *a - *b;

                DiffOffset1:
                Contract.Assert(*(a + 1) != *(b + 1), "This char must be different if we reach here!");
                return *(a + 1) - *(b + 1);
            }
        }
    
        // Provides a culture-correct string comparison. StrA is compared to StrB
        // to determine whether it is lexicographically less, equal, or greater, and then returns
        // either a negative integer, 0, or a positive integer; respectively.
        //
        [Pure]
        public static int Compare(String strA, String strB)
        {
            return Compare(strA, strB, StringComparison.CurrentCulture);
        }
    

        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        // The case-sensitive option is set by ignoreCase
        //
        [Pure]
        public static int Compare(String strA, String strB, bool ignoreCase)
        {
            var comparisonType = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            return Compare(strA, strB, comparisonType);
        }

  
        // Provides a more flexible function for string comparision. See StringComparison 
        // for meaning of different comparisonType.
        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static int Compare(String strA, String strB, StringComparison comparisonType) 
        {
            // Single comparison to check if comparisonType is within [CurrentCulture .. OrdinalIgnoreCase]
            if ((uint)(comparisonType - StringComparison.CurrentCulture) > (uint)(StringComparison.OrdinalIgnoreCase - StringComparison.CurrentCulture))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            Contract.EndContractBlock();

            if (object.ReferenceEquals(strA, strB))
            {
                return 0;
            }

            // They can't both be null at this point.
            if (strA == null)
            {
                return -1;
            }
            if (strB == null)
            {
                return 1;
            }

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    // Most common case: first character is different.
                    // Returns false for empty strings.
                    if (strA.m_firstChar != strB.m_firstChar)
                    {
                        return strA.m_firstChar - strB.m_firstChar;
                    }

                    return CompareOrdinalHelper(strA, strB);

                case StringComparison.OrdinalIgnoreCase:
                    // If both strings are ASCII strings, we can take the fast path.
                    if (strA.IsAscii() && strB.IsAscii()) {
                        return (CompareOrdinalIgnoreCaseHelper(strA, strB));
                    }

#if FEATURE_COREFX_GLOBALIZATION
                    return CompareInfo.CompareOrdinalIgnoreCase(strA, 0, strA.Length, strB, 0, strB.Length);
#else
                    // Take the slow path.
                    return TextInfo.CompareOrdinalIgnoreCase(strA, strB);
#endif

                default:
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_StringComparison"));
            }
        }


        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        //
        [Pure]
        public static int Compare(String strA, String strB, CultureInfo culture, CompareOptions options) {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            Contract.EndContractBlock();

            return culture.CompareInfo.Compare(strA, strB, options);
        }



        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        // The case-sensitive option is set by ignoreCase, and the culture is set
        // by culture
        //
        [Pure]
        public static int Compare(String strA, String strB, bool ignoreCase, CultureInfo culture)
        {
            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return Compare(strA, strB, culture, options);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length count is compared with the substring of strB
        // beginning at indexB of the same length.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length)
        {
            // NOTE: It's important we call the boolean overload, and not the StringComparison
            // one. The two have some subtly different behavior (see notes in the former).
            return Compare(strA, indexA, strB, indexB, length, ignoreCase: false);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length count is compared with the substring of strB
        // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, bool ignoreCase)
        {
            // Ideally we would just forward to the string.Compare overload that takes
            // a StringComparison parameter, and just pass in CurrentCulture/CurrentCultureIgnoreCase.
            // That function will return early if an optimization can be applied, e.g. if
            // (object)strA == strB && indexA == indexB then it will return 0 straightaway.
            // There are a couple of subtle behavior differences that prevent us from doing so
            // however:
            // - string.Compare(null, -1, null, -1, -1, StringComparison.CurrentCulture) works
            //   since that method also returns early for nulls before validation. It shouldn't
            //   for this overload.
            // - Since we originally forwarded to CompareInfo.Compare for all of the argument
            //   validation logic, the ArgumentOutOfRangeExceptions thrown will contain different
            //   parameter names.
            // Therefore, we have to duplicate some of the logic here.

            int lengthA = length;
            int lengthB = length;
            
            if (strA != null)
            {
                lengthA = Math.Min(lengthA, strA.Length - indexA);
            }

            if (strB != null)
            {
                lengthB = Math.Min(lengthB, strB.Length - indexB);
            }

            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, options);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length length is compared with the substring of strB
        // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean,
        // and the culture is set by culture.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
        {
            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return Compare(strA, indexA, strB, indexB, length, culture, options);
        }


        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length length is compared with the substring of strB
        // beginning at indexB of the same length.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            Contract.EndContractBlock();

            int lengthA = length;
            int lengthB = length;

            if (strA != null)
            {
                lengthA = Math.Min(lengthA, strA.Length - indexA);
            }

            if (strB != null)
            {
                lengthB = Math.Min(lengthB, strB.Length - indexB);
            }
    
            return culture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, options);
        }

        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, StringComparison comparisonType) {
            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase) {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            Contract.EndContractBlock();
            
            if (strA == null || strB == null)
            {
                if (object.ReferenceEquals(strA, strB))
                {
                    // They're both null
                    return 0;
                }

                return strA == null ? -1 : 1;
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }

            if (indexA < 0 || indexB < 0)
            {
                string paramName = indexA < 0 ? "indexA" : "indexB";
                throw new ArgumentOutOfRangeException(paramName, Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            if (strA.Length - indexA < 0 || strB.Length - indexB < 0)
            {
                string paramName = strA.Length - indexA < 0 ? "indexA" : "indexB";
                throw new ArgumentOutOfRangeException(paramName, Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            if (length == 0 || (object.ReferenceEquals(strA, strB) && indexA == indexB))
            {
                return 0;
            }

            int lengthA = Math.Min(length, strA.Length - indexA);
            int lengthB = Math.Min(length, strB.Length - indexB);

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CompareOrdinalHelper(strA, indexA, lengthA, strB, indexB, lengthB);

                case StringComparison.OrdinalIgnoreCase:
#if FEATURE_COREFX_GLOBALIZATION
                    return (CompareInfo.CompareOrdinalIgnoreCase(strA, indexA, lengthA, strB, indexB, lengthB));
#else
                    return (TextInfo.CompareOrdinalIgnoreCaseEx(strA, indexA, strB, indexB, lengthA, lengthB));
#endif

                default:
                    throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"));
            }

        }
    }
}