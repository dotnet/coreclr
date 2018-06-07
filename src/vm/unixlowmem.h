// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#ifndef _UNIXLOWMEM_H_
#define _UNIXLOWMEM_H_

class UnixLowMemoryDetector
{
    static size_t s_lowMemoryLimitBytes;

public:
    static void Init();
    static bool IsLowMemory();

    virtual ~UnixLowMemoryDetector() = delete; // make it unconstructible
};

#endif
