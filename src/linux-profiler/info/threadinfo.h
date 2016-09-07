#ifndef _THREAD_INFO_H_
#define _THREAD_INFO_H_

#include <atomic>

#include <signal.h>
#include <pthread.h>

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "stackchannel.h"
#include "mappedinfo.h"

struct ThreadInfo : public MappedInfo<ThreadID>
{
    DWORD                 osThreadId;
    pthread_t             nativeHandle;

    StackChannel          stackChannel;
    std::atomic_ulong     genTicks;
    ULONG                 fixTicks;
    volatile sig_atomic_t interruptible;
    // TODO: other useful stuff.

    ThreadInfo()
        : osThreadId(0)
        , nativeHandle()
        , stackChannel()
        , genTicks(0)
        , fixTicks(0)
        , interruptible(false)
    {}
};

#endif // _THREAD_INFO_H_
