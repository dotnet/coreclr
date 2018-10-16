// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "excep.h"
#include "log.h"
#include "tieredcompilation.h"
#include "methoddescvirtualinfo.h"

#ifdef FEATURE_TIERED_COMPILATION

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EntryPointSlotsToBackpatch

#ifndef DACCESS_COMPILE

void EntryPointSlotsToBackpatch::Backpatch_Locked(PCODE entryPoint)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescVirtualInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(entryPoint != NULL);

    TADDR *slots = m_slots.GetElements();
    COUNT_T slotCount = m_slots.GetCount();
    for (COUNT_T i = 0; i < slotCount; ++i)
    {
        TADDR slot = slots[i];
        SlotType slotType = (SlotType)(slot & SlotType_Mask);
        slot ^= slotType;
        Backpatch_Locked(slot, slotType, entryPoint);
    }
}

void EntryPointSlotsToBackpatch::Backpatch_Locked(TADDR slot, SlotType slotType, PCODE entryPoint)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescVirtualInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(slot != NULL);
    _ASSERTE(IS_ALIGNED((SIZE_T)slot, sizeof(void *)));
    _ASSERTE(!(slot & SlotType_Mask));
    _ASSERTE(slotType < SlotType_Count);
    _ASSERTE(entryPoint != NULL);

    switch (slotType)
    {
        case SlotType_Normal:
            *(PCODE *)slot = entryPoint;
            break;

        case SlotType_IsVtableSlot:
            ((MethodTable::VTableIndir2_t *)slot)->SetValue(entryPoint);
            break;

        case SlotType_IsExecutable:
            *(PCODE *)slot = entryPoint;
            goto Flush;

        case SlotType_IsExecutable_IsRelativeToEndOfSlot:
            *(PCODE *)slot = entryPoint - ((PCODE)slot + sizeof(PCODE));
            // fall through

        Flush:
            ClrFlushInstructionCache((LPCVOID)slot, sizeof(PCODE));
            break;

        default:
            UNREACHABLE();
            break;
    }
}

#endif // !DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MethodDescVirtualInfo

#ifndef DACCESS_COMPILE

void MethodDescVirtualInfo::AddDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(LoaderAllocator *dependentLoaderAllocator)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescVirtualInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(m_methodDesc != nullptr);
    _ASSERTE(dependentLoaderAllocator != nullptr);
    _ASSERTE(dependentLoaderAllocator != m_methodDesc->GetLoaderAllocator());

    LoaderAllocatorSet *set = m_dependentLoaderAllocatorsWithSlotsToBackpatch;
    if (set != nullptr)
    {
        if (set->Lookup(dependentLoaderAllocator) != nullptr)
        {
            return;
        }
        set->Add(dependentLoaderAllocator);
        return;
    }

    NewHolder<LoaderAllocatorSet> setHolder = new LoaderAllocatorSet();
    setHolder->Add(dependentLoaderAllocator);
    m_dependentLoaderAllocatorsWithSlotsToBackpatch = setHolder.Extract();
}

void MethodDescVirtualInfo::RemoveDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(
    LoaderAllocator *dependentLoaderAllocator)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescVirtualInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(m_methodDesc != nullptr);
    _ASSERTE(dependentLoaderAllocator != nullptr);
    _ASSERTE(dependentLoaderAllocator != m_methodDesc->GetLoaderAllocator());
    _ASSERTE(m_dependentLoaderAllocatorsWithSlotsToBackpatch != nullptr);
    _ASSERTE(m_dependentLoaderAllocatorsWithSlotsToBackpatch->Lookup(dependentLoaderAllocator) == dependentLoaderAllocator);

    m_dependentLoaderAllocatorsWithSlotsToBackpatch->Remove(dependentLoaderAllocator);
}

#endif // !DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MethodDescVirtualInfoTracker

CrstStatic MethodDescVirtualInfoTracker::s_lock;

#ifndef DACCESS_COMPILE

void MethodDescVirtualInfoTracker::StaticInitialize()
{
    WRAPPER_NO_CONTRACT;
    s_lock.Init(CrstMethodDescVirtualInfoTracker);
}

#endif // DACCESS_COMPILE

#ifdef _DEBUG

bool MethodDescVirtualInfoTracker::IsLockedByCurrentThread()
{
    WRAPPER_NO_CONTRACT;

#ifndef DACCESS_COMPILE
    return !!s_lock.OwnedByCurrentThread();
#else
    return true;
#endif
}

bool MethodDescVirtualInfoTracker::IsTieredVtableMethod(PTR_MethodDesc methodDesc)
{
    // The only purpose of this method is to allow asserts in inline functions defined in the .h file, by which time MethodDesc
    // is not fully defined

    WRAPPER_NO_CONTRACT;
    return methodDesc->IsTieredVtableMethod();
}

#endif // _DEBUG

#ifndef DACCESS_COMPILE

MethodDescVirtualInfo *MethodDescVirtualInfoTracker::AddVirtualInfo_Locked(MethodDesc *methodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(IsLockedByCurrentThread());
    _ASSERTE(methodDesc != nullptr);
    _ASSERTE(methodDesc->IsTieredVtableMethod());
    _ASSERTE(m_virtualInfoHash.Lookup(methodDesc) == nullptr);

    NewHolder<MethodDescVirtualInfo> virtualInfoHolder = new MethodDescVirtualInfo(methodDesc);
    m_virtualInfoHash.Add(virtualInfoHolder);
    return virtualInfoHolder.Extract();
}

#endif // DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // FEATURE_TIERED_COMPILATION
