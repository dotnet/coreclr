// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gchandleutilities.h"

void DiagHandleCreated(OBJECTHANDLE handle, OBJECTREF objRef)
{
#if defined(ENABLE_PERF_COUNTERS) || defined(FEATURE_EVENT_TRACE)
    g_dwHandles++;
#endif // defined(ENABLE_PERF_COUNTERS) || defined(FEATURE_EVENT_TRACE)

#ifdef GC_PROFILING
    BEGIN_PIN_PROFILER(CORProfilerTrackGC());
    g_profControlBlock.pProfInterface->HandleCreated((uintptr_t)handle, (ObjectID)OBJECTREF_TO_UNCHECKED_OBJECTREF(objRef));
    END_PIN_PROFILER();
#else
    UNREFERENCED_PARAMETER(handle);
    UNREFERENCED_PARAMETER(objRef);
#endif // GC_PROFILING
}

void DiagHandleDestroyed(OBJECTHANDLE handle)
{
#ifdef GC_PROFILING
    BEGIN_PIN_PROFILER(CORProfilerTrackGC());
    g_profControlBlock.pProfInterface->HandleDestroyed((uintptr_t)handle);
    END_PIN_PROFILER();
#else // GC_PROFILING
    UNREFERENCED_PARAMETER(handle);
#endif

#if defined(ENABLE_PERF_COUNTERS) || defined(FEATURE_EVENT_TRACE)
    g_dwHandles--;
#endif // defined(ENABLE_PERF_COUNTERS) || defined(FEATURE_EVENT_TRACE)
}
