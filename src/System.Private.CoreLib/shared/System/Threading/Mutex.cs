// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Threading
{
    /// <summary>
    /// Synchronization primitive that can also be used for interprocess synchronization
    /// </summary>
    public sealed partial class Mutex : WaitHandle
    {
        public Mutex(bool initiallyOwned, string name, out bool createdNew)
        {
            VerifyNameForCreate(name);
            CreateMutexCore(initiallyOwned, name, out createdNew);
        }

        public Mutex(bool initiallyOwned, string name)
        {
            VerifyNameForCreate(name);
            CreateMutexCore(initiallyOwned, name, out _);
        }

        public Mutex(bool initiallyOwned)
        {
            CreateMutexCore(initiallyOwned, null, out _);
        }

        public Mutex()
        {
            CreateMutexCore(false, null, out _);
        }

        private Mutex(SafeWaitHandle handle)
        {
            SafeWaitHandle = handle;
        }

        public static Mutex OpenExisting(string name)
        {
            switch (OpenExistingWorker(name, out Mutex result))
            {
                case OpenExistingResult.NameNotFound:
                    throw new WaitHandleCannotBeOpenedException();
                case OpenExistingResult.NameInvalid:
                    throw new WaitHandleCannotBeOpenedException(SR.Format(SR.Threading_WaitHandleCannotBeOpenedException_InvalidHandle, name));
                case OpenExistingResult.PathNotFound:
                    throw Win32Marshal.GetExceptionForWin32Error(Interop.Errors.ERROR_PATH_NOT_FOUND, name);

                default:
                    return result;
            }
        }

        public static bool TryOpenExisting(string name, out Mutex result) =>
            OpenExistingWorker(name, out result) == OpenExistingResult.Success;

        // Note: To call ReleaseMutex, you must have an ACL granting you
        // MUTEX_MODIFY_STATE rights (0x0001). The other interesting value
        // in a Mutex's ACL is MUTEX_ALL_ACCESS (0x1F0001).
        public void ReleaseMutex()
        {
#if CORECLR
            if (!Interop.Kernel32.ReleaseMutex(safeWaitHandle))
            {
                throw new ApplicationException(SR.Arg_SynchronizationLockException);
            }
#else
            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            SafeWaitHandle waitHandle = _waitHandle;
            if (waitHandle == null)
            {
                ThrowInvalidHandleException();
            }

            waitHandle.DangerousAddRef();
            try
            {
                ReleaseMutexCore(waitHandle);
            }
            finally
            {
                waitHandle.DangerousRelease();
            }
#endif
        }
    }
}
