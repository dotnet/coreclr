#include <system_error>

#include <pthread.h>
#include <errno.h>

#include "shared_mutex.h"

struct shared_mutex::impl
{
    pthread_rwlock_t rwlock;
};

shared_mutex::shared_mutex()
    : pimpl(new impl())
{
    if (pthread_rwlock_init(&pimpl->rwlock, NULL))
    {
        throw std::system_error(errno, std::system_category(),
            "can't create shared_mutex");
    }
}

shared_mutex::~shared_mutex()
{
    pthread_rwlock_destroy(&pimpl->rwlock);
}

void shared_mutex::lock()
{
    if (pthread_rwlock_wrlock(&pimpl->rwlock))
    {
        throw std::system_error(errno, std::system_category(),
            "can't lock shared_mutex");
    }
}

bool shared_mutex::try_lock()
{
    int err = pthread_rwlock_trywrlock(&pimpl->rwlock);

    if (err && err != EBUSY)
    {
        throw std::system_error(errno, std::system_category(),
            "can't exclusively lock shared_mutex");
    }

    return err == 0;
}

void shared_mutex::unlock()
{
    if (pthread_rwlock_unlock(&pimpl->rwlock))
    {
        throw std::system_error(errno, std::system_category(),
            "can't exclusively unlock shared_mutex");
    }
}

void shared_mutex::lock_shared()
{
    if (pthread_rwlock_rdlock(&pimpl->rwlock))
    {
        throw std::system_error(errno, std::system_category(),
            "can't shared lock shared_mutex");
    }
}

bool shared_mutex::try_lock_shared()
{
    int err = pthread_rwlock_tryrdlock(&pimpl->rwlock);

    if (err && err != EBUSY)
    {
        throw std::system_error(errno, std::system_category(),
            "can't shared lock shared_mutex");
    }

    return err == 0;
}

void shared_mutex::unlock_shared()
{
    if (pthread_rwlock_unlock(&pimpl->rwlock))
    {
        throw std::system_error(errno, std::system_category(),
            "can't shared unlock shared_mutex");
    }
}
