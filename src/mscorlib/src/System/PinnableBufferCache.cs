// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#define ENABLE
#define MINBUFFERS

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace System
{
    internal sealed class PinnableBufferCache
    {
        private const int DefaultNumberOfBuffers = 16;
        private string _cacheName;
        private Func<object> _factory;

        /// <summary>
        /// Contains 'good' buffers to reuse.  They are guaranteed to be Gen 2 ENFORCED!
        /// </summary>
        private ConcurrentStack<object> _freeList = new ConcurrentStack<object>();

        /// <summary>
        /// Contains buffers that are not gen 2 and thus we do not wish to give out unless we have to.
        /// To implement trimming we sometimes put aged buffers in here as a place to 'park' them
        /// before true deletion.
        /// </summary>
        private List<object> _notGen2;

        /// <summary>
        /// What was the gen 1 count the last time re restocked?  If it is now greater, then
        /// we know that all objects are in Gen 2 so we don't have to check.  Should be updated
        /// every time something gets added to the m_NotGen2 list.
        /// </summary>
        private int _gen1CountAtLastRestock;

        /// <summary>
        /// Used to ensure we have a minimum time between trimmings.
        /// </summary>
        private int _msecNoUseBeyondFreeListSinceThisTime;

        /// <summary>
        /// To trim, we remove things from the free list (which is Gen 2) and see if we 'hit bottom'
        /// This flag indicates that we hit bottom (we really needed a bigger free list).
        /// </summary>
        private bool _moreThanFreeListNeeded;

        /// <summary>
        /// The total number of buffers that this cache has ever allocated.
        /// Used in trimming heuristics.
        /// </summary>
        private int _buffersUnderManagement;

        /// <summary>
        /// The number of buffers we added the last time we restocked.
        /// </summary>
        private int _restockSize;

        /// <summary>
        /// Did we put some buffers into m_NotGen2 to see if we can trim?
        /// </summary>
        private bool _trimmingExperimentInProgress;

        /// <summary>
        /// A forced minimum number of buffers.
        /// </summary>
        private int _minBufferCount;

        /// <summary>
        /// The number of calls to Allocate.
        /// </summary>
        private int _numAllocCalls;

        /// <summary>
        /// Create a PinnableBufferCache that works on any object (it is intended for OverlappedData)
        /// This is only used in mscorlib.
        /// </summary>
        internal PinnableBufferCache(string cacheName, Func<object> factory)
        {
            _notGen2 = new List<object>(DefaultNumberOfBuffers);
            _factory = factory;
#if ENABLE
            // Check to see if we should disable the cache.
            string envVarName = "PinnableBufferCache_" + cacheName + "_Disabled";
            try
            {
                string envVar = Environment.GetEnvironmentVariable(envVarName);
                if (envVar != null)
                {
                    PinnableBufferCacheEventSource.Log.DebugMessage("Creating " + cacheName + " PinnableBufferCacheDisabled=" + envVar);
                    int index = envVar.IndexOf(cacheName, StringComparison.OrdinalIgnoreCase);
                    if (0 <= index)
                    {
                        // The cache is disabled because we haven't set the cache name.
                        PinnableBufferCacheEventSource.Log.DebugMessage("Disabling " + cacheName);
                        return;
                    }
                }
            }
            catch
            {
                // Ignore failures when reading the environment variable.
            }
#endif
#if MINBUFFERS
            // Allow the environment to specify a minimum buffer count.
            string minEnvVarName = "PinnableBufferCache_" + cacheName + "_MinCount";
            try
            {
                string minEnvVar = Environment.GetEnvironmentVariable(minEnvVarName);
                if (minEnvVar != null)
                {
                    if (int.TryParse(minEnvVar, out _minBufferCount))
                        CreateNewBuffers();
                }
            }
            catch
            {
                // Ignore failures when reading the environment variable.
            }
#endif

            PinnableBufferCacheEventSource.Log.Create(cacheName);
            _cacheName = cacheName;
        }

        /// <summary>
        /// Get a object from the buffer manager.  If no buffers exist, allocate a new one.
        /// </summary>
        internal object Allocate()
        {
#if ENABLE
            // Check to see whether or not the cache is disabled.
            if (_cacheName == null)
                return _factory();
#endif
            // Fast path, get it from our Gen2 aged m_FreeList.
            if (!_freeList.TryPop(out object returnBuffer))
                Restock(out returnBuffer);

            // Computing free count is expensive enough that we don't want to compute it unless logging is on.
            if (PinnableBufferCacheEventSource.Log.IsEnabled())
            {
                int numAllocCalls = Interlocked.Increment(ref _numAllocCalls);
                if (numAllocCalls >= 1024)
                {
                    lock (this)
                    {
                        int previousNumAllocCalls = Interlocked.Exchange(ref _numAllocCalls, 0);
                        if (previousNumAllocCalls >= 1024)
                        {
                            int nonGen2Count = 0;
                            foreach (object o in _freeList)
                            {
                                if (GC.GetGeneration(o) < GC.MaxGeneration)
                                {
                                    nonGen2Count++;
                                }
                            }

                            PinnableBufferCacheEventSource.Log.WalkFreeListResult(_cacheName, _freeList.Count, nonGen2Count);
                        }
                    }
                }

                PinnableBufferCacheEventSource.Log.AllocateBuffer(_cacheName, PinnableBufferCacheEventSource.AddressOf(returnBuffer), returnBuffer.GetHashCode(), GC.GetGeneration(returnBuffer), _freeList.Count);
            }
            return returnBuffer;
        }

        /// <summary>
        /// Return a buffer back to the buffer manager.
        /// </summary>
        internal void Free(object buffer)
        {
#if ENABLE
            // Check to see whether or not the cache is disabled.
            if (_cacheName == null)
                return;
#endif
            if (PinnableBufferCacheEventSource.Log.IsEnabled())
                PinnableBufferCacheEventSource.Log.FreeBuffer(_cacheName, PinnableBufferCacheEventSource.AddressOf(buffer), buffer.GetHashCode(), _freeList.Count);

            // After we've done 3 gen1 GCs, assume that all buffers have aged into gen2 on the free path.
            if ((_gen1CountAtLastRestock + 3) > GC.CollectionCount(GC.MaxGeneration - 1))
            {
                lock (this)
                {
                    if (GC.GetGeneration(buffer) < GC.MaxGeneration)
                    {
                        // The buffer is not aged, so put it in the non-aged free list.
                        _moreThanFreeListNeeded = true;
                        PinnableBufferCacheEventSource.Log.FreeBufferStillTooYoung(_cacheName, _notGen2.Count);
                        _notGen2.Add(buffer);
                        _gen1CountAtLastRestock = GC.CollectionCount(GC.MaxGeneration - 1);
                        return;
                    }
                }
            }

            // If we discovered that it is indeed Gen2, great, put it in the Gen2 list.
            _freeList.Push(buffer);
        }

        /// <summary>
        /// Called when we don't have any buffers in our free list to give out.
        /// </summary>
        /// <returns></returns>
        private void Restock(out object returnBuffer)
        {
            lock (this)
            {
                // Try again after getting the lock as another thread could have just filled the free list.  If we don't check
                // then we unnecessarily grab a new set of buffers because we think we are out.
                if (_freeList.TryPop(out returnBuffer))
                    return;

                // Lazy init, Ask that TrimFreeListIfNeeded be called on every Gen 2 GC.
                if (_restockSize == 0)
                    Gen2GcCallback.Register(Gen2GcCallbackFunc, this);

                // Indicate to the trimming policy that the free list is insufficent.
                _moreThanFreeListNeeded = true;
                PinnableBufferCacheEventSource.Log.AllocateBufferFreeListEmpty(_cacheName, _notGen2.Count);

                // Get more buffers if needed.
                if (_notGen2.Count == 0)
                    CreateNewBuffers();

                // We have no buffers in the aged freelist, so get one from the newer list.   Try to pick the best one.
                // Debug.Assert(m_NotGen2.Count != 0);
                int idx = _notGen2.Count - 1;
                if (GC.GetGeneration(_notGen2[idx]) < GC.MaxGeneration && GC.GetGeneration(_notGen2[0]) == GC.MaxGeneration)
                    idx = 0;
                returnBuffer = _notGen2[idx];
                _notGen2.RemoveAt(idx);

                // Remember any sub-optimial buffer so we don't put it on the free list when it gets freed.
                if (PinnableBufferCacheEventSource.Log.IsEnabled() && GC.GetGeneration(returnBuffer) < GC.MaxGeneration)
                {
                    PinnableBufferCacheEventSource.Log.AllocateBufferFromNotGen2(_cacheName, _notGen2.Count);
                }

                // If we have a Gen1 collection, then everything on m_NotGen2 should have aged.  Move them to the m_Free list.
                if (!AgePendingBuffers())
                {
                    // Before we could age at set of buffers, we have handed out half of them.
                    // This implies we should be proactive about allocating more (since we will trim them if we over-allocate).
                    if (_notGen2.Count == _restockSize / 2)
                    {
                        PinnableBufferCacheEventSource.Log.DebugMessage("Proactively adding more buffers to aging pool");
                        CreateNewBuffers();
                    }
                }
            }
        }

        /// <summary>
        /// See if we can promote the buffers to the free list.  Returns true if successful. 
        /// </summary>
        private bool AgePendingBuffers()
        {
            if (_gen1CountAtLastRestock < GC.CollectionCount(GC.MaxGeneration - 1))
            {
                // Allocate a temp list of buffers that are not actually in gen2, and swap it in once
                // we're done scanning all buffers.
                int promotedCount = 0;
                List<object> notInGen2 = new List<object>();
                PinnableBufferCacheEventSource.Log.AllocateBufferAged(_cacheName, _notGen2.Count);
                for (int i = 0; i < _notGen2.Count; i++)
                {
                    // We actually check every object to ensure that we aren't putting non-aged buffers into the free list.
                    object currentBuffer = _notGen2[i];
                    if (GC.GetGeneration(currentBuffer) >= GC.MaxGeneration)
                    {
                        _freeList.Push(currentBuffer);
                        promotedCount++;
                    }
                    else
                    {
                        notInGen2.Add(currentBuffer);
                    }
                }
                PinnableBufferCacheEventSource.Log.AgePendingBuffersResults(_cacheName, promotedCount, notInGen2.Count);
                _notGen2 = notInGen2;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates some buffers to age into Gen2.
        /// </summary>
        private void CreateNewBuffers()
        {
            // We choose a very modest number of buffers initially because for the client case.  This is often enough.
            if (_restockSize == 0)
                _restockSize = 4;
            else if (_restockSize < DefaultNumberOfBuffers)
                _restockSize = DefaultNumberOfBuffers;
            else if (_restockSize < 256)
                _restockSize = _restockSize * 2;                // Grow quickly at small sizes
            else if (_restockSize < 4096)
                _restockSize = _restockSize * 3 / 2;            // Less agressively at large ones
            else
                _restockSize = 4096;                            // Cap how agressive we are

            // Ensure we hit our minimums
            if (_minBufferCount > _buffersUnderManagement)
                _restockSize = Math.Max(_restockSize, _minBufferCount - _buffersUnderManagement);

            PinnableBufferCacheEventSource.Log.AllocateBufferCreatingNewBuffers(_cacheName, _buffersUnderManagement, _restockSize);
            for (int i = 0; i < _restockSize; i++)
            {
                // Make a new buffer.
                object newBuffer = _factory();

                // Create space between the objects.  We do this because otherwise it forms a single plug (group of objects)
                // and the GC pins the entire plug making them NOT move to Gen1 and Gen2.   by putting space between them
                // we ensure that object get a chance to move independently (even if some are pinned).
                var dummyObject = new object();
                _notGen2.Add(newBuffer);
            }
            _buffersUnderManagement += _restockSize;
            _gen1CountAtLastRestock = GC.CollectionCount(GC.MaxGeneration - 1);
        }

        /// <summary>
        /// This is the static function that is called from the gen2 GC callback.
        /// The input object is the instance we want the callback on.
        /// </summary>
        /// <remarks>
        /// The reason that we make this function static and take the instance as a parameter is that
        /// we would otherwise root the instance to the Gen2GcCallback object, leaking the instance even when
        /// the application no longer needs it.
        /// </remarks>
        private static bool Gen2GcCallbackFunc(object target)
        {
            return ((PinnableBufferCache)(target)).TrimFreeListIfNeeded();
        }

        /// <summary>
        /// This is called on every gen2 GC to see if we need to trim the free list.
        /// NOTE: DO NOT CALL THIS DIRECTLY FROM THE GEN2GCCALLBACK.  INSTEAD CALL IT VIA A STATIC FUNCTION (SEE ABOVE).
        /// If you register a non-static function as a callback, then this object will be leaked.
        /// </summary>
        private bool TrimFreeListIfNeeded()
        {
            int curMSec = Environment.TickCount;
            int deltaMSec = curMSec - _msecNoUseBeyondFreeListSinceThisTime;
            PinnableBufferCacheEventSource.Log.TrimCheck(_cacheName, _buffersUnderManagement, _moreThanFreeListNeeded, deltaMSec);

            // If we needed more than just the set of aged buffers since the last time we were called,
            // we obviously should not be trimming any memory, so do nothing except reset the flag 
            if (_moreThanFreeListNeeded)
            {
                _moreThanFreeListNeeded = false;
                _trimmingExperimentInProgress = false;
                _msecNoUseBeyondFreeListSinceThisTime = curMSec;
                return true;
            }

            // We require a minimum amount of clock time to pass  (10 seconds) before we trim.  Ideally this time
            // is larger than the typical buffer hold time.
            if (0 <= deltaMSec && deltaMSec < 10000)
                return true;

            // If we got here we have spend the last few second without needing to lengthen the free list.   Thus
            // we have 'enough' buffers, but maybe we have too many.
            // See if we can trim
            lock (this)
            {
                // Hit a race, try again later.
                if (_moreThanFreeListNeeded)
                {
                    _moreThanFreeListNeeded = false;
                    _trimmingExperimentInProgress = false;
                    _msecNoUseBeyondFreeListSinceThisTime = curMSec;
                    return true;
                }

                var freeCount = _freeList.Count;   // This is expensive to fetch, do it once.

                // If there is something in m_NotGen2 it was not used for the last few seconds, it is trimable.  
                if (_notGen2.Count > 0)
                {
                    // If we are not performing an experiment and we have stuff that is waiting to go into the
                    // free list but has not made it there, it could be becasue the 'slow path' of restocking
                    // has not happened, so force this (which should flush the list) and start over.  
                    if (!_trimmingExperimentInProgress)
                    {
                        PinnableBufferCacheEventSource.Log.TrimFlush(_cacheName, _buffersUnderManagement, freeCount, _notGen2.Count);
                        AgePendingBuffers();
                        _trimmingExperimentInProgress = true;
                        return true;
                    }

                    PinnableBufferCacheEventSource.Log.TrimFree(_cacheName, _buffersUnderManagement, freeCount, _notGen2.Count);
                    _buffersUnderManagement -= _notGen2.Count;

                    // Possibly revise the restocking down.  We don't want to grow agressively if we are trimming.
                    var newRestockSize = _buffersUnderManagement / 4;
                    if (newRestockSize < _restockSize)
                        _restockSize = Math.Max(newRestockSize, DefaultNumberOfBuffers);

                    _notGen2.Clear();
                    _trimmingExperimentInProgress = false;
                    return true;
                }

                // Set up an experiment where we use 25% less buffers in our free list.   We put them in
                // m_NotGen2, and if they are needed they will be put back in the free list again.
                var trimSize = freeCount / 4 + 1;

                // We are OK with a 15% overhead, do nothing in that case.  
                if (freeCount * 15 <= _buffersUnderManagement || _buffersUnderManagement - trimSize <= _minBufferCount)
                {
                    PinnableBufferCacheEventSource.Log.TrimFreeSizeOK(_cacheName, _buffersUnderManagement, freeCount);
                    return true;
                }

                // Move buffers from the free list back to the non-aged list.  If we don't use them by next time, then we'll consider trimming them.
                PinnableBufferCacheEventSource.Log.TrimExperiment(_cacheName, _buffersUnderManagement, freeCount, trimSize);
                for (int i = 0; i < trimSize; i++)
                {
                    if (_freeList.TryPop(out object buffer))
                        _notGen2.Add(buffer);
                }
                _msecNoUseBeyondFreeListSinceThisTime = curMSec;
                _trimmingExperimentInProgress = true;
            }

            // Indicate that we want to be called back on the next Gen 2 GC.
            return true;
        }
    }
}
