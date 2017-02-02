// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Exposes routines for enumerating through a 
** directory.
**
**          April 11,2000
**
===========================================================*/

using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace System.IO
{
    [ComVisible(true)]
    internal static class Directory {

        internal static String InternalGetDirectoryRoot(String path) {
              if (path == null) return null;
            return path.Substring(0, PathInternal.GetRootLength(path));
        }

         /*===============================CurrentDirectory===============================
        **Action:  Provides a getter and setter for the current directory.  The original
        **         current DirectoryInfo is the one from which the process was started.  
        **Returns: The current DirectoryInfo (from the getter).  Void from the setter.
        **Arguments: The current DirectoryInfo to which to switch to the setter.
        **Exceptions: 
        ==============================================================================*/
        public static String GetCurrentDirectory()
        {
            // Start with a buffer the size of MAX_PATH
            StringBuffer buffer = new StringBuffer(260);
            try
            {
                uint result = 0;
                while ((result = Win32Native.GetCurrentDirectoryW((uint)buffer.Capacity, buffer.UnderlyingArray)) > buffer.Capacity)
                {
                    // Reported size is greater than the buffer size. Increase the capacity.
                    // The size returned includes the null only if more space is needed (this case).
                    buffer.EnsureCapacity(checked((int)result));
                }

                if (result == 0)
                    __Error.WinIOError();

                buffer.Length = (int)result;

#if !PLATFORM_UNIX
                if (buffer.Contains('~'))
                    return Path.GetFullPath(buffer.ToString());
#endif

                return buffer.ToString();
            }
            finally
            {
                buffer.Free();
            }
        }

        public static void SetCurrentDirectory(String path)
        {
            if (path==null)
                throw new ArgumentNullException(nameof(path));
            if (path.Length==0)
                throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"));
            if (path.Length >= Path.MaxPath)
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));

            String fulldestDirName = Path.GetFullPath(path);
            
            if (!Win32Native.SetCurrentDirectory(fulldestDirName)) {
                // If path doesn't exist, this sets last error to 2 (File 
                // not Found).  LEGACY: This may potentially have worked correctly
                // on Win9x, maybe.
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == Win32Native.ERROR_FILE_NOT_FOUND)
                    errorCode = Win32Native.ERROR_PATH_NOT_FOUND;
                __Error.WinIOError(errorCode, fulldestDirName);
            }
        }
    }
}

