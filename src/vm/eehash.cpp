// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: eehash.cpp
//

//


#include "common.h"
#include "excep.h"
#include "eehash.h"
#include "stringliteralmap.h"
#include "clsload.hpp"
#include "typectxt.h"
#include "genericdict.h"

// ============================================================================
// UTF8 string hash table helper.
// ============================================================================
EEHashEntry_t * EEUtf8HashTableHelper::AllocateEntry(LPCUTF8 pKey, BOOL bDeepCopy, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        INJECT_FAULT(return NULL;);
    }
    CONTRACTL_END

    EEHashEntry_t *pEntry;

    if (bDeepCopy)
    {
        DWORD StringLen = (DWORD)strlen(pKey);
        DWORD BufLen = 0;
// Review conversion of size_t to DWORD.
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4267)
#endif
        if (!ClrSafeInt<DWORD>::addition(StringLen, SIZEOF_EEHASH_ENTRY + sizeof(LPUTF8) + 1, BufLen))
#ifdef _MSC_VER
#pragma warning(pop)
#endif
            return NULL;
        pEntry = (EEHashEntry_t *) new (nothrow) BYTE[BufLen];
        if (!pEntry)
            return NULL;

        memcpy(pEntry->Key + sizeof(LPUTF8), pKey, StringLen + 1); 
        *((LPUTF8*)pEntry->Key) = (LPUTF8)(pEntry->Key + sizeof(LPUTF8));
    }
    else
    {
        pEntry = (EEHashEntry_t *) new (nothrow)BYTE[SIZEOF_EEHASH_ENTRY + sizeof(LPUTF8)];
        if (pEntry)
            *((LPCUTF8*)pEntry->Key) = pKey;
    }

    return pEntry;
}


void EEUtf8HashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        FORBID_FAULT;
    }
    CONTRACTL_END

    delete [] (BYTE*)pEntry;
}


BOOL EEUtf8HashTableHelper::CompareKeys(EEHashEntry_t *pEntry, LPCUTF8 pKey)
{
    LIMITED_METHOD_DAC_CONTRACT;

    LPCUTF8 pEntryKey = *((LPCUTF8*)pEntry->Key);
    return (strcmp(pEntryKey, pKey) == 0) ? TRUE : FALSE;
}


DWORD EEUtf8HashTableHelper::Hash(LPCUTF8 pKey)
{
    LIMITED_METHOD_DAC_CONTRACT;

    DWORD dwHash = 0;

    while (*pKey != 0)
    {
        dwHash = (dwHash << 5) + (dwHash >> 5) + (*pKey);
        pKey++;
    }

    return dwHash;
}


LPCUTF8 EEUtf8HashTableHelper::GetKey(EEHashEntry_t *pEntry)
{
    LIMITED_METHOD_CONTRACT;

    return *((LPCUTF8*)pEntry->Key);
}

#ifndef DACCESS_COMPILE

// ============================================================================
// Unicode string hash table helper.
// ============================================================================
EEHashEntry_t * EEUnicodeHashTableHelper::AllocateEntry(EEStringData *pKey, BOOL bDeepCopy, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        INJECT_FAULT(return NULL;);
    }
    CONTRACTL_END

    EEHashEntry_t *pEntry;
    _ASSERTE(!pKey->GetIsUtf8());

    if (bDeepCopy)
    {
        pEntry = (EEHashEntry_t *) new (nothrow) BYTE[SIZEOF_EEHASH_ENTRY + sizeof(EEStringData) + ((pKey->GetCharCount() + 1) * sizeof(WCHAR))];
        if (pEntry) {
            EEStringData *pEntryKey = (EEStringData *)(&pEntry->Key);
            pEntryKey->SetCharCount (pKey->GetCharCount());
            pEntryKey->SetUTF16StringBuffer ((LPWSTR) ((LPBYTE)pEntry->Key + sizeof(EEStringData)));
            memcpy((LPWSTR)pEntryKey->GetUTF16StringBuffer(), pKey->GetUTF16StringBuffer(), pKey->GetCharCount() * sizeof(WCHAR)); 
        }
    }
    else
    {
        pEntry = (EEHashEntry_t *) new (nothrow) BYTE[SIZEOF_EEHASH_ENTRY + sizeof(EEStringData)];
        if (pEntry) {
            EEStringData *pEntryKey = (EEStringData *) pEntry->Key;
            pEntryKey->SetCharCount (pKey->GetCharCount());
            pEntryKey->SetUTF16StringBuffer (pKey->GetUTF16StringBuffer());
        }
    }

    return pEntry;
}


void EEUnicodeHashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, void *pHeap)
{
    LIMITED_METHOD_CONTRACT;

    delete [] (BYTE*)pEntry;
}


BOOL EEUnicodeHashTableHelper::CompareKeys(EEHashEntry_t *pEntry, EEStringData *pKey)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(!pKey->GetIsUtf8());

    EEStringData *pEntryKey = (EEStringData*) pEntry->Key;

    // Same buffer, same string.
    if (pEntryKey->GetUTF16StringBuffer() == pKey->GetUTF16StringBuffer())
        return TRUE;

    // Length not the same, never a match.
    if (pEntryKey->GetCharCount() != pKey->GetCharCount())
        return FALSE;

    // Compare the entire thing.
    // We'll deliberately ignore the bOnlyLowChars field since this derived from the characters
    return !memcmp(pEntryKey->GetUTF16StringBuffer(), pKey->GetUTF16StringBuffer(), pEntryKey->GetCharCount() * sizeof(WCHAR));
}


DWORD EEUnicodeHashTableHelper::Hash(EEStringData *pKey)
{
    LIMITED_METHOD_CONTRACT;

    return (HashBytes((const BYTE *) pKey->GetUTF16StringBuffer(), pKey->GetCharCount()*sizeof(WCHAR)));
}


EEStringData *EEUnicodeHashTableHelper::GetKey(EEHashEntry_t *pEntry)
{
    LIMITED_METHOD_CONTRACT;

    return (EEStringData*)pEntry->Key;
}

void EEUnicodeHashTableHelper::ReplaceKey(EEHashEntry_t *pEntry, EEStringData *pNewKey)
{
    LIMITED_METHOD_CONTRACT;

    ((EEStringData*)pEntry->Key)->SetUTF16StringBuffer (pNewKey->GetUTF16StringBuffer());
    ((EEStringData*)pEntry->Key)->SetCharCount (pNewKey->GetCharCount());
}

// ============================================================================
// Unicode stringliteral hash table helper.
// ============================================================================
EEHashEntry_t * EEUnicodeStringLiteralHashTableHelper::AllocateEntry(EEStringData *pKey, BOOL bDeepCopy, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        INJECT_FAULT(return NULL;);
    }
    CONTRACTL_END

    // We assert here because we expect that the heap is not null for EEUnicodeStringLiteralHash table. 
    // If someone finds more uses of this kind of hashtable then remove this asserte. 
    // Also note that in case of heap being null we go ahead and use new /delete which is EXPENSIVE
    // But for production code this might be ok if the memory is fragmented then thers a better chance 
    // of getting smaller allocations than full pages.
    _ASSERTE (pHeap);

    if (pHeap)
        return (EEHashEntry_t *) ((MemoryPool*)pHeap)->AllocateElementNoThrow ();
    else
        return (EEHashEntry_t *) new (nothrow) BYTE[SIZEOF_EEHASH_ENTRY];
}


void EEUnicodeStringLiteralHashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        FORBID_FAULT;
    }
    CONTRACTL_END

    // We assert here because we expect that the heap is not null for EEUnicodeStringLiteralHash table. 
    // If someone finds more uses of this kind of hashtable then remove this asserte. 
    // Also note that in case of heap being null we go ahead and use new /delete which is EXPENSIVE
    // But for production code this might be ok if the memory is fragmented then thers a better chance 
    // of getting smaller allocations than full pages.
    _ASSERTE (pHeap);

    if (pHeap)
        ((MemoryPool*)pHeap)->FreeElement(pEntry);
    else
        delete [] (BYTE*)pEntry;
}

BOOL EEUnicodeStringLiteralHashTableHelper::CompareKeys(EEHashEntry_t *pEntry, EEStringData *pKey)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        FORBID_FAULT;
    }
    CONTRACTL_END

    GCX_COOP();
    
    StringLiteralEntry *pHashData = (StringLiteralEntry *)pEntry->Data;

    EEStringData pEntryKey;
    pHashData->GetStringData(&pEntryKey);

    // Length not the same, never a match.
    if (pEntryKey.GetCharCount() != pKey->GetCharCount())
        return FALSE;

    // Compare the entire thing.
    return (!memcmp(pEntryKey.GetUTF16StringBuffer(), pKey->GetUTF16StringBuffer(), pEntryKey.GetCharCount() * sizeof(WCHAR)));
}

DWORD EEUnicodeStringLiteralHashTableHelper::Hash(EEStringData *pKey)
{
    LIMITED_METHOD_CONTRACT;

    return (HashBytes((const BYTE *) pKey->GetUTF16StringBuffer(), pKey->GetCharCount() * sizeof(WCHAR)));
}

EEHashEntry_t * EEUnicodeUtf8StringLiteralHashTableHelper::AllocateEntry(EEStringData *pKey, BOOL bDeepCopy, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        INJECT_FAULT(return NULL;);
    }
    CONTRACTL_END

    // We assert here because we expect that the heap is not null for EEUnicodeStringLiteralHash table. 
    // If someone finds more uses of this kind of hashtable then remove this asserte. 
    // Also note that in case of heap being null we go ahead and use new /delete which is EXPENSIVE
    // But for production code this might be ok if the memory is fragmented then thers a better chance 
    // of getting smaller allocations than full pages.
    _ASSERTE (pHeap);

    if (pHeap)
        return (EEHashEntry_t *) ((MemoryPool*)pHeap)->AllocateElementNoThrow ();
    else
        return (EEHashEntry_t *) new (nothrow) BYTE[SIZEOF_EEHASH_ENTRY];
}


void EEUnicodeUtf8StringLiteralHashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        FORBID_FAULT;
    }
    CONTRACTL_END

    // We assert here because we expect that the heap is not null for EEUnicodeStringLiteralHash table. 
    // If someone finds more uses of this kind of hashtable then remove this asserte. 
    // Also note that in case of heap being null we go ahead and use new /delete which is EXPENSIVE
    // But for production code this might be ok if the memory is fragmented then thers a better chance 
    // of getting smaller allocations than full pages.
    _ASSERTE (pHeap);

    if (pHeap)
        ((MemoryPool*)pHeap)->FreeElement(pEntry);
    else
        delete [] (BYTE*)pEntry;
}

bool IsCharacter1ByteUtf8(char c)
{
    LIMITED_METHOD_CONTRACT;
    return (c & 0x80) == 0;
}

bool IsCharacterFirstCodeUnitInMultibyteUtf8CodePoint(char c)
{
    LIMITED_METHOD_CONTRACT;
    return (c & 0xC) == 0xC;
}

template <class TStateVar, class TUtf16DataFunctor>
bool PerformOperationAcrossUtf8StringAsUtf16(const CHAR *pUtf8CurrentPtr, DWORD utf8CodeUnits, TStateVar *pStateVar, TUtf16DataFunctor functor)
{
    WRAPPER_NO_CONTRACT;

#ifdef _DEBUG
    // Use a much smaller buffer in debug builds to ensure testing of the multi-buffer scenario
    WCHAR szBuf[16];
#else
    WCHAR szBuf[512];
#endif
    while (utf8CodeUnits != 0)
    {
        // Compare around 512 UTF16 Code Units at a time
        int charactersTranslated;

        // Case 1: Less than or equal to _countof(szBuf) UTF8 Code Units to feed in
        if (utf8CodeUnits <= _countof(szBuf))
        {
            charactersTranslated = WszMultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, pUtf8CurrentPtr, utf8CodeUnits, szBuf, _countof(szBuf));
            utf8CodeUnits = 0;
        }
        else
        {
            // Case 2: going to have to split the utf8 data.

            // Find the start of a utf8 code point
            int charactersToTranslate = _countof(szBuf);
            int iterationsToFindValidPointToTruncate = 0;

            while (!IsCharacter1ByteUtf8(pUtf8CurrentPtr[charactersToTranslate - 1]))
            {
                if (IsCharacterFirstCodeUnitInMultibyteUtf8CodePoint(pUtf8CurrentPtr[charactersToTranslate - 1]))
                {
                    charactersToTranslate -= 1;
                    break;
                }

                charactersToTranslate--;
                if (iterationsToFindValidPointToTruncate > 5)
                {
                    // An invalid UTF8 String cannot equal a UTF16 string of any form.
                    // There must be a valid truncation point within 5 characters
                    return false;
                }
                iterationsToFindValidPointToTruncate++;
            }

            charactersTranslated = WszMultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, pUtf8CurrentPtr, charactersToTranslate, szBuf, _countof(szBuf));
            utf8CodeUnits -= charactersToTranslate;
            pUtf8CurrentPtr += charactersToTranslate;
        }

        if (charactersTranslated == 0)
        {
            // Error during conversion from UTF8 to UTF16. 
            // An invalid UTF8 String cannot equal a UTF16 string of any form.
            // Therefore return FALSE
            return false;
        }

        if (!functor.ProcessUtf8Data(pStateVar, szBuf, charactersTranslated))
        {
            return false;
        }
    }

    return true;
}

BOOL CompareEEStringDataMixedUtf8Utf16(EEStringData *pKey, EEStringData *pEntryKey)
{
    LIMITED_METHOD_CONTRACT;

    // Empty strings are equivalent
    if ((pEntryKey->GetCharCount() == 0) && (pKey->GetCharCount() == 0))
        return TRUE;

    // Mixed compare
    struct Utf16CompareState
    {
        const WCHAR *pUtf16CurrentPtr;
        DWORD utf16CodeUnits;
    } utf16CompareState;

    class Utf16CompareDataFunctor
    {
        public:
        bool ProcessUtf8Data(Utf16CompareState *pStateVar, const WCHAR *pUtf16Data, DWORD cchUtf16Data)
        {
            if (cchUtf16Data > pStateVar->utf16CodeUnits)
            {
                // UTF8 to UTF16 conversion resulted in a UTF16 string longer than the remaining UTF16 data to compare to
                // Therefore, the strings must not be equal
                return false;
            }

            if (memcmp(pUtf16Data, pStateVar->pUtf16CurrentPtr, cchUtf16Data * sizeof(WCHAR)) != 0)
            {
                // Comparison of UTF16 string data indicates that the strings are not identical
                return false;
            }

            // Advance pointers and reduce counts
            pStateVar->pUtf16CurrentPtr += cchUtf16Data;
            pStateVar->utf16CodeUnits -= cchUtf16Data;

            return true;
        }
    };

    const CHAR *pUtf8Ptr;
    DWORD utf8CodeUnits;

    if (pEntryKey->GetIsUtf8())
    {
        pUtf8Ptr = pEntryKey->GetUTF8StringBuffer();
        utf8CodeUnits = pEntryKey->GetCharCount();
        utf16CompareState.pUtf16CurrentPtr = pKey->GetUTF16StringBuffer();
        utf16CompareState.utf16CodeUnits = pKey->GetCharCount();
    }
    else
    {
        pUtf8Ptr = pKey->GetUTF8StringBuffer();
        utf8CodeUnits = pKey->GetCharCount();
        utf16CompareState.pUtf16CurrentPtr = pEntryKey->GetUTF16StringBuffer();
        utf16CompareState.utf16CodeUnits = pEntryKey->GetCharCount();
    }

    if (!PerformOperationAcrossUtf8StringAsUtf16(pUtf8Ptr, utf8CodeUnits, &utf16CompareState, Utf16CompareDataFunctor()))
    {
        // Conversion to Utf16 failed, or comparison failed
        return FALSE;
    }
    // At this point, all of the utf8 data has been processed

    if (utf16CompareState.utf16CodeUnits != 0)
    {
        // If the utf16 data isn't complete, then the strings aren't equal
        return FALSE;
    }

    return TRUE;
}

BOOL EEUnicodeUtf8StringLiteralHashTableHelper::CompareKeys(EEHashEntry_t *pEntry, EEStringData *pKey)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        FORBID_FAULT;
    }
    CONTRACTL_END

    GCX_COOP();
    
    Utf8StringLiteralEntry *pHashData = (Utf8StringLiteralEntry *)pEntry->Data;

    EEStringData pEntryKey;
    pHashData->GetStringData(&pEntryKey);

    // Compare the entire thing.
    if (pKey->GetIsUtf8() == pEntryKey.GetIsUtf8())
    {
        // Length not the same, never a match.
        if (pEntryKey.GetCharCount() != pKey->GetCharCount())
            return FALSE;

        if (pKey->GetIsUtf8())
        {
            return (!memcmp(pEntryKey.GetUTF8StringBuffer(), pKey->GetUTF8StringBuffer(), pEntryKey.GetCharCount() * sizeof(CHAR)));
        }
        else
        {
            return (!memcmp(pEntryKey.GetUTF16StringBuffer(), pKey->GetUTF16StringBuffer(), pEntryKey.GetCharCount() * sizeof(WCHAR)));
        }
    }
    else
    {
        return CompareEEStringDataMixedUtf8Utf16(pKey, &pEntryKey);
    }
}

DWORD EEUnicodeUtf8StringLiteralHashTableHelper::Hash(EEStringData *pKey)
{
    LIMITED_METHOD_CONTRACT;

    if (!pKey->GetIsUtf8())
    {
        return (HashBytes((const BYTE *) pKey->GetUTF16StringBuffer(), pKey->GetCharCount() * sizeof(WCHAR)));
    }
    else
    {
        // Generally try to convert Utf8 data to Utf16 and hash, but if the string isn't convertable, hash the entire thing
        HashBytesState hashState;
        InitHashBytesState(&hashState);

        class HashBytesFunctor
        {
        public:
            bool ProcessUtf8Data(HashBytesState *hashState, const WCHAR *pUtf16Data, DWORD cchUtf16Data)
            {
                LIMITED_METHOD_CONTRACT;
                AddBytesToHash(hashState, (const BYTE *)pUtf16Data, cchUtf16Data * sizeof(WCHAR));
                return true;
            }
        };

        if (!PerformOperationAcrossUtf8StringAsUtf16(pKey->GetUTF8StringBuffer(), pKey->GetCharCount(), &hashState, HashBytesFunctor()))
        {
            // Conversion to Utf16 failed, just hash the Utf8 data
            return (HashBytes((const BYTE *) pKey->GetUTF8StringBuffer(), pKey->GetCharCount() * sizeof(CHAR)));
        }
        else
        {
            // Hashing of UTF16 converted data complete, return computed hash
            return GetHashFromHashBytesState(&hashState);
        }
    }
}


// ============================================================================
// Instantiation hash table helper.
// ============================================================================

EEHashEntry_t *EEInstantiationHashTableHelper::AllocateEntry(const SigTypeContext *pKey, BOOL bDeepCopy, AllocationHeap pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
    }
    CONTRACTL_END
    
    EEHashEntry_t *pEntry = (EEHashEntry_t *) new (nothrow) BYTE[SIZEOF_EEHASH_ENTRY + sizeof(SigTypeContext)];
    if (!pEntry)
        return NULL;
    *((SigTypeContext*)pEntry->Key) = *pKey;

    return pEntry;
}

void EEInstantiationHashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, AllocationHeap pHeap)
{
    LIMITED_METHOD_CONTRACT;

    delete [] (BYTE*)pEntry;
}

BOOL EEInstantiationHashTableHelper::CompareKeys(EEHashEntry_t *pEntry, const SigTypeContext *pKey)
{
    LIMITED_METHOD_CONTRACT;

    SigTypeContext *pThis = (SigTypeContext*)&pEntry->Key;
    return SigTypeContext::Equal(pThis, pKey);
}

DWORD EEInstantiationHashTableHelper::Hash(const SigTypeContext *pKey)
{
    LIMITED_METHOD_CONTRACT;

    DWORD dwHash = 5381;
    DWORD i;

    for (i = 0; i < pKey->m_classInst.GetNumArgs(); i++)
        dwHash = ((dwHash << 5) + dwHash) ^ (unsigned int)(SIZE_T)pKey->m_classInst[i].AsPtr();

    for (i = 0; i < pKey->m_methodInst.GetNumArgs(); i++)
        dwHash = ((dwHash << 5) + dwHash) ^ (unsigned int)(SIZE_T)pKey->m_methodInst[i].AsPtr();

    return dwHash;
}

const SigTypeContext *EEInstantiationHashTableHelper::GetKey(EEHashEntry_t *pEntry)
{
    LIMITED_METHOD_CONTRACT;

    return (const SigTypeContext*)&pEntry->Key;
}



// ============================================================================
// ComComponentInfo hash table helper.
// ============================================================================

EEHashEntry_t *EEClassFactoryInfoHashTableHelper::AllocateEntry(ClassFactoryInfo *pKey, BOOL bDeepCopy, void *pHeap)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        INJECT_FAULT(return NULL;);
    }
    CONTRACTL_END

    EEHashEntry_t *pEntry;
    S_SIZE_T cbStringLen = S_SIZE_T(0);

    _ASSERTE(bDeepCopy && "Non deep copy is not supported by the EEComCompInfoHashTableHelper");

    if (pKey->m_strServerName)
        cbStringLen = (S_SIZE_T(wcslen(pKey->m_strServerName)) + S_SIZE_T(1)) * S_SIZE_T(sizeof(WCHAR));

    S_SIZE_T cbEntry = S_SIZE_T(SIZEOF_EEHASH_ENTRY + sizeof(ClassFactoryInfo)) + cbStringLen;

    if (cbEntry.IsOverflow())
        return NULL;

    _ASSERTE(!cbStringLen.IsOverflow());

    pEntry = (EEHashEntry_t *) new (nothrow) BYTE[cbEntry.Value()];
    if (pEntry) {
        memcpy(pEntry->Key + sizeof(ClassFactoryInfo), pKey->m_strServerName, cbStringLen.Value()); 
        ((ClassFactoryInfo*)pEntry->Key)->m_strServerName = pKey->m_strServerName ? (WCHAR*)(pEntry->Key + sizeof(ClassFactoryInfo)) : NULL;
        ((ClassFactoryInfo*)pEntry->Key)->m_clsid = pKey->m_clsid;
    }

    return pEntry;
}

void EEClassFactoryInfoHashTableHelper::DeleteEntry(EEHashEntry_t *pEntry, void *pHeap)
{
    LIMITED_METHOD_CONTRACT;

    delete [] (BYTE*) pEntry;
}

BOOL EEClassFactoryInfoHashTableHelper::CompareKeys(EEHashEntry_t *pEntry, ClassFactoryInfo *pKey)
{
    LIMITED_METHOD_CONTRACT;

    // First check the GUIDs.
    if (((ClassFactoryInfo*)pEntry->Key)->m_clsid != pKey->m_clsid)
        return FALSE;

    // Next do a trivial comparition on the server name pointer values.
    if (((ClassFactoryInfo*)pEntry->Key)->m_strServerName == pKey->m_strServerName)
        return TRUE;

    // If the pointers are not equal then if one is NULL then the server names are different.
    if (!((ClassFactoryInfo*)pEntry->Key)->m_strServerName || !pKey->m_strServerName)
        return FALSE;

    // Finally do a string comparition of the server names.
    return wcscmp(((ClassFactoryInfo*)pEntry->Key)->m_strServerName, pKey->m_strServerName) == 0;
}

DWORD EEClassFactoryInfoHashTableHelper::Hash(ClassFactoryInfo *pKey)
{
    LIMITED_METHOD_CONTRACT;

    DWORD dwHash = 0;
    BYTE *pGuidData = (BYTE*)&pKey->m_clsid;

    for (unsigned int i = 0; i < sizeof(GUID); i++)
    {
        dwHash = (dwHash << 5) + (dwHash >> 5) + (*pGuidData);
        pGuidData++;
    }

    if (pKey->m_strServerName)
    {
        WCHAR *pSrvNameData = pKey->m_strServerName;

        while (*pSrvNameData != 0)
        {
            dwHash = (dwHash << 5) + (dwHash >> 5) + (*pSrvNameData);
            pSrvNameData++;
        }
    }

    return dwHash;
}

ClassFactoryInfo *EEClassFactoryInfoHashTableHelper::GetKey(EEHashEntry_t *pEntry)
{
    LIMITED_METHOD_CONTRACT;

    return (ClassFactoryInfo*)pEntry->Key;
}
#endif // !DACCESS_COMPILE
