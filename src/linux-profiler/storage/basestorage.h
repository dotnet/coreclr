#ifndef _BASE_STORAGE_H_
#define _BASE_STORAGE_H_

#include <type_traits>
#include <deque>

#include "baseinfo.h"

template<typename INFO>
class BaseStorage
{
public:
    typedef std::deque<INFO> Container;
    typedef typename Container::iterator iterator;
    typedef typename Container::const_iterator const_iterator;

    static_assert(std::is_base_of<BaseInfo, INFO>::value,
        "INFO not derived from BaseInfo");

    bool HasValue(InternalID id) const noexcept
    {
        return id.id >= m_storage.size();
    }

    INFO &Get(InternalID id)
    {
        return const_cast<INFO&>(
            const_cast<const BaseStorage<INFO>&>(*this).Get(id));
    }

    const INFO &Get(InternalID id) const
    {
        _ASSERTE(this->HasValue(id));
        return m_storage[id.id];
    }

    INFO &Add()
    {
        m_storage.emplace_back();
        INFO &info = m_storage.back();
        info.internalId.id = m_storage.size() - 1;
        return info;
    }

    iterator begin() noexcept
    {
        return m_storage.begin();
    }

    const_iterator begin() const noexcept
    {
        return m_storage.begin();
    }

    iterator end() noexcept
    {
        return m_storage.end();
    }

    const_iterator end() const noexcept
    {
        return m_storage.end();
    }

protected:
    Container m_storage;
};

#endif // _BASE_STORAGE_H_
