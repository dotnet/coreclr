// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Threading
{
    public partial class SynchronizationContext
    {
        private static int InvokeWaitMethodHelper(SynchronizationContext syncContext, IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return syncContext.Wait(waitHandles, waitAll, millisecondsTimeout);
        }

        // Static helper to which the above method can delegate to in order to get the default
        // COM behavior.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int WaitHelperNative(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);

    }
}
