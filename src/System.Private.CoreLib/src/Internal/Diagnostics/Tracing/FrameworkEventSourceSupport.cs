// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Internal.Diagnostics.Tracing
{
    //
    // An internal contract that exposes FrameworkEventSourceSupport(ETW) support to System.Runtime.WindowsRuntime.dll
    //
    public static class FrameworkEventSourceSupport
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnabled(EventLevel level, EventKeywords keywords)
        {
            return FrameworkEventSource.Log.IsEnabled(level, keywords);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThreadTransferSendObj(object id, int kind, string info, bool multiDequeues)
        {
            FrameworkEventSource.Log.ThreadTransferSendObj(id, kind, info, multiDequeues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThreadTransferReceiveObj(object id, int kind, string info)
        {
            FrameworkEventSource.Log.ThreadTransferReceiveObj(id, kind, info);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThreadTransferReceiveHandledObj(object id, int kind, string info)
        {
            FrameworkEventSource.Log.ThreadTransferReceiveHandledObj(id, kind, info);
        }
    }
}

