// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


/*============================================================
**
** Header:  Map used for interning of string literals.
**
===========================================================*/

#include "common.h"
#include "eeconfig.h"
#include "stringliteralmap.h"

/*
    Thread safety in GlobalStringLiteralMap / StringLiteralMap

    A single lock protects the N StringLiteralMap objects and single
    GlobalStringLiteralMap rooted in the SystemDomain at any time. It is

    TEntryType::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal

    At one time each StringLiteralMap had it's own lock to protect
    the entry hash table as well, and Interlocked operations were done on the
    ref count of the contained StringLiteralEntries. But anything of import
    needed to be done under the global lock mentioned above or races would
    result. (For example, an app domain shuts down, doing final release on
    a StringLiteralEntry, but at that moment the entry is being handed out
    in another appdomain and addref'd only after the count went to 0.)

    The rule is:

    Any AddRef()/Release() calls on StringLiteralEntry need to be under the lock.
    Any insert/deletes from the StringLiteralMap or GlobalStringLiteralMap
    need to be done under the lock.

    The only thing you can do without the lock is look up an existing StringLiteralEntry
    in an StringLiteralMap hash table. This is true because these lookup calls
    will all come before destruction of the map, the hash table is safe for multiple readers,
    and we know the StringLiteralEntry so found 1) can't be destroyed because that table keeps
    an AddRef on it and 2) isn't internally modified once created.
*/
    
StringLiteralEntryArray *StringLiteralEntry::s_EntryList = NULL;
DWORD StringLiteralEntry::s_UsedEntries = NULL;
StringLiteralEntry *StringLiteralEntry::s_FreeEntryList = NULL;

Utf8StringLiteralEntryArray *Utf8StringLiteralEntry::s_EntryList = NULL;
DWORD Utf8StringLiteralEntry::s_UsedEntries = NULL;
Utf8StringLiteralEntry *Utf8StringLiteralEntry::s_FreeEntryList = NULL;

#ifdef LOGGING
void LogStringLiteral(__in_z const char* action, EEStringData *pStringData)
{
    STATIC_CONTRACT_NOTHROW;
    STATIC_CONTRACT_GC_NOTRIGGER;
    STATIC_CONTRACT_FORBID_FAULT;

    int length = pStringData->GetCharCount();
    length = min(length, 100);
    if (pStringData->GetIsUtf8())
    {
        CHAR *szString = (CHAR *)_alloca((length + 1) * sizeof(CHAR));
        memcpyNoGCRefs((void*)szString, (void*)pStringData->GetUTF8StringBuffer(), length * sizeof(CHAR));
        szString[length] = '\0';
        LOG((LF_APPDOMAIN, LL_INFO10000, "String literal \"%s\" %s to Global map, size %d bytes\n", szString, action, pStringData->GetCharCount()));
    }
    else
    {
        WCHAR *szString = (WCHAR *)_alloca((length + 1) * sizeof(WCHAR));
        memcpyNoGCRefs((void*)szString, (void*)pStringData->GetUTF16StringBuffer(), length * sizeof(WCHAR));
        szString[length] = '\0';
        LOG((LF_APPDOMAIN, LL_INFO10000, "String literal \"%S\" %s to Global map, size %d bytes\n", szString, action, pStringData->GetCharCount() * 2));
    }
}
#endif

STRINGREF AllocateStringObject(EEStringData *pStringData)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;  
    }
    CONTRACTL_END;  

    // No path that gets here should be able to have a Utf8 string
    _ASSERT(!pStringData->GetIsUtf8()); 

    // Create the COM+ string object.
    DWORD cCount = pStringData->GetCharCount();
    
    STRINGREF strObj = AllocateString(cCount);

    GCPROTECT_BEGIN(strObj)
    {
        // Copy the string constant into the COM+ string object.  The code
        // will add an extra null at the end for safety purposes, but since
        // we support embedded nulls, one should never treat the string as
        // null termianted.
        LPWSTR strDest = strObj->GetBuffer();
        memcpyNoGCRefs(strDest, pStringData->GetUTF16StringBuffer(), cCount*sizeof(WCHAR));
        strDest[cCount] = 0;
    }
    GCPROTECT_END();

    return strObj;
}

UTF8STRINGREF AllocateUTF8StringObject(EEStringData *pStringData)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;  
    }
    CONTRACTL_END;  
   
    // Create the COM+ string object.
    DWORD cCount = pStringData->GetCharCount();
    
    DWORD utf8CharacterCount;

    if (pStringData->GetIsUtf8() || (cCount == 0))
    {
        utf8CharacterCount = cCount;
    }
    else
    {
        utf8CharacterCount = WszWideCharToMultiByte(CP_UTF8, WC_ERR_INVALID_CHARS, pStringData->GetUTF16StringBuffer(), cCount, NULL, 0, NULL, NULL);
        if (utf8CharacterCount == 0)
        {
            COMPlusThrow(kInvalidProgramException);
        }
    }

    UTF8STRINGREF strObj = AllocateUtf8String(utf8CharacterCount);

    GCPROTECT_BEGIN(strObj)
    {
        // Copy the string constant into the COM+ string object.  The code
        // will add an extra null at the end for safety purposes, but since
        // we support embedded nulls, one should never treat the string as
        // null termianted.
        LPSTR strDest = strObj->GetBuffer();
        if (pStringData->GetIsUtf8())
        {
            memcpyNoGCRefs(strDest, pStringData->GetUTF8StringBuffer(), utf8CharacterCount);
        }
        else if (utf8CharacterCount == 0)
        {
            // do no copying
        }
        else
        {
            // Convert UTF16 data into UTF8 and place into UTF8 string
            int convertedCharacterCount = WszWideCharToMultiByte(CP_UTF8, WC_ERR_INVALID_CHARS, pStringData->GetUTF16StringBuffer(), cCount, strDest, utf8CharacterCount, NULL, NULL);
            if (convertedCharacterCount != utf8CharacterCount)
            {
                COMPlusThrow(kInvalidProgramException);
            }
        }
        strDest[utf8CharacterCount] = 0;
    }
    GCPROTECT_END();

    return strObj;
}

StringLiteralEntry *StringLiteralEntry::AllocateEntry(EEStringData *pStringData, STRINGREF *pStringObj)
{
   CONTRACTL
    {
        THROWS;
        GC_TRIGGERS; // GC_TRIGGERS because in the precondition below GetGlobalStringLiteralMap() might need to create the map
        MODE_COOPERATIVE;
        PRECONDITION(StringLiteralEntry::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal.OwnedByCurrentThread());
    }
    CONTRACTL_END; 

    // Note: we don't synchronize here because allocateEntry is called when HashCrst is held.
    void *pMem = NULL;
    if (s_FreeEntryList != NULL)
    {
        pMem = s_FreeEntryList;
        s_FreeEntryList = s_FreeEntryList->m_pNext;
        _ASSERTE (((StringLiteralEntry*)pMem)->m_bDeleted);        
    }
    else
    {
        if (s_EntryList == NULL || (s_UsedEntries >= MAX_ENTRIES_PER_CHUNK))
        {
            StringLiteralEntryArray *pNew = new StringLiteralEntryArray();
            pNew->m_pNext = s_EntryList;
            s_EntryList = pNew;
            s_UsedEntries = 0;
        }
        pMem = &(s_EntryList->m_Entries[s_UsedEntries++*sizeof(StringLiteralEntry)]);
    }
    _ASSERTE (pMem && "Unable to allocate String literal Entry");

    return new (pMem) StringLiteralEntry (pStringData, pStringObj);
}

void StringLiteralEntry::DeleteEntry (StringLiteralEntry *pEntry)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        PRECONDITION(SystemDomain::GetGlobalStringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());
    }
    CONTRACTL_END; 

    _ASSERTE (VolatileLoad(&pEntry->m_dwRefCount) == 0);
    
#ifdef _DEBUG
    memset (pEntry, 0xc, sizeof(StringLiteralEntry));
#endif

#ifdef _DEBUG        
    pEntry->m_bDeleted = TRUE;
#endif
    
    // The free list needs protection from the m_HashTableCrstGlobal
    pEntry->m_pNext = s_FreeEntryList;
    s_FreeEntryList = pEntry;
}

Utf8StringLiteralEntry *Utf8StringLiteralEntry::AllocateEntry(EEStringData *pStringData, UTF8STRINGREF *pStringObj)
{
   CONTRACTL
    {
        THROWS;
        GC_TRIGGERS; // GC_TRIGGERS because in the precondition below GetGlobalStringLiteralMap() might need to create the map
        MODE_COOPERATIVE;
        PRECONDITION(Utf8StringLiteralEntry::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal.OwnedByCurrentThread());
    }
    CONTRACTL_END; 

    // Note: we don't synchronize here because allocateEntry is called when HashCrst is held.
    void *pMem = NULL;
    if (s_FreeEntryList != NULL)
    {
        pMem = s_FreeEntryList;
        s_FreeEntryList = s_FreeEntryList->m_pNext;
        _ASSERTE (((Utf8StringLiteralEntry*)pMem)->m_bDeleted);        
    }
    else
    {
        if (s_EntryList == NULL || (s_UsedEntries >= MAX_ENTRIES_PER_CHUNK))
        {
            Utf8StringLiteralEntryArray *pNew = new Utf8StringLiteralEntryArray();
            pNew->m_pNext = s_EntryList;
            s_EntryList = pNew;
            s_UsedEntries = 0;
        }
        pMem = &(s_EntryList->m_Entries[s_UsedEntries++*sizeof(StringLiteralEntry)]);
    }
    _ASSERTE (pMem && "Unable to allocate String literal Entry");

    return new (pMem) Utf8StringLiteralEntry (pStringData, pStringObj);
}

void Utf8StringLiteralEntry::DeleteEntry (Utf8StringLiteralEntry *pEntry)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        PRECONDITION(SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());
    }
    CONTRACTL_END; 

    _ASSERTE (VolatileLoad(&pEntry->m_dwRefCount) == 0);
    
#ifdef _DEBUG
    memset (pEntry, 0xc, sizeof(Utf8StringLiteralEntry));
#endif

#ifdef _DEBUG        
    pEntry->m_bDeleted = TRUE;
#endif
    
    // The free list needs protection from the m_HashTableCrstGlobal
    pEntry->m_pNext = s_FreeEntryList;
    s_FreeEntryList = pEntry;
}
