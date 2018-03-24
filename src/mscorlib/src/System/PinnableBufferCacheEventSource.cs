// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace System
{
    [EventSource(Guid = "38ed3633-5e3f-5989-bf25-f0b1b3318c9b", Name = "Microsoft -DotNETRuntime-PinnableBufferCache-System")]
    internal sealed class PinnableBufferCacheEventSource : EventSource
    {
        public static readonly PinnableBufferCacheEventSource Log = new PinnableBufferCacheEventSource();

        [Event(1, Level = EventLevel.Verbose)]
        public void DebugMessage(string message) { if (IsEnabled()) WriteEvent(1, message); }
        [Event(2, Level = EventLevel.Verbose)]
        public void DebugMessage1(string message, long value) { if (IsEnabled()) WriteEvent(2, message, value); }
        [Event(3, Level = EventLevel.Verbose)]
        public void DebugMessage2(string message, long value1, long value2) { if (IsEnabled()) WriteEvent(3, message, value1, value2); }
        [Event(18, Level = EventLevel.Verbose)]
        public void DebugMessage3(string message, long value1, long value2, long value3) { if (IsEnabled()) WriteEvent(18, message, value1, value2, value3); }

        [Event(4)]
        public void Create(string cacheName) { if (IsEnabled()) WriteEvent(4, cacheName); }

        [Event(5, Level = EventLevel.Verbose)]
        public void AllocateBuffer(string cacheName, ulong objectId, int objectHash, int objectGen, int freeCountAfter) { if (IsEnabled()) WriteEvent(5, cacheName, objectId, objectHash, objectGen, freeCountAfter); }
        [Event(6)]
        public void AllocateBufferFromNotGen2(string cacheName, int notGen2CountAfter) { if (IsEnabled()) WriteEvent(6, cacheName, notGen2CountAfter); }
        [Event(7)]
        public void AllocateBufferCreatingNewBuffers(string cacheName, int totalBuffsBefore, int objectCount) { if (IsEnabled()) WriteEvent(7, cacheName, totalBuffsBefore, objectCount); }
        [Event(8)]
        public void AllocateBufferAged(string cacheName, int agedCount) { if (IsEnabled()) WriteEvent(8, cacheName, agedCount); }
        [Event(9)]
        public void AllocateBufferFreeListEmpty(string cacheName, int notGen2CountBefore) { if (IsEnabled()) WriteEvent(9, cacheName, notGen2CountBefore); }

        [Event(10, Level = EventLevel.Verbose)]
        public void FreeBuffer(string cacheName, ulong objectId, int objectHash, int freeCountBefore) { if (IsEnabled()) WriteEvent(10, cacheName, objectId, objectHash, freeCountBefore); }
        [Event(11)]
        public void FreeBufferStillTooYoung(string cacheName, int notGen2CountBefore) { if (IsEnabled()) WriteEvent(11, cacheName, notGen2CountBefore); }

        [Event(13)]
        public void TrimCheck(string cacheName, int totalBuffs, bool neededMoreThanFreeList, int deltaMSec) { if (IsEnabled()) WriteEvent(13, cacheName, totalBuffs, neededMoreThanFreeList, deltaMSec); }
        [Event(14)]
        public void TrimFree(string cacheName, int totalBuffs, int freeListCount, int toBeFreed) { if (IsEnabled()) WriteEvent(14, cacheName, totalBuffs, freeListCount, toBeFreed); }
        [Event(15)]
        public void TrimExperiment(string cacheName, int totalBuffs, int freeListCount, int numTrimTrial) { if (IsEnabled()) WriteEvent(15, cacheName, totalBuffs, freeListCount, numTrimTrial); }
        [Event(16)]
        public void TrimFreeSizeOK(string cacheName, int totalBuffs, int freeListCount) { if (IsEnabled()) WriteEvent(16, cacheName, totalBuffs, freeListCount); }
        [Event(17)]
        public void TrimFlush(string cacheName, int totalBuffs, int freeListCount, int notGen2CountBefore) { if (IsEnabled()) WriteEvent(17, cacheName, totalBuffs, freeListCount, notGen2CountBefore); }
        [Event(20)]
        public void AgePendingBuffersResults(string cacheName, int promotedToFreeListCount, int heldBackCount) { if (IsEnabled()) WriteEvent(20, cacheName, promotedToFreeListCount, heldBackCount); }
        [Event(21)]
        public void WalkFreeListResult(string cacheName, int freeListCount, int gen0BuffersInFreeList) { if (IsEnabled()) WriteEvent(21, cacheName, freeListCount, gen0BuffersInFreeList); }
        [Event(22)]
        public void FreeBufferNull(string cacheName, int freeCountBefore) { if (IsEnabled()) WriteEvent(22, cacheName, freeCountBefore); }

        static internal ulong AddressOf(object obj)
        {
            if (obj is byte[] asByteArray)
                return (ulong)AddressOfByteArray(asByteArray);
            return 0;
        }

        static internal unsafe long AddressOfByteArray(byte[] array)
        {
            if (array == null)
                return 0;
            fixed (byte* ptr = array)
                return (long)(ptr - 2 * sizeof(void*));
        }
    }
}
