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
** Purpose: Exception for accessing a file that doesn't exist.
**
**
===========================================================*/

using System;
using System.Runtime.Serialization;
using SecurityException = System.Security.SecurityException;
using System.Globalization;

namespace System.IO
{
    // Thrown when trying to access a file that doesn't exist on disk.
    [Serializable]
    public class FileNotFoundException : IOException
    {
        private String _fileName;  // The name of the file that isn't found.
        private String _fusionLog;  // fusion log (when applicable)

        public FileNotFoundException()
            : base(SR.IO_FileNotFound)
        {
            SetErrorCode(__HResults.COR_E_FILENOTFOUND);
        }

        public FileNotFoundException(String message)
            : base(message)
        {
            SetErrorCode(__HResults.COR_E_FILENOTFOUND);
        }

        public FileNotFoundException(String message, Exception innerException)
            : base(message, innerException)
        {
            SetErrorCode(__HResults.COR_E_FILENOTFOUND);
        }

        public FileNotFoundException(String message, String fileName) : base(message)
        {
            SetErrorCode(__HResults.COR_E_FILENOTFOUND);
            _fileName = fileName;
        }

        public FileNotFoundException(String message, String fileName, Exception innerException)
            : base(message, innerException)
        {
            SetErrorCode(__HResults.COR_E_FILENOTFOUND);
            _fileName = fileName;
        }

        public override String Message
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
            {
                if ((_fileName == null) &&
                    (HResult == System.__HResults.COR_E_EXCEPTION))
                    _message = SR.IO_FileNotFound;

                else if (_fileName != null)
                    _message = FileLoadException.FormatFileLoadExceptionMessage(_fileName, HResult);
            }
        }

        public String FileName
        {
            get { return _fileName; }
        }

        public override String ToString()
        {
            String s = GetType().FullName + ": " + Message;

            if (_fileName != null && _fileName.Length != 0)
                s += Environment.NewLine + SR.Format(SR.IO_FileName_Name, _fileName);

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

        protected FileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Base class constructor will check info != null.

            _fileName = info.GetString("FileNotFound_FileName");
            try
            {
                _fusionLog = info.GetString("FileNotFound_FusionLog");
            }
            catch
            {
                _fusionLog = null;
            }
        }

        private FileNotFoundException(String fileName, String fusionLog, int hResult)
            : base(null)
        {
            SetErrorCode(hResult);
            _fileName = fileName;
            _fusionLog = fusionLog;
            SetMessageField();
        }

        public String FusionLog
        {
            get { return _fusionLog; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize data for our base classes.  base will verify info != null.
            base.GetObjectData(info, context);

            // Serialize data for this class
            info.AddValue("FileNotFound_FileName", _fileName, typeof(String));

            try
            {
                info.AddValue("FileNotFound_FusionLog", FusionLog, typeof(String));
            }
            catch (SecurityException)
            {
            }
        }
    }
}

