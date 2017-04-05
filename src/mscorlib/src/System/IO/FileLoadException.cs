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
    [Serializable]
    public class FileLoadException : IOException
    {
        public FileLoadException()
            : base(SR.IO_FileLoad)
        {
            SetErrorCode(__HResults.COR_E_FILELOAD);
        }

        public FileLoadException(string message)
            : base(message)
        {
            SetErrorCode(__HResults.COR_E_FILELOAD);
        }

        public FileLoadException(string message, Exception inner)
            : base(message, inner)
        {
            SetErrorCode(__HResults.COR_E_FILELOAD);
        }

        public FileLoadException(string message, string fileName) : base(message)
        {
            SetErrorCode(__HResults.COR_E_FILELOAD);
            FileName = fileName;
        }

        public FileLoadException(string message, string fileName, Exception inner)
            : base(message, inner)
        {
            SetErrorCode(__HResults.COR_E_FILELOAD);
            FileName = fileName;
        }

        public override string Message
        {
            get
            {
                SetMessageField();
                return _message;
            }
        }

        private void SetMessageField()
        {
            if (_message == null)
                _message = FormatFileLoadExceptionMessage(FileName, HResult);
        }

        public string FileName { get; }

        public override string ToString()
        {
            string s = GetType().ToString() + ": " + Message;

            if (FileName != null && FileName.Length != 0)
                s += Environment.NewLine + SR.Format(SR.IO_FileName_Name, FileName);

            if (InnerException != null)
                s = s + " ---> " + InnerException.ToString();

            if (StackTrace != null)
                s += Environment.NewLine + StackTrace;

            try
            {
                if (FusionLog != null)
                {
                    if (s == null)
                        s = " ";
                    s += Environment.NewLine;
                    s += Environment.NewLine;
                    s += FusionLog;
                }
            }
            catch (SecurityException)
            {
            }

            return s;
        }

        protected FileLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Base class constructor will check info != null.

            FileName = info.GetString("FileLoad_FileName");

            try
            {
                FusionLog = info.GetString("FileLoad_FusionLog");
            }
            catch
            {
                FusionLog = null;
            }
        }

        private FileLoadException(string fileName, string fusionLog, int hResult)
            : base(null)
        {
            SetErrorCode(hResult);
            FileName = fileName;
            FusionLog = fusionLog;
            SetMessageField();
        }

        public string FusionLog { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize data for our base classes.  base will verify info != null.
            base.GetObjectData(info, context);

            // Serialize data for this class
            info.AddValue("FileLoad_FileName", FileName, typeof(string));

            try
            {
                info.AddValue("FileLoad_FusionLog", FusionLog, typeof(string));
            }
            catch (SecurityException)
            {
            }
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
