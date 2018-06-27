// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using System.Threading;

namespace Microsoft.Win32.SafeHandles
{
    public sealed class SafeWaitHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Called by P/Invoke marshaler
        private SafeWaitHandle() : base(true)
        {
        }

        public SafeWaitHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
#if !CORECLR && !PLATFORM_WINDOWS
            WaitSubsystem.DeleteHandle(handle);
		    return true;
#else
            return Interop.Kernel32.CloseHandle(handle);
#endif
        }
    }
}
