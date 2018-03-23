// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Abstract base class for all Streams.  Provides
** default implementations of asynchronous reads & writes, in
** terms of the synchronous reads & writes (and vice versa).
**
**
===========================================================*/

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Diagnostics;
using System.Reflection;

namespace System.IO
{
    public abstract class Stream : MarshalByRefObject, IDisposable
    {
        public static readonly Stream Null = new NullStream();

        //We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int _DefaultCopyBufferSize = 81920;

        // To implement Async IO operations on streams that don't support async IO

        private ReadWriteTask _activeReadWriteTask;
        private SemaphoreSlim _asyncActiveSemaphore;

        internal SemaphoreSlim EnsureAsyncActiveSemaphoreInitialized()
        {
            // Lazily-initialize _asyncActiveSemaphore.  As we're never accessing the SemaphoreSlim's
            // WaitHandle, we don't need to worry about Disposing it.
            return LazyInitializer.EnsureInitialized(ref _asyncActiveSemaphore, () => new SemaphoreSlim(1, 1));
        }

        public abstract bool CanRead
        {
            get;
        }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public abstract bool CanSeek
        {
            get;
        }

        public virtual bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public abstract bool CanWrite
        {
            get;
        }

        public abstract long Length
        {
            get;
        }

        public abstract long Position
        {
            get;
            set;
        }

        public virtual int ReadTimeout
        {
            get
            {
                throw new InvalidOperationException(SR.InvalidOperation_TimeoutsNotSupported);
            }
            set
            {
                throw new InvalidOperationException(SR.InvalidOperation_TimeoutsNotSupported);
            }
        }

        public virtual int WriteTimeout
        {
            get
            {
                throw new InvalidOperationException(SR.InvalidOperation_TimeoutsNotSupported);
            }
            set
            {
                throw new InvalidOperationException(SR.InvalidOperation_TimeoutsNotSupported);
            }
        }

        public Task CopyToAsync(Stream destination)
        {
            int bufferSize = GetCopyBufferSize();

            return CopyToAsync(destination, bufferSize);
        }

        public Task CopyToAsync(Stream destination, Int32 bufferSize)
        {
            return CopyToAsync(destination, bufferSize, CancellationToken.None);
        }

        public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
        {
            int bufferSize = GetCopyBufferSize();

            return CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public virtual Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);

            return CopyToAsyncInternal(destination, bufferSize, cancellationToken);
        }

        private async Task CopyToAsyncInternal(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                while (true)
                {
                    int bytesRead = await ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0) break;
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        // Reads the bytes from the current stream and writes the bytes to
        // the destination stream until all bytes are read, starting at
        // the current position.
        public void CopyTo(Stream destination)
        {
            int bufferSize = GetCopyBufferSize();

            CopyTo(destination, bufferSize);
        }

        public virtual void CopyTo(Stream destination, int bufferSize)
        {
            StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int read;
                while ((read = Read(buffer, 0, buffer.Length)) != 0)
                {
                    destination.Write(buffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private int GetCopyBufferSize()
        {
            int bufferSize = _DefaultCopyBufferSize;

            if (CanSeek)
            {
                long length = Length;
                long position = Position;
                if (length <= position) // Handles negative overflows
                {
                    // There are no bytes left in the stream to copy.
                    // However, because CopyTo{Async} is virtual, we need to
                    // ensure that any override is still invoked to provide its
                    // own validation, so we use the smallest legal buffer size here.
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0)
                    {
                        // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                    }
                }
            }

            return bufferSize;
        }

        // Stream used to require that all cleanup logic went into Close(),
        // which was thought up before we invented IDisposable.  However, we
        // need to follow the IDisposable pattern so that users can write 
        // sensible subclasses without needing to inspect all their base 
        // classes, and without worrying about version brittleness, from a
        // base class switching to the Dispose pattern.  We're moving
        // Stream to the Dispose(bool) pattern - that's where all subclasses 
        // should put their cleanup starting in V2.
        public virtual void Close()
        {
            // Ideally we would assert CanRead == CanWrite == CanSeek = false, 
            // but we'd have to fix PipeStream & NetworkStream very carefully.

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            // Ideally we would assert CanRead == CanWrite == CanSeek = false, 
            // but we'd have to fix PipeStream & NetworkStream very carefully.

            Close();
        }


        protected virtual void Dispose(bool disposing)
        {
            // Note: Never change this to call other virtual methods on Stream
            // like Write, since the state on subclasses has already been 
            // torn down.  This is the last code to run on cleanup for a stream.
        }

        public abstract void Flush();

        public Task FlushAsync()
        {
            return FlushAsync(CancellationToken.None);
        }

        public virtual Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(state => ((Stream)state).Flush(), this,
                cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        [Obsolete("CreateWaitHandle will be removed eventually.  Please use \"new ManualResetEvent(false)\" instead.")]
        protected virtual WaitHandle CreateWaitHandle()
        {
            return new ManualResetEvent(false);
        }

        public virtual IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            return BeginReadInternal(buffer, offset, count, callback, state, serializeAsynchronously: false, apm: true);
        }

        internal IAsyncResult BeginReadInternal(
            byte[] buffer, int offset, int count, AsyncCallback callback, Object state,
            bool serializeAsynchronously, bool apm)
        {
            if (!CanRead) throw Error.GetReadNotSupported();

            // To avoid a race with a stream's position pointer & generating race conditions 
            // with internal buffer indexes in our own streams that 
            // don't natively support async IO operations when there are multiple 
            // async requests outstanding, we will block the application's main
            // thread if it does a second IO request until the first one completes.
            var semaphore = EnsureAsyncActiveSemaphoreInitialized();
            Task semaphoreTask = null;
            if (serializeAsynchronously)
            {
                semaphoreTask = semaphore.WaitAsync();
            }
            else
            {
                semaphore.Wait();
            }

            // Create the task to asynchronously do a Read.  This task serves both
            // as the asynchronous work item and as the IAsyncResult returned to the user.
            var asyncResult = new ReadWriteTask(true /*isRead*/, apm, delegate
            {
                // The ReadWriteTask stores all of the parameters to pass to Read.
                // As we're currently inside of it, we can get the current task
                // and grab the parameters from it.
                var thisTask = Task.InternalCurrent as ReadWriteTask;
                Debug.Assert(thisTask != null, "Inside ReadWriteTask, InternalCurrent should be the ReadWriteTask");

                try
                {
                    // Do the Read and return the number of bytes read
                    return thisTask._stream.Read(thisTask._buffer, thisTask._offset, thisTask._count);
                }
                finally
                {
                    // If this implementation is part of Begin/EndXx, then the EndXx method will handle
                    // finishing the async operation.  However, if this is part of XxAsync, then there won't
                    // be an end method, and this task is responsible for cleaning up.
                    if (!thisTask._apm)
                    {
                        thisTask._stream.FinishTrackingAsyncOperation();
                    }

                    thisTask.ClearBeginState(); // just to help alleviate some memory pressure
                }
            }, state, this, buffer, offset, count, callback);

            // Schedule it
            if (semaphoreTask != null)
                RunReadWriteTaskWhenReady(semaphoreTask, asyncResult);
            else
                RunReadWriteTask(asyncResult);


            return asyncResult; // return it
        }

        public virtual int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));

            var readTask = _activeReadWriteTask;

            if (readTask == null)
            {
                throw new ArgumentException(SR.InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple);
            }
            else if (readTask != asyncResult)
            {
                throw new InvalidOperationException(SR.InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple);
            }
            else if (!readTask._isRead)
            {
                throw new ArgumentException(SR.InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple);
            }

            try
            {
                return readTask.GetAwaiter().GetResult(); // block until completion, then get result / propagate any exception
            }
            finally
            {
                FinishTrackingAsyncOperation();
            }
        }

        public Task<int> ReadAsync(Byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None);
        }

        public virtual Task<int> ReadAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // If cancellation was requested, bail early with an already completed task.
            // Otherwise, return a task that represents the Begin/End methods.
            return cancellationToken.IsCancellationRequested
                        ? Task.FromCanceled<int>(cancellationToken)
                        : BeginEndReadAsync(buffer, offset, count);
        }

        public virtual ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask<int>(ReadAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }
            else
            {
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                return FinishReadAsync(ReadAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer, buffer);

                async ValueTask<int> FinishReadAsync(Task<int> readTask, byte[] localBuffer, Memory<byte> localDestination)
                {
                    try
                    {
                        int result = await readTask.ConfigureAwait(false);
                        new Span<byte>(localBuffer, 0, result).CopyTo(localDestination.Span);
                        return result;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(localBuffer);
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool HasOverriddenBeginEndRead();

        private Task<Int32> BeginEndReadAsync(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (!HasOverriddenBeginEndRead())
            {
                // If the Stream does not override Begin/EndRead, then we can take an optimized path
                // that skips an extra layer of tasks / IAsyncResults.
                return (Task<Int32>)BeginReadInternal(buffer, offset, count, null, null, serializeAsynchronously: true, apm: false);
            }

            // Otherwise, we need to wrap calls to Begin/EndWrite to ensure we use the derived type's functionality.
            return TaskFactory<Int32>.FromAsyncTrim(
                        this, new ReadWriteParameters { Buffer = buffer, Offset = offset, Count = count },
                        (stream, args, callback, state) => stream.BeginRead(args.Buffer, args.Offset, args.Count, callback, state), // cached by compiler
                        (stream, asyncResult) => stream.EndRead(asyncResult)); // cached by compiler
        }

        private struct ReadWriteParameters // struct for arguments to Read and Write calls
        {
            internal byte[] Buffer;
            internal int Offset;
            internal int Count;
        }



        public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            return BeginWriteInternal(buffer, offset, count, callback, state, serializeAsynchronously: false, apm: true);
        }

        internal IAsyncResult BeginWriteInternal(
            byte[] buffer, int offset, int count, AsyncCallback callback, Object state,
            bool serializeAsynchronously, bool apm)
        {
            if (!CanWrite) throw Error.GetWriteNotSupported();

            // To avoid a race condition with a stream's position pointer & generating conditions 
            // with internal buffer indexes in our own streams that 
            // don't natively support async IO operations when there are multiple 
            // async requests outstanding, we will block the application's main
            // thread if it does a second IO request until the first one completes.
            var semaphore = EnsureAsyncActiveSemaphoreInitialized();
            Task semaphoreTask = null;
            if (serializeAsynchronously)
            {
                semaphoreTask = semaphore.WaitAsync(); // kick off the asynchronous wait, but don't block
            }
            else
            {
                semaphore.Wait(); // synchronously wait here
            }

            // Create the task to asynchronously do a Write.  This task serves both
            // as the asynchronous work item and as the IAsyncResult returned to the user.
            var asyncResult = new ReadWriteTask(false /*isRead*/, apm, delegate
            {
                // The ReadWriteTask stores all of the parameters to pass to Write.
                // As we're currently inside of it, we can get the current task
                // and grab the parameters from it.
                var thisTask = Task.InternalCurrent as ReadWriteTask;
                Debug.Assert(thisTask != null, "Inside ReadWriteTask, InternalCurrent should be the ReadWriteTask");

                try
                {
                    // Do the Write
                    thisTask._stream.Write(thisTask._buffer, thisTask._offset, thisTask._count);
                    return 0; // not used, but signature requires a value be returned
                }
                finally
                {
                    // If this implementation is part of Begin/EndXx, then the EndXx method will handle
                    // finishing the async operation.  However, if this is part of XxAsync, then there won't
                    // be an end method, and this task is responsible for cleaning up.
                    if (!thisTask._apm)
                    {
                        thisTask._stream.FinishTrackingAsyncOperation();
                    }

                    thisTask.ClearBeginState(); // just to help alleviate some memory pressure
                }
            }, state, this, buffer, offset, count, callback);

            // Schedule it
            if (semaphoreTask != null)
                RunReadWriteTaskWhenReady(semaphoreTask, asyncResult);
            else
                RunReadWriteTask(asyncResult);

            return asyncResult; // return it
        }

        private void RunReadWriteTaskWhenReady(Task asyncWaiter, ReadWriteTask readWriteTask)
        {
            Debug.Assert(readWriteTask != null);
            Debug.Assert(asyncWaiter != null);

            // If the wait has already completed, run the task.
            if (asyncWaiter.IsCompleted)
            {
                Debug.Assert(asyncWaiter.IsCompletedSuccessfully, "The semaphore wait should always complete successfully.");
                RunReadWriteTask(readWriteTask);
            }
            else  // Otherwise, wait for our turn, and then run the task.
            {
                asyncWaiter.ContinueWith((t, state) =>
                {
                    Debug.Assert(t.IsCompletedSuccessfully, "The semaphore wait should always complete successfully.");
                    var rwt = (ReadWriteTask)state;
                    rwt._stream.RunReadWriteTask(rwt); // RunReadWriteTask(readWriteTask);
                }, readWriteTask, default(CancellationToken), TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
        }

        private void RunReadWriteTask(ReadWriteTask readWriteTask)
        {
            Debug.Assert(readWriteTask != null);
            Debug.Assert(_activeReadWriteTask == null, "Expected no other readers or writers");

            // Schedule the task.  ScheduleAndStart must happen after the write to _activeReadWriteTask to avoid a race.
            // Internally, we're able to directly call ScheduleAndStart rather than Start, avoiding
            // two interlocked operations.  However, if ReadWriteTask is ever changed to use
            // a cancellation token, this should be changed to use Start.
            _activeReadWriteTask = readWriteTask; // store the task so that EndXx can validate it's given the right one
            readWriteTask.m_taskScheduler = TaskScheduler.Default;
            readWriteTask.ScheduleAndStart(needsProtection: false);
        }

        private void FinishTrackingAsyncOperation()
        {
            _activeReadWriteTask = null;
            Debug.Assert(_asyncActiveSemaphore != null, "Must have been initialized in order to get here.");
            _asyncActiveSemaphore.Release();
        }

        public virtual void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));

            var writeTask = _activeReadWriteTask;
            if (writeTask == null)
            {
                throw new ArgumentException(SR.InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple);
            }
            else if (writeTask != asyncResult)
            {
                throw new InvalidOperationException(SR.InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple);
            }
            else if (writeTask._isRead)
            {
                throw new ArgumentException(SR.InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple);
            }

            try
            {
                writeTask.GetAwaiter().GetResult(); // block until completion, then propagate any exceptions
                Debug.Assert(writeTask.Status == TaskStatus.RanToCompletion);
            }
            finally
            {
                FinishTrackingAsyncOperation();
            }
        }

        // Task used by BeginRead / BeginWrite to do Read / Write asynchronously.
        // A single instance of this task serves four purposes:
        // 1. The work item scheduled to run the Read / Write operation
        // 2. The state holding the arguments to be passed to Read / Write
        // 3. The IAsyncResult returned from BeginRead / BeginWrite
        // 4. The completion action that runs to invoke the user-provided callback.
        // This last item is a bit tricky.  Before the AsyncCallback is invoked, the
        // IAsyncResult must have completed, so we can't just invoke the handler
        // from within the task, since it is the IAsyncResult, and thus it's not
        // yet completed.  Instead, we use AddCompletionAction to install this
        // task as its own completion handler.  That saves the need to allocate
        // a separate completion handler, it guarantees that the task will
        // have completed by the time the handler is invoked, and it allows
        // the handler to be invoked synchronously upon the completion of the
        // task.  This all enables BeginRead / BeginWrite to be implemented
        // with a single allocation.
        private sealed class ReadWriteTask : Task<int>, ITaskCompletionAction
        {
            internal readonly bool _isRead;
            internal readonly bool _apm; // true if this is from Begin/EndXx; false if it's from XxAsync
            internal Stream _stream;
            internal byte[] _buffer;
            internal readonly int _offset;
            internal readonly int _count;
            private AsyncCallback _callback;
            private ExecutionContext _context;

            internal void ClearBeginState() // Used to allow the args to Read/Write to be made available for GC
            {
                _stream = null;
                _buffer = null;
            }

            public ReadWriteTask(
                bool isRead,
                bool apm,
                Func<object, int> function, object state,
                Stream stream, byte[] buffer, int offset, int count, AsyncCallback callback) :
                base(function, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach)
            {
                Debug.Assert(function != null);
                Debug.Assert(stream != null);
                Debug.Assert(buffer != null);

                // Store the arguments
                _isRead = isRead;
                _apm = apm;
                _stream = stream;
                _buffer = buffer;
                _offset = offset;
                _count = count;

                // If a callback was provided, we need to:
                // - Store the user-provided handler
                // - Capture an ExecutionContext under which to invoke the handler
                // - Add this task as its own completion handler so that the Invoke method
                //   will run the callback when this task completes.
                if (callback != null)
                {
                    _callback = callback;
                    _context = ExecutionContext.Capture();
                    base.AddCompletionAction(this);
                }
            }

            private static void InvokeAsyncCallback(object completedTask)
            {
                var rwc = (ReadWriteTask)completedTask;
                var callback = rwc._callback;
                rwc._callback = null;
                callback(rwc);
            }

            private static ContextCallback s_invokeAsyncCallback;

            void ITaskCompletionAction.Invoke(Task completingTask)
            {
                // Get the ExecutionContext.  If there is none, just run the callback
                // directly, passing in the completed task as the IAsyncResult.
                // If there is one, process it with ExecutionContext.Run.
                var context = _context;
                if (context == null)
                {
                    var callback = _callback;
                    _callback = null;
                    callback(completingTask);
                }
                else
                {
                    _context = null;

                    var invokeAsyncCallback = s_invokeAsyncCallback;
                    if (invokeAsyncCallback == null) s_invokeAsyncCallback = invokeAsyncCallback = InvokeAsyncCallback; // benign race condition

                    ExecutionContext.RunInternal(context, invokeAsyncCallback, this);
                }
            }

            bool ITaskCompletionAction.InvokeMayRunArbitraryCode { get { return true; } }
        }

        public Task WriteAsync(Byte[] buffer, int offset, int count)
        {
            return WriteAsync(buffer, offset, count, CancellationToken.None);
        }

        public virtual Task WriteAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // If cancellation was requested, bail early with an already completed task.
            // Otherwise, return a task that represents the Begin/End methods.
            return cancellationToken.IsCancellationRequested
                        ? Task.FromCanceled(cancellationToken)
                        : BeginEndWriteAsync(buffer, offset, count);
        }

        public virtual ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask(WriteAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }
            else
            {
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                buffer.Span.CopyTo(sharedBuffer);
                return new ValueTask(FinishWriteAsync(WriteAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer));
            }
        }

        private async Task FinishWriteAsync(Task writeTask, byte[] localBuffer)
        {
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(localBuffer);
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool HasOverriddenBeginEndWrite();

        private Task BeginEndWriteAsync(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (!HasOverriddenBeginEndWrite())
            {
                // If the Stream does not override Begin/EndWrite, then we can take an optimized path
                // that skips an extra layer of tasks / IAsyncResults.
                return (Task)BeginWriteInternal(buffer, offset, count, null, null, serializeAsynchronously: true, apm: false);
            }

            // Otherwise, we need to wrap calls to Begin/EndWrite to ensure we use the derived type's functionality.
            return TaskFactory<VoidTaskResult>.FromAsyncTrim(
                        this, new ReadWriteParameters { Buffer = buffer, Offset = offset, Count = count },
                        (stream, args, callback, state) => stream.BeginWrite(args.Buffer, args.Offset, args.Count, callback, state), // cached by compiler
                        (stream, asyncResult) => // cached by compiler
                        {
                            stream.EndWrite(asyncResult);
                            return default(VoidTaskResult);
                        });
        }

        public abstract long Seek(long offset, SeekOrigin origin);

        public abstract void SetLength(long value);

        public abstract int Read(byte[] buffer, int offset, int count);

        public virtual int Read(Span<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = Read(sharedBuffer, 0, buffer.Length);
                if ((uint)numRead > buffer.Length)
                {
                    throw new IOException(SR.IO_StreamTooLong);
                }
                new Span<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally { ArrayPool<byte>.Shared.Return(sharedBuffer); }
        }

        // Reads one byte from the stream by calling Read(byte[], int, int). 
        // Will return an unsigned byte cast to an int or -1 on end of stream.
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any 
        // subclass that maintains an internal buffer.  Then, it can help perf
        // significantly for people who are reading one byte at a time.
        public virtual int ReadByte()
        {
            byte[] oneByteArray = new byte[1];
            int r = Read(oneByteArray, 0, 1);
            if (r == 0)
                return -1;
            return oneByteArray[0];
        }

        public abstract void Write(byte[] buffer, int offset, int count);

        public virtual void Write(ReadOnlySpan<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                Write(buffer, 0, buffer.Length);
            }
            finally { ArrayPool<byte>.Shared.Return(sharedBuffer); }
        }

        // Writes one byte from the stream by calling Write(byte[], int, int).
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any 
        // subclass that maintains an internal buffer.  Then, it can help perf
        // significantly for people who are writing one byte at a time.
        public virtual void WriteByte(byte value)
        {
            byte[] oneByteArray = new byte[1];
            oneByteArray[0] = value;
            Write(oneByteArray, 0, 1);
        }

        public static Stream Synchronized(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (stream is SyncStream)
                return stream;

            return new SyncStream(stream);
        }

        [Obsolete("Do not call or override this method.")]
        protected virtual void ObjectInvariant()
        {
        }

        internal IAsyncResult BlockingBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            // To avoid a race with a stream's position pointer & generating conditions
            // with internal buffer indexes in our own streams that 
            // don't natively support async IO operations when there are multiple 
            // async requests outstanding, we will block the application's main
            // thread and do the IO synchronously.  
            // This can't perform well - use a different approach.
            SynchronousAsyncResult asyncResult;
            try
            {
                int numRead = Read(buffer, offset, count);
                asyncResult = new SynchronousAsyncResult(numRead, state);
            }
            catch (IOException ex)
            {
                asyncResult = new SynchronousAsyncResult(ex, state, isWrite: false);
            }

            if (callback != null)
            {
                callback(asyncResult);
            }

            return asyncResult;
        }

        internal static int BlockingEndRead(IAsyncResult asyncResult)
        {
            return SynchronousAsyncResult.EndRead(asyncResult);
        }

        internal IAsyncResult BlockingBeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            // To avoid a race condition with a stream's position pointer & generating conditions 
            // with internal buffer indexes in our own streams that 
            // don't natively support async IO operations when there are multiple 
            // async requests outstanding, we will block the application's main
            // thread and do the IO synchronously.  
            // This can't perform well - use a different approach.
            SynchronousAsyncResult asyncResult;
            try
            {
                Write(buffer, offset, count);
                asyncResult = new SynchronousAsyncResult(state);
            }
            catch (IOException ex)
            {
                asyncResult = new SynchronousAsyncResult(ex, state, isWrite: true);
            }

            if (callback != null)
            {
                callback(asyncResult);
            }

            return asyncResult;
        }

        internal static void BlockingEndWrite(IAsyncResult asyncResult)
        {
            SynchronousAsyncResult.EndWrite(asyncResult);
        }

        private sealed class NullStream : Stream
        {
            internal NullStream() { }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get { return 0; }
                set { }
            }

            public override void CopyTo(Stream destination, int bufferSize)
            {
                StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);

                // After we validate arguments this is a nop.
            }

            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                // Validate arguments here for compat, since previously this method
                // was inherited from Stream (which did check its arguments).
                StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);

                return cancellationToken.IsCancellationRequested ?
                    Task.FromCanceled(cancellationToken) :
                    Task.CompletedTask;
            }

            protected override void Dispose(bool disposing)
            {
                // Do nothing - we don't want NullStream singleton (static) to be closable
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return cancellationToken.IsCancellationRequested ?
                    Task.FromCanceled(cancellationToken) :
                    Task.CompletedTask;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                if (!CanRead) throw Error.GetReadNotSupported();

                return BlockingBeginRead(buffer, offset, count, callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException(nameof(asyncResult));

                return BlockingEndRead(asyncResult);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                if (!CanWrite) throw Error.GetWriteNotSupported();

                return BlockingBeginWrite(buffer, offset, count, callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException(nameof(asyncResult));

                BlockingEndWrite(asyncResult);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override int Read(Span<byte> buffer)
            {
                return 0;
            }

            public override Task<int> ReadAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return AsyncTaskMethodBuilder<int>.s_defaultResultTask;
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
            {
                return new ValueTask<int>(0);
            }

            public override int ReadByte()
            {
                return -1;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
            }

            public override Task WriteAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return cancellationToken.IsCancellationRequested ?
                    Task.FromCanceled(cancellationToken) :
                    Task.CompletedTask;
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
            {
                return cancellationToken.IsCancellationRequested ?
                    new ValueTask(Task.FromCanceled(cancellationToken)) :
                    default;
            }

            public override void WriteByte(byte value)
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override void SetLength(long length)
            {
            }
        }


        /// <summary>Used as the IAsyncResult object when using asynchronous IO methods on the base Stream class.</summary>
        internal sealed class SynchronousAsyncResult : IAsyncResult
        {
            private readonly Object _stateObject;
            private readonly bool _isWrite;
            private ManualResetEvent _waitHandle;
            private ExceptionDispatchInfo _exceptionInfo;

            private bool _endXxxCalled;
            private Int32 _bytesRead;

            internal SynchronousAsyncResult(Int32 bytesRead, Object asyncStateObject)
            {
                _bytesRead = bytesRead;
                _stateObject = asyncStateObject;
                //_isWrite = false;
            }

            internal SynchronousAsyncResult(Object asyncStateObject)
            {
                _stateObject = asyncStateObject;
                _isWrite = true;
            }

            internal SynchronousAsyncResult(Exception ex, Object asyncStateObject, bool isWrite)
            {
                _exceptionInfo = ExceptionDispatchInfo.Capture(ex);
                _stateObject = asyncStateObject;
                _isWrite = isWrite;
            }

            public bool IsCompleted
            {
                // We never hand out objects of this type to the user before the synchronous IO completed:
                get { return true; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return LazyInitializer.EnsureInitialized(ref _waitHandle, () => new ManualResetEvent(true));
                }
            }

            public Object AsyncState
            {
                get { return _stateObject; }
            }

            public bool CompletedSynchronously
            {
                get { return true; }
            }

            internal void ThrowIfError()
            {
                if (_exceptionInfo != null)
                    _exceptionInfo.Throw();
            }

            internal static Int32 EndRead(IAsyncResult asyncResult)
            {
                SynchronousAsyncResult ar = asyncResult as SynchronousAsyncResult;
                if (ar == null || ar._isWrite)
                    throw new ArgumentException(SR.Arg_WrongAsyncResult);

                if (ar._endXxxCalled)
                    throw new ArgumentException(SR.InvalidOperation_EndReadCalledMultiple);

                ar._endXxxCalled = true;

                ar.ThrowIfError();
                return ar._bytesRead;
            }

            internal static void EndWrite(IAsyncResult asyncResult)
            {
                SynchronousAsyncResult ar = asyncResult as SynchronousAsyncResult;
                if (ar == null || !ar._isWrite)
                    throw new ArgumentException(SR.Arg_WrongAsyncResult);

                if (ar._endXxxCalled)
                    throw new ArgumentException(SR.InvalidOperation_EndWriteCalledMultiple);

                ar._endXxxCalled = true;

                ar.ThrowIfError();
            }
        }   // class SynchronousAsyncResult


        // SyncStream is a wrapper around a stream that takes 
        // a lock for every operation making it thread safe.
        internal sealed class SyncStream : Stream, IDisposable
        {
            private Stream _stream;

            internal SyncStream(Stream stream)
            {
                if (stream == null)
                    throw new ArgumentNullException(nameof(stream));
                _stream = stream;
            }

            public override bool CanRead
            {
                get { return _stream.CanRead; }
            }

            public override bool CanWrite
            {
                get { return _stream.CanWrite; }
            }

            public override bool CanSeek
            {
                get { return _stream.CanSeek; }
            }

            public override bool CanTimeout
            {
                get
                {
                    return _stream.CanTimeout;
                }
            }

            public override long Length
            {
                get
                {
                    lock (_stream)
                    {
                        return _stream.Length;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    lock (_stream)
                    {
                        return _stream.Position;
                    }
                }
                set
                {
                    lock (_stream)
                    {
                        _stream.Position = value;
                    }
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return _stream.ReadTimeout;
                }
                set
                {
                    _stream.ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return _stream.WriteTimeout;
                }
                set
                {
                    _stream.WriteTimeout = value;
                }
            }

            // In the off chance that some wrapped stream has different 
            // semantics for Close vs. Dispose, let's preserve that.
            public override void Close()
            {
                lock (_stream)
                {
                    try
                    {
                        _stream.Close();
                    }
                    finally
                    {
                        base.Dispose(true);
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                lock (_stream)
                {
                    try
                    {
                        // Explicitly pick up a potentially methodimpl'ed Dispose
                        if (disposing)
                            ((IDisposable)_stream).Dispose();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }

            public override void Flush()
            {
                lock (_stream)
                    _stream.Flush();
            }

            public override int Read(byte[] bytes, int offset, int count)
            {
                lock (_stream)
                    return _stream.Read(bytes, offset, count);
            }

            public override int Read(Span<byte> buffer)
            {
                lock (_stream)
                    return _stream.Read(buffer);
            }

            public override int ReadByte()
            {
                lock (_stream)
                    return _stream.ReadByte();
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                bool overridesBeginRead = _stream.HasOverriddenBeginEndRead();

                lock (_stream)
                {
                    // If the Stream does have its own BeginRead implementation, then we must use that override.
                    // If it doesn't, then we'll use the base implementation, but we'll make sure that the logic
                    // which ensures only one asynchronous operation does so with an asynchronous wait rather
                    // than a synchronous wait.  A synchronous wait will result in a deadlock condition, because
                    // the EndXx method for the outstanding async operation won't be able to acquire the lock on
                    // _stream due to this call blocked while holding the lock.
                    return overridesBeginRead ?
                        _stream.BeginRead(buffer, offset, count, callback, state) :
                        _stream.BeginReadInternal(buffer, offset, count, callback, state, serializeAsynchronously: true, apm: true);
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException(nameof(asyncResult));

                lock (_stream)
                    return _stream.EndRead(asyncResult);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                lock (_stream)
                    return _stream.Seek(offset, origin);
            }

            public override void SetLength(long length)
            {
                lock (_stream)
                    _stream.SetLength(length);
            }

            public override void Write(byte[] bytes, int offset, int count)
            {
                lock (_stream)
                    _stream.Write(bytes, offset, count);
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                lock (_stream)
                    _stream.Write(buffer);
            }

            public override void WriteByte(byte b)
            {
                lock (_stream)
                    _stream.WriteByte(b);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                bool overridesBeginWrite = _stream.HasOverriddenBeginEndWrite();

                lock (_stream)
                {
                    // If the Stream does have its own BeginWrite implementation, then we must use that override.
                    // If it doesn't, then we'll use the base implementation, but we'll make sure that the logic
                    // which ensures only one asynchronous operation does so with an asynchronous wait rather
                    // than a synchronous wait.  A synchronous wait will result in a deadlock condition, because
                    // the EndXx method for the outstanding async operation won't be able to acquire the lock on
                    // _stream due to this call blocked while holding the lock.
                    return overridesBeginWrite ?
                        _stream.BeginWrite(buffer, offset, count, callback, state) :
                        _stream.BeginWriteInternal(buffer, offset, count, callback, state, serializeAsynchronously: true, apm: true);
                }
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException(nameof(asyncResult));

                lock (_stream)
                    _stream.EndWrite(asyncResult);
            }
        }
    }
}
