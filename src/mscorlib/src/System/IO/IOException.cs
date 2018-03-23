// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Exception for a generic IO error.
**
**
===========================================================*/

using System;
using System.Runtime.Serialization;

namespace System.IO
{
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class IOException : SystemException
    {
        // For debugging purposes, store the complete path in the IOException
        // if possible.  Don't give it back to users due to security concerns.
        // Let the code that throws the exception worry about that.  But we can
        // at least assist people attached to the process with a managed 
        // debugger.  Don't serialize it to avoid any security problems.
        // This information isn't guaranteed to be correct, but is our second 
        // best effort at a file or directory involved, after the exception 
        // message.
        [NonSerialized]
        private String _maybeFullPath;  // For debuggers on partial trust code

        public IOException()
            : base(SR.Arg_IOException)
        {
            HResult = __HResults.COR_E_IO;
        }

        public IOException(String message)
            : base(message)
        {
            HResult = __HResults.COR_E_IO;
        }

        public IOException(String message, int hresult)
            : base(message)
        {
            HResult = hresult;
        }

        // Adding this for debuggers when looking at exceptions in partial
        // trust code that may not have interesting path information in
        // the exception message.
        internal IOException(String message, int hresult, String maybeFullPath)
            : base(message)
        {
            HResult = hresult;
            _maybeFullPath = maybeFullPath;
        }

        public IOException(String message, Exception innerException)
            : base(message, innerException)
        {
            HResult = __HResults.COR_E_IO;
        }

        protected IOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
