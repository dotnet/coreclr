// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _STRINGDEDUPTABLE_H_
#define _STRINGDEDUPTABLE_H_
#include "common.h"
#include "stringobjhashtable.h"

class StringDedupTable
{
friend class StringDedup;
public:
	StringDedupTable();
	~StringDedupTable();
	bool Insert(uint8_t*& item);
private:    
    GCHashTableBase* ht;
};

#endif _STRINGDEDUPTABLE_H_
