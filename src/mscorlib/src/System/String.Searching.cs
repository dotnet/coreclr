// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System
{
    public partial class String
    {
        [Pure]
        public bool Contains( string value ) {
            return ( IndexOf(value, StringComparison.Ordinal) >=0 );
        }
    
        // Returns the index of the first occurrence of a specified character in the current instance.
        // The search starts at startIndex and runs thorough the next count characters.
        //
        [Pure]
        public int IndexOf(char value) {
            return IndexOf(value, 0, this.Length);
        }

        [Pure]
        public int IndexOf(char value, int startIndex) {
            return IndexOf(value, startIndex, this.Length - startIndex);
        }

        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe int IndexOf(char value, int startIndex, int count) {
            if (startIndex < 0 || startIndex > Length)
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));

            if (count < 0 || count > Length - startIndex)
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));

            fixed (char* pChars = &m_firstChar)
            {
                char* pCh = pChars + startIndex;

                while (count >= 4)
                {
                    if (*pCh == value) goto ReturnIndex;
                    if (*(pCh + 1) == value) goto ReturnIndex1;
                    if (*(pCh + 2) == value) goto ReturnIndex2;
                    if (*(pCh + 3) == value) goto ReturnIndex3;

                    count -= 4;
                    pCh += 4;
                }

                while (count > 0)
                {
                    if (*pCh == value)
                        goto ReturnIndex;

                    count--;
                    pCh++;
                }

                return -1;

                ReturnIndex3: pCh++;
                ReturnIndex2: pCh++;
                ReturnIndex1: pCh++;
                ReturnIndex:
                return (int)(pCh - pChars);
            }
        }
    
        // Returns the index of the first occurrence of any specified character in the current instance.
        // The search starts at startIndex and runs to startIndex + count -1.
        //
        [Pure]        
        public int IndexOfAny(char [] anyOf) {
            return IndexOfAny(anyOf,0, this.Length);
        }
    
        [Pure]
        public int IndexOfAny(char [] anyOf, int startIndex) {
            return IndexOfAny(anyOf, startIndex, this.Length - startIndex);
        }
    
        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char [] anyOf, int startIndex, int count);
    
        
        // Determines the position within this string of the first occurrence of the specified
        // string, according to the specified search criteria.  The search begins at
        // the first character of this string, it is case-sensitive and the current culture
        // comparison is used.
        //
        [Pure]
        public int IndexOf(String value) {
            return IndexOf(value, StringComparison.CurrentCulture);
        }

        // Determines the position within this string of the first occurrence of the specified
        // string, according to the specified search criteria.  The search begins at
        // startIndex, it is case-sensitive and the current culture comparison is used.
        //
        [Pure]
        public int IndexOf(String value, int startIndex) {
            return IndexOf(value, startIndex, StringComparison.CurrentCulture);
        }

        // Determines the position within this string of the first occurrence of the specified
        // string, according to the specified search criteria.  The search begins at
        // startIndex, ends at endIndex and the current culture comparison is used.
        //
        [Pure]
        public int IndexOf(String value, int startIndex, int count) {
            if (startIndex < 0 || startIndex > this.Length) {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            if (count < 0 || count > this.Length - startIndex) {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            Contract.EndContractBlock();
            
            return IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
        }

        [Pure]
        public int IndexOf(String value, StringComparison comparisonType) {
            return IndexOf(value, 0, this.Length, comparisonType);
        }

        [Pure]
        public int IndexOf(String value, int startIndex, StringComparison comparisonType) {
            return IndexOf(value, startIndex, this.Length - startIndex, comparisonType);
        }

        [Pure]
        [System.Security.SecuritySafeCritical]
        public int IndexOf(String value, int startIndex, int count, StringComparison comparisonType) {
            // Validate inputs
            if (value == null)
                throw new ArgumentNullException("value");

            if (startIndex < 0 || startIndex > this.Length)
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));

            if (count < 0 || startIndex > this.Length - count)
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            Contract.EndContractBlock();

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.Ordinal);

                case StringComparison.OrdinalIgnoreCase:
                    if (value.IsAscii() && this.IsAscii())
                        return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
                    else
                        return TextInfo.IndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);

                default:
                    throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }  
        }
    }
}
