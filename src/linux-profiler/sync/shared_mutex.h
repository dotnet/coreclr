#ifndef _SHARED_MUTEX_H_
#define _SHARED_MUTEX_H_

#include <memory>

class shared_mutex
{
public:
    shared_mutex();

    shared_mutex(const shared_mutex&) = delete;

    ~shared_mutex();

    shared_mutex &operator=(const shared_mutex&) = delete;

    void lock();

    bool try_lock();

    void unlock();

    void lock_shared();

    bool try_lock_shared();

    void unlock_shared();

private:
    struct impl;
    std::unique_ptr<impl> pimpl;
};

#endif // _SHARED_MUTEX_H_
