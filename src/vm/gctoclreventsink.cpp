// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gctoclreventsink.h"

GCToCLREventSink g_gcToClrEventSink;

void GCToCLREventSink::FirePerHeapHistory_V3(uint8_t *FreeListAllocated,
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
				    					   	 uint8_t *Values)
{	
    FireEtwGCPerHeapHistory_V3(GetClrInstanceId(),
	                           FreeListAllocated,
	                           FreeListRejected,
	                           EndOfSegAllocated,
	                           CondemnedAllocated,
	                           PinnedAllocated,
	                           PinnedAllocatedAdvance,
	                           RunningFreeListEfficiency,
	                           CondemnReasons0,
	                           CondemnReasons1,
	                           CompactMechanisms,
	                           ExpandMechanisms,
	                           HeapIndex,
	                           ExtraGen0Commit,
	                           Count,
	                           ValuesLen,
	                           Values);
}