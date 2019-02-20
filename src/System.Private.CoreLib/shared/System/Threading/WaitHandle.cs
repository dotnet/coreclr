// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
    public abstract partial class WaitHandle : MarshalByRefObject, IDisposable
    {
        internal const int WaitAbandoned = 0x80;
        internal const int WaitTimeout = 0x102;
        internal const int ErrorTooManyPosts = 0x12A;

        internal const int MaxWaitHandles = 64;

        protected static readonly IntPtr InvalidHandle = new IntPtr(-1);

        // IMPORTANT:
        // - Do not add or rearrange fields as the EE depends on this layout.

        internal SafeWaitHandle _waitHandle;

        internal enum OpenExistingResult
        {
            Success,
            NameNotFound,
            PathNotFound,
            NameInvalid
        }

        protected WaitHandle()
        {
        }

        [Obsolete("Use the SafeWaitHandle property instead.")]
        public virtual IntPtr Handle
        {
            get
            {
                return _waitHandle == null ? InvalidHandle : _waitHandle.DangerousGetHandle();
            }
            set
            {
                if (value == InvalidHandle)
                {
                    // This line leaks a handle.  However, it's currently
                    // not perfectly clear what the right behavior is here 
                    // anyways.  This preserves Everett behavior.  We should 
                    // ideally do these things:
                    // *) Expose a settable SafeHandle property on WaitHandle.
                    // *) Expose a settable OwnsHandle property on SafeHandle.
                    if (_waitHandle != null)
                    {
                        _waitHandle.SetHandleAsInvalid();
                        _waitHandle = null;
                    }
                }
                else
                {
                    _waitHandle = new SafeWaitHandle(value, true);
                }
            }
        }

        public SafeWaitHandle SafeWaitHandle
        {
            get
            {
                if (_waitHandle == null)
                {
                    _waitHandle = new SafeWaitHandle(InvalidHandle, false);
                }
                return _waitHandle;
            }

            set
            { _waitHandle = value; }
        }

        internal static int ToTimeoutMilliseconds(TimeSpan timeout)
        {
            var timeoutMilliseconds = (long)timeout.TotalMilliseconds;
            if (timeoutMilliseconds < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }
            if (timeoutMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            }
            return (int)timeoutMilliseconds;
        }

        public virtual void Close() => Dispose();

        protected virtual void Dispose(bool explicitDisposing)
        {
            if (_waitHandle != null)
            {
                _waitHandle.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual bool WaitOne(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }

            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            SafeWaitHandle waitHandle = _waitHandle;
            if (waitHandle == null)
            {
                // Throw ObjectDisposedException for backward compatibility even though it is not be representative of the issue
                throw new ObjectDisposedException(null, SR.ObjectDisposed_Generic);
            }

            bool success = false;
            waitHandle.DangerousAddRef(ref success);
            try
            {
                int waitResult;

                SynchronizationContext context = SynchronizationContext.Current;
                if (context != null && context.IsWaitNotificationRequired())
                {
                    var handles = new[] { waitHandle.DangerousGetHandle() };
                    waitResult = context.Wait(handles, false, millisecondsTimeout);
                    GC.KeepAlive(handles);
                }
                else
                {
                    waitResult = WaitOneCore(waitHandle.DangerousGetHandle(), millisecondsTimeout);
                }

                if (waitResult == WaitAbandoned)
                {
                    throw new AbandonedMutexException();
                }

                return waitResult != WaitTimeout;
            }
            finally
            {
                if (success)
                    waitHandle.DangerousRelease();
            }
        }

        /// <summary>
        /// Obtains all of the corresponding safe wait handles and adds a ref to each. Since the <see cref="SafeWaitHandle"/>
        /// property is publically modifiable, this makes sure that we add and release refs one the same set of safe wait
        /// handles to keep them alive during a multi-wait operation.
        /// </summary>
        private static void ObtainSafeWaitHandles(
            Span<WaitHandle> waitHandles,
            out SafeWaitHandle[] safeWaitHandles,
            out IntPtr[] unsafeWaitHandles)
        {
            Debug.Assert(waitHandles != null);
            Debug.Assert(waitHandles.Length > 0);
            Debug.Assert(waitHandles.Length <= MaxWaitHandles);

            safeWaitHandles = new SafeWaitHandle[waitHandles.Length];
            unsafeWaitHandles = new IntPtr[waitHandles.Length];
            bool success = false;
            try
            {
                for (int i = 0; i < waitHandles.Length; ++i)
                {
                    WaitHandle waitHandle = waitHandles[i];
                    if (waitHandle == null)
                    {
                        throw new ArgumentNullException("waitHandles[" + i + ']', SR.ArgumentNull_ArrayElement);
                    }

                    SafeWaitHandle safeWaitHandle = waitHandle._waitHandle;
                    if (safeWaitHandle == null)
                    {
                        // Throw ObjectDisposedException for backward compatibility even though it is not be representative of the issue
                        throw new ObjectDisposedException(null, SR.ObjectDisposed_Generic);
                    }

                    safeWaitHandle.DangerousAddRef(ref success);
                    safeWaitHandles[i] = safeWaitHandle;
                    unsafeWaitHandles[i] = safeWaitHandle.DangerousGetHandle();
                }
                success = true;
            }
            finally
            {
                if (!success)
                {
                    for (int i = 0; i < waitHandles.Length; ++i)
                    {
                        SafeWaitHandle safeWaitHandle = safeWaitHandles[i];
                        if (safeWaitHandle == null)
                        {
                            break;
                        }
                        safeWaitHandle.DangerousRelease();
                        safeWaitHandles[i] = null;
                    }
                }
            }
        }

        private static int WaitMultiple(Span<WaitHandle> waitHandles, bool waitAll, int millisecondsTimeout)
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
                throw new ArgumentException(SR.Argument_EmptyWaithandleArray, nameof(waitHandles));
            }
            if (waitHandles.Length > MaxWaitHandles)
            {
                throw new NotSupportedException(SR.NotSupported_MaxWaitHandles);
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            }

            ObtainSafeWaitHandles(waitHandles, out SafeWaitHandle[] safeWaitHandles, out IntPtr[] unsafeWaitHandles);
            try
            {
                int waitResult;

                SynchronizationContext context = SynchronizationContext.Current;
                if (context != null && context.IsWaitNotificationRequired())
                {
                    waitResult = context.Wait(unsafeWaitHandles, waitAll, millisecondsTimeout);
                }
                else
                {
                    waitResult = WaitMultipleIgnoringSyncContext(unsafeWaitHandles, waitAll, millisecondsTimeout);
                }

                if (waitResult >= WaitAbandoned && waitResult < WaitAbandoned + unsafeWaitHandles.Length)
                {
                    if (waitAll)
                    {
                        // In the case of WaitAll the OS will only provide the information that mutex was abandoned.
                        // It won't tell us which one.  So we can't set the Index or provide access to the Mutex
                        throw new AbandonedMutexException();
                    }

                    waitResult -= WaitAbandoned;
                    throw new AbandonedMutexException(waitResult, waitHandles[waitResult]);
                }

                GC.KeepAlive(unsafeWaitHandles);

                return waitResult;
            }
            finally
            {
                for (int i = 0; i < waitHandles.Length; ++i)
                {
                    safeWaitHandles[i].DangerousRelease();
                    safeWaitHandles[i] = null;
                }
            }
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

            bool successSignal = false, successWait = false;
            int ret;
            try
            {
                safeWaitHandleToSignal.DangerousAddRef(ref successSignal);
                safeWaitHandleToWaitOn.DangerousAddRef(ref successWait);

                ret = SignalAndWaitCore(
                    safeWaitHandleToSignal.DangerousGetHandle(),
                    safeWaitHandleToWaitOn.DangerousGetHandle(),
                    millisecondsTimeout);

                if (ret == WaitAbandoned)
                {
                    throw new AbandonedMutexException();
                }

                if (ret == ErrorTooManyPosts)
                {
                    throw new InvalidOperationException(SR.Threading_WaitHandleTooManyPosts);
                }

                return ret != WaitTimeout;
            }
            finally
            {
                if (successWait)
                {
                    safeWaitHandleToWaitOn.DangerousRelease();
                }
                if (successSignal)
                {
                    safeWaitHandleToSignal.DangerousRelease();
                }
            }
        }

        public virtual bool WaitOne(TimeSpan timeout) => WaitOne(ToTimeoutMilliseconds(timeout));
        public virtual bool WaitOne() => WaitOne(-1);
        public virtual bool WaitOne(int millisecondsTimeout, bool exitContext) => WaitOne(millisecondsTimeout);
        public virtual bool WaitOne(TimeSpan timeout, bool exitContext) => WaitOne(ToTimeoutMilliseconds(timeout));

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout) =>
            WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles) =>
            WaitMultiple(waitHandles, true, -1) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout) =>
            WaitMultiple(waitHandles, false, millisecondsTimeout);
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));
        public static int WaitAny(WaitHandle[] waitHandles) =>
            WaitMultiple(waitHandles, false, -1);
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitMultiple(waitHandles, false, millisecondsTimeout);
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));

        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn) =>
            SignalAndWait(toSignal, toWaitOn, -1);
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, TimeSpan timeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, ToTimeoutMilliseconds(timeout));
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, millisecondsTimeout);
    }
}
