// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Threading
{
    internal partial class TimerQueue
    {
        #region interface to native per-AppDomain timer

        private static int TickCount
        {
            get
            {
#if !FEATURE_PAL
                // We need to keep our notion of time synchronized with the calls to SleepEx that drive
                // the underlying native timer.  In Win8, SleepEx does not count the time the machine spends
                // sleeping/hibernating.  Environment.TickCount (GetTickCount) *does* count that time,
                // so we will get out of sync with SleepEx if we use that method.
                //
                // So, on Win8, we use QueryUnbiasedInterruptTime instead; this does not count time spent
                // in sleep/hibernate mode.
                if (Environment.IsWindows8OrAbove)
                {
                    ulong time100ns;

                    bool result = Win32Native.QueryUnbiasedInterruptTime(out time100ns);
                    if (!result)
                        throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());

                    // convert to 100ns to milliseconds, and truncate to 32 bits.
                    return (int)(uint)(time100ns / 10000);
                }
                else
#endif
                {
                    return Environment.TickCount;
                }
            }
        }

        // We use a SafeHandle to ensure that the native timer is destroyed when the AppDomain is unloaded.
        private sealed class AppDomainTimerSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public AppDomainTimerSafeHandle()
                : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return DeleteAppDomainTimer(handle);
            }
        }

        private AppDomainTimerSafeHandle m_appDomainTimer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetTimer(uint actualDuration)
        {
            if (m_appDomainTimer == null || m_appDomainTimer.IsInvalid)
            {
                Debug.Assert(!m_isAppDomainTimerScheduled);
                Debug.Assert(m_id >= 0 && m_id < Instances.Length && this == Instances[m_id]);

                m_appDomainTimer = CreateAppDomainTimer(actualDuration, m_id);
                if (!m_appDomainTimer.IsInvalid)
                {
                    m_isAppDomainTimerScheduled = true;
                    m_currentAppDomainTimerStartTicks = TickCount;
                    m_currentAppDomainTimerDuration = actualDuration;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (ChangeAppDomainTimer(m_appDomainTimer, actualDuration))
                {
                    m_isAppDomainTimerScheduled = true;
                    m_currentAppDomainTimerStartTicks = TickCount;
                    m_currentAppDomainTimerDuration = actualDuration;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // The VM calls this when a native timer fires.
        internal static void AppDomainTimerCallback(int id)
        {
            Debug.Assert(id >= 0 && id < Instances.Length && Instances[id].m_id == id);
            Instances[id].FireNextTimers();
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern AppDomainTimerSafeHandle CreateAppDomainTimer(uint dueTime, int id);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern bool ChangeAppDomainTimer(AppDomainTimerSafeHandle handle, uint dueTime);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern bool DeleteAppDomainTimer(IntPtr handle);

        #endregion
    }

    // A timer in our TimerQueue.
    internal sealed partial class TimerQueueTimer
    {
        internal void SignalNoCallbacksRunning()
        {
            object toSignal = m_notifyWhenNoCallbacksRunning;
            Debug.Assert(toSignal is WaitHandle || toSignal is Task<bool>);

            if (toSignal is WaitHandle wh)
            {
                Interop.Kernel32.SetEvent(wh.SafeWaitHandle);
            }
            else
            {
                ((Task<bool>)toSignal).TrySetResult(true);
            }
        }
    }
}
