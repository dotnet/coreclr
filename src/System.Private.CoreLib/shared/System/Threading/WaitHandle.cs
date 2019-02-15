// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
    public abstract partial class WaitHandle : MarshalByRefObject, IDisposable
    {
        public const int WaitTimeout = 0x102;
        protected static readonly IntPtr InvalidHandle = new IntPtr(-1);

        internal const int MaxWaitHandles = 64;

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

        public virtual bool WaitOne(TimeSpan timeout) => WaitOne(ToTimeoutMilliseconds(timeout));
        public virtual bool WaitOne() => WaitOne(-1);
        public virtual bool WaitOne(int millisecondsTimeout, bool exitContext) => WaitOne(millisecondsTimeout);
        public virtual bool WaitOne(TimeSpan timeout, bool exitContext) => WaitOne(ToTimeoutMilliseconds(timeout));

        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitAll(waitHandles, ToTimeoutMilliseconds(timeout));
        public static bool WaitAll(WaitHandle[] waitHandles) =>
            WaitAll(waitHandles, -1);
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitAll(waitHandles, millisecondsTimeout);
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitAll(waitHandles, ToTimeoutMilliseconds(timeout));

        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitAny(waitHandles, ToTimeoutMilliseconds(timeout));
        public static int WaitAny(WaitHandle[] waitHandles) =>
            WaitAny(waitHandles, -1);
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitAny(waitHandles, millisecondsTimeout);
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitAny(waitHandles, ToTimeoutMilliseconds(timeout));

        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn) =>
            SignalAndWait(toSignal, toWaitOn, -1);
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, TimeSpan timeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, ToTimeoutMilliseconds(timeout));
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, millisecondsTimeout);
    }
}
