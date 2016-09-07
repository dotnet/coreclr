#ifndef _RING_BUFFER_H_
#define _RING_BUFFER_H_

#include <atomic>
#include <limits>
#include <stdexcept>
#include <new>
#include <utility>

#include <stdlib.h>
#include <assert.h>

template<typename T>
class ring_buffer
{
public:
    explicit ring_buffer(size_t capacity = 0)
    {
        initialize(capacity);
    }

    ring_buffer(const ring_buffer &rb)
    {
        initialize(rb.m_cap);
        try
        {
            copy_from(rb);
        }
        catch (...)
        {
            ~ring_buffer();
            throw;
        }
    }

    ring_buffer(ring_buffer &&rb)
    {
        initialize();
        swap(rb);
    }

    ~ring_buffer()
    {
        clear();
        free(m_buf);
    }

    ring_buffer& operator=(const ring_buffer& other)
    {
        if (this == &other)
            return *this;

        ring_buffer tmp(other);
        swap(tmp);
        return *this;
    }

    ring_buffer& operator=(ring_buffer&& other)
    {
        swap(other);
        return *this;
    }

    T &front() noexcept
    {
        assert(!empty());
        return *m_begin;
    }

    const T &front() const noexcept
    {
        assert(!empty());
        return const_cast<const T&>(*m_begin);
    }

    T &back() noexcept
    {
        assert(!empty());
        return const_cast<T&>(const_cast<const ring_buffer<T>&>(*this).back());
    }

    const T &back() const noexcept
    {
        assert(!empty());

        const T *end = m_end;
        if (end == m_buf)
            end = m_buf + m_cap;
        --end;
        return *end;
    }

    bool empty() const noexcept
    {
        return m_size == 0;
    }

    bool full() const noexcept
    {
        return m_size == m_cap;
    }

    size_t size() const noexcept
    {
        return m_size;
    }

    size_t max_size() const noexcept
    {
        static size_t max_size = std::numeric_limits<size_t>::max() / sizeof(T);
        return max_size;
    }

    void reserve(size_t new_capacity)
    {
        if (new_capacity <= m_cap)
            return;

        ring_buffer tmp(new_capacity);
        tmp.move_from(std::move(*this));
        swap(tmp);
    }

    size_t capacity() const noexcept
    {
        return m_cap;
    }

    void clear() noexcept
    {
        size_t size = m_size;
        while (size > 0)
        {
            m_begin->~T();
            ++m_begin;
            if (m_begin == m_buf + m_cap)
                m_begin = m_buf;
            --size;
        }
        m_size = 0;
    }

    void push_back(const T &item)
    {
        push_back_imp(item);
    }

    void push_back(T &&item)
    {
        push_back_imp(std::move(item));
    }

    void push_front(const T &item)
    {
        push_front_imp(item);
    }

    void push_front(T &&item)
    {
        push_front_imp(std::move(item));
    }

    void pop_back()
    {
        if (m_size == 0)
            return;

        if (m_end == m_buf)
            m_end = m_buf + m_cap;
        --m_end;
        m_end->~T();
        m_size--;
    }

    void pop_front()
    {
        if (m_size == 0)
            return;

        m_begin->~T();
        ++m_begin;
        if (m_begin == m_buf + m_cap)
            m_begin = m_buf;
        m_size--;
    }

    void swap(ring_buffer &rb) noexcept
    {
        std::swap(m_buf,   rb.m_buf);
        std::swap(m_begin, rb.m_begin);
        std::swap(m_end,   rb.m_end);
        std::swap(m_cap,   rb.m_cap);
        rb.m_size = m_size.exchange(rb.m_size);
    }

private:
    void initialize() noexcept
    {
#ifdef _TARGET_AMD64_
        assert(m_size.is_lock_free()); // NOTE: With C++17 it can be checked
                                       // staticaly.
#endif // _TARGET_AMD64_
        m_begin = m_end = m_buf = nullptr;
        m_size = m_cap = 0;
    }

    void initialize(size_t capacity)
    {
        initialize();

        if (capacity == 0)
            return;
        else if (capacity > max_size())
            throw std::length_error("capacity exceeds the maximum size");

        m_buf = reinterpret_cast<T*>(malloc(capacity * sizeof(T)));

        if (m_buf == nullptr)
            throw std::bad_alloc();

        m_cap = capacity;
        m_begin = m_end = m_buf;
    }

    void copy_from(const ring_buffer &rb)
    {
        assert(
            m_begin == m_buf   &&
            m_end   == m_buf   &&
            m_size  == 0        &&
            m_cap  >=  rb.m_cap
        );

        T *begin = rb.m_begin;
        size_t size = 0;
        while (size != rb.m_size)
        {
            new (m_end) T(*begin);
            ++m_end;
            ++begin;
            if (begin == rb.m_buf + rb.m_cap)
                begin = rb.m_buf;
            ++size;
        }
        m_size = size;
    }

    void move_from(ring_buffer &&rb)
    {
        assert(
            m_begin == m_buf   &&
            m_end   == m_buf   &&
            m_size  == 0        &&
            m_cap  >=  rb.m_cap
        );

        T *begin = rb.m_begin;
        size_t size = 0;
        while (size != rb.m_size)
        {
            new (m_end) T(std::move(*begin));
            ++m_end;
            ++begin;
            if (begin == rb.m_buf + rb.m_cap)
                begin = rb.m_buf;
            ++size;
        }
        m_size = size;
    }

    template <typename ValT>
    void push_back_imp(ValT &&item)
    {
        if (full())
            throw std::out_of_range("ring_buffer capacity is exhausted");

        new (m_end) T(std::forward<ValT>(item));
        ++m_end;
        if (m_end == m_buf + m_cap)
            m_end = m_buf;
        m_size++;
    }

    template <typename ValT>
    void push_front_impl(ValT &&item)
    {
        if (full())
            throw std::out_of_range("ring_buffer capacity is exhausted");

        T *begin = m_begin;
        if (begin == m_buf)
            begin = m_buf + m_cap;
        --begin;
        new (begin) T(std::forward<ValT>(item));
        m_begin = begin;
        m_size++;
    }

private:
    T *m_buf;
    T *m_begin;
    T *m_end;
    std::atomic_size_t m_size;
    size_t m_cap;
};

#endif /* _RING_BUFFER_H_ */
