// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeprofilerapi.h"

#ifdef FEATURE_PERFTRACING

#ifdef PROFILING_SUPPORTED

void EventPipeProfilerApi::WriteEvent(EventPipeEventInstance &instance)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    auto event = instance.GetEvent();
    auto stackContents = instance.GetStack();

    BEGIN_PIN_PROFILER(CORProfilerIsMonitoringEventPipe());
    g_profControlBlock.pProfInterface->EventPipeEventDelivered(
        event->GetProvider()->GetProviderName().GetUnicode(),
        event->GetEventID(),
        event->GetEventVersion(),
        event->GetMetadataLength(),
        event->GetMetadata(),
        instance.GetThreadId(),
        instance.GetTimeStamp(),
        instance.GetDataLength(),
        instance.GetData(),
        stackContents->GetLength(),
        reinterpret_cast<UINT_PTR*>(stackContents->GetPointer()));
    END_PIN_PROFILER();
}

#endif // PROFILING_SUPPORTED

#endif // FEATURE_PERFTRACING
