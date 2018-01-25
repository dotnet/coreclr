// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __GCTOCLREVENTSINK_H__
#define __GCTOCLREVENTSINK_H__

#include "gcinterface.h"

class GCToCLREventSink : public IGCToCLREventSink
{
    /* [LOCALGC TODO] This will be filled with events as they get ported */

    // GCPrivate events
    void FirePerHeapHistory_V3(uint8_t *FreeListAllocated,
    					   	   uint8_t *FreeListRejected,
				   			   uint8_t *EndOfSegAllocated,
    						   uint8_t *CondemnedAllocated,
    						   uint8_t *PinnedAllocated,
    						   uint8_t *PinnedAllocatedAdvance,
    						   uint32_t RunningFreeListEfficiency,
    						   uint32_t CondemnReasons0,
    						   uint32_t CondemnReasons1,
    						   uint32_t CompactMechanisms,
    						   uint32_t ExpandMechanisms,
    						   uint32_t HeapIndex,
    						   uint8_t *ExtraGen0Commit,
    						   uint32_t Count,
	    					   uint32_t ValuesLen,
    						   uint8_t *Values);
};

extern GCToCLREventSink g_gcToClrEventSink;

#endif // __GCTOCLREVENTSINK_H__

