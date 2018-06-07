// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#include "common.h"
#include "unixlowmem.h"
#include <stdlib.h>
#include "debugmacros.h"

#ifdef FEATURE_PAL

UnixLowMemoryDetector::UnixLowMemoryDetector()
{
    LIMITED_METHOD_CONTRACT;

    auto percent = max(min(ReadLowMemoryLimitPercent(), 100), 1);
    this->m_szLowMemoryLimitBytes = ReadPhysicalMemoryLimitBytes() * percent / 100;
}

size_t UnixLowMemoryDetector::ReadLowMemoryLimitPercent()
{
    LIMITED_METHOD_CONTRACT;

    char* defaultStackSizeStr = getenv("COMPlus_UnixLowMemoryLimitPercent");
    if (defaultStackSizeStr != NULL)
    {
        errno = 0;
        auto size = atoi(defaultStackSizeStr);

        if (errno == 0)
            return (size_t)size;
    }
    return 75; // default value
}

size_t UnixLowMemoryDetector::ReadPhysicalMemoryLimitBytes()
{
    LIMITED_METHOD_CONTRACT;

    return PAL_GetRestrictedPhysicalMemoryLimit();
}

bool UnixLowMemoryDetector::IsLowMemory()
{
    LIMITED_METHOD_CONTRACT;

    size_t szWorkingSet;
    if (FALSE == PAL_GetWorkingSetSize(&szWorkingSet))
        return false;

    return szWorkingSet > this->m_szLowMemoryLimitBytes;
}

#endif
