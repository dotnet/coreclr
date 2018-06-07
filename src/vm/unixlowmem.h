// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#ifndef _UNIXLOWMEM_H_
#define _UNIXLOWMEM_H_

#ifdef FEATURE_PAL
class UnixLowMemoryDetector
{
    size_t m_szLowMemoryLimitBytes;

public:
    UnixLowMemoryDetector();

    size_t ReadLowMemoryLimitPercent();
    size_t ReadPhysicalMemoryLimitBytes();
    bool IsLowMemory();
};

#endif
#endif