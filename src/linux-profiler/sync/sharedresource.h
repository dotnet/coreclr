#ifndef _SHARED_RESOURCE_H_
#define _SHARED_RESOURCE_H_

#include "shared_mutex.h"

template<typename T, typename Mutex = shared_mutex>
class SharedResource
{
private:
    template<class SR>
    class AccessorBase
    {
    public:
        ~AccessorBase() = default;

        AccessorBase(const AccessorBase&) = delete;

        AccessorBase &operator=(const AccessorBase&) = delete;

        AccessorBase(AccessorBase &&other) :
            m_shared_resource(other.m_shared_resource)
        {
            other.m_shared_resource = nullptr;
        }

        AccessorBase &operator=(AccessorBase &&other)
        {
            if (&other != this)
            {
                m_shared_resource = other.m_shared_resource;
                other.m_shared_resource = nullptr;
            }
            return *this;
        }

        bool isValid() const noexcept
        {
            return m_shared_resource != nullptr;
        }

    protected:
        SR *m_shared_resource; // Mutable or constant pointer to SharedResource.

        AccessorBase(SR *resource)
            : m_shared_resource(resource)
        {}
    };

    template<class SR>
    class ExclusiveAccessor : public AccessorBase<SR>
    {
    public:
        ~ExclusiveAccessor()
        {
            if (this->isValid())
            {
                this->m_shared_resource->m_mutex.unlock();
            }
        }

        ExclusiveAccessor(ExclusiveAccessor &&other) = default;

        ExclusiveAccessor &operator=(ExclusiveAccessor &&other) = default;

    protected:
        ExclusiveAccessor(SR *resource)
            : AccessorBase<SR>(resource)
        {
            this->m_shared_resource->m_mutex.lock();
        }
    };

    template<class SR>
    class SharedAccessor : public AccessorBase<SR>
    {
    public:
        ~SharedAccessor()
        {
            if (this->isValid())
            {
                this->m_shared_resource->m_mutex.unlock_shared();
            }
        }

        SharedAccessor(SharedAccessor &&other) = default;

        SharedAccessor &operator=(SharedAccessor &&other) = default;

    protected:
        SharedAccessor(SR *resource)
            : AccessorBase<SR>(resource)
        {
            this->m_shared_resource->m_mutex.lock_shared();
        }
    };

public:
    template<class A>
    class MutableAccessor : public A
    {
        friend class SharedResource<T, Mutex>;

    public:
        T *operator->()
        {
            return &this->m_shared_resource->m_resource;
        }

        T &operator*()
        {
            return this->m_shared_resource->m_resource;
        }

        using A::A; // Protected constructor.
    };

    template<class A>
    class ConstAccessor : public A
    {
        friend class SharedResource<T, Mutex>;

    public:
        const T *operator->() const
        {
            return &this->m_shared_resource->m_resource;
        }

        const T &operator*() const
        {
            return this->m_shared_resource->m_resource;
        }

        using A::A; // Protected constructor.
    };

    template<typename ...Args>
    SharedResource(Args&& ...args)
        : m_resource(std::forward<Args>(args)...)
    {}

    ~SharedResource() = default;

    SharedResource(const SharedResource&) = delete;

    SharedResource(SharedResource&&) = delete;

    SharedResource &operator=(const SharedResource&) = delete;

    SharedResource &operator=(SharedResource&&) = delete;

    // Should not be used in concurent environment.
    T *get() const noexcept
    {
        return const_cast<T*>(&m_resource);
    }

    auto lock() ->
        MutableAccessor<ExclusiveAccessor<SharedResource<T, Mutex>>>
    {
        // Implicit conversion to accessor with mutable exclusive lock.
        return this;
    }

    auto lock_const() const ->
        ConstAccessor<ExclusiveAccessor<const SharedResource<T, Mutex>>>
    {
        // Implicit conversion to accessor with constant exclusive lock.
        return this;
    }

    auto lock_shared() const ->
        ConstAccessor<SharedAccessor<const SharedResource<T, Mutex>>>
    {
        // Implicit conversion to accessor with constant shared lock.
        return this;
    }

private:
    T             m_resource;
    mutable Mutex m_mutex;
};

#endif //_SHARED_RESOURCE_H_
