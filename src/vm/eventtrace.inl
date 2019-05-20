// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#ifdef FEATURE_EVENT_TRACE

FORCEINLINE bool ETW::TieredCompilationLog::Runtime::IsEnabled()
{
    CONTRACTL{
        NOTHROW;
        GC_NOTRIGGER;
    } CONTRACTL_END;

    return
        ETW_TRACING_CATEGORY_ENABLED(
            MICROSOFT_WINDOWS_DOTNETRUNTIME_PROVIDER_Context,
            TRACE_LEVEL_INFORMATION,
            CLR_TIERED_COMPILATION_KEYWORD);
}

FORCEINLINE bool ETW::TieredCompilationLog::Rundown::IsEnabled()
{
    CONTRACTL{
        NOTHROW;
        GC_NOTRIGGER;
    } CONTRACTL_END;

    return
        ETW_TRACING_CATEGORY_ENABLED(
            MICROSOFT_WINDOWS_DOTNETRUNTIME_RUNDOWN_PROVIDER_Context,
            TRACE_LEVEL_INFORMATION,
            CLR_TIERED_COMPILATION_RUNDOWN_KEYWORD);
}

#endif // FEATURE_EVENT_TRACE
