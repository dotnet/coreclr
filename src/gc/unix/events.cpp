// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <cstdint>
#include <cstddef>
#include <cassert>
#include <memory>
#include <mutex>
#include <pthread.h>
#include <errno.h>
#include "config.h"

#ifndef __out_z
#define __out_z
#endif // __out_z

#include "gcenv.structs.h"
#include "gcenv.base.h"
#include "gcenv.os.h"
#include "globals.h"

#if HAVE_MACH_ABSOLUTE_TIME
mach_timebase_info_data_t g_TimebaseInfo;
#endif // MACH_ABSOLUTE_TIME

namespace
{

#if HAVE_PTHREAD_CONDATTR_SETCLOCK
void TimeSpecAdd(timespec* time, uint32_t milliseconds)
{
    uint64_t nsec = time->tv_nsec + (uint64_t)milliseconds * tccMilliSecondsToNanoSeconds;
    if (nsec >= tccSecondsToNanoSeconds)
    {
        time->tv_sec += nsec / tccSecondsToNanoSeconds;
        nsec %= tccSecondsToNanoSeconds;
    }

    time->tv_nsec = nsec;
}
#endif // HAVE_PTHREAD_CONDATTR_SETCLOCK

#if HAVE_MACH_ABSOLUTE_TIME
// Convert nanoseconds to the timespec structure
// Parameters:
//  nanoseconds - time in nanoseconds to convert
//  t           - the target timespec structure
void NanosecondsToTimeSpec(uint64_t nanoseconds, timespec* t)
{
    t->tv_sec = nanoseconds / tccSecondsToNanoSeconds;
    t->tv_nsec = nanoseconds % tccSecondsToNanoSeconds;
}
#endif // HAVE_PTHREAD_CONDATTR_SETCLOCK

class UnixEvent : public GCEvent
{
    pthread_cond_t m_condition;
    pthread_mutex_t m_mutex;
    bool m_manualReset;
    bool m_state;
    bool m_isValid;

public:

    UnixEvent(bool manualReset, bool initialState)
    : m_manualReset(manualReset),
      m_state(initialState),
      m_isValid(false)
    {
    }

    bool Initialize()
    {
        pthread_condattr_t attrs;
        int st = pthread_condattr_init(&attrs);
        if (st != 0)
        {
            assert(!"Failed to initialize UnixEvent condition attribute");
            return false;
        }

        //PthreadCondAttrHolder attrsHolder(&attrs);

#if HAVE_PTHREAD_CONDATTR_SETCLOCK && !HAVE_MACH_ABSOLUTE_TIME
        // Ensure that the pthread_cond_timedwait will use CLOCK_MONOTONIC
        st = pthread_condattr_setclock(&attrs, CLOCK_MONOTONIC);
        if (st != 0)
        {
            assert(!"Failed to set UnixEvent condition variable wait clock");
            return false;
        }
#endif // HAVE_PTHREAD_CONDATTR_SETCLOCK && !HAVE_MACH_ABSOLUTE_TIME

        st = pthread_mutex_init(&m_mutex, NULL);
        if (st != 0)
        {
            assert(!"Failed to initialize UnixEvent mutex");
            return false;
        }

        st = pthread_cond_init(&m_condition, &attrs);
        if (st != 0)
        {
            assert(!"Failed to initialize UnixEvent condition variable");

            st = pthread_mutex_destroy(&m_mutex);
            assert(st == 0 && "Failed to destroy UnixEvent mutex");
            return false;
        }

        m_isValid = true;

        return true;
    }

    void CloseEvent() override
    {
        if (m_isValid)
        {
            int st = pthread_mutex_destroy(&m_mutex);
            assert(st == 0 && "Failed to destroy UnixEvent mutex");

            st = pthread_cond_destroy(&m_condition);
            assert(st == 0 && "Failed to destroy UnixEvent condition variable");
        }
    }

    uint32_t Wait(uint32_t milliseconds, bool alertable) override
    {
        UNREFERENCED_PARAMETER(alertable);

        timespec endTime;
#if HAVE_MACH_ABSOLUTE_TIME
        uint64_t endMachTime;
        if (milliseconds != INFINITE)
        {
            uint64_t nanoseconds = (uint64_t)milliseconds * tccMilliSecondsToNanoSeconds;
            NanosecondsToTimeSpec(nanoseconds, &endTime);
            endMachTime = mach_absolute_time() + nanoseconds * g_TimebaseInfo.denom / g_TimebaseInfo.numer;
        }
#elif HAVE_PTHREAD_CONDATTR_SETCLOCK
        if (milliseconds != INFINITE)
        {
            clock_gettime(CLOCK_MONOTONIC, &endTime);
            TimeSpecAdd(&endTime, milliseconds);
        }
#else
#error Don't know how to perfom timed wait on this platform
#endif

        int st = 0;

        pthread_mutex_lock(&m_mutex);
        while (!m_state)
        {
            if (milliseconds == INFINITE)
            {
                st = pthread_cond_wait(&m_condition, &m_mutex);
            }
            else
            {
#if HAVE_MACH_ABSOLUTE_TIME
                // Since OSX doesn't support CLOCK_MONOTONIC, we use relative variant of the 
                // timed wait and we need to handle spurious wakeups properly.
                st = pthread_cond_timedwait_relative_np(&m_condition, &m_mutex, &endTime);
                if ((st == 0) && !m_state)
                {
                    uint64_t machTime = mach_absolute_time();
                    if (machTime < endMachTime)
                    {
                        // The wake up was spurious, recalculate the relative endTime
                        uint64_t remainingNanoseconds = (endMachTime - machTime) * g_TimebaseInfo.numer / g_TimebaseInfo.denom;
                        NanosecondsToTimeSpec(remainingNanoseconds, &endTime);
                    }
                    else
                    {
                        // Although the timed wait didn't report a timeout, time calculated from the
                        // mach time shows we have already reached the end time. It can happen if
                        // the wait was spuriously woken up right before the timeout.
                        st = ETIMEDOUT;
                    }
                }
#else // HAVE_MACH_ABSOLUTE_TIME
                st = pthread_cond_timedwait(&m_condition, &m_mutex, &endTime);
#endif // HAVE_MACH_ABSOLUTE_TIME
                // Verify that if the wait timed out, the event was not set
                assert((st != ETIMEDOUT) || !m_state);
            }

            if (st != 0)
            {
                // wait failed or timed out
                break;
            }
        }

        if ((st == 0) && !m_manualReset)
        {
            // Clear the state for auto-reset events so that only one waiter gets released
            m_state = false;
        }

        pthread_mutex_unlock(&m_mutex);

        uint32_t waitStatus;

        if (st == 0)
        {
            waitStatus = WAIT_OBJECT_0;
        }
        else if (st == ETIMEDOUT)
        {
            waitStatus = WAIT_TIMEOUT;
        }
        else
        {
            waitStatus = WAIT_FAILED;
        }

        return waitStatus;
    }

    void Set() override
    {
        pthread_mutex_lock(&m_mutex);
        m_state = true;
        pthread_mutex_unlock(&m_mutex);

        // Unblock all threads waiting for the condition variable
        pthread_cond_broadcast(&m_condition);
    }

    void Reset() override
    {
        pthread_mutex_lock(&m_mutex);
        m_state = false;
        pthread_mutex_unlock(&m_mutex);
    }
};

} // anonymous namespace

GCEvent* GCToOSInterface::CreateAutoEvent(bool initialState)
{
    // [DESKTOP TODO] The difference between events and OS events is
    // whether or not the hosting API is made aware of them. When (if)
    // we implement hosting support for Local GC, we will need to be
    // aware of the host here.
    return CreateOSAutoEvent(initialState);
}

GCEvent* GCToOSInterface::CreateManualEvent(bool initialState)
{
    // [DESKTOP TODO] The difference between events and OS events is
    // whether or not the hosting API is made aware of them. When (if)
    // we implement hosting support for Local GC, we will need to be
    // aware of the host here.
    return CreateOSManualEvent(initialState);
}

GCEvent* GCToOSInterface::CreateOSAutoEvent(bool initialState)
{
    std::unique_ptr<UnixEvent> event(new (std::nothrow) UnixEvent(false, initialState));
    if (!event)
    {
        return nullptr;
    }

    if (!event->Initialize())
    {
        return nullptr;
    }

    return event.release();
}

GCEvent* GCToOSInterface::CreateOSManualEvent(bool initialState)
{
    std::unique_ptr<UnixEvent> event(new (std::nothrow) UnixEvent(true, initialState));
    if (!event)
    {
        return nullptr;
    }

    if (!event->Initialize())
    {
        return nullptr;
    }

    return event.release();
}


