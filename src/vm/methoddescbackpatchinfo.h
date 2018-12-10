// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "debugmacrosext.h"

typedef SHash<PtrSetSHashTraits<LoaderAllocator *>> LoaderAllocatorSet;

#ifdef FEATURE_TIERED_COMPILATION

#define DISABLE_COPY(T) \
    T(const T &) = delete; \
    T &operator =(const T &) = delete

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EntryPointSlotsToBackpatch

class EntryPointSlotsToBackpatch
{
public:
    enum SlotType : UINT8
    {
        SlotType_Normal,
        SlotType_IsVtableSlot,
        SlotType_IsExecutable,
        SlotType_IsExecutable_IsRelativeToEndOfSlot,

        SlotType_Count,
        SlotType_Mask = SlotType_IsVtableSlot | SlotType_IsExecutable | SlotType_IsExecutable_IsRelativeToEndOfSlot
    };

private:
    typedef SArray<TADDR> SlotArray;

private:
    SlotArray m_slots;

public:
    EntryPointSlotsToBackpatch()
    {
        LIMITED_METHOD_CONTRACT;
    }

#ifndef DACCESS_COMPILE
public:
    void AddSlot_Locked(TADDR slot, SlotType slotType);
    void Backpatch_Locked(PCODE entryPoint);
    static void Backpatch_Locked(TADDR slot, SlotType slotType, PCODE entryPoint);
#endif

    DISABLE_COPY(EntryPointSlotsToBackpatch);
};

class MethodDescEntryPointSlotsToBackpatch
{
private:
    MethodDesc *m_methodDesc;
    EntryPointSlotsToBackpatch m_slots;

public:
    MethodDescEntryPointSlotsToBackpatch(MethodDesc *methodDesc) : m_methodDesc(methodDesc)
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(methodDesc != nullptr);
    }

public:
    MethodDesc *GetMethodDesc() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_methodDesc;
    }

#ifndef DACCESS_COMPILE
    EntryPointSlotsToBackpatch *GetSlots()
    {
        WRAPPER_NO_CONTRACT;
        _ASSERTE(m_methodDesc != nullptr);

        return &m_slots;
    }
#endif

    DISABLE_COPY(MethodDescEntryPointSlotsToBackpatch);
};

class MethodDescEntryPointSlotsToBackpatchHashTraits
    : public DeleteElementsOnDestructSHashTraits<NoRemoveSHashTraits<DefaultSHashTraits<MethodDescEntryPointSlotsToBackpatch *>>>
{
public:
    typedef DeleteElementsOnDestructSHashTraits<NoRemoveSHashTraits<DefaultSHashTraits<MethodDescEntryPointSlotsToBackpatch *>>> Base;
    typedef Base::element_t element_t;
    typedef Base::count_t count_t;

    typedef MethodDesc *key_t;

    static key_t GetKey(element_t e)
    {
        LIMITED_METHOD_CONTRACT;
        return e->GetMethodDesc();
    }

    static BOOL Equals(key_t k1, key_t k2)
    {
        LIMITED_METHOD_CONTRACT;
        return k1 == k2;
    }

    static count_t Hash(key_t k)
    {
        LIMITED_METHOD_CONTRACT;
        return (count_t)((size_t)dac_cast<TADDR>(k) >> 2);
    }

    static const element_t Null() { LIMITED_METHOD_CONTRACT; return nullptr; }
    static bool IsNull(const element_t &e) { LIMITED_METHOD_CONTRACT; return e == nullptr; }
};

typedef SHash<MethodDescEntryPointSlotsToBackpatchHashTraits> MethodDescEntryPointSlotsToBackpatchHash;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MethodDescBackpatchInfo

class MethodDescBackpatchInfo
{
private:
    MethodDesc *m_methodDesc;

    // This field and its data is protected by MethodDescBackpatchInfoTracker's lock. Entry point slots that need to be
    // backpatched when the method's entry point changes. This may include vtable slots, and slots from virtual stub dispatch
    // for interface methods (slots from dispatch stubs and resolve cache entries). This collection only contains slots
    // associated with this MethodDesc's LoaderAllocator.
    EntryPointSlotsToBackpatch m_slotsToBackpatch;

    // This field is protected by MethodDescBackpatchInfoTracker's lock. This is a set of LoaderAllocators that are dependent on
    // this MethodDesc's LoaderAllocator, which have inherited this MethodDesc and have slots that need to be backpatched when
    // the method's entry point changes.
    LoaderAllocatorSet *m_dependentLoaderAllocatorsWithSlotsToBackpatch;

public:
    MethodDescBackpatchInfo(MethodDesc *methodDesc = nullptr);

#ifndef DACCESS_COMPILE
public:
    ~MethodDescBackpatchInfo()
    {
        LIMITED_METHOD_CONTRACT;

        LoaderAllocatorSet *set = m_dependentLoaderAllocatorsWithSlotsToBackpatch;
        if (set != nullptr)
        {
            delete set;
        }
    }
#endif

public:
    MethodDesc *GetMethodDesc() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_methodDesc;
    }

#ifndef DACCESS_COMPILE
public:
    EntryPointSlotsToBackpatch *GetSlotsToBackpatch()
    {
        WRAPPER_NO_CONTRACT;
        _ASSERTE(m_methodDesc != nullptr);

        return &m_slotsToBackpatch;
    }

public:
    template<class Visit> void ForEachDependentLoaderAllocatorWithSlotsToBackpatch_Locked(Visit visit);
    void AddDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(LoaderAllocator *dependentLoaderAllocator);
    void RemoveDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(LoaderAllocator *dependentLoaderAllocator);
#endif

    DISABLE_COPY(MethodDescBackpatchInfo);
};

class MethodDescBackpatchInfoHashTraits
    : public DeleteElementsOnDestructSHashTraits<NoRemoveSHashTraits<DefaultSHashTraits<MethodDescBackpatchInfo *>>>
{
public:
    typedef DeleteElementsOnDestructSHashTraits<NoRemoveSHashTraits<DefaultSHashTraits<MethodDescBackpatchInfo *>>> Base;
    typedef Base::element_t element_t;
    typedef Base::count_t count_t;

    typedef MethodDesc *key_t;

    static key_t GetKey(element_t e)
    {
        LIMITED_METHOD_CONTRACT;
        return e->GetMethodDesc();
    }

    static BOOL Equals(key_t k1, key_t k2)
    {
        LIMITED_METHOD_CONTRACT;
        return k1 == k2;
    }

    static count_t Hash(key_t k)
    {
        LIMITED_METHOD_CONTRACT;
        return (count_t)((size_t)dac_cast<TADDR>(k) >> 2);
    }

    static const element_t Null() { LIMITED_METHOD_CONTRACT; return nullptr; }
    static bool IsNull(const element_t &e) { LIMITED_METHOD_CONTRACT; return e == nullptr; }
};

typedef SHash<MethodDescBackpatchInfoHashTraits> MethodDescBackpatchInfoHash;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MethodDescBackpatchInfoTracker

class MethodDescBackpatchInfoTracker
{
private:
    static CrstStatic s_lock;

    // This field and some of its data is protected by s_lock
    MethodDescBackpatchInfoHash m_backpatchInfoHash;

#ifndef DACCESS_COMPILE
public:
    static void StaticInitialize();
#endif

#ifdef _DEBUG
public:
    static bool IsLockedByCurrentThread();
#endif

public:
    class ConditionalLockHolder : CrstHolderWithState
    {
    public:
        ConditionalLockHolder(bool acquireLock = true)
            : CrstHolderWithState(
#ifndef DACCESS_COMPILE
                acquireLock ? &MethodDescBackpatchInfoTracker::s_lock : nullptr
#else
                nullptr
#endif
                )
        {
            LIMITED_METHOD_CONTRACT;
        }
    };

public:
    MethodDescBackpatchInfoTracker()
    {
        LIMITED_METHOD_CONTRACT;
    }

#ifdef _DEBUG
public:
    static bool IsTieredVtableMethod(PTR_MethodDesc methodDesc);
#endif

#ifndef DACCESS_COMPILE
public:
    MethodDescBackpatchInfo *GetBackpatchInfo_Locked(MethodDesc *methodDesc) const
    {
        WRAPPER_NO_CONTRACT;
        _ASSERTE(IsLockedByCurrentThread());
        _ASSERTE(methodDesc != nullptr);
        _ASSERTE(IsTieredVtableMethod(methodDesc));

        return m_backpatchInfoHash.Lookup(methodDesc);
    }

    MethodDescBackpatchInfo *GetOrAddBackpatchInfo_Locked(MethodDesc *methodDesc)
    {
        WRAPPER_NO_CONTRACT;
        _ASSERTE(IsLockedByCurrentThread());
        _ASSERTE(methodDesc != nullptr);
        _ASSERTE(IsTieredVtableMethod(methodDesc));

        MethodDescBackpatchInfo *backpatchInfo = m_backpatchInfoHash.Lookup(methodDesc);
        if (backpatchInfo != nullptr)
        {
            return backpatchInfo;
        }
        return AddBackpatchInfo_Locked(methodDesc);
    }

private:
    MethodDescBackpatchInfo *AddBackpatchInfo_Locked(MethodDesc *methodDesc);
#endif

    friend class ConditionalLockHolder;

    DISABLE_COPY(MethodDescBackpatchInfoTracker);
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Inline and template definitions

#ifndef DACCESS_COMPILE

inline void EntryPointSlotsToBackpatch::AddSlot_Locked(TADDR slot, SlotType slotType)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(slot != NULL);
    _ASSERTE(IS_ALIGNED((SIZE_T)slot, sizeof(void *)));
    _ASSERTE(!(slot & SlotType_Mask));
    _ASSERTE(slotType < SlotType_Count);

    m_slots.Append(slot | slotType);
}

#endif // DACCESS_COMPILE

inline MethodDescBackpatchInfo::MethodDescBackpatchInfo(MethodDesc *methodDesc)
    : m_methodDesc(methodDesc), m_dependentLoaderAllocatorsWithSlotsToBackpatch(nullptr)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(methodDesc == nullptr || MethodDescBackpatchInfoTracker::IsTieredVtableMethod(PTR_MethodDesc(methodDesc)));
}

#ifndef DACCESS_COMPILE

template<class Visit>
inline void MethodDescBackpatchInfo::ForEachDependentLoaderAllocatorWithSlotsToBackpatch_Locked(Visit visit)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(m_methodDesc != nullptr);

    LoaderAllocatorSet *set = m_dependentLoaderAllocatorsWithSlotsToBackpatch;
    if (set == nullptr)
    {
        return;
    }

    for (LoaderAllocatorSet::Iterator it = set->Begin(), itEnd = set->End(); it != itEnd; ++it)
    {
        visit(*it);
    }
}

#endif // DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#undef DISABLE_COPY

#endif // FEATURE_TIERED_COMPILATION
