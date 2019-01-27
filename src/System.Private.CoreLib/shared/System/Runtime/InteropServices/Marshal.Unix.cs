// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace System.Runtime.InteropServices
{
    public static partial class Marshal
    {
        private static bool IsWin32Atom(IntPtr ptr) => false;

        internal static unsafe int ConvertStringToAnsi(string s, byte* pbNativeBuffer, int cbNativeBuffer)
        {
            fixed (char* firstChar = &s)
            {
                int convertedBytes = Encoding.UTF8.GetBytes(s, s.Length, pbNativeBuffer, cbNativeBuffer);
                pbNativeBuffer[convertedBytes] = 0;
            }
        }
    }
}