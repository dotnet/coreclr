// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __GCTOCLREVENTSINK_H__
#define __GCTOCLREVENTSINK_H__

#include "gcinterface.h"

class GCToCLREventSink : public IGCToCLREventSink
{
public:
    void FireDynamicEvent(const char* eventName, void* payload, uint32_t payloadSize);
    void FireGCStart_V2(uint32_t count, uint32_t depth, uint32_t reason, uint32_t type);
    void FireGCEnd_V1(uint32_t count, uint32_t depth);
    void FireGCGenerationRange(uint8_t generation, void* rangeStart, uint64_t rangeUsedLength, uint64_t rangeReservedLength);
};

extern GCToCLREventSink g_gcToClrEventSink;

#endif // __GCTOCLREVENTSINK_H__

