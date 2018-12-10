// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "excep.h"
#include "log.h"
#include "tieredcompilation.h"
#include "methoddescbackpatchinfo.h"

#ifdef FEATURE_TIERED_COMPILATION

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EntryPointSlotsToBackpatch

#ifndef DACCESS_COMPILE

void EntryPointSlotsToBackpatch::Backpatch_Locked(PCODE entryPoint)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
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
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
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
// MethodDescBackpatchInfo

#ifndef DACCESS_COMPILE

void MethodDescBackpatchInfo::AddDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(LoaderAllocator *dependentLoaderAllocator)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
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

void MethodDescBackpatchInfo::RemoveDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(
    LoaderAllocator *dependentLoaderAllocator)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(MethodDescBackpatchInfoTracker::IsLockedByCurrentThread());
    _ASSERTE(m_methodDesc != nullptr);
    _ASSERTE(dependentLoaderAllocator != nullptr);
    _ASSERTE(dependentLoaderAllocator != m_methodDesc->GetLoaderAllocator());
    _ASSERTE(m_dependentLoaderAllocatorsWithSlotsToBackpatch != nullptr);
    _ASSERTE(m_dependentLoaderAllocatorsWithSlotsToBackpatch->Lookup(dependentLoaderAllocator) == dependentLoaderAllocator);

    m_dependentLoaderAllocatorsWithSlotsToBackpatch->Remove(dependentLoaderAllocator);
}

#endif // !DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MethodDescBackpatchInfoTracker

CrstStatic MethodDescBackpatchInfoTracker::s_lock;

#ifndef DACCESS_COMPILE

void MethodDescBackpatchInfoTracker::StaticInitialize()
{
    WRAPPER_NO_CONTRACT;
    s_lock.Init(CrstMethodDescBackpatchInfoTracker);
}

#endif // DACCESS_COMPILE

#ifdef _DEBUG

bool MethodDescBackpatchInfoTracker::IsLockedByCurrentThread()
{
    WRAPPER_NO_CONTRACT;

#ifndef DACCESS_COMPILE
    return !!s_lock.OwnedByCurrentThread();
#else
    return true;
#endif
}

bool MethodDescBackpatchInfoTracker::IsTieredVtableMethod(PTR_MethodDesc methodDesc)
{
    // The only purpose of this method is to allow asserts in inline functions defined in the .h file, by which time MethodDesc
    // is not fully defined

    WRAPPER_NO_CONTRACT;
    return methodDesc->IsTieredVtableMethod();
}

#endif // _DEBUG

#ifndef DACCESS_COMPILE

MethodDescBackpatchInfo *MethodDescBackpatchInfoTracker::AddBackpatchInfo_Locked(MethodDesc *methodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(IsLockedByCurrentThread());
    _ASSERTE(methodDesc != nullptr);
    _ASSERTE(methodDesc->IsTieredVtableMethod());
    _ASSERTE(m_backpatchInfoHash.Lookup(methodDesc) == nullptr);

    NewHolder<MethodDescBackpatchInfo> backpatchInfoHolder = new MethodDescBackpatchInfo(methodDesc);
    m_backpatchInfoHash.Add(backpatchInfoHolder);
    return backpatchInfoHolder.Extract();
}

EntryPointSlotsToBackpatch *MethodDescBackpatchInfoTracker::GetDependencyMethodDescEntryPointSlotsToBackpatch_Locked(
    MethodDesc *methodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(IsLockedByCurrentThread());
    _ASSERTE(methodDesc != nullptr);
    _ASSERTE(methodDesc->IsTieredVtableMethod());

    MethodDescEntryPointSlotsToBackpatch *methodDescSlots =
        m_dependencyMethodDescEntryPointSlotsToBackpatchHash.Lookup(methodDesc);
    return methodDescSlots == nullptr ? nullptr : methodDescSlots->GetSlots();
}

EntryPointSlotsToBackpatch *MethodDescBackpatchInfoTracker::GetOrAddDependencyMethodDescEntryPointSlotsToBackpatch_Locked(
    MethodDesc *methodDesc)
{
    WRAPPER_NO_CONTRACT;
    _ASSERTE(IsLockedByCurrentThread());
    _ASSERTE(methodDesc != nullptr);
    _ASSERTE(methodDesc->IsTieredVtableMethod());

    MethodDescEntryPointSlotsToBackpatch *methodDescSlots =
        m_dependencyMethodDescEntryPointSlotsToBackpatchHash.Lookup(methodDesc);
    if (methodDescSlots != nullptr)
    {
        return methodDescSlots->GetSlots();
    }

    NewHolder<MethodDescEntryPointSlotsToBackpatch> methodDescSlotsHolder =
        new MethodDescEntryPointSlotsToBackpatch(methodDesc);
    m_dependencyMethodDescEntryPointSlotsToBackpatchHash.Add(methodDescSlotsHolder);
    return methodDescSlotsHolder.Extract()->GetSlots();
}

void MethodDescBackpatchInfoTracker::ClearDependencyMethodDescEntryPointSlotsToBackpatchHash()
{
    WRAPPER_NO_CONTRACT;

    ConditionalLockHolder lockHolder;

    for (MethodDescEntryPointSlotsToBackpatchHash::Iterator
            it = m_dependencyMethodDescEntryPointSlotsToBackpatchHash.Begin(),
            itEnd = m_dependencyMethodDescEntryPointSlotsToBackpatchHash.End();
        it != itEnd;
        ++it)
    {
        MethodDesc *methodDesc = (*it)->GetMethodDesc();
        MethodDescBackpatchInfo *backpatchInfo = methodDesc->GetBackpatchInfoTracker()->GetBackpatchInfo_Locked(methodDesc);
        if (backpatchInfo != nullptr)
        {
            backpatchInfo->RemoveDependentLoaderAllocatorsWithSlotsToBackpatch_Locked(this);
        }
    }

    m_dependencyMethodDescEntryPointSlotsToBackpatchHash.RemoveAll();
}

#endif // DACCESS_COMPILE

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // FEATURE_TIERED_COMPILATION
