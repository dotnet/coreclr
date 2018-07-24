// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


/*============================================================
**
** Header:  Map used for interning of string literals.
**
===========================================================*/

#ifndef _STRINGLITERALMAP_H
#define _STRINGLITERALMAP_H

#include "vars.hpp"
#include "appdomain.hpp"
#include "eehash.h"
#include "eeconfig.h" // For OS pages size
#include "memorypool.h"


// Allocate 16 entries (approx size sizeof(StringLiteralEntry)*16)
#define MAX_ENTRIES_PER_CHUNK 16
#define INIT_NUM_APP_DOMAIN_STRING_BUCKETS 59
#define INIT_NUM_GLOBAL_STRING_BUCKETS 131
#define GLOBAL_STRING_TABLE_BUCKET_SIZE 128

// assumes that memory pools's per block data is same as sizeof (StringLiteralEntry) 
#define EEHASH_MEMORY_POOL_GROW_COUNT 128

STRINGREF AllocateStringObject(EEStringData *pStringData);
UTF8STRINGREF AllocateUTF8StringObject(EEStringData *pStringData);
#ifdef LOGGING
void LogStringLiteral(__in_z const char* action, EEStringData *pStringData);
#endif

// Loader allocator specific string literal map.
template<class TEntryType>
class StringLiteralMap
{
    typedef Wrapper<TEntryType*,DoNothing,TEntryType::StaticRelease> StringLiteralEntryHolder; 

public:
    // Constructor and destructor.
    StringLiteralMap()
    : m_StringToEntryHashTable(NULL)
    , m_MemoryPool(NULL)
    {
        CONTRACTL
        {
            THROWS;
            GC_NOTRIGGER;
        }
        CONTRACTL_END;
    }

    ~StringLiteralMap()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_TRIGGERS;
        }
        CONTRACTL_END;

        // We do need to take the globalstringliteralmap lock because we are manipulating
        // StringLiteralEntry objects that belong to it.
        // Note that we remember the current entry and relaese it only when the 
        // enumerator has advanced to the next entry so that we don't endup deleteing the
        // current entry itself and killing the enumerator.

        if (m_StringToEntryHashTable != NULL)
        {
            // We need the global lock anytime we release StringLiteralEntry objects
            CrstHolder gch(&(TEntryType::GetGlobalStringLiteralMapNoCreate()->m_HashTableCrstGlobal));

            TEntryType *pEntry = NULL;
            EEHashTableIteration Iter;

#ifdef _DEBUG
            m_StringToEntryHashTable->SuppressSyncCheck();
#endif

            m_StringToEntryHashTable->IterateStart(&Iter);
            if (m_StringToEntryHashTable->IterateNext(&Iter))
            {
                pEntry = (TEntryType*)m_StringToEntryHashTable->IterateGetValue(&Iter);

                while (m_StringToEntryHashTable->IterateNext(&Iter))
                {
                    // Release the previous entry
                    _ASSERTE(pEntry);
                    pEntry->Release();

                    // Set the 
                    pEntry = (TEntryType*)m_StringToEntryHashTable->IterateGetValue(&Iter);
                }
                // Release the last entry
                _ASSERTE(pEntry);
                pEntry->Release();
            }
            // else there were no entries.

            // Delete the hash table first. The dtor of the hash table would clean up all the entries.
            delete m_StringToEntryHashTable;
        }

        // Delete the pool later, since the dtor above would need it.
        if (m_MemoryPool != NULL)
            delete m_MemoryPool;
    }

    // Initialization method.
    void  Init()
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            PRECONDITION(CheckPointer(this));
            INJECT_FAULT(ThrowOutOfMemory());
        }
        CONTRACTL_END;

#ifndef DACCESS_COMPILE
        // Allocate the memory pool and set the initial count to quarter as grow count
        m_MemoryPool = new MemoryPool (SIZEOF_EEHASH_ENTRY, EEHASH_MEMORY_POOL_GROW_COUNT, EEHASH_MEMORY_POOL_GROW_COUNT/4);

        m_StringToEntryHashTable =  new typename TEntryType::StringLiteralHashTable ();

        LockOwner lock = {&(TEntryType::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal), IsOwnerOfCrst};
        if (!m_StringToEntryHashTable->Init(INIT_NUM_APP_DOMAIN_STRING_BUCKETS, &lock, m_MemoryPool))
            ThrowOutOfMemory();
#else // DACCESS_COMPILE
        _ASSERTE(FALSE);
#endif // DACCESS_COMPILE
    }


    size_t GetSize()
    {
        LIMITED_METHOD_CONTRACT;
        return m_MemoryPool?m_MemoryPool->GetSize():0;
    }

    // Method to retrieve a string from the map.
    typename TEntryType::RefType *GetStringLiteral(EEStringData *pStringData, BOOL bAddIfNotFound, BOOL bAppDomainWontUnload)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pStringData));
        }
        CONTRACTL_END;

        HashDatum Data;

        DWORD dwHash = m_StringToEntryHashTable->GetHash(pStringData);
        if (m_StringToEntryHashTable->GetValue(pStringData, &Data, dwHash))
        {
            typename TEntryType::RefType *pStrObj = NULL;
            pStrObj = ((TEntryType*)Data)->GetStringObject();
            _ASSERTE(!bAddIfNotFound || pStrObj);
            return pStrObj;
        }
        else
        {
            // Retrieve the string literal from the global string literal map.
            CrstHolder gch(&(TEntryType::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal));

            // TODO: We can be more efficient by checking our local hash table now to see if
            // someone beat us to inserting it. (m_StringToEntryHashTable->GetValue(pStringData, &Data))
            // (Rather than waiting until after we look the string up in the global map) 
            
            StringLiteralEntryHolder pEntry(TEntryType::GetGlobalStringLiteralMap()->GetStringLiteral(pStringData, dwHash, bAddIfNotFound));

            _ASSERTE(pEntry || !bAddIfNotFound);

            // If pEntry is non-null then the entry exists in the Global map. (either we retrieved it or added it just now)
            if (pEntry)
            {
                // If the entry exists in the Global map and the appdomain wont ever unload then we really don't need to add a
                // hashentry in the appdomain specific map.
                // TODO: except that by not inserting into our local table we always take the global map lock
                // and come into this path, when we could succeed at a lock free lookup above.
                
                if (!bAppDomainWontUnload)
                {                
                    // Make sure some other thread has not already added it.
                    if (!m_StringToEntryHashTable->GetValue(pStringData, &Data))
                    {
                        // Insert the handle to the string into the hash table.
                        m_StringToEntryHashTable->InsertValue(pStringData, (LPVOID)pEntry, FALSE);
                    }
                    else
                    {
                        pEntry.Release(); //while we're still under lock
                    }
                }
#ifdef _DEBUG
                else
                {
                    LOG((LF_APPDOMAIN, LL_INFO10000, "Avoided adding String literal to appdomain map: size: %d bytes\n", pStringData->GetCharCount()));
                }
#endif
                pEntry.SuppressRelease();
                typename TEntryType::RefType *pStrObj = NULL;
                // Retrieve the string objectref from the string literal entry.
                pStrObj = pEntry->GetStringObject();
                _ASSERTE(!bAddIfNotFound || pStrObj);
                return pStrObj;
            }
        }
        // If the bAddIfNotFound flag is set then we better have a string
        // string object at this point.
        _ASSERTE(!bAddIfNotFound);
        return NULL;
    }

    // Method to explicitly intern a string object.
    typename TEntryType::RefType *GetInternedString(typename TEntryType::RefType *pString, BOOL bAddIfNotFound, BOOL bAppDomainWontUnload)
    {
        CONTRACTL
        {
            GC_TRIGGERS;
            THROWS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pString));
        }
        CONTRACTL_END;

        HashDatum Data;
        EEStringData StringData = EEStringData((*pString)->GetStringLength(), (*pString)->GetBuffer());

        DWORD dwHash = m_StringToEntryHashTable->GetHash(&StringData);
        if (m_StringToEntryHashTable->GetValue(&StringData, &Data, dwHash))
        {
            typename TEntryType::RefType *pStrObj = NULL;
            pStrObj = ((TEntryType*)Data)->GetStringObject();
            _ASSERTE(!bAddIfNotFound || pStrObj);
            return pStrObj;

        }
        else
        {
            CrstHolder gch(&(TEntryType::GetGlobalStringLiteralMap()->m_HashTableCrstGlobal));

            // TODO: We can be more efficient by checking our local hash table now to see if
            // someone beat us to inserting it. (m_StringToEntryHashTable->GetValue(pStringData, &Data))
            // (Rather than waiting until after we look the string up in the global map) 
            
            // Retrieve the string literal from the global string literal map.
            StringLiteralEntryHolder pEntry(TEntryType::GetGlobalStringLiteralMap()->GetInternedString(pString, dwHash, bAddIfNotFound));

            _ASSERTE(pEntry || !bAddIfNotFound);

            // If pEntry is non-null then the entry exists in the Global map. (either we retrieved it or added it just now)
            if (pEntry)
            {
                // If the entry exists in the Global map and the appdomain wont ever unload then we really don't need to add a
                // hashentry in the appdomain specific map.
                // TODO: except that by not inserting into our local table we always take the global map lock
                // and come into this path, when we could succeed at a lock free lookup above.

                if (!bAppDomainWontUnload)
                {
                    // Since GlobalStringLiteralMap::GetInternedString() could have caused a GC,
                    // we need to recreate the string data.
                    StringData = EEStringData((*pString)->GetStringLength(), (*pString)->GetBuffer());

                    // Make sure some other thread has not already added it.
                    if (!m_StringToEntryHashTable->GetValue(&StringData, &Data))
                    {
                        // Insert the handle to the string into the hash table.
                        m_StringToEntryHashTable->InsertValue(&StringData, (LPVOID)pEntry, FALSE);
                    }
                    else
                    {
                        pEntry.Release(); // while we're under lock
                    }
                }
                pEntry.SuppressRelease();
                // Retrieve the string objectref from the string literal entry.
                typename TEntryType::RefType *pStrObj = NULL;
                pStrObj = pEntry->GetStringObject();
                return pStrObj;
            }
        }
        // If the bAddIfNotFound flag is set then we better have a string
        // string object at this point.
        _ASSERTE(!bAddIfNotFound);

        return NULL;
    }

private:
    // Hash tables that maps a Unicode string to a COM+ string handle.
    typename TEntryType::StringLiteralHashTable    *m_StringToEntryHashTable;

    // The memorypool for hash entries for this hash table.
    MemoryPool                  *m_MemoryPool;
};

// Global string literal map.
template<class TEntryType>
class GlobalStringLiteralMap
{
    // StringLiteralMap and StringLiteralEntry need to acquire the crst of the global string literal map.
    friend class StringLiteralMap<TEntryType>;
    friend class StringLiteralEntry;
    friend class Utf8StringLiteralEntry;

public:
    typedef Wrapper<TEntryType*,DoNothing,TEntryType::StaticRelease> StringLiteralEntryHolder; 

    // Constructor and destructor.
    GlobalStringLiteralMap()
#ifndef DACCESS_COMPILE
    : m_StringToEntryHashTable(NULL)
    , m_MemoryPool(NULL)
    , m_HashTableCrstGlobal(CrstGlobalStrLiteralMap)
    , m_LargeHeapHandleTable(SystemDomain::System(), GLOBAL_STRING_TABLE_BUCKET_SIZE)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
        }
        CONTRACTL_END;

#ifdef _DEBUG
        m_LargeHeapHandleTable.RegisterCrstDebug(&m_HashTableCrstGlobal);
#endif
    }
#else // DACCESS_COMPILE
    {
        _ASSERTE(FALSE);
    }
#endif // DACCESS_COMPILE

    ~GlobalStringLiteralMap()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_TRIGGERS;
        }
        CONTRACTL_END;
        
        // if we are deleting the map then either it is shutdown time or else there was a race trying to create
        // the initial map and this one was the loser 
        // (i.e. two threads made a map and the InterlockedCompareExchange failed for one of them and 
        // now it is deleting the map)
        //
        // if it's not the main map, then the map we are deleting better be empty!

        // there must be *some* global table
        _ASSERTE(TEntryType::GetGlobalStringLiteralMapNoCreate()  != NULL);

        if (TEntryType::GetGlobalStringLiteralMapNoCreate() != this)
        {
            // if this isn't the real global table then it must be empty
            _ASSERTE(m_StringToEntryHashTable->IsEmpty());  

            // Delete the hash table first. The dtor of the hash table would clean up all the entries.
            delete m_StringToEntryHashTable;
            // Delete the pool later, since the dtor above would need it.
            delete m_MemoryPool;        
        }
        else
        {
            // We are shutting down, the OS will reclaim the memory from the StringLiteralEntries,
            // m_MemoryPool and m_StringToEntryHashTable.
            _ASSERTE(g_fProcessDetach);
        }        
    }

    // Initialization method.
    void Init()
    {
        CONTRACTL
        {
            THROWS;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer(this));
            INJECT_FAULT(ThrowOutOfMemory());
        } 
        CONTRACTL_END;

#ifndef DACCESS_COMPILE
        // Allocate the memory pool and set the initial count to quarter as grow count
        m_MemoryPool = new MemoryPool (SIZEOF_EEHASH_ENTRY, EEHASH_MEMORY_POOL_GROW_COUNT, EEHASH_MEMORY_POOL_GROW_COUNT/4);

        m_StringToEntryHashTable =  new typename TEntryType::StringLiteralHashTable ();

        LockOwner lock = {&m_HashTableCrstGlobal, IsOwnerOfCrst};
        if (!m_StringToEntryHashTable->Init(INIT_NUM_GLOBAL_STRING_BUCKETS, &lock, m_MemoryPool))
            ThrowOutOfMemory();
#else // DACCESS_COMPILE
        _ASSERTE(FALSE);
#endif // DACCESS_COMPILE
    }

    // Method to retrieve a string from the map. Takes a precomputed hash (for perf).
    TEntryType *GetStringLiteral(EEStringData *pStringData, DWORD dwHash, BOOL bAddIfNotFound)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pStringData));
            PRECONDITION(m_HashTableCrstGlobal.OwnedByCurrentThread());        
        }
        CONTRACTL_END;

        HashDatum Data;
        TEntryType *pEntry = NULL;

        if (m_StringToEntryHashTable->GetValue(pStringData, &Data, dwHash))
        {
            pEntry = (TEntryType*)Data;
            // If the entry is already in the table then addref it before we return it.
            if (pEntry)
                pEntry->AddRef();
        }
        else
        {
            if (bAddIfNotFound)
                pEntry = AddStringLiteral(pStringData);
        }

        return pEntry;
    }


    // Method to explicitly intern a string object. Takes a precomputed hash (for perf).
    TEntryType *GetInternedString(typename TEntryType::RefType *pString, DWORD dwHash, BOOL bAddIfNotFound)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pString));
            PRECONDITION(m_HashTableCrstGlobal.OwnedByCurrentThread());
        }
        CONTRACTL_END;  

        EEStringData StringData = EEStringData((*pString)->GetStringLength(), (*pString)->GetBuffer());

        HashDatum Data;
        TEntryType *pEntry = NULL;

        if (m_StringToEntryHashTable->GetValue(&StringData, &Data, dwHash))
        {
            pEntry = (TEntryType*)Data;
            // If the entry is already in the table then addref it before we return it.
            if (pEntry)
                pEntry->AddRef();
        }
        else
        {
            if (bAddIfNotFound)
                pEntry = AddInternedString(pString);
        }

        return pEntry;
    }


    // Method to calculate the hash
    DWORD GetHash(EEStringData* pData)
    {
        WRAPPER_NO_CONTRACT;
        return m_StringToEntryHashTable->GetHash(pData);
    }

    // public method to retrieve m_HashTableCrstGlobal
    Crst* GetHashTableCrstGlobal() 
    {
        LIMITED_METHOD_CONTRACT;
        return &m_HashTableCrstGlobal;
    }

private:    
    // Helper method to add a string to the global string literal map.
    TEntryType *AddStringLiteral(EEStringData *pStringData)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(m_HashTableCrstGlobal.OwnedByCurrentThread());        
        }
        CONTRACTL_END;  

        TEntryType *pRet;

        {
        LargeHeapHandleBlockHolder pStrObj(&m_LargeHeapHandleTable,1);
        // Create the COM+ string object.
        typename TEntryType::RefType strObj = TEntryType::AllocateStringObject(pStringData);
                    
        // Allocate a handle for the string.
        SetObjectReference(pStrObj[0], (OBJECTREF) strObj, NULL);
    

        // Allocate the StringLiteralEntry.
        StringLiteralEntryHolder pEntry(TEntryType::AllocateEntry(pStringData, (typename TEntryType::RefType*)pStrObj[0]));
        pStrObj.SuppressRelease();
        // Insert the handle to the string into the hash table.
        m_StringToEntryHashTable->InsertValue(pStringData, (LPVOID)pEntry, FALSE);
        pEntry.SuppressRelease();
        pRet = pEntry;

#ifdef LOGGING
        LogStringLiteral("added", pStringData);
#endif
        }

        return pRet;
    }


    // Helper method to add an interned string.
    TEntryType *AddInternedString(typename TEntryType::RefType *pString)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(m_HashTableCrstGlobal.OwnedByCurrentThread());        
        }
        CONTRACTL_END;  

        EEStringData StringData = EEStringData((*pString)->GetStringLength(), (*pString)->GetBuffer());    
        TEntryType *pRet;

        {
        LargeHeapHandleBlockHolder pStrObj(&m_LargeHeapHandleTable,1);
        SetObjectReference(pStrObj[0], (OBJECTREF) *pString, NULL);

        // Since the allocation might have caused a GC we need to re-get the
        // string data.
        StringData = EEStringData((*pString)->GetStringLength(), (*pString)->GetBuffer());

        StringLiteralEntryHolder pEntry(TEntryType::AllocateEntry(&StringData, (typename TEntryType::RefType*)pStrObj[0]));
        pStrObj.SuppressRelease();

        // Insert the handle to the string into the hash table.
        m_StringToEntryHashTable->InsertValue(&StringData, (LPVOID)pEntry, FALSE);
        pEntry.SuppressRelease();
        pRet = pEntry;
        }

        return pRet;
    }


    // Called by StringLiteralEntry when its RefCount falls to 0.
    void RemoveStringLiteralEntry(TEntryType *pEntry)
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer(pEntry)); 
            PRECONDITION(m_HashTableCrstGlobal.OwnedByCurrentThread());
            PRECONDITION(CheckPointer(this));
        }
        CONTRACTL_END;      

        // Remove the entry from the hash table.
        {
            GCX_COOP();

            EEStringData StringData;    
            pEntry->GetStringData(&StringData);

            BOOL bSuccess;
            bSuccess = m_StringToEntryHashTable->DeleteValue(&StringData);
            // this assert is comented out to accomodate case when StringLiteralEntryHolder 
            // releases this object after failed insertion into hash
            //_ASSERTE(bSuccess);

#ifdef LOGGING
            // We need to do this logging within the GCX_COOP(), as a gc will render
            // our StringData pointers stale.
            if (bSuccess)
            {
                LogStringLiteral("removed", &StringData);
            }
#endif

            // Release the object handle that the entry was using.
            typename TEntryType::RefType *pObjRef = pEntry->GetStringObject();
            m_LargeHeapHandleTable.ReleaseHandles((OBJECTREF*)pObjRef, 1);
        }

        // We do not delete the StringLiteralEntry itself that will be done in the
        // release method of the StringLiteralEntry.
    }
    
    // Hash tables that maps a Unicode string to a LiteralStringEntry.
    typename TEntryType::StringLiteralHashTable    *m_StringToEntryHashTable;

    // The memorypool for hash entries for this hash table.
    MemoryPool                  *m_MemoryPool;

    // The hash table table critical section.  
    // (the Global suffix is so that it is clear in context whether the global table is being locked 
    // or the per app domain table is being locked.  Sometimes there was confusion in the code
    // changing the name of the global one will avoid this problem and prevent copy/paste errors)
    
    Crst                        m_HashTableCrstGlobal;

    // The large heap handle table.
    LargeHeapHandleTable        m_LargeHeapHandleTable;

};

class StringLiteralEntryArray;
class Utf8StringLiteralEntryArray;

// Ref counted entry representing a string literal.
class StringLiteralEntry
{
public:
    typedef STRINGREF RefType;
    typedef EEUnicodeStringLiteralHashTable StringLiteralHashTable;

private:
    StringLiteralEntry(EEStringData *pStringData, STRINGREF *pStringObj)
    : m_pStringObj(pStringObj), m_dwRefCount(1)
#ifdef _DEBUG
      , m_bDeleted(FALSE)
#endif
    {
        LIMITED_METHOD_CONTRACT;
    }
protected:
    ~StringLiteralEntry()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
        }
        CONTRACTL_END;
    }

public:
    void AddRef()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
            PRECONDITION((LONG)VolatileLoad(&m_dwRefCount) > 0);            
            PRECONDITION(SystemDomain::GetGlobalStringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());            
        }
        CONTRACTL_END;

        _ASSERTE (!m_bDeleted);

        // We will keep the item alive forever if the refcount overflowed
        if ((LONG)VolatileLoad(&m_dwRefCount) < 0)
            return;

        VolatileStore(&m_dwRefCount, VolatileLoad(&m_dwRefCount) + 1);
    }
#ifndef DACCESS_COMPILE
    FORCEINLINE static void StaticRelease(StringLiteralEntry* pEntry)
    {        
        CONTRACTL
        {
            PRECONDITION(SystemDomain::GetGlobalStringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());            
        }
        CONTRACTL_END;
        
        pEntry->Release();
    }
#else
    FORCEINLINE static void StaticRelease(StringLiteralEntry* /* pEntry */)
    {
        WRAPPER_NO_CONTRACT;
        DacNotImpl();
    }
#endif // DACCESS_COMPILE

#ifndef DACCESS_COMPILE
    void Release()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
            PRECONDITION(VolatileLoad(&m_dwRefCount) > 0);
            PRECONDITION(SystemDomain::GetGlobalStringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());
        }
        CONTRACTL_END;

        // We will keep the item alive forever if the refcount overflowed
        if ((LONG)VolatileLoad(&m_dwRefCount) < 0)
            return;

        VolatileStore(&m_dwRefCount, VolatileLoad(&m_dwRefCount) - 1);
        if (VolatileLoad(&m_dwRefCount) == 0)
        {
            _ASSERTE(SystemDomain::GetGlobalStringLiteralMapNoCreate());
            SystemDomain::GetGlobalStringLiteralMapNoCreate()->RemoveStringLiteralEntry(this);
            // Puts this entry in the free list
            DeleteEntry (this);             
        }
    }
#endif // DACCESS_COMPILE
    
    LONG GetRefCount()
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            PRECONDITION(CheckPointer(this));
        }
        CONTRACTL_END;

        _ASSERTE (!m_bDeleted);

        return (VolatileLoad(&m_dwRefCount));
    }

    STRINGREF* GetStringObject()
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            PRECONDITION(CheckPointer(this));
        }
        CONTRACTL_END;
        return m_pStringObj;
    }

    void GetStringData(EEStringData *pStringData)
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pStringData));
        }
        CONTRACTL_END;
            
        WCHAR *thisChars;
        int thisLength;

        ObjectToSTRINGREF(*(StringObject**)m_pStringObj)->RefInterpretGetStringValuesDangerousForGC(&thisChars, &thisLength);
        pStringData->SetCharCount (thisLength); // thisLength is in WCHARs and that's what EEStringData's char count wants
        pStringData->SetUTF16StringBuffer (thisChars);
    }

    static StringLiteralEntry *AllocateEntry(EEStringData *pStringData, STRINGREF *pStringObj);
    static void DeleteEntry (StringLiteralEntry *pEntry);
#ifndef DACCESS_COMPILE
    static GlobalStringLiteralMap<StringLiteralEntry> *GetGlobalStringLiteralMap() { return SystemDomain::GetGlobalStringLiteralMap(); }
    static GlobalStringLiteralMap<StringLiteralEntry> *GetGlobalStringLiteralMapNoCreate() { return SystemDomain::GetGlobalStringLiteralMapNoCreate(); }
    static STRINGREF AllocateStringObject(EEStringData *pStringData) { WRAPPER_NO_CONTRACT; return ::AllocateStringObject(pStringData); }
#endif

private:
    STRINGREF*                  m_pStringObj;
    union
    {
        DWORD                       m_dwRefCount;
        StringLiteralEntry         *m_pNext;
    };

#ifdef _DEBUG
    BOOL m_bDeleted;       
#endif

    // The static lists below are protected by GetGlobalStringLiteralMap()->m_HashTableCrstGlobal
    static StringLiteralEntryArray *s_EntryList; // always the first entry array in the chain. 
    static DWORD                    s_UsedEntries;   // number of entries used up in the first array
    static StringLiteralEntry      *s_FreeEntryList; // free list chained thru the arrays.
};

// Ref counted entry representing a string literal.
class Utf8StringLiteralEntry
{
public:
    typedef UTF8STRINGREF RefType;
    typedef EEUnicodeUtf8StringLiteralHashTable StringLiteralHashTable;

private:
    Utf8StringLiteralEntry(EEStringData *pStringData, UTF8STRINGREF *pStringObj)
    : m_pStringObj(pStringObj), m_dwRefCount(1)
#ifdef _DEBUG
      , m_bDeleted(FALSE)
#endif
    {
        LIMITED_METHOD_CONTRACT;
    }
protected:
    ~Utf8StringLiteralEntry()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
        }
        CONTRACTL_END;
    }

public:
    void AddRef()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
            PRECONDITION((LONG)VolatileLoad(&m_dwRefCount) > 0);            
            PRECONDITION(SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());            
        }
        CONTRACTL_END;

        _ASSERTE (!m_bDeleted);

        // We will keep the item alive forever if the refcount overflowed
        if ((LONG)VolatileLoad(&m_dwRefCount) < 0)
            return;

        VolatileStore(&m_dwRefCount, VolatileLoad(&m_dwRefCount) + 1);
    }
#ifndef DACCESS_COMPILE
    FORCEINLINE static void StaticRelease(Utf8StringLiteralEntry* pEntry)
    {        
        CONTRACTL
        {
            PRECONDITION(SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());            
        }
        CONTRACTL_END;
        
        pEntry->Release();
    }
#else
    FORCEINLINE static void StaticRelease(Utf8StringLiteralEntry* /* pEntry */)
    {
        WRAPPER_NO_CONTRACT;
        DacNotImpl();
    }
#endif // DACCESS_COMPILE

#ifndef DACCESS_COMPILE
    void Release()
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            PRECONDITION(CheckPointer<void>(this));
            PRECONDITION(VolatileLoad(&m_dwRefCount) > 0);
            PRECONDITION(SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate()->m_HashTableCrstGlobal.OwnedByCurrentThread());
        }
        CONTRACTL_END;

        // We will keep the item alive forever if the refcount overflowed
        if ((LONG)VolatileLoad(&m_dwRefCount) < 0)
            return;

        VolatileStore(&m_dwRefCount, VolatileLoad(&m_dwRefCount) - 1);
        if (VolatileLoad(&m_dwRefCount) == 0)
        {
            _ASSERTE(SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate());
            SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate()->RemoveStringLiteralEntry(this);
            // Puts this entry in the free list
            DeleteEntry (this);             
        }
    }
#endif // DACCESS_COMPILE
    
    LONG GetRefCount()
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            PRECONDITION(CheckPointer(this));
        }
        CONTRACTL_END;

        _ASSERTE (!m_bDeleted);

        return (VolatileLoad(&m_dwRefCount));
    }

    UTF8STRINGREF* GetStringObject()
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            PRECONDITION(CheckPointer(this));
        }
        CONTRACTL_END;
        return m_pStringObj;
    }

    void GetStringData(EEStringData *pStringData)
    {
        CONTRACTL
        {
            NOTHROW;
            if(GetThread()){GC_NOTRIGGER;}else{DISABLED(GC_TRIGGERS);};
            MODE_COOPERATIVE;
            PRECONDITION(CheckPointer(this));
            PRECONDITION(CheckPointer(pStringData));
        }
        CONTRACTL_END;
            
        CHAR *thisChars;
        int thisLength;

        ObjectToUTF8STRINGREF(*(Utf8StringObject**)m_pStringObj)->RefInterpretGetStringValuesDangerousForGC(&thisChars, &thisLength);
        pStringData->SetCharCount (thisLength); // thisLength is in WCHARs and that's what EEStringData's char count wants
        pStringData->SetUTF8StringBuffer (thisChars);
    }

    static Utf8StringLiteralEntry *AllocateEntry(EEStringData *pStringData, UTF8STRINGREF *pStringObj);
    static void DeleteEntry (Utf8StringLiteralEntry *pEntry);

#ifndef DACCESS_COMPILE
    static GlobalStringLiteralMap<Utf8StringLiteralEntry> *GetGlobalStringLiteralMap() { return SystemDomain::GetGlobalUtf8StringLiteralMap(); }
    static GlobalStringLiteralMap<Utf8StringLiteralEntry> *GetGlobalStringLiteralMapNoCreate() { return SystemDomain::GetGlobalUtf8StringLiteralMapNoCreate(); }
    static UTF8STRINGREF AllocateStringObject(EEStringData *pStringData) { WRAPPER_NO_CONTRACT; return AllocateUTF8StringObject(pStringData); }
#endif
private:
    UTF8STRINGREF*                  m_pStringObj;
    union
    {
        DWORD                       m_dwRefCount;
        Utf8StringLiteralEntry         *m_pNext;
    };

#ifdef _DEBUG
    BOOL m_bDeleted;       
#endif

    // The static lists below are protected by GetGlobalStringLiteralMap()->m_HashTableCrstGlobal
    static Utf8StringLiteralEntryArray *s_EntryList; // always the first entry array in the chain. 
    static DWORD                    s_UsedEntries;   // number of entries used up in the first array
    static Utf8StringLiteralEntry      *s_FreeEntryList; // free list chained thru the arrays.
};



class StringLiteralEntryArray
{
public:
    StringLiteralEntryArray *m_pNext;
    BYTE                     m_Entries[MAX_ENTRIES_PER_CHUNK*sizeof(StringLiteralEntry)];
};

class Utf8StringLiteralEntryArray
{
public:
    Utf8StringLiteralEntryArray *m_pNext;
    BYTE                     m_Entries[MAX_ENTRIES_PER_CHUNK*sizeof(Utf8StringLiteralEntry)];
};

#endif // _STRINGLITERALMAP_H

