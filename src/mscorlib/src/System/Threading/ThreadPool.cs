// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: Class for creating and managing a threadpool
**
**
=============================================================================*/

#pragma warning disable 0420

/*
 * Below you'll notice two sets of APIs that are separated by the
 * use of 'Unsafe' in their names.  The unsafe versions are called
 * that because they do not propagate the calling stack onto the
 * worker thread.  This allows code to lose the calling stack and 
 * thereby elevate its security privileges.  Note that this operation
 * is much akin to the combined ability to control security policy
 * and control security evidence.  With these privileges, a person 
 * can gain the right to load assemblies that are fully trusted which
 * then assert full trust and can call any code they want regardless
 * of the previous stack information.
 */

namespace System.Threading
{
    using System.Security;
    using System.Security.Permissions;
    using System;
    using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Tasks;

    internal static class ThreadPoolGlobals
    {
        //Per-appDomain quantum (in ms) for which the thread keeps processing
        //requests in the current domain.
        public static uint tpQuantum;

        public static int processorCount;

        public static bool tpHosted;

        public static volatile bool vmTpInitialized;
        public static bool enableWorkerTracking;

        [SecurityCritical]
        public static ThreadPoolWorkQueue workQueue;

        [SecurityCritical]
        internal static void Initialize()
        {
            tpQuantum = 30U;
            processorCount = Environment.ProcessorCount;
            tpHosted = ThreadPool.IsThreadPoolHosted();
            workQueue = new ThreadPoolWorkQueue();
        }
    }

    internal sealed class ThreadTaskLocals
    {
        private readonly StackGuard m_stackGuard = new StackGuard();
        private ThreadPoolWorkQueueThreadLocals m_localQueues;

        internal Task CurrentTask;

        public StackGuard StackGuard
        {
            get { return m_stackGuard; }
        }

        public ThreadPoolWorkQueueThreadLocals LocalQueues
        {
            get { return m_localQueues; }
            set { m_localQueues = value; }
        }
    }

    internal sealed class ThreadPoolWorkQueue
    {
        // Simple sparsely populated array to allow lock-free reading.
        internal class SparseArray<T> where T : class
        {
            private Snapshot m_current;

            internal sealed class Snapshot
            {
                public readonly T[] Data;
                private int m_length;

                internal Snapshot(int initialSize, int initalLength = 0)
                {
                    Data = new T[initialSize];
                    m_length = initalLength;
                }

                internal int ActiveLength => m_length;

                internal void IncrementLength()
                {
                    m_length++;
                }
            }

            internal SparseArray(int initialSize)
            {
                m_current = new Snapshot(initialSize);  
            }

            internal Snapshot Current
            {
                get { return m_current; }
            }

            internal int Add(T e)
            {
                while (true)
                {
                    var current = m_current;
                    lock (current)
                    {
                        if (current != m_current)
                        {
                            // If there was a race condition, we start over again.
                            continue;
                        }

                        var array = current.Data;

                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] == null)
                            {
                                Volatile.Write(ref array[i], e);
                                if (i + 1 > current.ActiveLength)
                                {
                                    current.IncrementLength();
                                }
                                return i;
                            }
                        }

                        var oldLength = array.Length;
                        var newSnapshot = new Snapshot(oldLength * 2, oldLength + 1);
                        T[] newArray = newSnapshot.Data;

                        Array.Copy(array, newArray, oldLength);
                        newArray[oldLength + 1] = e;
                        m_current = newSnapshot;
                        return oldLength + 1;
                    }
                }
            }

            internal void Remove(T e)
            {
                while (true)
                {
                    var current = m_current;
                    lock (current)
                    {
                        if (current != m_current)
                        {
                            // If there was a race condition, we start over again.
                            continue;
                        }

                        var array = current.Data;
                        var length = current.ActiveLength;

                        for (int i = 0; i < length; i++)
                        {
                            if (array[i] == e)
                            {
                                Volatile.Write(ref array[i], null);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        internal class WorkStealingQueue
        {
            private const int INITIAL_SIZE = 32;
            internal volatile IThreadPoolWorkItem[] m_array = new IThreadPoolWorkItem[INITIAL_SIZE];
            private volatile int m_mask = INITIAL_SIZE - 1;

#if DEBUG
            // in debug builds, start at the end so we exercise the index reset logic.
            private const int START_INDEX = int.MaxValue;
#else
            private const int START_INDEX = 0;
#endif

            private volatile int m_headIndex = START_INDEX;
            private volatile int m_tailIndex = START_INDEX;

            private SpinLock m_foreignLock = new SpinLock(false);

            public void LocalPush(IThreadPoolWorkItem obj)
            {
                int tail = m_tailIndex;

                // We're going to increment the tail; if we'll overflow, then we need to reset our counts
                if (tail == int.MaxValue)
                {
                    tail = LocalPushOverflow(tail);
                }

                // When there are at least 2 elements' worth of space, we can take the fast path.
                if (tail < m_headIndex + m_mask)
                {
                    Volatile.Write(ref m_array[tail & m_mask], obj);
                    m_tailIndex = tail + 1;
                    return;
                }

                LocalPushPreventSteal(obj, tail);
            }

            private void LocalPushPreventSteal(IThreadPoolWorkItem obj, int tail)
            {
                // We need to contend with foreign pops, so we lock.
                bool lockTaken = false;
                try
                {
                    m_foreignLock.Enter(ref lockTaken);

                    int head = m_headIndex;
                    int count = m_tailIndex - m_headIndex;

                    // If there is still space (one left), just add the element.
                    if (count >= m_mask)
                    {
                        // We're full; expand the queue by doubling its size.
                        IThreadPoolWorkItem[] newArray = new IThreadPoolWorkItem[m_array.Length << 1];
                        for (int i = 0; i < m_array.Length; i++)
                            newArray[i] = m_array[(i + head) & m_mask];

                        // Reset the field values, incl. the mask.
                        m_array = newArray;
                        m_headIndex = 0;
                        m_tailIndex = tail = count;
                        m_mask = (m_mask << 1) | 1;
                    }

                    Volatile.Write(ref m_array[tail & m_mask], obj);
                    m_tailIndex = tail + 1;
                }
                finally
                {
                    if (lockTaken)
                        m_foreignLock.Exit(false);
                }
            }

            private int LocalPushOverflow(int tail)
            {
                bool lockTaken = false;
                try
                {
                    m_foreignLock.Enter(ref lockTaken);

                    if (m_tailIndex == int.MaxValue)
                    {
                        //
                        // Rather than resetting to zero, we'll just mask off the bits we don't care about.
                        // This way we don't need to rearrange the items already in the queue; they'll be found
                        // correctly exactly where they are.  One subtlety here is that we need to make sure that
                        // if head is currently < tail, it remains that way.  This happens to just fall out from
                        // the bit-masking, because we only do this if tail == int.MaxValue, meaning that all
                        // bits are set, so all of the bits we're keeping will also be set.  Thus it's impossible
                        // for the head to end up > than the tail, since you can't set any more bits than all of 
                        // them.
                        //
                        m_headIndex = m_headIndex & m_mask;
                        m_tailIndex = tail = m_tailIndex & m_mask;
                        Contract.Assert(m_headIndex <= m_tailIndex);
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_foreignLock.Exit(true);
                }

                return tail;
            }

            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
            public bool LocalFindAndPop(IThreadPoolWorkItem obj)
            {
                // Fast path: check the tail. If equal, we can skip the lock.
                if (m_array[(m_tailIndex - 1) & m_mask] == obj)
                {
                    IThreadPoolWorkItem unused = null;
                    if (LocalPop(ref unused))
                    {
                        Contract.Assert(unused == obj);
                        return true;
                    }
                    return false;
                }

                return LocalFindAndPopSteal(obj);
            }

            private bool LocalFindAndPopSteal(IThreadPoolWorkItem obj)
            {
                // Else, do an O(N) search for the work item. The theory of work stealing and our
                // inlining logic is that most waits will happen on recently queued work.  And
                // since recently queued work will be close to the tail end (which is where we
                // begin our search), we will likely find it quickly.  In the worst case, we
                // will traverse the whole local queue; this is typically not going to be a
                // problem (although degenerate cases are clearly an issue) because local work
                // queues tend to be somewhat shallow in length, and because if we fail to find
                // the work item, we are about to block anyway (which is very expensive).
                for (int i = m_tailIndex - 2; i >= m_headIndex; i--)
                {
                    if (m_array[i & m_mask] == obj)
                    {
                        // If we found the element, block out steals to avoid interference.
                        bool lockTaken = false;
                        try
                        {
                            m_foreignLock.Enter(ref lockTaken);

                            // If we encountered a race condition, bail.
                            if (m_array[i & m_mask] == null)
                                return false;

                            // Otherwise, null out the element.
                            Volatile.Write(ref m_array[i & m_mask], null);

                            // And then check to see if we can fix up the indexes (if we're at
                            // the edge).  If we can't, we just leave nulls in the array and they'll
                            // get filtered out eventually (but may lead to superflous resizing).
                            if (i == m_tailIndex)
                                m_tailIndex -= 1;
                            else if (i == m_headIndex)
                                m_headIndex += 1;

                            return true;
                        }
                        finally
                        {
                            if (lockTaken)
                                m_foreignLock.Exit(false);
                        }
                    }
                }

                return false;
            }

            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
            public bool LocalPop(ref IThreadPoolWorkItem obj)
            {
                while (true)
                {
                    // Decrement the tail using a fence to ensure subsequent read doesn't come before.
                    int tail = m_tailIndex;
                    if (m_headIndex >= tail)
                    {
                        return false;
                    }

                    tail -= 1;
                    Interlocked.Exchange(ref m_tailIndex, tail);

                    // If there is no interaction with a take, we can head down the fast path.
                    if (m_headIndex <= tail)
                    {
                        int idx = tail & m_mask;
                        obj = Volatile.Read(ref m_array[idx]);

                        // Check for nulls in the array.
                        if (obj == null) continue;

                        m_array[idx] = null;
                        return true;
                    }

                    bool skip;
                    bool result = LocalPopLocked(ref obj, ref tail, out skip);
                    // continue if null in array
                    if (skip) continue;

                    return result;
                }
            }

            private bool LocalPopLocked(ref IThreadPoolWorkItem obj, ref int tail, out bool skip)
            {
                // Interaction with takes: 0 or 1 elements left.
                bool lockTaken = false;
                try
                {
                    m_foreignLock.Enter(ref lockTaken);

                    if (m_headIndex <= tail)
                    {
                        // Element still available. Take it.
                        int idx = tail & m_mask;
                        obj = Volatile.Read(ref m_array[idx]);

                        // Check for nulls in the array.
                        if (obj == null) {
                            skip = true;
                            return false;
                        };

                        m_array[idx] = null;
                        skip = false;
                        return true;
                    }
                    else
                    {
                        // If we encountered a race condition and element was stolen, restore the tail.
                        m_tailIndex = tail + 1;
                        skip = false;
                        return false;
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_foreignLock.Exit(false);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TrySteal(ref IThreadPoolWorkItem obj, ref bool missedSteal)
            {
                return TrySteal(ref obj, ref missedSteal, 0); // no blocking by default.
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TrySteal(ref IThreadPoolWorkItem obj, ref bool missedSteal, int millisecondsTimeout)
            {
                return (m_headIndex >= m_tailIndex) ? false : TryStealWithItems(ref obj, ref missedSteal, millisecondsTimeout);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private bool TryStealWithItems(ref IThreadPoolWorkItem obj, ref bool missedSteal, int millisecondsTimeout)
            {
                while (true)
                {
                    bool taken = false;
                    try
                    {
                        m_foreignLock.TryEnter(millisecondsTimeout, ref taken);
                        if (taken)
                        {
                            // Increment head, and ensure read of tail doesn't move before it (fence).
                            int head = m_headIndex;
                            Interlocked.Exchange(ref m_headIndex, head + 1);

                            if (head < m_tailIndex)
                            {
                                int idx = head & m_mask;
                                obj = Volatile.Read(ref m_array[idx]);

                                // Check for nulls in the array.
                                if (obj == null)
                                {
                                    if (m_headIndex >= m_tailIndex) return false;
                                    continue;
                                };

                                m_array[idx] = null;
                                return true;
                            }
                            else
                            {
                                // Failed, restore head.
                                m_headIndex = head;
                                missedSteal = true;
                            }
                        }
                        else
                        {
                            missedSteal = true;
                        }
                    }
                    finally
                    {
                        if (taken)
                            m_foreignLock.Exit(false);
                    }

                    return false;
                }
            }
        }

        // To seperate `indexes` and `Next` to reduce cache line false sharing between them
        [StructLayout(LayoutKind.Explicit)]
        internal class QueueSegment
        {
            // Holds the indexes of the lowest and highest valid elements of the nodes array.
            // The low index is in the lower 16 bits, high index is in the upper 16 bits.
            // Use GetIndexes and CompareExchangeIndexes to manipulate this.
            [FieldOffset(0)]
            private volatile int indexes;

            // Holds a segment of the queue.  Enqueues/Dequeues start at element 0, and work their way up.
            [FieldOffset(64)]
            internal readonly IThreadPoolWorkItem[] nodes;
            private const int QueueSegmentLength = 256;

            [FieldOffset(128)]
            // The next segment in the queue.
            public volatile QueueSegment Next;


            const int SixteenBits = 0xffff;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void GetIndexes(out int upper, out int lower)
            {
                int i = indexes;
                upper = (i >> 16) & SixteenBits;
                lower = i & SixteenBits;

                Contract.Assert(upper >= lower);
                Contract.Assert(upper <= nodes.Length);
                Contract.Assert(lower <= nodes.Length);
                Contract.Assert(upper >= 0);
                Contract.Assert(lower >= 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool CompareExchangeIndexes(ref int prevUpper, int newUpper, ref int prevLower, int newLower)
            {
                Contract.Assert(newUpper >= newLower);
                Contract.Assert(newUpper <= nodes.Length);
                Contract.Assert(newLower <= nodes.Length);
                Contract.Assert(newUpper >= 0);
                Contract.Assert(newLower >= 0);
                Contract.Assert(newUpper >= prevUpper);
                Contract.Assert(newLower >= prevLower);
                Contract.Assert(newUpper == prevUpper ^ newLower == prevLower);

                int oldIndexes = (prevUpper << 16) | (prevLower & SixteenBits);
                int newIndexes = (newUpper << 16) | (newLower & SixteenBits);
                int prevIndexes = Interlocked.CompareExchange(ref indexes, newIndexes, oldIndexes);
                prevUpper = (prevIndexes >> 16) & SixteenBits;
                prevLower = prevIndexes & SixteenBits;
                return prevIndexes == oldIndexes;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            public QueueSegment()
            {
                Contract.Assert(QueueSegmentLength <= SixteenBits);
                nodes = new IThreadPoolWorkItem[QueueSegmentLength];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsUsedUp()
            {
                int upper, lower;
                GetIndexes(out upper, out lower);
                return (upper == nodes.Length) && 
                       (lower == nodes.Length);
            }

            public bool TryEnqueue(IThreadPoolWorkItem node)
            {
                //
                // If there's room in this segment, atomically increment the upper count (to reserve
                // space for this node), then store the node.
                // Note that this leaves a window where it will look like there is data in that
                // array slot, but it hasn't been written yet.  This is taken care of in TryDequeue
                // with a busy-wait loop, waiting for the element to become non-null.  This implies
                // that we can never store null nodes in this data structure.
                //
                Contract.Assert(null != node);

                int upper, lower;
                GetIndexes(out upper, out lower);

                while (true)
                {
                    if (upper == nodes.Length)
                        return false;

                    if (CompareExchangeIndexes(ref upper, upper + 1, ref lower, lower))
                    {
                        Contract.Assert(Volatile.Read(ref nodes[upper]) == null);
                        Volatile.Write(ref nodes[upper], node);
                        return true;
                    }
                }
            }

            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
            public bool TryDequeue(ref IThreadPoolWorkItem node)
            {
                //
                // If there are nodes in this segment, increment the lower count, then take the
                // element we find there.
                //
                int upper, lower;
                GetIndexes(out upper, out lower);

                while(true)
                {
                    if (lower == upper)
                    {
                        return false;
                    }

                    if (CompareExchangeIndexes(ref upper, upper, ref lower, lower + 1))
                    {
                        // It's possible that a concurrent call to Enqueue hasn't yet
                        // written the node reference to the array.  We need to spin until
                        // it shows up.
                        SpinWait spinner = new SpinWait();
                        while ((node = Volatile.Read(ref nodes[lower])) == null)
                            spinner.SpinOnce();

                        // Null-out the reference so the object can be GC'd earlier.
                        nodes[lower] = null;

                        return true;
                    }
                }
            }
        }

        // The head and tail of the queue.  We enqueue to the head, and dequeue from the tail.
        internal volatile QueueSegment queueHead;
        internal volatile QueueSegment queueTail;
        internal bool loggingEnabled;

        private static int NextSearchStart;

        internal static SparseArray<WorkStealingQueue> allThreadQueues = new SparseArray<WorkStealingQueue>(16);

        private volatile int numOutstandingThreadRequests = 0;
      
        public ThreadPoolWorkQueue()
        {
            queueTail = queueHead = new QueueSegment();
            loggingEnabled = FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.ThreadPool|FrameworkEventSource.Keywords.ThreadTransfer);
        }

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ThreadPoolWorkQueueThreadLocals EnsureCurrentThreadHasQueue()
        {
            // Don't add thread work pool for non-threadpool threads
            var currentThread = Thread.CurrentThread;
            if (!currentThread.IsThreadPoolThread) return null;

            var queue = currentThread.ThreadTaskLocals.LocalQueues;
            return null != queue ? queue : (currentThread.ThreadTaskLocals.LocalQueues = new ThreadPoolWorkQueueThreadLocals(this));
        }

        [SecurityCritical]
        internal void EnsureThreadRequested()
        {
            //
            // If we have not yet requested #procs threads from the VM, then request a new thread.
            // Note that there is a separate count in the VM which will also be incremented in this case, 
            // which is handled by RequestWorkerThread.
            //
            int count = numOutstandingThreadRequests;
            var procCount = ThreadPoolGlobals.processorCount;
            while (count < procCount)
            {
                int prev = Interlocked.CompareExchange(ref numOutstandingThreadRequests, count+1, count);
                if (prev == count)
                {
                    ThreadPool.RequestWorkerThread();
                    break;
                }
                count = prev;
            }
        }

        [SecurityCritical]
        internal void MarkThreadRequestSatisfied()
        {
            //
            // The VM has called us, so one of our outstanding thread requests has been satisfied.
            // Decrement the count so that future calls to EnsureThreadRequested will succeed.
            // Note that there is a separate count in the VM which has already been decremented by the VM
            // by the time we reach this point.
            //
            int count = numOutstandingThreadRequests;
            while (count > 0)
            {
                int prev = Interlocked.CompareExchange(ref numOutstandingThreadRequests, count - 1, count);
                if (prev == count)
                {
                    break;
                }
                count = prev;
            }
        }

        [SecurityCritical]
        public void EnqueueGlobal(IThreadPoolWorkItem callback)
        {
            if (loggingEnabled)
                System.Diagnostics.Tracing.FrameworkEventSource.Log.ThreadPoolEnqueueWorkObject(callback);

            QueueSegment head = queueHead;
            QueueSegment newSegment = null;

            while (!head.TryEnqueue(callback))
            {
                if (newSegment == null) newSegment = new QueueSegment();

                if (Interlocked.CompareExchange(ref head.Next, newSegment, null) == null)
                {
                    newSegment = null;
                }

                while (head.Next != null)
                {
                    Interlocked.CompareExchange(ref queueHead, head.Next, head);
                    head = queueHead;
                }
            }

            EnsureThreadRequested();
        }

        [SecurityCritical]
        public void Enqueue(IThreadPoolWorkItem callback, bool forceGlobal)
        {
            ThreadPoolWorkQueueThreadLocals tl = null;
            if (forceGlobal || (tl = EnsureCurrentThreadHasQueue()) == null)
            {
                EnqueueGlobal(callback);
                return;
            }

            if (loggingEnabled)
                System.Diagnostics.Tracing.FrameworkEventSource.Log.ThreadPoolEnqueueWorkObject(callback);
            
            tl.workStealingQueue.LocalPush(callback);

            EnsureThreadRequested();
        }

        [SecurityCritical]
        internal bool LocalFindAndPop(IThreadPoolWorkItem callback)
        {
            ThreadPoolWorkQueueThreadLocals tl = Thread.CurrentThread.ThreadTaskLocals.LocalQueues;
            if (null == tl)
                return false;

            return tl.workStealingQueue.LocalFindAndPop(callback);
        }

        [SecurityCritical]
        public void Dequeue(WorkStealingQueue wsq, ref IThreadPoolWorkItem callback, out bool missedSteal)
        {
            missedSteal = false;
            if (wsq?.LocalPop(ref callback) ?? false)
            {
                Contract.Assert(null != callback);
                return;
            }

            DequeueSeek(wsq, ref callback, ref missedSteal);
        }

        private void DequeueSeek(WorkStealingQueue wsq, ref IThreadPoolWorkItem callback, ref bool missedSteal)
        {
            QueueSegment tail = queueTail;
            while (true)
            {
                if (tail.TryDequeue(ref callback))
                {
                    Contract.Assert(null != callback);
                    return;
                }

                if (null == tail.Next || !tail.IsUsedUp())
                {
                    break;
                }
                else
                {
                    Interlocked.CompareExchange(ref queueTail, tail.Next, tail);
                    tail = queueTail;
                }
            }

            // allThreadQueues.Data.Length is a power of 2, initally 16
            // Move next steal start on by 9 = (8 + 1) rather than 1
            // It means the search still progresses through all start points evenly in a deterministic manner
            // However it also interleaves them to reduce collisions between threads  
            var startIndex = Interlocked.Add(ref NextSearchStart, 9);

            if (wsq == null)
            {
                DequeueSteal(startIndex, ref callback, ref missedSteal);
            }
            else
            {
                DequeueStealWithQueue(wsq, startIndex, ref callback, ref missedSteal);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DequeueStealWithQueue(WorkStealingQueue wsq, int index, ref IThreadPoolWorkItem callback, ref bool missedSteal)
        {
            var otherQueues = allThreadQueues.Current;
            var total = otherQueues.ActiveLength;
            var data = otherQueues.Data;
            Contract.Assert(data.Length >= total);

            var remaining = total;
            // Only positive indices
            index = (index & 0x7fff) % total;

            while (remaining > 0)
            {
                remaining--;
                WorkStealingQueue otherQueue = Volatile.Read(ref data[index]);
                index = (index + 1 >= total) ? 0 : index + 1;
                if (otherQueue != null &&
                    otherQueue != wsq &&
                    otherQueue.TrySteal(ref callback, ref missedSteal))
                {
                    Contract.Assert(null != callback);
                    return;
                }
            }

            if (total != otherQueues.ActiveLength) missedSteal = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DequeueSteal(int index, ref IThreadPoolWorkItem callback, ref bool missedSteal)
        {
            var otherQueues = allThreadQueues.Current;
            var total = otherQueues.ActiveLength;
            // No local queue, may not be other queues
            if (total == 0) return;
            var data = otherQueues.Data;
            Contract.Assert(data.Length >= total);

            var remaining = total;
            // Only positive indices
            index = (index & 0x7fff) % total;

            while (remaining > 0)
            {
                remaining--;
                WorkStealingQueue otherQueue = Volatile.Read(ref data[index]);
                index = (index + 1 == total) ? 0 : index + 1;
                if (otherQueue != null &&
                    otherQueue.TrySteal(ref callback, ref missedSteal))
                {
                    Contract.Assert(null != callback);
                    return;
                }
            }

            if (total != otherQueues.ActiveLength) missedSteal = true;
        }

        [SecurityCritical]
        static internal bool Dispatch()
        {
            var threadLocals = Thread.CurrentThread.ThreadTaskLocals;
            var workQueue = ThreadPoolGlobals.workQueue;
            //
            // The clock is ticking!  We have ThreadPoolGlobals.tpQuantum milliseconds to get some work done, and then
            // we need to return to the VM.
            //
            int quantumStartTime = Environment.TickCount;

            //
            // Update our records to indicate that an outstanding request for a thread has now been fulfilled.
            // From this point on, we are responsible for requesting another thread if we stop working for any
            // reason, and we believe there might still be work in the queue.
            //
            // Note that if this thread is aborted before we get a chance to request another one, the VM will
            // record a thread request on our behalf.  So we don't need to worry about getting aborted right here.
            //
            workQueue.MarkThreadRequestSatisfied();

            // Has the desire for logging changed since the last time we entered?
            workQueue.loggingEnabled = FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.ThreadPool|FrameworkEventSource.Keywords.ThreadTransfer);

            //
            // Assume that we're going to need another thread if this one returns to the VM.  We'll set this to 
            // false later, but only if we're absolutely certain that the queue is empty.

            bool needAnotherThread = true;
            IThreadPoolWorkItem workItem = null;
            try
            {

                //
                // Loop until our quantum expires.
                //
                while ((Environment.TickCount - quantumStartTime) < ThreadPoolGlobals.tpQuantum)
                {
                    //
                    // Get our thread-local queue, may have been created by work item
                    //
                    var wsq = threadLocals.LocalQueues?.workStealingQueue;

                    //
                    // Dequeue and EnsureThreadRequested must be protected from ThreadAbortException.  
                    // These are fast, so this will not delay aborts/AD-unloads for very long.
                    //
                    try { }
                    finally
                    {
                        bool missedSteal = false;
                        workQueue.Dequeue(wsq, ref workItem, out missedSteal);

                        if (workItem == null)
                        {
                            //
                            // No work.  We're going to return to the VM once we leave this protected region.
                            // If we missed a steal, though, there may be more work in the queue.
                            // Instead of looping around and trying again, we'll just request another thread.  This way
                            // we won't starve other AppDomains while we spin trying to get locks, and hopefully the thread
                            // that owns the contended work-stealing queue will pick up its own workitems in the meantime, 
                            // which will be more efficient than this thread doing it anyway.
                            //
                            needAnotherThread = missedSteal;
                        }
                        else
                        {
                            //
                            // If we found work, there may be more work.  Ask for another thread so that the other work can be processed
                            // in parallel.  Note that this will only ask for a max of #procs threads, so it's safe to call it for every dequeue.
                            //
                            workQueue.EnsureThreadRequested();
                        }
                    }

                    if (workItem == null)
                    {
                        // Tell the VM we're returning normally, not because Hill Climbing asked us to return.
                        return true;
                    }
                    else
                    {
                        if (workQueue.loggingEnabled)
                            System.Diagnostics.Tracing.FrameworkEventSource.Log.ThreadPoolDequeueWorkObject(workItem);

                        //
                        // Execute the workitem outside of any finally blocks, so that it can be aborted if needed.
                        //
                        if (ThreadPoolGlobals.enableWorkerTracking)
                        {
                            bool reportedStatus = false;
                            try
                            {
                                try { }
                                finally
                                {
                                    ThreadPool.ReportThreadStatus(true);
                                    reportedStatus = true;
                                }
                                workItem.ExecuteWorkItem();
                                workItem = null;
                            }
                            finally
                            {
                                if (reportedStatus)
                                    ThreadPool.ReportThreadStatus(false);
                            }
                        }
                        else
                        {
                            workItem.ExecuteWorkItem();
                            workItem = null;
                        }

                        // 
                        // Notify the VM that we executed this workitem.  This is also our opportunity to ask whether Hill Climbing wants
                        // us to return the thread to the pool or not.
                        //
                        if (!ThreadPool.NotifyWorkItemComplete())
                            return false;
                    }
                }
                // If we get here, it's because our quantum expired.  Tell the VM we're returning normally.
                return true;
            }
            catch (ThreadAbortException tae)
            {
                //
                // This is here to catch the case where this thread is aborted between the time we exit the finally block in the dispatch
                // loop, and the time we execute the work item.  QueueUserWorkItemCallback uses this to update its accounting of whether
                // it was executed or not (in debug builds only).  Task uses this to communicate the ThreadAbortException to anyone
                // who waits for the task to complete.
                //
                if (workItem != null)
                    workItem.MarkAborted(tae);
                
                //
                // In this case, the VM is going to request another thread on our behalf.  No need to do it twice.
                //
                needAnotherThread = false;
                // throw;  //no need to explicitly rethrow a ThreadAbortException, and doing so causes allocations on amd64.
            }
            finally
            {
                //
                // If we are exiting for any reason other than that the queue is definitely empty, ask for another
                // thread to pick up where we left off.
                //
                if (needAnotherThread)
                    workQueue.EnsureThreadRequested();
            }

            // we can never reach this point, but the C# compiler doesn't know that, because it doesn't know the ThreadAbortException will be reraised above.
            Contract.Assert(false);
            return true;
        }
    }

    // Holds a WorkStealingQueue, and remmoves it from the list when this object is no longer referened.
    internal sealed class ThreadPoolWorkQueueThreadLocals
    {

        public readonly ThreadPoolWorkQueue workQueue;
        public readonly ThreadPoolWorkQueue.WorkStealingQueue workStealingQueue;

        public ThreadPoolWorkQueueThreadLocals(ThreadPoolWorkQueue tpq)
        {
            workQueue = tpq;
            workStealingQueue = new ThreadPoolWorkQueue.WorkStealingQueue();
            ThreadPoolWorkQueue.allThreadQueues.Add(workStealingQueue);
        }

        [SecurityCritical]
        private void CleanUp()
        {
            if (null != workStealingQueue)
            {
                if (null != workQueue)
                {
                    bool done = false;
                    while (!done)
                    {
                        // Ensure that we won't be aborted between LocalPop and Enqueue.
                        try { }
                        finally
                        {
                            IThreadPoolWorkItem cb = null;
                            if (workStealingQueue.LocalPop(ref cb))
                            {
                                Contract.Assert(null != cb);
                                workQueue.EnqueueGlobal(cb);
                            }
                            else
                            {
                                done = true;
                            }
                        }
                    }
                }

                ThreadPoolWorkQueue.allThreadQueues.Remove(workStealingQueue);
            }
        }

        [SecuritySafeCritical]
        ~ThreadPoolWorkQueueThreadLocals()
        {
            // Since the purpose of calling CleanUp is to transfer any pending workitems into the global
            // queue so that they will be executed by another thread, there's no point in doing this cleanup
            // if we're in the process of shutting down or unloading the AD.  In those cases, the work won't
            // execute anyway.  And there are subtle race conditions involved there that would lead us to do the wrong
            // thing anyway.  So we'll only clean up if this is a "normal" finalization.
            if (!(Environment.HasShutdownStarted || AppDomain.CurrentDomain.IsFinalizingForUnload()))
                CleanUp();
        }
    }

    internal sealed class RegisteredWaitHandleSafe : CriticalFinalizerObject
    {
        private static IntPtr InvalidHandle
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get
            {
                return Win32Native.INVALID_HANDLE_VALUE;
            }
        }
        private IntPtr registeredWaitHandle;
        private WaitHandle m_internalWaitObject;
        private bool bReleaseNeeded = false;
        private volatile int m_lock = 0;

        #if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
        #endif
        internal RegisteredWaitHandleSafe()
        {
            registeredWaitHandle = InvalidHandle;
        }

        internal IntPtr GetHandle()
        {
           return registeredWaitHandle;
        }
    
        internal void SetHandle(IntPtr handle)
        {
            registeredWaitHandle = handle;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void SetWaitObject(WaitHandle waitObject)
        {
            // needed for DangerousAddRef
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                m_internalWaitObject = waitObject;
                if (waitObject != null)
                {
                    m_internalWaitObject.SafeWaitHandle.DangerousAddRef(ref bReleaseNeeded);
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal bool Unregister(
             WaitHandle     waitObject          // object to be notified when all callbacks to delegates have completed
             )
        {
            bool result = false;
            // needed for DangerousRelease
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                // lock(this) cannot be used reliably in Cer since thin lock could be
                // promoted to syncblock and that is not a guaranteed operation
                bool bLockTaken = false;
                do 
                {
                    if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
                    {
                        bLockTaken = true;
                        try
                        {
                            if (ValidHandle())
                            {
                                result = UnregisterWaitNative(GetHandle(), waitObject == null ? null : waitObject.SafeWaitHandle);
                                if (result == true)
                                {
                                    if (bReleaseNeeded)
                                    {
                                        m_internalWaitObject.SafeWaitHandle.DangerousRelease();
                                        bReleaseNeeded = false;
                                    }
                                    // if result not true don't release/suppress here so finalizer can make another attempt
                                    SetHandle(InvalidHandle);
                                    m_internalWaitObject = null;
                                    GC.SuppressFinalize(this);
                                }
                            }
                        }
                        finally
                        {
                            m_lock = 0;
                        }
                    }
                    Thread.SpinWait(1);     // yield to processor
                }
                while (!bLockTaken);
            }
            return result;
        }

        private bool ValidHandle()
        {
            return (registeredWaitHandle != InvalidHandle && registeredWaitHandle != IntPtr.Zero);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        ~RegisteredWaitHandleSafe()
        {
            // if the app has already unregistered the wait, there is nothing to cleanup
            // we can detect this by checking the handle. Normally, there is no race condition here
            // so no need to protect reading of handle. However, if this object gets 
            // resurrected and then someone does an unregister, it would introduce a race condition
            //
            // PrepareConstrainedRegions call not needed since finalizer already in Cer
            //
            // lock(this) cannot be used reliably even in Cer since thin lock could be
            // promoted to syncblock and that is not a guaranteed operation
            //
            // Note that we will not "spin" to get this lock.  We make only a single attempt;
            // if we can't get the lock, it means some other thread is in the middle of a call
            // to Unregister, which will do the work of the finalizer anyway.
            //
            // Further, it's actually critical that we *not* wait for the lock here, because
            // the other thread that's in the middle of Unregister may be suspended for shutdown.
            // Then, during the live-object finalization phase of shutdown, this thread would
            // end up spinning forever, as the other thread would never release the lock.
            // This will result in a "leak" of sorts (since the handle will not be cleaned up)
            // but the process is exiting anyway.
            //
            // During AD-unload, we don’t finalize live objects until all threads have been 
            // aborted out of the AD.  Since these locked regions are CERs, we won’t abort them 
            // while the lock is held.  So there should be no leak on AD-unload.
            //
            if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
            {
                try
                {
                    if (ValidHandle())
                    {
                        WaitHandleCleanupNative(registeredWaitHandle);
                        if (bReleaseNeeded)
                        {
                            m_internalWaitObject.SafeWaitHandle.DangerousRelease();
                            bReleaseNeeded = false;
                        }
                        SetHandle(InvalidHandle);
                        m_internalWaitObject = null;
                    }
                }
                finally
                {
                    m_lock = 0;
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void WaitHandleCleanupNative(IntPtr handle);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool UnregisterWaitNative(IntPtr handle, SafeHandle waitObject);
    }

[System.Runtime.InteropServices.ComVisible(true)]
#if FEATURE_REMOTING    
    public sealed class RegisteredWaitHandle : MarshalByRefObject {
#else // FEATURE_REMOTING
    public sealed class RegisteredWaitHandle {
#endif // FEATURE_REMOTING
        private RegisteredWaitHandleSafe internalRegisteredWait;
    
        internal RegisteredWaitHandle()
        {
            internalRegisteredWait = new RegisteredWaitHandleSafe();
        }

        internal void SetHandle(IntPtr handle)
        {
           internalRegisteredWait.SetHandle(handle);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void SetWaitObject(WaitHandle waitObject)
        {
           internalRegisteredWait.SetWaitObject(waitObject);
        }

    
[System.Security.SecuritySafeCritical]  // auto-generated
[System.Runtime.InteropServices.ComVisible(true)]
        // This is the only public method on this class
        public bool Unregister(
             WaitHandle     waitObject          // object to be notified when all callbacks to delegates have completed
             )
        {
            return internalRegisteredWait.Unregister(waitObject);
        }
    }
    
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void WaitCallback(Object state);

    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void WaitOrTimerCallback(Object state, bool timedOut);  // signalled or timed out

    //
    // This type is necessary because VS 2010's debugger looks for a method named _ThreadPoolWaitCallbacck.PerformWaitCallback
    // on the stack to determine if a thread is a ThreadPool thread or not.  We have a better way to do this for .NET 4.5, but
    // still need to maintain compatibility with VS 2010.  When compat with VS 2010 is no longer an issue, this type may be
    // removed.
    //
    internal static class _ThreadPoolWaitCallback
    {
        [System.Security.SecurityCritical]
        static internal bool PerformWaitCallback()
        {
            return ThreadPoolWorkQueue.Dispatch();
        }
    }

    //
    // Interface to something that can be queued to the TP.  This is implemented by 
    // QueueUserWorkItemCallback, Task, and potentially other internal types.
    // For example, SemaphoreSlim represents callbacks using its own type that
    // implements IThreadPoolWorkItem.
    //
    // If we decide to expose some of the workstealing
    // stuff, this is NOT the thing we want to expose to the public.
    //
    internal interface IThreadPoolWorkItem
    {
        [SecurityCritical]
        void ExecuteWorkItem();
        [SecurityCritical]
        void MarkAborted(ThreadAbortException tae);
    }

    internal abstract class DeferrableWorkItem : IThreadPoolWorkItem
    {
        [SecurityCritical]
        public abstract void ExecuteWorkItem();

        [SecurityCritical]
        void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae) => MarkAborted(tae);

        [SecurityCritical]
        internal virtual void MarkAborted(ThreadAbortException tae) { /* noop */ }
    }

    internal abstract class UserWorkItem : DeferrableWorkItem
    {
        protected WaitCallback callback;

        protected UserWorkItem(WaitCallback waitCallback)
        {
            Contract.Assert(waitCallback != null, "Null callback passed to UserWorkItem!");
            callback = waitCallback;
        }
        
#if DEBUG
        private volatile int executed;

        ~UserWorkItem()
        {
            Contract.Assert(
                executed != 0 || Environment.HasShutdownStarted || AppDomain.CurrentDomain.IsFinalizingForUnload(),
                "A UserWorkItem was never called!");
        }

        protected void MarkExecuted(bool aborted)
        {
            GC.SuppressFinalize(this);
            Contract.Assert(
                0 == Interlocked.Exchange(ref executed, 1) || aborted,
                "A UserWorkItem was called twice!");
        }

        [SecurityCritical]
        internal override void MarkAborted(ThreadAbortException tae)
        {
            // this workitem didn't execute because we got a ThreadAbortException prior to the call to ExecuteWorkItem.  
            // This counts as being executed for our purposes.
            MarkExecuted(true);
        }
#endif
    }

    internal sealed class QueueUserWorkItemCallback : UserWorkItem
    {
        private readonly ExecutionContext context;
        private Object state;

        [SecurityCritical]
        internal QueueUserWorkItemCallback(WaitCallback waitCallback, Object stateObj, ExecutionContext ec)
            : base(waitCallback)
        {
            state = stateObj;
            context = ec;
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            ExecutionContext.Run(context, ccb, this);
        }

        [System.Security.SecurityCritical]
        static internal ContextCallback<QueueUserWorkItemCallback> ccb;

        [System.Security.SecurityCritical]
        static internal void Initialize()
        {
            ccb = (helper) =>
            {
                var state = helper.state;
                var callback = helper.callback;
                // Detach state for early GC as it may be unreferenced early in the call chain 
                helper.state = null;
                helper.callback = null;
                callback(state);
            };
        }
    }

    internal sealed class QueueUserWorkItemCallbackNoState : UserWorkItem
    {
        private ExecutionContext context;

        [SecurityCritical]
        internal QueueUserWorkItemCallbackNoState(WaitCallback waitCallback, ExecutionContext ec)
            : base(waitCallback)
        {
            context = ec;
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            ExecutionContext.Run(context, ccb, this);
        }

        [System.Security.SecurityCritical]
        static internal ContextCallback<QueueUserWorkItemCallbackNoState> ccb;

        [System.Security.SecurityCritical]
        static internal void Initialize()
        {
            ccb = (helper) =>
            {
                var callback = helper.callback;
                helper.callback = null;
                callback(null);
            };
        }
    }

    internal sealed class QueueUserWorkItemCallbackNoContext : UserWorkItem
    {
        private Object state;

        [SecurityCritical]
        internal QueueUserWorkItemCallbackNoContext(WaitCallback waitCallback, Object stateObj)
            : base(waitCallback)
        {
            state = stateObj;
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            var cb = callback;
            var s = state;
            // Detach state for early GC as it may be unreferenced early in the call chain 
            state = null;
            callback = null;
            cb(s);
        }
    }

    internal sealed class QueueUserWorkItemCallbackNoContextNoState : UserWorkItem
    {
        [SecurityCritical]
        internal QueueUserWorkItemCallbackNoContextNoState(WaitCallback waitCallback)
            : base(waitCallback)
        {
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            var cb = callback;
            callback = null;
            cb(null);
        }
    }

    internal sealed class QueueUserWorkItemCallbackDefaultContext : UserWorkItem
    {
        private Object state;

        [SecurityCritical]
        internal QueueUserWorkItemCallbackDefaultContext(WaitCallback waitCallback, Object stateObj)
            : base(waitCallback)
        {
            state = stateObj;
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            ExecutionContext.Run(ExecutionContext.PreAllocatedDefault, ccb, this);
        }

        [System.Security.SecurityCritical]
        static internal ContextCallback<QueueUserWorkItemCallbackDefaultContext> ccb;

        [System.Security.SecurityCritical]
        static internal void Initialize()
        {
            ccb = (helper) =>
            {
                var state = helper.state;
                var callback = helper.callback;
                // Detach state for early GC as it may be unreferenced early in the call chain 
                helper.state = null;
                helper.callback = null;
                callback(state);
            };
        }
    }

    internal sealed class QueueUserWorkItemCallbackDefaultContextNoState : UserWorkItem
    {
        [SecurityCritical]
        internal QueueUserWorkItemCallbackDefaultContextNoState(WaitCallback waitCallback)
            : base(waitCallback)
        {
        }

        [SecurityCritical]
        public override void ExecuteWorkItem()
        {
#if DEBUG
            MarkExecuted(false);
#endif
            ExecutionContext.Run(ExecutionContext.PreAllocatedDefault, ccb, this);
        }

        [System.Security.SecurityCritical]
        static internal ContextCallback<QueueUserWorkItemCallbackDefaultContextNoState> ccb;

        [System.Security.SecurityCritical]
        static internal void Initialize()
        {
            ccb = (helper) =>
            {
                var callback = helper.callback;
                helper.callback = null;
                callback(null);
            };
        }
    }

    internal class _ThreadPoolWaitOrTimerCallback
    {
        WaitOrTimerCallback _waitOrTimerCallback;
        ExecutionContext _executionContext;
        Object _state;
        [System.Security.SecurityCritical]
        static private ContextCallback<_ThreadPoolWaitOrTimerCallback> _ccbt;
        [System.Security.SecurityCritical]
        static private ContextCallback<_ThreadPoolWaitOrTimerCallback> _ccbf;

        [System.Security.SecurityCritical]
        static internal void Initialize()
        {
            _ccbt = (helper) => helper._waitOrTimerCallback(helper._state, true);
            _ccbf = (helper) => helper._waitOrTimerCallback(helper._state, false);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal _ThreadPoolWaitOrTimerCallback(WaitOrTimerCallback waitOrTimerCallback, Object state, bool compressStack, ref StackCrawlMark stackMark)
        {
            Contract.Assert(waitOrTimerCallback != null, "Null callback passed to _ThreadPoolWaitOrTimerCallback!");

            _waitOrTimerCallback = waitOrTimerCallback;
            _state = state;

            if (compressStack && !ExecutionContext.IsFlowSuppressed())
            {
                // capture the exection context
                _executionContext = ExecutionContext.FastCapture();
            }
        }
            
        // call back helper
        [System.Security.SecurityCritical]  // auto-generated
        static internal void PerformWaitOrTimerCallback(Object state, bool timedOut)
        {
            ThreadPool.EnsureVMInitialized();

            _ThreadPoolWaitOrTimerCallback helper = (_ThreadPoolWaitOrTimerCallback)state; 
            Contract.Assert(helper != null, "Null state passed to PerformWaitOrTimerCallback!");
            // call directly if it is an unsafe call OR EC flow is suppressed
            if (helper._executionContext == null)
            {
                WaitOrTimerCallback callback = helper._waitOrTimerCallback;
                callback(helper._state, timedOut);
            }
            else
            {
                using (ExecutionContext executionContext = helper._executionContext.CreateCopy())
                {
                    if (timedOut)
                    {
                        ExecutionContext.Run(executionContext, _ccbt, helper);
                    }
                    else
                    {
                        ExecutionContext.Run(executionContext, _ccbf, helper);
                    }
                }
            }
        }    

    }

    [System.Security.SecurityCritical]
    [CLSCompliant(false)]
    [System.Runtime.InteropServices.ComVisible(true)]
    unsafe public delegate void IOCompletionCallback(uint errorCode, // Error code
                                       uint numBytes, // No. of bytes transferred 
                                       NativeOverlapped* pOVERLAP // ptr to OVERLAP structure
                                       );   

    [HostProtection(Synchronization=true, ExternalThreading=true)]
    public static class ThreadPool
    {

        #if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
        #else
        [System.Security.SecuritySafeCritical]
        #endif
#pragma warning disable 618
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
#pragma warning restore 618
        public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
        {
            return SetMaxThreadsNative(workerThreads, completionPortThreads);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
        {
            GetMaxThreadsNative(out workerThreads, out completionPortThreads);
        }

        #if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
        #else
        [System.Security.SecuritySafeCritical]
        #endif
#pragma warning disable 618
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
#pragma warning restore 618
        public static bool SetMinThreads(int workerThreads, int completionPortThreads)
        {
            return SetMinThreadsNative(workerThreads, completionPortThreads);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void GetMinThreads(out int workerThreads, out int completionPortThreads)
        {
            GetMinThreadsNative(out workerThreads, out completionPortThreads);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
        {
            GetAvailableThreadsNative(out workerThreads, out completionPortThreads);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable            
        public static RegisteredWaitHandle RegisterWaitForSingleObject(  // throws RegisterWaitException
             WaitHandle             waitObject,
             WaitOrTimerCallback    callBack,
             Object                 state,
             uint               millisecondsTimeOutInterval,
             bool               executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,true);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [CLSCompliant(false)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(  // throws RegisterWaitException
             WaitHandle             waitObject,
             WaitOrTimerCallback    callBack,
             Object                 state,
             uint               millisecondsTimeOutInterval,
             bool               executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,false);
        }


        [System.Security.SecurityCritical]  // auto-generated
        private static RegisteredWaitHandle RegisterWaitForSingleObject(  // throws RegisterWaitException
             WaitHandle             waitObject,
             WaitOrTimerCallback    callBack,
             Object                 state,
             uint               millisecondsTimeOutInterval,
             bool               executeOnlyOnce,   // NOTE: we do not allow other options that allow the callback to be queued as an APC
             ref StackCrawlMark stackMark,
             bool               compressStack
             )
        {
#if FEATURE_REMOTING
            if (RemotingServices.IsTransparentProxy(waitObject))
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
            Contract.EndContractBlock();
#endif            

            RegisteredWaitHandle registeredWaitHandle = new RegisteredWaitHandle();

            if (callBack != null)
            {
                _ThreadPoolWaitOrTimerCallback callBackHelper = new _ThreadPoolWaitOrTimerCallback(callBack, state, compressStack, ref stackMark);
                state = (Object)callBackHelper;
                // call SetWaitObject before native call so that waitObject won't be closed before threadpoolmgr registration
                // this could occur if callback were to fire before SetWaitObject does its addref
                registeredWaitHandle.SetWaitObject(waitObject);
                IntPtr nativeRegisteredWaitHandle = RegisterWaitForSingleObjectNative(waitObject,
                                                                               state, 
                                                                               millisecondsTimeOutInterval,
                                                                               executeOnlyOnce,
                                                                               registeredWaitHandle,
                                                                               ref stackMark,
                                                                               compressStack);
                registeredWaitHandle.SetHandle(nativeRegisteredWaitHandle);
            }
            else
            {
                throw new ArgumentNullException("WaitOrTimerCallback");
            }
            return registeredWaitHandle;
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle RegisterWaitForSingleObject(  // throws RegisterWaitException
             WaitHandle             waitObject,
             WaitOrTimerCallback    callBack,
             Object                 state,
             int                    millisecondsTimeOutInterval,
             bool               executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            Contract.EndContractBlock();
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,true);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable            
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(  // throws RegisterWaitException
             WaitHandle             waitObject,
             WaitOrTimerCallback    callBack,
             Object                 state,
             int                    millisecondsTimeOutInterval,
             bool               executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            Contract.EndContractBlock();
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,false);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle RegisterWaitForSingleObject(  // throws RegisterWaitException
            WaitHandle          waitObject,
            WaitOrTimerCallback callBack,
            Object                  state,
            long                    millisecondsTimeOutInterval,
            bool                executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
        )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            Contract.EndContractBlock();
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,true);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(  // throws RegisterWaitException
            WaitHandle          waitObject,
            WaitOrTimerCallback callBack,
            Object                  state,
            long                    millisecondsTimeOutInterval,
            bool                executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
        )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            Contract.EndContractBlock();
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)millisecondsTimeOutInterval,executeOnlyOnce,ref stackMark,false);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle RegisterWaitForSingleObject(
                          WaitHandle            waitObject,
                          WaitOrTimerCallback   callBack,
                          Object                state,
                          TimeSpan              timeout,
                          bool                  executeOnlyOnce
                          )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if (tm < -1)
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            if (tm > (long) Int32.MaxValue)
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)tm,executeOnlyOnce,ref stackMark,true);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(
                          WaitHandle            waitObject,
                          WaitOrTimerCallback   callBack,
                          Object                state,
                          TimeSpan              timeout,
                          bool                  executeOnlyOnce
                          )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if (tm < -1)
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            if (tm > (long) Int32.MaxValue)
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject,callBack,state,(UInt32)tm,executeOnlyOnce,ref stackMark,false);
        }
            
        [System.Security.SecuritySafeCritical]  // auto-generated  
        public static bool QueueUserWorkItem(           
             WaitCallback callBack,     // NOTE: we do not expose options that allow the callback to be queued as an APC
             Object state)
        {
            if (callBack == null) return ThrowWaitCallbackNullException();
            if (state == null) return QueueUserWorkItem(callBack);

            //The VM is responsible for the actual growing/shrinking of threads. 
            EnsureVMInitialized();

            // If we are able to create the workitem, we need to get it in the queue without being interrupted by a ThreadAbortException.
            try { }
            finally
            {
                ExecutionContext context = !ExecutionContext.IsFlowSuppressed() ? ExecutionContext.FastCapture() : null;

                IThreadPoolWorkItem tpcallBack = context == ExecutionContext.PreAllocatedDefault ?
                                        new QueueUserWorkItemCallbackDefaultContext(callBack, state) :
                                    (context == null ? (IThreadPoolWorkItem)
                                        new QueueUserWorkItemCallbackNoContext(callBack, state) :
                                        new QueueUserWorkItemCallback(callBack, state, context));

                //ThreadPool has per-appdomain managed queue of work-items. The VM is
                //responsible for just scheduling threads into appdomains. After that
                //work-items are dispatched from the managed queue.
                ThreadPoolGlobals.workQueue.EnqueueGlobal(tpcallBack);
            }
            return true;
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static bool QueueUserWorkItem(           
             WaitCallback callBack)     // NOTE: we do not expose options that allow the callback to be queued as an APC
        {
            if (callBack == null) return ThrowWaitCallbackNullException();

            //The VM is responsible for the actual growing/shrinking of threads. 
            EnsureVMInitialized();

            // If we are able to create the workitem, we need to get it in the queue without being interrupted by a ThreadAbortException.
            try { }
            finally
            {
                ExecutionContext context = !ExecutionContext.IsFlowSuppressed() ? ExecutionContext.FastCapture() : null;

                IThreadPoolWorkItem tpcallBack = context == ExecutionContext.PreAllocatedDefault ?
                                        new QueueUserWorkItemCallbackDefaultContextNoState(callBack) :
                                    (context == null ? (IThreadPoolWorkItem)
                                        new QueueUserWorkItemCallbackNoContextNoState(callBack) :
                                        new QueueUserWorkItemCallbackNoState(callBack, context));

                //ThreadPool has per-appdomain managed queue of work-items. The VM is
                //responsible for just scheduling threads into appdomains. After that
                //work-items are dispatched from the managed queue.
                ThreadPoolGlobals.workQueue.EnqueueGlobal(tpcallBack);
            }
            return true;
        }
    
        [System.Security.SecurityCritical]  // auto-generated_required
        public static bool UnsafeQueueUserWorkItem(
             WaitCallback callBack,     // NOTE: we do not expose options that allow the callback to be queued as an APC
             Object state)
        {
            if (callBack == null) return ThrowWaitCallbackNullException();

            //The VM is responsible for the actual growing/shrinking of threads. 
            EnsureVMInitialized();

            // If we are able to create the workitem, we need to get it in the queue without being interrupted by a ThreadAbortException.
            try { }
            finally
            {
                //ThreadPool has per-appdomain managed queue of work-items. The VM is
                //responsible for just scheduling threads into appdomains. After that
                //work-items are dispatched from the managed queue.
                ThreadPoolGlobals.workQueue.EnqueueGlobal(new QueueUserWorkItemCallbackNoContext(callBack, state));
            }
            return true;
        }

        private static bool ThrowWaitCallbackNullException()
        {
            throw new ArgumentNullException("WaitCallback");
        }

        [SecurityCritical]
        internal static void UnsafeQueueCustomWorkItem(IThreadPoolWorkItem workItem, bool forceGlobal)
        {
            Contract.Assert(null != workItem);
            EnsureVMInitialized();

            //
            // Enqueue needs to be protected from ThreadAbort
            //
            try { }
            finally
            {
                ThreadPoolGlobals.workQueue.Enqueue(workItem, forceGlobal);
            }
        }

        // This method tries to take the target callback out of the current thread's queue.
        [SecurityCritical]
        internal static bool TryPopCustomWorkItem(IThreadPoolWorkItem workItem)
        {
            Contract.Assert(null != workItem);
            if (!ThreadPoolGlobals.vmTpInitialized)
                return false; //Not initialized, so there's no way this workitem was ever queued.
            return ThreadPoolGlobals.workQueue.LocalFindAndPop(workItem);
        }

        // Get all workitems.  Called by TaskScheduler in its debugger hooks.
        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(ThreadPoolWorkQueue.allThreadQueues.Current.Data, ThreadPoolGlobals.workQueue.queueTail);
        }

        internal static IEnumerable<IThreadPoolWorkItem> EnumerateQueuedWorkItems(ThreadPoolWorkQueue.WorkStealingQueue[] wsQueues, ThreadPoolWorkQueue.QueueSegment globalQueueTail)
        {
            if (wsQueues != null)
            {
                // First, enumerate all workitems in thread-local queues.
                foreach (ThreadPoolWorkQueue.WorkStealingQueue wsq in wsQueues)
                {
                    if (wsq != null && wsq.m_array != null)
                    {
                        IThreadPoolWorkItem[] items = wsq.m_array;
                        for (int i = 0; i < items.Length; i++)
                        {
                            IThreadPoolWorkItem item = items[i];
                            if (item != null)
                                yield return item;
                        }
                    }
                }
            }

            if (globalQueueTail != null)
            {
                // Now the global queue
                for (ThreadPoolWorkQueue.QueueSegment segment = globalQueueTail;
                    segment != null;
                    segment = segment.Next)
                {
                    IThreadPoolWorkItem[] items = segment.nodes;
                    for (int i = 0; i < items.Length; i++)
                    {
                        IThreadPoolWorkItem item = items[i];
                        if (item != null)
                            yield return item;
                    }
                }
            }
        }

        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetLocallyQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(new ThreadPoolWorkQueue.WorkStealingQueue[] { Thread.CurrentThread.ThreadTaskLocals.LocalQueues?.workStealingQueue }, null);
        }

        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetGloballyQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(null, ThreadPoolGlobals.workQueue.queueTail);
        }

        private static object[] ToObjectArray(IEnumerable<IThreadPoolWorkItem> workitems)
        {
            int i = 0;
            foreach (IThreadPoolWorkItem item in workitems)
            {
                i++;
            }

            object[] result = new object[i];
            i = 0;
            foreach (IThreadPoolWorkItem item in workitems)
            {
                if (i < result.Length) //just in case someone calls us while the queues are in motion
                    result[i] = item;
                i++;
            }

            return result;
        }

        // This is the method the debugger will actually call, if it ends up calling
        // into ThreadPool directly.  Tests can use this to simulate a debugger, as well.
        [SecurityCritical]
        internal static object[] GetQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetQueuedWorkItems());
        }

        [SecurityCritical]
        internal static object[] GetGloballyQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetGloballyQueuedWorkItems());
        }

        [SecurityCritical]
        internal static object[] GetLocallyQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetLocallyQueuedWorkItems());
        }

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern bool RequestWorkerThread();

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        unsafe private static extern bool PostQueuedCompletionStatus(NativeOverlapped* overlapped);

        [System.Security.SecurityCritical]  // auto-generated_required
        [CLSCompliant(false)]
        unsafe public static bool UnsafeQueueNativeOverlapped(NativeOverlapped* overlapped)
        {
            return PostQueuedCompletionStatus(overlapped);
        }

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureVMInitialized()
        {
            if (!ThreadPoolGlobals.vmTpInitialized)
            {
                //The thread pool maintains a per-appdomain managed work queue.
                //New thread pool entries are added in the managed queue.
                //The VM is responsible for the actual growing/shrinking of 
                //threads. 

                InitalizeVM();
            }
        }

        [SecurityCritical]
        private static void InitalizeVM()
        {
            ThreadPool.InitializeVMTp(ref ThreadPoolGlobals.enableWorkerTracking);
            ThreadPoolGlobals.Initialize();
            QueueUserWorkItemCallback.Initialize();
            QueueUserWorkItemCallbackNoState.Initialize();
            QueueUserWorkItemCallbackDefaultContext.Initialize();
            QueueUserWorkItemCallbackDefaultContextNoState.Initialize();
            _ThreadPoolWaitOrTimerCallback.Initialize();
            ThreadPoolGlobals.vmTpInitialized = true;
        }

        // Native methods: 

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool SetMinThreadsNative(int workerThreads, int completionPortThreads);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool SetMaxThreadsNative(int workerThreads, int completionPortThreads);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void GetMinThreadsNative(out int workerThreads, out int completionPortThreads);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void GetMaxThreadsNative(out int workerThreads, out int completionPortThreads);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void GetAvailableThreadsNative(out int workerThreads, out int completionPortThreads);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool NotifyWorkItemComplete();

        [System.Security.SecurityCritical]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ReportThreadStatus(bool isWorking);

        [System.Security.SecuritySafeCritical]
        internal static void NotifyWorkItemProgress()
        {
            if (!ThreadPoolGlobals.vmTpInitialized)
            {
                EnsureVMInitialized();
            }
            NotifyWorkItemProgressNative();
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void NotifyWorkItemProgressNative();

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool IsThreadPoolHosted();

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void InitializeVMTp(ref bool enableWorkerTracking);

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern IntPtr RegisterWaitForSingleObjectNative(  
             WaitHandle             waitHandle,
             Object                 state,
             uint                   timeOutInterval,
             bool                   executeOnlyOnce,
             RegisteredWaitHandle   registeredWaitHandle,
             ref StackCrawlMark     stackMark,
             bool                   compressStack   
             );

#if !FEATURE_CORECLR
        [System.Security.SecuritySafeCritical]  // auto-generated
        [Obsolete("ThreadPool.BindHandle(IntPtr) has been deprecated.  Please use ThreadPool.BindHandle(SafeHandle) instead.", false)]
        [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool BindHandle(
             IntPtr osHandle
             )
        {
            return BindIOCompletionCallbackNative(osHandle);
        }
#endif

        #if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
        #else
        [System.Security.SecuritySafeCritical]
        #endif        
#pragma warning disable 618
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
#pragma warning restore 618
        public static bool BindHandle(SafeHandle osHandle)
        {
            if (osHandle == null)
                throw new ArgumentNullException("osHandle");
            
            bool ret = false;
            bool mustReleaseSafeHandle = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                osHandle.DangerousAddRef(ref mustReleaseSafeHandle);
                ret = BindIOCompletionCallbackNative(osHandle.DangerousGetHandle());
            }
            finally {
                if (mustReleaseSafeHandle)
                    osHandle.DangerousRelease();
            }
            return ret;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern bool BindIOCompletionCallbackNative(IntPtr fileHandle);
    }
}
