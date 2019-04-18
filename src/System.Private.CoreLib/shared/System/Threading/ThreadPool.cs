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

using Internal.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Threading
{
    internal static class ThreadPoolGlobals
    {
        public static readonly int outstandingRequestLimit = Environment.ProcessorCount;

        public static volatile bool threadPoolInitialized;
        public static bool enableWorkerTracking;

        public static readonly ThreadPoolWorkQueue workQueue = new ThreadPoolWorkQueue();

        /// <summary>Shim used to invoke <see cref="IAsyncStateMachineBox.MoveNext"/> of the supplied <see cref="IAsyncStateMachineBox"/>.</summary>
        internal static readonly Action<object?> s_invokeAsyncStateMachineBox = state =>
        {
            if (!(state is IAsyncStateMachineBox box))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.state);
                return;
            }

            box.MoveNext();
        };
    }

    [StructLayout(LayoutKind.Sequential)] // enforce layout so that padding reduces false sharing
    internal sealed class ThreadPoolWorkQueue
    {
        internal class WorkQueueBase
        {
            // This implementation provides an unbounded, multi-producer multi-consumer queue
            // that supports the standard Enqueue/Dequeue operations.
            // It is composed of a linked list of bounded ring buffers, each of which has an enqueue
            // and a dequeue index, isolated from each other to minimize false sharing.  As long as
            // the number of elements in the queue remains less than the size of the current
            // buffer (Segment), no additional allocations are required for enqueued items.  When
            // the number of items exceeds the size of the current segment, the current segment is
            // "frozen" to prevent further enqueues, and a new segment is linked from it and set
            // as the new tail segment for subsequent enqueues.  As old segments are consumed by
            // dequeues, the dequeue reference is updated to point to the segment that dequeuers should
            // try next.

            /// <summary>
            /// Initial length of the segments used in the queue.
            /// </summary>
            internal const int InitialSegmentLength = 32;

            /// <summary>
            /// Maximum length of the segments used in the queue.  This is a somewhat arbitrary limit:
            /// larger means that as long as we don't exceed the size, we avoid allocating more segments,
            /// but if we do exceed it, then the segment becomes garbage.
            /// </summary>
            internal const int MaxSegmentLength = 1024 * 1024;

            /// <summary>
            /// Lock used to protect cross-segment operations"/>
            /// and any operations that need to get a consistent view of them.
            /// </summary>
            internal object _addSegmentLock = new object();

            [StructLayout(LayoutKind.Explicit, Size = 3 * Internal.PaddingHelpers.CACHE_LINE_SIZE)] // padding before/between/after fields
            internal struct PaddedQueueEnds
            {
                [FieldOffset(1 * Internal.PaddingHelpers.CACHE_LINE_SIZE)] public int Dequeue;
                [FieldOffset(2 * Internal.PaddingHelpers.CACHE_LINE_SIZE)] public int Enqueue;
            }

            internal class QueueSegmentBase
            {
                // Segment design is inspired by the algorithm outlined at:
                // http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue

                /// <summary>The array of items in this queue.  Each slot contains the item in that slot and its "sequence number".</summary>
                internal readonly Slot[] _slots;

                /// <summary>Mask for quickly accessing a position within the queue's array.</summary>
                internal readonly int _slotsMask;

                /// <summary>The queue end positions, with padding to help avoid false sharing contention.</summary>
                internal PaddedQueueEnds _queueEnds; // mutable struct: do not make this readonly

                /// <summary>Indicates whether the segment has been marked such that no additional items may be enqueued.</summary>
                internal bool _frozenForEnqueues;

                internal const int Empty = 0;
                internal const int Full = 1;

                /// <summary>Creates the segment.</summary>
                /// <param name="length">
                /// The maximum number of elements the segment can contain.  Must be a power of 2.
                /// </param>
                internal QueueSegmentBase(int length)
                {
                    // Validate the length
                    Debug.Assert(length >= 2, $"Must be >= 2, got {length}");
                    Debug.Assert((length & (length - 1)) == 0, $"Must be a power of 2, got {length}");

                    // Initialize the slots and the mask.  The mask is used as a way of quickly doing "% _slots.Length",
                    // instead letting us do "& _slotsMask".
                    var slots = new Slot[length];
                    _slotsMask = length - 1;

                    // Initialize the sequence number for each slot.  The sequence number provides a ticket that
                    // allows dequeuers to know whether they can dequeue and enqueuers to know whether they can
                    // enqueue.  An enqueuer at position N can enqueue when the sequence number is N, and a dequeuer
                    // for position N can dequeue when the sequence number is N + 1.  When an enqueuer is done writing
                    // at position N, it sets the sequence number to N + 1 so that a dequeuer will be able to dequeue,
                    // and when a dequeuer is done dequeueing at position N, it sets the sequence number to N + _slots.Length,
                    // so that when an enqueuer loops around the slots, it'll find that the sequence number at
                    // position N is N.  This also means that when an enqueuer finds that at position N the sequence
                    // number is < N, there is still a value in that slot, i.e. the segment is full, and when a
                    // dequeuer finds that the value in a slot is < N + 1, there is nothing currently available to
                    // dequeue. (It is possible for multiple enqueuers to enqueue concurrently, writing into
                    // subsequent slots, and to have the first enqueuer take longer, so that the slots for 1, 2, 3, etc.
                    // may have values, but the 0th slot may still be being filled... in that case, TryDequeue will
                    // return false.)
                    for (int i = 0; i < slots.Length; i++)
                    {
                        slots[i].SequenceNumber = i;
                    }

                    _slots = slots;
                }

                /// <summary>Represents a slot in the queue.</summary>
                [DebuggerDisplay("Item = {Item}, SequenceNumber = {SequenceNumber}")]
                [StructLayout(LayoutKind.Auto)]
                internal struct Slot
                {
                    /// <summary>The item.</summary>
                    internal object? Item;
                    /// <summary>The sequence number for this slot, used to synchronize between enqueuers and dequeuers.</summary>
                    internal int SequenceNumber;
                }

                internal ref Slot this[int i]
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return ref Unsafe.Add(ref Unsafe.As<byte, Slot>(ref _slots.GetRawSzArrayData()), i & _slotsMask);
                    }
                }

                /// <summary>Gets the "freeze offset" for this segment.</summary>
                internal int FreezeOffset => _slots.Length * 2;
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        internal sealed class GlobalQueue : WorkQueueBase
        {
            /// <summary>The current enqueue segment.</summary>
            internal GlobalQueueSegment _enqSegment;
            /// <summary>The current dequeue segment.</summary>
            internal GlobalQueueSegment _deqSegment;

            /// <summary>
            /// Initializes a new instance of the <see cref="GlobalQueue"/> class.
            /// </summary>
            internal GlobalQueue()
            {
                _enqSegment = _deqSegment = new GlobalQueueSegment(InitialSegmentLength);
            }

            // for debugging
            internal int Count
            {
                get
                {
                    int count = 0;
                    for (GlobalQueueSegment? s = _deqSegment; s != null; s = s._nextSegment)
                    {
                        count += s.Count;
                    }
                    return count;
                }
            }

            /// <summary>
            /// Adds an object to the top of the queue
            /// </summary>
            internal void Enqueue(object item, LocalQueue lQueue)
            {
                // try enqueuing. Should normally succeed unless we need a new segment.
                if (!_enqSegment.TryEnqueue(item, lQueue))
                {
                    // If we're unable to enque, this segment will never take enqueues again.
                    // we need to take a slow path that will try adding a new segment.
                    EnqueueSlow(item, lQueue);
                }
            }

            /// <summary>
            /// Slow path for enqueue, adding a new segment if necessary.
            /// </summary>
            private void EnqueueSlow(object item, LocalQueue lQueue)
            {
                for (; ; )
                {
                    GlobalQueueSegment currentSegment = _enqSegment;
                    if (currentSegment.TryEnqueue(item, lQueue))
                    {
                        return;
                    }

                    // take the lock to add a new segment
                    // we can make this optimistically lock free, but it is a rare code path
                    // and we do not want stampeding enqueuers allocating a lot of new segments when only one will win.
                    lock (_addSegmentLock)
                    {
                        if (currentSegment == _enqSegment)
                        {
                            // Make sure that no more items could be added to the current segment.
                            // NB: there may be some strugglers still finishing up out-of-order enqueues
                            //     TryDequeue knows how to deal with that.
                            currentSegment.EnsureFrozenForEnqueues();

                            // We determine the new segment's length based on the old length.
                            // In general, we double the size of the segment, to make it less likely
                            // that we'll need to grow again.
                            int nextSize = Math.Min(currentSegment._slots.Length * 2, MaxSegmentLength);
                            var newEnq = new GlobalQueueSegment(nextSize);

                            // Hook up the new enqueue segment.
                            currentSegment._nextSegment = newEnq;
                            _enqSegment = newEnq;
                        }
                    }
                }
            }

            /// <summary>
            /// Removes an object at the bottom of the queue
            /// Returns null if the queue is empty.
            /// </summary>
            internal object? Dequeue(LocalQueue lQueue)
            {
                var currentSegment = _deqSegment;
                if (currentSegment.IsEmpty)
                {
                    return null;
                }

                object? result = currentSegment.TryDequeue(lQueue);

                if (result == null && currentSegment._nextSegment != null)
                {
                    // slow path that fixes up segments
                    result = TryDequeueSlow(currentSegment, lQueue);
                }

                return result;
            }

            /// <summary>
            /// Slow path for Dequeue, removing frozen segments as needed.
            /// </summary>
            private object? TryDequeueSlow(GlobalQueueSegment currentSegment, LocalQueue lQueue)
            {
                object? result;
                for (; ; )
                {
                    // At this point we know that this segment has been frozen for additional enqueues. But between
                    // the time that we ran TryDequeue and checked for a next segment,
                    // another item could have been added.  Try to dequeue one more time
                    // to confirm that the segment is indeed empty.
                    Debug.Assert(currentSegment._nextSegment != null);
                    result = currentSegment.TryDequeue(lQueue);
                    if (result != null)
                    {
                        return result;
                    }

                    // Current segment is frozen (nothing more can be added) and empty (nothing is in it).
                    // Update _deqSegment to point to the next segment in the list, assuming no one's beat us to it.
                    if (currentSegment == _deqSegment)
                    {
                        Interlocked.CompareExchange(ref _deqSegment, currentSegment._nextSegment, currentSegment);
                    }

                    currentSegment = _deqSegment;

                    // Try to take.  If we're successful, we're done.
                    result = currentSegment.TryDequeue(lQueue);
                    if (result != null)
                    {
                        return result;
                    }

                    // Check to see whether this segment is the last. If it is, we can consider
                    // this to be a moment-in-time when the queue is empty.
                    if (currentSegment._nextSegment == null)
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Provides a multi-producer, multi-consumer thread-safe bounded segment.  When the queue is full,
            /// enqueues fail and return false.  When the queue is empty, dequeues fail and return null.
            /// These segments are linked together to form the unbounded queue.
            ///
            /// The "global" flavor of the queue does not support Pop or Remove and that allows for some simplifications.
            /// </summary>
            [DebuggerDisplay("Count = {Count}")]
            internal sealed class GlobalQueueSegment : QueueSegmentBase
            {
                /// <summary>The segment following this one in the queue, or null if this segment is the last in the queue.</summary>
                internal GlobalQueueSegment? _nextSegment;

                /// <summary>Creates the segment.</summary>
                /// <param name="length">
                /// The maximum number of elements the segment can contain.  Must be a power of 2.
                /// </param>
                internal GlobalQueueSegment(int length) : base(length) { }

                // for debugging
                internal int Count => _queueEnds.Enqueue - _queueEnds.Dequeue;

                private static int sMaxReadCollisionDelay = Thread.OptimalMaxSpinWaitsPerSpinIteration * Environment.ProcessorCount / 2;

                internal bool IsEmpty
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        // Read Deq and then Enq. If not the same, there could be work for a dequeuer.
                        // Order of reads is unimportant here since if there is work we are responsible for, we must see it.
                        //
                        // NB: Frozen segments have artificially increased Enqueue and will appear as having work even when there are no items.
                        //     And they indeed require work - at very least to retire them.
                        return _queueEnds.Dequeue == _queueEnds.Enqueue;
                    }
                }

                /// <summary>Tries to dequeue an element from the queue.</summary>
                internal object? TryDequeue(LocalQueue lQueue)
                {
                    // Loop in case of contention...
                    var spinner = new SpinWait();
                    int delay = lQueue._coreContext.LastGlobalDequeueDelay >> 1;

                    for (; ; )
                    {
                        int position = _queueEnds.Dequeue;
                        ref Slot slot = ref this[position];

                        // Read the sequence number for the slot.
                        // Should read before reading Item, but we read Item after CAS, so ordinary read is ok.
                        int sequenceNumber = slot.SequenceNumber;

                        // Check if the slot is considered Full in the current generation.
                        if (sequenceNumber == position + Full)
                        {
                            // Attempt to acquire the slot for Dequeuing.
                            if (Interlocked.CompareExchange(ref _queueEnds.Dequeue, position + 1, position) == position)
                            {
                                var item = slot.Item;
                                slot.Item = null;

                                // make the slot appear empty in the next generation
                                Volatile.Write(ref slot.SequenceNumber, position + 1 + _slotsMask);
                                return item;
                            }
                        }
                        else if (sequenceNumber - position < Full)
                        {
                            // The sequence number was less than what we needed, which means we cannot return this item.
                            // Check if we have reached Enqueue and return null indicating the segment is in empty state.
                            // NB: reading stale _frozenForEnqueues is fine - we would just spin once more
                            var currentEnqueue = Volatile.Read(ref _queueEnds.Enqueue);
                            if (currentEnqueue == position || (_frozenForEnqueues && currentEnqueue == position + FreezeOffset))
                            {
                                return null;
                            }

                            // The enqueuer went ahead and took a slot, but it has not finished filling the value.
                            // We cannot return `null` since the segment is not empty, so we must retry.
                            // We should be careful though to not starve the enqueuer that we are waiting for.
                            // In a worst case it may have lower priority than us, so we have to sleep(1) at some point.
                            // Let the SpinWait handle that.
                            spinner.SpinOnce();
                            continue;
                        }

                        // We have a stale dequeue value. Another dequeuer was quicker than us.
                        // We should retry with a new dequeue and do not need to wait for anyone.
                        // However there could be many dequeuers trying the same and only one at a time makes progress.
                        // Trying and failing has indirect costs as it keeps memory busy, heats up CPU, etc..
                        // We will throttle the rate of polling on our side exponentially with every clash.
                        //
                        // Also since there is a cost of "learning" a good delay value, we will record the last one that worked
                        // and use 1/2 as a starting point when we are again in a similar situation.
                        //
                        // For more background about spinning see:
                        // [The Performance of Spin Lock Alternatives for Shared-Memory Multiprocessors] (https://homes.cs.washington.edu/~tom/pubs/spinlock.pdf)
                        //
                        lQueue._coreContext.LastGlobalDequeueDelay = delay;
                        Thread.SpinWait(lQueue.NextRnd() & delay);

                        if (delay < sMaxReadCollisionDelay)
                        {
                            delay = (delay << 1) + 1;
                        }
                    }
                }

                /// <summary>
                /// Attempts to enqueue the item.  If successful, the item will be stored
                /// in the queue and true will be returned; otherwise, the item won't be stored, the segment will be frozen
                /// and false will be returned.
                /// </summary>
                public bool TryEnqueue(object item, LocalQueue lQueue)
                {
                    // Loop in case of contention...
                    int delay = lQueue._coreContext.LastGlobalEnqueueDelay >> 1;
                    for (; ; )
                    {
                        int position = _queueEnds.Enqueue;
                        ref Slot slot = ref this[position];

                        // Read the sequence number for the enqueue position.
                        // Should read before writing Item, but our write is after CAS, so ordinary read is ok.
                        int sequenceNumber = slot.SequenceNumber;

                        // The slot is empty and ready for us to enqueue into it if its sequence number matches the slot.
                        if (sequenceNumber == position)
                        {
                            // Reserve the slot for Enqueuing.
                            if (Interlocked.CompareExchange(ref _queueEnds.Enqueue, position + 1, position) == position)
                            {
                                slot.Item = item;
                                Volatile.Write(ref slot.SequenceNumber, position + Full);
                                return true;
                            }
                        }
                        else if (sequenceNumber - position < 0)
                        {
                            // The sequence number was less than what we needed, which means we have caught up with previous generation
                            // Technically it's possible that we have dequeuers in progress and spaces are or about to be available.
                            // We still would be better off with a new segment.
                            return false;
                        }

                        // Lost a race to another enqueue. Need to retry, but throttle the rate of retries if keep failing.
                        // See more detailed coments on the same pattern in TryDequeue.
                        lQueue._coreContext.LastGlobalEnqueueDelay = delay;
                        Thread.SpinWait(lQueue.NextRnd() & delay);

                        if (delay < sMaxReadCollisionDelay)
                        {
                            delay = (delay << 1) + 1;
                        }
                    }
                }

                internal void EnsureFrozenForEnqueues()
                {
                    // flag used to ensure we don't increase the enqueue more than once
                    if (!_frozenForEnqueues)
                    {
                        // Increase the enqueue by FreezeOffset atomically.
                        // enqueuing will be impossible after that
                        // dequeuers would need to dequeue 2 generations to catch up, and they can't
                        Interlocked.Add(ref _queueEnds.Enqueue, FreezeOffset);
                        _frozenForEnqueues = true;
                    }
                }
            }
        }

        /// <summary>
        /// The "local" flavor of the queue is similar to the "global", but also supports Pop, Remove and Rob operations.
        ///
        /// - Pop is used to implement Busy-Leaves scheduling strategy.
        /// - Remove is used when the caller finds it benefitial to execute a workitem "inline" after it has been scheduled.
        ///   (such as waiting on a task completion).
        /// - Rob is used to rebalance queues if one is found to be "rich" by stealing 1/2 of its queue.
        ///   (this way we avoid having to continue stealing from the same single queue, Rob may lead to 2 rich queues, then 4, then ...)
        ///
        /// We create multiple local queues and softly affinitize them with CPU cores.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        internal sealed class LocalQueue : WorkQueueBase
        {
            /// <summary>The current enqueue segment.</summary>
            internal LocalQueueSegment _enqSegment;
            /// <summary>The current dequeue segment.</summary>
            internal LocalQueueSegment _deqSegment;

            // The above fields change infrequently, but accessible from other threads when stealing.
            // The fields below are more dynamic, but most of the use is from the same core thus there is little sharing.
            // separate this set in a padded struct

            [StructLayout(LayoutKind.Explicit, Size = 2 * Internal.PaddingHelpers.CACHE_LINE_SIZE)] // padding before/after fields
            internal struct CoreContext
            {
                [FieldOffset(1 * Internal.PaddingHelpers.CACHE_LINE_SIZE + 0 * sizeof(uint))] internal uint rndVal;
                [FieldOffset(1 * Internal.PaddingHelpers.CACHE_LINE_SIZE + 1 * sizeof(uint))] internal int LastGlobalEnqueueDelay;
                [FieldOffset(1 * Internal.PaddingHelpers.CACHE_LINE_SIZE + 2 * sizeof(uint))] internal int LastGlobalDequeueDelay;
            }

            internal CoreContext _coreContext = new CoreContext() { rndVal = 6247 };

            // Very cheap random sequence generator. We keep one per-local queue.
            // We do not need a lot of randomness, I think even _rnd++ would be fairly good here.
            // Sequences attached to different queues go out of sync quickly and that could be sufficient.
            // However this sequence is a bit more random at a very modest additional cost.
            // [Fast High Quality Parallel Random Number] (http://www.drdobbs.com/tools/fast-high-quality-parallel-random-number/231000484)
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal int NextRnd()
            {
                var r = _coreContext.rndVal;
                r -= Numerics.BitOperations.RotateRight(r, 11);
                return (int)(_coreContext.rndVal = r);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalQueue"/> class.
            /// </summary>
            internal LocalQueue()
            {
                _addSegmentLock = new object();
                _enqSegment = _deqSegment = new LocalQueueSegment(InitialSegmentLength);
            }

            // for debugging
            internal int Count
            {
                get
                {
                    int count = 0;
                    for (LocalQueueSegment? s = _deqSegment; s != null; s = s._nextSegment)
                    {
                        count += s.Count;
                    }
                    return count;
                }
            }

            /// <summary>
            /// Adds an object to the top of the queue
            /// </summary>
            internal void Enqueue(object item)
            {
                // try enqueuing. Should normally succeed unless we need a new segment.
                if (!_enqSegment.TryEnqueue(item))
                {
                    // If we're unable to enque, this segment is full.
                    // we need to take a slow path that will try adding a new segment.
                    EnqueueSlow(item);
                }
            }

            /// <summary>
            /// Slow path for Enqueue, adding a new segment if necessary.
            /// </summary>
            private void EnqueueSlow(object item)
            {
                LocalQueueSegment currentSegment = _enqSegment;
                for (; ; )
                {
                    if (currentSegment.TryEnqueue(item))
                    {
                        return;
                    }
                    currentSegment = EnsureNextSegment(currentSegment);
                }
            }

            private LocalQueueSegment EnsureNextSegment(LocalQueueSegment currentSegment)
            {
                var nextSegment = currentSegment._nextSegment;
                if (nextSegment != null)
                {
                    return nextSegment;
                }

                // take the lock to add a new segment
                // we can make this optimistically lock free, but it is a rare code path
                // and we do not want stampeding enqueuers allocating a lot of new segments when only one will win.
                lock (_addSegmentLock)
                {
                    if (currentSegment._nextSegment == null)
                    {
                        // We determine the new segment's length based on the old length.
                        // In general, we double the size of the segment, to make it less likely
                        // that we'll need to grow again.
                        int nextSize = Math.Min(currentSegment._slots.Length * 2, MaxSegmentLength);
                        var newEnq = new LocalQueueSegment(nextSize);

                        // Hook up the new enqueue segment.
                        currentSegment._nextSegment = newEnq;
                        _enqSegment = newEnq;
                    }
                }

                return currentSegment._nextSegment;
            }

            /// <summary>
            /// Removes an object at the bottom of the queue
            /// Returns null if the queue is empty or if there is a contention
            /// (no point to dwell on one local queue and make problem worse when there are other queues).
            /// </summary>
            internal object? Dequeue(ref bool missedSteal)
            {
                var currentSegment = _deqSegment;
                object? result = currentSegment.TryDequeue(ref missedSteal);

                // if there is a new segment, we must help with retiring the current.
                if (result == null && currentSegment._nextSegment != null)
                {
                    result = TryDequeueSlow(currentSegment, ref missedSteal);
                }

                return result;
            }

            /// <summary>
            /// Tries to dequeue an item, removing frozen segments as needed.
            /// </summary>
            private object? TryDequeueSlow(LocalQueueSegment currentSegment, ref bool missedSteal)
            {
                object? result;
                for (; ; )
                {
                    // At this point we know that this segment has been frozen for additional enqueues. But between
                    // the time that we ran TryDequeue and checked for a next segment,
                    // another item could have been added.  Try to dequeue one more time
                    // to confirm that the segment is indeed empty.
                    Debug.Assert(currentSegment._nextSegment != null);

                    // spin through missed steals, we must know for sure the segment is quiescent and empty
                    bool localMissedSteal;
                    do
                    {
                        localMissedSteal = false;
                        result = currentSegment.TryDequeue(ref localMissedSteal);
                        if (result != null)
                        {
                            return result;
                        }
                    } while (localMissedSteal == true);

                    // Current segment is frozen (nothing more can be added) and empty (nothing is in it).
                    // Update _deqSegment to point to the next segment in the list, assuming no one's beat us to it.
                    if (currentSegment == _deqSegment)
                    {
                        Interlocked.CompareExchange(ref _deqSegment, currentSegment._nextSegment, currentSegment);
                    }

                    currentSegment = _deqSegment;

                    // Try to dequeue.  If we're successful, we're done.
                    result = currentSegment.TryDequeue(ref missedSteal);
                    if (result != null)
                    {
                        return result;
                    }

                    // Check to see whether this segment is the last. If it is, we can consider
                    // this to be a moment-in-time when the queue is empty.
                    if (currentSegment._nextSegment == null)
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Pops an item from the top of the queue.
            /// Returns null if there is nothing to pop or there is a contention.
            /// </summary>
            internal object? TryPop()
            {
                return _enqSegment.TryPop();
            }

            /// <summary>
            /// Performs a limited search for the given item in the queue and removes the item if found.
            /// Returns true if item was indeed removed.
            /// </summary>
            internal bool TryRemove(object callback)
            {
                return _enqSegment.TryRemove(callback);
            }

            /// <summary>
            /// Provides a multi-producer, multi-consumer thread-safe bounded segment.  When the queue is full,
            /// enqueues fail and return false.  When the queue is empty, dequeues fail and return null.
            /// These segments are linked together to form the unbounded queue.
            ///
            /// The main difference of "local" segment from "global" is support for Pop as needed by the Busy Leaves algorithm.
            /// For more details on Busy Leaves see:
            /// [Scheduling Multithreaded Computations by Work Stealing] (http://supertech.csail.mit.edu/papers/steal.pdf)
            ///
            /// Supporting Pop leads to some additional complexity compared to "global" segment.
            /// In particular enqueue index may move both forward and backward and therefore we cannot rely on atomic
            /// update of the enqueue index as a way to acquire access to an Enqueue/Pop slot.
            /// Another issue that complicates things is that all operation (Dequeue, Enqueue, Pop, etc.) may be concurrently
            /// performed and should coordinate access to the slots when necessary.
            ///
            /// We resolve these issues by observing the following rules:
            ///
            /// - In an empty segment dequeue and enqueue ends point to the same slot. This is also the initial state.
            ///
            /// - The enqueue end is never behind the dequeue end.
            ///
            /// - Slots in "Full" state form a contiguous range.
            ///
            /// - Dequeue operation must acquire access to the dequeue slot by atomically moving it to the "Change" state.
            ///   Setting the slot to the next state can be done via regular write. After the Dequeue is complete and dequeue end is moved forward.
            ///
            /// - Pop and Enqueue operation must acquire the access to enqueue slot by atomically moving the previous slot to the "Change" state.
            ///   Setting the next state can be done via regular write. After the Enqueue/Pop is complete and enqueue end is moved appropriately.
            ///
            ///   To summarise:
            ///     In a rare case of concurrent Pop and Enqueue, the threads coordinate the access by locking the same slot.
            ///     Similarly, concurrent Dequeues will coordinate the access on the other end of the segment.
            ///     When the segment shrinks to just 1 element, all Pop/Enqueue/Dequeue operations would end up using the same coordinating slot.
            ///     This indirectly guarantees that concurrent Dequeue and Pop cannot use the same slot and move enqueue/dequeue ends across each other.
            ///
            /// - Rob/Remove operations get exclusive access to appropriate ranges of slots by setting "Change" on both sides of the range.
            ///
            /// - The enqueue and dequeue ends are updated only when corresponding slots are in the "Change" state.
            ///
            /// - It is possible, in a case of a contention, to move a wrong slot to the "Change" state.
            ///   (Ex: the slot is not longer previous to an enqueue slot because enqueue index has moved forward)
            ///   When such situation is possible, it can be detected by re-examining the condition after the slot has moved to "Change" state.
            ///   This kind of contentions are rare and handled by reverting the slot(s) to the original state and performing an appropriate backoff.
            ///   The reverting of failed enqueue slot is done via CAS - in case if the slot has changed the state due to robbing (that new state should win).
            ///   The reverting of failed dequeue slot is an ordinary write. Since dequeue moves monotonically, it cannot end up in a robbing range.
            ///
            /// </summary>
            [DebuggerDisplay("Count = {Count}")]
            internal sealed class LocalQueueSegment : QueueSegmentBase
            {
                /// <summary>The segment following this one in the queue, or null if this segment is the last in the queue.</summary>
                internal LocalQueueSegment? _nextSegment;

                /// <summary>
                /// Another state of the slot in addition to Empty and Full.
                /// "Change" means that the slot is reserved for possible modifications.
                /// The state is used for mutual communication between Enqueue/Dequeue/Pop/Remove.
                /// NB: Enqueue reserves the slot "to the left" of the slot that is targeted by Enqueue.
                ///     This ensures that "Full" slots occupy a contiguous range (not a requirement and is not true for the "global" flavor of the queue)
                /// </summary>
                private const int Change = 2;

                /// <summary>
                /// How far we look through items when asked to remove/deschedule one.
                /// This is a rather arbitrary number to mitigate degenerate cases.
                /// Normally the stretch of full items will not be nearly this long.
                /// On the other hand we can afford to search a bit, since the alternative is blocking and waiting, which is very expensive.
                /// </summary>
                private const int RemoveRange = 1024;

                /// <summary>
                /// When a segment has more than this, we steal half of its slots.
                /// </summary>
                private const int RichCount = 32;

                /// <summary>Creates the segment.</summary>
                /// <param name="length">
                /// The maximum number of elements the segment can contain.  Must be a power of 2.
                /// </param>
                internal LocalQueueSegment(int length) : base(length) { }

                /// <summary>
                /// Attempts to enqueue the item.  If successful, the item will be stored
                /// in the queue and true will be returned; otherwise, the item won't be stored, and false
                /// will be returned.
                /// </summary>
                internal bool TryEnqueue(object item)
                {
                    // Loop in case of contention...
                    int position = _queueEnds.Enqueue;
                    for (; ; )
                    {
                        ref Slot prevSlot = ref this[position - 1];
                        int prevSequenceNumber = prevSlot.SequenceNumber;
                        ref Slot slot = ref this[position];

                        // check if prev slot is empty in the next generation or full
                        // otherwise retry - we have some kind of race, most likely the prev item is being dequeued
                        if (prevSequenceNumber == position + _slotsMask | prevSequenceNumber == position)
                        {
                            // lock the previous slot (so noone could dequeue past us, pop the prev slot or enqueue into the same position)
                            if (Interlocked.CompareExchange(ref prevSlot.SequenceNumber, prevSequenceNumber + Change, prevSequenceNumber) == prevSequenceNumber)
                            {
                                // confirm that enqueue did not change while we were locking the slot
                                // it is extremely rare, but we may see another Pop or Enqueue on the same segment.
                                if (_queueEnds.Enqueue == position)
                                {
                                    // Successfully locked prev slot.
                                    // is the slot empty?   (most common path)
                                    int sequenceNumber = slot.SequenceNumber;
                                    if (sequenceNumber == position)
                                    {
                                        // update Enqueue - must do before marking the slot full.
                                        // otherwise someone could lock the full slot while having stale Enqueue.
                                        _queueEnds.Enqueue = position + 1;
                                        slot.Item = item;

                                        // make the slot appear full in the current generation.
                                        // since the slot on the left is still locked, only poppers/enqueuers can use it, but can use immediately
                                        Volatile.Write(ref slot.SequenceNumber, position + Full);

                                        // unlock prev slot
                                        // must be after we moved enq to the next slot, or someone may pop prev and break continuity of full slots.
                                        prevSlot.SequenceNumber = prevSequenceNumber;
                                        return true;
                                    }

                                    // do we see the prev generation?
                                    if (position - sequenceNumber > 0)
                                    {
                                        // Set Enqueue to throw off anyone else trying to enqueue or pop, unless we have already done that.
                                        // we need a fence between writing to Enqueue and unlocking, but we unlock with a CAS anyways
                                        _queueEnds.Enqueue = position + FreezeOffset;
                                        _frozenForEnqueues = true;
                                    }
                                }

                                // enqueue changed or segment is full (rare cases)
                                // unlock the slot through CAS in case slot was robbed
                                Interlocked.CompareExchange(ref prevSlot.SequenceNumber, prevSequenceNumber, prevSequenceNumber + Change);
                            }
                        }

                        if (_frozenForEnqueues)
                        {
                            return false;
                        }

                        // Lost a race. Most likely to the dequeuer of the last remaining item, which will be gone shortly. Try again.
                        // NOTE: We only need a compiler fence here. As long as we re-read Enqueue, it could be an ordinary read.
                        //       This is not a common code path though, so volatile read will do.
                        position = Volatile.Read(ref _queueEnds.Enqueue);
                    }
                }

                // for debugging
                internal int Count => _queueEnds.Enqueue - _queueEnds.Dequeue;

                internal object? TryPop()
                {
                    for (; ; )
                    {
                        int position = _queueEnds.Enqueue - 1;
                        ref Slot slot = ref this[position];

                        // Read the sequence number for the slot.
                        int sequenceNumber = slot.SequenceNumber;

                        // Check if the slot is considered Full in the current generation (other likely state - Empty).
                        if (sequenceNumber == position + Full)
                        {
                            // lock the slot.
                            if (Interlocked.CompareExchange(ref slot.SequenceNumber, position + Change, sequenceNumber) == sequenceNumber)
                            {
                                // confirm that enqueue did not change while we were locking the slot
                                // it is extremely rare, but we may see another Pop or Enqueue on the same segment.
                                // if (_queueEnds.Enqueue == position + 1)
                                if (_queueEnds.Enqueue == sequenceNumber)
                                {
                                    var item = slot.Item;
                                    slot.Item = null;

                                    // update Enqueue before marking slot empty. - if enqueue update is later than that it may happen after the slot is enqueued.
                                    _queueEnds.Enqueue = position;

                                    // make the slot appear empty in the current generation and update enqueue
                                    // that unlocks the slot
                                    Volatile.Write(ref slot.SequenceNumber, position);

                                    if (item == null)
                                    {
                                        // item was removed
                                        // this is not a lost race though, so continue.
                                        continue;
                                    }

                                    return item;
                                }
                                else
                                {
                                    // enqueue changed, in this rare case we just retry.
                                    // unlock the slot through CAS in case the slot was robbed
                                    Interlocked.CompareExchange(ref slot.SequenceNumber, sequenceNumber, position + Change);
                                    continue;
                                }
                            }
                        }

                        // found no items or encountered a contention (most likely with a dequeuer)
                        return null;
                    }
                }

                /// <summary>
                /// Tries to dequeue an element from the queue.
                ///
                /// "missedSteal" is set to true when we find the segment in a state where we cannot take an element and
                /// cannot claim the segment is empty.
                /// That generally happens when another thread did or is doing modifications and we do not see all the changes.
                /// We could spin here until we see a consistent state, but it makes more sense to look in other queues.
                /// </summary>
                internal object? TryDequeue(ref bool missedSteal)
                {
                    for (; ; )
                    {
                        int position = _queueEnds.Dequeue;

                        // if prev is not empty (in next generation), there might be more work in the segment.
                        // NB: enqueues are initiated by CAS-locking the prev slot.
                        //     it is unkikely, but theoretically possible that we will arrive here and see only that,
                        //     while all other changes are still write-buffered in the other core.
                        //     We cannot claim that the queue is empty, and should treat this as a missed steal
                        //     lest we risk that noone comes for this workitem, ever...
                        //     Also make sure we read the prev slot before reading the actual slot, reading after is pointless.
                        if (!missedSteal)
                        {
                            missedSteal = Volatile.Read(ref this[position - 1].SequenceNumber) != (position + _slotsMask);
                        }

                        // Read the sequence number for the slot.
                        ref Slot slot = ref this[position];
                        int sequenceNumber = slot.SequenceNumber;

                        // Check if the slot is considered Full in the current generation.
                        int diff = sequenceNumber - position;
                        if (diff == Full)
                        {
                            // Reserve the slot for Dequeuing.
                            if (Interlocked.CompareExchange(ref slot.SequenceNumber, position + Change, sequenceNumber) == sequenceNumber)
                            {
                                object? item;
                                var enqPos = _queueEnds.Enqueue;

                                if (enqPos - position < RichCount ||
                                    // take from the rich and give to the needy!!
                                    // NB: "this" is a sentinel for a failed robbing attempt
                                    (item = TryRob(position, enqPos)) == this)
                                {
                                    _queueEnds.Dequeue = position + 1;
                                    item = slot.Item;
                                    slot.Item = null;
                                }

                                // unlock the slot for enqueuing by making the slot empty in the next generation
                                Volatile.Write(ref slot.SequenceNumber, position + 1 + _slotsMask);

                                if (item == null)
                                {
                                    // the item was removed, so we have nothing to return.
                                    // this is not a lost race though, so must continue.
                                    continue;
                                }

                                return item;
                            }
                        }
                        else if (diff == 0)
                        {
                            // reached an empty slot
                            // since full slots are contiguous, finding an empty slot means that
                            // for our purposes and for the moment in time the segment is empty
                            return null;
                        }

                        // contention with other thread
                        // must check this segment again later
                        missedSteal = true;
                        return null;
                    }
                }

                internal bool IsEmpty
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        int position = _queueEnds.Dequeue;
                        int sequenceNumber = this[position].SequenceNumber;

                        // "position == sequenceNumber" means that we have reached an empty slot.
                        // since full slots are contiguous, finding an empty slot means that
                        // for our purposes and for the moment in time the segment is empty
                        return position == sequenceNumber;
                    }
                }

                internal object? TryRob(int deqPosition, int enqPosition)
                {
                    LocalQueueSegment other = ThreadPoolGlobals.workQueue.GetOrAddLocalQueue()._enqSegment;
                    if (this != other)
                    {
                        // same stanza as in TryEnqueue
                        int otherEnqPosition = other._queueEnds.Enqueue;
                        ref Slot enqPrevSlot = ref other[otherEnqPosition - 1];
                        int prevSequenceNumber = enqPrevSlot.SequenceNumber;

                        var srcSlotsMask = _slotsMask;
                        // mask in case the segment is frozen and enqueue is inflated
                        var count = (enqPosition - deqPosition) & srcSlotsMask;
                        int halfPosition = deqPosition + count / 2;
                        ref Slot halfSlot = ref this[halfPosition];

                        // unlike Enqueue, we require prev slot be empty
                        // not just to prevent rich getting richer
                        // we also do not want a possibility that the same segment is both robbed from and robbed to, which would be messy
                        if (prevSequenceNumber == otherEnqPosition + other._slotsMask)
                        {
                            // lock the other segment for enqueuing
                            if (Interlocked.CompareExchange(ref enqPrevSlot.SequenceNumber, prevSequenceNumber + Change, prevSequenceNumber) == prevSequenceNumber)
                            {
                                // confirm that enqueue did not change while we were locking the slot
                                // it is extremely rare, but we may see another Pop or Enqueue on the same segment.
                                if (other._queueEnds.Enqueue == otherEnqPosition)
                                {
                                    // lock halfslot, it must be full
                                    if (Interlocked.CompareExchange(ref halfSlot.SequenceNumber, halfPosition + Change, halfPosition + Full) == halfPosition + Full)
                                    {
                                        // our enqueue could have changed before we locked half
                                        // make sure that half-way slot is still before enqueue
                                        // in fact give it more space - we do not want to rob all the items, especially if someone else popping them fast.
                                        var enq = deqPosition + ((_queueEnds.Enqueue - deqPosition) & _slotsMask);
                                        if (enq - halfPosition > (RichCount / 4))
                                        {
                                            int i = deqPosition, j = otherEnqPosition;
                                            ref Slot last = ref this[i++];

                                            while (true)
                                            {
                                                ref Slot next = ref this[i];
                                                ref Slot to = ref other[j];

                                                // the other slot must be empty
                                                // next slot must be full
                                                if (to.SequenceNumber != j | next.SequenceNumber != i + Full)
                                                {
                                                    break;
                                                }

                                                to.Item = last.Item;
                                                // NB: enables "to" for dequeuing, which may immediately happen,
                                                // but not for popping, yet - since the other enq is locked
                                                Volatile.Write(ref to.SequenceNumber, j + Full);

                                                last.Item = null;

                                                // we are going to take from next, mark it empty already
                                                next.SequenceNumber = i + 1 + srcSlotsMask;
                                                last = ref next;

                                                i++;
                                                j++;
                                            }

                                            // return the last slot value
                                            // (it should already be marked empty, or will be, if it is at deqPosition)
                                            var result = last.Item;
                                            last.Item = null;

                                            // restore the half slot, must be after all the full->empty slot transitioning
                                            // to make sure that poppers cannot see robbed slots as still incorrectly full when moving to the left of half.
                                            Volatile.Write(ref halfSlot.SequenceNumber, halfPosition + Full);

                                            // advance the other enq, must be done before unlocking other prev slot, or someone could pop prev once unlocked.
                                            // enables enq/pop
                                            other._queueEnds.Enqueue = j;

                                            // advance Dequeue, must be after halfSlot is restored - someone could immediately start robbing.
                                            Volatile.Write(ref _queueEnds.Dequeue, i);

                                            // unlock other prev slot
                                            // must be after we moved other enq to the next slot, or someone may pop prev and break continuity of full slots.
                                            enqPrevSlot.SequenceNumber = prevSequenceNumber;
                                            return result;
                                        }

                                        // failed to lock the half-way slot.
                                        // restore via CAS, in case target slot has been robbed to
                                        Interlocked.CompareExchange(ref halfSlot.SequenceNumber, halfPosition + Full, halfPosition + Change);
                                    }
                                }

                                // failed to lock actual enqueue end, restore with CAS, in case target slot has been robbed to/from
                                Interlocked.CompareExchange(ref enqPrevSlot.SequenceNumber, prevSequenceNumber, prevSequenceNumber + Change);
                            }
                        }
                    }

                    // "this" is a sentinel value for a failed robbing attempt
                    return this;
                }

                /// <summary>
                /// Searches for the given callback and removes it.
                /// Returns "true" if actually removed the item.
                /// </summary>
                internal bool TryRemove(object callback)
                {
                    for (int position = _queueEnds.Enqueue - 1, l = position - RemoveRange; position != l; position--)
                    {
                        ref Slot slot = ref this[position];
                        if (slot.Item == callback)
                        {
                            // lock Dequeue (so that the slot would not be robbed while we are removing)
                            var deqPosition = _queueEnds.Dequeue;
                            ref var deqSlot = ref this[deqPosition];
                            if (Interlocked.CompareExchange(ref deqSlot.SequenceNumber, deqPosition + Change, deqPosition + Full) == deqPosition + Full)
                            {
                                // lock the slot.
                                if (Interlocked.CompareExchange(ref slot.SequenceNumber, position + Change, position + Full) == position + Full)
                                {
                                    // Successfully locked the slot.
                                    // check if the item is still there
                                    if (slot.Item == callback)
                                    {
                                        slot.Item = null;

                                        // unlock the slot and Dequeue and return success.
                                        Volatile.Write(ref slot.SequenceNumber, position + Full);
                                        deqSlot.SequenceNumber = deqPosition + Full;
                                        return true;
                                    }

                                    // unlock the slot and exit
                                    slot.SequenceNumber = position + Full;
                                }

                                // unlock Dequeue
                                deqSlot.SequenceNumber = deqPosition + Full;
                            }

                            // lost the item to someone else, will not see it again
                            break;
                        }
                        else if (slot.SequenceNumber - position > Change)
                        {
                            // reached the next gen
                            break;
                        }
                    }
                    return false;
                }
            }
        }

        internal readonly LocalQueue[] _localQueues;
        internal readonly GlobalQueue _globalQueue = new GlobalQueue();

        internal bool loggingEnabled;

        private readonly Internal.PaddingFor32 pad1;
        private int numOutstandingThreadRequests = 0;
        private readonly Internal.PaddingFor32 pad2;

        internal ThreadPoolWorkQueue()
        {
            loggingEnabled = FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.ThreadPool | FrameworkEventSource.Keywords.ThreadTransfer);
            _localQueues = new LocalQueue[RoundUpToPowerOf2(Environment.ProcessorCount)];
        }

        /// <summary>
        /// Round the specified value up to the next power of 2, if it isn't one already.
        /// </summary>
        private static int RoundUpToPowerOf2(int i)
        {
            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --i;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }

        /// <summary>
        /// Returns a local queue softly affinitized with the current thread.
        /// </summary>
        internal LocalQueue? GetLocalQueue()
        {
            return _localQueues[GetLocalQueueIndex()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal LocalQueue GetOrAddLocalQueue()
        {
            var index = GetLocalQueueIndex();
            var result = _localQueues[index];

            if (result == null)
            {
                result = EnsureLocalQueue(index);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private LocalQueue EnsureLocalQueue(int index)
        {
            var newQueue = new LocalQueue();
            Interlocked.CompareExchange(ref _localQueues[index], newQueue, null!);
            return _localQueues[index];
        }

        internal int GetLocalQueueIndex()
        {
            return GetLocalQueueIndex(Threading.Thread.GetCurrentProcessorId());
        }

        internal int GetLocalQueueIndex(int procId)
        {
            return procId & (_localQueues.Length - 1);
        }

        internal void RequestThread()
        {
            //
            // If we have not yet requested #procs threads, then request a new thread.
            //
            int count = numOutstandingThreadRequests;
            while (count < ThreadPoolGlobals.outstandingRequestLimit)
            {
                int prev = Interlocked.CompareExchange(ref numOutstandingThreadRequests, count + 1, count);
                if (prev == count)
                {
                    ThreadPool.RequestWorkerThread();
                    break;
                }

                count = prev;
            }
        }

        internal void EnsureThreadRequested()
        {
            if (numOutstandingThreadRequests == 0 &&
                Interlocked.CompareExchange(ref numOutstandingThreadRequests, 1, 0) == 0)
            {
                ThreadPool.RequestWorkerThread();
            }
        }

        internal void KeepThread()
        {
            //
            // The thread is leaving to VM, but we like the current concurrency level.
            // Don't bother about numOutstandingThreadRequests. Just ask for a thread.
            //
            Interlocked.Increment(ref numOutstandingThreadRequests);
            ThreadPool.RequestWorkerThread();
        }

        internal void MarkThreadRequestSatisfied()
        {
            //
            // One of our outstanding thread requests has been satisfied.
            // Decrement the count so that future calls to RequestThread will succeed.
            //
            // CoreCLR: Note that there is a separate count in the VM which has already been decremented
            // by the VM by the time we reach this point.
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

        public void Enqueue(object callback, bool forceGlobal)
        {
            Debug.Assert((callback is IThreadPoolWorkItem) ^ (callback is Task));

            if (loggingEnabled)
                System.Diagnostics.Tracing.FrameworkEventSource.Log.ThreadPoolEnqueueWorkObject(callback);

            var localQueue = GetOrAddLocalQueue();
            if (forceGlobal)
            {
                _globalQueue.Enqueue(callback, localQueue);
            }
            else
            {
                localQueue.Enqueue(callback);
            }

            // make sure there is at least one worker request
            // as long as there is one worker to come, this item will be noticed.
            // in fact ask anyways, up to #proc
            RequestThread();
        }

        internal bool TryRemove(object callback)
        {
            return GetLocalQueue()?.TryRemove(callback) ?? false;
        }

        public object? PopLocal(int localQueueIndex)
        {
            return _localQueues[localQueueIndex]?.TryPop();
        }

        public object? DequeueAny(ref bool missedSteal, LocalQueue localQueue)
        {
            object? callback = _globalQueue.Dequeue(localQueue);
            if (callback == null)
            {
                LocalQueue[] queues = _localQueues;
                int localQueueIndex = localQueue.NextRnd() & (_localQueues.Length - 1);

                // then traverse all local queues starting with those that differ in lower bits and going gradually up.
                // this way we want to minimize the chances that two threads concurrently go through the same sequence of queues.
                for (int i = 0; i < queues.Length; i++)
                {
                    var localWsq = queues[localQueueIndex ^ i];
                    callback = localWsq?.Dequeue(ref missedSteal);
                    if (callback != null)
                    {
                        break;
                    }
                }
            }

            return callback;
        }

        public long LocalCount
        {
            get
            {
                long count = 0;
                foreach (LocalQueue workStealingQueue in ThreadPoolGlobals.workQueue._localQueues)
                {
                    if (workStealingQueue != null)
                    {
                        count += workStealingQueue.Count;
                    }
                }
                return count;
            }
        }

        public long GlobalCount => ThreadPoolGlobals.workQueue._globalQueue.Count;

        /// <summary>
        /// Dispatches work items to this thread.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this thread did as much work as was available or its quantum expired.
        /// <c>false</c> if this thread stopped working early.
        /// </returns>
        internal static bool Dispatch()
        {
            // "Progress Guarantee"
            // To ensure eventual dequeue of every enqueued item we must guarantee that:
            //     1) after an enqueue, there is at least one worker request pending.
            //        (we can't gurantee that workers which are already in dispatch will see the new item)
            //     2) the newly dispatched worker will see the work, if it is still there.
            //     3) ensure another thread request when leaving Dequeue, unless certain that all the work that
            //        waited for us has been dequeued or that someone else will issue a thread request.
            //
            //  NB: We are not responsible for new work enqueued after we entered dispatch.
            //      We will do our best, but ultimately it is the #1 that guarantees that someone will take care of
            //      work enqueued in the future.

            ThreadPoolWorkQueue outerWorkQueue = ThreadPoolGlobals.workQueue;

            //
            // Update our records to indicate that an outstanding request for a thread has now been fulfilled.
            // From this point on, we are responsible for requesting another thread if we stop working for any
            // reason and are unsure whether the queue is completely empty.
            //
            // CoreCLR: this whole scheme does not support thread aborts.
            //          FWIW a thread could be aborted right after dequeuing an item and before executing.
            //          We do not handle or expect that.
            outerWorkQueue.MarkThreadRequestSatisfied();

            //
            // The clock is ticking!  We have ThreadPoolGlobals.TP_QUANTUM milliseconds to get some work done, and then
            // we need to return to the VM.
            //
            int quantumStartTime = Environment.TickCount;

            // Has the desire for logging changed since the last time we entered?
            var enabled = FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.ThreadPool | FrameworkEventSource.Keywords.ThreadTransfer);
            if (outerWorkQueue.loggingEnabled != enabled)
            {
                // writing shared state.
                outerWorkQueue.loggingEnabled = enabled;
            }

            //
            // Assume that we're going to need another thread if this one returns to the VM.  We'll set this to
            // false later, but only if we're absolutely certain that the queue is empty.
            //
            bool keepThreadSpinning = true;

            try
            {
                Thread currentThread = Thread.CurrentThread;
                // Start on clean ExecutionContext and SynchronizationContext
                currentThread._executionContext = null;
                currentThread._synchronizationContext = null;

                // Every new worker that finds work will ask for parallelizm increase, but only once.
                // This helps with front-edge ramping up from cold states.
                bool requestedAnotherWorker = false;

                //
                // Use operate on workQueue local to try block so it can be enregistered
                ThreadPoolWorkQueue workQueue = ThreadPoolGlobals.workQueue;

                //
                // Loop until our quantum expires or there is no work.
                //
                do
                {
                    var localQueue = workQueue.GetOrAddLocalQueue();
                    object? workItem = localQueue.TryPop();

                    if (workItem == null)
                    {
                        var spinner = new SpinWait();
                        for (; ; )
                        {
                            // we could not pop, try stealing
                            bool missedSteal = false;
                            workItem = workQueue.DequeueAny(ref missedSteal, localQueue);
                            if (workItem != null)
                            {
                                break;
                            }

                            // if there is no more work, start reducing parallelism exponentially
                            if (!missedSteal && (localQueue.NextRnd() & 1) == 0)
                            {
                                keepThreadSpinning = false;
                                // Tell the VM we're returning normally, not because Hill Climbing asked us to return.
                                return true;
                            }

                            // so back off a little and try again (as long as quantum has not expired)
                            spinner.SpinOnce();
                        }
                    }

                    if (workQueue.loggingEnabled)
                        System.Diagnostics.Tracing.FrameworkEventSource.Log.ThreadPoolDequeueWorkObject(workItem);

                    //
                    // We are about to execute external code, which can take a while, block or even wait on something from other tasks.
                    // Make sure there is a request, so that starvation is noticed if we do not come back for a while.
                    // If this is our first workitem, be more aggressive.
                    if (!requestedAnotherWorker)
                    {
                        requestedAnotherWorker = true;
                        workQueue.RequestThread();
                    }
                    else
                    {
                        workQueue.EnsureThreadRequested();
                    }

                    //
                    // Execute the workitem outside of any finally blocks, so that it can be aborted if needed.
                    //
                    if (ThreadPoolGlobals.enableWorkerTracking)
                    {
                        bool reportedStatus = false;
                        try
                        {
                            ThreadPool.ReportThreadStatus(isWorking: true);
                            reportedStatus = true;
                            if (workItem is Task task)
                            {
                                task.ExecuteFromThreadPool(currentThread);
                            }
                            else
                            {
                                Debug.Assert(workItem is IThreadPoolWorkItem);
                                Unsafe.As<IThreadPoolWorkItem>(workItem).Execute();
                            }
                        }
                        finally
                        {
                            if (reportedStatus)
                                ThreadPool.ReportThreadStatus(isWorking: false);
                        }
                    }
                    else if (workItem is Task task)
                    {
                        // Check for Task first as it's currently faster to type check
                        // for Task and then Unsafe.As for the interface, rather than
                        // vice versa, in particular when the object implements a bunch
                        // of interfaces.
                        task.ExecuteFromThreadPool(currentThread);
                    }
                    else
                    {
                        Debug.Assert(workItem is IThreadPoolWorkItem);
                        Unsafe.As<IThreadPoolWorkItem>(workItem).Execute();
                    }

                    currentThread.ResetThreadPoolThread();

                    // Return to clean ExecutionContext and SynchronizationContext
                    ExecutionContext.ResetThreadPoolThread(currentThread);

                    //
                    // Notify the VM that we executed this workitem.  This is also our opportunity to ask whether Hill Climbing wants
                    // us to return the thread to the pool or not.
                    //
                    if (!ThreadPool.NotifyWorkItemComplete())
                        return false;
                }
                while (ThreadPool.KeepDispatching(quantumStartTime));

                // If we get here, it's because our quantum expired.  Tell the VM we're returning normally.
                return true;
            }
            finally
            {
                //
                // We are exiting, but not because we failed to find work, so we want to keep the same number of spinners.
                // Make a request for a thread (up to #proc) to account for our leaving.
                //
                if (keepThreadSpinning)
                    ThreadPoolGlobals.workQueue.KeepThread();

                // we are releasing unneeded thread back to VM or the thread has run for a full quantum.
                // in either case it makes sense to flush the cached core Id.
                Thread.FlushCurrentProcessorId();
            }
        }
    }
    public delegate void WaitCallback(object? state);

    public delegate void WaitOrTimerCallback(object? state, bool timedOut);  // signaled or timed out

    internal abstract class QueueUserWorkItemCallbackBase : IThreadPoolWorkItem
    {
#if DEBUG
        private int executed;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~QueueUserWorkItemCallbackBase()
        {
            Interlocked.MemoryBarrier(); // ensure that an old cached value is not read below
            Debug.Assert(
                executed != 0, "A QueueUserWorkItemCallback was never called!");
        }
#endif

        public virtual void Execute()
        {
#if DEBUG
            GC.SuppressFinalize(this);
            Debug.Assert(
                0 == Interlocked.Exchange(ref executed, 1),
                "A QueueUserWorkItemCallback was called twice!");
#endif
        }
    }

    internal sealed class QueueUserWorkItemCallback : QueueUserWorkItemCallbackBase
    {
        private WaitCallback? _callback; // SOS's ThreadPool command depends on this name
        private readonly object? _state;
        private readonly ExecutionContext _context;

        private static readonly Action<QueueUserWorkItemCallback> s_executionContextShim = quwi =>
        {
            Debug.Assert(quwi._callback != null);
            WaitCallback callback = quwi._callback;
            quwi._callback = null;

            callback(quwi._state);
        };

        internal QueueUserWorkItemCallback(WaitCallback callback, object? state, ExecutionContext context)
        {
            Debug.Assert(context != null);

            _callback = callback;
            _state = state;
            _context = context;
        }

        public override void Execute()
        {
            base.Execute();

            ExecutionContext.RunForThreadPoolUnsafe(_context, s_executionContextShim, this);
        }
    }

    internal sealed class QueueUserWorkItemCallback<TState> : QueueUserWorkItemCallbackBase
    {
        private Action<TState>? _callback; // SOS's ThreadPool command depends on this name
        private readonly TState _state;
        private readonly ExecutionContext _context;

        internal QueueUserWorkItemCallback(Action<TState> callback, TState state, ExecutionContext context)
        {
            Debug.Assert(callback != null);

            _callback = callback;
            _state = state;
            _context = context;
        }

        public override void Execute()
        {
            base.Execute();

            Debug.Assert(_callback != null);
            Action<TState> callback = _callback;
            _callback = null;

            ExecutionContext.RunForThreadPoolUnsafe(_context, callback, in _state);
        }
    }

    internal sealed class QueueUserWorkItemCallbackDefaultContext : QueueUserWorkItemCallbackBase
    {
        private WaitCallback? _callback; // SOS's ThreadPool command depends on this name
        private readonly object? _state;

        internal QueueUserWorkItemCallbackDefaultContext(WaitCallback callback, object? state)
        {
            Debug.Assert(callback != null);

            _callback = callback;
            _state = state;
        }

        public override void Execute()
        {
            ExecutionContext.CheckThreadPoolAndContextsAreDefault();
            base.Execute();

            Debug.Assert(_callback != null);
            WaitCallback callback = _callback;
            _callback = null;

            callback(_state);

            // ThreadPoolWorkQueue.Dispatch will handle notifications and reset EC and SyncCtx back to default
        }
    }

    internal sealed class QueueUserWorkItemCallbackDefaultContext<TState> : QueueUserWorkItemCallbackBase
    {
        private Action<TState>? _callback; // SOS's ThreadPool command depends on this name
        private readonly TState _state;

        internal QueueUserWorkItemCallbackDefaultContext(Action<TState> callback, TState state)
        {
            Debug.Assert(callback != null);

            _callback = callback;
            _state = state;
        }

        public override void Execute()
        {
            ExecutionContext.CheckThreadPoolAndContextsAreDefault();
            base.Execute();

            Debug.Assert(_callback != null);
            Action<TState> callback = _callback;
            _callback = null;

            callback(_state);

            // ThreadPoolWorkQueue.Dispatch will handle notifications and reset EC and SyncCtx back to default
        }
    }

    internal sealed class _ThreadPoolWaitOrTimerCallback
    {
        private readonly WaitOrTimerCallback _waitOrTimerCallback;
        private readonly ExecutionContext? _executionContext;
        private readonly object? _state;
        private static readonly ContextCallback _ccbt = new ContextCallback(WaitOrTimerCallback_Context_t);
        private static readonly ContextCallback _ccbf = new ContextCallback(WaitOrTimerCallback_Context_f);

        internal _ThreadPoolWaitOrTimerCallback(WaitOrTimerCallback waitOrTimerCallback, object? state, bool flowExecutionContext)
        {
            _waitOrTimerCallback = waitOrTimerCallback;
            _state = state;

            if (flowExecutionContext)
            {
                // capture the exection context
                _executionContext = ExecutionContext.Capture();
            }
        }

        private static void WaitOrTimerCallback_Context_t(object? state) =>
            WaitOrTimerCallback_Context(state, timedOut: true);

        private static void WaitOrTimerCallback_Context_f(object? state) =>
            WaitOrTimerCallback_Context(state, timedOut: false);

        private static void WaitOrTimerCallback_Context(object? state, bool timedOut)
        {
            _ThreadPoolWaitOrTimerCallback helper = (_ThreadPoolWaitOrTimerCallback)state!;
            helper._waitOrTimerCallback(helper._state, timedOut);
        }

        // call back helper
        internal static void PerformWaitOrTimerCallback(_ThreadPoolWaitOrTimerCallback helper, bool timedOut)
        {
            Debug.Assert(helper != null, "Null state passed to PerformWaitOrTimerCallback!");
            // call directly if it is an unsafe call OR EC flow is suppressed
            ExecutionContext? context = helper._executionContext;
            if (context == null)
            {
                WaitOrTimerCallback callback = helper._waitOrTimerCallback;
                callback(helper._state, timedOut);
            }
            else
            {
                ExecutionContext.Run(context, timedOut ? _ccbt : _ccbf, helper);
            }
        }
    }

    public static partial class ThreadPool
    {
        [CLSCompliant(false)]
        public static RegisteredWaitHandle RegisterWaitForSingleObject(
             WaitHandle waitObject,
             WaitOrTimerCallback callBack,
             object? state,
             uint millisecondsTimeOutInterval,
             bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval > (uint)int.MaxValue && millisecondsTimeOutInterval != uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, true);
        }

        [CLSCompliant(false)]
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(
             WaitHandle waitObject,
             WaitOrTimerCallback callBack,
             object? state,
             uint millisecondsTimeOutInterval,
             bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval > (uint)int.MaxValue && millisecondsTimeOutInterval != uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, false);
        }

        public static RegisteredWaitHandle RegisterWaitForSingleObject(
             WaitHandle waitObject,
             WaitOrTimerCallback callBack,
             object? state,
             int millisecondsTimeOutInterval,
             bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, true);
        }

        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(
             WaitHandle waitObject,
             WaitOrTimerCallback callBack,
             object? state,
             int millisecondsTimeOutInterval,
             bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
             )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, false);
        }

        public static RegisteredWaitHandle RegisterWaitForSingleObject(
            WaitHandle waitObject,
            WaitOrTimerCallback callBack,
            object? state,
            long millisecondsTimeOutInterval,
            bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
        )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            if (millisecondsTimeOutInterval > (uint)int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, true);
        }

        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(
            WaitHandle waitObject,
            WaitOrTimerCallback callBack,
            object? state,
            long millisecondsTimeOutInterval,
            bool executeOnlyOnce    // NOTE: we do not allow other options that allow the callback to be queued as an APC
        )
        {
            if (millisecondsTimeOutInterval < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            if (millisecondsTimeOutInterval > (uint)int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeOutInterval), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, false);
        }

        public static RegisteredWaitHandle RegisterWaitForSingleObject(
                          WaitHandle waitObject,
                          WaitOrTimerCallback callBack,
                          object? state,
                          TimeSpan timeout,
                          bool executeOnlyOnce
                          )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if (tm < -1)
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            if (tm > (long)int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)tm, executeOnlyOnce, true);
        }

        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(
                          WaitHandle waitObject,
                          WaitOrTimerCallback callBack,
                          object? state,
                          TimeSpan timeout,
                          bool executeOnlyOnce
                          )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if (tm < -1)
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
            if (tm > (long)int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_LessEqualToIntegerMaxVal);
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)tm, executeOnlyOnce, false);
        }

        public static bool QueueUserWorkItem(WaitCallback callBack) =>
            QueueUserWorkItem(callBack, null);

        public static bool QueueUserWorkItem(WaitCallback callBack, object? state)
        {
            if (callBack == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.callBack);
            }

            EnsureInitialized();

            ExecutionContext? context = ExecutionContext.Capture();

            object tpcallBack = (context == null || context.IsDefault) ?
                new QueueUserWorkItemCallbackDefaultContext(callBack!, state) :
                (object)new QueueUserWorkItemCallback(callBack!, state, context);

            ThreadPoolGlobals.workQueue.Enqueue(tpcallBack, forceGlobal: true);

            return true;
        }

        public static bool QueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            if (callBack == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.callBack);
            }

            EnsureInitialized();

            ExecutionContext? context = ExecutionContext.Capture();

            object tpcallBack = (context == null || context.IsDefault) ?
                new QueueUserWorkItemCallbackDefaultContext<TState>(callBack!, state) :
                (object)new QueueUserWorkItemCallback<TState>(callBack!, state, context);

            ThreadPoolGlobals.workQueue.Enqueue(tpcallBack, forceGlobal: !preferLocal);

            return true;
        }

        public static bool UnsafeQueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            if (callBack == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.callBack);
            }

            // If the callback is the runtime-provided invocation of an IAsyncStateMachineBox,
            // then we can queue the Task state directly to the ThreadPool instead of
            // wrapping it in a QueueUserWorkItemCallback.
            //
            // This occurs when user code queues its provided continuation to the ThreadPool;
            // internally we call UnsafeQueueUserWorkItemInternal directly for Tasks.
            if (ReferenceEquals(callBack, ThreadPoolGlobals.s_invokeAsyncStateMachineBox))
            {
                if (!(state is IAsyncStateMachineBox))
                {
                    // The provided state must be the internal IAsyncStateMachineBox (Task) type
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.state);
                }

                UnsafeQueueUserWorkItemInternal((object)state!, preferLocal);
                return true;
            }

            EnsureInitialized();

            ThreadPoolGlobals.workQueue.Enqueue(
                new QueueUserWorkItemCallbackDefaultContext<TState>(callBack!, state), forceGlobal: !preferLocal);

            return true;
        }

        public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object? state)
        {
            if (callBack == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.callBack);
            }

            EnsureInitialized();

            object tpcallBack = new QueueUserWorkItemCallbackDefaultContext(callBack!, state);

            ThreadPoolGlobals.workQueue.Enqueue(tpcallBack, forceGlobal: true);

            return true;
        }

        public static bool UnsafeQueueUserWorkItem(IThreadPoolWorkItem callBack, bool preferLocal)
        {
            if (callBack == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.callBack);
            }
            if (callBack is Task)
            {
                // Prevent code from queueing a derived Task that also implements the interface,
                // as that would bypass Task.Start and its safety checks.
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.callBack);
            }

            UnsafeQueueUserWorkItemInternal(callBack!, preferLocal);
            return true;
        }

        internal static void UnsafeQueueUserWorkItemInternal(object callBack, bool preferLocal)
        {
            Debug.Assert((callBack is IThreadPoolWorkItem) ^ (callBack is Task));

            EnsureInitialized();

            ThreadPoolGlobals.workQueue.Enqueue(callBack, forceGlobal: !preferLocal);
        }

        // This method tries to take the target callback out of the current thread's queue.
        internal static bool TryPopCustomWorkItem(object workItem)
        {
            Debug.Assert(null != workItem);

            return
                ThreadPoolGlobals.threadPoolInitialized && // if not initialized, so there's no way this workitem was ever queued.
                ThreadPoolGlobals.workQueue.TryRemove(workItem);
        }

        // Get all workitems.  Called by TaskScheduler in its debugger hooks.
        internal static IEnumerable<object> GetQueuedWorkItems()
        {
            // Enumerate global queue
            foreach (object o in GetGloballyQueuedWorkItems())
            {
                yield return o;
            }

            // Enumerate each local queues
            var workQueue = ThreadPoolGlobals.workQueue;
            foreach (ThreadPoolWorkQueue.LocalQueue wsq in workQueue._localQueues)
            {
                if (wsq != null)
                {
                    for (ThreadPoolWorkQueue.LocalQueue.LocalQueueSegment? s = wsq._deqSegment; s != null; s = s._nextSegment)
                    {
                        foreach (var slot in s._slots)
                        {
                            object? item = slot.Item;
                            if (item != null)
                            {
                                yield return item;
                            }
                        }
                    }
                }
            }
        }

        internal static IEnumerable<object> GetLocallyQueuedWorkItems()
        {
            ThreadPoolWorkQueue.LocalQueue? wsq = ThreadPoolGlobals.workQueue.GetLocalQueue();
            if (wsq != null)
            {
                for (ThreadPoolWorkQueue.LocalQueue.LocalQueueSegment? s = wsq._deqSegment; s != null; s = s._nextSegment)
                {
                    foreach (var slot in s._slots)
                    {
                        object? item = slot.Item;
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        internal static IEnumerable<object> GetGloballyQueuedWorkItems()
        {
            var workQueue = ThreadPoolGlobals.workQueue;

            // Enumerate global queue
            for (ThreadPoolWorkQueue.GlobalQueue.GlobalQueueSegment? s = workQueue._globalQueue._deqSegment; s != null; s = s._nextSegment)
            {
                foreach (var slot in s._slots)
                {
                    object? item = slot.Item;
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
        }

        private static object[] ToObjectArray(IEnumerable<object> workitems)
        {
            int i = 0;
            foreach (object item in workitems)
            {
                i++;
            }

            object[] result = new object[i];
            i = 0;
            foreach (object item in workitems)
            {
                if (i < result.Length) // just in case someone calls us while the queues are in motion
                    result[i] = item;
                i++;
            }

            return result;
        }

        // This is the method the debugger will actually call, if it ends up calling
        // into ThreadPool directly.  Tests can use this to simulate a debugger, as well.
        internal static object[] GetQueuedWorkItemsForDebugger() =>
            ToObjectArray(GetQueuedWorkItems());

        internal static object[] GetGloballyQueuedWorkItemsForDebugger() =>
            ToObjectArray(GetGloballyQueuedWorkItems());

        internal static object[] GetLocallyQueuedWorkItemsForDebugger() =>
            ToObjectArray(GetLocallyQueuedWorkItems());

        /// <summary>
        /// Gets the number of work items that are currently queued to be processed.
        /// </summary>
        /// <remarks>
        /// For a thread pool implementation that may have different types of work items, the count includes all types that can
        /// be tracked, which may only be the user work items including tasks. Some implementations may also include queued
        /// timer and wait callbacks in the count. On Windows, the count is unlikely to include the number of pending IO
        /// completions, as they get posted directly to an IO completion port.
        /// </remarks>
        public static long PendingWorkItemCount
        {
            get
            {
                ThreadPoolWorkQueue workQueue = ThreadPoolGlobals.workQueue;
                return workQueue.LocalCount + workQueue.GlobalCount + PendingUnmanagedWorkItemCount;
            }
        }
    }
}
