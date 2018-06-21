// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Threading
{
    public sealed partial class Semaphore : WaitHandle
    {
        private const uint AccessRights = (uint)Win32Native.MAXIMUM_ALLOWED | Win32Native.SYNCHRONIZE | Win32Native.SEMAPHORE_MODIFY_STATE;

        private Semaphore(SafeWaitHandle handle)
        {
            SafeWaitHandle = handle;
        }

        private void CreateSemaphoreCore(int initialCount, int maximumCount, string name, out bool createdNew)
        {
            SafeWaitHandle myHandle = CreateSemaphore(initialCount, maximumCount, name);

            int errorCode = Marshal.GetLastWin32Error();
            if (myHandle.IsInvalid)
            {
                if (null != name && 0 != name.Length && Interop.Errors.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(
                        SR.Format(SR.Threading_WaitHandleCannotBeOpenedException_InvalidHandle, name));

                throw Win32Marshal.GetExceptionForLastWin32Error();
            }
            createdNew = errorCode != Interop.Errors.ERROR_ALREADY_EXISTS;
            this.SafeWaitHandle = myHandle;
        }

        private static SafeWaitHandle CreateSemaphore(int initialCount, int maximumCount, string name)
        {
#if !PLATFORM_WINDOWS
            if (name != null)
            {
                throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);
            }
#endif

            Debug.Assert(initialCount >= 0);
            Debug.Assert(maximumCount >= 1);
            Debug.Assert(initialCount <= maximumCount);

            return Win32Native.CreateSemaphoreEx(null, initialCount, maximumCount, name, 0, AccessRights);
        }

        private static OpenExistingResult OpenExistingWorker(string name, out Semaphore result)
        {
#if PLATFORM_WINDOWS
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyName, nameof(name));

            //Pass false to OpenSemaphore to prevent inheritedHandles
            SafeWaitHandle myHandle = Win32Native.OpenSemaphore(AccessRights, false, name);

            if (myHandle.IsInvalid)
            {
                result = null;

                int errorCode = Marshal.GetLastWin32Error();

                if (Interop.Errors.ERROR_FILE_NOT_FOUND == errorCode || Interop.Errors.ERROR_INVALID_NAME == errorCode)
                    return OpenExistingResult.NameNotFound;
                if (Interop.Errors.ERROR_PATH_NOT_FOUND == errorCode)
                    return OpenExistingResult.PathNotFound;
                if (null != name && 0 != name.Length && Interop.Errors.ERROR_INVALID_HANDLE == errorCode)
                    return OpenExistingResult.NameInvalid;
                //this is for passed through NativeMethods Errors
                throw Win32Marshal.GetExceptionForLastWin32Error();
            }

            result = new Semaphore(myHandle);
            return OpenExistingResult.Success;
#else
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);
#endif
        }

        private int ReleaseCore(int releaseCount)
        {
            // If ReleaseSempahore returns false when the specified value would cause
            // the semaphore's count to exceed the maximum count set when Semaphore was created
            // Non-Zero return 
            int previousCount;
            if (!Win32Native.ReleaseSemaphore(SafeWaitHandle, releaseCount, out previousCount))
            {
                throw new SemaphoreFullException();
            }

            return previousCount;
        }

    }
}
