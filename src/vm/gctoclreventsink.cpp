// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gctoclreventsink.h"
#include "eventtrace.h"

GCToCLREventSink g_gcToClrEventSink;

void GCToCLREventSink::FireDynamicEvent(const char* eventName, void* payload, uint32_t payloadSize)
{
    LIMITED_METHOD_CONTRACT;

    const size_t EventNameMaxSize = 255;

    WCHAR wideEventName[EventNameMaxSize];
    if (MultiByteToWideChar(CP_ACP, 0, eventName, -1, wideEventName, EventNameMaxSize) == 0)
    {
        return;
    }

    FireEtwGCDynamicEvent(wideEventName, payloadSize, (const BYTE*)payload, GetClrInstanceId());
}

void GCToCLREventSink::FireGCStart_V2(uint32_t count, uint32_t depth, uint32_t reason, uint32_t type)
{
    LIMITED_METHOD_CONTRACT;

    ETW::GCLog::ETW_GC_INFO gcStartInfo;
    gcStartInfo.GCStart.Count = count;
    gcStartInfo.GCStart.Depth = depth;
    gcStartInfo.GCStart.Reason = static_cast<ETW::GCLog::ETW_GC_INFO::GC_REASON>(reason);
    gcStartInfo.GCStart.Type = static_cast<ETW::GCLog::ETW_GC_INFO::GC_TYPE>(type);
    ETW::GCLog::FireGcStart(&gcStartInfo);
}

void GCToCLREventSink::FireGCGenerationRange(uint8_t generation, void* rangeStart, uint64_t rangeUsedLength, uint64_t rangeReservedLength)
{
    LIMITED_METHOD_CONTRACT;

    FireEtwGCGenerationRange(generation, rangeStart, rangeUsedLength, rangeReservedLength, GetClrInstanceId());
}