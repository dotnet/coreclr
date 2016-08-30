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
    }
}
