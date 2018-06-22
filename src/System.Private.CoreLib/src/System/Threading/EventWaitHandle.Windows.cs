// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
    public partial class EventWaitHandle
    {
        private const uint AccessRights = (uint)Win32Native.MAXIMUM_ALLOWED | Win32Native.SYNCHRONIZE | Win32Native.EVENT_MODIFY_STATE;

        private EventWaitHandle(SafeWaitHandle handle)
        {
            SafeWaitHandle = handle;
        }

        private void CreateEventCore(bool initialState, EventResetMode mode, string name, out bool createdNew)
        {
#if !PLATFORM_WINDOWS
            if (name != null)
            {
                throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);
            }
#endif
            uint eventFlags = initialState ? Win32Native.CREATE_EVENT_INITIAL_SET : 0;
            if (mode == EventResetMode.ManualReset)
            {
                eventFlags |= (uint)Interop.Constants.CreateEventManualReset;
            }

            SafeWaitHandle _handle = Win32Native.CreateEventEx(null, name, eventFlags, AccessRights);

            int errorCode = Marshal.GetLastWin32Error();
            if (_handle.IsInvalid)
            {
                _handle.SetHandleAsInvalid();
                if (null != name && 0 != name.Length && Interop.Errors.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(SR.Format(SR.Threading_WaitHandleCannotBeOpenedException_InvalidHandle, name));

                throw Win32Marshal.GetExceptionForWin32Error(errorCode, name);
            }
            createdNew = errorCode != Interop.Errors.ERROR_ALREADY_EXISTS;
            SafeWaitHandle = _handle;
        }

        private static OpenExistingResult OpenExistingWorker(string name, out EventWaitHandle result)
        {
#if PLATFORM_WINDOWS
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException(SR.Argument_EmptyName, nameof(name));
            }

            result = null;
            SafeWaitHandle myHandle = Win32Native.OpenEvent(AccessRights, false, name);

            if (myHandle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();

                if (Interop.Errors.ERROR_FILE_NOT_FOUND == errorCode || Interop.Errors.ERROR_INVALID_NAME == errorCode)
                    return OpenExistingResult.NameNotFound;
                if (Interop.Errors.ERROR_PATH_NOT_FOUND == errorCode)
                    return OpenExistingResult.PathNotFound;
                if (null != name && 0 != name.Length && Interop.Errors.ERROR_INVALID_HANDLE == errorCode)
                    return OpenExistingResult.NameInvalid;
                //this is for passed through Win32Native Errors
                throw Win32Marshal.GetExceptionForWin32Error(errorCode, "");
            }
            result = new EventWaitHandle(myHandle);
            return OpenExistingResult.Success;
#else
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);
#endif
        }

        public bool Reset()
        {
            bool res = Win32Native.ResetEvent(_waitHandle);
            if (!res)
                throw Win32Marshal.GetExceptionForLastWin32Error();
            return res;
        }
        
        public bool Set()
        {
            bool res = Win32Native.SetEvent(_waitHandle);

            if (!res)
                throw Win32Marshal.GetExceptionForLastWin32Error();
            return res;
        }
    }
}
