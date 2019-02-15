// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
    public abstract partial class WaitHandle : MarshalByRefObject, IDisposable
    {
        private const int WAIT_OBJECT_0 = 0;
        private const int WAIT_ABANDONED = 0x80;
        private const int WAIT_FAILED = 0x7FFFFFFF;
        private const int ERROR_TOO_MANY_POSTS = 0x12A;

        public virtual bool WaitOne(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }
            if (_waitHandle == null)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_Generic);
            }

            int ret = WaitOneNative(_waitHandle, (uint)millisecondsTimeout);
            if (ret == WAIT_ABANDONED)
            {
                throw new AbandonedMutexException();
            }

            return (ret != WaitTimeout);
        }

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles), SR.ArgumentNull_Waithandles);
            }
            if (waitHandles.Length == 0)
            {
                //
                // Some history: in CLR 1.0 and 1.1, we threw ArgumentException in this case, which was correct.
                // Somehow, in 2.0, this became ArgumentNullException.  This was not fixed until Silverlight 2,
                // which went back to ArgumentException.
                //
                // Now we're in a bit of a bind.  Backward-compatibility requires us to keep throwing ArgumentException
                // in CoreCLR, and ArgumentNullException in the desktop CLR.  This is ugly, but so is breaking
                // user code.
                //
                throw new ArgumentException(SR.Argument_EmptyWaithandleArray);
            }
            if (waitHandles.Length > MaxWaitHandles)
            {
                throw new NotSupportedException(SR.NotSupported_MaxWaitHandles);
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }
            WaitHandle[] internalWaitHandles = new WaitHandle[waitHandles.Length];
            for (int i = 0; i < waitHandles.Length; i++)
            {
                WaitHandle waitHandle = waitHandles[i];

                if (waitHandle == null)
                    throw new ArgumentNullException("waitHandles[" + i + "]", SR.ArgumentNull_ArrayElement);

                internalWaitHandles[i] = waitHandle;
            }
#if DEBUG
            // make sure we do not use waitHandles any more.
            waitHandles = null;
#endif

            int ret = WaitMultiple(internalWaitHandles, millisecondsTimeout, true /* waitall*/ );

            if ((WAIT_ABANDONED <= ret) && (WAIT_ABANDONED + internalWaitHandles.Length > ret))
            {
                //In the case of WaitAll the OS will only provide the
                //    information that mutex was abandoned.
                //    It won't tell us which one.  So we can't set the Index or provide access to the Mutex
                throw new AbandonedMutexException();
            }

            GC.KeepAlive(internalWaitHandles);
            return (ret != WaitTimeout);
        }

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles), SR.ArgumentNull_Waithandles);
            }
            if (waitHandles.Length == 0)
            {
                throw new ArgumentException(SR.Argument_EmptyWaithandleArray);
            }
            if (MaxWaitHandles < waitHandles.Length)
            {
                throw new NotSupportedException(SR.NotSupported_MaxWaitHandles);
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }
            WaitHandle[] internalWaitHandles = new WaitHandle[waitHandles.Length];
            for (int i = 0; i < waitHandles.Length; i++)
            {
                WaitHandle waitHandle = waitHandles[i];

                if (waitHandle == null)
                    throw new ArgumentNullException("waitHandles[" + i + "]", SR.ArgumentNull_ArrayElement);

                internalWaitHandles[i] = waitHandle;
            }
#if DEBUG
            // make sure we do not use waitHandles any more.
            waitHandles = null;
#endif
            int ret = WaitMultiple(internalWaitHandles, millisecondsTimeout, false /* waitany*/ );

            if ((WAIT_ABANDONED <= ret) && (WAIT_ABANDONED + internalWaitHandles.Length > ret))
            {
                int mutexIndex = ret - WAIT_ABANDONED;
                if (0 <= mutexIndex && mutexIndex < internalWaitHandles.Length)
                {
                    throw new AbandonedMutexException(mutexIndex, internalWaitHandles[mutexIndex]);
                }
                else
                {
                    throw new AbandonedMutexException();
                }
            }

            GC.KeepAlive(internalWaitHandles);
            return ret;
        }

        private static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout)
        {
            if (null == toSignal)
            {
                throw new ArgumentNullException(nameof(toSignal));
            }
            if (null == toWaitOn)
            {
                throw new ArgumentNullException(nameof(toWaitOn));
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }

            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            SafeWaitHandle safeWaitHandleToSignal = toSignal._waitHandle;
            SafeWaitHandle safeWaitHandleToWaitOn = toWaitOn._waitHandle;
            if (safeWaitHandleToSignal == null || safeWaitHandleToWaitOn == null)
            {
                // Throw ObjectDisposedException for backward compatibility even though it is not be representative of the issue
                throw new ObjectDisposedException(null, SR.ObjectDisposed_Generic);
            }

            //NOTE: This API is not supporting Pause/Resume as it's not exposed in CoreCLR (not in WP or SL)
            int ret = SignalAndWaitOne(safeWaitHandleToSignal, safeWaitHandleToWaitOn, millisecondsTimeout);

            if (WAIT_ABANDONED == ret)
            {
                throw new AbandonedMutexException();
            }

            if (ERROR_TOO_MANY_POSTS == ret)
            {
                throw new InvalidOperationException(SR.Threading_WaitHandleTooManyPosts);
            }

            //Object was signaled
            if (WAIT_OBJECT_0 == ret)
            {
                return true;
            }

            //Timeout
            return false;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int WaitOneNative(SafeWaitHandle waitableSafeHandle, uint millisecondsTimeout);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool WaitAll);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int WaitMultipleIgnoringSyncContext(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int SignalAndWaitOne(SafeWaitHandle waitHandleToSignal, SafeWaitHandle waitHandleToWaitOn, int millisecondsTimeout);
    }
}
