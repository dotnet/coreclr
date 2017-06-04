#ifndef _MAPPED_STORAGE_H_
#define _MAPPED_STORAGE_H_

#include <unordered_map>
#include <utility>
#include <functional>

#include "basestorage.h"
#include "mappedinfo.h"

template<typename ID, typename INFO>
class MappedStorage : public BaseStorage<INFO>
{
public:
    static_assert(std::is_base_of<MappedInfo<ID>, INFO>::value,
        "INFO not derived from MappedInfo<ID>");

    using BaseStorage<INFO>::HasValue;

    bool HasValue(ID id) const
    {
        return m_toInternal.find(id) != m_toInternal.end();
    }

    using BaseStorage<INFO>::Get;

    INFO &Get(ID id)
    {
        return const_cast<INFO&>(
            const_cast<const MappedStorage<ID, INFO>&>(*this).Get(id));
    }

    const INFO &Get(ID id) const
    {
        auto it = m_toInternal.find(id);
        _ASSERTE(it != m_toInternal.end());
        return m_storage[it->second.id];
    }

    // Add and/or get info by ID. Second value in pair denoting whether
    // the insertion took place.
    std::pair<INFO&, bool> Place(ID id)
    {
        auto it = m_toInternal.find(id);
        if (it != m_toInternal.end())
        {
            return std::make_pair(std::ref(m_storage[it->second.id]), false);
        }
        else
        {
            INFO &info = this->Add();
            try
            {
                info.id = id;
                m_toInternal[id] = info.internalId;
            }
            catch (...)
            {
                m_storage.pop_back(); // New value is always appended.
                throw;
            }
            return std::make_pair(std::ref(info), true);
        }
    }

    // Remap object accessible from iid to another ID. Returns old ID.
    // Should not be called if stirage doesn't have iid.
    ID Link(ID id, InternalID iid)
    {
        _ASSERTE(this->HasValue(iid));
        INFO &info = this->Get(iid);
        ID old_id = info.id;
        m_toInternal[id] = iid;
        info.id = id;
        return old_id;
    }

    // Remove ID from storage, so this ID can be used for another object later.
    // Only ID is removed. Associated object stays accessible from internal ID.
    // Function returns reference to this object.
    // Should not be called if storage doesn't have ID.
    INFO &Unlink(ID id)
    {
        auto it = m_toInternal.find(id);
        _ASSERTE(it != m_toInternal.end());
        INFO &info = m_storage[it->second.id];
        m_toInternal.erase(it);
        info.id = ID{};
        return info;
    }

protected:
    using BaseStorage<INFO>::m_storage;
    std::unordered_map<ID, InternalID> m_toInternal;
};

#endif // _MAPPED_STORAGE_H_
