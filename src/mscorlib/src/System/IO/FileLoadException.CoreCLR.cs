// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
** 
** 
**
**
** Purpose: Exception for failure to load a file that was successfully found.
**
**
===========================================================*/

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;
using System.Runtime.Versioning;
using SecurityException = System.Security.SecurityException;

namespace System.IO
{
    public partial class FileLoadException
    {
        // Do not delete: this is invoked from native code.
        private FileLoadException(string fileName, string fusionLog, int hResult)
            : base(null)
        {
            SetErrorCode(hResult);
            FileName = fileName;
            FusionLog = fusionLog;
            _message = FormatFileLoadExceptionMessage(FileName, HResult);
        }

        internal static string FormatFileLoadExceptionMessage(string fileName,
            int hResult)
        {
            string format = null;
            GetFileLoadExceptionMessage(hResult, JitHelpers.GetStringHandleOnStack(ref format));

            string message = null;
            GetMessageForHR(hResult, JitHelpers.GetStringHandleOnStack(ref message));

            return string.Format(CultureInfo.CurrentCulture, format, fileName, message);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetFileLoadExceptionMessage(int hResult, StringHandleOnStack retString);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetMessageForHR(int hresult, StringHandleOnStack retString);
    }
}
