// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "stringdeduptable.h"
#include "gcenv.h"

StringDedupTable::StringDedupTable()
{
}

bool StringDedupTable::Init()
{
    ht = new (nothrow) GCUnicodeStringHashTable();
    if (ht)
    {
        return ht->Init(11, NULL);
    }
    return false;
}

StringDedupTable::~StringDedupTable()
{
    ht->Destroy();
}

bool StringDedupTable::Insert(uint8_t*& item)
{
    StringObject* stringref = (StringObject*)item;
    GCStringData stringData(item, stringref->GetStringLength(), stringref->GetBuffer());
    uint32_t hash = ht->GetHash(&stringData);
    StringDupsList** data = ht->InsertOrGetValue(&stringData, hash);
    if (!data)
    {
        return false;
    }
    if (*data)
    {
        return (*data)->Write(item);
    }    
    return true;
}
