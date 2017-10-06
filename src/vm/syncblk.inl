// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


#ifndef _SYNCBLK_INL_
#define _SYNCBLK_INL_

#ifndef DACCESS_COMPILE

FORCEINLINE bool AwareLock::LockState::InterlockedTryLock()
{
    WRAPPER_NO_CONTRACT;
    return InterlockedTryLock(*this);
}

FORCEINLINE bool AwareLock::LockState::InterlockedTryLock(LockState state)
{
    WRAPPER_NO_CONTRACT;

    // The monitor is fair to release waiters in FIFO order, but allows non-waiters to acquire the lock if it's available to
    // avoid lock convoys.
    //
    // Lock convoys can be detrimental to performance in scenarios where work is being done on multiple threads and the work
    // involves periodically taking a particular lock for a short time to access shared resources. With a lock convoy, once
    // there is a waiter for the lock (which is not uncommon in such scenarios), a worker thread would be forced to
    // context-switch on the subsequent attempt to acquire the lock, often long before the worker thread exhausts its time
    // slice. This process repeats as long as the lock has a waiter, forcing every worker to context-switch on each attempt to
    // acquire the lock, killing performance and creating a negative feedback loop that makes it more likely for the lock to
    // have waiters. To avoid the lock convoy, each worker needs to be allowed to acquire the lock multiple times in sequence
    // despite there being a waiter for the lock in order to have the worker continue working efficiently during its time slice
    // as long as the lock is not contended.
    //
    // This scheme has the possibility to starve waiters, but that would only happen if the lock is taken very frequently or if
    // it is heavily contended. Neither of these issues is common, and they typically indicate issues with lock usage in the
    // app, which the app may have to fix. For example, lock is held for too long, lock does not remain released for long
    // enough, lock is acquired and released too frequently, too many threads, etc.
    if (!state.IsLocked())
    {
        LockState newState = state;
        newState.InvertIsLocked();

        return CompareExchangeAcquire(newState, state) == state;
    }
    return false;
}

FORCEINLINE bool AwareLock::LockState::InterlockedUnlock()
{
    WRAPPER_NO_CONTRACT;
    static_assert_no_msg(IsLockedMask == 1);
    _ASSERTE(IsLocked());

    LockState state = InterlockedDecrementRelease((LONG *)&m_state);
    while (true)
    {
        // Keep track of whether a thread has been signaled to wake but has not yet woken from the wait.
        // IsWaiterSignaledToWakeMask is cleared when a signaled thread wakes up by observing a signal. Since threads can
        // preempt waiting threads and acquire the lock (see InterlockedTryLock()), it allows for example, one thread to acquire
        // and release the lock multiple times while there are multiple waiting threads. In such a case, we don't want that
        // thread to signal a waiter every time it releases the lock, as that will cause unnecessary context switches with more
        // and more signaled threads waking up, finding that the lock is still locked, and going right back into a wait state.
        // So, signal only one waiting thread at a time.
        if (!state.NeedToSignalWaiter())
        {
            return false;
        }

        LockState newState = state;
        newState.InvertIsWaiterSignaledToWake();

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return true;
        }

        state = stateBeforeUpdate;
    }
}

FORCEINLINE AwareLock::EnterHelperResult AwareLock::LockState::InterlockedTry_LockOrRegisterSpinner(LockState state)
{
    WRAPPER_NO_CONTRACT;

    while (true)
    {
        LockState newState = state;
        if (!state.IsLocked())
        {
            newState.InvertIsLocked();
        }
        else if (!newState.IncrementSpinnerCount())
        {
            return EnterHelperResult_UseSlowPath;
        }

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return !state.IsLocked() ? EnterHelperResult_Entered : EnterHelperResult_Contention;
        }

        state = stateBeforeUpdate;
    }
}

FORCEINLINE bool AwareLock::LockState::InterlockedTry_LockAndUnregisterSpinner()
{
    WRAPPER_NO_CONTRACT;

    // This function is called from inside a spin loop, it must unregister the spinner if and only if the lock is acquired
    LockState state = *this;
    while (true)
    {
        _ASSERTE(state.HasAnySpinners());
        if (state.IsLocked())
        {
            return false;
        }

        LockState newState = state;
        newState.InvertIsLocked();
        newState.DecrementSpinnerCount();

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return true;
        }

        state = stateBeforeUpdate;
    }
}

FORCEINLINE bool AwareLock::LockState::InterlockedUnregisterSpinner_TryLock()
{
    WRAPPER_NO_CONTRACT;

    // This function is called at the end of a spin loop, it must unregister the spinner always and acquire the lock if it's
    // available. If the lock is available, a spinner must acquire the lock along with unregistering itself, because a lock
    // releaser does not wake a waiter when there is a spinner registered.

    LockState stateBeforeUpdate = InterlockedExchangeAdd((LONG *)&m_state, -(LONG)SpinnerCountIncrement);
    _ASSERTE(stateBeforeUpdate.HasAnySpinners());
    if (stateBeforeUpdate.IsLocked())
    {
        return false;
    }

    LockState state = stateBeforeUpdate;
    state.DecrementSpinnerCount();
    _ASSERTE(!state.IsLocked());
    do
    {
        LockState newState = state;
        newState.InvertIsLocked();

        LockState stateBeforeUpdate = CompareExchangeAcquire(newState, state);
        if (stateBeforeUpdate == state)
        {
            return true;
        }

        state = stateBeforeUpdate;
    } while (!state.IsLocked());
    return false;
}

FORCEINLINE bool AwareLock::LockState::InterlockedTryLock_Or_RegisterWaiter(LockState state)
{
    WRAPPER_NO_CONTRACT;

    while (true)
    {
        LockState newState = state;
        if (!state.IsLocked())
        {
            newState.InvertIsLocked();
        }
        else
        {
            newState.IncrementWaiterCount();
        }

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return !state.IsLocked();
        }

        state = stateBeforeUpdate;
    }
}

FORCEINLINE void AwareLock::LockState::InterlockedUnregisterWaiter()
{
    WRAPPER_NO_CONTRACT;

    LockState stateBeforeUpdate = InterlockedExchangeAdd((LONG *)&m_state, -(LONG)WaiterCountIncrement);
    _ASSERTE(stateBeforeUpdate.HasAnyWaiters());
}

FORCEINLINE bool AwareLock::LockState::InterlockedTry_LockAndUnregisterWaiterAndObserveWakeSignal()
{
    WRAPPER_NO_CONTRACT;

    // This function is called from the waiter's spin loop and should observe the wake signal only if the lock is taken, to
    // prevent a lock releaser from waking another waiter while one is already spinning to acquire the lock
    LockState state = *this;
    while (true)
    {
        _ASSERTE(state.HasAnyWaiters());
        _ASSERTE(state.IsWaiterSignaledToWake());
        if (state.IsLocked())
        {
            return false;
        }

        LockState newState = state;
        newState.InvertIsLocked();
        newState.InvertIsWaiterSignaledToWake();
        newState.DecrementWaiterCount();

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return true;
        }

        state = stateBeforeUpdate;
    }
}

FORCEINLINE bool AwareLock::LockState::InterlockedObserveWakeSignal_Try_LockAndUnregisterWaiter()
{
    WRAPPER_NO_CONTRACT;

    // This function is called at the end of the waiter's spin loop. It must observe the wake signal always, and if the lock is
    // available, it must acquire the lock and unregister the waiter. If the lock is available, a waiter must acquire the lock
    // along with observing the wake signal, because a lock releaser does not wake a waiter when a waiter was signaled but the
    // wake signal has not been observed.

    LockState stateBeforeUpdate = InterlockedExchangeAdd((LONG *)&m_state, -(LONG)IsWaiterSignaledToWakeMask);
    _ASSERTE(stateBeforeUpdate.IsWaiterSignaledToWake());
    if (stateBeforeUpdate.IsLocked())
    {
        return false;
    }

    LockState state = stateBeforeUpdate;
    state.InvertIsWaiterSignaledToWake();
    _ASSERTE(!state.IsLocked());
    do
    {
        _ASSERTE(state.HasAnyWaiters());
        LockState newState = state;
        newState.InvertIsLocked();
        newState.DecrementWaiterCount();

        LockState stateBeforeUpdate = CompareExchange(newState, state);
        if (stateBeforeUpdate == state)
        {
            return true;
        }

        state = stateBeforeUpdate;
    } while (!state.IsLocked());
    return false;
}

FORCEINLINE void AwareLock::SpinWait(DWORD spinCount)
{
    LIMITED_METHOD_CONTRACT;

    _ASSERTE(g_SystemInfo.dwNumberOfProcessors != 1);
    _ASSERTE(spinCount != 0);
    _ASSERTE(spinCount <= g_SpinConstants.dwMaximumDuration);

    do
    {
        YieldProcessor();
    } while (--spinCount != 0);
}

FORCEINLINE bool AwareLock::TryEnterHelper(Thread* pCurThread)
{
    CONTRACTL{
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    if (m_lockState.InterlockedTryLock())
    {
        m_HoldingThread = pCurThread;
        m_Recursion = 1;
        pCurThread->IncLockCount();
        return true;
    }

    if (GetOwningThread() == pCurThread) /* monitor is held, but it could be a recursive case */
    {
        m_Recursion++;
        return true;
    }
    return false;
}

FORCEINLINE AwareLock::EnterHelperResult AwareLock::TryEnterBeforeSpinLoopHelper(Thread *pCurThread)
{
    CONTRACTL{
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    LockState state = m_lockState;

    // Check the recursive case once before the spin loop. If it's not the recursive case in the beginning, it will not
    // be in the future, so the spin loop can avoid checking the recursive case.
    if (!state.IsLocked() || GetOwningThread() != pCurThread)
    {
        // Not a recursive enter, try to acquire the lock or register the spinner
        EnterHelperResult result = m_lockState.InterlockedTry_LockOrRegisterSpinner(state);
        if (result != EnterHelperResult_Entered)
        {
            // EnterHelperResult_Contention: Lock was not acquired and the spinner was registered
            // EnterHelperResult_UseSlowPath: Reached the maximum number of spinners, just wait
            return result;
        }

        // Lock was acquired and the spinner was not registered
        m_HoldingThread = pCurThread;
        m_Recursion = 1;
        pCurThread->IncLockCount();
        return EnterHelperResult_Entered;
    }

    // Recursive enter
    m_Recursion++;
    return EnterHelperResult_Entered;
}

FORCEINLINE bool AwareLock::TryEnterInsideSpinLoopHelper(Thread *pCurThread)
{
    CONTRACTL{
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    // Try to acquire the lock and unregister the spinner. The recursive case is not checked here because
    // TryEnterBeforeSpinLoopHelper() would have taken care of that case before the spin loop.
    if (!m_lockState.InterlockedTry_LockAndUnregisterSpinner())
    {
        // Lock was not acquired and the spinner was not unregistered
        return false;
    }

    // Lock was acquired and spinner was unregistered
    m_HoldingThread = pCurThread;
    m_Recursion = 1;
    pCurThread->IncLockCount();
    return true;
}

FORCEINLINE bool AwareLock::TryEnterAfterSpinLoopHelper(Thread *pCurThread)
{
    CONTRACTL{
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    // Unregister the spinner and try to acquire the lock. A spinner must not unregister itself without trying to acquire the
    // lock because a lock releaser does not wake a waiter when a spinner can acquire the lock.
    if (!m_lockState.InterlockedUnregisterSpinner_TryLock())
    {
        // Spinner was unregistered and the lock was not acquired
        return false;
    }

    // Spinner was unregistered and the lock was acquired
    m_HoldingThread = pCurThread;
    m_Recursion = 1;
    pCurThread->IncLockCount();
    return true;
}

FORCEINLINE AwareLock::EnterHelperResult ObjHeader::EnterObjMonitorHelper(Thread* pCurThread)
{
    CONTRACTL{
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    } CONTRACTL_END;

    LONG oldValue = m_SyncBlockValue.LoadWithoutBarrier();

    if ((oldValue & (BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX +
                     BIT_SBLK_SPIN_LOCK +
                     SBLK_MASK_LOCK_THREADID +
                     SBLK_MASK_LOCK_RECLEVEL)) == 0)
    {
        DWORD tid = pCurThread->GetThreadId();
        if (tid > SBLK_MASK_LOCK_THREADID)
        {
            return AwareLock::EnterHelperResult_UseSlowPath;
        }

        LONG newValue = oldValue | tid;
        if (InterlockedCompareExchangeAcquire((LONG*)&m_SyncBlockValue, newValue, oldValue) == oldValue)
        {
            pCurThread->IncLockCount();
            return AwareLock::EnterHelperResult_Entered;
        }

        return AwareLock::EnterHelperResult_Contention;
    }

    if (oldValue & BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX)
    {
        // If we have a hash code already, we need to create a sync block
        if (oldValue & BIT_SBLK_IS_HASHCODE)
        {
            return AwareLock::EnterHelperResult_UseSlowPath;
        }

        SyncBlock *syncBlock = g_pSyncTable[oldValue & MASK_SYNCBLOCKINDEX].m_SyncBlock;
        _ASSERTE(syncBlock != NULL);
        if (syncBlock->m_Monitor.TryEnterHelper(pCurThread))
        {
            return AwareLock::EnterHelperResult_Entered;
        }

        return AwareLock::EnterHelperResult_Contention;
    }

    // The header is transitioning - treat this as if the lock was taken
    if (oldValue & BIT_SBLK_SPIN_LOCK)
    {
        return AwareLock::EnterHelperResult_Contention;
    }

    // Here we know we have the "thin lock" layout, but the lock is not free.
    // It could still be the recursion case - compare the thread id to check
    if (pCurThread->GetThreadId() != (DWORD)(oldValue & SBLK_MASK_LOCK_THREADID))
    {
        return AwareLock::EnterHelperResult_Contention;
    }

    // Ok, the thread id matches, it's the recursion case.
    // Bump up the recursion level and check for overflow
    LONG newValue = oldValue + SBLK_LOCK_RECLEVEL_INC;

    if ((newValue & SBLK_MASK_LOCK_RECLEVEL) == 0)
    {
        return AwareLock::EnterHelperResult_UseSlowPath;
    }

    if (InterlockedCompareExchangeAcquire((LONG*)&m_SyncBlockValue, newValue, oldValue) == oldValue)
    {
        return AwareLock::EnterHelperResult_Entered;
    }

    // Use the slow path instead of spinning. The compare-exchange above would not fail often, and it's not worth forcing the
    // spin loop that typically follows the call to this function to check the recursive case, so just bail to the slow path.
    return AwareLock::EnterHelperResult_UseSlowPath;
}

// Helper encapsulating the core logic for releasing monitor. Returns what kind of 
// follow up action is necessary. This is FORCEINLINE to make it provide a very efficient implementation.
FORCEINLINE AwareLock::LeaveHelperAction AwareLock::LeaveHelper(Thread* pCurThread)
{
    CONTRACTL {
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    } CONTRACTL_END;

    if (m_HoldingThread != pCurThread)
        return AwareLock::LeaveHelperAction_Error;

    _ASSERTE(m_lockState.IsLocked());
    _ASSERTE(m_Recursion >= 1);

#if defined(_DEBUG) && defined(TRACK_SYNC) && !defined(CROSSGEN_COMPILE)
    // The best place to grab this is from the ECall frame
    Frame   *pFrame = pCurThread->GetFrame();
    int      caller = (pFrame && pFrame != FRAME_TOP ? (int) pFrame->GetReturnAddress() : -1);
    pCurThread->m_pTrackSync->LeaveSync(caller, this);
#endif

    if (--m_Recursion == 0)
    {
        m_HoldingThread->DecLockCount();
        m_HoldingThread = NULL;

        // Clear lock bit and determine whether we must signal a waiter to wake
        if (!m_lockState.InterlockedUnlock())
        {
            return AwareLock::LeaveHelperAction_None;
        }

        // There is a waiter and we must signal a waiter to wake
        return AwareLock::LeaveHelperAction_Signal;
    }
    return AwareLock::LeaveHelperAction_None;
}

// Helper encapsulating the core logic for releasing monitor. Returns what kind of 
// follow up action is necessary. This is FORCEINLINE to make it provide a very efficient implementation.
FORCEINLINE AwareLock::LeaveHelperAction ObjHeader::LeaveObjMonitorHelper(Thread* pCurThread)
{
    CONTRACTL {
        SO_TOLERANT;
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    } CONTRACTL_END;

    DWORD syncBlockValue = m_SyncBlockValue.LoadWithoutBarrier();

    if ((syncBlockValue & (BIT_SBLK_SPIN_LOCK + BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX)) == 0)
    {
        if ((syncBlockValue & SBLK_MASK_LOCK_THREADID) != pCurThread->GetThreadId())
        {
            // This thread does not own the lock.
            return AwareLock::LeaveHelperAction_Error;                
        }

        if (!(syncBlockValue & SBLK_MASK_LOCK_RECLEVEL))
        {
            // We are leaving the lock
            DWORD newValue = (syncBlockValue & (~SBLK_MASK_LOCK_THREADID));
            if (InterlockedCompareExchangeRelease((LONG*)&m_SyncBlockValue, newValue, syncBlockValue) != (LONG)syncBlockValue)
            {
                return AwareLock::LeaveHelperAction_Yield;
            }
            pCurThread->DecLockCount();
        }
        else
        {
            // recursion and ThinLock
            DWORD newValue = syncBlockValue - SBLK_LOCK_RECLEVEL_INC;
            if (InterlockedCompareExchangeRelease((LONG*)&m_SyncBlockValue, newValue, syncBlockValue) != (LONG)syncBlockValue)
            {
                return AwareLock::LeaveHelperAction_Yield;
            }
        }

        return AwareLock::LeaveHelperAction_None;
    }

    if ((syncBlockValue & (BIT_SBLK_SPIN_LOCK + BIT_SBLK_IS_HASHCODE)) == 0)
    {
        _ASSERTE((syncBlockValue & BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX) != 0);
        SyncBlock *syncBlock = g_pSyncTable[syncBlockValue & MASK_SYNCBLOCKINDEX].m_SyncBlock;
        _ASSERTE(syncBlock != NULL);
        return syncBlock->m_Monitor.LeaveHelper(pCurThread);
    }

    if (syncBlockValue & BIT_SBLK_SPIN_LOCK)
    {
        return AwareLock::LeaveHelperAction_Contention;        
    }

    // This thread does not own the lock.
    return AwareLock::LeaveHelperAction_Error;
}

#endif // DACCESS_COMPILE

// Provide access to the object associated with this awarelock, so client can
// protect it.
inline OBJECTREF AwareLock::GetOwningObject()
{
    LIMITED_METHOD_CONTRACT;
    SUPPORTS_DAC;

    // gcc on mac needs these intermediate casts to avoid some ambiuous overloading in the DAC case
    PTR_SyncTableEntry table = SyncTableEntry::GetSyncTableEntry();
    return (OBJECTREF)(Object*)(PTR_Object)table[(m_dwSyncIndex & ~SyncBlock::SyncBlockPrecious)].m_Object;
}

#endif  // _SYNCBLK_INL_
