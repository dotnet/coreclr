// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: genericdict.cpp
//

//
// WARNING: Do NOT turn try to save dictionary slots except in the
// hardbind case.  Saving further dictionary slots can lead
// to ComputeNeedsRestore returning TRUE for the dictionary and
// the associated method table (though of course only if some
// entries in the dictionary are prepopulated).  However at
// earlier stages in the NGEN, code may have been compiled
// under the assumption that ComputeNeedsRestore was
// FALSE for the assocaited method table, and indeed this result
// may have been cached in the ComputeNeedsRestore
// for the MethodTable.  Thus the combination of populating
// the dictionary and saving further dictionary slots could lead
// to inconsistencies and unsoundnesses in compilation.
//

//
// ============================================================================

#include "common.h"
#include "genericdict.h"
#include "typestring.h"
#include "field.h"
#include "typectxt.h"
#include "virtualcallstub.h"
#include "sigbuilder.h"
#include "compile.h"

#ifndef DACCESS_COMPILE 

//---------------------------------------------------------------------------------------
//
//static
DictionaryLayout * 
DictionaryLayout::Allocate(
    WORD              numSlots, 
    LoaderAllocator * pAllocator, 
    AllocMemTracker * pamTracker)
{
    CONTRACT(DictionaryLayout*)
    {
        THROWS;
        GC_NOTRIGGER;
        INJECT_FAULT(COMPlusThrowOM(););
        PRECONDITION(CheckPointer(pAllocator));
        PRECONDITION(numSlots > 0);
        POSTCONDITION(CheckPointer(RETVAL));
    }
    CONTRACT_END

    S_SIZE_T bytes = S_SIZE_T(sizeof(DictionaryLayout)) + S_SIZE_T(sizeof(DictionaryEntryLayout)) * S_SIZE_T(numSlots-1);

    TaggedMemAllocPtr ptr = pAllocator->GetLowFrequencyHeap()->AllocMem(bytes);

    if (pamTracker != NULL)
        pamTracker->Track(ptr);

    DictionaryLayout * pD = (DictionaryLayout *)(void *)ptr;

    // When bucket spills we'll allocate another layout structure
    pD->m_pNext = NULL;

    // This is the number of slots excluding the type parameters
    pD->m_numSlots = numSlots;

    RETURN pD;
} // DictionaryLayout::Allocate

#endif //!DACCESS_COMPILE

//---------------------------------------------------------------------------------------
//
// Count the number of bytes that are required by the first bucket in a dictionary with the specified layout
// 
//static
DWORD 
DictionaryLayout::GetFirstDictionaryBucketSize(
    DWORD                numGenericArgs, 
    PTR_DictionaryLayout pDictLayout)
{
    LIMITED_METHOD_DAC_CONTRACT;
    PRECONDITION(numGenericArgs > 0);
    PRECONDITION(CheckPointer(pDictLayout, NULL_OK));

    DWORD bytes = numGenericArgs * sizeof(TypeHandle);
    if (pDictLayout != NULL)
        bytes += pDictLayout->m_numSlots * sizeof(void*);

    return bytes;
}

#ifndef DACCESS_COMPILE 
//---------------------------------------------------------------------------------------
//
// Find a token in the dictionary layout and return the offsets of indirections
// required to get to its slot in the actual dictionary
//
// NOTE: We will currently never return more than one indirection. We don't
// cascade dictionaries but we will record overflows in the dictionary layout
// (and cascade that accordingly) so we can prepopulate the overflow hash in
// reliability scenarios.
//
// Optimize the case of a token being !i (for class dictionaries) or !!i (for method dictionaries)
// 
/* static */
BOOL 
DictionaryLayout::FindTokenWorker(LoaderAllocator *                 pAllocator,
                                  DWORD                             numGenericArgs,
                                  DictionaryLayout *                pDictLayout,
                                  CORINFO_RUNTIME_LOOKUP *          pResult,
                                  SigBuilder *                      pSigBuilder,
                                  BYTE *                            pSig,
                                  DWORD                             cbSig,
                                  int                               nFirstOffset,
                                  DictionaryEntrySignatureSource    signatureSource,
                                  WORD *                            pSlotOut)
{
    CONTRACTL
    {
        STANDARD_VM_CHECK;
        PRECONDITION(numGenericArgs > 0);
        PRECONDITION(CheckPointer(pDictLayout));
        PRECONDITION(CheckPointer(pSlotOut));
        PRECONDITION(CheckPointer(pSig));
        PRECONDITION((pSigBuilder == NULL && cbSig == -1) || (CheckPointer(pSigBuilder) && cbSig > 0));
    }
    CONTRACTL_END

    BOOL isFirstBucket = TRUE;

    // First bucket also contains type parameters
    _ASSERTE(FitsIn<WORD>(numGenericArgs));
    WORD slot = static_cast<WORD>(numGenericArgs);
    for (;;)
    {
        for (DWORD iSlot = 0; iSlot < pDictLayout->m_numSlots; iSlot++)
        {
        RetryMatch:
            BYTE * pCandidate = (BYTE *)pDictLayout->m_slots[iSlot].m_signature;
            if (pCandidate != NULL)
            {
                bool signaturesMatch = false;

                if (pSigBuilder != NULL)
                {
                    // JIT case: compare signatures by comparing the bytes in them. We exclude
                    // any ReadyToRun signatures from the JIT case.

                    if (pDictLayout->m_slots[iSlot].m_signatureSource != FromReadyToRunImage)
                    {
                        // Compare the signatures. We do not need to worry about the size of pCandidate. 
                        // As long as we are comparing one byte at a time we are guaranteed to not overrun.
                        DWORD j;
                        for (j = 0; j < cbSig; j++)
                        {
                            if (pCandidate[j] != pSig[j])
                                break;
                        }
                        signaturesMatch = (j == cbSig);
                    }
                }
                else
                {
                    // ReadyToRun case: compare signatures by comparing their pointer values
                    signaturesMatch = (pCandidate == pSig);
                }

                // We've found it
                if (signaturesMatch)
                {
                    pResult->signature = pDictLayout->m_slots[iSlot].m_signature;

                    // We don't store entries outside the first bucket in the layout in the dictionary (they'll be cached in a hash
                    // instead).
                    if (!isFirstBucket)
                    {
                        return FALSE;
                    }
                    _ASSERTE(FitsIn<WORD>(nFirstOffset + 1));
                    pResult->indirections = static_cast<WORD>(nFirstOffset + 1);
                    pResult->offsets[nFirstOffset] = slot * sizeof(DictionaryEntry);
                    *pSlotOut = slot;
                    return TRUE;
                }
            }
            // If we hit an empty slot then there's no more so use it
            else
            {
                {
                    BaseDomain::LockHolder lh(pAllocator->GetDomain());

                    if (pDictLayout->m_slots[iSlot].m_signature != NULL)
                        goto RetryMatch;

                    PVOID pResultSignature = pSig;

                    if (pSigBuilder != NULL)
                    {
                        pSigBuilder->AppendData(isFirstBucket ? slot : 0);

                        DWORD cbNewSig;
                        PVOID pNewSig = pSigBuilder->GetSignature(&cbNewSig);

                        pResultSignature = pAllocator->GetLowFrequencyHeap()->AllocMem(S_SIZE_T(cbNewSig));
                        memcpy(pResultSignature, pNewSig, cbNewSig);
                    }

                    *EnsureWritablePages(&(pDictLayout->m_slots[iSlot].m_signature)) = pResultSignature;
                    *EnsureWritablePages(&(pDictLayout->m_slots[iSlot].m_signatureSource)) = signatureSource;
                }

                pResult->signature = pDictLayout->m_slots[iSlot].m_signature;

                // Again, we only store entries in the first layout bucket in the dictionary.
                if (!isFirstBucket)
                {
                    return FALSE;
                }
                _ASSERTE(FitsIn<WORD>(nFirstOffset + 1));
                pResult->indirections = static_cast<WORD>(nFirstOffset + 1);
                pResult->offsets[nFirstOffset] = slot * sizeof(DictionaryEntry);
                *pSlotOut = slot;
                return TRUE;
            }
            slot++;
        }

        // If we've reached the end of the chain we need to allocate another bucket. Make the pointer update carefully to avoid
        // orphaning a bucket in a race. We leak the loser in such a race (since the allocation comes from the loader heap) but both
        // the race and the overflow should be very rare.
        if (pDictLayout->m_pNext == NULL)
            FastInterlockCompareExchangePointer(EnsureWritablePages(&(pDictLayout->m_pNext)), Allocate(4, pAllocator, NULL), 0);

        pDictLayout = pDictLayout->m_pNext;
        isFirstBucket = FALSE;
    }
} // DictionaryLayout::FindToken

/* static */
BOOL 
DictionaryLayout::FindToken(LoaderAllocator *               pAllocator, 
                            DWORD                           numGenericArgs, 
                            DictionaryLayout *              pDictLayout, 
                            CORINFO_RUNTIME_LOOKUP *        pResult, 
                            SigBuilder *                    pSigBuilder, 
                            int                             nFirstOffset, 
                            DictionaryEntrySignatureSource  signatureSource)
{
    WRAPPER_NO_CONTRACT;

    DWORD cbSig;
    BYTE * pSig = (BYTE *)pSigBuilder->GetSignature(&cbSig);

    WORD slotDummy;
    return FindTokenWorker(pAllocator, numGenericArgs, pDictLayout, pResult, pSigBuilder, pSig, cbSig, nFirstOffset, signatureSource, &slotDummy);
}

/* static */
BOOL 
DictionaryLayout::FindToken(LoaderAllocator *               pAllocator,
                            DWORD                           numGenericArgs, 
                            DictionaryLayout *              pDictLayout, 
                            CORINFO_RUNTIME_LOOKUP *        pResult, 
                            BYTE *                          signature,
                            int                             nFirstOffset,
                            DictionaryEntrySignatureSource  signatureSource,
                            WORD *                          pSlotOut)
{
    WRAPPER_NO_CONTRACT;

    return FindTokenWorker(pAllocator, numGenericArgs, pDictLayout, pResult, NULL, signature, -1, nFirstOffset, signatureSource, pSlotOut);
}

#endif //!DACCESS_COMPILE

//---------------------------------------------------------------------------------------
//
DWORD 
DictionaryLayout::GetMaxSlots()
{
    LIMITED_METHOD_CONTRACT;
    return m_numSlots;
}

//---------------------------------------------------------------------------------------
//
DWORD 
DictionaryLayout::GetNumUsedSlots()
{
    LIMITED_METHOD_CONTRACT;

    DWORD numUsedSlots = 0;
    for (DWORD i = 0; i < m_numSlots; i++)
    {
        if (GetEntryLayout(i)->m_signature != NULL)
            numUsedSlots++;
    }
    return numUsedSlots;
}


//---------------------------------------------------------------------------------------
// 
DictionaryEntryKind 
DictionaryEntryLayout::GetKind()
{
    STANDARD_VM_CONTRACT;
    
    if (m_signature == NULL)
        return EmptySlot;

    SigPointer ptr((PCCOR_SIGNATURE)dac_cast<TADDR>(m_signature));

    ULONG kind; // DictionaryEntryKind
    IfFailThrow(ptr.GetData(&kind));

    return (DictionaryEntryKind)kind;
}

#ifndef DACCESS_COMPILE
#ifdef FEATURE_NATIVE_IMAGE_GENERATION 

//---------------------------------------------------------------------------------------
// 
DWORD 
DictionaryLayout::GetObjectSize()
{
    LIMITED_METHOD_CONTRACT;
    return sizeof(DictionaryLayout) + sizeof(DictionaryEntryLayout) * (m_numSlots-1);
}

//---------------------------------------------------------------------------------------
//
// Save the dictionary layout for prejitting
// 
void 
DictionaryLayout::Save(
    DataImage * image)
{
    STANDARD_VM_CONTRACT;

    DictionaryLayout *pDictLayout = this;

    while (pDictLayout)
    {
        image->StoreStructure(pDictLayout, pDictLayout->GetObjectSize(), DataImage::ITEM_DICTIONARY_LAYOUT);
        pDictLayout = pDictLayout->m_pNext;
    }

}

//---------------------------------------------------------------------------------------
//
// Save the dictionary layout for prejitting
// 
void 
DictionaryLayout::Trim()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
    }
    CONTRACTL_END;

    // Only the last bucket in the chain may have unused entries
    DictionaryLayout *pDictLayout = this;
    while (pDictLayout->m_pNext)
        pDictLayout = pDictLayout->m_pNext;

    // Trim down the size to what's actually used
    DWORD dwSlots = pDictLayout->GetNumUsedSlots();
    _ASSERTE(FitsIn<WORD>(dwSlots));
    *EnsureWritablePages(&pDictLayout->m_numSlots) = static_cast<WORD>(dwSlots);

}

//---------------------------------------------------------------------------------------
//
// Fixup pointers in the dictionary layout for prejitting
// 
void 
DictionaryLayout::Fixup(
    DataImage * image, 
    BOOL        fMethod)
{
    STANDARD_VM_CONTRACT;

    DictionaryLayout *pDictLayout = this;

    while (pDictLayout)
    {
        for (DWORD i = 0; i < pDictLayout->m_numSlots; i++)
        {
            PVOID signature = pDictLayout->m_slots[i].m_signature;
            if (signature != NULL)
            {
                image->FixupFieldToNode(pDictLayout, (BYTE *)&pDictLayout->m_slots[i].m_signature - (BYTE *)pDictLayout,
                    image->GetGenericSignature(signature, fMethod));
            }
        }
        image->FixupPointerField(pDictLayout, offsetof(DictionaryLayout, m_pNext));
        pDictLayout = pDictLayout->m_pNext;
    }
}

//---------------------------------------------------------------------------------------
//
// Fixup pointers in the actual dictionary, including the type arguments.  Delete entries
// that are expensive or difficult to restore.
// 
void 
Dictionary::Fixup(
    DataImage *        image, 
    BOOL               canSaveInstantiation, 
    BOOL               canSaveSlots, 
    DWORD              numGenericArgs, 
    Module *           pModule, 
    DictionaryLayout * pDictLayout)
{
    STANDARD_VM_CONTRACT;

    // First fixup the type handles in the instantiation itself
    FixupPointer<TypeHandle> *pInst = GetInstantiation();
    for (DWORD j = 0; j < numGenericArgs; j++)
    {
        if (canSaveInstantiation)
        {
            image->FixupTypeHandlePointer(pInst, &pInst[j]);
        }
        else
        {
            image->ZeroPointerField(AsPtr(), j * sizeof(DictionaryEntry));
        }
    }

    // Now traverse the remaining slots
    if (pDictLayout != NULL)
    {
        for (DWORD i = 0; i < pDictLayout->m_numSlots; i++)
        {
            int slotOffset = (numGenericArgs + i) * sizeof(DictionaryEntry);

            // First check if we can simply hardbind to a prerestored object
            DictionaryEntryLayout *pLayout = pDictLayout->GetEntryLayout(i);
            switch (pLayout->GetKind())
            {
            case TypeHandleSlot:
            case DeclaringTypeHandleSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanPrerestoreEagerBindToTypeHandle(GetTypeHandleSlot(numGenericArgs, i), NULL) &&
                    image->CanHardBindToZapModule(GetTypeHandleSlot(numGenericArgs, i).GetLoaderModule()))
                {
                    image->HardBindTypeHandlePointer(AsPtr(), slotOffset);
                }
                else
                {
                    // Otherwise just zero the slot
                    image->ZeroPointerField(AsPtr(), slotOffset);
                }
                break;
            case MethodDescSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanPrerestoreEagerBindToMethodDesc(GetMethodDescSlot(numGenericArgs,i), NULL) &&
                    image->CanHardBindToZapModule(GetMethodDescSlot(numGenericArgs,i)->GetLoaderModule()))
                {
                    image->FixupPointerField(AsPtr(), slotOffset);
                }
                else
                {
                    // Otherwise just zero the slot
                    image->ZeroPointerField(AsPtr(), slotOffset);
                }
                break;
            case FieldDescSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanEagerBindToFieldDesc(GetFieldDescSlot(numGenericArgs,i)) &&
                    image->CanHardBindToZapModule(GetFieldDescSlot(numGenericArgs,i)->GetLoaderModule()))
                {
                    image->FixupPointerField(AsPtr(), slotOffset);
                }
                else
                {
                    // Otherwise just zero the slot
                    image->ZeroPointerField(AsPtr(), slotOffset);
                }
                break;
            default:
                // <TODO> Method entry points are currently not saved </TODO>
                // <TODO> Stub dispatch slots are currently not saved </TODO>
                // Otherwise just zero the slot
                image->ZeroPointerField(AsPtr(), slotOffset);
            }
        }
    }
} // Dictionary::Fixup

//---------------------------------------------------------------------------------------
//
BOOL 
Dictionary::IsWriteable(
    DataImage *         image, 
    BOOL                canSaveSlots, 
    DWORD               numGenericArgs, // Must be non-zero
    Module *            pModule,        // module of the generic code
    DictionaryLayout *  pDictLayout)
{
    STANDARD_VM_CONTRACT;

    // Traverse dictionary slots
    if (pDictLayout != NULL)
    {
        for (DWORD i = 0; i < pDictLayout->m_numSlots; i++)
        {
            // First check if we can simply hardbind to a prerestored object
            DictionaryEntryLayout *pLayout = pDictLayout->GetEntryLayout(i);
            switch (pLayout->GetKind())
            {
            case TypeHandleSlot:
            case DeclaringTypeHandleSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanPrerestoreEagerBindToTypeHandle(GetTypeHandleSlot(numGenericArgs, i), NULL) &&
                    image->CanHardBindToZapModule(GetTypeHandleSlot(numGenericArgs, i).GetLoaderModule()))
                {
                    // do nothing
                }
                else
                {
                    return TRUE;
                }
                break;
            case MethodDescSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanPrerestoreEagerBindToMethodDesc(GetMethodDescSlot(numGenericArgs,i), NULL) &&
                    image->CanHardBindToZapModule(GetMethodDescSlot(numGenericArgs,i)->GetLoaderModule()))
                {
                    // do nothing
                }
                else
                {
                    return TRUE;
                }
                break;
            case FieldDescSlot:
                if (canSaveSlots &&
                    !IsSlotEmpty(numGenericArgs,i) &&
                    image->CanEagerBindToFieldDesc(GetFieldDescSlot(numGenericArgs,i)) &&
                    image->CanHardBindToZapModule(GetFieldDescSlot(numGenericArgs,i)->GetLoaderModule()))
                {
                    // do nothing                
                }
                else
                {
                    return TRUE;
                }
                break;
            default:
                // <TODO> Method entry points are currently not saved </TODO>
                // <TODO> Stub dispatch slots are currently not saved </TODO>
                return TRUE;
            }
        }
    }

    return FALSE;
} // Dictionary::IsWriteable

//---------------------------------------------------------------------------------------
//
BOOL 
Dictionary::ComputeNeedsRestore(
    DataImage *      image, 
    TypeHandleList * pVisited, 
    DWORD            numGenericArgs)
{
    STANDARD_VM_CONTRACT;

    // First check the type handles in the instantiation itself
    FixupPointer<TypeHandle> *inst = GetInstantiation();
    for (DWORD j = 0; j < numGenericArgs; j++)
    {
        if (!image->CanPrerestoreEagerBindToTypeHandle(inst[j].GetValue(), pVisited))
            return TRUE;
    }

    // Unless prepopulating we don't need to check the entries
    // of the dictionary because if we can't
    // hardbind to them we just zero the dictionary entry and recover
    // it on demand.

    return FALSE;
}
#endif //FEATURE_NATIVE_IMAGE_GENERATION

#ifdef FEATURE_PREJIT
//---------------------------------------------------------------------------------------
//
void 
Dictionary::Restore(
    DWORD          numGenericArgs, 
    ClassLoadLevel level)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        INSTANCE_CHECK;
    }
    CONTRACTL_END

    // First restore the type handles in the instantiation itself
    FixupPointer<TypeHandle> *inst = GetInstantiation();
    for (DWORD j = 0; j < numGenericArgs; j++)
    {
        Module::RestoreTypeHandlePointer(&inst[j], NULL, level);
    }

    // We don't restore the remainder of the dictionary - see
    // long comment at the start of this file as to why
}
#endif // FEATURE_PREJIT

//---------------------------------------------------------------------------------------
// 
DictionaryEntry 
Dictionary::PopulateEntry(
    MethodDesc *       pMD, 
    MethodTable *      pMT, 
    LPVOID             signature, 
    BOOL               nonExpansive, 
    DictionaryEntry ** ppSlot,
    DWORD              dictionaryIndexAndSlot, /* = -1 */
    Module *           pModule /* = NULL */)
{
     CONTRACTL {
        THROWS;
        GC_TRIGGERS;
    } CONTRACTL_END;

    CORINFO_GENERIC_HANDLE result = NULL;
    Dictionary * pDictionary = NULL;
    *ppSlot = NULL;

    bool isReadyToRunModule = (pModule != NULL && pModule->IsReadyToRun());

    ZapSig::Context zapSigContext(NULL, NULL, ZapSig::NormalTokens);
    ZapSig::Context * pZapSigContext = NULL;

    ULONG kind = DictionaryEntryKind::EmptySlot;

    SigPointer ptr((PCCOR_SIGNATURE)signature);

    if (isReadyToRunModule)
    {
        PCCOR_SIGNATURE pBlob = (PCCOR_SIGNATURE)signature;

        BYTE fixupKind = *pBlob++;

        Module * pInfoModule = pModule;
        if (fixupKind & ENCODE_MODULE_OVERRIDE)
        {
            DWORD moduleIndex = CorSigUncompressData(pBlob);
            pInfoModule = pModule->GetModuleFromIndex(moduleIndex);
            fixupKind &= ~ENCODE_MODULE_OVERRIDE;
        }

        _ASSERTE(fixupKind == ENCODE_DICTIONARY_LOOKUP_THISOBJ ||
                 fixupKind == ENCODE_DICTIONARY_LOOKUP_TYPE ||
                 fixupKind == ENCODE_DICTIONARY_LOOKUP_METHOD);

        if (fixupKind == ENCODE_DICTIONARY_LOOKUP_THISOBJ)
        {
            SigPointer p(pBlob);
            p.SkipExactlyOne();
            pBlob = p.GetPtr();
        }

        BYTE signatureKind = *pBlob++;
        if (signatureKind & ENCODE_MODULE_OVERRIDE)
        {
            DWORD moduleIndex = CorSigUncompressData(pBlob);
            Module * pSignatureModule = pModule->GetModuleFromIndex(moduleIndex);
            if (pInfoModule == pModule)
            {
                pInfoModule = pSignatureModule;
            }
            _ASSERTE(pInfoModule == pSignatureModule);
            signatureKind &= ~ENCODE_MODULE_OVERRIDE;
        }

        switch ((CORCOMPILE_FIXUP_BLOB_KIND) signatureKind)
        {
            case ENCODE_DECLARINGTYPE_HANDLE:   kind = DeclaringTypeHandleSlot; break;
            case ENCODE_TYPE_HANDLE:            kind = TypeHandleSlot; break;
            case ENCODE_FIELD_HANDLE:           kind = FieldDescSlot; break;
            case ENCODE_METHOD_HANDLE:          kind = MethodDescSlot; break;
            case ENCODE_METHOD_ENTRY:           kind = MethodEntrySlot; break;
            case ENCODE_VIRTUAL_ENTRY:          kind = DispatchStubAddrSlot; break;

            default:
                _ASSERTE(!"Unexpected CORCOMPILE_FIXUP_BLOB_KIND");
                ThrowHR(COR_E_BADIMAGEFORMAT);
        }

        ptr = SigPointer(pBlob);

        zapSigContext = ZapSig::Context(pInfoModule, pModule, ZapSig::NormalTokens);
        pZapSigContext = &zapSigContext;
    }
    else
    {
        ptr = SigPointer((PCCOR_SIGNATURE)signature);
        IfFailThrow(ptr.GetData(&kind));

        Module * pContainingZapModule = ExecutionManager::FindZapModule(dac_cast<TADDR>(signature));

        zapSigContext = ZapSig::Context(MscorlibBinder::GetModule(), (void *)pContainingZapModule, ZapSig::NormalTokens);
        pZapSigContext = (pContainingZapModule != NULL) ? &zapSigContext : NULL;
    }

    Module * pLookupModule = (isReadyToRunModule) ? pZapSigContext->pInfoModule : MscorlibBinder::GetModule();

    if (pMT != NULL)
    {
        // We need to normalize the class passed in (if any) for reliability purposes. That's because preparation of a code region that
        // contains these handle lookups depends on being able to predict exactly which lookups are required (so we can pre-cache the
        // answers and remove any possibility of failure at runtime). This is hard to do if the lookup (in this case the lookup of the
        // dictionary overflow cache) is keyed off the somewhat arbitrary type of the instance on which the call is made (we'd need to
        // prepare for every possible derived type of the type containing the method). So instead we have to locate the exactly
        // instantiated (non-shared) super-type of the class passed in.

        pDictionary = pMT->GetDictionary();

        ULONG dictionaryIndex = 0;

        if (isReadyToRunModule)
        {
            dictionaryIndex = dictionaryIndexAndSlot >> 16;
        }
        else
        {
            IfFailThrow(ptr.GetData(&dictionaryIndex));
        }
        
        // MethodTable is expected to be normalized      
        _ASSERTE(pDictionary == pMT->GetPerInstInfo()[dictionaryIndex].GetValueMaybeNull());
    }
    else
    {
        pDictionary = pMD->GetMethodDictionary();
    }

    {
        SigTypeContext typeContext;

        if (pMT != NULL)
        {
            SigTypeContext::InitTypeContext(pMT, &typeContext);
        }
        else
        {
            SigTypeContext::InitTypeContext(pMD, &typeContext);
        }

        TypeHandle constraintType;
        TypeHandle declaringType;

        switch (kind)
        {
        case DeclaringTypeHandleSlot:
        {
            declaringType = ptr.GetTypeHandleThrowing(
                pLookupModule,
                &typeContext, 
                (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes), 
                CLASS_LOADED, 
                FALSE, 
                NULL, 
                pZapSigContext);
            if (declaringType.IsNull())
            {
                _ASSERTE(nonExpansive);
                return NULL;
            }
            IfFailThrow(ptr.SkipExactlyOne());

            // fall through
        }

        case TypeHandleSlot:
        {
            TypeHandle th = ptr.GetTypeHandleThrowing(
                pLookupModule,
                &typeContext, 
                (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes), 
                CLASS_LOADED, 
                FALSE, 
                NULL, 
                pZapSigContext);
            if (th.IsNull())
            {
                _ASSERTE(nonExpansive);
                return NULL;
            }
            IfFailThrow(ptr.SkipExactlyOne());

            if (!declaringType.IsNull())
            {
                th = th.GetMethodTable()->GetMethodTableMatchingParentClass(declaringType.AsMethodTable());
            }

            if (!IsCompilationProcess())
            {
                th.GetMethodTable()->EnsureInstanceActive();
            }

            result = (CORINFO_GENERIC_HANDLE)th.AsPtr();
            break;
        }

        case ConstrainedMethodEntrySlot:
        {
            constraintType = ptr.GetTypeHandleThrowing(
                pLookupModule,
                &typeContext, 
                (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes), 
                CLASS_LOADED, 
                FALSE, 
                NULL, 
                pZapSigContext);
            if (constraintType.IsNull())
            {
                _ASSERTE(nonExpansive);
                return NULL;
            }
            IfFailThrow(ptr.SkipExactlyOne());

            // fall through
        }

        case MethodDescSlot:
        case DispatchStubAddrSlot:
        case MethodEntrySlot:
        {
            TypeHandle ownerType;
            MethodTable * pOwnerMT = NULL;
            MethodDesc * pMethod = NULL;

            DWORD methodFlags = 0;
            BOOL isInstantiatingStub = 0;
            BOOL isUnboxingStub = 0;
            BOOL fMethodNeedsInstantiation = 0;

            DWORD methodSlot = -1;
            BOOL fRequiresDispatchStub = 0;

            if (isReadyToRunModule)
            {
                IfFailThrow(ptr.GetData(&methodFlags));

                if (methodFlags & ENCODE_METHOD_SIG_Constrained)
                    kind = ConstrainedMethodEntrySlot;

                isInstantiatingStub = ((methodFlags & ENCODE_METHOD_SIG_InstantiatingStub) != 0) || (kind == MethodEntrySlot);
                isUnboxingStub = ((methodFlags & ENCODE_METHOD_SIG_UnboxingStub) != 0);
                fMethodNeedsInstantiation = ((methodFlags & ENCODE_METHOD_SIG_MethodInstantiation) != 0);

                if (methodFlags & ENCODE_METHOD_SIG_OwnerType)
                {
                    ownerType = ptr.GetTypeHandleThrowing(
                        pZapSigContext->pInfoModule,
                        &typeContext,
                        ClassLoader::LoadTypes,
                        CLASS_LOADED,
                        FALSE,
                        NULL,
                        pZapSigContext);

                    IfFailThrow(ptr.SkipExactlyOne());
                }

                if (methodFlags & ENCODE_METHOD_SIG_SlotInsteadOfToken)
                {
                    // get the method desc using slot number
                    IfFailThrow(ptr.GetData(&methodSlot));

                    _ASSERTE(!ownerType.IsNull());
                    pMethod = ownerType.GetMethodTable()->GetMethodDescForSlot(methodSlot);
                }
                else
                {
                    //
                    // decode method token
                    //
                    RID rid;
                    IfFailThrow(ptr.GetData(&rid));

                    if (methodFlags & ENCODE_METHOD_SIG_MemberRefToken)
                    {
                        if (ownerType.IsNull())
                        {
                            FieldDesc * pFDDummy = NULL;

                            MemberLoader::GetDescFromMemberRef(pZapSigContext->pInfoModule, TokenFromRid(rid, mdtMemberRef), &pMethod, &pFDDummy, NULL, FALSE, &ownerType);
                            _ASSERTE(pMethod != NULL && pFDDummy == NULL);
                        }
                        else
                        {
                            pMethod = MemberLoader::GetMethodDescFromMemberRefAndType(pZapSigContext->pInfoModule, TokenFromRid(rid, mdtMemberRef), ownerType.GetMethodTable());
                        }
                    }
                    else
                    {
                        pMethod = MemberLoader::GetMethodDescFromMethodDef(pZapSigContext->pInfoModule, TokenFromRid(rid, mdtMethodDef), FALSE);
                    }
                }

                if (ownerType.IsNull())
                    ownerType = pMethod->GetMethodTable();

                _ASSERT(!ownerType.IsNull() && !nonExpansive);
                pOwnerMT = ownerType.GetMethodTable();

                if (kind == DispatchStubAddrSlot && pMethod->IsVtableMethod())
                {
                    fRequiresDispatchStub = TRUE;
                    methodSlot = pMethod->GetSlot();
                }
            }
            else
            {
                ownerType = ptr.GetTypeHandleThrowing(
                    pLookupModule,
                    &typeContext,
                    (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes),
                    CLASS_LOADED,
                    FALSE,
                    NULL,
                    pZapSigContext);
                if (ownerType.IsNull())
                {
                    _ASSERTE(nonExpansive);
                    return NULL;
                }
                IfFailThrow(ptr.SkipExactlyOne());

                // <NICE> wsperf: Create a path that doesn't load types or create new handles if nonExpansive is set </NICE>
                if (nonExpansive)
                    return NULL;

                pOwnerMT = ownerType.GetMethodTable();
                _ASSERTE(pOwnerMT != NULL);

                IfFailThrow(ptr.GetData(&methodFlags));

                isInstantiatingStub = ((methodFlags & ENCODE_METHOD_SIG_InstantiatingStub) != 0);
                isUnboxingStub = ((methodFlags & ENCODE_METHOD_SIG_UnboxingStub) != 0);
                fMethodNeedsInstantiation = ((methodFlags & ENCODE_METHOD_SIG_MethodInstantiation) != 0);

                if ((methodFlags & ENCODE_METHOD_SIG_SlotInsteadOfToken) != 0)
                {
                    // get the method desc using slot number
                    IfFailThrow(ptr.GetData(&methodSlot));

                    if (kind == DispatchStubAddrSlot)
                    {
                        if (NingenEnabled())
                            return NULL;

#ifndef CROSSGEN_COMPILE
                        fRequiresDispatchStub = TRUE;
#endif
                    }

                    if (!fRequiresDispatchStub)
                        pMethod = pOwnerMT->GetMethodDescForSlot(methodSlot);
                }
                else
                {
                    // Decode type where the method token is defined
                    TypeHandle thMethodDefType = ptr.GetTypeHandleThrowing(
                        pLookupModule,
                        &typeContext,
                        (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes),
                        CLASS_LOADED,
                        FALSE,
                        NULL,
                        pZapSigContext);
                    if (thMethodDefType.IsNull())
                    {
                        _ASSERTE(nonExpansive);
                        return NULL;
                    }
                    IfFailThrow(ptr.SkipExactlyOne());
                    MethodTable * pMethodDefMT = thMethodDefType.GetMethodTable();
                    _ASSERTE(pMethodDefMT != NULL);

                    // decode method token
                    RID rid;
                    IfFailThrow(ptr.GetData(&rid));
                    mdMethodDef token = TokenFromRid(rid, mdtMethodDef);

                    // The RID map should have been filled out if we fully loaded the class
                    pMethod = pMethodDefMT->GetModule()->LookupMethodDef(token);
                    _ASSERTE(pMethod != NULL);
                    pMethod->CheckRestore();
                }
            }

            if (fRequiresDispatchStub)
            {
#ifndef CROSSGEN_COMPILE
                // Generate a dispatch stub and store it in the dictionary.
                //
                // We generate an indirection so we don't have to write to the dictionary
                // when we do updates, and to simplify stub indirect callsites.  Stubs stored in
                // dictionaries use "RegisterIndirect" stub calling, e.g. "call [eax]",
                // i.e. here the register "eax" would contain the value fetched from the dictionary,
                // which in turn points to the stub indirection which holds the value the current stub
                // address itself. If we just used "call eax" then we wouldn't know which stub indirection
                // to update.  If we really wanted to avoid the extra indirection we could return the _address_ of the
                // dictionary entry to the  caller, still using "call [eax]", and then the
                // stub dispatch mechanism can update the dictitonary itself and we don't
                // need an indirection.
                LoaderAllocator * pDictLoaderAllocator = (pMT != NULL) ? pMT->GetLoaderAllocator() : pMD->GetLoaderAllocator();

                VirtualCallStubManager * pMgr = pDictLoaderAllocator->GetVirtualCallStubManager();

                // We indirect through a cell so that updates can take place atomically.
                // The call stub and the indirection cell have the same lifetime as the dictionary itself, i.e.
                // are allocated in the domain of the dicitonary.
                //
                // In the case of overflow (where there is no dictionary, just a global hash table) then
                // the entry will be placed in the overflow hash table (JitGenericHandleCache).  This
                // is partitioned according to domain, i.e. is scraped each time an AppDomain gets unloaded.
                PCODE addr = pMgr->GetCallStub(ownerType, methodSlot);

                result = (CORINFO_GENERIC_HANDLE)pMgr->GenerateStubIndirection(addr);
                break;
#endif // CROSSGEN_COMPILE
            }

            Instantiation inst;

            // Instantiate the method if needed, or create a stub to a static method in a generic class.
            if (fMethodNeedsInstantiation)
            {
                DWORD nargs;
                IfFailThrow(ptr.GetData(&nargs));

                SIZE_T cbMem;

                if (!ClrSafeInt<SIZE_T>::multiply(nargs, sizeof(TypeHandle), cbMem/* passed by ref */))
                    ThrowHR(COR_E_OVERFLOW);

                TypeHandle * pInst = (TypeHandle*)_alloca(cbMem);
                for (DWORD i = 0; i < nargs; i++)
                {
                    pInst[i] = ptr.GetTypeHandleThrowing(
                        pLookupModule,
                        &typeContext,
                        ClassLoader::LoadTypes,
                        CLASS_LOADED,
                        FALSE,
                        NULL,
                        pZapSigContext);
                    IfFailThrow(ptr.SkipExactlyOne());
                }

                inst = Instantiation(pInst, nargs);
            }
            else
            {
                inst = pMethod->GetMethodInstantiation();
            }

            // This must be called even if nargs == 0, in order to create an instantiating
            // stub for static methods in generic classees if needed, also for BoxedEntryPointStubs
            // in non-generic structs.
            pMethod = MethodDesc::FindOrCreateAssociatedMethodDesc(
                pMethod,
                pOwnerMT,
                isUnboxingStub,
                inst,
                (!isInstantiatingStub && !isUnboxingStub));

            if (kind == ConstrainedMethodEntrySlot)
            {
                if (isReadyToRunModule)
                {
                    _ASSERTE((methodFlags & ENCODE_METHOD_SIG_Constrained) == ENCODE_METHOD_SIG_Constrained);

                    constraintType = ptr.GetTypeHandleThrowing(
                        pZapSigContext->pInfoModule,
                        &typeContext,
                        ClassLoader::LoadTypes,
                        CLASS_LOADED,
                        FALSE,
                        NULL,
                        pZapSigContext);
                }
                _ASSERTE(!constraintType.IsNull());

                MethodDesc *pResolvedMD = constraintType.GetMethodTable()->TryResolveConstraintMethodApprox(ownerType, pMethod);

                // All such calls should be resolvable.  If not then for now just throw an error.
                _ASSERTE(pResolvedMD);
                INDEBUG(if (!pResolvedMD) constraintType.GetMethodTable()->TryResolveConstraintMethodApprox(ownerType, pMethod);)
                if (!pResolvedMD)
                    COMPlusThrowHR(COR_E_BADIMAGEFORMAT);

                result = (CORINFO_GENERIC_HANDLE)pResolvedMD->GetMultiCallableAddrOfCode();
            }
            else
            if (kind == MethodEntrySlot)
            {
                result = (CORINFO_GENERIC_HANDLE)pMethod->GetMultiCallableAddrOfCode();
            }
            else
            if (kind == DispatchStubAddrSlot)
            {
                _ASSERTE((methodFlags & ENCODE_METHOD_SIG_SlotInsteadOfToken) == 0);
                PCODE *ppCode = (PCODE*)(void*)pMethod->GetLoaderAllocator()->GetHighFrequencyHeap()->AllocMem(S_SIZE_T(sizeof(PCODE)));
                *ppCode = pMethod->GetMultiCallableAddrOfCode();
                result = (CORINFO_GENERIC_HANDLE)ppCode;
            }
            else
            {
                _ASSERTE(kind == MethodDescSlot);
                result = (CORINFO_GENERIC_HANDLE)pMethod;
            }
            break;
        }

        case FieldDescSlot:
        {
            TypeHandle ownerType;

            if (isReadyToRunModule)
            {
                FieldDesc* pField = ZapSig::DecodeField((Module*)pZapSigContext->pModuleContext, pZapSigContext->pInfoModule, ptr.GetPtr(), &typeContext, &ownerType);
                _ASSERTE(!ownerType.IsNull());
                
                if (!IsCompilationProcess())
                    ownerType.AsMethodTable()->EnsureInstanceActive();

                result = (CORINFO_GENERIC_HANDLE)pField;
            }
            else
            {
                ownerType = ptr.GetTypeHandleThrowing(
                    pLookupModule,
                    &typeContext,
                    (nonExpansive ? ClassLoader::DontLoadTypes : ClassLoader::LoadTypes),
                    CLASS_LOADED,
                    FALSE,
                    NULL,
                    pZapSigContext);
                if (ownerType.IsNull())
                {
                    _ASSERTE(nonExpansive);
                    return NULL;
                }
                IfFailThrow(ptr.SkipExactlyOne());

                DWORD fieldIndex;
                IfFailThrow(ptr.GetData(&fieldIndex));

                if (!IsCompilationProcess())
                    ownerType.AsMethodTable()->EnsureInstanceActive();

                result = (CORINFO_GENERIC_HANDLE)ownerType.AsMethodTable()->GetFieldDescByIndex(fieldIndex);
            }
            break;
        }

        default:
            _ASSERTE(!"Invalid DictionaryEntryKind");
            break;
        }

        ULONG slotIndex;
        if (isReadyToRunModule)
        {
            _ASSERT(dictionaryIndexAndSlot != -1);
            slotIndex = (ULONG)(dictionaryIndexAndSlot & 0xFFFF);
        }
        else
        {
            IfFailThrow(ptr.GetData(&slotIndex));
        }

        MemoryBarrier();

        if ((slotIndex != 0) && !IsCompilationProcess())
        {
            *EnsureWritablePages(pDictionary->GetSlotAddr(0, slotIndex)) = result;
            *ppSlot = pDictionary->GetSlotAddr(0, slotIndex);
        }
    }

    return result;
} // Dictionary::PopulateEntry

//---------------------------------------------------------------------------------------
// 
void 
Dictionary::PrepopulateDictionary(
    MethodDesc *  pMD, 
    MethodTable * pMT, 
    BOOL          nonExpansive)
{
    STANDARD_VM_CONTRACT;

    DictionaryLayout * pDictLayout = (pMT != NULL) ? pMT->GetClass()->GetDictionaryLayout() : pMD->GetDictionaryLayout();
    DWORD numGenericArgs = (pMT != NULL) ? pMT->GetNumGenericArgs() : pMD->GetNumGenericMethodArgs();

    if (pDictLayout != NULL)
    {
        for (DWORD i = 0; i < pDictLayout->GetNumUsedSlots(); i++)
        {
            if (IsSlotEmpty(numGenericArgs,i))
            {
                DictionaryEntry * pSlot;
                DictionaryEntry entry;
                entry = PopulateEntry(
                    pMD, 
                    pMT, 
                    pDictLayout->GetEntryLayout(i)->m_signature, 
                    nonExpansive, 
                    &pSlot);
                
                _ASSERT((entry == NULL) || (entry == GetSlot(numGenericArgs,i)) || IsCompilationProcess());
                _ASSERT((pSlot == NULL) || (pSlot == GetSlotAddr(numGenericArgs,i)));
            }
        }
    }
} // Dictionary::PrepopulateDictionary

#endif //!DACCESS_COMPILE
