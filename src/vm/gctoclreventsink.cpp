// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gctoclreventsink.h"

GCToCLREventSink g_gcToClrEventSink;

void GCToCLREventSink::FireGCAllocationTick_V3(
    uint64_t allocationAmount,
    AllocationKind kind,
    uint32_t heapIndex,
    void* address)
{
    LIMITED_METHOD_CONTRACT;

    InlineSString<MAX_CLASSNAME_LENGTH> strTypeName;
    EX_TRY
    {
        TypeHandle th = GetThread()->GetTHAllocContextObj();

        void* typeId = nullptr;
        const WCHAR * name = nullptr;
        if (th != 0)
        {
            th.GetName(strTypeName);
            name = strTypeName.GetUnicode();
            typeId = th.GetMethodTable();
        }

        if (typeId != nullptr)
        {
            FireEtwGCAllocationTick_V3(static_cast<uint32_t>(allocationAmount),
                                   static_cast<uint32_t>(kind),
                                   GetClrInstanceId(),
                                   allocationAmount,
                                   typeId,
                                   name,
                                   heapIndex,
                                   address);
        }
    }
    EX_CATCH {}
    EX_END_CATCH(SwallowAllExceptions)
}
