// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#include "common.h"
#include "unixlowmem.h"

size_t UnixLowMemoryDetector::s_lowMemoryLimitBytes;

void UnixLowMemoryDetector::Init()
{
    LIMITED_METHOD_CONTRACT;

    size_t percent = max(min(CLRConfig::GetConfigValue(CLRConfig::EXTERNAL_UnixLowMemoryLimitPercent), 100), 1);
    s_lowMemoryLimitBytes = PAL_GetRestrictedPhysicalMemoryLimit() * percent / 100;
}

bool UnixLowMemoryDetector::IsLowMemory()
{
    LIMITED_METHOD_CONTRACT;

    size_t workingSet;
    if (!PAL_GetWorkingSetSize(&workingSet))
        return false;

    return workingSet > s_lowMemoryLimitBytes;
}

