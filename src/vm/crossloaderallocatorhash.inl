// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef CROSSLOADERALLOCATORHASH_INL
#define CROSSLOADERALLOCATORHASH_INL
#ifdef CROSSLOADERALLOCATORHASH_H
#ifndef CROSSGEN_COMPILE

#include "gcheaphashtable.inl"

template <class TKey_, class TValue_>
/*static*/ DWORD NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::ComputeUsedEntries(OBJECTREF &keyValueStore, DWORD *pEntriesInArrayTotal)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    DWORD entriesInArrayTotal = (((I1ARRAYREF)keyValueStore)->GetNumComponents() - sizeof(TKey))/sizeof(TValue);
    DWORD usedEntries;
    TValue* pStartOfValuesData = (TValue*)(((I1ARRAYREF)keyValueStore)->GetDirectPointerToNonObjectElements() + sizeof(TKey));

    if (entriesInArrayTotal == 0)
    {
        usedEntries = 0;
    }
    else if ((entriesInArrayTotal >= 2) && (pStartOfValuesData[entriesInArrayTotal - 2] == (TValue)0))
    {
        usedEntries = (DWORD)pStartOfValuesData[entriesInArrayTotal - 1];
    }
    else if (pStartOfValuesData[entriesInArrayTotal - 1] == (TValue)0)
    {
        usedEntries = entriesInArrayTotal - 1;
    }
    else
    {
        usedEntries = entriesInArrayTotal;
    }

    *pEntriesInArrayTotal = entriesInArrayTotal;
    return usedEntries;
}

#ifndef DACCESS_COMPILE
template <class TKey_, class TValue_>
/*static*/ void NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::SetUsedEntries(TValue* pStartOfValuesData, DWORD entriesInArrayTotal, DWORD usedEntries)
{
    if (usedEntries < entriesInArrayTotal)
    {
        if (usedEntries == (entriesInArrayTotal - 1))
        {
            pStartOfValuesData[entriesInArrayTotal - 1] = (TValue)0;
        }
        else
        {
            pStartOfValuesData[entriesInArrayTotal - 1] = (TValue)(usedEntries);
            pStartOfValuesData[entriesInArrayTotal - 2] = (TValue)0;
        }
    }
}

template <class TKey_, class TValue_>
/*static*/ void NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::AddToValuesInHeapMemory(OBJECTREF &keyValueStore, OBJECTREF &newKeyValueStore, const TKey& key, const TValue& value)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    static_assert(sizeof(TKey)==sizeof(TValue), "Assume keys and values are the same size");

    if (keyValueStore == NULL)
    {
        newKeyValueStore = AllocatePrimitiveArray(ELEMENT_TYPE_I1, (value == NULL) ? sizeof(TKey) : sizeof(TKey) + sizeof(TValue), FALSE);
        TKey* pKeyLoc = (TKey*)((I1ARRAYREF)newKeyValueStore)->GetDirectPointerToNonObjectElements();
        *pKeyLoc = key;
        if (value != NULL)
        {
            BYTE* pInitialValueLocLoc = (BYTE*)((I1ARRAYREF)newKeyValueStore)->GetDirectPointerToNonObjectElements();
            TValue* pValueLoc = (TValue*)(((I1ARRAYREF)newKeyValueStore)->GetDirectPointerToNonObjectElements() + sizeof(TKey));
            *pValueLoc = value;
        }
    }
    else if (value != NULL)
    {
        DWORD entriesInArrayTotal;
        DWORD usedEntries = ComputeUsedEntries(keyValueStore, &entriesInArrayTotal);
        TValue* pStartOfValuesData = (TValue*)(((I1ARRAYREF)keyValueStore)->GetDirectPointerToNonObjectElements() + sizeof(TKey));

        if (usedEntries == entriesInArrayTotal)
        {
            // There isn't free space. Build a new, bigger array with the existing data 
            DWORD newSize;
            if (usedEntries < 8)
                newSize = usedEntries + 1; // Grow very slowly initially. The cost of allocation/copy is cheap, and this holds very tight on memory usage
            else
                newSize = usedEntries * 2;

            if (newSize < usedEntries)
                COMPlusThrow(kOverflowException);

            newKeyValueStore = AllocatePrimitiveArray(ELEMENT_TYPE_I1, newSize*sizeof(TValue) + sizeof(TKey), FALSE);
            void* pStartOfNewArray = ((I1ARRAYREF)newKeyValueStore)->GetDirectPointerToNonObjectElements();
            void* pStartOfOldArray = ((I1ARRAYREF)keyValueStore)->GetDirectPointerToNonObjectElements();

            memcpyNoGCRefs(pStartOfNewArray, pStartOfOldArray, ((I1ARRAYREF)keyValueStore)->GetNumComponents());

            keyValueStore = newKeyValueStore;
            pStartOfValuesData = (TValue*)(((BYTE*)pStartOfNewArray) + sizeof(TKey));
            entriesInArrayTotal = newSize;
        }

        // There is free space. Append on the end
        SetUsedEntries(pStartOfValuesData, entriesInArrayTotal, usedEntries + 1);
        pStartOfValuesData[usedEntries] = value;
    }
}
#endif //!DACCESS_COMPILE

template <class TKey_, class TValue_>
/*static*/ TKey_ NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::ReadKeyFromKeyValueStore(OBJECTREF *pKeyValueStore)
{
    WRAPPER_NO_CONTRACT;

    TKey* pKeyLoc = (TKey*)((I1ARRAYREF)*pKeyValueStore)->GetDirectPointerToNonObjectElements();
    return *pKeyLoc;
}

template <class TKey_, class TValue_>
template <class Visitor>
/*static*/ bool NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::VisitKeyValueStore(OBJECTREF *pLoaderAllocatorRef, OBJECTREF *pKeyValueStore, Visitor &visitor)
{
    WRAPPER_NO_CONTRACT;

    DWORD entriesInArrayTotal;
    DWORD usedEntries = ComputeUsedEntries(*pKeyValueStore, &entriesInArrayTotal);

    for (DWORD index = 0; index < usedEntries; ++index)
    {
        // Capture pKeyLoc and pStartOfValuesData inside of loop, as we aren't protecting these pointers into the GC heap, so they
        // are not permitted to live across the call to visitor (in case visitor triggers a GC)
        TKey* pKeyLoc = (TKey*)((I1ARRAYREF)*pKeyValueStore)->GetDirectPointerToNonObjectElements();
        TValue* pStartOfValuesData = (TValue*)(((I1ARRAYREF)*pKeyValueStore)->GetDirectPointerToNonObjectElements() + sizeof(TKey));

        if (!visitor(*pLoaderAllocatorRef, *pKeyLoc, pStartOfValuesData[index]))
        {
            return false;
        }
    }

    return true;
}

#ifndef DACCESS_COMPILE
template <class TKey_, class TValue_>
/*static*/ void DefaultCrossLoaderAllocatorHashTraits<TKey_, TValue_>::DeleteValueInHeapMemory(OBJECTREF keyValueStore, const TValue& value)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    // TODO: Consider optimizing this by changing the add to ensure that the 
    // values list is sorted, and then doing a binary search for the value instead
    // of the linear search

    DWORD entriesInArrayTotal;
    DWORD usedEntries = NoRemoveDefaultCrossLoaderAllocatorHashTraits<TKey,TValue>::ComputeUsedEntries(keyValueStore, &entriesInArrayTotal);
    TValue* pStartOfValuesData = (TValue*)(((I1ARRAYREF)keyValueStore)->GetDirectPointerToNonObjectElements() + sizeof(TKey));

    for (DWORD iEntry = 0; iEntry < usedEntries; iEntry++)
    {
        if (pStartOfValuesData[iEntry] == value)
        {
            memmove(pStartOfValuesData + iEntry, pStartOfValuesData + iEntry + 1, (usedEntries - iEntry - 1) * sizeof(TValue));
            SetUsedEntries(pStartOfValuesData, entriesInArrayTotal, usedEntries - 1);
            return;
        }
    }
}
#endif //!DACCESS_COMPILE

/*static*/ inline INT32 GCHeapHashDependentHashTrackerHashTraits::Hash(PtrTypeKey *pValue)
{
    LIMITED_METHOD_CONTRACT;
    return (INT32)*pValue;
}

/*static*/ inline INT32 GCHeapHashDependentHashTrackerHashTraits::Hash(PTRARRAYREF arr, INT32 index)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    LAHASHDEPENDENTHASHTRACKERREF value = (LAHASHDEPENDENTHASHTRACKERREF)arr->GetAt(index);
    LoaderAllocator *pLoaderAllocator = value->GetLoaderAllocatorUnsafe();
    return Hash(&pLoaderAllocator);
}

/*static*/ inline bool GCHeapHashDependentHashTrackerHashTraits::DoesEntryMatchKey(PTRARRAYREF arr, INT32 index, PtrTypeKey *pKey)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    LAHASHDEPENDENTHASHTRACKERREF value = (LAHASHDEPENDENTHASHTRACKERREF)arr->GetAt(index);

    return value->IsTrackerFor(*pKey);
}

/*static*/ inline bool GCHeapHashDependentHashTrackerHashTraits::IsDeleted(PTRARRAYREF arr, INT32 index, GCHEAPHASHOBJECTREF gcHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    OBJECTREF valueInHeap = arr->GetAt(index);

    if (valueInHeap == NULL)
        return false;

    if (gcHeap == valueInHeap)
        return true;

    // This is a tricky bit of logic used which detects freed loader allocators lazily
    // and deletes them from the GCHeapHash while looking up or otherwise walking the hashtable
    // for any purpose.
    LAHASHDEPENDENTHASHTRACKERREF value = (LAHASHDEPENDENTHASHTRACKERREF)valueInHeap;
    if (!value->IsLoaderAllocatorLive())
    {
#ifndef DACCESS_COMPILE
        arr->SetAt(index, gcHeap);
        gcHeap->DecrementCount(true);
#endif // DACCESS_COMPILE

        return true;
    }

    return false;
}

template<class TRAITS>
template <class TKey>
/*static*/ INT32 KeyToValuesGCHeapHashTraits<TRAITS>::Hash(TKey *pValue)
{
    LIMITED_METHOD_CONTRACT;
    return (INT32)(DWORD)*pValue;
}

template<class TRAITS>
/*static*/ inline INT32 KeyToValuesGCHeapHashTraits<TRAITS>::Hash(PTRARRAYREF arr, INT32 index)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    OBJECTREF hashKeyEntry = arr->GetAt(index);
    LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
    OBJECTREF keyValueStore;

    if (hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
    {
        hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)hashKeyEntry;
        keyValueStore = hashKeyToTrackers->_laLocalKeyValueStore;
    }
    else
    {
        keyValueStore = hashKeyEntry;
    }

    typename TRAITS::TKey key = TRAITS::ReadKeyFromKeyValueStore(&keyValueStore);
    return Hash(&key);
}

template<class TRAITS>
template<class TKey>
/*static*/ bool KeyToValuesGCHeapHashTraits<TRAITS>::DoesEntryMatchKey(PTRARRAYREF arr, INT32 index, TKey *pKey)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    OBJECTREF hashKeyEntry = arr->GetAt(index);
    LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
    OBJECTREF keyValueStore;

    if (hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
    {
        hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)hashKeyEntry;
        keyValueStore = hashKeyToTrackers->_laLocalKeyValueStore;
    }
    else
    {
        keyValueStore = hashKeyEntry;
    }

    TKey key = TRAITS::ReadKeyFromKeyValueStore(&keyValueStore);

    return key == *pKey;
}

#ifndef DACCESS_COMPILE
template <class TRAITS>
void CrossLoaderAllocatorHash<TRAITS>::Add(TKey key, TValue value, LoaderAllocator *pLoaderAllocatorOfValue)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;


    struct {
        KeyToValuesGCHeapHash keyToTrackersHash;
        KeyToValuesGCHeapHash keyToValuePerLAHash;
        OBJECTREF keyValueStore;
        OBJECTREF newKeyValueStore;
        OBJECTREF objRefNull;
        OBJECTREF hashKeyEntry;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    GCPROTECT_BEGIN(gc)
    {
        EnsureManagedObjectsInitted();

        bool addToKeyValuesHash = false;
        // This data structure actually doesn't have this invariant, but it is expected that uses of this
        // data structure will require that the key's loader allocator is the same as that of this data structure.
        _ASSERTE(key->GetLoaderAllocator() == _loaderAllocator);

        gc.keyToTrackersHash = KeyToValuesGCHeapHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(KeyToDependentTrackersHash));
        INT32 index = gc.keyToTrackersHash.GetValueIndex(&key);

        if (index == -1)
        {
            addToKeyValuesHash = true;
            TRAITS::AddToValuesInHeapMemory(gc.keyValueStore, gc.newKeyValueStore, key, pLoaderAllocatorOfValue == _loaderAllocator ? value : NULL);
            gc.keyValueStore = gc.newKeyValueStore;

            if (pLoaderAllocatorOfValue != _loaderAllocator)
            {
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)AllocateObject(MscorlibBinder::GetClass(CLASS__LAHASHKEYTOTRACKERS));
                SetObjectReference(&gc.hashKeyToTrackers->_laLocalKeyValueStore, gc.newKeyValueStore, GetAppDomain());
                gc.hashKeyEntry = gc.hashKeyToTrackers;
            }
            else
            {
                gc.hashKeyEntry = gc.newKeyValueStore;
            }

            gc.keyToTrackersHash.Add(&key, [&gc](PTRARRAYREF arr, INT32 index)
            {
                arr->SetAt(index, (OBJECTREF)gc.hashKeyEntry);
            });
        }
        else
        {
            gc.keyToTrackersHash.GetElement(index, gc.hashKeyEntry);

            if (gc.hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
            {
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)gc.hashKeyEntry;
                gc.keyValueStore = gc.hashKeyToTrackers->_laLocalKeyValueStore;
            }
            else
            {
                gc.keyValueStore = gc.hashKeyEntry;
            }

            if (pLoaderAllocatorOfValue == _loaderAllocator)
            {
                TRAITS::AddToValuesInHeapMemory(gc.keyValueStore, gc.newKeyValueStore, key, value);
            }

            if (gc.newKeyValueStore != NULL)
            {
                if (gc.hashKeyToTrackers != NULL)
                {
                    SetObjectReference(&gc.hashKeyToTrackers->_laLocalKeyValueStore, gc.newKeyValueStore, GetAppDomain());
                }
                else
                {
                    gc.hashKeyEntry = gc.newKeyValueStore;
                    gc.keyToTrackersHash.SetElement(index, gc.hashKeyEntry);
                }
                gc.keyValueStore = gc.newKeyValueStore;
            }
        }

        // If the LoaderAllocator matches, we've finished adding by now, otherwise, we need to get the remove hash and work with that
        if (pLoaderAllocatorOfValue != _loaderAllocator)
        {
            if (gc.hashKeyToTrackers == NULL)
            {
                // Nothing has yet caused the trackers proxy object to be setup. Create it now, and update the keyToTrackersHash
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)AllocateObject(MscorlibBinder::GetClass(CLASS__LAHASHKEYTOTRACKERS));
                SetObjectReference(&gc.hashKeyToTrackers->_laLocalKeyValueStore, gc.keyValueStore, GetAppDomain());
                gc.hashKeyEntry = gc.hashKeyToTrackers;
                gc.keyToTrackersHash.SetElement(index, gc.hashKeyEntry);
            }

            // Must add it to the cross LA structure
            GCHEAPHASHOBJECTREF gcheapKeyToValue = GetKeyToValueCrossLAHashForHashkeyToTrackers(gc.hashKeyToTrackers, pLoaderAllocatorOfValue);

            gc.keyToValuePerLAHash = KeyToValuesGCHeapHash(gcheapKeyToValue);

            INT32 indexInKeyValueHash = gc.keyToValuePerLAHash.GetValueIndex(&key);
            if (indexInKeyValueHash != -1)
            {
                gc.keyToValuePerLAHash.GetElement(indexInKeyValueHash, gc.keyValueStore);

                TRAITS::AddToValuesInHeapMemory(gc.keyValueStore, gc.newKeyValueStore, key, value);

                if (gc.newKeyValueStore != NULL)
                {
                    gc.keyToValuePerLAHash.SetElement(indexInKeyValueHash, gc.newKeyValueStore);
                }
            }
            else
            {
                TRAITS::AddToValuesInHeapMemory(gc.objRefNull, gc.newKeyValueStore, key, value);

                gc.keyToValuePerLAHash.Add(&key, [&gc](PTRARRAYREF arr, INT32 index)
                {
                    arr->SetAt(index, gc.newKeyValueStore);
                });
            }
        }
    }
    GCPROTECT_END();
}
#endif // !DACCESS_COMPILE

#ifndef DACCESS_COMPILE
template <class TRAITS>
void CrossLoaderAllocatorHash<TRAITS>::Remove(TKey key, TValue value, LoaderAllocator *pLoaderAllocatorOfValue)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    // This data structure actually doesn't have this invariant, but it is expected that uses of this
    // data structure will require that the key's loader allocator is the same as that of this data structure.
    _ASSERTE(key->GetLoaderAllocator() == _loaderAllocator);

    if (KeyToDependentTrackersHash == NULL)
    {
        // If the heap objects haven't been initted, then there is nothing to delete
        return;
    }

    struct {
        KeyToValuesGCHeapHash keyToTrackersHash;
        KeyToValuesGCHeapHash keyToValuePerLAHash;
        OBJECTREF hashKeyEntry;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
        OBJECTREF keyValueStore;
    } gc;

    ZeroMemory(&gc, sizeof(gc));
    GCPROTECT_BEGIN(gc)
    {
        gc.keyToTrackersHash = KeyToValuesGCHeapHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(KeyToDependentTrackersHash));
        INT32 index = gc.keyToTrackersHash.GetValueIndex(&key);

        if (index != -1)
        {
            gc.keyToTrackersHash.GetElement(index, gc.hashKeyEntry);

            if (gc.hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
            {
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)gc.hashKeyEntry;
                gc.keyValueStore = gc.hashKeyToTrackers->_laLocalKeyValueStore;
            }
            else
            {
                gc.keyValueStore = gc.hashKeyEntry;
            }

            // Check to see if value can be added to this data structure directly.
            if (_loaderAllocator == pLoaderAllocatorOfValue)
            {
                TRAITS::DeleteValueInHeapMemory(gc.keyValueStore, value);
            }
            else if (gc.hashKeyToTrackers != NULL)
            {
                // Must remove it from the cross LA structure
                GCHEAPHASHOBJECTREF gcheapKeyToValue = GetKeyToValueCrossLAHashForHashkeyToTrackers(gc.hashKeyToTrackers, pLoaderAllocatorOfValue);

                gc.keyToValuePerLAHash = KeyToValuesGCHeapHash(gcheapKeyToValue);

                INT32 indexInKeyValueHash = gc.keyToValuePerLAHash.GetValueIndex(&key);
                if (indexInKeyValueHash != -1)
                {
                    gc.keyToValuePerLAHash.GetElement(indexInKeyValueHash, gc.keyValueStore);
                    TRAITS::DeleteValueInHeapMemory(gc.keyValueStore, value);
                }
            }
        }
    }
    GCPROTECT_END();
}
#endif // !DACCESS_COMPILE

template <class TRAITS>
template <class Visitor>
bool CrossLoaderAllocatorHash<TRAITS>::VisitValuesOfKey(TKey key, Visitor &visitor)
{
    WRAPPER_NO_CONTRACT;

    class VisitIndividualEntryKeyValueHash
    {
        public:
        TKey _key;
        Visitor *_pVisitor;
        GCHeapHashDependentHashTrackerHash *_pDependentTrackerHash;

        VisitIndividualEntryKeyValueHash(TKey key, Visitor *pVisitor,  GCHeapHashDependentHashTrackerHash *pDependentTrackerHash) : 
            _key(key),
            _pVisitor(pVisitor),
            _pDependentTrackerHash(pDependentTrackerHash)
            {}

        bool operator()(INT32 index)
        {
            WRAPPER_NO_CONTRACT;

            LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
            _pDependentTrackerHash->GetElement(index, dependentTracker);
            return VisitTracker(_key, dependentTracker, *_pVisitor);
        }
    };

    // This data structure actually doesn't have this invariant, but it is expected that uses of this
    // data structure will require that the key's loader allocator is the same as that of this data structure.
    _ASSERTE(key->GetLoaderAllocator() == _loaderAllocator);

    // Check to see that something has been added
    if (KeyToDependentTrackersHash == NULL)
        return true;

    bool result = true;
    struct 
    {
        KeyToValuesGCHeapHash keyToTrackersHash;
        GCHeapHashDependentHashTrackerHash dependentTrackerHash;
        LAHASHDEPENDENTHASHTRACKERREF dependentTrackerMaybe;
        LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
        OBJECTREF hashKeyEntry;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
        OBJECTREF keyValueStore;
        OBJECTREF nullref;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    GCPROTECT_BEGIN(gc)
    {
        gc.keyToTrackersHash = KeyToValuesGCHeapHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(KeyToDependentTrackersHash));
        INT32 index = gc.keyToTrackersHash.GetValueIndex(&key);
        if (index != -1)
        {
            // We have an entry in the hashtable for the key/dependenthandle.
            gc.keyToTrackersHash.GetElement(index, gc.hashKeyEntry);

            if (gc.hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
            {
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)gc.hashKeyEntry;
                gc.keyValueStore = gc.hashKeyToTrackers->_laLocalKeyValueStore;
            }
            else
            {
                gc.keyValueStore = gc.hashKeyEntry;
            }

            // Now gc.hashKeyToTrackers is filled in and keyValueStore

            // visit local entries
            result = VisitKeyValueStore(&gc.nullref, &gc.keyValueStore, visitor);

            if (gc.hashKeyToTrackers != NULL)
            {
                // Is there a single dependenttracker here, or a set.

                if (gc.hashKeyToTrackers->_trackerOrTrackerSet->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHDEPENDENTHASHTRACKER))
                {
                    gc.dependentTracker = (LAHASHDEPENDENTHASHTRACKERREF)gc.hashKeyToTrackers->_trackerOrTrackerSet;
                    result = VisitTracker(key, gc.dependentTracker, visitor);
                }
                else
                {
                    gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash(gc.hashKeyToTrackers->_trackerOrTrackerSet);
                    VisitIndividualEntryKeyValueHash visitIndivididualKeys(key, &visitor, &gc.dependentTrackerHash);
                    result = gc.dependentTrackerHash.VisitAllEntryIndices(visitIndivididualKeys);
                }
            }
        }
    }
    GCPROTECT_END();

    return result;
}

template <class TRAITS>
template <class Visitor>
bool CrossLoaderAllocatorHash<TRAITS>::VisitAllKeyValuePairs(Visitor &visitor)
{
    CONTRACTL
    {
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    class VisitAllEntryKeyToDependentTrackerHash
    {
        public:
        Visitor *_pVisitor;
        KeyToValuesGCHeapHash *_pKeyToTrackerHash;

        VisitAllEntryKeyToDependentTrackerHash(Visitor *pVisitor,  KeyToValuesGCHeapHash *pKeyToTrackerHash) : 
            _pVisitor(pVisitor),
            _pKeyToTrackerHash(pKeyToTrackerHash)
            {}

        bool operator()(INT32 index)
        {
            WRAPPER_NO_CONTRACT;

            OBJECTREF hashKeyEntry;
            _pKeyToTrackerHash->GetElement(index, hashKeyEntry);
            return VisitKeyToTrackerAllEntries(hashKeyEntry, *_pVisitor);
        }
    };

    class VisitAllEntryDependentTrackerHash
    {
        public:
        Visitor *_pVisitor;
        GCHeapHashDependentHashTrackerHash *_pDependentTrackerHash;

        VisitAllEntryDependentTrackerHash(Visitor *pVisitor,  GCHeapHashDependentHashTrackerHash *pDependentTrackerHash) : 
            _pVisitor(pVisitor),
            _pDependentTrackerHash(pDependentTrackerHash)
            {}

        bool operator()(INT32 index)
        {
            WRAPPER_NO_CONTRACT;

            LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
            _pDependentTrackerHash->GetElement(index, dependentTracker);
            return VisitTrackerAllEntries(dependentTracker, *_pVisitor);
        }
    };

    struct 
    {
        KeyToValuesGCHeapHash keyToTrackersHash;
        GCHeapHashDependentHashTrackerHash dependentTrackerHash;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    bool result = true;
    GCPROTECT_BEGIN(gc)
    {
        if (KeyToDependentTrackersHash != NULL)
        {
            // Visit all local entries
            gc.keyToTrackersHash = KeyToValuesGCHeapHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(KeyToDependentTrackersHash));
            VisitAllEntryKeyToDependentTrackerHash visitAllEntryKeys(&visitor, &gc.keyToTrackersHash);
            result = gc.keyToTrackersHash.VisitAllEntryIndices(visitAllEntryKeys);
        }

        if (LAToDependentTrackerHash != NULL)
        {
            // Visit the non-local data
            gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(LAToDependentTrackerHash));
            VisitAllEntryDependentTrackerHash visitDependentTrackers(&visitor, &gc.dependentTrackerHash);
            result = gc.dependentTrackerHash.VisitAllEntryIndices(visitDependentTrackers);
        }
    }
    GCPROTECT_END();

    return result;
}

#ifndef DACCESS_COMPILE
template <class TRAITS>
void CrossLoaderAllocatorHash<TRAITS>::RemoveAll(TKey key)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    class DeleteIndividualEntryKeyValueHash
    {
        public:
        TKey _key;
        GCHeapHashDependentHashTrackerHash *_pDependentTrackerHash;

        DeleteIndividualEntryKeyValueHash(TKey key, GCHeapHashDependentHashTrackerHash *pDependentTrackerHash) : 
            _key(key),
            _pDependentTrackerHash(pDependentTrackerHash)
            {}

        bool operator()(INT32 index)
        {
            WRAPPER_NO_CONTRACT;

            LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
            _pDependentTrackerHash->GetElement(index, dependentTracker);
            DeleteEntryTracker(_key, dependentTracker);
            return true;
        }
    };

    // This data structure actually doesn't have this invariant, but it is expected that uses of this
    // data structure will require that the key's loader allocator is the same as that of this data structure.
    _ASSERTE(key->GetLoaderAllocator() == _loaderAllocator);

    if (KeyToDependentTrackersHash == NULL)
    {
        return; // Nothing was ever added, so removing all is easy
    }

    struct 
    {
        KeyToValuesGCHeapHash keyToTrackersHash;
        GCHeapHashDependentHashTrackerHash dependentTrackerHash;
        LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
        OBJECTREF hashKeyEntry;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
        OBJECTREF keyValueStore;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    GCPROTECT_BEGIN(gc)
    {
        gc.keyToTrackersHash = KeyToValuesGCHeapHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(KeyToDependentTrackersHash));
        INT32 index = gc.keyToTrackersHash.GetValueIndex(&key);
        if (index != -1)
        {
            // We have an entry in the hashtable for the key/dependenthandle.
            gc.keyToTrackersHash.GetElement(index, gc.hashKeyEntry);

            if (gc.hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
            {
                gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)gc.hashKeyEntry;
                gc.keyValueStore = gc.hashKeyToTrackers->_laLocalKeyValueStore;
            }
            else
            {
                gc.keyValueStore = gc.hashKeyEntry;
            }

            // Now gc.hashKeyToTrackers is filled in 

            if (gc.hashKeyToTrackers != NULL)
            {
                // Is there a single dependenttracker here, or a set.

                if (gc.hashKeyToTrackers->_trackerOrTrackerSet->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHDEPENDENTHASHTRACKER))
                {
                    gc.dependentTracker = (LAHASHDEPENDENTHASHTRACKERREF)gc.hashKeyToTrackers->_trackerOrTrackerSet;
                    DeleteEntryTracker(key, gc.dependentTracker);
                }
                else
                {
                    gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash(gc.hashKeyToTrackers->_trackerOrTrackerSet);
                    DeleteIndividualEntryKeyValueHash deleteIndividualKeyValues(key, &gc.dependentTrackerHash);
                    gc.dependentTrackerHash.VisitAllEntryIndices(deleteIndividualKeyValues);
                }
            }

            // Remove entry from key to tracker hash
            gc.keyToTrackersHash.DeleteEntry(&key);
        }
    }
    GCPROTECT_END();
}
#endif // !DACCESS_COMPILE

template <class TRAITS>
void CrossLoaderAllocatorHash<TRAITS>::Init(LoaderAllocator *pAssociatedLoaderAllocator)
{
    LIMITED_METHOD_CONTRACT;
    _loaderAllocator = pAssociatedLoaderAllocator;
}

template <class TRAITS>
template <class Visitor>
/*static*/ bool CrossLoaderAllocatorHash<TRAITS>::VisitKeyValueStore(OBJECTREF *pLoaderAllocatorRef, OBJECTREF *pKeyValueStore, Visitor &visitor)
{
    WRAPPER_NO_CONTRACT;

    return TRAITS::VisitKeyValueStore(pLoaderAllocatorRef, pKeyValueStore, visitor);
}

template <class TRAITS>
template <class Visitor>
/*static*/ bool CrossLoaderAllocatorHash<TRAITS>::VisitTracker(TKey key, LAHASHDEPENDENTHASHTRACKERREF trackerUnsafe, Visitor &visitor)
{
    CONTRACTL
    {
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct 
    {
        LAHASHDEPENDENTHASHTRACKERREF tracker;
        OBJECTREF loaderAllocatorRef;
        GCHEAPHASHOBJECTREF keyToValuesHashObject;
        KeyToValuesGCHeapHash keyToValuesHash;
        OBJECTREF keyValueStore;
    }gc;

    ZeroMemory(&gc, sizeof(gc));
    gc.tracker = trackerUnsafe;

    bool result = true;

    GCPROTECT_BEGIN(gc);
    {
        gc.tracker->GetDependentAndLoaderAllocator(&gc.loaderAllocatorRef, &gc.keyToValuesHashObject);
        if (gc.keyToValuesHashObject != NULL)
        {
            gc.keyToValuesHash = KeyToValuesGCHeapHash(gc.keyToValuesHashObject);
            INT32 indexInKeyValueHash = gc.keyToValuesHash.GetValueIndex(&key);
            if (indexInKeyValueHash != -1)
            {
                gc.keyToValuesHash.GetElement(indexInKeyValueHash, gc.keyValueStore);

                result = VisitKeyValueStore(&gc.loaderAllocatorRef, &gc.keyValueStore, visitor);
            }
        }
    }
    GCPROTECT_END();

    return result;
}

template <class TRAITS>
template <class Visitor>
/*static*/ bool CrossLoaderAllocatorHash<TRAITS>::VisitTrackerAllEntries(LAHASHDEPENDENTHASHTRACKERREF trackerUnsafe, Visitor &visitor)
{
    CONTRACTL
    {
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct _gcStruct
    {
        LAHASHDEPENDENTHASHTRACKERREF tracker;
        OBJECTREF loaderAllocatorRef;
        GCHEAPHASHOBJECTREF keyToValuesHashObject;
        KeyToValuesGCHeapHash keyToValuesHash;
        OBJECTREF keyValueStore;
    }gc;

    class VisitAllEntryKeyValueHash
    {
        public:
        Visitor *_pVisitor;
        KeyToValuesGCHeapHash *_pKeysToValueHash;
        OBJECTREF *_pKeyValueStore;
        OBJECTREF *_pLoaderAllocatorRef;

        VisitAllEntryKeyValueHash(Visitor *pVisitor,  KeyToValuesGCHeapHash *pKeysToValueHash, OBJECTREF *pKeyValueStore, OBJECTREF *pLoaderAllocatorRef) : 
            _pVisitor(pVisitor),
            _pKeysToValueHash(pKeysToValueHash),
            _pKeyValueStore(pKeyValueStore),
            _pLoaderAllocatorRef(pLoaderAllocatorRef)
            {}

        bool operator()(INT32 index)
        {
            WRAPPER_NO_CONTRACT;

            _pKeysToValueHash->GetElement(index, *_pKeyValueStore);
            return VisitKeyValueStore(_pLoaderAllocatorRef, _pKeyValueStore, visitor);
        }
    };

    ZeroMemory(&gc, sizeof(gc));
    gc.tracker = trackerUnsafe;

    bool result = true;

    GCPROTECT_BEGIN(gc);
    {
        gc.tracker->GetDependentAndLoaderAllocator(&gc.loaderAllocatorRef, &gc.keyToValuesHashObject);
        if (gc.keyToValuesHashObject != NULL)
        {
            gc.keyToValuesHash = KeyToValuesGCHeapHash(gc.keyToValuesHashObject);
            result = gc.keyToValuesHash.VisitAllEntryIndices(VisitAllEntryKeyValueHash(&visitor, &gc.keyToValuesHash, &gc.keyValueStore, &gc.loaderAllocatorRef));
        }
    }
    GCPROTECT_END();

    return result;
}

template <class TRAITS>
template <class Visitor>
/*static*/ bool CrossLoaderAllocatorHash<TRAITS>::VisitKeyToTrackerAllEntries(OBJECTREF hashKeyEntryUnsafe, Visitor &visitor)
{
    CONTRACTL
    {
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct _gcStruct
    {
        OBJECTREF hashKeyEntry;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
        OBJECTREF keyValueStore;
        OBJECTREF loaderAllocatorRef;
    } gc;

    ZeroMemory(&gc, sizeof(gc));
    gc.hashKeyEntry = hashKeyEntryUnsafe;

    bool result = true;

    GCPROTECT_BEGIN(gc);
    {
        if (gc.hashKeyEntry->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHKEYTOTRACKERS))
        {
            gc.hashKeyToTrackers = (LAHASHKEYTOTRACKERSREF)gc.hashKeyEntry;
            gc.keyValueStore = gc.hashKeyToTrackers->_laLocalKeyValueStore;
        }
        else
        {
            gc.keyValueStore = gc.hashKeyEntry;
        }

        result = VisitKeyValueStore(&gc.loaderAllocatorRef, &gc.keyValueStore, visitor);
    }
    GCPROTECT_END();

    return result;
}

template <class TRAITS>
/*static*/ void CrossLoaderAllocatorHash<TRAITS>::DeleteEntryTracker(TKey key, LAHASHDEPENDENTHASHTRACKERREF trackerUnsafe)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct 
    {
        LAHASHDEPENDENTHASHTRACKERREF tracker;
        OBJECTREF loaderAllocatorRef;
        GCHEAPHASHOBJECTREF keyToValuesHashObject;
        KeyToValuesGCHeapHash keyToValuesHash;
    }gc;

    ZeroMemory(&gc, sizeof(gc));
    gc.tracker = trackerUnsafe;

    GCPROTECT_BEGIN(gc);
    {
        gc.tracker->GetDependentAndLoaderAllocator(&gc.loaderAllocatorRef, &gc.keyToValuesHashObject);
        if (gc.keyToValuesHashObject != NULL)
        {
            gc.keyToValuesHash = KeyToValuesGCHeapHash(gc.keyToValuesHashObject);
            gc.keyToValuesHash.DeleteEntry(&key);
        }
    }
    GCPROTECT_END();
}

#ifndef DACCESS_COMPILE
template <class TRAITS>
void CrossLoaderAllocatorHash<TRAITS>::EnsureManagedObjectsInitted()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    // Force these types to be loaded, so that the nothrow logic can use MscorlibBinder::GetExistingClass
    MscorlibBinder::GetClass(CLASS__LAHASHKEYTOTRACKERS);
    MscorlibBinder::GetClass(CLASS__LAHASHDEPENDENTHASHTRACKER);

    if (LAToDependentTrackerHash == NULL)
    {
        OBJECTREF laToDependentHandleHash = AllocateObject(MscorlibBinder::GetClass(CLASS__GCHEAPHASH));
        LAToDependentTrackerHash = _loaderAllocator->GetDomain()->CreateHandle(laToDependentHandleHash);
        _loaderAllocator->RegisterHandleForCleanup(LAToDependentTrackerHash);
    }

    if (KeyToDependentTrackersHash == NULL)
    {
        OBJECTREF keyToDependentTrackersHash = AllocateObject(MscorlibBinder::GetClass(CLASS__GCHEAPHASH));
        KeyToDependentTrackersHash = _loaderAllocator->GetDomain()->CreateHandle(keyToDependentTrackersHash);
        _loaderAllocator->RegisterHandleForCleanup(KeyToDependentTrackersHash);
    }
}
#endif // !DACCESS_COMPILE

#ifndef DACCESS_COMPILE
template <class TRAITS>
LAHASHDEPENDENTHASHTRACKERREF CrossLoaderAllocatorHash<TRAITS>::GetDependentTrackerForLoaderAllocator(LoaderAllocator* pLoaderAllocator)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct 
    {
        GCHeapHashDependentHashTrackerHash dependentTrackerHash;
        LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
        GCHEAPHASHOBJECTREF GCHeapHashForKeyToValueStore;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    GCPROTECT_BEGIN(gc)
    {
        gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash((GCHEAPHASHOBJECTREF)ObjectFromHandle(LAToDependentTrackerHash));
        INT32 index = gc.dependentTrackerHash.GetValueIndex(&pLoaderAllocator);
        if (index != -1)
        {
            // We have an entry in the hashtable for the key/dependenthandle.
            gc.dependentTrackerHash.GetElement(index, gc.dependentTracker);
        }
        else
        {
            gc.dependentTracker = (LAHASHDEPENDENTHASHTRACKERREF)AllocateObject(MscorlibBinder::GetClass(CLASS__LAHASHDEPENDENTHASHTRACKER));
            gc.GCHeapHashForKeyToValueStore = (GCHEAPHASHOBJECTREF)AllocateObject(MscorlibBinder::GetClass(CLASS__GCHEAPHASH));
            OBJECTHANDLE dependentHandle = GetAppDomain()->CreateDependentHandle(pLoaderAllocator->GetExposedObject(), gc.GCHeapHashForKeyToValueStore);
            gc.dependentTracker->Init(dependentHandle, pLoaderAllocator);
            gc.dependentTrackerHash.Add(&pLoaderAllocator, [&gc](PTRARRAYREF arr, INT32 index)
            {
                arr->SetAt(index, (OBJECTREF)gc.dependentTracker);
            });
        }
    }
    GCPROTECT_END();

    return gc.dependentTracker;
}
#endif // !DACCESS_COMPILE

#ifndef DACCESS_COMPILE
template <class TRAITS>
GCHEAPHASHOBJECTREF CrossLoaderAllocatorHash<TRAITS>::GetKeyToValueCrossLAHashForHashkeyToTrackers(LAHASHKEYTOTRACKERSREF hashKeyToTrackersUnsafe, LoaderAllocator* pValueLoaderAllocator)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    struct 
    {
        GCHeapHashDependentHashTrackerHash dependentTrackerHash;
        LAHASHDEPENDENTHASHTRACKERREF dependentTrackerMaybe;
        LAHASHDEPENDENTHASHTRACKERREF dependentTracker;
        LAHASHKEYTOTRACKERSREF hashKeyToTrackers;
        GCHEAPHASHOBJECTREF returnValue;
    } gc;
    ZeroMemory(&gc, sizeof(gc));
    // Now gc.hashKeyToTrackers is filled in.
    gc.hashKeyToTrackers = hashKeyToTrackersUnsafe;
    GCPROTECT_BEGIN(gc)
    {
        EnsureManagedObjectsInitted();

        // Is there a single dependenttracker here, or a set, or no dependenttracker at all
        if (gc.hashKeyToTrackers->_trackerOrTrackerSet == NULL)
        {
            gc.dependentTracker = GetDependentTrackerForLoaderAllocator(pValueLoaderAllocator);
            SetObjectReference(&gc.hashKeyToTrackers->_trackerOrTrackerSet, gc.dependentTracker, GetAppDomain());
        }
        else if (gc.hashKeyToTrackers->_trackerOrTrackerSet->GetMethodTable() == MscorlibBinder::GetExistingClass(CLASS__LAHASHDEPENDENTHASHTRACKER))
        {
            gc.dependentTrackerMaybe = (LAHASHDEPENDENTHASHTRACKERREF)gc.hashKeyToTrackers->_trackerOrTrackerSet;
            if (gc.dependentTrackerMaybe->IsTrackerFor(pValueLoaderAllocator))
            {
                // We've found the right dependent tracker.
                gc.dependentTracker = gc.dependentTrackerMaybe;
            }
            else
            {
                gc.dependentTracker = GetDependentTrackerForLoaderAllocator(pValueLoaderAllocator);
                if (!gc.dependentTrackerMaybe->IsLoaderAllocatorLive())
                {
                    SetObjectReference(&gc.hashKeyToTrackers->_trackerOrTrackerSet, gc.dependentTracker, GetAppDomain());
                }
                else
                {
                    // Allocate the dependent tracker hash
                    // Fill with the existing dependentTrackerMaybe, and gc.DependentTracker
                    gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash(AllocateObject(MscorlibBinder::GetClass(CLASS__GCHEAPHASH)));
                    LoaderAllocator *pLoaderAllocatorKey = gc.dependentTracker->GetLoaderAllocatorUnsafe();
                    gc.dependentTrackerHash.Add(&pLoaderAllocatorKey, [&gc](PTRARRAYREF arr, INT32 index)
                        {
                            arr->SetAt(index, (OBJECTREF)gc.dependentTracker);
                        });
                    pLoaderAllocatorKey = gc.dependentTrackerMaybe->GetLoaderAllocatorUnsafe();
                    gc.dependentTrackerHash.Add(&pLoaderAllocatorKey, [&gc](PTRARRAYREF arr, INT32 index)
                        {
                            arr->SetAt(index, (OBJECTREF)gc.dependentTrackerMaybe);
                        });
                    SetObjectReference(&gc.hashKeyToTrackers->_trackerOrTrackerSet, gc.dependentTrackerHash.GetGCHeapRef(), GetAppDomain());
                }
            }
        }
        else
        {
            gc.dependentTrackerHash = GCHeapHashDependentHashTrackerHash(gc.hashKeyToTrackers->_trackerOrTrackerSet);

            INT32 indexOfTracker = gc.dependentTrackerHash.GetValueIndex(&pValueLoaderAllocator);
            if (indexOfTracker == -1)
            {
                // Dependent tracker not yet attached to this key
                
                // Get dependent tracker
                gc.dependentTracker = GetDependentTrackerForLoaderAllocator(pValueLoaderAllocator);
                gc.dependentTrackerHash.Add(&pValueLoaderAllocator, [&gc](PTRARRAYREF arr, INT32 index)
                    {
                        arr->SetAt(index, (OBJECTREF)gc.dependentTracker);
                    });
            }
            else
            {
                gc.dependentTrackerHash.GetElement(indexOfTracker, gc.dependentTracker);
            }
        }

        // At this stage gc.dependentTracker is setup to have a good value
        gc.returnValue = gc.dependentTracker->GetDependentTarget();
    }
    GCPROTECT_END();

    return gc.returnValue;
}
#endif // !DACCESS_COMPILE

#endif // !CROSSGEN_COMPILE
#endif // CROSSLOADERALLOCATORHASH_H
#endif // CROSSLOADERALLOCATORHASH_INL
