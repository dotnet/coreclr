// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Threading
{
    public abstract partial class WaitHandle
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int WaitOneCore(IntPtr waitHandle, int millisecondsTimeout);
 
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern int WaitMultipleIgnoringSyncContext(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);
 
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int SignalAndWaitCore(IntPtr waitHandleToSignal, IntPtr waitHandleToWaitOn, int millisecondsTimeout);
    }
}
